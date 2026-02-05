import {
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit,
  computed,
  inject,
  input,
  model,
  signal,
} from '@angular/core';
import { PlanningShiftDTO } from '../../models/shift.model';
import { CommonModule, DatePipe } from '@angular/common';
import { SpShiftService } from '../../services/sp-shift.service';
import { SpSettingService } from '../../services/sp-setting.service';
import {
  ShiftMenuComponent,
  ShiftMenuItemSelected,
} from '../../context-menus/sp-shift-menu/sp-shift-menu.component';
import { CdkContextMenuTrigger } from '@angular/cdk/menu';
import { SpEventService } from '../../services/sp-event.service';
import { CdkDrag, CdkDragPlaceholder } from '@angular/cdk/drag-drop';
import { Subscription } from 'rxjs';
import { IconModule } from '@ui/icon/icon.module';
import { SpFilterService } from '../../services/sp-filter.service';
import { SpGraphicsService } from '../../services/sp-graphics.service';
import { TranslatePipe } from '@ngx-translate/core';
import { SpEmployeeDaySlot } from '../../models/time-slot.model';
import { MatTooltip, MatTooltipModule } from '@angular/material/tooltip';
import { SpShiftDragService } from './sp-shift-drag.service';
import { MinutesToTimeSpanPipe } from '@shared/pipes';
import { ShiftUtil } from '../../util/shift-util';

@Component({
  selector: 'sp-shift',
  imports: [
    CdkContextMenuTrigger,
    CdkDrag,
    CdkDragPlaceholder,
    CommonModule,
    DatePipe,
    IconModule,
    MatTooltipModule,
    MinutesToTimeSpanPipe,
    ShiftMenuComponent,
    TranslatePipe,
  ],
  templateUrl: './sp-shift.component.html',
  styleUrls: [
    '../schedule-planning/schedule-planning-shift-common.scss',
    './sp-shift.component.scss',
  ],
})
export class SpShiftComponent implements OnInit, OnDestroy {
  shift = model.required<PlanningShiftDTO>();
  daySlot = input.required<SpEmployeeDaySlot>();

  readonly dragService = inject(SpShiftDragService);
  private readonly eventService = inject(SpEventService);
  readonly filterService = inject(SpFilterService);
  private readonly graphicsService = inject(SpGraphicsService);
  readonly settingService = inject(SpSettingService);
  readonly shiftService = inject(SpShiftService);

  private pixelsPerMinuteSubscription?: Subscription;
  private selectedShiftsChangedSubscription?: Subscription;

  shiftBreaksLength = signal(0);
  showLeftBar = computed(() => {
    return (
      this.shift().isAbsence ||
      this.shift().isAbsenceRequest ||
      this.shift().isOnDuty ||
      this.shift().isStandby ||
      this.shift().nbrOfWantedInQueue > 0
    );
  });

  scheduleTypeText: string = '';
  tooltipText: string = '';

  constructor(private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.pixelsPerMinuteSubscription =
      this.graphicsService.pixelsPerMinute.subscribe((pixels: number) => {
        if (this.filterService.isCommonDayView()) {
          this.shiftService.setShiftPositionAndWidthForDayView(this.shift());
        } else if (this.filterService.isCommonScheduleView()) {
          this.shiftService.setShiftPositionAndWidthForScheduleView(
            this.shift()
          );
        }
      });

    this.selectedShiftsChangedSubscription =
      this.shiftService.selectedShiftsChanged.subscribe(
        (shifts: PlanningShiftDTO[]) => {
          const isSelected = this.shift().isSelected;
          const shouldBeSelected = shifts
            .map(s => s.timeScheduleTemplateBlockId)
            .includes(this.shift().timeScheduleTemplateBlockId);

          if (
            (!isSelected && shouldBeSelected) ||
            (isSelected && !shouldBeSelected)
          ) {
            // Need to create a clone with correct type,
            // otherwise the object will not be of type PlanningShiftDTO.
            const clone = new PlanningShiftDTO();
            Object.assign(clone, this.shift());
            clone.isSelected = shouldBeSelected;
            this.shift.update(s => clone);
          }
        }
      );

    this.shiftBreaksLength.set(
      ShiftUtil.shiftBreaksLength(this.shift(), this.daySlot().shifts)
    );

    this.scheduleTypeText = this.shift().getTimeScheduleTypeCodes(
      this.settingService.useMultipleScheduleTypes()
    );
  }

  ngOnDestroy(): void {
    this.pixelsPerMinuteSubscription?.unsubscribe();
    this.selectedShiftsChangedSubscription?.unsubscribe();
  }

  // EVENTS

  onShiftMouseEnter(event: MouseEvent, tooltip: MatTooltip): void {
    // Shift mouse enter
    this.tooltipText = this.shiftService.createShiftTooltip(this.shift());
    this.cdr.detectChanges();
    tooltip.show();
  }

  onShiftClick(event: MouseEvent): void {
    // Shift click
    event.stopPropagation();
    if (event.detail === 1) {
      // Select shift
      this.shiftService.shiftSelected(
        this.shift(),
        event.ctrlKey,
        event.shiftKey
      );
    }
  }

  onShiftDoubleClick(event: MouseEvent): void {
    // Shift double click
    event.stopPropagation();
    if (event.detail === 2) {
      // Edit shift
      this.eventService.editShift(this.shift());
    }
  }

  onShiftRightClick(event: MouseEvent): void {
    // Shift right click
    // If current shift is not selected, select it before opening the context menu
    if (!this.shiftService.isShiftSelected(this.shift())) {
      this.shiftService.shiftSelected(this.shift(), false, false);
    }
  }

  onMenuSelected(event: ShiftMenuItemSelected) {
    // Right click menu selected
    // Options handled in the context menu component
    switch (event.option) {
      default:
        console.log('ShiftMenuOption', event.option);
        break;
    }
  }

  onDragStarted(): void {
    // If current shift is not selected, select it before starting the drag
    // That makes sure that all linked shifts are dragged
    if (!this.shiftService.isShiftSelected(this.shift())) {
      this.shiftService.shiftSelected(this.shift(), false, false);
    }

    this.dragService.onDragStarted();
  }
}
