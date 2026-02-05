import {
  Component,
  EventEmitter,
  inject,
  Input,
  Output,
  signal,
  OnInit,
} from '@angular/core';
import { ProjectTimeReportGridHeaderForm } from '@features/billing/project-time-report/models/project-time-report-grid-header-form.model';
import {
  IEmployeeTimeCodeDTO,
  ProjectTimeReportGridHeaderDTO,
} from '@features/billing/project-time-report/models/project-time-report.model';
import { ProjectTimeReportService } from '@features/billing/project-time-report/services/project-time-report.service';
import { TranslateService } from '@ngx-translate/core';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { IGetProjectTimeBlocksForTimesheetModel } from '@shared/models/generated-interfaces/CoreModels';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CoreService } from '@shared/services/core.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import {
  TimeProjectButtonFunctions,
  TimeProjectContainer,
  TimeProjectSearchFunctions,
} from '@shared/util/Enumerations';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { BehaviorSubject, Observable, take, tap } from 'rxjs';

@Component({
  selector: 'soe-project-time-report-grid-header',
  templateUrl: './project-time-report-grid-header.component.html',
  standalone: false,
})
export class ProjectTimeReportGridHeaderComponent implements OnInit {
  @Output() searchEvent =
    new EventEmitter<IGetProjectTimeBlocksForTimesheetModel>();
  @Output() dateRange = new EventEmitter<[]>();
  @Output() saveDone = new EventEmitter<[]>();

  @Output() groupByDate = new EventEmitter<boolean>();
  @Output() projectsOrdersLoad = new EventEmitter<number[]>();
  @Output() categoriesLoad = new EventEmitter<boolean>();
  @Output() timeDeviationCausesLoad = new EventEmitter<boolean>();
  @Output() employeesLoad = new EventEmitter<number[]>();
  @Output() actionEvent = new EventEmitter<number>();

  @Input() isDisable = signal(true);
  @Input() projectContainer = TimeProjectContainer.TimeSheet;
  @Input() isExpenses = new BehaviorSubject<boolean>(false);
  @Input() employeesDict: ISmallGenericType[] = [];
  @Input() projectsDict: ISmallGenericType[] = [];
  @Input() ordersDict: ISmallGenericType[] = [];
  @Input() timeDeviationCauseDict: ISmallGenericType[] = [];
  @Input() employeeCategoryDict: ISmallGenericType[] = [];
  @Input() useExtendedTimeRegistration = false;
  @Input() splitTimeProductRowsPermission = false;

  perform = new Perform<unknown>(this.progressService);
  service = inject(ProjectTimeReportService);
  coreService = inject(CoreService);
  validationHandler = inject(ValidationHandler);
  messageBoxService = inject(MessageboxService);
  translate = inject(TranslateService);
  _employeesDict: SmallGenericType[] = [];
  employees: IEmployeeTimeCodeDTO[] = [];

  searchFunctions: MenuButtonItem[] = [];
  actionFunctions: MenuButtonItem[] = [];

  formHeader: ProjectTimeReportGridHeaderForm =
    new ProjectTimeReportGridHeaderForm({
      validationHandler: this.validationHandler,
      element: new ProjectTimeReportGridHeaderDTO(),
    });

  constructor(private progressService: ProgressService) {
    this.formHeader.dateRange.patchValue([
      DateUtil.getDateFirstInWeek(new Date()),
      DateUtil.getDateLastInWeek(new Date()),
    ]);
  }

  ngOnInit(): void {
    this.populateFunctionButtons();
  }

  public get isOrder(): boolean {
    return this.projectContainer === TimeProjectContainer.Order;
  }

  public get isTimeSheet(): boolean {
    return this.projectContainer === TimeProjectContainer.TimeSheet;
  }

  public get isOrderRows(): boolean {
    return this.projectContainer === TimeProjectContainer.OrderRows;
  }

  public get isProjectCentral(): boolean {
    return this.projectContainer === TimeProjectContainer.ProjectCentral;
  }

  get showEmployeeSelect(): boolean {
    return this.isTimeSheet || this.isProjectCentral;
  }

  get showTimeDeviationCauseSelect(): boolean {
    return (
      this.isTimeSheet &&
      this.useExtendedTimeRegistration &&
      !this.isExpenses.value
    );
  }

  populateFunctionButtons() {
    this.searchFunctions = [];
    this.actionFunctions = [];

    this.translate
      .get([
        'core.search',
        'billing.project.timesheet.searchincplannedabsence',
        'billing.project.timesheet.groupbydateincplannedabsence',
        'billing.project.timesheet.groupbydate',
        'billing.order.timeproject.getall',
        'billing.order.timeproject.searchintervall',
        'core.loading',
        'common.newrow',
        'core.deleterow',
        'billing.project.timesheet.changeorder',
        'billing.project.timesheet.movetonewproductrow',
        'billing.project.timesheet.movetoexistingproductrow',
        'billing.project.timesheet.changedate',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        //Search button
        if (this.isTimeSheet) {
          this.searchFunctions.push({
            id: TimeProjectSearchFunctions.SearchIntervall,
            label: terms['core.search'],
          });
          if (this.useExtendedTimeRegistration) {
            this.searchFunctions.push({
              id: TimeProjectSearchFunctions.SearchIncPlannedAbsence,
              label: terms['billing.project.timesheet.searchincplannedabsence'],
            });
          }
          this.searchFunctions.push({
            id: TimeProjectSearchFunctions.SearchWithGroupOnDate,
            label: this.useExtendedTimeRegistration
              ? terms['billing.project.timesheet.groupbydateincplannedabsence']
              : terms['billing.project.timesheet.groupbydate'],
          });
        } else if (this.isProjectCentral) {
          this.searchFunctions.push({
            id: TimeProjectSearchFunctions.SearchIntervall,
            label: terms['core.search'],
          });
          this.searchFunctions.push({
            id: TimeProjectSearchFunctions.GetAll,
            label: terms['billing.order.timeproject.getall'],
          });
        } else if (this.isExpenses.asObservable()) {
          this.searchFunctions.push({
            id: TimeProjectSearchFunctions.SearchIntervall,
            label: terms['core.search'],
          });
        } else {
          this.searchFunctions.push({
            id: TimeProjectSearchFunctions.SearchIntervall,
            label: terms['billing.order.timeproject.searchintervall'],
          });
          this.searchFunctions.push({
            id: TimeProjectSearchFunctions.GetAll,
            label: terms['billing.order.timeproject.getall'],
          });
        }

        //Add row button
        if (!this.isOrderRows) {
          this.actionFunctions.push({
            id: TimeProjectButtonFunctions.AddRow,
            label: terms['common.newrow'],
            icon: 'plus',
          });
          this.actionFunctions.push({
            id: TimeProjectButtonFunctions.DeleteRow,
            label: terms['core.deleterow'],
            icon: 'remove',
            disabled: this.isDisable,
          });
          this.actionFunctions.push({
            id: TimeProjectButtonFunctions.MoveRow,
            label: terms['billing.project.timesheet.changeorder'],
            icon: 'arrow-right',
            disabled: this.isDisable,
          });
          if (this.useExtendedTimeRegistration) {
            this.actionFunctions.push({
              id: TimeProjectButtonFunctions.ChangeDate,
              label: terms['billing.project.timesheet.changedate'],
              icon: 'arrow-right',
              disabled: this.isDisable,
            });
          }
        }
        if (this.isOrderRows && this.splitTimeProductRowsPermission) {
          this.actionFunctions.push({
            id: TimeProjectButtonFunctions.MoveRowToNewInvoiceRow,
            label: terms['billing.project.timesheet.movetonewproductrow'],
            icon: 'file-invoice-dollar',
            disabled: this.isDisable,
          });
          this.actionFunctions.push({
            id: TimeProjectButtonFunctions.MoveRowToExistingInvoiceRow,
            label: terms['billing.project.timesheet.movetoexistingproductrow'],
            icon: 'file-invoice-dollar',
            disabled: this.isDisable,
          });
        }
      });
  }

  performFunctionAction(selected: MenuButtonItem): void {
    this.actionEvent.emit(selected.id);
  }

  //#region button Action
  performSearch(event?: MenuButtonItem): void {
    const getAll: boolean =
      this.isProjectCentral && event?.id == TimeProjectSearchFunctions.GetAll
        ? true
        : false;

    const isGroupByDate =
      event?.id === TimeProjectSearchFunctions.SearchWithGroupOnDate;
    this.groupByDate.emit(isGroupByDate);

    if (this.formHeader.employeeIds.value.length == 0) {
      this.formHeader.employeeIds.patchValue(this.getSelectedEmployees([]));
    }

    if (this.formHeader.projectIds.value.length == 0 && this.isProjectCentral) {
      this.formHeader.projectIds.patchValue(this.projectsDict.map(p => p.id));
    }

    this.searchEvent.emit({
      from: !getAll ? this.formHeader.from.value : new Date(1900, 1, 1),
      to: !getAll ? this.formHeader.to.value : new Date(9999, 1, 1),
      employeeId: 0,
      projects: this.formHeader.value.projectIds,
      orders: this.formHeader.value.orderIds,
      employees: this.formHeader.value.employeeIds,
      groupByDate: isGroupByDate,
      incPlannedAbsence:
        event?.id === TimeProjectSearchFunctions.SearchIncPlannedAbsence,
      incInternOrderText: false,
      employeeCategories: this.formHeader.value.categoriesIds,
      timeDeviationCauses: this.formHeader.value.timeDeviationCauseIds,
    });
  }

  //#endregion

  //onSelectionComplete
  categoriesSelectionComplete(event: number[]) {
    if (event.length > 0) {
      this.formHeader.categoriesIds.patchValue(event);
      this.employeesDict = [];
    }
  }

  timeDeviationCauseSelectionComplete(event: number[]) {
    if (event.length > 0) {
      this.formHeader.timeDeviationCauseIds.patchValue(event);
    }
  }

  employeeSelectionComplete(empIds: number[]) {
    this.populateProjectAndInvoice(this.getSelectedEmployees(empIds));
  }

  private getSelectedEmployees(employeeIds: number[]): number[] {
    let empIds: number[] = [];
    if (employeeIds.length > 0) empIds = employeeIds;
    else if (this.isExpenses.value) {
      empIds = this.employeesDict.map(e => e.id);
    }
    this.formHeader.employeeIds.patchValue(empIds);

    return empIds;
  }

  orderSelectionComplete(event: number[]) {
    if (event != null) {
      console.log('order selected?? ', event.length);
    }
  }

  // #onSelectOpen
  populateEmployeeCategories() {
    if (this.employeeCategoryDict.length == 0) this.categoriesLoad.emit(true);
  }

  populateEmployees() {
    if (this.employeesDict.length == 0) {
      this.employeesLoad.emit(this.formHeader.value.categoriesIds);
    }
  }

  populateTimeDeviationCauses() {
    if (this.timeDeviationCauseDict.length == 0)
      this.timeDeviationCausesLoad.emit(true);
  }

  populateProjectAndInvoice(empIds: number[] = []) {
    if (this.ordersDict.length == 0) this.projectsOrdersLoad.emit(empIds);
  }

  // #endRegion onSelectClose
}
