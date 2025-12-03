import { useMemo } from 'react';
import type { EmployeeWithVisitCount, EmployeeId } from '@/features/employees/employee.types';

export interface GroupedEmployee {
  id: EmployeeId;
  employeeFullName: string;
  recordCount: number;
}

/**
 * Hook to group employees by ID and count their visit records
 * @param {EmployeeWithVisitCount[]} employees - Array of employees with visit data
 * @returns {GroupedEmployee[]} Array of employees with aggregated record counts
 */
export function useEmployeeAssignmentSummary(employees: EmployeeWithVisitCount[]): GroupedEmployee[] {
  return useMemo(() => {
    if (!Array.isArray(employees)) return [];
    const grouped = employees.reduce<Record<EmployeeId, GroupedEmployee>>((acc, visit: EmployeeWithVisitCount) => {
      const key = visit.employeeId;
      acc[key] ??= {
        id: key,
        employeeFullName: visit.employeeFirstName + ' ' + visit.employeeLastName,
        recordCount: 0
      };
      acc[key].recordCount += 1;
      return acc;
    }, {} as Record<EmployeeId, GroupedEmployee>);
    return Object.values(grouped);
  }, [employees]);
}
