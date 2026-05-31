import { Component, OnInit, signal } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { AuthService } from './core/services/auth.service';
import { UserRole } from './core/models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    MatToolbarModule,
    MatButtonModule,
    MatSelectModule,
    MatFormFieldModule,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent implements OnInit {
  readonly roles: UserRole[] = ['handler', 'supervisor', 'manager'];
  readonly selectedRole = signal<UserRole>('handler');
  readonly authReady = signal(false);

  constructor(private readonly auth: AuthService) {}

  ngOnInit(): void {
    const user = this.auth.getCurrentUser();
    if (user) {
      this.selectedRole.set(user.role);
    }
    this.authReady.set(true);
  }

  onRoleChange(role: UserRole): void {
    this.selectedRole.set(role);
    this.auth.switchRole(role).subscribe();
  }

  get currentUserName(): string {
    return this.auth.getCurrentUser()?.name ?? '—';
  }
}
