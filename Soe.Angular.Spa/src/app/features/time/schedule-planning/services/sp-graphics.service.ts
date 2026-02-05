import { inject, Injectable, signal } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { SpFilterService } from './sp-filter.service';
import { SpSettingService } from './sp-setting.service';
import { SpSlotService } from './sp-slot.service';
import { DateUtil } from '@shared/util/date-util';
import { TermGroup_StaffingNeedsHeadInterval } from '@shared/models/generated-interfaces/Enumerations';

@Injectable({
  providedIn: 'root',
})
export class SpGraphicsService {
  private readonly filterService = inject(SpFilterService);
  private readonly settingService = inject(SpSettingService);
  private readonly slotService = inject(SpSlotService);

  contentWidth = 0; // Width of the SpContentComponent
  offsetLeft = 200; // With of the first column (employee)
  pixelsPerMinute = new BehaviorSubject<number>(0);
  pixelsPerDay = new BehaviorSubject<number>(0);

  constructor() {}

  setContentWidth(
    width: number,
    forceCalculatePixelsPerTimeUnit = false
  ): void {
    const newWidth = width - this.offsetLeft;
    if (newWidth !== this.contentWidth) {
      this.contentWidth = newWidth;
      forceCalculatePixelsPerTimeUnit = true;
    }

    if (forceCalculatePixelsPerTimeUnit) this.calculatePixelsPerTimeUnit();
  }

  calculatePixelsPerTimeUnit(): void {
    if (this.filterService.isCommonDayView()) {
      this.pixelsPerMinute.next(
        this.contentWidth / this.filterService.nbrOfHours() / 60
      );
      this.pixelsPerDay.next(0);
    } else if (this.filterService.isCommonScheduleView()) {
      this.pixelsPerDay.next(
        this.contentWidth / this.filterService.nbrOfDays()
      );
      this.pixelsPerMinute.next(0);
    }
  }

  getPixelsForTime(time: Date): number {
    let pixels: number = 0;

    let dayViewStartTime: Date;
    switch (this.settingService.dayViewMinorTickLength()) {
      case TermGroup_StaffingNeedsHeadInterval.SixtyMinutes:
        dayViewStartTime = this.slotService.hourSlots()[0].start;
        break;
      case TermGroup_StaffingNeedsHeadInterval.ThirtyMinutes:
        dayViewStartTime = this.slotService.halfHourSlots()[0].start;
        break;
      case TermGroup_StaffingNeedsHeadInterval.FifteenMinutes:
        dayViewStartTime = this.slotService.quarterHourSlots()[0].start;
        break;
    }

    // Get number of minutes after visible start
    const minutesFromVisibleStart = this.getDifferenceInMinutes(
      dayViewStartTime,
      time
    );
    pixels = minutesFromVisibleStart * this.pixelsPerMinute.value;

    // Make sure pixels are inside visible range
    if (pixels < 0) pixels = 0;
    else if (pixels > this.contentWidth) pixels = this.contentWidth;

    return pixels;
  }

  private getDifferenceInMinutes(startDate: Date, stopDate: Date) {
    // Get shift actual stop time or end of display if shift goes beyond that
    let actualStop = DateUtil.getMinDate(
      stopDate,
      this.filterService.dateTo().addSeconds(1)
    );
    // Get shift actual stop time or beginning of display if shift goes beyond that
    actualStop = DateUtil.getMaxDate(actualStop, this.filterService.dateFrom());

    return actualStop.diffMinutes(startDate);
  }
}
