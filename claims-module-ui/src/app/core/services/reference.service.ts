import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CauseOfLossCode, ClaimStatusTransition } from '../models';
import { NotificationService } from './notification.service';
import { extractApiErrorMessage } from '../utils/api-error.util';

@Injectable({ providedIn: 'root' })
export class ReferenceService {
  constructor(
    private readonly http: HttpClient,
    private readonly notification: NotificationService
  ) {}

  getCauseOfLossCodes(perilCategory?: string): Observable<CauseOfLossCode[]> {
    let params = new HttpParams();
    if (perilCategory) {
      params = params.set('perilCategory', perilCategory);
    }
    return this.http
      .get<CauseOfLossCode[]>(`${environment.apiUrl}/api/reference/cause-of-loss-codes`, { params })
      .pipe(catchError((error) => this.handleError(error)));
  }

  getClaimStatuses(): Observable<ClaimStatusTransition[]> {
    return this.http
      .get<ClaimStatusTransition[]>(`${environment.apiUrl}/api/reference/claim-statuses`)
      .pipe(catchError((error) => this.handleError(error)));
  }

  private handleError(error: unknown): Observable<never> {
    this.notification.error(extractApiErrorMessage(error));
    return throwError(() => error);
  }
}
