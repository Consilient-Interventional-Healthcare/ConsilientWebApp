import { Badge } from '@/shared/components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/shared/components/ui/table';
import type { ProviderAssignment } from '../services/ProviderAssignmentsService';

interface AssignmentsTableProps {
  items: ProviderAssignment[];
}

const formatName = (person: { firstName?: string | null; lastName?: string | null } | null): string =>
  person ? `${person.lastName ?? ''}, ${person.firstName ?? ''}`.replace(/^, |, $/g, '') : '';

const isNewEntity = (resolvedId: number | null | undefined, lastName: string | null | undefined): boolean =>
  resolvedId == null && !!lastName;

const parseErrorMessages = (json: string | null | undefined): string[] => {
  if (!json) return [];
  try {
    const errors = JSON.parse(json) as Array<{ message?: string }>;
    return errors.map(e => e.message).filter((m): m is string => !!m);
  } catch {
    return [];
  }
};

export function AssignmentsTable({ items }: AssignmentsTableProps) {
  const renderNameWithBadge = (
    person: { firstName?: string | null; lastName?: string | null } | null | undefined,
    resolvedId: number | null | undefined
  ) => (
    <span className="flex items-center gap-2">
      {formatName(person ?? null)}
      {isNewEntity(resolvedId, person?.lastName) && <Badge variant="success">new</Badge>}
    </span>
  );

  const renderPatientWithBadge = (
    patient: { firstName?: string | null; lastName?: string | null } | null | undefined,
    resolvedId: number | null | undefined
  ) => (
    <span className="flex items-center gap-2">
      {formatName(patient ?? null)}
      {resolvedId == null && <Badge variant="success">new</Badge>}
    </span>
  );

  const renderCaseIdWithBadge = (
    hospitalization: { caseId?: string | null } | null | undefined,
    resolvedId: number | null | undefined
  ) => (
    <span className="flex items-center gap-2">
      {hospitalization?.caseId ?? ''}
      {resolvedId == null && <Badge variant="success">new</Badge>}
    </span>
  );

  if (items.length === 0) {
    return <div className="text-center py-8 text-gray-500">No assignments found</div>;
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Patient</TableHead>
          <TableHead>MRN</TableHead>
          <TableHead>Physician</TableHead>
          <TableHead>Nurse Practitioner</TableHead>
          <TableHead>Hospitalization</TableHead>
          <TableHead>Room</TableHead>
          <TableHead>Imported</TableHead>
          <TableHead>Errors</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {items.map((item: ProviderAssignment) => (
          <TableRow key={item.id}>
            <TableCell>{renderPatientWithBadge(item.patient, item.resolvedPatientId)}</TableCell>
            <TableCell>{item.patient?.mrn ?? ''}</TableCell>
            <TableCell>{renderNameWithBadge(item.physician, item.resolvedPhysicianId)}</TableCell>
            <TableCell>{renderNameWithBadge(item.nursePractitioner, item.resolvedNursePractitionerId)}</TableCell>
            <TableCell>{renderCaseIdWithBadge(item.hospitalization, item.resolvedHospitalizationId)}</TableCell>
            <TableCell>{`${item.visit?.room ?? ''} ${item.visit?.bed ?? ''}`.trim()}</TableCell>
            <TableCell>{item.imported ? 'Yes' : 'No'}</TableCell>
            <TableCell className="text-red-600">
              {parseErrorMessages(item.validationErrorsJson).map((msg, i) => (
                <div key={i}>{msg}</div>
              ))}
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
