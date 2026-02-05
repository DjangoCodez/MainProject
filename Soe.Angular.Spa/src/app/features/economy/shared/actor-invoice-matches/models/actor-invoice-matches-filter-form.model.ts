import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ActorInvoiceMatchesFilterDTO } from './actor-invoice-matches-filter-dto.model';

export interface IActorInvoiceMatchesFilterForm {
  validationHandler: ValidationHandler;
  element?: ActorInvoiceMatchesFilterDTO;
}

export class ActorInvoiceMatchesFilterForm extends SoeFormGroup {
  constructor({
    validationHandler,
    element
  }: IActorInvoiceMatchesFilterForm) {

    super(validationHandler, {
      actorId: new SoeNumberFormControl(
        element?.actorId ?? null, 
        {
          required: true
        }),
      type: new SoeNumberFormControl(element?.type ?? 0),
      amountFrom: new SoeNumberFormControl(element?.amountFrom ?? null),
      amountTo: new SoeNumberFormControl(element?.amountTo ?? null),
      dateFrom: new SoeDateFormControl(element?.dateFrom ?? null),
      dateTo: new SoeDateFormControl(element?.dateTo ?? null),
      originType: new SoeSelectFormControl(element?.originType ?? 0),
    });

    this.setvalidationMessageBoxTitleTranslationKey(
      'error.unabletosearch_title'
    );

  }

  get actorId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.actorId;
  }


  get amountFrom(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.amountFrom;
  }

  get amountTo(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.amountTo;
  }

  get dateFrom(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateFrom;
  }

  get dateTo(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.dateTo;
  }

}
