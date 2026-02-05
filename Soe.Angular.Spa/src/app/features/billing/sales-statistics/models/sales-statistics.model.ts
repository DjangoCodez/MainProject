import { DateUtil } from '@shared/util/date-util';
import { IGeneralProductStatisticsModel } from '@shared/models/generated-interfaces/CoreModels';
import { ICustomerStatisticsDTO } from '@shared/models/generated-interfaces/CustomerStatisticsDTO';
import { SoeOriginType } from '@shared/models/generated-interfaces/Enumerations';

export class CustomerStatisticsDTO implements ICustomerStatisticsDTO {
  date?: Date;
  originType: SoeOriginType;
  originUsers: string;
  mainUserName: string;
  orderType: number;
  orderTypeName: string;
  invoiceNr: string;
  orderNr: string;
  customerName: string;
  customerStreetAddress: string;
  customerPostalAddress: string;
  customerPostalCode: string;
  customerCountry: string;
  productNr: string;
  productName: string;
  productQuantity: number;
  productSumAmount: number;
  productPurchasePrice: number;
  productPurchasePriceCurrency: number;
  productPurchaseAmount: number;
  productPrice: number;
  productMarginalIncome: number;
  productMarginalRatio: number;
  projectNr: string;
  contractCategory: string;
  customerCategory: string;
  orderCategory: string;
  productCategory: string;
  costCentre: string;
  wholeSellerName: string;
  referenceOur: string;
  currencyCode: string;
  productSumAmountCurrency: number;
  payingCustomerName: string;
  attestStateId: number;
  attestStateName: string;
  attestStateColor: string;
  productGroupId: number;
  productGroupName: string;
  timeCodeId: number;
  timeCodeName: string;
  parentProductCategories: string;

  constructor() {
    this.originType = SoeOriginType.None;
    this.originUsers = '';
    this.mainUserName = '';
    this.orderType = 0;
    this.orderTypeName = '';
    this.invoiceNr = '';
    this.orderNr = '';
    this.customerName = '';
    this.customerStreetAddress = '';
    this.customerPostalAddress = '';
    this.customerPostalCode = '';
    this.customerCountry = '';
    this.productNr = '';
    this.productName = '';
    this.productQuantity = 0;
    this.productSumAmount = 0;
    this.productPurchasePrice = 0;
    this.productPurchaseAmount = 0;
    this.productPurchasePriceCurrency = 0;
    this.productPrice = 0;
    this.productMarginalIncome = 0;
    this.productMarginalRatio = 0;
    this.projectNr = '';
    this.contractCategory = '';
    this.customerCategory = '';
    this.orderCategory = '';
    this.productCategory = '';
    this.costCentre = '';
    this.wholeSellerName = '';
    this.referenceOur = '';
    this.currencyCode = '';
    this.productSumAmountCurrency = 0;
    this.payingCustomerName = '';
    this.attestStateId = 0;
    this.attestStateName = '';
    this.attestStateColor = '';
    this.productGroupId = 0;
    this.productGroupName = '';
    this.timeCodeId = 0;
    this.timeCodeName = '';
    this.parentProductCategories = '';
  }

  fixDates(removeTime: boolean = false): void {
    if (removeTime) {
      if (this.date && DateUtil.isValidDateOrString(this.date)) {
        this.date = DateUtil.parseDateOrJson(this.date)!;
        this.date.clearHours();
      }
    } else {
      this.date = DateUtil.parseDateOrJson(this.date);
    }
  }
}

export class GeneralProductStatisticsDTO
  implements IGeneralProductStatisticsModel
{
  originType: SoeOriginType;
  fromDate: Date;
  toDate: Date;

  constructor() {
    this.originType = SoeOriginType.CustomerInvoice;
    this.fromDate = new Date(
      new Date().getFullYear(),
      new Date().getMonth() - 1,
      1
    );
    this.toDate = new Date();
  }
}
