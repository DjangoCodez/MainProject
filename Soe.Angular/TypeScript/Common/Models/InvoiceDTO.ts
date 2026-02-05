import { IInvoiceDTO, IOriginUserDTO, ICustomerInvoiceDTO, ISupplierInvoiceDTO, ISupplierInvoiceOrderRowDTO, IGenericImageDTO, ISupplierInvoiceProjectRowDTO, ISupplierInvoiceGridDTO, ISupplierPaymentGridDTO, ICustomerInvoiceRowAttestStateViewDTO, IEdiEntryDTO, IScanningEntryDTO, IScanningEntryRowDTO, ITextblockDTO, IUpdateEdiEntryDTO, IOrderDTO, IOriginUserSmallDTO, IProductRowDTO, ICustomerInvoiceAccountRowDTO, ICustomerInvoiceRowDTO, ISupplierInvoiceRowDTO, ICustomerInvoiceGridDTO, IBillingInvoiceDTO, System, IEdiEntryViewDTO, IMarkupDTO, ISupplierInvoiceCostOverviewDTO, IPriceBasedMarkupDTO, ISupplierInvoiceCostAllocationDTO } from "../../Scripts/TypeLite.Net4";
import { Validators } from "../../Core/Validators/Validators";
import { CalendarUtility } from "../../Util/CalendarUtility";
import { SplitAccountingRowDTO, AccountingRowDTO } from "./AccountingRowDTO";
import { SmallGenericType } from "./SmallGenericType";
import { FileUploadDTO } from "./FileUploadDTO";
import { CustomerInvoiceRowDTO } from "./CustomerInvoiceRowDTO";
import { SupplierInvoiceRowDTO } from "./SupplierInvoiceRowDTO";
import { TermGroup_BillingType, SoeOriginStatus, SoeEntityState, SoeStatusIcon, SoeInvoiceType, TermGroup_InvoiceVatType, TermGroup_OrderType, OrderInvoiceRegistrationType, TermGroup_VatDeductionType, TermGroup_EDISourceType, TermGroup_EDIStatus, TermGroup_EdiMessageType, TermGroup_EDIOrderStatus, TermGroup_EDIInvoiceStatus, TermGroup_ScanningMessageType, TermGroup_ScanningStatus, ScanningEntryRowType, TermGroup_ScanningInterpretation, SoeInvoiceRowType, AccountingRowType, TermGroup_InvoiceProductCalculationType, TermGroup_OrderEdiTransferMode, TermGroup_HouseHoldTaxDeductionType, EdiImportSource, BatchUpdatePayrollProduct, TermGroup_GrossMarginCalculationType, SoeOriginType } from "../../Util/CommonEnumerations";

export class InvoiceDTO implements IInvoiceDTO {
    actorId: number;
    billingType: TermGroup_BillingType;
    claimAccountId: number;
    contactEComId: number;
    contactGLNId: number;
    created: Date;
    createdBy: string;
    currencyDate: Date;
    currencyId: number;
    currencyRate: number;
    defaultDim1AccountId: number;
    defaultDim2AccountId: number;
    defaultDim3AccountId: number;
    defaultDim4AccountId: number;
    defaultDim5AccountId: number;
    defaultDim6AccountId: number;
    deliveryCustomerId: number;
    dueDate: Date;
    fullyPayed: boolean;
    invoiceDate: Date;
    invoiceId: number;
    invoiceNr: string;
    isTemplate: boolean;
    manuallyAdjustedAccounting: boolean;
    modified: Date;
    modifiedBy: string;
    ocr: string;
    onlyPayment: boolean;
    originDescription: string;
    originStatus: SoeOriginStatus;
    originStatusName: string;
    originUsers: IOriginUserDTO[];
    paidAmount: number;
    paidAmountCurrency: number;
    paidAmountEntCurrency: number;
    paidAmountLedgerCurrency: number;
    paymentNr: string;
    projectId: number;
    projectName: string;
    projectNr: string;
    referenceOur: string;
    referenceYour: string;
    remainingAmount: number;
    remainingAmountExVat: number;
    seqNr: number;
    state: SoeEntityState;
    statusIcon: SoeStatusIcon;
    sysPaymentTypeId: number;
    timeDiscountDate: Date;
    timeDiscountPercent: number;
    totalAmount: number;
    totalAmountCurrency: number;
    totalAmountEntCurrency: number;
    totalAmountLedgerCurrency: number;
    type: SoeInvoiceType;
    vatAmount: number;
    vatAmountCurrency: number;
    vatAmountEntCurrency: number;
    vatAmountLedgerCurrency: number;
    vatCodeId: number;
    vatType: TermGroup_InvoiceVatType;
    voucheHead2Id: number;
    voucheHeadId: number;
    voucherDate: Date;
    voucherSeriesId: number;
    voucherSeriesTypeId: number;

    constructor() {
    }
}

export class CustomerInvoiceDTO extends InvoiceDTO implements ICustomerInvoiceDTO {
    addAttachementsToEInvoice: boolean;
    addSupplierInvoicesToEInvoice: boolean;
    billingAddressId: number;
    billingAdressText: string;
    billingInvoicePrinted: boolean;
    cashSale: boolean;
    centRounding: number;
    contractGroupId: number;
    customerBlockNote: string;
    customerInvoicePaymentService: number;
    customerInvoiceRows: CustomerInvoiceRowDTO[];
    customerNameFromInvoice: string;
    deliveryAddressId: number;
    deliveryConditionId: number;
    deliveryDate: Date;
    deliveryDateText: string;
    deliveryTypeId: number;
    estimatedTime: number;
    externalDescription: string;
    externalId: string;
    fixedPriceOrder: boolean;
    freightAmount: number;
    freightAmountCurrency: number;
    freightAmountEntCurrency: number;
    freightAmountLedgerCurrency: number;
    hasHouseholdTaxDeduction: boolean;
    hasManuallyDeletedTimeProjectRows: boolean;
    hasOrder: boolean;
    includeOnInvoice: boolean;
    includeOnlyInvoicedTime: boolean;
    insecureDebt: boolean;
    internalDescription: string;
    invoiceDeliveryType: number;
    invoiceFee: number;
    invoiceFeeCurrency: number;
    invoiceFeeEntCurrency: number;
    invoiceFeeLedgerCurrency: number;
    invoiceHeadText: string;
    invoiceLabel: string;
    invoicePaymentService: number;
    invoiceText: string;
    marginalIncome: number;
    marginalIncomeCurrency: number;
    marginalIncomeEntCurrency: number;
    marginalIncomeLedgerCurrency: number;
    marginalIncomeRatio: number;
    multipleAssetRows: boolean;
    nextContractPeriodDate: Date;
    nextContractPeriodValue: number;
    nextContractPeriodYear: number;
    noOfReminders: number;
    orderDate: Date;
    orderType: TermGroup_OrderType;
    originateFrom: number;
    paymentConditionId: number;
    plannedStartDate: Date;
    plannedStopDate: Date;
    priceListTypeId: number;
    printTimeReport: boolean;
    priority: number;
    registrationType: OrderInvoiceRegistrationType;
    remainingTime: number;
    shiftTypeId: number;
    sumAmount: number;
    sumAmountCurrency: number;
    sumAmountEntCurrency: number;
    sumAmountLedgerCurrency: number;
    sysWholeSellerId: number;
    workingDescription: string;
    triangulationSales: boolean;

    // Extensions
    accountingRows: AccountingRowDTO[];

    constructor() {
        super();

        this.type = SoeInvoiceType.CustomerInvoice;
        this.billingType = TermGroup_BillingType.Debit;
        this.vatType = TermGroup_InvoiceVatType.Merchandise;
        this.totalAmountCurrency = 0;
        this.vatAmountCurrency = 0;
    }
}

export class SupplierInvoiceDTO extends InvoiceDTO implements ISupplierInvoiceDTO {
    attestGroupId: number;
    attestStateId: number;
    attestStateName: string;
    blockPayment: boolean;
    blockReason: string;
    blockReasonTextId: number;
    ediEntryId: number;
    hasImage: boolean;
    image: any;
    interimInvoice: boolean;
    multipleDebtRows: boolean;
    orderCustomerInvoiceId: number;
    orderCustomerName: string;
    orderNr: number;
    orderProjectId: number;
    paymentMethodId: number;
    prevInvoiceId: number;
    scanningImage: IGenericImageDTO;
    supplierInvoiceCostAllocationRows: SupplierInvoiceCostAllocationDTO[];
    supplierInvoiceFiles: FileUploadDTO[];
    supplierInvoiceOrderRows: ISupplierInvoiceOrderRowDTO[];
    supplierInvoiceProjectRows: ISupplierInvoiceProjectRowDTO[];
    supplierInvoiceRows: SupplierInvoiceRowDTO[];
    vatDeductionAccountId: number;
    vatDeductionPercent: number;
    vatDeductionType: TermGroup_VatDeductionType;
    hasOrderRows: boolean;
    hasProjectRows: boolean;

    // Extensions
    accountingRows: AccountingRowDTO[];
    supplierInvoiceAttestRows: AccountingRowDTO[];

    constructor() {
        super();

        this.type = SoeInvoiceType.SupplierInvoice;
        this.billingType = TermGroup_BillingType.Debit;
        this.vatType = TermGroup_InvoiceVatType.Merchandise;
        this.interimInvoice = false;
        this.blockPayment = false;
        this.totalAmountCurrency = 0;
        this.vatAmountCurrency = 0;
    }
}

export class SupplierInvoiceGridDTO implements ISupplierInvoiceGridDTO {
    ediMessageType: number;
    isOverdue: boolean;
    supplierInvoiceId: number;
    type: number;
    typeName: string;
    seqNr: string;
    invoiceNr: string;
    billingTypeId: number;
    billingTypeName: string;
    ocr: string;
    status: number;
    statusName: string;
    supplierId: number;
    supplierName: string;
    supplierNr: string;
    internalText: string;
    totalAmount: number;
    totalAmountText: string;
    totalAmountCurrency: number;
    totalAmountCurrencyText: string;
    totalAmountExVat: number;
    totalAmountExVatText: string;
    totalAmountExVatCurrency: number;
    totalAmountExVatCurrencyText: string;
    vATAmount: number;
    vATAmountCurrency: number;
    payAmount: number;
    payAmountText: string;
    payAmountCurrency: number;
    payAmountCurrencyText: string;
    paidAmount: number;
    paidAmountCurrency: number;
    vatType: number;
    vatRate: number;
    sysCurrencyId: number;
    currencyCode: string;
    currencyRate: number;
    invoiceDate: Date;
    dueDate: Date;
    payDate: Date;
    voucherDate: Date;
    attestStateId: number;
    attestStateName: string;
    currentAttestUserName: string;
    isAttestRejected: boolean;
    attestGroupId: number;
    attestGroupName: string;
    ownerActorId: number;
    fullyPaid: boolean;
    paymentStatuses: string;
    timeDiscountDate: Date;
    timeDiscountPercent: number;
    statusIcon: number;
    hasVoucher: boolean;
    multipleDebtRows: boolean;
    useClosedStyle: boolean;
    isSelectDisabled: boolean;
    guid: System.IGuid;
    ediEntryId: number;
    roundedInterpretation: number;
    hasPDF: boolean;
    ediType: number;
    scanningEntryId: number;
    operatorMessage: string;
    errorCode: number;
    invoiceStatus: number;
    sourceTypeName: string;
    created: Date;
    ediMessageTypeName: string;
    errorMessage: string;
    hasAttestComment: boolean;
    noOfCheckedPaymentRows: number;
    noOfPaymentRows: number;
    blockPayment: boolean;
    blockReason: string;
    projectAmount: number;
    projectInvoicedAmount: number;
    projectInvoicedSalesAmount: number;
    ediMessageProviderName: string;

    // Extensions
    blockIcon: string;
    pdfIcon: string;
    interpretationStateColor: string;
    interpretationStateTooltip: string;
    expandableDataIsLoaded: boolean;
    attestStateColor: string;
    attestStateMessage: string;
    infoIconValue: string;
    infoIconTooltip: string;
    infoIconMessage: string;
    showCreateInvoiceIcon: string;
    showCreateInvoiceTooltip: string;
    hasNoValidPaymentInfo: boolean;
    validPaymentInformations: any[];
}

export class SupplierInvoiceCostOverviewDTO implements ISupplierInvoiceCostOverviewDTO {
    attestGroupName: string;
    diffPercent: number;
    diffAmount: number;
    dueDate: Date;
    internalText: string;
    invoiceDate: Date;
    invoiceNr: string;
    orderAmount: number;
    orderIds: number[];
    orderNr: string;
    projectAmount: number;
    projectId: number;
    projectIds: number[];
    projectNr: string;
    seqNr: string;
    status: number;
    statusName: string;
    supplierId: number;
    supplierInvoiceId: number;
    supplierName: string;
    supplierNr: string;
    totalAmountCurrency: number;
    totalAmountExVat: number;
    vATAmountCurrency: number;

    // Extensions
    diffIcon: string;
    diffTooltip: string;
}

export class SupplierPaymentGridDTO implements ISupplierPaymentGridDTO {
    supplierPaymentId: number;
    supplierInvoiceId: number;
    invoiceSeqNr: string;
    paymentSeqNr: number;
    sequenceNumber: number;
    sequenceNumberRecordId: number;
    invoiceNr: string;
    billingTypeId: number;
    billingTypeName: string;
    status: number;
    statusName: string;
    supplierId: number;
    supplierName: string;
    supplierNr: string;
    totalAmount: number;
    totalAmountCurrency: number;
    totalAmountExVat: number;
    totalAmountExVatCurrency: number;
    vATAmount: number;
    vATAmountCurrency: number;
    payAmount: number;
    payAmountCurrency: number;
    paidAmount: number;
    paidAmountCurrency: number;
    vatRate: number;
    sysCurrencyId: number;
    currencyCode: string;
    currencyRate: number;
    invoiceDate: Date;
    dueDate: Date;
    payDate: Date;
    voucherDate: Date;
    attestStateId: number;
    attestStateName: string;
    currentAttestUserName: string;
    ownerActorId: number;
    fullyPaid: boolean;
    paymentStatuses: string;
    sysPaymentMethodId: number;
    sysPaymentTypeId: number;
    paymentMethodName: string;
    paymentRowId: number;
    paymentNr: string;
    paymentNrString: string;
    paymentAmount: number;
    paymentAmountCurrency: number;
    paymentAmountDiff: number;
    bankFee: number;
    timeDiscountDate: Date;
    timeDiscountPercent: number;
    blockPayment: boolean;
    supplierBlockPayment: boolean;
    statusIcon: number;
    multipleDebtRows: boolean;
    hasVoucher: boolean;
    isModified: boolean;
    guid: System.IGuid;
    hasAttestComment: boolean;
    blockReason: string;
    description: string;

    // Extensions
    interpretationStateColor: string;
    interpretationStateTooltip: string;
    expandableDataIsLoaded: boolean;
    attestStateIcon: string;
    attestStateMessage: string;
    infoIconValue: string;
    infoIconTooltip: string;
    infoIconMessage: string;
    showCreateInvoiceIcon: string;
    showCreateInvoiceTooltip: string;
    hasNoValidPaymentInfo: boolean;
    validPaymentInformations: any[];
}

export class CustomerInvoiceGridDTO implements ICustomerInvoiceGridDTO {
    actorCustomerId: number;
    actorCustomerName: string;
    actorCustomerNr: string;
    actorCustomerNrName: string;
    attestStateNames: string;
    attestStates: ICustomerInvoiceRowAttestStateViewDTO[];
    bankFee: number;
    billingAddress: string;
    billingAddressId: number;
    billingInvoicePrinted: boolean;
    billingTypeId: number;
    billingTypeName: string;
    categories: string;
    contactEComId: number;
    contactEComText: string;
    contractGroupName: string;
    contractYearlyValue: number;
    contractYearlyValueExVat: number;
    created: Date;
    currencyCode: string;
    currencyRate: number;
    customerCategories: string;
    customerGracePeriodDays: number;
    customerInvoiceId: number;
    customerPaymentId: number;
    customerPaymentRowId: number;
    defaultDim2AccountId: number;
    defaultDim2AccountName: string;
    defaultDim3AccountId: number;
    defaultDim3AccountName: string;
    defaultDim4AccountId: number;
    defaultDim4AccountName: string;
    defaultDim5AccountId: number;
    defaultDim5AccountName: string;
    defaultDim6AccountId: number;
    defaultDim6AccountName: string;
    defaultDimAccountNames: string;
    deliverDateText: string;
    deliveryAddress: string;
    deliveryAddressId: number;
    deliveryDate: Date;
    deliveryType: number;
    deliveryTypeName: string;
    dueDate: Date;
    einvoiceDistStatus: number;
    exportStatus: number;
    exportStatusName: string;
    fixedPriceOrder: boolean;
    fixedPriceOrderName: string;
    fullyPaid: boolean;
    guid: System.IGuid;
    hasHouseholdTaxDeduction: boolean;
    hasInterest: boolean;
    hasVoucher: boolean;
    householdTaxDeductionType: number;
    infoIcon: number;
    insecureDebt: boolean;
    internalText: string;
    invoiceDate: Date;
    invoiceDeliveryProvider: number;
    invoiceDeliveryProviderName: string;
    invoiceHeadText: string;
    invoiceLabel: string;
    invoiceNr: string;
    invoicePaymentServiceId: number;
    invoicePaymentServiceName: string;
    isCashSales: boolean;
    isCashSalesText: string;
    isOverdued: boolean;
    isSelectDisabled: boolean;
    isTotalAmountPaid: boolean;
    lastCreatedReminder: Date;
    mainUserName: string;
    multipleAssetRows: boolean;
    myReadyState: number;
    nextContractPeriod: string;
    nextInvoiceDate: Date;
    noOfPrintedReminders: number;
    noOfReminders: number;
    onlyPayment: boolean;
    orderNumbers: string;
    orderReadyStatePercent: number;
    orderReadyStateText: string;
    orderType: number;
    orderTypeName: string;
    originType: number;
    ownerActorId: number;
    paidAmount: number;
    paidAmountCurrency: number;
    paidAmountCurrencyText: string;
    paidAmountText: string;
    payAmount: number;
    payAmountCurrency: number;
    payAmountCurrencyText: string;
    payAmountText: string;
    payDate: Date;
    paymentAmount: number;
    paymentAmountCurrency: number;
    paymentAmountDiff: number;
    paymentNr: string;
    paymentSeqNr: number;
    priceListName: string;
    projectName: string;
    projectNr: string;
    referenceOur: string;
    referenceYour: string;
    registrationType: number;
    remainingAmount: number;
    remainingAmountExVat: number;
    remainingAmountExVatText: string;
    remainingAmountText: string;
    reminderContactEComId: number;
    reminderContactEComText: string;
    seqNr: number;
    shiftTypeColor: string;
    shiftTypeName: string;
    status: number;
    statusIcon: number;
    statusName: string;
    sysCurrencyId: number;
    totalAmount: number;
    totalAmountCurrency: number;
    totalAmountCurrencyText: string;
    totalAmountExVat: number;
    totalAmountExVatCurrency: number;
    totalAmountExVatCurrencyText: string;
    totalAmountExVatText: string;
    totalAmountText: string;
    useClosedStyle: boolean;
    users: string;
    vATAmount: number;
    vATAmountCurrency: number;
    vatRate: number;

    // Extensions
    expandableDataIsLoaded: boolean;
    statusIconValue: string;
    statusIconMessage: string;
    attestStateColor: string;
    useGradient: boolean;
    billingIconValue: string;
    billingIconMessage: string;
    showCreatePayment: boolean;
    myReadyStateIconText: string;
    myReadyStateIcon: string;
    orderReadyStateIcon: string;
}

export class EdiEntryDTO implements IEdiEntryDTO {
    ediEntryId: number;
    actorCompanyId: number;
    type: TermGroup_EDISourceType;
    status: TermGroup_EDIStatus;
    messageType: TermGroup_EdiMessageType;
    sysWholesellerId: number;
    wholesellerName: string;
    buyerId: string;
    buyerReference: string;
    vatRate: number;
    postalGiro: string;
    bankGiro: string;
    ocr: string;
    iban: string;
    billingType: TermGroup_BillingType;
    xml: string;
    pdf: number[];
    fileName: string;
    errorCode: number;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;
    scanningEntryArrivalId: number;
    scanningEntryInvoiceId: number;
    date: Date;
    invoiceDate: Date;
    dueDate: Date;
    sum: number;
    sumCurrency: number;
    sumVat: number;
    sumVatCurrency: number;
    currencyId: number;
    currencyRate: number;
    currencyDate: Date;
    orderId: number;
    orderStatus: TermGroup_EDIOrderStatus;
    orderNr: string;
    invoiceId: number;
    invoiceStatus: TermGroup_EDIInvoiceStatus;
    seqNr: number;
    invoiceNr: string;
    actorSupplierId: number;
    scanningEntryInvoice: ScanningEntryDTO;
    hasPDF: boolean;
    sellerOrderNr: string;

    constructor() {
    }
}

export class ScanningEntryDTO implements IScanningEntryDTO {
    scanningEntryId: number;
    actorCompanyId: number;
    batchId: string;
    companyId: string;
    type: number;
    messageType: TermGroup_ScanningMessageType;
    status: TermGroup_ScanningStatus;
    nrOfPages: number;
    nrOfInvoices: number;
    image: number[];
    xml: string;
    errorCode: number;
    operatorMessage: string;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;
    scanningEntryRow: ScanningEntryRowDTO[];

    constructor() {
    }

    public getScanningInterpretation(scanningEntryRowType: ScanningEntryRowType) {
        var interpretation = 0;
        var row = _.find(this.scanningEntryRow, { type: scanningEntryRowType });
        if (row)
            interpretation = Validators.validateScanningEntryRow(row.newText, row.validationError)
        else
            interpretation = TermGroup_ScanningInterpretation.ValueNotFound
        return interpretation;
    }
}

export class ScanningEntryRowDTO implements IScanningEntryRowDTO {
    type: ScanningEntryRowType;
    typeName: string;
    name: string;
    text: string;
    format: string;
    validationError: string;
    position: string;
    pageNumber: string;
    newText: string;
    created: Date;
    createdBy: string;
    modified: Date;
    modifiedBy: string;
    state: SoeEntityState;

    constructor() {
    }
}

export class IInterpretationValueDTO<T> {
    value: T;
    hasValue: boolean;
    confidenceLevel: TermGroup_ScanningInterpretation;
}

export class InterpretationValueDTO<T> {
    value: T;
    confidenceLevel: TermGroup_ScanningInterpretation;
    hasValue: boolean;

    constructor(
        value: IInterpretationValueDTO<T>,
    ) {
        this.value = value?.value;
        this.confidenceLevel = value?.confidenceLevel;
        this.hasValue = value?.hasValue;
    }
}
export class InterpretationValueDateDTO extends InterpretationValueDTO<Date> {
    constructor(value: IInterpretationValueDTO<Date>) {
        super(value);
        if (this.hasValue)
            this.value = new Date(this.value);
    }
}


export class IInvoiceInterpretationDTO {
    isCreditInvoice: InterpretationValueDTO<boolean | null>;
    supplierId: InterpretationValueDTO<number | null>;
    supplierName: InterpretationValueDTO<string>;
    invoiceNumber: InterpretationValueDTO<string>;
    invoiceDate: InterpretationValueDTO<Date | null>;
    dueDate: InterpretationValueDTO<Date | null>;
    currencyDate: InterpretationValueDTO<Date | null>;
    description: InterpretationValueDTO<string>;
    currencyRate: InterpretationValueDTO<number>;
    currencyCode: InterpretationValueDTO<string>;
    currencyId: InterpretationValueDTO<number | null>;
    buyerOrderNumber: InterpretationValueDTO<string>;
    buyerContactName: InterpretationValueDTO<string>;
    buyerReference: InterpretationValueDTO<string>;
    sellerContactName: InterpretationValueDTO<string>;
    amountExVat: InterpretationValueDTO<number | null>;
    amountIncVat: InterpretationValueDTO<number | null>;
    vatRatePercent: InterpretationValueDTO<number | null>;
    amountExVatCurrency: InterpretationValueDTO<number | null>;
    amountIncVatCurrency: InterpretationValueDTO<number | null>;
    vatAmount: InterpretationValueDTO<number | null>;
    vatAmountCurrency: InterpretationValueDTO<number | null>;
    paymentReferenceNumber: InterpretationValueDTO<string>;
    bankAccountPG: InterpretationValueDTO<string>;
    bankAccountBG: InterpretationValueDTO<string>;
    bankAccountIBAN: InterpretationValueDTO<string>;
    orgNumber: InterpretationValueDTO<string>;
    vatRegistrationNumber: InterpretationValueDTO<string>;
    deliveryCost: InterpretationValueDTO<number | null>;
    amountRounding: InterpretationValueDTO<number | null>;
    email: InterpretationValueDTO<string>;
    context: {
        ediEntryId?: number;
        scanningEntryId?: number;
    }
    metadata: {
        arrivalTime: Date;
        provider: string;
        rawResponse: string;
    }
}

export class InvoiceInterpretationDTO implements IInvoiceInterpretationDTO {
    isCreditInvoice: InterpretationValueDTO<boolean | null>;
    supplierId: InterpretationValueDTO<number | null>;
    supplierName: InterpretationValueDTO<string>;
    invoiceNumber: InterpretationValueDTO<string>;
    invoiceDate: InterpretationValueDTO<Date | null>;
    dueDate: InterpretationValueDTO<Date | null>;
    description: InterpretationValueDTO<string>;
    currencyCode: InterpretationValueDTO<string>;
    currencyId: InterpretationValueDTO<number | null>;
    buyerOrderNumber: InterpretationValueDTO<string>;
    buyerContactName: InterpretationValueDTO<string>;
    buyerReference: InterpretationValueDTO<string>;
    sellerContactName: InterpretationValueDTO<string>;
    amountExVat: InterpretationValueDTO<number | null>;
    amountIncVat: InterpretationValueDTO<number | null>;
    amountExVatCurrency: InterpretationValueDTO<number | null>;
    amountIncVatCurrency: InterpretationValueDTO<number | null>;
    vatRatePercent: InterpretationValueDTO<number | null>;
    vatAmount: InterpretationValueDTO<number | null>;
    vatAmountCurrency: InterpretationValueDTO<number | null>;
    paymentReferenceNumber: InterpretationValueDTO<string>;
    bankAccountPG: InterpretationValueDTO<string>;
    bankAccountBG: InterpretationValueDTO<string>;
    bankAccountIBAN: InterpretationValueDTO<string>;
    orgNumber: InterpretationValueDTO<string>;
    vatRegistrationNumber: InterpretationValueDTO<string>;
    deliveryCost: InterpretationValueDTO<number | null>;
    amountRounding: InterpretationValueDTO<number | null>;
    email: InterpretationValueDTO<string>;
    currencyDate: InterpretationValueDTO<Date>;
    currencyRate: InterpretationValueDTO<number>;
    context: { ediEntryId?: number; scanningEntryId?: number; };
    metadata: { arrivalTime: Date; provider: string; rawResponse: string; };

    constructor(interpretation: IInvoiceInterpretationDTO) {
        this.isCreditInvoice = new InterpretationValueDTO(interpretation.isCreditInvoice);
        this.supplierId = new InterpretationValueDTO(interpretation.supplierId);
        this.supplierName = new InterpretationValueDTO(interpretation.supplierName);
        this.invoiceNumber = new InterpretationValueDTO(interpretation.invoiceNumber);
        this.invoiceDate = new InterpretationValueDateDTO(interpretation.invoiceDate);
        this.dueDate = new InterpretationValueDateDTO(interpretation.dueDate);
        this.description = new InterpretationValueDTO(interpretation.description);
        this.currencyCode = new InterpretationValueDTO(interpretation.currencyCode);
        this.currencyId = new InterpretationValueDTO(interpretation.currencyId);
        this.currencyDate = new InterpretationValueDateDTO(interpretation.currencyDate);
        this.currencyRate = new InterpretationValueDTO(interpretation.currencyRate);
        this.buyerOrderNumber = new InterpretationValueDTO(interpretation.buyerOrderNumber);
        this.buyerContactName = new InterpretationValueDTO(interpretation.buyerContactName);
        this.buyerReference = new InterpretationValueDTO(interpretation.buyerReference);
        this.sellerContactName = new InterpretationValueDTO(interpretation.sellerContactName);
        this.amountExVat = new InterpretationValueDTO(interpretation.amountExVat);
        this.amountExVatCurrency = new InterpretationValueDTO(interpretation.amountExVatCurrency);
        this.amountIncVat = new InterpretationValueDTO(interpretation.amountIncVat);
        this.amountIncVatCurrency = new InterpretationValueDTO(interpretation.amountIncVatCurrency);
        this.vatAmount = new InterpretationValueDTO(interpretation.vatAmount);
        this.vatAmountCurrency = new InterpretationValueDTO(interpretation.vatAmountCurrency);
        this.paymentReferenceNumber = new InterpretationValueDTO(interpretation.paymentReferenceNumber);
        this.vatRatePercent = new InterpretationValueDTO(interpretation.vatRatePercent);
        this.bankAccountPG = new InterpretationValueDTO(interpretation.bankAccountPG);
        this.bankAccountBG = new InterpretationValueDTO(interpretation.bankAccountBG);
        this.bankAccountIBAN = new InterpretationValueDTO(interpretation.bankAccountIBAN);
        this.orgNumber = new InterpretationValueDTO(interpretation.orgNumber);
        this.vatRegistrationNumber = new InterpretationValueDTO(interpretation.vatRegistrationNumber);
        this.deliveryCost = new InterpretationValueDTO(interpretation.deliveryCost);
        this.amountRounding = new InterpretationValueDTO(interpretation.amountRounding);
        this.email = new InterpretationValueDTO(interpretation.email);
        this.context = interpretation.context;
        this.metadata = interpretation.metadata;
    }
}

export class TextBlockDTO implements ITextblockDTO {
    actorCompanyId: number;
    created: Date;
    createdBy: string;
    headline: string;
    modified: Date;
    modifiedBy: string;
    showInContract: boolean;
    showInInvoice: boolean;
    showInOffer: boolean;
    showInOrder: boolean;
    showInPurchase: boolean;
    state: SoeEntityState;
    type: number;
    isModified: boolean;
    text: string;
    textblockId: number;
}

export class UpdateEdiEntryDTO implements IUpdateEdiEntryDTO {
    ediEntryId: number;
    supplierId: number;
    attestGroupId: number;
    scanningEntryId: number;
    orderNr: string;
}

export class OrderDTO implements IOrderDTO {
    actorId: number;
    addAttachementsToEInvoice: boolean;
    addSupplierInvoicesToEInvoice: boolean;
    billingAddressId: number;
    billingAdressText: string;
    //billingInvoicePrinted: boolean;
    billingType: TermGroup_BillingType;
    //cashSale: boolean;
    categoryIds: number[];
    centRounding: number;
    checkConflictsOnSave: boolean;
    contactEComId: number;
    contactGLNId: number;
    contractGroupId: number;
    contractNr: string;
    created: Date;
    createdBy: string;
    currencyDate: Date;
    currencyId: number;
    currencyRate: number;
    customerBlockNote: string;
    customerEmail: string;
    customerInvoiceRows: ProductRowDTO[];
    customerName: string;
    customerPhoneNr: string;
    defaultDim1AccountId: number;
    defaultDim2AccountId: number;
    defaultDim3AccountId: number;
    defaultDim4AccountId: number;
    defaultDim5AccountId: number;
    defaultDim6AccountId: number;
    deliveryAddressId: number;
    deliveryConditionId: number;
    deliveryCustomerId: number;
    deliveryDate: Date;
    deliveryDateText: string;
    deliveryTypeId: number;
    dueDate: Date;
    estimatedTime: number;
    //externalDescription: string;
    fixedPriceOrder: boolean;
    forceSave: boolean;
    freightAmount: number;
    freightAmountCurrency: number;
    //freightAmountEntCurrency: number;
    //freightAmountLedgerCurrency: number;
    //hasHouseholdTaxDeduction: boolean;
    hasManuallyDeletedTimeProjectRows: boolean;
    includeExpenseInReport: number;
    includeOnInvoice: boolean;
    includeOnlyInvoicedTime: boolean;
    //insecureDebt: boolean;
    //internalDescription: string;
    invoiceDate: Date;
    invoiceDeliveryType: number;
    invoiceFee: number;
    invoiceFeeCurrency: number;
    //invoiceFeeEntCurrency: number;
    //invoiceFeeLedgerCurrency: number;
    invoiceHeadText: string;
    invoiceId: number;
    invoiceLabel: string;
    invoiceNr: string;
    invoicePaymentService: number;
    invoiceText: string;
    isMainInvoice: boolean;
    isTemplate: boolean;
    keepAsPlanned: boolean;
    mainInvoice: string;
    mainInvoiceId: number;
    mainInvoiceNr: string;
    //manuallyAdjustedAccounting: boolean;
    //marginalIncome: number;
    marginalIncomeCurrency: number;
    //marginalIncomeEntCurrency: number;
    //marginalIncomeLedgerCurrency: number;
    marginalIncomeRatio: number;
    modified: Date;
    modifiedBy: string;
    //multipleAssetRows: boolean;
    //ocr: string;
    nbrOfChecklists: number;
    note: string;
    nextContractPeriodDate: Date;
    nextContractPeriodValue: number;
    nextContractPeriodYear: number;
    orderDate: Date;
    orderInvoiceTemplateId: number;
    orderType: TermGroup_OrderType;
    //originateFrom: number;
    originDescription: string;
    originStatus: SoeOriginStatus;
    originStatusName: string;
    originUsers: IOriginUserSmallDTO[];
    //paidAmount: number;
    //paidAmountCurrency: number;
    //paidAmountEntCurrency: number;
    //paidAmountLedgerCurrency: number;
    payingCustomerId: number;
    paymentConditionId: number;
    plannedStartDate: Date;
    plannedStopDate: Date;
    prevInvoiceId: number;
    priceListTypeId: number;
    printTimeReport: boolean;
    priority: number;
    projectId: number;
    projectIsActive: boolean;
    projectNr: string;
    referenceOur: string;
    referenceYour: string;
    remainingAmount: number;
    remainingAmountExVat: number;
    //remainingAmountVat: number;
    remainingTime: number;
    seqNr: number;
    shiftTypeId: number;
    showNote: boolean;
    //state: SoeEntityState;
    statusIcon: SoeStatusIcon;
    sumAmount: number;
    sumAmountCurrency: number;
    //sumAmountEntCurrency: number;
    //sumAmountLedgerCurrency: number;
    //sysPaymentTypeId: number;
    sysWholeSellerId: number;
    //timeDiscountDate: Date;
    //timeDiscountPercent: number;
    totalAmount: number;
    totalAmountCurrency: number;
    transferAttachments: boolean;
    //totalAmountEntCurrency: number;
    //totalAmountLedgerCurrency: number;
    vatAmount: number;
    vatAmountCurrency: number;
    //vatAmountEntCurrency: number;
    //vatAmountLedgerCurrency: number;
    //vatCodeId: number;
    vatType: TermGroup_InvoiceVatType;
    //voucheHead2Id: number;
    //voucheHeadId: number;
    voucherDate: Date;
    voucherSeriesId: number;
    workingDescription: string;
    orderReference: string;
    triangulationSales: boolean;
    ediTransferMode: TermGroup_OrderEdiTransferMode;

    // Extensions
    get estimatedTimeFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.estimatedTime);
    }
    set estimatedTimeFormatted(time: string) {
        this.remainingTime = this.remainingTime || 0;
        this.estimatedTime = this.estimatedTime || 0;

        let span = CalendarUtility.parseTimeSpan(time);
        let newValue = CalendarUtility.timeSpanToMinutes(span);

        let diff = newValue - this.estimatedTime;
        this.remainingTime = this.remainingTime + diff > 0 ? this.remainingTime + diff : this.remainingTime;
        this.estimatedTime = newValue;
    }

    get remainingTimeFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.remainingTime);
    }
    set remainingTimeFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.remainingTime = CalendarUtility.timeSpanToMinutes(span);
    }

    public static getPropertiesToSkipOnSave(): string[] {
        return ['created', 'createdBy', 'customerInvoiceRows', 'modified', 'modifiedBy', 'originStatusName', 'projectNr', 'estimatedTimeFormatted', 'remainingTimeFormatted'];
    }
}

export class BillingInvoiceDTO implements IBillingInvoiceDTO {
    actorId: number;
    addAttachementsToEInvoice: boolean;
    addSupplierInvoicesToEInvoice: boolean;
    billingAddressId: number;
    billingAdressText: string;
    billingInvoicePrinted: boolean;
    billingType: TermGroup_BillingType;
    cashSale: boolean;
    categoryIds: number[];
    centRounding: number;
    checkConflictsOnSave: boolean;
    contactEComId: number;
    contractNr: string;
    created: Date;
    createdBy: string;
    currencyDate: Date;
    currencyId: number;
    currencyRate: number;
    customerBlockNote: string;
    customerEmail: string;
    customerInvoiceRows: ProductRowDTO[];
    customerName: string;
    customerPhoneNr: string;
    defaultDim1AccountId: number;
    defaultDim2AccountId: number;
    defaultDim3AccountId: number;
    defaultDim4AccountId: number;
    defaultDim5AccountId: number;
    defaultDim6AccountId: number;
    deliveryAddressId: number;
    deliveryConditionId: number;
    deliveryCustomerId: number;
    deliveryDate: Date;
    deliveryDateText: string;
    deliveryTypeId: number;
    dueDate: Date;
    estimatedTime: number;
    //externalDescription: string;
    fixedPriceOrder: boolean;
    forceSave: boolean;
    freightAmount: number;
    freightAmountCurrency: number;
    //freightAmountEntCurrency: number;
    //freightAmountLedgerCurrency: number;
    //hasHouseholdTaxDeduction: boolean;
    hasManuallyDeletedTimeProjectRows: boolean;
    hasOrder: boolean;
    includeOnInvoice: boolean;
    includeOnlyInvoicedTime: boolean;
    insecureDebt: boolean;
    //internalDescription: string;
    invoiceDate: Date;
    invoiceDeliveryProvider: number;
    invoiceDeliveryType: number;
    invoiceFee: number;
    invoiceFeeCurrency: number;
    //invoiceFeeEntCurrency: number;
    //invoiceFeeLedgerCurrency: number;
    invoiceHeadText: string;
    invoiceId: number;
    invoiceLabel: string;
    invoiceNr: string;
    invoicePaymentService: number;
    invoiceText: string;
    isTemplate: boolean;
    manuallyAdjustedAccounting: boolean;
    //marginalIncome: number;
    marginalIncomeCurrency: number;
    //marginalIncomeEntCurrency: number;
    //marginalIncomeLedgerCurrency: number;
    marginalIncomeRatio: number;
    modified: Date;
    modifiedBy: string;
    //multipleAssetRows: boolean;
    //ocr: string;
    nbrOfChecklists: number;
    note: string;
    orderDate: Date;
    orderNumbers: string;
    orderType: TermGroup_OrderType;
    //originateFrom: number;
    originDescription: string;
    originStatus: SoeOriginStatus;
    originStatusName: string;
    originUsers: IOriginUserSmallDTO[];
    paidAmount: number;
    paidAmountCurrency: number;
    //paidAmountEntCurrency: number;
    //paidAmountLedgerCurrency: number;
    //payingCustomerId: number;
    paymentConditionId: number;
    plannedStartDate: Date;
    plannedStopDate: Date;
    prevInvoiceId: number;
    priceListTypeId: number;
    printTimeReport: boolean;
    priority: number;
    projectId: number;
    projectNr: string;
    referenceOur: string;
    referenceYour: string;
    remainingAmount: number;
    remainingAmountExVat: number;
    //remainingAmountVat: number;
    remainingTime: number;
    seqNr: number;
    shiftTypeId: number;
    //state: SoeEntityState;
    statusIcon: SoeStatusIcon;
    sumAmount: number;
    sumAmountCurrency: number;
    //sumAmountEntCurrency: number;
    //sumAmountLedgerCurrency: number;
    //sysPaymentTypeId: number;
    sysWholeSellerId: number;
    //timeDiscountDate: Date;
    //timeDiscountPercent: number;
    totalAmount: number;
    totalAmountCurrency: number;
    transferedFromOffer: boolean;
    transferedFromOrder: boolean;
    transferedFromOriginType: SoeOriginType;
    triangulationSales: boolean;
    //totalAmountEntCurrency: number;
    //totalAmountLedgerCurrency: number;
    vatAmount: number;
    vatAmountCurrency: number;
    //vatAmountEntCurrency: number;
    //vatAmountLedgerCurrency: number;
    //vatCodeId: number;
    vatType: TermGroup_InvoiceVatType;
    voucheHead2Id: number;
    //voucheHeadId: number;
    voucherDate: Date;
    voucherSeriesId: number;
    voucherSeriesTypeId: number;
    workingDescription: string;
    orderReference: string;
    contactGLNId: number;
    // Extensions
    get estimatedTimeFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.estimatedTime);
    }
    set estimatedTimeFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.estimatedTime = CalendarUtility.timeSpanToMinutes(span);
    }

    get remainingTimeFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.remainingTime);
    }
    set remainingTimeFormatted(time: string) {
        var span = CalendarUtility.parseTimeSpan(time);
        this.remainingTime = CalendarUtility.timeSpanToMinutes(span);
    }

    public static getPropertiesToSkipOnSave(): string[] {
        return ['created', 'createdBy', 'customerInvoiceRows', 'modified', 'modifiedBy', 'originStatusName', 'projectNr', 'estimatedTimeFormatted', 'remainingTimeFormatted'];
    }
}

export class ProductRowDTO implements IProductRowDTO {
    amount: number;
    amountCurrency: number;
    attestStateId: number;
    created: Date;
    createdBy: string;
    customerInvoiceInterestId: number;
    customerInvoiceReminderId: number;
    customerInvoiceRowId: number;
    date: Date;
    dateTo: Date;
    deliveryDateText: string;
    discountAmount: number;
    discountAmountCurrency: number;
    discountPercent: number;
    discountType: number;
    discountValue: number;
    discount2Value: number;
    discount2Type: number;
    discount2Amount: number;
    discount2Percent: number;
    discount2AmountCurrency: number;
    ediEntryId: number;
    householdAmount: number;
    householdAmountCurrency: number;
    householdApartmentNbr: string;
    householdCooperativeOrgNbr: string;
    householdDeductionType: number;
    householdName: string;
    householdProperty: string;
    householdSocialSecNbr: string;
    householdTaxDeductionType: TermGroup_HouseHoldTaxDeductionType;
    householdTypeIsRUT: boolean;
    intrastatCodeId: number;
    intrastatTransactionId: number;
    invoiceQuantity: number;
    isCentRoundingRow: boolean;
    isClearingProduct: boolean;
    isFixedPriceProduct: boolean;
    isContractProduct: boolean;
    isExpenseRow: boolean;
    isFreightAmountRow: boolean;
    isInterestRow: boolean;
    isInvoiceFeeRow: boolean;
    isLiftProduct: boolean;
    isReminderRow: boolean;
    isStockRow: boolean;
    isTimeBillingRow: boolean;
    isTimeProjectRow: boolean;
    marginalIncome: number;
    marginalIncomeCurrency: number;
    marginalIncomeRatio: number;
    mergeToId: number;
    modified: Date;
    modifiedBy: string;
    parentRowId: number;
    prevCustomerInvoiceRowId: number;
    previouslyInvoicedQuantity: number;
    productId: number;
    productUnitId: number;
    purchasePrice: number;
    //purchasePriceCurrency: number;
    //quantity: number;
    rowNr: number;
    state: SoeEntityState;
    stockCode: string;
    stockId: number;
    sumAmount: number;
    sumAmountCurrency: number;
    sysCountryId: number;
    sysWholesellerName: string;
    tempRowId: number;
    text: string;
    timeManuallyChanged: boolean;
    type: SoeInvoiceRowType;
    vatAccountId: number;
    vatAmount: number;
    vatAmountCurrency: number;
    vatCodeId: number;
    vatRate: number;

    // Extensions
    amountFormula: string;
    attestStateColor: string;
    attestStateName: string;
    currencyCode: string;
    discountTypeText: string;
    discount2TypeText: string;
    ediTextValue: string;
    hasMultipleSalesRows: boolean;
    householdDeductionTypeText: string;
    isHouseholdTextRow: boolean;
    isModified: boolean;
    isReadOnly: boolean;
    isSupplementChargeProduct: boolean;
    marginalIncomeLimit: number;
    productNr: string;
    productName: string;
    productUnitCode: string;
    rowTypeIcon: string;
    supplementCharge: number;
    supplementChargePercent: number;
    supplierInvoiceId: number;
    vatAccountEnabled: boolean;
    vatAccountNr: string;
    vatAccountName: string;
    vatCodeCode: string;
    splitAccountingRows: SplitAccountingRowDTO[];
    stocksForProduct: SmallGenericType[];
    purchasePriceSum: number;
    sumTotalAmountCurrency: number;
    purchaseStatus: string;
    purchaseNr: string;
    purchaseId: number;
    grossMarginCalculationType: TermGroup_GrossMarginCalculationType;
    _quantity: number;
    public get quantity(): number {
        return this._quantity;
    }
    public set quantity(value: number) {
        this._quantity = value;
        this.purchasePriceSum = this._quantity && this._purchasePriceCurrency ? this._quantity * this._purchasePriceCurrency : 0;
    }

    _purchasePriceCurrency: number;
    public get purchasePriceCurrency(): number {
        return this._purchasePriceCurrency;
    }
    public set purchasePriceCurrency(value: number) {
        this._purchasePriceCurrency = value;
        this.purchasePriceSum = this._quantity && this._purchasePriceCurrency ? this._quantity * this._purchasePriceCurrency : 0;
    }

    public get isProductRow(): boolean {
        return this.type === SoeInvoiceRowType.ProductRow;
    }

    public get isTextRow(): boolean {
        return this.type === SoeInvoiceRowType.TextRow;
    }

    public get isSubTotalRow(): boolean {
        return this.type === SoeInvoiceRowType.SubTotalRow;
    }

    public get isPageBreakRow(): boolean {
        return this.type === SoeInvoiceRowType.PageBreakRow;
    }

    public setCalculationTypeFlag(value: TermGroup_InvoiceProductCalculationType) {
        switch (value) {
            case TermGroup_InvoiceProductCalculationType.Regular:
                break;
            case TermGroup_InvoiceProductCalculationType.Lift:
                this.isLiftProduct = true;
                break;
            case TermGroup_InvoiceProductCalculationType.FixedPrice:
                this.isFixedPriceProduct = true;
                break;
            //case TermGroup_InvoiceProductCalculationType.Clearing:
            //    this.isClearingProduct = true;
            //    break;
            //case TermGroup_InvoiceProductCalculationType.Contract:
            //    this.isContractProduct = true;
            //    break;
            //case TermGroup_InvoiceProductCalculationType.SupplementCharge:
            //    this.isSupplementChargeProduct = true;
            //    break;
        }
    }

    public get isHouseholdRow(): boolean {
        return !!this.householdAmountCurrency;
    }

    public static getNextRowNr(rows: ProductRowDTO[]) {
        var rowNr = 0;
        var maxRow = _.maxBy(rows, 'rowNr');
        if (maxRow)
            rowNr = maxRow.rowNr;

        return rowNr + 1;
    }

    public static getPropertiesToSkipOnSave(): string[] {
        return ['amountFormula', 'created', 'createdBy', 'discountValue', 'modified', 'modifiedBy', 'productName', 'productNr', 'productUnitCode',
            'stockCode', 'vatAccountName', 'vatAccountNr', 'vatCodeCode', 'amountFormula',
            'attestStateColor', 'attestStateName', 'currencyCode', 'discountTypeText', 'ediTextValue', 'householdDeductionTypeText',
            'isModified', 'isReadOnly', 'vatAccountEnabled', 'supplementCharge', 'supplementChargePercent', 'rowtypeicon', 'stocksForProduct'];
    }
}

export class CustomerInvoiceAccountRowDTO implements ICustomerInvoiceAccountRowDTO {
    amount: number;
    amountCurrency: number;
    amountEntCurrency: number;
    amountLedgerCurrency: number;
    balance: number;
    creditAmount: number;
    creditAmountCurrency: number;
    creditAmountEntCurrency: number;
    creditAmountLedgerCurrency: number;
    debitAmount: number;
    debitAmountCurrency: number;
    debitAmountEntCurrency: number;
    debitAmountLedgerCurrency: number;
    dim1Disabled: boolean;
    dim1Id: number;
    dim1Mandatory: boolean;
    dim1ManuallyChanged: boolean;
    dim1Name: string;
    dim1Nr: string;
    dim1Stop: boolean;
    dim2Disabled: boolean;
    dim2Id: number;
    dim2Mandatory: boolean;
    dim2ManuallyChanged: boolean;
    dim2Name: string;
    dim2Nr: string;
    dim2Stop: boolean;
    dim3Disabled: boolean;
    dim3Id: number;
    dim3Mandatory: boolean;
    dim3ManuallyChanged: boolean;
    dim3Name: string;
    dim3Nr: string;
    dim3Stop: boolean;
    dim4Disabled: boolean;
    dim4Id: number;
    dim4Mandatory: boolean;
    dim4ManuallyChanged: boolean;
    dim4Name: string;
    dim4Nr: string;
    dim4Stop: boolean;
    dim5Disabled: boolean;
    dim5Id: number;
    dim5Mandatory: boolean;
    dim5ManuallyChanged: boolean;
    dim5Name: string;
    dim5Nr: string;
    dim5Stop: boolean;
    dim6Disabled: boolean;
    dim6Id: number;
    dim6Mandatory: boolean;
    dim6ManuallyChanged: boolean;
    dim6Name: string;
    dim6Nr: string;
    dim6Stop: boolean;
    invoiceAccountRowId: number;
    invoiceRowId: number;
    isContractorVatRow: boolean;
    isCreditRow: boolean;
    isDebitRow: boolean;
    isDeleted: boolean;
    isModified: boolean;
    isVatRow: boolean;
    parentRowId: number;
    quantity: number;
    rowNr: number;
    splitPercent: number;
    splitType: number;
    splitValue: number;
    state: SoeEntityState;
    tempInvoiceRowId: number;
    tempRowId: number;
    text: string;
    type: AccountingRowType;
}

export class OriginUserDTO implements IOriginUserDTO {
    created: Date;
    createdBy: string;
    loginName: string;
    main: boolean;
    modified: Date;
    modifiedBy: string;
    name: string;
    originId: number;
    originUserId: number;
    readyDate: Date;
    roleId: number;
    state: SoeEntityState;
    userId: number;
}

export class EdiEntryViewDTO implements IEdiEntryViewDTO {
    actorCompanyId: number;
    billingType: TermGroup_BillingType;
    billingTypeName: string;
    buyerId: string;
    buyerReference: string;
    created: Date;
    currencyCode: string;
    currencyId: number;
    currencyRate: number;
    customerId: number;
    customerName: string;
    customerNr: string;
    date: Date;
    dueDate: Date;
    ediEntryId: number;
    ediMessageType: TermGroup_EdiMessageType;
    ediMessageTypeName: string;
    errorCode: number;
    errorMessage: string;
    hasPdf: boolean;
    importSource: EdiImportSource;
    invoiceDate: Date;
    invoiceId: number;
    invoiceNr: string;
    invoiceStatus: TermGroup_EDIInvoiceStatus;
    invoiceStatusName: string;
    isModified: boolean;
    isSelectDisabled: boolean;
    isSelected: boolean;
    isVisible: boolean;
    langId: number;
    nrOfInvoices: number;
    nrOfPages: number;
    operatorMessage: string;
    orderId: number;
    orderNr: string;
    orderStatus: TermGroup_EDIOrderStatus;
    orderStatusName: string;
    roundedInterpretation: number;
    scanningEntryId: number;
    scanningMessageType: TermGroup_ScanningMessageType;
    scanningMessageTypeName: string;
    scanningStatus: TermGroup_ScanningStatus;
    sellerOrderNr: string;
    seqNr: number;
    sourceTypeName: string;
    state: SoeEntityState;
    status: TermGroup_EDIStatus;
    statusName: string;
    sum: number;
    sumCurrency: number;
    sumVat: number;
    sumVatCurrency: number;
    supplierAttestGroupId: number;
    supplierAttestGroupName: string;
    supplierId: number;
    supplierName: string;
    supplierNr: string;
    sysCurrencyId: number;
    type: TermGroup_EDISourceType;
    wholesellerId: number;
    wholesellerName: string;

    //Extensions
    supplierNrName: string;
}

export class MarkupDTO implements IMarkupDTO {
    actorCompanyId: number;
    actorCustomerId: number;
    categoryId: number;
    categoryName: string;
    code: string;
    created: Date;
    createdBy: string;
    customerName: string;
    discountPercent: number;
    markupId: number;
    markupPercent: number;
    modified: Date;
    modifiedBy: string;
    productIdFilter: string;
    state: SoeEntityState;
    sysWholesellerId: number;
    wholesellerDiscountPercent: number;
    wholesellerName: string;
}


export class PriceBasedMarkupDTO implements IPriceBasedMarkupDTO {
    created: Date;
    createdBy: string;
    markupPercent: number;
    maxPrice: number;
    minPrice: number;
    modified: Date;
    modifiedBy: string;
    priceBasedMarkupId: number;
    priceListName: string;
    priceListTypeId: number;
    state: SoeEntityState;
}

export class SupplierInvoiceCostAllocationDTO implements ISupplierInvoiceCostAllocationDTO {

    attestStateColor: string;
    attestStateId: number;
    attestStateName: string;
    customerInvoiceNumberName: string;
    customerInvoiceRowId: number;
    chargeCostToProject: boolean;
    employeeDescription: string;
    employeeId: number;
    employeeName: string;
    employeeNr: string;
    includeSupplierInvoiceImage: boolean;
    isConnectToProjectRow: boolean;
    isReadOnly: boolean;
    isTransferToOrderRow: boolean;
    orderAmount: number;
    orderAmountCurrency: number;
    orderId: number;
    orderNr: string;
    productId: number;
    productName: string;
    productNr: string;
    projectAmount: number;
    projectAmountCurrency: number;
    projectId: number;
    projectName: string;
    projectNr: string;
    rowAmount: number;
    rowAmountCurrency: number;
    state: SoeEntityState;
    supplementCharge: number;
    supplierInvoiceId: number;
    timeCodeCode: string;
    timeCodeDescription: string;
    timeCodeId: number;
    timeCodeName: string;
    timeCodeTransactionId: number;
    timeInvoiceTransactionId: number;

    // Extensions
    projectLock: boolean;
    orderLock: boolean;
    isModified: boolean;
    rowTypeIcon: string;
    rowTypeTooltip: string;
}
