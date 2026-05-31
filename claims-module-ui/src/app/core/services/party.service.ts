import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AddPartyCommand } from '../models';
import { NotificationService } from './notification.service';
import { extractApiErrorMessage } from '../utils/api-error.util';

@Injectable({ providedIn: 'root' })
export class PartyService {
  constructor(
    private readonly http: HttpClient,
    private readonly notification: NotificationService
  ) {}

  addParty(claimId: string, command: AddPartyCommand): Observable<{ partyId: string }> {
    return this.http
      .post<{ partyId: string }>(
        `${environment.apiUrl}/api/claims/${claimId}/parties`,
        command,
        { headers: { 'Idempotency-Key': crypto.randomUUID() } }
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  removeParty(claimId: string, partyId: string): Observable<void> {
    return this.http
      .delete<void>(`${environment.apiUrl}/api/claims/${claimId}/parties/${partyId}`)
      .pipe(catchError((error) => this.handleError(error)));
  }

  private handleError(error: unknown): Observable<never> {
    this.notification.error(extractApiErrorMessage(error));
    return throwError(() => error);
  }
}
