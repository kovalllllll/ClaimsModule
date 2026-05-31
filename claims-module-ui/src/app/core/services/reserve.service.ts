import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { OpenReserveCommand, ReserveDetail } from '../models';
import { NotificationService } from './notification.service';
import { extractApiErrorMessage } from '../utils/api-error.util';

@Injectable({ providedIn: 'root' })
export class ReserveService {
  constructor(
    private readonly http: HttpClient,
    private readonly notification: NotificationService
  ) {}

  getReserves(claimId: string): Observable<ReserveDetail> {
    return this.http
      .get<ReserveDetail>(`${environment.apiUrl}/api/claims/${claimId}/reserves`)
      .pipe(catchError((error) => this.handleError(error)));
  }

  openReserve(claimId: string, command: OpenReserveCommand): Observable<unknown> {
    return this.http
      .post(`${environment.apiUrl}/api/claims/${claimId}/reserves`, command, {
        headers: { 'Idempotency-Key': crypto.randomUUID() },
      })
      .pipe(catchError((error) => this.handleError(error)));
  }

  approveReserve(claimId: string, txnId: string, managerOverride = false): Observable<void> {
    return this.http
      .post<void>(
        `${environment.apiUrl}/api/claims/${claimId}/reserves/${txnId}/approve`,
        { managerOverride },
        { headers: { 'Idempotency-Key': crypto.randomUUID() } }
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  rejectReserve(claimId: string, txnId: string, reason: string): Observable<void> {
    return this.http
      .post<void>(
        `${environment.apiUrl}/api/claims/${claimId}/reserves/${txnId}/reject`,
        { rejectionReason: reason },
        { headers: { 'Idempotency-Key': crypto.randomUUID() } }
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  retractReserve(claimId: string, txnId: string): Observable<void> {
    return this.http
      .post<void>(
        `${environment.apiUrl}/api/claims/${claimId}/reserves/${txnId}/retract`,
        {},
        { headers: { 'Idempotency-Key': crypto.randomUUID() } }
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  retryGlPosting(claimId: string, txnId: string): Observable<void> {
    return this.http
      .post<void>(
        `${environment.apiUrl}/api/claims/${claimId}/reserves/transactions/${txnId}/retry-gl`,
        {}
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  private handleError(error: unknown): Observable<never> {
    this.notification.error(extractApiErrorMessage(error));
    return throwError(() => error);
  }
}
