import { CopyFromTemplateCompanyInputDTO } from "../../../../../Common/Models/CompanyDTOs";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { ISmallGenericType, System } from "../../../../../Scripts/TypeLite.Net4";
import { TemplateCompanyCopy, TermGroup } from "../../../../../Util/CommonEnumerations";
import { SOEMessageBoxButton, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { ICompanyService } from "../../../CompanyService";

export class TemplateCopyController {

    // When new selection values needs to be added just add to termgroup 635 and to the enum TemplateCompanyCopy

    // Variables
    private selectedTemplateCompanyId: number;

    // Collections
    private templateCopyTerms: any[]
    private templateCompanies: ISmallGenericType[] = [];
    private templateCopyValues: any[];

    // Flags
    private baseIsOpen = false;
    private settingsIsOpen = false;
    private reportsIsOpen = false;
    private economyIsOpen = false;
    private billingIsOpen = false;
    private timeIsOpen = false;

    // Properties
    private _selectedAll: any;
    get selectedAll() {
        return this._selectedAll;
    }
    set selectedAll(item: any) {
        this._selectedAll = item;

        _.forEach(this.templateCopyValues, (val) => {
            val.value = this._selectedAll;
        });

        if (this._selectedAll)
            this.expandAll();
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private companyService: ICompanyService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private $q: ng.IQService,
        private licenseId: number) {
        this.init();
    }

    get general(){
        return _.filter(this.templateCopyValues, (v) => (v.key > 100 && v.key <= 200));
    }

    get settings() {
        return _.filter(this.templateCopyValues, (v) => (v.key > 200 && v.key <= 299));
    }

    get reports() {
        return _.filter(this.templateCopyValues, (v) => (v.key > 300 && v.key <= 399));
    }

    get economy() {
        return _.filter(this.templateCopyValues, (v) => (v.key > 400 && v.key <= 499));
    }

    get billing() {
        return _.filter(this.templateCopyValues, (v) => (v.key > 500 && v.key <= 599));
    }

    get time() {
        return _.filter(this.templateCopyValues, (v) => (v.key > 600 && v.key <= 699));
    }

    private init() {
        return this.$q.all([
            this.loadTemplateCompanies(),
            this.loadCopyCompanyTerms(),
        ]).then(() => {
            this.templateCopyValues = [];
            _.forEach(TemplateCompanyCopy, (val) => {
                if (!isNaN(Number(val))) {
                    var term = _.find(this.templateCopyTerms, { id: val });
                    if (term) {
                        this.templateCopyValues.push({ key: val, value: false, label: term.name });
                    }
                }
            });

            this.selectedTemplateCompanyId = 0;
        });
    }

    private loadTemplateCompanies(): ng.IPromise<any> {
        return this.companyService.getTemplateCompanies(this.licenseId).then((comps) => {
            this.templateCompanies = comps;            
        });
    }

    private loadCopyCompanyTerms(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.CompanyTemplateCopyValues, false, false, true).then((x) => {
            this.templateCopyTerms = x;
        });
    }

    private selectionChangedGeneral(val: any) {
        this.$timeout(() => {
            if (val && this.selectedAll && !val.value)
                this._selectedAll = false;
        });
    }

    private selectionChangedSettings(val: any) {
        this.$timeout(() => {
            if (val.key === TemplateCompanyCopy.ProjectSettings) {
                const timeCodes = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.PayrollProductsAndTimeCodes });
                if (val.value)
                    timeCodes.value = true;
            }
            if (val.key === TemplateCompanyCopy.AccountingSettings) {
                const voucherSeriesTypes = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.VoucherSeriesTypes });
                if (val.value)
                    voucherSeriesTypes.value = true;
            }
            if (val.key === TemplateCompanyCopy.CustomerSettings) {
                const reports = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.ReportsAndReportTemplates });
                if (val.value)
                    reports.value = true;
                const reportsSettings = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.ReportSettings });
                if (val.value)
                    reportsSettings.value = true;
            }


            if (val.key === TemplateCompanyCopy.SupplierSettings) {
                const financeAttest = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.CompanyAttestSupplier });
                if (val.value) {
                    financeAttest.value = true;
                }
            }

            if (val && this.selectedAll && !val.value)
                this._selectedAll = false;
        });
    }

    private selectionChangedReports(val: any) {
        this.$timeout(() => {
            const copyReports = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.ReportsAndReportTemplates });
            const copyGroupsAndHeaders = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.ReportGroupsAndReportHeaders });
            const copyReportSettings = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.ReportSettings });
            const copyReportSelections = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.ReportSelections });

            if (copyGroupsAndHeaders.value || copyReportSettings.value || copyReportSelections.value)
                copyReports.value = true;

            if (val && this.selectedAll && !val.value)
                this._selectedAll = false;
        });
    }

    private selectionChangedEconomy(val: any) {
        this.$timeout(() => {
            if (val.key === TemplateCompanyCopy.AccountStds || val.key === TemplateCompanyCopy.PaymentMethods || 
                val.key === TemplateCompanyCopy.VoucherSeriesTypes || val.key === TemplateCompanyCopy.AccountYearsAndPeriods || 
                val.key === TemplateCompanyCopy.AccountInternals || val.key === TemplateCompanyCopy.Inventory ||
                val.key === TemplateCompanyCopy.PeriodAccountDistributionTemplates || val.key === TemplateCompanyCopy.AutomaticAccountDistributionTemplates ||
                val.key === TemplateCompanyCopy.DistributionCodes || val.key === TemplateCompanyCopy.ResidualCodes ||
                val.key === TemplateCompanyCopy.VoucherTemplates || val.key === TemplateCompanyCopy.Suppliers) {
                const copyAccountSTDs = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.AccountStds });
                const copyPaymentMethods = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.PaymentMethods });
                const copyVoucherSeries = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.VoucherSeriesTypes });
                const copyYearsAndPeriods = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.AccountYearsAndPeriods });
                const copyAccountInternals = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.AccountInternals });
                const copyInventory = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.Inventory });
                const copyAutoTemplates = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.AutomaticAccountDistributionTemplates });
                const copyPeriodTemplates = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.PeriodAccountDistributionTemplates });
                const copyDistributionCodes = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.DistributionCodes });
                const copySuppliers = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.Suppliers });
                const copyPaymentConditions = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.PaymentConditions });
                const copyVatCodes = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.VatCodes });
                const copyAttest = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.CompanyAttestSupplier });
                const copyBillingSettings = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.BillingSettings });

                const copyResidualCodes = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.ResidualCodes });
                const copyVoucherTemplates = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.VoucherTemplates });

                if (copyPaymentMethods.value)
                    copyAccountSTDs.value = true;

                if (copyYearsAndPeriods.value)
                    copyVoucherSeries.value = true;

                if (copyAutoTemplates.value || copyDistributionCodes.value) {
                    copyAccountSTDs.value = true;
                    copyAccountInternals.value = true;
                }

                if (copyInventory.value || copyPeriodTemplates.value) {
                    copyAccountSTDs.value = true;
                    copyAccountInternals.value = true;
                    copyVoucherSeries.value = true;
                }
                if (copyVoucherTemplates.value) {
                    copyAccountSTDs.value = true;
                    copyAccountInternals.value = true;
                    copyVoucherSeries.value = true;
                    copyYearsAndPeriods.value = true;
                }

                if (copyResidualCodes.value) {
                    copyAccountSTDs.value = true;
                    copyAccountInternals.value = true;
                    copyDistributionCodes.value = true;
                }

                if (copySuppliers.value) {
                    copyAccountSTDs.value = true;
                    copyAccountInternals.value = true;
                    copyPaymentConditions.value = true;
                    copyAttest.value = true;
                    copyVatCodes.value = true;
                    copyBillingSettings.value = true;
                }

                if (val.key === TemplateCompanyCopy.AccountStds || val.key === TemplateCompanyCopy.AccountInternals) {
                    const customerSettings = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.Customers });
                    if (!val.value)
                        customerSettings.value = false;
                }
            }

            if (val.key === TemplateCompanyCopy.CompanyAttestSupplier) {
                const suppliers = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.SupplierSettings });
                if (!val.value) {
                    suppliers.value = false;
                }
            }

            if (val && this.selectedAll && !val.value)
                this._selectedAll = false;
        });
    }

    private selectionChangedBilling(val: any) {
        this.$timeout(() => {
            if (val.key === TemplateCompanyCopy.PriceRules || val.key === TemplateCompanyCopy.PricesLists) {
                const copyPriceRules = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.PriceRules });
                const copyPriceLists = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.PricesLists });

                if (copyPriceRules.value)
                    copyPriceLists.value = true;
            }

            if (val.key === TemplateCompanyCopy.CompanyExternalProducts) {
                const prodSettings = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.CompanyProducts });
                if (val.value)
                    prodSettings.value = true;
            }

            if (val.key === TemplateCompanyCopy.CompanyProducts) {
                const extProdSettings = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.CompanyExternalProducts });
                const customerSettings = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.Customers });
                if (!val.value) {
                    extProdSettings.value = false;
                    customerSettings.value = false;
                }
            }

            if (val.key === TemplateCompanyCopy.Customers) { 
                const prodSettings = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.CompanyProducts });
                const accountStdSettings = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.AccountStds });
                const accountInternalSettings = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.AccountInternals });
                if (val.value) {
                    prodSettings.value = true;
                    accountStdSettings.value = true;
                    accountInternalSettings.value = true;
                }
            }

            if (val && this.selectedAll && !val.value)
                this._selectedAll = false;
        });
    }

    private selectionChangedTime(val: any) {
        this.$timeout(() => {
            if (val.key === TemplateCompanyCopy.PayrollProductsAndTimeCodes) {
                const projSettings = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.ProjectSettings });
                if (!val.value)
                    projSettings.value = false;
            }

            if (val && this.selectedAll && !val.value)
                this._selectedAll = false;
        });
    }

    private expandAll() {
        this.baseIsOpen = true;
        this.settingsIsOpen = true;
        this.reportsIsOpen = true;
        this.economyIsOpen = true;
        this.billingIsOpen = true;
        this.timeIsOpen = true;
    }

    private collapseAll() {
        this.baseIsOpen = false;
        this.settingsIsOpen = false;
        this.reportsIsOpen = false;
        this.economyIsOpen = false;
        this.billingIsOpen = false;
        this.timeIsOpen = false;
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }

    buttonOkClick() {
        const copyCustomers = _.find(this.templateCopyValues, { key: TemplateCompanyCopy.Customers });
        if (copyCustomers.value) {
            const keys: string[] = [
                "core.warning",
                "manage.company.copycustomerwarning"
            ];

            this.translationService.translateMany(keys).then(terms => {
                const modal = this.notificationService.showDialogEx(terms["core.warning"], terms["manage.company.copycustomerwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo, { initialFocusButton: SOEMessageBoxButton.Yes });
                modal.result.then(val => {
                    if (val === true) {
                        this.closeDialog();
                    }
                });
            });
        }
        else {
            this.closeDialog();
        }
    }

    closeDialog() {
        const dto = new CopyFromTemplateCompanyInputDTO();
        dto.templateCompanyId = this.selectedTemplateCompanyId
        dto.copyDict = {};

        _.forEach(this.templateCopyValues, (val) => {
            if (val.key === TemplateCompanyCopy.All)
                dto.copyDict[val.key] = this.selectedAll;
            else
                dto.copyDict[val.key] = val.value;
        });
        this.$uibModalInstance.close(dto);
    }
}