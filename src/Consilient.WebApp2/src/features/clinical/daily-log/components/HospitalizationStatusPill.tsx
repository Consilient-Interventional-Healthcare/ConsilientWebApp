import type { Hospitalizations } from '@/types/api.generated';
import { dataProvider } from '@/data/DataProvider';
interface HospitalizationStatusPillProps {
  statusId: Hospitalizations.HospitalizationStatusDto['id'];
}

export function HospitalizationStatusPill({ statusId }: HospitalizationStatusPillProps) {
  const [status = null] = dataProvider.query<Hospitalizations.HospitalizationStatusDto>('SELECT * FROM hospitalizationStatuses WHERE id = ?', [statusId]);
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