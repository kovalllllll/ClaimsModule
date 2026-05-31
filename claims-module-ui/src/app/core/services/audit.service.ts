import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuditLogEntry, PagedResult } from '../models';
import { NotificationService } from './notification.service';
import { extractApiErrorMessage } from '../utils/api-error.util';

@Injectable({ providedIn: 'root' })
export class AuditService {
  constructor(
    private readonly http: HttpClient,
    private readonly notification: NotificationService
  ) {}

  getAuditLog(claimId: string, page = 1, pageSize = 50): Observable<PagedResult<AuditLogEntry>> {
    const params = new HttpParams()
      .set('pageNumber', String(page))
      .set('pageSize', String(pageSize));

    return this.http
      .get<PagedResult<AuditLogEntry>>(
        `${environment.apiUrl}/api/claims/${claimId}/audit`,
        { params }
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  private handleError(error: unknown): Observable<never> {
    this.notification.error(extractApiErrorMessage(error));
    return throwError(() => error);
  }
}
