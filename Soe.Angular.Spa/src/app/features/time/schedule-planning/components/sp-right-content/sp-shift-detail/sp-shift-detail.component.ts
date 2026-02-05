import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { PlanningShiftDTO } from '@features/time/schedule-planning/models/shift.model';
import { SpSettingService } from '@features/time/schedule-planning/services/sp-setting.service';
import { SpShiftService } from '@features/time/schedule-planning/services/sp-shift.service';
import { ShiftUtil } from '@features/time/schedule-planning/util/shift-util';
import { TranslatePipe } from '@ngx-translate/core';
import { MinutesToTimeSpanPipe } from '@shared/pipes';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { IconModule } from '@ui/icon/icon.module';
import { Subscription } from 'rxjs';

@Component({
  selector: 'sp-shift-detail',
  imports: [
    DatePipe,
    DecimalPipe,
    MinutesToTimeSpanPipe,
    TranslatePipe,
    IconModule,
  ],
  templateUrl: './sp-shift-detail.component.html',
  styleUrl: './sp-shift-detail.component.scss',
})
export class SpShiftDetailComponent implements OnInit, OnDestroy {
  shifts = signal<PlanningShiftDTO[] | undefined>(undefined);

  readonly settingService = inject(SpSettingService);
  private readonly shiftService = inject(SpShiftService);

  isSupportAdmin = SoeConfigUtil.isSupportAdmin;

  private selectedShiftsChangedSubscription?: Subscription;

  ngOnInit(): void {
    this.selectedShiftsChangedSubscription =
      this.shiftService.selectedShiftsChanged.subscribe(
        (shifts: PlanningShiftDTO[] | undefined) => {
          if (shifts) this.onSelectedShiftsChanged(shifts);
        }
      );
  }

  ngOnDestroy(): void {
    this.selectedShiftsChangedSubscription?.unsubscribe();
  }

  onSelectedShiftsChanged(shifts: PlanningShiftDTO[]) {
    this.shifts.set(shifts);
  }

  getBreakTimeWithinShift(shift: PlanningShiftDTO) {
    // TODO: This does not support breaks that span multiple shifts
    return ShiftUtil.getBreakTimeWithinShift(shift, [shift]);
  }
}
