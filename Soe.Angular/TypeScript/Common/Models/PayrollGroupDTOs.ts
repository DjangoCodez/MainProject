import { IPayrollGroupDTO, IPayrollGroupPayrollProductDTO, IPayrollGroupPriceFormulaDTO, IPayrollGroupPriceTypeDTO, IPayrollGroupReportDTO, IPayrollGroupVacationGroupDTO, IPayrollGroupSmallDTO, IPayrollGroupPriceTypePeriodDTO, IPayrollPriceFormulaResultDTO, IPayrollGroupGridDTO, IPayrollGroupAccountsDTO, IPayrollPriceFormulaDTO, IForaColletiveAgrementDTO, IPayrollPriceTypeDTO, IPriceTypeLevelDTO } from "../../Scripts/TypeLite.Net4";
import { SoeEntityState, TermGroup_VacationGroupCalculationType, TermGroup_VacationGroupVacationDaysHandleRule, TermGroup_VacationGroupVacationHandleRule, SettingDataType, PayrollGroupSettingType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { TimePeriodHeadDTO } from "./TimePeriodHeadDTO";

export declare type SettingType = boolean | string | number | Date;

export class PayrollGroupDTO implements IPayrollGroupDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    externalCodes: string[];
    externalCodesString: string;
    modified: Date;
    modifiedBy: string;
    name: string;
    oneTimeTaxFormulaId: number;
    payrollGroupId: number;
    payrollProducts: PayrollGroupPayrollProductDTO[];
    priceFormulas: PayrollGroupPriceFormulaDTO[];
    priceTypes: PayrollGroupPriceTypeDTO[];
    reportIds: number[];
    reports: PayrollGroupReportDTO[];
    state: SoeEntityState;
    timePeriodHead: TimePeriodHeadDTO;
    timePeriodHeadId: number;
    vacations: PayrollGroupVacationGroupDTO[];

    accounts: PayrollGroupAccountsDTO[]
    settings: PayrollGroupSettingDTO[];

    // Extensions
    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }

    // Settings

    private getSetting(type: PayrollGroupSettingType): PayrollGroupSettingDTO {
        return _.find(this.settings, s => s.type == type);
    }

    private getSettingValue(type: PayrollGroupSettingType): SettingType {
        const setting: PayrollGroupSettingDTO = this.getSetting(type);
        if (setting) {
            switch (setting.dataType) {
                case SettingDataType.Boolean:
                    return setting.boolData;
                case SettingDataType.String:
                    return setting.strData;
                case SettingDataType.Integer:
                    return setting.intData;
                case SettingDataType.Decimal:
                    return setting.decimalData;
                case SettingDataType.Date:
                    return setting.dateData;
                case SettingDataType.Time:
                    return CalendarUtility.convertToDate(setting.timeData);
            }
        }

        return null;
    }

    private setStringSetting(type: PayrollGroupSettingType, value: string) {
        let setting: PayrollGroupSettingDTO = this.getSetting(type);
        if (!setting) {
            setting = new PayrollGroupSettingDTO(type, SettingDataType.String);
            this.settings.push(setting);
        }
        setting.strData = value;
    }

    private setIntSetting(type: PayrollGroupSettingType, value: number) {
        let setting: PayrollGroupSettingDTO = this.getSetting(type);
        if (!setting) {
            setting = new PayrollGroupSettingDTO(type, SettingDataType.Integer);
            this.settings.push(setting);
        }
        setting.intData = value;
    }

    private setDecimalSetting(type: PayrollGroupSettingType, value: number) {
        let setting: PayrollGroupSettingDTO = this.getSetting(type);
        if (!setting) {
            setting = new PayrollGroupSettingDTO(type, SettingDataType.Decimal);
            this.settings.push(setting);
        }
        setting.decimalData = value;
    }

    private setBoolSetting(type: PayrollGroupSettingType, value: boolean) {
        let setting: PayrollGroupSettingDTO = this.getSetting(type);
        if (!setting) {
            setting = new PayrollGroupSettingDTO(type, SettingDataType.Boolean);
            this.settings.push(setting);
        }
        setting.boolData = value;
    }

    private setDateSetting(type: PayrollGroupSettingType, value: Date) {
        let setting: PayrollGroupSettingDTO = this.getSetting(type);
        if (!setting) {
            setting = new PayrollGroupSettingDTO(type, SettingDataType.Date);
            this.settings.push(setting);
        }
        setting.dateData = value;
    }

    private setTimeSetting(type: PayrollGroupSettingType, value: Date) {
        let setting: PayrollGroupSettingDTO = this.getSetting(type);
        if (!setting) {
            setting = new PayrollGroupSettingDTO(type, SettingDataType.Time);
            this.settings.push(setting);
        }
        setting.timeData = value;
    }

    public get overTimeCompensation(): boolean {
        return <boolean>this.getSettingValue(PayrollGroupSettingType.OverTimeCompensation);
    }
    public set overTimeCompensation(value: boolean) {
        this.setBoolSetting(PayrollGroupSettingType.OverTimeCompensation, value);
    }

    public get exceptionInWorkingAgreement(): boolean {
        return <boolean>this.getSettingValue(PayrollGroupSettingType.Exception2to6InWorkingAgreement);
    }
    public set exceptionInWorkingAgreement(value: boolean) {
        this.setBoolSetting(PayrollGroupSettingType.Exception2to6InWorkingAgreement, value);
    }

    public get travelCompensation(): boolean {
        return <boolean>this.getSettingValue(PayrollGroupSettingType.TravelCompensation);
    }
    public set travelCompensation(value: boolean) {
        this.setBoolSetting(PayrollGroupSettingType.TravelCompensation, value);
    }

    public get vacationRights(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.VacationRights);
    }
    public set vacationRights(value: number) {
        this.setIntSetting(PayrollGroupSettingType.VacationRights, value);
    }

    public get workTimeShiftCompensation(): boolean {
        return <boolean>this.getSettingValue(PayrollGroupSettingType.WorkTimeShiftCompensation);
    }
    public set workTimeShiftCompensation(value: boolean) {
        this.setBoolSetting(PayrollGroupSettingType.WorkTimeShiftCompensation, value);
    }

    public get monthlyWorkTime(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.MonthlyWorkTime);
    }
    public set monthlyWorkTime(value: number) {
        this.setDecimalSetting(PayrollGroupSettingType.MonthlyWorkTime, value);
    }

    public get earnedHoliday(): boolean {
        return <boolean>this.getSettingValue(PayrollGroupSettingType.EarnedHoliday);
    }
    public set earnedHoliday(value: boolean) {
        this.setBoolSetting(PayrollGroupSettingType.EarnedHoliday, value);
    }

    public get sicknessSalaryRegulation(): boolean {
        return <boolean>this.getSettingValue(PayrollGroupSettingType.SicknessSalaryRegulation);
    }
    public set sicknessSalaryRegulation(value: boolean) {
        this.setBoolSetting(PayrollGroupSettingType.SicknessSalaryRegulation, value);
    }

    public get payrollReportsPersonalCategory(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.PayrollReportsPersonalCategory);
    }
    public set payrollReportsPersonalCategory(value: number) {
        this.setIntSetting(PayrollGroupSettingType.PayrollReportsPersonalCategory, value);
    }

    public get payrollReportsWorkTimeCategory(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.PayrollReportsWorkTimeCategory);
    }
    public set payrollReportsWorkTimeCategory(value: number) {
        this.setIntSetting(PayrollGroupSettingType.PayrollReportsWorkTimeCategory, value);
    }
    public get payrollReportsJobStatus(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.PayrollReportsJobStatus);
    }
    public set payrollReportsJobStatus(value: number) {
        this.setIntSetting(PayrollGroupSettingType.PayrollReportsJobStatus, value);
    }

    public get payrollReportsSalaryType(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.PayrollReportsSalaryType);
    }
    public set payrollReportsSalaryType(value: number) {
        this.setIntSetting(PayrollGroupSettingType.PayrollReportsSalaryType, value);
    }

    public get experienceMonthsFormula(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.ExperienceMonthsFormula);
    }
    public set experienceMonthsFormula(value: number) {
        this.setIntSetting(PayrollGroupSettingType.ExperienceMonthsFormula, value);
    }

    public get payrollFormula(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.PayrollFormula);
    }
    public set payrollFormula(value: number) {
        this.setIntSetting(PayrollGroupSettingType.PayrollFormula, value);
    }

    public get partnerNumber(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.PartnerNumber);
    }
    public set partnerNumber(value: number) {
        this.setIntSetting(PayrollGroupSettingType.PartnerNumber, value);
    }

    public get agreementCode(): string {
        return <string>this.getSettingValue(PayrollGroupSettingType.AgreementCode);
    }
    public set agreementCode(value: string) {
        this.setStringSetting(PayrollGroupSettingType.AgreementCode, value);
    }

    public get kpaAgreementNumber(): string {
        return <string>this.getSettingValue(PayrollGroupSettingType.KPAAgreementNumber);
    }
    public set kpaAgreementNumber(value: string) {
        this.setStringSetting(PayrollGroupSettingType.KPAAgreementNumber, value);
    }

    public get kpaAgreementType(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.KPAAgreementType);
    }
    public set kpaAgreementType(value: number) {
        this.setIntSetting(PayrollGroupSettingType.KPAAgreementType, value);
    }

    public get kpaBelonging(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.KPABelonging);
    }
    public set kpaBelonging(value: number) {
        this.setIntSetting(PayrollGroupSettingType.KPABelonging, value);
    }

    public get kpaRetirementAge(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.KPARetirementAge);
    }
    public set kpaRetirementAge(value: number) {
        this.setIntSetting(PayrollGroupSettingType.KPARetirementAge, value);
    }

    public get kpaFireman(): boolean {
        return <boolean>this.getSettingValue(PayrollGroupSettingType.KPAFireman);
    }
    public set kpaFireman(value: boolean) {
        this.setBoolSetting(PayrollGroupSettingType.KPAFireman, value);
    }

    public get kpaPercentBelowBaseAmount(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.KPAPercentBelowBaseAmount);
    }
    public set kpaPercentBelowBaseAmount(value: number) {
        this.setDecimalSetting(PayrollGroupSettingType.KPAPercentBelowBaseAmount, value);
    }

    public get kpaPercentAboveBaseAmount(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.KPAPercentAboveBaseAmount);
    }
    public set kpaPercentAboveBaseAmount(value: number) {        
        this.setDecimalSetting(PayrollGroupSettingType.KPAPercentAboveBaseAmount, value);
    }

    public get kpaDirektSalaryFormula(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.KPADirektSalaryFormula);
    }
    public set kpaDirektSalaryFormula(value: number) {
        this.setIntSetting(PayrollGroupSettingType.KPADirektSalaryFormula, value);
    }

    public get foraCollectiveAgreement(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.ForaCollectiveAgreement);
    }
    public set foraCollectiveAgreement(value: number) {
        this.setIntSetting(PayrollGroupSettingType.ForaCollectiveAgreement, value);
    }

    public get bygglosenAgreementArea(): string {
        return <string>this.getSettingValue(PayrollGroupSettingType.BygglosenAgreementArea);
    }
    public set bygglosenAgreementArea(value: string) {
        this.setStringSetting(PayrollGroupSettingType.BygglosenAgreementArea, value);
    }

    public get bygglosenAllocationNumber(): string {
        return <string>this.getSettingValue(PayrollGroupSettingType.BygglosenAllocationNumber);
    }
    public set bygglosenAllocationNumber(value: string) {
        this.setStringSetting(PayrollGroupSettingType.BygglosenAllocationNumber, value);
    }

    public get bygglosenSalaryFormula(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.BygglosenSalaryFormula);
    }
    public set bygglosenSalaryFormula(value: number) {
        this.setIntSetting(PayrollGroupSettingType.BygglosenSalaryFormula, value);
    }

    public get bygglosenSalaryType(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.BygglosenSalaryType);
    }
    public set bygglosenSalaryType(value: number) {
        this.setIntSetting(PayrollGroupSettingType.BygglosenSalaryType, value);
    }

    public get byggLosenWorkPlaceNr(): string {
        return <string>this.getSettingValue(PayrollGroupSettingType.ByggLosenWorkPlaceNr);
    }

    public set byggLosenWorkPlaceNr(value: string) {
        this.setStringSetting(PayrollGroupSettingType.ByggLosenWorkPlaceNr, value);
    }

    public get bygglosenAgreedHourlyPayLevel(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.BygglosenAgreedHourlyPayLevel);
    }

    public set bygglosenAgreedHourlyPayLevel(value: number) {
        this.setDecimalSetting(PayrollGroupSettingType.BygglosenAgreedHourlyPayLevel, value);
    }

    public get gtpAgreementNumber(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.GTPAgreementNumber);
    }
    public set gtpAgreementNumber(value: number) {
        this.setIntSetting(PayrollGroupSettingType.GTPAgreementNumber, value);
    }

    public get monthlyWorkTimeCalculationType(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.MonthlyWorkTimeCalculationType);
    }
    public set monthlyWorkTimeCalculationType(value: number) {
        this.setIntSetting(PayrollGroupSettingType.MonthlyWorkTimeCalculationType, value);
    }

    public get skandiaPensionType(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.SkandiaPensionType);
    }
    public set skandiaPensionType(value: number) {
        this.setIntSetting(PayrollGroupSettingType.SkandiaPensionType, value);
    }

    public get skandiaPensionCategory(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.SkandiaPensionCategory);
    }
    public set skandiaPensionCategory(value: number) {
        this.setIntSetting(PayrollGroupSettingType.SkandiaPensionCategory, value);
    }

    public get skandiaPensionSalaryFormula(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.SkandiaPensionSalaryFormula);
    }
    public set skandiaPensionSalaryFormula(value: number) {
        this.setIntSetting(PayrollGroupSettingType.SkandiaPensionSalaryFormula, value);
    }

    public get skandiaPensionPercentBelowBaseAmount(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.SkandiaPensionPercentBelowBaseAmount);
    }
    public set skandiaPensionPercentBelowBaseAmount(value: number) {        
        this.setDecimalSetting(PayrollGroupSettingType.SkandiaPensionPercentBelowBaseAmount, value);
    }

    public get skandiaPensionPercentAboveBaseAmount(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.SkandiaPensionPercentAboveBaseAmount);
    }
    public set skandiaPensionPercentAboveBaseAmount(value: number) {        
        this.setDecimalSetting(PayrollGroupSettingType.SkandiaPensionPercentAboveBaseAmount, value);
    }
    public get skandiaPensionStartDate(): Date {
        return <Date>this.getSettingValue(PayrollGroupSettingType.SkandiaPensionStartDate);
    }
    public set skandiaPensionStartDate(value: Date) {
        this.setDateSetting(PayrollGroupSettingType.SkandiaPensionStartDate, value);
    }

    public get foraCategory(): number {
        return <number>this.getSettingValue(PayrollGroupSettingType.ForaCategory);
    }
    public set foraCategory(value: number) {
        this.setIntSetting(PayrollGroupSettingType.ForaCategory, value);
    }


    public get foraFok(): string {
        return <string>this.getSettingValue(PayrollGroupSettingType.ForaFok);
    }

    public set foraFok(value: string) {
        this.setStringSetting(PayrollGroupSettingType.ForaFok, value);
    }
}

export class PayrollGroupGridDTO implements IPayrollGroupGridDTO {
    name: string;
    payrollGroupId: number;
    state: SoeEntityState;
    timePeriodHeadName: string;

    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }
}

export class PayrollGroupSmallDTO implements IPayrollGroupSmallDTO {
    name: string;
    payrollGroupId: number;
    priceTypes: PayrollGroupPriceTypeDTO[];
    priceTypeLevels: IPriceTypeLevelDTO[];
}

export class PayrollGroupAccountsDTO implements IPayrollGroupAccountsDTO {
    employmentTaxAccountId: number;
    employmentTaxAccountName: string;
    employmentTaxAccountNr: string;
    employmentTaxPercent: number;
    fromInterval: number;
    isModified: boolean;
    ownSupplementChargeAccountId: number;
    ownSupplementChargeAccountName: string;
    ownSupplementChargeAccountNr: string;
    ownSupplementChargePercent: number;
    payrollTaxAccountId: number;
    payrollTaxAccountName: string;
    payrollTaxAccountNr: string;
    payrollTaxPercent: number;
    toInterval: number;

    // Extensions
    public get fromIntervalSort(): number {
        return this.fromInterval || 0;
    }
    public get toIntervalSort(): number {
        return this.toInterval || 0;
    }
}

export class PayrollGroupPriceTypeDTO implements IPayrollGroupPriceTypeDTO {
    payrollLevelId: number;
    payrollLevelName: string;
    currentAmount: number;
    payrollGroupId: number;
    payrollGroupPriceTypeId: number;
    payrollPriceType: IPayrollPriceTypeDTO;
    payrollPriceTypeCurrentAmount: number;
    payrollPriceTypeId: number;
    periods: PayrollGroupPriceTypePeriodDTO[];
    priceTypeCode: string;
    priceTypeName: string;
    readOnlyOnEmployee: boolean;
    showOnEmployee: boolean;
    sort: number;
    priceTypeLevel: IPriceTypeLevelDTO;

    public setTypes() {
        if (this.periods) {
            this.periods = this.periods.map(p => {
                const pObj = new PayrollGroupPriceTypePeriodDTO();
                angular.extend(pObj, p);
                pObj.fixDates();
                return pObj;
            });
        } else {
            this.periods = [];
        }
    }

    public get priceTypeNameAndLevelName(): string {
        if (this.payrollLevelId && this.payrollLevelId != 0)
            return this.priceTypeName + "-" + this.payrollLevelName;
        else
            return this.priceTypeName;
    }
}

export class PayrollGroupPriceTypePeriodDTO implements IPayrollGroupPriceTypePeriodDTO {
    amount: number;
    fromDate: Date;
    payrollGroupPriceTypeId: number;
    payrollGroupPriceTypePeriodId: number;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
    }
}

export class PayrollGroupPriceFormulaDTO implements IPayrollGroupPriceFormulaDTO {
    formulaExtracted: string;
    formulaName: string;
    formulaNames: string;
    formulaPlain: string;
    fromDate: Date;
    payrollGroupId: number;
    payrollGroupPriceFormulaId: number;
    payrollPriceFormulaId: number;
    result: number;
    showOnEmployee: boolean;
    toDate: Date;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
        this.toDate = CalendarUtility.convertToDate(this.toDate);
    }
}

export class PayrollGroupPayrollProductDTO implements IPayrollGroupPayrollProductDTO {
    distribute: boolean;
    payrollGroupId: number;
    payrollGroupPayrollProductId: number;
    productId: number;
    productName: string;
    productNr: string;
    state: SoeEntityState;
}

export class PayrollGroupReportDTO implements IPayrollGroupReportDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    payrollGroupId: number;
    payrollGroupReportId: number;
    reportDescription: string;
    reportId: number;
    reportName: string;
    reportNameDesc: string;
    reportNr: number;
    state: SoeEntityState;
    sysReportTemplateTypeId: number;
    employeeTemplateId: number;
}

export class PayrollGroupSettingDTO {
    boolData: boolean;
    dataType: SettingDataType;
    dateData: Date;
    decimalData: number;
    id: number;
    intData: number;
    name: string;
    payrollGroupId: number;
    state: SoeEntityState;
    strData: string;
    timeData: Date;
    type: PayrollGroupSettingType;

    constructor(type: PayrollGroupSettingType, dataType: SettingDataType) {
        this.type = type;
        this.dataType = dataType;
    }

    public fixDates() {
        this.dateData = CalendarUtility.convertToDate(this.dateData);
        this.timeData = CalendarUtility.convertToDate(this.timeData);
    }
}

export class PayrollGroupVacationGroupDTO implements IPayrollGroupVacationGroupDTO {
    calculationType: TermGroup_VacationGroupCalculationType;
    created: Date;
    createdBy: string;
    isDefault: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    payrollGroupId: number;
    payrollGroupVacationGroupId: number;
    state: SoeEntityState;
    type: number;
    vacationDaysHandleRule: TermGroup_VacationGroupVacationDaysHandleRule;
    vacationGroupId: number;
    vacationHandleRule: TermGroup_VacationGroupVacationHandleRule;
}

export class PayrollPriceFormulaDTO implements IPayrollPriceFormulaDTO {
    actorCompanyId: number;
    code: string;
    created: Date;
    createdBy: string;
    description: string;
    formula: string;
    formulaPlain: string;
    isActive: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    payrollPriceFormulaId: number;
    state: SoeEntityState;
}

export class PayrollPriceFormulaResultDTO implements IPayrollPriceFormulaResultDTO {
    amount: number;
    formula: string;
    formulaExtracted: string;
    formulaNames: string;
    formulaOrigin: string;
    formulaPlain: string;
    payrollPriceFormulaId: number;
    payrollPriceTypeId: number;
}

export class ForaColletiveAgrementDTO implements IForaColletiveAgrementDTO {
    id: number;
    longText: string;
    shortText: string;

    constructor(id: number, shortText: string, longText: string) {
        this.id = id;
        this.shortText = shortText;
        this.longText = longText;
    }

    public static getForColletiveAgrements(): ForaColletiveAgrementDTO[] {
        let list: ForaColletiveAgrementDTO[] = [];
        list.push(new ForaColletiveAgrementDTO(0, '', ''));
        list.push(new ForaColletiveAgrementDTO(1, 'M', 'Målare'));
        list.push(new ForaColletiveAgrementDTO(2, 'I', 'Installationsavtalet'));
        list.push(new ForaColletiveAgrementDTO(3, 'K', 'Kraftverksavtalet'));
        list.push(new ForaColletiveAgrementDTO(4, 'E', 'Electroscandiaavtalet'));
        list.push(new ForaColletiveAgrementDTO(5, 'C', 'Teknikavtalet IF Metall'));
        list.push(new ForaColletiveAgrementDTO(6, 'D', 'TEKO avtalet'));
        list.push(new ForaColletiveAgrementDTO(7, 'L', 'Livsmedelsavtalet'));
        list.push(new ForaColletiveAgrementDTO(8, 'F', 'Tobaksavtalet'));
        list.push(new ForaColletiveAgrementDTO(9, 'V', 'Avtalet för vin- och spritindustrin'));
        list.push(new ForaColletiveAgrementDTO(10, 'Y', 'Kafferosterier och kryddfabriker'));
        list.push(new ForaColletiveAgrementDTO(11, 'B', 'Byggnadsämnesindustrin'));
        list.push(new ForaColletiveAgrementDTO(12, 'U', 'Buteljglasindustrin'));
        list.push(new ForaColletiveAgrementDTO(13, 'Z', 'Motorbranschavtalet'));
        list.push(new ForaColletiveAgrementDTO(14, '1', 'Industriavtalet'));
        list.push(new ForaColletiveAgrementDTO(15, '2', 'Kemiska fabriker'));
        list.push(new ForaColletiveAgrementDTO(16, 'G', 'Glasindustrin'));
        list.push(new ForaColletiveAgrementDTO(17, '3', 'Gemensamma Metall'));
        list.push(new ForaColletiveAgrementDTO(18, '4', 'Explosivämnesindustrin'));
        list.push(new ForaColletiveAgrementDTO(19, 'A', 'I-avtalet'));
        list.push(new ForaColletiveAgrementDTO(27, '5', 'Återvinningsföretag'));
        list.push(new ForaColletiveAgrementDTO(28, 'R', 'Tvättindustrin'));
        list.push(new ForaColletiveAgrementDTO(29, '8', 'Oljeraffinaderier'));
        list.push(new ForaColletiveAgrementDTO(30, 'H', 'Sockerindustrin (Nordic Sugar AB)'));
        list.push(new ForaColletiveAgrementDTO(31, '6', 'IMG-avtalet'));
        list.push(new ForaColletiveAgrementDTO(32, 'J', 'Sågverksavtalet'));
        list.push(new ForaColletiveAgrementDTO(33, '7', 'Skogsbruk'));
        list.push(new ForaColletiveAgrementDTO(34, 'W', 'Virkesmätning'));
        list.push(new ForaColletiveAgrementDTO(35, 'P', 'Stoppmöbelindustrin'));
        list.push(new ForaColletiveAgrementDTO(36, 'T', 'Träindustri'));
        list.push(new ForaColletiveAgrementDTO(37, '9', 'Infomediaavtalet'));
        list.push(new ForaColletiveAgrementDTO(38, 'O', 'Förpackningsavtalet'));
        list.push(new ForaColletiveAgrementDTO(40, '+', 'Handel- & Metallavtalet'));
        list.push(new ForaColletiveAgrementDTO(41, '@', 'Studsviksavtalet'));
        list.push(new ForaColletiveAgrementDTO(42, '|', 'Flygtekniker med typcertifikat (medarbetaravtal)'));
        list.push(new ForaColletiveAgrementDTO(43, ']', 'Massa- och pappersindustrin'));
        list.push(new ForaColletiveAgrementDTO(44, '^', 'Stål- och metallindustrin blå avtalet'));
        list.push(new ForaColletiveAgrementDTO(45, '$', 'Tidningsavtalet'));
        list.push(new ForaColletiveAgrementDTO(47, '?', 'Bemanningsföretag'));
        list.push(new ForaColletiveAgrementDTO(48, 'Å', 'Byggavtalet'));
        list.push(new ForaColletiveAgrementDTO(49, '=', 'Dalslands kanal'));
        list.push(new ForaColletiveAgrementDTO(50, 'Ä', 'Detaljhandeln'));
        list.push(new ForaColletiveAgrementDTO(51, 'Ö', 'Entreprenadmaskinavtalet'));
        list.push(new ForaColletiveAgrementDTO(52, '[', 'Glasmästeriavtalet'));
        list.push(new ForaColletiveAgrementDTO(53, '< ', 'Göta kanalbolag AB'));
        list.push(new ForaColletiveAgrementDTO(54, '%', 'Lageravtalet'));
        list.push(new ForaColletiveAgrementDTO(55, '0', 'Lager- och E-handelsavtalet'));
        list.push(new ForaColletiveAgrementDTO(56, '€', 'Lagerpersonal vid glassföretag, filialer och depålager samt direktsäljare'));
        list.push(new ForaColletiveAgrementDTO(57, '~', 'Larm- och säkerhetsteknikavtalet'));
        list.push(new ForaColletiveAgrementDTO(58, ':', 'Maskinföraravtalet'));
        list.push(new ForaColletiveAgrementDTO(60, '*', 'Plåt- och ventilationsavtalet'));
        list.push(new ForaColletiveAgrementDTO(61, '\\', 'Privatteateravtalet(medarbetaravtal)'));
        list.push(new ForaColletiveAgrementDTO(62, '-', 'Städavtalet'));
        list.push(new ForaColletiveAgrementDTO(63, '£', 'Teknikinstallation VVS och Kyl'));
        list.push(new ForaColletiveAgrementDTO(64, '/', 'Restaurang- och caféanställda'));
        list.push(new ForaColletiveAgrementDTO(65, '{', 'Skärgårdstrafik ASL'));
        list.push(new ForaColletiveAgrementDTO(66, '> ', 'Värdepapper'));
        list.push(new ForaColletiveAgrementDTO(67, '}', 'Väg- och banavtalet'));

        return list;
    }

    public static getForaColletiveAgrement(id: number): ForaColletiveAgrementDTO {
        let list = this.getForColletiveAgrements();
        let agreement = _.find(list, a => a.id === id);
        if (!agreement)
            agreement = _.find(list, a => a.id === 0);

        return agreement;
    }
}