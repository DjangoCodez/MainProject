import {
  Component,
  OnDestroy,
  OnInit,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { SpEmployeeService } from '../../services/sp-employee.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { SpShiftService } from '../../services/sp-shift.service';
import { SpSlotService } from '../../services/sp-slot.service';
import { Subscription } from 'rxjs';
import {
  EmployeesAndShiftsRecalculatedEvent,
  SpEventService,
} from '../../services/sp-event.service';
import { SpFilterService } from '../../services/sp-filter.service';
import { SpDaySlot } from '../../models/time-slot.model';
import { TermGroup_TimeSchedulePlanningViews } from '@shared/models/generated-interfaces/Enumerations';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DatePipe } from '@angular/common';
import { SpSettingService } from '../../services/sp-setting.service';
import { CdkContextMenuTrigger } from '@angular/cdk/menu';
import {
  EmployeeMenuItemSelected,
  SpEmployeeMenuComponent,
} from '../../context-menus/sp-employee-menu/sp-employee-menu.component';

@Component({
  selector: 'sp-time-slot-header',
  imports: [
    CdkContextMenuTrigger,
    MatTooltipModule,
    SpEmployeeMenuComponent,
    TranslatePipe,
    DatePipe,
  ],
  templateUrl: './sp-time-slot-header.component.html',
  styleUrl: './sp-time-slot-header.component.scss',
})
export class SpTimeSlotHeaderComponent implements OnInit, OnDestroy {
  hasScrollbar = input(false);

  readonly employeeService = inject(SpEmployeeService);
  private readonly eventService = inject(SpEventService);
  readonly filterService = inject(SpFilterService);
  readonly settingService = inject(SpSettingService);
  readonly shiftService = inject(SpShiftService);
  readonly slotService = inject(SpSlotService);
  private readonly translate = inject(TranslateService);

  // Number of shifts currently loaded, calculates shifts on all employees
  nbrOfEmployees = signal(0);
  nbrOfVisibleEmployees = signal(0);
  nbrOfShifts = signal(0);
  nbrOfFilteredShifts = signal(0);

  nbrOfEmployeesInfo = computed(() => {
    if (this.nbrOfVisibleEmployees() === this.nbrOfEmployees())
      return `${this.nbrOfVisibleEmployees()} ${this.employeesLabel}`;
    else
      return `${this.nbrOfVisibleEmployees()} (${this.nbrOfEmployees()}) ${this.employeesLabel}`;
  });

  nbrOfShiftsInfo = computed(() => {
    if (this.nbrOfFilteredShifts() === this.nbrOfShifts())
      return `${this.nbrOfFilteredShifts()} ${this.shiftsLabel}`;
    else
      return `${this.nbrOfFilteredShifts()} (${this.nbrOfShifts()}) ${this.shiftsLabel}`;
  });

  private settingsLoadedSubscription?: Subscription;
  private employeesAndShiftsRecalculatedSubscription?: Subscription;

  settingsLoaded = signal(false);

  private employeesLabel = '';
  private shiftsLabel = '';

  ngOnInit(): void {
    this.settingsLoadedSubscription =
      this.settingService.settingsLoaded.subscribe(value => {
        // When BehaviorSubject is initialized, this will be called with the value = false, ignore that.
        // We only want to react when the settings are actually loaded.
        if (value) this.onSettingsLoaded(value);
      });

    this.employeesAndShiftsRecalculatedSubscription =
      this.eventService.employeesAndShiftsRecalculated.subscribe(
        (event: EmployeesAndShiftsRecalculatedEvent | undefined) => {
          if (event) this.onEmployeesAndShiftsRecalculated(event);
        }
      );

    this.loadTerms();
  }

  ngOnDestroy(): void {
    this.settingsLoadedSubscription?.unsubscribe();
    this.employeesAndShiftsRecalculatedSubscription?.unsubscribe();
  }

  private loadTerms() {
    this.translate
      .get(['common.employees', 'time.schedule.planning.shifts'])
      .subscribe(terms => {
        this.employeesLabel = terms['common.employees'].toLocaleLowerCase();
        this.shiftsLabel =
          terms['time.schedule.planning.shifts'].toLocaleLowerCase();
      });
  }

  // EVENTS

  private onSettingsLoaded(value: boolean) {
    if (value) {
      this.settingsLoaded.set(true);
    }
  }

  private onEmployeesAndShiftsRecalculated(
    event: EmployeesAndShiftsRecalculatedEvent
  ) {
    this.nbrOfEmployees.set(event.allEmployees);
    this.nbrOfVisibleEmployees.set(event.visibleEmployees);
    this.nbrOfShifts.set(event.allShifts);
    this.nbrOfFilteredShifts.set(event.visibleShifts);
  }

  onDaySlotClicked(daySlot: SpDaySlot) {
    if (this.filterService.isDayView()) {
      this.filterService.setViewDefinition(
        TermGroup_TimeSchedulePlanningViews.Schedule,
        daySlot.start
      );
    } else if (this.filterService.isScheduleView()) {
      this.filterService.setViewDefinition(
        TermGroup_TimeSchedulePlanningViews.Day,
        daySlot.start
      );
    }
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
