import {
  Component,
  computed,
  DestroyRef,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ProjectService } from '../../services/project.service';
import { ProjectForm } from '../../models/project-form.model';
import {
  IProjectUserExDTO,
  SaveInvoiceProjectModel,
  TimeProjectDTO,
} from '../../models/project.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  CompanySettingType,
  Feature,
  ProjectAccountType,
  SoeCategoryRecordEntity,
  SoeCategoryType,
  SoeDataStorageRecordType,
  SoeEntityType,
  TermGroup,
  TermGroup_ProjectStatus,
  TermGroup_TimeProjectInvoiceProductAccountingPrio,
  TermGroup_TimeProjectPayrollProductAccountingPrio,
} from '@shared/models/generated-interfaces/Enumerations';
import { catchError, map, Observable, of, Subject, take, tap } from 'rxjs';
import {
  IDecimalKeyValue,
  ISmallGenericType,
} from '@shared/models/generated-interfaces/GenericType';
import { IProjectTinyDTO } from '@shared/models/generated-interfaces/ProjectDTOs';
import { SupplierService } from '@features/economy/services/supplier.service';
import {
  AccountDims,
  AccountDimsForm,
  SelectedAccounts,
  SelectedAccountsChangeSet,
} from '@shared/components/account-dims/account-dims-form.model';
import { ValidationHandler } from '@shared/handlers';
import { CategoryService } from '@shared/features/category/services/category.service';
import { CommonCustomerService } from '@billing/shared/services/common-customer.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { DateUtil } from '@shared/util/date-util';
import { SettingsUtil } from '@shared/util/settings-util';
import { IAccountingSettingsRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { IPriceListDTO } from '@shared/models/generated-interfaces/PriceListDTOs';
import { TraceRowPageName } from '@shared/components/trace-rows/models/trace-rows.model';
import { CrudActionTypeEnum } from '@shared/enums';
import { ProgressOptions } from '@shared/services/progress';
import { PriceListDTO } from '@features/billing/models/pricelist.model';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CategoryItem } from '@shared/components/categories/categories.model';
import { SelectProjectDialogData } from '@shared/components/select-project-dialog/models/select-project-dialog.model';
import { SelectProjectDialogComponent } from '@shared/components/select-project-dialog/components/select-project-dialog/select-project-dialog.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { FilesHelper } from '@shared/components/files-helper/files-helper.component';

@Component({
  selector: 'soe-project-edit',
  templateUrl: './project-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProjectEditComponent
  extends EditBaseDirective<
    SaveInvoiceProjectModel,
    ProjectService,
    ProjectForm
  >
  implements OnInit
{
  service = inject(ProjectService);
  coreService = inject(CoreService);
  supplierService = inject(SupplierService);
  categoryService = inject(CategoryService);
  commonCustomerService = inject(CommonCustomerService);
  dialogService = inject(DialogService);
  destroyed$ = inject(DestroyRef);

  projectId = signal<number | undefined>(undefined);
  projectIdNotSet = computed(() => !this.projectId());

  project!: TimeProjectDTO;
  projects!: IProjectTinyDTO[];
  projectsDict!: ISmallGenericType[];
  allocationTypes: ISmallGenericType[] = [];
  attestGroups: ISmallGenericType[] = [];
  orderTemplates: ISmallGenericType[] = [];
  categoryIds: number[] = [];
  customers: ISmallGenericType[] = [];
  projectUsers: IProjectUserExDTO[] = [];

  invoiceProductAccountingPrios!: any[];
  payrollProductAccountingPrios!: any[];
  projectAccountSettingTypes: ISmallGenericType[] = [];
  projectBaseAccounts: ISmallGenericType[] = [];
  projectStatuses: ISmallGenericType[] = [];

  overheadCostAsFixedAmount: boolean = false;
  overheadCostAsAmountPerHour: boolean = false;
  defaultPriceListType: boolean = false;
  priceListTypeId: number = 0;
  comparisonPricelistId: any = 0;
  comparisonPriceLists: ISmallGenericType[] = [];
  projectPriceLists: ISmallGenericType[] = [];
  projectPriceListGridRows: IPriceListDTO[] = [];
  loadAllProducts: boolean = false;
  priceDate: string = DateUtil.getISODateString(new Date());
  pricelistExpanderIsOpen: boolean = false;
  pageName = TraceRowPageName.Project;
  pageLabel: string = '';
  autoUpdateInternalAccounts: boolean = false;

  //Permissions
  modifyPermission: boolean = false;
  attestFlowPermission: boolean = false;
  budgetPermission: boolean = false;
  categories: any[] = [];
  accountDimsForm!: AccountDimsForm;
  accountsDims!: AccountDims;

  isLocked = signal(true);
  projectUserRendered: boolean = false;
  traceRowsRendered: boolean = false;

  ProjectAutoUpdateAccountSettings$: Subject<void> = new Subject<void>();
  autoUpdateInternalAccounts$: Subject<SelectedAccountsChangeSet> =
    new Subject<SelectedAccountsChangeSet>();

  filesHelper!: FilesHelper;

  constructor() {
    super();
    this.filesHelper = new FilesHelper(
      true,
      SoeEntityType.TimeProject,
      SoeDataStorageRecordType.ProjectFileAttachment,
      Feature.Billing_Project,
      this.performLoadData
    );
  }

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Billing_Project_Edit, {
      additionalModifyPermissions: [
        Feature.Economy_Supplier_Invoice_AttestFlow,
        Feature.Billing_Project_Edit_Budget,
        Feature.Time_Project_Invoice_ShowProjectsWithoutCustomer,
      ],
      additionalReadPermissions: [],
      lookups: [
        this.loadProjects(),
        this.loadallocationTypes(),
        this.loadAttestGroups(),
        this.loadOrderTemplates(),
        this.loadCategories(),
        this.loadCustomers(),
        this.loadSettingTypes(),
        this.loadPayrollProductAccountingPriority(),
        this.loadInvoiceProductAccountingPriority(),
        this.loadPriceLists(),
        this.loadProjectStatuses(),
      ],
    });
    this.pageLabel = this.translate.instant('billing.project.project');

    this.accountDimsForm = new AccountDimsForm({
      accountDimsValidationHandler: new ValidationHandler(
        this.translate,
        this.messageboxService
      ),
      element: this.accountsDims,
    });

    this.form
      ?.getIdControl()
      ?.valueChanges.pipe(takeUntilDestroyed(this.destroyed$))
      .subscribe(this.projectId.set);
  }

  override loadData(): Observable<void> {
    this.filesHelper.recordId.set(this.form?.getIdControl()?.value);

    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: TimeProjectDTO) => {
          value.accountingSettings = this.remapAccountSettings(
            value.accountingSettings
          );
          this.project = value;
          this.accountsDims = {
            account1: value.defaultDim1AccountId ?? 0,
            account2: value.defaultDim2AccountId ?? 0,
            account3: value.defaultDim3AccountId ?? 0,
            account4: value.defaultDim4AccountId ?? 0,
            account5: value.defaultDim5AccountId ?? 0,
            account6: value.defaultDim6AccountId ?? 0,
          };
          this.accountDimsForm.reset(this.accountsDims);
          this.form?.customPatchValue(value);
        })
      )
    );
  }

  private remapAccountSettings(
    rows: IAccountingSettingsRowDTO[]
  ): IAccountingSettingsRowDTO[] {
    return rows.map(r => {
      const newSetting = {
        type: r.type,
        percent: r.percent,
      };

      this.swap(r, 1, newSetting);
      this.swap(r, 2, newSetting);
      this.swap(r, 3, newSetting);
      this.swap(r, 4, newSetting);
      this.swap(r, 5, newSetting);
      this.swap(r, 6, newSetting);

      return newSetting as IAccountingSettingsRowDTO;
    });
  }

  private swap(source: any, dimNr: number, target: any) {
    const toDimNr = source[`accountDim${dimNr}Nr`] as number;

    target[`accountDim${toDimNr}Nr`] = toDimNr;
    target[`account${toDimNr}Nr`] = source[`account${dimNr}Nr`];
    target[`account${toDimNr}Id`] = source[`account${dimNr}Id`];
    target[`account${toDimNr}Name`] = source[`account${dimNr}Name`];
  }

  override onFinished(): void {
    if (!this.form?.getIdControl()?.value) {
      this.project = new TimeProjectDTO(TermGroup_ProjectStatus.Active);
      this.form?.projectStatus.setValue(this.project.status);
    }

    this.loadPayrollProductAccountingPriorityRows();
    this.loadInvoiceProductAccountingPriorityRows();
    this.setSuggestedProjectNumber();

    if (this.form?.parentProjectId.value > 0) {
      if (
        this.projectsDict.find(p => p.id === this.form?.parentProjectId.value)
      ) {
        this.projectsDict.push({
          id: this.form?.parentProjectId.value,
          name: this.form?.number.value + '' + this.form?.name.value,
        });
      }
    }
  }

  override createEditToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton(
          'billing.projects.list.unlockcustomer',
          {
            iconName: signal('unlock-alt'),
            tooltip: signal('billing.projects.list.unlockcustomer'),
            onAction: this.unlockCustomer.bind(this),
            hidden: this.projectIdNotSet,
          }
        ),
        this.toolbarService.createToolbarButton(
          'billing.projects.list.openprojectcentral',
          {
            iconName: signal('calculator-alt'),
            tooltip: signal('billing.projects.list.openprojectcentral'),
            onAction: this.openProjectCentral.bind(this),
            hidden: this.projectIdNotSet,
          }
        ),
        this.toolbarService.createToolbarButton(
          'billing.projects.list.createunderproject',
          {
            iconName: signal('plus'),
            tooltip: signal('billing.projects.list.createunderproject'),
            onAction: this.openNewProjectTab.bind(this),
            hidden: this.projectIdNotSet,
          }
        ),
      ],
    });
  }

  override newRecord(): Observable<void> {
    const clearValues = () => {
      if (this.form?.isNew || this.form?.isCopy) {
        this.form?.reset();
      }

      //set property when creating a sub project
      if (this.form?.data?.parentProjectId > 0) {
        this.isLocked.set(false);
        this.form?.controls.parentProjectId.setValue(
          this.form?.data.parentProjectId
        );
      }

      if (this.form?.isNew)
        this.form?.controls.parentProjectId.setValue(
          this.form?.additionalPropsOnCopy?.parentProjectId || 0
        );
    };

    return of(clearValues());
  }

  //create sub project
  openNewProjectTab() {
    super.openEditInNewTab({
      id: 0,
      additionalProps: {
        editComponent: ProjectEditComponent,
        FormClass: ProjectForm,
        editTabLabel: 'billing.projects.list.new_project',
        isNew: true,
        data: { parentProjectId: this.form?.projectId.value },
      },
    });
  }

  override onPermissionsLoaded(): void {
    this.modifyPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Project_Edit
    );
    this.attestFlowPermission = this.flowHandler.hasModifyAccess(
      Feature.Economy_Supplier_Invoice_AttestFlow
    );
    this.budgetPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Project_Edit_Budget
    );
  }

  override loadCompanySettings() {
    const settingTypes: CompanySettingType[] = [
      CompanySettingType.ProjectOverheadCostAsFixedAmount,
      CompanySettingType.ProjectOverheadCostAsAmountPerHour,
      CompanySettingType.BillingDefaultPriceListType,
      CompanySettingType.ProjectAutoUpdateInternalAccounts,
    ];

    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(x => {
        this.overheadCostAsFixedAmount = SettingsUtil.getBoolCompanySetting(
          x,
          CompanySettingType.ProjectOverheadCostAsFixedAmount
        );
        this.overheadCostAsAmountPerHour = SettingsUtil.getBoolCompanySetting(
          x,
          CompanySettingType.ProjectOverheadCostAsAmountPerHour
        );
        this.defaultPriceListType = SettingsUtil.getBoolCompanySetting(
          x,
          CompanySettingType.BillingDefaultPriceListType
        );
        this.autoUpdateInternalAccounts = SettingsUtil.getBoolCompanySetting(
          x,
          CompanySettingType.ProjectAutoUpdateInternalAccounts
        );
      })
    );
  }

  private loadProjects() {
    this.projectsDict = [];
    return this.service.getProjectsSmall(true, true, true).pipe(
      tap(x => {
        this.projects = x;
        this.projectsDict.push({ id: 0, name: '' });
        for (let i = 0; i < this.projects.length; i++) {
          const row = this.projects[i];
          if (
            (!this.form?.getIdControl()?.value ||
              row.projectId !== this.form?.getIdControl()?.value) &&
            row.status < 4
          ) {
            this.projectsDict.push({
              id: row.projectId,
              name: row.number + ' ' + row.name,
            });
          }
        }
      })
    );
  }

  private loadProjectStatuses() {
    return this.coreService
      .getTermGroupContent(TermGroup.ProjectStatus, true, false)
      .pipe(
        tap(statuses => {
          // this.projectStatuses = statuses;
          // this.projectStatusSelection = this.projectStatuses.find(y => y.id == 2) || this.projectStatuses[0];

          statuses.forEach((status, i) => {
            this.projectStatuses.push({ id: status.id, name: status.name });
          });
        })
      );
  }

  private loadallocationTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.ProjectAllocationType, false, true)
      .pipe(tap(x => (this.allocationTypes = x)));
  }

  private loadAttestGroups() {
    return this.supplierService
      .getAttestWorkFlowGroupsDict(true)
      .pipe(tap(x => (this.attestGroups = x)));
  }

  private loadOrderTemplates(useCache: boolean = true) {
    return this.service
      .getOrderTemplates(useCache)
      .pipe(tap(x => (this.orderTemplates = x)));
  }

  private loadCategories() {
    return this.categoryService
      .getCompCategoryRecords(
        SoeCategoryType.Project,
        SoeCategoryRecordEntity.Project,
        this.form?.getIdControl()?.value
      )
      .pipe(
        tap(x => {
          this.categoryIds = x.map(y => y.categoryId);
        })
      );
  }

  private loadCustomers() {
    return this.commonCustomerService.getCustomersDict(true, true, true).pipe(
      tap(x => {
        this.customers = x;
      })
    );
  }

  getCustomerEmailFunc = (): Observable<string> => {
    return this.commonCustomerService
      .getCustomerEmails(this.form?.customerId.value, false, false)
      .pipe(
        take(1),
        map(x => (x.length > 0 ? x[0].name : '')),
        catchError(() => of(''))
      );
  };

  onCategoriesChanged(categoryItems: CategoryItem[]) {
    this.categories = categoryItems;
  }

  accountDimsChanged(dimsChanged: SelectedAccountsChangeSet): void {
    this.form?.markAsDirty();

    this.form?.defaultDim2AccountId.setValue(
      dimsChanged.selectedAccounts.account2?.accountId ?? 0
    );
    this.form?.defaultDim3AccountId.setValue(
      dimsChanged.selectedAccounts.account3?.accountId ?? 0
    );
    this.form?.defaultDim4AccountId.setValue(
      dimsChanged.selectedAccounts.account4?.accountId ?? 0
    );
    this.form?.defaultDim5AccountId.setValue(
      dimsChanged.selectedAccounts.account5?.accountId ?? 0
    );
    this.form?.defaultDim6AccountId.setValue(
      dimsChanged.selectedAccounts.account6?.accountId ?? 0
    );

    if (this.autoUpdateInternalAccounts && dimsChanged.manuallyChanged) {
      this.autoUpdateInternalAccounts$.next(dimsChanged);
    }
  }

  customerOnChange(value: any) {
    this.changeCustomer();
  }

  changeCustomer(init = true) {
    const customerId = this.form?.getRawValue().customerId;
  }

  reloadCustomers() {
    this.loadCustomers().subscribe();
  }

  reloadProjects() {
    this.loadProjects().subscribe();
    this.form?.patchValue({ parentProjectId: 0 });
    this.form?.markAsDirty();
  }

  showSelectProject() {
    const dialogData = new SelectProjectDialogData();
    dialogData.title = this.translate.instant(
      'billing.projects.list.searchprojects'
    );

    dialogData.size = 'lg';
    dialogData.projectsWithoutCustomer = true;
    dialogData.excludeProjectId = this.projectId() || 0;

    this.dialogService
      .open(SelectProjectDialogComponent, dialogData)
      .afterClosed()
      .subscribe(result => {
        if (result.projectId) {
          this.form?.patchValue({
            parentProjectId: result.projectId,
          });
          this.form?.markAsDirty();
        }
      });
  }

  private unlockCustomer() {
    this.isLocked.set(true);
    this.form?.customerId.enable();
  }

  private openProjectCentral() {
    const url = `/soe/billing/project/central/?project=${this.form?.getIdControl()?.value}`;

    BrowserUtil.openInNewTab(window, url);
  }

  openProjectUserExpander() {
    if (!this.projectUserRendered) {
      if (this.form?.getIdControl()?.value) {
        this.performLoadData.load(this.loadProjectPersons());
      }
    } else {
      this.projectUserRendered = true;
    }
  }

  private loadProjectPersons() {
    return this.service
      .getProjectPersons(this.form?.getIdControl()?.value, true)
      .pipe(
        tap(x => {
          this.projectUsers = x as IProjectUserExDTO[];
          this.projectUserRendered = true;
        })
      );
  }

  onProjectPersonsChanged(persons: IProjectUserExDTO[]) {
    this.projectUsers = [...persons];
    this.form?.markAsDirty();
  }

  setSuggestedProjectNumber() {
    let projectNumber = this.form?.getRawValue().number;

    if (projectNumber) return;

    if (this.projects) {
      const numerics: number[] = this.projects
        .filter(p => Number(p.number))
        .map(p => +p.number)
        .sort((a, b) => a - b);
      if (numerics.length > 0) {
        projectNumber = (numerics[numerics.length - 1] + 1).toString();
      } else {
        projectNumber = '0';
      }
      this.form?.number.setValue(projectNumber);
    }
  }

  private loadSettingTypes() {
    const keys: string[] = [
      'billing.projects.list.accountincome',
      'billing.projects.list.accountcost',
      'billing.products.products.accountingsettingtype.salesnovat',
      'billing.products.products.accountingsettingtype.salescontractor',
    ];

    return of(
      this.translate.get(keys).subscribe(terms => {
        this.projectAccountSettingTypes = [];
        this.projectAccountSettingTypes.push(
          new SmallGenericType(
            ProjectAccountType.Debit,
            terms['billing.projects.list.accountcost']
          )
        );
        this.projectAccountSettingTypes.push(
          new SmallGenericType(
            ProjectAccountType.Credit,
            terms['billing.projects.list.accountincome']
          )
        );
        this.projectAccountSettingTypes.push(
          new SmallGenericType(
            ProjectAccountType.SalesNoVat,
            terms['billing.products.products.accountingsettingtype.salesnovat']
          )
        );
        this.projectAccountSettingTypes.push(
          new SmallGenericType(
            ProjectAccountType.SalesContractor,
            terms[
              'billing.products.products.accountingsettingtype.salescontractor'
            ]
          )
        );
      })
    );
  }

  projectAccountSettingsChanged(rows: IAccountingSettingsRowDTO[]) {
    this.form?.customProjectAccountingSettingsPatchValue(rows);
  }

  private loadPayrollProductAccountingPriority() {
    this.payrollProductAccountingPrios = [];

    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeProjectPayrollProductAccountingPrio,
        false,
        false
      )
      .pipe(
        tap(x => {
          // this.payrollProductAccountingPrios = x;

          const notUsed = x.find(
            y =>
              y.id === TermGroup_TimeProjectPayrollProductAccountingPrio.NotUsed
          );
          if (notUsed) this.payrollProductAccountingPrios.push(notUsed);

          const employee = x.find(
            y =>
              y.id ===
              TermGroup_TimeProjectPayrollProductAccountingPrio.EmployeeAccount
          );
          if (employee) this.payrollProductAccountingPrios.push(employee);

          const project = x.find(
            y =>
              y.id === TermGroup_TimeProjectPayrollProductAccountingPrio.Project
          );
          if (project) this.payrollProductAccountingPrios.push(project);

          const customer = x.find(
            y =>
              y.id ===
              TermGroup_TimeProjectPayrollProductAccountingPrio.Customer
          );
          if (customer) this.payrollProductAccountingPrios.push(customer);

          const payrollProduct = x.find(
            y =>
              y.id ===
              TermGroup_TimeProjectPayrollProductAccountingPrio.PayrollProduct
          );
          if (payrollProduct)
            this.payrollProductAccountingPrios.push(payrollProduct);

          const employeeGroup = x.find(
            y =>
              y.id ===
              TermGroup_TimeProjectPayrollProductAccountingPrio.EmployeeGroup
          );
          if (employeeGroup)
            this.payrollProductAccountingPrios.push(employeeGroup);
        })
      );
  }

  private loadInvoiceProductAccountingPriority() {
    this.invoiceProductAccountingPrios = [];
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeProjectInvoiceProductAccountingPrio,
        false,
        false
      )
      .pipe(
        tap(x => {
          const notUsed = x.find(
            y =>
              y.id === TermGroup_TimeProjectInvoiceProductAccountingPrio.NotUsed
          );
          if (notUsed) this.invoiceProductAccountingPrios.push(notUsed);

          const employee = x.find(
            y =>
              y.id ===
              TermGroup_TimeProjectInvoiceProductAccountingPrio.EmployeeAccount
          );
          if (employee) this.invoiceProductAccountingPrios.push(employee);

          const project = x.find(
            y =>
              y.id === TermGroup_TimeProjectInvoiceProductAccountingPrio.Project
          );
          if (project) this.invoiceProductAccountingPrios.push(project);

          const customer = x.find(
            y =>
              y.id ===
              TermGroup_TimeProjectInvoiceProductAccountingPrio.Customer
          );
          if (customer) this.invoiceProductAccountingPrios.push(customer);

          const invoiceProduct = x.find(
            y =>
              y.id ===
              TermGroup_TimeProjectInvoiceProductAccountingPrio.InvoiceProduct
          );
          if (invoiceProduct)
            this.invoiceProductAccountingPrios.push(invoiceProduct);
        })
      );
  }

  private loadPayrollProductAccountingPriorityRows() {
    if (!this.form?.payrollProductAccountingPrio.value) {
      this.form?.payrollProductAccountingPrio.setValue('0, 0, 0, 0, 0');
    }

    const accArray: string[] =
      this.form?.payrollProductAccountingPrio.value.split(',');
    let counter = 1;
    accArray.forEach(row => {
      if (counter === 1) this.form?.payrollPrio1.setValue(parseInt(row) || 0);
      if (counter === 2) this.form?.payrollPrio2.setValue(parseInt(row) || 0);
      if (counter === 3) this.form?.payrollPrio3.setValue(parseInt(row) || 0);
      if (counter === 4) this.form?.payrollPrio4.setValue(parseInt(row) || 0);
      if (counter === 5) this.form?.payrollPrio5.setValue(parseInt(row) || 0);

      counter = counter + 1;
    });
  }

  private loadInvoiceProductAccountingPriorityRows() {
    if (!this.form?.invoiceProductAccountingPrio.value) {
      this.form?.invoiceProductAccountingPrio.setValue('0, 0, 0, 0, 0');
    }

    const accArray: string[] =
      this.form?.invoiceProductAccountingPrio.value.split(',');
    let counter = 1;
    accArray.forEach(row => {
      if (counter === 1) this.form?.invoicePrio1.setValue(parseInt(row) || 0);
      if (counter === 2) this.form?.invoicePrio2.setValue(parseInt(row) || 0);
      if (counter === 3) this.form?.invoicePrio3.setValue(parseInt(row) || 0);
      if (counter === 4) this.form?.invoicePrio4.setValue(parseInt(row) || 0);
      if (counter === 5) this.form?.invoicePrio5.setValue(parseInt(row) || 0);

      counter = counter + 1;
    });
  }

  openPricelistExpander() {
    if (!this.pricelistExpanderIsOpen) {
      this.pricelistExpanderIsOpen = true;
      this.performLoadData.load(this.loadProjectPriceListGridRows());
    }
  }

  private loadPriceLists(
    useCache: boolean = false,
    setPriceListId = undefined
  ) {
    this.comparisonPriceLists = [];
    this.projectPriceLists = [];
    return this.translate.get(['billing.projects.list.nopricelist']).pipe(
      tap(term => {
        return this.commonCustomerService
          .getPriceListsDict(false, useCache)
          .subscribe(x => {
            this.projectPriceLists.push(
              new SmallGenericType(0, term['billing.projects.list.nopricelist'])
            );
            x.forEach(y => {
              this.comparisonPriceLists.push(y);
              this.projectPriceLists.push(y);
            });

            if (setPriceListId && setPriceListId > 0)
              this.priceListTypeId = setPriceListId;

            //Set default
            this.comparisonPricelistId = this.defaultPriceListType;
          });
      })
    );
  }

  private loadProjectPriceListGridRows() {
    const ptid = this.comparisonPricelistId ? this.comparisonPricelistId : 0;
    const prid =
      this.form?.getIdControl()?.value && this.form?.priceListTypeId.value
        ? this.form.priceListTypeId.value
        : 0;
    return this.service
      .getProjectPricelists(ptid, prid, this.loadAllProducts, this.priceDate)
      .pipe(
        tap(data => {
          this.projectPriceListGridRows = data.map(x =>
            PriceListDTO.fromServer(x)
          );
        })
      );
  }

  onPricelistChanged(priceListTypeId: number) {
    this.form?.priceListTypeId.setValue(priceListTypeId);
    this.performLoadData.load(this.loadProjectPriceListGridRows());
    this.form?.markAsDirty();
  }

  onComparisonPricelistChanged(priceListTypeId: number) {
    this.comparisonPricelistId = priceListTypeId;
    this.performLoadData.load(this.loadProjectPriceListGridRows());
    this.form?.markAsDirty();
  }

  onPriceDateChanged(pricedate: Date) {
    this.priceDate = DateUtil.getISODateString(pricedate);
    this.performLoadData.load(this.loadProjectPriceListGridRows());
    this.form?.markAsDirty();
  }

  onLoadAllProductsChanged(loadAllProducts: boolean) {
    this.loadAllProducts = loadAllProducts;
    this.performLoadData.load(this.loadProjectPriceListGridRows());
    this.form?.markAsDirty();
  }

  onPricesChanged(changed: boolean) {
    if (changed) {
      this.form?.markAsDirty();
    }
  }

  protected saveProject(options?: ProgressOptions): void {
    if (!this.form || this.form.invalid || !this.service) return;

    const model = new SaveInvoiceProjectModel();

    let invoiceProjectModel = new TimeProjectDTO(
      this.form?.projectStatus.value
    );

    const { projectStatus, ...formValuesWithoutStatus } =
      this.form?.getRawValue();
    invoiceProjectModel = Object.assign(
      invoiceProjectModel,
      formValuesWithoutStatus
    );

    model.invoiceProject = invoiceProjectModel;
    model.invoiceProject.accountingSettings =
      this.form?.projectAccountingSettings.value || [];
    model.invoiceProject.invoiceProductAccountingPrio =
      this.form?.getInvoiceProductAccountingPrio();
    model.invoiceProject.payrollProductAccountingPrio =
      this.form?.getPayrollProductAccountingPrio();

    model.categoryRecords = this.categories;

    model.projectUsers = this.projectUsers.filter(x => x.isModified);

    const priceRows = this.projectPriceListGridRows.map(r =>
      PriceListDTO.fromClient(r)
    );

    model.priceLists = priceRows
      .filter(p => p.isModified)
      .map(p => <IDecimalKeyValue>{ key: p.productId, value: p.price });

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model).pipe(
        tap((response: any) => {
          if (response.integerValue && response.integerValue > 0) {
            this.ProjectAutoUpdateAccountSettings$.next();
            this.updateFormValueAndEmitChange(response);
            if (model.projectUsers.length > 0) {
              this.loadProjectPersons().subscribe();
            }
          }
        })
      ),
      undefined,
      undefined,
      options
    );
  }
}
