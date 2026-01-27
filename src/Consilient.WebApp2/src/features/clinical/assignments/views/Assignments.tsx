import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Badge } from '@/shared/components/ui/badge';
import { Button } from '@/shared/components/ui/button';
import { Tabs, TabsList, TabsTrigger } from '@/shared/components/ui/tabs';
import {
  providerAssignmentsService,
  type ProviderAssignmentBatch,
} from '../services/ProviderAssignmentsService';
import { AssignmentsTable } from '../components/AssignmentsTable';
import { GraphQL } from '@/types/api.generated';
import { useFacilities } from '@/shared/stores/FacilityStore';
import { useProviderAssignmentBatchStatuses } from '@/shared/stores/ProviderAssignmentBatchStatusStore';

const POLL_INTERVAL = 5000;

type TabType = 'ready' | 'imported' | 'invalid';

export default function Assignments() {
  const { id: batchId } = useParams<{ id: string }>();
  const [batch, setBatch] = useState<ProviderAssignmentBatch | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isProcessing, setIsProcessing] = useState(false);
  const [activeTab, setActiveTab] = useState<TabType | null>(null);
  const pollingIntervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const { data: facilities } = useFacilities();
  const { data: batchStatuses } = useProviderAssignmentBatchStatuses();

  const fetchBatch = useCallback(async () => {
    if (!batchId) return;

    try {
      const data = await providerAssignmentsService.getBatch(batchId);
      setBatch(data);

      // Stop polling if status is not Pending
      if (data && data.status !== GraphQL.ProviderAssignmentBatchStatus.Pending && pollingIntervalRef.current) {
        clearInterval(pollingIntervalRef.current);
        pollingIntervalRef.current = null;
      }
    } catch (error) {
      console.error('Failed to fetch batch:', error);
    } finally {
      setIsLoading(false);
    }
  }, [batchId]);

  useEffect(() => {
    void fetchBatch();

    // Start polling
    pollingIntervalRef.current = setInterval(() => void fetchBatch(), POLL_INTERVAL);

    return () => {
      if (pollingIntervalRef.current) {
        clearInterval(pollingIntervalRef.current);
      }
    };
  }, [fetchBatch]);

  const items = batch?.items ?? [];

  const facilityName = useMemo(() => {
    return facilities?.find(f => f.id === batch?.facilityId)?.name ?? batch?.facilityId;
  }, [facilities, batch?.facilityId]);

  const statusName = useMemo(() => {
    return batchStatuses?.find(s => s.name === batch?.status)?.name ?? batch?.status;
  }, [batchStatuses, batch?.status]);

  const handleProcessImports = useCallback(async () => {
    if (!batchId) return;

    setIsProcessing(true);
    try {
      await providerAssignmentsService.processBatch(batchId);
      await fetchBatch();
    } catch (error) {
      console.error('Failed to process imports:', error);
    } finally {
      setIsProcessing(false);
    }
  }, [batchId, fetchBatch]);

  const { readyToImport, imported, invalid } = useMemo(() => {
    const ready = items.filter(item =>
      item.shouldImport === true &&
      item.imported === false &&
      !item.validationErrorsJson
    );

    const imp = items.filter(item =>
      item.imported === true
    );

    const inv = items.filter(item =>
      item.shouldImport === false &&
      item.imported === false &&
      !!item.validationErrorsJson
    );

    return { readyToImport: ready, imported: imp, invalid: inv };
  }, [items]);

  const tabOptions = useMemo(() => {
    const options: { label: string; value: TabType }[] = [];
    if (readyToImport.length > 0) {
      options.push({ label: `Ready to Import (${readyToImport.length})`, value: 'ready' });
    }
    if (imported.length > 0) {
      options.push({ label: `Imported (${imported.length})`, value: 'imported' });
    }
    if (invalid.length > 0) {
      options.push({ label: `Invalid (${invalid.length})`, value: 'invalid' });
    }
    return options;
  }, [readyToImport.length, imported.length, invalid.length]);

  // Set default tab when options change
  useEffect(() => {
    if (tabOptions.length > 0 && (!activeTab || !tabOptions.some(opt => opt.value === activeTab))) {
      const firstOption = tabOptions[0];
      if (firstOption) {
        setActiveTab(firstOption.value);
      }
    }
  }, [tabOptions, activeTab]);

  const currentItems = useMemo(() => {
    switch (activeTab) {
      case 'ready':
        return readyToImport;
      case 'imported':
        return imported;
      case 'invalid':
        return invalid;
      default:
        return [];
    }
  }, [activeTab, readyToImport, imported, invalid]);

  if (isLoading) {
    return (
      <div className="bg-white min-h-screen p-8">
        <div className="text-center py-8 text-gray-500">Loading...</div>
      </div>
    );
  }

  if (!batch) {
    return (
      <div className="bg-white min-h-screen p-8">
        <div className="text-center py-8 text-gray-500">Batch not found</div>
      </div>
    );
  }

  return (
    <div className="bg-white min-h-screen p-8">
      <div className="mb-6">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Provider Assignments</h1>
        <p className="text-gray-600">
          Date: {batch.date} | Facility: {facilityName} | Batch: {batch.batchId}
        </p>
        <p className="text-gray-500 text-sm mt-1">
          Status: <Badge variant={batch.status === GraphQL.ProviderAssignmentBatchStatus.Pending ? 'warning' : 'default'}>{statusName}</Badge>
        </p>
      </div>

      {tabOptions.length === 0 ? (
        <div className="text-center py-8 text-gray-500">No assignments found</div>
      ) : (
        <Tabs value={activeTab ?? ''} onValueChange={(value) => setActiveTab(value as TabType)}>
          <div className="mb-4 flex justify-between items-center">
            <TabsList>
              {tabOptions.map((opt) => (
                <TabsTrigger key={opt.value} value={opt.value}>
                  {opt.label}
                </TabsTrigger>
              ))}
            </TabsList>
            {activeTab === 'ready' && (
              <Button onClick={handleProcessImports} disabled={isProcessing}>
                {isProcessing ? 'Processing...' : 'Process imports'}
              </Button>
            )}
          </div>

          <AssignmentsTable items={currentItems} />
        </Tabs>
      )}
    </div>
  );
}
