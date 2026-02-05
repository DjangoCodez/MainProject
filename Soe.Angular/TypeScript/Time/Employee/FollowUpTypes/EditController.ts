import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IEmployeeService } from "../EmployeeService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { Feature, TermGroup, SoeEntityState } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Constants } from "../../../Util/Constants";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    followUpType: any;
    followUpTypeId: number;

    // Lookups
    types: any;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    // Lookups

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private employeeService: IEmployeeService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookUp())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    // SETUP
    public onInit(parameters: any) {
        this.followUpTypeId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;

        this.flowHandler.start([{ feature: Feature.Time_Employee_FollowUpTypes, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => !this.followUpTypeId);

        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.followUpTypeId, recordId => {
            if (recordId !== this.followUpTypeId) {
                this.followUpTypeId = recordId;
                this.onLoadData();
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_FollowUpTypes].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_FollowUpTypes].modifyPermission;
    }

    // LOOKUPS
    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.FollowUpTypeType, false, false).then((x) => {
            this.types = x;
        });
    }
    private onDoLookUp(): ng.IPromise<any> {
        return this.loadTypes();
    }

    private onLoadData() {
        if (this.followUpTypeId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        }
        else {
            this.new();
        }
    }

    private load(): ng.IPromise<any> {
        return this.employeeService.getFollowUpType(this.followUpTypeId).then((x) => {
            this.isNew = false;
            this.followUpType = x;
            if (this.followUpType.state == SoeEntityState.Active)
                this.followUpType.active = true;
            else
                this.followUpType.active = false;

            this.messagingHandler.publishSetTabLabel(this.guid, this.followUpType.name);
        });
    }

    // Save
    private save() {
        if (this.followUpType.active === true)
            this.followUpType.state = SoeEntityState.Active;
        else
            this.followUpType.state = SoeEntityState.Inactive;
        this.progress.startSaveProgress((completion) => {
            this.employeeService.saveFollowUpType(this.followUpType).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.followUpTypeId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.followUpType.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }

                        this.followUpTypeId = result.integerValue;
                        this.followUpType.followUpTypeId = result.integerValue;

                        this.toolbar.setSelectedRecord(this.followUpType.followUpTypeId);
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.followUpType);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.load();
            });
    }
    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.employeeService.getFollowUpTypes().then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.followUpTypeId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.followUpTypeId) {
                    this.followUpTypeId = recordId;
                    this.load();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    protected delete() {
        if (!this.followUpTypeId)
            return;

        this.progress.startDeleteProgress((completion) => {
            this.employeeService.deleteFollowUpType(this.followUpType.followUpTypeId).then((result) => {
                if (result.success) {
                    completion.completed(this.followUpType, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(true);
        });
    }

    private new() {
        this.isNew = true;
        this.followUpTypeId = 0;
        this.followUpType = {
            active: true,
        };
    }

    protected copy() {
        super.copy();

        this.isNew = true;
        this.followUpTypeId = 0;
        this.followUpType.followUpTypeId = 0;
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.followUpType) {
                // Mandatory fields
                if (!this.followUpType.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}
