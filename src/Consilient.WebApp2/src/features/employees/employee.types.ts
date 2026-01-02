// Employee type definitions

export type EmployeeId = number & { readonly __brand: 'EmployeeId' };

export interface Employee {
  id: EmployeeId;
  firstName: string;
  lastName: string;
  email: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface EmployeeWithVisitCount {
  employeeId: EmployeeId;
  employeeFirstName: string;
  employeeLastName: string;
  visitCount?: number;
}

export interface CreateEmployeeDto {
  firstName: string;
  lastName: string;
  email: string;
}

export interface UpdateEmployeeDto {
  firstName?: string;
  lastName?: string;
  email?: string;
}

export interface EmployeeVisitCountParams {
  date: string; // YYYY-MM-DD format
}
