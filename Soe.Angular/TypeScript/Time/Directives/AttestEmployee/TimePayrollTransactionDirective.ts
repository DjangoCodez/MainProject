import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { AttestEmployeeDayDTO } from "../../../Common/Models/TimeEmployeeTreeDTO";
import { AttestPayrollTransactionDTO } from "../../../Common/Models/AttestPayrollTransactionDTO";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../../Time/TimeService";
import { Constants } from "../../../Util/Constants";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { Feature, TimeAttestMode, TermGroup_TimeDeviationCauseType, TermGroup_AttestEntity, TermGroup_TimeCodeRegistrationType } from "../../../Util/CommonEnumerations";
import { TimeDeviationCauseGridDTO } from "../../../Common/Models/TimeDeviationCauseDTOs";
import { TimePayrollTransactionDialogController } from "./Dialogs/TimePayrollTransactionDialog/TimePayrollTransactionDialogController";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { AttestStateDTO } from "../../../Common/Models/AttestStateDTO";
import { Guid } from "../../../Util/StringUtility";


export class TimePayrollTransactionDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Directives/AttestEmployee/Views/TimePayrollTransaction.html'),
            scope: {
                model: '=',
                isModal: '=?',
                reloading: '=',
                validating: '=',
                isModified: '=',
                selectedTimeCodeTransactionId: '=',
                onResizeNeeded: '&'
            },
            restrict: 'E',
            replace: true,
            controller: TimePayrollTransactionController,
            controllerAs: 'ctrl',
            bindToController: true,
        };
    }
}

export class TimePayrollTransactionController {

    // Init parameters
    private model: AttestEmployeeDayDTO;
    private isModal: boolean;
    private reloading: boolean;
    private validating: boolean;
    private isModified: boolean;
    private selectedTimeCodeTransactionId: number;
    private onResizeNeeded: Function;

    private isGenerated: boolean = false;

    private attestMode: TimeAttestMode;
    private get isMyTime(): boolean {
        return this.attestMode == TimeAttestMode.TimeUser;
    }

    // Permissions
    private showTimePayrollTransactionsPermission: boolean = false;
    private editTransactionsPermission: boolean = false;

    // Terms
    private terms: { [index: string]: string; };
    private progressMessage: string = '';

    // Data
    private deviationCauses: TimeDeviationCauseGridDTO[] = [];
    private payrollProducts: SmallGenericType[] = [];
    private attestStateInitial: AttestStateDTO;

    // Flags
    private expanded: boolean = true;
    private executing: boolean = false;

    // Properties
    private selectedTransaction: AttestPayrollTransactionDTO;
    private sortBy: string = 'startTimeString';
    private sortByReverse: boolean = false;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        private timeService: ITimeService) {

        // Config parameters
        this.attestMode = soeConfig.attestMode ? soeConfig.attestMode : TimeAttestMode.Time;
    }

    public $onInit() {
        this.$q.all([
            this.loadTerms(),
            this.loadReadPermissions(),
            this.loadModifyPermissions(),
            this.loadDeviationCauses(),
            this.loadAttestStateInitial()
        ]).then(() => {
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.reloading, (newVal, oldVal) => {
            if (newVal !== oldVal) {
                if (this.reloading) {
                    this.progressMessage = this.terms["time.time.attest.timeblocks.reloading"];
                    this.executing = true;
                } else {
                    this.executing = false;
                    this.isModified = false;
                    this.isGenerated = false;
                }
            }
        });

        this.$scope.$watch(() => this.validating, (newVal, oldVal) => {
            if (newVal !== oldVal) {
                if (this.validating) {
                    this.progressMessage = this.terms["time.time.attest.timeblocks.validating"];
                    this.executing = true;
                    this.isGenerated = true;
                } else {
                    this.executing = false;
                }
            }
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.info",
            "core.error",
            "time.payroll.payrollproducts.workedtime",
            "time.payroll.payrollproducts.notworkedtime",
            "time.time.attest.timeblocks.reloading",
            "time.time.attest.timeblocks.validating",
            "time.time.attest.timeblocks.saving"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadReadPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];

        if (this.isMyTime) {
            featureIds.push(Feature.Time_Time_AttestUser_ShowPayrollTransactions);
        } else {
            featureIds.push(Feature.Time_Time_Attest_ShowPayrollTransactions);
        }

        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            if (this.isMyTime) {
                this.showTimePayrollTransactionsPermission = x[Feature.Time_Time_AttestUser_ShowPayrollTransactions];
            } else {
                this.showTimePayrollTransactionsPermission = x[Feature.Time_Time_Attest_ShowPayrollTransactions];
            }
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Time_Time_Attest_EditTransactions);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.editTransactionsPermission = x[Feature.Time_Time_Attest_EditTransactions];
        });
    }

    private loadDeviationCauses(): ng.IPromise<any> {
        return this.timeService.getTimeDeviationCausesByEmployeeGroupGrid(this.model.employeeGroupId, false, false).then(x => {
            this.deviationCauses = x;
        });
    }

    private loadPayrollProducts(): ng.IPromise<any> {
        return this.timeService.getPayrollProductsDict(false, true, true).then(x => {
            this.payrollProducts = x;
        });
    }

    private loadAttestStateInitial() {
        this.coreService.getAttestStateInitial(TermGroup_AttestEntity.PayrollTime).then(x => {
            this.attestStateInitial = x;
        });
    }

    // EVENTS

    private toggleExpanded() {
        this.expanded = !this.expanded;

        if (this.onResizeNeeded) {
            this.$timeout(() => {
                this.onResizeNeeded();
            });
        }
    }

    private sort(column: string) {
        this.sortByReverse = !this.sortByReverse && this.sortBy === column;
        this.sortBy = column;
    }

    private transactionSelected(trans: AttestPayrollTransactionDTO) {
        if (!trans)
            return;
        this.selectedTransaction = trans;
        this.messagingService.publish(Constants.EVENT_ATTESTEMPLOYEE_TIMECODETRANSACTION_SELECTED, trans.timeCodeTransactionId);
    }

    private showInfo(trans: AttestPayrollTransactionDTO) {
        this.timeService.getAttestTransitionLogs(trans.timeBlockDateId, trans.employeeId, trans.timePayrollTransactionId).then(x => {
            trans.attestTransitionLogs = x;

            var keys: string[] = [
                "core.info",
                "common.transaction",
                "common.createdbyat",
                "common.modifiedbyat",
                "time.time.attest.transactionchangedfromtoat",
                "time.payroll.payrollproduct.payrollproduct",
                "time.time.attest.calculation"
            ];

            this.translationService.translateMany(keys).then(terms => {

                var message: string = "{0} ID: {1}\n{2} ID: {3}\n\n".format(
                    terms["common.transaction"],
                    trans.timePayrollTransactionId.toString(),
                    terms["time.payroll.payrollproduct.payrollproduct"],
                    trans.payrollProductId.toString());
                
                if (trans.created) {
                    message += terms["common.createdbyat"].format(trans.createdBy || '', trans.created.toFormattedDateTime()) + "\n";
                    if (trans.modified)
                        message += terms["common.modifiedbyat"].format(trans.modifiedBy || '', trans.modified.toFormattedDateTime()) + "\n";
                    message += "<br />";
                }                     

                if (trans.formulaPlain) {
                    message += "<b>" + terms["time.time.attest.calculation"] + "</b>";
                    message += "<br />";
                    if (trans.formulaPlain) {
                        message += trans.formulaPlain;
                        message += "<br />";
                    }
                    if (trans.formulaExtracted) {
                        message += trans.formulaExtracted;
                        message += "<br />";
                    }
                    message += "<br />";
                }

                if (trans.attestTransitionLogs.length > 0) {
                    _.forEach(trans.attestTransitionLogs, log => {
                        message += terms["time.time.attest.transactionchangedfromtoat"].format(log.attestStateFromName,
                            log.attestStateToName,
                            log.attestTransitionCreatedBySupport ? "SoftOne" + " (" + log.attestTransitionUserId + ")" : log.attestTransitionUserName,
                            log.attestTransitionDate.toFormattedDateTime()) + "\n";
                    });
                }

                this.notificationService.showDialogEx(terms["core.info"], message, SOEMessageBoxImage.Information);
            });
        });
    }

    private showComment(trans: AttestPayrollTransactionDTO) {
        var keys: string[] = [
            "common.comment",
            "common.quantity",
            "time.payrollproduct.payrollproduct"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var message: string = "{0}: {1} {2}\n{3}: {4}\n{5}".format(terms["time.payrollproduct.payrollproduct"], trans.payrollProductNumber, trans.payrollProductName, terms["common.quantity"], trans.quantityString, trans.comment);
            this.notificationService.showDialogEx(terms["common.comment"], message, SOEMessageBoxImage.Information);
        });
    }

    private initEditTransaction(trans: AttestPayrollTransactionDTO) {
        if (!this.canEditTransaction(trans))
            return;

        if (!this.payrollProducts || this.payrollProducts.length === 0) {
            this.loadPayrollProducts().then(() => {
                this.editTransaction(trans);
            });
        } else {
            this.editTransaction(trans);
        }
    }

    private editTransaction(trans: AttestPayrollTransactionDTO) {
        if (trans)
            this.selectedTransaction = trans;

        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Directives/AttestEmployee/Dialogs/TimePayrollTransactionDialog/TimePayrollTransactionDialog.html"),
            controller: TimePayrollTransactionDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                payrollProducts: () => { return this.payrollProducts },
                employeeId: () => { return this.model.employeeId },
                date: () => { return this.model.date },
                trans: () => { return trans }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.trans) {
                if (result.save) {
                    var editTrans: AttestPayrollTransactionDTO = result.trans;

                    if (!trans) {
                        // Add
                        trans = new AttestPayrollTransactionDTO();
                        trans.guidId = Guid.newGuid();
                        trans.manuallyAdded = true;
                        trans.attestStateId = this.attestStateInitial ? this.attestStateInitial.attestStateId : 0;
                        trans.attestStateName = this.attestStateInitial ? this.attestStateInitial.name : '';
                        trans.attestStateColor = this.attestStateInitial ? this.attestStateInitial.color : '';
                        this.model.attestPayrollTransactions.push(trans);
                        this.selectedTransaction = trans;
                    }
                    else {
                        trans.isModified = true;
                    }

                    if (editTrans.payrollProductId !== trans.payrollProductId) {
                        var product: SmallGenericType = _.find(this.payrollProducts, p => p.id === editTrans.payrollProductId);
                        trans.payrollProductId = editTrans.payrollProductId;
                        trans.payrollProductNumber = product ? product.name : '';
                    }

                    if (trans.quantity !== editTrans.quantity) {
                        trans.quantity = editTrans.quantity;
                        trans.quantityString = trans.quantityTimeFormatted;
                    }

                    trans.accountingSettings = editTrans.accountingSettings;
                    this.setAccountingIds(trans);
                    this.setAccountingStrings(trans);

                    if (result.updateQuantityOnChildren) {
                        let chainedTransactions: AttestPayrollTransactionDTO[] = [];
                        AttestPayrollTransactionDTO.getChain(this.model.attestPayrollTransactions, trans, chainedTransactions);
                        _.forEach(chainedTransactions, ct => {
                            ct.quantity = trans.quantity;
                            ct.quantityString = trans.quantityTimeFormatted;
                        });
                    }

                    trans.comment = editTrans.comment;

                    trans.isModified = this.isModified = true;
                } else if (result.delete) {
                    _.pull(this.model.attestPayrollTransactions, trans);
                    trans.isModified = this.isModified = true;

                    if (result.deleteChildren) {
                        let chainedTransactions: AttestPayrollTransactionDTO[] = [];
                        AttestPayrollTransactionDTO.getChain(this.model.attestPayrollTransactions, trans, chainedTransactions);
                        _.forEach(chainedTransactions, chainedTransaction => {
                            _.pull(this.model.attestPayrollTransactions, chainedTransaction);
                            chainedTransaction.isModified = true;
                        });
                    }

                    this.selectedTransaction = this.model.attestPayrollTransactions.length > 0 ? this.model.attestPayrollTransactions[0] : null;
                }

                if (this.onResizeNeeded) {
                    this.$timeout(() => {
                        this.onResizeNeeded();
                    });
                }
            }
        });
    }

    private reload() {
        this.reloading = true;

        // Clear data
        this.model.timeBlocks = [];
        this.model.attestPayrollTransactions = [];
        this.model.timeCodeTransactions = [];

        this.messagingService.publish(Constants.EVENT_ATTESTEMPLOYEE_ROWS_RELOAD, { date: this.model.date, fromModal: this.isModal });
    }

    private save() {
        this.progressMessage = this.terms["time.time.attest.timeblocks.saving"];
        this.executing = true;
        
        this.timeService.saveGeneratedDeviations(this.model.timeBlocks, this.model.timeCodeTransactions, this.model.attestPayrollTransactions, this.model['applyAbsenceItems'], this.model.timeBlockDateId, this.model.timeScheduleTemplatePeriodId, this.model.employeeId, this.model['payrollImportEmployeeTransactionIds']).then(result => {
            if (result.success) {

                if (result.infoMessage)
                    this.notificationService.showDialogEx(this.terms["core.info"], result.infoMessage, SOEMessageBoxImage.Information);

                var hasAbsence: boolean = false;
                _.forEach(_.filter(this.model.timeBlocks, e => e.timeDeviationCauseStartId), block => {
                    var cause = _.find(this.deviationCauses, d => d.timeDeviationCauseId === block.timeDeviationCauseStartId);
                    if (cause && cause.type === TermGroup_TimeDeviationCauseType.Absence) {
                        hasAbsence = true;
                        return false;
                    }
                });

                this.messagingService.publish(Constants.EVENT_ATTESTEMPLOYEE_ROWS_RELOAD, { date: hasAbsence ? null : this.model.date, fromModal: this.isModal });

                this.progressMessage = '';
                this.isModified = false;
            }
            else {
                this.notificationService.showDialog(this.terms["core.error"], result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            }
            this.executing = false;
        });
    }

    // HELP-METHODS

    private canEditTransaction(trans: AttestPayrollTransactionDTO): boolean {
        if (!trans) {
            // New transaction
            return !this.isGenerated;
        }

        // Modify transaction
        if (this.isGenerated)
            return false;

        if (!this.attestStateInitial || trans.attestStateId !== this.attestStateInitial.attestStateId)
            return false;

        if (trans.isReversed)
            return false;

        if (trans.isScheduleTransaction)
            return false;

        return true;
    }

    private setAccountingIds(trans: AttestPayrollTransactionDTO) {
        trans.accountInternalIds = [];
        trans.accountStdId = 0;

        if (trans.accountingSettings.length > 0) {
            let settingRow = trans.accountingSettings[0];
            if (settingRow.account1Id)
                trans.accountStdId = settingRow.account1Id;
            if (settingRow.account2Id)
                trans.accountInternalIds.push(settingRow.account2Id);
            if (settingRow.account3Id)
                trans.accountInternalIds.push(settingRow.account3Id);
            if (settingRow.account4Id)
                trans.accountInternalIds.push(settingRow.account4Id);
            if (settingRow.account5Id)
                trans.accountInternalIds.push(settingRow.account5Id);
            if (settingRow.account6Id)
                trans.accountInternalIds.push(settingRow.account6Id);
        }
    }

    private setAccountingStrings(trans: AttestPayrollTransactionDTO) {
        if (trans.accountingSettings && trans.accountingSettings.length > 0) {
            let setting = trans.accountingSettings[0];
            let accStrings: string[] = [];
            if (setting.account1Nr)
                accStrings.push(setting.account1Nr);
            if (setting.account2Nr)
                accStrings.push(setting.account2Nr);
            if (setting.account3Nr)
                accStrings.push(setting.account3Nr);
            if (setting.account4Nr)
                accStrings.push(setting.account4Nr);
            if (setting.account5Nr)
                accStrings.push(setting.account5Nr);
            if (setting.account6Nr)
                accStrings.push(setting.account6Nr);
            trans.accountingShortString = accStrings.join(';');
        }
    }

    private getPayrollProductPayedToolTip(trans: AttestPayrollTransactionDTO): string {
        if (!this.terms)
            return '';

        return trans.payrollProductPayed ? this.terms["time.payroll.payrollproducts.workedtime"] : this.terms["time.payroll.payrollproducts.notworkedtime"];
    }
}
