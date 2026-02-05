import { ValidationHandler } from '@shared/handlers';
import { ITimeScheduleTypeFactorDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
} from '@shared/extensions';

interface ITimeScheduleTypeFactorForm {
  validationHandler: ValidationHandler;
  element: ITimeScheduleTypeFactorDTO | undefined;
}
export class TimeScheduleTypeFactorForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ITimeScheduleTypeFactorForm) {
    super(validationHandler, {
      factor: new SoeNumberFormControl(element?.factor || 0, {
        decimals: 2,
        maxValue: 999.99,
        maxDecimals: 2,
        minDecimals: 2,
      }),
      fromTime: new SoeDateFormControl(element?.fromTime, {}),
      toTime: new SoeDateFormControl(element?.toTime, {}),
      length: new SoeNumberFormControl(0),
    });
  }
  get timeScheduleTypeFactorId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.timeScheduleTypeFactorId;
  }
  get timeScheduleTypeId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.timeScheduleTypeId;
  }
  get factor(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.factor;
  }
  get fromTime(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.fromTime;
  }
  get toTime(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.toTime;
  }
  get length(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.length;
  }

  setLength() {
    if (!this.controls.fromTime.value || !this.controls.toTime.value) return;

    // Make sure start and stop time is on the same day (or just over midnight)
    const diffDays = this.controls.toTime.value.diffDays(
      this.controls.fromTime.value
    );

    if (diffDays < 0) {
      this.controls.toTime.setValue(
        this.controls.toTime.value.addDays(Math.abs(diffDays))
      );
    } else if (diffDays >= 1) {
      this.controls.toTime.setValue(
        this.controls.toTime.value.addDays(-Math.abs(diffDays))
      );
    }

    let newLength = this.controls.toTime.value.diffMinutes(
      this.controls.fromTime.value
    );
    if (newLength < 0) newLength = 0;

    const isModified = this.controls.length.value !== newLength;

    this.controls.length.setValue(newLength);

    if (isModified) {
      this.controls.length.markAsDirty();
    }
  }

  customPatchValue(element: ITimeScheduleTypeFactorDTO) {
    this.patchValue(element);
  }
}
