import { RightMenuControllerBase, RightMenuType } from "../RightMenuControllerBase";
import { IUrlHelperService } from "../../Services/UrlHelperService";
import { Feature, XEMailAnswerType, XEMailType } from "../../../Util/CommonEnumerations";
import { DocumentDTO, DocumentFolder } from "../../../Common/Models/DocumentDTOs";
import { IContextMenuHandler } from "../../Handlers/ContextMenuHandler";
import { IContextMenuHandlerFactory } from "../../Handlers/ContextMenuHandlerFactory";
import { IMessagingService } from "../../Services/MessagingService";
import { ITranslationService } from "../../Services/TranslationService";
import { INotificationService } from "../../Services/NotificationService";
import { ICoreService } from "../../Services/CoreService";
import { ILazyLoadService } from "../../Services/LazyLoadService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Constants } from "../../../Util/Constants";
import { SOEMessageBoxImage } from "../../../Util/Enumerations";
import { EditController } from "./EditController";
import { EditController as MessageEditController } from "../MessageMenu/EditController";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { IStorageService } from "../../Services/StorageService";

declare var pdfjsLib;

export class DocumentMenuDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('/Core/RightMenu/DocumentMenu/DocumentMenu.html'),
            scope: {
                positionIndex: "@",
                feature: "@"
            },
            restrict: 'E',
            replace: true,
            controller: DocumentMenuController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class DocumentMenuController extends RightMenuControllerBase {

    // Init parameters
    private feature: Feature;

    // Terms
    private terms: { [index: string]: string; };
    private toggleTooltip: string;

    // Permissions
    private modifyPermission: boolean = false;

    // Data
    private nbrOfUnreadCompanyDocuments: number = 0;
    private folders: DocumentFolder[] = [];
    private documents: DocumentDTO[] = [];
    private selectedDocument: DocumentDTO;
    private pdf: any;
    private image: any;

    // Properties
    private types: any[] = [];
    private selectedType: any;

    private get isGeneralType(): boolean {
        return this.selectedType && this.selectedType.id === DocumentMenuTypes.General;
    }
    private get isEmployeeType(): boolean {
        return this.selectedType && this.selectedType.id === DocumentMenuTypes.Employee;
    }

    // Flags
    private loadingDocuments: boolean = false;

    private contextMenuHandler: IContextMenuHandler;

    private htmlEditorLoaderPromise: Promise<any>;

    // Timer
    private unreadTimer;
    readonly UNREAD_TIMER_INTERVAL: number = (60 * 1000 * 10);  // 10 minutes

    // Legacy parameters
    private openFolder: string;
    private editDocumentId: number;
    private createNewDocument: boolean;

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        messagingService: IMessagingService,
        private $scope: ng.IScope,
        private $interval: ng.IIntervalService,
        private $q: ng.IQService,
        private $uibModal: ng.ui.bootstrap.IModalService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private storageService: IStorageService,
        private contextMenuHandlerFactory: IContextMenuHandlerFactory,
        private lazyLoadService: ILazyLoadService) {
        super($timeout, messagingService, RightMenuType.Document);

        this.$q.all([
            this.loadTerms()
        ]).then(() => {
            this.setupUnreadTimer();
        });
    }


    public $onInit() {
        this.setTopPosition();
        this.htmlEditorLoaderPromise = this.lazyLoadService.loadBundle("Soe.Common.HtmlEditor.Bundle");

        this.messagingService.subscribe(Constants.EVENT_TOGGLE_DOCUMENT_MENU, (data: any) => {
            this.toggleShowMenu();
        });

        this.messagingService.subscribe(Constants.EVENT_SHOW_DOCUMENT_MENU, (data: any) => {
            if (!this.showMenu)
                this.toggleShowMenu();

            if (data) {
                this.openFolder = data.folder;
                this.editDocumentId = data.id;
                this.createNewDocument = data.createNew;
                this.selectType(data.tab);
            }
        });
    }

    // SETUP

    private setupUnreadTimer() {
        this.loadNbrOfUnreadCompanyDocuments(true);

        this.unreadTimer = this.$interval(() => {
            this.loadNbrOfUnreadCompanyDocuments(true);
        }, this.UNREAD_TIMER_INTERVAL);

        this.$scope.$on('$destroy', () => {
            this.$interval.cancel(this.unreadTimer);
        });
    }

    private init() {
        this.$q.all([
            this.loadModifyPermissions()
        ]).then(() => {
            this.setupTypes();
            this.setupContextMenu();
        });
    }

    private setupTypes() {
        this.types = [];
        this.types.push({ id: DocumentMenuTypes.General, title: this.terms["core.documentmenu.type.general"], selected: true });
        this.types.push({ id: DocumentMenuTypes.Employee, title: this.terms["core.documentmenu.type.employee"], selected: false });
        this.selectType(this.types[0], this.showMenu);
    }

    private setupContextMenu() {
        this.contextMenuHandler = this.contextMenuHandlerFactory.create();
    }

    private getContextMenuOptions(document: DocumentDTO): any[] {
        this.createContextMenuOptions(document);
        return this.contextMenuHandler.getContextMenuOptions();
    }

    private createContextMenuOptions(document: DocumentDTO) {
        this.contextMenuHandler.clearContextMenuItems();
        if (this.modifyPermission) {
            this.contextMenuHandler.addContextMenuItem(this.terms["core.document.new"], 'fa-plus', ($itemScope, $event, modelValue) => { this.edit(null); }, () => { return true; });
            this.contextMenuHandler.addContextMenuItem(this.terms["core.document.edit"], 'fa-pencil iconEdit', ($itemScope, $event, modelValue) => { this.edit(document); }, () => { return true; });
            this.contextMenuHandler.addContextMenuSeparator();
        }
        this.contextMenuHandler.addContextMenuItem(this.terms["core.document.view"], 'fa-eye', ($itemScope, $event, modelValue) => { this.viewFile(document); }, () => { return this.canViewDocument(document); });
        this.contextMenuHandler.addContextMenuItem(this.terms["core.document.downloadfile"], 'fa-download', ($itemScope, $event, modelValue) => { this.downloadFile(document); }, () => { return true; });
        if (document.messageId)
            this.contextMenuHandler.addContextMenuItem(this.terms["core.document.viewmessage"], 'fa-envelope-open-text', ($itemScope, $event, modelValue) => { this.viewMessage(document); }, () => { return true; });
        if (document.needsConfirmation)
            this.contextMenuHandler.addContextMenuItem(this.terms["common.messages.sendconfirmation"], 'fa-file-check okColor', ($itemScope, $event, modelValue) => { this.setDocumentAsConfirmed(document); }, () => { return !!document.readDate && !document.answerDate; });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.documentmenu.type.general",
            "core.documentmenu.type.employee",
            "core.document.view",
            "core.document.new",
            "core.document.edit",
            "core.document.downloadfile",
            "core.document.viewmessage",
            "core.documentmenu.nofolder",
            "common.created",
            "common.messages.confirmed",
            "common.messages.notconfirmed",
            "common.messages.sendconfirmation"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];
        features.push(Feature.Manage_Preferences_UploadedFiles);

        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.modifyPermission = x[Feature.Manage_Preferences_UploadedFiles];
        });
    }

    private hasNewDocuments(useCache: boolean): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        if (useCache) {
            // Get last time checked from local storage
            var time = this.storageService.fetch('hasNewDocuments');
            if (!time)
                time = CalendarUtility.DefaultDateTime().toDateTimeString();

            this.coreService.hasNewDocuments(time).then(result => {
                // Update last time checked in local storage
                this.storageService.add('hasNewDocuments', new Date().toDateTimeString());
                deferral.resolve(result);
            });
        } else {
            // Force check
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private loadNbrOfUnreadCompanyDocuments(useCache: boolean) {
        this.hasNewDocuments(useCache).then(hasNewInformation => {
            // If new documents exists, do not use cache to get number of unread documents
            this.coreService.getNbrOfUnreadCompanyDocuments(!hasNewInformation).then(x => {
                this.nbrOfUnreadCompanyDocuments = x;

                let keys: string[] = ["core.documentmenu.title", "core.documentmenu.unread"];
                this.translationService.translateMany(keys).then(terms => {
                    this.toggleTooltip = terms["core.documentmenu.title"]
                    if (this.nbrOfUnreadCompanyDocuments > 0)
                        this.toggleTooltip += " ({0} {1})".format(this.nbrOfUnreadCompanyDocuments.toString(), terms["core.documentmenu.unread"].toLocaleLowerCase());
                });
            });
        });
    }

    private loadDocuments() {
        if (this.loadingDocuments)
            return;

        this.loadingDocuments = true;

        switch (this.selectedType.id) {
            case DocumentMenuTypes.General:
                this.coreService.getCompanyDocuments().then(x => {
                    this.documents = x;

                    _.forEach(this.documents, document => {
                        let recipient = _.find(document.recipients, r => r.userId === CoreUtility.userId);
                        if (recipient) {
                            document.readDate = recipient.readDate;
                            document.answerDate = recipient.confirmedDate;
                            document.answerType = document.answerDate ? XEMailAnswerType.Yes : XEMailAnswerType.No;
                        }
                    });

                    this.documentsLoaded();
                });
                break;
            case DocumentMenuTypes.Employee:
                this.coreService.getMyDocuments().then(x => {
                    this.documents = x;
                    this.documentsLoaded();
                });
                break;
            default:
                this.loadingDocuments = false;
        }
    }

    private setDocumentAsRead(document: DocumentDTO) {
        if (document.readDate)
            return;

        this.coreService.setDocumentAsRead(document.dataStorageId, false).then((result) => {
            if (result.success) {
                this.loadNbrOfUnreadCompanyDocuments(false);

                document.readDate = CalendarUtility.convertToDate(result.dateTimeValue);
                let folder = _.find(this.folders, f => f.name === document.folder);
                if (folder && folder.nbrOfUnread > 0)
                    folder.nbrOfUnread--;
            }
        });
    }

    private setDocumentAsConfirmed(document: DocumentDTO) {
        if (!document.readDate || document.answerDate)
            return;

        this.coreService.setDocumentAsRead(document.dataStorageId, true).then((result) => {
            if (result.success && this.isGeneralType) {
                document.answerDate = CalendarUtility.convertToDate(result.dateTimeValue);
                document.answerType = XEMailAnswerType.Yes;
            }
        });
    }

    // ACTIONS

    private edit(document: DocumentDTO) {
        if (!this.modifyPermission)
            return;

        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Core/RightMenu/DocumentMenu/edit.html"),
            controller: EditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            scope: this.$scope,
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                modal: modal,
                id: document ? document.dataStorageId : 0
            });
        });

        modal.result.then((result) => {
            if (result)
                this.loadDocuments();
        });
    }

    private viewMessage(document: DocumentDTO) {
        this.$q.all([this.htmlEditorLoaderPromise]).then(() => {
            var modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Core/RightMenu/MessageMenu/edit.html"),
                controller: MessageEditController,
                controllerAs: 'ctrl',
                bindToController: true,
                backdrop: 'static',
                size: 'xl',
                windowClass: 'fullsize-modal',
                scope: this.$scope,
            });

            modal.rendered.then(() => {
                this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, {
                    source: 'DocumentMenu',
                    modal: modal,
                    type: XEMailType.Incoming,
                    title: document.description,
                    id: document.messageId
                });
            });

            modal.result.then(result => {
                this.loadDocuments();
            }, () => {
                this.loadDocuments();
            });
        });
    }

    // EVENTS

    protected toggleShowMenu() {
        super.toggleShowMenu();

        if (this.showMenu && this.types.length === 0)
            this.init();
    }

    private selectType(type, loadDocuments: boolean = true) {
        if (this.selectedType == type || this.loadingDocuments)
            return;

        _.forEach(this.types, p => {
            p['selected'] = false;
        });
        type['selected'] = true;

        this.selectedType = type;

        if (loadDocuments)
            this.loadDocuments();
    }

    private toggleFolder(folder: DocumentFolder) {
        folder.expanded = !folder.expanded;
    }

    private viewFile(document: DocumentDTO) {
        if (!this.canViewDocument(document))
            return;

        if (!this.fullscreen)
            this.toggleFullscreen();

        this.selectedDocument = document;

        this.coreService.getDocumentData(document.dataStorageId).then(data => {
            if (data)
                this.setDocumentAsRead(document);

            this.pdf = document.isPdf ? data : null;
            this.image = document.isImage ? data : null;
        });
    }

    private downloadFile(document: DocumentDTO) {
        this.coreService.getDocumentUrl(document.dataStorageId).then(x => {
            if (x) {
                window.location.assign(x);
                this.setDocumentAsRead(document);
            } else {
                let keys: string[] = ["core.document.filenotfound.title", "core.document.filenotfound.message"];
                this.translationService.translateMany(keys).then(terms => {
                    this.notificationService.showDialogEx(terms["core.document.filenotfound.title"], terms["core.document.filenotfound.message"].format(document.displayName), SOEMessageBoxImage.Error);
                });
            }
        });
    }

    // HELP-METHODS

    private documentsLoaded() {
        // Create distinct collection of folder names from loaded documents
        _.filter(this.documents, d => !d.folder).forEach(d => d.folder = this.terms["core.documentmenu.nofolder"]);

        this.folders = [];
        let folderNames = _.orderBy(Array.from(new Set(_.map(this.documents, f => f.folder))));
        _.forEach(folderNames, folderName => {
            let folder = new DocumentFolder(folderName);
            folder.expanded = this.documents.length < 11;
            folder.nbrOfUnread = this.nbrOfUnreadDocumentsInFolder(folderName);
            this.folders.push(folder);
        })

        this.loadingDocuments = false;

        if (this.openFolder) {
            let fld = this.folders.find(f => f.name === this.openFolder);
            if (fld)
                fld.expanded = true;

            this.openFolder = undefined;
        }

        if (this.editDocumentId || this.createNewDocument) {
            let doc = new DocumentDTO();
            doc.dataStorageId = this.editDocumentId;
            this.edit(doc);
            this.editDocumentId = undefined;
        }
    }

    private documentsInFolder(folderName: string): DocumentDTO[] {
        return _.filter(this.documents, f => f.folder === folderName);
    }

    private nbrOfUnreadDocumentsInFolder(folderName: string): number {
        return _.filter(this.documentsInFolder(folderName), i => !i.readDate).length;
    }

    private canViewDocument(document: DocumentDTO): boolean {
        return document.isPdf || document.isImage;
    }

    private getDocumentToolTip(document: DocumentDTO): string {
        if (document.created)
            return "{0} {1}".format(this.terms["common.created"], document.created.toFormattedDate());
        else
            return null;
    }

    private getConfirmedTooltip(document: DocumentDTO): string {
        if (document.answerDate)
            return "{0} {1}".format(this.terms["common.messages.confirmed"], document.answerDate.toFormattedDateTime());
        else
            return this.terms["common.messages.notconfirmed"];
    }
}

export enum DocumentMenuTypes {
    General = 1,
    Employee = 2
}