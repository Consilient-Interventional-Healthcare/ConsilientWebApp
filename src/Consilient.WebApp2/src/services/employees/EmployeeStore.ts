import { useQuery, useMutation, useQueryClient, type UseQueryResult, type UseMutationResult } from '@tanstack/react-query';
import { employeeService } from '@/services/employees/EmployeeService';
import { logger } from '@/services/logging/logger';
import type { 
  Employee, 
  EmployeeWithVisitCount, 
  CreateEmployeeDto, 
  UpdateEmployeeDto 
} from '@/types/employee';

// Query keys
export const employeeKeys = {
  all: ['employees'] as const,
  lists: () => [...employeeKeys.all, 'list'] as const,
  list: (filters?: unknown) => [...employeeKeys.lists(), { filters }] as const,
  details: () => [...employeeKeys.all, 'detail'] as const,
  detail: (id: number) => [...employeeKeys.details(), id] as const,
  byEmail: (email: string) => [...employeeKeys.all, 'email', email] as const,
  visitCounts: (date: string) => [...employeeKeys.all, 'visitCounts', date] as const,
};

// Get all employees
export const useEmployees = (): UseQueryResult<Employee[], Error> => {
  return useQuery({
    queryKey: employeeKeys.lists(),
    queryFn: () => employeeService.getAll(),
  });
};

// Get employee by ID
export const useEmployee = (id: number): UseQueryResult<Employee, Error> => {
  return useQuery({
    queryKey: employeeKeys.detail(id),
    queryFn: () => employeeService.getById(id),
    enabled: !!id,
  });
};

// Get employee by email
export const useEmployeeByEmail = (email: string): UseQueryResult<Employee, Error> => {
  return useQuery({
    queryKey: employeeKeys.byEmail(email),
    queryFn: () => employeeService.getByEmail(email),
    enabled: !!email,
  });
};

// Get employees with visit count
export const useEmployeesWithVisitCount = (date: string): UseQueryResult<EmployeeWithVisitCount[], Error> => {
  return useQuery({
    queryKey: employeeKeys.visitCounts(date),
    queryFn: () => employeeService.getEmployeesWithVisitCount(date),
    enabled: !!date,
  });
};

// Create employee mutation
export const useCreateEmployee = (): UseMutationResult<Employee, Error, CreateEmployeeDto> => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateEmployeeDto) => employeeService.create(data),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: employeeKeys.lists() });
    },
    onError: (error) => {
      logger.error('Failed to create employee', error, { component: 'EmployeeStore', action: 'create' });
    },
  });
};

// Update employee mutation
interface UpdateEmployeeVariables {
  id: number;
  data: UpdateEmployeeDto;
}

export const useUpdateEmployee = (): UseMutationResult<Employee, Error, UpdateEmployeeVariables> => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: UpdateEmployeeVariables) => employeeService.update(id, data),
    onSuccess: (_data, variables) => {
      void queryClient.invalidateQueries({ queryKey: employeeKeys.lists() });
      void queryClient.invalidateQueries({ queryKey: employeeKeys.detail(variables.id) });
    },
    onError: (error, variables) => {
      logger.error(`Failed to update employee ${variables.id}`, error, { component: 'EmployeeStore', action: 'update', employeeId: variables.id });
    },
  });
};

// Delete employee mutation
export const useDeleteEmployee = (): UseMutationResult<boolean, Error, number> => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: number) => employeeService.delete(id),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: employeeKeys.lists() });
    },
    onError: (error, id) => {
      logger.error(`Failed to delete employee ${id}`, error, { component: 'EmployeeStore', action: 'delete', employeeId: id });
    },
  });
};
