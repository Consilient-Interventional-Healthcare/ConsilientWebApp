import { useEffect, useState, useRef, type ChangeEvent } from 'react';
import { useNavigate, useLoaderData, Link } from 'react-router-dom';
import { useToast } from '@/shared/hooks/useToast';
import { providerAssignmentsService } from '../../assignments/services/ProviderAssignmentsService';
import { visitService } from '../services/VisitService';
import { facilityService } from '../services/FacilityService';
import { formatDateFromUrl, formatDateToUrl, getToday } from '@/shared/utils/dateUtils';
import { format, differenceInDays } from 'date-fns';
import { GraphQL, type Facilities } from '@/types/api.generated';
import { Table, TableHeader, TableBody, TableRow, TableCell, TableHead } from "@/shared/components/ui/table";
import { DateNavigator } from "@/shared/components/ui/date-navigator";
import { NativeSelect } from "@/shared/components/ui/native-select";
import { HospitalizationStatusPill } from "@/shared/components/HospitalizationStatusPill";
import { RoomBed } from "@/shared/components/RoomBed";

export default function Visits() {
  const { date: urlDate, facilityId } = useLoaderData<{ date: string; facilityId: number | null }>();
  const navigate = useNavigate();
  const { success, error } = useToast();

  const [visits, setVisits] = useState<GraphQL.Visit[]>([]);
  const [facilities, setFacilities] = useState<Facilities.FacilityDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const dateISO = formatDateFromUrl(urlDate);
  const today = getToday();

  // Fetch facilities on mount and set default if none selected
  useEffect(() => {
    facilityService.getAll().then((fetchedFacilities) => {
      setFacilities(fetchedFacilities);
      // Auto-select the first facility if none is selected
      const firstFacility = fetchedFacilities[0];
      if (!facilityId && firstFacility?.id) {
        void navigate(`/clinical/visits/${urlDate}/${firstFacility.id}`, { replace: true });
      }
    }).catch(console.error);
  }, [facilityId, urlDate, navigate]);

  // Fetch visits when date and facilityId change
  useEffect(() => {
    if (!facilityId) {
      setVisits([]);
      return;
    }
    setIsLoading(true);
    visitService.getVisits(dateISO, facilityId)
      .then(setVisits)
      .catch(console.error)
      .finally(() => setIsLoading(false));
  }, [dateISO, facilityId]);

  const handleDateChange = (newDateISO: string) => {
    const newUrlDate = formatDateToUrl(newDateISO);
    void navigate(`/clinical/visits/${newUrlDate}${facilityId ? `/${facilityId}` : ''}`);
  };

  const handleFacilityChange = (newFacilityId: number | null) => {
    void navigate(`/clinical/visits/${urlDate}${newFacilityId ? `/${newFacilityId}` : ''}`);
  };

  const handleImportClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    if (!facilityId) {
      error('Please select a facility before importing.');
      return;
    }

    setIsUploading(true);
    try {
      const result = await providerAssignmentsService.uploadFile(file, dateISO, facilityId);
      success(result.message ?? 'File uploaded successfully');
      void navigate(`/clinical/assignments/${result.batchId}`);
    } catch {
      error('Failed to upload file. Please try again.');
    } finally {
      setIsUploading(false);
      event.target.value = '';
    }
  };

  return (
    <div className="bg-white min-h-screen p-8">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Visits</h1>
        </div>
        <div className="flex items-center gap-4">
          <DateNavigator
            value={dateISO}
            max={today}
            onChange={handleDateChange}
          />
          <NativeSelect
            value={facilityId ?? ''}
            onChange={(e) => handleFacilityChange(e.target.value ? Number(e.target.value) : null)}
            className="w-auto"
          >
            <option value="">Select a facility...</option>
            {facilities.map((f) => (
              <option key={f.id} value={f.id}>{f.name}</option>
            ))}
          </NativeSelect>
          <button
            onClick={handleImportClick}
            disabled={isUploading || !facilityId}
            className="bg-blue-600 text-white px-5 py-2 rounded font-semibold shadow hover:bg-blue-700 transition disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isUploading ? 'Uploading...' : 'Import Provider Assignment'}
          </button>
        </div>
        <input
          ref={fileInputRef}
          type="file"
          accept=".xlsx,.xls,.xlsm"
          onChange={handleFileChange}
          className="hidden"
        />
      </div>
      <div className="p-6">
        {isLoading ? (
          <div className="text-center py-8 text-gray-500">Loading visits...</div>
        ) : !facilityId ? (
          <div className="text-center py-8 text-gray-500">Please select a facility to view visits.</div>
        ) : visits.length === 0 ? (
          <div className="text-center py-8 text-gray-500">No visits found for the selected date and facility.</div>
        ) : (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Providers</TableHead>
                <TableHead>Patient</TableHead>
                <TableHead>Hospitalization Status</TableHead>
                <TableHead>Room / Bed</TableHead>
                <TableHead>Admission Date</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {visits.map((visit) => (
                <TableRow key={visit.id}>
                  <TableCell>
                    {(visit.visitAttendants ?? []).map((attendant) => (
                      <Link
                        key={attendant.id}
                        to={`/clinical/daily-log/${dateISO}/${facilityId}/${attendant.providerId}/${visit.id}`}
                        className="block text-xs text-blue-600 hover:underline"
                      >
                        {attendant.provider?.lastName}, {attendant.provider?.firstName}
                      </Link>
                    ))}
                  </TableCell>
                  <TableCell>
                    <span className="font-semibold">{visit.patient?.firstName} {visit.patient?.lastName}</span>
                    <span className="text-xs text-gray-500 ml-2">MRN: {visit.patient?.mrn}</span>
                    <span className="text-xs text-gray-500 ml-2">Hosp ID: {visit.hospitalization?.id}</span>
                  </TableCell>
                  <TableCell>
                    <HospitalizationStatusPill statusId={visit.hospitalization?.hospitalizationStatusId ?? 0} />
                  </TableCell>
                  <TableCell><RoomBed room={visit.room} bed={visit.bed} /></TableCell>
                  <TableCell>
                    {visit.hospitalization?.admissionDate ? (() => {
                      const admissionDate = new Date(visit.hospitalization.admissionDate);
                      const daysHospitalized = differenceInDays(new Date(), admissionDate);
                      return (
                        <span title={format(admissionDate, 'MMM d, yyyy h:mm a')}>
                          {format(admissionDate, 'MMM d, yyyy')}
                          <span className="text-xs text-gray-500 ml-2">({daysHospitalized} {daysHospitalized === 1 ? 'day' : 'days'})</span>
                        </span>
                      );
                    })() : ''}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        )}
      </div>
    </div>
  );
}
