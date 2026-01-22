import api from '@/shared/core/api/ApiClient';
import type { Employees } from '@/types/api.generated';
import type {
  EmployeeId,
  EmployeeWithVisitCount,
  CreateEmployeeDto
} from '@/features/employees/employee.types';

/**
 * Employee Service
 * Handles all employee-related API operations
 */
class EmployeeService {
  private readonly baseUrl = '/employees';

  /**
   * Get all employees
   * @returns {Promise<Employees.EmployeeDto[]>} List of all employees
   */
  async getAll(): Promise<Employees.EmployeeDto[]> {
    const response = await api.get<Employees.EmployeeDto[]>(this.baseUrl);
    return response.data;
  }

  /**
   * Get employee by ID
   * @param {number} id - Employee ID
   * @returns {Promise<Employees.EmployeeDto>} Employee data
   */
  async getById(id: EmployeeId): Promise<Employees.EmployeeDto> {
    const response = await api.get<Employees.EmployeeDto>(`${this.baseUrl}/${id}`);
    return response.data;
  }

  /**
   * Get employee by email
   * @param {string} email - Employee email
   * @returns {Promise<Employees.EmployeeDto>} Employee data
   */
  async getByEmail(email: string): Promise<Employees.EmployeeDto> {
    const response = await api.get<Employees.EmployeeDto>(`${this.baseUrl}/email/${email}`);
    return response.data;
  }

  /**
   * Get employees with visit count for a specific date
   * @param {string} date - Date in format YYYY-MM-DD
   * @returns {Promise<EmployeeWithVisitCount[]>} List of employees with visit counts
   */
  async getEmployeesWithVisitCount(date: string): Promise<EmployeeWithVisitCount[]> {
    const response = await api.get<EmployeeWithVisitCount[]>(`${this.baseUrl}/visit-counts`, {
      params: { date }
    });
    return response.data;
  }

  /**
   * Create a new employee
   * @param {CreateEmployeeDto} data - Employee data
   * @returns {Promise<Employees.EmployeeDto>} Created employee data
   */
  async create(data: CreateEmployeeDto): Promise<Employees.EmployeeDto> {
    const response = await api.post<Employees.EmployeeDto>(this.baseUrl, data);
    return response.data;
  }

  /**
   * Update an existing employee
   */
  async update(id: EmployeeId, data: Employees.UpdateEmployeeRequest): Promise<Employees.EmployeeDto> {
    const response = await api.put<Employees.EmployeeDto>(`${this.baseUrl}/${id}`, data);
    return response.data;
  }

  /**
   * Delete an employee
   * @param {number} id - Employee ID
   * @returns {Promise<boolean>} True if deleted successfully
   */
  async delete(id: EmployeeId): Promise<boolean> {
    const response = await api.delete<boolean>(`${this.baseUrl}/${id}`);
    return response.data;
  }
}

// Export singleton instance
export const employeeService = new EmployeeService();
