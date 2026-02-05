import { Injectable } from '@angular/core';
import { AbstractControl, FormArray, ValidationErrors } from '@angular/forms';
import { SieImportPreviewForm } from './sie-form.model';
import {
  ISieAccountMappingDTO,
  ISieImportPreviewDTO,
} from '@shared/models/generated-interfaces/SieImportDTO';

@Injectable({
  providedIn: 'root',
})
export class SieFormValidators {
  static selectAtLeastOneImport(
    control: AbstractControl
  ): ValidationErrors | null {
    const importAccounts = control.get('importAccounts')?.value;
    const importAccountBalances = control.get('importAccountBalances')?.value;
    const importVouchers = control.get('importVouchers')?.value;

    return importAccounts || importAccountBalances || importVouchers
      ? null
      : { custom: { translationKey: 'economy.import.sie.noimportselected' } };
  }

  static unmappedVoucherSeries(
    control: AbstractControl
  ): ValidationErrors | null {
    const voucherIsImported = control.get('importVouchers')?.value;
    if (!voucherIsImported) return null;

    const invalid = (
      control.get('voucherSeriesMapping') as FormArray
    ).controls.some(c => !c.get('voucherSeriesTypeId')?.value);
    return invalid
      ? {
          custom: { translationKey: 'economy.import.sie.seriesmissingmapping' },
        }
      : null;
  }

  static missingAccountName(control: AbstractControl): ValidationErrors | null {
    const importAccounts = control.get('importAccounts')?.value;
    const approveEmptyAccountNames =
      control.get('approveEmptyAccountNames')?.value ?? false;
    const preview = (control.get('preview') as SieImportPreviewForm)?.value as
      | ISieImportPreviewDTO
      | undefined;

    // Exit early if required data is missing
    if (!importAccounts || !preview) return null;

    // Check if any account name is missing in accountStd and each dim accountMappings
    if (
      SieFormValidators.hasMissingAccountName(
        preview.accountStd?.accountMappings,
        approveEmptyAccountNames
      ) ||
      preview.accountDims?.some(dim =>
        SieFormValidators.hasMissingAccountName(
          dim.accountMappings,
          approveEmptyAccountNames
        )
      )
    ) {
      return {
        custom: {
          translationKey: 'economy.import.sie.preview.missing.account.name',
        },
      };
    }

    return null;
  }

  private static hasMissingAccountName(
    mappings: ISieAccountMappingDTO[] | undefined,
    allowEmptyNames: boolean
  ): boolean {
    if (!mappings || allowEmptyNames) return false;
    return mappings.some(mapping => mapping.name.trim() === '');
  }
}
