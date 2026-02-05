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
import {
  TermGroup_TimeSchedulePlanningDayViewSortBy,
  TermGroup_TimeSchedulePlanningScheduleViewSortBy,
  TermGroup_TimeSchedulePlanningVisibleDays,
} from '@shared/models/generated-interfaces/Enumerations';
import { DateRangeValue } from '@ui/forms/datepicker/daterangepicker/daterangepicker.component';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ToolbarDatepickerAction } from '@ui/toolbar/toolbar-datepicker/toolbar-datepicker.component';
import { ToolbarDaterangepickerAction } from '@ui/toolbar/toolbar-daterangepicker/toolbar-daterangepicker.component';
import { ToolbarMenuButtonAction } from '@ui/toolbar/toolbar-menu-button/toolbar-menu-button.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { debounce } from 'lodash';
import { Subscription } from 'rxjs';
import { SpFilterDialogResult } from '../../dialogs/sp-filter-dialog/sp-filter-dialog.component';
import { SpSettingDialogResult } from '../../dialogs/sp-setting-dialog/sp-setting-dialog.component';
import { SpDialogService } from '../../services/sp-dialog.service';
import { SpEmployeeService } from '../../services/sp-employee.service';
import {
  DateRangeChangedValue,
  NbrOfDaysChangedValue,
  SpFilterService,
  ViewDefinitionChangedValue,
} from '../../services/sp-filter.service';
import { SpSettingService } from '../../services/sp-setting.service';
import { ToolbarCheckboxAction } from '@ui/toolbar/toolbar-checkbox/toolbar-checkbox.component';
import { ToolbarSelectAction } from '@ui/toolbar/toolbar-select/toolbar-select.component';
import { DateUtil } from '@shared/util/date-util';
import { CheckboxBehaviour } from '@ui/forms/checkbox/checkbox.component';

export enum SpFunctions {
  ShowRightContent,
}

@Component({
  selector: 'sp-header',
  imports: [ToolbarComponent],
  templateUrl: './sp-header.component.html',
  styleUrl: './sp-header.component.scss',
  providers: [ToolbarService],
})
export class SpHeaderComponent implements OnInit, OnDestroy {
  hasScrollbar = input(false);

  filterChanged = output();
  loadEmployeesClicked = output();
  loadShiftsClicked = output();
  settingsChanged = output<SpSettingDialogResult>();

  private readonly dialogService = inject(SpDialogService);
  private readonly employeeService = inject(SpEmployeeService);
  private readonly filterService = inject(SpFilterService);
  readonly settingService = inject(SpSettingService);
  readonly toolbarService = inject(ToolbarService);

  private readonly debounceTimeoutMs = 250;

  // Toolbar signals
  private toolbarDateInitialDate = signal<Date>(this.filterService.dateFrom());
  private toolbarDaterangeInitialDates = signal<DateRangeValue>([
    this.filterService.dateFrom(),
    this.filterService.dateTo(),
  ]);
  private toolbarNbrOfDaysInitialSelectedId = signal(
    this.filterService.nbrOfDays()
  );
  private toolbarFilterIconClass = signal('');
  private toolbarFilterIconPrefix = signal('fal');
  private toolbarSortEmployeesList = signal<MenuButtonItem[]>([]);
  private toolbarSortEmployeesInitialSelectedItemId = signal(
    this.filterService.sortEmployeesBy()
  );

  private viewDefinitionChangedSubscription?: Subscription;
  private dayViewDefaultSortBySubscription?: Subscription;
  private scheduleViewDefaultSortBySubscription?: Subscription;
  private dateRangeChangedSubscription?: Subscription;
  private nbrOfDaysSelectorSubscription?: Subscription;
  private isFilteredChangedSubscription?: Subscription;

  ngOnInit() {
    // Toolbar
    this.setupToolbar();

    // Subscriptions
    this.viewDefinitionChangedSubscription =
      this.filterService.viewDefinitionChanged.subscribe(
        (value: ViewDefinitionChangedValue | undefined) => {
          if (value) this.onViewDefinitionChanged(value);
        }
      );

    this.dateRangeChangedSubscription =
      this.filterService.dateRangeChanged.subscribe(
        (value: DateRangeChangedValue | undefined) => {
          if (value) this.onDateRangeChangedFromFilter(value);
        }
      );

    this.dayViewDefaultSortBySubscription =
      this.settingService.dayViewDefaultSortByChanged.subscribe(
        (value: TermGroup_TimeSchedulePlanningDayViewSortBy) => {
          this.onEmployeeSortByChanged(value);
        }
      );

    this.scheduleViewDefaultSortBySubscription =
      this.settingService.scheduleViewDefaultSortByChanged.subscribe(
        (value: TermGroup_TimeSchedulePlanningScheduleViewSortBy) => {
          this.onEmployeeSortByChanged(value);
        }
      );

    this.nbrOfDaysSelectorSubscription =
      this.filterService.nbrOfDaysChanged.subscribe(
        (value: NbrOfDaysChangedValue | undefined) => {
          if (value) this.onNbrOfDaysChanged(value);
        }
      );

    this.isFilteredChangedSubscription =
      this.filterService.isFilteredChanged.subscribe((value: boolean) => {
        this.onFilterChanged(value);
      });

    // Lookups
    this.filterService.loadVisibleDays().subscribe();
  }

  ngOnDestroy(): void {
    this.viewDefinitionChangedSubscription?.unsubscribe();
    this.dayViewDefaultSortBySubscription?.unsubscribe();
    this.scheduleViewDefaultSortBySubscription?.unsubscribe();
    this.dateRangeChangedSubscription?.unsubscribe();
    this.nbrOfDaysSelectorSubscription?.unsubscribe();
    this.isFilteredChangedSubscription?.unsubscribe();
  }

  private setupToolbar(): void {
    // Date (day view)
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarDatepicker('date', {
          initialDate: this.filterService.dateFrom,
          showArrows: signal(true),
          hidden: this.filterService.isCommonScheduleView,
          onValueChanged: debounce(event => {
            this.onDateChanged((event as ToolbarDatepickerAction)?.value);
          }, this.debounceTimeoutMs),
        }),
      ],
    });

    // Date range (schedule view)
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarDaterangepicker('dateRange', {
          initialDates: this.toolbarDaterangeInitialDates,
          deltaDays: computed(() => {
            // When month is selected, calculate number of days in month,
            // since it's not always the same as nbrOfDays (31).
            if (this.filterService.isMonthSelected()) {
              return DateUtil.diffDays(
                this.filterService.dateTo(),
                this.filterService.dateFrom()
              );
            } else {
              return this.filterService.nbrOfDays() - 1;
            }
          }),
          offsetDaysOnStep: computed(() => {
            return this.filterService.isWorkWeekSelected() ? 2 : 0;
          }),
          showArrows: signal(true),
          separatorDash: signal(true),
          hidden: this.filterService.isCommonDayView,
          onValueChanged: debounce(event => {
            this.onDateRangeChanged(
              (event as ToolbarDaterangepickerAction)?.value
            );
          }, this.debounceTimeoutMs),
        }),

        this.toolbarService.createToolbarSelect('nbrOfDays', {
          initialSelectedId: this.toolbarNbrOfDaysInitialSelectedId,
          items: this.filterService.visibleDays,
          disabled: computed(
            () => this.filterService.visibleDays().length === 0
          ),
          hidden: this.filterService.isCommonDayView,
          onValueChanged: event =>
            this.filterService.setSelectedNbrOfDays(
              (event as ToolbarSelectAction).value
            ),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        // Filter
        this.toolbarService.createToolbarButton('filter', {
          iconName: signal('filter'),
          iconClass: this.toolbarFilterIconClass,
          iconPrefix: this.toolbarFilterIconPrefix,
          tooltip: signal('core.filter'),
          onAction: () => this.onFilterClick(),
        }),
        // Sort employees
        this.toolbarService.createToolbarMenuButton('sortEmployees', {
          iconName: signal('arrow-down-1-9'),
          tooltip: signal('core.sortingon'),
          list: this.toolbarSortEmployeesList,
          hideDropdownArrow: signal(true),
          showSelectedItemIcon: signal(true),
          initialSelectedItemId: this.toolbarSortEmployeesInitialSelectedItemId,
          onItemSelected: event => this.onSortEmployeesClick(event),
        }),
      ],
    });

    // Load data
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarButton('loadShifts', {
          iconName: signal('sync'),
          tooltip: signal('time.schedule.planning.refreshdate'),
          onAction: () => this.loadShiftsClicked.emit(),
        }),
      ],
    });

    // TEST
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarCheckbox('showTimes', {
          labelKey: signal('Visa klockslag'),
          checkboxBehaviour: signal<CheckboxBehaviour>('switch'),
          checked: this.settingService.showTimesOnShift,
          onValueChanged: event =>
            this.settingService.showTimesOnShift.set(
              (event as ToolbarCheckboxAction).value
            ),
        }),
      ],
    });

    /*
    Separator between left and right aligned items.
    Remember that right aligned items start with the rightmost item and goes left.
    */

    // Settings
    this.toolbarService.createItemGroup({
      alignLeft: false,
      items: [
        this.toolbarService.createToolbarButton('settings', {
          iconName: signal('gear'),
          tooltip: signal('common.settings'),
          onAction: () => this.onSettingsClick(),
        }),
      ],
    });

    // Functions
    this.toolbarService.createItemGroup({
      alignLeft: false,
      items: [
        this.toolbarService.createToolbarMenuButton('functions', {
          caption: signal('core.functions'),
          tooltip: signal('core.functions'),
          list: signal([
            { type: 'header', label: 'Högerpanel' },
            {
              id: SpFunctions.ShowRightContent,
              label: 'Visa detaljer',
              icon: ['fal', 'square-info'],
            },
          ]),
          unselectItemAfterSelect: signal(true),
          onItemSelected: event => this.onFunctionsClick(event),
        }),
      ],
    });

    // Skip work rules
    this.toolbarService.createItemGroup({
      alignLeft: false,
      items: [
        this.toolbarService.createToolbarLabel('workRulesSkipped', {
          labelKey: signal('time.schedule.planning.workrulesskipped'),
          labelClass: signal('warning-color'),
          hidden: computed(() => {
            return !this.settingService.skipWorkRules();
          }),
        }),
      ],
    });
  }

  getSortEmployeesToolbarList(): MenuButtonItem[] {
    if (this.filterService.isCommonDayView()) {
      return [
        { type: 'header', label: 'core.sortingon' },
        {
          id: TermGroup_TimeSchedulePlanningDayViewSortBy.EmployeeNr,
          label: 'time.employee.employeenumber',
          icon: ['fal', 'sort-numeric-down'],
        },
        {
          id: TermGroup_TimeSchedulePlanningDayViewSortBy.Firstname,
          label: 'common.firstname',
          icon: ['fal', 'sort-alpha-down'],
        },
        {
          id: TermGroup_TimeSchedulePlanningDayViewSortBy.Lastname,
          label: 'common.lastname',
          icon: ['fal', 'sort-alpha-down'],
        },
        {
          id: TermGroup_TimeSchedulePlanningDayViewSortBy.StartTime,
          label: 'time.schedule.planning.starttime',
          icon: ['fal', 'chart-gantt'],
        },
      ];
    } else if (this.filterService.isCommonScheduleView()) {
      return [
        { type: 'header', label: 'core.sortingon' },
        {
          id: TermGroup_TimeSchedulePlanningScheduleViewSortBy.EmployeeNr,
          label: 'time.employee.employeenumber',
          icon: ['fal', 'sort-numeric-down'],
        },
        {
          id: TermGroup_TimeSchedulePlanningScheduleViewSortBy.Firstname,
          label: 'common.firstname',
          icon: ['fal', 'sort-alpha-down'],
        },
        {
          id: TermGroup_TimeSchedulePlanningScheduleViewSortBy.Lastname,
          label: 'common.lastname',
          icon: ['fal', 'sort-alpha-down'],
        },
      ];
    }

    return [];
  }

  // EVENTS

  private onViewDefinitionChanged(value: ViewDefinitionChangedValue) {
    // Change some toolbar items based on view definition
    this.toolbarSortEmployeesList.set(this.getSortEmployeesToolbarList());
  }

  private onDateRangeChangedFromFilter(value: DateRangeChangedValue) {
    if (this.filterService.isCommonDayView()) {
      // Date changed from code, update date picker
      this.toolbarDateInitialDate.set(value.from);
    } else if (this.filterService.isCommonScheduleView()) {
      // Date range changed from code, update date range picker
      this.toolbarDaterangeInitialDates.set([value.from, value.to]);
    }
  }

  private onDateChanged(value: Date | undefined) {
    if (value)
      this.filterService.setDateRange('onDateChanged', value, value, true);
  }

  private onDateRangeChanged(value: DateRangeValue | undefined) {
    if (value && value[0] && value[1])
      this.filterService.setDateRange(
        'onDateRangeChanged',
        value[0],
        value[1],
        true
      );
  }

  private onNbrOfDaysChanged(value: NbrOfDaysChangedValue) {
    // Set selected item in toolbar
    this.toolbarNbrOfDaysInitialSelectedId.set(
      value.isCustom
        ? TermGroup_TimeSchedulePlanningVisibleDays.Custom
        : value.to
    );
  }

  private onEmployeeSortByChanged(
    value:
      | TermGroup_TimeSchedulePlanningDayViewSortBy
      | TermGroup_TimeSchedulePlanningScheduleViewSortBy
  ) {
    // Set selected item in toolbar and sort employees
    this.toolbarSortEmployeesInitialSelectedItemId.set(value);
    this.employeeService.sortEmployees();
  }

  private onFilterChanged(value?: boolean) {
    // Set color on filter button if filtered
    this.toolbarFilterIconClass.set(value ? 'icon-secondary' : '');
    this.toolbarFilterIconPrefix.set(value ? 'fas' : 'fal');
  }

  private onFilterClick() {
    this.dialogService
      .openFilterDialog()
      .subscribe((result: SpFilterDialogResult) => {
        if (result?.filterChanged) {
          this.filterChanged.emit();
        }
      });
  }

  private onSortEmployeesClick(event: ToolbarMenuButtonAction) {
    if (event?.value) {
      if (this.filterService.isCommonDayView()) {
        this.filterService.sortEmployeesByForDayView.set(
          <TermGroup_TimeSchedulePlanningDayViewSortBy>event.value.id
        );
      } else if (this.filterService.isCommonScheduleView()) {
        this.filterService.sortEmployeesByForScheduleView.set(
          <TermGroup_TimeSchedulePlanningScheduleViewSortBy>event.value.id
        );
      }
      this.employeeService.sortEmployees();
    }
  }

  private onFunctionsClick(event: ToolbarMenuButtonAction) {
    if (event?.value) {
      switch (event.value.id) {
        case SpFunctions.ShowRightContent:
          // Toggle right content panel
          this.settingService.showRightContent.set(
            !this.settingService.showRightContent()
          );
          break;
      }
    }
  }

  private onSettingsClick() {
    this.dialogService
      .openSettingDialog()
      .subscribe((result: SpSettingDialogResult) => {
        this.settingsChanged.emit(result);
      });
  }
}
