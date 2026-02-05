import {
  Component,
  OnInit,
  inject,
  input,
  model,
  output,
  signal,
} from '@angular/core';
import { PlanningShiftDTO } from '../../models/shift.model';
import { CommonModule, DatePipe } from '@angular/common';
import { SpSettingService } from '../../services/sp-setting.service';
import { IconModule } from '@ui/icon/icon.module';
import { TranslatePipe } from '@ngx-translate/core';
import { MinutesToTimeSpanPipe } from '@shared/pipes';
import { ShiftUtil } from '../../util/shift-util';

@Component({
  selector: 'sp-shift-simple',
  imports: [
    CommonModule,
    DatePipe,
    IconModule,
    MinutesToTimeSpanPipe,
    TranslatePipe,
  ],
  templateUrl: './sp-shift-simple.component.html',
  styleUrls: [
    '../schedule-planning/schedule-planning-shift-common.scss',
    './sp-shift-simple.component.scss',
  ],
})
export class SpShiftSimpleComponent implements OnInit {
  shift = model.required<PlanningShiftDTO>();
  dayShifts = input<PlanningShiftDTO[]>([]);
  showDate = input(false);
  isClickable = input(false);

  onClick = output<PlanningShiftDTO>();

  private readonly settingService = inject(SpSettingService);

  shiftBreaksLength = signal<number>(0);
  scheduleTypeText = signal('');

  constructor() {}

  ngOnInit(): void {
    this.shiftBreaksLength.set(
      ShiftUtil.shiftBreaksLength(this.shift(), this.dayShifts())
    );

    this.scheduleTypeText.set(
      this.shift().getTimeScheduleTypeCodes(
        this.settingService.useMultipleScheduleTypes()
      )
    );
  }

  onShiftClick(event: MouseEvent): void {
    // Shift click
    event.stopPropagation();
    this.onClick.emit(this.shift());
  }
}
