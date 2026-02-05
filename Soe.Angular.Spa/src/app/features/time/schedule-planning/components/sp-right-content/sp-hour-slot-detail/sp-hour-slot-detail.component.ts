import { DatePipe } from '@angular/common';
import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import {
  SpEmployeeDaySlot,
  SpEmployeeHalfHourSlot,
  SpEmployeeHourSlot,
  SpEmployeeQuarterHourSlot,
} from '@features/time/schedule-planning/models/time-slot.model';
import { SpEmployeeService } from '@features/time/schedule-planning/services/sp-employee.service';
import { TranslatePipe } from '@ngx-translate/core';
import { Subscription } from 'rxjs';

@Component({
  selector: 'sp-hour-slot-detail',
  imports: [DatePipe, TranslatePipe],
  templateUrl: './sp-hour-slot-detail.component.html',
  styleUrl: './sp-hour-slot-detail.component.scss',
})
export class SpHourSlotDetailComponent implements OnInit, OnDestroy {
  slot = signal<
    | SpEmployeeHourSlot
    | SpEmployeeHalfHourSlot
    | SpEmployeeQuarterHourSlot
    | undefined
  >(undefined);

  private readonly employeeService = inject(SpEmployeeService);

  private selectedSlotChangedSubscription?: Subscription;

  ngOnInit(): void {
    this.selectedSlotChangedSubscription =
      this.employeeService.selectedSlotChanged.subscribe(
        (
          slot:
            | SpEmployeeDaySlot
            | SpEmployeeHourSlot
            | SpEmployeeHalfHourSlot
            | SpEmployeeQuarterHourSlot
            | undefined
        ) => {
          if (
            slot instanceof SpEmployeeHourSlot ||
            slot instanceof SpEmployeeHalfHourSlot ||
            slot instanceof SpEmployeeQuarterHourSlot ||
            !slot
          ) {
            this.slot.set(slot);
          }
        }
      );
  }

  ngOnDestroy(): void {
    this.selectedSlotChangedSubscription?.unsubscribe();
  }
}
