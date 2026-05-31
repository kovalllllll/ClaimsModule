import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface RejectReserveDialogResult {
  confirmed: boolean;
  rejectionReason?: string;
}

@Component({
  selector: 'app-reject-reserve-dialog',
  standalone: true,
  imports: [FormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  template: `
    <h2 mat-dialog-title>Reject reserve</h2>
    <mat-dialog-content>
      <mat-form-field appearance="outline" class="reason-field">
        <mat-label>Rejection reason</mat-label>
        <textarea matInput rows="3" [(ngModel)]="rejectionReason"></textarea>
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" (click)="cancel()">Cancel</button>
      <button
        mat-flat-button
        color="warn"
        type="button"
        [disabled]="!rejectionReason.trim()"
        (click)="confirm()"
      >
        Reject
      </button>
    </mat-dialog-actions>
  `,
  styles: `
    .reason-field {
      width: 100%;
      min-width: 320px;
    }
  `,
})
export class RejectReserveDialogComponent {
  rejectionReason = '';

  constructor(private readonly dialogRef: MatDialogRef<RejectReserveDialogComponent, RejectReserveDialogResult>) {}

  cancel(): void {
    this.dialogRef.close({ confirmed: false });
  }

  confirm(): void {
    const reason = this.rejectionReason.trim();
    if (!reason) {
      return;
    }
    this.dialogRef.close({ confirmed: true, rejectionReason: reason });
  }
}
