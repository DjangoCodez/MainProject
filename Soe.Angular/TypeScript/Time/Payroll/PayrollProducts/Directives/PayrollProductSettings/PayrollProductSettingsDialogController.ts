import { PayrollProductSettingDTO, PayrollProductDTO } from "../../../../../Common/Models/ProductDTOs";
import { IPayrollPriceTypeAndFormulaDTO, ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { TermGroup_SysPayrollType, TermGroup_PayrollProductCentRoundingType, TermGroup_PayrollProductCentRoundingLevel, TermGroup_PayrollProductTaxCalculationType, TermGroup_PensionCompany, TermGroup_PayrollProductTimeUnit, TermGroup_PayrollProductQuantityRoundingType, TermGroup_PayrollResultType } from "../../../../../Util/CommonEnumerations";
import { AccountDimSmallDTO } from "../../../../../Common/Models/AccountDimDTO";
import { ExtraFieldRecordDTO } from "../../../../../Common/Models/ExtraFieldDTO";
import { Guid } from "../../../../../Util/StringUtility";

export class PayrollProductSettingsDialogController {

    private setting: PayrollProductSettingDTO;
    private isNew: boolean;

    private isCentRoundingTypeDisabled: boolean;
    private isCentRoundingLevelDisabled: boolean;
    private isTaxCalculationTypeDisabled: boolean;
    private isPensionCompanyDisabled: boolean;
    private isTimeUnitDisabled: boolean;
    private isQuantityRoundingTypeDisabled: boolean;
    private isQuantityRoundingMinutesDisabled: boolean;
    private isChildProductDisabled: boolean;

    private isPrintOnSalarySpecificationReadOnly: boolean;
    private isPrintDateReadOnly: boolean;
    private isDontIncludeInRetroactivePayrollReadOnly: boolean;
    private isVacationSalaryPromotedReadOnly: boolean;
    private isUnionFeePromotedReadOnly: boolean;
    private isWorkingTimePromotedReadOnly: boolean;
    private isCalculateSupplementChargeReadOnly: boolean;
    private isCalculateSicknessSalaryReadOnly: boolean;

    private isPriceTypesAndFormulasDisabled: boolean;

    private timeUnits: ISmallGenericType[]
    private payrollProductAccountingPrioRows: number[];

    private _centRoundingType: ISmallGenericType;
    private get centRoundingType(): ISmallGenericType {
        return this._centRoundingType;
    }
    private set centRoundingType(type: ISmallGenericType) {
        this._centRoundingType = type;

        this.setting.centRoundingType = type ? <TermGroup_PayrollProductCentRoundingType>type.id : TermGroup_PayrollProductCentRoundingType.None;
        this.setting.centRoundingTypeName = type ? type.name : "";

        if (this.setting.centRoundingType === TermGroup_PayrollProductCentRoundingType.None) {
            this.centRoundingLevel = _.find(this.centRoundingLevels, t => t.id === TermGroup_PayrollProductCentRoundingLevel.None);
            this.isCentRoundingLevelDisabled = true;
        } else {
            this.isCentRoundingLevelDisabled = false;
        }
    }

    private _centRoundingLevel: ISmallGenericType;
    private get centRoundingLevel(): ISmallGenericType {
        return this._centRoundingLevel;
    }
    private set centRoundingLevel(type: ISmallGenericType) {
        this._centRoundingLevel = type;

        this.setting.centRoundingLevel = type ? <TermGroup_PayrollProductCentRoundingLevel>type.id : TermGroup_PayrollProductCentRoundingLevel.None;
        this.setting.centRoundingLevelName = type ? type.name : "";
    }

    private _taxCalculationType: ISmallGenericType;
    private get taxCalculationType(): ISmallGenericType {
        return this._taxCalculationType;
    }
    private set taxCalculationType(type: ISmallGenericType) {
        this._taxCalculationType = type;

        this.setting.taxCalculationType = type ? <TermGroup_PayrollProductTaxCalculationType>type.id : TermGroup_PayrollProductTaxCalculationType.TableTax;
        this.setting.taxCalculationTypeName = type ? type.name : "";
    }

    private _pensionCompany: ISmallGenericType;
    private get pensionCompany(): ISmallGenericType {
        return this._pensionCompany;
    }
    private set pensionCompany(type: ISmallGenericType) {
        this._pensionCompany = type;

        this.setting.pensionCompany = type ? <TermGroup_PensionCompany>type.id : TermGroup_PensionCompany.NotSelected;
        this.setting.pensionCompanyName = type ? type.name : "";
    }

    private _timeUnit: ISmallGenericType;
    private get timeUnit(): ISmallGenericType {
        return this._timeUnit;
    }
    private set timeUnit(type: ISmallGenericType) {
        this._timeUnit = type;

        this.setting.timeUnit = type ? <TermGroup_PayrollProductTimeUnit>type.id : TermGroup_PayrollProductTimeUnit.Hours;
        this.setting.timeUnitName = type ? type.name : "";

        if (this.setting.timeUnit !== TermGroup_PayrollProductTimeUnit.Hours) {
            this.isQuantityRoundingTypeDisabled = true;
            this.quantityRoundingType = _.find(this.quantityRoundingTypes, t => t.id === TermGroup_PayrollProductQuantityRoundingType.None);
        }
        else {
            this.isQuantityRoundingTypeDisabled = false;
        }
    }

    private _quantityRoundingType: ISmallGenericType;
    private get quantityRoundingType(): ISmallGenericType {
        return this._quantityRoundingType;
    }
    private set quantityRoundingType(type: ISmallGenericType) {
        this._quantityRoundingType = type;

        this.setting.quantityRoundingType = type ? <TermGroup_PayrollProductQuantityRoundingType>type.id : TermGroup_PayrollProductQuantityRoundingType.None;
        this.setting.quantityRoundingTypeName = type ? type.name : "";

        if (this.setting.quantityRoundingType === TermGroup_PayrollProductQuantityRoundingType.None) {
            this.isQuantityRoundingMinutesDisabled = true;
            this.setting.quantityRoundingMinutes = 0;
        } else {
            this.isQuantityRoundingMinutesDisabled = false;
        }
    }

    private _childProduct: ISmallGenericType;
    private get childProduct(): ISmallGenericType {
        return this._childProduct;
    }
    private set childProduct(product: ISmallGenericType) {
        this._childProduct = product;

        this.setting.childProductId = product ? product.id : null;
        this.setting.childProductName = product ? product.name : "";
    }

    // Extra fields
    public guid: Guid = Guid.newGuid();
    private extraFieldRecords: ExtraFieldRecordDTO[];
    get showExtraFieldsExpander() {
        return this.hasExtraFields;
    }
    extraFieldsExpanderRendered = false;

    //@ngInject
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private readOnly: boolean,
        private settingOrginal: PayrollProductSettingDTO,
        private product: PayrollProductDTO,
        private centRoundingTypes: ISmallGenericType[],
        private centRoundingLevels: ISmallGenericType[],
        private taxCalculationTypes: ISmallGenericType[],
        private pensionCompanies: ISmallGenericType[],
        private quantityRoundingTypes: ISmallGenericType[],
        private payrollProductChildren: ISmallGenericType[],
        private payrollPriceTypesAndFormulas: IPayrollPriceTypeAndFormulaDTO[],
        timeUnits: ISmallGenericType[],
        private payrollProductAccountingPrios: ISmallGenericType[],
        private accountDims: AccountDimSmallDTO[],
        private accountSettingTypes: ISmallGenericType[],
        private hasExtraFields: boolean) {

        this.isNew = !this.settingOrginal;
        this.setting = new PayrollProductSettingDTO();
        angular.extend(this.setting, this.settingOrginal);

        if (this.product.sysPayrollTypeLevel1 > 0) {
            if (this.isAbsenceVacation() || this.isVacationAddition() || this.isVacationSalary())
                this.timeUnits = _.filter(timeUnits, x => x.id !== TermGroup_PayrollProductTimeUnit.CalenderDayFactor);
            else
                this.timeUnits = _.filter(timeUnits, x => x.id !== TermGroup_PayrollProductTimeUnit.VacationCoefficient);
        }
        this.setup();
    }

    public setup() {

        this.centRoundingType = _.find(this.centRoundingTypes, t => t.id === this.setting.centRoundingType);
        this.centRoundingLevel = _.find(this.centRoundingLevels, t => t.id === this.setting.centRoundingLevel);
        this.taxCalculationType = _.find(this.taxCalculationTypes, t => t.id === this.setting.taxCalculationType);
        this.pensionCompany = _.find(this.pensionCompanies, t => t.id === this.setting.pensionCompany);
        this.timeUnit = _.find(this.timeUnits, t => t.id === this.setting.timeUnit);
        this.quantityRoundingType = _.find(this.quantityRoundingTypes, t => t.id === this.setting.quantityRoundingType);
        this.childProduct = _.find(this.payrollProductChildren, t => t.id === this.setting.childProductId);

        // Accounting prio
        this.payrollProductAccountingPrioRows = [];
        let prios = this.setting.accountingPrio.split(',');
        _.forEach(prios, prio => {
            // Format can be one of these:
            // 0,0,0,0,0,0
            // 1=0,2=0,3=0,4=0,5=0,6=0
            let parts = prio.split('=');
            let accPrio = _.find(this.payrollProductAccountingPrios, p => p.id === parseInt(parts[(parts.length - 1)], 10));
            if (!accPrio)
                accPrio = _.find(this.payrollProductAccountingPrios, p => p.id === 0);
            if (accPrio)
                this.payrollProductAccountingPrioRows.push(accPrio.id);
        });

        this.extraFieldRecords = this.setting.extraFields;

        this.setDisabled();
    }

    //Events

    public onExtraFieldsExpanderOpenClose() {
        this.extraFieldsExpanderRendered = !this.extraFieldsExpanderRendered;
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        // Accounting prio
        let prioString: string = '';
        for (let i = 0; i < 6; i++) {
            if (prioString.length > 0)
                prioString += ',';
            prioString += "{0}={1}".format((i + 1).toString(), this.payrollProductAccountingPrioRows[i] ? this.payrollProductAccountingPrioRows[i].toString() : "0");
        }
        this.setting.accountingPrio = prioString;

        this.setting.extraFields = this.extraFieldRecords;

        this.$uibModalInstance.close({ setting: this.setting });
    }

    //Help Methods

    setDisabled() {
        //Enable/Disable
        let enablePromotion = (
            this.product.sysPayrollTypeLevel1 === TermGroup_SysPayrollType.SE_GrossSalary ||
            this.product.sysPayrollTypeLevel1 === TermGroup_SysPayrollType.SE_Benefit);

        let disableAllExcepySalarySpecificationSettings = (
            this.product.sysPayrollTypeLevel1 === TermGroup_SysPayrollType.SE_Tax ||
            this.product.sysPayrollTypeLevel1 === TermGroup_SysPayrollType.SE_EmploymentTaxCredit ||
            this.product.sysPayrollTypeLevel1 === TermGroup_SysPayrollType.SE_EmploymentTaxDebit ||
            this.product.sysPayrollTypeLevel1 === TermGroup_SysPayrollType.SE_SupplementChargeCredit ||
            this.product.sysPayrollTypeLevel1 === TermGroup_SysPayrollType.SE_SupplementChargeDebit
        );

        let disableCalculationExpander = this.product.sysPayrollTypeLevel2 === TermGroup_SysPayrollType.SE_Deduction_SalaryDistress;

        //Rounding
        this.isCentRoundingTypeDisabled = this.readOnly || disableAllExcepySalarySpecificationSettings || disableCalculationExpander;
        this.isCentRoundingLevelDisabled = this.isCentRoundingTypeDisabled || this.setting.centRoundingType === TermGroup_PayrollProductCentRoundingType.None;

        //Tax
        this.isTaxCalculationTypeDisabled = this.readOnly || disableAllExcepySalarySpecificationSettings || disableCalculationExpander;

        //Pension
        this.isPensionCompanyDisabled = this.readOnly || disableAllExcepySalarySpecificationSettings || disableCalculationExpander;

        //Timeunit
        this.isTimeUnitDisabled = this.readOnly || disableAllExcepySalarySpecificationSettings || disableCalculationExpander || this.product.resultType === TermGroup_PayrollResultType.Quantity;
        this.isQuantityRoundingTypeDisabled = this.isTimeUnitDisabled || this.setting.timeUnit !== TermGroup_PayrollProductTimeUnit.Hours;
        this.isQuantityRoundingMinutesDisabled = this.isQuantityRoundingTypeDisabled || this.setting.quantityRoundingType === TermGroup_PayrollProductQuantityRoundingType.None;

        //ProductChain
        this.isChildProductDisabled = this.readOnly || disableAllExcepySalarySpecificationSettings || disableCalculationExpander;

        //Salaryspecification
        this.isPrintOnSalarySpecificationReadOnly = this.readOnly || disableCalculationExpander;
        this.isPrintDateReadOnly = this.readOnly || disableCalculationExpander;

        //Retroactive payroll
        this.isDontIncludeInRetroactivePayrollReadOnly = this.readOnly || disableCalculationExpander;

        //Promotion
        this.isVacationSalaryPromotedReadOnly = this.readOnly || !enablePromotion || disableAllExcepySalarySpecificationSettings || disableCalculationExpander;
        this.isUnionFeePromotedReadOnly = this.readOnly || !enablePromotion || disableAllExcepySalarySpecificationSettings || disableCalculationExpander;
        this.isWorkingTimePromotedReadOnly = this.readOnly || !enablePromotion || disableAllExcepySalarySpecificationSettings || disableCalculationExpander;
        this.isCalculateSupplementChargeReadOnly = this.readOnly || !enablePromotion || disableAllExcepySalarySpecificationSettings || disableCalculationExpander;
        this.isCalculateSicknessSalaryReadOnly = this.readOnly || !enablePromotion || disableAllExcepySalarySpecificationSettings || disableCalculationExpander;

        //Pricetypes and formulas
        this.isPriceTypesAndFormulasDisabled = this.readOnly || disableAllExcepySalarySpecificationSettings;
    }

    isAbsenceVacation() {
        return this.product.sysPayrollTypeLevel1 === TermGroup_SysPayrollType.SE_GrossSalary &&
            this.product.sysPayrollTypeLevel2 === TermGroup_SysPayrollType.SE_GrossSalary_Absence &&
            this.product.sysPayrollTypeLevel3 === TermGroup_SysPayrollType.SE_GrossSalary_Absence_Vacation;
    }

    isVacationAddition() {
        return this.product.sysPayrollTypeLevel1 === TermGroup_SysPayrollType.SE_GrossSalary &&
            this.product.sysPayrollTypeLevel2 === TermGroup_SysPayrollType.SE_GrossSalary_VacationAddition;
    }

    isVacationSalary() {
        return this.product.sysPayrollTypeLevel1 === TermGroup_SysPayrollType.SE_GrossSalary &&
            this.product.sysPayrollTypeLevel2 === TermGroup_SysPayrollType.SE_GrossSalary_VacationSalary;
    }
}
