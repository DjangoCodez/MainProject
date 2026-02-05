import { Component, inject, OnInit } from '@angular/core';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { IconModule } from '@ui/icon/icon.module';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TimeboxComponent } from '@ui/forms/timebox/timebox.component';
import { SpSettingService } from '../../services/sp-setting.service';
import { ValidationHandler } from '@shared/handlers';
import { SpSettingDialogForm } from './sp-setting-dialog-form.model';
import { SchedulePlanningSetting } from '../../models/setting.model';
import { ReactiveFormsModule } from '@angular/forms';
import {
  CompanySettingType,
  SettingMainType,
  TermGroup_TimeSchedulePlanningViews,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  MatTab,
  MatTabChangeEvent,
  MatTabGroup,
  MatTabsModule,
} from '@angular/material/tabs';
import { TranslateModule } from '@ngx-translate/core';
import { SpFilterService } from '../../services/sp-filter.service';
import { DateUtil } from '@shared/util/date-util';
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component';
import { LabelComponent } from '@ui/label/label.component';

export class SpSettingDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
}

export class SpSettingDialogResult {
  // Changed user and company settins
  changedSettings: SpSettingChangedEvent[] = [];

  // Changed selectable information that needs action
  loadGrossNetAndCost = false;
  calculateTimes = false;
  updateEmployeeTooltip = false;
  updateContentHeight = false;
}

export class SpSettingChangedEvent {
  settingMainType: SettingMainType = SettingMainType.Company;
  settingType: CompanySettingType | UserSettingType =
    CompanySettingType.Unknown;
}

@Component({
  selector: 'sp-setting-dialog',
  imports: [
    DialogComponent,
    ExpansionPanelComponent,
    ReactiveFormsModule,
    MatTabsModule,
    MatTabGroup,
    MatTab,
    ButtonComponent,
    CheckboxComponent,
    IconModule,
    InstructionComponent,
    LabelComponent,
    NumberboxComponent,
    SelectComponent,
    TimeboxComponent,
    TranslateModule,
    ExpansionPanelComponent,
  ],
  templateUrl: './sp-setting-dialog.component.html',
  styleUrl: './sp-setting-dialog.component.scss',
})
export class SpSettingDialogComponent
  extends DialogComponent<SpSettingDialogData>
  implements OnInit
{
  readonly filterService = inject(SpFilterService);
  readonly settingService = inject(SpSettingService);

  validationHandler = inject(ValidationHandler);
  form: SpSettingDialogForm = new SpSettingDialogForm({
    validationHandler: this.validationHandler,
    element: new SchedulePlanningSetting(),
  });

  private result?: SpSettingDialogResult;

  ngOnInit(): void {
    this.loadSettings();
  }

  private loadSettings() {
    // Get values from service and set them to the form
    this.form.patchValue({
      dayViewStartTime: this.settingService.dayViewStartTime(),
      dayViewEndTime: this.settingService.dayViewEndTime(),
      dayViewMinorTickLength: this.settingService.dayViewMinorTickLength(),
      defaultView: this.settingService.defaultView(),
      defaultInterval: this.settingService.defaultInterval(),
      dayViewDefaultSortBy: this.settingService.dayViewDefaultSortBy(),
      scheduleViewDefaultSortBy:
        this.settingService.scheduleViewDefaultSortBy(),
      disableAutoLoad: this.settingService.disableAutoLoad(),
      showEmployeeGroup: this.settingService.showEmployeeGroup(),
      showCyclePlannedTime: this.settingService.showCyclePlannedTime(),
      showScheduleTypeFactorTime:
        this.settingService.showScheduleTypeFactorTime(),
      showGrossTime: this.settingService.showGrossTime(),
      showTotalCost: this.settingService.showTotalCost(),
      showTotalCostIncEmpTaxAndSuppCharge:
        this.settingService.showTotalCostIncEmpTaxAndSuppCharge(),
      showAvailability: this.settingService.showAvailability(),
      skipXEMailOnChanges: this.settingService.skipXEMailOnChanges(),
      skipWorkRules: this.settingService.skipWorkRules(),
      shiftRequestPreventTooEarly:
        this.settingService.shiftRequestPreventTooEarly(),
      shiftRequestPreventTooEarlyWarnHoursBefore:
        this.settingService.shiftRequestPreventTooEarlyWarnHoursBefore(),
      shiftRequestPreventTooEarlyStopHoursBefore:
        this.settingService.shiftRequestPreventTooEarlyStopHoursBefore(),
      summaryInFooter: this.settingService.summaryInFooter(),
    });
    this.form.setInitialFormattedValues();
  }

  // EVENTS

  onTabChanged(event: MatTabChangeEvent) {
    console.log(event);
  }

  onShowTotalCostChanged() {
    if (!this.form.controls.showTotalCost.value) {
      this.form.controls.showTotalCostIncEmpTaxAndSuppCharge.setValue(false);
      //this.form.controls.showWeekendSalary.setValue(false);
    }
  }

  onShowTotalCostIncEmpTaxAndSuppChargeChanged() {
    if (this.form.controls.showTotalCostIncEmpTaxAndSuppCharge.value) {
      this.form.controls.showTotalCost.setValue(true);
    }
  }

  // toggleShowWeekendSalary() {
  //   if (this.form.controls.showWeekendSalary.value) this.form.controls.showTotalCost.setValue(true);
  // }

  cancel() {
    this.dialogRef.close(false);
  }

  ok() {
    // Save form values to service
    // Only save if the value has changed and return the changed settings
    this.result = new SpSettingDialogResult();

    // Company settings

    const newDayViewStartTime = DateUtil.timeSpanToMinutes(
      this.form.value.dayViewStartTimeFormatted
    );
    if (
      this.isCompanySettingChanged<number>(
        CompanySettingType.TimeSchedulePlanningDayViewStartTime,
        this.settingService.dayViewStartTime(),
        newDayViewStartTime
      )
    ) {
      this.settingService.dayViewStartTime.set(newDayViewStartTime);
      // TODO: Notify changes
    }

    const newDayViewEndTime = DateUtil.timeSpanToMinutes(
      this.form.value.dayViewEndTimeFormatted
    );
    if (
      this.isCompanySettingChanged<number>(
        CompanySettingType.TimeSchedulePlanningDayViewEndTime,
        this.settingService.dayViewEndTime(),
        newDayViewEndTime
      )
    ) {
      this.settingService.dayViewEndTime.set(newDayViewEndTime);
      // TODO: Notify changes
    }

    const newDayViewMinorTickLength = this.form.value.dayViewMinorTickLength;
    if (
      this.isCompanySettingChanged<number>(
        CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength,
        this.settingService.dayViewMinorTickLength(),
        newDayViewMinorTickLength
      )
    ) {
      this.settingService.dayViewMinorTickLength.set(newDayViewMinorTickLength);
      // TODO: Notify changes
    }

    const newShiftRequestPreventTooEarly =
      this.form.value.shiftRequestPreventTooEarly;
    if (
      this.isCompanySettingChanged<boolean>(
        CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarly,
        this.settingService.shiftRequestPreventTooEarly(),
        newShiftRequestPreventTooEarly
      )
    ) {
      this.settingService.shiftRequestPreventTooEarly.set(
        newShiftRequestPreventTooEarly
      );
      // TODO: Notify changes
    }

    const newShiftRequestPreventTooEarlyWarnHoursBefore =
      this.form.value.shiftRequestPreventTooEarlyWarnHoursBefore;
    if (
      this.isCompanySettingChanged<number>(
        CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarlyWarnHoursBefore,
        this.settingService.shiftRequestPreventTooEarlyWarnHoursBefore(),
        newShiftRequestPreventTooEarlyWarnHoursBefore
      )
    ) {
      this.settingService.shiftRequestPreventTooEarlyWarnHoursBefore.set(
        newShiftRequestPreventTooEarlyWarnHoursBefore
      );
      // TODO: Notify changes
    }

    const newShiftRequestPreventTooEarlyStopHoursBefore =
      this.form.value.shiftRequestPreventTooEarlyStopHoursBefore;
    if (
      this.isCompanySettingChanged<number>(
        CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarlyStopHoursBefore,
        this.settingService.shiftRequestPreventTooEarlyStopHoursBefore(),
        newShiftRequestPreventTooEarlyStopHoursBefore
      )
    ) {
      this.settingService.shiftRequestPreventTooEarlyStopHoursBefore.set(
        newShiftRequestPreventTooEarlyStopHoursBefore
      );
      // TODO: Notify changes
    }

    // User settings

    const newDefaultView = this.form.value.defaultView;
    if (
      this.isUserSettingChanged<number>(
        UserSettingType.TimeSchedulePlanningDefaultView,
        this.settingService.defaultView(),
        newDefaultView
      )
    ) {
      this.settingService.defaultView.set(newDefaultView);
      this.filterService.setViewDefinition(newDefaultView);
    }

    const newDefaultInterval = this.form.value.defaultInterval;
    if (
      this.isUserSettingChanged<number>(
        UserSettingType.TimeSchedulePlanningDefaultInterval,
        this.settingService.defaultInterval(),
        newDefaultInterval
      )
    ) {
      this.settingService.defaultInterval.set(newDefaultInterval);
      this.filterService.setScheduleViewInterval(newDefaultInterval);
    }

    const newDayViewDefaultSortBy = this.form.value.dayViewDefaultSortBy;
    if (
      this.isUserSettingChanged<number>(
        UserSettingType.TimeSchedulePlanningDayViewDefaultSortBy,
        this.settingService.dayViewDefaultSortBy(),
        newDayViewDefaultSortBy
      )
    ) {
      this.settingService.dayViewDefaultSortBy.set(newDayViewDefaultSortBy);
      this.settingService.dayViewDefaultSortByChanged.next(
        newDayViewDefaultSortBy
      );
    }

    const newScheduleViewDefaultSortBy =
      this.form.value.scheduleViewDefaultSortBy;
    if (
      this.isUserSettingChanged<number>(
        UserSettingType.TimeSchedulePlanningScheduleViewDefaultSortBy,
        this.settingService.scheduleViewDefaultSortBy(),
        newScheduleViewDefaultSortBy
      )
    ) {
      this.settingService.scheduleViewDefaultSortBy.set(
        newScheduleViewDefaultSortBy
      );
      this.settingService.scheduleViewDefaultSortByChanged.next(
        newScheduleViewDefaultSortBy
      );
    }

    const newDisableAutoLoad = this.form.value.disableAutoLoad;
    if (
      this.isUserSettingChanged<boolean>(
        UserSettingType.TimeSchedulePlanningDisableAutoLoad,
        this.settingService.disableAutoLoad(),
        newDisableAutoLoad
      )
    ) {
      this.settingService.disableAutoLoad.set(newDisableAutoLoad);
      this.settingService.disableAutoLoadChanged.next(newDisableAutoLoad);
    }

    // Selectable information

    let selectableInformationChanged = false;
    const newShowEmployeeGroup = this.form.value.showEmployeeGroup;
    if (newShowEmployeeGroup !== this.settingService.showEmployeeGroup()) {
      this.settingService.showEmployeeGroup.set(newShowEmployeeGroup);
      selectableInformationChanged = true;
      this.result.updateEmployeeTooltip = true;
    }

    const newShowCyclePlannedTime = this.form.value.showCyclePlannedTime;
    if (
      newShowCyclePlannedTime !== this.settingService.showCyclePlannedTime()
    ) {
      this.settingService.showCyclePlannedTime.set(newShowCyclePlannedTime);
      selectableInformationChanged = true;
      this.result.calculateTimes = true;
    }

    const newShowScheduleTypeFactorTime =
      this.form.value.showScheduleTypeFactorTime;
    if (
      newShowScheduleTypeFactorTime !==
      this.settingService.showScheduleTypeFactorTime()
    ) {
      this.settingService.showScheduleTypeFactorTime.set(
        newShowScheduleTypeFactorTime
      );
      selectableInformationChanged = true;
      this.result.calculateTimes = true;
    }

    const newShowGrossTime = this.form.value.showGrossTime;
    if (newShowGrossTime !== this.settingService.showGrossTime()) {
      this.settingService.showGrossTime.set(newShowGrossTime);
      selectableInformationChanged = true;

      if (newShowGrossTime) {
        this.result.loadGrossNetAndCost = true;
      } else {
        this.result.calculateTimes = true;
      }
      this.result.updateContentHeight = true;
    }

    const newShowTotalCost = this.form.value.showTotalCost;
    if (newShowTotalCost !== this.settingService.showTotalCost()) {
      this.settingService.showTotalCost.set(newShowTotalCost);
      selectableInformationChanged = true;

      if (newShowTotalCost) {
        this.result.loadGrossNetAndCost = true;
      } else {
        this.result.calculateTimes = true;
      }
      this.result.updateContentHeight = true;
    }

    const newShowTotalCostIncEmpTaxAndSuppCharge =
      this.form.value.showTotalCostIncEmpTaxAndSuppCharge;
    if (
      newShowTotalCostIncEmpTaxAndSuppCharge !==
      this.settingService.showTotalCostIncEmpTaxAndSuppCharge()
    ) {
      this.settingService.showTotalCostIncEmpTaxAndSuppCharge.set(
        newShowTotalCostIncEmpTaxAndSuppCharge
      );
      selectableInformationChanged = true;

      if (newShowTotalCostIncEmpTaxAndSuppCharge) {
        this.result.loadGrossNetAndCost = true;
      } else {
        this.result.calculateTimes = true;
      }
      this.result.updateContentHeight = true;
    }

    const newShowAvailability = this.form.value.showAvailability;
    if (newShowAvailability !== this.settingService.showAvailability()) {
      this.settingService.showAvailability.set(newShowAvailability);
      selectableInformationChanged = true;
    }

    const newSkipXEMailOnChanges = this.form.value.skipXEMailOnChanges;
    if (newSkipXEMailOnChanges !== this.settingService.skipXEMailOnChanges()) {
      this.settingService.skipXEMailOnChanges.set(newSkipXEMailOnChanges);
      selectableInformationChanged = true;
    }

    const newSkipWorkRules = this.form.value.skipWorkRules;
    if (newSkipWorkRules !== this.settingService.skipWorkRules()) {
      this.settingService.skipWorkRules.set(newSkipWorkRules);
      selectableInformationChanged = true;
    }

    const newSummaryInFooter = this.form.value.summaryInFooter;
    if (newSummaryInFooter !== this.settingService.summaryInFooter()) {
      this.settingService.summaryInFooter.set(newSummaryInFooter);
      selectableInformationChanged = true;
    }

    if (selectableInformationChanged) {
      this.settingService
        .saveSelectableInformationSettings(this.filterService.viewDefinition())
        .subscribe();

      switch (this.filterService.viewDefinition()) {
        case TermGroup_TimeSchedulePlanningViews.Day:
          this.addChangedUserSettingToResult(
            UserSettingType.TimeSchedulePlanningSelectableInformationSettingsDayView
          );
          break;
        case TermGroup_TimeSchedulePlanningViews.Schedule:
          this.addChangedUserSettingToResult(
            UserSettingType.TimeSchedulePlanningSelectableInformationSettingsScheduleView
          );
          break;
      }
    }

    this.dialogRef.close(this.result);
  }

  private isCompanySettingChanged<T>(
    settingType: CompanySettingType,
    oldValue: T,
    newValue: T
  ): boolean {
    if (newValue !== oldValue) {
      switch (typeof oldValue) {
        case 'boolean':
          this.settingService.saveBoolCompanySetting(
            settingType,
            <boolean>newValue
          );
          break;
        case 'number':
          this.settingService.saveIntCompanySetting(
            settingType,
            <number>newValue
          );
          break;
      }
      this.addChangedCompanySettingToResult(settingType);
      return true;
    }

    return false;
  }

  private isUserSettingChanged<T>(
    settingType: UserSettingType,
    oldValue: T,
    newValue: T
  ): boolean {
    if (newValue !== oldValue) {
      switch (typeof oldValue) {
        case 'boolean':
          this.settingService.saveBoolUserSetting(
            settingType,
            <boolean>newValue
          );
          break;
        case 'number':
          this.settingService.saveIntUserSetting(settingType, <number>newValue);
          break;
      }
      this.addChangedUserSettingToResult(settingType);
      return true;
    }

    return false;
  }

  private addChangedCompanySettingToResult(settingType: CompanySettingType) {
    this.result?.changedSettings.push({
      settingMainType: SettingMainType.Company,
      settingType,
    });
  }

  private addChangedUserSettingToResult(settingType: UserSettingType) {
    this.result?.changedSettings.push({
      settingMainType: SettingMainType.User,
      settingType,
    });
  }
}
