import {
  Component,
  computed,
  inject,
  input,
  OnDestroy,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { IconModule } from '@ui/icon/icon.module';
import { PlanningShiftDTO } from '../../models/shift.model';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CdkMenu, CdkMenuItem } from '@angular/cdk/menu';
import { SpEventService } from '../../services/sp-event.service';
import { DateUtil } from '@shared/util/date-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { SpEmployeeService } from '../../services/sp-employee.service';
import { SpTranslateService } from '../../services/sp-translate.service';
import { SpShiftService } from '../../services/sp-shift.service';
import { Subscription } from 'rxjs';

export enum ShiftMenuOption {
  NewShift,
  EditShift,
  DeleteShift,
  SplitShift,
  ShiftRequest,
  ShiftAbsence,
  Debug,
}

export type ShiftMenuItemSelected = {
  shift?: PlanningShiftDTO;
  date?: Date;
  employeeId?: number;
  option: ShiftMenuOption;
};

@Component({
  selector: 'sp-shift-menu',
  imports: [CdkMenu, CdkMenuItem, IconModule, TranslateModule],
  templateUrl: './sp-shift-menu.component.html',
  styleUrls: [
    '../../../../../shared/styles/shared-styles/shared-context-menu-styles.scss',
    './sp-shift-menu.component.scss',
  ],
})
export class ShiftMenuComponent implements OnInit, OnDestroy {
  shift = input<PlanningShiftDTO | undefined>(undefined);
  date = input<Date | undefined>(undefined);
  employeeId = input<number | undefined>(undefined);

  menuSelected = output<ShiftMenuItemSelected>();

  private readonly employeeService = inject(SpEmployeeService);
  private readonly eventService = inject(SpEventService);
  readonly shiftService = inject(SpShiftService);
  private readonly spTranslateService = inject(SpTranslateService);
  private readonly translateService = inject(TranslateService);

  readonly SoeConfigUtil = SoeConfigUtil;
  readonly ShiftMenuOption = ShiftMenuOption;

  // Terms
  splitShiftLabel = '';

  private selectedShiftsChangedSubscription?: Subscription;

  selectedShiftsIsSameDay = signal(false);
  selectedShiftsIsLinked = signal(false);
  selectedShiftsIsInFuture = signal(false);

  showShiftSplitOption = computed(() => {
    return (
      this.shift() &&
      !this.shift()?.isReadOnly &&
      this.shiftService.nbrOfSelectedShifts() === 1
    );
  });

  showShiftRequestOption = computed(() => {
    return (
      this.shift() &&
      !this.shift()?.isAbsence &&
      !this.shift()?.isAbsenceRequest &&
      !this.shift()?.isBooking
    );
  });
  disableShiftRequestOption = computed(() => {
    return (
      (this.shiftService.nbrOfSelectedShifts() > 1 &&
        !this.selectedShiftsIsLinked()) ||
      !this.selectedShiftsIsInFuture()
    );
  });

  showShiftAbsenceOption = computed(() => {
    return this.shift() && !this.shift()?.isReadOnly;
  });
  disableShiftAbsenceOption = computed(() => {
    return !this.selectedShiftsIsSameDay();
  });

  ngOnInit(): void {
    this.selectedShiftsChangedSubscription =
      this.shiftService.selectedShiftsChanged.subscribe(
        (shifts: PlanningShiftDTO[]) => {
          if (shifts.length > 0) {
            const firstShiftDate = shifts[0].actualStartDate;
            this.selectedShiftsIsSameDay.set(
              shifts.every(shift =>
                shift.actualStartDate.isSameDay(firstShiftDate)
              )
            );

            const firstLink = shifts[0].link;
            this.selectedShiftsIsLinked.set(
              shifts.every(shift => shift.link === firstLink)
            );

            const earliestStopTime = shifts
              .map(s => s.actualStopTime)
              .reduce((min, cur) =>
                cur.getTime() < min.getTime() ? cur : min
              );
            this.selectedShiftsIsInFuture.set(
              earliestStopTime.isSameOrAfterOnMinute(new Date())
            );
          } else {
            this.selectedShiftsIsSameDay.set(false);
            this.selectedShiftsIsLinked.set(false);
          }
        }
      );

    this.setupTerms();
  }

  ngOnDestroy(): void {
    this.selectedShiftsChangedSubscription?.unsubscribe();
  }

  private setupTerms(): void {
    this.translateService
      .get(['time.schedule.planning.contextmenu.splitshift'])
      .subscribe(terms => {
        this.splitShiftLabel = terms[
          'time.schedule.planning.contextmenu.splitshift'
        ].format(this.spTranslateService.shiftUndefined());
      });
  }

  onMenuSelected(option: ShiftMenuOption) {
    const date =
      this.date() ?? this.shift()?.actualStartDate ?? DateUtil.getToday();
    const employeeId = this.employeeId() ?? this.shift()?.employeeId ?? 0;

    switch (option) {
      case ShiftMenuOption.NewShift:
        this.eventService.addShift(
          date,
          employeeId,
          this.shift()?.timeScheduleTemplateBlockId
        );
        break;
      case ShiftMenuOption.EditShift:
        if (this.shift()) this.eventService.editShift(this.shift()!);
        break;
      case ShiftMenuOption.DeleteShift:
        const employee = this.employeeService.getEmployee(employeeId);
        const selectedShifts = this.shiftService.selectedShiftsChanged.value;
        if (employee && selectedShifts) {
          // Also send intersecting on duty shifts to the dialog
          const onDutyShifts: PlanningShiftDTO[] = [];
          selectedShifts.forEach(shift => {
            const ods = this.shiftService.getIntersectingOnDutyShifts(shift);
            ods.forEach(sh => {
              if (
                !onDutyShifts.find(
                  s =>
                    s.timeScheduleTemplateBlockId ===
                    sh.timeScheduleTemplateBlockId
                )
              ) {
                onDutyShifts.push(sh);
              }
            });
          });
          this.eventService.deleteShifts(
            employee,
            selectedShifts,
            onDutyShifts
          );
        }
        break;
      case ShiftMenuOption.SplitShift:
        if (this.shift())
          this.eventService.splitShift(this.splitShiftLabel, this.shift()!);
        break;
      case ShiftMenuOption.ShiftRequest:
        const employeeForRequest = this.employeeService.getEmployee(employeeId);
        const shiftsForRequest = this.shiftService.selectedShiftsChanged.value;
        if (employeeForRequest && shiftsForRequest) {
          this.eventService.shiftRequest(
            employeeForRequest,
            shiftsForRequest[0]
          );
        }
        break;
      case ShiftMenuOption.ShiftAbsence:
        const employeeForAbsence = this.employeeService.getEmployee(employeeId);
        const shiftsForAbsence = this.shiftService.selectedShiftsChanged.value;
        if (employeeForAbsence && shiftsForAbsence) {
          this.eventService.shiftAbsence(employeeForAbsence, shiftsForAbsence);
        }
        break;
      case ShiftMenuOption.Debug:
        console.log('Shift:', this.shift());
        break;
      default:
        this.menuSelected.emit({
          shift: this.shift(),
          date: this.date(),
          employeeId: this.employeeId(),
          option: option,
        });
        break;
    }
  }
}
