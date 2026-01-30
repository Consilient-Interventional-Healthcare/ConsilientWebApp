import { useState, useEffect, useMemo } from 'react';
import { NativeSelect } from '@/shared/components/ui/native-select';
import { Button } from '@/shared/components/ui/button';
import { useServiceTypes } from '@/shared/stores/ServiceTypeStore';
import { useBillingCodes } from '@/shared/stores/BillingCodeStore';
import { Check, X } from 'lucide-react';
import type { VisitServiceBillings } from '@/types/api.generated';

type CreateRequest = Omit<VisitServiceBillings.CreateVisitServiceBillingRequest, 'visitId'>;

interface AddServiceBillingFormProps {
  onSubmit: (request: CreateRequest) => Promise<void>;
  onCancel: () => void;
  isSubmitting?: boolean;
  existingBillings?: { serviceTypeId: number; billingCodeId: number }[];
}

export function AddServiceBillingForm({
  onSubmit,
  onCancel,
  isSubmitting = false,
  existingBillings = [],
}: AddServiceBillingFormProps) {
  const [serviceTypeId, setServiceTypeId] = useState<number | null>(null);
  const [billingCodeId, setBillingCodeId] = useState<number | null>(null);

  const { data: serviceTypes = [], isLoading: loadingServiceTypes } = useServiceTypes();
  const { data: billingCodes = [], isLoading: loadingBillingCodes } = useBillingCodes();

  // Get selected service type and filter billing codes
  const selectedServiceType = serviceTypes.find((st) => st.id === serviceTypeId);
  const validCodes = useMemo(
    () => selectedServiceType?.billingCodes.map((bc) => bc.code) ?? [],
    [selectedServiceType]
  );
  const filteredBillingCodes = useMemo(
    () => billingCodes.filter((bc) => validCodes.includes(bc.code)),
    [billingCodes, validCodes]
  );

  // Filter out billing codes already used for this service type on this visit
  const usedBillingCodeIds = useMemo(
    () =>
      existingBillings
        .filter((b) => b.serviceTypeId === serviceTypeId)
        .map((b) => b.billingCodeId),
    [existingBillings, serviceTypeId]
  );

  const availableBillingCodes = useMemo(
    () => filteredBillingCodes.filter((bc) => !usedBillingCodeIds.includes(bc.id)),
    [filteredBillingCodes, usedBillingCodeIds]
  );

  // Check if all billing codes for this service type are already used
  const allCodesUsed = serviceTypeId !== null && availableBillingCodes.length === 0;

  // Auto-select billing code when service type changes
  useEffect(() => {
    if (!selectedServiceType || availableBillingCodes.length === 0) {
      setBillingCodeId(null);
      return;
    }

    const firstCode = availableBillingCodes[0];
    if (availableBillingCodes.length === 1 && firstCode) {
      // Only one option - auto-select it
      setBillingCodeId(firstCode.id);
    } else {
      // Multiple options - find the one marked as default (if still available)
      const defaultAssoc = selectedServiceType.billingCodes.find((bc) => bc.isDefault);
      const defaultBilling = defaultAssoc
        ? availableBillingCodes.find((bc) => bc.code === defaultAssoc.code)
        : undefined;
      setBillingCodeId(defaultBilling?.id ?? null);
    }
  }, [serviceTypeId, selectedServiceType, availableBillingCodes]);

  const isValid = serviceTypeId !== null && billingCodeId !== null;
  const isLoading = loadingServiceTypes || loadingBillingCodes;

  const handleSubmit = async () => {
    if (!isValid) return;
    await onSubmit({
      serviceTypeId: serviceTypeId!,
      billingCodeId: billingCodeId!,
    });
  };

  return (
    <div className="flex items-center gap-2 py-2 px-1 bg-gray-50 rounded">
      <NativeSelect
        size="sm"
        value={serviceTypeId ?? ''}
        onChange={(e) => setServiceTypeId(e.target.value ? Number(e.target.value) : null)}
        disabled={isLoading || isSubmitting}
        className="flex-1 min-w-0"
      >
        <option value="">Select Service Type...</option>
        {serviceTypes.map((st) => (
          <option key={st.id} value={st.id}>
            {st.name}
          </option>
        ))}
      </NativeSelect>

      {allCodesUsed ? (
        <span className="flex-1 min-w-0 text-sm text-amber-600 px-2">
          All billing codes for this service type have been used
        </span>
      ) : (
        <NativeSelect
          size="sm"
          value={billingCodeId ?? ''}
          onChange={(e) => setBillingCodeId(e.target.value ? Number(e.target.value) : null)}
          disabled={isLoading || isSubmitting || !serviceTypeId}
          className="flex-1 min-w-0"
        >
          <option value="">Select Billing Code...</option>
          {availableBillingCodes.map((bc) => (
            <option key={bc.id} value={bc.id}>
              {bc.code} - {bc.description}
            </option>
          ))}
        </NativeSelect>
      )}

      <Button
        size="sm"
        variant="ghost"
        onClick={handleSubmit}
        disabled={!isValid || isSubmitting || allCodesUsed}
        className="h-8 w-8 p-0 text-green-600 hover:text-green-700 hover:bg-green-50"
      >
        <Check className="h-4 w-4" />
      </Button>

      <Button
        size="sm"
        variant="ghost"
        onClick={onCancel}
        disabled={isSubmitting}
        className="h-8 w-8 p-0 text-gray-500 hover:text-gray-700"
      >
        <X className="h-4 w-4" />
      </Button>
    </div>
  );
}
