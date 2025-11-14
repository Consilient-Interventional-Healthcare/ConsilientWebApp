import { useEmployeesWithVisitCount } from '@/services/employees/EmployeeStore';
import EmployeeVisitTable from '@/views/dashboard/EmployeeVisitTable';
import { useTodayDate } from '@/hooks/useTodayDate';
import { useEmployeeAssignmentSummary } from '@/hooks/useEmployeeAssignmentSummary';
import Loading from '@/components/common/Loading';
import { Button } from '@/components/ui/button';

export default function Dashboard() {
  const today = useTodayDate();
  
  const { data: employees = [], isLoading, isError, error } = useEmployeesWithVisitCount(today);

  const groupedEmployees = useEmployeeAssignmentSummary(employees);

  if (isLoading) {
    return <Loading message="Loading dashboard data..." />;
  }

  if (isError) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="max-w-md bg-red-50 border border-red-200 rounded-lg p-6 shadow-lg">
          <div className="flex items-center justify-center w-12 h-12 mx-auto bg-red-100 rounded-full mb-4">
            <svg className="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <h2 className="text-lg font-semibold text-red-900 text-center mb-2">
            Failed to Load Dashboard
          </h2>
          <p className="text-red-700 text-center text-sm mb-4">
            {error?.message ?? 'An unexpected error occurred while loading the dashboard.'}
          </p>
          <Button 
            onClick={() => window.location.reload()}
            variant="destructive"
            className="w-full"
          >
            Retry
          </Button>
        </div>
      </div>
    );
  }

  if (groupedEmployees.length === 0) {
    return (
      <div className="p-8">
        <div className="mb-6 text-left">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Dashboard</h1>
        </div>
        <div className="flex flex-col items-center justify-center py-12 bg-gray-50 rounded-lg border-2 border-dashed border-gray-300">
          <svg className="w-16 h-16 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No Assignment Data</h3>
          <p className="text-gray-600 text-center max-w-md mb-4">
            There are no employee assignments for today. Check back later or select a different date.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-8">
      <div className="mb-6 text-left">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">Dashboard</h1>
        <p className="text-gray-600">Employee assignment summary for {today}</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <section>
          <EmployeeVisitTable employees={groupedEmployees} />
        </section>
      </div>
    </div>
  );
}
