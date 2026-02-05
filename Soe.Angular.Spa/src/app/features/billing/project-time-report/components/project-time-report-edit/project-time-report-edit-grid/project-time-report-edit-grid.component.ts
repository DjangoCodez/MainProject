import { Component, inject, Input, OnInit, signal } from '@angular/core';
import { MaterialCodesService } from '@features/billing/material-codes/services/material-codes.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  CompanySettingType,
  Feature,
  SettingMainType,
  SoeTimeCodeType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IEmployeeProjectInvoiceDTO,
  IProjectInvoiceSmallDTO,
  IProjectSmallDTO,
} from '@shared/models/generated-interfaces/ProjectDTOs';
import {
  IEmployeeScheduleTransactionInfoDTO,
  ITimeCodeDTO,
  ITimeDeviationCauseDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { UserCompanySettingCollection } from '@shared/util/settings-util';
import { ProjectTimeRegistrationType } from '@shared/util/Enumerations';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellEditingStoppedEvent } from 'ag-grid-community';
import { orderBy } from 'lodash';
import { BehaviorSubject, Observable, of, take, tap } from 'rxjs';
import {
  EmployeeInformationDialogDTO,
  IEmployeeTimeCodeDTO,
  ProjectTimeBlockDTO,
  StartTimeDTO,
} from '../../../models/project-time-report.model';
import { ProjectTimeReportService } from '../../../services/project-time-report.service';
import { EditNoteDialogComponent } from '../../project-time-report-grid/edit-note-dialog/edit-note-dialog.component';
import { EmployeeInfoDialogComponent } from '../employee-info-dialog/employee-info-dialog.component';

@Component({
  selector: 'soe-project-time-report-edit-grid',
  templateUrl: './project-time-report-edit-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProjectTimeReportEditGridComponent
  extends GridBaseDirective<ProjectTimeBlockDTO, ProjectTimeReportService>
  implements OnInit
{
  dialogService = inject(DialogService);
  coreService = inject(CoreService);
  materialCodesService = inject(MaterialCodesService);
  service = inject(ProjectTimeReportService);
  messageBoxService = inject(MessageboxService);

  @Input() employeesDict: ISmallGenericType[] = [];
  @Input() employees: IEmployeeTimeCodeDTO[] = [];
  @Input() timeDeviationCausesDict: ISmallGenericType[] = [];

  @Input() projects = new BehaviorSubject<IProjectSmallDTO[]>([]);
  @Input() timeDeviationCauses!: ITimeDeviationCauseDTO[];
  @Input() orders = new BehaviorSubject<IProjectInvoiceSmallDTO[]>([]);

  @Input() employeeId!: number;
  @Input() employee!: IEmployeeTimeCodeDTO;

  @Input() rows = new BehaviorSubject<ProjectTimeBlockDTO[]>([]);
  @Input() workTimePermission!: boolean;
  @Input() invoiceTimePermission!: boolean;
  @Input() useExtendedTimeRegistration!: boolean;
  @Input() invoiceTimeAsWorkTime!: boolean;
  @Input() isDirty = signal(false);
  @Input() isNew = signal(true);
  @Input() isReadOnly = signal(false);
  @Input() defaultTimeCodeId!: number;
  @Input() registrationType!: ProjectTimeRegistrationType;
  @Input() projectInvoices!: IEmployeeProjectInvoiceDTO[];
  @Input() addRow!: BehaviorSubject<boolean>;

  employeeDaysWithSchedule: IEmployeeScheduleTransactionInfoDTO[] = [];
  timeCodeEditable = signal(true);
  chargingTypeDict: ISmallGenericType[] = [];

  startTimes: StartTimeDTO[] = [];
  childDict: ISmallGenericType[] = [];
  private readonly perform = new Perform<any>(this.progressService);
  private limitToProjectUser: boolean = true;
  private lastEmployeeId: number = 0;
  private filteredTimeDeviationCauses: ITimeDeviationCauseDTO[] = [];
  private filteredProjects: IProjectSmallDTO[] = [];
  private filteredOrders: IProjectInvoiceSmallDTO[] = [];

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.None, '', {
      lookups: [this.loadChargingTypeDict(), this.loadSettings()],
    });

    this.orders?.subscribe(o => {
      this.filteredOrders = o;
    });

    this.addRow.subscribe((add: boolean) => {
      if (add) {
        this.doAddRow();
      }
    });
  }

  override onFinished() {
    if (this.isNew()) {
      this.doAddRow();
    }
  }

  public doAddRow() {
    console.log('doAddRow');
    this.createNewRow().subscribe(newRow => {
      //this.createNewRow().pipe(tap newRow => {
      this.rows.value.push(newRow);
      console.log('set data', this.rows.value);
      this.grid.setData(this.rows.value);
      this.focusCell();
    });
  }

  private createNewRow(): Observable<ProjectTimeBlockDTO> {
    const newRow = new ProjectTimeBlockDTO();
    newRow.isNew = true;
    newRow.date = DateUtil.getToday();
    newRow.startTime = DateUtil.defaultDateTime();
    newRow.stopTime = DateUtil.defaultDateTime();
    newRow.employeeId = this.lastEmployeeId;
    newRow.isEditable = true;
    newRow.isPayrollEditable = true;

    if (!newRow.employeeId && this.employee) {
      newRow.employeeId = this.employee.employeeId;
    }

    newRow.selectedEmployee = this.getEmployee(newRow.employeeId);
    this.timeCodeChanged(newRow, this.getDefaultTimeCodeId(newRow));

    //Load previous times and schedule
    if (this.useExtendedTimeRegistration && newRow.employeeId) {
      this.loadEmployeeTimesAndSchedule(newRow).subscribe(() => {
        this.setDefaultTimeDeviationCause(newRow);
      });
      return of(newRow);
    } else {
      return of(newRow);
    }
  }

  loadChargingTypeDict(): Observable<ITimeCodeDTO[]> {
    const type = this.useExtendedTimeRegistration
      ? SoeTimeCodeType.Work
      : SoeTimeCodeType.WorkAndAbsense;
    const onlyWithProducts = this.useExtendedTimeRegistration ? true : false;

    return this.materialCodesService
      .getTimeCodes(type, true, false, onlyWithProducts)
      .pipe(
        tap(result => {
          result.forEach(element => {
            this.chargingTypeDict.push({
              id: element.timeCodeId,
              name: element.name,
            });
          });
        })
      );
  }

  private loadSettings(): Observable<UserCompanySettingCollection | any> {
    return this.coreService
      .getBoolSetting(
        SettingMainType.Company,
        CompanySettingType.ProjectLimitOrderToProjectUsers
      )
      .pipe(
        tap(data => {
          this.limitToProjectUser = data;
        })
      );
  }

  private focusCell(): void {
    setTimeout((): void => {
      const lastRowIdx = this.grid?.api.getLastDisplayedRowIndex();
      this.grid?.api.setFocusedCell(lastRowIdx, 'employeeId');
      this.grid?.api.startEditingCell({
        rowIndex: lastRowIdx,
        colKey: 'employeeId',
      });
    }, 300);
  }

  override onGridReadyToDefine(grid: GridComponent<ProjectTimeBlockDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellEditingStopped: this.onAfterCellEditStopped.bind(this),
    });

    this.translate
      .get([
        'common.employee',
        'billing.project.timesheet.invoice',
        'billing.project.project',
        'common.date',
        'common.week',
        'common.time.timedeviationcause',
        'billing.project.timesheet.chargingtype',
        'billing.project.timesheet.edittime.workedtimefromto',
        'billing.project.timesheet.edittime.workedtime',
        'billing.project.timesheet.edittime.invoicedtime',
        'billing.project.timesheet.child',
        'billing.project.timesheet.edittime.externalnote',
        'billing.project.timesheet.edittime.internalnote',
        'common.from',
        'common.to',
        'billing.project.timesheet.child',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnModified('isModified');

        this.grid.addColumnAutocomplete(
          'employeeId',
          terms['common.employee'],
          {
            source: () => this.employeesDict,
            flex: 1,
            editable: row => this.isEmployeeEditable(row.data),
            sortable: false,
            suppressFilter: true,
          }
        );
        this.grid.addColumnAutocomplete(
          'customerInvoiceId',
          terms['billing.project.timesheet.invoice'],
          {
            source: () => this.filteredOrders,
            flex: 1,
            tooltipField: 'columnNameTooltip',
            editable: row => this.isNewAndEditable(row.data),
            optionIdField: 'invoiceId',
            optionNameField: 'numberName',
            optionDisplayNameField: 'invoiceNr',
            suppressFilter: true,
          }
        );
        this.grid.addColumnAutocomplete(
          'projectId',
          terms['billing.project.project'],
          {
            source: () => this.filteredProjects,
            flex: 1,
            editable: row => this.isNewAndEditable(row.data),
            suppressFilter: true,
            optionIdField: 'projectId',
            optionNameField: 'numberName',
            optionDisplayNameField: 'projectNr',
          }
        );
        this.grid.addColumnDate('date', terms['common.date'], {
          width: 80,
          editable: row => this.isNewAndEditable(row.data),
          suppressFilter: true,
        });
        this.grid.addColumnText('week', terms['common.week'], {
          width: 38,
          editable: false,
          suppressFilter: true,
        });
        this.grid.addColumnAutocomplete(
          'timeDeviationCauseId',
          terms['common.time.timedeviationcause'],
          {
            source: () => this.filteredTimeDeviationCauses,
            editable: row => this.isPayrollEditable(row.data),
            maxWidth: 140,
            suppressFilter: true,
            optionIdField: 'timeDeviationCauseId',
            optionNameField: 'name',
            optionDisplayNameField: 'timeDeviationCauseName',
          }
        );
        this.grid.addColumnAutocomplete(
          'timeCodeId',
          terms['billing.project.timesheet.chargingtype'],
          {
            source: () => this.chargingTypeDict,
            flex: 1,
            editable: row => this.isTimeCodeEditable(row.data),
            suppressFilter: true,
            maxWidth: 140,
          }
        );
        this.grid.addColumnTime('startTime', terms['common.from'], {
          width: 60,
          editable: row => this.startStopEditableFunc(row.data),
          hide: true,
          suppressFilter: true,
          dateFormat: 'HH:mm',
        });
        this.grid.addColumnTime('stopTime', terms['common.to'], {
          width: 60,
          editable: row => this.startStopEditableFunc(row.data),
          hide: true,
          suppressFilter: true,
          dateFormat: 'HH:mm',
        });

        if (this.workTimePermission) {
          this.grid.addColumnTimeSpan(
            'timePayrollQuantity',
            terms['billing.project.timesheet.edittime.workedtime'],
            { flex: 1, editable: row => row.data?.isPayrollEditable === true }
          );
        }

        if (this.invoiceTimePermission) {
          this.grid.addColumnTimeSpan(
            'invoiceQuantity',
            terms['billing.project.timesheet.edittime.invoicedtime'],
            { flex: 1, editable: row => this.invoiceTimeEditableFunc(row.data) }
          );
        }

        this.grid.addColumnAutocomplete(
          'employeeChildId',
          terms['billing.project.timesheet.child'],
          {
            source: () => this.childDict,
            flex: 1,
            editable: row => this.isPayrollEditable(row.data),
            suppressFilter: true,
            maxWidth: 140,
            hide: true,
          }
        );

        this.grid.addColumnText(
          'externalNote',
          terms['billing.project.timesheet.edittime.externalnote'],
          {
            flex: 1,
            editable: row => this.isEditable(row.data),
            suppressFilter: true,
          }
        );
        this.grid.addColumnText(
          'internalNote',
          terms['billing.project.timesheet.edittime.internalnote'],
          {
            flex: 1,
            editable: row => this.isEditable(row.data),
            suppressFilter: true,
          }
        );
        this.grid.addColumnIcon('noteIcon', '', {
          maxWidth: 22,
          headerSeparator: true,
          onClick: row => this.showNote(row),
          suppressFilter: true,
        });

        if (this.useExtendedTimeRegistration) {
          this.grid.addColumnIcon(null, '', {
            maxWidth: 22,
            showIcon: r => r.hasError,
            iconClass: 'warningColor',
            headerSeparator: true,
            suppressFilter: true,
            iconName: 'exclamation-triangle',
          });
          this.grid.addColumnIcon(null, '', {
            maxWidth: 22,
            headerSeparator: true,
            suppressFilter: true,
            iconName: 'info-circle',
            onClick: this.showDayInfo.bind(this),
          });
        }
        this.grid.addColumnIconDelete({ onClick: r => this.deleteRow(r) });
      });

    this.grid.context.suppressGridMenu = true;
    this.grid.finalizeInitGrid({ hidden: true });
  }

  private showNote(row: ProjectTimeBlockDTO) {
    const title = '';

    const dialogData: any = {
      title: title,
      size: 'lg',
      row: row,
      rows: this.rows.value,
      isDisable: this.isReadOnly,
      workTimePermission: this.workTimePermission,
      invoiceTimePermission: this.invoiceTimePermission,
    };
    this.dialogService
      .open(EditNoteDialogComponent, dialogData)
      .afterClosed()
      .pipe()
      .subscribe(result => {
        if (result) {
          this.isDirty.set(true);

          row.externalNote = result.externalNote;
          row.internalNote = result.internalNote;
          this.grid.refreshCells();
        }
      });
  }

  private isEditable(row: ProjectTimeBlockDTO | undefined): boolean {
    return row?.isEditable ?? true;
  }

  private isNewAndEditable(row: ProjectTimeBlockDTO | undefined): boolean {
    return this.isEditable(row) && (row?.isNew ?? false);
  }

  private isTimeCodeEditable(row: ProjectTimeBlockDTO | undefined): boolean {
    const isTimeCodeEditable = this.isEditable(row) && !row?.timeCodeReadOnly;
    return isTimeCodeEditable;
  }

  private isPayrollEditable(row: ProjectTimeBlockDTO | undefined): boolean {
    return (row?.isPayrollEditable ?? false) && (!row?.hasError || false);
  }

  private isEmployeeEditable(row: ProjectTimeBlockDTO | undefined): boolean {
    return (
      this.isEditable(row) &&
      (row?.isPayrollEditable ?? false) &&
      (row?.isNew || false)
    );
  }

  deleteRow(row: ProjectTimeBlockDTO) {
    this.grid.deleteRow(row);
  }

  private isColEditable(): boolean {
    return this.timeCodeEditable();
  }

  private startStopEditableFunc(row: ProjectTimeBlockDTO | undefined): boolean {
    return (
      (row?.isPayrollEditable &&
        (!row.autoGenTimeAndBreakForProject || row.mandatoryTime)) ||
      false
    );
  }

  private invoiceTimeEditableFunc(
    row: ProjectTimeBlockDTO | undefined
  ): boolean {
    return row?.isEditable && row.timeCodeId && !row['hasError'] ? true : false;
  }

  override loadData(
    id?: number,
    additionalProps?: any
  ): Observable<ProjectTimeBlockDTO[]> {
    return of([]);
  }

  private showDayInfo(row: ProjectTimeBlockDTO) {
    const info = this.employeeDaysWithSchedule?.find(
      e =>
        e.employeeId === row.employeeId &&
        e.date.toDateString() === row.date.toDateString()
    );
    if (info) {
      this.showInfoDialog(info);
    } else {
      this.getEmployeeScheduleAndTransactionInfo(row, true).subscribe();
    }
  }

  private showInfoDialog(info: IEmployeeScheduleTransactionInfoDTO) {
    const employee = this.employees.find(e => e.employeeId === info.employeeId);

    const title =
      this.translate.instant(
        'billing.project.timesheet.edittime.dialogschema'
      ) + employee?.name ||
      '' + ' (' + DateUtil.toSwedishFormattedDate(info.date) + ')';
    const dialogData: EmployeeInformationDialogDTO = {
      size: 'md',
      title: title,
      data: info,
    };
    this.dialogService
      .open(EmployeeInfoDialogComponent, dialogData)
      .afterClosed()
      .pipe()
      .subscribe(() => {});
  }

  private onAfterCellEditStopped(event: CellEditingStoppedEvent) {
    if (event.newValue === event.oldValue) return;

    switch (event.colDef.field) {
      case 'employeeId': {
        this.selectedEmployeeChanged(event);
        break;
      }
      case 'customerInvoiceId': {
        this.selectedOrderChanged(event.data, event.newValue);
        break;
      }
      case 'projectId': {
        this.selectedProjectChanged(event.data, event.newValue);
        break;
      }
      case 'timeDeviationCauseId': {
        const timeDeviationCause = this.filteredTimeDeviationCauses.find(
          x => x.timeDeviationCauseId === event.newValue
        );
        this.timeDeviationCauseChanged(
          event.data,
          timeDeviationCause?.timeDeviationCauseId ?? 0
        );
        break;
      }
      case 'timeCodeId': {
        const timeCode = event.data.filteredTimeCodes.find(
          (x: { label: string }) => x.label === event.newValue
        );
        this.timeCodeChanged(event.data, timeCode ? timeCode.value : 0);
        break;
      }
      case 'timePayrollQuantity': {
        this.payrollQuantityChanged(event.data);
        break;
      }
      case 'startTime':
      case 'stopTime': {
        this.timeChanged(event.data);
        break;
      }
      case 'date': {
        this.selectedDateChanged(event);
        break;
      }
      case 'employeeChildId': {
        const child = event.data.children?.find(
          (x: { name: string }) => x.name === event.newValue
        );
        event.data.employeeChildId = child ? child.id : 0;
        event.data.employeeChildName = child ? child.name : '';
        break;
      }
    }
  }

  selectedEmployeeChanged(event: CellEditingStoppedEvent) {
    this.checkDirty();
    event.data.isEdited = true;

    event.data.selectedEmployee = event.newValue
      ? this.employeesDict.find(e => e.id === event.newValue)
      : null;
    event.data.filteredTimeCodes = this.chargingTypeDict;

    //Set first time
    const employeeRows = orderBy(
      this.rows.value.filter(
        r =>
          r.employeeId === event.data.employeeId &&
          r.date.toDateString() === event.data.date.toDateString()
      ),
      'startTime'
    );
    if (employeeRows && employeeRows.length > 0) {
      const lastRow = employeeRows[employeeRows.length - 1];
      event.data.startTime = lastRow.stopTime;
    } else {
      const startTimeItem = this.startTimes.find(
        startTime =>
          startTime.employeeId === event.data.employeeId &&
          startTime.date.toDateString() === event.data.date.toDateString()
      );
      if (startTimeItem) event.data.startTime = startTimeItem.startTime;
      else this.getFirstEligibleTimeForEmployee(event.data, event.data.date);
    }

    if (this.registrationType !== ProjectTimeRegistrationType.Order) {
      if (event.data.employeeId) {
        if (!this.filterProjectsAndInvoices(event.data)) {
          event.data.filteredProjects = [];
          event.data.filteredInvoices = [];
        }
      }
    }

    if (this.useExtendedTimeRegistration) {
      event.data.employeeChildId = 0;
      event.data.child = [];
      this.loadEmployeeTimesAndSchedule(event.data).pipe(
        tap(() => {
          if (event.data.isNewRow) {
            this.setDefaultTimeDeviationCause(event.data);
            this.timeCodeChanged(
              event.data,
              this.getDefaultTimeCodeId(event.data)
            );
            this.setTimeCodeReadOnly(event.data);
          }
        })
      );
    } else {
      this.timeCodeChanged(event.data, this.getDefaultTimeCodeId(event.data));
    }
  }

  private filterProjectsAndInvoices(row: ProjectTimeBlockDTO): boolean {
    const projectInvoice = this.getProjectInvoiceRow(row.employeeId);
    if (projectInvoice) {
      row.filteredProjects = projectInvoice.projects;
      row.filteredInvoices = projectInvoice.invoices;
      return true;
    }
    return false;
  }

  private getProjectInvoiceRow(employeeId: number) {
    console.log('getProjectInvoiceRow', employeeId, this.limitToProjectUser);

    if (
      !this.limitToProjectUser &&
      this.projectInvoices &&
      this.projectInvoices.length > 0
    ) {
      return this.projectInvoices[0];
    } else {
      return this.projectInvoices.find(p => p.employeeId === employeeId);
    }
  }

  private loadEmployeeTimesAndSchedule(
    row: ProjectTimeBlockDTO
  ): Observable<IEmployeeScheduleTransactionInfoDTO> {
    //Load previous times and schedule
    const dayObject =
      this.employeeDaysWithSchedule?.filter(
        e =>
          e.employeeId === row.employeeId &&
          e.date.toDateString() === row.date.toDateString()
      ) ?? [];

    this.setRowErrorMessage('', row);
    if (dayObject && dayObject.length > 0) {
      this.filterTimeDeviationCauses(dayObject[0].employeeGroupId);
      this.employeeScheduleInfoChanged(row, dayObject[0]);
      return of(dayObject[0]);
    } else {
      return this.getEmployeeScheduleAndTransactionInfo(row);
    }
  }

  private getEmployeeScheduleAndTransactionInfo(
    row: ProjectTimeBlockDTO,
    openDialog = false
  ): Observable<IEmployeeScheduleTransactionInfoDTO> {
    if (this.employeeDaysWithSchedule == undefined && this.employeeId) {
      this.employeeDaysWithSchedule = [];
    }
    return this.service
      .GetEmployeeScheduleAndTransactionInfo(
        row.employeeId,
        DateUtil.getISODateString(row.date)
      )
      .pipe(
        tap(result => {
          if (result) {
            this.employeeScheduleInfoChanged(row, result);
            this.filterTimeDeviationCauses(result.employeeGroupId);
            if (
              this.employeeDaysWithSchedule.filter(
                e => e.employeeId === row.employeeId && e.date === row.date
              ).length === 0
            ) {
              this.employeeDaysWithSchedule.push(result);

              if (openDialog) this.showDayInfo(row);
            }
          }
        })
      );
  }

  private filterTimeDeviationCauses(employeeGroupId: number) {
    //TODO...needs a server call
    this.filteredTimeDeviationCauses = this.timeDeviationCauses;
    console.log('filterTimeDeviationCauses', this.filteredTimeDeviationCauses);
    /*
    this.rows.value.forEach(row => {
      if (row.timeDeviationCauseId) {
        if (
          this.timeDeviationCauses?.find(
            e => e.timeDeviationCauseId === row.timeDeviationCauseId
          )
        ) {
          this.timeDeviationCauses?.push(
            this.createTimeDeviationCause(row) as ITimeDeviationCauseDTO
          );
        }
        if (row.projectTimeBlockId) {
          this.setTimeCodeReadOnly(row);
        }
      }
    });
    */
  }

  private createTimeDeviationCause(row: ProjectTimeBlockDTO): unknown {
    return {
      actorCompanyId: 0,
      adjustTimeInsideOfPlannedAbsence: 0,
      adjustTimeOutsideOfPlannedAbsence: 0,
      attachZeroDaysNbrOfDaysAfter: 0,
      attachZeroDaysNbrOfDaysBefore: 0,
      calculateAsOtherTimeInSales: false,
      changeCauseInsideOfPlannedAbsence: 0,
      changeCauseOutsideOfPlannedAbsence: 0,
      employeeGroupIds: [],
      externalCodes: [],
      name: row.timeDeviationCauseName,
      timeDeviationCauseId: row.timeDeviationCauseId,
      validForHibernating: false,
      candidateForOvertime: false,
    };
  }

  private employeeScheduleInfoChanged(
    row: ProjectTimeBlockDTO,
    data: IEmployeeScheduleTransactionInfoDTO
  ) {
    row.autoGenTimeAndBreakForProject = data.autoGenTimeAndBreakForProject;
    const matchingRows = this.rows.value.filter(
      x => x.date.toDateString() === data.date.toDateString()
    );

    if (matchingRows.length > 0) {
      matchingRows.forEach(
        r =>
          (r.autoGenTimeAndBreakForProject = data.autoGenTimeAndBreakForProject)
      );
    }

    if (data.employeeGroupId) {
      if (
        this.employee &&
        !this.employee.timeDeviationCauseId &&
        data.timeDeviationCauseId
      ) {
        this.employee.timeDeviationCauseId = data.timeDeviationCauseId;
      }

      this.toggleStartStopColumns();
    } else {
      this.setRowErrorMessage(
        'billing.project.timesheets.employeegroupmissing',
        row
      );

      row.timeDeviationCauseId = 0;
    }
  }

  private setRowErrorMessage(msgKey: string, row: ProjectTimeBlockDTO) {
    if (msgKey) {
      row.hasError = true;
      row.errorText = this.terms[msgKey];
    } else {
      row.hasError = false;
      row.errorText = '';
    }
  }

  private getFirstEligibleTimeForEmployee(
    row: ProjectTimeBlockDTO,
    date: string
  ) {
    this.service.GetEmployeeFirstEligibleTime(row.employeeId, date).pipe(
      tap(result => {
        if (result) {
          // result = DateUtil.convertToDate(result);
          row.startTime = new Date(
            row.startTime.getFullYear(),
            row.startTime.getMonth(),
            row.startTime.getDate(),
            result.getHours(),
            result.getMinutes(),
            result.getSeconds(),
            0
          );
          this.startTimes.push({
            employeeId: row.employeeId,
            date: new Date(date),
            startTime: row.startTime,
          });
        }
      })
    );
  }

  private checkDirty() {
    if (this.rows.value.length > 0) {
      const modified = this.rows.value.filter(x => x.isModified).length > 0;
      this.isDirty.set(modified);
    }
  }

  selectedOrderChanged(row: ProjectTimeBlockDTO, newValue: number) {
    this.orders.subscribe(orders => {
      row.selectedOrder =
        orders.find(x => x.invoiceId === newValue) ??
        ({} as IProjectInvoiceSmallDTO);
      row.customerInvoiceId = row.selectedOrder
        ? row.selectedOrder.invoiceId
        : 0;
      row.invoiceNr = row.selectedOrder?.invoiceNr ?? '';
      row.customerName = row.selectedOrder?.customerName ?? '';

      if (row.customerInvoiceId) {
        this.selectedProjectChanged(row, row.selectedOrder.projectId);
      } else {
        this.filteredOrders = this.orders.value;
        this.filteredProjects = this.projects.value;
      }
      this.rowDataChanged(row);
    });
  }

  selectedProjectChanged(row: ProjectTimeBlockDTO, newValue: number) {
    this.projects.subscribe(projects => {
      row.selectedProject =
        projects.find(x => x.projectId === newValue) ??
        ({} as IProjectSmallDTO);

      console.log('selectedProjectChanged', row.selectedProject);
      row.projectId = row.selectedProject?.projectId ?? 0;
      row.projectName = row.selectedProject?.name ?? '';
      row.projectNr = row.selectedProject?.number ?? '';

      if (row.projectId) {
        this.orders.subscribe(orders => {
          this.filteredOrders = this.orders.value?.filter(
            order => order.projectId === row.projectId
          );
        });
      } else {
        this.filteredOrders = this.orders.value;
        this.filteredProjects = this.projects.value;
      }
      this.rowDataChanged(row);
    });
  }

  rowDataChanged(row: ProjectTimeBlockDTO) {
    row.isModified = true;
    if (this.grid) this.grid.refreshCells();
    this.checkDirty();
  }

  timeDeviationCauseChanged(
    row: ProjectTimeBlockDTO,
    timeDeviationCauseId: number
  ) {
    const timeDeviationCause = timeDeviationCauseId
      ? this.getTimeDeviationCause(timeDeviationCauseId)
      : undefined;

    row.timeDeviationCauseName = timeDeviationCause?.name ?? '';
    row.timeDeviationCauseId = timeDeviationCauseId;

    //charging type
    this.setTimeCodeReadOnly(row);
    if (row.isEditable && !row.timeCodeReadOnly) {
      this.timeCodeChanged(row, this.getDefaultTimeCodeId(row));
    }

    //child
    if (
      timeDeviationCause &&
      timeDeviationCause.specifyChild &&
      (!row.children || row.children?.length === 0)
    ) {
      this.loadEmployeeChildren(row, true);
    } else if (timeDeviationCause && !timeDeviationCause.specifyChild) {
      row.children = [];
      row.employeeChildId = 0;
      row.employeeChildName = '';
    }

    //from-to
    row.mandatoryTime = timeDeviationCause?.mandatoryTime ?? false;

    if (!row.mandatoryTime && timeDeviationCause?.mandatoryTime) {
      row.startTime = DateUtil.defaultDateTime();
      row.stopTime = DateUtil.defaultDateTime();
    }

    row.mandatoryTime = timeDeviationCause?.mandatoryTime ?? false;
    row.additionalTime =
      timeDeviationCause?.calculateAsOtherTimeInSales ?? false;

    if (row.mandatoryTime) {
      this.toggleStartStopColumns(true);
    }

    console.log(
      'timeDeviationCauseChanged',
      row.timeDeviationCauseId,
      row.timeDeviationCauseName
    );

    this.rowDataChanged(row);
  }

  private setDefaultTimeDeviationCause(row: ProjectTimeBlockDTO) {
    let timeDeviationCauseId = 0;
    if (
      row.selectedEmployee &&
      row.selectedEmployee.timeDeviationCauseId &&
      row.selectedEmployee.timeDeviationCauseId > 0
    )
      timeDeviationCauseId = row.selectedEmployee.timeDeviationCauseId;
    else if (
      this.filteredTimeDeviationCauses &&
      this.filteredTimeDeviationCauses.length > 0
    ) {
      timeDeviationCauseId =
        this.filteredTimeDeviationCauses[0].timeDeviationCauseId;
    }

    this.timeDeviationCauseChanged(row, timeDeviationCauseId);
  }

  private loadEmployeeChildren(
    row: ProjectTimeBlockDTO,
    setFirstChildAsDefault: boolean
  ) {
    this.getEmployeeChildren(row.employeeId).subscribe(child => {
      this.childDict = child;
      this.toggleChildColumn(true);
      if (
        setFirstChildAsDefault &&
        child.length === 1 &&
        !row.employeeChildId
      ) {
        row.employeeChildId = child[0].id;
        row.employeeChildName = child[0].name;
      }
    });
  }

  private getEmployeeChildren(
    employeeId: number
  ): Observable<ISmallGenericType[]> {
    return this.perform.load$(this.service.getEmployeeChildren(employeeId));
  }

  toggleChildColumn(show = false) {
    if (show) this.grid.showColumns(['employeeChildId']);
    else this.grid.hideColumns(['employeeChildId']);
  }

  toggleStartStopColumns(show = false) {
    if (this.useExtendedTimeRegistration && this.workTimePermission) {
      show =
        this.rows.value.filter(
          x => !x.autoGenTimeAndBreakForProject || x.mandatoryTime
        ).length > 0;
    }

    if (show) {
      this.grid.showColumns(['startTime']);
      this.grid.showColumns(['stopTime']);
    } else {
      this.grid.hideColumns(['startTime']);
      this.grid.hideColumns(['stopTime']);
    }
  }

  timeCodeChanged(row: ProjectTimeBlockDTO, newValue: number) {
    const timeCode = row.filteredTimeCodes.find(x => x.value === newValue);
    row.timeCodeId = newValue;
    row.timeCodeName = timeCode ? timeCode.label : '';
    this.rowDataChanged(row);
  }

  payrollQuantityChanged(row: ProjectTimeBlockDTO) {
    if (row.startTime) {
      row.stopTime = row.startTime.addMinutes(row.timePayrollQuantity);
      row['payrollQuantityChanged'] = true;
    }

    this.updateInvoiceQuantityFromPayroll(row, false);

    this.rowDataChanged(row);
  }

  timeChanged(row: ProjectTimeBlockDTO) {
    if (row.stopTime.getHours() < row.startTime.getHours()) {
      if (row.startTime.getDay() === row.stopTime.getDay()) {
        row.stopTime = row.stopTime.addDays(1);
      }
    } else if (
      row.stopTime.getHours() >= row.startTime.getHours() &&
      row.stopTime.getDay() > row.startTime.getDay()
    ) {
      row.stopTime = row.stopTime.addDays(-1);
    }
    if (
      row.startTime &&
      row.stopTime &&
      (row.stopTime.getHours() > 0 || row.stopTime.getMinutes() > 0)
    ) {
      row.timePayrollQuantity = row.stopTime.diffMinutes(row.startTime);
      this.updateInvoiceQuantityFromPayroll(row);
    }

    this.rowDataChanged(row);
  }

  selectedDateChanged(event: CellEditingStoppedEvent) {
    event.data.week = DateUtil.getWeekNumber(event.newValue).toString();
    event.data.isEdited = true;
    this.grid.refreshCells();
  }

  private setTimeCodeReadOnly(row: ProjectTimeBlockDTO) {
    const currentCause = this.getTimeDeviationCause(row.timeDeviationCauseId);

    if (currentCause && this.useExtendedTimeRegistration && row.isEditable) {
      row.timeCodeReadOnly =
        currentCause.isPresence && !currentCause.notChargeable ? false : true;
      this.timeCodeEditable.set(!row.timeCodeReadOnly);

      if (row.timeCodeReadOnly) {
        this.timeCodeChanged(row, 0);
        row.invoiceQuantity = 0;
      }
    }
  }

  private getDefaultTimeCodeId(row: ProjectTimeBlockDTO): number {
    if (row.timeDeviationCauseId > 0) {
      const currentTimeDeviationCause = this.getTimeDeviationCause(
        row.timeDeviationCauseId
      );
      const timeCodeId = currentTimeDeviationCause?.timeCodeId ?? 0;
      if (timeCodeId > 0) {
        if (this.timeCodeIdExists(timeCodeId)) {
          return timeCodeId;
        }
      }
    }

    return row.selectedEmployee &&
      row.selectedEmployee.defaultTimeCodeId &&
      row.selectedEmployee.defaultTimeCodeId > 0
      ? row.selectedEmployee.defaultTimeCodeId
      : this.defaultTimeCodeId;
  }

  private getTimeDeviationCause(
    timeDeviationCauseId: number
  ): ITimeDeviationCauseDTO | undefined {
    return this.timeDeviationCauses
      ? this.timeDeviationCauses.find(
          x => x.timeDeviationCauseId === timeDeviationCauseId
        )
      : undefined;
  }

  private getEmployee(employeeId: number) {
    return this.employees.find(x => x.employeeId === employeeId);
  }

  private updateInvoiceQuantityFromPayroll(
    row: ProjectTimeBlockDTO,
    refreshCells = true
  ) {
    if (
      row['isNew'] === true &&
      this.invoiceTimeAsWorkTime &&
      this.invoiceTimePermission &&
      !row.timeCodeReadOnly
    ) {
      row.invoiceQuantity = row.timePayrollQuantity;
      if (refreshCells) {
        this.grid.refreshCells();
      }
    }
  }

  private timeCodeIdExists(timeCodeId: number): boolean {
    const chargingTypeDict = this.chargingTypeDict.filter(
      x => x.id === timeCodeId
    ).length;
    return chargingTypeDict > 0;
  }
}
