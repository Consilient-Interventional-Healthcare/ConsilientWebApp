import api from '@/shared/core/api/ApiClient';
import type { 
  Employee, 
  EmployeeWithVisitCount, 
  CreateEmployeeDto, 
  UpdateEmployeeDto 
} from '@/features/employees/types/employee';

/**
 * Employee Service
 * Handles all employee-related API operations
 */
class EmployeeService {
  private readonly baseUrl = '/employees';

  /**
   * Get all employees
   * @returns {Promise<Employee[]>} List of all employees
   */
  async getAll(): Promise<Employee[]> {
    return await api.get<Employee[]>(this.baseUrl);
  }

  /**
   * Get employee by ID
   * @param {number} id - Employee ID
   * @returns {Promise<Employee>} Employee data
   */
  async getById(id: EmployeeId): Promise<Employee> {
    return await api.get<Employee>(`${this.baseUrl}/${id}`);
  }

  /**
   * Get employee by email
   * @param {string} email - Employee email
   * @returns {Promise<Employee>} Employee data
   */
  async getByEmail(email: string): Promise<Employee> {
    return await api.get<Employee>(`${this.baseUrl}/email/${email}`);
  }

  /**
   * Get employees with visit count for a specific date
   * @param {string} date - Date in format YYYY-MM-DD
   * @returns {Promise<EmployeeWithVisitCount[]>} List of employees with visit counts
   */
  async getEmployeesWithVisitCount(date: string): Promise<EmployeeWithVisitCount[]> {
    return await api.get<EmployeeWithVisitCount[]>(`${this.baseUrl}/visit-counts`, {
      params: { date }
    });
  }

  /**
   * Create a new employee
   * @param {CreateEmployeeDto} data - Employee data
   * @returns {Promise<Employee>} Created employee data
   */
  async create(data: CreateEmployeeDto): Promise<Employee> {
    return await api.post<Employee>(this.baseUrl, data);
  }

  /**
   * Update an existing employee
   * @param {number} id - Employee ID
   * @param {UpdateEmployeeDto} data - Updated employee data
   * @returns {Promise<Employee>} Updated employee data
   */
  async update(id: EmployeeId, data: UpdateEmployeeDto): Promise<Employee> {
    return await api.put<Employee>(`${this.baseUrl}/${id}`, data);
  }

  /**
   * Delete an employee
   * @param {number} id - Employee ID
   * @returns {Promise<boolean>} True if deleted successfully
   */
  async delete(id: EmployeeId): Promise<boolean> {
    return await api.delete<boolean>(`${this.baseUrl}/${id}`);
  }
}

// Export singleton instance
export const employeeService = new EmployeeService();
