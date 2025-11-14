import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import type { GroupedEmployee } from "@/hooks/useEmployeeAssignmentSummary";

interface EmployeeVisitTableProps {
  employees: GroupedEmployee[];
}

export default function EmployeeVisitTable({ employees }: EmployeeVisitTableProps) {
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
