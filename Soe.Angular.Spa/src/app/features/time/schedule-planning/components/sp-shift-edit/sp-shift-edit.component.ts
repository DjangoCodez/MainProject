import {
  AfterViewInit,
  Component,
  ElementRef,
  inject,
  input,
  OnInit,
  signal,
  viewChild,
  viewChildren,
} from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component';
import { IconModule } from '@ui/icon/icon.module';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { LabelComponent } from '@ui/label/label.component';
import {
  MenuButtonComponent,
  MenuButtonItem,
} from '@ui/button/menu-button/menu-button.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import {
  TimeboxComponent,
  TimeboxValue,
} from '@ui/forms/timebox/timebox.component';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { SpShiftEditForm } from './sp-shift-edit-form.model';
import { SpFilterService } from '../../services/sp-filter.service';
import { ShiftUtil } from '../../util/shift-util';
import { focusOnElement } from '@shared/util/focus-util';
import { TranslatePipe } from '@ngx-translate/core';
import { SchedulePlanningService } from '../../services/schedule-planning.service';
import { SpTranslateService } from '../../services/sp-translate.service';
import { SpShiftBreakEditForm } from './sp-shift-break-edit-form.model';
import { PlanningShiftBreakDTO } from '../../models/shift.model';
import { SpEventService } from '../../services/sp-event.service';
import { TermGroup_TimeSchedulePlanningShiftStartsOnDay } from '@shared/models/generated-interfaces/Enumerations';

@Component({
  selector: 'sp-shift-edit',
  imports: [
    ReactiveFormsModule,
    ButtonComponent,
    CheckboxComponent,
    DeleteButtonComponent,
    ExpansionPanelComponent,
    IconModule,
    InstructionComponent,
    LabelComponent,
    MenuButtonComponent,
    SelectComponent,
    TextboxComponent,
    TimeboxComponent,
    TranslatePipe,
  ],
  templateUrl: './sp-shift-edit.component.html',
  styleUrl: './sp-shift-edit.component.scss',
})
export class SpShiftEditComponent implements OnInit, AfterViewInit {
  form = input.required<SpShiftEditForm>();

  startTimeElem = viewChild<ElementRef>('startTime');
  stopTimeElem = viewChild<ElementRef>('stopTime');
  breakStartTimeElems = viewChildren<ElementRef>('breakStartTime');

  readonly service = inject(SchedulePlanningService);
  private readonly eventService = inject(SpEventService);
  readonly filterService = inject(SpFilterService);
  readonly spTranslate = inject(SpTranslateService);
  private readonly toasterService = inject(ToasterService);

  hasSkills = signal(true); // TODO: Implement skills functionality

  selectedBlockType: MenuButtonItem | undefined;

  ngOnInit(): void {
    this.selectedBlockType = this.filterService.blockTypeItems.find(
      (item: MenuButtonItem) => item.id === this.form().controls.type.value
    );
  }

  ngAfterViewInit(): void {
    if (this.form().controls.isCreatedAsFirstOnDay.value)
      this.focusOnStartTime();
    else this.focusOnStopTime();
  }

  focusOnStartTime() {
    focusOnElement((<any>this.startTimeElem())?.inputER?.nativeElement);
  }

  focusOnStopTime() {
    focusOnElement((<any>this.stopTimeElem())?.inputER?.nativeElement);
  }

  focusOnBreakStartTime(index: number) {
    const elems = this.breakStartTimeElems();
    const elem = (<any>elems?.[index])?.inputER?.nativeElement;
    focusOnElement(elem);
  }

  deleteShift() {
    this.eventService.deleteCurrentShift();
    this.eventService.updateShiftSummary();
  }

  blockTypeSelected(selected: MenuButtonItem): void {
    if (selected) {
      this.selectedBlockType = selected;
      this.form().controls.type.setValue(selected.id);
    }
  }

  startTimeChanged(time: TimeboxValue): void {
    if (time === '' || time === undefined) {
      // If start time is cleared, set it to midnight current day
      this.form().controls.actualStartTime.setValue(
        new Date(this.form().controls.actualStartDate.value)
      );
    }

    this.timeChanged();
  }

  stopTimeChanged(time: TimeboxValue): void {
    if (time === '' || time === undefined) {
      // If stop time is cleared, set it to start time
      this.form().controls.actualStopTime.setValue(
        new Date(
          this.form().controls.actualStartTime.value ||
            this.form().controls.actualStartDate.value
        )
      );
    }

    this.timeChanged();
  }

  private timeChanged(): void {
    this.form().adjustStopTimeConsideringMidnight();
    this.form().setStopTimeStartsOnBasedOnActualStopTimeAndDate();
    this.form().setShiftLength();
    this.eventService.updateShiftSummary();
  }

  startTimeStartsOnChanged(
    startsOn: TermGroup_TimeSchedulePlanningShiftStartsOnDay
  ): void {
    // Remember shift length, to keep it when changing start time
    const shiftLength = this.form().shiftLength;

    this.form().setStartTimeBasedOnStartsOn(startsOn);
    this.form().setBelongsToBasedOnStartTime();
    this.form().setStopTimeBasedOnStartAndLength(shiftLength);
    this.form().adjustStopTimeConsideringMidnight();
    this.form().setStopTimeStartsOnBasedOnActualStopTimeAndDate();
    this.eventService.updateShiftSummary();
  }

  shiftTypeChanged(shiftTypeId: number): void {
    const shiftType = this.filterService.shiftTypes.find(
      item => item.shiftTypeId === shiftTypeId
    );
    this.form().patchValue({
      shiftTypeName: shiftType?.name,
      shiftTypeColor: shiftType?.color,
      textColor: ShiftUtil.textColor({ shiftTypeColor: shiftType?.color }),
    });
    this.eventService.updateShiftSummary();
  }

  async breakStartTimeChanged(
    breakForm: SpShiftBreakEditForm,
    time: TimeboxValue
  ): Promise<void> {
    let newStartTime: Date;
    if (time === '' || time === undefined) {
      // If start time is cleared, set it to current shift start time
      newStartTime = new Date(this.form().controls.actualStartTime.value);
    } else {
      newStartTime = new Date(time);
    }

    breakForm.controls.startTime.setValue(newStartTime, {
      emitEvent: true,
    });

    if (await this.validateBreakStartTimeBoundary(breakForm)) {
      breakForm.adjustBreakStopTimeConsideringMidnight();
      breakForm.setBreakStopTimeStartsOnBasedOnActualStopTimeAndDate();
    }
    this.breakTimeChanged(breakForm);
  }

  async breakStopTimeChanged(
    breakForm: SpShiftBreakEditForm,
    time: TimeboxValue
  ): Promise<void> {
    let newStopTime: Date;
    if (time === '' || time === undefined) {
      // If stop time is cleared, set it to start time
      newStopTime = new Date(
        breakForm.controls.startTime.value ||
          this.form().controls.actualStartTime.value
      );
    } else {
      newStopTime = new Date(time);
    }

    breakForm.controls.stopTime.setValue(newStopTime, {
      emitEvent: true,
    });

    breakForm.adjustBreakStopTimeConsideringMidnight();
    await this.validateBreakStopTimeBoundary(breakForm);
    this.breakTimeChanged(breakForm);
  }

  async breakStartTimeStartsOnChanged(
    breakForm: SpShiftBreakEditForm,
    startsOn: TermGroup_TimeSchedulePlanningShiftStartsOnDay
  ) {
    breakForm.setBreakStartTimeBasedOnStartsOn(startsOn);
    await this.validateBreakStartTimeBoundary(breakForm);
    breakForm.setBreakBelongsToBasedOnStartTime();
    breakForm.adjustBreakStopTimeConsideringMidnight();
    await this.validateBreakStopTimeBoundary(breakForm);
    breakForm.setBreakStopTimeStartsOnBasedOnActualStopTimeAndDate();
    this.eventService.updateShiftSummary();
  }

  private async validateBreakStartTimeBoundary(
    breakForm: SpShiftBreakEditForm
  ): Promise<boolean> {
    const { startTimeChanged, stopTimeChanged } =
      await breakForm.validateBreakStartTimeBoundary(
        this.form().actualStartTime!,
        this.form().actualStopTime!
      );

    if (startTimeChanged) {
      // TODO: New terms
      this.toasterService.warning(
        'Rasten måste ligga inom passets tider',
        'Rastens starttid justerades'
      );
    }

    if (stopTimeChanged) {
      // TODO: New terms
      this.toasterService.warning(
        'Rasten kan inte sluta innan den har startat',
        'Rastens sluttid justerades'
      );
    }

    return startTimeChanged || stopTimeChanged;
  }

  private async validateBreakStopTimeBoundary(
    breakForm: SpShiftBreakEditForm
  ): Promise<boolean> {
    const { stopTimeChanged, stopTimeBeforeStartTime } =
      await breakForm.validateBreakStopTimeBoundary(
        this.form().actualStartTime!,
        this.form().actualStopTime!
      );

    if (stopTimeChanged) {
      // TODO: New terms
      this.toasterService.warning(
        'Rasten måste ligga inom passets tider',
        'Rastens sluttid justerades'
      );
    }
    if (stopTimeBeforeStartTime) {
      // TODO: New terms
      this.toasterService.warning(
        'Rasten kan inte sluta innan den har startat',
        'Rastens sluttid justerades'
      );
    }

    return stopTimeChanged;
  }

  private breakTimeChanged(breakForm: SpShiftBreakEditForm): void {
    breakForm.setBreakLength();
    this.setTimeCodeFromLength(breakForm);
    this.eventService.updateShiftSummary();
  }

  breakTimeCodeChanged(
    breakForm: SpShiftBreakEditForm,
    timeCodeId: number
  ): void {
    const timeCode = this.service.getTimeCodeBreak(timeCodeId, false);
    if (timeCode) {
      if (breakForm.startTime) {
        breakForm.controls.stopTime.setValue(
          breakForm.startTime?.addMinutes(timeCode.defaultMinutes)
        );
        breakForm.setBreakLength();
        breakForm.setBreakStopTimeStartsOnBasedOnActualStopTimeAndDate();
        this.eventService.updateShiftSummary();
      }
    }
  }

  private setTimeCodeFromLength(breakForm: SpShiftBreakEditForm) {
    const timeCode = this.service.getTimeCodeBreakFromLength(breakForm.minutes);
    breakForm.controls.timeCodeId.setValue(timeCode?.timeCodeId || 0);
  }

  addBreak() {
    const breakForm = this.form().createShiftBreakFormFromDTO(
      new PlanningShiftBreakDTO()
    );

    // If there are existing breaks, set the new break start time to the latest stop time.
    // If no existing breaks, set it to the shift start time.
    let tempBreakId = 0;
    let start: Date;
    const breaks = this.form().breaks;
    if (breaks.length > 0) {
      tempBreakId =
        Math.max(
          ...this.form().breaks.controls.map(b => b.value.tempBreakId || 0)
        ) + 1;

      const latestStopTime = breaks.controls
        .map(b => b.value.stopTime)
        .filter(Boolean)
        .map(d => new Date(d))
        .sort((a, b) => b.getTime() - a.getTime())[0];

      start = latestStopTime
        ? new Date(latestStopTime)
        : new Date(this.form().actualStartTime!);
    } else {
      tempBreakId = 1;
      start = new Date(this.form().actualStartTime!);
    }
    const stop: Date = new Date(start);

    breakForm.patchValue({
      tempBreakId: tempBreakId,
      timeCodeId: 0,
      startTime: start,
      stopTime: stop,
      actualStartDate: new Date(start),
      minutes: 0,
    });
    const index = this.form().addBreak(breakForm);
    setTimeout(() => {
      this.focusOnBreakStartTime(index);
    }, 0);
  }

  deleteBreak(index: number) {
    this.form().deleteBreak(index);
    this.eventService.updateShiftSummary();
  }
}
