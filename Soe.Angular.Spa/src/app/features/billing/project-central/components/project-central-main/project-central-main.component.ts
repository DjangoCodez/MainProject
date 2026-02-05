import {
  Component,
  inject,
  OnInit,
  signal,
  AfterViewInit,
  computed,
} from '@angular/core';
import { ProjectCentralService } from '../../services/project-central.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  IProjectGridDTO,
  IProjectSearchResultDTO,
} from '@shared/models/generated-interfaces/ProjectDTOs';
import {
  Feature,
  SoeOriginStatusClassificationGroup,
  SoeReportTemplateType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ProjectCentralForm } from '../../models/project-central-form.model';
import { ValidationHandler } from '@shared/handlers';
import {
  ProjectSearchResultDTO,
  SelectProjectDialogData,
} from '@shared/components/select-project-dialog/models/select-project-dialog.model';
import { SelectProjectDialogComponent } from '@shared/components/select-project-dialog/components/select-project-dialog/select-project-dialog.component';
import { ProjectService } from '@features/billing/project/services/project.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { DateUtil } from '@shared/util/date-util';
import { ProjectEditComponent } from '@features/billing/project/components/project-edit/project-edit.component';
import { ProjectForm } from '@features/billing/project/models/project-form.model';
import { TimeProjectDTO } from '@features/billing/project/models/project.model';
import {
  SelectReportDialogCloseData,
  SelectReportDialogData,
} from '@shared/components/select-report-dialog/models/select-report-dialog.model';
import { SelectReportDialogComponent } from '@shared/components/select-report-dialog/components/select-report-dialog/select-report-dialog.component';
import { ProjectPrintDTO } from '@shared/models/report-print/project-print.model';
import { RequestReportService } from '@shared/services/request-report.service';

@Component({
  selector: 'soe-project-central-main',
  standalone: false,
  templateUrl: './project-central-main.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class ProjectCentralMainComponent
  extends GridBaseDirective<IProjectSearchResultDTO, ProjectCentralService>
  implements OnInit, AfterViewInit
{
  // service = inject(ProjectCentralService);
  projectService = inject(ProjectService);
  coreService = inject(CoreService);
  validationHandler = inject(ValidationHandler);
  form: ProjectCentralForm = new ProjectCentralForm({
    validationHandler: this.validationHandler,
    element: new ProjectSearchResultDTO(),
  });
  dialogService = inject(DialogService);
  private readonly requestReportService = inject(RequestReportService);
  //Properties
  projectId!: number;
  project!: IProjectGridDTO;
  timeProject!: TimeProjectDTO;
  projectData: ProjectSearchResultDTO | undefined;
  showBreadCrumbs: boolean = false;
  projectBreadCrumbs: any[] = [];
  projectInfoLabel!: string;
  fromDate!: Date;
  toDate!: Date;
  loadDetails = signal(false);
  includeChildProjects = signal(false);

  //Permissions
  modifyPermission: boolean = false;
  readOnlyPermission: boolean = false;
  hasEditProjectPermission: boolean = false;
  hasEditCustomerPermission: boolean = false;
  hasEditSupplierInvoicePermission: boolean = false;
  hasEditCustomerInvoicePermission: boolean = false;
  hasEditCustomerOrderPermission: boolean = false;
  hasProjectListPermission = signal(false);
  hasBillingReportPermission: boolean = false;

  defaultEmailTemplateProject!: number;
  isProjectSelected = signal(false);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Billing_Project_Central, '', {
      additionalModifyPermissions: [
        Feature.Billing_Customer_Customers_Edit,
        Feature.Billing_Project_Edit,
        Feature.Billing_Invoice_Invoices_Edit,
        Feature.Billing_Order_Orders_Edit,
        Feature.Economy_Supplier_Invoice_Invoices_Edit,
        Feature.Billing_Project_List,
        Feature.Billing_Distribution_Reports,
      ],
      skipInitialLoad: true,
    });
    this.projectInfoLabel = this.translate.instant('billing.project.project');
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.openSelectProject();
    }, 1000);
  }

  override onPermissionsLoaded(): void {
    super.onPermissionsLoaded();
    this.readOnlyPermission = this.flowHandler.readPermission();
    this.modifyPermission = this.flowHandler.modifyPermission();

    this.hasEditCustomerPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Customer_Customers_Edit
    );
    this.hasEditProjectPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Project_Edit
    );
    this.hasEditCustomerInvoicePermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Invoice_Invoices_Edit
    );
    this.hasEditCustomerOrderPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Order_Orders_Edit
    );
    this.hasEditSupplierInvoicePermission = this.flowHandler.hasModifyAccess(
      Feature.Economy_Supplier_Invoice_Invoices_Edit
    );
    this.hasProjectListPermission.set(
      this.flowHandler.hasModifyAccess(Feature.Billing_Project_List)
    );
    this.hasBillingReportPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Distribution_Reports
    );

    this.setupGridToolbar();
  }

  setupGridToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('findproject', {
          iconName: signal('search'),
          caption: signal('billing.project.central.findproject'),
          tooltip: signal('billing.project.central.findproject'),
          onAction: () => this.openSelectProject(),
        }),
      ],
    });
    if (this.hasEditCustomerOrderPermission) {
      this.toolbarService.createItemGroup({
        items: [
          this.toolbarService.createToolbarButton('neworder', {
            iconName: signal('plus'),
            caption: signal('billing.project.central.neworder'),
            tooltip: signal('billing.project.central.newordertooltip'),
            onAction: () => this.openNewOrder(),
          }),
        ],
      });
    }
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('projectList', {
          iconName: signal('list'),
          caption: signal('billing.project.central.projectlist'),
          tooltip: signal('billing.project.central.projectlist'),
          hidden: computed(() => !this.hasProjectListPermission),
          onAction: () => this.openProjectList(),
        }),
      ],
    });
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('projectReports', {
          iconName: signal('print'),
          caption: signal('billing.project.central.projectreports'),
          tooltip: signal('billing.project.central.projectreports'),
          disabled: computed(() => !this.isProjectSelected),
          onAction: () => this.openSelectReport(),
        }),
      ],
    });
  }

  openSelectProject() {
    const dialogData = new SelectProjectDialogData();
    dialogData.title = this.translate.instant(
      'billing.projects.list.searchprojects'
    );

    dialogData.size = 'lg';
    dialogData.showFindHidden = false;
    dialogData.loadHidden = true;

    this.dialogService
      .open(SelectProjectDialogComponent, dialogData)
      .afterClosed()
      .subscribe(result => {
        if (result) {
          this.changeActiveProject(result, true);
          this.form.patchProjectCentralData(this.project);
          this.isProjectSelected.set(true);
        }
      });
  }

  openProject(): void {
    if (!this.project.projectId) return;

    const additionalProps = {
      editComponent: ProjectEditComponent,
      FormClass: ProjectForm,
      editTabLabel:
        this.translate.instant('billing.project.project') +
        ' ' +
        this.project.name,
    };

    this.openEditInNewTab.emit({
      id: this.project.projectId,
      additionalProps: additionalProps,
    });
  }

  openNewOrder() {
    if (!this.project) return;
    let url = `/soe/billing/order/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleOrders}&invoiceId=${0}&invoiceNr=${''}`;
    if (this.project) {
      url = `/soe/billing/order/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleOrders}&invoiceId=${0}&projectId=${this.project.projectId}&customerId=${this.project.customerId}`;
    }
    BrowserUtil.openInNewTab(window, url);
  }

  openProjectList() {
    const url = `/soe/billing/project/list/default.aspx?c=7&spa=True`;
    BrowserUtil.openInNewTab(window, url);
  }

  openSelectReport() {
    if (!this.project || !this.project.projectId) return;

    const dialogData = new SelectReportDialogData();
    dialogData.title = 'common.selectreport';
    dialogData.size = 'lg';
    dialogData.reportTypes = [
      SoeReportTemplateType.ProjectTransactionsReport,
      SoeReportTemplateType.ProjectStatisticsReport,
      SoeReportTemplateType.OrderContractChange,
    ];
    dialogData.showCopy = false;
    dialogData.showEmail = false;
    dialogData.copyValue = false;
    dialogData.reports = [];
    dialogData.defaultReportId = 0;
    dialogData.langId = 0;
    dialogData.showReminder = false;
    dialogData.showLangSelection = false;
    dialogData.showSavePrintout = false;
    dialogData.savePrintout = true;
    const selectReportDialog = this.dialogService.open(
      SelectReportDialogComponent,
      dialogData
    );

    selectReportDialog
      .afterClosed()
      .subscribe((result: SelectReportDialogCloseData) => {
        if (result && result.reportId) {
          const model = new ProjectPrintDTO([this.projectId]);
          model.reportId = result.reportId;
          model.sysReportTemplateTypeId = result.reportType;
          model.dateFrom = this.fromDate;
          model.dateTo = this.toDate;
          model.includeChildProjects = this.includeChildProjects();

          this.requestReportService.printProjectReport(model).subscribe();
        }
      });
  }

  private loadProject(reloadBreadCrumbs: boolean = true) {
    return this.projectService
      .getProjectGridDTO(this.projectId)
      .subscribe(x => {
        this.changeActiveProject(x, reloadBreadCrumbs);
        this.project.startDate =
          typeof this.project.startDate === 'string'
            ? DateUtil.parseDate(this.project.startDate)
            : this.project.startDate;
        this.project.stopDate =
          typeof this.project.stopDate === 'string'
            ? DateUtil.parseDate(this.project.stopDate)
            : this.project.stopDate;
      });
  }

  private loadTimeProject(projectId: number) {
    this.projectService.get(projectId).subscribe(x => {
      this.timeProject = x;
    });
  }

  private initiateLoadBreadCrumbs() {
    this.projectBreadCrumbs = [];
    this.projectBreadCrumbs.push(this.project);
    if (this.project.parentProjectId)
      this.recursivelyLoadProjectBreadCrumbs(this.project.parentProjectId);
  }

  private recursivelyLoadProjectBreadCrumbs(parentProjectId: number) {
    if (parentProjectId) {
      this.projectService.getProjectGridDTO(parentProjectId).subscribe(x => {
        if (!this.projectBreadCrumbs.some(el => el.projectId === x.projectId))
          this.projectBreadCrumbs.unshift(x);
        if (x.parentProjectId)
          this.recursivelyLoadProjectBreadCrumbs(x.parentProjectId);
      });
    }
    this.showBreadCrumbs = this.projectBreadCrumbs.length > 1 ? true : false;
  }

  private changeActiveProject(
    project: IProjectGridDTO,
    reloadBreadCrumbs: boolean = true
  ) {
    this.project = project;
    this.projectId = project.projectId;
    this.projectInfoLabel =
      this.project.number +
      ' ' +
      this.project.name +
      ' | ' +
      this.project.statusName;
    if (reloadBreadCrumbs) this.initiateLoadBreadCrumbs();
  }
}
