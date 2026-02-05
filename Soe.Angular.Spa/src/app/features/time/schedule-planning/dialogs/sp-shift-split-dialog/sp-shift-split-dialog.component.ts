import { Component, inject, OnInit, signal, viewChild } from '@angular/core';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component';
import { ButtonComponent } from '@ui/button/button/button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { IconModule } from '@ui/icon/icon.module';
import {
  TimeboxComponent,
  TimeboxValue,
} from '@ui/forms/timebox/timebox.component';
import { PlanningShiftDTO } from '../../models/shift.model';
import { ReactiveFormsModule } from '@angular/forms';
import { ValidationHandler } from '@shared/handlers';
import { SpShiftSplitDialogForm } from './sp-shift-split-dialog-form.model';
import { SpSettingService } from '../../services/sp-setting.service';
import { SpWorkRuleService } from '../../services/sp-work-rule.service';
import { Observable, tap } from 'rxjs';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { IEvaluateWorkRulesActionResult } from '@shared/models/generated-interfaces/EvaluateWorkRuleResultDTO';
import { TermGroup_ShiftHistoryType } from '@shared/models/generated-interfaces/Enumerations';
import { SchedulePlanningService } from '../../services/schedule-planning.service';
import { SpEmployeeService } from '../../services/sp-employee.service';
import { SpFilterService } from '../../services/sp-filter.service';
import { CrudActionTypeEnum } from '@shared/enums';
import { SkillMatcherComponent } from '@shared/components/time/skill-matcher/skill-matcher.component';
import { SliderComponent } from '@ui/slider/slider.component';
import { SpToolbarEmployeeDate } from '../../toolbar/sp-toolbar-employee-date/sp-toolbar-employee-date';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SpShiftSimpleComponent } from '../../components/sp-shift-simple/sp-shift-simple.component';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

export class SpShiftSplitDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  shift!: PlanningShiftDTO;
}

export class SpShiftSplitDialogResult {
  employeeId1 = 0;
  employeeId2 = 0;
}

@Component({
  selector: 'sp-shift-split-dialog',
  imports: [
    ReactiveFormsModule,
    AutocompleteComponent,
    ButtonComponent,
    DialogComponent,
    ExpansionPanelComponent,
    IconModule,
    SkillMatcherComponent,
    SliderComponent,
    SpShiftSimpleComponent,
    SpToolbarEmployeeDate,
    TimeboxComponent,
    ToolbarComponent,
  ],
  templateUrl: './sp-shift-split-dialog.component.html',
  styleUrl: './sp-shift-split-dialog.component.scss',
})
export class SpShiftSplitDialogComponent
  extends DialogComponent<SpShiftSplitDialogData>
  implements OnInit
{
  validationHandler = inject(ValidationHandler);
  form: SpShiftSplitDialogForm = new SpShiftSplitDialogForm({
    validationHandler: this.validationHandler,
    element: new PlanningShiftDTO(),
  });

  employee1Comp = viewChild('employee1');

  private readonly service = inject(SchedulePlanningService);
  readonly employeeService = inject(SpEmployeeService);
  private readonly filterService = inject(SpFilterService);
  private readonly progressService = inject(ProgressService);
  readonly settingService = inject(SpSettingService);
  private readonly workRuleService = inject(SpWorkRuleService);

  private perform = new Perform<any>(this.progressService);
  executing = signal(false);

  sliderMinValue = signal(0);
  sliderMaxValue = signal(0);
  sliderStep = signal(0);

  keepShiftsTogether = false;

  constructor() {
    super();

    // Bind function to the component instance to be able to use correct this in the function
    this.formatSliderValue = this.formatSliderValue.bind(this);
  }

  ngOnInit(): void {
    if (this.data.shift) {
      this.form.patchShift(this.data.shift);
      this.form.setDefaultSplitTime();
      this.form.setInitialEmployees();

      this.setupSlider();
    }
  }

  private setupSlider() {
    // Value will be in minutes from start time with interval minutes steps
    this.sliderStep.set(this.settingService.dayViewMinorTickLength());
    this.sliderMinValue.set(this.sliderStep());
    this.sliderMaxValue.set(this.form.duration - this.sliderStep());
  }

  formatSliderValue = (value: number) => {
    return this.form.startTime
      ? this.form.startTime.addMinutes(value).toFormattedTime()
      : '';
  };

  onSliderChanged(value: number) {
    this.form.patchValue({
      splitTime: this.form.startTime.addMinutes(value),
    });
  }

  onSplitTimeChanged(value: TimeboxValue) {
    const invalidTime =
      !value ||
      (value as Date).isSameOrBeforeOnMinute(this.form.startTime) ||
      (value as Date).isSameOrAfterOnMinute(this.form.stopTime);

    if (invalidTime) {
      this.form.setDefaultSplitTime();
    } else {
      this.form.setSplitTimeOffset();
    }
  }

  onEmployeeExpansionPanelOpened() {
    // When opening the employee expansion panel, set focus on the first employee autocomplete component, but keep the dropdown closed
    (<any>this.employee1Comp()).setFocus(undefined, 200, true);
  }

  private validateWorkRules(): Observable<IEvaluateWorkRulesActionResult> {
    return this.workRuleService.evaluateSplitShiftAgainstWorkRules(
      this.data.shift,
      this.form.splitTime,
      this.form.employeeId1,
      this.form.employeeId2,
      this.keepShiftsTogether,
      this.filterService.isTemplateView()
    );
  }

  private performSplitShift() {
    this.executing.set(true);

    // TODO: Change server side so timeScheduleTemplateBlockId is passed instead of the whole shift, then load the shift on the server side.
    // Currently there are two different models to handle TimeSchedulePlanningDayDTO vs ShiftDTO.
    // The server requires a TimeSchedulePlanningDayDTO to be passed, so the ShiftDTO is converted to a TimeSchedulePlanningDayDTO in the WebApi.

    setTimeout(() => {
      // A timeout is needed for the validateWorkRules dialog to close before starting this new one.
      // Otherwise the validateWorkRules completed will close the new crud dialog.
      this.perform.crud(
        CrudActionTypeEnum.Save,
        this.service.splitShift(
          this.data.shift,
          this.form.splitTime,
          this.form.employeeId1,
          this.form.employeeId2,
          this.keepShiftsTogether,
          this.filterService.isTemplateView(),
          this.settingService.skipXEMailOnChanges()
        ),
        (res: BackendResponse) => {
          if (res.success) {
            this.dialogRef.close({
              employeeId1: this.form.employeeId1,
              employeeId2: this.form.employeeId2,
            } as SpShiftSplitDialogResult);
          } else {
            this.executing.set(false);
          }
        }
      );
    }, 0);
  }

  cancel() {
    this.dialogRef.close({} as SpShiftSplitDialogResult);
  }

  ok() {
    this.executing.set(true);

    this.perform.load(
      this.validateWorkRules().pipe(
        tap(result => {
          this.executing.set(false);

          this.workRuleService
            .showValidateWorkRulesResult(
              TermGroup_ShiftHistoryType.TaskSplitTimeScheduleShift,
              result,
              this.form.employeeId1
            )
            .subscribe(passed => {
              if (passed) this.performSplitShift();
            });
        })
      ),
      { message: 'time.schedule.planning.evaluateworkrules.executing' }
    );
  }
}
