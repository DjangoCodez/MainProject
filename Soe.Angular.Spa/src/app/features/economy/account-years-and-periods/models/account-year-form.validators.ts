import { AbstractControl, ValidationErrors } from '@angular/forms';
import { AccountYearDTO } from './account-years-and-periods.model';
import { DateUtil } from '@shared/util/date-util';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AccountYearsValidator {
  public sortedAccountYears: AccountYearDTO[] = [];

  public setSortedAccountYears(sortedAccountYears: AccountYearDTO[]) {
    this.sortedAccountYears = sortedAccountYears;
  }

  ensureNoOverlap(control: AbstractControl): ValidationErrors | null {
    if (this.sortedAccountYears.length === 0) return null;

    const accountYearId = control.get('accountYearId')?.value;
    const from = control.get('from')?.value as Date;
    const to = control.get('to')?.value as Date;

    if (!from || !to) return null;

    const hasOverlap = this.sortedAccountYears.some(
      year =>
        accountYearId !== year.accountYearId &&
        DateUtil.isRangesOverlapping(from, to, year.from, year.to, true)
    );

    return hasOverlap
      ? {
          custom: {
            translationKey: 'economy.accounting.accountyear.overlapping',
          },
        }
      : null;
  }

  ensureNoGapBehind(control: AbstractControl): ValidationErrors | null {
    if (this.sortedAccountYears.length === 0) return null;

    const from = control.get('from')?.value as Date;
    if (!from) return null;

    // If there are years behind, the gap in days should be exactly 1.
    let hasYearsBehind = false;
    const hasGapBehind = this.sortedAccountYears.some(year => {
      if (year.to < from) {
        hasYearsBehind = true;
      }
      return DateUtil.diffDays(from, year.to) == 1;
    });

    return hasYearsBehind && !hasGapBehind
      ? {
          custom: {
            translationKey: 'economy.accounting.accountyear.gapsbehind',
          },
        }
      : null;
  }

  ensureNoGapAhead(control: AbstractControl): ValidationErrors | null {
    if (this.sortedAccountYears.length === 0) return null;

    const to = control.get('to')?.value as Date;
    if (!to) return null;

    // If there are years ahead, the gap in days should be exactly 1.
    let hasYearsAhead = false;
    const hasGapAhead = this.sortedAccountYears.some(year => {
      if (year.from > to) {
        hasYearsAhead = true;
      }
      return DateUtil.diffDays(year.from, to) == 1;
    });

    return hasYearsAhead && !hasGapAhead
      ? {
          custom: {
            translationKey: 'economy.accounting.accountyear.gapsahead',
          },
        }
      : null;
  }
}
