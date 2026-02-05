import {
  ProjectCentralStatusRowType,
  ProjectCentralHeaderGroupType,
  SoeOriginType,
  ProjectCentralBudgetRowType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IProjectCentralStatusDTO } from '@shared/models/generated-interfaces/ProjectCentralDTOs';
import { IProjectProductRowDTO } from '@shared/models/generated-interfaces/ProjectProductRowDTO';
import { ISupplierInvoiceGridDTO } from '@shared/models/generated-interfaces/SupplierInvoiceDTOs';

export class ProjectCentralSelectionDTO {
  dateFrom!: Date;
  dateTo!: Date;
  includeSubProjects!: boolean;
  showDetailedInformation!: boolean;
}

export class ProjectCentralStatusDTO implements IProjectCentralStatusDTO {
  associatedId!: number;
  employeeId!: number;
  description!: string;
  name!: string;
  typeName!: string;
  costTypeName!: string;
  groupRowTypeName!: string;
  employeeName!: string;
  modified!: string;
  modifiedBy!: string;
  budget!: number;
  budgetTime!: number;
  value!: number;
  value2!: number;
  diff!: number;
  diff2!: number;
  hasInfo!: boolean;
  info!: string;
  actorName!: string;
  isEditable!: boolean;
  isVisible!: boolean;
  date?: Date | undefined;
  rowType!: ProjectCentralStatusRowType;
  groupRowType!: ProjectCentralHeaderGroupType;
  originType!: SoeOriginType;
  type!: ProjectCentralBudgetRowType;
  budgetTimeFormatted?: string;
  valueTimeFormatted?: string;
  diffTimeFormatted?: string;
}

export class ProjectCentralSummaryDTO {
  projectId?: number;
  customerId?: number;
  includeChildProjects: boolean = false;
  showDetailedInformation: boolean = false;
  orders: number[] = [];
  customerInvoices: number[] = [];
  supplierInvoices: number[] = [];
  projectCentralRows: ProjectCentralStatusDTO[] = [];
  fromDate?: Date;
  toDate?: Date;

  constructor(
    projectId?: number,
    customerId?: number,
    includeChildProjects: boolean = false,
    showDetailedInformation: boolean = false,
    orders: number[] = [],
    customerInvoices: number[] = [],
    supplierInvoices: number[] = [],
    projectCentralRows: ProjectCentralStatusDTO[] = [],
    fromDate?: Date,
    toDate?: Date
  ) {
    this.projectId = projectId;
    this.customerId = customerId;
    this.includeChildProjects = includeChildProjects;
    this.showDetailedInformation = showDetailedInformation;
    this.orders = orders;
    this.customerInvoices = customerInvoices;
    this.supplierInvoices = supplierInvoices;
    this.projectCentralRows = projectCentralRows;
    this.fromDate = fromDate;
    this.toDate = toDate;
  }
}

export class SupplierInvoiceGridDTO implements ISupplierInvoiceGridDTO {
  supplierInvoiceId!: number;
  type!: number;
  typeName!: string;
  seqNr!: string;
  invoiceNr!: string;
  billingTypeId!: number;
  billingTypeName!: string;
  status!: number;
  statusName!: string;
  supplierId!: number;
  supplierName!: string;
  supplierNr!: string;
  internalText!: string;
  totalAmount!: number;
  totalAmountText!: string;
  totalAmountCurrency!: number;
  totalAmountCurrencyText!: string;
  totalAmountExVat!: number;
  totalAmountExVatText!: string;
  totalAmountExVatCurrency!: number;
  totalAmountExVatCurrencyText!: string;
  vatAmount!: number;
  vatAmountCurrency!: number;
  payAmount!: number;
  payAmountText!: string;
  payAmountCurrency!: number;
  payAmountCurrencyText!: string;
  paidAmount!: number;
  paidAmountCurrency!: number;
  vatType!: number;
  vatRate!: number;
  sysCurrencyId!: number;
  currencyCode!: string;
  currencyRate!: number;
  invoiceDate?: Date | undefined;
  dueDate?: Date | undefined;
  payDate?: Date | undefined;
  voucherDate?: Date | undefined;
  attestStateId?: number | undefined;
  attestStateName!: string;
  currentAttestUserName!: string;
  attestGroupId!: number;
  attestGroupName!: string;
  isAttestRejected!: boolean;
  ownerActorId!: number;
  fullyPaid!: boolean;
  paymentStatuses!: string;
  timeDiscountDate?: Date | undefined;
  timeDiscountPercent?: number | undefined;
  statusIcon!: number;
  multipleDebtRows!: boolean;
  hasVoucher!: boolean;
  ocr!: string;
  hasAttestComment!: boolean;
  noOfPaymentRows!: number;
  noOfCheckedPaymentRows!: number;
  blockPayment!: boolean;
  blockReason!: string;
  useClosedStyle!: boolean;
  isSelectDisabled!: boolean;
  guid!: string;
  isOverdue!: boolean;
  isAboutToDue!: boolean;
  ediEntryId!: number;
  roundedInterpretation!: number;
  hasPDF!: boolean;
  ediType!: number;
  scanningEntryId!: number;
  operatorMessage!: string;
  errorCode!: number;
  invoiceStatus!: number;
  sourceTypeName!: string;
  created?: Date | undefined;
  ediMessageType!: number;
  ediMessageTypeName!: string;
  ediMessageProviderName!: string;
  errorMessage!: string;
  projectAmount!: number;
  projectInvoicedAmount!: number;
  projectInvoicedSalesAmount!: number;
  isClosed!: boolean;

  //Extensions
  blockIcon!: string;
  pdfIcon!: string;
  interpretationStateColor!: string;
  interpretationStateTooltip!: string;
  expandableDataIsLoaded!: boolean;
  attestStateColor!: string;
  attestStateMessage!: string;
  infoIconValue!: string;
  infoIconTooltip!: string;
  infoIconMessage!: string;
  showCreateInvoiceIcon!: string;
  showCreateInvoiceTooltip!: string;
  hasNoValidPaymentInfo!: boolean;
  validPaymentInformations!: any[];
  paymentStatusColor?: string;
  paymentStatusTooltip?: string;

  static toSupplierInvoiceGridDTO(
    dto: ISupplierInvoiceGridDTO
  ): SupplierInvoiceGridDTO {
    const result = new SupplierInvoiceGridDTO();
    Object.assign(result, dto);
    return result;
  }
}

export interface IProjectProductGridRow extends IProjectProductRowDTO {
  invoiceNr: string;
  customerInvoiceId: number;
  customerInvoiceNumber?: string;
  associatedId?: number;
  orderNumber?: string;
  orderDate?: Date;

  // Extensions
  rowTypeIcon?: string;
}
