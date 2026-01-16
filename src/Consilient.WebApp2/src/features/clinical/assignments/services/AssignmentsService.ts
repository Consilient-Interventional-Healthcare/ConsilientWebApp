import api from '@/shared/core/api/ApiClient';
import type { Assignments } from '@/types/api.generated';

class AssignmentsService {
  private readonly baseUrl = '/assignments';

  async uploadFile(file: File, serviceDate: string, facilityId: number): Promise<Assignments.FileUploadResult> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('serviceDate', serviceDate);
    formData.append('facilityId', facilityId.toString());

    // Remove Content-Type to let browser set multipart/form-data with boundary
    const response = await api.post<Assignments.FileUploadResult>(`${this.baseUrl}/upload`, formData, {
      headers: { 'Content-Type': null as unknown as string }
    });
    return response.data;
  }
}

export const assignmentsService = new AssignmentsService();
