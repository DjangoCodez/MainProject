import { CurrencyHelper, CurrencyHelperEvent } from "../../../../../Common/Directives/Helpers/CurrencyHelper";
import { ProductRowsContainers, CurrencyEvent, AmountEvent, ProductRowsAmountField } from "../../../../../Util/Enumerations";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ProductRowDTO } from "../../../../../Common/Models/InvoiceDTO";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { TermGroup_CurrencyType, SoeInvoiceRowDiscountType, SoeInvoiceRowType } from "../../../../../Util/CommonEnumerations";


export class AmountHelper {

    // Helpers
    private currencyHelper: CurrencyHelper;


    // Properties
    public productGuaranteeId = 0;
    public marginalIncomeLimit = 0;
    public isCredit: boolean;
    public priceListTypeInclusiveVat: boolean;
    public calculateMarginalIncomeOnZeroPurchase: boolean;

    public get currencyHelperObject(): any {
        return this.currencyHelper;
    }

    public get currencyId(): number {
        return this.currencyHelper ? this.currencyHelper.currencyId : 0;
    }
    public set currencyId(value: number) {
        if (this.currencyHelper)
            this.currencyHelper.currencyId = value;
    }

    public get currencyDate(): Date {
        return this.currencyHelper ? this.currencyHelper.currencyDate : null;
    }
    public set currencyDate(date: Date) {
        if (this.currencyHelper) {
            this.currencyHelper.currencyDate = date;
        }
    }

    public get currencyRateDate(): Date {
        return this.currencyHelper ? this.currencyHelper.currencyRateDate : null;
    }
    public set currencyRateDate(date: Date) {
        if (this.currencyHelper)
            this.currencyHelper.currencyRateDate = date;
    }
    
    public get baseCurrencyCode(): string {
        return this.currencyHelper ? this.currencyHelper.getBaseCurrencyCode() : '';
    }

    public get ledgerCurrencyCode(): string {
        return this.currencyHelper ? this.currencyHelper.getLedgerCurrencyCode() : '';
    }

    public get transactionCurrencyCode(): string {
        return this.currencyHelper ? this.currencyHelper.getTransactionCurrencyCode() : '';
    }

    public get transactionCurrencyRate(): number {
        return this.currencyHelper ? this.currencyHelper.transactionCurrencyRate : 1;
    }

    public get isBaseCurrency(): boolean {
        return this.currencyHelper ? this.currencyHelper.getIsBaseCurrency() : true;
    }

    public get isLedgerCurrency(): boolean {
        return this.currencyHelper ? this.currencyHelper.getIsLedgerCurrency() : true;
    }

    constructor(private container: ProductRowsContainers, private coreService: ICoreService, private $timeout: ng.ITimeoutService, private $q: ng.IQService, private currencyChangedCallback: () => void, private currencyIdChangedCallback: () => void) {
        this.currencyHelper = new CurrencyHelper(this.coreService, this.$timeout, this.$q);
    }

    public init() {
        // Currency helper
        const subscriptions = [];
        if (this.currencyChangedCallback) {
            const currencyChanged: CurrencyHelperEvent = new CurrencyHelperEvent(CurrencyEvent.CurrencyChanged, () => {
                if (this.currencyChangedCallback) {
                    this.currencyChangedCallback();
                }
            });
            subscriptions.push(currencyChanged);
        }

        if (this.currencyIdChangedCallback) {
            const currencyIdChanged: CurrencyHelperEvent = new CurrencyHelperEvent(CurrencyEvent.CurrencyIdChanged, () => {
                if (this.currencyIdChangedCallback) {
                    this.currencyIdChangedCallback();
                }
            });
            subscriptions.push(currencyIdChanged);
        }

        this.currencyHelper.subscribe(subscriptions);
        this.currencyHelper.init();
    }

    // Events

    private events: AmountHelperEvent[];
    public subscribe(events: AmountHelperEvent[]) {
        if (!this.events)
            this.events = events;
        else
            events.forEach(x => this.events.push(x));
    }

    private getEventFunctions(ev: AmountEvent): any {
        if (this.events)
            return this.events.filter(e => e.event === ev).map(e => e.func);

        return [];
    }

    private setFixedPrice(row: ProductRowDTO) {
        const funcs = this.getEventFunctions(AmountEvent.SetFixedPrice);
        funcs.forEach(f => f(row));
    }

    private calculateAmounts() {
        const funcs = this.getEventFunctions(AmountEvent.CalculateAmounts);
        funcs.forEach(f => f());
    }

    public calculateRowSum(row: ProductRowDTO, checkFixedPrice = true, deleteIfZero = false, workQuantity: number = null, forceUpdate = false, isTaxDeductionRow = false) {
        if (!row)
            return;

        if (row.isLiftProduct) {
            if (!row.quantity) {
                row.quantity = 1;
                row.invoiceQuantity = 1;
            }
            
            if ((row.amountCurrency > 0 && this.container == ProductRowsContainers.Order) /*|| (row.amountCurrency < 0 && this.container == ProductRowsContainers.Invoice && !this.isCredit)*/) { // Temporary removed due to error in recalculation of deduction rows for previously invoiced lift rows when changing invoie date
                row.amount = -row.amount;
                row.amountCurrency = -row.amountCurrency;
            }
        } else if (row.isClearingProduct) {
            if (!row.quantity) {
                row.quantity = 1;
                row.invoiceQuantity = 1;
            }

            if ((row.amountCurrency < 0 && this.container == ProductRowsContainers.Order) || (row.amountCurrency > 0 && this.container == ProductRowsContainers.Invoice)) {
                row.amount = -row.amount;
                row.amountCurrency = -row.amountCurrency;
            }
        } else {
            if (checkFixedPrice && (row.productId !== this.productGuaranteeId) && !row.isHouseholdRow ) {
                this.setFixedPrice(row);
            }
        }

        if (row.amountCurrency != 0)
            this.getCurrencyAmount(row.amountCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { row.amount = am });
        else
            row.amount = 0;

        // Calculate discount
        const quantity: number = row.quantity ? row.quantity : 0;
        this.calculateDiscount(row, quantity);

        // Calculate row sum
        row.sumAmountCurrency = ((row.amountCurrency * row.quantity) - row.discountAmountCurrency).round(2);
        this.calculateDiscount2(row, quantity);

        // If credit invoice, negate amount
        if (this.isCredit && !row.isCentRoundingRow && !row.isInvoiceFeeRow && !row.isFreightAmountRow)
            row.sumAmountCurrency = -row.sumAmountCurrency;

        this.calculateRowCurrencyAmount(row, ProductRowsAmountField.SumAmount, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);

        this.calculateRowCurrencyAmount(row, ProductRowsAmountField.PurchasePrice, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);

        if (row.householdAmountCurrency)
            row.householdAmountCurrency = row.householdAmount = -row.sumAmountCurrency;

        if (!row.isCentRoundingRow) {
            // Calculate row VAT amount
            if (!row.householdProperty)
                this.calculateRowVatAmount(row, forceUpdate);

            // Calculate marginal income
            this.calculateMarginalIncome(row, workQuantity ? workQuantity : quantity);
            this.calculateMarginalIncomeRatio(row);
            this.calculateMarginalIncomeLimit(row);

            // Calculate supplement charge
            this.calculateSupplementCharge(row);
        }

        //if (calculateTotalAmounts)
        this.calculateAmounts();

        this.getCurrencyAmount(row.sumAmountCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { row.sumAmount = am.round(2) });

        // Always clear split accounting
        row.splitAccountingRows = [];
        
        row.sumTotalAmountCurrency = this.priceListTypeInclusiveVat ? row.sumAmountCurrency : row.sumAmountCurrency + row.vatAmountCurrency;

        if (isTaxDeductionRow) {
            row.householdAmount = row.sumAmount;
            row.householdAmountCurrency = row.sumAmountCurrency;
        }

        //(sourceRow.amount != row.amount || sourceRow.amountCurrency != row.amountCurrency || sourceRow.vatAmount != row.vatAmount || sourceRow.vatAmountCurrency != row.vatAmountCurrency) ? true : false;
        row.isModified = true;
    }

    private calculateDiscount(row: ProductRowDTO, quantity: number) {
        row.discountAmountCurrency = 0;
        row.discountPercent = 0;

        if (row.amountCurrency !== 0 && !row.isCentRoundingRow) {
            var amountSum: number = row.amountCurrency * quantity;

            if (!row.discountValue)
                row.discountValue = 0;

            if (row.discountType === SoeInvoiceRowDiscountType.Amount) {
                row.discountAmountCurrency = NumberUtility.parseNumericDecimal(row.discountValue).round(2);
                row.discountPercent = amountSum != 0 ? row.discountValue / amountSum * 100 : 0;
            } else if (row.discountType === SoeInvoiceRowDiscountType.Percent) {
                row.discountPercent = row.discountValue;
                row.discountAmountCurrency = (amountSum * row.discountValue / 100).round(2);
            }
        }

       this.calculateRowCurrencyAmount(row, ProductRowsAmountField.DiscountAmount, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);
    }

    private calculateDiscount2(row: ProductRowDTO, quantity: number) {
        row.discount2AmountCurrency = 0;
        row.discount2Percent = 0;

        if (row.amountCurrency !== 0 && !row.isCentRoundingRow) {
            var amountSum: number = row.amountCurrency * quantity;

            if (!row.discount2Value)
                row.discount2Value = 0;

            if (row.discount2Type === SoeInvoiceRowDiscountType.Amount) {
                row.discount2AmountCurrency = NumberUtility.parseNumericDecimal(row.discount2Value).round(2);
                row.discount2Percent = amountSum != 0 ? row.discount2Value / amountSum * 100 : 0;
            } else if (row.discount2Type === SoeInvoiceRowDiscountType.Percent) {
                row.discount2Percent = row.discount2Value;
                row.discount2AmountCurrency = ((amountSum - row.discountAmountCurrency) * row.discount2Value / 100).round(2);
            }

            if (row.discount2AmountCurrency > 0) {
                row.sumAmountCurrency = (row.sumAmountCurrency - row.discount2AmountCurrency).round(2); //total value
            }
        }

        this.calculateRowCurrencyAmount(row, ProductRowsAmountField.Discount2Amount, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);
    }


    private calculateRowVatAmount(rowItem: ProductRowDTO, forceUpdate = false): number {
        let amountCurrency: number = 0;
        if (rowItem.vatAccountId) {
            // Need to ensure that rounding is made in same way for both postivie and negative numbers
            let sumAmountAbs: number = rowItem.sumAmountCurrency;
            let isNegative: boolean = false;
            if (sumAmountAbs < 0) {
                isNegative = true;
                sumAmountAbs = Math.abs(sumAmountAbs);
            }

            if (this.priceListTypeInclusiveVat)
                amountCurrency = (sumAmountAbs - (sumAmountAbs / (1 + rowItem.vatRate / 100))).roundToNearest(2);
            else
                amountCurrency = (sumAmountAbs * rowItem.vatRate / 100).roundToNearest(2);

            // Convert it back to negative if needed
            if (isNegative)
                amountCurrency = -amountCurrency;

        }
        if (rowItem.vatAmountCurrency !== amountCurrency || forceUpdate) {
            rowItem.vatAmountCurrency = amountCurrency;
            this.currencyHelper.getCurrencyAmount(amountCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(amount => { rowItem.vatAmount = amount.roundToNearest(2); });
        }
        
        return amountCurrency;
    }

    public calculateSubTotals(rows: ProductRowDTO[]): boolean {
        let anyRowsUpdated = false;
        if (!_.some(rows, r => r.type === SoeInvoiceRowType.SubTotalRow)) {
            return;
        }

        let sum: number = 0;
        let sumCurrency: number = 0;

        _(rows)
            .filter(r => r.type === SoeInvoiceRowType.ProductRow || r.type === SoeInvoiceRowType.SubTotalRow)
            .orderBy("rowNr")
            .forEach(row => {
                if (row.type === SoeInvoiceRowType.SubTotalRow) {
                    
                    if (!row.sumAmountCurrency) {
                        row.sumAmountCurrency = 0;
                    }
                    if (!row.sumAmount) {
                        row.sumAmount = 0;
                    }
                    
                    if (row.sumAmountCurrency.toFixed(4) !== sumCurrency.toFixed(4) || row.sumAmount.toFixed(4) !== sum.toFixed(4)) {
                        row.sumAmount = sum;
                        row.sumAmountCurrency = sumCurrency;
                        row.sumTotalAmountCurrency = this.priceListTypeInclusiveVat ? row.sumAmountCurrency : row.sumAmountCurrency + row.vatAmountCurrency;
                        row.isModified = true;
                        anyRowsUpdated = true;
                    }
                    sumCurrency = 0;
                    sum = 0;
                } else {
                    sum += row.sumAmount;
                    sumCurrency += row.sumAmountCurrency;
                }
            });

        return anyRowsUpdated;
    }

    public supplementChargeChanged(row: ProductRowDTO): boolean {
        if (!row.quantity)
            return false;

        let ret = true;

        if (row.purchasePriceCurrency === 0) {
            row.marginalIncomeCurrency = row.sumAmountCurrency;
            row.marginalIncomeRatio = 100;
            ret = false;
            this.getCurrencyAmount(row.marginalIncomeCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { row.marginalIncome = am });
        } else {
            row.amountCurrency = row.purchasePriceCurrency * (1 + (row.supplementCharge / 100));
            row.amountCurrency = row.amountCurrency.round(2);
            this.getCurrencyAmount(row.amountCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { row.amount = am.round(2) });
        }

        this.calculateRowSum(row);

        return ret;
    }

    public purchasePriceChanged(row: ProductRowDTO) {
        this.getCurrencyAmount(row.purchasePriceCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { row.purchasePrice = am });
        this.calculateMarginalIncome(row, row.quantity);
        this.calculateMarginalIncomeRatio(row);
        this.calculateMarginalIncomeLimit(row);
        this.calculateSupplementCharge(row);
    }

    public marginalIncomeChanged(row: ProductRowDTO) {
        if (!row.quantity)
            return;

        var sum: number = NumberUtility.parseNumericDecimal(row.purchasePriceCurrency * row.quantity) + NumberUtility.parseNumericDecimal(row.marginalIncomeCurrency);
        row.amountCurrency = sum / row.quantity;
        this.getCurrencyAmount(row.amountCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { row.amount = am });
        this.calculateRowSum(row);

        this.calculateMarginalIncomeRatio(row);
        this.calculateSupplementCharge(row);
    }

    public marginalIncomeRatioChanged(row: ProductRowDTO) {
        row.amountCurrency = (this.priceListTypeInclusiveVat ? row.purchasePriceCurrency * (1 + (row.vatRate / 100)) : row.purchasePriceCurrency) / (1 - (row.marginalIncomeRatio / 100));
        row.amountCurrency = row.amountCurrency.round(2);
        this.getCurrencyAmount(row.amountCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { row.amount = am.round(2) });
        this.calculateRowSum(row);

        this.calculateMarginalIncome(row, row.quantity);
        this.calculateSupplementCharge(row);
    }

    private calculateMarginalIncome(row: ProductRowDTO, quantity: number) {
        if (row.isHouseholdRow || ((row.isLiftProduct || row.isClearingProduct) && this.container == ProductRowsContainers.Order)) {
            row.marginalIncomeCurrency = 0;
            row.marginalIncome = 0;
            return;
        }

        row.marginalIncomeCurrency = ((this.priceListTypeInclusiveVat ? row.sumAmountCurrency - row.vatAmountCurrency : row.sumAmountCurrency) - (row.purchasePriceCurrency * quantity));

        this.getCurrencyAmount(row.marginalIncomeCurrency, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency).then(am => { row.marginalIncome = am.round(2) });
        this.calculateMarginalIncomeLimit(row);
    }

    public calculateMarginalIncomeLimit(row: ProductRowDTO) {
        // Marginal income color
        row.marginalIncomeLimit = row.purchasePriceCurrency === 0 ? 0 : row.marginalIncomeRatio - this.marginalIncomeLimit;
    }

    private calculateMarginalIncomeRatio(row: ProductRowDTO) {
        const tb = (this.priceListTypeInclusiveVat ? row.sumAmountCurrency - row.vatAmountCurrency : row.sumAmountCurrency) - (row.purchasePriceCurrency * row.quantity);
        row.marginalIncomeRatio = !row.amountCurrency ? 0 : ((tb / (this.priceListTypeInclusiveVat ? row.sumAmountCurrency - row.vatAmountCurrency : row.sumAmountCurrency)) * 100).round(2);

        // Null fix
        if (!row.marginalIncomeRatio)
            row.marginalIncomeRatio = 0;
    }

    public calculateSupplementCharge(row: ProductRowDTO) {
        if (!row.purchasePriceCurrency && !row.sumAmountCurrency)
            row.supplementCharge = 0;
        else if (!row.purchasePriceCurrency)
            row.supplementCharge = 100; // TODO: decimal.MaxValue - VAD GÖRA MED DETTA?
        else
            row.supplementCharge = ((((row.sumAmountCurrency / row.quantity) / row.purchasePriceCurrency) - 1) * 100).round(2);

        // Null fix
        if (!row.supplementCharge)
            row.supplementCharge = 0;
    }

    // Set amounts in all currency fields based on amount in another currency field
    public calculateAllRowsCurrencyAmounts(row: ProductRowDTO, field: ProductRowsAmountField, sourceCurrencyType: TermGroup_CurrencyType) {
        this.calculateRowCurrencyAmount(row, field, TermGroup_CurrencyType.BaseCurrency, sourceCurrencyType);
        //this.calculateRowCurrencyAmount(row, field, sourceCurrencyType, TermGroup_CurrencyType.TransactionCurrency);
        //this.calculateRowCurrencyAmount(row, field, sourceCurrencyType, TermGroup_CurrencyType.EnterpriseCurrency);
        //this.calculateRowCurrencyAmount(row, field, sourceCurrencyType, TermGroup_CurrencyType.LedgerCurrency);
    }

    // Set amount in one currency field based on amount in another currency field
    public calculateRowCurrencyAmount(row: ProductRowDTO, field: ProductRowsAmountField, sourceCurrencyType: TermGroup_CurrencyType, targetCurrencyType: TermGroup_CurrencyType) {
        if (sourceCurrencyType === targetCurrencyType)
            return;

        var sourceAmount: number = this.getAmount(row, field, sourceCurrencyType);
        var amount = this.currencyHelper.getCurrencyAmountNonAsync(sourceAmount, sourceCurrencyType, targetCurrencyType);
        this.setAmount(row, field, targetCurrencyType, amount);
    }

    // Get amount from one currency field
    private getAmount(row: ProductRowDTO, field: ProductRowsAmountField, sourceCurrencyType: TermGroup_CurrencyType): number {
        var amount: number = 0;

        switch (field) {
            case ProductRowsAmountField.Amount:
                if (sourceCurrencyType === TermGroup_CurrencyType.BaseCurrency)
                    amount = row.amount || 0;
                else if (sourceCurrencyType === TermGroup_CurrencyType.TransactionCurrency)
                    amount = row.amountCurrency || 0;
                break;
            case ProductRowsAmountField.DiscountAmount:
                if (sourceCurrencyType === TermGroup_CurrencyType.BaseCurrency)
                    amount = row.discountAmount || 0;
                else if (sourceCurrencyType === TermGroup_CurrencyType.TransactionCurrency)
                    amount = row.discountAmountCurrency || 0;
                break;
            case ProductRowsAmountField.Discount2Amount:
                if (sourceCurrencyType === TermGroup_CurrencyType.BaseCurrency)
                    amount = row.discount2Amount || 0;
                else if (sourceCurrencyType === TermGroup_CurrencyType.TransactionCurrency)
                    amount = row.discount2AmountCurrency || 0;
                break;
            case ProductRowsAmountField.SumAmount:
                if (sourceCurrencyType === TermGroup_CurrencyType.BaseCurrency)
                    amount = row.sumAmount || 0;
                else if (sourceCurrencyType === TermGroup_CurrencyType.TransactionCurrency)
                    amount = row.sumAmountCurrency || 0;
                break;
            case ProductRowsAmountField.VatAmount:
                if (sourceCurrencyType === TermGroup_CurrencyType.BaseCurrency)
                    amount = row.vatAmount || 0;
                else if (sourceCurrencyType === TermGroup_CurrencyType.TransactionCurrency)
                    amount = row.vatAmountCurrency || 0;
                break;
            case ProductRowsAmountField.PurchasePrice:
                if (sourceCurrencyType === TermGroup_CurrencyType.BaseCurrency)
                    amount = row.purchasePrice || 0;
                else if (sourceCurrencyType === TermGroup_CurrencyType.TransactionCurrency)
                    amount = row.purchasePriceCurrency || 0;
                break;
            case ProductRowsAmountField.MarginalIncome:
                if (sourceCurrencyType === TermGroup_CurrencyType.BaseCurrency)
                    amount = row.marginalIncome || 0;
                else if (sourceCurrencyType === TermGroup_CurrencyType.TransactionCurrency)
                    amount = row.marginalIncomeCurrency || 0;
                break;
            case ProductRowsAmountField.HouseholdAmount:
                if (sourceCurrencyType === TermGroup_CurrencyType.BaseCurrency)
                    amount = row.householdAmount || 0;
                else if (sourceCurrencyType === TermGroup_CurrencyType.TransactionCurrency)
                    amount = row.householdAmountCurrency || 0;
                break;
        }

        return amount;
    }

    // Set amount in one currency field
    private setAmount(row: ProductRowDTO, field: ProductRowsAmountField, currencyType: TermGroup_CurrencyType, amount: number) {
        amount = amount || 0;

        switch (field) {
            case ProductRowsAmountField.Amount:
                if (currencyType === TermGroup_CurrencyType.BaseCurrency)
                    row.amount = amount;
                else if (currencyType === TermGroup_CurrencyType.TransactionCurrency)
                    row.amountCurrency = amount;
                break;
            case ProductRowsAmountField.DiscountAmount:
                if (currencyType === TermGroup_CurrencyType.BaseCurrency) {
                    row.discountAmount = amount.round(2);
                } else if (currencyType === TermGroup_CurrencyType.TransactionCurrency) { 
                    row.discountAmountCurrency = amount.round(2);
                }
                break;
            case ProductRowsAmountField.Discount2Amount:
                if (currencyType === TermGroup_CurrencyType.BaseCurrency) {
                    row.discount2Amount = amount.round(2);
                } else if (currencyType === TermGroup_CurrencyType.TransactionCurrency) {
                    row.discount2AmountCurrency = amount.round(2);
                }
                break;
             case ProductRowsAmountField.SumAmount:
                if (currencyType === TermGroup_CurrencyType.BaseCurrency)
                    row.sumAmount = amount.round(2);
                else if (currencyType === TermGroup_CurrencyType.TransactionCurrency)
                    row.sumAmountCurrency = amount.round(2);
                break;
            case ProductRowsAmountField.VatAmount:
                if (currencyType === TermGroup_CurrencyType.BaseCurrency)
                    row.vatAmount = amount.round(2);
                else if (currencyType === TermGroup_CurrencyType.TransactionCurrency)
                    row.vatAmountCurrency = amount.round(2);
                break;
            case ProductRowsAmountField.PurchasePrice:
                if (currencyType === TermGroup_CurrencyType.BaseCurrency)
                    row.purchasePrice = amount.round(5);
                else if (currencyType === TermGroup_CurrencyType.TransactionCurrency)
                    row.purchasePriceCurrency = amount.round(5);
                break;
            case ProductRowsAmountField.MarginalIncome:
                if (currencyType === TermGroup_CurrencyType.BaseCurrency)
                    row.marginalIncome = amount.round(2);
                else if (currencyType === TermGroup_CurrencyType.TransactionCurrency)
                    row.marginalIncomeCurrency = amount.round(2);
                break;
            case ProductRowsAmountField.HouseholdAmount:
                if (currencyType === TermGroup_CurrencyType.BaseCurrency)
                    row.householdAmount = amount.round(2);
                else if (currencyType === TermGroup_CurrencyType.TransactionCurrency)
                    row.householdAmountCurrency = amount.round(2);
                break;
        }
        
        row.sumTotalAmountCurrency = this.priceListTypeInclusiveVat ? row.sumAmountCurrency : row.sumAmountCurrency + row.vatAmountCurrency;
    }

    public getCurrencyAmount(amount: number, sourceCurrencyType: TermGroup_CurrencyType, targetCurrencyType: TermGroup_CurrencyType): ng.IPromise<number> {
        var deferral = this.$q.defer<number>();

        if (!this.currencyHelper)
            deferral.resolve(amount);
        else {
            this.currencyHelper.getCurrencyAmount(amount, sourceCurrencyType, targetCurrencyType).then(am => {
                deferral.resolve(am);
            });
        }

        return deferral.promise;
    }
}

export class AmountHelperEvent {
    constructor(public event: AmountEvent, public func = (...args: any[]) => { }) {
    }
}

