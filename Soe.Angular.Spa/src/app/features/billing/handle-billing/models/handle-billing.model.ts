import { ISearchCustomerInvoiceRowModel } from '@shared/models/generated-interfaces/BillingModels';
import {
  SoeInvoiceRowType,
  SoeProductRowType,
  TermGroup_InvoiceProductCalculationType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IHandleBillingRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class HandleBillingRowDTO implements IHandleBillingRowDTO {
  customerInvoiceRowId: number;
  rowNr: number;
  type: SoeInvoiceRowType;
  quantity?: number;
  invoiceQuantity?: number;
  previouslyInvoicedQuantity?: number;
  discountType: number;
  text: string;
  description: string;
  status: number;
  created?: Date;
  ediEntryId?: number;
  ediTextValue: string;
  vatAmount: number;
  vatAmountCurrency: number;
  amount: number;
  amountCurrency: number;
  sumAmount: number;
  sumAmountCurrency: number;
  discountPercent: number;
  discountAmount: number;
  discountAmountCurrency: number;
  purchasePrice: number;
  purchasePriceCurrency: number;
  marginalIncome: number;
  marginalIncomeCurrency: number;
  marginalIncomeRatio: number;
  marginalIncomeLimit: number;
  isTimeProjectRow: boolean;
  timeManuallyChanged: boolean;
  householdDeductionType?: number;
  date?: Date;
  isStockRow: boolean;
  productRowType: SoeProductRowType;
  invoiceId: number;
  invoiceNr: string;
  currencyId: number;
  currencyCode: string;
  actorCustomerId: number;
  customer: string;
  projectId: number;
  projectNr: string;
  project: string;
  attestStateId?: number;
  attestStateName: string;
  attestStateColor: string;
  productId?: number;
  productNr: string;
  productName: string;
  productCalculationType: TermGroup_InvoiceProductCalculationType;
  productUnitId?: number;
  productUnitCode: string;
  validForInvoice: boolean;

  // Extensions
  discountTypeText!: string;
  discountValue!: number;
  rowTypeIcon!: string;

  constructor() {
    this.customerInvoiceRowId = 0;
    this.rowNr = 0;
    this.type = SoeInvoiceRowType.Unknown;
    this.discountType = 0;
    this.text = '';
    this.description = '';
    this.status = 0;
    this.ediTextValue = '';
    this.vatAmount = 0;
    this.vatAmountCurrency = 0;
    this.amount = 0;
    this.amountCurrency = 0;
    this.sumAmount = 0;
    this.sumAmountCurrency = 0;
    this.discountPercent = 0;
    this.discountAmount = 0;
    this.discountAmountCurrency = 0;
    this.purchasePrice = 0;
    this.purchasePriceCurrency = 0;
    this.marginalIncome = 0;
    this.marginalIncomeCurrency = 0;
    this.marginalIncomeRatio = 0;
    this.marginalIncomeLimit = 0;
    this.isTimeProjectRow = false;
    this.timeManuallyChanged = false;
    this.isStockRow = false;
    this.productRowType = SoeProductRowType.None;
    this.invoiceId = 0;
    this.invoiceNr = '';
    this.currencyId = 0;
    this.currencyCode = '';
    this.actorCustomerId = 0;
    this.customer = '';
    this.projectId = 0;
    this.projectNr = '';
    this.project = '';
    this.attestStateName = '';
    this.attestStateColor = '';
    this.productNr = '';
    this.productName = '';
    this.productCalculationType =
      TermGroup_InvoiceProductCalculationType.Regular;
    this.productUnitCode = '';
    this.validForInvoice = false;
  }
}

export class SearchCustomerInvoiceRowModel
  implements ISearchCustomerInvoiceRowModel
{
  projects: number[];
  orders: number[];
  customers: number[];
  orderTypes: number[];
  orderContractTypes: number[];
  from: Date;
  to: Date;
  onlyValid: boolean;
  onlyMine: boolean;
  customerInvoiceRowId?: number;

  constructor() {
    this.projects = [];
    this.orders = [];
    this.customers = [];
    this.orderTypes = [];
    this.orderContractTypes = [];
    this.from = new Date();
    this.to = new Date();
    this.onlyValid = false;
    this.onlyMine = false;
    this.customerInvoiceRowId = 0;
  }
}
