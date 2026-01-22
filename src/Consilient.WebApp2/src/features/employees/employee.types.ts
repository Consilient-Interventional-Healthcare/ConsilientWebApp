// Employee type definitions
import type { Employees } from '@/types/api.generated';

export type EmployeeId = number & { readonly __brand: 'EmployeeId' };

export interface EmployeeWithVisitCount {
  employeeId: EmployeeId;
  employeeFirstName: string;
  employeeLastName: string;
  visitCount?: number;
}

// Re-export API type for create operations
export type CreateEmployeeDto = Employees.CreateEmployeeRequest;

export interface EmployeeVisitCountParams {
  date: string; // YYYY-MM-DD format
}
