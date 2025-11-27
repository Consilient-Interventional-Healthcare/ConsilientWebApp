export interface Hospitalization {
    admissionDate: string;
    hospitalizationId: string;
    hospitalizationStatusId: HospitalizationStatus['id'];
}
export interface Patient {
    id: string;
    lastName: string;
    firstName: string;
    dateOfBirth: string;
    gender: string;
    patientMRN: string;
}
export interface Assignment {
  patient: Patient
  hospitalization: Hospitalization;
}

export interface ProviderAssignments {
  providerId: string;
  providerLastName: string;
  providerFirstName: string;
  assignments: Assignment[];
}

export interface HospitalizationStatus {
  id: number;
  code: string;
  name: string;
  billingCode?: string;
  color: string;
  order: number;
}

export const HOSPITALIZATION_STATUSES: HospitalizationStatus[] = [
  { id: 1, code: 'DTS', name: 'Acute', billingCode: '99233', color: '#64ffda', order: 1 },
  { id: 2, code: 'DTO', name: 'Acute', billingCode: '99233', color: '#64ffda', order: 2 },
  { id: 3, code: 'GD', name: 'Acute', billingCode: '99233', color: '#64ffda', order: 3 },
  { id: 4, code: 'SND', name: 'Status Next Day', billingCode: '99232', color: '#ffd180', order: 4 },
  { id: 5, code: 'DC', name: 'Discharge Summary', billingCode: '99239', color: '#bbdefb', order: 5 },
  { id: 6, code: 'PP', name: 'Pending Placement', color: '#e0e0e0', order: 6 },
  { id: 7, code: 'TCON-PP', name: 'TCON-PP', color: '#bdbdbd', order: 7 },
  { id: 8, code: 'PE', name: 'Psychiatric Evaluation', billingCode: '90792', color: '#fff176', order: 8 },
];