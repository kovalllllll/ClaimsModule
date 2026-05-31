const USER_NAMES: Record<string, string> = {
  '11111111-0000-0000-0000-000000000001': 'John Handler',
  '22222222-0000-0000-0000-000000000002': 'Sarah Supervisor',
  '33333333-0000-0000-0000-000000000003': 'Mike Manager',
};

export function displayUserName(userId?: string | null): string {
  if (!userId) {
    return '—';
  }
  return USER_NAMES[userId.toLowerCase()] ?? userId;
}
