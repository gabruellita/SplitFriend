import { axiosClient } from './axiosClient';
import { API_ENDPOINTS } from './endpoints';
import type { ExportReportRequest } from '@/types/export.types';

export const exportApi = {
  /** Cere PDF-ul, il salveaza ca fisier (download in browser). */
  generateReport: async (request: ExportReportRequest): Promise<void> => {
    const response = await axiosClient.post(API_ENDPOINTS.EXPORT.REPORT, request, {
      responseType: 'blob',
    });

    // numele din Content-Disposition, fallback generic
    const disposition = response.headers['content-disposition'] as string | undefined;
    const match = disposition?.match(/filename="?([^"]+)"?/);
    const fileName = match?.[1] ?? 'raport-financiar.pdf';

    const url = window.URL.createObjectURL(response.data as Blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    a.remove();
    window.URL.revokeObjectURL(url);
  },
};
