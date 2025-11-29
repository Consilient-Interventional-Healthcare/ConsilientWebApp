import type { HospitalizationStatus } from '@/types/db.types';;
import { dataProvider } from '@/data/DataProvider';
interface HospitalizationStatusPillProps {
  statusId: HospitalizationStatus['id'];
}

export function HospitalizationStatusPill({ statusId }: HospitalizationStatusPillProps) {
  const [status = null] = dataProvider.query<HospitalizationStatus>('SELECT * FROM hospitalizationStatuses WHERE id = ?', [statusId]);

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