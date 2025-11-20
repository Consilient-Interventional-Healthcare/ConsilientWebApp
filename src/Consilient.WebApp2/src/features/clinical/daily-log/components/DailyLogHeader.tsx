import { differenceInDays, parseISO, isValid } from 'date-fns';
import { Icon } from '@/shared/components/ui/icon';

interface DailyLogHeaderProps {
  patientName: string;
  patientId: string;
  patientDateOfBirth?: string | null;
  patientGender?: string | null;
  admissionDate?: string | null;
  patientStatus?: 'active' | 'pending' | 'completed' | null;
}

export function DailyLogHeader({
  patientName,
  patientId,
  patientDateOfBirth,
  patientGender,
  admissionDate,
  patientStatus,
}: DailyLogHeaderProps) {
  let age: number | null = null;
  if (patientDateOfBirth) {
    const dob = parseISO(patientDateOfBirth);
    if (isValid(dob)) {
      age = differenceInDays(new Date(), dob) / 365.25;
      age = Math.floor(age);
    }
  }
  let daysHospitalized: number | null = null;
  if (admissionDate) {
    const admission = parseISO(admissionDate);
    if (isValid(admission)) {
      daysHospitalized = differenceInDays(new Date(), admission);
    }
  }
  return (
    <div className="flex-shrink-0 border-b border-gray-200 px-6 py-4 bg-white">
      <h2 className="text-lg font-semibold text-gray-900">
        {patientName || patientId}
      </h2>
      {(patientStatus ?? age ?? patientGender ?? admissionDate) != null && (
        <div className="mt-2 flex flex-wrap gap-1 items-center">
          {patientStatus && (
            <span
              className={
                `inline-flex items-center text-xs font-semibold px-2 py-0.5 rounded-full ` +
                (patientStatus === 'active'
                  ? 'bg-green-100 text-green-800'
                  : patientStatus === 'pending'
                  ? 'bg-yellow-100 text-yellow-800'
                  : 'bg-gray-100 text-gray-800')
              }
            >
              <span className="mr-1 flex items-center justify-center w-4 h-4">
                {patientStatus === 'active' ? (
                  <Icon name="circle-check" className="w-4 h-4 text-green-500" ariaHidden={true} />
                ) : patientStatus === 'pending' ? (
                  <Icon name="clock" className="w-4 h-4 text-yellow-500" ariaHidden={true} />
                ) : (
                  <Icon name="circle-exclamation" className="w-4 h-4 text-gray-500" ariaHidden={true} />
                )}
              </span>
              {patientStatus.charAt(0).toUpperCase() + patientStatus.slice(1)}
            </span>
          )}
          {age != null && (
            <span className="inline-flex items-center bg-blue-100 text-blue-800 text-xs font-semibold px-2 py-0.5 rounded-full">
              <span className="mr-1 flex items-center justify-center w-4 h-4">
                <Icon name="user" className="w-4 h-4 text-blue-500" ariaHidden={true} />
              </span>
              {age} {age === 1 ? 'year' : 'years'} old
            </span>
          )}
          {patientGender && (
            <span className="inline-flex items-center bg-green-100 text-green-800 text-xs font-semibold px-2 py-0.5 rounded-full">
              <span className="mr-1 flex items-center justify-center w-4 h-4">
                {patientGender === 'Male' ? (
                  <Icon name="mars" className="w-4 h-4 text-green-500" ariaHidden={true} />
                ) : patientGender === 'Female' ? (
                  <Icon name="venus" className="w-4 h-4 text-green-500" ariaHidden={true} />
                ) : (
                  <Icon name="user" className="w-4 h-4 text-green-500" ariaHidden={true} />
                )}
              </span>
              {patientGender}
            </span>
          )}
          {admissionDate && daysHospitalized != null && (
            <span
              className="inline-flex items-center bg-purple-100 text-purple-800 text-xs font-semibold px-2 py-0.5 rounded-full cursor-help"
              title={`Admitted on ${admissionDate}`}
            >
              <span className="mr-1 flex items-center justify-center w-4 h-4">
                <Icon name="calendar-days" className="w-4 h-4 text-purple-500" ariaHidden={true} />
              </span>
              {`Admitted ${daysHospitalized} ${daysHospitalized === 1 ? 'day' : 'days'} ago`}
            </span>
          )}
        </div>
      )}
    </div>
  );
}
