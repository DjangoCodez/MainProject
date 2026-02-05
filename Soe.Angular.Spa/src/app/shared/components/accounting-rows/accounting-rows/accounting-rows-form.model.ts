import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { AccountingRowDTO } from '../models/accounting-rows-model';
import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

interface IAccountingRowsForm {
  validationHandler: ValidationHandler;
  element: AccountingRowDTO | undefined;
}

export class AccountingRowsForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  constructor({ validationHandler, element }: IAccountingRowsForm) {
    super(
      validationHandler,
      {
        type: new SoeNumberFormControl(element?.type || undefined),
        invoiceRowId: new SoeNumberFormControl(element?.invoiceRowId || 0),
        invoiceAccountRowId: new SoeNumberFormControl(
          element?.invoiceAccountRowId || 0
        ),
        tempRowId: new SoeNumberFormControl(element?.tempRowId || 0),
        tempInvoiceRowId: new SoeNumberFormControl(
          element?.tempInvoiceRowId || 0
        ),
        parentRowId: new SoeNumberFormControl(element?.parentRowId || 0),
        voucherRowId: new SoeNumberFormControl(element?.voucherRowId || 0),
        invoiceId: new SoeNumberFormControl(element?.invoiceId || 0),
        rowNr: new SoeNumberFormControl(element?.rowNr || 0),
        productRowNr: new SoeNumberFormControl(element?.productRowNr || 0),
        productName: new SoeTextFormControl(element?.productName || ''),
        quantity: new SoeNumberFormControl(element?.quantity || undefined),
        text: new SoeTextFormControl(element?.text || ''),
        date: new SoeDateFormControl(element?.date || undefined),
        startDate: new SoeDateFormControl(element?.startDate || undefined),
        numberOfPeriods: new SoeNumberFormControl(
          element?.numberOfPeriods || undefined
        ),

        unit: new SoeTextFormControl(element?.unit || undefined),
        quantityStop: new SoeCheckboxFormControl(
          element?.quantityStop || undefined
        ),
        rowTextStop: new SoeCheckboxFormControl(
          element?.rowTextStop || undefined
        ),
        amountStop: new SoeNumberFormControl(element?.amountStop || undefined),
        debitAmount: new SoeNumberFormControl(
          element?.debitAmount || undefined
        ),
        debitAmountCurrency: new SoeNumberFormControl(
          element?.debitAmountCurrency || undefined
        ),
        debitAmountEntCurrency: new SoeNumberFormControl(
          element?.debitAmountEntCurrency || undefined
        ),
        debitAmountLedgerCurrency: new SoeNumberFormControl(
          element?.debitAmountLedgerCurrency || undefined
        ),
        creditAmount: new SoeNumberFormControl(
          element?.creditAmount || undefined
        ),
        creditAmountCurrency: new SoeNumberFormControl(
          element?.creditAmountCurrency || undefined
        ),
        creditAmountEntCurrency: new SoeNumberFormControl(
          element?.creditAmountEntCurrency || undefined
        ),
        creditAmountLedgerCurrency: new SoeNumberFormControl(
          element?.creditAmountLedgerCurrency || undefined
        ),
        amount: new SoeNumberFormControl(element?.amount || undefined),
        amountCurrency: new SoeNumberFormControl(
          element?.amountCurrency || undefined
        ),
        amountEntCurrency: new SoeNumberFormControl(
          element?.amountEntCurrency || undefined
        ),
        amountLedgerCurrency: new SoeNumberFormControl(
          element?.amountLedgerCurrency || undefined
        ),
        balance: new SoeNumberFormControl(element?.balance || undefined),
        splitValue: new SoeNumberFormControl(element?.splitValue || undefined),
        inventoryId: new SoeNumberFormControl(
          element?.inventoryId || undefined
        ),
        attestStatus: new SoeNumberFormControl(
          element?.attestStatus || undefined
        ),
        attestUserName: new SoeTextFormControl(
          element?.attestUserName || undefined
        ),
        isCreditRow: new SoeNumberFormControl(
          element?.isCreditRow || undefined
        ),
        isDebitRow: new SoeCheckboxFormControl(
          element?.isDebitRow || undefined
        ),
        isVatRow: new SoeCheckboxFormControl(element?.isVatRow || undefined),
        isContractorVatRow: new SoeCheckboxFormControl(
          element?.isContractorVatRow || undefined
        ),
        isCentRoundingRow: new SoeCheckboxFormControl(
          element?.isCentRoundingRow || undefined
        ),
        isInterimRow: new SoeCheckboxFormControl(
          element?.isInterimRow || undefined
        ),
        isTemplateRow: new SoeCheckboxFormControl(
          element?.isTemplateRow || undefined
        ),
        isClaimRow: new SoeCheckboxFormControl(
          element?.isClaimRow || undefined
        ),
        isHouseholdRow: new SoeCheckboxFormControl(
          element?.isHouseholdRow || undefined
        ),
        state: new SoeCheckboxFormControl(element?.state || undefined),

        dim1Id: new SoeNumberFormControl(element?.dim1Id || 0),
        dim1Nr: new SoeTextFormControl(element?.dim1Nr || undefined),
        dim1Name: new SoeTextFormControl(element?.dim1Name || undefined),
        dim1Disabled: new SoeCheckboxFormControl(
          element?.dim1Disabled || undefined
        ),
        dim1Mandatory: new SoeCheckboxFormControl(
          element?.dim1Mandatory || undefined
        ),
        dim1Stop: new SoeCheckboxFormControl(element?.dim1Stop || undefined),

        dim2Id: new SoeNumberFormControl(element?.dim2Id || 0),
        dim2Nr: new SoeTextFormControl(element?.dim2Nr || undefined),
        dim2Name: new SoeTextFormControl(element?.dim2Name || undefined),
        dim2Disabled: new SoeCheckboxFormControl(
          element?.dim2Disabled || undefined
        ),
        dim2Mandatory: new SoeCheckboxFormControl(
          element?.dim2Mandatory || undefined
        ),
        dim2Stop: new SoeCheckboxFormControl(element?.dim2Stop || undefined),

        dim3Id: new SoeNumberFormControl(element?.dim3Id || 0),
        dim3Nr: new SoeTextFormControl(element?.dim3Nr || undefined),
        dim3Name: new SoeTextFormControl(element?.dim3Name || undefined),
        dim3Disabled: new SoeCheckboxFormControl(
          element?.dim3Disabled || undefined
        ),
        dim3Mandatory: new SoeCheckboxFormControl(
          element?.dim3Mandatory || undefined
        ),
        dim3Stop: new SoeCheckboxFormControl(element?.dim3Stop || undefined),

        dim4Id: new SoeNumberFormControl(element?.dim4Id || 0),
        dim4Nr: new SoeTextFormControl(element?.dim4Nr || undefined),
        dim4Name: new SoeTextFormControl(element?.dim4Name || undefined),
        dim4Disabled: new SoeCheckboxFormControl(
          element?.dim4Disabled || undefined
        ),
        dim4Mandatory: new SoeCheckboxFormControl(
          element?.dim4Mandatory || undefined
        ),
        dim4Stop: new SoeCheckboxFormControl(element?.dim4Stop || undefined),

        dim5Id: new SoeNumberFormControl(element?.dim5Id || 0),
        dim5Nr: new SoeTextFormControl(element?.dim5Nr || undefined),
        dim5Name: new SoeTextFormControl(element?.dim5Name || undefined),
        dim5Disabled: new SoeCheckboxFormControl(
          element?.dim5Disabled || undefined
        ),
        dim5Mandatory: new SoeCheckboxFormControl(
          element?.dim5Mandatory || undefined
        ),
        dim5Stop: new SoeCheckboxFormControl(element?.dim5Stop || undefined),

        dim6Id: new SoeNumberFormControl(element?.dim6Id || 0),
        dim6Nr: new SoeTextFormControl(element?.dim6Nr || undefined),
        dim6Name: new SoeTextFormControl(element?.dim6Name || undefined),
        dim6Disabled: new SoeCheckboxFormControl(
          element?.dim6Disabled || undefined
        ),
        dim6Mandatory: new SoeCheckboxFormControl(
          element?.dim6Mandatory || undefined
        ),
        dim6Stop: new SoeCheckboxFormControl(element?.dim6Stop || undefined),

        isAccrualAccount: new SoeCheckboxFormControl(
          element?.isAccrualAccount || undefined
        ),
        dim1Error: new SoeTextFormControl(element?.dim1Error || undefined),
        dim2Error: new SoeTextFormControl(element?.dim2Error || undefined),
        dim3Error: new SoeTextFormControl(element?.dim3Error || undefined),
        dim4Error: new SoeTextFormControl(element?.dim4Error || undefined),
        dim5Error: new SoeTextFormControl(element?.dim5Error || undefined),
        dim6Error: new SoeTextFormControl(element?.dim6Error || undefined),
      },
      [
        AccountingRowsValidator.accountingCheck(),
        AccrualColumnValidator.accrualColumnCheck(),
      ]
    );

    this.thisValidationHandler = validationHandler;
  }
}

export class AccountingRowsValidator {
  static accountingCheck(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      for (let i = 1; i <= 6; i++) {
        const dimId = control.get(`dim${i}Id`)?.value;
        const isMandatory = control.get(`dim${i}Mandatory`)?.value;
        const dimName = control.get(`dim${i}Name`)?.value;

        if (isMandatory && !dimId) {
          return {
            custom: {
              translationKey:
                i == 1
                  ? 'common.accountingrows.missingmainaccount'
                  : 'common.accountingrows.missinginternalaccount',
            },
          };
        }
        if (dimId && !dimName) {
          return {
            custom: {
              translationKey:
                i == 1
                  ? 'common.accountingrows.invalidaccount'
                  : 'common.accountingrows.invalidinternalaccount',
            },
          };
        }
      }
      return null;
    };
  }
}

export class AccrualColumnValidator {
  static accrualColumnCheck(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const numberOfPeriods = control.get(`numberOfPeriods`)?.value ?? 0;
      const startDate = control.get(`startDate`)?.value;

      if (numberOfPeriods > 0 && !startDate) {
        return {
          custom: {
            translationKey: 'common.accountingrows.missingaccrualstartdate',
          },
        };
      }
      if (startDate && numberOfPeriods < 1) {
        return {
          custom: {
            translationKey:
              'common.accountingrows.missingnumberofaccrualperiods',
          },
        };
      }

      return null;
    };
  }
}