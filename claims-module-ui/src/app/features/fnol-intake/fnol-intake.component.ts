import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import {
  FormArray,
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { debounceTime, distinctUntilChanged, switchMap, of, merge } from 'rxjs';
import { ClaimsService } from '../../core/services/claims.service';
import { PolicyService } from '../../core/services/policy.service';
import { ReferenceService } from '../../core/services/reference.service';
import { NotificationService } from '../../core/services/notification.service';
import {
  AssetType,
  CauseOfLossCode,
  ClaimSeverity,
  CreateClaimCommand,
  CreateClaimPartyInput,
  CreateClaimRiskObjectInput,
  PartyRole,
  PartyType,
  PolicySummary,
  ReserveComponentType,
  ValidateClaimIntakeResult,
} from '../../core/models';
import {
  getAuthorityLabel,
  getReserveAuthorityLevel,
} from '../../core/utils/authority.util';
import {
  isValidReserveTransactionAmount,
  reserveTransactionAmountValidator,
  shouldShowReserveAmountErrors,
} from '../../core/utils/reserve-amount.util';
import { extractApiErrorMessage } from '../../core/utils/api-error.util';

@Component({
  selector: 'app-fnol-intake',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    CurrencyPipe,
    DatePipe,
    MatStepperModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatCheckboxModule,
    MatAutocompleteModule,
    MatChipsModule,
    MatIconModule,
    MatCardModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatTableModule,
  ],
  templateUrl: './fnol-intake.component.html',
  styleUrl: './fnol-intake.component.scss',
})
export class FnolIntakeComponent implements OnInit {
  private readonly fb = inject(FormBuilder);

  @ViewChild('stepper') private stepper?: MatStepper;

  causeCodes: CauseOfLossCode[] = [];
  filteredCauseCodes: CauseOfLossCode[] = [];
  readonly causeFilterCtrl = new FormControl('', { nonNullable: true });
  policyResults: PolicySummary[] = [];
  selectedPolicy: PolicySummary | null = null;
  submitting = false;
  stepBannerError: string | null = null;
  readonly reviewColumns = ['label', 'value'] as const;
  readonly partyRoles: PartyRole[] = [
    'Claimant',
    'Insured',
    'ThirdParty',
    'Witness',
    'Attorney',
  ];
  readonly partyTypes: PartyType[] = ['Person', 'Company'];
  readonly assetTypes: AssetType[] = ['Vehicle', 'Property', 'Person', 'Equipment', 'Other'];
  readonly reserveComponents: ReserveComponentType[] = [
    'Indemnity',
    'Expense',
    'ALAE',
    'SubrogationRecoverable',
  ];
  readonly claimSeverityOptions: ClaimSeverity[] = [
    'Minor',
    'Standard',
    'Critical',
    'Catastrophic',
  ];

  readonly hourOptions = Array.from({ length: 24 }, (_, hour) => hour);
  readonly minuteOptions = Array.from({ length: 60 }, (_, minute) => minute);

  readonly step1Form = this.fb.group({
    policySearch: [''],
    unknownPolicy: [false],
    policyId: [null as string | null],
    policyNumber: [''],
    clientName: [''],
    effectiveDate: [''],
    expirationDate: [''],
    lossDate: [null as Date | null, Validators.required],
    lossHour: [new Date().getHours(), Validators.required],
    lossMinute: [new Date().getMinutes(), Validators.required],
    causeOfLossCode: ['', Validators.required],
    lossDescription: ['', [Validators.required, Validators.minLength(20)]],
    lossLocation: [''],
    estimatedLossAmount: [null as number | null, Validators.min(0)],
    severity: ['Standard' as ClaimSeverity],
  });

  readonly partyForm = this.fb.group({
    partyRole: ['Claimant' as PartyRole, Validators.required],
    partyType: ['Person' as PartyType, Validators.required],
    firstName: [''],
    lastName: [''],
    companyName: [''],
    email: ['', Validators.email],
    phone: [''],
  });

  readonly riskForm = this.fb.group({
    assetType: ['Vehicle' as AssetType, Validators.required],
    assetDescription: ['', Validators.required],
    damageDescription: [''],
    assetReference: [''],
  });

  readonly parties = this.fb.array([] as ReturnType<typeof this.createPartyGroup>[]);
  readonly riskObjects = this.fb.array([] as ReturnType<typeof this.createRiskGroup>[]);

  readonly step2Form = this.fb.group({
    step2Complete: [false, Validators.requiredTrue],
  });

  readonly step3Form = this.fb.group({
    includeReserve: [false],
    componentType: ['Indemnity' as ReserveComponentType],
    amount: [null as number | null],
    changeReason: ['Initial reserve'],
  });

  constructor(
    private readonly claimsService: ClaimsService,
    private readonly policyService: PolicyService,
    private readonly referenceService: ReferenceService,
    private readonly notification: NotificationService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.referenceService.getCauseOfLossCodes().subscribe((codes) => {
      this.causeCodes = codes;
      this.filterCauseCodes(this.causeFilterCtrl.value);
    });

    this.causeFilterCtrl.addValidators(this.causeOfLossSelectionRequired);
    this.causeFilterCtrl.valueChanges.subscribe((term) => {
      this.filterCauseCodes(term);
      this.onCauseFilterInput(term);
    });

    this.step1Form
      .get('policySearch')
      ?.valueChanges.pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((q) => (q && q.length >= 2 && !this.step1Form.value.unknownPolicy
          ? this.policyService.searchPolicies(q)
          : of(null)))
      )
      .subscribe((result) => {
        this.policyResults = result?.items ?? [];
      });

    this.step1Form.get('unknownPolicy')?.valueChanges.subscribe((unknown) => {
      if (unknown) {
        this.selectedPolicy = null;
        this.step1Form.patchValue({
          policyId: null,
          policyNumber: '',
          clientName: '',
        });
      }
      this.step1Form.get('policyId')?.updateValueAndValidity();
    });

    this.step1Form.get('policyId')?.addValidators(this.policyIdRequired);
    this.step1Form.get('lossDate')?.addValidators(this.notFutureLossDateTime);
    this.step1Form.get('policyId')?.updateValueAndValidity();

    merge(
      this.step1Form.get('lossHour')!.valueChanges,
      this.step1Form.get('lossMinute')!.valueChanges
    ).subscribe(() => {
      this.step1Form.get('lossDate')?.updateValueAndValidity({ emitEvent: false });
    });

    this.syncStep2Validity();

    this.step3Form
      .get('amount')
      ?.addValidators(
        reserveTransactionAmountValidator(
          () => this.step3Form.get('componentType')?.value as ReserveComponentType | null
        )
      );

    this.step3Form.get('componentType')?.valueChanges.subscribe(() => {
      this.step3Form.get('amount')?.updateValueAndValidity();
    });
    this.step3Form.get('includeReserve')?.valueChanges.subscribe((include) => {
      const amountControl = this.step3Form.get('amount');
      if (include === true) {
        amountControl?.addValidators(Validators.required);
      } else {
        amountControl?.removeValidators(Validators.required);
      }
      amountControl?.updateValueAndValidity();
    });
  }

  showReserveAmountErrors(): boolean {
    return shouldShowReserveAmountErrors(this.step3Form.get('amount'));
  }

  get hasClaimant(): boolean {
    return this.parties.controls.some((p) => p.value.partyRole === 'Claimant');
  }

  get reserveAuthorityLabel(): string {
    const amount = this.step3Form.value.amount ?? 0;
    return amount !== 0 ? getAuthorityLabel(amount) : '';
  }

  get reserveRequiresApproval(): boolean {
    const amount = this.step3Form.value.amount ?? 0;
    return amount !== 0 && getReserveAuthorityLevel(amount) !== 'auto';
  }

  get reviewRows(): { label: string; value: string }[] {
    const s1 = this.step1Form.getRawValue();
    const s3 = this.step3Form.getRawValue();
    const causeName =
      this.causeCodes.find((c) => c.code === s1.causeOfLossCode)?.name ?? s1.causeOfLossCode ?? '—';
    const partySummary = this.parties.controls.map((_, i) => this.partyDisplay(i)).join('; ') || '—';
    const riskSummary =
      this.riskObjects.controls.map((_, i) => this.riskDisplay(i)).join('; ') || '—';
    let initialReserve = 'None';
    if (s3.includeReserve && s3.amount != null && s3.amount !== 0) {
      initialReserve = `${s3.componentType}: ${s3.amount} (${this.reserveAuthorityLabel || 'pending'})`;
    }

    return [
      { label: 'Policy', value: s1.policyNumber || (s1.unknownPolicy ? 'Unknown' : 'None') },
      { label: 'Client', value: s1.clientName || '—' },
      {
        label: 'Policy effective',
        value:
          this.selectedPolicy && !s1.unknownPolicy
            ? `${this.formatDate(this.selectedPolicy.effectiveDate)} – ${this.formatDate(this.selectedPolicy.expirationDate)}`
            : '—',
      },
      {
        label: 'Coverage types',
        value:
          this.selectedPolicy?.coverageTypes?.length && !s1.unknownPolicy
            ? this.selectedPolicy.coverageTypes.join(', ')
            : '—',
      },
      {
        label: 'Loss date & time',
        value: this.formatLossDateTimeSummary(
          s1.lossDate as Date | null,
          s1.lossHour as number,
          s1.lossMinute as number
        ),
      },
      { label: 'Cause of loss', value: causeName },
      { label: 'Description', value: s1.lossDescription || '—' },
      { label: 'Location', value: s1.lossLocation || '—' },
      {
        label: 'Estimated loss',
        value: s1.estimatedLossAmount != null ? String(s1.estimatedLossAmount) : '—',
      },
      { label: 'Severity', value: s1.severity ?? 'Standard' },
      { label: 'Parties', value: `${this.parties.length}: ${partySummary}` },
      { label: 'Risk objects', value: `${this.riskObjects.length}: ${riskSummary}` },
      { label: 'Initial reserve', value: initialReserve },
    ];
  }

  advanceFromStep1(): void {
    this.stepBannerError = null;
    this.resolveCauseOfLossFromFilter();
    this.step1Form.markAllAsTouched();
    this.causeFilterCtrl.markAsTouched();
    this.syncCauseFilterValidity();
    this.step1Form.updateValueAndValidity();

    if (!this.step1Form.valid) {
      this.stepBannerError = this.getStep1ValidationMessage();
      return;
    }

    this.stepper?.next();
  }

  onCauseFilterBlur(): void {
    this.resolveCauseOfLossFromFilter();
    this.causeFilterCtrl.markAsTouched();
    this.syncCauseFilterValidity();
  }

  private onCauseFilterInput(term: string): void {
    const code = this.step1Form.get('causeOfLossCode')?.value?.trim();
    if (!code) {
      this.step1Form.patchValue({ causeOfLossCode: '' }, { emitEvent: false });
      this.syncCauseFilterValidity();
      return;
    }

    const selected = this.causeCodes.find((c) => c.code === code);
    const expected = selected ? `${selected.code} — ${selected.name}` : '';
    if (term.trim() !== expected) {
      this.step1Form.patchValue({ causeOfLossCode: '' }, { emitEvent: false });
    }
    this.syncCauseFilterValidity();
  }

  private syncCauseFilterValidity(): void {
    this.causeFilterCtrl.updateValueAndValidity({ emitEvent: false });
    this.step1Form.get('causeOfLossCode')?.updateValueAndValidity({ emitEvent: false });
  }

  private resolveCauseOfLossFromFilter(): void {
    const term = this.causeFilterCtrl.value.trim();
    if (!term) {
      this.step1Form.patchValue({ causeOfLossCode: '' }, { emitEvent: false });
      this.syncCauseFilterValidity();
      return;
    }

    const exactCode = this.causeCodes.find((c) => c.code.toLowerCase() === term.toLowerCase());
    if (exactCode) {
      this.applyCauseOfLossSelection(exactCode);
      return;
    }

    const exactLabel = this.causeCodes.find(
      (c) => `${c.code} — ${c.name}`.toLowerCase() === term.toLowerCase()
    );
    if (exactLabel) {
      this.applyCauseOfLossSelection(exactLabel);
      return;
    }

    const singleMatch = this.causeCodes.filter(
      (c) =>
        c.code.toLowerCase().includes(term.toLowerCase()) ||
        c.name.toLowerCase().includes(term.toLowerCase())
    );
    if (singleMatch.length === 1) {
      this.applyCauseOfLossSelection(singleMatch[0]);
    }
  }

  private applyCauseOfLossSelection(code: CauseOfLossCode): void {
    this.step1Form.patchValue({ causeOfLossCode: code.code });
    this.causeFilterCtrl.setValue(`${code.code} — ${code.name}`, { emitEvent: false });
    this.syncCauseFilterValidity();
  }

  private getStep1ValidationMessage(): string {
    const policyId = this.step1Form.get('policyId');
    const lossDate = this.step1Form.get('lossDate');
    const cause = this.step1Form.get('causeOfLossCode');
    const description = this.step1Form.get('lossDescription');

    if (policyId?.hasError('policyRequired')) {
      return 'Select a policy from the search results, or enable Unknown policy.';
    }
    if (lossDate?.hasError('required')) {
      return 'Loss date is required.';
    }
    if (lossDate?.hasError('futureDate')) {
      return 'Loss date and time cannot be in the future.';
    }
    if (cause?.hasError('required') || this.causeFilterCtrl.hasError('required')) {
      return 'Select a cause of loss from the dropdown (click a matching row).';
    }
    if (description?.hasError('required')) {
      return 'Loss description is required.';
    }
    if (description?.hasError('minlength')) {
      return 'Loss description must be at least 20 characters.';
    }
    if (this.step1Form.get('estimatedLossAmount')?.hasError('min')) {
      return 'Estimated loss amount cannot be negative.';
    }

    return 'Please fix the highlighted fields on this step before continuing.';
  }

  get policyInForce(): 'yes' | 'warn' | 'unknown' {
    const lossDate = this.step1Form.value.lossDate as Date | null;
    if (!this.selectedPolicy || !lossDate) {
      return 'unknown';
    }
    const effective = new Date(this.selectedPolicy.effectiveDate);
    const expiration = new Date(this.selectedPolicy.expirationDate);
    const lossOnly = new Date(lossDate.getFullYear(), lossDate.getMonth(), lossDate.getDate());
    if (lossOnly >= effective && lossOnly <= expiration) {
      return 'yes';
    }
    return 'warn';
  }

  selectPolicy(policy: PolicySummary): void {
    this.selectedPolicy = policy;
    this.policyResults = [];
    this.step1Form.patchValue({
      policyId: policy.id,
      policyNumber: policy.policyNumber,
      clientName: policy.clientName,
      effectiveDate: policy.effectiveDate,
      expirationDate: policy.expirationDate,
      policySearch: policy.policyNumber,
    });
    this.step1Form.get('policyId')?.updateValueAndValidity();
  }

  addParty(): void {
    if (this.partyForm.invalid) {
      this.partyForm.markAllAsTouched();
      return;
    }
    this.parties.push(this.createPartyGroup(this.partyForm.getRawValue()));
    this.partyForm.reset({ partyRole: 'Claimant', partyType: 'Person' });
    this.syncStep2Validity();
  }

  removeParty(index: number): void {
    this.parties.removeAt(index);
    this.syncStep2Validity();
  }

  addRiskObject(): void {
    if (this.riskForm.invalid) {
      this.riskForm.markAllAsTouched();
      return;
    }
    this.riskObjects.push(this.createRiskGroup(this.riskForm.getRawValue()));
    this.riskForm.reset({ assetType: 'Vehicle' });
  }

  removeRisk(index: number): void {
    this.riskObjects.removeAt(index);
  }

  onCauseSelected(event: MatAutocompleteSelectedEvent): void {
    const code = event.option.value as CauseOfLossCode;
    this.applyCauseOfLossSelection(code);
  }

  filterCauseCodes(term: string): void {
    const t = term.trim().toLowerCase();
    if (!t) {
      this.filteredCauseCodes = [...this.causeCodes];
      return;
    }
    this.filteredCauseCodes = this.causeCodes.filter(
      (c) => c.code.toLowerCase().includes(t) || c.name.toLowerCase().includes(t)
    );
  }

  syncStep2Validity(): void {
    this.step2Form.patchValue({ step2Complete: this.hasClaimant });
    this.step2Form.updateValueAndValidity();
  }

  partyDisplay(index: number): string {
    const p = this.parties.at(index).value;
    const name = this.partyName(index);
    return `${p.partyRole}: ${name}`;
  }

  partyName(index: number): string {
    const p = this.parties.at(index).value;
    if (p.partyType === 'Company') {
      return p.companyName?.trim() || 'Company';
    }
    return [p.firstName, p.lastName].filter(Boolean).join(' ').trim() || 'Person';
  }

  partyContact(index: number): string {
    const p = this.parties.at(index).value;
    return [p.email, p.phone].filter((v) => !!v?.trim()).join(' · ');
  }

  riskDisplay(index: number): string {
    const r = this.riskObjects.at(index).value;
    return `${r.assetType}: ${r.assetDescription}`;
  }

  riskSecondaryLine(index: number): string {
    const r = this.riskObjects.at(index).value;
    const parts = [r.damageDescription, r.assetReference].filter((v) => !!v?.trim());
    return parts.join(' · ');
  }

  createClaim(): void {
    this.stepBannerError = null;
    this.step1Form.get('causeOfLossCode')?.markAsTouched();
    if (!this.step1Form.valid || !this.hasClaimant) {
      this.step1Form.markAllAsTouched();
      if (this.step1Form.get('lossDate')?.hasError('futureDate')) {
        this.stepBannerError = 'Loss date and time cannot be in the future.';
      } else if (this.step1Form.get('policyId')?.hasError('policyRequired')) {
        this.stepBannerError =
          'Select a policy from search results, or enable Unknown policy.';
      }
      return;
    }

    const command = this.buildCreateCommand();
    this.submitting = true;
    this.claimsService.validateIntake(command).subscribe({
      next: (result) => this.handleValidationResult(result, command),
      error: (err) => {
        this.submitting = false;
        this.stepBannerError = this.formatStepError(err);
      },
    });
  }

  private handleValidationResult(
    result: ValidateClaimIntakeResult,
    command: CreateClaimCommand
  ): void {
    if (!result.canCreate) {
      const criticalMessages = result.issues
        .filter((i) => i.severity === 'Critical')
        .map((i) => i.message);
      this.submitting = false;
      this.stepBannerError =
        criticalMessages.length > 0
          ? criticalMessages.join(' ')
          : 'Fix critical validation issues before creating the claim.';
      return;
    }

    const warningMessages = result.issues
      .filter((i) => i.severity === 'Warning')
      .map((i) => i.message);

    const openHint =
      !result.canOpen && result.issues.some((i) => i.field === 'Parties')
        ? '\n\nNote: Claim can be created in Draft but cannot move to Open until a Claimant is added.'
        : '';

    if (warningMessages.length) {
      const confirmed = window.confirm(
        `Warnings:\n${warningMessages.join('\n')}${openHint}\n\nCreate claim anyway?`
      );
      if (!confirmed) {
        this.submitting = false;
        return;
      }
    } else if (openHint) {
      const confirmed = window.confirm(`${openHint.trim()}\n\nCreate claim anyway?`);
      if (!confirmed) {
        this.submitting = false;
        return;
      }
    }

    this.submitCreate(command);
  }

  private buildCreateCommand(): CreateClaimCommand {
    const s1 = this.step1Form.getRawValue();
    const lossDate = this.combineLossDateTime(
      s1.lossDate as Date,
      s1.lossHour as number,
      s1.lossMinute as number
    );

    const command: CreateClaimCommand = {
      policyId: s1.unknownPolicy ? null : s1.policyId,
      policyNumber: s1.policyNumber || undefined,
      clientName: s1.clientName || undefined,
      lossDate: lossDate.toISOString(),
      lossDescription: s1.lossDescription!,
      lossLocation: s1.lossLocation || undefined,
      causeOfLossCode: s1.causeOfLossCode!,
      estimatedLossAmount: s1.estimatedLossAmount ?? undefined,
      severity: (s1.severity as ClaimSeverity) ?? 'Standard',
      parties: this.parties.getRawValue() as CreateClaimPartyInput[],
      riskObjects: this.riskObjects.getRawValue().map((r, i) => ({
        ...(r as CreateClaimRiskObjectInput),
        isPrimary: i === 0,
      })),
    };

    const s3 = this.step3Form.getRawValue();
    if (
      s3.includeReserve &&
      s3.amount != null &&
      isValidReserveTransactionAmount(s3.componentType ?? undefined, s3.amount)
    ) {
      command.initialReserve = {
        componentType: s3.componentType!,
        amount: s3.amount,
        changeReason: s3.changeReason || 'Initial reserve',
      };
    }

    return command;
  }

  private submitCreate(command: CreateClaimCommand): void {
    this.claimsService.createClaim(command).subscribe({
      next: (result) => {
        this.submitting = false;
        void this.router.navigate(['/claims', result.claimId], {
          state: { createdClaimNumber: result.claimNumber },
        });
      },
      error: (err) => {
        this.submitting = false;
        this.stepBannerError = this.formatStepError(err);
      },
    });
  }

  private createPartyGroup(value: Record<string, unknown>) {
    return this.fb.group({
      partyRole: [value['partyRole'] as PartyRole, Validators.required],
      partyType: [value['partyType'] as PartyType, Validators.required],
      firstName: [value['firstName'] as string],
      lastName: [value['lastName'] as string],
      companyName: [value['companyName'] as string],
      email: [value['email'] as string],
      phone: [value['phone'] as string],
    });
  }

  private createRiskGroup(value: Record<string, unknown>) {
    return this.fb.group({
      assetType: [value['assetType'] as AssetType, Validators.required],
      assetDescription: [value['assetDescription'] as string, Validators.required],
      damageDescription: [value['damageDescription'] as string],
      assetReference: [value['assetReference'] as string],
      isPrimary: [false],
    });
  }

  private formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString();
  }

  private readonly policyIdRequired = (control: AbstractControl): ValidationErrors | null => {
    const parent = control.parent;
    if (!parent || parent.get('unknownPolicy')?.value === true) {
      return null;
    }
    return control.value ? null : { policyRequired: true };
  };

  private readonly causeOfLossSelectionRequired = (): ValidationErrors | null => {
    const code = this.step1Form.get('causeOfLossCode')?.value?.trim();
    return code ? null : { required: true };
  };

  formatTimePart(value: number): string {
    return value.toString().padStart(2, '0');
  }

  private combineLossDateTime(date: Date, hour: number | string, minute: number | string): Date {
    return new Date(
      date.getFullYear(),
      date.getMonth(),
      date.getDate(),
      Number(hour),
      Number(minute),
      0,
      0
    );
  }

  private formatLossDateTimeSummary(
    date: Date | null,
    hour: number | null | undefined,
    minute: number | null | undefined
  ): string {
    if (!date || hour == null || minute == null) {
      return '—';
    }
    return this.combineLossDateTime(date, hour, minute).toLocaleString();
  }

  private readonly notFutureLossDateTime = (control: AbstractControl): ValidationErrors | null => {
    const parent = control.parent;
    if (!parent) {
      return null;
    }
    const date = control.value as Date | null;
    const hour = parent.get('lossHour')?.value as number | null | undefined;
    const minute = parent.get('lossMinute')?.value as number | null | undefined;
    if (!date || hour == null || minute == null) {
      return null;
    }
    const combined = this.combineLossDateTime(date, hour, minute);
    return combined > new Date() ? { futureDate: true } : null;
  };

  private formatStepError(error: unknown): string {
    return extractApiErrorMessage(error);
  }
}
