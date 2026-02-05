import { AmountHelper } from "../Directives/ProductRows/Helpers/AmountHelper";
import { ProductRowsContainers } from "../../../Util/Enumerations";
import { ICoreService } from "../../../Core/Services/CoreService";
import { TermGroup_BillingType, TermGroup_CurrencyType } from "../../../Util/CommonEnumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class InvoiceCurrencyHelper {

    private _amountHelper: AmountHelper;

    public currencyCode: string;
    public currencyRate = 1;
    public ledgerCurrencyCode: string;
    
    private _selectedVoucherDate: Date;
    get currencyHelperObject() {
        return this.amountHelper?.currencyHelperObject;
    }
    get currencyDate() {
        return this.amountHelper.currencyDate;
    }
    set currencyDate(date: Date) {
        this.amountHelper.currencyDate = date;
    }

    get currencyId() {
        return this.amountHelper.currencyId;
    }
    set currencyId(id: number) {
        this.amountHelper.currencyId = id;
    }

    get currencyIdReadonly() {
        return this.amountHelper.currencyId;
    }
    set currencyIdReadonly(id: number) {
        //do nothing
    }

    get transactionCurrencyRate() {
        return this.amountHelper.transactionCurrencyRate;
    }

    get currencyRateDate() {
        return this.amountHelper.currencyRateDate;
    }

    set isLedgerCurrency(value: boolean) {
        //do nothing
    }
    get isLedgerCurrency() {
        return this.amountHelper?.isLedgerCurrency ?? true;
    }

    set isBaseCurrency(value: boolean) {
        //do nothing
    }
    get isBaseCurrency() {
        return this.amountHelper?.isBaseCurrency ?? true;
    }

    get baseCurrencyCode() {
        return this.amountHelper?.baseCurrencyCode ?? '';
    }

    get amountHelper() {
        return this._amountHelper;
    }

    set amountHelper(value:any) {
        //do nothing
    }


    //@ngInject
    constructor(container: ProductRowsContainers, coreService: ICoreService, $q: ng.IQService, $timeout: ng.ITimeoutService, currencyChangedCallback: () => void, private currencyIdChangedCallback: () => void) {
        this._amountHelper = new AmountHelper(container, coreService, $timeout, $q, currencyChangedCallback, currencyIdChangedCallback);
        this.currencyDate = CalendarUtility.getDateToday();
        this.amountHelper.init();
    }

    public fromInvoice(invoice: any) {
        this.currencyId = invoice.currencyId;
        this.currencyRate = invoice.currencyRate;
        this.currencyDate = invoice.currencyDate;
        this.amountHelper.isCredit = (invoice.billingType === TermGroup_BillingType.Credit);
    }

    public toInvoice(invoice: any) {
        invoice.currencyId = this.currencyId;
        invoice.currencyRate = this.transactionCurrencyRate;
        invoice.currencyDate = this.currencyRateDate;
    }

    public amountOrCurrencyChanged(invoice: any) {
        this.amountHelper.currencyId = invoice.currencyId;
        this.amountHelper.getCurrencyAmount(invoice.invoiceFee, TermGroup_CurrencyType.BaseCurrency, TermGroup_CurrencyType.TransactionCurrency).then(am => { invoice.invoiceFeeCurrency = am; });
        this.amountHelper.getCurrencyAmount(invoice.freightAmount, TermGroup_CurrencyType.BaseCurrency, TermGroup_CurrencyType.TransactionCurrency).then(am => { invoice.freightAmountCurrency = am; });
    }

    public priceListTypeInclusiveVatChanged(value:boolean) {
        this.amountHelper.priceListTypeInclusiveVat = value;
    }

    public isCreditChanged(value: boolean) {
        this.amountHelper.isCredit = value;
    }
}