import { ValidationHandler } from '@shared/handlers';
import { SearchVoucherFilterDTO } from './voucher-search.model';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeRadioFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';

interface ISearchVoucherFilterForm {
  validationHandler: ValidationHandler;
  element?: SearchVoucherFilterDTO;
}

export class SearchVoucherFilterForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISearchVoucherFilterForm) {
    super(validationHandler, {
      actorCompanyId: new SoeTextFormControl(
        element?.actorCompanyId || undefined
      ),
      voucherDateFrom: new SoeDateFormControl(
        element?.voucherDateFrom || undefined
      ),
      voucherDateTo: new SoeDateFormControl(
        element?.voucherDateTo || undefined
      ),
      voucherSeriesIdFrom: new SoeNumberFormControl(
        element?.voucherSeriesIdFrom || undefined
      ),
      voucherSeriesIdTo: new SoeNumberFormControl(
        element?.voucherSeriesIdTo || undefined
      ),
      debitFrom: new SoeNumberFormControl(element?.debitFrom || 0),
      debitTo: new SoeNumberFormControl(element?.debitTo || 0),
      creditFrom: new SoeNumberFormControl(element?.creditFrom || 0),
      creditTo: new SoeNumberFormControl(element?.creditTo || 0),
      amountFrom: new SoeNumberFormControl(element?.amountFrom || 0),
      amountTo: new SoeNumberFormControl(element?.amountTo || 0),
      voucherText: new SoeTextFormControl(element?.voucherText || ''),
      createdFrom: new SoeDateFormControl(element?.createdFrom || undefined),
      createdTo: new SoeDateFormControl(element?.createdTo || undefined),
      createdBy: new SoeSelectFormControl(element?.createdBy || ''),
      dim1AccountId: new SoeSelectFormControl(
        element?.dim1AccountId || undefined
      ),
      dim1AccountFr: new SoeTextFormControl(element?.dim1AccountFr || ''),
      dim1AccountTo: new SoeTextFormControl(element?.dim1AccountTo || ''),
      dim2AccountId: new SoeSelectFormControl(
        element?.dim2AccountId || undefined
      ),
      dim2AccountFr: new SoeTextFormControl(element?.dim2AccountFr || ''),
      dim2AccountTo: new SoeTextFormControl(element?.dim2AccountTo || ''),
      dim3AccountId: new SoeSelectFormControl(
        element?.dim3AccountId || undefined
      ),
      dim3AccountFr: new SoeTextFormControl(element?.dim3AccountFr || ''),
      dim3AccountTo: new SoeTextFormControl(element?.dim3AccountTo || ''),
      dim4AccountId: new SoeSelectFormControl(
        element?.dim4AccountId || undefined
      ),
      dim4AccountFr: new SoeTextFormControl(element?.dim4AccountFr || ''),
      dim4AccountTo: new SoeTextFormControl(element?.dim4AccountTo || ''),
      dim5AccountId: new SoeSelectFormControl(
        element?.dim5AccountId || undefined
      ),
      dim5AccountFr: new SoeTextFormControl(element?.dim5AccountFr || ''),
      dim5AccountTo: new SoeTextFormControl(element?.dim5AccountTo || ''),
      dim6AccountId: new SoeSelectFormControl(
        element?.dim6AccountId || undefined
      ),
      dim6AccountFr: new SoeTextFormControl(element?.dim6AccountFr || ''),
      dim6AccountTo: new SoeTextFormControl(element?.dim7AccountTo || ''),
      dim7AccountId: new SoeSelectFormControl(
        element?.dim6AccountId || undefined
      ),
      dim7AccountFr: new SoeTextFormControl(element?.dim7AccountFr || ''),
      dim7AccountTo: new SoeTextFormControl(element?.dim7AccountTo || ''),
      voucherSeriesTypeIds: new SoeSelectFormControl(
        element?.voucherSeriesTypeIds || []
      ),
      isCredit: new SoeCheckboxFormControl(element?.isCredit || false),
      isDebit: new SoeCheckboxFormControl(element?.isDebit || false),
      userId: new SoeSelectFormControl(0),
    });
    this.dim1AccountFr.valueChanges.subscribe(v => {
      this.dim1AccountTo.setValue(v);
    });
    this.dim2AccountFr.valueChanges.subscribe(v => {
      this.dim2AccountTo.setValue(v);
    });
    this.dim3AccountFr.valueChanges.subscribe(v => {
      this.dim3AccountTo.setValue(v);
    });
    this.dim4AccountFr.valueChanges.subscribe(v => {
      this.dim4AccountTo.setValue(v);
    });
    this.dim5AccountFr.valueChanges.subscribe(v => {
      this.dim5AccountTo.setValue(v);
    });
    this.dim6AccountFr.valueChanges.subscribe(v => {
      this.dim6AccountTo.setValue(v);
    });
    this.dim7AccountFr.valueChanges.subscribe(v => {
      this.dim7AccountTo.setValue(v);
    });
  }

  get voucherDateFrom(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.voucherDateFrom;
  }

  get voucherDateTo(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.voucherDateTo;
  }

  get voucherSeriesIdFrom(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.voucherSeriesIdFrom;
  }

  get voucherSeriesIdTo(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.voucherSeriesIdTo;
  }

  get debitFrom(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.debitFrom;
  }

  get debitTo(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.debitTo;
  }

  get creditFrom(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.creditFrom;
  }

  get creditTo(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.creditTo;
  }

  get amountFrom(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.amountFrom;
  }

  get amountTo(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.amountTo;
  }

  get voucherText(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.voucherText;
  }

  get createdFrom(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.createdFrom;
  }

  get createdTo(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.createdTo;
  }

  get createdBy(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.createdBy;
  }

  get dim1AccountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.dim1AccountId;
  }

  get dim1AccountFr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim1AccountFr;
  }

  get dim1AccountTo(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim1AccountTo;
  }

  get dim2AccountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.dim2AccountId;
  }

  get dim2AccountFr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim2AccountFr;
  }

  get dim2AccountTo(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim2AccountTo;
  }

  get dim3AccountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.dim3AccountId;
  }

  get dim3AccountFr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim3AccountFr;
  }

  get dim3AccountTo(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim3AccountTo;
  }

  get dim4AccountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.dim4AccountId;
  }

  get dim4AccountFr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim4AccountFr;
  }

  get dim4AccountTo(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim4AccountTo;
  }

  get dim5AccountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.dim5AccountId;
  }

  get dim5AccountFr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim5AccountFr;
  }

  get dim5AccountTo(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim5AccountTo;
  }

  get dim6AccountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.dim6AccountId;
  }

  get dim6AccountFr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim6AccountFr;
  }

  get dim6AccountTo(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim6AccountTo;
  }

  get dim7AccountId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.dim7AccountId;
  }

  get dim7AccountFr(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim7AccountFr;
  }

  get dim7AccountTo(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.dim7AccountTo;
  }

  get voucherSeriesTypeIds(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.voucherSeriesTypeIds;
  }

  get isCredit(): SoeCheckboxFormControl {
    return <SoeRadioFormControl>this.controls.isCredit;
  }

  get isDebit(): SoeCheckboxFormControl {
    return <SoeRadioFormControl>this.controls.isDebit;
  }

  get userId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.userId;
  }
}
