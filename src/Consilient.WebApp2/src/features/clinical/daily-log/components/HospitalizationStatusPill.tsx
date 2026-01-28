import { useHospitalizationStatusById } from '@/shared/stores/HospitalizationStatusStore';

interface HospitalizationStatusPillProps {
  statusId: number;
}

export function HospitalizationStatusPill({ statusId }: HospitalizationStatusPillProps) {
  const status = useHospitalizationStatusById(statusId);
  if (!status) {
    return null;
  }

  return (
    <span
      className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
      style={{ backgroundColor: status.color, color: '#000' }}
    >
      {status.code}
    </span>
  );
}