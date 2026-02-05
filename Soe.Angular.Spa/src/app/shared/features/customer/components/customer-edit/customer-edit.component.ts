import { Component, effect, inject, OnInit, signal } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  CustomerUserDTO,
  SaveCustomerModel,
} from '../../models/customer.model';
import { CustomerService } from '../../services/customer.service';
import { CustomerForm } from '../../models/customer-form.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  CompanySettingType,
  ContactAddressItemType,
  CustomerAccountType,
  ExternalCompanySearchProvider,
  Feature,
  SoeDataStorageRecordType,
  SoeEntityType,
  SoeInvoiceDeliveryType,
  SoeReportTemplateType,
  TermGroup,
  TermGroup_EInvoiceFormat,
  TermGroup_Languages,
  TermGroup_SysContactAddressType,
  TermGroup_SysContactEComType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ProgressOptions } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import { ISaveCustomerModel } from '@shared/models/generated-interfaces/CoreModels';
import { distinctUntilChanged, Observable, of, take, tap } from 'rxjs';
import {
  IAccountingSettingsRowDTO,
  ICustomerUserDTO,
  IHouseholdTaxDeductionApplicantDTO,
  IUserSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  SelectUsersDialogData,
  UserSmallDTO,
} from '@shared/components/billing/select-users-dialog/models/select-users-dialog.model';
import { AttachedFile } from '@ui/forms/file-upload/file-upload.component';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { SelectUsersDialogComponent } from '@shared/components/billing/select-users-dialog/components/select-users-dialog/select-users-dialog.component';
import { ContactPersonDTO } from '@shared/components/contact-persons/models/contact-persons.model';
import { CommonCustomerService } from '@billing/shared/services/common-customer.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { SettingsUtil } from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { ReportService } from '@shared/services/report.service';
import { CategoryItem } from '@shared/components/categories/categories.model';
import { FilesHelper } from '@shared/components/files-helper/files-helper.component';
import { IExtraFieldRecordDTO } from '@shared/models/generated-interfaces/ExtraFieldDTO';
import {
  ContactAddressItem,
  getIcon,
} from '@shared/components/contact-addresses/contact-addresses.model';
import { ExternalCompanySearchDialogComponent } from '@shared/components/external-company-search-dialog/external-company-search-dialog.component';
import { ExternalCompanySearchDialogData } from '@shared/components/external-company-search-dialog/models/external-company-search-dialog-data.model';
import { IExternalCompanyAddressDTO } from '@shared/models/generated-interfaces/ExternalCompanyResultDTO';
import {
  EInvoiceRecipientModelDTO,
  SearchEinvoiceRecipientDialogData,
} from '../../models/search-einvoice-recipient-dialog.model';
import { SearchEinvoiceRecipientDialogComponent } from '../search-einvoice-recipient-dialog/search-einvoice-recipient-dialog.component';

@Component({
  selector: 'soe-customer-edit',
  templateUrl: './customer-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CustomerEditComponent
  extends EditBaseDirective<ISaveCustomerModel, CustomerService, CustomerForm>
  implements OnInit
{
  service = inject(CustomerService);
  coreService = inject(CoreService);
  commonCustomerService = inject(CommonCustomerService);
  dialogService = inject(DialogService);
  messageboxService = inject(MessageboxService);
  reportService = inject(ReportService);

  disabledParticipantButton = signal(false);

  // Permissions

  modifyUsersPermission: boolean = false;
  finvoicePermission: boolean = false;
  eInvoicePermission: boolean = false;
  archivePermission: boolean = false;
  documentsPermission: boolean = false;
  private modifyHHTDApplicantsPermission: boolean = false; // Edit Household Tax Deduction Applicants
  hasExtraFieldPermission: boolean = false;
  finvoiceLinkLabel!: string;

  //Lookups
  customers: ISmallGenericType[] = [];
  countries: ISmallGenericType[] = [];
  languages: ISmallGenericType[] = [];
  currencies: ISmallGenericType[] = [];
  customerUsers: CustomerUserDTO[] = [];
  selectedUsers: IUserSmallDTO[] = [];
  vatTypes: ISmallGenericType[] = [];
  priceLists: ISmallGenericType[] = [];
  wholesellers: ISmallGenericType[] = [];
  invoiceDeliveryTypes: ISmallGenericType[] = [];
  invoiceDeliveryProviders: ISmallGenericType[] = [];
  emails: ISmallGenericType[] = [];
  customerGLNs: ISmallGenericType[] = [];
  deliveryTypes: ISmallGenericType[] = [];
  deliveryConditions: ISmallGenericType[] = [];
  paymentConditions: ISmallGenericType[] = [];
  invoicePaymentServices: ISmallGenericType[] = [];
  agreementTemplates: ISmallGenericType[] = [];
  orderTemplates: ISmallGenericType[] = [];
  offerTemplates: ISmallGenericType[] = [];
  billingTemplates: ISmallGenericType[] = [];

  contactExpanderRendered: boolean = false;
  accountExpanderRendered: boolean = false;
  rotExpanderRendered: boolean = false;
  productsExpanderRendered: boolean = false;
  statisticsExpanderRendered: boolean = false;
  extraFieldsExpanderRendered: boolean = false;

  entityType = SoeEntityType.Customer;

  //Settings Section
  eInvoiceFormat!: number;

  // CompanySettings
  defaultGracePeriodDays: number = 0;
  setOwnerAutomatically: boolean = false;
  useDeliveryCustomer: boolean = false;
  useInvoiceDeliveryProvider: boolean = false;
  hideTaxDeductionContacts: boolean = false;
  isAdditionalDiscount: boolean = false;

  filesHelper!: FilesHelper;

  settingTypes!: SmallGenericType[];
  baseAccounts!: SmallGenericType[];

  contactPersons: ContactPersonDTO[] = [];
  taxDeductionContacts: IHouseholdTaxDeductionApplicantDTO[] = [];
  extraFieldRecords: IExtraFieldRecordDTO[] = [];

  private currentId = 0;
  private copyContextApplied = false;

  constructor() {
    super();

    this.filesHelper = new FilesHelper(
      true,
      SoeEntityType.Customer,
      SoeDataStorageRecordType.CustomerFileAttachment,
      Feature.Billing_Customer,
      this.performLoadData
    );

    effect(() => {
      if (this.copyContextApplied) return; //to prevent re-runs

      const copyAction = this.copyActionTakenSignal()?.();
      if (
        this.form?.isCopy &&
        copyAction?.additionalProps?.copiedFromCustomerId
      ) {
        this.currentId = copyAction.additionalProps.copiedFromCustomerId;
        this.filesHelper.recordId.set(this.currentId);

        this.copyContextApplied = true;
      }
    });
  }

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Billing_Customer_Customers_Edit, {
      additionalReadPermissions: [
        Feature.Billing_Customer_Customers_Edit,
        Feature.Economy_Customer_Customers_Edit,
        Feature.Billing_Customer_Customers_Edit_Users,
        Feature.Economy_Customer_Customers_Edit_Users,
        Feature.Billing_Customer_Customers_Edit_Documents,
        Feature.Economy_Customer_Customers_Edit_Documents,
        Feature.Billing_Customer_Customers_Edit_HouseholdTaxDeductionApplicants,
        Feature.Billing_Invoice_Invoices_Edit_EInvoice,
        Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice,
        Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura,
        Feature.Archive,
      ],
      additionalModifyPermissions: [
        Feature.Billing_Customer_Customers_Edit,
        Feature.Economy_Customer_Customers_Edit,
        Feature.Billing_Customer_Customers_Edit_Users,
        Feature.Economy_Customer_Customers_Edit_Users,
        Feature.Billing_Customer_Customers_Edit_Documents,
        Feature.Economy_Customer_Customers_Edit_Documents,
        Feature.Billing_Customer_Customers_Edit_HouseholdTaxDeductionApplicants,
        Feature.Billing_Invoice_Invoices_Edit_EInvoice,
        Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice,
        Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura,
        Feature.Archive,
        Feature.Common_ExtraFields_Customer,
      ],
      lookups: [
        this.loadCustomers(),
        this.loadCompanySettings(),
        this.loadCountries(),
        this.loadLanguages(),
        this.loadCurrencies(),
        this.loadUsers(),
        this.loadVatTypes(),
        this.loadPriceLists(),
        this.loadWholeSellers(),
        this.loadInvoiceDeliveryTypes(),
        this.loadEmails(),
        this.loadCustomerGLNs(),
        this.loadDeliveryTypes(),
        this.loadDeliveryConditions(),
        this.loadPaymentConditions(),
        this.loadInvoicePaymentServices(),
        this.loadAgreementTemplates(),
        this.loadOfferTemplates(),
        this.loadOrderTemplates(),
        this.loadBillingTemplates(),
        this.loadSettingTypes(),
        this.loadInvoiceDeliveryProviders(),
      ],
    });

    this.translate
      .get(['common.customer.customer.finvoice.search'])
      .pipe(take(1))
      .subscribe(
        terms =>
          (this.finvoiceLinkLabel =
            terms['common.customer.customer.finvoice.search'])
      );

    this.form?.hasConsent.valueChanges
      .pipe(distinctUntilChanged())
      .subscribe(value => {
        if (value) this.form?.consentDate.enable();
        else this.form?.consentDate.disable();
      });

    this.form?.isFinvoiceCustomer.valueChanges
      .pipe(distinctUntilChanged())
      .subscribe(value => {
        if (value) {
          this.form?.finvoiceAddress.enable();
          this.form?.finvoiceOperator.enable();
        } else {
          this.form?.finvoiceAddress.disable();
          this.form?.finvoiceOperator.disable();
        }
      });
  }

  override createEditToolbar(): void {
    super.createEditToolbar();

    if (SoeConfigUtil.sysCountryId === TermGroup_Languages.Finnish) {
      this.toolbarService.createItemGroup({
        items: [
          this.toolbarService.createToolbarButton('YTJWEB', {
            iconName: signal('globe-europe'),
            caption: signal('YTJ Web'),
            tooltip: signal('common.external.company.search.web.tooltip'),
            onAction: (): void => {
              const url = this.form?.orgNr.value
                ? `https://tietopalvelu.ytj.fi/yritys/${this.form?.orgNr.value}`
                : `https://tietopalvelu.ytj.fi/?isCompanyValid=true&isCompanyTerminated=true&companyName=${this.form?.name.value}`;
              BrowserUtil.openInNewTab(window, url);
            },
          }),
        ],
      });

      this.toolbarService.createItemGroup({
        items: [
          this.toolbarService.createToolbarButton('YTJAPI', {
            iconName: signal('gear-complex-code'),
            caption: signal('YTJ API'),
            tooltip: signal('common.customer.customer.ytj.search'),
            onAction: this.showExternalCompanySearch.bind(this),
          }),
        ],
      });
    }

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('search', {
          iconName: signal('magnifying-glass'),
          caption: signal(
            this.translate.instant(
              'common.customer.customer.searcheinvoicerecipient'
            )
          ),
          onAction: this.showCustomerLookup.bind(this),
          hidden: signal(
            this.eInvoiceFormat !== TermGroup_EInvoiceFormat.SvefakturaAPI
          ),
        }),
      ],
    });
  }

  override loadData(): Observable<void> {
    this.filesHelper.recordId.set(this.form?.getIdControl()?.value);

    return this.performLoadData.load$(
      this.service
        .get(
          this.form?.getIdControl()?.value,
          false,
          false,
          true,
          true,
          true,
          true
        )
        .pipe(
          tap(value => {
            this.form?.reset(value);
            this.form?.customContactAddressesPatchValue(
              value.contactAddresses,
              true,
              true
            );
            this.form?.customProductRowsPatchValues(value.customerProducts);
            this.form?.customOriginUsersPatchValue(value.customerUsers);
          })
        )
    );
  }

  override newRecord(): Observable<void> {
    return super.newRecord().pipe(
      tap(() => {
        if (this.currencies.length) {
          this.form!.currencyId.setValue(this.currencies[0].id);
        }
      })
    );
  }

  override copy(): void {
    this.currentId = this.form?.getIdControl()?.value;
    this.form!.customerNr.setValue('');

    super.copy({
      copiedFromCustomerId: this.currentId,
    });
  }

  override onPermissionsLoaded() {
    const readPermission =
      this.flowHandler.hasReadAccess(Feature.Billing_Customer_Customers_Edit) ||
      this.flowHandler.hasReadAccess(Feature.Economy_Customer_Customers_Edit);

    const modifyPermission =
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Customer_Customers_Edit
      ) ||
      this.flowHandler.hasModifyAccess(Feature.Economy_Customer_Customers_Edit);

    this.flowHandler.readPermission.set(readPermission);
    this.flowHandler.modifyPermission.set(modifyPermission);
    super.onPermissionsLoaded();

    this.modifyUsersPermission =
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Customer_Customers_Edit_Users
      ) ||
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Customer_Customers_Edit_Users
      );

    this.modifyHHTDApplicantsPermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Customer_Customers_Edit_HouseholdTaxDeductionApplicants
    );
    this.finvoicePermission = this.flowHandler.hasModifyAccess(
      Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice
    );
    this.eInvoicePermission =
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Invoice_Invoices_Edit_EInvoice
      ) ||
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateFinvoice
      ) ||
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Invoice_Invoices_Edit_EInvoice_CreateSvefaktura
      );

    this.archivePermission = this.flowHandler.hasModifyAccess(Feature.Archive);
    this.documentsPermission =
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Customer_Customers_Edit_Documents
      ) ||
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Customer_Customers_Edit_Documents
      );
    this.hasExtraFieldPermission = this.flowHandler.hasModifyAccess(
      Feature.Common_ExtraFields_Customer
    );
  }

  override loadCompanySettings() {
    const settingTypes: number[] = [
      CompanySettingType.CustomerGracePeriodDays,
      CompanySettingType.BillingAutomaticCustomerOwner,
      CompanySettingType.CustomerInvoiceUseDeliveryCustomer,
      CompanySettingType.BillingEInvoiceFormat,
      CompanySettingType.BillingUseInvoiceDeliveryProvider,
      CompanySettingType.BillingCustomerHideTaxDeductionContacts,
      CompanySettingType.BillingUseAdditionalDiscount,

      // Base accounts
      CompanySettingType.AccountCustomerClaim,
      CompanySettingType.AccountCustomerSalesVat,
      CompanySettingType.AccountCommonVatPayable1,
    ];

    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(x => {
        this.defaultGracePeriodDays = SettingsUtil.getIntCompanySetting(
          x,
          CompanySettingType.CustomerGracePeriodDays
        );
        this.setOwnerAutomatically = SettingsUtil.getBoolCompanySetting(
          x,
          CompanySettingType.BillingAutomaticCustomerOwner
        );
        this.useDeliveryCustomer = SettingsUtil.getBoolCompanySetting(
          x,
          CompanySettingType.CustomerInvoiceUseDeliveryCustomer
        );
        this.eInvoiceFormat = SettingsUtil.getIntCompanySetting(
          x,
          CompanySettingType.BillingEInvoiceFormat
        );
        this.useInvoiceDeliveryProvider = SettingsUtil.getBoolCompanySetting(
          x,
          CompanySettingType.BillingUseInvoiceDeliveryProvider
        );
        this.hideTaxDeductionContacts = SettingsUtil.getBoolCompanySetting(
          x,
          CompanySettingType.BillingCustomerHideTaxDeductionContacts
        );
        this.isAdditionalDiscount = SettingsUtil.getBoolCompanySetting(
          x,
          CompanySettingType.BillingUseAdditionalDiscount
        );

        // Base accounts
        this.baseAccounts = [];
        this.baseAccounts.push(
          new SmallGenericType(
            CustomerAccountType.Debit,
            SettingsUtil.getIntCompanySetting(
              x,
              CompanySettingType.AccountCustomerClaim
            ).toString()
          )
        );
        this.baseAccounts.push(
          new SmallGenericType(
            CustomerAccountType.Credit,
            SettingsUtil.getIntCompanySetting(
              x,
              CompanySettingType.AccountCustomerSalesVat
            ).toString()
          )
        );
        this.baseAccounts.push(
          new SmallGenericType(
            CustomerAccountType.VAT,
            SettingsUtil.getIntCompanySetting(
              x,
              CompanySettingType.AccountCommonVatPayable1
            ).toString()
          )
        );
      })
    );
  }

  loadSettingTypes() {
    const keys: string[] = [
      'common.customer.customer.accountingsettingtype.credit',
      'common.customer.customer.accountingsettingtype.debit',
      'common.customer.customer.accountingsettingtype.vat',
    ];

    return this.translate.get(keys).pipe(
      take(1),
      tap(term => {
        this.settingTypes = [];
        this.settingTypes.push(
          new SmallGenericType(
            CustomerAccountType.Debit,
            term['common.customer.customer.accountingsettingtype.debit']
          )
        );
        this.settingTypes.push(
          new SmallGenericType(
            CustomerAccountType.Credit,
            term['common.customer.customer.accountingsettingtype.credit']
          )
        );
        this.settingTypes.push(
          new SmallGenericType(
            CustomerAccountType.VAT,
            term['common.customer.customer.accountingsettingtype.vat']
          )
        );
      })
    );
  }

  loadCustomers() {
    if (this.useDeliveryCustomer) {
      return this.commonCustomerService
        .getCustomersDict(true, true, true)
        .pipe(tap(x => (this.customers = x)));
    } else {
      return of(undefined);
    }
  }

  loadCountries() {
    return this.coreService.getCountries(true, false).pipe(
      tap(x => {
        this.countries = x;
      })
    );
  }

  loadLanguages() {
    return this.coreService
      .getLanguages(true)
      .pipe(tap(x => (this.languages = x)));
  }

  loadCurrencies() {
    return this.coreService.getCompCurrenciesDict(false).pipe(
      tap(x => {
        this.currencies = x;
      })
    );
  }

  loadUsers() {
    return this.coreService.getUsers(false, true, false, false, true).pipe(
      tap(x => {
        x.forEach(value => {
          const c = new CustomerUserDTO();
          c.userId = value.userId;
          c.name = value.name;
          c.main = value.main;
          this.customerUsers.push(c);
        });
      })
    );
  }

  loadVatTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.InvoiceVatType, true, false)
      .pipe(
        tap(x => {
          this.vatTypes = x;
        })
      );
  }

  loadPriceLists() {
    return this.commonCustomerService
      .getPriceListsDict(true, false)
      .pipe(tap(x => (this.priceLists = [{ id: 0, name: '' }, ...x])));
  }

  loadWholeSellers() {
    return this.commonCustomerService
      .getSysWholesellersDict(true)
      .pipe(tap(x => (this.wholesellers = x)));
  }

  loadInvoiceDeliveryTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.InvoiceDeliveryType, true, false)
      .pipe(
        tap(x => {
          this.invoiceDeliveryTypes = x;

          if (this.eInvoiceFormat != TermGroup_EInvoiceFormat.Intrum) {
            this.invoiceDeliveryTypes = this.invoiceDeliveryTypes.filter(
              x => x.id !== SoeInvoiceDeliveryType.EDI
            );
          }
          if (!this.eInvoicePermission) {
            this.invoiceDeliveryTypes = this.invoiceDeliveryTypes.filter(
              y => y.id !== SoeInvoiceDeliveryType.Electronic
            );
          }
        })
      );
  }

  loadInvoiceDeliveryProviders() {
    return this.coreService
      .getTermGroupContent(TermGroup.InvoiceDeliveryProvider, true, false)
      .pipe(
        tap(x => {
          this.invoiceDeliveryProviders = x;
        })
      );
  }

  loadEmails() {
    return this.commonCustomerService
      .getCustomerEmails(this.form?.actorCustomerId.value, true, true)
      .pipe(
        tap(x => {
          this.emails = x;
        })
      );
  }

  loadCustomerGLNs() {
    return this.commonCustomerService
      .getCustomerGLNs(this.form?.actorCustomerId.value, true)
      .pipe(tap(x => (this.customerGLNs = x)));
  }

  getFinvoiceSearchUrl() {
    return (
      'https://verkkolaskuosoite.fi/client/index.html#/?searchText=' +
      (this.form?.orgNr.value || this.form?.name.value)
    );
  }

  loadDeliveryTypes() {
    return this.commonCustomerService
      .getDeliveryTypesDict(true)
      .pipe(tap(x => (this.deliveryTypes = x)));
  }

  loadDeliveryConditions() {
    return this.commonCustomerService
      .getDeliveryConditionsDict(true)
      .pipe(tap(x => (this.deliveryConditions = x)));
  }

  loadPaymentConditions() {
    return this.commonCustomerService
      .getPaymentConditionsDict(true)
      .pipe(tap(x => (this.paymentConditions = x)));
  }

  loadInvoicePaymentServices() {
    return this.coreService
      .getTermGroupContent(TermGroup.InvoicePaymentService, true, false)
      .pipe(tap(x => (this.invoicePaymentServices = x)));
  }

  loadAgreementTemplates() {
    return this.reportService
      .getReportsDict(
        SoeReportTemplateType.BillingContract,
        false,
        false,
        true,
        false
      )
      .pipe(
        tap(x => {
          this.agreementTemplates = x;
        })
      );
  }

  loadOfferTemplates() {
    return this.reportService
      .getReportsDict(
        SoeReportTemplateType.BillingOffer,
        false,
        false,
        true,
        false
      )
      .pipe(tap(x => (this.offerTemplates = x)));
  }

  loadOrderTemplates() {
    return this.reportService
      .getReportsDict(
        SoeReportTemplateType.BillingOrder,
        false,
        false,
        true,
        false
      )
      .pipe(tap(x => (this.orderTemplates = x)));
  }

  loadBillingTemplates() {
    return this.reportService
      .getReportsDict(
        SoeReportTemplateType.BillingInvoice,
        false,
        false,
        true,
        false
      )
      .pipe(tap(x => (this.billingTemplates = x)));
  }

  contactAddressesChanged(rows: ContactAddressItem[]) {
    this.form?.customContactAddressesPatchValue(
      rows,
      this.flowHandler.modifyPermission(),
      !this.flowHandler.modifyPermission()
    );
  }

  openSelectUser() {
    const dialogData = new SelectUsersDialogData();
    dialogData.title = 'common.customer.customer.selectusers';
    dialogData.size = 'lg';
    dialogData.showMain = true;
    dialogData.showParticipant = false;
    this.selectedUsers = [];

    this.form?.value.customerUsers.forEach((user: any) => {
      const u = new UserSmallDTO();
      u.name = user.name;
      u.main = user.main;
      u.userId = user.userId;
      this.selectedUsers.push(u);
    });
    dialogData.selectedUsers = this.selectedUsers;

    const userSelectDialog = this.dialogService.open(
      SelectUsersDialogComponent,
      dialogData
    );

    userSelectDialog.afterClosed().subscribe((res: IUserSmallDTO[]) => {
      const originUsers: ICustomerUserDTO[] = [];
      if (res) {
        res.forEach(user => {
          if (user) {
            const o = new CustomerUserDTO();
            o.userId = user.userId;
            o.main = user.main;
            o.name = user.name;
            originUsers.push(o);
          }
        });
        this.form?.customOriginUsersPatchValue(originUsers);
        this.form?.markAsDirty();
      }
    });
  }

  contactPersonsChanged(persons: ContactPersonDTO[]) {
    if (!persons) return;

    this.form?.customContactPersonsPatchValue(
      persons.map(p => p.actorContactPersonId)
    );
  }

  override performSave(options?: ProgressOptions): void {
    if (!this.form || this.form.invalid || !this.service) return;

    const model = new SaveCustomerModel();
    model.customer = this.form.getRawValue();
    model.contactPersons = this.contactPersons;
    model.extraFields = this.extraFieldRecords;
    model.houseHoldTaxApplicants = this.taxDeductionContacts;

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model),
      this.updateFormValueAndEmitChange,
      undefined,
      options
    );
  }

  showPayingCustomerNote() {
    if (this.form?.payingCustomerId.value) {
      this.commonCustomerService
        .getCustomer(
          this.form?.payingCustomerId.value,
          false,
          false,
          true,
          false,
          false,
          false
        )
        .subscribe(x => {
          const payingCustomer = x;
          this.messageboxService.show(
            this.translate.instant('common.customer.customer.customernote'),
            ' ',
            {
              type: 'information',
              showInputText: true,
              inputTextLabel: 'common.customer.customer.customernote',
              inputTextValue: payingCustomer.note ?? '',
              buttons: 'ok',
            }
          );
        });
    }
  }

  categoriesChanged(categories: CategoryItem[]) {
    this.form?.customCategoryIdsPatchValue(categories.map(c => c.categoryId));
  }

  accountSettingsChanged(rows: IAccountingSettingsRowDTO[]) {
    this.form?.customAccountingSettingsPathValue(rows);
  }

  extraFieldsChanged(items: IExtraFieldRecordDTO[]) {
    this.extraFieldRecords = items;
    this.form?.markAsDirty();
  }

  onExtraFieldsExpanderOpenClose() {
    this.extraFieldRecords = [];
    this.extraFieldsExpanderRendered = !this.extraFieldsExpanderRendered;
  }

  taxDeductionContactChanged(contacts: IHouseholdTaxDeductionApplicantDTO[]) {
    this.taxDeductionContacts = contacts;
  }

  private showExternalCompanySearch(): void {
    this.dialogService
      .open(ExternalCompanySearchDialogComponent, <
        Partial<ExternalCompanySearchDialogData>
      >{
        title: 'common.external.company.search.dialog.title',
        size: 'lg',
        searchProvider: ExternalCompanySearchProvider.PRH,
        source: 'YTJ',
        searchFilter: {
          registrationNr: this.form?.orgNr.value ?? '',
          name: this.form?.name.value ?? '',
        },
      })
      .afterClosed()
      .subscribe((response?: ExternalCompanySearchDialogData) => {
        if (response && response.result) {
          this.form?.patchValue({
            orgNr: response.result.registrationNr,
            name: response.result.name,
          });

          const addresses: ContactAddressItem[] = [];

          if (response.result.postalAddress) {
            addresses.push(
              this.getAddress(
                ContactAddressItemType.AddressDistribution,
                TermGroup_SysContactAddressType.Distribution,
                response.result.postalAddress,
                this.translate.instant(
                  'common.contactaddresses.addressmenu.distribution'
                ),
                response.source
              )
            );
          }

          if (response.result.streetAddress) {
            addresses.push(
              this.getAddress(
                ContactAddressItemType.AddressVisiting,
                TermGroup_SysContactAddressType.Visiting,
                response.result.streetAddress,
                this.translate.instant(
                  'common.contactaddresses.addressmenu.visiting'
                ),
                response.source
              )
            );
          }

          if (response.result.streetAddress || response.result.postalAddress) {
            addresses.push(
              this.getAddress(
                ContactAddressItemType.AddressBilling,
                TermGroup_SysContactAddressType.Billing,
                response.result.postalAddress ?? response.result.streetAddress,
                this.translate.instant(
                  'common.contactaddresses.addressmenu.billing'
                ),
                response.source
              )
            );
          }

          if (response.result.webUrl) {
            const url = new ContactAddressItem();
            url.eComText = response.result.webUrl;
            url.contactAddressItemType = ContactAddressItemType.EComWeb;
            url.sysContactEComTypeId = TermGroup_SysContactEComType.Web;
            url.name = `${this.translate.instant('common.contactaddresses.ecommenu.web')} (${this.translate.instant('core.source')}: ${response.source})`;

            addresses.push(url);
          }

          if (addresses.length > 0) {
            this.form?.customContactAddressesPatchValue(
              addresses,
              this.flowHandler.modifyPermission(),
              !this.flowHandler.modifyPermission()
            );
          }
          this.form?.markAsDirty();
        }
      });
  }

  private getAddress(
    addressType: ContactAddressItemType,
    addressSysType: TermGroup_SysContactAddressType,
    addressInput: IExternalCompanyAddressDTO,
    title: string,
    source: string
  ): ContactAddressItem {
    const address = new ContactAddressItem();

    if (addressType === ContactAddressItemType.AddressVisiting)
      address.streetAddress = addressInput.addressLine1;
    else address.address = addressInput.addressLine1;

    address.postalCode = addressInput.zipCode;
    address.postalAddress = addressInput.city;
    address.addressCO = addressInput.co;
    address.contactAddressItemType = addressType;
    address.sysContactAddressTypeId = addressSysType;
    address.typeIcon = getIcon(addressType);
    address.isAddress = true;
    address.name = `${title} (${this.translate.instant('core.source')}: ${source})`;
    return address;
  }

  showCustomerLookup() {
    const dialogData = new SearchEinvoiceRecipientDialogData();
    dialogData.title = 'Search e-invoice recipient';
    dialogData.size = 'lg';

    this.dialogService
      .open(SearchEinvoiceRecipientDialogComponent, dialogData)
      .afterClosed()
      .subscribe(result => {
        if (result) {
          this.updateCustomerRecipient(result);
        }
      });
  }

  updateCustomerRecipient(recipient: EInvoiceRecipientModelDTO) {
    if (!recipient) return;
    if (recipient.gln) {
      const addressItem = new ContactAddressItem();
      addressItem.name = this.translate.instant(
        'common.contactaddresses.ecommenu.gln'
      );
      addressItem.eComText = recipient.gln;
      addressItem.contactAddressItemType = ContactAddressItemType.GlnNumber;
      addressItem.sysContactEComTypeId = TermGroup_SysContactEComType.GlnNumber;
      addressItem.typeIcon = getIcon(ContactAddressItemType.GlnNumber);
      this.form?.customContactAddressesPatchValue(
        [...this.form.contactAddresses.value, addressItem],
        true,
        false
      );
    }
    if (recipient.vatNo) this.form?.vatNr.setValue(recipient.vatNo);
    if (recipient.orgNo) this.form?.orgNr.setValue(recipient.orgNo);
    this.form?.contactAddresses.markAllAsDirty();
    this.form?.markAsDirty();
  }

  openFinvoiceAddressSearch() {
    const url = this.getFinvoiceSearchUrl();
    BrowserUtil.openInNewTab(window, url);
  }
}
