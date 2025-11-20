export interface Hospitalization {
    admissionDate: string;
    hospitalizationId: string;
    status: 'active' | 'pending' | 'completed';
}
export interface Patient {
    id: string;
    lastName: string;
    firstName: string;
    dateOfBirth: string;
    gender: string;
}
export interface Assignment {
  id: string;
  patient: Patient
  hospitalization: Hospitalization;
}

export interface ProviderAssignments {
  providerId: string;
  providerLastName: string;
  providerFirstName: string;
  assignments: Assignment[];
}