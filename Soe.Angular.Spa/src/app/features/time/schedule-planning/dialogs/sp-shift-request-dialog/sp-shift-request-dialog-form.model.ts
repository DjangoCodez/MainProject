import { FormArray } from '@angular/forms';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SpShiftRequestDialogShiftForm } from './sp-shift-request-dialog-shift-form.model';
import { PlanningShiftDTO } from '../../models/shift.model';
import { ShiftUtil } from '../../util/shift-util';
import { PlanningEmployeeDTO } from '../../models/employee.model';
import { IAvailableEmployeesDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { arrayToFormArray } from '@shared/util/form-util';

interface ISpShiftRequestDialogFormElement {
  employeeId: number;
  employeeName: string;
  date: Date;
  subject: string;
  text: string;
  shortText: string;
  shifts: PlanningShiftDTO[];
  availableEmployees: IAvailableEmployeesDTO[];
  selectedEmployees: IAvailableEmployeesDTO[];
}

export class SpShiftRequestDialogForm extends SoeFormGroup {
  thisValidationHandler: ValidationHandler;

  shifts: PlanningShiftDTO[] = [];
  possibleEmployees: PlanningEmployeeDTO[] = [];

  constructor(args: {
    validationHandler: ValidationHandler;
    element: ISpShiftRequestDialogFormElement | undefined;
  }) {
    super(args.validationHandler, {
      employeeId: new SoeNumberFormControl(args.element?.employeeId ?? 0),
      employeeName: new SoeTextFormControl(args.element?.employeeName ?? ''),
      date: new SoeDateFormControl(args.element?.date ?? new Date()),
      shifts: new FormArray<SpShiftRequestDialogShiftForm>([]),
      subject: new SoeTextFormControl(args.element?.subject ?? '', {
        required: true,
      }),
      text: new SoeTextFormControl(args.element?.text ?? ''),
      shortText: new SoeTextFormControl(args.element?.shortText ?? ''),
      copyToEmail: new SoeCheckboxFormControl(false),
      filterOnShiftType: new SoeCheckboxFormControl(true),
      filterOnSkills: new SoeCheckboxFormControl(true),
      filterOnWorkRules: new SoeCheckboxFormControl(true),
      filterOnAvailability: new SoeCheckboxFormControl(true),
      filterOnMessageGroupId: new SoeNumberFormControl(0),
      availableEmployees: arrayToFormArray(
        args.element?.availableEmployees || []
      ),
      selectedEmployees: arrayToFormArray(
        args.element?.selectedEmployees || []
      ),
    });

    this.thisValidationHandler = args.validationHandler;
  }

  get filterOnShiftType(): boolean {
    return this.controls.filterOnShiftType.value;
  }

  get filterOnSkills(): boolean {
    return this.controls.filterOnSkills.value;
  }

  get filterOnWorkRules(): boolean {
    return this.controls.filterOnWorkRules.value;
  }

  get filterOnAvailability(): boolean {
    return this.controls.filterOnAvailability.value;
  }

  get filterOnMessageGroupId(): number {
    return this.controls.filterOnMessageGroupId.value;
  }

  get shiftForms(): FormArray<SpShiftRequestDialogShiftForm> {
    return <FormArray>this.controls.shifts;
  }

  get availableEmployees(): FormArray<AvailableEmployeeForm> {
    return <FormArray>this.controls.availableEmployees;
  }

  get selectedEmployees(): FormArray<AvailableEmployeeForm> {
    return <FormArray>this.controls.selectedEmployees;
  }

  get recipientsSummary(): string {
    return `${this.possibleEmployees.length} möjliga, ${this.availableEmployees.length} tillgängliga, ${this.selectedEmployees.length} valda`;
  }

  patchShifts(shifts: PlanningShiftDTO[]) {
    this.shifts = shifts;
    this.shiftForms.clear();

    for (const shift of shifts) {
      this.shiftForms.push(
        ShiftUtil.createShiftFormFromDTO(SpShiftRequestDialogShiftForm, {
          validationHandler: this.thisValidationHandler,
          element: shift,
        }),
        {
          emitEvent: false,
        }
      );
    }
    this.shiftForms.updateValueAndValidity();
  }

  patchAvailableEmployees(employees: IAvailableEmployeesDTO[]) {
    this.availableEmployees.clear({ emitEvent: false });
    employees.forEach(dt => {
      const form = new AvailableEmployeeForm({
        validationHandler: this.thisValidationHandler,
        element: dt,
      });
      form.customPatchValue(dt);
      this.availableEmployees.push(form);
    });
    this.availableEmployees.markAsUntouched({
      onlySelf: true,
    });
    this.availableEmployees.markAsPristine({
      onlySelf: true,
    });
    this.availableEmployees.updateValueAndValidity();
  }

  patchSelectedEmployees(employees: IAvailableEmployeesDTO[]) {
    this.selectedEmployees.clear({ emitEvent: false });
    employees.forEach(dt => {
      const form = new AvailableEmployeeForm({
        validationHandler: this.thisValidationHandler,
        element: dt,
      });
      form.customPatchValue(dt);
      this.selectedEmployees.push(form);
    });
    this.selectedEmployees.markAsUntouched({
      onlySelf: true,
    });
    this.selectedEmployees.markAsPristine({
      onlySelf: true,
    });
    this.selectedEmployees.updateValueAndValidity();
  }
}

interface IAvailableEmployeeForm {
  validationHandler: ValidationHandler;
  element: IAvailableEmployeesDTO | undefined;
}
export class AvailableEmployeeForm extends SoeFormGroup {
  constructor({ validationHandler, element }: IAvailableEmployeeForm) {
    super(validationHandler, {
      employeeId: new SoeNumberFormControl(element?.employeeId || 0),
      employeeNr: new SoeTextFormControl(element?.employeeNr || ''),
      employeeName: new SoeTextFormControl(element?.employeeName || ''),
    });
  }

  customPatchValue(element: IAvailableEmployeesDTO) {
    this.patchValue(element);
  }
}
