import api from '../api/apiClient';

const EMPLOYEES_BASE_URL = '/employees';

export const employeeService = {
  /**
   * Get all employees
   * @returns {Promise<Array>} List of all employees
   */
  getAll: async () => {
    return await api.get(EMPLOYEES_BASE_URL);
  },

  /**
   * Get employee by ID
   * @param {number} id - Employee ID
   * @returns {Promise<Object|null>} Employee data or null if not found
   */
  getById: async (id) => {
    return await api.get(`${EMPLOYEES_BASE_URL}/${id}`);
  },

  /**
   * Get employee by email
   * @param {string} email - Employee email
   * @returns {Promise<Object|null>} Employee data or null if not found
   */
  getByEmail: async (email) => {
    return await api.get(`${EMPLOYEES_BASE_URL}/email/${email}`);
  },

  /**
   * Get employees with visit count for a specific date
   * @param {string} date - Date in format YYYY-MM-DD
   * @returns {Promise<Array>} List of employees with visit counts
   */
  getEmployeesWithVisitCount: async (date) => {
    return await api.get(`${EMPLOYEES_BASE_URL}/visit-counts`, {
      params: { date }
    });
  },

  /**
   * Create a new employee
   * @param {Object} data - Employee data
   * @param {string} data.firstName - First name
   * @param {string} data.lastName - Last name
   * @param {string} data.email - Email
   * @returns {Promise<Object>} Created employee data
   */
  create: async (data) => {
    return await api.post(EMPLOYEES_BASE_URL, data);
  },

  /**
   * Update an existing employee
   * @param {number} id - Employee ID
   * @param {Object} data - Updated employee data
   * @returns {Promise<Object|null>} Updated employee data or null if not found
   */
  update: async (id, data) => {
    return await api.put(`${EMPLOYEES_BASE_URL}/${id}`, data);
  },

  /**
   * Delete an employee
   * @param {number} id - Employee ID
   * @returns {Promise<boolean>} True if deleted successfully
   */
  delete: async (id) => {
    return await api.delete(`${EMPLOYEES_BASE_URL}/${id}`);
  },
};

export default employeeService;