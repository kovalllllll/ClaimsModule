import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { ReserveComponentType } from '../models';

export function isValidReserveTransactionAmount(
  componentType: ReserveComponentType | null | undefined,
  amount: number | null | undefined
): boolean {
  if (amount === null || amount === undefined || Number.isNaN(amount)) {
    return false;
  }
  if (amount === 0) {
    return false;
  }
  if (componentType === 'SubrogationRecoverable') {
    return true;
  }
  return amount > 0;
}

export function reserveTransactionAmountValidator(
  getComponentType: () => ReserveComponentType | null | undefined
): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const raw = control.value;
    if (raw === null || raw === undefined || raw === '') {
      return null;
    }
    const amount = Number(raw);
    if (Number.isNaN(amount)) {
      return { invalidAmount: true };
    }
    if (isValidReserveTransactionAmount(getComponentType(), amount)) {
      return null;
    }
    if (amount === 0) {
      return { zero: true };
    }
    return { positive: true };
  };
}

export function shouldShowReserveAmountErrors(control: AbstractControl | null): boolean {
  return !!control && (control.touched || control.dirty);
}
