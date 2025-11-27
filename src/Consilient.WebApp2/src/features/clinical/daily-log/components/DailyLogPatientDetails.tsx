import type { Assignment } from '../types/dailylog.types';
// import PatientProgressGraph from './components/PatientProgressGraph';
import { differenceInDays, parseISO, isValid } from 'date-fns';
import { Icon } from '@/shared/components/ui/icon';

interface DailyLogPatientDetailsProps {
  assignment: Assignment | null;
}

export default function DailyLogPatientDetails({ assignment }: DailyLogPatientDetailsProps) {
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
  return <>
    <div 
        className="inline-flex items-center bg-gray-100 text-gray-800 text-xs font-semibold px-2 py-0.5 rounded-full"
        title={`MRN: ${assignment.patient.patientMRN}`}
    >
        MRN: {assignment.patient.patientMRN}
    </div>
    <div
        className="inline-flex items-center bg-gray-100 text-gray-800 text-xs font-semibold px-2 py-0.5 rounded-full"
        title={`Hospitalization ID: ${assignment.hospitalization.hospitalizationId}`}
    >
        HID: {assignment.hospitalization.hospitalizationId}
    </div>

    {age != null && (
        <div className="inline-flex items-center bg-blue-100 text-blue-800 text-xs font-semibold px-2 py-0.5 rounded-full">
            <div className="mr-1 flex items-center justify-center w-4 h-4">
                <Icon name="user" className="w-4 h-4 text-blue-500" ariaHidden={true} />
            </div>
            {age} {age === 1 ? 'year' : 'years'} old
        </div>
    )}
        {assignment.patient.gender && (
            <div className="inline-flex items-center bg-green-100 text-green-800 text-xs font-semibold px-2 py-0.5 rounded-full">
            <div className="mr-1 flex items-center justify-center w-4 h-4">
                {assignment.patient.gender === 'Male' ? (
                <Icon name="mars" className="w-4 h-4 text-green-500" ariaHidden={true} />
                ) : assignment.patient.gender === 'Female' ? (
                <Icon name="venus" className="w-4 h-4 text-green-500" ariaHidden={true} />
                ) : (
                <Icon name="user" className="w-4 h-4 text-green-500" ariaHidden={true} />
                )}
            </div>
            {assignment.patient.gender}
            </div>
        )}
        {assignment.hospitalization.admissionDate && daysHospitalized != null && (
            <div
            className="inline-flex items-center bg-purple-100 text-purple-800 text-xs font-semibold px-2 py-0.5 rounded-full cursor-help"
            title={`Admitted on ${assignment.hospitalization.admissionDate}`}
            >
            <div className="mr-1 flex items-center justify-center w-4 h-4">
                <Icon name="calendar-days" className="w-4 h-4 text-purple-500" ariaHidden={true} />
            </div>
            {`Admitted ${daysHospitalized} ${daysHospitalized === 1 ? 'day' : 'days'} ago`}
            </div>
        )}
      </>
}