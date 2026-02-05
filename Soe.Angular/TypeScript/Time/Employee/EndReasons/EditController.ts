import { IEmployeeService } from "../EmployeeService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { Feature, SoeEntityState } from "../../../Util/CommonEnumerations";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Constants } from "../../../Util/Constants";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    endReason: any;
    endReasonId: number
    terms: any = [];

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    //@ngInject
    constructor(private employeeService: IEmployeeService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private translationService: ITranslationService,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.loadTerms())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }


    public onInit(parameters: any) {
        this.endReasonId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        if (parameters.navigatorRecords) {
            this.navigatorRecords = _.filter(parameters.navigatorRecords, (row) => (row.id > 0));
        }
        this.flowHandler.start([{ feature: Feature.Time_Employee_EndReasons, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_EndReasons].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_EndReasons].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => !this.endReasonId);

        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.endReasonId, recordId => {
            if (recordId !== this.endReasonId) {
                this.endReasonId = recordId;
                this.onLoadData();
            }
        });
    }

    private onLoadData() {
        if (this.endReasonId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    }

    private load(): ng.IPromise<any> {
        return this.employeeService.getEndReason(this.endReasonId).then((x) => {
            this.isNew = false;
            this.endReason = x;
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.employee.endreason.endreason"] + ' ' + this.endReason.name);
        });
    }

    private save() {
        this.endReason.state = this.endReason.isActive ? SoeEntityState.Active : SoeEntityState.Inactive;

        this.progress.startSaveProgress((completion) => {
            this.employeeService.saveEndReason(this.endReason).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.endReasonId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.endReason.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }

                        this.endReasonId = result.integerValue;
                        this.endReason.endReasonId = this.endReasonId;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.endReason);
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
        this.employeeService.getEndReasons().then(data => {
            data = _.filter(data, (row) => (row.active));
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.endReasonId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.endReasonId) {
                    this.endReasonId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    protected delete() {

        if (!this.endReason.endReasonId)
            return;

        this.progress.startDeleteProgress((completion) => {
            this.employeeService.deleteEndReason(this.endReason.endReasonId).then((result) => {
                if (result.success) {
                    completion.completed(this.endReason, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(true);
        });

    }

    private new() {
        this.isNew = true;
        this.endReasonId = 0;
        this.endReason = {
            state: SoeEntityState.Active,
            isActive: true,
        };
    }
    protected copy() {
        super.copy();

        this.isNew = true;
        this.endReasonId = 0;
        this.endReason.endReasonId = 0;
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.endReason) {
                // Mandatory fields
                if (!this.endReason.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }

    public loadTerms() {
        var keys: string[] = [
            "time.employee.endreason.endreason"
        ];
        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }
}
