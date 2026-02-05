import { SoeDateFormControl, SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';

interface ITrackChangesForm {
  validationHandler: ValidationHandler;
  element: TrackChangesRowsFilter;
}

export class TrackChangesRowsFilter {
  from: Date = new Date(new Date().addDays(-30));
  to: Date = new Date();

  constructor() {}
}

export class TrackChangesForm extends SoeFormGroup {
  minutes = 60 * 24;

  get fromDate(): SoeDateFormControl {
    return this.controls.from as SoeDateFormControl;
  }

  get toDate(): SoeDateFormControl {
    return this.controls.to as SoeDateFormControl;
  }

  constructor({ validationHandler, element }: ITrackChangesForm) {
    super(validationHandler, {
      from: new SoeDateFormControl(element.from),
      to: new SoeDateFormControl(element.to),
    });
  }

  public moveBackward(): void {
    const diffMinutes =
      this.fromDate.value.diffMinutes(this.toDate.value) - this.minutes;
    this.fromDate.patchValue(
      this.fromDate.value.addDays(diffMinutes / this.minutes)
    );
    this.toDate.patchValue(
      this.toDate.value.addDays(diffMinutes / this.minutes)
    );
  }

  public moveForward(): void {
    const diffMinutes =
      this.toDate.value.diffMinutes(this.fromDate.value) + this.minutes;
    this.fromDate.patchValue(
      this.fromDate.value.addDays(diffMinutes / this.minutes)
    );
    this.toDate.patchValue(
      this.toDate.value.addDays(diffMinutes / this.minutes)
    );
  }
}
