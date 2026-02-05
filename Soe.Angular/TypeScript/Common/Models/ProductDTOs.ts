import { IPayrollProductPriceTypeAndFormulaDTO, IProductAccountsItem, IProductSmallDTO, IProductDTO, IInvoiceProductDTO, IPriceListDTO, IAccountingSettingsRowDTO, IProductRowsProductDTO, IInvoiceProductPriceSearchViewDTO, IPayrollProductGridDTO, IPayrollProductDTO, IPayrollProductSettingDTO, IPayrollProductPriceFormulaDTO, IPayrollProductPriceTypeDTO, IPayrollProductPriceTypePeriodDTO, IPayrollPriceTypePeriodDTO } from "../../Scripts/TypeLite.Net4";
import { SoeProductType, SoeEntityState, TermGroup_InvoiceProductVatType, TermGroup_InvoiceProductCalculationType, SoeSysPriceListProviderType, PriceListOrigin, TermGroup_PayrollType, TermGroup_PayrollResultType, TermGroup_PayrollProductCentRoundingLevel, TermGroup_PayrollProductCentRoundingType, TermGroup_PensionCompany, TermGroup_PayrollProductQuantityRoundingType, TermGroup_PayrollProductTaxCalculationType, TermGroup_PayrollProductTimeUnit, TermGroup_GrossMarginCalculationType } from "../../Util/CommonEnumerations";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { AccountingSettingsRowDTO } from "./AccountingSettingsRowDTO";
import { ExtraFieldRecordDTO } from "./ExtraFieldDTO";

export class ProductAccountsDTO implements IProductAccountsItem {
    rowId: number;
    productId: number;
    salesAccountDim1Id: number;
    salesAccountDim1Nr: string;
    salesAccountDim1Name: string;
    salesAccountDim2Id: number;
    salesAccountDim2Nr: string;
    salesAccountDim2Name: string;
    salesAccountDim3Id: number;
    salesAccountDim3Nr: string;
    salesAccountDim3Name: string;
    salesAccountDim4Id: number;
    salesAccountDim4Nr: string;
    salesAccountDim4Name: string;
    salesAccountDim5Id: number;
    salesAccountDim5Nr: string;
    salesAccountDim5Name: string;
    salesAccountDim6Id: number;
    salesAccountDim6Nr: string;
    salesAccountDim6Name: string;
    purchaseAccountDim1Id: number;
    purchaseAccountDim1Nr: string;
    purchaseAccountDim1Name: string;
    purchaseAccountDim2Id: number;
    purchaseAccountDim2Nr: string;
    purchaseAccountDim2Name: string;
    purchaseAccountDim3Id: number;
    purchaseAccountDim3Nr: string;
    purchaseAccountDim3Name: string;
    purchaseAccountDim4Id: number;
    purchaseAccountDim4Nr: string;
    purchaseAccountDim4Name: string;
    purchaseAccountDim5Id: number;
    purchaseAccountDim5Nr: string;
    purchaseAccountDim5Name: string;
    purchaseAccountDim6Id: number;
    purchaseAccountDim6Nr: string;
    purchaseAccountDim6Name: string;
    vatAccountDim1Id: number;
    vatAccountDim1Nr: string;
    vatAccountDim1Name: string;
    vatRate: number;
}

export class ProductSmallDTO implements IProductSmallDTO {
    productId: number;
    number: string;
    name: string;
    numberName: string;
}

export class ProductComparisonDTO extends ProductSmallDTO {
    comparisonPrice: number;
    price: number;
    purchasePrice: number;
    startDate: Date;
    stopDate: Date;
}

export class ProductDTO extends ProductSmallDTO implements IProductDTO {
    productUnitId: number;
    productGroupId: number;
    type: SoeProductType;
    description: string;
    accountingPrio: string;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;
    productUnitCode: string;
}

export class InvoiceProductDTO extends ProductDTO implements IInvoiceProductDTO {
    sysProductId: number;
    sysPriceListHeadId: number;
    sysProductType: number;
    vatType: TermGroup_InvoiceProductVatType;
    vatFree: boolean;
    ean: string;
    purchasePrice: number;
    sysWholesellerName: string;
    calculationType: TermGroup_InvoiceProductCalculationType;
    guaranteePercentage: number;
    timeCodeId: number;
    priceListOrigin: number;
    showDescriptionAsTextRow: boolean;
    showDescrAsTextRowOnPurchase: boolean;
    dontUseDiscountPercent: boolean;
    useCalculatedCost: boolean;
    vatCodeId: number;
    householdDeductionPercentage: number;
    isStockProduct: boolean;
    householdDeductionType: number;
    isExternal: boolean;
    salesPrice: number;
    isSupplementCharge: boolean;
    priceLists: IPriceListDTO[];
    categoryIds: number[];
    accountingSettings: IAccountingSettingsRowDTO[];
    weight: number;
    intrastatCodeId: number;
    sysCountryId: number;
    defaultGrossMarginCalculationType: number;
}

export class ProductRowsProductDTO implements IProductRowsProductDTO {
    calculationType: TermGroup_InvoiceProductCalculationType;
    description: string;
    dontUseDiscountPercent: boolean;
    grossMarginCalculationType: TermGroup_GrossMarginCalculationType;
    guaranteePercentage: number;
    householdDeductionPercentage: number;
    householdDeductionType: number;
    intrastatCodeId: number;
    isInactive: boolean;
    isExternal: boolean;
    isLiftProduct: boolean;
    isStockProduct: boolean;
    isSupplementCharge: boolean;
    name: string;
    number: string;
    productId: number;
    productUnitCode: string;
    productUnitId: number;
    purchasePrice: number;
    salesPrice: number;
    showDescriptionAsTextRow: boolean;
    showDescrAsTextRowOnPurchase: boolean;
    sysProductId: number;
    sysCountryId: number;
    sysWholesellerName: string;
    vatCodeId: number;
    vatType: TermGroup_InvoiceProductVatType;
    weight: number;
}

export class InvoiceProductPriceSearchViewDTO implements IInvoiceProductPriceSearchViewDTO {
    code: string;
    companyWholesellerPriceListId: number;
    customerPrice: number;
    gnp: number;
    marginalIncome: number;
    marginalIncomeRatio: number;
    name: string;
    nettoNettoPrice: number;
    number: string;
    priceFormula: string;
    priceListOrigin: number;
    priceListType: string;
    priceStatus: number;
    productId: number;
    productProviderType: SoeSysPriceListProviderType;
    productType: number;
    purchaseUnit: string;
    salesUnit: string;
    sysPriceListHeadId: number;
    sysWholesellerId: number;
    type: number;
    wholeseller: string;

    // Extensions 
    productProviderTypeText: string;
}

export class ProductSearchResult {
    productId: number;
    priceListTypeId: number;
    purchasePrice: number;
    salesPrice: number;
    productUnit: string;
    sysPriceListHeadId: number;
    sysWholesellerName: string;
    priceListOrigin: PriceListOrigin;
    quantity: number;
}

export class PayrollProductDTO implements IPayrollProductDTO {
    averageCalculated: boolean;
    excludeInWorkTimeSummary: boolean;
    export: boolean;
    externalNumber: string;
    factor: number;
    includeAmountInExport: boolean;
    payed: boolean;
    payrollType: TermGroup_PayrollType;
    resultType: TermGroup_PayrollResultType;
    settings: PayrollProductSettingDTO[];
    shortName: string;
    sysPayrollProductId: number;
    sysPayrollTypeLevel1: number;
    sysPayrollTypeLevel2: number;
    sysPayrollTypeLevel3: number;
    sysPayrollTypeLevel4: number;
    useInPayroll: boolean;
    dontUseFixedAccounting: boolean;
    accountingPrio: string;
    created: Date;
    createdBy: string;
    description: string;
    modified: Date;
    modifiedBy: string;
    productGroupId: number;
    productUnitCode: string;
    productUnitId: number;
    state: SoeEntityState;
    type: SoeProductType;
    name: string;
    number: string;
    numberName: string;
    productId: number;
    isAbsence: boolean;

    // Extensions
    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }
}

export class PayrollProductSettingDTO implements IPayrollProductSettingDTO {
    accountingPrio: string;
    accountingSettings: AccountingSettingsRowDTO[];
    calculateSicknessSalary: boolean;
    calculateSupplementCharge: boolean;
    centRoundingLevel: TermGroup_PayrollProductCentRoundingLevel;
    centRoundingType: TermGroup_PayrollProductCentRoundingType;
    childProductId: number;
    isReadOnly: boolean;
    isSelected: boolean;
    payrollGroupId: number;
    payrollGroupName: string;
    payrollProductSettingId: number;
    pensionCompany: TermGroup_PensionCompany;
    priceFormulas: PayrollProductPriceFormulaDTO[];
    priceTypes: PayrollProductPriceTypeDTO[];
    printDate: boolean;
    printOnSalarySpecification: boolean;
    dontIncludeInRetroactivePayroll: boolean;
    dontIncludeInAbsenceCost: boolean;
    dontPrintOnSalarySpecificationWhenZeroAmount: boolean;
    extraFields: ExtraFieldRecordDTO[];
    productId: number;
    quantityRoundingMinutes: number;
    quantityRoundingType: TermGroup_PayrollProductQuantityRoundingType;
    taxCalculationType: TermGroup_PayrollProductTaxCalculationType;
    timeUnit: TermGroup_PayrollProductTimeUnit;
    unionFeePromoted: boolean;
    vacationSalaryPromoted: boolean;
    workingTimePromoted: boolean;

    // Extensions
    sort: number;
    centRoundingTypeName: string;
    centRoundingLevelName: string;
    taxCalculationTypeName: string;
    pensionCompanyName: string;
    timeUnitName: string;
    quantityRoundingTypeName: string;
    childProductName: string;
    priceTypesName: string;
    priceFormulasName: string;
    accountingName: string;
    accountingPrioName: string;
}

export class PayrollProductPriceFormulaDTO implements IPayrollProductPriceFormulaDTO {
    formulaName: string;
    fromDate: Date;
    payrollPriceFormulaId: number;
    payrollProductPriceFormulaId: number;
    payrollProductSettingId: number;
    toDate: Date;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
        this.toDate = CalendarUtility.convertToDate(this.toDate);
    }
}

export class PayrollProductPriceTypeDTO implements IPayrollProductPriceTypeDTO {
    payrollPriceTypeId: number;
    payrollProductPriceTypeId: number;
    payrollProductSettingId: number;
    periods: PayrollProductPriceTypePeriodDTO[];
    priceTypeName: string;
    priceTypePeriods: IPayrollPriceTypePeriodDTO[];
}

export class PayrollProductPriceTypePeriodDTO implements IPayrollProductPriceTypePeriodDTO {
    amount: number;
    fromDate: Date;
    payrollProductPriceTypeId: number;
    payrollProductPriceTypePeriodId: number;

    public fixDates() {
        this.fromDate = CalendarUtility.convertToDate(this.fromDate);
    }
}

export class PayrollProductPriceTypeAndFormulaDTO implements IPayrollProductPriceTypeAndFormulaDTO {
    amount: number;
    fromDate: Date;
    name: string;

    payrollProductPriceFormulaId: number;
    payrollPriceFormulaId: number;

    payrollProductPriceTypeId: number;
    payrollProductPriceTypePeriodId: number;
    payrollPriceTypeId: number;        
}

export class PayrollProductGridDTO implements IPayrollProductGridDTO {
    averageCalculated: boolean;
    excludeInWorkTimeSummary: boolean;
    export: boolean;
    externalNumber: string;
    factor: number;
    includeAmountInExport: boolean;
    isAbsence: boolean;
    isSelected: boolean;
    isVisible: boolean;
    name: string;
    number: string;
    numberSort: string;
    payed: boolean;
    payrollType: TermGroup_PayrollType;
    productId: number;
    resultType: TermGroup_PayrollResultType;
    resultTypeText: string;
    shortName: string;
    state: SoeEntityState;
    sysPayrollTypeLevel1: number;
    sysPayrollTypeLevel1Name: string;
    sysPayrollTypeLevel2: number;
    sysPayrollTypeLevel2Name: string;
    sysPayrollTypeLevel3: number;
    sysPayrollTypeLevel3Name: string;
    sysPayrollTypeLevel4: number;
    sysPayrollTypeLevel4Name: string;
    sysPayrollTypeName: string;
    useInPayroll: boolean;

    // Extensions
    public get isActive(): boolean {
        return this.state === SoeEntityState.Active;
    }
    public set isActive(value: boolean) {
        this.state = value ? SoeEntityState.Active : SoeEntityState.Inactive;
    }

    public get description(): string {
        return "{0} - {1}".format(this.number, this.name);
    }
}