import { differenceInDays, parseISO, isValid } from 'date-fns';
import { Icon } from '@/shared/components/ui/icon';
import type { Assignment } from '../types/dailylog.types';

interface DailyLogHeaderProps {
  assignment: Assignment | null;
}

export function DailyLogHeader({ assignment }: DailyLogHeaderProps) {
  if (!assignment) return null;
  let age: number | null = null;
  if (assignment.patient.dateOfBirth) {
    const dob = parseISO(assignment.patient.dateOfBirth);
    if (isValid(dob)) {
      age = differenceInDays(new Date(), dob) / 365.25;
      age = Math.floor(age);
    }
  }
  let daysHospitalized: number | null = null;
  if (assignment.hospitalization.admissionDate) {
    const admission = parseISO(assignment.hospitalization.admissionDate);
    if (isValid(admission)) {
      daysHospitalized = differenceInDays(new Date(), admission);
    }
  }
  return (
    <div className="flex-shrink-0 border-b border-gray-200 px-6 py-4 bg-white">
      <h2 className="text-lg font-semibold text-gray-900">
        {assignment.patient.firstName} {assignment.patient.lastName}
      </h2>
      {(assignment.hospitalization.status ?? age ?? assignment.patient.gender ?? assignment.hospitalization.admissionDate) != null && (
        <div className="mt-2 flex flex-wrap gap-1 items-center">
          {assignment.hospitalization.status && (
            <span
              className={
                `inline-flex items-center text-xs font-semibold px-2 py-0.5 rounded-full ` +
                (assignment.hospitalization.status === 'active'
                  ? 'bg-green-100 text-green-800'
                  : assignment.hospitalization.status === 'pending'
                  ? 'bg-yellow-100 text-yellow-800'
                  : 'bg-gray-100 text-gray-800')
              }
            >
              <span className="mr-1 flex items-center justify-center w-4 h-4">
                {assignment.hospitalization.status === 'active' ? (
                  <Icon name="circle-check" className="w-4 h-4 text-green-500" ariaHidden={true} />
                ) : assignment.hospitalization.status === 'pending' ? (
                  <Icon name="clock" className="w-4 h-4 text-yellow-500" ariaHidden={true} />
                ) : (
                  <Icon name="circle-exclamation" className="w-4 h-4 text-gray-500" ariaHidden={true} />
                )}
              </span>
              {assignment.hospitalization.status.charAt(0).toUpperCase() + assignment.hospitalization.status.slice(1)}
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
          {assignment.patient.gender && (
            <span className="inline-flex items-center bg-green-100 text-green-800 text-xs font-semibold px-2 py-0.5 rounded-full">
              <span className="mr-1 flex items-center justify-center w-4 h-4">
                {assignment.patient.gender === 'Male' ? (
                  <Icon name="mars" className="w-4 h-4 text-green-500" ariaHidden={true} />
                ) : assignment.patient.gender === 'Female' ? (
                  <Icon name="venus" className="w-4 h-4 text-green-500" ariaHidden={true} />
                ) : (
                  <Icon name="user" className="w-4 h-4 text-green-500" ariaHidden={true} />
                )}
              </span>
              {assignment.patient.gender}
            </span>
          )}
          {assignment.hospitalization.admissionDate && daysHospitalized != null && (
            <span
              className="inline-flex items-center bg-purple-100 text-purple-800 text-xs font-semibold px-2 py-0.5 rounded-full cursor-help"
              title={`Admitted on ${assignment.hospitalization.admissionDate}`}
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
