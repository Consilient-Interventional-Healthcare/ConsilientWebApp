import { HOSPITALIZATION_STATUSES, type HospitalizationStatus } from '../types/dailylog.types';

interface HospitalizationStatusPillProps {
  statusId: HospitalizationStatus['id'];
}

export function HospitalizationStatusPill({ statusId }: HospitalizationStatusPillProps) {
  const status = HOSPITALIZATION_STATUSES.find(s => s.id === statusId);

  if (!status) {
    return null;
  }

  return (
    <span
      className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
      style={{ backgroundColor: status.color, color: '#000' }}
    >
      {status.name}
    </span>
  );
}