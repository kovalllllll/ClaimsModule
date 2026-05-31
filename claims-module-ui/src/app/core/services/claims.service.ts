import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, map, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ClaimDetail,
  ClaimListFilters,
  ClaimSummary,
  ClaimClosureConditionsResult,
  CreateClaimCommand,
  CreateClaimResult,
  PagedResult,
  TransitionStatusCommand,
  ValidateClaimIntakeResult,
} from '../models';
import { NotificationService } from './notification.service';
import { extractApiErrorMessage } from '../utils/api-error.util';
import { resolveDocumentDownloadUrls } from '../utils/storage-url.util';

const documentStorageUrlOptions = { useLocalUploadProxy: !environment.production };

@Injectable({ providedIn: 'root' })
export class ClaimsService {
  private readonly baseUrl = `${environment.apiUrl}/api/claims`;

  constructor(
    private readonly http: HttpClient,
    private readonly notification: NotificationService
  ) {}

  getAllClaims(filters: ClaimListFilters = {}): Observable<PagedResult<ClaimSummary>> {
    let params = new HttpParams()
      .set('pageNumber', String(filters.pageNumber ?? 1))
      .set('pageSize', String(filters.pageSize ?? 20));

    if (filters.statuses?.length) {
      for (const status of filters.statuses) {
        params = params.append('statuses', status);
      }
    } else if (filters.status) {
      params = params.set('status', filters.status);
    }
    if (filters.dateFrom) {
      params = params.set('dateFrom', filters.dateFrom);
    }
    if (filters.dateTo) {
      params = params.set('dateTo', filters.dateTo);
    }
    if (filters.causeOfLossCode) {
      params = params.set('causeOfLossCode', filters.causeOfLossCode);
    }
    if (filters.search) {
      params = params.set('search', filters.search);
    }
    if (filters.assignedHandlerSearch?.trim()) {
      params = params.set('assignedHandlerSearch', filters.assignedHandlerSearch.trim());
    } else if (filters.assignedHandlerId) {
      params = params.set('assignedHandlerId', filters.assignedHandlerId);
    }

    return this.http
      .get<PagedResult<ClaimSummary>>(this.baseUrl, { params })
      .pipe(catchError((error) => this.handleError(error)));
  }

  getClaimById(id: string): Observable<ClaimDetail> {
    return this.http
      .get<ClaimDetail>(`${this.baseUrl}/${id}`)
      .pipe(
        map((claim) => ({
          ...claim,
          documents: resolveDocumentDownloadUrls(
            claim.documents,
            environment.apiUrl,
            documentStorageUrlOptions
          ),
        })),
        catchError((error) => this.handleError(error))
      );
  }

  validateIntake(command: CreateClaimCommand): Observable<ValidateClaimIntakeResult> {
    return this.http
      .post<ValidateClaimIntakeResult>(`${this.baseUrl}/validate`, command)
      .pipe(catchError((error) => this.handleError(error)));
  }

  createClaim(command: CreateClaimCommand): Observable<CreateClaimResult> {
    return this.http
      .post<CreateClaimResult>(this.baseUrl, command, {
        headers: { 'Idempotency-Key': crypto.randomUUID() },
      })
      .pipe(catchError((error) => this.handleError(error)));
  }

  transitionStatus(id: string, command: TransitionStatusCommand, rowVer?: string): Observable<void> {
    const headers: Record<string, string> = {};
    if (rowVer) {
      headers['If-Match'] = rowVer;
    }
    return this.http
      .put<void>(`${this.baseUrl}/${id}/status`, command, { headers })
      .pipe(catchError((error) => this.handleError(error)));
  }

  linkPolicy(id: string, policyId: string): Observable<void> {
    return this.http
      .patch<void>(`${this.baseUrl}/${id}/policy`, { policyId })
      .pipe(catchError((error) => this.handleError(error)));
  }

  updateNotes(id: string, notes: string | null): Observable<void> {
    return this.http
      .patch<void>(`${this.baseUrl}/${id}/notes`, { notes })
      .pipe(catchError((error) => this.handleError(error)));
  }

  getClosureConditions(id: string, reason?: string): Observable<ClaimClosureConditionsResult> {
    let params = new HttpParams();
    if (reason) {
      params = params.set('reason', reason);
    }
    return this.http
      .get<ClaimClosureConditionsResult>(`${this.baseUrl}/${id}/closure-conditions`, { params })
      .pipe(catchError((error) => this.handleError(error)));
  }

  private handleError(error: unknown): Observable<never> {
    this.notification.error(extractApiErrorMessage(error));
    return throwError(() => error);
  }
}
