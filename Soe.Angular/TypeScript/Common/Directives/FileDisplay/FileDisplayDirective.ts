import { IQService } from 'angular';
import '../../../Common/Dialogs/AddDocumentToAttestFlow/Module';

import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from '../../../Core/Services/NotificationService';
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Feature, SoeEntityType, TermGroup, TermGroup_AttestEntity, TermGroup_DataStorageRecordAttestStatus, TermGroup_FileDisplaySortBy } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { AddDocumentToAttestFlowController } from "../../Dialogs/AddDocumentToAttestFlow/AddDocumentToAttestFlowController";
import { SelectRolesController } from "../../Dialogs/SelectRoles/SelectRolesController";
import { ShowDocumentSigningStatusController } from '../../Dialogs/ShowDocumentSigningStatus/ShowDocumentSigningStatusController';
import { ShowImageController } from "../../Dialogs/ShowImage/ShowImageController";
import { ImageDTO } from "../../Models/ImageDTO";
import { MessageAttachmentDTO } from "../../Models/MessageDTOs";
import { SmallGenericType } from '../../Models/SmallGenericType';

export class FileDisplayDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('FileDisplay', 'FileDisplay.html'),
            scope: {
                useGrid: '@',
                files: '=',
                linkedToUserId: '=?',
                nbrOfFilesChanged: '&',
                fileNameChanged: '&',
                rolesChanged: '&',
                onReload: '&',
                showRoles: '=',
                rolesMandatory: '=',
                permission: '=',
                readOnly: '=',
                useSigning: '=',
                useConfirmation: '=',
                changeRole: '='
            },
            restrict: 'E',
            replace: true,
            controller: FileDisplayController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

class FileDisplayController extends GridControllerBase2Ag implements ICompositionGridController {

    // Init parameters
    private useGrid: boolean;
    private files: any[];
    private linkedToUserId: number;
    private activeFiles: any[] = [];
    private showRoles: boolean;
    private rolesMandatory: boolean;
    private permission: any;
    private readOnly: boolean;
    private fileCount: number = 0;
    private useConfirmation = false;
    private useSigning = false;
    private changeRole = false;
    // Terms
    private terms: { [index: string]: string; };
    private attestStatuses: SmallGenericType[] = [];

    // Permissions
    private initSigningDocumentPermission: boolean = false;

    // Properties
    private sortBy: TermGroup_FileDisplaySortBy = TermGroup_FileDisplaySortBy.Description;
    private resetGridData: boolean = false;

    // Flags
    private hasSigningTemplates: boolean = false;

    // Events
    nbrOfFilesChanged: (nbrOfFiles: any) => void;
    fileNameChanged: (file: any) => void;
    rolesChanged: (file: any) => void;
    onReload: () => void;

    private modalInstance: any;

    //@ngInject
    constructor(
        private $window,
        private $uibModal,
        private $q: IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private messagingService: IMessagingService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Common.Directives.Documents", progressHandlerFactory, messagingHandlerFactory);
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())

        this.modalInstance = $uibModal;

        this.onInit({});
    }

    // SETUP

    $onInit() {
        if (!this.files)
            this.files = [];

        let queue = [];
        queue.push(this.loadTerms());
        if (this.useGrid) {
            queue.push(this.getHasTemplates());
            queue.push(this.loadAttestStatuses());
        }

        this.$q.all(queue).then(() => {
            this.flowHandler.start([
                { feature: this.permission, loadReadPermissions: true, loadModifyPermissions: true },
                { feature: Feature.Time_Employee_Employees_Edit_OtherEmployees_Files_InitSigning, loadModifyPermissions: true, loadReadPermissions: true }
            ]);
            this.setupWatchers();
        });
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.progress.setProgressBusy(true);
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.files, () => {
            _.forEach(this.files, file => {
                file.created = CalendarUtility.convertToDate(file.created);
            });
            this.updateActiveFiles();
        }, true);

        this.$scope.$watch(() => this.files.length, (newVal, oldVal) => {
            if (newVal) {
                this.handleRows();
                this.resetGridData = false;
            }
        });

        this.$scope.$watch(() => this.resetGridData, (newVal, oldVal) => {
            if (newVal) {
                this.handleRows();
                this.resetGridData = false;
            }
        });

        this.$scope.$on('stopEditing', (e, a) => {
            if (this.gridAg && this.gridAg.options) {
                this.gridAg.options.stopEditing(false);
                this.$timeout(() => {
                    a.functionComplete();
                }, 100)
            }
        });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[this.permission].readPermission;
        this.modifyPermission = response[this.permission].modifyPermission;

        this.initSigningDocumentPermission = response[Feature.Time_Employee_Employees_Edit_OtherEmployees_Files_InitSigning].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        //this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("core.fileupload.choosefilestoupload", "core.fileupload.choosefilestoupload", IconLibrary.FontAwesome, "fa-plus", () => { this.uploadItem(); }, null, () => { return this.readOnly; })));
    }

    private setupGrid(): void {
        if (!this.useGrid)
            return;

        this.gridAg.options.enableRowSelection = false;
        this.gridAg.options.setMinRowsToShow(10);
        this.gridAg.options.setTooltipDelay = 200;

        this.gridAg.addColumnText("fileName", this.terms["core.filename"], null, true, { editable: false });
        this.gridAg.addColumnText("description", this.terms["common.description"], null, true, { editable: (row) => row.canDelete && !this.readOnly, onChanged: () => this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.guid }) });
        this.gridAg.addColumnSelect("fileFormat", this.terms["common.fileextension"], 100, { enableHiding: true, displayField: "fileFormat", populateFilterFromGrid: true, selectOptions: [] });
        this.gridAg.addColumnDateTime("created", this.terms["common.created"], 100, true, null, { editable: false });
        if (this.useConfirmation) { 
            this.gridAg.addColumnBoolEx("needsConfirmation", this.terms["common.messages.needsconfirmation.short"], 80, { enableHiding: true });
            this.gridAg.addColumnDateTime("confirmedDate", this.terms["common.messages.confirmed"], 100, true, null, { editable: false });
        }
        this.gridAg.addColumnIcon("icon", null, null, { pinned: "right", toolTip: this.terms["common.download"], suppressFilter: true, onClick: this.show.bind(this) });
        if (this.hasSigningTemplates && this.useSigning) {
            this.gridAg.addColumnIcon("signIcon", null, null, { pinned: "right", toolTipField: "attestStatusText", suppressFilter: true, showIcon: (row) => row.attestStateId || this.initSigningDocumentPermission, onClick: this.signingDocumentStatusClicked.bind(this) });
            this.gridAg.addColumnShape("attestStateColor", null, 22, { pinned: "right", shape: Constants.SHAPE_CIRCLE, showIconField: "attestStateColor", shapeField: "attestStateColor", colorField: "attestStateColor", toolTipField: "attestStateName" });
        }
        if (!this.readOnly) {
            if (this.changeRole) {
                this.gridAg.addColumnIcon(null, null, null, { pinned: "right", icon: "fal fa-user-tag", toolTip: this.terms["common.editroles"], suppressFilter: true, showIcon: (row) => row.canDelete, onClick: this.editRoles.bind(this) });
            }
            this.gridAg.addColumnIcon(null, null, null, { pinned: "right", toolTip: this.terms["core.delete"], icon: "fal fa-times iconDelete", showIcon: (row) => row.canDelete, onClick: this.initDeleteFile.bind(this), suppressFilter: true });
        }

        let events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.show(row); }));
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        this.gridAg.options.subscribe(events);

        this.gridAg.finalizeInitGrid("core.document", true);

        this.$timeout(() => {
            this.handleRows();
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.delete",
            "core.filename",
            "common.messages.confirmed",
            "common.created",
            "common.description",
            "common.download",
            "common.editroles",
            "common.fileextension",
            "common.messages.needsconfirmation.short",
            "common.signdoc.init",
            "common.signdoc.initializer",
            "common.signdoc.onlyread",
            "common.signdoc.status"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private getHasTemplates(): ng.IPromise<any> {
        return this.coreService.hasAttestWorkFlowTemplates(TermGroup_AttestEntity.SigningDocument).then(x => {
            this.hasSigningTemplates = x;
        });
    }

    private loadAttestStatuses(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.DataStorageRecordAttestStatus, false, false).then(x => {
            this.attestStatuses = x;
        });
    }

    // EVENTS

    private setSortBy(index: number) {
        this.sortBy = index;
        this.updateActiveFiles();
    }

    private initDeleteFile(file: any) {
        if (file.attestStateId) {
            let keys: string[] = [
                "common.signdoc.deletewarning.title",
                "common.signdoc.deletewarning.message"
            ];

            return this.translationService.translateMany(keys).then(terms => {
                const modal = this.notificationService.showDialogEx(terms["common.signdoc.deletewarning.title"], terms["common.signdoc.deletewarning.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val)
                        this.deleteFile(file);
                });
            });

        } else {
            this.deleteFile(file);
        }
    }

    private deleteFile(file: any) {
        if (file.isAdded) {
            _.pull(this.files, file);
        } else {
            file.isDeleted = true;
            file.isModified = true;
        }

        this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.guid });
        this.updateActiveFiles();

        if (this.useGrid)
            this.setGridData();
    }

    private editFilename(file: any) {
        this.updateActiveFiles();
        this.fileNameChanged(file);
    }

    private show(file: ImageDTO) {
        if (file.isImage())
            this.showImage(file);
        else
            HtmlUtility.openInNewTab(this.$window, `/ajax/downloadTextFile.aspx?table=datastoragerecord&id=${file.imageId}&cid=${soeConfig.actorCompanyId}&useedi=${false}`);
    }

    private showImage(file: any) {
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/ShowImage/ShowImage.html"),
            controller: ShowImageController,
            controllerAs: "ctrl",
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                imageId: () => file.imageId ? file.imageId : file.id,
                description: () => file.description
            }
        }
        this.$uibModal.open(options);
    }

    private editRoles(file: any) {
        let id: number;
        if (file.id)
            id = file.id;
        else if (file.imageId)
            id = file.imageId;

        this.coreService.getFileRoleIds(id).then(selectedRoleIds => {
            const modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/SelectRoles", "SelectRoles.html"),
                controller: SelectRolesController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'sm',
                resolve: {
                    coreService: () => { return this.coreService },
                    rolesMandatory: () => { return this.rolesMandatory },
                    selectedRoleIds: () => { return selectedRoleIds },
                }
            });

            modal.result.then(result => {
                if (result.selectedRoles) {
                    this.progress.startSaveProgress((completion) => {
                        this.coreService.updateFileRoleIds(id, _.map(result.selectedRoles, r => r['id'])).then(res => {
                            completion.completed();
                            this.rolesChanged(file);
                        });
                    }, null);
                }
            });
        });
    }

    private signingDocumentStatusClicked(file: ImageDTO) {
        if (!file.attestStateId)
            this.initSigningDocument(file);
        else
            this.showSigningDocumentStatus(file);
    }

    private initSigningDocument(file: ImageDTO) {
        let modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/AddDocumentToAttestFlow/Views/addDocumentToAttestFlow.html"),
            controller: AddDocumentToAttestFlowController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                recordId: () => { return file.imageId },
                endUserId: () => { return this.linkedToUserId }
            }
        });

        modal.result.then(result => {
            if (result && result.success)
                this.onReload();
        }, (reason) => {
            // User closed
        });
    }

    private showSigningDocumentStatus(file: ImageDTO) {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/ShowDocumentSigningStatus/ShowDocumentSigningStatus.html"),
            controller: ShowDocumentSigningStatusController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                cancelPermission: () => { return this.initSigningDocumentPermission },
                entity: () => { return SoeEntityType.DataStorageRecord },
                recordId: () => { return file.imageId },
                attestStatus: () => { return file.attestStatus },
                registeredTerm: () => { return this.terms["common.signdoc.initializer"] },
                openedTerm: () => { return this.terms["common.signdoc.onlyread"] }
            }
        });

        modal.result.then(result => {
            if (result && (result.answered || result.cancelled))
                this.onReload();
        }, (reason) => {
            // User closed
        });
    }

    private afterCellEdit(file: ImageDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        switch (colDef.field) {
            //case 'fileName':
            //    if (StringUtility.getFileExtension(newValue) !== StringUtility.getFileExtension(oldValue)) {
            //        row.fileName = row.fileName + "." + row.extension;
            //        this.setGridData();
            //    }
            //    this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.guid });
            //    break;
            case 'description':
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.guid });
                break;
        }
    }

    // HELP-METHODS

    private updateActiveFiles() {
        this.activeFiles = this.files.filter(f => !f.isDeleted);

        switch (this.sortBy) {
            case TermGroup_FileDisplaySortBy.Description:
                this.activeFiles = _.sortBy(this.activeFiles, f => (<string>f.description).toLocaleLowerCase());
                break;
            case TermGroup_FileDisplaySortBy.Created:
                this.activeFiles = _.sortBy(this.activeFiles, f => f.created);
                break;
        }

        if (this.fileCount !== this.activeFiles.length) {
            this.fileCount = this.activeFiles.length;
            this.nbrOfFilesChanged({ nbrOfFiles: this.activeFiles.length });
        }

        if (!this.useGrid)
            this.progress.setProgressBusy(false);
    }

    private handleRows() {
        _.forEach(this.files, y => {
            if (y instanceof ImageDTO || y instanceof MessageAttachmentDTO) {
                y.setFileFormat();
                y.setIcon();

                if (y instanceof ImageDTO)
                    this.setSignInfo(y);
            }
        });

        this.setGridData();
        this.progress.setProgressBusy(false);
    }

    private setGridData() {
        if (this.useGrid)
            this.gridAg.setData(_.filter(this.files, (row) => !row.isDeleted));
    }

    private setSignInfo(file: ImageDTO) {
        if (!this.terms)
            return;

        if (!file.attestStateId) {
            file['signIcon'] = "fal fa-file-signature";
            file.attestStatusText = this.terms["common.signdoc.init"];
        } else {
            switch (file.attestStatus) {
                case TermGroup_DataStorageRecordAttestStatus.Initialized:
                    file['signIcon'] = "fal fa-thumbs-up disabledTextColor";
                    break;
                case TermGroup_DataStorageRecordAttestStatus.PartlySigned:
                    file['signIcon'] = "fal fa-thumbs-up infoColor";
                    break;
                case TermGroup_DataStorageRecordAttestStatus.Signed:
                    file['signIcon'] = "fal fa-thumbs-up okColor";
                    break;
                case TermGroup_DataStorageRecordAttestStatus.Rejected:
                    file['signIcon'] = "fal fa-thumbs-down errorColor";
                    break;
                case TermGroup_DataStorageRecordAttestStatus.Cancelled:
                    file['signIcon'] = "fal fa-thumbs-down warningColor";
                    break;
            }

            file.attestStatusText = this.attestStatuses.find(a => a.id == file.attestStatus)?.name;
        }
    }
}