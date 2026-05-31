import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError, of, switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthTokenResponse, AuthUser, UserRole } from '../models';
import { NotificationService } from './notification.service';
import { extractApiErrorMessage } from '../utils/api-error.util';

const ROLE_STORAGE_KEY = 'claims-module-role';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = environment.apiUrl;
  private token: string | null = null;
  private readonly currentUserSignal = signal<AuthUser | null>(null);

  readonly currentUser = this.currentUserSignal.asReadonly();

  constructor(
    private readonly http: HttpClient,
    private readonly notification: NotificationService
  ) {}

  init(): Observable<AuthUser | null> {
    const savedRole = (localStorage.getItem(ROLE_STORAGE_KEY) as UserRole | null) ?? 'handler';
    return this.switchRole(savedRole);
  }

  getToken(): string | null {
    return this.token;
  }

  getCurrentUser(): AuthUser | null {
    return this.currentUserSignal();
  }

  switchRole(role: UserRole): Observable<AuthUser | null> {
    return this.http
      .post<AuthTokenResponse>(`${this.apiUrl}/api/auth/token`, { role })
      .pipe(
        tap((response) => {
          this.token = response.token;
          this.currentUserSignal.set({
            userId: response.userId,
            name: response.name,
            role: response.role,
          });
          localStorage.setItem(ROLE_STORAGE_KEY, role);
        }),
        switchMap((response) =>
          of({
            userId: response.userId,
            name: response.name,
            role: response.role,
          } as AuthUser)
        ),
        catchError((error) => {
          this.notification.error(extractApiErrorMessage(error));
          return throwError(() => error);
        })
      );
  }

  hasRole(...roles: UserRole[]): boolean {
    const user = this.getCurrentUser();
    return !!user && roles.includes(user.role);
  }

  getUsers(): Observable<AuthUser[]> {
    return this.http.get<Array<{ userId: string; name: string; role: UserRole }>>(
      `${this.apiUrl}/api/auth/users`
    ).pipe(
      switchMap((users) =>
        of(users.map((u) => ({ userId: u.userId, name: u.name, role: u.role })))
      ),
      catchError(() => of([]))
    );
  }
}
