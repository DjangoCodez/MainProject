import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { SupplierDTO } from "../../../../Common/Models/supplierdto";
import { SmallGenericType } from "../../../../Common/Models/smallgenerictype";
import { IFocusService } from "../../../../Core/Services/FocusService";
import { IAccountingService } from "../../Accounting/AccountingService";
import { ISupplierService } from "../SupplierService";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { Guid } from "../../../../Util/StringUtility";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { ContactAddressItemDTO } from "../../../../Common/Models/ContactAddressDTOs";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { Constants } from "../../../../Util/Constants";
import { Feature, SoeEntityState, CompanySettingType, SupplierAccountType, TermGroup, SoeEntityType, SoeEntityImageType } from "../../../../Util/CommonEnumerations";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../../Util/Enumerations";
import { ExportUtility } from "../../../../Util/ExportUtility";
import { FilesHelper } from "../../../../Common/Files/FilesHelper";
import { ICommonCustomerService } from "../../../../Common/Customer/CommonCustomerService";
import { ExtraFieldGridDTO, ExtraFieldRecordDTO } from "../../../../Common/Models/ExtraFieldDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private modal;
    isModal = false;

    // Data
    private supplierId: number;
    private supplier: SupplierDTO;
    terms: any = [];

    // Helpers
    private filesHelper: FilesHelper;

    // Lookups 
    private countries: any[];
    private languages: any[];
    private currencies: any[];
    private vatTypes: any[];
    private vatCodes: any[];
    private paymentConditions: any[];
    private factoringSuppliers: any[];
    private sysWholesellers: any[];
    private attestGroups: any[];
    private deliveryTypes: SmallGenericType[] = [];
    private deliveryConditions: SmallGenericType[] = [];
    private supplierEmails: SmallGenericType[] = [];
    private extraFieldRecords: ExtraFieldRecordDTO[];
    private commodityCodes: any[];

    // Permissions
    public documentsPermission = false;
    public trackChangesPermission = false;
    hasExtraFieldPermission = false;
    hasCommodityCodesPermission = false;

    // CompanySettings
    private settingTypes: SmallGenericType[];
    private baseAccounts: SmallGenericType[];
    private allowInterim = false;

    // Properties
    public supplierIsLoaded = false;
    private documentExpanderIsOpen = false;
    private trackChangesRendered = false;

    private isContactAddressesValid = true;
    private contactAddressesValidationErrors: string;
    private consentToolTip: string;

    set hasConsent(value: any) {
        this.supplier.hasConsent = value;
        if (this.supplier.hasConsent && !this.supplier.consentDate) {
            this.supplier.consentDate = CalendarUtility.getDateToday();
        }
    }

    get hasConsent() {
        return this.supplier.hasConsent;
    }

    // Flags
    private updateCaption = false;
    extraFieldsExpanderRendered = false;

    // Extra fields
    private extraFields: ExtraFieldGridDTO[] = [];
    get showExtraFieldsExpander() {
        return this.hasExtraFieldPermission;
    }

    private _selectedCommodityCode;
    get selectedCommodityCode() {
        return this._selectedCommodityCode;
    }
    set selectedCommodityCode(item: any) {
        this._selectedCommodityCode = item;
        if (this._selectedCommodityCode && this._selectedCommodityCode.id > 0) {
            this.supplier.intrastatCodeId = item.id;
        }
        else {
            this.supplier.intrastatCodeId = undefined;
        }

        this.setCommodityCodeTooltip();
        this.dirtyHandler.setDirty();
    }

    private commodityCodeTooltip: string;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private focusService: IFocusService,
        private coreService: ICoreService,
        private commonCustomerService: ICommonCustomerService,
        private $timeout: ng.ITimeoutService,
        urlHelperService: IUrlHelperService,
        private accountingService: IAccountingService,
        private supplierService: ISupplierService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory
    ) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;
            this.onInit(parameters);
            this.focusService.focusByName(parameters.id ? "ctrl_supplier_name" : "ctrl_supplier_supplierNr");
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    onInit(parameters: any) {
        this.guid = parameters.guid;
        this.supplierId = parameters.id || 0;
        this.updateCaption = parameters.updateCaption || false;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([
            { feature: Feature.Economy_Supplier_Suppliers_Edit, loadModifyPermissions: true },                          // Use currency
            { feature: Feature.Manage_Attest_Supplier_WorkFlowTemplate_Supplier, loadModifyPermissions: true },         // Invoice report
            { feature: Feature.Economy_Preferences_VoucherSettings_VatCodes, loadModifyPermissions: true },             // Invoice report
            { feature: Feature.Economy_Preferences_SuppInvoiceSettings_AttestGroups, loadModifyPermissions: true },     // Edit supplier 
            { feature: Feature.Economy_Supplier_Suppliers_Documents, loadModifyPermissions: true },                     // Documents
            { feature: Feature.Economy_Supplier_Suppliers_TrackChanges, loadModifyPermissions: true },                  // Documents
            { feature: Feature.Common_ExtraFields_Supplier, loadModifyPermissions: true },                  // Extra fields
            { feature: Feature.Economy_Intrastat, loadModifyPermissions: true },
        ]);

        this.filesHelper = new FilesHelper(this.coreService, this.$q, this.dirtyHandler, true, SoeEntityType.Supplier, SoeEntityImageType.Unknown, () => this.supplierId);
    }

    public closeModal(isModified: boolean, isRemoved: boolean) {
        if (this.isModal) {
            if (this.supplierId) {
                this.modal.close({ id: this.supplierId, isModified: isModified, isRemoved: isRemoved });
            } else {
                this.modal.dismiss();
            }
        }
    }

    public save() {
        //so for paymentinformationgrids
        this.$scope.$broadcast('stopEditing', {});

        this.$timeout(() => {
            this.progress.startSaveProgress((completion) => {

                if (this.supplier.active)
                    this.supplier.state = SoeEntityState.Active;
                else
                    this.supplier.state = SoeEntityState.Inactive;
                
                this.supplierService.saveSupplier(this.supplier, this.filesHelper.getAsDTOs(), _.filter(this.extraFieldRecords, (r) => r.isModified === true)).then((result) => {
                    
                    if (result.success) {
                        if (!this.supplierId)
                            // Clear Cache
                            this.supplierService.getSuppliersDict(true, true, false);

                        if (result.integerValue && result.integerValue > 0) {
                            if (this.supplierId == 0) {
                                if (this.navigatorRecords) {
                                    this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.supplier.name));
                                    this.toolbar.setSelectedRecord(result.integerValue);
                                } else {
                                    this.reloadNavigationRecords(result.integerValue);
                                }

                            }
                            this.supplierId = result.integerValue;
                            this.supplier.actorSupplierId = result.integerValue;
                        }
                        //clear cache
                        this.accountingService.getPaymentInformationViews(this.supplier.actorSupplierId, false);
                        
                        if (this.extraFieldsExpanderRendered)
                            this.$scope.$broadcast('reloadExtraFields', { guid: this.guid, recordId: this.supplierId });

                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.supplier);
                    } else {
                        completion.failed(result.errorMessage);
                    }
                })
            }, this.guid).then(data => {
                var isModified = this.dirtyHandler.isDirty;
                this.dirtyHandler.clean();

                if (this.isModal)
                    this.closeModal(isModified, false);
                else
                    this.onLoadData();
            }, error => {

            });
        });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.supplierService.getSuppliers(false, false).then(data => {
            _.forEach(data, (row) => {
                if (row.isActive) {
                    this.navigatorRecords.push(new SmallGenericType(row.supplierId, row.name));
                }
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.supplierId) {
                    this.supplierId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.supplierService.deleteSupplier(this.supplier.actorSupplierId).then((result) => {
                if (result.success) {
                    completion.completed(this.supplier);
                    super.closeMe(true);

                    this.closeModal(false, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }
    public showValidationError() {
        if (this.supplier) {
            this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
                const errors = this['edit'].$error;

                if (!this.supplier.supplierNr)
                    mandatoryFieldKeys.push("economy.supplier.supplier.suppliernr");
                if (!this.supplier.name)
                    mandatoryFieldKeys.push("common.name");

                if (errors['contactAddress'])
                    validationErrorStrings.push(this.contactAddressesValidationErrors);
            });
        }
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.modifyPermission = response[Feature.Economy_Supplier_Suppliers_Edit].modifyPermission;
        this.documentsPermission = response[Feature.Economy_Supplier_Suppliers_Documents].modifyPermission;
        this.trackChangesPermission = response[Feature.Economy_Supplier_Suppliers_TrackChanges].modifyPermission;
        this.hasExtraFieldPermission = response[Feature.Common_ExtraFields_Supplier].modifyPermission;
        this.hasCommodityCodesPermission = response[Feature.Economy_Intrastat].modifyPermission;
    }

    private onDoLookups() {
        return this.progress.startLoadingProgress([
            () => this.$q.all([
                this.loadTerms(),
                this.loadCompanySettings(),
                this.loadDeliveryTypes(),
                this.loadDeliveryConditions(),
                this.loadSettingTypes()]).then(() => {
                    return this.$q.all([
                        this.loadCountries(),
                        this.loadLanguages(),
                        this.loadCurrencies(),
                        this.loadVatTypes(),
                        this.loadVatCodes(),
                        this.loadPaymentConditions(),
                        this.loadFactoringSuppliers(),
                        this.loadSysWholesellers(),
                        this.loadAttestGroups(),
                        this.loadCommodityCodes()]);
                })
        ]);
        
    }

    private setConsentToolTip() {
        if (this.supplier.isPrivatePerson) {
            this.consentToolTip = this.terms["common.consentdescr"] + "\n"
            if (this.supplier.consentModifiedBy) {
                this.consentToolTip = this.consentToolTip + this.terms["common.modifiedby"] + ": " + this.supplier.consentModifiedBy + " " + CalendarUtility.toFormattedDate(this.supplier.consentModified);
            }            
        }
        else {
            this.consentToolTip = "";
        }
    }

    private onLoadData(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.load()
        ]);
    }

    private load(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (this.supplierId > 0) {
            this.supplierService.getSupplier(this.supplierId, true, true, true, true).then((x) => {
                this.supplier = x;
                this.dirtyHandler.clean();
                this.messagingHandler.publishSetTabLabel(this.guid, this.terms["economy.supplier.supplier.supplier"] + ' ' + this.supplier.supplierNr);
                this.supplier.consentDate = CalendarUtility.convertToDate(this.supplier.consentDate);
                this.setConsentToolTip();
                this.supplier.contactAddresses = this.supplier.contactAddresses.map(ca => {
                    var obj = new ContactAddressItemDTO();
                    angular.extend(obj, ca);
                    return obj;
                });

                this.isNew = false;
                this.focusService.focusByName("ctrl_supplier_name");
                this.supplierIsLoaded = true;

                if (this.documentExpanderIsOpen)
                    this.filesHelper.loadFiles(true);

                if (this.updateCaption) {
                    this.updateTabCaption();
                    this.updateCaption = false;
                }

                this.loadSupplierEmails(this.supplier.actorSupplierId);

                if (this.extraFieldsExpanderRendered) {                    
                    this.$scope.$broadcast('reloadExtraFields', { guid: this.guid, recordId: this.supplierId });
                }

                this._selectedCommodityCode = this.supplier.intrastatCodeId ? _.find(this.commodityCodes, (g) => g.id === this.supplier.intrastatCodeId) : undefined;

                deferral.resolve();
            });
        }
        else {
            this.new();
            this.supplierIsLoaded = true;

            if (this.updateCaption) {
                this.updateTabCaption();
                this.updateCaption = false;
            }

            deferral.resolve();
        }

        return deferral.promise;
    }
    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createEmpty();
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.download", "common.download", IconLibrary.FontAwesome, "fal fa-download", () => {
            this.exportSupplier();
        }, null, () => {
            return (!this.supplierId || this.supplierId === 0);
        })));
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.supplierId, recordId => {
            if (recordId !== this.supplierId) {
                this.supplierId = recordId;
                this.onLoadData();
            }
        });

    }

    private loadTerms() {
        const keys: string[] = [
            "economy.supplier.supplier.supplier",
            "common.consentdescr",
            "common.modifiedby",
            "economy.supplier.supplier.new"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }
    
    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.AccountSupplierDebt);
        settingTypes.push(CompanySettingType.AccountSupplierPurchase);
        settingTypes.push(CompanySettingType.AccountCommonVatReceivable);
        settingTypes.push(CompanySettingType.AccountSupplierInterim);
        settingTypes.push(CompanySettingType.SupplierInvoiceAllowInterim);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.baseAccounts = [];
            this.baseAccounts.push(new SmallGenericType(SupplierAccountType.Credit, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountSupplierDebt).toString()));
            this.baseAccounts.push(new SmallGenericType(SupplierAccountType.Debit, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountSupplierPurchase).toString()));
            this.baseAccounts.push(new SmallGenericType(SupplierAccountType.VAT, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatReceivable).toString()));
            this.baseAccounts.push(new SmallGenericType(SupplierAccountType.Interim, SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountSupplierInterim).toString()));
            this.allowInterim = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceAllowInterim);
        });
    }
    private loadSettingTypes(): ng.IPromise<any> {        
        this.settingTypes = [];

        const keys: string[] = [
            "economy.supplier.supplier.accountingsettingtype.credit",
            "economy.supplier.supplier.accountingsettingtype.debit",
            "economy.supplier.supplier.accountingsettingtype.vat",
            "economy.supplier.supplier.accountingsettingtype.interim",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.settingTypes.push(new SmallGenericType(SupplierAccountType.Credit, terms["economy.supplier.supplier.accountingsettingtype.credit"]));
            this.settingTypes.push(new SmallGenericType(SupplierAccountType.Debit, terms["economy.supplier.supplier.accountingsettingtype.debit"]));
            this.settingTypes.push(new SmallGenericType(SupplierAccountType.VAT, terms["economy.supplier.supplier.accountingsettingtype.vat"]));
            this.settingTypes.push(new SmallGenericType(SupplierAccountType.Interim, terms["economy.supplier.supplier.accountingsettingtype.interim"]));
        });
    }
    private loadCountries(): ng.IPromise<any> {
        return this.coreService.getSysCountries(true, false).then(x => {
            this.countries = x;
        });
    }
    private loadLanguages(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.Language, true, false).then(x => {
            this.languages = x;
        });
    }
    private loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesDict(false).then(x => {
            this.currencies = x;
        });
    }
    private loadVatTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceVatType, true, false).then(x => {
            this.vatTypes = _.filter(x, (y) => y.id < 7);
        });
    }
    private loadVatCodes(): ng.IPromise<any> {
        return this.accountingService.getVatCodes(true).then(x => {
            this.vatCodes = x;
            // Insert empty row
            this.vatCodes.splice(0, 0, { vatCodeId: 0, name: '', percent: 0 });
        });
    }
    private loadPaymentConditions(): ng.IPromise<any> {
        return this.supplierService.getPaymentConditionsDict(true).then(x => {
            this.paymentConditions = x;
        });
    }
    private loadDeliveryTypes(): ng.IPromise<any> {
        return this.commonCustomerService.getDeliveryTypesDict(true).then(x => {
            this.deliveryTypes = x;
        });
    }

    private loadDeliveryConditions(): ng.IPromise<any> {
        return this.commonCustomerService.getDeliveryConditionsDict(true).then(x => {
            this.deliveryConditions = x;
        });
    }

    private loadSupplierEmails(supplierId: number): ng.IPromise<any> {
        return this.supplierService.getSupplierEmails(supplierId, true, true).then(x => {
            this.supplierEmails = x;
        });
    }

    private loadFactoringSuppliers(): ng.IPromise<any> {
        return this.supplierService.getSuppliersDict(true, true, true).then(x => {
            this.factoringSuppliers = x;

            // Remove current supplier
            if (this.supplierId !== 0) {
                const currentSupplier = _.find(this.factoringSuppliers, { id: this.supplierId });
                if (currentSupplier) {
                    const idx = this.factoringSuppliers.indexOf(currentSupplier);
                    this.factoringSuppliers.splice(idx, 1);
                }
            };
        });
    }
    private loadSysWholesellers(): ng.IPromise<any> {
        return this.supplierService.getSysWholesellersDict(true).then(x => {
            this.sysWholesellers = x;
        });
    }
    private loadAttestGroups(): ng.IPromise<any> {
        return this.supplierService.getAttestWorkFlowGroupsDict(true).then(x => {
            this.attestGroups = x;
        });
    }

    private loadCommodityCodes(): ng.IPromise<any> {
        return this.supplierService.getCustomerCommodityCodesDict(true).then(x => {
            this.commodityCodes = x;
        });
    }

    private getNextSupplierNr() {
        this.supplierService.getNextSupplierNr().then(x => {
            this.supplier.supplierNr = x;
        });
    }

    private exportSupplier() {
        this.progress.startLoadingProgress([() => {
            return this.supplierService.getSupplierForExport(this.supplierId).then((supplier) => {
                ExportUtility.Export(supplier, 'supplier.json');
            });
        }]);
    }

    // HELP-METHODS


    private new() {
        this.isNew = true;
        this.supplierId = 0;
        this.supplier = new SupplierDTO();
        // default values
        if (this.currencies && this.currencies.length)
            this.supplier.currencyId = this.currencies[0].id;
        this.supplier.active = true;
        this.supplier.interim = this.allowInterim;

        this.getNextSupplierNr();
        this.supplier.contactAddresses = [];
        this.focusService.focusByName("ctrl_supplier_supplierNr");
        this.supplierEmails = [];

        this.filesHelper.reset();
    }

    private updateTabCaption() {
        const number = this.supplier && this.supplier.supplierNr ? this.supplier.supplierNr : "";
        const label = this.isNew ? this.terms["economy.supplier.supplier.new"] : this.terms["economy.supplier.supplier.supplier"] + " " + number;
        this.messagingHandler.publishSetTabLabel(this.guid, label);        
    }

    public onExtraFieldsExpanderOpenClose() {
        this.extraFieldRecords = [];
        this.extraFieldsExpanderRendered = !this.extraFieldsExpanderRendered;
    }

    public setCommodityCodeTooltip() {
        this.$timeout(() => {
            this.commodityCodeTooltip = this._selectedCommodityCode ? this._selectedCommodityCode.text as string : "TEXT";;
        });
    }
}