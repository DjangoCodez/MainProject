import { RightMenuControllerBase, RightMenuType } from "../RightMenuControllerBase";
import { IUrlHelperService } from "../../Services/UrlHelperService";
import { MessageGridDTO } from "../../../Common/Models/MessageDTOs";
import { IProgressHandler } from "../../Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../Handlers/progresshandlerfactory";
import { EmbeddedGridController } from "../../Controllers/EmbeddedGridController";
import { IContextMenuHandler } from "../../Handlers/ContextMenuHandler";
import { IContextMenuHandlerFactory } from "../../Handlers/ContextMenuHandlerFactory";
import { IMessagingService } from "../../Services/MessagingService";
import { ITranslationService } from "../../Services/TranslationService";
import { ICoreService } from "../../Services/CoreService";
import { IGridHandlerFactory } from "../../Handlers/gridhandlerfactory";
import { ILazyLoadService } from "../../Services/LazyLoadService";
import { Constants } from "../../../Util/Constants";
import { Feature, TermGroup_MessageType, XEMailType } from "../../../Util/CommonEnumerations";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { EditController } from "./EditController";
import { INotificationService } from "../../Services/NotificationService";

export class MessageMenuDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('/Core/RightMenu/MessageMenu/MessageMenu.html'),
            scope: {
                positionIndex: "@"
            },
            restrict: 'E',
            replace: true,
            controller: MessageMenuController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class MessageMenuController extends RightMenuControllerBase {

    // Terms
    private terms: { [index: string]: string; };
    private toggleTooltip: string;

    // Permissions
    private messageModifyPermission = false;
    private messageSendPermission = false;
    private messageDeletePermission = false;

    // Data
    private nbrOfUnreadMessages = 0;
    private messages: MessageGridDTO[] = [];

    // Properties
    private types: any[] = [];
    private selectedType: any;

    // Flags
    private loadingMessages = false;

    // Handlers
    private progress: IProgressHandler;
    private gridHandler: EmbeddedGridController;
    private contextMenuHandler: IContextMenuHandler;

    // Timer
    private unreadTimer;
    readonly UNREAD_TIMER_INTERVAL: number = (60 * 1000 * 10);  // 10 minutes

    private moduleLoaderPromise: Promise<any>;
    private htmlEditorLoaderPromise: Promise<any>;

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        messagingService: IMessagingService,
        private $scope: ng.IScope,
        private $q: ng.IQService,
        private $interval: ng.IIntervalService,
        private $uibModal,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        progressHandlerFactory: IProgressHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private contextMenuHandlerFactory: IContextMenuHandlerFactory,
        private lazyLoadService: ILazyLoadService) {
        super($timeout, messagingService, RightMenuType.Message);

        this.progress = progressHandlerFactory.create();
        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "MessageMenu.Messages");

        this.messagingService.subscribe(Constants.EVENT_RELOAD_MESSAGES, (data: any) => {
            if (this.showMenu) {
                if (data.type == -1 || data.type == this.selectedType.id)
                    this.loadMessages(data.messageId);
            }
        });

        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions()
        ]).then(() => {
            this.setupUnreadTimer();
        });
    }

    public $onInit() {
        this.setTopPosition();
        this.moduleLoaderPromise = this.lazyLoadService.loadBundle("Soe.Core.RightMenu.MessageMenu.Bundle");
        this.htmlEditorLoaderPromise = this.lazyLoadService.loadBundle("Soe.Common.HtmlEditor.Bundle");

        this.messagingService.subscribe(Constants.EVENT_TOGGLE_MESSAGE_MENU, (data: any) => {
            this.toggleShowMenu();
        });

        this.messagingService.subscribe(Constants.EVENT_SHOW_MESSAGE_MENU, (data: any) => {
            if (!this.showMenu)
                this.toggleShowMenu();

            if (data?.id) {
                let row: MessageGridDTO = new MessageGridDTO();
                row.type = data.type;
                row.subject = data.title;
                row.messageId = data.id;
                this.edit(row);
            }
        });
    }

    // SETUP

    private setupUnreadTimer() {
        this.loadNbrOfUnreadMessages();

        this.unreadTimer = this.$interval(() => {
            this.loadNbrOfUnreadMessages();
        }, this.UNREAD_TIMER_INTERVAL);

        this.$scope.$on('$destroy', () => {
            this.$interval.cancel(this.unreadTimer);
        });
    }

    private init() {
        this.setupTypes();
        this.setupContextMenu();
    }

    private setupTypes() {
        this.types = [];
        this.types.push({ id: XEMailType.Incoming, title: this.terms["common.messages.incoming"], selected: true });
        this.types.push({ id: XEMailType.Sent, title: this.terms["common.messages.sent"], selected: false });
        this.types.push({ id: XEMailType.Deleted, title: this.terms["common.messages.deleted"], selected: false });
        //this.types.push({ id: XEMailType.Outgoing, title: this.terms["common.messages.saved"], selected: false });
        this.selectType(this.types[0], this.showMenu);
    }

    private setupGrid() {
        this.gridHandler.gridAg.options.resetColumnDefs();

        this.gridHandler.gridAg.options.addColumnIcon("hasAttachment", "", null, { icon: "fal fa-paperclip", showIcon: (message) => message.hasAttachment, toolTip: this.terms["common.messages.hasattachment"], suppressFilter: true, pinned: "left" });

        switch (this.selectedType.id) {
            case (XEMailType.Incoming):
                this.gridHandler.gridAg.options.addColumnIcon("", "", null, { icon: 'fal fa-reply', showIcon: (message: MessageGridDTO) => !!message.replyDate, toolTipField: "replyDateFormatted", suppressFilter: true, pinned: "left" });
                this.gridHandler.gridAg.options.addColumnIcon("", "", null, { icon: 'fal fa-arrow-alt-right', showIcon: (message: MessageGridDTO) => !!message.forwardDate, toolTipField: "forwardDateFormatted", suppressFilter: true, pinned: "left" });
                this.gridHandler.gridAg.options.addColumnIcon("confirmationIconValue", "", null, { showIcon: (message) => message.confirmationIconValue, toolTipField: "confirmationIconMessage", suppressFilter: true, pinned: "left" });
                this.gridHandler.gridAg.options.addColumnText("senderName", this.terms["common.messages.sender"], null, { cellClassRules: { "indiscreet": (gridRow: any) => !gridRow.data.readDate }, toolTipField: "senderName" });
                this.gridHandler.gridAg.options.addColumnText("subject", this.terms["common.messages.subject"], null, { cellClassRules: { "indiscreet": (gridRow: any) => !gridRow.data.readDate }, toolTipField: "subject" });
                this.gridHandler.gridAg.options.addColumnDateTime("created", this.terms["common.messages.received"], null, true, null, null, { cellClassRules: { "indiscreet": (gridRow: any) => !gridRow.data.readDate }, toolTipField: "createdFormatted" });
                this.gridHandler.gridAg.options.addColumnDateTime("readDate", this.terms["common.messages.read"], null, true, null, null, { cellClassRules: { "indiscreet": (gridRow: any) => !gridRow.data.readDate }, toolTipField: "readDateFormatted", hide: !this.fullscreen });
                this.gridHandler.gridAg.options.addColumnDateTime("answerDate", this.terms["common.messages.answerdate"], null, true, null, null, { cellClassRules: { "indiscreet": (gridRow: any) => !gridRow.data.readDate }, toolTipField: "answerDateFormatted", hide: !this.fullscreen });
                this.gridHandler.gridAg.options.addColumnDateTime("replyDate", this.terms["common.messages.replydate"], null, true, null, null, { cellClassRules: { "indiscreet": (gridRow: any) => !gridRow.data.readDate }, toolTipField: "replyDateFormatted", hide: !this.fullscreen });
                this.gridHandler.gridAg.options.addColumnDateTime("forwardDate", this.terms["common.messages.forwarddate"], null, true, null, null, { cellClassRules: { "indiscreet": (gridRow: any) => !gridRow.data.readDate }, toolTipField: "forwardDateFormatted", hide: !this.fullscreen });
                break;
            case (XEMailType.Sent):
                this.gridHandler.gridAg.options.addColumnText("subject", this.terms["common.messages.subject"], null, { cellClassRules: { "indiscreet": (gridRow: any) => gridRow.data.hasBeenRead.startsWith('0') }, toolTipField: "subject" });
                this.gridHandler.gridAg.options.addColumnText("recieversName", this.terms["common.messages.reciever"], null, { cellClassRules: { "indiscreet": (gridRow: any) => gridRow.data.hasBeenRead.startsWith('0') }, toolTipField: "recieversName" });
                this.gridHandler.gridAg.options.addColumnDateTime("sentDate", this.terms["common.messages.sentdate"], null, true, null, null, { cellClassRules: { "indiscreet": (gridRow: any) => gridRow.data.hasBeenRead.startsWith('0') }, toolTipField: "sentDateFormatted" });
                this.gridHandler.gridAg.options.addColumnText("hasBeenRead", this.terms["common.messages.read"], null, { cellClassRules: { "indiscreet": (gridRow: any) => gridRow.data.hasBeenRead.startsWith('0') }, toolTipField: "hasBeenRead", hide: !this.fullscreen });
                this.gridHandler.gridAg.options.addColumnText("hasBeenConfirmed", this.terms["common.messages.confirmed"], null, { cellClassRules: { "indiscreet": (gridRow: any) => gridRow.data.hasBeenConfirmed.startsWith('0') }, toolTipField: "hasBeenConfirmed", hide: !this.fullscreen });
                break;
            case (XEMailType.Deleted):
                this.gridHandler.gridAg.options.addColumnText("senderName", this.terms["common.messages.sender"], null, { cellClassRules: { "indiscreet": (gridRow: any) => !gridRow.data.readDate }, toolTipField: "senderName" });
                this.gridHandler.gridAg.options.addColumnText("subject", this.terms["common.messages.subject"], null, { cellClassRules: { "indiscreet": (gridRow: any) => !gridRow.data.readDate }, toolTipField: "subject" });
                this.gridHandler.gridAg.options.addColumnDateTime("created", this.terms["common.messages.received"], null, true, null, null, { cellClassRules: { "indiscreet": (gridRow: any) => !gridRow.data.readDate }, toolTipField: "createdFormatted" });
                this.gridHandler.gridAg.options.addColumnDateTime("readDate", this.terms["common.messages.read"], null, true, null, null, { cellClassRules: { "indiscreet": (gridRow: any) => !gridRow.data.readDate }, toolTipField: "readDateFormatted", hide: !this.fullscreen });
                this.gridHandler.gridAg.options.addColumnDateTime("deletedDate", this.terms["common.messages.deleteddate"], null, true, null, null, { cellClassRules: { "indiscreet": (gridRow: any) => !gridRow.data.readDate }, toolTipField: "deletedDateFormatted", hide: !this.fullscreen });
                break;
            case (XEMailType.Outgoing):
                this.gridHandler.gridAg.options.addColumnText("subject", this.terms["common.messages.subject"], null, { cellClassRules: { "indiscreet": (gridRow: any) => !gridRow.data.readDate }, toolTipField: "subject" });
                this.gridHandler.gridAg.options.addColumnDateTime("sentDate", this.terms["common.messages.received"], null, true, null, null, { cellClassRules: { "indiscreet": (gridRow: any) => !gridRow.data.readDate }, toolTipField: "sentDateFormatted" });
                break;
        }

        this.gridHandler.gridAg.options.addColumnEdit(this.terms["core.edit"], this.edit.bind(this));

        // Grid events
        let events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.rowDoubleClicked(row); }));
        this.gridHandler.gridAg.options.subscribe(events);

        this.gridHandler.gridAg.options.enableGridMenu = false;
        this.gridHandler.gridAg.finalizeInitGrid("common.messages.messages", true, "message-totals-grid");
    }

    private showGridColumns() {
        switch (this.selectedType.id) {
            case (XEMailType.Incoming):
                if (this.fullscreen) {
                    this.gridHandler.gridAg.options.showColumn("readDate");
                    this.gridHandler.gridAg.options.showColumn("answerDate");
                    this.gridHandler.gridAg.options.showColumn("replyDate");
                    this.gridHandler.gridAg.options.showColumn("forwardDate");
                } else {
                    this.gridHandler.gridAg.options.hideColumn("readDate");
                    this.gridHandler.gridAg.options.hideColumn("answerDate");
                    this.gridHandler.gridAg.options.hideColumn("replyDate");
                    this.gridHandler.gridAg.options.hideColumn("forwardDate");
                }
                break;
            case (XEMailType.Sent):
                if (this.fullscreen) {
                    this.gridHandler.gridAg.options.showColumn("hasBeenRead");
                    this.gridHandler.gridAg.options.showColumn("hasBeenConfirmed");
                } else {
                    this.gridHandler.gridAg.options.hideColumn("hasBeenRead");
                    this.gridHandler.gridAg.options.hideColumn("hasBeenConfirmed");
                }
                break;
            case (XEMailType.Deleted):
                if (this.fullscreen) {
                    this.gridHandler.gridAg.options.showColumn("readDate");
                    this.gridHandler.gridAg.options.showColumn("deletedDate");
                } else {
                    this.gridHandler.gridAg.options.hideColumn("readDate");
                    this.gridHandler.gridAg.options.hideColumn("deletedDate");
                }
                break;
            case (XEMailType.Outgoing):
                break;
        }
    }

    private setupContextMenu() {
        this.contextMenuHandler = this.contextMenuHandlerFactory.create();
        this.createContextMenuOptions();
    }

    private getContextMenuOptions(): any[] {
        let row: MessageGridDTO = this.gridHandler.gridAg.options.getCurrentRow();
        if (row && this.selectedCount === 0)
            this.gridHandler.gridAg.options.selectRow(row, true);

        return this.contextMenuHandler.getContextMenuOptions();
    }

    private createContextMenuOptions() {
        this.contextMenuHandler.clearContextMenuItems();
        if (this.messageModifyPermission && this.messageSendPermission) {
            this.contextMenuHandler.addContextMenuItem(this.terms["common.messages.new"], 'fa-plus', ($itemScope, $event, modelValue) => { this.edit(null); }, () => { return true; });
            this.contextMenuHandler.addContextMenuSeparator();
        }
        this.contextMenuHandler.addContextMenuItem(this.terms["common.messages.setasread"], 'fa-envelope-open', ($itemScope, $event, modelValue) => { this.setMessagesAsRead(); }, () => { return true; });
        this.contextMenuHandler.addContextMenuItem(this.terms["common.messages.setasunread"], 'fa-envelope', ($itemScope, $event, modelValue) => { this.setMessagesAsUnread(); }, () => { return true; });
        this.contextMenuHandler.addContextMenuSeparator();
        if (this.messageModifyPermission && this.messageDeletePermission) {
            this.contextMenuHandler.addContextMenuItem(this.terms["core.delete"], 'fa-times iconDelete', ($itemScope, $event, modelValue) => { this.deleteMessages(); }, () => { return this.selectedType.id !== XEMailType.Deleted; });
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.messages.new",
            "common.messages.messages",
            "common.messages.unread",
            "common.messages.incoming",
            "common.messages.sent",
            "common.messages.deleted",
            "common.messages.saved",
            "common.messages.hasattachment",
            "common.messages.sender",
            "common.messages.subject",
            "common.messages.received",
            "common.messages.read",
            "common.messages.answerdate",
            "common.messages.replydate",
            "common.messages.forwarddate",
            "common.messages.confirmed",
            "common.messages.needsconfirmation",
            "common.messages.reciever",
            "common.messages.sentdate",
            "common.messages.deleteddate",
            "common.messages.setasread",
            "common.messages.setasunread",
            "core.edit",
            "core.delete",
            "core.deleteselectedwarning",
            "core.selectedmessagesmismatch",
            "core.warning"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        let featureIds: number[] = [];

        featureIds.push(Feature.Communication_XEmail);
        featureIds.push(Feature.Communication_XEmail_Send);
        featureIds.push(Feature.Communication_XEmail_Delete);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.messageModifyPermission = x[Feature.Communication_XEmail];
            this.messageSendPermission = x[Feature.Communication_XEmail_Send];
            this.messageDeletePermission = x[Feature.Communication_XEmail_Delete];
        });
    }

    private loadNbrOfUnreadMessages(): ng.IPromise<any> {
        return this.coreService.getNbrOfUnreadMessages().then(x => {
            this.nbrOfUnreadMessages = x;

            this.toggleTooltip = this.terms["common.messages.messages"]
            if (this.nbrOfUnreadMessages > 0)
                this.toggleTooltip += " ({0} {1})".format(this.nbrOfUnreadMessages.toString(), this.terms["common.messages.unread"].toLocaleLowerCase());
        });
    }

    private loadMessages(messageId?: number) {
        if (this.loadingMessages)
            return;

        if (!messageId)
            this.loadingMessages = true;

        this.coreService.getXeMailItems(this.selectedType.id).then(x => {
            if (messageId && x.length === 1) {
                let msg = x[0];
                this.setMessageData(msg);
                this.gridHandler.gridAg.options.refreshRows(msg);
            } else {
                this.messages = x;
                _.forEach(this.messages, message => {
                    this.setMessageData(message);
                });
                this.gridHandler.gridAg.setData(this.messages);
            }

            this.loadingMessages = false;
        });
    }

    private setMessageData(message: MessageGridDTO) {
        // Need to set selected mail type, will be passed to EditController
        message.type = this.selectedType;

        if (message.messageType == TermGroup_MessageType.PayrollSlip) {
            message.senderName = "";
        }

        if (message.needsConfirmation) {
            message.confirmationIconValue = "fal fa-check-square " + (message.answerDate ? "okColor" : "errorColor");
            message.confirmationIconMessage = message.answerDate ? "{0} {1}".format(this.terms["common.messages.confirmed"], message.answerDate.toFormattedDateTime()) : this.terms["common.messages.needsconfirmation"];
        }
    }

    // EVENTS

    protected toggleShowMenu() {
        super.toggleShowMenu();

        if (this.showMenu && this.types.length === 0)
            this.init();
    }

    protected toggleFullscreen(): ng.IPromise<any> {
        return super.toggleFullscreen().then(() => {
            this.showGridColumns();
        });
    }

    private selectType(type, loadMessages: boolean = true) {
        if (this.selectedType == type || this.loadingMessages)
            return;

        _.forEach(this.types, p => {
            p['selected'] = false;
        });
        type['selected'] = true;

        this.selectedType = type;
        this.setupGrid();

        if (loadMessages)
            this.loadMessages();
    }

    private rowDoubleClicked(row) {
        this.edit(row);
    }

    private edit(row: MessageGridDTO) {
        this.$q.all([this.htmlEditorLoaderPromise]).then(() => {
            const modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Core/RightMenu/MessageMenu/edit.html"),
                controller: EditController,
                controllerAs: 'ctrl',
                bindToController: true,
                backdrop: 'static',
                size: 'xl',
                windowClass: 'fullsize-modal',
                scope: this.$scope,
            });

            modal.rendered.then(() => {
                this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                    source: 'MessageMenu',
                    modal: modal,
                    type: row ? row.type : XEMailType.Outgoing,
                    title: row ? row.subject : this.terms["common.messages.new"],
                    id: row ? row.messageId : 0
                });
            });
        });
    }

    private setMessagesAsRead() {
        this.validateSelectedAgainstFiltered().then(passed => {
            if (passed) {
                this.progress.startSaveProgress((completion) => {
                    this.coreService.setMessagesAsRead(this.selectedRows.map(r => r.messageId)).then(result => {
                        if (result.success) {
                            this.loadMessages();
                            completion.completed(null, true);
                        } else {
                            completion.failed(result.errorMessage);
                        };
                    }, error => {
                        completion.failed(error.message);
                    });
                }, null);
            }
        });
    }

    private setMessagesAsUnread() {
        this.validateSelectedAgainstFiltered().then(passed => {
            if (passed) {
                this.progress.startSaveProgress((completion) => {
                    this.coreService.setMessagesAsUnread(this.selectedRows.map(r => r.messageId)).then(result => {
                        if (result.success) {
                            this.loadMessages();
                            completion.completed(null, true);
                        } else {
                            completion.failed(result.errorMessage);
                        };
                    }, error => {
                        completion.failed(error.message);
                    });
                }, null);
            }
        });
    }

    private deleteMessages() {
        this.validateSelectedAgainstFiltered().then(passed => {
            if (passed) {
                this.progress.startDeleteProgress((completion) => {
                    this.coreService.deleteMessages(this.selectedRows.map(r => r.messageId), this.selectedType.id === XEMailType.Incoming).then(result => {
                        if (result.success) {
                            this.loadMessages();
                            completion.completed(null, true);
                        } else {
                            completion.failed(result.errorMessage);
                        };
                    }, error => {
                        completion.failed(error.message);
                    });
                }, null, this.terms["core.deleteselectedwarning"]);
            }
        });
    }

    // HELP-METHODS

    private validateSelectedAgainstFiltered(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        if (this.allSelectedIncludedInFiltered) {
            deferral.resolve(true);
        } else {
            let filteredIds = this.filteredRows.map(r => r.messageId);
            let selectedIds = this.selectedRows.map(r => r.messageId);
            this.notificationService.showDialogEx(this.terms["core.warning"], this.terms["core.selectedmessagesmismatch"].format(selectedIds.length.toString(), filteredIds.length.toString()), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result.then(val => {
                deferral.resolve(val);
            }, (cancel) => {
                deferral.resolve(false);
            });
        }

        return deferral.promise;
    }

    private get selectedCount(): number {
        return this.gridHandler.gridAg.options.getSelectedCount();
    }

    private get selectedRows(): MessageGridDTO[] {
        return this.gridHandler.gridAg.options.getSelectedRows();
    }

    private get filteredRows(): MessageGridDTO[] {
        return this.gridHandler.gridAg.options.getFilteredRows();
    }

    private get allSelectedIncludedInFiltered(): boolean {
        let filteredIds = this.filteredRows.map(r => r.messageId);
        let selectedIds = this.selectedRows.map(r => r.messageId);

        let allIncluded = true;
        selectedIds.forEach(id => {
            if (!_.includes(filteredIds, id)) {
                allIncluded = false;
                return false;
            }
        });

        return allIncluded
    }
}

