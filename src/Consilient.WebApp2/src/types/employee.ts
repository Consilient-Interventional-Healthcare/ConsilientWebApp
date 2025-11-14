// Employee type definitions

export interface Employee {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface EmployeeWithVisitCount {
  employeeId: number;
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