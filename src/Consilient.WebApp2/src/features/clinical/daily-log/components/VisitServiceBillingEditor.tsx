import { useState } from 'react';
import { Button } from '@/shared/components/ui/button';
import { Plus, Trash2 } from 'lucide-react';
import { useToast } from '@/shared/hooks/useToast';
import { visitServiceBillingService } from '../services/VisitServiceBillingService';
import { AddServiceBillingForm } from './AddServiceBillingForm';
import type { VisitServiceBillings } from '@/types/api.generated';
import type { VisitServiceBillingInfo } from '../types/visitServiceBilling.types';

type CreateRequest = Omit<VisitServiceBillings.CreateVisitServiceBillingRequest, 'visitId'>;

interface VisitServiceBillingEditorProps {
  visitId: number;
  serviceBillings: VisitServiceBillingInfo[];
  onBillingAdded?: (() => void) | undefined;
  onBillingDeleted?: (() => void) | undefined;
}

export function VisitServiceBillingEditor({
  visitId,
  serviceBillings,
  onBillingAdded,
  onBillingDeleted,
}: VisitServiceBillingEditorProps) {
  const [showAddForm, setShowAddForm] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [deletingId, setDeletingId] = useState<number | null>(null);
  const { success, error } = useToast();

  const handleAdd = async (request: CreateRequest) => {
    setIsSubmitting(true);
    try {
      await visitServiceBillingService.create(visitId, request);
      success('Service billing added');
      setShowAddForm(false);
      onBillingAdded?.();
    } catch (err) {
      error('Failed to add service billing');
      console.error('Failed to add service billing:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async (id: number) => {
    setDeletingId(id);
    try {
      await visitServiceBillingService.delete(visitId, id);
      success('Service billing removed');
      onBillingDeleted?.();
    } catch (err) {
      error('Failed to remove service billing');
      console.error('Failed to remove service billing:', err);
    } finally {
      setDeletingId(null);
    }
  };

  return (
    <div className="space-y-3">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h2 className="text-base font-semibold text-gray-800">Service Billing</h2>
        {!showAddForm && (
          <Button
            size="sm"
            variant="outline"
            onClick={() => setShowAddForm(true)}
            className="h-7 text-xs"
          >
            <Plus className="h-3 w-3 mr-1" />
            Add
          </Button>
        )}
      </div>

      {/* Add Form */}
      {showAddForm && (
        <AddServiceBillingForm
          onSubmit={handleAdd}
          onCancel={() => setShowAddForm(false)}
          isSubmitting={isSubmitting}
        />
      )}

      {/* Table */}
      {serviceBillings.length > 0 ? (
        <div className="border rounded-md overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="text-left px-3 py-2 font-medium text-gray-600">
                  Service Type
                </th>
                <th className="text-left px-3 py-2 font-medium text-gray-600">
                  Billing Code
                </th>
                <th className="w-10 px-2 py-2"></th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {serviceBillings.map((billing) => (
                <tr key={billing.id} className="hover:bg-gray-50">
                  <td className="px-3 py-2 text-gray-900">
                    {billing.serviceTypeName}
                  </td>
                  <td className="px-3 py-2 text-gray-900">
                    <span className="font-mono text-xs bg-gray-100 px-1 rounded mr-1">
                      {billing.billingCodeCode}
                    </span>
                    {billing.billingCodeDescription}
                  </td>
                  <td className="px-2 py-2">
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => handleDelete(billing.id)}
                      disabled={deletingId === billing.id}
                      className="h-7 w-7 p-0 text-gray-400 hover:text-red-600 hover:bg-red-50"
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        !showAddForm && (
          <p className="text-sm text-gray-500 italic">
            No billing codes assigned to this visit.
          </p>
        )
      )}
    </div>
  );
}
