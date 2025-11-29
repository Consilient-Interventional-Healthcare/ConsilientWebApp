import type { DailyLogVisit } from '../dailylog.types';
import PatientTimeline from './PatientTimeline';
import { calculateAge, diffInDays, formatDate } from "@/shared/utils/utils";
  
interface DailyLogAdditionalInfoProps {
  visit: DailyLogVisit | null;
}

export default function DailyLogAdditionalInfo({ visit }: DailyLogAdditionalInfoProps) {
  if (!visit) return null;
  const age = calculateAge(visit.patientDateOfBirth);
  const daysHospitalized = diffInDays(visit.hospitalizationAdmissionDate);
    return <>
    <div className="w-full p-4 min-w-0 min-h-0">
      <h2 className="text-base font-semibold text-gray-800">Other Relevant Details</h2>
      {age != null && (
        <div>
            <div>
                <span className="font-semibold text-gray-900">Age:</span>
                <span className="ml-1">{age} {age === 1 ? 'year' : 'years'} old</span>
            </div>
            <div>
                <span className="font-semibold text-gray-900">MRN:</span>
                <span className="ml-1 text-blue-700">{visit.patientMRN}</span>
            </div>
            <div>
                <span className="font-semibold text-gray-900">Hospitalization ID:</span>
                <span className="ml-1 text-blue-700">{visit.hospitalizationId}</span>
            </div>
        </div>
      )}
      {visit.hospitalizationAdmissionDate && daysHospitalized != null && (
        <div>
          <span className="font-semibold text-gray-900">Admission Date:</span>
          <span className="ml-1">{formatDate(visit.hospitalizationAdmissionDate)}</span>
          <span className="ml-2 font-semibold text-gray-900">LOS:</span>
          <span className="ml-1">{daysHospitalized} {daysHospitalized === 1 ? 'day' : 'days'}</span>
        </div>
      )}
    </div>
    <div className="w-full p-4 min-w-0 min-h-0">
      <h2 className="text-base font-semibold text-gray-800">Patient Progress</h2>
      <PatientTimeline />
    </div>
      </>
}