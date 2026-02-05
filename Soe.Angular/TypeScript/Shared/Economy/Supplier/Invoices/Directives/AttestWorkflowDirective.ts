import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ISupplierService } from "../../../../../Shared/Economy/Supplier/SupplierService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { SupplierInvoiceAttestFlowButtonFunctions, SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { IAccountingRowDTO, IAttestWorkFlowRowDTO } from "../../../../../Scripts/TypeLite.Net4";
import { AttestWorkFlowUserSelectorController } from "../Dialogs/AttestWorkFlowUserSelector/AttestWorkFlowUserSelectorController";
import { Feature, CompanySettingType, TermGroup_AttestWorkFlowRowProcessType, AttestStatus_SupplierInvoice, SupplierInvoiceAccountRowAttestStatus, AttestFlow_ReplaceUserReason } from "../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../Util/Constants";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { GridControllerBaseAg } from "../../../../../Core/Controllers/GridControllerBaseAg";

//@ngInject
export function attestWorkflowDirective(urlHelperService: IUrlHelperService): ng.IDirective {
    return {
        templateUrl: urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/attestWorkFlow.html"),
        replace: true,
        restrict: "E",
        controller: AttestWorkflowController,
        controllerAs: "ctrl",
        bindToController: true,
        scope: {
            attestWorkflowHead: "=",
            comment: "=",
            invoiceIsLoaded: "=",
            attestFlowCancelPermission: "=",
            attestFlowTransferRowsPermission: "=",
            attestFlowAdminPermission: "=",
            hasTransferredRows: "=",
            accountYearId: "=",
            accountingRows: "=",
            supplierInvoiceId: "@",
            showTransactionCurrency: '=?',
            defaultAttestRowDebitAccountId: '=',
            defaultAttestRowAmount: '=',
            disableTransfer: "="
        },
        link(scope: ng.IScope, element: JQuery, attributes: ng.IAttributes, ngModelController: any) {
            scope.$watch(() => (ngModelController.attestWorkflowHead), (newValue, oldValue) => {
                if (newValue) {
                    ngModelController.setGridData();
                }
            }, true);
        }
    }
}

export class AttestWorkflowController extends GridControllerBaseAg {
    // Init parameters
    private attestWorkflowHead: any;
    private comment: string;
    private selectedRow: any;
    private invoiceIsLoaded: boolean;
    private attestFlowCancelPermission: boolean;
    private attestFlowTransferRowsPermission: boolean;
    private attestFlowAdminPermission: boolean;
    private hasTransferredRows: boolean;
    private accountYearId: number;
    public accountingRows: any[];
    private supplierInvoiceId: number;
    private pendingGridLoad: boolean = false;
    private disableTransfer: boolean = false;
    

    private terms: any;
    private modal: angular.ui.bootstrap.IModalService;
    private answerText: string;
    public enableAddAdjustmentRowButton: boolean;
    public enableTransferRowsButton: boolean;
    private functionButtonEnabled: boolean;
    private savingAdjustedRows: boolean;
    private currencyDate: Date;
    private attestWorkflowRows: IAttestWorkFlowRowDTO[];

    // Functions
    buttonFunctions: any = [];

    // Company settings
    transferToVoucherOnAcceptedAttest: boolean = false;

    // Flags
    private showAcceptButton: boolean = false;

    get isLocked(): boolean {
        return this.attestWorkflowHead && this.attestWorkflowHead.isDeleted;
    }

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        coreService: ICoreService,
        private supplierService: ISupplierService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService) {

        super("economy.supplier.invoice.linktoorder",
            "economy.supplier.invoice.linktoorder",
            Feature.None,
            $http,
            $templateCache,
            $timeout,
            $uibModal,
            coreService,
            translationService,
            urlHelperService,
            messagingService,
            notificationService,
            uiGridConstants);

        this.modal = $uibModal;
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableGridMenu = false;
        
        this.$q.all([this.loadCompanySettings()]).then(() => { });
    }

    public $onInit() {
        this.currencyDate = CalendarUtility.getDateToday();
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.SupplierInvoiceTransferToVoucherOnAcceptedAttest);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.transferToVoucherOnAcceptedAttest = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceTransferToVoucherOnAcceptedAttest);
        });
    }

    public setupGrid() {
        var keys: string[] = [
            "economy.supplier.invoice.atteststatetoname",
            "economy.supplier.invoice.processtype",
            "economy.supplier.invoice.answer",
            "common.name",
            "common.role",
            "common.user",
            "common.answer",
            "common.date",
            "core.deleterow",
            "core.yes",
            "core.no",
            "core.attestflowaccept",
            "core.attestflowtransfertoother",
            "core.attestflowtransfertootherwithreturn",
            "core.attestflowreject",
            "core.attestflowregistered",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected",
            "economy.supplier.attestflowoverview.isdeleted",
            "economy.supplier.attestflowoverview.deletedby"
        ];

        this.translationService.translateMany(keys)
            .then((terms) => {
                this.terms = terms;
                this.buttonFunctions.push({ id: SupplierInvoiceAttestFlowButtonFunctions.Accept, name: terms["core.attestflowaccept"] });
                this.buttonFunctions.push({ id: SupplierInvoiceAttestFlowButtonFunctions.TransferToOther, name: terms["core.attestflowtransfertoother"] });
                this.buttonFunctions.push({ id: SupplierInvoiceAttestFlowButtonFunctions.TransferToOtherWithReturn, name: terms["core.attestflowtransfertootherwithreturn"] });
                this.buttonFunctions.push({ id: SupplierInvoiceAttestFlowButtonFunctions.Reject, name: terms["core.attestflowreject"] });

                this.soeGridOptions.addColumnText("attestStateToName", terms["economy.supplier.invoice.atteststatetoname"], null);
                this.soeGridOptions.addColumnText("processTypeName", terms["economy.supplier.invoice.processtype"], null);
                this.soeGridOptions.addColumnText("loginName", terms["common.user"], null);
                this.soeGridOptions.addColumnText("name", terms["common.name"], null);
                this.soeGridOptions.addColumnText("attestRoleName", terms["common.role"], null);
                this.soeGridOptions.addColumnText("answerStr", terms["economy.supplier.invoice.answer"], null);
                this.soeGridOptions.addColumnDateTime("modified", terms["common.date"], null);
                // Removed by request - item 34356
                //this.addColumnDelete(terms["core.deleterow"], "deleteAttest", null, false, null, "showDelete");

                this.soeGridOptions.getColumnDefs().forEach(col => {
                    var cellClass: string = col.cellClass ? col.cellClass.toString() : "";
                    col.cellClass = (grid: any) => {
                        var cls = cellClass + (grid.data.isCurrentUser ? " indiscreet" : "");
                        cls += (grid.data.processType === TermGroup_AttestWorkFlowRowProcessType.LevelNotReached ? " disabled" : "");
                        cls += (grid.data.isDeleted ? " deleted" : "");

                        if (col.field === "answerStr") {
                            if (grid.data.answer === true)
                                cls += " answer_yes";
                            else if (grid.data.answer === false)
                                cls += " answer_no";
                        }
                        return cls;
                    };
                });

                this.soeGridOptions.subscribe([
                    new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, row => {
                        if (!row || row.length === 0)
                            return;

                        this.selectedRow = row[0];
                        this.comment = '';
                        
                        this.$timeout(() => {
                            this.functionButtonEnabled = (!this.attestFlowAdminPermission && this.attestFlowTransferRowsPermission && this.attestWorkflowHead.state == AttestStatus_SupplierInvoice.AttestFlowOnGoing && (this.selectedRow.isCurrentUser || this.selectedRow.roleId === CoreUtility.roleId)) ||
                                (this.attestFlowAdminPermission && this.attestWorkflowHead.state == AttestStatus_SupplierInvoice.AttestFlowOnGoing)

                            this.showAcceptButton = this.attestFlowTransferRowsPermission && (this.selectedRow && this.selectedRow.processType === TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess);
                        });
                    })
                ]);

                //Set up totals row
                this.soeGridOptions.addTotalRow("#totals-grid", {
                    filtered: this.terms["core.aggrid.totals.filtered"],
                    total: this.terms["core.aggrid.totals.total"],
                    selected: this.terms["core.aggrid.totals.selected"]
                });

                // Set row selection
                if (this.attestWorkflowHead && this.attestWorkflowHead.isDeleted)
                    this.soeGridOptions.enableRowSelection = false
                else
                    this.soeGridOptions.enableSingleSelection();

                this.soeGridOptions.finalizeInitGrid();

                if (this.pendingGridLoad) {
                    this.pendingGridLoad = false;
                    this.setGridData();
                }
            });
    }

    public setGridData() {
        if (!this.terms) {
            this.pendingGridLoad = true;
            return;
        }

        this.attestWorkflowRows = [];
        this.supplierService.getAttestWorkFlowTemplateHeadRowsUser(this.attestWorkflowHead.attestWorkFlowHeadId).then((rows: IAttestWorkFlowRowDTO[]) => {
            this.attestWorkflowRows = rows;
            var commentsAndAnswers: any[] = [];
            _.forEach(rows, row => {
                row['answerStr'] = (row.answer === true ? this.terms["core.yes"] : this.terms["core.no"]);

                if (row.processType === TermGroup_AttestWorkFlowRowProcessType.Registered) {
                    var reg = { date: CalendarUtility.toFormattedDate(row.created), rows: [] };
                    reg.rows.push({ time: CalendarUtility.toFormattedTime(row.created, false), text: row.name + " " + this.terms["core.attestflowregistered"], isComment: false });
                    commentsAndAnswers.push(reg);
                }

                // Find corresponding item for answerdate
                if (row.answerDate) {
                    var caa1 = _.find(commentsAndAnswers, (item) => item.date === CalendarUtility.toFormattedDate(row.answerDate));
                    if (caa1) {
                        caa1.rows.push({ time: CalendarUtility.toFormattedTime(row.answerDate, false), text: row.answerText, isComment: false });
                    }
                    else {
                        caa1 = { date: CalendarUtility.toFormattedDate(row.answerDate), rows: [] };
                        caa1.rows.push({ time: CalendarUtility.toFormattedTime(row.answerDate, false), text: row.answerText, isComment: false });
                        commentsAndAnswers.push(caa1);
                    }
                }

                // Find corresponding item for commentdate
                if (row.commentDate) {
                    var caa2 = _.find(commentsAndAnswers, (item) => item.date === CalendarUtility.toFormattedDate(row.commentDate));
                    if (caa2) {
                        caa2.rows.push({ time: CalendarUtility.toFormattedTime(row.commentDate, false), text: row.comment, name: row.commentUser, isComment: true });
                    }
                    else {
                        caa2 = { date: CalendarUtility.toFormattedDate(row.commentDate), rows: [] };
                        caa2.rows.push({ time: CalendarUtility.toFormattedTime(row.commentDate, false), text: row.comment, name: row.commentUser, isComment: true });
                        commentsAndAnswers.push(caa2);
                    }
                }
            });

            if (this.attestWorkflowHead.isDeleted && this.attestWorkflowHead.modified) {
                var caa3 = _.find(commentsAndAnswers, (item) => item.date === CalendarUtility.toFormattedDate(this.attestWorkflowHead.modified));
                if (caa3) {
                    caa3.rows.push({ time: CalendarUtility.toFormattedTime(this.attestWorkflowHead.modified, false), text: this.terms["economy.supplier.attestflowoverview.deletedby"] + " " + this.attestWorkflowHead.modifiedBy, isComment: false, deleted: true });
                }
                else {
                    caa3 = { date: CalendarUtility.toFormattedDate(this.attestWorkflowHead.modified), rows: [] };
                    caa3.rows.push({ time: CalendarUtility.toFormattedTime(this.attestWorkflowHead.modified, false), text: this.terms["economy.supplier.attestflowoverview.deletedby"] + " " + this.attestWorkflowHead.modifiedBy, isComment: false, deleted: true });
                    commentsAndAnswers.push(caa3);
                }
            }

            this.gridDataLoaded(rows);

            if (rows && rows.length) {
                var userId = rows[0].userId;
                if (userId) {
                    _.find(this.soeGridOptions.getColumnDefs(), (col) => col.field === "attestRoleName").visible = false;
                } else {
                    this.soeGridOptions.getColumnDefs().forEach(col => {
                        col.visible = !(col.field === "loginName" || col.field === "name");
                    });
                }
            }

            var currentUserIndex = _.findIndex(rows, (row: any) => row.isCurrentUser && !row.isDeleted);
            if (!this.attestWorkflowHead.isDeleted) {
                this.$timeout(() => {
                    this.soeGridOptions.selectRowByVisibleIndex(currentUserIndex > 0 ? currentUserIndex : 0);
                });
            }

            this.enableAddAdjustmentRowButton = !!_.find(rows, (row: any) => row.userId === CoreUtility.userId || row.roleId === CoreUtility.roleId) || this.attestFlowTransferRowsPermission;            
            this.enableTransferRowsButton = !this.hasTransferredRows && _.filter(rows, r => r.processType == TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess || r.processType == TermGroup_AttestWorkFlowRowProcessType.Returned || r.processType == TermGroup_AttestWorkFlowRowProcessType.LevelNotReached).length == 0;
        });
    }

    /*
    public createHistoryLog(commentsAndAnswers: any[]) {
        var container = document.getElementById(this.historyContainer);

        //Clear
        if (container.childNodes.length > 0) {
            while (container.firstChild) {
                container.removeChild(container.firstChild);
            }
        }

        if (this.attestWorkflowHead.isDeleted) {
            //economy.supplier.attestflowoverview.isdeleted
            var deletedRow = document.createElement("div");
            deletedRow.classList.add("row");

            var labelDeleted = document.createElement("label");
            labelDeleted.classList.add("margin-large-left");
            labelDeleted.classList.add("errorColor");
            labelDeleted.innerText = "* " + this.terms["economy.supplier.attestflowoverview.isdeleted"];

            deletedRow.appendChild(labelDeleted);
            container.appendChild(deletedRow);
        }

        _.forEach(commentsAndAnswers, (dateItem) => {
            //Header
            var headerRow = document.createElement("div");
            headerRow.classList.add("row");

            var paragraph = document.createElement("p");
            paragraph.classList.add("history-paragraph");
            paragraph.classList.add("margin-large-left");
            paragraph.classList.add("margin-large-right");

            var span = document.createElement("span");
            span.classList.add("history-paragraph-span");
            span.innerText = dateItem.date;

            paragraph.appendChild(span);
            headerRow.appendChild(paragraph);
            container.appendChild(headerRow);

            _.forEach(_.orderBy(dateItem.rows, ['time', 'isComment']), (row) => {
                var historyRow = document.createElement("div");
                historyRow.classList.add("row");
                historyRow.classList.add("margin-large-bottom");

                var timeRow = document.createElement("div");
                timeRow.classList.add("margin-large-left");

                var labelTime = document.createElement("label");
                labelTime.classList.add("discreet");
                labelTime.innerText = row.time;
                timeRow.appendChild(labelTime);
                if (row.isComment) {
                    var labelName = document.createElement("label");
                    labelName.classList.add("margin-large-left");
                    labelName.innerText = row.name;
                    timeRow.appendChild(labelName);
                }

                var textRow = document.createElement("div");
                textRow.classList.add("margin-large-left");
                textRow.classList.add("margin-large-right");
                textRow.classList.add("padding-small-left");
                textRow.classList.add("padding-small-top");
                textRow.classList.add("padding-small-bottom");
                if (row.isComment)
                    textRow.classList.add("history-speech-bubble");
                else if (row.deleted)
                    textRow.classList.add("history-bubble-deleted");
                else
                    textRow.classList.add("history-bubble");
                textRow.innerText = row.text;

                historyRow.appendChild(timeRow);
                historyRow.appendChild(textRow);
                container.appendChild(historyRow);
            });
        });
    }
    */
    public showDelete(row: any) {
        return this.attestFlowCancelPermission &&
            (row.processType === TermGroup_AttestWorkFlowRowProcessType.WaitingForProcess ||
                row.processType === TermGroup_AttestWorkFlowRowProcessType.Returned ||
                row.processType === TermGroup_AttestWorkFlowRowProcessType.LevelNotReached) &&
            !row.IsDeleted;
    }

    private saveDisabled() {
        return this.savingAdjustedRows || this.hasValidationError() || !_.find(this.accountingRows, (row: IAccountingRowDTO) => row.isModified);
    }

    private showTransfer() {
        return this.enableTransferRowsButton && _.filter(this.accountingRows, r => r.attestStatus == SupplierInvoiceAccountRowAttestStatus.New).length > 0;
    }

    private isDeleted(row: IAccountingRowDTO) {
        return row.isDeleted || row.attestStatus === SupplierInvoiceAccountRowAttestStatus.Deleted;
    }

    private hasInvalidAmount(row: IAccountingRowDTO) {
        return !row.amount || row.amount <= 0 || isNaN(row.amount);
    }

    private hasValidationError() {
        return !!_.find(this.accountingRows, (row: IAccountingRowDTO) => (!this.isDeleted(row) && row.dim1Id && (row["dim1Error"] || this.hasInvalidAmount(row))));
    }  

    public deleteAttest(row: any) {
        var rows: any[] = this.soeGridOptions.getData();
        if (_.filter(rows, a => a.attestStateFromId === row.attestStateFromId && !a.isDeleted && a.processType !== TermGroup_AttestWorkFlowRowProcessType.Registered).length === 1) {
            this.replaceUserAndSaveAnswerToAttestFlow(AttestFlow_ReplaceUserReason.Remove, false, row, this.supplierInvoiceId);
        } else {
            var keys: string[] = [
                "core.warning",
                "core.deletewarning"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["core.deletewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val) {
                        _.find(rows, x => x.attestWorkFlowRowId === row.attestWorkFlowRowId).isDeleted = true;
                        this.soeGridOptions.refreshColumns();
                    }
                });
            });
        }
    }

    private executeButtonFunction(option) {
        switch (option.id) {
            case SupplierInvoiceAttestFlowButtonFunctions.Accept:
                this.accept();
                break;
            case SupplierInvoiceAttestFlowButtonFunctions.TransferToOther:
                this.transferToOther(false);
                break;
            case SupplierInvoiceAttestFlowButtonFunctions.TransferToOtherWithReturn:
                this.transferToOther(true);
                break;
            case SupplierInvoiceAttestFlowButtonFunctions.Reject:
                this.reject();
                break;
        }
    }

    private transferToOther(withReturn: boolean) {
        // Show dialog to transfer to other
        //var rows: any[] = this.soeGridOptions.getData(); - REMOVED DUE TO BUG 82572
        //if (_.filter(rows, a => a.attestStateFromId === this.selectedRow.attestStateFromId && !a.isDeleted && a.processType !== TermGroup_AttestWorkFlowRowProcessType.Registered).length === 1) {
            var result: any = [];
            var reason: AttestFlow_ReplaceUserReason = withReturn ? AttestFlow_ReplaceUserReason.TransferWithReturn : AttestFlow_ReplaceUserReason.Transfer;

            var options: angular.ui.bootstrap.IModalSettings = {
                templateUrl: this.urlHelperService
                    .getGlobalUrl("Shared/Economy/Supplier/Invoices/Dialogs/AttestWorkFlowUserSelector/Views/attestWorkFlowUserSelector.html"),
                controller: AttestWorkFlowUserSelectorController,
                controllerAs: "ctrl",
                resolve: {
                    result: () => result,
                    row: () => this.selectedRow,
                    reason: () => reason
                }
            }
            this.modal.open(options).result.then((res: any) => {
                if (res && res.selectedUser) {
                    this.replaceUser(reason, this.selectedRow, res.selectedUser.userId, this.supplierInvoiceId, res.sendMessage);
                }
            });
        /*} else {
            var keys: string[] = [
                "core.warning",
                "core.deletewarning"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["core.deletewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val) {
                        _.find(rows, x => x.attestWorkFlowRowId === this.selectedRow.attestWorkFlowRowId).isDeleted = true;
                        this.soeGridOptions.refreshColumns();
                    }
                });
            });
        }*/
    }

    private accept() {
        //Accept
        if (!this.selectedRow) {
            if (this.selectedRow.attestWorkFlowId == 0)
                return;
        }

        if (this.selectedRow.workFlowRowIdToReplace > 0) {
            this.replaceUserAndSaveAnswerToAttestFlow(AttestFlow_ReplaceUserReason.Remove, true, this.selectedRow, this.supplierInvoiceId);
            return;
        }

        //Just save answer
        this.saveAnswerToAttestFlow(this.selectedRow.attestWorkFlowRowId, true);
    }

    private reject() {
        //Reject
        if (!this.selectedRow) {
            if (this.selectedRow.attestWorkFlowId == 0)
                return;
        }

        if (!this.comment) {
            //Must have comment if reject
            var keys: string[] = [
                "core.unabletosave",
                "core.missingcomment"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                this.notificationService.showDialog(terms["core.unabletosave"], terms["core.missingcomment"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            });
            return;
        }

        if (this.selectedRow.workFlowRowIdToReplace > 0) {
            this.replaceUserAndSaveAnswerToAttestFlow(AttestFlow_ReplaceUserReason.Remove, false, this.selectedRow, this.supplierInvoiceId);
            return;
        }

        //Just save reject
        this.saveAnswerToAttestFlow(this.selectedRow.attestWorkFlowRowId, false);
    }

    private cancelAttestFlow() {
        //Cancel whole invoices attestFlow
        var keys: string[] = [
            "core.warning",
            "core.attestflowcancelwarning"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["core.warning"], terms["core.attestflowcancelwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.supplierService.deleteAttestWorkFlow(this.attestWorkflowHead.attestWorkFlowHeadId).then((data) => {
                        if (data.success) {
                            this.messagingService.publish(Constants.EVENT_RELOAD_INVOICE, undefined);
                            this.messagingService.publish(Constants.EVENT_RELOAD_ATTEST_FLOW_OVERVIEW, {});
                        } else {
                            this.failedDelete(<string>data.errorMessage);
                        }
                    }, error => {
                        this.failedDelete(<string>error.message);
                    });
                }
            });
        });
    }

    private saveAnswerToAttestFlow(attestWorkFlowRowId: number, answer: boolean) {
        if (!this.comment)
            this.comment = '';

        this.startSave();
        this.supplierService.saveAttestWorkFlowRowAnswer(attestWorkFlowRowId, this.comment, answer, this.accountYearId).then((result) => {
            if (result.success) {
                this.completedSave(null);
                this.setGridData();
                this.messagingService.publish(Constants.EVENT_RELOAD_ATTEST_FLOW_OVERVIEW, {});
            } else {
                this.failedSave(result.errorMessage);
            }
        });
    }

    private replaceUser(reason: AttestFlow_ReplaceUserReason, row: any, replacementUserId: number, invoiceId: number, sendMessage: boolean) {
        this.startSave();
        this.supplierService.replaceAttestWorkFlowUser(reason, row.attestWorkFlowRowId, this.comment, replacementUserId, invoiceId, sendMessage).then((result) => {
            if (result.success) {
                this.completedSave(null);
                this.setGridData();
                this.messagingService.publish(Constants.EVENT_RELOAD_ATTEST_FLOW_OVERVIEW, {});
            } else {
                this.failedSave(result.errorMessage);
            }
        });
    }

    private replaceUserAndSaveAnswerToAttestFlow(reason: AttestFlow_ReplaceUserReason, answer: boolean, row: any, invoiceId: number) {
        //First do replace
        this.startSave();
        this.supplierService.replaceAttestWorkFlowUser(reason, row.attestWorkFlowRowId, this.comment, row.userId, invoiceId, answer).then((result) => {
            if (result.success) {
                this.completedSave(null);
                //Then save answer
                if (reason !== AttestFlow_ReplaceUserReason.Remove)
                    this.saveAnswerToAttestFlow(result.integerValue, answer);
            } else {
                this.failedSave(result.errorMessage);
            }
        });
    }

    private saveAdjustedRows() {
        this.savingAdjustedRows = true;
        // Only save deleted rows if they are modified (has been delete now).
        // The server side will only fetch active rows, so otherwise they will be added again.
        this.startSave();
        this.supplierService.saveSupplierInvoiceAccountingRows(this.supplierInvoiceId, _.filter(this.accountingRows, r => r.dim1Id && ((r.isDeleted && r.isModified) || !r.isDeleted)),null).then(() => {
            this.savingAdjustedRows = false;
            this.accountingRows.forEach((r: IAccountingRowDTO) => r.isModified = false);
            this.completedSave(null);
            this.messagingService.publish(Constants.EVENT_RELOAD_INVOICE, {});
        }, () => this.savingAdjustedRows = false);
    }

    private transferAdjustedRows() {
        var keys: string[] = ["core.verifyquestion", "economy.supplier.invoice.attestaccountingrows.asktransfer"];
        this.translationService.translateMany(keys).then(terms => {
            var modal = this.notificationService.showDialogEx(terms["core.verifyquestion"], terms["economy.supplier.invoice.attestaccountingrows.asktransfer"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.YesNo);
            modal.result.then(val => {
                if (val === true)
                    this.messagingService.publish(Constants.EVENT_TRANSFER_ATTEST_ROWS, this.supplierInvoiceId);
            });
        });
    }

    private addInvoiceToAttestFlow() {
        this.messagingService.publish(Constants.EVENT_ADD_INVOICE_TO_ATTESTFLOW, this.supplierInvoiceId);
    }
}