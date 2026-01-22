import type { Employees } from '@/types/api.generated';
import type {
  EmployeeId,
  EmployeeWithVisitCount,
  CreateEmployeeDto
} from '@/features/employees/employee.types';

/**
 * Mock Employee Service
 * Provides mock data for development and testing
 */
class MockEmployeeService {

  // Mock data storage
  private mockEmployees: Employees.EmployeeDto[] = [
    {
      id: 1,
      firstName: 'John',
      lastName: 'Doe',
      email: 'john.doe@example.com',
      isProvider: true,
      isAdministrator: false,
      canApproveVisits: false,
    },
    {
      id: 2,
      firstName: 'Jane',
      lastName: 'Smith',
      email: 'jane.smith@example.com',
      isProvider: true,
      isAdministrator: false,
      canApproveVisits: true,
    },
    {
      id: 3,
      firstName: 'Bob',
      lastName: 'Johnson',
      email: 'bob.johnson@example.com',
      isProvider: false,
      isAdministrator: true,
      canApproveVisits: true,
    },
    {
      id: 4,
      firstName: 'Alice',
      lastName: 'Williams',
      email: 'alice.williams@example.com',
      isProvider: true,
      isAdministrator: false,
      canApproveVisits: false,
    },
  ];

  /**
   * Get all employees
   */
  async getAll(): Promise<Employees.EmployeeDto[]> {
    // Simulate API delay
    await this.delay(300);
    return [...this.mockEmployees];
  }

  /**
   * Get employee by ID
   */
  async getById(id: EmployeeId): Promise<Employees.EmployeeDto> {
    await this.delay(200);
    const employee = this.mockEmployees.find(emp => emp.id === id);

    if (!employee) {
      throw new Error(`Employee with ID ${id} not found`);
    }

    return { ...employee };
  }

  /**
   * Get employee by email
   */
  async getByEmail(email: string): Promise<Employees.EmployeeDto> {
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
        employeeId: employee.id as EmployeeId,
        employeeFirstName: employee.firstName ?? '',
        employeeLastName: employee.lastName ?? '',
        visitCount,
      };
    });
  }

  /**
   * Create a new employee
   */
  async create(data: CreateEmployeeDto): Promise<Employees.EmployeeDto> {
    await this.delay(300);

    // Check if email already exists
    if (this.mockEmployees.some(emp => emp.email === data.email)) {
      throw new Error(`Employee with email ${data.email} already exists`);
    }

    const newEmployee: Employees.EmployeeDto = {
      id: Math.max(...this.mockEmployees.map(e => e.id)) + 1,
      firstName: data.firstName ?? null,
      lastName: data.lastName ?? null,
      email: data.email ?? null,
      isProvider: data.isProvider,
      isAdministrator: data.isAdministrator,
      canApproveVisits: data.canApproveVisits,
    };

    this.mockEmployees.push(newEmployee);
    return { ...newEmployee };
  }

  /**
   * Update an existing employee
   */
  async update(id: EmployeeId, data: Employees.UpdateEmployeeRequest): Promise<Employees.EmployeeDto> {
    await this.delay(300);

    const employeeIndex = this.mockEmployees.findIndex(emp => emp.id === id);

    if (employeeIndex === -1) {
      throw new Error(`Employee with ID ${id} not found`);
    }

    const currentEmployee = this.mockEmployees[employeeIndex];

    if (!currentEmployee) {
      throw new Error(`Employee with ID ${id} not found`);
    }

    const updatedEmployee: Employees.EmployeeDto = {
      id: currentEmployee.id,
      firstName: data.firstName,
      lastName: data.lastName,
      titleExtension: data.titleExtension ?? null,
      role: data.role !== undefined ? String(data.role) : (currentEmployee.role ?? null),
      canApproveVisits: data.canApproveVisits,
      isProvider: currentEmployee.isProvider,
      isAdministrator: currentEmployee.isAdministrator,
      email: currentEmployee.email ?? null,
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
        id: 1,
        firstName: 'John',
        lastName: 'Doe',
        email: 'john.doe@example.com',
        isProvider: true,
        isAdministrator: false,
        canApproveVisits: false,
      },
      {
        id: 2,
        firstName: 'Jane',
        lastName: 'Smith',
        email: 'jane.smith@example.com',
        isProvider: true,
        isAdministrator: false,
        canApproveVisits: true,
      },
      {
        id: 3,
        firstName: 'Bob',
        lastName: 'Johnson',
        email: 'bob.johnson@example.com',
        isProvider: false,
        isAdministrator: true,
        canApproveVisits: true,
      },
      {
        id: 4,
        firstName: 'Alice',
        lastName: 'Williams',
        email: 'alice.williams@example.com',
        isProvider: true,
        isAdministrator: false,
        canApproveVisits: false,
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