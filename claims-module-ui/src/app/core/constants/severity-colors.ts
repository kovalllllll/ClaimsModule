export function severityColor(severity?: string | null): string {
  switch (severity?.toLowerCase()) {
    case 'minor':
      return '#388e3c';
    case 'standard':
      return '#1976d2';
    case 'critical':
      return '#f57c00';
    case 'catastrophic':
      return '#b71c1c';
    default:
      return '#757575';
  }
}
