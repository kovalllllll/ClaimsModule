import { HttpErrorResponse } from '@angular/common/http';
import { ValidationErrorResponse } from '../models';

export function extractApiErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'An unexpected error occurred.';
  }

  const body = error.error as ValidationErrorResponse | string | null;

  if (typeof body === 'string' && body.trim()) {
    return body;
  }

  if (body && typeof body === 'object' && body.errors) {
    const messages = Object.values(body.errors).flat();
    if (messages.length) {
      return messages.join(' ');
    }
  }

  if (body && typeof body === 'object' && body.blockingConditions?.length) {
    const failed = body.blockingConditions
      .filter((c) => !c.passed)
      .map((c) => c.detail ?? c.description);
    if (failed.length) {
      return failed.join(' ');
    }
  }

  if (body && typeof body === 'object' && body.title) {
    return body.title;
  }

  if (error.status === 0) {
    return 'Unable to reach the API. Is the backend running?';
  }

  return error.message || `Request failed (${error.status}).`;
}
