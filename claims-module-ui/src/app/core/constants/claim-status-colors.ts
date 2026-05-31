export const CLAIM_STATUS_COLORS: Record<string, string> = {
  Draft: '#9e9e9e',
  Open: '#1976d2',
  UnderInvestigation: '#f57c00',
  PendingPayment: '#7b1fa2',
  Closed: '#388e3c',
  Reopened: '#ffa000',
  Withdrawn: '#616161',
};

export function statusColor(status: string): string {
  return CLAIM_STATUS_COLORS[status] ?? '#757575';
}

export function formatStatus(status: string): string {
  return status.replace(/([A-Z])/g, ' $1').trim();
}
