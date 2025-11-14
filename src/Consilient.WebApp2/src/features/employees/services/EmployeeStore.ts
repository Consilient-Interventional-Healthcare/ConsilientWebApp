import { useQuery, useMutation, useQueryClient, type UseQueryResult, type UseMutationResult } from '@tanstack/react-query';
import { employeeService } from '@/features/employees/services/EmployeeService';
import { logger } from '@/shared/core/logging/logger';
import type { 
  Employee, 
  EmployeeWithVisitCount, 
  CreateEmployeeDto, 
  UpdateEmployeeDto 
} from '@/features/employees/types/employee';

/**
 * Update employee mutation variables
 */
interface UpdateEmployeeVariables {
  id: EmployeeId;
  data: UpdateEmployeeDto;
}

/**
 * Employee Store
 * Manages React Query hooks for employee data and operations
 */
class EmployeeStore {
  /**
   * Type-safe query key factory for employee-related queries
   * Uses readonly tuples with 'as const' for precise type inference
   * This prevents typos and enables better cache management
   */
  readonly keys = {
    /** Base key for all employee queries */
    all: ['employees'] as const,
    
    /** Key for employee list queries */
    lists: () => [...this.keys.all, 'list'] as const,
    
    /** Key for filtered employee list queries */
    list: (filters?: Record<string, unknown>) => [...this.keys.lists(), { filters }] as const,
    
    /** Base key for employee detail queries */
    details: () => [...this.keys.all, 'detail'] as const,
    
    /** Key for specific employee by ID */
    detail: (id: EmployeeId) => [...this.keys.details(), id] as const,
    
    /** Key for employee lookup by email */
    byEmail: (email: string) => [...this.keys.all, 'email', email] as const,
    
    /** Key for employees with visit counts for a specific date (YYYY-MM-DD) */
    visitCounts: (date: string) => [...this.keys.all, 'visitCounts', date] as const,
  };

  /**
   * Get all employees
   */
  useEmployees(): UseQueryResult<Employee[], Error> {
    return useQuery({
      queryKey: this.keys.lists(),
      queryFn: () => employeeService.getAll(),
    });
  }

  /**
   * Get employee by ID
   * @param id - Employee ID
   */
  useEmployee(id: EmployeeId): UseQueryResult<Employee, Error> {
    return useQuery({
      queryKey: this.keys.detail(id),
      queryFn: () => employeeService.getById(id),
      enabled: !!id,
    });
  }

  /**
   * Get employee by email
   * @param email - Employee email
   */
  useEmployeeByEmail(email: string): UseQueryResult<Employee, Error> {
    return useQuery({
      queryKey: this.keys.byEmail(email),
      queryFn: () => employeeService.getByEmail(email),
      enabled: !!email,
    });
  }

  /**
   * Get employees with visit count for a specific date
   * @param date - Date in format YYYY-MM-DD
   */
  useEmployeesWithVisitCount(date: string): UseQueryResult<EmployeeWithVisitCount[], Error> {
    return useQuery({
      queryKey: this.keys.visitCounts(date),
      queryFn: () => employeeService.getEmployeesWithVisitCount(date),
      enabled: !!date,
    });
  }

  /**
   * Create employee mutation
   */
  useCreateEmployee(): UseMutationResult<Employee, Error, CreateEmployeeDto> {
    const queryClient = useQueryClient();

    return useMutation({
      mutationFn: (data: CreateEmployeeDto) => employeeService.create(data),
      onSuccess: () => {
        void queryClient.invalidateQueries({ queryKey: this.keys.lists() });
      },
      onError: (error) => {
        logger.error('Failed to create employee', error, { component: 'EmployeeStore', action: 'create' });
      },
    });
  }

  /**
   * Update employee mutation
   */
  useUpdateEmployee(): UseMutationResult<Employee, Error, UpdateEmployeeVariables> {
    const queryClient = useQueryClient();

    return useMutation({
      mutationFn: ({ id, data }: UpdateEmployeeVariables) => employeeService.update(id, data),
      onSuccess: (_data, variables) => {
        void queryClient.invalidateQueries({ queryKey: this.keys.lists() });
        void queryClient.invalidateQueries({ queryKey: this.keys.detail(variables.id) });
      },
      onError: (error, variables) => {
        logger.error(`Failed to update employee ${variables.id}`, error, { component: 'EmployeeStore', action: 'update', employeeId: variables.id });
      },
    });
  }

  /**
   * Delete employee mutation
   */
  useDeleteEmployee(): UseMutationResult<boolean, Error, EmployeeId> {
    const queryClient = useQueryClient();

    return useMutation({
      mutationFn: (id: EmployeeId) => employeeService.delete(id),
      onSuccess: () => {
        void queryClient.invalidateQueries({ queryKey: this.keys.lists() });
      },
      onError: (error, id) => {
        logger.error(`Failed to delete employee ${id}`, error, { component: 'EmployeeStore', action: 'delete', employeeId: id });
      },
    });
  }
}

// Export singleton instance
export const employeeStore = new EmployeeStore();

// Export individual hooks for convenience and backward compatibility
export const employeeKeys = employeeStore.keys;
export const useEmployees = () => employeeStore.useEmployees();
export const useEmployee = (id: EmployeeId) => employeeStore.useEmployee(id);
export const useEmployeeByEmail = (email: string) => employeeStore.useEmployeeByEmail(email);
export const useEmployeesWithVisitCount = (date: string) => employeeStore.useEmployeesWithVisitCount(date);
export const useCreateEmployee = () => employeeStore.useCreateEmployee();
export const useUpdateEmployee = () => employeeStore.useUpdateEmployee();
export const useDeleteEmployee = () => employeeStore.useDeleteEmployee();

// Export UpdateEmployeeVariables type for consumers
export type { UpdateEmployeeVariables };
