import { useState, useEffect } from "react";
import type { DailyLogVisit } from '../dailylog.types';
import type { StatusChangeEvent } from '../dailylog.types'; // Ensure StatusChangeEvent is imported
import { getDailyLogService } from '../services/DailyLogServiceFactory';
import PatientTimeline from './PatientTimeline';
import { calculateAge, diffInDays, formatDate } from "@/shared/utils/utils";
  
interface DailyLogAdditionalInfoProps {
  visit: DailyLogVisit | null;
}

const dailyLogService = getDailyLogService();

export default function DailyLogAdditionalInfo({ visit }: DailyLogAdditionalInfoProps) {
  const [timelineData, setTimelineData] = useState<StatusChangeEvent[]>([]);

  useEffect(() => {
    if (visit) {
      dailyLogService.getPatientTimelineData(visit.hospitalizationId)
        .then((d) => { console.log("Fetched timeline data:", d); setTimelineData(d); })
        .catch((error) => {
          console.error("Failed to fetch patient timeline data:", error);
        });
    }
  }, [visit]);

  if (!visit) return null;
  const age = calculateAge(visit.patientDateOfBirth);
  const daysHospitalized = diffInDays(visit.hospitalizationAdmissionDate);

  return <>
    <div className="w-full min-w-0 min-h-0">
      <div className="p-4" style={{borderBottom: '1px solid gray'}}>
        <button
          className="mb-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition"
          type="button"
        >
        Generate
      </button>
      </div>
      <div className="flex flex-col md:flex-row gap-4 p-4">
        <div id='demographic-details' className="md:w-1/2">
          <h2 className="text-base font-semibold text-gray-800">Demographic Details</h2>
          <div>
            <div>
              <span className="font-semibold text-gray-900">Age:</span>
              <span className="ml-1">{age} {age === 1 ? 'year' : 'years'} old</span>
            </div>
            <div>
                <span className="font-semibold text-gray-900">Gender:</span>
                <span className="ml-1 text-blue-700">{visit.patientGender}</span>
            </div>
          </div>
        </div>
        <div id='hospitalization-details' className="md:w-1/2">
          <h2 className="text-base font-semibold text-gray-800">Hospitalization Details</h2>
          <div>
            <div>
              <span className="font-semibold text-gray-900">MRN:</span>
              <span className="ml-1 text-blue-700">{visit.patientMRN}</span>
            </div>
            <div>
              <span className="font-semibold text-gray-900">Hospitalization ID:</span>
              <span className="ml-1 text-blue-700">{visit.hospitalizationId}</span>
            </div>
            {visit.hospitalizationAdmissionDate && daysHospitalized != null && (
              <div>
                <span className="font-semibold text-gray-900">Admission Date:</span>
                <span className="ml-1">{formatDate(visit.hospitalizationAdmissionDate)}</span>
                <span className="ml-2 font-semibold text-gray-900">LOS:</span>
                <span className="ml-1">{daysHospitalized} {daysHospitalized === 1 ? 'day' : 'days'}</span>
              </div>
            )}
            <div>
              <span className="font-semibold text-gray-900">Room:</span>
              <span className="ml-1 text-blue-700">{visit.room}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div className="w-full p-4 min-w-0 min-h-0">
      <h2 className="text-base font-semibold text-gray-800">Patient Progress</h2>
      <PatientTimeline statusChanges={timelineData} />
    </div>
  </>
}