import api from '@/services/api/ApiClient';
import type { 
  Employee, 
  EmployeeWithVisitCount, 
  CreateEmployeeDto, 
  UpdateEmployeeDto 
} from '@/types/employee';

const EMPLOYEES_BASE_URL = '/employees';

export const employeeService = {
  /**
   * Get all employees
   * @returns {Promise<Employee[]>} List of all employees
   */
  getAll: async (): Promise<Employee[]> => {
    return await api.get<Employee[]>(EMPLOYEES_BASE_URL);
  },

  /**
   * Get employee by ID
   * @param {number} id - Employee ID
   * @returns {Promise<Employee>} Employee data
   */
  getById: async (id: number): Promise<Employee> => {
    return await api.get<Employee>(`${EMPLOYEES_BASE_URL}/${id}`);
  },

  /**
   * Get employee by email
   * @param {string} email - Employee email
   * @returns {Promise<Employee>} Employee data
   */
  getByEmail: async (email: string): Promise<Employee> => {
    return await api.get<Employee>(`${EMPLOYEES_BASE_URL}/email/${email}`);
  },

  /**
   * Get employees with visit count for a specific date
   * @param {string} date - Date in format YYYY-MM-DD
   * @returns {Promise<EmployeeWithVisitCount[]>} List of employees with visit counts
   */
  getEmployeesWithVisitCount: async (date: string): Promise<EmployeeWithVisitCount[]> => {
    return await api.get<EmployeeWithVisitCount[]>(`${EMPLOYEES_BASE_URL}/visit-counts`, {
      params: { date }
    });
  },

  /**
   * Create a new employee
   * @param {CreateEmployeeDto} data - Employee data
   * @returns {Promise<Employee>} Created employee data
   */
  create: async (data: CreateEmployeeDto): Promise<Employee> => {
    return await api.post<Employee>(EMPLOYEES_BASE_URL, data);
  },

  /**
   * Update an existing employee
   * @param {number} id - Employee ID
   * @param {UpdateEmployeeDto} data - Updated employee data
   * @returns {Promise<Employee>} Updated employee data
   */
  update: async (id: number, data: UpdateEmployeeDto): Promise<Employee> => {
    return await api.put<Employee>(`${EMPLOYEES_BASE_URL}/${id}`, data);
  },

  /**
   * Delete an employee
   * @param {number} id - Employee ID
   * @returns {Promise<boolean>} True if deleted successfully
   */
  delete: async (id: number): Promise<boolean> => {
    return await api.delete<boolean>(`${EMPLOYEES_BASE_URL}/${id}`);
  },
};
