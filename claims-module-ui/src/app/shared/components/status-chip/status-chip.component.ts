import { Component, Input } from '@angular/core';
import { MatChipsModule } from '@angular/material/chips';
import { formatStatus, statusColor } from '../../../core/constants/claim-status-colors';

@Component({
  selector: 'app-status-chip',
  standalone: true,
  imports: [MatChipsModule],
  template: `
    <mat-chip [style.background-color]="color" [style.color]="'#fff'">
      {{ label }}
    </mat-chip>
  `,
})
export class StatusChipComponent {
  @Input({ required: true }) status!: string;

  get color(): string {
    return statusColor(this.status);
  }

  get label(): string {
    return formatStatus(this.status);
  }
}
