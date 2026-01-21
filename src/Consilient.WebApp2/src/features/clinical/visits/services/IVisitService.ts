export interface PatientDto {
  id: number;
  firstName: string;
  lastName: string;
  mrn: string;
}

export interface HospitalizationDto {
  id: number;
  hospitalizationStatusId: number;
  admissionDate: string;
}

export interface AssignedProfessionalDto {
  id: number;
  firstName: string;
  lastName: string;
  role: string;
}

export interface VisitDto {
  id: number;
  dateServiced: string;
  room: string;
  bed: string;
  patient: PatientDto;
  hospitalization: HospitalizationDto;
  assignedProfessionals: AssignedProfessionalDto[];
}

export interface IVisitService {
  getVisits(date: string, facilityId: number): Promise<VisitDto[]>;
}
