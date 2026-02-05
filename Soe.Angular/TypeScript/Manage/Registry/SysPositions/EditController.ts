import { IMessagingHandler } from "../../../Core/Handlers/MessagingHandler";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { EditControllerBase } from "../../../Core/Controllers/EditControllerBase";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { IEditControllerFlowHandler, IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandler } from "../../../Core/Handlers/DirtyHandler";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandler } from "../../../Core/Handlers/ValidationSummaryHandler";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IRegistryService } from "../RegistryService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Guid } from "../../../Util/StringUtility";
import { Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { SysPositionDTO } from "../../../Common/Models/PositionDTOs";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private sysPositionId: number;
    private sysPosition: SysPositionDTO;

    private countries: ISmallGenericType[];
    private languages: ISmallGenericType[];

    //@ngInject
    constructor(
        private coreService: ICoreService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.deleteButtonTemplateUrl = urlHelperService.getCoreComponent("deleteButtonComposition.html");
        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButtonComposition.html");

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.sysPositionId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Preferences_Registry_Positions_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.coreService.saveSysPosition(this.sysPosition).then(result => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.sysPositionId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.sysPosition);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.loadData();
            }, error => {

            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.coreService.deleteSysPosition(this.sysPositionId).then(result => {
                if (result.success) {
                    completion.completed(this.sysPosition, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(false);
        });
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.sysPosition) {
                if (!this.sysPosition.name) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Preferences_Registry_Positions_Edit].readPermission;
        this.modifyPermission = response[Feature.Manage_Preferences_Registry_Positions_Edit].modifyPermission;
    }

    private doLookups() {
        if (this.sysPositionId > 0) {
            return this.progress.startLoadingProgress([
                () => this.loadCountries(),
                () => this.loadLanguages(),
                () => this.loadData()
            ])
        } else {
            this.isNew = true;
            this.sysPositionId = 0;
            this.sysPosition = new SysPositionDTO();
            return this.progress.startLoadingProgress([
                () => this.loadCountries(),
                () => this.loadLanguages(),
            ]);
        }
    }

    private loadCountries(): ng.IPromise<any> {
        return this.coreService.getSysCountries(false, true).then(x => {
            this.countries = x;
        });
    }

    private loadLanguages(): ng.IPromise<any> {
        return this.coreService.getSysLanguages(false, true).then(x => {
            this.languages = x;
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        //this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => { /*this.copy()*/ }, () => this.isNew);
    }

    private loadData() {
        return this.coreService.getSysPosition(this.sysPositionId).then(x => {
            this.sysPosition = x;
            this.isNew = false;
        });
    }

    private setModified() {
        this.dirtyHandler.setDirty();
    }
}