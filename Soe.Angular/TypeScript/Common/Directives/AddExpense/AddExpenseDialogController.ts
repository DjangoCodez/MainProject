import { DialogControllerBase } from "../../../Core/Controllers/DialogControllerBase";
import { ITimeCodeAdditionDeductionDTO, ITimeCodeInvoiceProductDTO } from "../../../Scripts/TypeLite.Net4";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService} from "../../../Core/Services/UrlHelperService";
import { CompanySettingType, SoeEntityImageType, SoeEntityType, TermGroup_ExpenseType, TermGroup_TimeCodeRegistrationType } from "../../../Util/CommonEnumerations";
import { AccountingSettingsRowDTO } from "../../../Common/Models/AccountingSettingsRowDTO";
import { NumberUtility } from "../../../Util/NumberUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Constants } from "../../../Util/Constants";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { ExpenseRowDTO } from "../../../Common/Models/ExpenseDTO";
import { TimeCodeAdditionDeductionDTO } from "../../../Common/Models/TimeCode";
import { SmallGenericType } from "../../Models/SmallGenericType";
import { IFocusService } from "../../../Core/Services/focusservice";
import { FilesHelper } from "../../Files/FilesHelper";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Guid } from "../../../Util/StringUtility";
import { IDirtyHandler } from "../../../Core/Handlers/DirtyHandler";

export class AddExpenseDialogController extends DialogControllerBase {

    private guid: Guid;

    // Terms
    private terms: { [index: string]: string; };
    private unitPriceInfo: string;

    private tabIndexes;
    private row: ExpenseRowDTO;

    // Flags    
    private saveInProgress: boolean = false;
    private isPopulating: boolean = false;
    private useEmployees: boolean = false;
    private ignoreCalculateAmounts = false;
    private commentMandatory: boolean = false;
    private billingUseQuantityPrices = false;

    // Files
    private dirtyHandler: IDirtyHandler;
    private filesHelper: FilesHelper;


    private edit: ng.IFormController;

    private _selectedTimeCode;
    get selectedTimeCode(): ITimeCodeAdditionDeductionDTO {
        return this._selectedTimeCode;
    }
    set selectedTimeCode(item: ITimeCodeAdditionDeductionDTO) {
        this._selectedTimeCode = item;


        if (this.row && this.selectedTimeCode) { 
            this.row.timeCodeId = this.selectedTimeCode.timeCodeId;
            this.commentMandatory = this.selectedTimeCode.commentMandatory ? true : false;
        }

        if (item) {
            this.ignoreCalculateAmounts = true;
            if (this.isPopulating && this.row.invoicedAmount > 0)
                this.transferToOrder = true;
            else if (!this.transferToOrder && !this.isPopulating)
                this.transferToOrder = true;           

            this.showStdAccount = item.expenseType === TermGroup_ExpenseType.Expense;   
        }

        if (item && !this.isPopulating) {
            if (item.expenseType === TermGroup_ExpenseType.Expense) {
                this.selectedQuantity = 1;                

                if (!this.selectedStartDate)
                    this.selectedStartDate = CalendarUtility.getDateToday();               

                if (this.row && !this.row.isSpecifiedUnitPrice)
                    this.row.isSpecifiedUnitPrice = true;

                this.selectedStopDate = this.selectedStartDate;
            } else {
                this.row.amount = 0;
                this.row.isSpecifiedUnitPrice = false;
                this.row.unitPrice = 0;
                this.row.vat = 0;
                this._selectedUnitPrice = 0;

                if (item.expenseType === TermGroup_ExpenseType.TravellingTime) {
                    this._selectedQuantity = 60;
                    this.row.quantity = 60;
                }
                else {
                    this._selectedQuantity = 1;
                    this.row.quantity = 1;
                }
            }
            this.calculateInvoiceAmount();
        }

        this.ignoreCalculateAmounts = false;
        this.setupTabIndexes();
        this.calculateAmount();
    }

    private _selectedQuantity: number;
    get selectedQuantity(): number {
        return this._selectedQuantity;
    }
    set selectedQuantity(value: number) {
        this._selectedQuantity = (!this.isPopulating && this.projectId && value < 0) ? 0 : value;
        this.row.quantity = this._selectedQuantity;
        this.calculateAmount();
        if(!this.ignoreCalculateAmounts)
            this.calculateInvoiceAmount();
    }
   
    get quantityFormatted(): string {
        return CalendarUtility.minutesToTimeSpan(this.selectedQuantity);
    }
    set quantityFormatted(time: string) {
        const span = CalendarUtility.parseTimeSpan(time);
        this.selectedQuantity = CalendarUtility.timeSpanToMinutes(span);
    }

    private _selectedUnitPrice: number;
    get selectedUnitPrice(): number {
        return this._selectedUnitPrice;
    }
    set selectedUnitPrice(value: number) {
        this._selectedUnitPrice = value;
        this.row.unitPrice = this._selectedUnitPrice;
        this.calculateAmount();
        this.calculateInvoiceAmount();
    }

    private _selectedEmployee: any;
    get selectedEmployee(): any {
        return this._selectedEmployee;
    }
    set selectedEmployee(value: any) {
        this._selectedEmployee = value;
        if (this.row && this.selectedEmployee)
            this.row.employeeId = this.selectedEmployee.id;
    }
    
    get selectedStartDate(): Date {
        return this.row ? this.row.start : undefined;
    }
    set selectedStartDate(date: Date) {
        this.row.start = date;
        
        if ((!this.selectedStopDate || CalendarUtility.isEmptyDate(this.selectedStopDate) || this.selectedStopDate.isSameDayAs(Constants.DATETIME_DEFAULT) || !this.selectedStopDate.isSameDayAs(this.selectedStartDate)))
            this.selectedStopDate = this.selectedStartDate;
        this.calculateQuantityFromDates();
    }

    get selectedStopDate(): Date {
        return this.row ? this.row.stop : undefined;
    }
    set selectedStopDate(date: Date) {
        this.row.stop = date;
        if (date && this.row.start && this.row.stop.diffDays(this.row.start) < 0) {
            this.row.stop = this.row.start;
        }
        this.calculateQuantityFromDates();
    }

    get transferToOrder(): boolean {
        return this.row ? this.row.transferToOrder : false;
    }
    set transferToOrder(value: boolean) {
        if (value && this.showInvoiceFields()) {
            this.row.transferToOrder = true;
            if (this.row.amount != 0 && (!this.row.invoicedAmount || this.row.invoicedAmount === 0) && !this.isPopulating)
                this.row.invoicedAmount = this.row.amount;

            if(!this.ignoreCalculateAmounts)
                this.calculateInvoiceAmount();
        }
        else {
            this.row.transferToOrder = false;
            if (this.row && !this.isPopulating)
                this.row.invoicedAmount = 0;
        }
    }

    get specifiedUnitPrice(): boolean {
        return this.row ? this.row.isSpecifiedUnitPrice : undefined;
    }
    set specifiedUnitPrice(val: boolean) {
        this.row.isSpecifiedUnitPrice = val;
        if (!this.row.isSpecifiedUnitPrice)
            this.calculateInvoiceAmount();
        else {
            this.row.invoicedAmount = 0
        }
    }

    private showStdAccount: boolean = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private focusService: IFocusService,
        translationService: ITranslationService,
        coreService: ICoreService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private readOnly: boolean,
        private isMySelf: boolean,//opened from time or payroll
        private settingTypes: SmallGenericType[],
        private settings: AccountingSettingsRowDTO[],
        private employeeId: number,
        private timePeriodId: number,
        private standsOnDate: Date,
        private isProjectMode: boolean,        
        private timeCodes: TimeCodeAdditionDeductionDTO[],
        private employees: SmallGenericType[],        
        private currencyCode: string,
        private expenseRowId?: number, 
        private customerInvoiceId?: number,
        private projectId?: number,
        private priceListTypeInclusiveVat?: boolean,
        private hasFiles?: boolean    ) {

        super(null, translationService, coreService, notificationService, urlHelperService);

        this.guid = Guid.newGuid();
        this.isNew = !expenseRowId;
        this.modifyPermission = true;        
        this.useEmployees = this.employees && this.employees.length > 0;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.filesHelper = new FilesHelper(this.coreService, this.$q, this.dirtyHandler, true, SoeEntityType.Expense, SoeEntityImageType.Expense, () => this.expenseRowId, null, true);

        this.load();      
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [CompanySettingType.BillingUseQuantityPrices];
        
        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.billingUseQuantityPrices = x[CompanySettingType.BillingUseQuantityPrices];
        });
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public save() {
        
        if (this.isReadOnly() && this.isInvoicingReadOnly() )
            return;

        if (this.commentMandatory && (!this.row.comment || this.row.comment === "")){
            return this.notificationService.showDialog(this.terms["core.error"], this.terms["common.commentrequired"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);

        }

        if (this.selectedStopDate && this.selectedStartDate.isAfterOnDay(this.selectedStopDate))
            return this.notificationService.showDialog(this.terms["core.error"], this.terms["common.startdateafterstopdate"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
        
        if (this.row.vat > this.row.amount) 
            return this.notificationService.showDialog(this.terms["core.error"], this.terms["common.vatlargerthenamout"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);

        if (this.isMySelf) {
            this.row.accounting = this.settingsToString();
            this.row.timePeriodId = this.timePeriodId;
            this.row.employeeId = this.employeeId;
            this.row.standOnDate = this.standsOnDate;
        } 

        if (this.filesHelper.filesLoaded) 
            this.row.files = this.filesHelper.getAsDTOs();

        this.saveExpenseRowsValidation(this.row);         
    }

    public isReadOnly() {
        return this.row ? (this.row.isTimeReadOnly || this.readOnly) : this.readOnly;
    }

    public isInvoicingReadOnly() {
        return this.row ? (this.row.isReadOnly || this.readOnly) : this.readOnly;
    }

    public isTimeCodeReadOnly() {
        if (!this.isReadOnly())
            return false;

        return true;
    }

    public isStartDateReadonly() {
        if (this.isReadOnly())
            return true;

        if (this.selectedTimeCode) {
            switch (this.selectedTimeCode.expenseType) {
                case TermGroup_ExpenseType.Mileage:
                case TermGroup_ExpenseType.AllowanceDomestic:
                case TermGroup_ExpenseType.AllowanceAbroad:
                case TermGroup_ExpenseType.Expense:
                case TermGroup_ExpenseType.TravellingTime:
                case TermGroup_ExpenseType.Time:
                    return false;
                default:
                    return true;
            }
        }
        return true;
    }

    public isStartTimeReadonly() {
        if (this.isReadOnly())
            return true;

        if (this.selectedTimeCode) {
            switch (this.selectedTimeCode.expenseType) {
                case TermGroup_ExpenseType.AllowanceDomestic:
                case TermGroup_ExpenseType.AllowanceAbroad:
                    return false;
                default:
                    return true;
            }
        }
        return true;
    }

    public isStopDateReadonly() {
        if (this.isReadOnly())
            return true;

        if (this.selectedTimeCode) {
            switch (this.selectedTimeCode.expenseType) {
                case TermGroup_ExpenseType.AllowanceDomestic:
                case TermGroup_ExpenseType.AllowanceAbroad:
                    return false;
                default:
                    return true;
            }
        }
        return true;
    }

    public isStopTimeReadonly() {
        if (this.isReadOnly())
            return true;

        if (this.selectedTimeCode) {
            switch (this.selectedTimeCode.expenseType) {
                case TermGroup_ExpenseType.AllowanceDomestic:
                case TermGroup_ExpenseType.AllowanceAbroad:
                    return false;
                default:
                    return true;
            }
        }
        return true;
    }

    public isQuantityReadOnly() {
        if (this.isReadOnly())
            return true;
        
        if (this.selectedTimeCode) {
            switch (this.selectedTimeCode.expenseType) {
                case TermGroup_ExpenseType.Mileage:
                case TermGroup_ExpenseType.AllowanceAbroad:
                case TermGroup_ExpenseType.AllowanceDomestic:
                case TermGroup_ExpenseType.Expense:
                case TermGroup_ExpenseType.TravellingTime:
                case TermGroup_ExpenseType.Time:
                    return false;
                default:
                    return true;
            }
        }
        return true;
    }

    public isSpecifiedUnitPriceReadOnly() {        
        return this.isUnitPriceReadOnlyBase();
    }

    public isUnitPriceReadOnly() {        
        if (!this.row.isSpecifiedUnitPrice)
            return true;

        return this.isUnitPriceReadOnlyBase();
    }

    public isUnitPriceReadOnlyBase() {
        if (this.isReadOnly())
            return true;

        if (this.selectedTimeCode) {
            switch (this.selectedTimeCode.expenseType) {
                case TermGroup_ExpenseType.AllowanceAbroad:
                case TermGroup_ExpenseType.Expense:
                case TermGroup_ExpenseType.TravellingTime:
                case TermGroup_ExpenseType.Time:
                    return false;
                default:
                    return true;
            }
        }
        return true;
    }

    public isAmountReadOnly() {
        return true;
    }

    public isVatReadOnly() {
        if (this.isReadOnly())
            return true;

        if (this.selectedTimeCode) {
            switch (this.selectedTimeCode.expenseType) {
                case TermGroup_ExpenseType.Expense:
                    return false;
                default:
                    return true;
            }
        }
        return true;
    }

    public isInternalCommentReadOnly() {
        return ( this.isReadOnly() || (this.row && this.row.isTimeReadOnly) );
    }

    public isCommentReadOnly() {
        if (!this.isReadOnly() || !this.isInvoicingReadOnly())
            return false;

        return true;
    }

    public isAccountingReadOnly() {
        if (!this.isReadOnly())
            return false;

        return true;
    }

    public showInvoiceFields() {

        if (this.isMySelf && this.isNew) //create new expense from time or payroll => dont show invoice fields
            return false;

        if (this.isMySelf && this.row && (!this.row.customerInvoiceId || this.row.customerInvoiceId === 0)) //create new expense from time or payroll => dont show invoice fields
            return false;

        return (this.selectedTimeCode && this.selectedTimeCode.hasInvoiceProducts)
    }

    //Service calls

    private load() {
        if (this.isNew) {
            this.focusService.focusByName("ctrl_selectedTimeCode");
            this.$q.all([
                this.loadTerms(),
                this.loadCompanySettings(),
            ]).then(() => {
                this.initNewRow();
                this.setupWatches();
            });
        }
        else {
            this.$q.all([
                this.loadTerms(),
                this.loadCompanySettings(),
                this.loadExpenseRow(),
            ]).then(() => {
                if (this.hasFiles || this.row.hasFiles)
                    this.filesHelper.nbrOfFiles = '*';

                this.populateTransaction();
                this.setupWatches();
            });
        }
    }

    private setupWatches() {
        this.$scope.$watch(() => this.row.amount, (newValue, oldValue) => {
            if (newValue !== oldValue) {
                if (this.transferToOrder) {
                    this.calculateInvoiceAmount();
                }
            }
        });
        this.$scope.$watch(() => this.row.vat, (newValue, oldValue) => {
            if (newValue !== oldValue) {
                this.calculateInvoiceAmount();
            }
        });
        this.$scope.$watch(() => this.dirtyHandler.isDirty, (newValue, oldValue) => {
            if (newValue !== oldValue)
                this.edit.$dirty = true;
        });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [            
            "core.warning",
            "common.commentrequired",
            "common.startdateafterstopdate",
            "common.additiondeductions.pricefromformula",
            "common.customer.invoices.amountincvat",
            "common.vatlargerthenamout"
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
            this.unitPriceInfo = this.terms["common.additiondeductions.pricefromformula"];
        });
    }

    private loadExpenseRow(): ng.IPromise<any> {
        return this.coreService.getExpenseRow(this.expenseRowId).then((row) => {
            this.row = row;
            
            // Fix dates
            if (this.row.start)
                this.row.start = new Date(<any>this.row.start);
            if (this.row.stop)
                this.row.stop = new Date(<any>this.row.stop);
            if (this.row.created)
                this.row.created = new Date(<any>this.row.created);
            if (this.row.modified)
                this.row.modified = new Date(<any>this.row.modified);
        });
    }

    private saveExpenseRowsValidation(row: any) {
        this.coreService.saveExpenseRowsValidation([row]).then((validationResult) => {
            if (validationResult.success) {
                this.saveExpense(row);
            }
            else {
                const modal = this.notificationService.showDialog(validationResult.title, validationResult.message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(result => {
                    this.saveExpense(row);
                });
            }
        });
    }

    private saveExpense(row: any) {
        if (this.isMySelf) {
            this.saveInProgress = true;
            this.startSave();
            this.coreService.saveExpenseRows([row], this.row.customerInvoiceId).then((result) => {
                if (result.success) {
                    this.$uibModalInstance.close(true);
                } else {
                    this.saveInProgress = false;
                    this.failedSave(result.errorMessage);
                }
            }, error => {
                this.saveInProgress = false;
                this.failedSave(error.message);
            });
        }
        else {
            this.$uibModalInstance.close({ rowToSave: row });  
        }
    }

    private accountingChanged() {
        this.edit.$dirty = true;
    }

    //Help methods
    private settingsToString(): string {
        if (!this.settings || this.settings.length === 0)
            return '';

        var setting: AccountingSettingsRowDTO = this.settings[0];

        return "{0};{1};{2};{3};{4};{5}".format(setting.account1Nr, setting.account2Nr, setting.account3Nr, setting.account4Nr, setting.account5Nr, setting.account6Nr);        
    }

    private calculateQuantityFromDates() {
        if ((this.selectedTimeCode.expenseType === TermGroup_ExpenseType.AllowanceAbroad) || (this.selectedTimeCode.expenseType === TermGroup_ExpenseType.AllowanceDomestic)) {
            if (this.row.start && this.row.stop) {
                const days = this.row.stop.diffDays(this.row.start)+1;
                this.selectedQuantity = days ? days : 1;
            }
        }
    }

    private calculateInvoiceAmount() {
        if ((this.isPopulating)) {
            return;
        }

        if (
            (!this.selectedTimeCode) ||
            (!this.selectedTimeCode.invoiceProducts) ||
            (this.selectedTimeCode.invoiceProducts.length === 0) ||
            (!this.transferToOrder)
        ) {
            this.row.invoicedAmount = 0;
            return;
        }

        if (this.row.isSpecifiedUnitPrice) {
            if (this.row.amount && this.row.vat && !this.priceListTypeInclusiveVat) {
                this.row.invoicedAmount = this.row.amount - this.row.vat;
            }
            else if (this.row.amount) {
                this.row.invoicedAmount = this.row.amount;
            }
            else {
                this.row.invoicedAmount = 0;
            }
        }
        else {
            this.calculateInvoiceAmountFromProductPrice();
        }
    }

    private setPriceListPrice(invoiceProduct: ITimeCodeInvoiceProductDTO): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (!invoiceProduct["priceListPrice"] || this.billingUseQuantityPrices) {
            this.coreService.getProductPriceForInvoice(invoiceProduct.invoiceProductId, this.customerInvoiceId, this.getCalculationQuantity()).then((priceListPrice: number) => {
                invoiceProduct["priceListPrice"] = (priceListPrice) ? priceListPrice : invoiceProduct.invoiceProductPrice;
                deferral.resolve();
            });
        }
        else {
            deferral.resolve();
        }

        return deferral.promise;
    }

    private calculateInvoiceAmountFromProductPrice() {
        this.row.invoicedAmount = 0;
        
        if (this.customerInvoiceId) {
            const promises = [];
            this.selectedTimeCode.invoiceProducts.forEach((invoiceProduct) => {
                promises.push(this.setPriceListPrice(invoiceProduct));
            });
            this.$q.all(promises).then(() => {
                this.row.invoicedAmount = 0;
                this.selectedTimeCode.invoiceProducts.forEach((invoiceProduct) => {
                    this.row.invoicedAmount += invoiceProduct.factor * this.getCalculationQuantity() * invoiceProduct["priceListPrice"]
                });
            });
        }
        else {
            this.selectedTimeCode.invoiceProducts.forEach((invoiceProduct) => {
                this.row.invoicedAmount += invoiceProduct.factor * this.getCalculationQuantity() * invoiceProduct.invoiceProductPrice;
            });
        };
    }

    private calculateAmount() {
        if (!this.isPopulating) {            
            if (this.selectedUnitPrice && this.selectedQuantity) {
                this.row.amount = NumberUtility.parseDecimal(this.selectedUnitPrice.toString()) * this.getCalculationQuantity();
            }
            else {
                this.row.amount = 0;
            }
        }
    }

    private getCalculationQuantity(): number
    {
        if (!this.selectedQuantity)
            return 0;

        return this.isRegistrationTypeQuantity() ? NumberUtility.parseDecimal(this.selectedQuantity.toString()) : (NumberUtility.parseDecimal(this.selectedQuantity.toString()) / 60);
    }

    private populateTransaction() {  
        this.isPopulating = true;
        this.selectedTimeCode = _.find(this.timeCodes, e => e.timeCodeId === this.row.timeCodeId);
        if(this.useEmployees)
            this.selectedEmployee = _.find(this.employees, e => e.id === this.row.employeeId);
        this.selectedQuantity = this.row.quantity;
        this.selectedUnitPrice = this.row.unitPrice;
        this.isPopulating = false;
    }

    protected validate() {
        if (!this.selectedTimeCode)
            this.mandatoryFieldKeys.push("common.timecode");
        if (!this.selectedStartDate)
            this.mandatoryFieldKeys.push("common.fromdate");
        if (this.useEmployees) {
            if (!this.selectedEmployee)
                this.mandatoryFieldKeys.push("common.employee");
        }
        
        if (!this.selectedQuantity)
            this.mandatoryFieldKeys.push("common.quantity");
        if (this.commentMandatory && (!this.row.comment || this.row.comment === ""))
            this.mandatoryFieldKeys.push("common.comment");
    }

    private setupTabIndexes() {

        this.tabIndexes = new Array();
        if (this.selectedTimeCode) {

            let i = 0;
            this.tabIndexes['timecode'] = ++i;
            if (this.selectedTimeCode.stopAtDateStart && !this.isStartDateReadonly())
                this.tabIndexes['startdate'] = ++i;
            else
                this.tabIndexes['startdate'] = -1;

            if (this.selectedTimeCode.stopAtDateStart && !this.isStartTimeReadonly())
                this.tabIndexes['starttime'] = ++i;
            else
                this.tabIndexes['starttime'] = -1;

            if (this.selectedTimeCode.stopAtDateStop && !this.isStopDateReadonly())
                this.tabIndexes['stopdate'] = ++i;
            else
                this.tabIndexes['stopdate'] = -1;

            if (this.selectedTimeCode.stopAtDateStop && !this.isStopTimeReadonly())
                this.tabIndexes['stoptime'] = ++i;
            else
                this.tabIndexes['stoptime'] = -1;

            if (!this.isQuantityReadOnly())
                this.tabIndexes['quantity'] = ++i;
            else
                this.tabIndexes['quantity'] = -1;

            if (this.selectedTimeCode.stopAtPrice)
                this.tabIndexes['unitprice'] = ++i;
            else
                this.tabIndexes['unitprice'] = -1;

            if (!this.isAmountReadOnly())
                this.tabIndexes['amount'] = ++i;
            else
                this.tabIndexes['amount'] = -1;

            if (this.selectedTimeCode.stopAtVat && !this.isVatReadOnly())
                this.tabIndexes['vat'] = ++i;
            else
                this.tabIndexes['vat'] = -1;

            if (this.selectedTimeCode.stopAtComment)
                this.tabIndexes['comment'] = ++i;
            else
                this.tabIndexes['comment'] = -1;

            if (this.selectedTimeCode.stopAtAccounting)
                this.tabIndexes['accounting'] = ++i;
            else
                this.tabIndexes['accounting'] = -1;
        }
    }

    private isRegistrationTypeQuantity() {
        if (this.selectedTimeCode) {
            if (this.selectedTimeCode.registrationType === TermGroup_TimeCodeRegistrationType.Time) {                
                return false;                
            }
            else {                                
                return true;                
            }
        }        
        return true;
    }

    private initNewRow() {
        this.row = new ExpenseRowDTO();
        if (this.projectId)
            this.row.projectId = this.projectId;
        this.row.start = null;
        this.row.stop = null;
    }
}
