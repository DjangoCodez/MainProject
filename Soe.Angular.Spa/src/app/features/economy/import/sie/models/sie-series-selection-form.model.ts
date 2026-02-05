import { Validators } from '@angular/forms';
import {
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ISieVoucherSeriesMappingDTO } from '@shared/models/generated-interfaces/SieImportDTO';
import { IVoucherSeriesDTO } from '@shared/models/generated-interfaces/VoucherSeriesDTOs';

interface ISieExportAccountSelectionForm {
  validationHandler: ValidationHandler;
  element?: ISieVoucherSeriesMappingDTO;
}

export class SieImportVoucheriesMappingForm extends SoeFormGroup {
  voucherSeries: IVoucherSeriesDTO[] = [];
  originallyMappedVoucherSeriesTypeId?: number;
  constructor({ validationHandler, element }: ISieExportAccountSelectionForm) {
    super(validationHandler, {
      number: new SoeTextFormControl(element?.number, {
        disabled: true,
      }),
      voucherNrFrom: new SoeNumberFormControl(element?.voucherNrFrom, {
        disabled: true,
      }),
      voucherNrTo: new SoeTextFormControl(element?.voucherNrTo, {
        disabled: true,
      }),
      voucherSeriesTypeId: new SoeSelectFormControl(
        element?.voucherSeriesTypeId || 0,
        {
          disabled: true,
        }
      ),
    });

    this.originallyMappedVoucherSeriesTypeId = element?.voucherSeriesTypeId;
  }

  public enable() {
    this.voucherSeriesTypeId.enable();
  }
  public disable() {
    this.voucherSeriesTypeId.disable();
    this.voucherNrFrom.disable();
    this.voucherNrTo.disable();
  }
  public getVoucherSeriesTypeId = () => this.voucherSeriesTypeId.value;

  public defaultVoucherSeriesChanged(voucherSeriesTypeId: number) {
    const current = this.getVoucherSeriesTypeId();
    const isMapped = this.voucherSeries.some(
      v => v.voucherSeriesTypeId === voucherSeriesTypeId
    );
    const isUsingDefault = current === this.originallyMappedVoucherSeriesTypeId;
    if (!current || !isMapped || !isUsingDefault) {
      this.voucherSeriesTypeId.setValue(voucherSeriesTypeId);
    }
  }

  public voucherSeriesChanged(voucherSeries: IVoucherSeriesDTO[]) {
    this.voucherSeries = voucherSeries;

    if (
      voucherSeries.some(
        v => v.voucherSeriesTypeId === this.originallyMappedVoucherSeriesTypeId
      )
    ) {
      this.voucherSeriesTypeId.setValue(
        this.originallyMappedVoucherSeriesTypeId
      );
    } else {
      this.voucherSeriesTypeId.setValue(0);
    }
  }

  public setRequired() {
    this.voucherSeriesTypeId.clearValidators();
    this.voucherSeriesTypeId.addValidators([Validators.required]);
    this.voucherSeriesTypeId.updateValueAndValidity();
  }

  public setNotRequired() {
    this.voucherSeriesTypeId.clearValidators();
    this.voucherSeriesTypeId.updateValueAndValidity();
  }

  get number() {
    return <SoeTextFormControl>this.controls.number;
  }
  get voucherNrFrom() {
    return <SoeNumberFormControl>this.controls.voucherNrFrom;
  }
  get voucherNrTo() {
    return <SoeTextFormControl>this.controls.voucherNrTo;
  }
  get voucherSeriesTypeId() {
    return <SoeSelectFormControl>this.controls.voucherSeriesTypeId;
  }
}
