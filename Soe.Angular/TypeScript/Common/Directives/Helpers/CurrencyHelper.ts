import { ICoreService } from "../../../Core/Services/CoreService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { CompCurrencyDTO } from "../../Models/CompCurrencyDTO";
import { CurrencyEvent } from "../../../Util/Enumerations";
import { TermGroup_CurrencyType } from "../../../Util/CommonEnumerations";

export class CurrencyHelper {

    private currencies: CompCurrencyDTO[];
    private baseCurrency: CompCurrencyDTO = undefined;
    private enterpriseCurrency: CompCurrencyDTO;
    private ledgerCurrency: CompCurrencyDTO;
    private transactionCurrency: CompCurrencyDTO;

    private baseCurrencyCode: string;
    public getBaseCurrencyCode(): string {
        return this.baseCurrencyCode;
    }
    private enterpriseCurrencyCode: string;
    public getEnterpriseCurrencyCode(): string {
        return this.enterpriseCurrencyCode;
    }
    private ledgerCurrencyCode: string;
    public getLedgerCurrencyCode(): string {
        return this.ledgerCurrencyCode;
    }
    private transactionCurrencyCode: string;
    public getTransactionCurrencyCode(): string {
        return this.transactionCurrencyCode;
    }

    private isBaseCurrency = true;
    public getIsBaseCurrency(): boolean {
        return this.isBaseCurrency;
    }
    private isLedgerCurrency = true;
    public getIsLedgerCurrency(): boolean {
        return this.isLedgerCurrency;
    }

    private _currencyId: number;
    public get currencyId(): number {
        return this._currencyId;
    }
    public set currencyId(value: number) {
        if (this._currencyId !== value) {
            this._currencyId = value;
            if (this.loaded) {
                this.setCurrency(this._currencyId, (this.currencyRateDate ? true : false), true);
                this.currencyIdChanged();
            }
        }
    }

    private _currencyDate: Date;
    public get currencyDate() {
        return this._currencyDate;
    }
    public set currencyDate(date: any) {
        const prevDate = this._currencyDate;

        if (date === undefined) {
            date = null;
        }

        if (date instanceof Date) {
            this._currencyDate = date;
        }
        else {
            this._currencyDate = new Date(<any>date);
        }

        if (date && (!prevDate || !this._currencyDate.isSameDayAs(prevDate))) {
            if (this.loaded) {
                this.setCurrency(this.currencyId, true, true);
            }
        }
    }

    private _currencyRateDate: Date;
    public get currencyRateDate(): Date {
        return this._currencyRateDate;
    }
    public set currencyRateDate(date: Date) {
        if (date && (!this._currencyRateDate?.isSameDayAs(date))) {
            this._currencyRateDate = new Date(<any>date);
            this.loadEnterpriseCurrencyRate();
            this.loadLedgerCurrencyRate();
            if (this.loaded)
                this.setCurrency(this.currencyId, true, true);
        }
    }

    public baseCurrencyRate = 1;
    public enterpriseCurrencyRate = 1;
    private _ledgerCurrencyRate = 1;
    public get ledgerCurrencyRate(): number {
        return this._ledgerCurrencyRate;
    }
    public set ledgerCurrencyRate(rate: number) {
        if (rate === 0)
            rate = 1;

        if (this._ledgerCurrencyRate !== rate) {
            const notify: boolean = !!this._ledgerCurrencyRate;
            this._ledgerCurrencyRate = rate;
            if (notify)
                this.currencyChanged();
        }
    }
    private _transactionCurrencyRate: number;
    public get transactionCurrencyRate(): number {
        return this._transactionCurrencyRate;
    }
    public set transactionCurrencyRate(rate: number) {
        if (rate === 0)
            rate = 1;

        if (this._transactionCurrencyRate !== rate) {
            const notify: boolean = !!this._transactionCurrencyRate;
            this._transactionCurrencyRate = rate;
            if (notify)
                this.currencyChanged();
        }
    }

    public get hasLedgerCurrency(): boolean {
        if (this.ledgerCurrency)
            return true;
        else
            return false;
    }

    public notifyCurrencyChanged = true;
    public isInitialized = false;

    private loaded = false;

    constructor(private coreService: ICoreService, private $timeout: ng.ITimeoutService, private $q: ng.IQService) {
    }

    public init() {
        if (this.isInitialized)
            return;

        this.$q.all([
            this.loadCurrencies(),
            this.loadEnterpriseCurrency()]).then(() => {
                if (this.currencyId) {
                    if (this.currencyRateDate) { this.setCurrency(this.currencyId, true); }
                    else { this.setCurrency(this.currencyId); }
                }
                this.isInitialized = true;
            });
    }

    // Events

    private events: CurrencyHelperEvent[];
    public subscribe(events: CurrencyHelperEvent[]) {
        if (!this.events)
            this.events = events;
        else
            events.forEach(x => this.events.push(x));
    }

    private getEventFunctions(ev: CurrencyEvent): any {
        if (this.events)
            return this.events.filter(e => e.event === ev).map(e => e.func);

        return [];
    }

    private currencyChanged = _.debounce(() => { 
        if (this.notifyCurrencyChanged) {
            const funcs = this.getEventFunctions(CurrencyEvent.CurrencyChanged);
            funcs.forEach(f => f());
        }
    }, 50, { leading: false, trailing: true });

    private currencyIdChanged = _.debounce(() => {
        if (this.notifyCurrencyChanged) {
            const funcs = this.getEventFunctions(CurrencyEvent.CurrencyIdChanged);
            funcs.forEach(f => f());
        }
    }, 50, { leading: false, trailing: true });

    // Lookups
    private loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrencies(true).then((x: CompCurrencyDTO[]) => {
            this.currencies = x;
            for (const curr of this.currencies) {
                curr.date = CalendarUtility.convertToDate(curr.date);
                for (const compRate of curr.compCurrencyRates) {
                    compRate.date = CalendarUtility.convertToDate(compRate.date);
                }
            }
            if (!this.currencyId)
                this.setCurrency(0); // Will set base currency
            else if (!this.baseCurrency)
                this.SetBaseCurrency();
        });
    }

    private loadEnterpriseCurrency(): ng.IPromise<any> {
        return this.coreService.getEnterpriseCurrency().then(x => {
            this.enterpriseCurrency = x;
            this.setCurrencyFields();
            
            if (this.currencyRateDate && this.enterpriseCurrency)
                this.loadEnterpriseCurrencyRate();
            else {
                this.enterpriseCurrencyRate = 1;
                this.loaded = true;
            }
        });
    }

    private loadEnterpriseCurrencyRate() {
        if (!this.enterpriseCurrency)
            return;

        this.coreService.getCompCurrencyRate(this.enterpriseCurrency.sysCurrencyId, this.currencyDate, true).then(x => {
            if (x === 0)
                x = 1;
            this.enterpriseCurrencyRate = x;
            this.loaded = true;
        });
    }

    public loadLedgerCurrency(actorId: number) {
        if (!actorId || actorId === 0) {
            this.ledgerCurrency = this.baseCurrency;
            this.setCurrencyFields();
        } else if (this.coreService) {
            this.coreService.getLedgerCurrency(actorId).then(x => {
                this.ledgerCurrency = x;
                this.setCurrencyFields();
                if (this.currencyRateDate)
                    this.loadLedgerCurrencyRate();
            });
        }
    }

    private loadLedgerCurrencyRate() {
        if (!this.ledgerCurrency)
            return;

        this.coreService.getCompCurrencyRate(this.ledgerCurrency.sysCurrencyId, this.currencyDate, true).then(x => {
            this.ledgerCurrencyRate = x;
        });
    }

    private SetBaseCurrency() {
        // If no base currency is specified, use first currency in list as base currency
        if (!this.baseCurrency && this.currencies && this.currencies.length > 0)
            this.baseCurrency = this.currencies[0];

        if (this.baseCurrency) {
            this.baseCurrencyCode = this.baseCurrency.code;
            if (this.currencyId) {
                this.isBaseCurrency = this.currencyId == this.baseCurrency.currencyId;
            }
        }
    }

    private setCurrency(currencyId: number, useCurrencyDate?: boolean, notify?: boolean): number {
        notify = notify && !!currencyId;

        // If no currency specified, use base currency
        if (currencyId === 0 && this.baseCurrency)
            currencyId = this.baseCurrency.currencyId;

        if (currencyId !== 0) {
            this.transactionCurrency = _.find(this.currencies, { currencyId: currencyId });
            if (this.transactionCurrency) {
                if (useCurrencyDate && this.transactionCurrency.compCurrencyRates && this.transactionCurrency.compCurrencyRates.length > 0) {
                    // Get last rate before specified currency date
                    const rates = _.orderBy(_.filter(this.transactionCurrency.compCurrencyRates, r => CalendarUtility.convertToDate(r.date).isSameOrBeforeOnDay(this.currencyDate)), 'date', 'desc');
                    const currentRate = _.head(rates);
                    this.transactionCurrencyRate = currentRate ? currentRate.rateToBase : this.transactionCurrency.rateToBase;
                    this.currencyRateDate = currentRate ? currentRate.date : null;
                }
                else {
                    this.transactionCurrencyRate = this.transactionCurrency.rateToBase;
                    this.currencyRateDate = (this.baseCurrency.currencyId === this.transactionCurrency.currencyId) ? CalendarUtility.getDateToday() : this.transactionCurrency.date;
                }
            }
        } else {
            this.transactionCurrency = null;
            this.transactionCurrencyRate = 1;
            this.currencyRateDate = null;
        }

        this.currencyId = currencyId;

        this.SetBaseCurrency();
        this.setCurrencyFields();

        if (notify)
            this.currencyChanged();

        return currencyId;
    }

    private setCurrencyFields() {
        this.baseCurrencyCode = this.baseCurrency ? this.baseCurrency.code : null;
        this.enterpriseCurrencyCode = this.enterpriseCurrency ? this.enterpriseCurrency.code : null;
        this.ledgerCurrencyCode = this.ledgerCurrency ? this.ledgerCurrency.code : null;
        this.transactionCurrencyCode = this.transactionCurrency ? this.transactionCurrency.code : null;

        this.isBaseCurrency = !this.baseCurrency || this.currencyId == this.baseCurrency.currencyId;
        this.isLedgerCurrency = !this.ledgerCurrency || this.ledgerCurrency.currencyId == this.currencyId;
    }

    // Public methods
    public getCurrencyAmountNonAsync(amount: number, sourceCurrencyType: TermGroup_CurrencyType, targetCurrencyType: TermGroup_CurrencyType): number {
        // Convert from source currency to base currency
        let sourceRate: number;
        switch (sourceCurrencyType) {
            case TermGroup_CurrencyType.TransactionCurrency:
                sourceRate = this.transactionCurrencyRate;
                break;
            case TermGroup_CurrencyType.EnterpriseCurrency:
                sourceRate = this.enterpriseCurrencyRate;
                break;
            case TermGroup_CurrencyType.LedgerCurrency:
                sourceRate = this.ledgerCurrencyRate;
                break;
        }

        if (!sourceRate)
            sourceRate = 1;

        amount *= sourceRate;

        // Convert from base currency to target currency
        let targetRate: number;
        switch (targetCurrencyType) {
            case TermGroup_CurrencyType.TransactionCurrency:
                targetRate = this.transactionCurrencyRate;
                break;
            case TermGroup_CurrencyType.EnterpriseCurrency:
                targetRate = this.enterpriseCurrencyRate;
                break;
            case TermGroup_CurrencyType.LedgerCurrency:
                targetRate = this.ledgerCurrencyRate;
                break;
        }

        if (!targetRate)
            targetRate = 1;
        amount /= targetRate;

        return amount.round(5);
    }

    public getCurrencyAmount(amount: number, sourceCurrencyType: TermGroup_CurrencyType, targetCurrencyType: TermGroup_CurrencyType, count = 10, time = 250): ng.IPromise<number> {
        const deferral = this.$q.defer<number>();

        if (!this.loaded) {
            if (count-- > 0) {
                return this.$timeout(() => { }, time)
                    .then(() => {
                        return this.getCurrencyAmount(amount, sourceCurrencyType, targetCurrencyType, count, time * 2);
                    });
            } else {
                deferral.resolve(amount);
            }
        }

        amount = this.getCurrencyAmountNonAsync(amount, sourceCurrencyType, targetCurrencyType);
        deferral.resolve(amount);

        return deferral.promise;
    }
}

export class CurrencyHelperEvent {
    constructor(public event: CurrencyEvent, public func = (...args: any[]) => { }) {
    }
}