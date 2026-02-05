import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeNumberFormControl,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SchedulePlanningSetting } from '../../models/setting.model';
import {
  TermGroup_StaffingNeedsHeadInterval,
  TermGroup_TimeSchedulePlanningDayViewSortBy,
  TermGroup_TimeSchedulePlanningScheduleViewSortBy,
  TermGroup_TimeSchedulePlanningViews,
  TermGroup_TimeSchedulePlanningVisibleDays,
} from '@shared/models/generated-interfaces/Enumerations';
import { DateUtil } from '@shared/util/date-util';

interface ISpSettingDialogForm {
  validationHandler: ValidationHandler;
  element: SchedulePlanningSetting | undefined;
}
export class SpSettingDialogForm extends SoeFormGroup {
  constructor({ validationHandler, element }: ISpSettingDialogForm) {
    super(validationHandler, {
      dayViewStartTime: new SoeTextFormControl(element?.dayViewStartTime || 0), // Minutes since midnight
      dayViewStartTimeFormatted: new SoeTextFormControl(
        element?.dayViewStartTime
      ),
      dayViewEndTime: new SoeTextFormControl(element?.dayViewEndTime || 0),
      dayViewEndTimeFormatted: new SoeTextFormControl(element?.dayViewEndTime), // Minutes since midnight
      dayViewMinorTickLength: new SoeSelectFormControl(
        element?.dayViewMinorTickLength ||
          TermGroup_StaffingNeedsHeadInterval.FifteenMinutes
      ),
      defaultView: new SoeSelectFormControl(
        element?.defaultView || TermGroup_TimeSchedulePlanningViews.Schedule
      ),
      defaultInterval: new SoeSelectFormControl(
        element?.defaultInterval ||
          TermGroup_TimeSchedulePlanningVisibleDays.Week
      ),
      dayViewDefaultSortBy: new SoeSelectFormControl(
        element?.dayViewDefaultSortBy ||
          TermGroup_TimeSchedulePlanningDayViewSortBy.EmployeeNr
      ),
      scheduleViewDefaultSortBy: new SoeSelectFormControl(
        element?.scheduleViewDefaultSortBy ||
          TermGroup_TimeSchedulePlanningScheduleViewSortBy.EmployeeNr
      ),
      disableAutoLoad: new SoeCheckboxFormControl(
        element?.disableAutoLoad || true
      ),
      showEmployeeGroup: new SoeCheckboxFormControl(
        element?.showEmployeeGroup || false
      ),
      showCyclePlannedTime: new SoeCheckboxFormControl(
        element?.showCyclePlannedTime || false
      ),
      showScheduleTypeFactorTime: new SoeCheckboxFormControl(
        element?.showScheduleTypeFactorTime || false
      ),
      showGrossTime: new SoeCheckboxFormControl(
        element?.showGrossTime || false
      ),
      showTotalCost: new SoeCheckboxFormControl(
        element?.showTotalCost || false
      ),
      showTotalCostIncEmpTaxAndSuppCharge: new SoeCheckboxFormControl(
        element?.showTotalCostIncEmpTaxAndSuppCharge || false
      ),
      showAvailability: new SoeCheckboxFormControl(
        element?.showAvailability || false
      ),
      skipXEMailOnChanges: new SoeCheckboxFormControl(
        element?.skipXEMailOnChanges || false
      ),
      skipWorkRules: new SoeCheckboxFormControl(
        element?.skipWorkRules || false
      ),
      shiftRequestPreventTooEarly: new SoeCheckboxFormControl(
        element?.shiftRequestPreventTooEarly || false
      ),
      shiftRequestPreventTooEarlyWarnHoursBefore: new SoeNumberFormControl(
        element?.shiftRequestPreventTooEarlyWarnHoursBefore || 0
      ),
      shiftRequestPreventTooEarlyStopHoursBefore: new SoeNumberFormControl(
        element?.shiftRequestPreventTooEarlyStopHoursBefore || 0
      ),
      summaryInFooter: new SoeCheckboxFormControl(
        element?.summaryInFooter || false
      ),
    });
  }

  setInitialFormattedValues() {
    this.controls.dayViewStartTimeFormatted.setValue(
      DateUtil.minutesToTimeSpan(
        this.controls.dayViewStartTime.value,
        false,
        false,
        true,
        false
      )
    );

    this.controls.dayViewEndTimeFormatted.setValue(
      DateUtil.minutesToTimeSpan(
        this.controls.dayViewEndTime.value,
        false,
        false,
        true,
        false
      )
    );
  }
}
