import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import {
  SpEmployeeDaySlot,
  SpEmployeeHourSlot,
} from '@features/time/schedule-planning/models/time-slot.model';
import { SpEmployeeService } from '@features/time/schedule-planning/services/sp-employee.service';
import { SpFilterService } from '@features/time/schedule-planning/services/sp-filter.service';
import { SpSettingService } from '@features/time/schedule-planning/services/sp-setting.service';
import { TranslatePipe } from '@ngx-translate/core';
import { MinutesToTimeSpanPipe } from '@shared/pipes';
import { Subscription } from 'rxjs';

@Component({
  selector: 'sp-day-slot-detail',
  imports: [DatePipe, DecimalPipe, MinutesToTimeSpanPipe, TranslatePipe],
  templateUrl: './sp-day-slot-detail.component.html',
  styleUrl: './sp-day-slot-detail.component.scss',
})
export class SpDaySlotDetailComponent implements OnInit, OnDestroy {
  slot = signal<SpEmployeeDaySlot | undefined>(undefined);

  private readonly employeeService = inject(SpEmployeeService);
  readonly filterService = inject(SpFilterService);
  readonly settingService = inject(SpSettingService);

  private selectedSlotChangedSubscription?: Subscription;

  ngOnInit(): void {
    this.selectedSlotChangedSubscription =
      this.employeeService.selectedSlotChanged.subscribe(
        (slot: SpEmployeeDaySlot | SpEmployeeHourSlot | undefined) => {
          if (slot instanceof SpEmployeeDaySlot || !slot) {
            this.slot.set(slot);
          }
        }
      );
  }

  ngOnDestroy(): void {
    this.selectedSlotChangedSubscription?.unsubscribe();
  }
}
