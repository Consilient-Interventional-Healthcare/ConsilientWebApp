import { config } from '@/config';
import { employeeService } from './EmployeeService';
import { mockEmployeeService } from './EmployeeService.mock';

/**
 * Employee Service Factory
 * Exports either the real or mock service based on the VITE_USE_MOCK_SERVICES flag
 * 
 * To use mock services, set VITE_USE_MOCK_SERVICES=true in your .env file
 */

// Export the appropriate service based on the feature flag
export const service = config.features.useMockServices 
  ? mockEmployeeService 
  : employeeService;

// Re-export types for convenience
export type { 
  Employee, 
  EmployeeId,
  EmployeeWithVisitCount,
  CreateEmployeeDto,
  UpdateEmployeeDto,
  EmployeeVisitCountParams
} from '../employee.types';