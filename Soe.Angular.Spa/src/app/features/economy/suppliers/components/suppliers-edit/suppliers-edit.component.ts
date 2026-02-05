import {
  Component,
  OnDestroy,
  OnInit,
  effect,
  inject,
  signal,
} from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { SupplierService } from '../../../services/supplier.service';
import { SupplierHeadForm } from '../../models/supplier-head-form.model';
import { SupplierDTO } from '../../models/supplier.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { SharedService } from '@shared/services/shared.service';
import {
  CompanySettingType,
  ContactAddressItemType,
  ExternalCompanySearchProvider,
  Feature,
  SoeDataStorageRecordType,
  SoeEntityType,
  SupplierAccountType,
  TermGroup,
  TermGroup_InvoiceVatType,
  TermGroup_Languages,
  TermGroup_SysContactAddressType,
  TermGroup_SysContactEComType,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable, Subject, forkJoin, of, takeUntil, tap } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ExportUtil } from '@shared/util/export-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import {
  IAccountingSettingsRowDTO,
  IPaymentInformationDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ContactPersonDTO } from '@shared/components/contact-persons/models/contact-persons.model';
import { VatCodeService } from '../../../vat-codes/services/vat-codes.service';
import { DeliveryTypesService } from '@src/app/features/billing/delivery-types/services/delivery-types.service';
import { DeliveryConditionService } from '@src/app/features/billing/delivery-condition/services/delivery-condition.service';
import { DeliveryConditionDTO } from '@src/app/features/billing/delivery-condition/models/delivery-condition.model';
import { CommodityCodesService } from '@src/app/features/manage/commodity-codes/services/commodity-codes.service';
import { CategoryItem } from '@shared/components/categories/categories.model';
import { FilesHelper } from '@shared/components/files-helper/files-helper.component';
import { TermCollection } from '@shared/localization/term-types';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IExtraFieldRecordDTO } from '@shared/models/generated-interfaces/ExtraFieldDTO';
import {
  ContactAddressItem,
  getIcon,
} from '@shared/components/contact-addresses/contact-addresses.model';
import { ExternalCompanySearchDialogComponent } from '@shared/components/external-company-search-dialog/external-company-search-dialog.component';
import { ExternalCompanySearchDialogData } from '@shared/components/external-company-search-dialog/models/external-company-search-dialog-data.model';
import { IExternalCompanyAddressDTO } from '@shared/models/generated-interfaces/ExternalCompanyResultDTO';

@Component({
  selector: 'soe-suppliers-edit',
  templateUrl: './suppliers-edit.component.html',
  styleUrls: ['./suppliers-edit.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SuppliersEditComponent
  extends EditBaseDirective<SupplierDTO, SupplierService, SupplierHeadForm>
  implements OnInit, OnDestroy
{
  readonly service = inject(SupplierService);
  readonly coreService = inject(CoreService);
  readonly sharedService = inject(SharedService);
  readonly vatCodeService = inject(VatCodeService);
  readonly deliveryTypesService = inject(DeliveryTypesService);
  readonly deliveryConditionService = inject(DeliveryConditionService);
  readonly commodityCodesService = inject(CommodityCodesService);
  readonly progressService = inject(ProgressService);
  readonly translate = inject(TranslateService);
  readonly dialogService = inject(DialogService);
  readonly _destroy$ = new Subject<void>();

  hideExport = signal(true);
  showExtraFields = signal(false);
  showCommodityCodes = signal(false);
  showTrackChanges = signal(false);
  noteLengthLabel = signal('core.no');

  isExtraFieldsAccordionOpen = false;
  isTrackChangesAccordionOpen = false;
  isAccountingSettingsAccordionOpen = false;
  isContactPersonsAccordionOpen = false;
  isPaymentInformationAccordionOpen = false;
  isFileDisplayAccordionOpen = false;

  consentToolTip = '';
  countries: SmallGenericType[] = [];
  languages: SmallGenericType[] = [];
  currencies: SmallGenericType[] = [];
  accountSettingTypes: SmallGenericType[] = [];
  baseAccounts: SmallGenericType[] = [];
  vatTypes: SmallGenericType[] = [];
  vatCodes: SmallGenericType[] = [];
  paymentConditions: SmallGenericType[] = [];
  factoringSuppliers: SmallGenericType[] = [];
  sysWholesellers: SmallGenericType[] = [];
  attestGroups: SmallGenericType[] = [];
  deliveryTypes: SmallGenericType[] = [];
  deliveryConditions: DeliveryConditionDTO[] = [];
  supplierEmails: SmallGenericType[] = [];
  commodityCodes: SmallGenericType[] = [];
  allowInterim = false;
  filesHelper!: FilesHelper;

  entityType = SoeEntityType.Supplier;

  get supplierId() {
    return this.form?.getIdControl()?.value;
  }

  constructor() {
    super();

    this.filesHelper = new FilesHelper(
      true,
      SoeEntityType.Supplier,
      SoeDataStorageRecordType.SupplierFileAttachment,
      Feature.Economy_Supplier,
      this.performLoadData
    );
  }
  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Economy_Supplier, {
      permission: Feature.Economy_Supplier,
      additionalModifyPermissions: [
        Feature.Economy_Supplier_Suppliers_Edit,
        Feature.Manage_Attest_Supplier_WorkFlowTemplate_Supplier,
        Feature.Economy_Preferences_VoucherSettings_VatCodes,
        Feature.Economy_Preferences_SuppInvoiceSettings_AttestGroups,
        Feature.Economy_Supplier_Suppliers_Documents,
        Feature.Economy_Supplier_Suppliers_TrackChanges,
        Feature.Common_ExtraFields_Supplier,
        Feature.Economy_Intrastat,
      ],
      lookups: [this.loadSettingTypes(), this.executeLookups()],
    });

    this.form?.note.valueChanges
      .pipe(takeUntil(this._destroy$))
      .subscribe(note => {
        this.noteLengthLabel.set(note?.length > 0 ? 'core.yes' : 'core.no');
      });
  }

  override createEditToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('download', {
          iconName: signal('download'),
          caption: signal('common.download'),
          hidden: this.hideExport,
          onAction: () => {
            this.exportSupplier();
          },
        }),
      ],
    });

    if (SoeConfigUtil.sysCountryId === TermGroup_Languages.Finnish) {
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
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    if (this.flowHandler.hasModifyAccess(Feature.Common_ExtraFields_Supplier)) {
      this.showExtraFields.set(true);
    }
    if (this.flowHandler.hasModifyAccess(Feature.Economy_Intrastat)) {
      this.showCommodityCodes.set(true);
    }

    if (
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Supplier_Suppliers_TrackChanges
      )
    ) {
      this.showTrackChanges.set(true);
    }
  }

  override loadData() {
    const supplierId = this.supplierId;
    this.filesHelper.recordId.set(supplierId);
    return this.performLoadData.load$(
      this.service.get(supplierId).pipe(
        tap(value => {
          if (this.supplierId !== supplierId) {
            //Prevent setting values to form if the form's id has changed
            return;
          }

          this.form?.customPatchValue(
            value,
            this.flowHandler.modifyPermission(),
            !this.flowHandler.modifyPermission()
          );

          this.setConsentToolTip(value);
          this.hideExport.set(false);
          this.form?.markAsPristine();
        })
      )
    );
  }

  override newRecord() {
    this.hideExport.set(true);
    return this.performLoadData.load$(
      this.service.getNextSupplierNr().pipe(
        tap(res => {
          this.form?.patchValue({
            active: true,
            interim: this.allowInterim,
            supplierNr: res,
          });
          if (this.currencies.length > 0) {
            this.form?.patchValue({ currencyId: this.currencies[0].id });
          }
        })
      )
    );
  }

  setConsentToolTip(supplier: SupplierDTO) {
    if (supplier.isPrivatePerson) {
      this.consentToolTip = this.terms['common.consentdescr'] + '\n';
      if (supplier.consentModifiedBy) {
        this.consentToolTip =
          this.consentToolTip +
          this.terms['common.modifiedby'] +
          ': ' +
          supplier.consentModifiedBy +
          ' ' +
          supplier.consentModified?.toFormattedDate();
      }
    }
  }

  paymentInformationDomesticChange(row: IPaymentInformationDTO) {
    this.form?.customPaymentInformationDomesticPatchValue(row);
  }

  paymentInformationForeginChange(row: IPaymentInformationDTO) {
    this.form?.customPaymentInformationForeignPatchValue(row);
  }

  accountSettingsChanged(rows: IAccountingSettingsRowDTO[]) {
    this.form?.accountingSettings.rawPatch(rows);
  }

  contactAddressesChanged(rows: ContactAddressItem[]) {
    this.form?.customContactAddressesPatchValue(
      rows,
      this.flowHandler.modifyPermission(),
      !this.flowHandler.modifyPermission()
    );
  }

  contactPersonsChanged(persons: ContactPersonDTO[]) {
    this.form?.customContactPersonsPatchValue(
      persons.map(p => p.actorContactPersonId)
    );
  }

  categoriesChanged(categories: CategoryItem[]) {
    this.form?.customCategoryIdsPatchValue(categories.map(c => c.categoryId));
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'economy.supplier.supplier.supplier',
      'common.consentdescr',
      'common.modifiedby',
      'economy.supplier.supplier.new',
    ]);
  }

  loadCompanySettings() {
    const settingTypes = [
      CompanySettingType.AccountSupplierDebt,
      CompanySettingType.AccountSupplierPurchase,
      CompanySettingType.AccountCommonVatReceivable,
      CompanySettingType.AccountSupplierInterim,
      CompanySettingType.SupplierInvoiceAllowInterim,
    ];

    const getAccountSmallGeneric = (
      accountType: SupplierAccountType,
      setting: CompanySettingType,
      res: any
    ) => {
      return new SmallGenericType(
        accountType,
        SettingsUtil.getIntCompanySetting(res, setting).toString()
      );
    };

    return this.performLoadData.load$(
      this.coreService.getCompanySettings(settingTypes).pipe(
        tap(res => {
          this.baseAccounts = [
            getAccountSmallGeneric(
              SupplierAccountType.Credit,
              CompanySettingType.AccountSupplierDebt,
              res
            ),
            getAccountSmallGeneric(
              SupplierAccountType.Debit,
              CompanySettingType.AccountSupplierPurchase,
              res
            ),
            getAccountSmallGeneric(
              SupplierAccountType.VAT,
              CompanySettingType.AccountCommonVatReceivable,
              res
            ),
            getAccountSmallGeneric(
              SupplierAccountType.Interim,
              CompanySettingType.AccountSupplierInterim,
              res
            ),
          ];
          this.allowInterim = SettingsUtil.getBoolCompanySetting(
            res,
            CompanySettingType.SupplierInvoiceAllowInterim
          );
        })
      )
    );
  }

  loadSettingTypes() {
    const keys: string[] = [
      'economy.supplier.supplier.accountingsettingtype.credit',
      'economy.supplier.supplier.accountingsettingtype.debit',
      'economy.supplier.supplier.accountingsettingtype.vat',
      'economy.supplier.supplier.accountingsettingtype.interim',
    ];
    return this.translate.get(keys).pipe(
      tap(terms => {
        this.accountSettingTypes = [
          new SmallGenericType(
            SupplierAccountType.Credit,
            terms['economy.supplier.supplier.accountingsettingtype.credit']
          ),
          new SmallGenericType(
            SupplierAccountType.Debit,
            terms['economy.supplier.supplier.accountingsettingtype.debit']
          ),
          new SmallGenericType(
            SupplierAccountType.VAT,
            terms['economy.supplier.supplier.accountingsettingtype.vat']
          ),
          new SmallGenericType(
            SupplierAccountType.Interim,
            terms['economy.supplier.supplier.accountingsettingtype.interim']
          ),
        ];
      })
    );
  }

  fileListOpened(opened: boolean) {
    this.isFileDisplayAccordionOpen = opened;
    this.loadFileList();
  }

  loadFileList() {
    if (this.isFileDisplayAccordionOpen) {
      this.performLoadData
        .load$(this.filesHelper.loadFiles(true, true))
        .subscribe();
    }
  }

  executeLookups() {
    const supplierId = this.supplierId;
    return this.performLoadData.load$(
      forkJoin([
        this.coreService.getCountries(true, false),
        this.coreService.getLanguages(true),
        this.sharedService.getCurrencies(true),
        this.coreService.getTermGroupContent(
          TermGroup.InvoiceVatType,
          true,
          false,
          true,
          true
        ),
        this.vatCodeService.getDict(true, true),
        this.service.getSmallGenericTypePaymentConditions(true, true),
        this.service.getSupplierDict(true, true, true),
        this.service.getSmallGenericSysWholesellers(true, true),
        this.service.getAttestWorkFlowGroupsDict(true),
        this.deliveryTypesService.getDeliveryTypesDict(true, true),
        this.deliveryConditionService.getDeliveryConditionsDict(true, true),
        supplierId
          ? this.service.getSupplierEmails(supplierId, true, true)
          : of([]),
        this.commodityCodesService.getCustomerCommodyCodesDict(true, true),
      ]).pipe(
        tap(
          ([
            countries,
            languages,
            currencies,
            vatTypes,
            vatCodes,
            paymentConditions,
            suppliers,
            sysWholesellers,
            attestGroups,
            deliveryTypes,
            deliveryConditions,
            supplierEmails,
            commodityCodes,
          ]) => {
            this.countries = countries;
            this.languages = languages;
            this.currencies = currencies;
            this.vatTypes = vatTypes.filter(
              x => x.id < TermGroup_InvoiceVatType.ExportWithinEU
            );
            this.vatCodes = vatCodes;
            this.paymentConditions = paymentConditions;
            this.factoringSuppliers = suppliers.filter(
              x => x.id !== supplierId
            );
            this.sysWholesellers = sysWholesellers;
            this.attestGroups = attestGroups;
            this.deliveryTypes = deliveryTypes;
            this.deliveryConditions = deliveryConditions;
            this.supplierEmails = supplierEmails;
            this.commodityCodes = commodityCodes;
          }
        )
      )
    );
  }

  exportSupplier() {
    new Perform<any>(this.progressService).load(
      this.service.getSupplierForExport(this.supplierId).pipe(
        tap(supplier => {
          ExportUtil.Export(supplier, 'supplier.json');
        })
      )
    );
  }

  uplooadedFilesChanged(files: any[]) {
    this.additionalSaveData = {
      ...this.additionalSaveData,
      files: [...this.filesHelper.files],
    };
  }

  extraFieldsChanged(items: IExtraFieldRecordDTO[]) {
    this.additionalSaveData = {
      ...this.additionalSaveData,
      extraFields: items,
    };
    this.form?.markAsDirty();
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
          registrationNr: this.form?.get('orgNr')?.value ?? '',
          name: this.form?.get('name')?.value ?? '',
        },
      })
      .afterClosed()
      .subscribe((response?: ExternalCompanySearchDialogData) =>
        this.processExCompanyResult(response)
      );
  }

  private processExCompanyResult(
    response?: ExternalCompanySearchDialogData
  ): void {
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

  ngOnDestroy(): void {
    this._destroy$.next();
    this._destroy$.complete();
  }
}
