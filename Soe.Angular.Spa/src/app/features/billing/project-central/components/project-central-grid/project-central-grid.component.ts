import { Component, inject, Input, OnInit, signal } from '@angular/core';
import { ProjectCentralSelectionForm } from '../../models/project-central-selection-form.model';
import { ValidationHandler } from '@shared/handlers';
import {
  ProjectCentralSelectionDTO,
  ProjectCentralStatusDTO,
  ProjectCentralSummaryDTO,
} from '../../models/project-central.model';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, tap } from 'rxjs';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { BrowserUtil } from '@shared/util/browser-util';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import {
  Feature,
  ProjectCentralBudgetRowType,
  ProjectCentralHeaderGroupType,
  SettingMainType,
  SoeOriginType,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { TermCollection } from '@shared/localization/term-types';
import { TextColumnOptions, TimeColumnOptions } from '@ui/grid/util';
import { ProjectCentralService } from '../../services/project-central.service';
import { IProjectGridDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { SaveUserCompanySettingModel } from '@shared/components/select-project-dialog/models/select-project-dialog.model';
import { ProjectCentralDataService } from '../../services/project-central-data.service';
import { ToolbarCheckboxAction } from '@ui/toolbar/toolbar-checkbox/toolbar-checkbox.component';
import { ToolbarDatepickerAction } from '@ui/toolbar/toolbar-datepicker/toolbar-datepicker.component';
import { MessagingService } from '@shared/services/messaging.service';

@Component({
  selector: 'soe-project-central-grid',
  standalone: false,
  templateUrl: './project-central-grid.component.html',
  providers: [ToolbarService, FlowHandlerService],
})
export class ProjectCentralGridComponent
  extends GridBaseDirective<ProjectCentralStatusDTO>
  implements OnInit
{
  projectCentralService = inject(ProjectCentralService);
  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  perform = new Perform(this.progressService);
  projectCentralDataService = inject(ProjectCentralDataService);
  messagingService = inject(MessagingService);

  @Input({ required: true }) project!: IProjectGridDTO;
  @Input({ required: true }) hasEditSupplierInvoicePermission = false;
  @Input({ required: true }) hasEditCustomerOrderPermission = false;
  gridDataParams!: ProjectCentralSummaryDTO;

  includeSubProjects = signal(false);
  showDetailedInformation = signal(false);

  toolbarService = inject(ToolbarService);
  validationHandler = inject(ValidationHandler);
  selectionForm: ProjectCentralSelectionForm = new ProjectCentralSelectionForm({
    validationHandler: this.validationHandler,
    element: new ProjectCentralSelectionDTO(),
  });

  fromDate = signal<Date | undefined>(undefined);
  toDate = signal<Date | undefined>(undefined);
  loadDetails: boolean = false;
  includeChildProjects: boolean = false;

  //Datagrid
  projectRows: ProjectCentralStatusDTO[] = [];
  costTypeColumnOption: TextColumnOptions<ProjectCentralStatusDTO> = {};
  employeeColumnOption: TextColumnOptions<ProjectCentralStatusDTO> = {};

  //Sums
  incomeinvoiced!: number;
  costs!: number;
  result!: number;
  resultbudget!: number;
  days: string = 'N/A';
  workedhours!: string;
  notbilledhours!: string;
  fixedprice!: number;
  marginalincomeratio!: number;

  ngOnInit(): void {
    this.startFlow(Feature.None, 'project.overview', {
      lookups: [this.loadTerms(), this.loadUserSettings()],
      skipInitialLoad: true,
      skipDefaultToolbar: true,
    });
    this.setupToolbar();
  }

  clearSelection(): void {
    this.includeSubProjects.set(false);
    this.showDetailedInformation.set(false);
    this.fromDate.set(undefined);
    this.toDate.set(undefined);
  }

  loadUserSettings() {
    const settingTypes: number[] = [];
    settingTypes.push(UserSettingType.ProjectUseDetailedViewInProjectOverview);
    settingTypes.push(UserSettingType.ProjectUseChildProjectsInProjectOverview);

    return this.coreService.getUserSettings(settingTypes).pipe(
      tap(x => {
        this.showDetailedInformation.set(
          SettingsUtil.getBoolUserSetting(
            x,
            UserSettingType.ProjectUseDetailedViewInProjectOverview,
            false
          )
        );
        this.includeSubProjects.set(
          SettingsUtil.getBoolUserSetting(
            x,
            UserSettingType.ProjectUseChildProjectsInProjectOverview,
            false
          )
        );
        this.projectCentralDataService.setData(
          new ProjectCentralSummaryDTO(
            this.project?.projectId || 0,
            this.project?.customerId || 0,
            this.includeSubProjects(),
            this.showDetailedInformation()
          )
        );
      })
    );
  }

  setupToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('loadButton', {
          caption: signal('billing.project.central.getdata'),
          onAction: () => this.loadProjectCentralGridData(),
        }),
        this.toolbarService.createToolbarButton('clearButton', {
          iconName: signal('filter'),
          onAction: () => this.clearSelection(),
        }),
      ],
    });
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarDatepicker('dateFrom', {
          labelKey: signal('common.from'),
          initialDate: this.fromDate,
          onValueChanged: event =>
            this.fromDate.set(
              (event as ToolbarDatepickerAction)?.value || undefined
            ),
        }),
      ],
    });
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarDatepicker('dateTo', {
          labelKey: signal('common.to'),
          initialDate: this.toDate,
          onValueChanged: event =>
            this.toDate.set(
              (event as ToolbarDatepickerAction)?.value || undefined
            ),
        }),
      ],
    });
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarCheckbox('includeSubProjects', {
          labelKey: signal('billing.project.central.inclchildprojects'),
          checked: this.includeSubProjects,
          onValueChanged: event =>
            this.onIncludeSubProjectsChange(
              (event as ToolbarCheckboxAction).value
            ),
        }),
      ],
    });
    this.toolbarService.createItemGroup({
      alignLeft: true,
      items: [
        this.toolbarService.createToolbarCheckbox('showDetails', {
          labelKey: signal('billing.project.central.showdetails'),
          checked: this.showDetailedInformation,
          onValueChanged: event =>
            this.onShowDetailsChange((event as ToolbarCheckboxAction).value),
        }),
      ],
    });
  }

  override loadTerms(): Observable<TermCollection> {
    const keys: string[] = [
      'billing.project.central.specification',
      'billing.project.central.budget',
      'common.type',
      'common.time',
      'common.amount',
      'billing.project.central.outcome',
      'billing.project.central.deviation',
      'billing.project.central.showinfo',
      'core.edit',
      'economy.supplier.invoice.timecodes',
      'common.employee',
    ];
    return super.loadTerms(keys);
  }

  override onGridReadyToDefine(
    grid: GridComponent<ProjectCentralStatusDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.grid.addColumnText('groupRowTypeName', this.terms['common.type'], {
      enableGrouping: true,
      grouped: true,
      hide: true,
    });

    this.grid.addColumnText(
      'typeName',
      this.terms['billing.project.central.specification'],
      {
        flex: 1,
        enableGrouping: true,
        enableHiding: false,
        buttonConfiguration: {
          iconName: 'pen',
          iconPrefix: 'fas',
          onClick: (row: ProjectCentralStatusDTO) => this.openOrderInvoice(row),
          show: (row: ProjectCentralStatusDTO) => this.showIcon(row),
        },
      }
    );

    const timeColumnOptions: TimeColumnOptions<ProjectCentralStatusDTO> = {
      enableHiding: false,
      enableGrouping: true,
      alignLeft: false,
      filter: false,
      flex: 1,
      aggFuncOnGrouping: 'sumTimeSpan',
      cellClassRules: {
        excelTime: () => true,
      },
    };

    this.costTypeColumnOption = {
      flex: 1,
      enableGrouping: true,
      hide: true,
      grouped: true,
    };
    this.grid.addColumnText(
      'costTypeName',
      this.terms['economy.supplier.invoice.timecodes'],
      this.costTypeColumnOption
    );

    this.employeeColumnOption = {
      flex: 1,
      enableGrouping: true,
      filter: false,
      hide: true,
    };

    this.grid.addColumnText(
      'employeeName',
      this.terms['common.employee'],
      this.employeeColumnOption
    );

    const colBudgetHeader = this.grid.addColumnHeader(
      'budget',
      this.terms['billing.project.central.budget'],
      {}
    );
    this.grid.addColumnNumber('budget', this.terms['common.amount'], {
      flex: 1,
      headerColumnDef: colBudgetHeader,
      enableHiding: false,
      filter: false,
      decimals: 2,
      aggFuncOnGrouping: 'sum',
    });
    this.grid.addColumnTimeSpan(
      'budgetTimeFormatted',
      this.terms['common.time'],
      {
        ...timeColumnOptions,
        headerColumnDef: colBudgetHeader,
        clearZero: true,
      }
    );

    const colOutcomeHeader = this.grid.addColumnHeader(
      'value',
      this.terms['billing.project.central.outcome'],
      { marryChildren: true }
    );
    this.grid.addColumnNumber('value', this.terms['common.amount'], {
      flex: 1,
      enableHiding: false,
      filter: false,
      decimals: 2,
      aggFuncOnGrouping: 'sum',
      headerColumnDef: colOutcomeHeader,
    });
    this.grid.addColumnTimeSpan(
      'valueTimeFormatted',
      this.terms['common.time'],
      {
        ...timeColumnOptions,
        headerColumnDef: colOutcomeHeader,
        clearZero: true,
      }
    );

    const colDiffHeader = this.grid.addColumnHeader(
      'diff',
      this.terms['billing.project.central.deviation'],
      { marryChildren: true }
    );
    this.grid.addColumnNumber('diff', this.terms['common.amount'], {
      flex: 1,
      enableHiding: false,
      filter: false,
      decimals: 2,
      aggFuncOnGrouping: 'sum',
      headerColumnDef: colDiffHeader,
    });
    this.grid.addColumnTimeSpan(
      'diffTimeFormatted',
      this.terms['common.time'],
      {
        ...timeColumnOptions,
        headerColumnDef: colDiffHeader,
        clearZero: true,
      }
    );

    this.grid.useGrouping();
    super.finalizeInitGrid();

    this.handleDetails();
  }

  loadProjectCentralGridData() {
    if (!this.project || !this.project.projectId) return;

    this.perform.load(
      this.projectCentralService
        .getProjectCentralStatus(
          this.project.projectId,
          this.includeSubProjects(),
          this.showDetailedInformation(),
          this.fromDate(),
          this.toDate()
        )
        .pipe(
          tap(x => {
            this.projectRows = x;
            const orders: number[] = [];
            const customerInvoices: number[] = [];
            const supplierInvoices: number[] = [];
            this.projectRows.forEach(row => {
              if (
                row.originType === SoeOriginType.Order &&
                !orders.includes(row.associatedId)
              ) {
                orders.push(row.associatedId);
              } else if (
                row.originType === SoeOriginType.CustomerInvoice &&
                !customerInvoices.includes(row.associatedId)
              ) {
                customerInvoices.push(row.associatedId);
              } else if (
                row.originType === SoeOriginType.SupplierInvoice &&
                !supplierInvoices.includes(row.associatedId)
              ) {
                supplierInvoices.push(row.associatedId);
              }

              if (row.type === ProjectCentralBudgetRowType.CostPersonell) {
                row['budgetTimeFormatted'] =
                  row.budgetTime > 0
                    ? DateUtil.minutesToTimeSpan(row.budgetTime)
                    : '';
                row['valueTimeFormatted'] =
                  row.value2 > 0
                    ? DateUtil.minutesToTimeSpan(row.value2 * 60)
                    : '';
              } else if (row.type === ProjectCentralBudgetRowType.CostExpense) {
                row['budgetTimeFormatted'] =
                  row.budgetTime > 0
                    ? DateUtil.minutesToTimeSpan(row.budgetTime)
                    : '';
                row['valueTimeFormatted'] =
                  row.value2 > 0 ? DateUtil.minutesToTimeSpan(row.value2) : '';
                row['diffTimeFormatted'] =
                  row.budgetTime > 0 || row.value2 > 0
                    ? DateUtil.minutesToTimeSpan(row.value2 - row.budgetTime)
                    : '';
              }
            });
            this.rowData.next(
              this.projectRows.filter(
                r =>
                  r.groupRowType != ProjectCentralHeaderGroupType.Time &&
                  r.groupRowType != ProjectCentralHeaderGroupType.None
              )
            );
            this.summarize();
            this.enableTabs();
            this.projectCentralDataService.setData(
              new ProjectCentralSummaryDTO(
                this.project.projectId,
                this.project.customerId,
                this.includeSubProjects(),
                this.showDetailedInformation(),
                orders,
                customerInvoices,
                supplierInvoices,
                this.projectRows,
                this.fromDate(),
                this.toDate()
              )
            );
          })
        )
    );
  }

  private summarize(): void {
    this.incomeinvoiced = this.projectRows
      .filter(r => r.type === ProjectCentralBudgetRowType.IncomeInvoiced)
      .reduce((x, y) => x + y.value, 0);

    this.costs = this.projectRows
      .filter(
        r =>
          r.type === ProjectCentralBudgetRowType.CostExpense ||
          r.type === ProjectCentralBudgetRowType.CostMaterial ||
          r.type === ProjectCentralBudgetRowType.CostPersonell ||
          r.type === ProjectCentralBudgetRowType.OverheadCost
      )
      .reduce((x, y) => x + y.value, 0);

    this.result = this.incomeinvoiced - this.costs;

    const budgetIncome = this.projectRows
      .filter(
        r =>
          r.type === ProjectCentralBudgetRowType.IncomeInvoiced ||
          r.type === ProjectCentralBudgetRowType.IncomeNotInvoiced
      )
      .reduce((x, y) => x + y.budget, 0);

    const budgetCosts = this.projectRows
      .filter(
        r =>
          r.type === ProjectCentralBudgetRowType.CostExpense ||
          r.type === ProjectCentralBudgetRowType.CostMaterial ||
          r.type === ProjectCentralBudgetRowType.CostPersonell ||
          r.type === ProjectCentralBudgetRowType.OverheadCost ||
          r.type === ProjectCentralBudgetRowType.OverheadCostPerHour
      )
      .reduce((x, y) => x + y.budget, 0);

    this.resultbudget = budgetIncome - budgetCosts;

    this.fixedprice = this.projectRows
      .filter(r => r.type === ProjectCentralBudgetRowType.FixedPriceTotal)
      .reduce((x, y) => x + y.value, 0);

    this.marginalincomeratio =
      ((this.incomeinvoiced - this.costs) / this.incomeinvoiced) * 100;

    const personellCost = this.projectRows
      .filter(r => r.type === ProjectCentralBudgetRowType.CostPersonell)
      .reduce((x, y) => x + y.value2, 0);

    this.workedhours = DateUtil.minutesToTimeSpan(personellCost * 60);

    const billableNotInvoiced = this.projectRows
      .filter(
        r => r.type === ProjectCentralBudgetRowType.BillableMinutesNotInvoiced
      )
      .reduce((x, y) => x + y.value, 0);

    this.notbilledhours = DateUtil.minutesToTimeSpan(billableNotInvoiced);

    if (this.project.startDate) {
      const today = new Date();
      const startDate = this.project.startDate;
      const oneDay = 1000 * 60 * 60 * 24;
      const timeDiff = today.getTime() - startDate.getTime();
      this.days = Math.ceil(timeDiff / oneDay).toString();
    } else {
      this.days = 'N/A';
    }
  }

  private handleDetails(): void {
    if (!this.grid) return;

    setTimeout(() => {
      if (this.showDetailedInformation()) {
        this.grid.showColumns(['costTypeName']);
        this.grid.groupRowsByColumn?.('groupRowTypeName', false);
        this.grid.groupRowsByColumn?.('costTypeName', false);
      } else {
        this.grid.ungroupColumn('costTypeName');
        this.grid.groupRowsByColumn?.('groupRowTypeName', false);
        this.grid.hideColumns(['costTypeName']);
      }
      this.grid.resetColumns();

      if (this.showDetailedInformation()) {
        this.grid.showColumns(['employeeName']);
      } else {
        this.grid.hideColumns(['employeeName']);
      }
    }, 100);
  }

  onIncludeSubProjectsChange(checked: boolean): void {
    this.includeSubProjects.set(checked);
    this.gridDataParams.includeChildProjects = checked;
    this.selectionForm.includeSubProjects.setValue(checked);
    setTimeout(() => {
      this.coreService
        .saveBoolSetting(
          new SaveUserCompanySettingModel(
            SettingMainType.User,
            UserSettingType.ProjectUseChildProjectsInProjectOverview,
            this.includeSubProjects()
          )
        )
        .subscribe();
    });
  }

  onShowDetailsChange(checked: boolean): void {
    this.showDetailedInformation.set(checked);
    this.gridDataParams.showDetailedInformation = checked;
    this.selectionForm.showDetailedInformation.setValue(checked);

    setTimeout(() => {
      this.handleDetails();
    }, 50);

    setTimeout(() => {
      this.coreService
        .saveBoolSetting(
          new SaveUserCompanySettingModel(
            SettingMainType.User,
            UserSettingType.ProjectUseDetailedViewInProjectOverview,
            this.showDetailedInformation()
          )
        )
        .subscribe();
    });
  }

  openOrderInvoice(row: ProjectCentralStatusDTO): void {
    let url = '';
    switch (row.originType) {
      case SoeOriginType.Order:
        url = `/soe/billing/order/status/default.aspx?invoiceId=${row.associatedId}`;
        break;
      case SoeOriginType.CustomerInvoice:
        url = `/soe/billing/invoice/status/default.aspx?classificationgroup=${row.groupRowType}&invoiceId=${row.associatedId}`;
        break;
      case SoeOriginType.SupplierInvoice:
        url = `/soe/economy/supplier/invoice/status/default.aspx?invoiceId=${row.associatedId}&invoiceNr=${row.name}`;
        break;
    }

    BrowserUtil.openInNewTab(window, url);
  }

  showIcon(row: ProjectCentralStatusDTO): boolean {
    if (!row || !row.originType) return false;

    let editPermission: boolean;
    switch (row.originType) {
      case SoeOriginType.SupplierInvoice:
        editPermission = this.hasEditSupplierInvoicePermission;
        break;
      case SoeOriginType.CustomerInvoice:
        editPermission = this.hasEditCustomerOrderPermission;
        break;
      case SoeOriginType.Order:
        editPermission = this.hasEditCustomerOrderPermission;
        break;
      default:
        editPermission = false;
    }

    return editPermission && row.isEditable;
  }

  private enableTabs() {
    this.messagingService.publishEnableTabByKey(
      'project-central-timesheet',
      true
    );
    this.messagingService.publishEnableTabByKey(
      'project-central-productrows',
      true
    );
    this.messagingService.publishEnableTabByKey(
      'project-central-supplierinvoices',
      true
    );
  }
}
