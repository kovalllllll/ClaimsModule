import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { DatePipe, CurrencyPipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatCardModule } from '@angular/material/card';
import { ClaimsService } from '../../core/services/claims.service';
import { ReferenceService } from '../../core/services/reference.service';
import { ClaimStatus, ClaimSummary, CauseOfLossCode } from '../../core/models';
import { StatusChipComponent } from '../../shared/components/status-chip/status-chip.component';

const ALL_STATUSES: ClaimStatus[] = [
  'Draft',
  'Open',
  'UnderInvestigation',
  'PendingPayment',
  'Closed',
  'Reopened',
  'Withdrawn',
];

@Component({
  selector: 'app-claims-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DatePipe,
    CurrencyPipe,
    MatTableModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    MatCardModule,
    StatusChipComponent,
  ],
  templateUrl: './claims-list.component.html',
  styleUrl: './claims-list.component.scss',
})
export class ClaimsListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);

  readonly displayedColumns = [
    'claimNumber',
    'policyNumber',
    'clientName',
    'lossDate',
    'causeOfLoss',
    'status',
    'totalReserves',
  ];

  readonly statusOptions = ALL_STATUSES;
  causeCodes: CauseOfLossCode[] = [];
  claims: ClaimSummary[] = [];
  totalCount = 0;
  pageSize = 20;
  pageIndex = 0;
  loading = false;

  readonly filterForm = this.fb.group({
    statuses: [[] as ClaimStatus[]],
    dateFrom: [null as Date | null],
    dateTo: [null as Date | null],
    assignedHandlerSearch: [''],
    causeOfLossCode: [''],
    search: [''],
  });

  constructor(
    private readonly claimsService: ClaimsService,
    private readonly referenceService: ReferenceService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.referenceService.getCauseOfLossCodes().subscribe((codes) => {
      this.causeCodes = codes;
    });
    this.loadClaims();
  }

  applyFilters(): void {
    this.pageIndex = 0;
    this.loadClaims();
  }

  clearFilters(): void {
    this.filterForm.reset({
      statuses: [],
      dateFrom: null,
      dateTo: null,
      assignedHandlerSearch: '',
      causeOfLossCode: '',
      search: '',
    });
    this.pageIndex = 0;
    this.loadClaims();
  }

  onPage(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadClaims();
  }

  openClaim(claim: ClaimSummary): void {
    void this.router.navigate(['/claims', claim.id]);
  }

  private loadClaims(): void {
    const value = this.filterForm.getRawValue();
    const statuses = (value.statuses ?? []) as ClaimStatus[];

    this.loading = true;
    this.claimsService
      .getAllClaims({
        statuses: statuses.length ? statuses : undefined,
        dateFrom: value.dateFrom ? this.toIsoStart(value.dateFrom) : undefined,
        dateTo: value.dateTo ? this.toIsoEnd(value.dateTo) : undefined,
        causeOfLossCode: value.causeOfLossCode || undefined,
        assignedHandlerSearch: value.assignedHandlerSearch || undefined,
        search: value.search || undefined,
        pageNumber: this.pageIndex + 1,
        pageSize: this.pageSize,
      })
      .subscribe({
        next: (result) => {
          this.claims = result.items;
          this.totalCount = result.totalCount;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
        },
      });
  }

  private toIsoStart(date: Date): string {
    return new Date(date.getFullYear(), date.getMonth(), date.getDate()).toISOString();
  }

  private toIsoEnd(date: Date): string {
    return new Date(
      date.getFullYear(),
      date.getMonth(),
      date.getDate(),
      23,
      59,
      59
    ).toISOString();
  }
}
