import { useEmployeesWithVisitCount } from '@/services/employees/employeeStore';
import EmployeeVisitTable from './EmployeeVisitTable';
import { useMemo } from 'react';

export default function Dashboard() {
  const selectedDate = new Date().toISOString().split('T')[0];
  const p = '2025-03-18';
  
  const { data: employees = [], isLoading, isError, error } = useEmployeesWithVisitCount(p);

  const groupedEmployees = useMemo(() => {
    const grouped = employees.reduce((acc, visit) => {
      const key = visit.employeeId;
      if (!acc[key]) {
        acc[key] = {
          id: key,
          employeeFullName: visit.employeeFirstName + ' ' + visit.employeeLastName,
          recordCount: 0
        };
      }
      acc[key].recordCount += 1;
      return acc;
    }, {});

    return Object.values(grouped);
  }, [employees]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-lg">Loading...</div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-red-600">Error: {error?.message}</div>
      </div>
    );
  }

  return (
    <div className="p-8">
      <div className="mb-6 text-left">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Dashboard</h1>
      </div>

      <div className="grid grid-cols-2 gap-6">
        <section>
          <EmployeeVisitTable employees={groupedEmployees} />
        </section>
      </div>
    </div>
  );
}