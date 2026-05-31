import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent } from '@angular/common/http';
import { Observable, catchError, map, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ClaimDocument, DocumentType } from '../models';
import { NotificationService } from './notification.service';
import { extractApiErrorMessage } from '../utils/api-error.util';
import { resolveDocumentDownloadUrls } from '../utils/storage-url.util';

const documentStorageUrlOptions = { useLocalUploadProxy: !environment.production };

@Injectable({ providedIn: 'root' })
export class DocumentService {
  constructor(
    private readonly http: HttpClient,
    private readonly notification: NotificationService
  ) {}

  getDocuments(claimId: string): Observable<ClaimDocument[]> {
    return this.http
      .get<ClaimDocument[]>(`${environment.apiUrl}/api/claims/${claimId}/documents`)
      .pipe(
        map((documents) =>
          resolveDocumentDownloadUrls(documents, environment.apiUrl, documentStorageUrlOptions)
        ),
        catchError((error) => this.handleError(error))
      );
  }

  uploadDocument(
    claimId: string,
    file: File,
    documentType: DocumentType,
    notes?: string
  ): Observable<HttpEvent<{ documentId: string }>> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('documentType', documentType);
    if (notes) {
      formData.append('notes', notes);
    }

    return this.http
      .post<{ documentId: string }>(
        `${environment.apiUrl}/api/claims/${claimId}/documents`,
        formData,
        {
          headers: { 'Idempotency-Key': crypto.randomUUID() },
          reportProgress: true,
          observe: 'events',
        }
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  private handleError(error: unknown): Observable<never> {
    this.notification.error(extractApiErrorMessage(error));
    return throwError(() => error);
  }
}
