import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { PayrollProductSettingDTO, PayrollProductDTO } from "../../../../../Common/Models/ProductDTOs";
import { PayrollProductSettingsDialogController } from "./PayrollProductSettingsDialogController";
import { PayrollProductSettingsAddDialogController } from "./PayrollProductSettingsAddDialogController";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { TermGroup, ProductAccountType, TermGroup_PayrollProductAccountingPrio, TermGroup_SysPayrollType, TermGroup_PayrollProductCentRoundingType, TermGroup_PayrollProductCentRoundingLevel, TermGroup_PayrollProductTaxCalculationType, TermGroup_PensionCompany, TermGroup_PayrollProductTimeUnit, TermGroup_PayrollProductQuantityRoundingType, SoeEntityType } from "../../../../../Util/CommonEnumerations";
import { ISmallGenericType, IPayrollPriceTypeAndFormulaDTO } from "../../../../../Scripts/TypeLite.Net4";
import { IPayrollService } from "../../../PayrollService";
import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { PayrollGroupSmallDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { ExtraFieldGridDTO } from "../../../../../Common/Models/ExtraFieldDTO";


export class PayrollProductSettingsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Payroll/PayrollProducts/Directives/PayrollProductSettings/Views/PayrollProductSettings.html'),
            scope: {
                product: '=',
                readOnly: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: PayrollProductSettingsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class PayrollProductSettingsController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private product: PayrollProductDTO;
    private centRoundingTypes: ISmallGenericType[];
    private centRoundingLevels: ISmallGenericType[];
    private taxCalculationTypes: ISmallGenericType[];
    private pensionCompanies: ISmallGenericType[];
    private timeUnits: ISmallGenericType[];
    private quantityRoundingTypes: ISmallGenericType[];
    private payrollProductChildren: ISmallGenericType[];
    private allPayrollProductAccountingPrios: ISmallGenericType[];
    private payrollProductAccountingPrios: ISmallGenericType[];
    private payrollProductAccountingPrioRows: any[];
    private accountDims: AccountDimSmallDTO[];
    private payrollGroups: ISmallGenericType[] = [];
    private selectedPayrollGroups: ISmallGenericType[] = [];
    private payrollPriceTypesAndFormulas: IPayrollPriceTypeAndFormulaDTO[] = [];
    private accountSettingTypes: SmallGenericType[];
    private allPayrollGroups: PayrollGroupSmallDTO[] = [];
    private extraFields: ExtraFieldGridDTO[] = [];

    // Flags
    private readOnly: boolean;
    private selectedSetting: PayrollProductSettingDTO;

    // Events
    private onChange: Function;

    //@ngInject
    constructor(
        private $uibModal,
        private $scope: ng.IScope,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private payrollService: IPayrollService) {
    }

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.loadCentRoundingTypes(),
            this.loadCentRoundingLevels(),
            this.loadTaxCalculationTypes(),
            this.loadPensionCompanies(),
            this.loadTimeUnits(),
            this.loadQuantityRoundingTypes(),
            this.loadPayrollProductAccountingPrios(),
            this.loadAccountDims(),
            this.loadPayrollPriceTypesAndFormulas(),
            this.loadPayrollGroups(),
            this.loadExtraFields()
        ]).then(() => {
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.product, (newVal, oldVal) => {
            if (newVal) {
                this.$timeout(() => {
                    this.loadPayrollProductChildren().then(() => {
                        this.setupPayrollProductAccountingPrios();
                        this.sortSettings();
                        this.setSettingExtensions();
                        this.buildPayrollGroupFilter();
                        this.selectFirstSetting();
                    });
                });
            }
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.accountingsettings.account",
            "time.payroll.payrollproduct.setting.accountingprio",
            "time.payroll.payrollproduct.setting.accounting.purchase",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.accountSettingTypes = [];
            this.accountSettingTypes.push(new SmallGenericType(ProductAccountType.Purchase, terms["time.payroll.payrollproduct.setting.accounting.purchase"]));
        });
    }

    private loadCentRoundingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollProductCentRoundingType, false, false).then(x => {
            this.centRoundingTypes = x;
        });
    }

    private loadCentRoundingLevels(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollProductCentRoundingLevel, false, false).then(x => {
            this.centRoundingLevels = x;
        });
    }

    private loadTaxCalculationTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollProductTaxCalculationType, false, false).then(x => {
            this.taxCalculationTypes = x;
        });
    }

    private loadPensionCompanies(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PensionCompany, false, false).then(x => {
            this.pensionCompanies = x;
        });
    }

    private loadTimeUnits(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollProductTimeUnit, false, false).then(x => {
            this.timeUnits = x;
        });
    }

    private loadQuantityRoundingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.QuantityRoundingType, false, false).then(x => {
            this.quantityRoundingTypes = x;
        });
    }

    private loadPayrollProductChildren(): ng.IPromise<any> {
        return this.payrollService.getPayrollProductChildren(this.product.productId).then(x => {
            this.payrollProductChildren = x;
        });
    }

    private loadPayrollProductAccountingPrios(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollProductAccountingPrio, false, false).then(x => {
            this.allPayrollProductAccountingPrios = x;
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.coreService.getAccountDimsSmall(false, false, false, false, false, false).then(x => {
            this.accountDims = x;
        });
    }

    private loadPayrollPriceTypesAndFormulas(): ng.IPromise<any> {
        return this.payrollService.getPayrollPriceTypesAndFormulas().then(x => {
            this.payrollPriceTypesAndFormulas = x;
        });
    }

    private loadPayrollGroups(): ng.IPromise<any> {
        return this.payrollService.getPayrollGroupsSmall(false, false).then(x => {
            this.allPayrollGroups = x;
        });
    }

    private loadExtraFields(): ng.IPromise<any> {
        return this.coreService.getExtraFields(SoeEntityType.PayrollProductSetting, false).then(x => {
            this.extraFields = x;
        });
    }

    // EVENTS

    private addSetting() {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Payroll/PayrollProducts/Directives/PayrollProductSettings/Views/PayrollProductSettingsAddDialog.html"),
            controller: PayrollProductSettingsAddDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {

                usedPayrollGroups: () => { return this.payrollGroups },
                availablePayrollGroups: () => { return this.getAvailablePayrollGroups() },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.createForPayrollGroupId && result.createForPayrollGroupId !== 0) {
                let selectedPayrollGroup = _.find(this.allPayrollGroups, p => p.payrollGroupId === result.createForPayrollGroupId);
                if (selectedPayrollGroup) {
                    let newPayrollProductSetting = new PayrollProductSettingDTO();
                    newPayrollProductSetting.payrollGroupId = selectedPayrollGroup.payrollGroupId;
                    newPayrollProductSetting.payrollGroupName = selectedPayrollGroup.name;
                    newPayrollProductSetting.priceTypes = [];
                    newPayrollProductSetting.priceFormulas = [];
                    newPayrollProductSetting.accountingPrio = "1=0,2=0,3=0,4=0,5=0,6=0";
                    newPayrollProductSetting.accountingSettings = [];

                    let fromPayrollGroupSetting = _.find(this.product.settings, s => s.payrollGroupId === result.createFromPayrollGroupId);
                    if (fromPayrollGroupSetting) {
                        this.copyPayrollProductSettings(fromPayrollGroupSetting, newPayrollProductSetting, true);
                    }
                    else {
                        this.setDefaultSettings(newPayrollProductSetting);
                    }
                    this.editSetting(newPayrollProductSetting, true);
                }
            }
        });
    }

    private editSetting(setting: PayrollProductSettingDTO, addSetting: boolean = false) {
        this.selectedSetting = setting;
       
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Payroll/PayrollProducts/Directives/PayrollProductSettings/Views/PayrollProductSettingsDialog.html"),
            controller: PayrollProductSettingsDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                readOnly: () => { return this.readOnly },
                settingOrginal: () => { return this.selectedSetting },
                product: () => { return this.product },
                centRoundingTypes: () => { return this.centRoundingTypes },
                centRoundingLevels: () => { return this.centRoundingLevels },
                taxCalculationTypes: () => { return this.taxCalculationTypes },
                pensionCompanies: () => { return this.pensionCompanies },
                quantityRoundingTypes: () => { return this.quantityRoundingTypes },
                payrollProductChildren: () => { return this.payrollProductChildren },
                payrollPriceTypesAndFormulas: () => { return this.payrollPriceTypesAndFormulas },
                timeUnits: () => { return this.timeUnits },
                payrollProductAccountingPrios: () => { return this.payrollProductAccountingPrios },
                accountDims: () => { return this.accountDims },
                accountSettingTypes: () => { return this.accountSettingTypes },
                hasExtraFields: () => { return this.extraFields && this.extraFields.length > 0 },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.setting) {
                if (addSetting)
                    this.product.settings.push(this.selectedSetting);

                this.copyPayrollProductSettings(result.setting, this.selectedSetting, false);
                if (!setting.payrollProductSettingId) {
                    this.sortSettings();
                    this.buildPayrollGroupFilter();
                }
                this.setSettingExtensions();

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private deleteSetting(setting: PayrollProductSettingDTO) {
        if (!setting.payrollGroupId)
            return;

        const keys: string[] = [
            "time.payroll.payrollproduct.setting.askdelete.title",
            "time.payroll.payrollproduct.setting.askdelete.message"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.notificationService.showDialogEx(terms["time.payroll.payrollproduct.setting.askdelete.title"], terms["time.payroll.payrollproduct.setting.askdelete.message"].format(setting.payrollGroupName), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
                if (val) {
                    _.pull(this.product.settings, setting);
                    this.selectFirstSetting();
                    this.buildPayrollGroupFilter();

                    if (this.onChange)
                        this.onChange();
                }
            });
        });
    }

    private onPayrollGroupFiltered(items) {
    }

    // HELP-METHODS    

    private setupPayrollProductAccountingPrios() {
        let level1: number = this.product.sysPayrollTypeLevel1 || 0;
        let isEmploymentTax: boolean = (level1 == TermGroup_SysPayrollType.SE_EmploymentTaxCredit || level1 == TermGroup_SysPayrollType.SE_EmploymentTaxDebit);

        this.payrollProductAccountingPrios = [];
        if (!isEmploymentTax)
            this.payrollProductAccountingPrios.push(_.find(this.allPayrollProductAccountingPrios, p => p.id == TermGroup_PayrollProductAccountingPrio.NotUsed));
        this.payrollProductAccountingPrios.push(_.find(this.allPayrollProductAccountingPrios, p => p.id == TermGroup_PayrollProductAccountingPrio.PayrollProduct));
        if (!isEmploymentTax)
            this.payrollProductAccountingPrios.push(_.find(this.allPayrollProductAccountingPrios, p => p.id == TermGroup_PayrollProductAccountingPrio.EmploymentAccount));
        if (!isEmploymentTax)
            this.payrollProductAccountingPrios.push(_.find(this.allPayrollProductAccountingPrios, p => p.id == TermGroup_PayrollProductAccountingPrio.Project));
        if (!isEmploymentTax)
            this.payrollProductAccountingPrios.push(_.find(this.allPayrollProductAccountingPrios, p => p.id == TermGroup_PayrollProductAccountingPrio.Customer));
        this.payrollProductAccountingPrios.push(_.find(this.allPayrollProductAccountingPrios, p => p.id == TermGroup_PayrollProductAccountingPrio.PayrollGroup));
        if (!isEmploymentTax)
            this.payrollProductAccountingPrios.push(_.find(this.allPayrollProductAccountingPrios, p => p.id == TermGroup_PayrollProductAccountingPrio.EmployeeGroup));
        this.payrollProductAccountingPrios.push(_.find(this.allPayrollProductAccountingPrios, p => p.id == TermGroup_PayrollProductAccountingPrio.Company));
        if (!isEmploymentTax)
            this.payrollProductAccountingPrios.push(_.find(this.allPayrollProductAccountingPrios, p => p.id == TermGroup_PayrollProductAccountingPrio.NoAccounting));
    }

    private getAvailablePayrollGroups() {
        let availablePayrollGroups: PayrollGroupSmallDTO[] = [];

        _.forEach(this.allPayrollGroups, payrollGroup => {

            let value = _.find(this.payrollGroups, p => p.id === payrollGroup.payrollGroupId);
            if (!value)
                availablePayrollGroups.push(payrollGroup);
        });

        return availablePayrollGroups;
    }

    private selectFirstSetting() {
        this.selectedSetting = _.find(this.product.settings, s => s.sort === 0);
    }

    private sortSettings() {
        let sort = 0;
        _.forEach(_.sortBy(this.product.settings, s => s.payrollGroupName), setting => {
            setting.sort = setting.payrollGroupId ? ++sort : 0;
        });
    }

    private setSettingExtensions() {
        _.forEach(this.product.settings, setting => {
            let centRoundingType = _.find(this.centRoundingTypes, c => c.id === setting.centRoundingType);
            setting.centRoundingTypeName = centRoundingType ? centRoundingType.name : '';

            let centRoundingLevel = setting.centRoundingLevel ? _.find(this.centRoundingLevels, c => c.id === setting.centRoundingLevel) : null;
            setting.centRoundingLevelName = centRoundingLevel ? centRoundingLevel.name : '';

            let taxCalculationType = _.find(this.taxCalculationTypes, t => t.id === setting.taxCalculationType);
            setting.taxCalculationTypeName = taxCalculationType ? taxCalculationType.name : '';

            let pensionCompany = _.find(this.pensionCompanies, t => t.id === setting.pensionCompany);
            setting.pensionCompanyName = pensionCompany ? pensionCompany.name : '';

            let timeUnit = _.find(this.timeUnits, t => t.id === setting.timeUnit);
            setting.timeUnitName = timeUnit ? timeUnit.name : '';

            let quantityRoundingType = setting.quantityRoundingType ? _.find(this.quantityRoundingTypes, t => t.id === setting.quantityRoundingType) : null;
            setting.quantityRoundingTypeName = quantityRoundingType ? quantityRoundingType.name : '';

            let childProduct = setting.childProductId ? _.find(this.payrollProductChildren, t => t.id === setting.childProductId) : null;
            setting.childProductName = childProduct ? childProduct.name : '';

            setting.priceTypesName = _.map(setting.priceTypes, p => p.priceTypeName).join(', ');

            setting.priceFormulasName = _.map(setting.priceFormulas, p => p.formulaName).join(', ');

            // Only one account used (purchase)
            setting.accountingName = '';
            if (setting.accountingSettings.length > 0) {
                let account = setting.accountingSettings[0];
                let dimCounter: number = 0;
                _.forEach(this.accountDims, dim => {
                    dimCounter++;
                    if (account[`account${dimCounter}Id`]) {
                        if (setting.accountingName.length > 0)
                            setting.accountingName += ', ';
                        setting.accountingName += "{0}: {1} {2}".format(dimCounter === 1 ? this.terms["common.accountingsettings.account"] : dim.name, account[`account${dimCounter}Nr`], account[`account${dimCounter}Name`]);
                    }
                });
            }

            setting.accountingPrioName = '';
            let prios = setting.accountingPrio.split(',');
            _.forEach(prios, prio => {
                let parts = prio.split('=');
                if (parts.length === 2) {
                    let dim = this.accountDims[parseInt(parts[0], 10) - 1];
                    if (dim) {
                        let accPrio = _.find(this.payrollProductAccountingPrios, p => p.id === parseInt(parts[1], 10));
                        if (!accPrio)
                            accPrio = _.find(this.payrollProductAccountingPrios, p => p.id === 0);
                        if (accPrio) {
                            if (setting.accountingPrioName.length > 0)
                                setting.accountingPrioName += ', ';
                            setting.accountingPrioName += '{0}: {1}'.format(dim.name, accPrio.name);
                        }
                    }
                }
            });
        });
    }

    private buildPayrollGroupFilter() {
        this.payrollGroups = [];
        this.selectedPayrollGroups = [];
        _.forEach(this.product.settings, s => {
            let group = new SmallGenericType(s.payrollGroupId, s.payrollGroupName);
            this.payrollGroups.push(group);
            this.selectedPayrollGroups.push(new SmallGenericType(s.payrollGroupId, s.payrollGroupName));
        });
    }

    private get filteredSettings(): PayrollProductSettingDTO[] {
        if (!this.product)
            return [];

        return _.orderBy(_.filter(this.product.settings, s => _.includes(_.map(this.selectedPayrollGroups, g => g.id), s.payrollGroupId)), 'sort');
    }

    getExtraFieldValue(payrollProductSetting: PayrollProductSettingDTO, extraFieldId: number): string {
        if (!payrollProductSetting || !extraFieldId || !this.extraFields)
            return '';

        const extraField = this.extraFields.find(e => e.extraFieldId === extraFieldId);
        if (!extraField)
            return '';

        const settingExtraField = (payrollProductSetting.extraFields || []).find(e => e.extraFieldId === extraFieldId);
        if (!settingExtraField)
            return '';

        return settingExtraField.value;
    }

    private copyPayrollProductSettings(from: PayrollProductSettingDTO, to: PayrollProductSettingDTO, clone: boolean) {
        if (!to) {
            to = new PayrollProductSettingDTO();
        }

        to.centRoundingType = from.centRoundingType;
        to.centRoundingTypeName = from.centRoundingTypeName;
        to.centRoundingLevel = from.centRoundingLevel;
        to.centRoundingLevelName = from.centRoundingLevelName;
        to.taxCalculationType = from.taxCalculationType;
        to.taxCalculationTypeName = from.taxCalculationTypeName;
        to.pensionCompany = from.pensionCompany;
        to.pensionCompanyName = from.pensionCompanyName;
        to.timeUnit = from.timeUnit;
        to.timeUnitName = from.timeUnitName;
        to.quantityRoundingType = from.quantityRoundingType;
        to.quantityRoundingTypeName = from.quantityRoundingTypeName;
        to.quantityRoundingMinutes = from.quantityRoundingMinutes;
        to.childProductId = from.childProductId;
        to.childProductName = from.childProductName;

        to.printOnSalarySpecification = from.printOnSalarySpecification;
        to.dontPrintOnSalarySpecificationWhenZeroAmount = from.dontPrintOnSalarySpecificationWhenZeroAmount;
        to.dontIncludeInRetroactivePayroll = from.dontIncludeInRetroactivePayroll;
        to.dontIncludeInAbsenceCost = from.dontIncludeInAbsenceCost;
        to.printDate = from.printDate;
        to.vacationSalaryPromoted = from.vacationSalaryPromoted;
        to.unionFeePromoted = from.unionFeePromoted;
        to.workingTimePromoted = from.workingTimePromoted;
        to.calculateSupplementCharge = from.calculateSupplementCharge;
        to.calculateSicknessSalary = from.calculateSicknessSalary;

        to.accountingPrio = from.accountingPrio;

        if (clone === true) {
            to.priceTypes = _.cloneDeep(from.priceTypes);
            to.priceFormulas = _.cloneDeep(from.priceFormulas);
            to.accountingSettings = _.cloneDeep(from.accountingSettings);
            to.extraFields = _.cloneDeep(from.extraFields);

            _.forEach(to.priceTypes, pt => {
                pt.payrollProductPriceTypeId = 0;
                pt.payrollProductSettingId = to.payrollProductSettingId

                _.forEach(pt.periods, period => {
                    period.payrollProductPriceTypeId = 0
                    period.payrollProductPriceTypePeriodId = 0;
                });
            });

            _.forEach(to.priceFormulas, formula => {
                formula.payrollProductPriceFormulaId = 0;
                formula.payrollProductSettingId = to.payrollProductSettingId;
            });

        } else {
            to.priceTypes = from.priceTypes;
            to.priceFormulas = from.priceFormulas;
            to.accountingSettings = from.accountingSettings;
            to.extraFields = from.extraFields;
        }
    }

    private setDefaultSettings(payrollProductSetting: PayrollProductSettingDTO) {

        //Rounding
        payrollProductSetting.centRoundingType = TermGroup_PayrollProductCentRoundingType.None;
        payrollProductSetting.centRoundingLevel = TermGroup_PayrollProductCentRoundingLevel.None

        //Tax
        payrollProductSetting.taxCalculationType = TermGroup_PayrollProductTaxCalculationType.TableTax;

        //Pension
        payrollProductSetting.pensionCompany = TermGroup_PensionCompany.NotSelected;

        //Timeunit
        payrollProductSetting.timeUnit = TermGroup_PayrollProductTimeUnit.Hours;
        payrollProductSetting.quantityRoundingType = TermGroup_PayrollProductQuantityRoundingType.None;
        payrollProductSetting.quantityRoundingMinutes = 0;

        //Salary specification
        payrollProductSetting.printOnSalarySpecification = true;
        payrollProductSetting.printDate = true;
    }
}