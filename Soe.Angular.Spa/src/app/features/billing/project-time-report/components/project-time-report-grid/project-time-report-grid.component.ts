import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import {
  AttestPayrollTransactionDTO,
  EditTimeReportDialogDTO,
  IEmployeeTimeCodeDTO,
  ProjectTimeBlockDTO,
  ProjectTimeBlockSaveDTO,
} from '@features/billing/project-time-report/models/project-time-report.model';
import { ProjectTimeReportService } from '@features/billing/project-time-report/services/project-time-report.service';
import { ProjectEditComponent } from '@features/billing/project/components/project-edit/project-edit.component';
import { ProjectForm } from '@features/billing/project/models/project-form.model';
import { AttestStateDTO } from '@shared/components/billing/purchase-customer-invoice-rows/models/purchase-customer-invoice-rows.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { CustomerInvoiceGridDTO } from '@shared/features/customer-central/models/customer-central.model';
import { CustomerEditComponent } from '@shared/features/customer/components/customer-edit/customer-edit.component';
import { CustomerForm } from '@shared/features/customer/models/customer-form.model';
import { IAttestPayrollTransactionDTO } from '@shared/models/generated-interfaces/AttestDTO';
import {
  IGetProjectTimeBlocksForTimesheetModel,
  IMoveProjectTimeBlocksToDateModel,
  IMoveProjectTimeBlocksToOrderModel,
} from '@shared/models/generated-interfaces/CoreModels';
import {
  CompanySettingType,
  Feature,
  SoeCategoryType,
  SoeOriginType,
  TermGroup_AttestEntity,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IEmployeeProjectInvoiceDTO,
  IProjectInvoiceSmallDTO,
  IProjectSmallDTO,
} from '@shared/models/generated-interfaces/ProjectDTOs';
import {
  ICategoryDTO,
  IEmployeeScheduleTransactionInfoDTO,
  IProjectTimeBlockSaveDTO,
  ITimeDeviationCauseDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import {
  ISaveAttestForTransactionsModel,
  ISaveAttestForTransactionsValidationModel,
} from '@shared/models/generated-interfaces/TimeModels';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import {
  TimeProjectButtonFunctions,
  TimeProjectContainer,
} from '@shared/util/Enumerations';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, forkJoin, Observable, of, take, tap } from 'rxjs';
import { ProjectTimeReportEditDialogComponent } from '../project-time-report-edit/project-time-report-edit.dialog/project-time-report-edit.dialog.component';
import { EditNoteDialogComponent } from './edit-note-dialog/edit-note-dialog.component';
import { ProjectCentralDataService } from '@features/billing/project-central/services/project-central-data.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import {
  ICustomerInvoiceSearchResultDTO,
  SelectInvoiceDialogDTO,
} from '@shared/components/select-customer-invoice-dialog/model/customer-invoice-search.model';
import { SelectCustomerInvoiceDialogComponent } from '@shared/components/select-customer-invoice-dialog/component/select-customer-invoice-dialog/select-customer-invoice-dialog.component';
import { IGetProjectEmployeesModel } from '@shared/models/generated-interfaces/BillingModels';

@Component({
  selector: 'soe-project-time-report-grid',
  templateUrl: './project-time-report-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProjectTimeReportGridComponent
  extends GridBaseDirective<ProjectTimeBlockDTO, ProjectTimeReportService>
  implements OnInit
{
  service = inject(ProjectTimeReportService);
  private readonly coreService = inject(CoreService);
  dialogService = inject(DialogService);
  messageboxService = inject(MessageboxService);
  projectCentralDataService = inject(ProjectCentralDataService);
  private destroyRef = inject(DestroyRef);

  showMigrateButton = false;

  private defaultTimeCodeId = 0;

  usedPayrollSince!: Date;
  usePayroll = false;
  isReadOnly = false;

  //project central...
  private selectedProjectId!: number;

  // Permissions
  private editProjectPermission = false;
  private invoiceTimePermission = false;
  private workTimePermission = false;
  private modifyOtherEmployeesPermission = false;
  private isProjectParticipant = true;
  private editOrderPermission = false;
  private editCustomerPermission = false;
  public splitTimeProductRowsPermission = signal(false);

  //settings
  useExtendedTimeRegistration = false;
  useProjectTimeBlocks = false;
  invoiceTimeAsWorkTime = false;

  projectContainer = TimeProjectContainer.TimeSheet;
  employeeDaysWithSchedule!: IEmployeeScheduleTransactionInfoDTO[];

  selectedTimeSheetRows = new BehaviorSubject<ProjectTimeBlockDTO[]>([]);

  userValidPayrollAttestStates: AttestStateDTO[] = [];
  searchObject!: IGetProjectTimeBlocksForTimesheetModel;

  menuList: MenuButtonItem[] = [];
  employee!: IEmployeeTimeCodeDTO;

  //Passing Data
  ordersDict: ISmallGenericType[] = [];
  projectsDict: ISmallGenericType[] = [];
  employeeCategoryDict: ISmallGenericType[] = [];
  timeDeviationCauseDict: ISmallGenericType[] = [];
  employeesDict: ISmallGenericType[] = [];

  employees: IEmployeeTimeCodeDTO[] = [];
  timeDeviationCauses: ITimeDeviationCauseDTO[] = [];
  timeDeviationCauseDictForEdit: ISmallGenericType[] = [];

  employeeId = SoeConfigUtil.employeeId;
  projects = new BehaviorSubject<IProjectSmallDTO[]>([]);
  orders = new BehaviorSubject<IProjectInvoiceSmallDTO[]>([]);
  protected disableButtonFunction = true;
  protected isRowNotSelected = signal(true);
  protected groupByDate = signal(false);
  public showOtherTime = signal(false);
  private timeProjectFrom = signal(
    DateUtil.getISODateString(DateUtil.getDateFirstInWeek(new Date()))
  );
  private timeProjectTo = signal(
    DateUtil.getISODateString(DateUtil.getDateLastInWeek(new Date()))
  );
  private readonly perform = new Perform<any>(this.progressService);

  projectInvoices!: IEmployeeProjectInvoiceDTO[];
  private rows: ProjectTimeBlockDTO[] = [];
  // Properties
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

  public get showFooter(): boolean {
    return this.isOrder;
  }

  ngOnInit(): void {
    super.ngOnInit();

    this.projectContainer =
      this.additionalGridProps()?.projectContainer ||
      TimeProjectContainer.TimeSheet;

    if (!this.isProjectCentral) {
      this.doStartFlow();
    }
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideReload: true,
    });
  }

  override onTabActivated(): void {
    if (!this.gridIsDefined) {
      this.doStartFlow();
    }
    if (this.isProjectCentral) {
      this.getProjectCentralData().subscribe();
    }
  }

  private doStartFlow() {
    this.startFlow(Feature.None, 'Common.Directives.TimeProjectReport', {
      additionalReadPermissions: [
        Feature.Time_Project_Invoice_WorkedTime,
        Feature.Time_Project_Invoice_InvoicedTime,
        Feature.Time_Project_Invoice_ShowAllPersons,
      ],
      lookups: [
        this.loadAttestStates(),
        this.loadEmployee(),
        this.loadModifyPermissions(),
        this.loadReadOnlyPermissions(),
      ],
    });
  }

  private getProjectCentralData() {
    return this.projectCentralDataService.projectCentralData$.pipe(
      takeUntilDestroyed(this.destroyRef),
      tap(data => {
        if (this.selectedProjectId !== data.projectId) {
          this.selectedProjectId = data.projectId || 0;
          if (this.grid) {
            this.grid.setData([]);
          }
        }
        if (data.fromDate) {
          this.timeProjectFrom.set(DateUtil.getISODateString(data.fromDate));
        }
        if (data.toDate) {
          this.timeProjectTo.set(DateUtil.getISODateString(data.toDate));
        }
      })
    );
  }

  private loadModifyPermissions(): Observable<Record<number, boolean>> {
    const featureIds: number[] = [
      Feature.Billing_Project_Edit,
      Feature.Billing_Project_TimeSheetUser_OtherEmployees,
      Feature.Time_Time_TimeSheetUser_OtherEmployees,
      Feature.Billing_Order_Orders_Edit,
      Feature.Billing_Customer_Customers_Edit,
      Feature.Billing_Order_Orders,
      Feature.Billing_Order_OrdersAll,
      Feature.Billing_Order_OrdersUser,
    ];

    if (this.isProjectCentral) {
      featureIds.push(Feature.Billing_Project_Central_TimeSheetUser);
    }

    if (this.isOrder) {
      featureIds.push(Feature.Billing_Order_Orders_Edit_Splitt_TimeRows);
    }

    return this.coreService.hasModifyPermissions(featureIds).pipe(
      tap(x => {
        this.editProjectPermission = x[Feature.Billing_Project_Edit];
        this.editOrderPermission = x[Feature.Billing_Order_Orders_Edit];
        this.editCustomerPermission =
          x[Feature.Billing_Customer_Customers_Edit];
        this.modifyOtherEmployeesPermission =
          this.isTimeSheet || this.isProjectCentral
            ? x[Feature.Billing_Project_TimeSheetUser_OtherEmployees] ||
              x[Feature.Time_Time_TimeSheetUser_OtherEmployees]
            : true;

        if (this.isProjectCentral) {
          this.isReadOnly =
            !this.modifyOtherEmployeesPermission &&
            x[Feature.Billing_Project_Central_TimeSheetUser];
        }

        if (this.isOrder) {
          this.splitTimeProductRowsPermission.set(
            x[Feature.Billing_Order_Orders_Edit_Splitt_TimeRows]
          );
        }
      })
    );
  }

  private loadReadOnlyPermissions(): Observable<Record<number, boolean>> {
    const featureIds: number[] = [];
    featureIds.push(Feature.Time_Project_Invoice_WorkedTime);
    featureIds.push(Feature.Time_Project_Invoice_InvoicedTime);
    featureIds.push(Feature.Time_Project_Invoice_ShowAllPersons);

    return this.coreService.hasReadOnlyPermissions(featureIds).pipe(
      tap(x => {
        this.workTimePermission = x[Feature.Time_Project_Invoice_WorkedTime];
        this.invoiceTimePermission =
          x[Feature.Time_Project_Invoice_InvoicedTime];
      })
    );
  }

  override selectionChanged(event: any) {
    this.isRowNotSelected.set(this.grid.getSelectedCount() > 0 ? false : true);

    this.updateButton();

    this.selectedTimeSheetRows.next(this.grid.getSelectedRows());
  }

  updateButton() {
    if (!this.groupByDate() && !this.isRowNotSelected())
      this.disableButtonFunction = false;
    else this.disableButtonFunction = true;
  }

  override onFinished(): void {
    this.showMigrateButton =
      SoeConfigUtil.isSupportAdmin &&
      this.useProjectTimeBlocks &&
      this.usePayroll;
  }

  loadAll() {
    return forkJoin([
      this.loadEmployees(),
      this.loadProjectsOrder(),
      this.loadTimeDeviationCauses(),
    ]);
  }

  loadEmployee(): Observable<IEmployeeTimeCodeDTO> {
    return this.perform.load$(
      this.service.getEmployeeForUserWithTimeCode(this.timeProjectFrom()).pipe(
        tap(empl => {
          this.employee = empl;
        })
      )
    );
  }

  loadCategories() {
    return this.perform.load(
      this.coreService
        .getCategoriesGrid(SoeCategoryType.Employee, false, false, false)
        .pipe(
          tap(data => {
            this.employeeCategoryDict = data.map((category: ICategoryDTO) => ({
              id: category.categoryId,
              name: category.name,
            }));
          })
        )
    );
  }

  loadTimeDeviationCausesWithProgress() {
    return this.perform.load(this.loadTimeDeviationCauses());
  }

  loadTimeDeviationCauses(employeeGroupId = 0) {
    if (this.timeDeviationCauseDict.length > 0) {
      return of(this.timeDeviationCauses);
    }

    return this.service
      .getTimeDeviationCauses(employeeGroupId, false, true)
      .pipe(
        tap(deviationCauses => {
          this.timeDeviationCauses = deviationCauses;

          this.timeDeviationCauseDict = deviationCauses.map(
            (timeDeviationCause: ITimeDeviationCauseDTO) => ({
              id: timeDeviationCause.timeDeviationCauseId,
              name: timeDeviationCause.name,
            })
          );

          deviationCauses.forEach(c => {
            if (c.calculateAsOtherTimeInSales) {
              this.showOtherTime.set(true);
            }
          });
        })
      );
  }

  private loadAttestStates() {
    this.userValidPayrollAttestStates = [];

    return this.coreService
      .getUserValidAttestStates(
        TermGroup_AttestEntity.PayrollTime,
        this.timeProjectFrom(),
        this.timeProjectTo(),
        true,
        this.employee?.employeeGroupId || 1
      )
      .pipe(
        tap(result => {
          this.userValidPayrollAttestStates = result;

          result.forEach((attestState: AttestStateDTO) => {
            this.menuList.push({
              id: attestState.attestStateId,
              label: attestState.name,
            });
          });
        })
      );
  }

  private getAttestState(attestStateId?: number): AttestStateDTO | undefined {
    return this.userValidPayrollAttestStates.find(
      x => x.attestStateId === attestStateId
    );
  }

  loadEmployeeswithProgess(categoriesIds: number[]) {
    return this.perform.load(this.loadEmployees(categoriesIds));
  }

  private loadEmployees(
    categoriesIds: number[] = []
  ): Observable<IEmployeeTimeCodeDTO[]> {
    if (this.employees.length > 0 && categoriesIds.length === 0) {
      return of(this.employees);
    }

    if (this.selectedProjectId) {
      if (this.modifyOtherEmployeesPermission) {
        return this.service
          .getEmployeesForTimeProjectRegistrationSmall(
            this.selectedProjectId,
            this.timeProjectFrom(),
            this.timeProjectTo()
          )
          .pipe(
            tap((x: IEmployeeTimeCodeDTO[]) => {
              this.employees = x;
              x.forEach(e => {
                this.employeesDict.push({ id: e.employeeId, name: e.name });
              });
            })
          );
      } else {
        if (this.employee) {
          this.employees.push({
            employeeId: this.employee.employeeId,
            name: this.employee.name,
            employeeNr: this.employee.employeeNr,
            defaultTimeCodeId: this.employee.defaultTimeCodeId,
            timeDeviationCauseId: this.employee.timeDeviationCauseId,
            employeeGroupId: this.employee.employeeGroupId,
            autoGenTimeAndBreakForProject:
              this.employee.autoGenTimeAndBreakForProject,
          });
          this.employeesDict.push({
            id: this.employee.employeeId,
            name: this.employee.name,
          });
        }
        return of(this.employees);
      }
    } else {
      if (this.modifyOtherEmployeesPermission) {
        const getEmployeesModel: IGetProjectEmployeesModel = {
          employeeCategories: categoriesIds,
          fromDateString: this.timeProjectFrom(),
          toDateString: this.timeProjectTo(),
          addEmptyRow: false,
          getHidden: false,
          addNoReplacementEmployee: false,
        };
        return this.service
          .getEmployeesForProjectTimeCode(getEmployeesModel)
          .pipe(
            tap((x: IEmployeeTimeCodeDTO[]) => {
              this.employees = x;
              this.employeesDict = [];
              x.forEach(e => {
                this.employeesDict.push({
                  id: e.employeeId,
                  name: e.name + ' (' + e.employeeNr + ')',
                });
              });
            })
          );
      } else {
        if (this.employee) {
          this.employeesDict = [];
          this.employees.push({
            employeeId: this.employee.employeeId,
            name: this.employee.name,
            employeeNr: this.employee.employeeNr,
            defaultTimeCodeId: this.employee.defaultTimeCodeId,
            timeDeviationCauseId: this.employee.timeDeviationCauseId,
            employeeGroupId: this.employee.employeeGroupId,
            autoGenTimeAndBreakForProject:
              this.employee.autoGenTimeAndBreakForProject,
          });
          this.employeesDict.push({
            id: this.employee.employeeId,
            name: this.employee.name,
          });
        }
        return of(this.employees);
      }
    }
  }

  saveAttestState(event: MenuButtonItem): any {
    const attestStateTo = this.getAttestState(event.id);
    if (!attestStateTo) return;

    const transactionItems: AttestPayrollTransactionDTO[] = [];

    this.grid.getSelectedRows().forEach((row: ProjectTimeBlockDTO) => {
      row.timePayrollTransactionIds.forEach(
        (timePayrollTransactionId: number) => {
          const transactionItem: IAttestPayrollTransactionDTO =
            new AttestPayrollTransactionDTO();
          transactionItem.employeeId = row.employeeId;
          transactionItem.timePayrollTransactionId = timePayrollTransactionId;
          transactionItem.attestStateId = row.timePayrollAttestStateId;
          transactionItem.date = row.date;
          transactionItem.isScheduleTransaction = false;
          transactionItem.isExported = false;
          transactionItem.isPreliminary = false;
          transactionItems.push(transactionItem);
        }
      );
    });

    const model: ISaveAttestForTransactionsValidationModel = {
      items: transactionItems,
      attestStateToId: attestStateTo.attestStateId,
      isMySelf: false,
    };

    this.perform.crud(
      CrudActionTypeEnum.Save,
      this.service.saveAttestForTransactionsValidation(model).pipe(
        tap(response => {
          this.saveTransaction(response, event.id);
        })
      ),
      undefined
    );
  }

  saveTransaction(res: any, attestId?: number) {
    if (res.success) {
      const model: ISaveAttestForTransactionsModel = {
        items: res.validItems,
        attestStateToId: attestId || 0,
        isMySelf: false,
      };
      this.service
        .saveAttestForTransactions(model)
        .pipe(
          tap(result => {
            if (result.success) {
              this.doSearch(this.searchObject);
            }
          })
        )
        .subscribe();
    } else {
      this.messageboxService.error(res.title, res.message);
    }
  }

  recalculateWork() {
    const saveModel: IProjectTimeBlockSaveDTO[] = [];

    this.grid.getSelectedRows().forEach(row => {
      const model: any = {
        employeeId: row.employeeId,
        projectTimeBlockId: row.projectTimeBlockId,
        timeBlockDateId: row.timeBlockDateId,
      };
      saveModel.push(model);
    });

    this.perform.crud(
      CrudActionTypeEnum.Save,
      this.service.recalculateWorkTime(saveModel),
      response => {
        if (response.success) {
          this.refreshGrid();
        }
      },
      undefined
    );
  }

  runMigrateTimesJob() {
    this.coreService.getCompany(SoeConfigUtil.actorCompanyId).pipe(
      tap(company => {
        let message =
          this.terms['common.licensename'] + ': ' + company.licenseNr + '</br>';
        message +=
          this.terms['common.company'] +
          ': ' +
          company.number +
          ' - ' +
          company.name +
          '</br>';
        message +=
          this.terms['billing.project.timesheet.payrollactivatedfrom'] +
          ': ' +
          this.usedPayrollSince.toLocaleDateString() +
          '</br></br>';
        message += this.terms['billing.project.timesheet.migratewarning'];
        this.messageboxService.warning(
          'billing.project.timesheet.converttimes',
          message
        );
      })
    );
  }

  override loadCompanySettings(): Observable<void> {
    const settingTypes: number[] = [
      CompanySettingType.ProjectCreateInvoiceRowFromTransaction,
      CompanySettingType.ProjectLimitOrderToProjectUsers,
      CompanySettingType.TimeDefaultTimeCode,
      CompanySettingType.BillingDefaultTimeProjectReportTemplate,
      CompanySettingType.BillingStatusTransferredOrderToInvoice,
      CompanySettingType.ProjectUseExtendedTimeRegistration,
      CompanySettingType.ProjectCreateTransactionsBaseOnTimeRules,
      CompanySettingType.UseProjectTimeBlocks,
      CompanySettingType.UsePayroll,
      CompanySettingType.ProjectInvoiceTimeAsWorkTime,
      CompanySettingType.UsedPayrollSince,
    ];

    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap((settings: any) => {
        this.useExtendedTimeRegistration =
          settings[CompanySettingType.ProjectUseExtendedTimeRegistration];

        this.useProjectTimeBlocks =
          settings[CompanySettingType.UseProjectTimeBlocks];

        this.usePayroll = settings[CompanySettingType.UsePayroll];
        this.invoiceTimeAsWorkTime =
          settings[CompanySettingType.ProjectInvoiceTimeAsWorkTime];

        this.usedPayrollSince = settings[CompanySettingType.UsedPayrollSince];
        this.defaultTimeCodeId =
          settings[CompanySettingType.TimeDefaultTimeCode];
      })
    );
  }

  private showNote(row: ProjectTimeBlockDTO) {
    const title = '';

    const dialogData: any = {
      title: title,
      size:
        this.isTimeSheet ||
        this.isProjectCentral ||
        this.useExtendedTimeRegistration
          ? 'xl'
          : 'lg',
      row: row,
      rows: this.rows,
      saveDirect: true,
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
          this.doSearch(this.searchObject);
        }
      });
  }

  private openOrder(row: CustomerInvoiceGridDTO) {
    BrowserUtil.openInNewTab(
      window,
      `/soe/billing/order/status/default.aspx?invoiceId=${row.customerInvoiceId}&invoiceNr=${row.invoiceNr}`
    );
  }

  getGroupByDate(isGroupByDate: boolean) {
    this.groupByDate.set(isGroupByDate);

    this.updateButton();
  }

  getDateRange(dateRange: any[]) {
    this.timeProjectFrom.set(dateRange[0]);
    this.timeProjectTo.set(dateRange[1]);
  }

  doSearch(searchDto: IGetProjectTimeBlocksForTimesheetModel) {
    this.searchObject = searchDto;
    if (
      (this.selectedProjectId && !this.searchObject.projects) ||
      this.searchObject.projects.length === 0
    )
      this.searchObject.projects = [this.selectedProjectId];

    this.performSearch(searchDto);
    this.updateButton();
  }

  performSearch(searchDto: IGetProjectTimeBlocksForTimesheetModel) {
    console.log('performSearch', searchDto);
    this.perform.load(
      this.service.getTimeBlocksForTimeSheetFiltered(searchDto).pipe(
        tap(value => {
          value.forEach(obj => {
            obj.showOrderButton =
              obj.customerInvoiceId !== undefined &&
              obj.customerInvoiceId !== null &&
              obj.customerInvoiceId !== 0;
            obj.showCustomerButton =
              obj.customerId !== undefined &&
              obj.customerId !== null &&
              obj.customerId !== 0;
            obj.showProjectButton =
              obj.projectId !== undefined &&
              obj.projectId !== null &&
              obj.projectId !== 0;

            this.service.setNotesIcon(obj);
          });
          this.rows = value;
          this.rowData.next(this.rows);
        })
      )
    );
  }

  openEdit(row: ProjectTimeBlockDTO | undefined) {
    ///checkLookups
    this.perform.load(
      this.loadAll().pipe(
        tap(() => {
          this.performOpenEdit(row);
        })
      )
    );
  }

  performOpenEdit(row: ProjectTimeBlockDTO | undefined) {
    //set rows
    let rows: ProjectTimeBlockDTO[] = [];
    if (row) {
      rows = this.rows.filter(
        r =>
          r.employeeId === row.employeeId &&
          r.date.toString() === row.date.toString()
      );
    }

    const dialogData = new EditTimeReportDialogDTO();
    dialogData.title = this.translate.instant(
      'billing.project.timesheet.edittime.title'
    );

    dialogData.size = 'fullscreen';
    dialogData.bindToController = true;
    dialogData.isNew.set(row === undefined);
    dialogData.rows = rows;
    dialogData.invoiceTimePermission = this.invoiceTimePermission;
    dialogData.workTimePermission = this.workTimePermission;
    dialogData.isTimeSheet = this.isTimeSheet;
    dialogData.useExtendedTimeRegistration = this.useExtendedTimeRegistration;
    dialogData.invoiceTimeAsWorkTime = this.invoiceTimeAsWorkTime;
    dialogData.isProjectCentral = this.isProjectCentral;
    dialogData.defaultTimeCodeId = this.defaultTimeCodeId;

    dialogData.employeeId = row?.employeeId ?? 0;
    dialogData.employee = this.employee;
    dialogData.employeesDict = this.employeesDict;
    dialogData.employees = this.employees;
    dialogData.projects = this.projects;
    dialogData.orders = this.orders;
    dialogData.timeDeviationCauseDict = this.timeDeviationCauseDict;
    dialogData.timeDeviationCauses = this.timeDeviationCauses;

    this.dialogService
      .open(ProjectTimeReportEditDialogComponent, dialogData)
      .afterClosed()
      .pipe()
      .subscribe(rowData => {
        if (rowData) {
          this.doSearch(this.searchObject);
        }
      });
  }

  loadProjectsOrderWithProgress() {
    return this.perform.load(this.loadProjectsOrder());
  }
  loadProjectsOrder(
    selectedEmpIds: number[] = []
  ): Observable<IEmployeeProjectInvoiceDTO[]> {
    const empIds =
      selectedEmpIds.length > 0 ? selectedEmpIds : [this.employeeId];

    if (this.projectsDict.length > 0) {
      return of(this.projectInvoices);
    }

    console.log('loadProjectOrder');

    return this.service
      .getProjectsForTimeSheetEmployees(empIds, this.selectedProjectId)
      .pipe(
        tap(data => {
          if (data[0]) {
            data[0].projects.unshift({
              projectId: 0,
              name: '',
              numberName: '',
            } as IProjectSmallDTO);
            data[0].invoices.unshift({
              invoiceId: 0,
              name: '',
              numberName: '',
              projectId: 0,
              customerName: '',
              invoiceNr: '',
            } as IProjectInvoiceSmallDTO);
            this.projectInvoices = data;
            this.projects.next(data[0].projects);
            this.orders.next(data[0].invoices);
            //project
            this.projectsDict = data[0].projects.map(project => ({
              id: project.projectId,
              name: project.numberName,
            }));

            //order
            this.ordersDict = data[0].invoices.map(invoice => ({
              id: invoice.invoiceId,
              name: invoice.numberName,
            }));
          }
        })
      );
  }

  override onGridReadyToDefine(grid: GridComponent<ProjectTimeBlockDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'core.edit',
        'core.newrow',
        'core.deleterow',
        'core.donotshowagain',
        'core.warning',
        'common.date',
        'common.yearmonth',
        'common.yearweek',
        'common.weekday',
        'common.employee',
        'common.sum',
        'common.time.timedeviationcause',
        'billing.project.timesheet.chargingtype',
        'common.order',
        'common.customer.customer.customer',
        'billing.project.timesheet.invoice',
        'billing.project.project',
        'billing.project.timesheet.quantity.short',
        'billing.project.timesheet.invoicequantity.short',
        'billing.project.timesheet.totalquantity',
        'billing.project.timesheet.note',
        'billing.project.timesheet.note.edit',
        'billing.project.timesheet.note.editfor',
        'billing.project.timesheet.note.internal',
        'billing.project.timesheet.note.external',
        'billing.project.timesheet.wholeweek',
        'billing.project.timesheet.invoicedtime',
        'billing.project.timesheet.workedtime',
        'billing.project.timesheet.includetimeinreport.none',
        'billing.project.timesheet.includetimeinreport.all',
        'billing.project.timesheet.includetimeinreport.invoiced',
        'billing.project.timesheet.timesheet',
        'billing.project.timesheet.savebeforeedittimerow',
        'billing.project.timesheet.timerowsstatuschange',
        'billing.project.timesheet.employeenr',
        'billing.project.timesheet.scheduletime',
        'billing.project.timesheet.othertime',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
        'core.aggrid.totals.selected',
        'core.deleterowwarning',
        'billing.project.timesheet.employeeinactivated',
        'billing.project.projectnr',
        'billing.project.timesheet.asksaveorder',
        'billing.project.timesheet.ourreference',
        'billing.project.timesheet.internaltext',
        'time.employee.position.updatesyspositions',
        'time.employee.position.updateandlinksyspositions',
        'billing.project.timesheet.converttimes',
        'common.licensename',
        'common.company',
        'billing.project.timesheet.payrollactivatedfrom',
        'billing.project.timesheet.migratewarning',
        'common.dailyrecurrencepattern.startdate',
        'billing.project.timesheet.edittime.externalnote',
        'billing.project.timesheet.edittime.internalnote',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();

        this.grid.addColumnText(
          'employeeNr',
          terms['billing.project.timesheet.employeenr'],
          { flex: 1, enableHiding: true, enableGrouping: true }
        );
        this.grid.addColumnText('employeeName', terms['common.employee'], {
          flex: 1,
          tooltipField: 'columnNameTooltip',
          enableGrouping: true,
          cellClassRules: {
            errorRow: (row: any) =>
              row && row.data && row.data.employeeIsInactive,
          },
        });
        this.grid.addColumnDate('date', terms['common.date'], {
          flex: 1,
          tooltipField: 'dateFormatted',
          enableGrouping: true,
          enableHiding: true,
          cellClassRules: {
            excelDate: () => true,
          },
        });

        this.grid.addColumnText('weekDay', terms['common.weekday'], {
          flex: 1,
          enableHiding: true,
          enableGrouping: true,
        });

        this.grid.addColumnText('yearWeek', terms['common.yearweek'], {
          flex: 1,
          enableHiding: true,
          hide: true,
          enableGrouping: true,
        });

        if (this.useExtendedTimeRegistration) {
          this.grid.addColumnText(
            'timeDeviationCauseName',
            terms['common.time.timedeviationcause'],
            { flex: 1, enableHiding: true, enableGrouping: true }
          );
        }

        this.grid.addColumnText(
          'timeCodeName',
          terms['billing.project.timesheet.chargingtype'],
          { flex: 1, enableGrouping: true }
        );

        if (this.workTimePermission) {
          this.grid.addColumnTimeSpan(
            'timePayrollQuantity',
            terms['billing.project.timesheet.workedtime'],
            {
              enableHiding: true,
              aggFuncOnGrouping: 'sum',
              enableGrouping: true,
              flex: 1,
            }
          );
          this.grid.addColumnShape('timePayrollAttestStateName', '', {
            maxWidth: 40,
            shape: 'circle',
            tooltipField: 'timePayrollAttestStateName',
            colorField: 'timePayrollAttestStateColor',
            showShapeField: 'timePayrollAttestStateColor',
            showIcon: (data: any) =>
              !data ||
              (!data?.timePayrollAttestStateId && data?.timePayrollQuantity),
            iconClass: 'warningColor',
            iconPrefix: 'fas',
            iconName: 'exclamation',
          });
        }
        if (this.invoiceTimePermission) {
          this.grid.addColumnTimeSpan(
            'invoiceQuantity',
            terms['billing.project.timesheet.invoicedtime'],
            {
              enableHiding: true,
              enableGrouping: true,
              aggFuncOnGrouping: 'sum',
              flex: 1,
            }
          );
          this.grid.addColumnShape('customerInvoiceRowAttestStateName', '', {
            maxWidth: 40,
            shape: 'circle',
            colorField: 'customerInvoiceRowAttestStateColor',
            tooltipField: 'customerInvoiceRowAttestStateName',
            showShapeField: 'customerInvoiceRowAttestStateColor',
            showIcon: (data: any) =>
              !data ||
              (!data?.customerInvoiceRowAttestStateId && data?.invoiceQuantity),
            iconClass: 'warningColor',
            iconPrefix: 'fas',
            iconName: 'exclamation',
          });
        }

        this.grid.addColumnIcon('noteIcon', '', {
          onClick: row => this.showNote(row),
          suppressExport: true,
          // useIconFromField: true,
          // iconName: 'file',
        });

        if (this.isTimeSheet || this.isProjectCentral) {
          this.grid.addColumnText(
            'invoiceNr',
            terms['billing.project.timesheet.invoice'],
            {
              flex: 1,
              enableHiding: true,
              enableGrouping: true,
              buttonConfiguration: {
                iconPrefix: 'fal',
                iconName: 'pencil',
                onClick: row => this.openOrder(row),
                show: row => row?.showOrderButton && this.editOrderPermission,
              },
            }
          );
          this.grid.addColumnText(
            'customerName',
            terms['common.customer.customer.customer'],
            {
              flex: 1,
              enableHiding: true,
              enableGrouping: true,
              buttonConfiguration: {
                iconPrefix: 'fal',
                iconName: 'pen',
                show: row =>
                  row?.showCustomerButton && this.editCustomerPermission,
                onClick: r => {
                  this.edit(
                    {
                      ...r,
                      actorCustomerId: r.customerId,
                      name: r.customerNr,
                    },
                    {
                      filteredRows: [],
                      editComponent: CustomerEditComponent,
                      editTabLabel: 'common.customer.customer.customer',
                      FormClass: CustomerForm,
                    }
                  );
                },
              },
            }
          );
          this.grid.addColumnText(
            'projectNr',
            terms['billing.project.projectnr'],
            { flex: 1, enableHiding: true, hide: true, enableGrouping: true }
          );
          this.grid.addColumnText(
            'projectName',
            terms['billing.project.project'],
            {
              flex: 1,
              enableHiding: true,
              hide: true,
              enableGrouping: true,
              buttonConfiguration: {
                iconPrefix: 'fal',
                iconName: 'pencil',
                show: row =>
                  row?.showProjectButton && this.editProjectPermission,
                onClick: r => {
                  this.edit(
                    {
                      ...r,
                      projectId: r.projectId,
                      name: r.name,
                    },
                    {
                      filteredRows: [],
                      editComponent: ProjectEditComponent,
                      editTabLabel: 'billing.project.project',
                      FormClass: ProjectForm,
                    }
                  );
                },
              },
            }
          );

          this.grid.addColumnText(
            'referenceOur',
            terms['billing.project.timesheet.ourreference'],
            { flex: 1, enableHiding: true, hide: true, enableGrouping: true }
          );
          this.grid.addColumnText(
            'internOrderText',
            terms['billing.project.timesheet.internaltext'],
            { flex: 1, enableHiding: true, hide: true, enableGrouping: true }
          );
        }

        this.grid.addColumnText(
          'internalNote',
          terms['billing.project.timesheet.edittime.internalnote'],
          {
            flex: 1,
            enableHiding: true,
            hide: true,
            enableGrouping: true,
            tooltipField: 'internalNote',
          }
        );

        this.grid.addColumnText(
          'externalNote',
          terms['billing.project.timesheet.edittime.externalnote'],
          {
            flex: 1,
            enableHiding: true,
            hide: true,
            enableGrouping: true,
            tooltipField: 'externalNote',
          }
        );

        if (this.isTimeSheet || this.isProjectCentral) {
          this.grid.addColumnTimeSpan(
            'scheduledQuantityFormatted',
            terms['billing.project.timesheet.scheduletime'],
            {
              flex: 1,
              enableHiding: true,
              hide: true,
              enableGrouping: true,
              aggFuncOnGrouping: 'sumTimeSpan',
            }
          );
        }

        if (this.workTimePermission && this.showOtherTime()) {
          this.grid.addColumnTimeSpan(
            'timeOtherQuantityFormatted',
            terms['billing.project.timesheet.othertime'],
            {
              flex: 1,
              enableHiding: true,
              enableGrouping: true,
              alignLeft: false,
              aggFuncOnGrouping: 'sumTimeSpan',
              cellClassRules: {
                errorRow: (gridRow: any) =>
                  gridRow.data &&
                  gridRow.data.timePayrollQuantity <
                    gridRow.data.scheduledQuantity,
              },
            }
          );
        }

        if (this.isProjectParticipant && !this.isReadOnly) {
          this.grid.addColumnIconEdit({
            tooltip: terms['core.edit'],
            onClick: row => {
              this.openEdit(row);
            },
          });
        }
        this.grid.useGrouping({
          stickyGroupTotalRow: 'bottom',
          stickyGrandTotalRow: 'bottom',
          selectChildren: true,
          groupSelectsFiltered: true,
          totalTerm: terms['common.sum'],
        });

        this.grid.addGroupTimeSpanSumAggFunction(true);

        super.finalizeInitGrid();
      });
  }

  //#region Actions
  doActionEvent(eventId: number) {
    switch (eventId) {
      case TimeProjectButtonFunctions.AddRow:
        this.openEdit(undefined);
        break;
      case TimeProjectButtonFunctions.DeleteRow:
        this.deleteRows();
        break;
      case TimeProjectButtonFunctions.MoveRow:
        this.moveRows();
        break;
      case TimeProjectButtonFunctions.ChangeDate:
        this.changeDate();
        break;
      //for fure when migrating order...
      case TimeProjectButtonFunctions.MoveRowToNewInvoiceRow:
        //this.moveRowsToInvoiceRow();
        break;
      case TimeProjectButtonFunctions.MoveRowToExistingInvoiceRow:
        //this.moveRowsToExistingInvoiceRow();
        break;
    }
  }

  private getSelectedRowIds(): number[] {
    const ids: number[] = [];

    this.grid.getSelectedRows().forEach(row => {
      ids.push(row.projectTimeBlockId);
    });
    return ids;
  }

  private deleteRows(): void {
    this.messageboxService
      .warning('core.warning', 'core.deleterowwarning')
      .afterClosed()
      .subscribe((res: { result: boolean }): void => {
        if (res.result === true) {
          this.performDeleteRows();
        }
      });
  }

  private performDeleteRows() {
    const selectedRows = this.grid.getSelectedRows();
    const saveRows: ProjectTimeBlockSaveDTO[] = [];

    selectedRows.forEach(row => {
      row.isDeleted = true;
      const saveRow = new ProjectTimeBlockSaveDTO();
      saveRow.fromProjectTimeBlock(
        row,
        SoeConfigUtil.actorCompanyId,
        this.isTimeSheet,
        this.isProjectCentral
      );
      saveRows.push(saveRow);
    });

    this.perform.crud(
      CrudActionTypeEnum.Save,
      this.service.saveProjectTimeBlockSaveDTO(saveRows),
      response => {
        if (response.success) {
          this.doSearch(this.searchObject);
        }
      }
    );
  }

  private changeDate() {
    const mb = this.messageboxService.show(
      this.translate.instant('common.choosedate'),
      ' ',
      {
        showInputDate: true,
        inputDateLabel: 'common.date',
        inputDateValue: new Date(),
        buttons: 'okCancel',
        size: 'sm',
      }
    );

    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response.dateValue) {
        this.performChangeDate(response.dateValue);
      }
    });
  }

  private performChangeDate(newDate: Date) {
    const ids = this.getSelectedRowIds();

    const saveModel: IMoveProjectTimeBlocksToDateModel = {
      //selectedDate: date.toISOString(),
      selectedDate: DateUtil.getISODateString(newDate),
      projectTimeBlockIds: ids,
    };

    this.perform.crud(
      CrudActionTypeEnum.Save,
      this.service.moveTimeRowsToDate(saveModel),
      response => {
        if (response.success) {
          this.doSearch(this.searchObject);
        }
      }
    );
  }

  private moveRows() {
    const dialogData = new SelectInvoiceDialogDTO();
    dialogData.title = this.translate.instant('core.search');
    dialogData.size = 'lg';
    dialogData.originType = SoeOriginType.Order;
    this.dialogService
      .open(SelectCustomerInvoiceDialogComponent, dialogData)
      .afterClosed()
      .subscribe((selected: ICustomerInvoiceSearchResultDTO) => {
        if (selected) {
          this.performMoveRows(selected.customerInvoiceId);
        }
      });
  }

  private performMoveRows(invoiceId: number) {
    const ids = this.getSelectedRowIds();
    const saveModel: IMoveProjectTimeBlocksToOrderModel = {
      customerInvoiceId: invoiceId,
      customerInvoiceRowId: 0,
      projectTimeBlockIds: ids,
    };
    this.perform.crud(
      CrudActionTypeEnum.Save,
      this.service.moveTimeRowsToOrder(saveModel),
      response => {
        if (response.success) {
          this.doSearch(this.searchObject);
        }
      }
    );
  }

  //#endRegion Actions
}
