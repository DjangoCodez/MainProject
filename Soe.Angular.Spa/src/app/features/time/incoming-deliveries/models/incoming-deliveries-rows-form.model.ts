import { ValidationErrors, ValidatorFn } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  IIncomingDeliveryRowDTO,
  IIncomingDeliveryTypeDTO,
  IIncomingDeliveryTypeSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

interface IIncomingDeliveriesRowsForm {
  validationHandler: ValidationHandler;
  element: IIncomingDeliveryRowDTO | undefined;
}
export class IncomingDeliveriesRowsForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IIncomingDeliveriesRowsForm) {
    super(validationHandler, {
      incomingDeliveryRowId: new SoeTextFormControl(
        element?.incomingDeliveryRowId || 0,
        {
          isIdField: true,
        }
      ),
      name: new SoeTextFormControl(
        element?.name || '',
        { isNameField: true, required: true, maxLength: 100, minLength: 1 },
        'common.name'
      ),
      description: new SoeTextFormControl(
        element?.description || '',
        { maxLength: 512 },
        'common.description'
      ),
      shiftTypeId: new SoeTextFormControl(element?.shiftTypeId || 0),
      shiftTypeName: new SoeTextFormControl(element?.shiftTypeName || ''),
      incomingDeliveryTypeId: new SoeTextFormControl(
        element?.incomingDeliveryTypeId || 0
      ),
      typeName: new SoeTextFormControl(element?.typeName || ''),
      incomingDeliveryTypeLength: new SoeNumberFormControl(0),
      nbrOfPackages: new SoeNumberFormControl(
        element?.nbrOfPackages || 0,
        {
          minValue: 1,
        },
        'time.schedule.incomingdelivery.validation.nbrofpackagesislowerthanallowed'
      ),
      totalLength: new SoeNumberFormControl(0),
      length: new SoeTextFormControl(element?.length ?? 0), // Length in minutes (stored in database)
      minSplitLength: new SoeTextFormControl(element?.minSplitLength ?? 0), // Length in minutes (stored in database)
      nbrOfPersons: new SoeNumberFormControl(element?.nbrOfPersons || 0),
      startTime: new SoeDateFormControl(element?.startTime),
      stopTime: new SoeDateFormControl(element?.stopTime),
      onlyOneEmployee: new SoeCheckboxFormControl(
        element?.onlyOneEmployee || false
      ),
      allowOverlapping: new SoeCheckboxFormControl(
        element?.allowOverlapping || false
      ),
      dontAssignBreakLeftovers: new SoeCheckboxFormControl(
        element?.dontAssignBreakLeftovers || false
      ),
    });
  }

  customPatchValue(element: IIncomingDeliveryRowDTO, setLength = true) {
    this.patchValue(element);
    this.customPatchIncomingDeliveryType(
      element.incomingDeliveryTypeDTO,
      setLength
    );
  }

  customPatchIncomingDeliveryType(
    type: IIncomingDeliveryTypeDTO | IIncomingDeliveryTypeSmallDTO | undefined,
    setLength = true
  ) {
    this.controls.incomingDeliveryTypeId.patchValue(
      type?.incomingDeliveryTypeId ?? 0
    );
    this.controls.typeName.patchValue(type?.name ?? '');
    this.controls.incomingDeliveryTypeLength.patchValue(type?.length ?? 0);

    this.updateTotalLength();
    if (setLength) this.updateLength();
  }

  updateIncomingDeliveryType(type?: IIncomingDeliveryTypeSmallDTO): void {
    this.customPatchIncomingDeliveryType(type);
    this.updateValueAndValidity();
    this.markAsDirty();
  }

  updateTotalLength(): void {
    this.controls.totalLength.patchValue(
      this.controls.incomingDeliveryTypeLength.value *
        this.controls.nbrOfPackages.value
    );
  }

  updateLength(): void {
    this.controls.length.patchValue(
      (
        this.controls.totalLength.value / this.controls.nbrOfPersons.value
      ).round(0)
    );
  }

  updateStopTime(): void {
    this.controls.stopTime.patchValue(
      (this.controls.startTime.value as Date).addMinutes(
        this.controls.length.value
      )
    );
  }

  validateRow(minLength: number): string[] {
    const warnings: string[] = [];

    if (this.controls.nbrOfPackages.value <= 0) {
      warnings.push(
        'time.schedule.incomingdelivery.validation.nbrofpackagesislowerthanallowed'
      );
    } else if (this.controls.length.value < minLength) {
      // Only show this warning if the number of packages is greater than 0
      warnings.push(
        'time.schedule.incomingdelivery.validation.lengthislowerthanallowed'
      );
    }

    if (this.controls.minSplitLength.value < minLength) {
      warnings.push(
        'time.schedule.incomingdelivery.validation.minsplitlengthislowerthanallowed'
      );
    }

    if (
      this.controls.startTime.value &&
      this.controls.stopTime.value &&
      this.controls.startTime.value > this.controls.stopTime.value
    ) {
      warnings.push(
        'time.schedule.incomingdelivery.validation.startlaterthanstop'
      );
    }

    if (
      !this.controls.length.value ||
      (this.controls.startTime.value &&
        this.controls.stopTime.value &&
        this.controls.stopTime.value.diffMinutes(
          this.controls.startTime.value
        ) < this.controls.length.value)
    ) {
      warnings.push(
        'time.schedule.incomingdelivery.validation.plannedtimelowerthanlength'
      );
    }

    return warnings;
  }
}

export function createLengthValidator(
  errorMessage: string,
  minLength: number
): ValidatorFn {
  return (form): ValidationErrors | null => {
    let invalidLength = false;
    const rows: IIncomingDeliveryRowDTO[] = form.get('rows')?.value || [];
    rows.forEach(row => {
      if (row.length < minLength) invalidLength = true;
    });

    return invalidLength ? { [errorMessage]: true } : null;
  };
}

export function createMinSplitLengthValidator(
  errorMessage: string,
  minLength: number
): ValidatorFn {
  return (form): ValidationErrors | null => {
    let invalidLength = false;
    const rows: IIncomingDeliveryRowDTO[] = form.get('rows')?.value || [];
    rows.forEach(row => {
      if (row.minSplitLength < minLength) invalidLength = true;
    });

    return invalidLength ? { [errorMessage]: true } : null;
  };
}

export function createStartStopTimeValidator(
  errorMessage: string
): ValidatorFn {
  return (form): ValidationErrors | null => {
    let invalidTime = false;
    const rows: IIncomingDeliveryRowDTO[] = form.get('rows')?.value || [];
    rows.forEach(row => {
      if (row.startTime && row.stopTime && row.startTime > row.stopTime)
        invalidTime = true;
    });

    return invalidTime ? { [errorMessage]: true } : null;
  };
}
