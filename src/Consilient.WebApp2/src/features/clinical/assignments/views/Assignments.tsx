import { useCallback, useEffect, useRef, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Badge } from '@/shared/components/ui/badge';
import { Button } from '@/shared/components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';
import {
  providerAssignmentBatchService,
  type ProviderAssignmentBatch,
  type ProviderAssignment,
} from '../services/ProviderAssignmentBatchService';

const POLL_INTERVAL = 5000;

const formatName = (person: { firstName: string | null; lastName: string | null } | null): string =>
  person ? `${person.lastName ?? ''}, ${person.firstName ?? ''}`.replace(/^, |, $/g, '') : '';

export default function Assignments() {
  const { id: batchId } = useParams<{ id: string }>();
  const [batch, setBatch] = useState<ProviderAssignmentBatch | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedIds, setSelectedIds] = useState<Set<number>>(new Set());
  const pollingIntervalRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const fetchBatch = useCallback(async () => {
    if (!batchId) return;

    try {
      const data = await providerAssignmentBatchService.getBatch(batchId);
      setBatch(data);

      // Stop polling if status is not Pending
      if (data && data.status !== 'Pending' && pollingIntervalRef.current) {
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
    fetchBatch();

    // Start polling
    pollingIntervalRef.current = setInterval(fetchBatch, POLL_INTERVAL);

    return () => {
      if (pollingIntervalRef.current) {
        clearInterval(pollingIntervalRef.current);
      }
    };
  }, [fetchBatch]);

  const handleCheckboxChange = (id: number, checked: boolean) => {
    setSelectedIds((prev) => {
      const newSet = new Set(prev);
      if (checked) {
        newSet.add(id);
      } else {
        newSet.delete(id);
      }
      return newSet;
    });
  };

  const handleSelectAll = (checked: boolean) => {
    if (checked && batch?.items) {
      const selectableItems = batch.items.filter((item) => !item.visit?.imported);
      setSelectedIds(new Set(selectableItems.map((item) => item.id)));
    } else {
      setSelectedIds(new Set());
    }
  };

  const renderNameWithBadge = (
    person: { firstName: string | null; lastName: string | null } | null,
    resolvedId: number | null
  ) => (
    <span className="flex items-center gap-2">
      {formatName(person)}
      {resolvedId === null && <Badge variant="success">new</Badge>}
    </span>
  );

  const renderPatientWithBadge = (
    patient: { firstName: string | null; lastName: string | null; mrn: string | null } | null,
    resolvedId: number | null
  ) => (
    <span className="flex items-center gap-2">
      {formatName(patient)}{patient?.mrn ? ` (${patient.mrn})` : ''}
      {resolvedId === null && <Badge variant="success">new</Badge>}
    </span>
  );

  const renderCaseIdWithBadge = (
    hospitalization: { caseId: number | null } | null,
    resolvedId: number | null
  ) => (
    <span className="flex items-center gap-2">
      {hospitalization?.caseId ?? ''}
      {resolvedId === null && <Badge variant="success">new</Badge>}
    </span>
  );

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

  const items = batch.items ?? [];
  const selectableItems = items.filter((item) => !item.visit?.imported);
  const allSelected = selectableItems.length > 0 && selectedIds.size === selectableItems.length;

  return (
    <div className="bg-white min-h-screen p-8">
      <div className="mb-6 flex justify-between items-start">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Provider Assignments</h1>
          <p className="text-gray-600">
            Date: {batch.date} | Facility: {batch.facilityId} | Batch: {batch.batchId} | Items: {items.length}
          </p>
          <p className="text-gray-500 text-sm mt-1">
            Status: <Badge variant={batch.status === 'Pending' ? 'warning' : 'default'}>{batch.status}</Badge>
          </p>
        </div>
        <Button disabled={!allSelected}>
          Process imports
        </Button>
      </div>

      {items.length === 0 ? (
        <div className="text-center py-8 text-gray-500">No assignments found</div>
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-12">
                <input
                  type="checkbox"
                  checked={allSelected}
                  onChange={(e) => handleSelectAll(e.target.checked)}
                  className="rounded border-gray-300"
                />
              </TableHead>
              <TableHead>Patient</TableHead>
              <TableHead>Physician</TableHead>
              <TableHead>Nurse Practitioner</TableHead>
              <TableHead>Hospitalization</TableHead>
              <TableHead>Room</TableHead>
              <TableHead>Imported</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {items.map((item: ProviderAssignment) => (
              <TableRow key={item.id}>
                <TableCell>
                  <input
                    type="checkbox"
                    checked={selectedIds.has(item.id)}
                    onChange={(e) => handleCheckboxChange(item.id, e.target.checked)}
                    disabled={item.visit?.imported}
                    className="rounded border-gray-300"
                  />
                </TableCell>
                <TableCell>{renderPatientWithBadge(item.patient, item.resolvedPatientId)}</TableCell>
                <TableCell>{renderNameWithBadge(item.physician, item.resolvedPhysicianId)}</TableCell>
                <TableCell>{renderNameWithBadge(item.nursePractitioner, item.resolvedNursePractitionerId)}</TableCell>
                <TableCell>{renderCaseIdWithBadge(item.hospitalization, item.resolvedHospitalizationId)}</TableCell>
                <TableCell>{`${item.visit?.room ?? ''} ${item.visit?.bed ?? ''}`.trim()}</TableCell>
                <TableCell>{item.visit?.imported ? 'Yes' : 'No'}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  );
}
