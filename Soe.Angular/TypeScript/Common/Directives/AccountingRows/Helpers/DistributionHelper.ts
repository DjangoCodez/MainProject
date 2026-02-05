import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { Constants } from "../../../../Util/Constants";
import { StringUtility } from "../../../../Util/StringUtility";
import { AccountingRowDTO } from "../../../Models/AccountingRowDTO";
import { AccountDistributionEntryDTO } from "../../../Models/AccountDistributionEntryDTO";
import { IAccountDistributionHeadDTO, IAccountDistributionRowDTO, IAccountingRowDTO } from "../../../../Scripts/TypeLite.Net4";
import { IAccountingService } from "../../../../Shared/Economy/Accounting/AccountingService";
import { AccountingRowsContainers, SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize } from "../../../../Util/Enumerations";
import { AccountDistributionHeadDTO } from "../../../Models/AccountDistributionHeadDTO";
import { WildCard, SoeAccountDistributionType, TermGroup_AccountDistributionCalculationType, TermGroup_AccountDistributionTriggerType, TermGroup_AccountDistributionRegistrationType } from "../../../../Util/CommonEnumerations";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { SelectAccountDistributionController } from "../../../Dialogs/SelectAccountDistribution/SelectAccountDistributionController";

export class DistributionHelper {
    private accountingRows: AccountingRowDTO[];
    private selectedAccountDistribution: IAccountDistributionHeadDTO;
    private matches: IAccountDistributionHeadDTO[];
    private existingEntryRows: AccountDistributionEntryDTO[];
    private parentGuid: string;
    private accountDistributionHeadName: string;

    constructor(private accountDistributions: IAccountDistributionHeadDTO[],
        private accountDistributionsForImport: IAccountDistributionHeadDTO[],
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private messagingService: IMessagingService,
        private accountingService: IAccountingService,
        private addRow: () => { row, rowIndex },
        private deleteRow: Function,
        private allChangesDone: Function,
        private container: AccountingRowsContainers,        
        private useAutomaticAccountDistribution: boolean,
        private $q: ng.IQService,        
        private $scope: ng.IScope,
        private $uibModal,
        private urlHelperService: IUrlHelperService) {

        this.$scope.$on('accountDistributionName', (e, a) => {            
            this.accountDistributionHeadName = a;
        });

    }

    public setAccountingRows(accountingRows: AccountingRowDTO[]) {
        this.accountingRows = accountingRows;
    }

    public checkAccountDistribution(row: AccountingRowDTO, parentGuid: string): boolean {    
        this.parentGuid = parentGuid;
        
        if (this.accountDistributions.length === 0 && !row.isAccrualAccount)
            return false;

        if (row.debitAmount == 0 && row.creditAmount == 0)
            return false;
        
        // Check existing account distribution        
        if (row.accountDistributionHeadId) {

            this.selectedAccountDistribution = _.find(this.accountDistributions, a => a.accountDistributionHeadId === row.accountDistributionHeadId);

            if (this.selectedAccountDistribution && this.selectedAccountDistribution.type == SoeAccountDistributionType.Auto) {
                // This row is involved in a previous account distribution                
                if (_.filter(this.accountingRows, r => r.accountDistributionHeadId == row.accountDistributionHeadId && (r.invoiceRowId <= row.invoiceRowId || (r.parentRowId != row.tempInvoiceRowId))).length === 1) {
                    
                    // This is a parent row, if changed ask if generate                                                                     
                    var keys: string[] = [
                        "economy.accounting.distribution.automaticdistribution.askregenerate.title",
                        "economy.accounting.distribution.automaticdistribution.askregenerate.message"
                    ];

                    this.translationService.translateMany(keys).then((terms) => {
                        var modal = this.notificationService.showDialog(terms["economy.accounting.distribution.automaticdistribution.askregenerate.title"], terms["economy.accounting.distribution.automaticdistribution.askregenerate.message"].format(this.selectedAccountDistribution.name), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo, SOEMessageBoxSize.Medium, true);
                        modal.result.then(val => {
                            if (val) {
                                this.deleteChildRows(row);

                                // Use account distribution
                                //this.generateAccountDistribution(row);
                                this.doAccountDistribution(row);
                            } else {
                                this.selectedAccountDistribution = null;
                                this.allChangesDone(true);
                            }
                        });
                    });
                    return true;
                }
                else
                    return false;

            } else if (this.selectedAccountDistribution && this.selectedAccountDistribution.type == SoeAccountDistributionType.Period &&
                this.selectedAccountDistribution.triggerType == TermGroup_AccountDistributionTriggerType.Registration) {
                // This is a period account distribution row, if changed ask if generate                
                var registrationType: number = 0;
                var sourceId: number = 0;

                if (this.container == AccountingRowsContainers.Voucher) {
                    registrationType = TermGroup_AccountDistributionRegistrationType.Voucher;
                    sourceId = row.voucherHeadId;
                }
                else if (this.container == AccountingRowsContainers.SupplierInvoice) {
                    registrationType = TermGroup_AccountDistributionRegistrationType.SupplierInvoice
                    sourceId = row.invoiceId;
                }
                else if (this.container == AccountingRowsContainers.CustomerInvoice) {
                    registrationType = TermGroup_AccountDistributionRegistrationType.CustomerInvoice;
                    sourceId = row.invoiceId;
                }

                if (!sourceId || sourceId == 0) {
                    this.doAccountDistribution(row);
                    return true;
                }                    

                this.accountingService.getAccountDistributionEntriesForSource(row.accountDistributionHeadId, registrationType, sourceId).then((x) => {       
                    this.existingEntryRows = x;

                    var transferredRows: boolean = this.existingEntryRows.filter(i => i.voucherHeadId != null).length > 0;

                    const keys: string[] = [
                        "economy.accounting.distribution.perioddistribution",
                        "economy.accounting.distribution.perioddistribution.warning",
                        "economy.accounting.distribution.perioddistribution.voucherexistsmessage",
                        "economy.accounting.distribution.perioddistribution.replacemessage"
                    ];

                    if (transferredRows) {
                        this.translationService.translateMany(keys).then((terms) => {
                            var modal = this.notificationService.showDialog(terms["economy.accounting.distribution.perioddistribution"], terms["economy.accounting.distribution.perioddistribution.voucherexistsmessage"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Small, true);
                            modal.result.then(() => {
                                this.selectedAccountDistribution = null;
                                this.allChangesDone(true);
                                return false;
                            });
                        });
                    }
                    else {
                        this.translationService.translateMany(keys).then((terms) => {
                            var modal = this.notificationService.showDialog(terms["economy.accounting.distribution.perioddistribution"], terms["economy.accounting.distribution.perioddistribution.replacemessage"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel, SOEMessageBoxSize.Medium, true);
                            modal.result.then(ok => {                               
                                this.doAccountDistribution(row);
                                return true;
                            },
                                cancel => {                                    
                                    this.allChangesDone(true);
                                    return false;
                                }
                            );
                        });
                    }

                });

            } else {
                // This is a child row (has been generated from an account distribution)
                // Do not generate anything from this row                
                return false;
            }
        }
        else {                    
            return this.doAccountDistribution(row);
            
        }
    }

    public deleteAccountDistributionEntries(row: any) {

    }

    private doAccountDistribution(row: AccountingRowDTO) {
        // Copy all distributions
        this.matches = [];
        this.accountDistributions.forEach(ad => this.matches.push(ad));
        
        // Match on date
        if (row.date) { 
            var temp = [];
            this.matches = this.matches.filter(a => (a.startDate == null || row.date.date() >= a.startDate.date()) &&
                (a.endDate == null || row.date.date() <= a.endDate.date()));
            if (this.matches.length === 0 && !row.isAccrualAccount)
                return false;
        }

        // Match on accounts
        this.matches = this.matches.filter(match => {
            // Dim 1
            if (!this.matchAccount(match.dim1Expression, row.dim1Nr)) {
                return false;
            }
            // Dim 2
            if (match.dim2Expression != null && !this.matchAccount(match.dim2Expression, row.dim2Nr)) {
                return false;
            }
            // Dim 3
            if (match.dim3Expression != null && !this.matchAccount(match.dim3Expression, row.dim3Nr)) {
                return false;
            }
            // Dim 4
            if (match.dim4Expression != null && !this.matchAccount(match.dim4Expression, row.dim4Nr)) {
                return false;
            }
            // Dim 5
            if (match.dim5Expression != null && !this.matchAccount(match.dim5Expression, row.dim5Nr)) {
                return false;
            }
            // Dim 6
            if (match.dim6Expression != null && !this.matchAccount(match.dim6Expression, row.dim6Nr)) {
                return false;
            }

            return true;
        });

        if (this.matches.length === 0 && !row.isAccrualAccount)
            return false;

        // Match on Amount
        var amount = Math.abs(row.debitAmount - row.creditAmount);
        // Loop through remaining matches where an amount condition is specified
        this.matches = this.matches.filter(match => {
            if (match.amount === 0)
                return true;

            switch (match.amountOperator) {
                case WildCard.LessThan:
                    if (amount >= match.amount)
                        return false;
                    break;
                case WildCard.LessThanOrEquals:
                    if (amount > match.amount)
                        return false;
                    break;
                case WildCard.Equals:
                    if (amount !== match.amount)
                        return false;
                    break;
                case WildCard.GreaterThan:
                    if (amount <= match.amount)
                        return false;
                    break;
                case WildCard.GreaterThanOrEquals:
                    if (amount < match.amount)
                        return false;
                    break;
                case WildCard.NotEquals:
                    if (amount === match.amount)
                        return false;
                    break;
            }

            return true;
        });

        if (this.matches.length === 0 && !row.isAccrualAccount)
            return false;

        var periodAccountDistributions = this.matches.filter(match => match.type === SoeAccountDistributionType.Period);
        if (periodAccountDistributions.length > 0 || row.isAccrualAccount) {
            this.selectedAccountDistribution = periodAccountDistributions[0];            
            this.messagingService.publish(Constants.EVENT_SELECT_ACCOUNTDISTRIBUTION_DIALOG, this.parentGuid);
            this.showAccountDistributionPeriodDialog(row, periodAccountDistributions);
            return true;
        } else {
            this.selectedAccountDistribution = _.find(this.matches, match => match.type == SoeAccountDistributionType.Auto);
            if (this.selectedAccountDistribution && this.selectedAccountDistribution.type == SoeAccountDistributionType.Auto) {
                if (this.useAutomaticAccountDistribution)
                    this.generateAccountDistribution(row);
                else {
                    var keys: string[] = [
                        "economy.accounting.distribution.automaticdistribution.askgenerate.title",
                        "economy.accounting.distribution.automaticdistribution.askgenerate.message"
                    ];

                    this.translationService.translateMany(keys).then((terms) => {
                        var modal = this.notificationService.showDialog(terms["economy.accounting.distribution.automaticdistribution.askgenerate.title"], terms["economy.accounting.distribution.automaticdistribution.askgenerate.message"].format(this.selectedAccountDistribution.name), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo, SOEMessageBoxSize.Medium, true);
                        modal.result.then(val => {
                            if (val) {
                                // Use account distribution
                                this.generateAccountDistribution(row);
                            }
                            else {
                                // Don't use account distribution
                                this.allChangesDone(true);//NOTE: Checkinventorytrigger does nothing right now, so this is the correct behaviour for now.
                            }
                        });
                    });
                }
            }
        }
    }

    public checkDeleteAccountDistribution(row: AccountingRowDTO): ng.IPromise<any> {
        var deferral = this.$q.defer();

        // Check if row is parent to any account distrubution rows
        //parentRow is not working, have to be done another way
        //var parentRowId: number = (row.invoiceRowId ? row.invoiceRowId : row.tempInvoiceRowId);

        this.selectedAccountDistribution = _.find(this.accountDistributions, dist => dist.accountDistributionHeadId == row.accountDistributionHeadId);

        // Fallback, check distributions for import not connected to current container
        if (!this.selectedAccountDistribution)
            this.selectedAccountDistribution = _.find(this.accountDistributionsForImport, dist => dist.accountDistributionHeadId == row.accountDistributionHeadId);

        if (this.selectedAccountDistribution && this.selectedAccountDistribution.type == SoeAccountDistributionType.Auto) {
            //check for automatic distribution
            //_.filter(this.accountingRows, r => r.accountDistributionHeadId == row.accountDistributionHeadId && r.invoiceRowId <= row.invoiceRowId).length == 1
            if (row.accountDistributionHeadId && _.filter(this.accountingRows, r => r.accountDistributionHeadId == row.accountDistributionHeadId && (r.invoiceRowId <= row.invoiceRowId) || r.tempRowId == row.tempInvoiceRowId).length > 0) {
            
                var keys: string[] = [
                    "economy.accounting.distribution.automaticdistribution.deleterowwarning.title",
                    "economy.accounting.distribution.automaticdistribution.deleterowwarning.message",
                    "economy.accounting.distribution.automaticdistribution.deleterowwarning.messageexisting",
                    "economy.accounting.distribution.automaticdistribution.deleterowwarning.messagerow2"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    var distribution = _.find(this.accountDistributions, a => a.accountDistributionHeadId === row.accountDistributionHeadId);
                    var text: string;
                    if (distribution)
                        text = terms["economy.accounting.distribution.automaticdistribution.deleterowwarning.messageexisting"].format(distribution.name);
                    else
                        text = terms["economy.accounting.distribution.automaticdistribution.deleterowwarning.message"];
                    text = text + "\n" + terms["economy.accounting.distribution.automaticdistribution.deleterowwarning.messagerow2"];

                    var modal = this.notificationService.showDialog(terms["economy.accounting.distribution.automaticdistribution.deleterowwarning.title"], text, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNoCancel);
                    modal.result.then(val => {
                        if (val === true)
                            this.deleteChildRows(row);

                        deferral.resolve(val);  // Yes/No
                    }, (reason) => {
                        deferral.resolve(null); // Cancel
                    });
                });
            }
            else
                this.deleteRow(row)
        } else if (this.selectedAccountDistribution && this.selectedAccountDistribution.type == SoeAccountDistributionType.Period &&
            this.selectedAccountDistribution.triggerType == TermGroup_AccountDistributionTriggerType.Registration) {
            
            // This is a period account distribution row, if removed inform the user                
            var registrationType: number = 0;
            var sourceId: number = 0;

            if (this.container == AccountingRowsContainers.Voucher) {
                registrationType = TermGroup_AccountDistributionRegistrationType.Voucher;
                sourceId = row.voucherHeadId;
            }
            else if (this.container == AccountingRowsContainers.SupplierInvoice) {
                registrationType = TermGroup_AccountDistributionRegistrationType.SupplierInvoice
                sourceId = row.invoiceId;
            }
            else if (this.container == AccountingRowsContainers.CustomerInvoice) {
                registrationType = TermGroup_AccountDistributionRegistrationType.CustomerInvoice;
                sourceId = row.invoiceId;
            }

            this.accountingService.getAccountDistributionEntriesForSource(row.accountDistributionHeadId, registrationType, sourceId).then((x) => {
                this.existingEntryRows = x;
                var transferredRows: boolean = this.existingEntryRows.filter(i => i.voucherHeadId != null).length > 0;

                const keys: string[] = [
                    "economy.accounting.distribution.perioddistribution",
                    "economy.accounting.distribution.perioddistribution.warning",
                    "economy.accounting.distribution.perioddistribution.deleteaccountrow.voucherexistsmessage",
                    "economy.accounting.distribution.perioddistribution.deleteaccountrow.deletemessage"
                ];                
                
                this.translationService.translateMany(keys).then((terms) => {
                    var message: string = terms["economy.accounting.distribution.perioddistribution.deleteaccountrow.deletemessage"];
                    if (transferredRows)
                        message = terms["economy.accounting.distribution.perioddistribution.deleteaccountrow.voucherexistsmessage"];

                    var modal = this.notificationService.showDialog(terms["economy.accounting.distribution.perioddistribution"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo, SOEMessageBoxSize.Small, true);
                    modal.result.then(
                        val => { if (val === true) deferral.resolve(val); },
                        (reason) => { deferral.resolve(null); }
                    );
                });
                                

            });
        }
        else {
            deferral.resolve(false);
        }

        return deferral.promise;
    }

    private matchAccount(expression: string, accountNr: string): boolean {
        // If expression is empty, entered value must also be empty
        if (!expression && accountNr)
            return false;

        if (!accountNr)
            return false;

        var regEx = new RegExp(StringUtility.WildCardToRegEx(expression));
        return regEx.test(accountNr);
    }

    private showAccountDistributionPeriodDialog(row: AccountingRowDTO, periodAccountDistributions?: IAccountDistributionHeadDTO[]) {
        const keys: string[] = [
            "economy.accounting.distribution.perioddistribution.match",
            "economy.accounting.distribution.perioddistribution.selectdistribution",
            "economy.accounting.distribution.perioddistribution.match.questionplusnbrofperiods",
            "economy.accounting.distribution.perioddistribution.selectdistributionorcreatenew",
            "economy.accounting.distribution.perioddistribution.newtemplate",
            "economy.accounting.distribution.perioddistribution.createnew"
        ];

        this.translationService.translateMany(keys).then((terms) => {

            var message: string = !row.isAccrualAccount ? terms["economy.accounting.distribution.perioddistribution.match.questionplusnbrofperiods"].format(this.selectedAccountDistribution.name) : "";
            var size: SOEMessageBoxSize = SOEMessageBoxSize.Medium;

            if (periodAccountDistributions.length > 1 && !row.isAccrualAccount) {
                message = terms["economy.accounting.distribution.perioddistribution.selectdistribution"];
            }
            else
                if (row.isAccrualAccount) {
                    message = periodAccountDistributions.length >= 1 ? terms["economy.accounting.distribution.perioddistribution.selectdistributionorcreatenew"] : terms["economy.accounting.distribution.perioddistribution.createnew"];
                    size = SOEMessageBoxSize.Large;

                    var emptyDistributionHead = new AccountDistributionHeadDTO();
                    emptyDistributionHead.accountDistributionHeadId = 0;
                    emptyDistributionHead.name = terms["economy.accounting.distribution.perioddistribution.newtemplate"];                    
                    periodAccountDistributions.unshift(emptyDistributionHead);
                }

            this.selectedAccountDistribution = periodAccountDistributions[0];

            var modal = this.showAccountDistributionPeriodDialogEx(terms["economy.accounting.distribution.perioddistribution.match"], message, size, row, this.selectedAccountDistribution, periodAccountDistributions, this.container);
            modal.result.then(val => {
                this.accountDistributionPeriodDialog_Closed(val, row);
            });
        });
    }

    private showAccountDistributionPeriodDialogEx(title: string, message: string, size: SOEMessageBoxSize, rowItem: AccountingRowDTO, selectedAccountDistribution: IAccountDistributionHeadDTO, periodAccountDistributions?: IAccountDistributionHeadDTO[], container?: number) {
        return this.$uibModal.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectAccountDistribution", "SelectAccountDistributionDialog.html"),
            controller: SelectAccountDistributionController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: CoreUtility.getSOEMessageBoxSizeString(size),
            resolve: {
                translationService: () => { return this.translationService },
                text: () => { return message },
                title: () => { return title },
                rowItem: () => { return rowItem },
                selectedAccountDistribution: () => { return selectedAccountDistribution },
                periodAccountDistributions: () => { return periodAccountDistributions },
                container: () => { return container },
                accountDistributionHeadName: () => { return this.accountDistributionHeadName },
            }
        });
    }

    private accountDistributionPeriodDialog_Closed(data, row: AccountingRowDTO) {

        if (data.result) {
            // Get selected distribution
            var head = data.distributionHead;
            // Set distribution and number of periods on row (will be used when row is saved)
            row.accountDistributionHeadId = head.accountDistributionHeadId;
            row.accountDistributionNbrOfPeriods = data.nbrOfPeriods;
            row.accountDistributionStartDate = data.startDate;
        }

        this.selectedAccountDistribution = _.find(this.matches, match => match.type == SoeAccountDistributionType.Auto);
        if (this.selectedAccountDistribution && this.selectedAccountDistribution.type == SoeAccountDistributionType.Auto) {
            if (this.useAutomaticAccountDistribution)
                this.generateAccountDistribution(row);
            else {
                const keys: string[] = [
                    "economy.accounting.distribution.automaticdistribution.match",
                    "economy.accounting.distribution.automaticdistribution.match.question"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    var modal = this.notificationService.showDialog(terms["economy.accounting.distribution.automaticdistribution.match"], terms["economy.accounting.distribution.automaticdistribution.match.question"].format(this.selectedAccountDistribution.name), SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo, SOEMessageBoxSize.Medium, true);
                    modal.result.then(val => {
                        this.askAccountDistributionDialogClosed(val, row);
                    });
                });
            }
        } else {
            this.allChangesDone(false);
        }
    }

    private askAccountDistributionDialogClosed(val, row) {
        if (val.result) {
            // Use account distribution
            this.generateAccountDistribution(row);
        }
        else {
            // Don't use account distribution                
            this.allChangesDone(true);//NOTE: Checkinventorytrigger does nothing right now, so this is the correct behaviour for now.
        }
    }

    private deleteChildRows(row: AccountingRowDTO) {
        //let deleteRow = false;
        if (row.parentRowId && row.parentRowId > 0) {
            var parentRow = _.find(this.accountingRows, (r) => r.tempRowId === row.parentRowId);
            if (parentRow) { 
                row = parentRow;
                //deleteRow = true;
            }
        }

        let children = this.accountingRows.filter(r => r.accountDistributionHeadId === row.accountDistributionHeadId && r.parentRowId === row.tempRowId || r.parentRowId === row.tempInvoiceRowId);
        if (!children || children.length == 0)
            children = this.accountingRows.filter(r => r.accountDistributionHeadId === row.accountDistributionHeadId && (r.invoiceRowId > row.invoiceRowId || (r.parentRowId && r.parentRowId === row.invoiceRowId) || (r.parentRowId && r.parentRowId === row.tempInvoiceRowId)));

        children.forEach(c => this.deleteRow(c));

        var newRow = this.accountingRows.filter(r => r.dim1Id === 0)[0];
        if (newRow)
            this.deleteRow(newRow);

        //if (deleteRow)
        //    this.deleteRow(row);
    }

    private generateAccountDistribution(row: AccountingRowDTO) {
        if (row == null || this.selectedAccountDistribution == null)
            return;

        if (!this.selectedAccountDistribution.keepRow)
            this.deleteRow(row);

        this.loadAccountDistributionRows(this.selectedAccountDistribution.accountDistributionHeadId, row);
    }

    private loadAccountDistributionRows(acountDistributionHeadId: number, row: AccountingRowDTO) {
        //TODO: replace with adm.GetAccountDistributionRows(accountDistributionHeadId)??
        this.accountingService.getAccountDistributionHead(acountDistributionHeadId).then((data: IAccountDistributionHeadDTO) => {
            var distributionRows = data.rows;
            if (this.selectedAccountDistribution.keepRow)
                row.accountDistributionHeadId = this.selectedAccountDistribution.accountDistributionHeadId;
            var oldLength = this.accountingRows.length;

            var totalSumDebitAmounts = 0;
            var totalSumCreditAmounts = 0;
            var diff = 0;
            var indexOf = 0;

            _.forEach(_.orderBy(distributionRows, r => r.rowNbr), distributionRow => {
                indexOf++;                

                var newRow = this.addRow().row;
                newRow.accountDistributionHeadId = row.accountDistributionHeadId;
                if (this.selectedAccountDistribution.keepRow)
                    newRow.parentRowId = row.tempInvoiceRowId;

                newRow.dim1Id = distributionRow.dim1Id ? distributionRow.dim1Id : row.dim1Id;

                newRow.dim2Id = distributionRow.dim2KeepSourceRowAccount ? row.dim2Id : distributionRow.dim2Id;
                newRow.dim3Id = distributionRow.dim3KeepSourceRowAccount ? row.dim3Id : distributionRow.dim3Id;
                newRow.dim4Id = distributionRow.dim4KeepSourceRowAccount ? row.dim4Id : distributionRow.dim4Id;
                newRow.dim5Id = distributionRow.dim5KeepSourceRowAccount ? row.dim5Id : distributionRow.dim5Id;
                newRow.dim6Id = distributionRow.dim6KeepSourceRowAccount ? row.dim6Id : distributionRow.dim6Id;

                // Set amount
                this.setAmountFromDistribution(distributionRow, newRow, row, <any>this.selectedAccountDistribution.calculationType, oldLength);

                totalSumDebitAmounts = (totalSumDebitAmounts + newRow.debitAmount);
                totalSumCreditAmounts = (totalSumCreditAmounts + newRow.creditAmount);

                if (distributionRows.length == indexOf) {

                    if (newRow.debitAmount > 0 && totalSumCreditAmounts > 0) {
                        newRow.debitAmount = (newRow.debitAmount - (totalSumDebitAmounts - totalSumCreditAmounts)).round(2);
                    }

                    if (newRow.creditAmount > 0 && totalSumDebitAmounts > 0) {
                        newRow.creditAmount = (newRow.creditAmount - (totalSumCreditAmounts - totalSumDebitAmounts)).round(2);
                    }
                }
            });

            this.allChangesDone(false);//should cause accountingRows to set all accounts and mandatory settings on all rows, and also validate everything again.
        });
    }

    private setAmountFromDistribution(distributionRow: IAccountDistributionRowDTO, newRow: AccountingRowDTO, row: AccountingRowDTO, calculationType: TermGroup_AccountDistributionCalculationType, calculateRowNbrOffset: number) {
        var calculateRowItem: IAccountingRowDTO = null;
        if (distributionRow.calculateRowNbr !== 0)
            calculateRowItem = _.find(this.accountingRows, r => r.rowNr === distributionRow.calculateRowNbr + calculateRowNbrOffset);
        if (calculateRowItem == null)
            calculateRowItem = row;

        var sourceAmount = Math.abs(calculateRowItem.debitAmount || 0 - calculateRowItem.creditAmount || 0);
        var targetAmount = 0;
        var isDebitAmount = calculateRowItem.debitAmount > 0;

        switch (calculationType) {
            case TermGroup_AccountDistributionCalculationType.Percent:
                targetAmount = sourceAmount * (distributionRow.sameBalance != 0 ? distributionRow.sameBalance : distributionRow.oppositeBalance) / 100;
                break;
            case TermGroup_AccountDistributionCalculationType.Amount:
                targetAmount = distributionRow.sameBalance !== 0 ? distributionRow.sameBalance : distributionRow.oppositeBalance;
                break;
            case TermGroup_AccountDistributionCalculationType.TotalAmount:
                // TODO: Implement <-- this actually was like this, so it isnt me, dev02 who left it like that.
                break;
        }

        targetAmount = targetAmount.round(2);

        newRow.debitAmount = (distributionRow.sameBalance !== 0 ? (isDebitAmount ? targetAmount : 0) : (isDebitAmount ? 0 : targetAmount)).round(2);
        newRow.creditAmount = (distributionRow.sameBalance !== 0 ? (isDebitAmount ? 0 : targetAmount) : (isDebitAmount ? targetAmount : 0)).round(2);

        newRow.amount = (newRow.debitAmount - newRow.creditAmount).round(2);

        // Set row type
        newRow.isDebitRow = newRow.debitAmount > 0;
        newRow.isCreditRow = newRow.creditAmount > 0;
    }
}