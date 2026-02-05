import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Guid } from "../../../Util/StringUtility";
import { PositionDTO, SysPositionGridDTO } from "../../../Common/Models/EmployeePositionDTO";
import { IFocusService } from "../../../Core/Services/FocusService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IEmployeeService } from "../EmployeeService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Constants } from "../../../Util/Constants";
import { Feature } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Init parameters
    private positionId: number;

    position: PositionDTO;
    isNew = true;
    deleteButtonTemplateUrl: string;
    saveButtonTemplateUrl: string;
    modifyPermission: boolean;
    readOnlyPermission: boolean;
    isLinked: boolean;
    sysPositions: SysPositionGridDTO[];

    private modal;
    isModal = false;
    terms: any = [];

    //@ngInject
    constructor(
        protected $uibModal,
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private employeeService: IEmployeeService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.deleteButtonTemplateUrl = urlHelperService.getCoreComponent("deleteButtonComposition.html");
        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButtonComposition.html");

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;
            this.onInit(parameters);
            this.focusService.focusByName("ctrl_position_name");
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.positionId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Employee_Positions, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.positionId, recordId => {
            if (recordId !== this.positionId) {
                this.positionId = recordId;
                this.onLoadData();
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_Positions].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_Positions].modifyPermission;
    }

    // LOOKUPS

    protected doLookups() {
        return this.loadTerms().then(() => {
            return this.loadSysPositions();
        });
    }

    private onLoadData() {
        if (this.positionId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    }

    private loadSysPositions() {
        this.sysPositions = [];
        return this.coreService.getSysPositions(CoreUtility.sysCountryId, CoreUtility.languageId, true).then(x => {
            this.sysPositions = x;
        });
    }

    private load(): ng.IPromise<any> {
        return this.employeeService.getEmployeePosition(this.positionId, true).then((x) => {
            this.isNew = false;
            this.position = x;
            this.isLinked = false;
            if (this.position.sysPositionId && this.position.sysPositionId > 0) {
                this.isLinked = true;
            }
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.employee.position.position"] + ' ' + this.position.name);
        });
    }

    private loadTerms() {
        var keys: string[] = [
            "time.employee.position.position"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }
    //ACTIONS

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.employeeService.savePosition(this.position).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.positionId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.position.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                        this.positionId = result.integerValue;
                        this.position.positionId = result.integerValue;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.position);
                    }
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();

                if (this.isModal)
                    this.closeModal();
                else
                    this.onLoadData();
            }, error => {

            });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.employeeService.getPositionsGrid(false,false).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.positionId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.positionId) {
                    this.positionId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.employeeService.deletePosition(this.position.positionId).then((result) => {
                if (result.success) {
                    completion.completed(this.position);
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

    private new() {
        this.isNew = true;
        this.positionId = 0;
        this.position = new PositionDTO();
        this.position.positionSkills = [];
    }

    protected copy() {
        if (!this.position)
            return;

        super.copy();

        this.isNew = true;
        this.positionId = 0;
        this.position.positionId = 0;
        this.position.created = null;
        this.position.createdBy = "";
        this.position.modified = null;
        this.position.modifiedBy = "";

        this.dirtyHandler.setDirty();
        this.focusService.focusByName("ctrl_position_name");
        this.translationService.translate("time.employee.position.new_position").then((term) => {
            this.messagingHandler.publishSetTabLabel(this.guid, term);
        });
    }

    //EVENTS

    private isLinkedChanged() {
        this.$timeout(() => {
            if (!this.isLinked) {
                this.position.sysPositionId = 0;
            }
        });
    }
    public closeModal() {
        if (this.isModal) {
            if (this.positionId) {
                this.modal.close(this.positionId);
            } else {
                this.modal.dismiss();
            }
        }
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.position) {
                if (!this.position.name) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }
}