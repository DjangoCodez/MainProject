import { SoeBankerRequestFilterDTO } from '@features/manage/models/bankintegration.model';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

interface IBankintegrationDownloadRequestGridFilterForm {
  validationHandler: ValidationHandler;
  element: SoeBankerRequestFilterDTO;
}

export class BankintegrationDownloadRequestGridFilterForm extends SoeFormGroup {
  statusCodes: number[] = [];
  constructor({
    validationHandler,
    element,
  }: IBankintegrationDownloadRequestGridFilterForm) {
    super(validationHandler, {
      fromDate: new SoeSelectFormControl(
        element?.fromDate || new Date().addDays(-30)
      ),
      toDate: new SoeSelectFormControl(element?.toDate || new Date()),
      onlyError: new SoeSelectFormControl(element?.onlyError || false),
      downloaded: new SoeCheckboxFormControl(false),
    });

    this.setDownloadedStatusCode(element?.statusCodes);
  }

  get fromDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.fromDate;
  }

  get toDate(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.toDate;
  }
  get onlyError(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.onlyError;
  }

  get downloaded(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.downloaded;
  }

  private setDownloadedStatusCode(statusCodes?: number[]): void {
    this.statusCodes = statusCodes ?? [11]; //Downloaded;
    this.downloaded.setValue(true);

    this.downloaded.valueChanges.subscribe(value =>
      this.updateDownloadedStatusCode(!!value)
    );
  }
  private updateDownloadedStatusCode(downloaded: boolean): void {
    if (downloaded) {
      this.statusCodes.push(11);
    } else {
      const index = this.statusCodes.indexOf(11);
      if (index > -1) {
        this.statusCodes.splice(index, 1);
      }
    }
  }
}
