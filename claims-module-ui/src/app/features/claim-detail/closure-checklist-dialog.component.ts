import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { ClaimClosureConditionsResult } from '../../core/models';

export interface ClosureDialogData {
  conditions: ClaimClosureConditionsResult;
}

export interface ClosureDialogResult {
  confirmed: boolean;
  reason?: string;
}

@Component({
  selector: 'app-closure-checklist-dialog',
  standalone: true,
  imports: [
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  template: `
    <h2 mat-dialog-title>Close claim — checklist</h2>
    <mat-dialog-content>
      <ul class="checklist">
        @for (item of data.conditions.conditions; track item.code) {
          <li [class.pass]="item.passed" [class.fail]="!item.passed">
            <mat-checkbox [checked]="item.passed" disabled>
              {{ item.code }}: {{ item.description }}
            </mat-checkbox>
            @if (item.detail) {
              <p class="detail">{{ item.detail }}</p>
            }
          </li>
        }
      </ul>
      @if (needsReason) {
        <mat-form-field appearance="outline" class="reason-field">
          <mat-label>Closure justification (CC-04)</mat-label>
          <textarea matInput rows="3" [(ngModel)]="reason"></textarea>
        </mat-form-field>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" (click)="cancel()">Cancel</button>
      <button
        mat-flat-button
        color="primary"
        type="button"
        [disabled]="!canConfirm"
        (click)="confirm()"
      >
        Close claim
      </button>
    </mat-dialog-actions>
  `,
  styles: [
    `
      .checklist {
        list-style: none;
        padding: 0;
        margin: 0 0 1rem;
      }
      .fail {
        color: #b45309;
      }
      .detail {
        margin: 0 0 0.5rem 1.75rem;
        font-size: 0.875rem;
        opacity: 0.85;
      }
      .reason-field {
        width: 100%;
      }
    `,
  ],
})
export class ClosureChecklistDialogComponent {
  reason = '';

  constructor(
    @Inject(MAT_DIALOG_DATA) readonly data: ClosureDialogData,
    private readonly dialogRef: MatDialogRef<ClosureChecklistDialogComponent, ClosureDialogResult>
  ) {}

  get needsReason(): boolean {
    return this.data.conditions.conditions.some((c) => c.code === 'CC-04' && !c.passed);
  }

  get canConfirm(): boolean {
    const blockers = this.data.conditions.conditions.filter(
      (c) => !c.passed && c.code !== 'CC-04'
    );
    if (blockers.length > 0) {
      return false;
    }
    const cc04 = this.data.conditions.conditions.find((c) => c.code === 'CC-04');
    if (cc04 && !cc04.passed && !this.reason.trim()) {
      return false;
    }
    return true;
  }

  cancel(): void {
    this.dialogRef.close({ confirmed: false });
  }

  confirm(): void {
    this.dialogRef.close({ confirmed: true, reason: this.reason.trim() || undefined });
  }
}
