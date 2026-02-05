import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ISystemService } from "../../SystemService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { Constants } from "../../../../Util/Constants";
import { IFocusService } from "../../../../Core/Services/focusservice";
import { Feature, TermGroup, TermGroup_InformationSeverity, TermGroup_InformationStickyType, SoeInformationType, SoeInformationSourceType } from "../../../../Util/CommonEnumerations";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { StringUtility, Guid } from "../../../../Util/StringUtility";
import { InformationDTO, InformationRecipientDTO } from "../../../../Common/Models/InformationDTOs";
import { TinyMCEUtility } from "../../../../Util/TinyMCEUtility";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { SelectSysFeatureDialogController } from "./Dialogs/SelectSysFeature/SelectSysFeatureDialogController";
import { NotificationDetailsController } from "./Dialogs/NotificationDetails/NotificationDetailsController";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };
    private shortTextInfo: string;

    // Data
    private sysInformationId: number;
    private information: InformationDTO;
    private languages: ISmallGenericType[];
    private severities: ISmallGenericType[];
    private stickyTypes: ISmallGenericType[];
    private folders: string[] = [];
    private recipients: InformationRecipientDTO[] = [];
    private recipientFilters: ISmallGenericType[] = [];

    // Properties
    private _selectedFolder: string;
    private get selectedFolder(): string {
        return this._selectedFolder;
    }
    private set selectedFolder(item: string) {
        if (item && this.information) {
            this.information.folder = item;
        }
    }

    private selectableSysCompDbs: ISmallGenericType[] = [];
    private selectedSysCompDbs: ISmallGenericType[] = [];
    private selectableSysFeatures: ISmallGenericType[] = [];
    private selectedSysFeatures: ISmallGenericType[] = [];

    private recipientFilter: RecipientFilter;

    // Flags
    private loadingInformation: boolean = true;
    private loadingRecipients: boolean = false;
    private recipientsLoaded: boolean = false;
    private hasConfirmations: boolean = false;

    private tinyMceOptions: any;

    private modal;
    private isModal = false;
    private modalInstance: any;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        protected $uibModal,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private systemService: ISystemService,
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

        this.modalInstance = $uibModal;

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
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
        this.sysInformationId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_System, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_System].readPermission;
        this.modifyPermission = response[Feature.Manage_System].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
    }

    private setupTinyMCE() {
        if (this.modifyPermission && !this.hasConfirmations) {
            this.tinyMceOptions = TinyMCEUtility.setupDefaultOptions();

            this.$timeout(() => {
                this.focusService.focusByName("ctrl_information_subject");
            }, 500);
        }
    }

    private onDoLookups() {
        return this.$q.all([
            this.loadTerms(),
            this.loadLanguages(),
            this.loadSeverities(),
            this.loadStickyTypes(),
            this.loadFolders(),
            this.loadSysCompDbs(),
            this.loadSysFeatures()
        ]).then(() => {
        });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.sysInformationId) {
            return this.load();
        } else {
            return this.new();
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.informationmenu.companyinformation.shorttext.info",
            "core.document.recipientfilter.all",
            "core.document.recipientfilter.unread",
            "core.document.recipientfilter.read",
            "core.document.recipientfilter.readnotconfirmed",
            "core.document.recipientfilter.confirmed"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.shortTextInfo = this.terms["core.informationmenu.companyinformation.shorttext.info"];

            this.recipientFilters.push({ id: RecipientFilter.All, name: terms["core.document.recipientfilter.all"] });
            this.recipientFilters.push({ id: RecipientFilter.Unread, name: terms["core.document.recipientfilter.unread"] });
            this.recipientFilters.push({ id: RecipientFilter.Read, name: terms["core.document.recipientfilter.read"] });
            this.recipientFilters.push({ id: RecipientFilter.Confirmed, name: terms["core.document.recipientfilter.confirmed"] });
            this.recipientFilter = RecipientFilter.All;
        });
    }

    private loadLanguages(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.Language, false, false, true).then(x => {
            this.languages = x;
        });
    }

    private loadSeverities(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InformationSeverity, false, false, true).then((x => {
            this.severities = x;
        }));
    }

    private loadStickyTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InformationStickyType, false, false, true).then((x => {
            this.stickyTypes = x;
        }));
    }

    private loadFolders(): ng.IPromise<any> {
        return this.systemService.getSysInformationFolders().then(x => {
            this.folders = x;
        });
    }

    private loadSysCompDbs(): ng.IPromise<any> {
        return this.systemService.getSysInformationSysCompDbs().then(x => {
            this.selectableSysCompDbs = x;
        });
    }

    private loadSysFeatures(): ng.IPromise<any> {
        return this.systemService.getSysInformationSysFeatures().then(x => {
            this.selectableSysFeatures = x;
        });
    }

    private load(): ng.IPromise<any> {
        this.loadingInformation = true;

        return this.systemService.getSysInformationForEdit(this.sysInformationId).then(x => {
            this.isNew = false;
            this.information = x;

            if (this.information.sysCompDbIds && this.selectableSysCompDbs)
                this.selectedSysCompDbs = _.filter(this.selectableSysCompDbs, g => _.includes(this.information.sysCompDbIds, g.id));
            if (this.information.sysFeatureIds && this.selectableSysFeatures)
                this.selectedSysFeatures = _.filter(this.selectableSysFeatures, g => _.includes(this.information.sysFeatureIds, g.id));

            this.systemService.getSysInformationHasConfirmations(this.sysInformationId).then(value => {
                this.hasConfirmations = value;
                this.setupTinyMCE();
            });

            this.setupRecipientFilter();

            this.loadingInformation = false;
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.information.subject);
        });
    }

    private new(): ng.IPromise<any> {
        return this.$timeout(() => {
            this.isNew = true;
            this.sysInformationId = 0;
            this.information = new InformationDTO();
            this.information.type = SoeInformationType.Information;
            this.information.sourceType = SoeInformationSourceType.Company;
            this.information.sysLanguageId = CoreUtility.languageId;
            this.information.severity = TermGroup_InformationSeverity.Information;
            this.information.stickyType = TermGroup_InformationStickyType.CanHide;

            this.setupRecipientFilter();

            this.loadingInformation = false;
            this.setupTinyMCE();
        });
    }

    private getInformationRecipientInfo(): ng.IPromise<any> {
        this.loadingRecipients = true;
        return this.systemService.getSysInformationRecipientInfo(this.sysInformationId).then(x => {
            this.recipients = x;
            this.recipientsLoaded = true;
            this.loadingRecipients = false;
        });
    }

    // ACTIONS

    private save() {
        if (!this.information.showInWeb && !this.information.showInMobile && !this.information.showInTerminal) {
            let keys: string[] = [
                "core.informationmenu.companyinformation.nopublication.title",
                "core.informationmenu.companyinformation.nopublication.message"
            ];

            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialogEx(terms["core.informationmenu.companyinformation.nopublication.title"], terms["core.informationmenu.companyinformation.nopublication.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val)
                        this.doSave();
                });
            });
        } else {
            this.doSave();
        }
    }

    private doSave() {
        this.information.sysCompDbIds = this.selectedSysCompDbs.map(g => g.id);
        this.information.sysFeatureIds = this.selectedSysFeatures.map(g => g.id);

        this.progress.startSaveProgress((completion) => {
            this.systemService.saveSysInformation(this.information).then(result => {
                if (result.success) {
                    this.sysInformationId = result.integerValue;
                    this.information.informationId = this.sysInformationId;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.information);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            if (this.isModal)
                this.closeModal(true);
            else
                this.load();
        }, error => {
        });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.systemService.deleteSysInformation(this.information.informationId).then(result => {
                if (result.success) {
                    completion.completed(this.information, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            if (this.isModal)
                this.closeModal(true);
            else
                super.closeMe(true);
        });
    }

    protected copy() {
        super.copy();

        this.information.informationId = 0;
        this.information.notificationSent = null;
        this.information.created = null;
        this.information.createdBy = null;
        this.information.modified = null;
        this.information.modifiedBy = null;
        this.isNew = true;
        this.dirtyHandler.isDirty = true;

        this.$timeout(() => {
            this.focusService.focusByName("ctrl_information_subject");
        });
    }

    private showNotificationDetails() {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/System/Admin/SysInformation/Dialogs/NotificationDetails/NotificationDetails.html"),
            controller: NotificationDetailsController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                information: () => { return this.information },
                modifyPermission: () => { return this.modifyPermission }
            }
        }
        this.$uibModal.open(options).result.then(result => {
            if (result && result.deleted) {
                if (!this.dirtyHandler.isDirty)
                    this.load();
            }
        }, (reason) => { });
    }

    private deleteNotificationSent() {
        let keys: string[] = [
            "core.informationmenu.companyinformation.notificationsent.delete",
            "core.informationmenu.companyinformation.notificationsent.delete.message"
        ];

        this.translationService.translateMany(keys).then(terms => {
            var modal = this.notificationService.showDialogEx(terms["core.informationmenu.companyinformation.notificationsent.delete"], terms["core.informationmenu.companyinformation.notificationsent.delete.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.systemService.deleteSysInformationNotificationSent(this.sysInformationId, 0).then(result => {
                        if (result.success) {
                            if (!this.dirtyHandler.isDirty)
                                this.load();
                            else {
                                this.information.notificationSent = null;
                                _.forEach(this.information.sysInformationSysCompDbs, compDb => {
                                    compDb.notificationSent = null;
                                });
                            }
                        }
                    });
                }
            });
        });
    }

    // EVENTS

    private severityChanged() {
        this.$timeout(() => {
            if (this.information.isSeverityEmergency)
                this.information.needsConfirmation = true;
        });
    }

    private needsConfirmationChanged() {
        this.$timeout(() => {
            this.setupRecipientFilter();
        });
    }

    private showInMobileChanged() {
        this.$timeout(() => {
            if (!this.information.showInMobile)
                this.information.notify = false;
        });
    }

    private selectSysFeatures() {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Manage/System/Admin/SysInformation/Dialogs/SelectSysFeature/SelectSysFeatureDialog.html"),
            controller: SelectSysFeatureDialogController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                selectableSysFeatures: () => { return this.selectableSysFeatures },
                selectedSysFeatures: () => { return this.selectedSysFeatures }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                this.selectedSysFeatures = result.selectedSysFeatures;
                this.setDirty();
            }
        });
    }

    private closeModal(modified: boolean) {
        if (this.isModal) {
            if (this.sysInformationId) {
                this.modal.close({ modified: modified, id: this.sysInformationId });
            } else {
                this.modal.dismiss();
            }
        }
    }

    // HELP-METHODS

    private showRecipients() {
        if (!this.recipientsLoaded)
            this.getInformationRecipientInfo();
    }

    private setupRecipientFilter() {
        this.recipientFilters = [];
        this.recipientFilters.push({ id: RecipientFilter.All, name: this.terms["core.document.recipientfilter.all"] });
        this.recipientFilters.push({ id: RecipientFilter.Unread, name: this.terms["core.document.recipientfilter.unread"] });
        if (this.information.needsConfirmation) {
            this.recipientFilters.push({ id: RecipientFilter.ReadNotConfirmed, name: this.terms["core.document.recipientfilter.readnotconfirmed"] });
            this.recipientFilters.push({ id: RecipientFilter.Confirmed, name: this.terms["core.document.recipientfilter.confirmed"] });
        } else {
            this.recipientFilters.push({ id: RecipientFilter.Read, name: this.terms["core.document.recipientfilter.read"] });
        }

        this.recipientFilter = RecipientFilter.All;
    }

    private get filteredRecipients(): InformationRecipientDTO[] {
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

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.information) {
                if (!this.information.subject)
                    mandatoryFieldKeys.push("common.subject");
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
