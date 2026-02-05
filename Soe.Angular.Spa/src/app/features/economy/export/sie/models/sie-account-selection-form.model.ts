import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ISieExportAccountSelectionDTO } from '@shared/models/generated-interfaces/SieExportDTO';
import { IAccountDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { distinctUntilChanged } from 'rxjs';

interface ISieExportAccountSelectionForm {
  validationHandler: ValidationHandler;
  element?: ISieExportAccountSelectionDTO;
}

export class SieExportAccountSelectionForm extends SoeFormGroup {
  accounts: IAccountDTO[] = [];
  constructor({ validationHandler, element }: ISieExportAccountSelectionForm) {
    super(validationHandler, {
      accountDimId: new SoeSelectFormControl(element?.accountDimId || 0),
      accountNrFrom: new SoeTextFormControl(element?.accountNrFrom || ''),
      accountNrTo: new SoeTextFormControl(element?.accountNrTo || ''),
      accountNrFromId: new SoeSelectFormControl(0),
      accountNrToId: new SoeSelectFormControl(0),
    });

    this.accountNrFromId.valueChanges
      .pipe(distinctUntilChanged())
      .subscribe(accountNrFromId => {
        if (typeof accountNrFromId === 'number') {
          this.accountNrFrom.setValue(
            this.accounts.find(a => a.accountId === accountNrFromId)
              ?.accountNr ?? ''
          );

          if (!this.accountNrToId.value)
            this.accountNrToId.setValue(accountNrFromId);
        }
      });

    this.accountNrToId.valueChanges
      .pipe(distinctUntilChanged())
      .subscribe(accountNrToId => {
        if (typeof accountNrToId === 'number') {
          this.accountNrTo.setValue(
            this.accounts.find(a => a.accountId === accountNrToId)?.accountNr ??
              ''
          );
        }
      });
  }
  get accountDimId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountDimId;
  }
  get accountNrFrom(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.accountNrFrom;
  }
  get accountNrTo(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.accountNrTo;
  }
  get accountNrFromId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountNrFromId;
  }
  get accountNrToId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.accountNrToId;
  }
}
