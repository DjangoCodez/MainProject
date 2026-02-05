import {
  Component,
  inject,
  input,
  signal,
  OnInit,
  OnDestroy,
} from '@angular/core';
import { SpFilterService } from '../../services/sp-filter.service';
import { SpSlotService } from '../../services/sp-slot.service';
import { MinutesToTimeSpanPipe } from '@shared/pipes';
import { SpEmployeeService } from '../../services/sp-employee.service';
import {
  EmployeesAndShiftsRecalculatedEvent,
  SpEventService,
} from '../../services/sp-event.service';
import { Subscription } from 'rxjs';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { SpSettingService } from '../../services/sp-setting.service';
import { DecimalPipe } from '@angular/common';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DateUtil } from '@shared/util/date-util';
import { NumberUtil } from '@shared/util/number-util';
import {
  EmployeeMenuItemSelected,
  SpEmployeeMenuComponent,
} from '../../context-menus/sp-employee-menu/sp-employee-menu.component';
import { CdkContextMenuTrigger } from '@angular/cdk/menu';

@Component({
  selector: 'sp-time-slot-sum',
  imports: [
    CdkContextMenuTrigger,
    DecimalPipe,
    MatTooltipModule,
    MinutesToTimeSpanPipe,
    SpEmployeeMenuComponent,
    TranslatePipe,
  ],
  templateUrl: './sp-time-slot-sum.component.html',
  styleUrl: './sp-time-slot-sum.component.scss',
})
export class SpTimeSlotSumComponent implements OnInit, OnDestroy {
  hasScrollbar = input(false);

  private readonly employeeService = inject(SpEmployeeService);
  private readonly eventService = inject(SpEventService);
  readonly filterService = inject(SpFilterService);
  readonly settingService = inject(SpSettingService);
  readonly slotService = inject(SpSlotService);
  private readonly translate = inject(TranslateService);

  // TODO: Staffing needs not implemented yet
  totalNeedTime = signal(0);
  totalNetTime = signal(0);
  totalFactorTime = signal(0);
  totalWorkTime = signal(0);
  totalGrossTime = signal(0);
  totalCost = signal(0);
  totalCostIncEmpTaxAndSuppCharge = signal(0);
  totalTooltip = signal('');

  private employeesAndShiftsRecalculatedSubscription?: Subscription;

  ngOnInit(): void {
    this.employeesAndShiftsRecalculatedSubscription =
      this.eventService.employeesAndShiftsRecalculated.subscribe(
        (event: EmployeesAndShiftsRecalculatedEvent | undefined) => {
          if (event) this.updateTimesAndCosts();
        }
      );
  }

  ngOnDestroy(): void {
    this.employeesAndShiftsRecalculatedSubscription?.unsubscribe();
  }

  private updateTimesAndCosts() {
    this.totalNetTime.set(
      this.employeeService.getTotalNetTimeForVisibleEmployees()
    );
    this.totalFactorTime.set(
      this.employeeService.getTotalFactorTimeForVisibleEmployees()
    );
    this.totalWorkTime.set(
      this.employeeService.getTotalWorkTimeForVisibleEmployees()
    );
    this.totalGrossTime.set(
      this.employeeService.getTotalGrossTimeForVisibleEmployees()
    );
    this.totalCost.set(this.employeeService.getTotalCostForVisibleEmployees());
    this.totalCostIncEmpTaxAndSuppCharge.set(
      this.employeeService.getTotalCostIncEmpTaxAndSuppChargeForVisibleEmployees()
    );

    this.setTotalTooltip();
  }

  private setTotalTooltip() {
    const tooltip: string[] = [];

    // Staffing needs
    if (
      this.filterService.isCommonScheduleView() &&
      this.settingService.showFollowUpOnNeed()
    ) {
      tooltip.push(
        `${this.translate.instant('time.schedule.staffingneeds.planning.need')}: ${DateUtil.minutesToTimeSpan(this.totalNeedTime())}`
      );
    }

    // Net time
    tooltip.push(
      `${this.translate.instant('time.schedule.planning.nettime')}: ${DateUtil.minutesToTimeSpan(this.totalNetTime())}`
    );

    // Factor time
    if (
      this.settingService.showScheduleTypeFactorTime() &&
      this.totalFactorTime() !== 0
    ) {
      tooltip.push(
        `${this.translate.instant('time.schedule.planning.scheduletypefactortime')}: ${DateUtil.minutesToTimeSpan(this.totalFactorTime())}`
      );
    }

    // Work time week
    if (this.filterService.isCommonScheduleView()) {
      tooltip.push(
        `${this.translate.instant('time.schedule.planning.worktimeweek')}: ${DateUtil.minutesToTimeSpan(this.totalWorkTime())}`
      );
    }

    // Gross time
    if (this.settingService.showGrossTime()) {
      tooltip.push(
        `${this.translate.instant('time.schedule.planning.grosstime')}: ${DateUtil.minutesToTimeSpan(this.totalGrossTime())}`
      );
    }

    // Cost
    if (this.settingService.showTotalCostIncEmpTaxAndSuppCharge()) {
      tooltip.push(
        `${this.translate.instant('time.schedule.planning.cost')}: ${NumberUtil.formatDecimal(this.totalCostIncEmpTaxAndSuppCharge(), 0)}`
      );
    } else if (this.settingService.showTotalCost()) {
      tooltip.push(
        `${this.translate.instant('time.schedule.planning.cost')}: ${NumberUtil.formatDecimal(this.totalCost(), 0)}`
      );
    }

    this.totalTooltip.set(tooltip.join('\n'));
  }

  // CONTEXT MENU EVENTS

  onEmployeeMenuSelected(event: EmployeeMenuItemSelected) {
    switch (event.option) {
      default:
        console.log('onEmployeeMenuSelected', event);
        break;
    }
  }
}
