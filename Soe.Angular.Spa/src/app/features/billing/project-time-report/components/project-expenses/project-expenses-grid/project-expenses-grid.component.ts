import { Component, inject, OnInit, signal } from '@angular/core';
import {
  AttestPayrollTransactionDTO,
  ProjectTimeBlockDTO,
} from '@features/billing/project-time-report/models/project-time-report.model';
import { ProjectExpenseService } from '@features/billing/project-time-report/services/project-expense.service';
import { ProjectTimeReportService } from '@features/billing/project-time-report/services/project-time-report.service';
import { ProjectEditComponent } from '@features/billing/project/components/project-edit/project-edit.component';
import { ProjectForm } from '@features/billing/project/models/project-form.model';
import { AttestStateDTO } from '@shared/components/billing/purchase-customer-invoice-rows/models/purchase-customer-invoice-rows.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { CustomerEditComponent } from '@shared/features/customer/components/customer-edit/customer-edit.component';
import { CustomerForm } from '@shared/features/customer/models/customer-form.model';
import { IAttestPayrollTransactionDTO } from '@shared/models/generated-interfaces/AttestDTO';
import { IFilterExpensesModel } from '@shared/models/generated-interfaces/BillingModels';
import {
  Feature,
  SoeCategoryType,
  TermGroup_AttestEntity,
} from '@shared/models/generated-interfaces/Enumerations';
import { IExpenseRowGridDTO } from '@shared/models/generated-interfaces/ExpenseDTO';
import {
  ICategoryDTO,
  IEmployeeTimeCodeDTO,
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
import { TimeProjectContainer } from '@shared/util/Enumerations';
import { GridComponent } from '@ui/grid/grid.component';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellClassParams } from 'ag-grid-community';
import { BehaviorSubject, take, tap } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  IEmployeeProjectInvoiceDTO,
  IProjectInvoiceSmallDTO,
  IProjectSmallDTO,
} from '@shared/models/generated-interfaces/ProjectDTOs';

@Component({
  selector: 'soe-project-expenses-grid',
  templateUrl: './project-expenses-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProjectExpensesGridComponent
  extends GridBaseDirective<IExpenseRowGridDTO, ProjectExpenseService>
  implements OnInit
{
  expenseService = inject(ProjectExpenseService);
  coreService = inject(CoreService);
  projectTimeReportService = inject(ProjectTimeReportService);
  messageboxService = inject(MessageboxService);

  private readonly perform = new Perform<any>(this.progressService);

  customerInvoiceId!: number;
  isOrderMode: boolean = false;
  expenseRows: IExpenseRowGridDTO[] = [];
  filteredExpenseRows = new BehaviorSubject<IExpenseRowGridDTO[]>([]);
  isExpenses = new BehaviorSubject<boolean>(true);
  isRowSelected = signal(false);
  groupByDate = signal(false);
  projectContainer = TimeProjectContainer.TimeSheet;
  isBaseCurrency: boolean = false;
  searchFilterDTO!: IFilterExpensesModel;

  hasCurrencyPermission: boolean = false;
  editProjectPermission: boolean = false;
  editOrderPermission: boolean = false;
  editCustomerPermission: boolean = false;
  modifyOtherEmployeesPermission: boolean = false;

  userValidPayrollAttestStatesOptions: MenuButtonItem[] = [];
  userValidPayrollAttestStates: AttestStateDTO[] = [];
  private fromDate!: Date;
  private toDate!: Date;
  employee!: IEmployeeTimeCodeDTO;
  employees: IEmployeeTimeCodeDTO[] = [];
  employeeId?: number;
  projectId?: number;
  employeeCategoryDict: ISmallGenericType[] = [];
  projectsDict: ISmallGenericType[] = [];
  ordersDict: ISmallGenericType[] = [];

  projectInvoices!: IEmployeeProjectInvoiceDTO[];
  projects = new BehaviorSubject<IProjectSmallDTO[]>([]);
  orders = new BehaviorSubject<IProjectInvoiceSmallDTO[]>([]);
  selectedTimeSheetRows = new BehaviorSubject<ProjectTimeBlockDTO[]>([]);

  public get isTimeSheet(): boolean {
    return this.projectContainer === TimeProjectContainer.TimeSheet;
  }

  public get isProjectCentral(): boolean {
    return this.projectContainer === TimeProjectContainer.ProjectCentral;
  }

  ngOnInit(): void {
    super.ngOnInit();
  }

  override onTabActivated(): void {
    if (!this.gridIsDefined) {
      this.doStartFlow();
    }
  }

  doStartFlow() {
    this.fromDate = DateUtil.getDateFirstInWeek(new Date());
    this.toDate = DateUtil.getDateLastInWeek(new Date());
    this.employeeId = SoeConfigUtil.employeeId;
    this.projectId = 0;

    this.startFlow(
      Feature.Billing_Order_Orders_Edit_Expenses,
      'Common.Directives.ExpenseRows',
      {
        skipInitialLoad: true,
        additionalModifyPermissions: [
          Feature.Economy_Customer_Invoice_Status_Foreign,
          Feature.Time_Time_TimeSheetUser_OtherEmployees,
          Feature.Billing_Project_Edit,
          Feature.Billing_Order_Orders_Edit,
          Feature.Billing_Customer_Customers_Edit,
          Feature.Billing_Project_TimeSheetUser_OtherEmployees,
          Feature.Time_Time_TimeSheetUser_OtherEmployees,
        ],
        additionalReadPermissions: [
          Feature.Economy_Customer_Invoice_Status_Foreign,
          Feature.Time_Time_TimeSheetUser_OtherEmployees,
          Feature.Billing_Project_Edit,
          Feature.Billing_Order_Orders_Edit,
          Feature.Billing_Customer_Customers_Edit,
          Feature.Billing_Project_TimeSheetUser_OtherEmployees,
          Feature.Time_Time_TimeSheetUser_OtherEmployees,
        ],
        lookups: [this.loadEmployee()],
      }
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideReload: true,
    });
  }

  override onPermissionsLoaded(): void {
    super.onPermissionsLoaded();
    this.hasCurrencyPermission = this.flowHandler.hasReadAccess(
      Feature.Economy_Customer_Invoice_Status_Foreign
    );
    this.editProjectPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Project_Edit
    );
    this.editOrderPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order_Orders_Edit
    );
    this.editCustomerPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Customer_Customers_Edit
    );
    this.modifyOtherEmployeesPermission =
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Project_TimeSheetUser_OtherEmployees
      ) ||
      this.flowHandler.hasModifyAccess(
        Feature.Time_Time_TimeSheetUser_OtherEmployees
      );
  }

  protected searchGridData(model: IFilterExpensesModel) {
    this.perform.load(
      this.expenseService.getExpenseRowsFiltered(model).pipe(
        tap(x => {
          this.expenseRows = x;
          this.grid.setData(x);
        })
      )
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IExpenseRowGridDTO>): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.employee',
        'billing.project.timesheet.chargingtype',
        'common.date',
        'common.timecode',
        'common.quantity',
        'common.customer.invoices.amount',
        'common.customer.invoices.amounttoinvoice',
        'common.customer.invoices.foreignamount',
        'common.customer.invoices.currencyamounttotransfer',
        'common.customer.invoices.amount',
        'common.expensetype',
        'core.edit',
        'core.delete',
        'billing.project.timesheet.employeenr',
        'common.customer.invoices.specifiedunitprice',
        'common.customer.invoices.amountexvat',
        'common.customer.invoices.order',
        'common.customer.customer.customer',
        'billing.project.projectnr',
        'common.customer.customer.orderproject',
        'common.order',
        'common.sum',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();

        if (!this.isOrderMode) {
          this.grid.addColumnText(
            'orderNr',
            terms['common.customer.invoices.order'],
            {
              flex: 1,
              enableGrouping: true,
              buttonConfiguration: {
                iconPrefix: 'fal',
                iconName: 'pen',
                show: row => {
                  return (
                    (this.editOrderPermission &&
                      row?.orderId &&
                      row?.orderId > 0) ||
                    false
                  );
                },
                onClick: row => this.openOrder(row),
              },
            }
          );
          this.grid.addColumnText(
            'customerName',
            terms['common.customer.customer.customer'],
            {
              flex: 1,
              enableHiding: false,
              buttonConfiguration: {
                iconPrefix: 'fal',
                iconName: 'pen',
                onClick: r => {
                  this.edit(
                    {
                      ...r,
                      actorCustomerId: r.actorCustomerId,
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
            { flex: 1, enableGrouping: true, enableHiding: true }
          );
          this.grid.addColumnText(
            'projectName',
            terms['common.customer.customer.orderproject'],
            {
              flex: 1,
              enableGrouping: true,
              buttonConfiguration: {
                iconPrefix: 'fal',
                iconName: 'pen',
                show: row => this.editProjectPermission && row?.projectId > 0,
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
        } else {
          this.grid.setNbrOfRowsToShow(10);
        }

        this.grid.addColumnText(
          'employeeNumber',
          terms['billing.project.timesheet.employeenr'],
          { flex: 1, hide: true, enableGrouping: true, enableHiding: true }
        );
        this.grid.addColumnText('employeeName', terms['common.employee'], {
          flex: 1,
          hide: true,
          enableGrouping: true,
          tooltipField: 'columnNameTooltip',
        });
        this.grid.addColumnText('timeCodeName', terms['common.expensetype'], {
          flex: 1,
          hide: true,
          enableGrouping: true,
          tooltipField: 'columnNameTooltip',
        });
        this.grid.addColumnText('guantityFormatted', terms['common.quantity'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnDate('from', terms['common.date'], {
          enableGrouping: true,
          tooltipField: 'dateFormatted',
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnNumber(
          'amount',
          terms['common.customer.invoices.amount'],
          {
            flex: 1,
            enableHiding: false,
            decimals: 2,
            aggFuncOnGrouping: 'sum',
          }
        );
        this.grid.addColumnNumber(
          'amountExVat',
          terms['common.customer.invoices.amountexvat'],
          {
            flex: 1,
            enableHiding: false,
            decimals: 2,
            aggFuncOnGrouping: 'sum',
          }
        );
        if (this.hasCurrencyPermission && !this.isBaseCurrency) {
          this.grid.addColumnNumber(
            'amountCurrency',
            terms['common.customer.invoices.foreignamount'],
            {
              flex: 1,
              enableHiding: true,
              hide: true,
              decimals: 2,
              aggFuncOnGrouping: 'sum',
            }
          );
        }
        this.grid.addColumnShape('payrollAttestStateColor', '', {
          maxWidth: 40,
          shape: 'circle',
          tooltipField: 'payrollAttestStateName',
          showShapeField: 'payrollAttestStateColor',
          enableHiding: false,
          colorField: 'payrollAttestStateColor',
        });
        this.grid.addColumnNumber(
          'invoicedAmount',
          terms['common.customer.invoices.amounttoinvoice'],
          {
            flex: 1,
            enableHiding: false,
            decimals: 2,
            aggFuncOnGrouping: 'sum',
          }
        );
        if (this.hasCurrencyPermission && !this.isBaseCurrency) {
          this.grid.addColumnNumber(
            'invoicedAmountCurrency',
            terms['common.customer.invoices.currencyamounttotransfer'],
            {
              flex: 1,
              enableHiding: false,
              decimals: 2,
              aggFuncOnGrouping: 'sum',
              cellClassRules: {
                'text-right': () => true,
                errorRow: (row: CellClassParams) =>
                  row &&
                  row.data &&
                  row.data.invoicedAmountCurrency &&
                  row.data.invoicedAmountCurrency > 0 &&
                  row.data.invoicedAmountCurrency < row.data.amountCurrency,
              },
            }
          );
        }
        this.grid.addColumnShape('invoiceRowAttestStateColor', '', {
          maxWidth: 40,
          shape: 'circle',
          tooltipField: 'invoiceRowAttestStateName',
          showShapeField: 'invoiceRowAttestStateColor',
          colorField: 'invoiceRowAttestStateColor',
          enableHiding: false,
        });

        this.grid.addColumnBool(
          'isSpecifiedUnitPrice',
          terms['common.customer.invoices.specifiedunitprice'],
          {
            flex: 1,
            hide: true,
            enableHiding: true,
            pinned: 'right',
          }
        );

        this.grid.addColumnIcon('', '', {
          pinned: undefined,
          maxWidth: 40,
          iconPrefix: 'fal',
          iconName: 'paperclip',
          iconClass: 'paperclip',
          showIcon: (row: IExpenseRowGridDTO) => row?.hasFiles,
        });
        /*
        Dialog/edit page not implemented yet
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => this.edit(row),
        });
        */
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: r => this.deleteRow(r),
          showIcon: () => this.isOrderMode,
        });

        if (!this.isOrderMode) {
          const localHasSelectedMyOwnRows =
            this.grid
              .getSelectedRows()
              .filter(row => row.employeeId === this.employeeId).length > 0;
          this.loadAttestStates(
            localHasSelectedMyOwnRows && this.employees.length > 0
          ).subscribe();
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

  get isAttestDisabled() {
    return !(
      this.expenseRows &&
      this.expenseRows.length > 0 &&
      this.grid.getSelectedRows().length > 0 &&
      this.userValidPayrollAttestStatesOptions &&
      this.userValidPayrollAttestStatesOptions.length > 0
    );
  }

  private formatDate(date: Date, format = `yyyyMMdd'T'HHmmss`) {
    return DateUtil.format(date, format);
  }

  deleteRow(row: IExpenseRowGridDTO) {
    console.log('Deleted row: ', row);
  }

  private openOrder(row: IExpenseRowGridDTO) {
    BrowserUtil.openInNewTab(
      window,
      `/soe/billing/order/status/default.aspx?invoiceId=${row.orderId}&invoiceNr=${row.orderNr}`
    );
  }

  private loadEmployee() {
    return this.perform
      .load$(
        this.projectTimeReportService.getEmployeeForUserWithTimeCode(
          this.formatDate(DateUtil.getToday())
        )
      )
      .pipe(
        tap(x => {
          this.employee = x;
        })
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

  loadAttestStates(forceUseEmployeeGroup: boolean) {
    this.userValidPayrollAttestStates = [];
    return this.performLoadData.load$(
      this.coreService
        .getUserValidAttestStates(
          TermGroup_AttestEntity.PayrollTime,
          this.formatDate(this.fromDate),
          this.formatDate(this.toDate),
          true,
          this.modifyOtherEmployeesPermission && !forceUseEmployeeGroup
            ? 0
            : this.employee.employeeGroupId
        )
        .pipe(
          tap(x => {
            this.userValidPayrollAttestStates = x;
            x.forEach(state => {
              this.userValidPayrollAttestStatesOptions.push({
                id: state.attestStateId,
                label: state.name,
              });
            });
          })
        )
    );
  }

  saveAttest(event: MenuButtonItem): void {
    const attestStateTo: AttestStateDTO | undefined =
      this.userValidPayrollAttestStates.find(x => x.attestStateId === event.id);
    if (!attestStateTo) return;

    const transactionItems: AttestPayrollTransactionDTO[] = [];

    this.grid.getSelectedRows().forEach((row: IExpenseRowGridDTO) => {
      row.timePayrollTransactionIds.forEach(
        (timePayrollTransactionId: number) => {
          const transactionItem: IAttestPayrollTransactionDTO =
            new AttestPayrollTransactionDTO();
          transactionItem.employeeId = row.employeeId;
          transactionItem.timePayrollTransactionId = timePayrollTransactionId;
          transactionItem.attestStateId = row.payrollAttestStateId;
          if (row.payrollTransactionDate)
            transactionItem.date = row.payrollTransactionDate;

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
      this.projectTimeReportService
        .saveAttestForTransactionsValidation(model)
        .pipe(
          tap(response => {
            this.saveTransaction(response, event.id);
          })
        ),
      undefined
    );
  }

  loadProjectsOrder(selectedEmpIds: number[] = []) {
    const empIds =
      selectedEmpIds.length > 0
        ? selectedEmpIds
        : this.employeeId
          ? [this.employeeId]
          : [];

    if (!empIds || empIds.length === 0) return;

    this.perform.load(
      this.projectTimeReportService
        .getProjectsForTimeSheetEmployees(empIds)
        .pipe(
          tap(data => {
            if (data[0]) {
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
        )
    );
  }

  saveTransaction(res: any, attestId?: number) {
    if (res.success) {
      const model: ISaveAttestForTransactionsModel = {
        items: res.validItems,
        attestStateToId: attestId || 0,
        isMySelf: false,
      };
      this.projectTimeReportService
        .saveAttestForTransactions(model)
        .pipe(
          tap(result => {
            if (result.success) {
              this.searchGridData(this.searchFilterDTO);
            }
          })
        )
        .subscribe();
    } else {
      this.messageboxService.error(res.title, res.message);
    }
  }
}
