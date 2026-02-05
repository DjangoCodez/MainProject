import { DialogControllerBase } from "../../../../../Core/Controllers/DialogControllerBase";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IReportService } from "../../../../../Core/Services/ReportService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../../Core/Services/UrlHelperService";
import { ISmallGenericType, IAccountDTO, IPayrollPriceFormulaResultDTO } from "../../../../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "../../../../../Common/Models/smallgenerictype";
import { AccountingSettingsRowDTO } from "../../../../../Common/Models/AccountingSettingsRowDTO";
import { IPayrollService } from "../../../PayrollService";
import { AttestPayrollTransactionDTO } from "../../../../../Common/Models/AttestPayrollTransactionDTO";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";

export class AddedTransactionDialogControlller extends DialogControllerBase {

    isLoading: boolean = false;
    isPopulating: boolean = false;

    title: string = "";
    originalQuantity: number = 0;

    payrollProductsDict: ISmallGenericType[] = [];
    payrollProducts: any[];
    mandatoryFieldKeys: string[] = [];
    accountStd: IAccountDTO;
    formulaResult: IPayrollPriceFormulaResultDTO = null;
    settingTypes: SmallGenericType[];
    accountingSettings: AccountingSettingsRowDTO[] = [];
    recalculayePayrollPeriodAfterSave: boolean = true;

    // Filters
    //amountFilter: any;

    private _selectedPayrollProduct;
    get selectedPayrollProduct(): ISmallGenericType {
        return this._selectedPayrollProduct;
    }
    set selectedPayrollProduct(item: ISmallGenericType) {
        this._selectedPayrollProduct = item;
        if (this.transaction && this.selectedPayrollProduct)
            this.transaction.payrollProductId = this.selectedPayrollProduct.id;

        this.validate();
        this.tryEvaluatePayrollPriceFormulaGivenEmployeeId();
    }

    private _selectedDateFrom: Date;
    get selectedDateFrom() {
        return this._selectedDateFrom;
    }
    set selectedDateFrom(date: Date) {
        if (!date) {
            date = new Date();
        }
        this._selectedDateFrom = new Date(<any>date.toString());
        if (this.transaction)
            this.transaction.addedDateFrom = date;

        this.validate();
    }

    private _selectedDateTo: Date;
    get selectedDateTo() {
        return this._selectedDateTo;
    }
    set selectedDateTo(date: Date) {
        if (!date) {
            date = new Date();
        }
        this._selectedDateTo = new Date(<any>date.toString());
        if (this.transaction)
            this.transaction.addedDateTo = date;

        this.validate();
        this.tryEvaluatePayrollPriceFormulaGivenEmployeeId();
    }

    private _selectedQuantity: number;
    get selectedQuantity(): number {
        return this._selectedQuantity;
    }
    set selectedQuantity(value: number) {
        this._selectedQuantity = value;
        this.transaction.quantity = this._selectedQuantity;
        this.calculateTotalAmount();
    }

    private _selectedUnitPrice: number;
    get selectedUnitPrice(): number {
        return this._selectedUnitPrice;
    }
    set selectedUnitPrice(value: number) {
        this._selectedUnitPrice = value;
        this.transaction.unitPrice = this._selectedUnitPrice;
        this.calculateTotalAmount();
    }

    //@ngInject
    constructor(private $uibModalInstance,
        private payrollService: IPayrollService,
        translationService: ITranslationService,
        coreService: ICoreService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private $filter: ng.IFilterService,
        private employeeId,
        private timePeriodId,
        private ignoreEmploymentHasEnded,
        private transaction: AttestPayrollTransactionDTO) {

        super(null, translationService, coreService, notificationService, urlHelperService);

        if (!this.transaction) {
            this.transaction = new AttestPayrollTransactionDTO();
        }
        //this.amountFilter = $filter("amount");
        //this.transaction.unitPrice = this.amountFilter(this.transaction.unitPrice, 2);
        //this.transaction.quantity = this.amountFilter(this.transaction.quantity, 2);            
        //this.transaction.vatAmount = this.amountFilter(this.transaction.vatAmount, 2);
        //this.transaction.amount = this.amountFilter(this.transaction.amount, 2);

        this.originalQuantity = transaction !== null ? transaction.quantity : 0;
        this.setupLabels();
        this.loadLookups();
    }


    // EVENTS               
    protected buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }
    // LOOKUPS

    private loadPayrollProductsDict() {        
        this.payrollService.getPayrollProductsForAddedTransactionDialog(true).then((x) => {
            this.payrollProducts = x;

            var i: number = 1;
            _.forEach(x, (y: any) => {
                if (y.useInPayroll === true) {
                    this.payrollProductsDict.push({ id: y.productId, name: y.number + ' ' + y.name })
                    i += 1;
                }
            });

            this.lookupLoaded();
        });
    }

    private loadTerms() {

        var keys: string[] = [
            "common.accountingsettings.account",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.settingTypes = [];
            this.settingTypes.push(new SmallGenericType(1, terms["common.accountingsettings.account"]));
            this.lookupLoaded();
        });
    }

    private loadLookups() {
        this.startLoad();
        this.lookups = 3;
        this.isLoading = true;
        this.loadTerms();
        if (this.transaction.timePayrollTransactionId && this.transaction.timePayrollTransactionId != 0)
            this.loadTimePayrollTransactionAccountStd();
        else
            this.lookupLoaded();

        this.loadPayrollProductsDict();
    }

    protected lookupLoaded() {
        super.lookupLoaded();
        if (this.lookups == 0) {
            this.isLoading = false;
            this.readOnlyPermission = true;
            this.modifyPermission = true;

            if (this.transaction.timePayrollTransactionId)
                this.populateTransaction();
            else
                this.newTransaction();

            this.validate();
            this.setIsReadOnly();
        }
    }

    private populateTransaction() {
        this.isNew = false;
        this.isPopulating = true;
        this.selectedPayrollProduct = _.find(this.payrollProductsDict, { id: this.transaction.payrollProductId });
        this.selectedQuantity = this.transaction.quantity;
        this.selectedUnitPrice = this.transaction.unitPrice;

        if (this.transaction.addedDateFrom) {
            this.transaction.addedDateFrom = new Date(<any>this.transaction.addedDateFrom);
            this.selectedDateFrom = this.transaction.addedDateFrom;
        }
        if (this.transaction.addedDateTo) {
            this.transaction.addedDateTo = new Date(<any>this.transaction.addedDateTo);
            this.selectedDateTo = this.transaction.addedDateTo;
        }

        this.accountingSettings = [];
        var accountingSetting = new AccountingSettingsRowDTO(1);
        if (this.accountStd) {
            accountingSetting.accountDim1Nr = this.accountStd.accountDimNr;
            accountingSetting.account1Id = this.accountStd.accountId;
            accountingSetting.account1Nr = this.accountStd.accountNr;
            accountingSetting.account1Name = this.accountStd.name;
        }

        if (this.transaction.accountInternals) {
            var orderedAccountInternals = _.orderBy(this.transaction.accountInternals, ['accountDimNr'], ['asc']);

            orderedAccountInternals.forEach((accInt, index) => {
                if (index === 0) {
                    accountingSetting.accountDim2Nr = accInt.accountDimNr;
                    accountingSetting.account2Id = accInt.accountId;
                    accountingSetting.account2Nr = accInt.accountNr;
                    accountingSetting.account2Name = accInt.name;
                } else if (index === 1) {
                    accountingSetting.accountDim3Nr = accInt.accountDimNr;
                    accountingSetting.account3Id = accInt.accountId;
                    accountingSetting.account3Nr = accInt.accountNr;
                    accountingSetting.account3Name = accInt.name;
                } else if (index === 2) {
                    accountingSetting.accountDim4Nr = accInt.accountDimNr;
                    accountingSetting.account4Id = accInt.accountId;
                    accountingSetting.account4Nr = accInt.accountNr;
                    accountingSetting.account4Name = accInt.name;
                } else if (index === 3) {
                    accountingSetting.accountDim5Nr = accInt.accountDimNr;
                    accountingSetting.account5Id = accInt.accountId;
                    accountingSetting.account5Nr = accInt.accountNr;
                    accountingSetting.account5Name = accInt.name;
                } else if (index === 4) {
                    accountingSetting.accountDim6Nr = accInt.accountDimNr;
                    accountingSetting.account6Id = accInt.accountId;
                    accountingSetting.account6Nr = accInt.accountNr;
                    accountingSetting.account6Name = accInt.name;
                }
            });
        }
        this.accountingSettings.push(accountingSetting);
        this.isPopulating = false;
    }

    private newTransaction() {
        this.isNew = true;
        this.transaction.payrollProductId = 0;
        this.selectedQuantity = 1;
        this.selectedUnitPrice = 0;
        this.transaction.vatAmount = 0;
        this.transaction.amount = 0;
        this.transaction.isSpecifiedUnitPrice = false;
    }

    private setIsReadOnly() {
        //TODO?
    }

    // ACTIONS
    private setupLabels() {

        this.translationService.translate("time.time.transaction.create").then((term) => {
            this.title = term;
        });
    }

    private tryEvaluatePayrollPriceFormulaGivenEmployeeId() {

        if (this.isLoading === true || this.isPopulating === true)
            return;

        if (this.formulaResult !== null) {
            this.formulaResult = null;
            this.selectedUnitPrice = 0;
        }

        if (this.transaction.addedDateTo && this.transaction.payrollProductId > 0 && this.transaction.isSpecifiedUnitPrice === false)
            this.getEvaluatedPayrollPriceFormulaGivenEmployee();
    }

    private calculateTotalAmount() {
        if (this.selectedUnitPrice && this.selectedQuantity) {
            this.transaction.amount = NumberUtility.parseDecimal(this.selectedUnitPrice.toString()) * NumberUtility.parseDecimal(this.selectedQuantity.toString());
            //this.transaction.amount = this.amountFilter(this.transaction.amount, 2);                                                                        
        }
    }

    protected validate() {
        if (this.transaction) {
            if (!this.selectedPayrollProduct) {
                this.mandatoryFieldKeys.push("time.time.payrollproduct.payrollproduct");
            }
            if (!this.selectedDateFrom) {
                this.mandatoryFieldKeys.push("common.from");
            }
            if (!this.selectedDateTo) {
                this.mandatoryFieldKeys.push("common.to");
            }
            if (this.selectedDateFrom && this.selectedDateTo && this.selectedDateFrom > this.selectedDateTo) {
                this.validationErrorKeys.push("error.invaliddaterange");
            }
            if (this.transaction.timePayrollTransactionId != 0 && this.transaction.attestStateInitial == false) {
                this.validationErrorKeys.push("time.time.transaction.atteststate.modifynotvalid");
            }
        }
    }

    private save() {

        var product = _.find(this.payrollProducts, { productId: this.selectedPayrollProduct.id });

        if (this.transaction != null && this.transaction.isPayrollProductChainMainParent === true && this.selectedQuantity != this.originalQuantity) {
            let keys: string[] = [
                "time.time.transaction.parent.quantitychangedquestion",
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog("", terms["time.time.transaction.parent.quantitychangedquestion"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNoCancel);
                modal.result.then(val => {
                    if (val === true) {
                        this.saveTransaction(true);
                    } else {
                        this.saveTransaction(false);
                    }
                }, (reason) => {
                    if ("cancel") {
                        //do nothing
                    }
                });
            });
        }
        else if (product && product.isAbsence) {
            let keys: string[] = [
                "time.payroll.payrollcalculation.added.isabsencequestion",
                "core.warning"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.payroll.payrollcalculation.added.isabsencequestion"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val === true) {
                        this.saveTransaction(false);
                    }
                }, (reason) => {
                    if ("cancel") {
                        //do nothing
                    }
                });
            });
        }
        else {
            this.saveTransaction(false);
        }
    }

    private saveTransaction(updateChildren: boolean) {
        this.transaction.updateChildren = updateChildren;
        this.startSave();
        //this.transaction.addedDateFrom = this.transaction.addedDateFrom.toServerDate();
        //this.transaction.addedDateTo = this.transaction.addedDateTo.toServerDate();            
        this.transaction.unitPrice = NumberUtility.parseDecimal(this.transaction.unitPrice.toString())
        this.transaction.quantity = NumberUtility.parseDecimal(this.transaction.quantity.toString())
        this.transaction.vatAmount = NumberUtility.parseDecimal(this.transaction.vatAmount.toString())
        this.transaction.amount = NumberUtility.parseDecimal(this.transaction.amount.toString())

        this.payrollService.saveAddedTransaction(this.transaction, this.accountingSettings, this.employeeId, this.timePeriodId, this.ignoreEmploymentHasEnded).then((result) => {
            if (result.success) {
                this.$uibModalInstance.close(this.recalculayePayrollPeriodAfterSave);
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected initDelete() {

        if (this.transaction !== null && this.transaction.isPayrollProductChainMainParent === true) {
            let keys: string[] = [
                "time.time.transaction.parent.deletequestion",
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog("", terms["time.time.transaction.parent.deletequestion"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNoCancel);
                modal.result.then(val => {
                    if (val === true) {
                        this.deleteTransaction(true);
                    } else {
                        this.deleteTransaction(false);
                    }
                }, (reason) => {
                    if ("cancel") {
                        //do nothing
                    }
                });
            });
        }
        else {
            let keys: string[] = [
                "core.warning",
                "core.deletewarning"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["core.deletewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val === true) {
                        this.deleteTransaction(false);
                    }
                });
            });
        }
    }

    private deleteTransaction(deleteChilds: boolean) {
        this.payrollService.deleteTimePayrollTransaction(this.transaction.timePayrollTransactionId, deleteChilds).then((result) => {
            if (result.success) {
                this.$uibModalInstance.close(true);
            } else {
                this.failedDelete(result.errorMessage);
            }
        }, error => {
            this.failedDelete(error.message);
        });
    }

    private getEvaluatedPayrollPriceFormulaGivenEmployee() {
        this.payrollService.getEvaluatedFormulaGivenEmployee(this.transaction.addedDateTo, this.employeeId, this.transaction.payrollProductId).then((x) => {
            this.formulaResult = x;
            if (this.formulaResult != null && this.formulaResult.amount > 0) {
                this.selectedUnitPrice = this.formulaResult.amount;
            }
            else {
                this.formulaResult = null;
                this.selectedUnitPrice = 0;
            }
        });
    }

    private loadTimePayrollTransactionAccountStd() {
        this.payrollService.getTimePayrollTransactionAccountStd(this.transaction.timePayrollTransactionId).then((x) => {
            this.accountStd = x;
            this.lookupLoaded();
        });
    }
}
