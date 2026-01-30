import { useState } from 'react';
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
}

export function AddServiceBillingForm({
  onSubmit,
  onCancel,
  isSubmitting = false,
}: AddServiceBillingFormProps) {
  const [serviceTypeId, setServiceTypeId] = useState<number | null>(null);
  const [billingCodeId, setBillingCodeId] = useState<number | null>(null);

  const { data: serviceTypes = [], isLoading: loadingServiceTypes } = useServiceTypes();
  const { data: billingCodes = [], isLoading: loadingBillingCodes } = useBillingCodes();

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
            {st.description}
          </option>
        ))}
      </NativeSelect>

      <NativeSelect
        size="sm"
        value={billingCodeId ?? ''}
        onChange={(e) => setBillingCodeId(e.target.value ? Number(e.target.value) : null)}
        disabled={isLoading || isSubmitting}
        className="flex-1 min-w-0"
      >
        <option value="">Select Billing Code...</option>
        {billingCodes.map((bc) => (
          <option key={bc.id} value={bc.id}>
            {bc.code} - {bc.description}
          </option>
        ))}
      </NativeSelect>

      <Button
        size="sm"
        variant="ghost"
        onClick={handleSubmit}
        disabled={!isValid || isSubmitting}
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
