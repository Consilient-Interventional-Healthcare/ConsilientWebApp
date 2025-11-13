import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { employeeService } from '@/services/employees/employeeService';

// Query keys
export const employeeKeys = {
  all: ['employees'],
  lists: () => [...employeeKeys.all, 'list'],
  list: (filters) => [...employeeKeys.lists(), { filters }],
  details: () => [...employeeKeys.all, 'detail'],
  detail: (id) => [...employeeKeys.details(), id],
  visitCounts: (date) => [...employeeKeys.all, 'visitCounts', date],
};

// Get all employees
export const useEmployees = () => {
  return useQuery({
    queryKey: employeeKeys.lists(),
    queryFn: () => employeeService.getAll(),
  });
};

// Get employee by ID
export const useEmployee = (id) => {
  return useQuery({
    queryKey: employeeKeys.detail(id),
    queryFn: () => employeeService.getById(id),
    enabled: !!id,
  });
};

// Get employee by email
export const useEmployeeByEmail = (email) => {
  return useQuery({
    queryKey: ['employees', 'email', email],
    queryFn: () => employeeService.getByEmail(email),
    enabled: !!email,
  });
};

// Get employees with visit count
export const useEmployeesWithVisitCount = (date) => {
  return useQuery({
    queryKey: employeeKeys.visitCounts(date),
    queryFn: () => employeeService.getEmployeesWithVisitCount(date),
    enabled: !!date,
  });
};

// Create employee mutation
export const useCreateEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data) => employeeService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: employeeKeys.lists() });
    },
  });
};

// Update employee mutation
export const useUpdateEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }) => employeeService.update(id, data),
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({ queryKey: employeeKeys.lists() });
      queryClient.invalidateQueries({ queryKey: employeeKeys.detail(variables.id) });
    },
  });
};

// Delete employee mutation
export const useDeleteEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id) => employeeService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: employeeKeys.lists() });
    },
  });
};