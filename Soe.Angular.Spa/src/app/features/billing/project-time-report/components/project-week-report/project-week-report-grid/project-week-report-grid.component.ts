import { Component, inject, OnInit, signal } from '@angular/core';
import { ProjectTimeReportService } from '@features/billing/project-time-report/services/project-time-report.service';
import { ProjectWeekReportService } from '@features/billing/project-time-report/services/project-week-report.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  CompanySettingType,
  Feature,
  SettingMainType,
  SoeTimeCodeType,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IProjectInvoiceSmallDTO,
  IProjectSmallDTO,
} from '@shared/models/generated-interfaces/ProjectDTOs';
import {
  IEmployeeTimeCodeDTO,
  IProjectTimeMatrixSaveDTO,
  IProjectTimeMatrixSaveRowDTO,
  ITimeCodeDTO,
  ITimeDeviationCauseDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DateUtil } from '@shared/util/date-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, take, tap } from 'rxjs';
import { CellEditingStoppedEvent } from 'ag-grid-community';
import {
  GetProjectTimeBlocksForMatrixModel,
  ProjectTimeMatrixDTO,
  ProjectTimeMatrixSaveRowDTO,
  WeekReportFooterDTO,
} from '@features/billing/project-time-report/models/project-time-report.model';
import { MaterialCodesService } from '@features/billing/material-codes/services/material-codes.service';
import { IGetProjectEmployeesModel } from '@shared/models/generated-interfaces/BillingModels';
import { DayOfWeek } from '@shared/util/Enumerations';
import { WeekReportForm } from '@features/billing/project-time-report/models/project-week-report-form.model';
import { ValidationHandler } from '@shared/handlers';
import { CrudActionTypeEnum } from '@shared/enums';
import { EditNoteDialogComponent } from '../../project-time-report-grid/edit-note-dialog/edit-note-dialog.component';
import { SaveUserCompanySettingModel } from '@shared/components/select-project-dialog/models/select-project-dialog.model';

@Component({
  selector: 'soe-project-week-report-grid',
  templateUrl: './project-week-report-grid.component.html',
  styleUrls: ['./project-week-report-grid.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProjectWeekReportGridComponent
  extends GridBaseDirective<ProjectTimeMatrixDTO, ProjectWeekReportService>
  implements OnInit
{
  service = inject(ProjectWeekReportService);
  validationHandler = inject(ValidationHandler);
  projectService = inject(ProjectTimeReportService);
  projectWeekReportService = inject(ProjectWeekReportService);
  private readonly coreService = inject(CoreService);
  private readonly materialCodesService = inject(MaterialCodesService);
  dialogService = inject(DialogService);

  _showWeekend = signal(false);
  showWeekend = signal(false);
  isDeleteDisable = signal(true);
  isCopyDisable = signal(true);

  filteredOrdersDict: IProjectInvoiceSmallDTO[] = [];
  allProjects: ISmallGenericType[] = [];
  allOrders: IProjectInvoiceSmallDTO[] = [];
  allProjectsAndInvoices: ISmallGenericType[] = [];
  projectInvoices: IProjectSmallDTO[] = [];
  filteredProjectsDict: IProjectSmallDTO[] = [];
  private timeDeviationCauseDict: ISmallGenericType[] = [];
  private timeCodeDicts: ISmallGenericType[] = [];
  employees: IEmployeeTimeCodeDTO[] = [];
  employeesDict: ISmallGenericType[] = [];
  protected projectTimeBlockRows: ProjectTimeMatrixDTO[] = [];

  workTimePermission: boolean = false;
  showSelection: boolean = false;
  invoiceTimePermission: boolean = false;
  modifyOtherEmployeesPermission: boolean = false;
  useExtendedTimeRegistration: boolean = false;
  protected timeProjectFrom!: Date;
  protected timeProjectTo!: Date;
  includeEmployeeId: number = 0;
  employee!: IEmployeeTimeCodeDTO;
  employeeId = SoeConfigUtil.employeeId;
  selectedEmployeeId: number = this.employeeId;

  form: WeekReportForm = new WeekReportForm({
    validationHandler: this.validationHandler,
    element: new WeekReportFooterDTO(),
  });
  saveLabel = '';

  ngOnInit(): void {
    super.ngOnInit();
  }

  override onTabActivated(): void {
    if (!this.gridIsDefined) {
      this.doStartFlow();
    }
  }

  doStartFlow() {
    this.startFlow(Feature.None, 'Billing.Projects.Project.Matrix', {
      additionalModifyPermissions: [
        Feature.Time_Project_Invoice_WorkedTime,
        Feature.Time_Project_Invoice_InvoicedTime,
        Feature.Billing_Project_TimeSheetUser_OtherEmployees,
      ],
      lookups: [
        this.loadTimeCodes(),
        this.loadEmployee(),
        this.loadProjectInvoices(),
      ],
    });
  }

  doSearch(selectedEmp: GetProjectTimeBlocksForMatrixModel) {
    this.selectedEmployeeId = selectedEmp.selectedEmp;

    this.timeProjectFrom = selectedEmp.from;
    this.timeProjectTo = DateUtil.getDateLastInWeek(
      selectedEmp.from || new Date()
    );

    this.searchGridData();
  }

  protected searchGridData(lastWeekStart?: Date, lastWeekStop?: Date) {
    this.performLoadData.load(
      this.service
        .loadProjectTimeBlockForMatrix(
          this.employeeId,
          this.selectedEmployeeId,
          lastWeekStart
            ? DateUtil.getISODateString(lastWeekStart)
            : DateUtil.getISODateString(this.timeProjectFrom),
          lastWeekStop
            ? DateUtil.getISODateString(lastWeekStop)
            : DateUtil.getISODateString(this.timeProjectTo),
          false
        )
        .pipe(
          tap(rows => {
            if (rows.length > 0) {
              rows.forEach(row => this.map(row));
              this.grid.setData(rows);
              this.checkWeekends();
              this.updateFooterTotals(rows);
            }

            this.isCopyDisable.set(this.projectTimeBlockRows.length > 0);
          })
        )
    );
  }

  map(row: ProjectTimeMatrixDTO) {
    for (let d = 1; d <= 7; d++) {
      (row as any)[`invoiceQuantityFormatted_${d}`] = 0;
      (row as any)[`timePayrollQuantityFormatted_${d}`] = 0;
    }

    row.rows.forEach(cell => {
      if (cell.weekDay != null) {
        const day = cell.weekDay.toString();
        (row as any)[`invoiceQuantityFormatted_${day}`] =
          cell.invoiceQuantity || 0;
        (row as any)[`timePayrollQuantityFormatted_${day}`] =
          cell.payrollQuantity || 0;

        this.noteIcon(cell, row);
        this.isEditableAll(cell);
      }
    });
  }

  private isEditableAll(row: ProjectTimeMatrixSaveRowDTO) {
    if (!row.isInvoiceEditable || !row.isPayrollEditable) {
      this.showSelection = false;
    } else this.showSelection = true;
  }

  noteIcon(cell: IProjectTimeMatrixSaveRowDTO, row: ProjectTimeMatrixDTO) {
    const day = cell.weekDay.toString();

    const isNote =
      (cell.externalNote && cell.externalNote.length > 0) ||
      (cell.internalNote && cell.internalNote.length > 0);
    (row as any)[`noteIcon_${day}`] = isNote ? 'file-alt' : 'file';
  }

  override onPermissionsLoaded(): void {
    super.onPermissionsLoaded();

    this.workTimePermission = this.flowHandler.hasModifyAccess(
      Feature.Time_Project_Invoice_WorkedTime
    );
    this.invoiceTimePermission = this.flowHandler.hasModifyAccess(
      Feature.Time_Project_Invoice_InvoicedTime
    );
    this.modifyOtherEmployeesPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Project_TimeSheetUser_OtherEmployees
    );
  }

  private isOrderEditable(
    timeMatrix: ProjectTimeMatrixDTO | undefined
  ): boolean {
    const hasBlocked = timeMatrix?.rows?.some(r => !!r.projectTimeBlockId);
    return !hasBlocked;
  }

  protected employeeChange(event: number) {
    this.selectedEmployeeId = event;
    this.grid.setData([]);
    this.loadProjectInvoices().subscribe();
  }

  protected deleteRow() {
    const selectedRows: ProjectTimeMatrixDTO[] = this.grid.getSelectedRows();

    if (selectedRows.length > 1) {
      selectedRows.forEach(row => {
        if (row) {
          row.isDeleted = true;
          row.isModified = true;
          this.grid.refreshCells();
        }
      });
      this.save();
    }
  }

  protected copyLastWeek() {
    const lastWeekStart = this.timeProjectFrom.addDays(-1).beginningOfWeek();
    const lastWeekStop = this.timeProjectFrom.endOfWeek();

    this.searchGridData(lastWeekStart, lastWeekStop);
    this.grid.refreshCells();
    this.form.markAsDirty();
  }

  protected addRow() {
    const rows = this.grid.getAllRows();
    const row = new ProjectTimeMatrixDTO();
    row.employeeId = this.selectedEmployeeId;
    row.rows = [];
    row.isModified = true;
    this.grid.setData([...rows, row]);
    this.grid.refreshCells();
    this.form.markAsDirty();

    this.grid.options.context.newRow = true;
    this.focusFirstCell();
  }

  private focusFirstCell(): void {
    const lastRowIdx = this.grid.getAllRows().length - 1;
    this.grid?.api.setFocusedCell(lastRowIdx, 'customerInvoiceId');
    this.grid?.api.startEditingCell({
      rowIndex: lastRowIdx,
      colKey: 'customerInvoiceId',
    });
  }

  private updateFooterTotals(rows: ProjectTimeMatrixDTO[]) {
    let invoiceQuantity = 0;
    let timePayrollQuantity = 0;
    for (const element of rows) {
      const row = element;
      timePayrollQuantity += this.getTimePayrollQuantity_Total(row.rows);
      invoiceQuantity += this.getInvoiceQuantity_Total(row.rows);
    }

    if (this.workTimePermission) {
      this.form.patchValue({
        workedTime: DateUtil.minutesToTimeSpan(
          invoiceQuantity,
          false,
          false,
          true
        ),
      });
    }
    if (this.invoiceTimePermission) {
      this.form.patchValue({
        invoicedTime: DateUtil.minutesToTimeSpan(
          timePayrollQuantity,
          false,
          false,
          true
        ),
      });
    }
  }

  public getTimePayrollQuantity_Total(
    rows: IProjectTimeMatrixSaveRowDTO[]
  ): number {
    let total = 0;
    rows.forEach(row => {
      total += row.payrollQuantity;
    });
    return total;
  }

  public getInvoiceQuantity_Total(
    rows: IProjectTimeMatrixSaveRowDTO[]
  ): number {
    let total = 0;
    rows.forEach(row => {
      total += row.invoiceQuantity;
    });
    return total;
  }

  private loadTimeCodes(): Observable<ITimeCodeDTO[]> {
    const type = this.useExtendedTimeRegistration
      ? SoeTimeCodeType.Work
      : SoeTimeCodeType.WorkAndAbsense;
    const onlyWithProducts = this.useExtendedTimeRegistration ? true : false;

    return this.materialCodesService
      .getTimeCodes(type, true, false, onlyWithProducts)
      .pipe(
        tap(x => {
          this.timeCodeDicts = x.map(
            x => <ISmallGenericType>{ id: x.timeCodeId, name: x.name }
          );
        })
      );
  }

  loadProjectInvoices() {
    const employeeIds = [this.selectedEmployeeId];

    return this.projectService
      .getProjectsForTimeSheetEmployees(employeeIds)
      .pipe(
        tap(result => {
          this.allProjectsAndInvoices = [];
          this.allProjects = [];
          this.allOrders = [];

          //Filter invoices
          for (const ord of result[0].invoices) {
            if (
              this.allOrders.filter(x => x.invoiceId === ord.invoiceId)
                .length === 0
            ) {
              this.allOrders = result[0].invoices;
              this.filteredOrdersDict = result[0].invoices;
            }
          }

          this.filteredOrdersDict = result[0].invoices;
          this.filteredProjectsDict = result[0].projects;
        })
      );
  }

  private loadTimeDeviationCausesForEmployee(
    employeeGroupId: number
  ): Observable<ITimeDeviationCauseDTO[]> {
    return this.performLoadData.load$(
      this.projectService
        .getTimeDeviationCauses(employeeGroupId, false, true)
        .pipe(
          tap((x: ITimeDeviationCauseDTO[]) => {
            this.timeDeviationCauseDict = x.map(
              y =>
                <ISmallGenericType>{ id: y.timeDeviationCauseId, name: y.name }
            );
          })
        )
    );
  }

  onCellEdit(event: CellEditingStoppedEvent) {
    const row = event.data as ProjectTimeMatrixDTO;
    switch (event.colDef.field) {
      case 'projectId':
        {
          const project = this.filteredProjectsDict.find(
            p => p.projectId === event.newValue
          );
          if (project) {
            if (project.projectId !== row.projectId) {
              this.projectChanged(project.projectId, project.numberName, row);
              const projectOrders = this.allOrders.filter(
                x => x.projectId === project.projectId
              );
              if (projectOrders.length > 0) {
                const order = projectOrders[0];
                this.orderChanged(order.invoiceId, order.invoiceNr, row);
                this.grid.refreshCells();
              }
            }
          } else {
            this.projectChanged(0, '', row);
          }
        }
        break;
      case 'customerInvoiceId':
        {
          const order = this.filteredOrdersDict.find(
            e => e.invoiceId == event.newValue
          );
          if (order && order.projectId && order.projectId !== row.projectId) {
            const project = this.filteredProjectsDict.find(
              e => e.projectId === order.projectId
            );
            if (project) {
              this.projectChanged(project.projectId, project.numberName, row);
            }
            this.grid.refreshCells();
          } else {
            this.orderChanged(0, '', row);
          }
        }
        break;
    }

    if (event.value) {
      //#region timePayrollQuantityFormatted|invoiceQuantityFormatted
      const field = event.colDef.field || '';
      const match = field.match(
        /(timePayrollQuantityFormatted|invoiceQuantityFormatted)_(\d+)/
      );

      if (match) {
        //ADD ROW
        const newValue = event.newValue ?? 0;
        const [type, dayStr] = [match[1], match[2]];
        const newRow = new ProjectTimeMatrixSaveRowDTO();

        newRow.weekDay = parseInt(dayStr);
        newRow.isModified = true;
        if (type === 'timePayrollQuantityFormatted') {
          newRow.payrollQuantity = newValue;
          newRow.invoiceQuantity = newValue;
        } else {
          newRow.invoiceQuantity = newValue;
        }

        this.form.markAsDirty();
        this.grid.refreshCells();
        event.data.rows.push(newRow);
        event.data.isModified = true;
      }
      // #endregion;
    }
  }

  private projectChanged(
    newProjectId: number,
    newProjectName: string,
    row: ProjectTimeMatrixDTO
  ) {
    if (newProjectId && newProjectId !== row.projectId) {
      row.isModified = true;
      row.rows?.forEach(r => {
        r.isModified = true;
      });
      this.form.markAsDirty();
      row.projectId = newProjectId;
      row.projectName = newProjectName;
    }
  }

  private orderChanged(
    customerInvoiceId: number,
    invoiceNr: string,
    row: ProjectTimeMatrixDTO
  ) {
    if (row.customerInvoiceId !== customerInvoiceId) {
      row.isModified = true;
      row.rows?.forEach(r => {
        r.isModified = true;
      });
      this.form.markAsDirty();
    }
    row.customerInvoiceId = customerInvoiceId;
    row.invoiceNr = invoiceNr;
  }

  selectionChanged(selectedRows: ProjectTimeMatrixDTO[]) {
    super.selectionChanged(selectedRows);
    this.isDeleteDisable.set(selectedRows.length < 1 ? false : true);
  }

  override onGridReadyToDefine(grid: GridComponent<ProjectTimeMatrixDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellEditingStopped: this.onCellEdit.bind(this),
    });

    const weekdays = <ISmallGenericType[]>DateUtil.getDayOfWeekNames(true);
    if (weekdays[0].id === DayOfWeek.Sunday) {
      const sunday = weekdays.shift();
      if (sunday)
        weekdays.push(<ISmallGenericType>{ id: 7, name: sunday.name });
    }

    this.translate
      .get([
        'billing.project.timesheet.chargingtype',
        'billing.project.timesheet.invoice',
        'common.time.timedeviationcause',
        'billing.project.project',
        'billing.project.timesheet.workedtime',
        'billing.project.timesheet.invoicedtime',
        'common.customer.customer.customer',
        'common.total',
        'core.save',
        'common.active',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.saveLabel = terms['core.save'] + ' (Ctrl+S)';

        const commonHeader = this.grid.addColumnHeader('', '');
        this.grid.enableRowSelection(() => this.showSelection);
        this.grid.addColumnModified('isModified');
        this.grid.addColumnAutocomplete(
          'customerInvoiceId',
          terms['billing.project.timesheet.invoice'],
          {
            flex: 1,
            source: () => this.filteredOrdersDict,
            optionIdField: 'invoiceId',
            optionNameField: 'numberName',
            optionDisplayNameField: 'invoiceNr',
            enableHiding: false,
            editable: row => this.isOrderEditable(row.data),
            headerColumnDef: commonHeader,
          }
        );

        this.grid.addColumnAutocomplete(
          'projectId',
          terms['billing.project.project'],
          {
            flex: 1,
            source: () => this.filteredProjectsDict,
            optionIdField: 'projectId',
            optionNameField: 'name',
            optionDisplayNameField: 'projectName',
            enableHiding: false,
            editable: row => this.isOrderEditable(row.data),
            headerColumnDef: commonHeader,
          }
        );

        this.grid.addColumnText(
          'customerName',
          terms['common.customer.customer.customer'],
          {
            flex: 1,
            editable: false,
            headerColumnDef: commonHeader,
            enableHiding: false,
          }
        );

        if (this.useExtendedTimeRegistration) {
          this.grid.addColumnSelect(
            'timeDeviationCauseId',
            terms['common.time.timedeviationcause'],
            this.timeDeviationCauseDict,
            null,
            {
              flex: 1,
              editable: true,
              enableHiding: false,
              headerColumnDef: commonHeader,
            }
          );
        }

        this.grid.addColumnSelect(
          'timeCodeId',
          terms['billing.project.timesheet.chargingtype'],
          this.timeCodeDicts,
          undefined,
          {
            flex: 1,
            editable: true,
            enableHiding: false,
            headerColumnDef: commonHeader,
          }
        );

        weekdays.forEach(day => {
          const columnSuffix = day.id.toString();
          const colHeaderDay = this.grid.addColumnHeader('', day.name);
          colHeaderDay.marryChildren = true;

          if (this.workTimePermission) {
            this.grid.addColumnTimeSpan(
              'timePayrollQuantityFormatted_' + columnSuffix,
              terms['billing.project.timesheet.workedtime'],
              {
                flex: 1,
                aggFuncOnGrouping: 'sum',
                enableHiding: false,
                editable: true,
                clearZero: true,
                padHours: true,
                headerColumnDef: colHeaderDay,
                filter: true,
                cellClassRules: {
                  'time-border': () => true,
                  // 'border-right-color': '"#F0F0F0"',
                  // 'border-right-width': '"5px"',
                },
              }
            );
          }
          if (this.invoiceTimePermission) {
            this.grid.addColumnTimeSpan(
              'invoiceQuantityFormatted_' + columnSuffix,
              terms['billing.project.timesheet.invoicedtime'],
              {
                flex: 1,
                enableHiding: false,
                aggFuncOnGrouping: 'sum',
                headerColumnDef: colHeaderDay,
                clearZero: true,
                padHours: true,
                editable: true,
              }
            );
          }

          this.grid.addColumnIcon('noteIcon_' + columnSuffix, '', {
            flex: 1,
            enableHiding: false,
            headerColumnDef: colHeaderDay,
            onClick: this.showNote.bind(this, day.id),
          });
        });

        // #region total
        const colHeaderTotal = this.grid.addColumnHeader(
          'total',
          terms['common.total']
        );

        if (this.workTimePermission) {
          this.grid.addColumnTimeSpan(
            'timePayrollQuantityFormatted_Total',
            terms['billing.project.timesheet.workedtime'],
            {
              flex: 1,
              enableHiding: false,
              aggFuncOnGrouping: 'sum',
              headerColumnDef: colHeaderTotal,
              valueGetter: (params: any) => {
                const minutes = this.getTimePayrollQuantity_Total(
                  params.data.rows
                );
                return minutes;
              },
            }
          );
        }

        if (this.invoiceTimePermission) {
          this.grid.addColumnTimeSpan(
            'invoiceQuantityFormatted_Total',
            terms['billing.project.timesheet.invoicedtime'],
            {
              flex: 1,
              enableHiding: false,
              aggFuncOnGrouping: 'sum',
              headerColumnDef: colHeaderTotal,
              valueGetter: (params: any) => {
                const minutes = this.getInvoiceQuantity_Total(params.data.rows);
                return minutes;
              },
            }
          );
        }
        // #endregion

        this.grid.useGrouping({
          stickyGrandTotalRow: 'bottom',
          hideGroupPanel: true,
        });

        this.grid.addGroupTimeSpanSumAggFunction(true);

        super.finalizeInitGrid();
      });
  }

  private showNote(weekDay: number, row: ProjectTimeMatrixDTO) {
    const title = '';
    const projectTimeBlock: any = {};

    const selectedRow = row.rows.find(r => r.weekDay === weekDay);
    if (selectedRow) {
      projectTimeBlock.externalNote = selectedRow.externalNote;
      projectTimeBlock.internalNote = selectedRow.internalNote;

      projectTimeBlock.invoiceQuantityFormatted =
        this.getInvoiceQuantityFormatted(selectedRow);
      projectTimeBlock.timePayrollQuantityFormatted =
        this.getTimePayrollQuantityFormatted(selectedRow);
    }

    projectTimeBlock.date = this.timeProjectFrom.addDays(weekDay - 1);
    projectTimeBlock.isEditable = true;

    const employee = this.employeesDict.find(
      e => e.id == this.selectedEmployeeId
    );
    projectTimeBlock.employeeName = employee ? employee.name : '';

    const dialogData: any = {
      title: title,
      size: 'lg',
      row: projectTimeBlock,
      rows: [projectTimeBlock],
      isReadonly: false,
      saveDirect: false,
      workTimePermission: this.workTimePermission,
      invoiceTimePermission: this.invoiceTimePermission,
    };

    this.dialogService
      .open(EditNoteDialogComponent, dialogData)
      .afterClosed()
      .pipe()
      .subscribe(result => {
        if (result) {
          const internalExternalNoteRow = row.rows.find(
            r => r.weekDay === weekDay
          );

          if (internalExternalNoteRow) {
            internalExternalNoteRow.internalNote = result.internalNote;
            internalExternalNoteRow.externalNote = result.externalNote;
            this.noteIcon(internalExternalNoteRow, row);
            internalExternalNoteRow.isModified = true;
          }

          row.isModified = true;
          this.grid.refreshCells();
          this.form.markAsDirty();
        }
      });
  }

  public getInvoiceQuantityFormatted(
    row: IProjectTimeMatrixSaveRowDTO
  ): string {
    const value = row ? row.invoiceQuantity : 0;
    if (!value) return '';
    return DateUtil.minutesToTimeSpan(value);
  }

  public getTimePayrollQuantityFormatted(
    row: IProjectTimeMatrixSaveRowDTO
  ): string {
    const value = row ? row.payrollQuantity : 0;
    if (!value) return '';

    return DateUtil.minutesToTimeSpan(value);
  }

  override loadCompanySettings() {
    return this.coreService
      .getCompanySettings([
        CompanySettingType.ProjectUseExtendedTimeRegistration,
        CompanySettingType.ProjectInvoiceTimeAsWorkTime,
      ])
      .pipe(
        tap(settings => {
          this.useExtendedTimeRegistration =
            settings[CompanySettingType.ProjectUseExtendedTimeRegistration];
        })
      );
  }

  override loadUserSettings() {
    return this.coreService
      .getUserSettings([UserSettingType.TimeSheetShowWeekend])
      .pipe(
        tap(settings => {
          this._showWeekend.set(settings[UserSettingType.TimeSheetShowWeekend]);
        })
      );
  }

  //KEEP THIS HERE
  private loadEmployee() {
    this.timeProjectFrom = new Date().beginningOfWeek();
    this.timeProjectTo = this.timeProjectFrom.endOfWeek();

    return this.projectService
      .getEmployeeForUserWithTimeCode(
        DateUtil.getISODateString(this.timeProjectFrom)
      )
      .pipe(
        tap(x => {
          this.employee = x;
          this.loadEmployees().subscribe();
          this.loadTimeDeviationCausesForEmployee(
            this.employee.employeeGroupId
          ).subscribe();
          return x;
        })
      );
  }

  //KEEP THIS HERE
  private loadEmployees() {
    this.employees = [];
    this.employeesDict.length = 0;

    if (this.modifyOtherEmployeesPermission) {
      const model: IGetProjectEmployeesModel = {
        addEmptyRow: false,
        getHidden: false,
        addNoReplacementEmployee: false,
        includeEmployeeId: this.includeEmployeeId,
        fromDateString: DateUtil.getISODateString(this.timeProjectFrom),
        toDateString: DateUtil.getISODateString(this.timeProjectTo),
        employeeCategories: [],
      };

      return this.projectService.getEmployeesForProjectTimeCode(model).pipe(
        tap(x => {
          this.employees = x;
          this.employeesDict.length = 0;
          x.forEach(e => {
            this.employeesDict.push({
              id: e.employeeId,
              name: e.name + ' (' + e.employeeNr + ')',
            });
          });
        })
      );
    } else {
      this.employees.push(this.employee);
      return of([]);
    }
  }

  protected save() {
    const model: IProjectTimeMatrixSaveDTO[] = this.grid
      .getAllRows()
      .filter(row => row.isModified)
      .map(row => {
        return {
          employeeId: row.employeeId,
          timeDeviationCauseId: row.timeDeviationCauseId,
          projectId: row.projectId,
          customerInvoiceId: row.customerInvoiceId,
          timeCodeId: row.timeCodeId,
          weekDate: this.timeProjectFrom,
          projectInvoiceWeekId: row.projectInvoiceWeekId ?? 0,
          isDeleted: row.isDeleted,
          rows: row.rows.filter(r => r.isModified),
        };
      });

    this.performLoadData.crud(
      CrudActionTypeEnum.Save,
      this.projectWeekReportService.save(model).pipe(
        tap((res: any) => {
          if (res.success) {
            this.searchGridData();
            this.form.markAsPristine();
          }
        })
      )
    );
  }

  protected onWeekendClick(event: boolean) {
    this.toggleWeekendColumns(false, event);
  }

  private checkWeekends() {
    const showWeekend = this.hasWeekendTimes();
    this.toggleWeekendColumns(false, showWeekend);
    this.showWeekend.set(showWeekend);
  }

  private toggleWeekendColumns(saveSetting: boolean, showWeekend: boolean) {
    if (showWeekend) {
      this.grid.showColumns([`invoiceQuantityFormatted_${DayOfWeek.Saturday}`]);
      this.grid.showColumns([`invoiceQuantityFormatted_${DayOfWeek.Sunday}`]);
      this.grid.showColumns([
        `timePayrollQuantityFormatted_${DayOfWeek.Saturday}`,
      ]);
      this.grid.showColumns([
        `timePayrollQuantityFormatted_${DayOfWeek.Sunday}`,
      ]);
      this.grid.showColumns([`noteIcon_${DayOfWeek.Saturday}`]);
      this.grid.showColumns([`noteIcon_${DayOfWeek.Sunday}`]);
    } else {
      this.grid.hideColumns([`invoiceQuantityFormatted_${DayOfWeek.Saturday}`]);
      this.grid.hideColumns([`invoiceQuantityFormatted_${DayOfWeek.Sunday}`]);
      this.grid.hideColumns([
        `timePayrollQuantityFormatted_${DayOfWeek.Saturday}`,
      ]);
      this.grid.hideColumns([
        `timePayrollQuantityFormatted_${DayOfWeek.Sunday}`,
      ]);
      this.grid.hideColumns([`noteIcon_${DayOfWeek.Saturday}`]);
      this.grid.hideColumns([`noteIcon_${DayOfWeek.Sunday}`]);
    }

    if (saveSetting) {
      const model = new SaveUserCompanySettingModel(
        SettingMainType.User,
        UserSettingType.ProjectDefaultExcludeMissingCustomer,
        showWeekend
      );

      this.coreService.saveBoolSetting(model).pipe(
        tap(result => {
          if (!result.success) {
            console.log('Error when saving setting', result);
          }
        })
      );
    }
  }

  private hasWeekendTimes(): boolean {
    let weekendTimes: ProjectTimeMatrixDTO | null = null;

    if (this.grid.getAllRows().length > 0) {
      weekendTimes =
        this.grid
          .getAllRows()
          .find(
            row =>
              row[`invoiceQuantityFormatted_${DayOfWeek.Saturday}`] > 0 ||
              row[`invoiceQuantityFormatted_${DayOfWeek.Sunday}`] > 0
          ) || null;
    }
    return weekendTimes ? true : false;
  }
}
