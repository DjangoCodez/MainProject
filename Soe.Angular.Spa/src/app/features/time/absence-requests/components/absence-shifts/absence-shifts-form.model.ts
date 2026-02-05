import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import {
  TermGroup_TimeScheduleTemplateBlockType,
  TermGroup_YesNo,
} from '@shared/models/generated-interfaces/Enumerations';
import { IShiftDTO } from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';

interface IAbsenceShiftsForm {
  validationHandler: ValidationHandler;
  element: IShiftDTO | undefined;
}
export class AbsenceShiftsForm extends SoeFormGroup {
  private originalElement?: IShiftDTO;
  constructor({ validationHandler, element }: IAbsenceShiftsForm) {
    super(validationHandler, {
      timeScheduleTemplateBlockId: new SoeNumberFormControl(
        element?.timeScheduleTemplateBlockId || 0,
        {
          isIdField: true,
        }
      ),
      type: new SoeSelectFormControl(
        element?.type || TermGroup_TimeScheduleTemplateBlockType.Schedule,
        {}
      ),
      timeScheduleEmployeePeriodId: new SoeNumberFormControl(
        element?.timeScheduleEmployeePeriodId || 0
      ),
      // timeScheduleTemplatePeriodId: new SoeNumberFormControl(
      //   element?.timeScheduleTemplatePeriodId || 0
      // ),
      startTime: new SoeDateFormControl(element?.startTime || undefined, {}),
      stopTime: new SoeDateFormControl(element?.stopTime || undefined, {}),
      shiftTypeId: new SoeNumberFormControl(element?.shiftTypeId || 0),
      shiftTypeName: new SoeTextFormControl(element?.shiftTypeName || '', {}),
      approvalTypeId: new SoeSelectFormControl(
        undefined,
        { required: false },
        'time.schedule.absencerequests.approve'
      ),
      replaceWithEmployeeId: new SoeSelectFormControl(
        0,
        {},
        'time.schedule.absencerequests.replacewith'
      ),
      shiftTypeColor: new SoeTextFormControl(
        element?.shiftTypeColor || undefined,
        {
          disabled: false,
        }
      ),
    });
    this.originalElement = element;
  }

  get timeScheduleTemplateBlockId(): SoeNumberFormControl {
    return <SoeNumberFormControl>this.controls.timeScheduleTemplateBlockId;
  }

  get type(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.type;
  }

  get startTime(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.startTime;
  }

  get stopTime(): SoeDateFormControl {
    return <SoeDateFormControl>this.controls.stopTime;
  }

  get shiftTypeName(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.shiftTypeName;
  }

  get approvalTypeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.approvalTypeId;
  }

  get replaceWithEmployeeId(): SoeSelectFormControl {
    return <SoeSelectFormControl>this.controls.replaceWithEmployeeId;
  }

  get onDutyIcon(): string {
    return this.type.value === TermGroup_TimeScheduleTemplateBlockType.OnDuty
      ? 'alarm-exclamation'
      : '';
  }

  get shiftTypeColor(): SoeTextFormControl {
    return <SoeTextFormControl>this.controls.shiftTypeColor;
  }

  toShiftDTO(approveAll: boolean): IShiftDTO | null {
    const formValues = this.getRawValue();

    if (!formValues.replaceWithEmployeeId) {
      return null;
    }

    return {
      ...this.originalElement, // All original backend data
      ...formValues,
      employeeId: formValues.replaceWithEmployeeId,
      absenceStartTime: formValues.startTime,
      absenceStopTime: formValues.stopTime,
      approvalTypeId: approveAll
        ? TermGroup_YesNo.Yes
        : formValues.approvalTypeId,
    } as IShiftDTO;
  }
}
