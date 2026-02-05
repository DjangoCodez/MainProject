import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../../Core/Services/MessagingService";
import { ICoreService } from "../../../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../../../Core/Services/UrlHelperService";
import { INotificationService } from "../../../../../../Core/Services/NotificationService";
import { IInventoryService } from "../../../../../../Shared/Economy/Inventory/InventoryService";
import { IAccountingService } from "../../../../Accounting/AccountingService";
import { InventoryAdjustFunctions, SOEMessageBoxImage } from "../../../../../../Util/Enumerations";
import { CalendarUtility } from "../../../../../../Util/CalendarUtility";
import { AccountingRowDTO } from "../../../../../../Common/Models/AccountingRowDTO";
import { Constants } from "../../../../../../Util/Constants";
import { TermGroup_InventoryLogType, InventoryAccountType, AccountingRowType, SoeEntityState } from "../../../../../../Util/CommonEnumerations";

export class InventoryAdjustmentController {
    // Terms
    private terms: any;

    private dialogLabel: string;
    private amount: number = 0;
    private adjustmentDate: Date;
    private note: string;
    private accountingRows: AccountingRowDTO[];
    private isNew: boolean = true;
    private inventoryLogType: number;
    private voucherSeries: any;
    private voucherSeriesTypeId: number;
    private isDispose: boolean = false;
    private isMissingStandardAccounts: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        private inventoryService: IInventoryService,
        private accountingService: IAccountingService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private inventoryId: number,
        private adjustmentType: number,
        private purchaseDate: Date,
        private noteText: string,
        private purchaseAmount: number,
        private accWriteOffAmount: number,
        private accountingSettings: any[],
        private inventoryBaseAccounts: any[]) {

        this.messagingService.subscribe(Constants.EVENT_REGENERATE_ACCOUNTING_ROWS, (x) => {
            this.createAccountingRows();
        }, this.$scope);

        this.$q.all([
            this.loadTerms(), this.loadVoucherSeriesTypes()]).then(() => {
                this.setDialogLabel();
                this.amount = 0;
                if (this.adjustmentType == InventoryAdjustFunctions.Discarded)
                    this.amount = this.purchaseAmount;
                this.adjustmentDate = CalendarUtility.getDateToday();
                this.note = this.dialogLabel + ", " + this.noteText;
                this.isDispose = this.adjustmentType == InventoryAdjustFunctions.Sold.valueOf() || adjustmentType == InventoryAdjustFunctions.Discarded.valueOf();
            }).then(() => { this.createAccountingRows(); });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "economy.inventory.inventories.overwriteoff",
            "economy.inventory.inventories.underwriteoff",
            "economy.inventory.inventories.writeup",
            "economy.inventory.inventories.writedown",
            "economy.inventory.inventories.discarded",
            "economy.inventory.inventories.sold",
            "common.amount",
            "common.note",
            "economy.inventory.inventories.adjustmentdate",
            "core.warning",
            "core.info",
            "economy.inventory.inventories.adjustment.amountmissing",
            "economy.inventory.inventories.adjustment.datemissing",
            "economy.inventory.inventories.adjustment.voucherseriemissing",
            "economy.inventory.inventories.adjustment.purchasedatecomparisonerror",
            "economy.inventory.inventories.adjustment.saveadjustmentnotsucceedederror",
            "economy.inventory.inventories.adjustment.standardaccountsmissing"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadVoucherSeriesTypes(): ng.IPromise<any> {
        return this.accountingService.getVoucherSeriesTypes().then((x) => {
            this.voucherSeries = x;
        });
    }

    private setDialogLabel() {
        switch (this.adjustmentType) {
            case InventoryAdjustFunctions.OverWriteOff:
                this.dialogLabel = this.terms["economy.inventory.inventories.overwriteoff"];
                this.inventoryLogType = TermGroup_InventoryLogType.OverWriteOff;
                break;
            case InventoryAdjustFunctions.UnderWriteOff:
                this.dialogLabel = this.terms["economy.inventory.inventories.underwriteoff"];
                this.inventoryLogType = TermGroup_InventoryLogType.UnderWriteOff;
                break;
            case InventoryAdjustFunctions.WriteUp:
                this.dialogLabel = this.terms["economy.inventory.inventories.writeup"];
                this.inventoryLogType = TermGroup_InventoryLogType.WriteUp;
                break;
            case InventoryAdjustFunctions.WriteDown:
                this.dialogLabel = this.terms["economy.inventory.inventories.writedown"];
                this.inventoryLogType = TermGroup_InventoryLogType.WriteDown;
                break;
            case InventoryAdjustFunctions.Discarded:
                this.dialogLabel = this.terms["economy.inventory.inventories.discarded"];
                this.inventoryLogType = TermGroup_InventoryLogType.Discarded;
                break;
            case InventoryAdjustFunctions.Sold:
                this.dialogLabel = this.terms["economy.inventory.inventories.sold"];
                this.inventoryLogType = TermGroup_InventoryLogType.Sold;
                break;
        }
    }

    private createAccountingRows() {
        this.accountingRows = [];

        switch (this.adjustmentType) {
            case InventoryAdjustFunctions.OverWriteOff:
                this.createAccountingRow(InventoryAccountType.OverWriteOff, 0, true, this.amount);
                this.createAccountingRow(InventoryAccountType.AccOverWriteOff, 0, false, this.amount);
                break;
            case InventoryAdjustFunctions.UnderWriteOff:
                this.createAccountingRow(InventoryAccountType.AccOverWriteOff, 0, true, this.amount);
                this.createAccountingRow(InventoryAccountType.OverWriteOff, 0, false, this.amount);
                break;
            case InventoryAdjustFunctions.WriteUp:
                this.createAccountingRow(InventoryAccountType.AccWriteUp, 0, true, this.amount);
                this.createAccountingRow(InventoryAccountType.WriteUp, 0, false, this.amount);
                break;
            case InventoryAdjustFunctions.WriteDown:
                this.createAccountingRow(InventoryAccountType.WriteDown, 0, true, this.amount);
                this.createAccountingRow(InventoryAccountType.AccWriteDown, 0, false, this.amount);
                break;
            case InventoryAdjustFunctions.Discarded:
                this.createAccountingRow(InventoryAccountType.Inventory, 0, false, this.purchaseAmount);
                this.createAccountingRow(InventoryAccountType.AccWriteOff, 0, true, this.accWriteOffAmount);

                var diffAmount: number = this.purchaseAmount - this.accWriteOffAmount;

                if (diffAmount < 0)
                    this.createAccountingRow(InventoryAccountType.SalesProfit, 0, false, Math.abs(diffAmount));
                else
                    if (diffAmount > 0)
                        this.createAccountingRow(InventoryAccountType.SalesLoss, 0, true, Math.abs(diffAmount));

                break;
            case InventoryAdjustFunctions.Sold:                
                this.createAccountingRow(InventoryAccountType.Sales, 0, true, this.amount);
                this.createAccountingRow(InventoryAccountType.Inventory, 0, false, this.purchaseAmount);
                this.createAccountingRow(InventoryAccountType.AccWriteOff, 0, true, this.accWriteOffAmount);

                var salesProfitAmount: number = this.amount - (this.purchaseAmount - this.accWriteOffAmount);
                
                if (salesProfitAmount > 0)
                    this.createAccountingRow(InventoryAccountType.SalesProfit, 0, false,salesProfitAmount);
                else
                    if (salesProfitAmount < 0)
                        this.createAccountingRow(InventoryAccountType.SalesLoss, 0, true, salesProfitAmount);                        
                break;
        }

        this.$timeout(() => {
            this.$scope.$broadcast('setRowItemAccountsOnAllRows');
            this.$scope.$broadcast('rowsAdded');
        });

        if (this.isMissingStandardAccounts && this.isNew)
            this.notificationService.showDialogEx(this.terms["core.info"], this.terms["economy.inventory.inventories.adjustment.standardaccountsmissing"], SOEMessageBoxImage.Information);

        this.isNew = false;
    }
    
    private createAccountingRow(type: InventoryAccountType, accountId: number, isDebitRow: boolean, amount: number) {        
                
        var row = new AccountingRowDTO();
        row.type = AccountingRowType.AccountingRow;
        row.invoiceAccountRowId = 0;
        row.tempRowId = 0;

        row.amountCurrency = isDebitRow ? Math.abs(amount) : -Math.abs(amount);
        row.debitAmountCurrency = isDebitRow ? Math.abs(amount) : 0;
        row.creditAmountCurrency = isDebitRow ? 0 : Math.abs(amount);

        row.amount = isDebitRow ? Math.abs(amount) : -Math.abs(amount);
        row.debitAmount = isDebitRow ? Math.abs(amount) : 0;
        row.creditAmount = isDebitRow ? 0 : Math.abs(amount);

        row.quantity = null;        
        row.isCreditRow = !isDebitRow;
        row.isDebitRow = isDebitRow;
        row.isVatRow = false;
        row.isContractorVatRow = false;
        row.isInterimRow = false;
        row.state = SoeEntityState.Active;
        row.isModified = false;
        row.text = this.note;

        // Set accounts
        var rowItem = _.filter(this.accountingSettings, x => x.type == type)[0];
        var baseAccount = _.filter(this.inventoryBaseAccounts, x => x.id == type)[0];

        if (rowItem) {
            row.dim1Id = rowItem.account1Id && rowItem.account1Id != 0 ? rowItem.account1Id : _.toNumber(baseAccount.name);
            row.dim2Id = rowItem.account2Id;
            row.dim3Id = rowItem.account3Id;
            row.dim4Id = rowItem.account4Id;
            row.dim5Id = rowItem.account5Id;
            row.dim6Id = rowItem.account6Id;
        } else {
            row.dim1Id = baseAccount != null ? _.toNumber(baseAccount.name) : accountId;
            row.dim1Nr = '';
            row.dim1Name = '';
            row.dim1Mandatory = true;
            row.dim2Id = 0;
            row.dim2Nr = '';
            row.dim2Name = '';
            row.dim3Id = 0;
            row.dim3Nr = '';
            row.dim3Name = '';
            row.dim4Id = 0;
            row.dim4Nr = '';
            row.dim4Name = '';
            row.dim5Id = 0;
            row.dim5Nr = '';
            row.dim5Name = '';
            row.dim6Id = 0;
            row.dim6Nr = '';
            row.dim6Name = '';
        }

        row.rowNr = this.accountingRows.length + 1;
        this.accountingRows.push(row);

        if (row.dim1Id == 0)
            this.isMissingStandardAccounts = true;
    }

    buttonCancelClick() {
        this.close(null);
    }

    buttonOkClick() {

        var msg: string = '';

        if (this.amount == 0)
            msg += this.terms["economy.inventory.inventories.adjustment.amountmissing"] + "\n";
        if (this.adjustmentDate == null)
            msg += this.terms["economy.inventory.inventories.adjustment.datemissing"] + "\n";
        if (this.isDispose && (this.voucherSeriesTypeId == null || this.voucherSeriesTypeId == 0))
            msg += this.terms["economy.inventory.inventories.adjustment.voucherseriemissing"] + "\n";
        if (this.adjustmentDate < this.purchaseDate)
            msg += this.terms["economy.inventory.inventories.adjustment.purchasedatecomparisonerror"] + "\n";

        if (msg != '') {
            this.notificationService.showDialogEx(this.terms["core.warning"], msg, SOEMessageBoxImage.Error);
        }
        else {

            this.inventoryService.saveAdjustment(this.inventoryId, this.inventoryLogType, this.voucherSeriesTypeId, this.amount, this.adjustmentDate, this.note, this.accountingRows).then((saveResult) => {
                if (saveResult.success) {
                    saveResult.decimalValue = this.amount;
                    this.$uibModalInstance.close(saveResult);
                }
                else {
                    this.notificationService.showDialogEx(this.terms["economy.inventory.inventories.adjustment.saveadjustmentnotsucceedederror"], saveResult.errorMessage, SOEMessageBoxImage.Error);
                }
            });
        }
    }

    close(result: any) {
        if (!result) {
            this.$uibModalInstance.dismiss('cancel');
        }
        else {
            this.$uibModalInstance.close({ rows: result });
        }
    }
}