import { FormArray } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  IPaymentInformationDTO,
  IPaymentInformationRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { PaymentInformationValidatorService } from './payment-information-validator.service';

interface IPaymentInformationForm {
  paymentValidationHandler: ValidationHandler;
  element: IPaymentInformationDTO | undefined;
  isForeign: boolean;
}

export class PaymentInformationForm
  extends SoeFormGroup
  implements IPaymentInformationForm
{
  paymentValidationHandler: ValidationHandler;
  isForeign: boolean;
  element: IPaymentInformationDTO | undefined;
  validatorService?: PaymentInformationValidatorService;

  get rows(): FormArray<PaymentInformationRowForm> {
    return <FormArray>this.controls.rows;
  }

  constructor({
    paymentValidationHandler,
    element,
    isForeign,
  }: IPaymentInformationForm) {
    super(paymentValidationHandler, {
      paymentInformationId: new SoeNumberFormControl(
        element?.paymentInformationId
      ),
      actorId: new SoeNumberFormControl(element?.actorId),
      defaultSysPaymentTypeId: new SoeSelectFormControl(
        element?.defaultSysPaymentTypeId
      ),
      created: new SoeDateFormControl(element?.created),
      createdBy: new SoeTextFormControl(element?.createdBy),
      modified: new SoeDateFormControl(element?.modified),
      modifiedBy: new SoeTextFormControl(element?.modifiedBy),
      state: new SoeSelectFormControl(element?.state),
      rows: new FormArray<PaymentInformationRowForm>([]),
    });
    this.paymentValidationHandler = paymentValidationHandler;
    this.isForeign = isForeign;
  }

  customPatchValue(element: IPaymentInformationDTO) {
    this.patchValue(element);
    this.patchRows(element.rows);
  }

  private patchRows(items: IPaymentInformationRowDTO[]) {
    this.rows?.clear();
    if (items && items.length > 0) {
      items.forEach(x => {
        const row = new PaymentInformationRowForm({
          validationHandler: this.paymentValidationHandler,
          element: x,
          isForeign: this.isForeign,
        });
        this.rows.push(row, { emitEvent: false });
        if (this.validatorService)
          row.paymentNr.setAsyncValidators(
            this.validatorService?.ibanValidator()
          );
        row.paymentNr.updateValueAndValidity();
      });
      this.rows.updateValueAndValidity();
    }
  }

  public setValidatorService(
    validatorService: PaymentInformationValidatorService
  ) {
    this.validatorService = validatorService;
  }
}

interface IPaymentInformationRowForm {
  validationHandler: ValidationHandler;
  element: IPaymentInformationRowDTO | undefined;
  isForeign: boolean;
}

export class PaymentInformationRowForm extends SoeFormGroup {
  get sysPaymentTypeId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.sysPaymentTypeId;
  }

  get paymentNr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.paymentNr;
  }

  isForeign: boolean;

  constructor({
    validationHandler,
    element,
    isForeign,
  }: IPaymentInformationRowForm) {
    super(validationHandler, {
      paymentInformationRowId: new SoeNumberFormControl(
        element?.paymentInformationRowId
      ),
      paymentInformationId: new SoeNumberFormControl(
        element?.paymentInformationId
      ),
      sysPaymentTypeId: new SoeNumberFormControl(element?.sysPaymentTypeId),
      paymentNr: new SoeTextFormControl(element?.paymentNr),
      default: new SoeCheckboxFormControl(element?.default),
      shownInInvoice: new SoeCheckboxFormControl(element?.shownInInvoice),
      created: new SoeDateFormControl(element?.created),
      createdBy: new SoeTextFormControl(element?.createdBy),
      modified: new SoeDateFormControl(element?.modified),
      modifiedBy: new SoeTextFormControl(element?.modifiedBy),
      state: new SoeSelectFormControl(element?.state),
      bic: new SoeTextFormControl(element?.bic),
      clearingCode: new SoeTextFormControl(element?.clearingCode),
      paymentCode: new SoeTextFormControl(element?.paymentCode),
      paymentMethodCode: new SoeNumberFormControl(element?.paymentMethodCode),
      paymentForm: new SoeNumberFormControl(element?.paymentForm),
      chargeCode: new SoeNumberFormControl(element?.chargeCode),
      intermediaryCode: new SoeNumberFormControl(element?.intermediaryCode),
      currencyAccount: new SoeTextFormControl(element?.currencyAccount),
      payerBankId: new SoeTextFormControl(element?.payerBankId),
      bankConnected: new SoeCheckboxFormControl(element?.bankConnected),
      currencyId: new SoeNumberFormControl(element?.currencyId),
      sysPaymentTypeName: new SoeTextFormControl(element?.sysPaymentTypeName),
      paymentMethodCodeName: new SoeTextFormControl(
        element?.paymentMethodCodeName
      ),
      paymentFormName: new SoeTextFormControl(element?.paymentFormName),
      chargeCodeName: new SoeTextFormControl(element?.chargeCodeName),
      intermediaryCodeName: new SoeTextFormControl(
        element?.intermediaryCodeName
      ),
      currencyCode: new SoeTextFormControl(element?.currencyCode),
      billingType: new SoeSelectFormControl(element?.billingType),
    });

    this.isForeign = isForeign;

    // // TODO: #95135 This is shown after saving the form, not before...
    // this.paymentNr.addAsyncValidators([
    //   this.ibanValidator(
    //     serviceProvider.coreService,
    //     isForeign,
    //     serviceProvider.translate
    //   ),
    // ]);
  }
}
