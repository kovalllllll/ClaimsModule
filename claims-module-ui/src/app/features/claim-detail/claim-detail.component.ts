import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpEventType } from '@angular/common/http';
import { DatePipe, CurrencyPipe, DecimalPipe, NgClass, AsyncPipe } from '@angular/common';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ClaimsService } from '../../core/services/claims.service';
import { ReserveService } from '../../core/services/reserve.service';
import { PartyService } from '../../core/services/party.service';
import { DocumentService } from '../../core/services/document.service';
import { AuditService } from '../../core/services/audit.service';
import { AuthService } from '../../core/services/auth.service';
import { PolicyService } from '../../core/services/policy.service';
import { NotificationService } from '../../core/services/notification.service';
import {
  AddPartyCommand,
  ClaimDetail,
  ClaimDocument,
  ClaimParty,
  ClaimStatus,
  DocumentType,
  PartyRole,
  PartyType,
  PolicySummary,
  ReserveComponentType,
  ReserveTransaction,
  AuditLogEntry,
  ValidationErrorResponse,
} from '../../core/models';
import { extractApiErrorMessage } from '../../core/utils/api-error.util';
import { StatusChipComponent } from '../../shared/components/status-chip/status-chip.component';
import { getAuthorityLabel } from '../../core/utils/authority.util';
import {
  reserveTransactionAmountValidator,
  shouldShowReserveAmountErrors,
} from '../../core/utils/reserve-amount.util';
import { displayUserName } from '../../core/utils/user-display.util';
import { severityColor } from '../../core/constants/severity-colors';
import {
  ClosureChecklistDialogComponent,
  ClosureDialogResult,
} from './closure-checklist-dialog.component';
import {
  RejectReserveDialogComponent,
  RejectReserveDialogResult,
} from './reject-reserve-dialog.component';

@Component({
  selector: 'app-claim-detail',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    DatePipe,
    CurrencyPipe,
    DecimalPipe,
    NgClass,
    AsyncPipe,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatMenuModule,
    MatPaginatorModule,
    MatChipsModule,
    MatDialogModule,
    MatProgressBarModule,
    StatusChipComponent,
  ],
  templateUrl: './claim-detail.component.html',
  styleUrl: './claim-detail.component.scss',
})
export class ClaimDetailComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);

  claimId = '';
  claim: ClaimDetail | null = null;
  loading = true;
  uploading = false;
  uploadProgress = 0;
  savingNotes = false;
  savingParty = false;
  removingPartyId: string | null = null;
  transitioningStatus = false;
  submittingReserve = false;
  processingTxnId: string | null = null;
  highlightedTxnId: string | null = null;
  selectedTabIndex = 0;

  showPolicySearch = false;
  linkingPolicy = false;
  policySearchResults: PolicySummary[] = [];
  private readonly policySearch$ = new Subject<string>();

  readonly displayUserName = displayUserName;

  auditEntries: import('../../core/models').AuditLogEntry[] = [];
  auditTotal = 0;
  auditPage = 0;
  auditPageSize = 20;

  readonly txnColumns = [
    'createdAt',
    'transactionType',
    'componentType',
    'amount',
    'approvalStatus',
    'submittedBy',
    'approvedBy',
    'postingStatus',
    'actions',
  ];

  readonly auditColumns = ['createdAt', 'eventType', 'description', 'related', 'createdBy'] as const;

  readonly documentColumns = [
    'documentName',
    'documentType',
    'uploadedAt',
    'uploadedBy',
    'fileSizeBytes',
    'notes',
    'actions',
  ] as const;

  readonly partyColumns = [
    'partyRole',
    'partyType',
    'name',
    'status',
    'email',
    'phone',
    'notes',
    'actions',
  ] as const;

  readonly partyRoles: PartyRole[] = [
    'Claimant',
    'Insured',
    'ThirdParty',
    'Witness',
    'Attorney',
  ];
  readonly partyTypes: PartyType[] = ['Person', 'Company'];
  readonly reserveComponents: ReserveComponentType[] = [
    'Indemnity',
    'Expense',
    'ALAE',
    'SubrogationRecoverable',
  ];
  readonly documentTypes: DocumentType[] = [
    'PoliceReport',
    'MedicalReport',
    'Invoice',
    'Other',
  ];

  readonly addPartyForm = this.fb.group({
    partyRole: ['Claimant' as PartyRole, Validators.required],
    partyType: ['Person' as PartyType, Validators.required],
    firstName: [''],
    lastName: [''],
    companyName: [''],
    email: [''],
    phone: [''],
  });

  readonly reserveForm = this.fb.group({
    component: ['Indemnity' as ReserveComponentType, Validators.required],
    amount: [null as number | null, Validators.required],
    changeReason: ['', Validators.required],
  });

  readonly uploadForm = this.fb.group({
    documentType: ['Other' as DocumentType, Validators.required],
    notes: [''],
  });

  readonly notesForm = this.fb.group({
    notes: [''],
  });

  constructor(
    private readonly route: ActivatedRoute,
    private readonly claimsService: ClaimsService,
    private readonly reserveService: ReserveService,
    private readonly partyService: PartyService,
    private readonly documentService: DocumentService,
    private readonly auditService: AuditService,
    private readonly policyService: PolicyService,
    readonly auth: AuthService,
    private readonly notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.showFnolCreatedMessage();

    this.route.paramMap.subscribe((params) => {
      this.claimId = params.get('id') ?? '';
      if (this.claimId) {
        this.loadClaim();
        this.loadAudit();
      }
    });

    this.policySearch$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((q) =>
          q.trim().length >= 2 ? this.policyService.searchPolicies(q) : of({ items: [], totalCount: 0 })
        )
      )
      .subscribe((result) => {
        this.policySearchResults = result.items;
      });

    this.reserveForm
      .get('amount')
      ?.addValidators(
        reserveTransactionAmountValidator(
          () => this.reserveForm.get('component')?.value as ReserveComponentType | null
        )
      );

    this.reserveForm.get('component')?.valueChanges.subscribe(() => {
      this.reserveForm.get('amount')?.updateValueAndValidity();
    });
  }

  showReserveAmountErrors(): boolean {
    return shouldShowReserveAmountErrors(this.reserveForm.get('amount'));
  }

  get primaryLoss() {
    return this.claim?.lossEvents?.[0];
  }

  get reserveAuthorityLabel(): string {
    const amount = this.reserveForm.value.amount ?? 0;
    return amount !== 0 ? getAuthorityLabel(amount) : '';
  }

  get reserveRequiresApproval(): boolean {
    const amount = this.reserveForm.value.amount ?? 0;
    return amount !== 0 && getAuthorityLabel(amount).includes('approval required');
  }

  get allParties(): ClaimParty[] {
    return this.claim?.parties ?? [];
  }

  get activeClaimants(): number {
    return this.allParties.filter((p) => p.isActive && p.partyRole === 'Claimant').length;
  }

  severityChipColor(severity?: string | null): string {
    return severityColor(severity);
  }

  loadClaim(): void {
    this.loading = true;
    this.claimsService.getClaimById(this.claimId).subscribe({
      next: (detail) => {
        this.claim = detail;
        this.notesForm.patchValue({ notes: detail.notes ?? '' });
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  loadAudit(page = 0): void {
    this.auditPage = page;
    this.auditService
      .getAuditLog(this.claimId, page + 1, this.auditPageSize)
      .subscribe((result) => {
        this.auditEntries = result.items;
        this.auditTotal = result.totalCount;
      });
  }

  onAuditPage(event: PageEvent): void {
    this.auditPageSize = event.pageSize;
    this.loadAudit(event.pageIndex);
  }

  copyClaimNumber(): void {
    if (this.claim?.claimNumber) {
      void navigator.clipboard.writeText(this.claim.claimNumber);
      this.notification.success('Claim number copied');
    }
  }

  onPolicySearchInput(event: Event): void {
    const q = (event.target as HTMLInputElement).value;
    this.policySearch$.next(q);
  }

  selectPolicy(policy: PolicySummary): void {
    this.linkingPolicy = true;
    this.policySearchResults = [];
    this.claimsService.linkPolicy(this.claimId, policy.id).subscribe({
      next: () => {
        this.linkingPolicy = false;
        this.showPolicySearch = false;
        this.notification.success(`Policy ${policy.policyNumber} linked`);
        this.loadClaim();
      },
      error: () => {
        this.linkingPolicy = false;
      },
    });
  }

  cancelPolicySearch(): void {
    this.showPolicySearch = false;
    this.policySearchResults = [];
  }

  transitionStatus(target: string): void {
    if (!this.claim) {
      return;
    }

    if (target === 'Closed') {
      this.claimsService.getClosureConditions(this.claimId).subscribe((conditions) => {
        const ref = this.dialog.open(ClosureChecklistDialogComponent, {
          width: '480px',
          data: { conditions },
        });
        ref.afterClosed().subscribe((result: ClosureDialogResult | undefined) => {
          if (result?.confirmed) {
            this.performTransition(target, result.reason);
          }
        });
      });
      return;
    }

    const reason =
      target === 'Withdrawn' || target === 'Reopened'
        ? window.prompt('Reason (required):') ?? ''
        : undefined;

    if ((target === 'Withdrawn' || target === 'Reopened') && !reason?.trim()) {
      return;
    }

    this.performTransition(target, reason || undefined);
  }

  saveNotes(): void {
    const notes = this.notesForm.value.notes ?? '';
    this.savingNotes = true;
    this.claimsService.updateNotes(this.claimId, notes || null).subscribe({
      next: () => {
        this.savingNotes = false;
        this.notification.success('Notes saved');
        this.loadClaim();
      },
      error: () => {
        this.savingNotes = false;
      },
    });
  }

  retryGlPosting(txn: ReserveTransaction): void {
    this.reserveService.retryGlPosting(this.claimId, txn.id).subscribe({
      next: () => {
        this.notification.success('GL posting retry enqueued');
        this.loadClaim();
      },
    });
  }

  canRetryGl(txn: ReserveTransaction): boolean {
    return (
      txn.postingStatus === 'Failed' &&
      this.auth.hasRole('supervisor', 'manager')
    );
  }

  postingChipClass(status: string): string {
    switch (status) {
      case 'Posted':
        return 'gl-posted';
      case 'Failed':
        return 'gl-failed';
      default:
        return 'gl-pending';
    }
  }

  approvalChipClass(status: string): string {
    if (status === 'PendingApproval') {
      return 'approval-pending';
    }
    return '';
  }

  private performTransition(target: string, reason?: string): void {
    const rowVer = this.claim?.rowVer;
    this.transitioningStatus = true;
    this.claimsService
      .transitionStatus(
        this.claimId,
        {
          targetStatus: target as ClaimStatus,
          reason,
        },
        rowVer
      )
      .subscribe({
        next: () => {
          this.transitioningStatus = false;
          this.notification.success(`Status updated to ${target}`);
          this.loadClaim();
        },
        error: (err) => {
          this.transitioningStatus = false;
          if (target === 'Closed' && err instanceof HttpErrorResponse) {
            const body = err.error as ValidationErrorResponse | null;
            if (body?.blockingConditions?.length) {
              this.dialog.open(ClosureChecklistDialogComponent, {
                width: '480px',
                data: { conditions: body.blockingConditions },
              });
              return;
            }
          }
          this.notification.error(extractApiErrorMessage(err));
        },
      });
  }

  addParty(): void {
    if (this.addPartyForm.invalid) {
      return;
    }
    this.savingParty = true;
    this.partyService
      .addParty(this.claimId, this.addPartyForm.getRawValue() as AddPartyCommand)
      .subscribe({
        next: () => {
          this.savingParty = false;
          this.notification.success('Party added');
          this.addPartyForm.reset({ partyRole: 'Claimant', partyType: 'Person' });
          this.loadClaim();
        },
        error: () => {
          this.savingParty = false;
        },
      });
  }

  removeParty(party: ClaimParty): void {
    if (party.partyRole === 'Claimant' && this.activeClaimants <= 1) {
      this.notification.warning('Cannot remove the last Claimant');
      return;
    }
    if (!window.confirm('Remove this party?')) {
      return;
    }
    this.removingPartyId = party.id;
    this.partyService.removeParty(this.claimId, party.id).subscribe({
      next: () => {
        this.removingPartyId = null;
        this.notification.success('Party removed');
        this.loadClaim();
      },
      error: () => {
        this.removingPartyId = null;
      },
    });
  }

  submitReserve(): void {
    if (this.reserveForm.invalid) {
      this.reserveForm.markAllAsTouched();
      return;
    }
    const v = this.reserveForm.getRawValue();
    this.submittingReserve = true;
    this.reserveService
      .openReserve(this.claimId, {
        component: v.component!,
        amount: v.amount!,
        changeReason: v.changeReason!,
      })
      .subscribe({
        next: () => {
          this.submittingReserve = false;
          this.notification.success('Reserve submitted');
          this.reserveForm.reset({ component: 'Indemnity', changeReason: '' });
          this.loadClaim();
        },
        error: () => {
          this.submittingReserve = false;
        },
      });
  }

  canApprove(txn: ReserveTransaction): boolean {
    const user = this.auth.getCurrentUser();
    return (
      txn.approvalStatus === 'PendingApproval' &&
      this.auth.hasRole('supervisor', 'manager') &&
      !!user &&
      txn.submittedByUserId !== user.userId
    );
  }

  canRetract(txn: ReserveTransaction): boolean {
    const user = this.auth.getCurrentUser();
    return (
      txn.approvalStatus === 'PendingApproval' &&
      !!user &&
      txn.submittedByUserId === user.userId
    );
  }

  approveReserve(txn: ReserveTransaction): void {
    const managerOverride =
      txn.amount > 100_000 && this.auth.hasRole('manager')
        ? window.confirm('Apply manager override for high amount?')
        : false;
    this.processingTxnId = txn.id;
    this.reserveService.approveReserve(this.claimId, txn.id, managerOverride).subscribe({
      next: () => {
        this.processingTxnId = null;
        this.notification.success('Reserve approved');
        this.loadClaim();
      },
      error: () => {
        this.processingTxnId = null;
      },
    });
  }

  rejectReserve(txn: ReserveTransaction): void {
    const ref = this.dialog.open(RejectReserveDialogComponent, { width: '420px' });
    ref.afterClosed().subscribe((result: RejectReserveDialogResult | undefined) => {
      if (!result?.confirmed || !result.rejectionReason) {
        return;
      }
      this.processingTxnId = txn.id;
      this.reserveService.rejectReserve(this.claimId, txn.id, result.rejectionReason).subscribe({
        next: () => {
          this.processingTxnId = null;
          this.notification.success('Reserve rejected');
          this.loadClaim();
        },
        error: () => {
          this.processingTxnId = null;
        },
      });
    });
  }

  retractReserve(txn: ReserveTransaction): void {
    this.processingTxnId = txn.id;
    this.reserveService.retractReserve(this.claimId, txn.id).subscribe({
      next: () => {
        this.processingTxnId = null;
        this.notification.success('Reserve retracted');
        this.loadClaim();
      },
      error: () => {
        this.processingTxnId = null;
      },
    });
  }

  downloadDocument(doc: ClaimDocument): void {
    const sasUrl = doc.sasUrl?.trim();
    if (sasUrl && sasUrl !== '#') {
      window.open(sasUrl, '_blank', 'noopener,noreferrer');
      return;
    }

    this.notification.error('Download link is not available. Reload the claim and try again.');
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || this.uploadForm.invalid) {
      return;
    }
    this.uploading = true;
    this.uploadProgress = 0;
    const { documentType, notes } = this.uploadForm.getRawValue();
    this.documentService
      .uploadDocument(this.claimId, file, documentType!, notes || undefined)
      .subscribe({
        next: (event) => {
          if (event.type === HttpEventType.UploadProgress) {
            this.uploadProgress = event.total
              ? Math.round((100 * event.loaded) / event.total)
              : 0;
            return;
          }
          if (event.type === HttpEventType.Response) {
            this.uploading = false;
            this.uploadProgress = 100;
            this.notification.success('Document uploaded');
            input.value = '';
            this.loadClaim();
          }
        },
        error: () => {
          this.uploading = false;
          this.uploadProgress = 0;
        },
      });
  }

  partyName(party: ClaimParty): string {
    if (party.partyType === 'Company') {
      return party.companyName ?? 'Company';
    }
    return [party.firstName, party.lastName].filter(Boolean).join(' ') || 'Person';
  }

  reserveAmountClass(txn: ReserveTransaction): string {
    const delta = txn.newBalance - txn.previousBalance;
    const effectiveDelta = delta !== 0 ? delta : txn.amount;
    if (effectiveDelta > 0) {
      return 'positive';
    }
    if (effectiveDelta < 0) {
      return 'negative';
    }
    return '';
  }

  isTxnRowHighlighted(txn: ReserveTransaction): boolean {
    return this.highlightedTxnId === txn.id;
  }

  focusAuditRelated(entry: AuditLogEntry): void {
    if (entry.relatedEntityType === 'ReserveHistory' && entry.relatedEntityId) {
      this.selectedTabIndex = 2;
      this.highlightedTxnId = entry.relatedEntityId;
      return;
    }
    if (entry.relatedEntityType === 'ClaimDocument' && entry.relatedEntityId) {
      this.selectedTabIndex = 3;
    }
  }

  auditRelatedLabel(entry: AuditLogEntry): string | null {
    if (!entry.relatedEntityId || !entry.relatedEntityType) {
      return null;
    }
    return `${entry.relatedEntityType}`;
  }

  private showFnolCreatedMessage(): void {
    const claimNumber = history.state?.['createdClaimNumber'] as string | undefined;
    if (!claimNumber) {
      return;
    }
    this.notification.success(`Claim ${claimNumber} created`);
    history.replaceState({ ...history.state, createdClaimNumber: undefined }, '');
  }
}
