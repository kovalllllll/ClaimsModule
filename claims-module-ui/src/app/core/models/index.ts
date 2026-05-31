export type UserRole = 'handler' | 'supervisor' | 'manager';

export type ClaimStatus =
  | 'Draft'
  | 'Open'
  | 'UnderInvestigation'
  | 'PendingPayment'
  | 'Closed'
  | 'Reopened'
  | 'Withdrawn';

export type PartyRole = 'Claimant' | 'Insured' | 'ThirdParty' | 'Witness' | 'Attorney';
export type PartyType = 'Person' | 'Company';
export type AssetType = 'Vehicle' | 'Property' | 'Person' | 'Equipment' | 'Other';
export type ReserveComponentType = 'Indemnity' | 'Expense' | 'ALAE' | 'SubrogationRecoverable';
export type DocumentType = 'PoliceReport' | 'MedicalReport' | 'Invoice' | 'Other';
export type ClaimSeverity = 'Catastrophic' | 'Critical' | 'Standard' | 'Minor';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface AuthUser {
  userId: string;
  name: string;
  role: UserRole;
}

export interface AuthTokenResponse {
  token: string;
  userId: string;
  name: string;
  role: UserRole;
  expiresAt: string;
}

export interface ClaimSummary {
  id: string;
  claimNumber: string;
  clientName?: string;
  policyNumber?: string;
  lossDate?: string;
  status: string;
  severity?: string;
  totalReserves: number;
  reportedDate: string;
  createdAt: string;
  assignedHandlerId?: string;
  assignedHandlerName?: string;
  causeOfLossCode?: string;
  causeOfLossName?: string;
}

export interface ClaimListFilters {
  status?: ClaimStatus;
  statuses?: ClaimStatus[];
  dateFrom?: string;
  dateTo?: string;
  assignedHandlerSearch?: string;
  assignedHandlerId?: string;
  causeOfLossCode?: string;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface LossEvent {
  id: string;
  lossDate: string;
  lossDescription: string;
  lossLocation?: string;
  causeOfLossCode: string;
  estimatedLossAmount?: number;
  reportDate: string;
  policeReportNumber?: string;
}

export interface ClaimParty {
  id: string;
  partyRole: string;
  partyType: string;
  firstName?: string;
  lastName?: string;
  companyName?: string;
  email?: string;
  phone?: string;
  notes?: string;
  isActive: boolean;
}

export interface ClaimRiskObject {
  id: string;
  assetType: string;
  assetDescription: string;
  damageDescription?: string;
  isPrimary: boolean;
  assetReference?: string;
}

export interface ReserveComponentSummary {
  id: string;
  componentType: string;
  currentAmount: number;
  status: string;
  hasPendingApproval: boolean;
  pendingAmount?: number | null;
  notes?: string;
}

export interface ReserveTransaction {
  id: string;
  reserveComponentId: string;
  componentType: string;
  transactionType: string;
  amount: number;
  previousBalance: number;
  newBalance: number;
  approvalStatus: string;
  approvedByUserId?: string;
  approvedAt?: string;
  rejectedByUserId?: string;
  rejectedAt?: string;
  rejectionReason?: string;
  changeReason: string;
  postingStatus: string;
  idempotencyKey: string;
  changeSequence: number;
  submittedByUserId?: string;
  createdAt: string;
}

export interface ClaimDocument {
  id: string;
  documentType: string;
  documentName: string;
  contentType: string;
  fileSizeBytes: number;
  uploadedAt: string;
  uploadedByUserId?: string;
  notes?: string;
  sasUrl: string;
}

export interface AuditLogEntry {
  id: string;
  eventType: string;
  description: string;
  oldValue?: string;
  newValue?: string;
  relatedEntityId?: string;
  relatedEntityType?: string;
  correlationId?: string;
  createdAt: string;
  createdByUserId?: string;
}

export interface ClaimDetail {
  id: string;
  claimNumber: string;
  policyId?: string;
  policyNumber?: string;
  clientName?: string;
  status: string;
  validNextStatuses: string[];
  severity?: string;
  reportedDate: string;
  assignedHandlerId?: string;
  assignedHandlerName?: string;
  rowVer?: string;
  closedAt?: string;
  closureReason?: string;
  notes?: string;
  managerOverrideFlag: boolean;
  createdAt: string;
  updatedAt?: string;
  lossEvents: LossEvent[];
  parties: ClaimParty[];
  riskObjects: ClaimRiskObject[];
  reserveComponents: ReserveComponentSummary[];
  reserveTransactions: ReserveTransaction[];
  totalReserves: number;
  documents: ClaimDocument[];
  recentAuditEntries: AuditLogEntry[];
}

export interface ReserveDetail {
  claimId: string;
  components: ReserveComponentSummary[];
  transactions: ReserveTransaction[];
  totalApprovedAmount: number;
}

export interface PolicySummary {
  id: string;
  policyNumber: string;
  clientName: string;
  effectiveDate: string;
  expirationDate: string;
  status: string;
  coverageTypes: string[];
}

export interface CauseOfLossCode {
  id: string;
  code: string;
  name: string;
  perilCategory: string;
  isActive: boolean;
  sortOrder: number;
}

export interface ClaimStatusTransition {
  status: string;
  allowedNextStatuses: string[];
}

export interface CreateClaimPartyInput {
  partyRole: PartyRole;
  partyType: PartyType;
  firstName?: string;
  lastName?: string;
  companyName?: string;
  email?: string;
  phone?: string;
  notes?: string;
}

export interface CreateClaimRiskObjectInput {
  assetType: AssetType;
  assetDescription: string;
  damageDescription?: string;
  isPrimary: boolean;
  assetReference?: string;
}

export interface CreateClaimInitialReserveInput {
  componentType: ReserveComponentType;
  amount: number;
  changeReason: string;
}

export interface CreateClaimCommand {
  policyId?: string | null;
  policyNumber?: string;
  clientName?: string;
  lossDate: string;
  lossDescription: string;
  lossLocation?: string;
  causeOfLossCode: string;
  estimatedLossAmount?: number;
  severity?: ClaimSeverity;
  policeReportNumber?: string;
  parties: CreateClaimPartyInput[];
  riskObjects: CreateClaimRiskObjectInput[];
  initialReserve?: CreateClaimInitialReserveInput;
}

export interface CreateClaimResult {
  claimId: string;
  claimNumber: string;
  warnings: string[];
}

export interface ClaimIntakeValidationIssue {
  severity: 'Critical' | 'Warning';
  field: string;
  message: string;
}

export interface ValidateClaimIntakeResult {
  canCreate: boolean;
  canOpen: boolean;
  issues: ClaimIntakeValidationIssue[];
}

export interface TransitionStatusCommand {
  targetStatus: ClaimStatus;
  reason?: string;
}

export interface ClaimClosureCondition {
  code: string;
  description: string;
  passed: boolean;
  detail?: string;
}

export interface ClaimClosureConditionsResult {
  canClose: boolean;
  conditions: ClaimClosureCondition[];
}

export interface OpenReserveCommand {
  component: ReserveComponentType;
  amount: number;
  changeReason: string;
  transactionType?: 'Add' | 'Adjust' | 'Reverse';
}

export interface AddPartyCommand {
  partyRole: PartyRole;
  partyType: PartyType;
  firstName?: string;
  lastName?: string;
  companyName?: string;
  email?: string;
  phone?: string;
  notes?: string;
}

export interface ValidationErrorResponse {
  type?: string;
  title?: string;
  status?: number;
  errors?: Record<string, string[]>;
  blockingConditions?: ClaimClosureCondition[];
}
