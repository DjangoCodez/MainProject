import {
  Component,
  input,
  OnInit,
  OnDestroy,
  signal,
  inject,
} from '@angular/core';
import { SpShiftEditForm } from '../sp-shift-edit-form.model';
import { ReactiveFormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { IconModule } from '@ui/icon/icon.module';
import { MinutesToTimeSpanPipe } from '@shared/pipes';
import { SpShiftDialogForm } from '@features/time/schedule-planning/dialogs/sp-shift-dialog/sp-shift-dialog-form.model';
import { ShiftUtil } from '@features/time/schedule-planning/util/shift-util';
import { Subscription } from 'rxjs';
import { SpEventService } from '@features/time/schedule-planning/services/sp-event.service';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'sp-shift-edit-overview',
  imports: [
    ReactiveFormsModule,
    IconModule,
    DatePipe,
    MinutesToTimeSpanPipe,
    TranslatePipe,
  ],
  templateUrl: './sp-shift-edit-overview.component.html',
  styleUrls: [
    '../../schedule-planning/schedule-planning-shift-common.scss',
    './sp-shift-edit-overview.component.scss',
  ],
})
export class SpShiftEditOverviewComponent implements OnInit, OnDestroy {
  form = input.required<SpShiftEditForm>();
  dayForm = input.required<SpShiftDialogForm>();
  isSelected = input(false);

  private readonly eventService = inject(SpEventService);

  actualStartTime = signal<Date | undefined>(undefined);
  actualStopTime = signal<Date | undefined>(undefined);

  shiftLengthExcludingBreaks = signal<number>(0);
  shiftBreaksLength = signal<number>(0);

  private startTimeChangedSubscription?: Subscription;
  private stopTimeChangedSubscription?: Subscription;
  private shiftSummaryNeedsUpdateSubscription?: Subscription;

  ngOnInit(): void {
    // Set initial values for start and stop time
    this.actualStartTime.set(this.form().controls.actualStartTime.value);
    this.actualStopTime.set(this.form().controls.actualStopTime.value);
    this.setShiftLengths();

    // Seems to be a problem with the timebox not emitting the change correctly.
    // The form is being updated, but the signals are not reflecting the changes.
    // Have to do this ugly workaround for now with it's own signals for startTime and stopTime,
    // that every time they change, first is set to undefined and then back again to make the UI update.
    if (this.form) {
      this.startTimeChangedSubscription =
        this.form().controls.actualStartTime.valueChanges.subscribe(
          (value: any) => {
            this.actualStartTime.set(undefined);
            setTimeout(() => {
              this.actualStartTime.set(value);
            });
          }
        );
      this.stopTimeChangedSubscription =
        this.form().controls.actualStopTime.valueChanges.subscribe(
          (value: any) => {
            this.actualStopTime.set(undefined);
            setTimeout(() => {
              this.actualStopTime.set(value);
            });
          }
        );
      this.shiftSummaryNeedsUpdateSubscription =
        this.eventService.shiftSummaryNeedsUpdate.subscribe(
          (event: any | undefined) => {
            if (event) this.setShiftLengths();
          }
        );
    }
  }

  ngOnDestroy(): void {
    if (this.startTimeChangedSubscription) {
      this.startTimeChangedSubscription.unsubscribe();
    }
    if (this.stopTimeChangedSubscription) {
      this.stopTimeChangedSubscription.unsubscribe();
    }
    if (this.shiftSummaryNeedsUpdateSubscription) {
      this.shiftSummaryNeedsUpdateSubscription.unsubscribe();
    }
  }

  private setShiftLengths() {
    this.shiftLengthExcludingBreaks.set(
      ShiftUtil.shiftLengthExcludingBreaks(
        this.form().value,
        this.dayForm().shifts.controls.map(s => s.value)
      )
    );

    this.shiftBreaksLength.set(
      ShiftUtil.shiftBreaksLength(
        this.form().value,
        this.dayForm().shifts.controls.map(s => s.value)
      )
    );
  }
}
