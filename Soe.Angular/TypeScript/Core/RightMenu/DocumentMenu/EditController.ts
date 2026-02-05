import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Constants } from "../../../Util/Constants";
import { IFocusService } from "../../../Core/Services/focusservice";
import { Feature } from "../../../Util/CommonEnumerations";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { StringUtility, Guid } from "../../../Util/StringUtility";
import { DocumentDTO, DataStorageRecipientDTO } from "../../../Common/Models/DocumentDTOs";
import { ISmallGenericType, IActionResult } from "../../../Scripts/TypeLite.Net4";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Modal
    private modal;

    // Data
    private folders: string[] = [];
    private dataStorageId: number;
    private document: DocumentDTO;
    private fileData: any;
    private recipients: DataStorageRecipientDTO[] = [];
    private recipientFilters: ISmallGenericType[] = [];

    // Properties
    private _selectedFolder: string;
    private get selectedFolder(): string {
        return this._selectedFolder;
    }
    private set selectedFolder(item: string) {
        if (item && this.document) {
            this.document.folder = item;
        }
    }

    private selectableMessageGroups: ISmallGenericType[] = [];
    private selectedMessageGroups: ISmallGenericType[] = [];

    private recipientFilter: RecipientFilter;

    // Flags
    private loadingDocument: boolean = true;
    private loadingRecipients: boolean = false;
    private recipientsLoaded: boolean = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout:ng.ITimeoutService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private focusService: IFocusService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.modal = parameters.modal;
            this.onInit(parameters);
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.dataStorageId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Preferences_UploadedFiles, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Preferences_UploadedFiles].readPermission;
        this.modifyPermission = response[Feature.Manage_Preferences_UploadedFiles].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadFolders(),
            this.loadMessageGroups()
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.dataStorageId) {
            return this.load();
        } else {
            this.new();
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.document.recipientfilter.all",
            "core.document.recipientfilter.unread",
            "core.document.recipientfilter.read",
            "core.document.recipientfilter.readnotconfirmed",
            "core.document.recipientfilter.confirmed"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadFolders(): ng.IPromise<any> {
        return this.coreService.getDocumentFolders().then(x => {
            this.folders = x;
        });
    }

    private loadMessageGroups(): ng.IPromise<any> {
        return this.coreService.getMessageGroupsDict(false).then(x => {
            this.selectableMessageGroups = x;
        });
    }

    private load(): ng.IPromise<any> {
        this.loadingDocument = true;

        return this.coreService.getDocument(this.dataStorageId).then(x => {
            this.isNew = false;
            this.document = x;

            if (this.document.messageGroupIds && this.selectableMessageGroups)
                this.selectedMessageGroups = _.filter(this.selectableMessageGroups, g => _.includes(this.document.messageGroupIds, g.id));

            this.setupRecipientFilter();

            this.loadingDocument = false;
            this.dirtyHandler.clean();
        });
    }

    private new() {
        this.isNew = true;
        this.dataStorageId = 0;
        this.document = new DocumentDTO();
        this.setupRecipientFilter();

        this.loadingDocument = false;
    }

    private getDocumentRecipientInfo(): ng.IPromise<any> {
        this.loadingRecipients = true;
        return this.coreService.getDocumentRecipientInfo(this.dataStorageId).then(x => {
            this.recipients = x;
            this.recipientsLoaded = true;
            this.loadingRecipients = false;
        });
    }

    // EVENTS

    private needsConfirmationChanged() {
        this.$timeout(() => {
            this.setupRecipientFilter();
        });
    }

    private openUploadFileDialog() {
        this.translationService.translate("core.document.choosefile").then(term => {
            var url = CoreUtility.apiPrefix + Constants.WEBAPI_CORE_DOCUMENT_UPLOAD;
            var modal = this.notificationService.showFileUploadEx(term, { url: url, allowMultipleFiles: false });
            modal.result.then(result => {
                let res: IActionResult = result.result;
                if (res && res.success) {
                    this.fileData = res.value;
                    this.document.fileName = res.value2;
                    this.document.extension = StringUtility.getFileExtension(this.document.fileName);
                    if (this.document.extension)
                        this.document.extension = "." + this.document.extension;
                    // Set name from filename
                    if (!this.document.name) {
                        this.document.name = this.document.fileName;
                        if (this.document.extension && this.document.fileName.length > this.document.extension.length)
                            this.document.name = this.document.name.left(this.document.name.length - this.document.extension.length);
                    }
                    this.setDirty();
                    this.focusService.focusById("ctrl_document_name");
                }
            }, error => {
            });
        });
    }

    private downloadFile() {
        this.coreService.getDocumentUrl(this.dataStorageId).then(x => {
            if (x)
                window.location.assign(x);
            else
                console.log("File not found", this.dataStorageId);
        });
    }

    private showRecipients() {
        if (!this.recipientsLoaded)
            this.getDocumentRecipientInfo();
    }

    // ACTIONS

    private initSave() {
        if (this.validateSave())
            this.save();
    }

    private save() {
        this.document.messageGroupIds = this.selectedMessageGroups.map(g => g.id);

        this.progress.startSaveProgress((completion) => {
            this.coreService.saveDocument(this.document, this.fileData).then(result => {
                if (result.success) {
                    this.dataStorageId = result.integerValue;
                    this.document.dataStorageId = this.dataStorageId;
                    this.fileData = null;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.document);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.modal.close(true);
        }, error => {
        });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.coreService.deleteDocument(this.dataStorageId).then(result => {
                if (result.success) {
                    completion.completed(this.document, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.modal.close(true);
        });
    }

    // HELP-METHODS

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    private setupRecipientFilter() {
        this.recipientFilters = [];
        this.recipientFilters.push({ id: RecipientFilter.All, name: this.terms["core.document.recipientfilter.all"] });
        this.recipientFilters.push({ id: RecipientFilter.Unread, name: this.terms["core.document.recipientfilter.unread"] });
        if (this.document.needsConfirmation) {
            this.recipientFilters.push({ id: RecipientFilter.ReadNotConfirmed, name: this.terms["core.document.recipientfilter.readnotconfirmed"] });
            this.recipientFilters.push({ id: RecipientFilter.Confirmed, name: this.terms["core.document.recipientfilter.confirmed"] });
        } else {
            this.recipientFilters.push({ id: RecipientFilter.Read, name: this.terms["core.document.recipientfilter.read"] });
        }

        this.recipientFilter = RecipientFilter.All;
    }

    private get filteredRecipients(): DataStorageRecipientDTO[] {
        switch (this.recipientFilter) {
            case RecipientFilter.All:
                return this.recipients;
            case RecipientFilter.Unread:
                return _.filter(this.recipients, r => !r.readDate);
            case RecipientFilter.Read:
                return _.filter(this.recipients, r => r.readDate);
            case RecipientFilter.ReadNotConfirmed:
                return _.filter(this.recipients, r => r.readDate && !r.confirmedDate);
            case RecipientFilter.Confirmed:
                return _.filter(this.recipients, r => r.readDate && r.confirmedDate);
        }
    }

    // VALIDATION

    private validateSave(): boolean {
        return true;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.document) {
                var errors = this['edit'].$error;

                if (!this.document.fileName)
                    mandatoryFieldKeys.push("common.filename");

                if (errors['dateRange'])
                    validationErrorKeys.push("error.invaliddaterange");
            }
        });
    }
}

export enum RecipientFilter {
    All = 0,
    Unread = 1,
    Read = 2,
    ReadNotConfirmed = 3,
    Confirmed = 4
}