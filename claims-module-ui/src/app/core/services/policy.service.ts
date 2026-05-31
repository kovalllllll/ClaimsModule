import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult, PolicySummary } from '../models';
import { NotificationService } from './notification.service';
import { extractApiErrorMessage } from '../utils/api-error.util';

@Injectable({ providedIn: 'root' })
export class PolicyService {
  constructor(
    private readonly http: HttpClient,
    private readonly notification: NotificationService
  ) {}

  searchPolicies(query: string): Observable<PagedResult<PolicySummary>> {
    const params = new HttpParams().set('q', query).set('pageSize', '10');
    return this.http
      .get<PagedResult<PolicySummary>>(`${environment.apiUrl}/api/policies/search`, { params })
      .pipe(catchError((error) => this.handleError(error)));
  }

  getCoverageTypes(policyId: string): Observable<{ policyId: string; coverageTypes: string[] }> {
    return this.http
      .get<{ policyId: string; coverageTypes: string[] }>(
        `${environment.apiUrl}/api/policies/${policyId}/coverage`
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  private handleError(error: unknown): Observable<never> {
    this.notification.error(extractApiErrorMessage(error));
    return throwError(() => error);
  }
}
