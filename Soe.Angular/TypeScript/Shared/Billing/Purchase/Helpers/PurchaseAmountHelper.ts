import { CurrencyHelper, CurrencyHelperEvent } from "../../../../Common/Directives/Helpers/CurrencyHelper";
import { PurchaseDTO } from "../../../../Common/Models/PurchaseDTO";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { TermGroup_CurrencyType } from "../../../../Util/CommonEnumerations";
import { CurrencyEvent } from "../../../../Util/Enumerations";

export class PurchaseAmountHelper {
    private currencyHelper: CurrencyHelper;

    public currencyCode: string;
    public currencyRate = 1;

    public currencies: any[];

    get currencyId() {
        return this.currencyHelper.currencyId;
    }
    set currencyId(id: number) {
        this.currencyHelper.currencyId = id;
    }

    get currencyDate() {
        return this.currencyHelper.currencyDate;
    }
    set currencyDate(date: Date) {
        this.currencyHelper.currencyDate = date;
    }

    get transactionCurrencyRate() {
        return this.currencyHelper.transactionCurrencyRate;
    }

    get currencyRateDate() {
        return this.currencyHelper.currencyRateDate;
    }

    public get isBaseCurrency(): boolean {
        return this.currencyHelper ? this.currencyHelper.getIsBaseCurrency() : true;
    }

    //@ngInject
    constructor(private coreService: ICoreService, $timeout: ng.ITimeoutService, private $q: ng.IQService, private currencyChangedCallback: () => void) {
        this.currencyHelper = new CurrencyHelper(coreService, $timeout, this.$q);
        this.currencyDate = CalendarUtility.getDateToday();
        this.initCurrencyHelper();
    }

    private initCurrencyHelper() {
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

        this.currencyHelper.subscribe(subscriptions);
        this.currencyHelper.init();
    }

    public loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesSmall().then(x => {
            this.currencies = x;
        });
    }

    public fromPurchase(purchase: PurchaseDTO) {
        this.currencyId = purchase.currencyId;
        this.currencyRate = purchase.currencyRate;
        this.currencyDate = purchase.currencyDate;
    }

    public toPurchase(purchase: PurchaseDTO) {
        purchase.currencyId = this.currencyId;
        purchase.currencyRate = this.transactionCurrencyRate;
        purchase.currencyDate = this.currencyRateDate;
        purchase.totalAmount = purchase.totalAmountCurrency * purchase.currencyRate;
        purchase.vatAmount = purchase.vatAmountCurrency * purchase.currencyRate;
    }

    public getCurrencyAmount(amount: number, sourceCurrencyType: TermGroup_CurrencyType, targetCurrencyType: TermGroup_CurrencyType): ng.IPromise<number> {
        const deferral = this.$q.defer<number>();

        if (!this.currencyHelper)
            deferral.resolve(amount);
        else {
            this.currencyHelper.getCurrencyAmount(amount, sourceCurrencyType, targetCurrencyType).then(am => {
                deferral.resolve(am);
            });
        }

        return deferral.promise;
    }

    public get baseCurrencyCode(): string {
        return this.currencyHelper ? this.currencyHelper.getBaseCurrencyCode() : '';
    }
}