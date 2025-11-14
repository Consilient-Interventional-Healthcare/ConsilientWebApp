import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/shared/components/ui/table";
import type { GroupedEmployee } from "@/features/employees/hooks/useEmployeeAssignmentSummary";

interface EmployeeVisitTableProps {
  employees: GroupedEmployee[];
  isLoading?: boolean;
  error?: Error | null;
}

export default function EmployeeVisitTable({ 
  employees, 
  isLoading = false, 
  error = null 
}: EmployeeVisitTableProps) {
  // Loading state
  if (isLoading) {
    return (
      <div className="bg-white rounded-lg shadow p-8">
        <div className="flex flex-col items-center justify-center py-8">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mb-4"></div>
          <p className="text-gray-600 text-sm">Loading employees...</p>
        </div>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="bg-white rounded-lg shadow p-6">
        <div className="flex flex-col items-center text-center py-8">
          <div className="flex items-center justify-center w-12 h-12 mx-auto bg-red-100 rounded-full mb-4">
            <svg className="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <p className="font-semibold text-red-900 mb-2">Failed to load employees</p>
          <p className="text-sm text-red-700">{error.message}</p>
        </div>
      </div>
    );
  }

  // Data rendering
  return (
    <div className="bg-white rounded-lg shadow overflow-hidden">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead className="text-left">Name</TableHead>
            <TableHead className="text-left">Number of Records</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {employees.length === 0 ? (
            <TableRow>
              <TableCell colSpan={2} className="text-center text-gray-500">
                No employees found for this date
              </TableCell>
            </TableRow>
          ) : (
            employees.map((employee) => (
              <TableRow key={`employee-${employee.id}`}>
                <TableCell className="font-medium text-left">
                  {employee.employeeFullName}
                </TableCell>
                <TableCell className="text-left">{employee.recordCount}</TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </div>
  );
}
