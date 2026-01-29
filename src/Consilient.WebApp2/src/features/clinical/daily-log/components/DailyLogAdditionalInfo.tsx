import { useState, useEffect } from "react";
import type { StatusChangeEvent } from '../services/IDailyLogService';
import type { GraphQL } from "@/types/api.generated";
import { dailyLogService } from '../services/DailyLogService';
import PatientTimeline from './PatientTimeline';
import { RoomBed } from "@/shared/components/RoomBed";
import { calculateAge, diffInDays, formatDate } from "@/shared/utils/utils";

interface DailyLogAdditionalInfoProps {
  visit: GraphQL.DailyLogVisit | null;
}

export default function DailyLogAdditionalInfo({ visit }: DailyLogAdditionalInfoProps) {
  const [timelineData, setTimelineData] = useState<StatusChangeEvent[]>([]);

  useEffect(() => {
    if (visit?.hospitalization?.id) {
      dailyLogService.getPatientTimelineData(visit.hospitalization.id)
        .then(setTimelineData)
        .catch((error) => {
          console.error("Failed to fetch patient timeline data:", error);
        });
    }
  }, [visit]);

  if (!visit) return null;

  const birthDate = visit.patient?.birthDate;
  const admissionDate = visit.hospitalization?.admissionDate;
  const age = birthDate ? calculateAge(birthDate) : null;
  const daysHospitalized = admissionDate ? diffInDays(admissionDate) : null;
  const genderDisplay = visit.patient?.gender ?? 'Unknown';

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
              <span className="ml-1">{age != null ? `${age} ${age === 1 ? 'year' : 'years'} old` : 'Unknown'}</span>
            </div>
            <div>
                <span className="font-semibold text-gray-900">Gender:</span>
                <span className="ml-1 text-blue-700">{genderDisplay}</span>
            </div>
          </div>
        </div>
        <div id='hospitalization-details' className="md:w-1/2">
          <h2 className="text-base font-semibold text-gray-800">Hospitalization Details</h2>
          <div>
            <div>
              <span className="font-semibold text-gray-900">MRN:</span>
              <span className="ml-1 text-blue-700">{visit.patient?.mrn ?? 'N/A'}</span>
            </div>
            <div>
              <span className="font-semibold text-gray-900">Hospitalization ID:</span>
              <span className="ml-1 text-blue-700">{visit.hospitalization?.id ?? 'N/A'}</span>
            </div>
            {admissionDate && daysHospitalized != null && (
              <div>
                <span className="font-semibold text-gray-900">Admission Date:</span>
                <span className="ml-1">{formatDate(admissionDate)}</span>
                <span className="ml-2 font-semibold text-gray-900">LOS:</span>
                <span className="ml-1">{daysHospitalized} {daysHospitalized === 1 ? 'day' : 'days'}</span>
              </div>
            )}
            <div>
              <span className="font-semibold text-gray-900">Room/Bed:</span>
              <RoomBed room={visit.room} bed={visit.bed} className="ml-1 text-blue-700" />
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