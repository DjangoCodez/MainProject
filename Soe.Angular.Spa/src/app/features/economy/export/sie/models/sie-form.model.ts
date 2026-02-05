import { FormArray } from '@angular/forms';
import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeRadioFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  ISieExportDTO,
  ISieExportVoucherSelectionDTO,
  ISieExportAccountSelectionDTO,
} from '@shared/models/generated-interfaces/SieExportDTO';
import { SieExportAccountSelectionForm } from './sie-account-selection-form.model';
import { SieExportVoucherSelectionForm } from './sie-voucher-selection-form.model';

interface ISieExportForm {
  validationHandler: ValidationHandler;
  element?: ISieExportDTO;
}

export class SieExportForm extends SoeFormGroup {
  sieValidationHandler: ValidationHandler;
  constructor({ validationHandler, element }: ISieExportForm) {
    super(validationHandler, {
      exportType: new SoeSelectFormControl(
        element?.exportType || 0,
        { required: true, zeroNotAllowed: true },
        'economy.export.sie.type'
      ),
      accountingYearId: new SoeSelectFormControl(
        element?.accountingYearId || 0
      ),
      dateFrom: new SoeDateFormControl(element?.dateFrom || undefined),
      dateTo: new SoeDateFormControl(element?.dateTo || undefined),
      exportPreviousYear: new SoeRadioFormControl(
        element?.exportPreviousYear || true
      ),
      exportObject: new SoeRadioFormControl(element?.exportObject || true),
      exportAccount: new SoeRadioFormControl(element?.exportAccount || true),
      exportAccountType: new SoeRadioFormControl(
        element?.exportAccountType || true
      ),
      exportSruCodes: new SoeRadioFormControl(element?.exportSruCodes || true),
      comment: new SoeTextFormControl(element?.comment || ''),
      accountSelection: new FormArray<SieExportAccountSelectionForm>([]),
      voucherSelection: new FormArray<SieExportVoucherSelectionForm>([]),
      budgetHeadId: new SoeSelectFormControl(element?.budgetHeadId || 0),
      sortVoucherBy: new SoeSelectFormControl(element?.sortVoucherBy || 0),
    });
    this.sieValidationHandler = validationHandler;
    this.setvalidationMessageBoxTitleTranslationKey(
      'error.unabletoexport_title'
    );
  }

  get exportType(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.exportType;
  }
  get accountingYearId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountingYearId;
  }
  get dateFrom(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateFrom;
  }
  get dateTo(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateTo;
  }
  get exportPreviousYear(): SoeRadioFormControl {
    return <SoeRadioFormControl>this.controls.exportPreviousYear;
  }
  get exportObject(): SoeRadioFormControl {
    return <SoeRadioFormControl>this.controls.exportObject;
  }
  get exportAccount(): SoeRadioFormControl {
    return <SoeRadioFormControl>this.controls.exportAccount;
  }
  get exportAccountType(): SoeRadioFormControl {
    return <SoeRadioFormControl>this.controls.exportAccountType;
  }
  get exportSruCodes(): SoeRadioFormControl {
    return <SoeRadioFormControl>this.controls.exportSruCodes;
  }
  get comment(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.comment;
  }
  get accountSelection(): FormArray<SieExportAccountSelectionForm> {
    return <FormArray<SieExportAccountSelectionForm>>(
      this.controls.accountSelection
    );
  }
  get voucherSelection(): FormArray<SieExportVoucherSelectionForm> {
    return <FormArray<SieExportVoucherSelectionForm>>(
      this.controls.voucherSelection
    );
  }
  get budgetHeadId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.budgetHeadId;
  }

  addAccountDim(): void {
    this.accountSelection.push(
      new SieExportAccountSelectionForm({
        validationHandler: this.sieValidationHandler,
        element: <ISieExportAccountSelectionDTO>{},
      })
    );
  }

  removeAccountDim(idx: number): void {
    this.accountSelection.removeAt(idx);
  }

  addVoucherSerie(): void {
    this.voucherSelection.push(
      new SieExportVoucherSelectionForm({
        validationHandler: this.sieValidationHandler,
        element: <ISieExportVoucherSelectionDTO>{},
      })
    );
  }

  removeVoucherSerie(idx: number): void {
    this.voucherSelection.removeAt(idx);
  }

  resetAccountYears(): void {
    for (let i = 0; i < this.accountSelection.length; i++) {
      this.accountSelection.at(i).accountDimId.setValue(0);
      this.accountSelection.at(i).accountNrFromId.setValue(0);
      this.accountSelection.at(i).accountNrToId.setValue(0);
    }
    for (let i = 0; i < this.voucherSelection.length; i++) {
      this.voucherSelection.at(i).voucherSeriesId.setValue(0);
      this.voucherSelection.at(i).voucherNoFrom.setValue('');
      this.voucherSelection.at(i).voucherNoTo.setValue('');
    }
  }
}
