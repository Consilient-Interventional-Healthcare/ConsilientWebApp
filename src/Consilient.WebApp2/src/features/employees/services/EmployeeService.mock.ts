import type { 
  EmployeeId,
  Employee, 
  EmployeeWithVisitCount, 
  CreateEmployeeDto, 
  UpdateEmployeeDto 
} from '@/features/employees/employee.types';

/**
 * Mock Employee Service
 * Provides mock data for development and testing
 */
class MockEmployeeService {
  
  // Mock data storage
  private mockEmployees: Employee[] = [
    {
      id: 1 as EmployeeId,
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@example.com',
      createdAt: '2025-01-01T00:00:00Z',
      updatedAt: '2025-01-01T00:00:00Z',
    },
    {
      id: 2 as EmployeeId,
      firstName: 'Jane',
      lastName: 'Smith',
      email: 'jane.smith@example.com',
      createdAt: '2025-01-02T00:00:00Z',
      updatedAt: '2025-01-02T00:00:00Z',
    },
    {
      id: 3 as EmployeeId,
      firstName: 'Bob',
      lastName: 'Johnson',
      email: 'bob.johnson@example.com',
      createdAt: '2025-01-03T00:00:00Z',
      updatedAt: '2025-01-03T00:00:00Z',
    },
    {
      id: 4 as EmployeeId,
      firstName: 'Alice',
      lastName: 'Williams',
      email: 'alice.williams@example.com',
      createdAt: '2025-01-04T00:00:00Z',
      updatedAt: '2025-01-04T00:00:00Z',
    },
  ];

  /**
   * Get all employees
   * @returns {Promise<Employee[]>} List of all employees
   */
  async getAll(): Promise<Employee[]> {
    // Simulate API delay
    await this.delay(300);
    return [...this.mockEmployees];
  }

  /**
   * Get employee by ID
   * @param {number} id - Employee ID
   * @returns {Promise<Employee>} Employee data
   */
  async getById(id: EmployeeId): Promise<Employee> {
    await this.delay(200);
    const employee = this.mockEmployees.find(emp => emp.id === id);
    
    if (!employee) {
      throw new Error(`Employee with ID ${id} not found`);
    }
    
    return { ...employee };
  }

  /**
   * Get employee by email
   * @param {string} email - Employee email
   * @returns {Promise<Employee>} Employee data
   */
  async getByEmail(email: string): Promise<Employee> {
    await this.delay(200);
    const employee = this.mockEmployees.find(emp => emp.email === email);
    
    if (!employee) {
      throw new Error(`Employee with email ${email} not found`);
    }
    
    return { ...employee };
  }

  /**
   * Get employees with visit count for a specific date
   * @param {string} date - Date in format YYYY-MM-DD
   * @returns {Promise<EmployeeWithVisitCount[]>} List of employees with visit counts
   */
  async getEmployeesWithVisitCount(date: string): Promise<EmployeeWithVisitCount[]> {
    await this.delay(400);
    
    // Generate mock visit counts based on employee ID and date
    // This creates consistent but varied data
    const dateHash = this.hashString(date);
    
    return this.mockEmployees.map((employee) => {
      // Create pseudo-random visit count based on date and employee
      const seed = dateHash + employee.id;
      const visitCount = this.generateVisitCount(seed);
      
      return {
        employeeId: employee.id,
        employeeFirstName: employee.firstName,
        employeeLastName: employee.lastName,
        visitCount,
      };
    });
  }

  /**
   * Create a new employee
   * @param {CreateEmployeeDto} data - Employee data
   * @returns {Promise<Employee>} Created employee data
   */
  async create(data: CreateEmployeeDto): Promise<Employee> {
    await this.delay(300);
    
    // Check if email already exists
    if (this.mockEmployees.some(emp => emp.email === data.email)) {
      throw new Error(`Employee with email ${data.email} already exists`);
    }
    
    const newEmployee: Employee = {
      id: (Math.max(...this.mockEmployees.map(e => e.id)) + 1) as EmployeeId,
      firstName: data.firstName,
      lastName: data.lastName,
      email: data.email,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
    
    this.mockEmployees.push(newEmployee);
    return { ...newEmployee };
  }

  /**
   * Update an existing employee
   * @param {number} id - Employee ID
   * @param {UpdateEmployeeDto} data - Updated employee data
   * @returns {Promise<Employee>} Updated employee data
   */
  async update(id: EmployeeId, data: UpdateEmployeeDto): Promise<Employee> {
    await this.delay(300);
    
    const employeeIndex = this.mockEmployees.findIndex(emp => emp.id === id);
    
    if (employeeIndex === -1) {
      throw new Error(`Employee with ID ${id} not found`);
    }
    
    const currentEmployee = this.mockEmployees[employeeIndex];
    
    if (!currentEmployee) {
      throw new Error(`Employee with ID ${id} not found`);
    }
    
    // Check if email is being changed to an existing email
    if (data.email && data.email !== currentEmployee.email) {
      if (this.mockEmployees.some(emp => emp.email === data.email)) {
        throw new Error(`Employee with email ${data.email} already exists`);
      }
    }
    
    const updatedEmployee: Employee = {
      id: currentEmployee.id,
      firstName: data.firstName ?? currentEmployee.firstName,
      lastName: data.lastName ?? currentEmployee.lastName,
      email: data.email ?? currentEmployee.email,
      ...(currentEmployee.createdAt && { createdAt: currentEmployee.createdAt }),
      updatedAt: new Date().toISOString(),
    };
    
    this.mockEmployees[employeeIndex] = updatedEmployee;
    return { ...updatedEmployee };
  }

  /**
   * Delete an employee
   * @param {number} id - Employee ID
   * @returns {Promise<boolean>} True if deleted successfully
   */
  async delete(id: EmployeeId): Promise<boolean> {
    await this.delay(300);
    
    const initialLength = this.mockEmployees.length;
    this.mockEmployees = this.mockEmployees.filter(emp => emp.id !== id);
    
    if (this.mockEmployees.length === initialLength) {
      throw new Error(`Employee with ID ${id} not found`);
    }
    
    return true;
  }

  /**
   * Reset mock data to initial state
   */
  resetMockData(): void {
    this.mockEmployees = [
      {
        id: 1 as EmployeeId,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@example.com',
        createdAt: '2025-01-01T00:00:00Z',
        updatedAt: '2025-01-01T00:00:00Z',
      },
      {
        id: 2 as EmployeeId,
        firstName: 'Jane',
        lastName: 'Smith',
        email: 'jane.smith@example.com',
        createdAt: '2025-01-02T00:00:00Z',
        updatedAt: '2025-01-02T00:00:00Z',
      },
      {
        id: 3 as EmployeeId,
        firstName: 'Bob',
        lastName: 'Johnson',
        email: 'bob.johnson@example.com',
        createdAt: '2025-01-03T00:00:00Z',
        updatedAt: '2025-01-03T00:00:00Z',
      },
      {
        id: 4 as EmployeeId,
        firstName: 'Alice',
        lastName: 'Williams',
        email: 'alice.williams@example.com',
        createdAt: '2025-01-04T00:00:00Z',
        updatedAt: '2025-01-04T00:00:00Z',
      },
    ];
  }

  // Helper methods
  
  /**
   * Simulate API delay
   */
  private delay(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  /**
   * Simple string hash function for generating consistent mock data
   */
  private hashString(str: string): number {
    let hash = 0;
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convert to 32-bit integer
    }
    return Math.abs(hash);
  }

  /**
   * Generate a visit count based on a seed value
   * Returns a number between 0 and 10
   */
  private generateVisitCount(seed: number): number {
    // Use seed to generate pseudo-random but consistent visit count
    const random = Math.abs(Math.sin(seed) * 10000);
    return Math.floor(random % 11); // 0-10
  }
}

// Export singleton instance
export const mockEmployeeService = new MockEmployeeService();