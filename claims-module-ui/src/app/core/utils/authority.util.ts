export type AuthorityLevel = 'auto' | 'supervisor' | 'manager';

export function getReserveAuthorityLevel(amount: number): AuthorityLevel {
  const abs = Math.abs(amount);
  if (abs <= 10_000) {
    return 'auto';
  }
  if (abs <= 100_000) {
    return 'supervisor';
  }
  return 'manager';
}

export function getAuthorityLabel(amount: number): string {
  const level = getReserveAuthorityLevel(amount);
  switch (level) {
    case 'auto':
      return '✓ Auto-approved (≤ $10,000)';
    case 'supervisor':
      return '⚠ Supervisor approval required';
    case 'manager':
      return '⚠ Manager approval required';
  }
}
