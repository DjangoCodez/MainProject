import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { TimePeriodHeadDTO } from "../../../Common/Models/TimePeriodHeadDTO";
import { TimePeriodDTO } from "../../../Common/Models/TimePeriodDTO";
import { Feature } from "../../../Util/CommonEnumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Constants } from "../../../Util/Constants";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { EventHistoryDTO } from "../../../Common/Models/EventHistoryDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data       
    private eventHistoryId: number;
    private eventHistory: EventHistoryDTO;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    // Flags
    private stringHasValue: boolean = false;
    private integerHasValue: boolean = false;
    private decimalHasValue: boolean = false;
    private boolHasValue: boolean = false;
    private dateHasValue: boolean = false;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.messagingHandler.onSetDirty(() => { this.dirtyHandler.setDirty() })
    }

    public onInit(parameters: any) {
        this.eventHistoryId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Employee_EventHistory, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    // LOOKUPS

    protected onDoLookups() {
        return this.$q.all([
        ]).then(() => {
            this.$q.all([
            ]);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_EventHistory].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_EventHistory].modifyPermission;
    }

    private onLoadData() {
        if (this.eventHistoryId > 0) {
            return this.coreService.getEventHistory(this.eventHistoryId, true).then(x => {
                this.eventHistory = x;
                this.isNew = false;

                if (this.eventHistory.stringValue !== undefined)
                    this.stringHasValue = true;
                if (this.eventHistory.integerValue !== undefined)
                    this.integerHasValue = true;
                if (this.eventHistory.decimalValue !== undefined)
                    this.decimalHasValue = true;
                if (this.eventHistory.booleanValue !== undefined)
                    this.boolHasValue = true;
                if (this.eventHistory.dateValue !== null)
                    this.dateHasValue = true;
            });
        }
    }
    
    // ACTIONS

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.coreService.saveEventHistory(this.eventHistory).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.eventHistoryId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.eventHistory);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.onLoadData();
            }, error => {

            });
    }

    protected delete() {
        this.progress.startDeleteProgress((completion) => {
            this.coreService.deleteEventHistory(this.eventHistory.eventHistoryId).then((result) => {
                if (result.success) {
                    completion.completed(this.eventHistory, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(false);
        });
    }

    // HELP-METHODS

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
        });
    }
}
