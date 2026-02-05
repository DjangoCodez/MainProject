import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Guid } from "../../../Util/StringUtility";
import { TimeScheduleTaskTypeDTO } from "../../../Common/Models/StaffingNeedsDTOs";
import { IFocusService } from "../../../Core/Services/FocusService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IScheduleService } from "../ScheduleService";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { Constants } from "../../../Util/Constants";
import { CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { ICoreService } from "../../../Core/Services/CoreService";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Company settings
    private useAccountsHierarchy: boolean;

    // Init parameters
    private timeScheduleTaskTypeId: number;

    timeScheduleTaskType: TimeScheduleTaskTypeDTO;
    isNew = true;
    deleteButtonTemplateUrl: string;
    saveButtonTemplateUrl: string;
    modifyPermission: boolean;
    readOnlyPermission: boolean;
    private modal;
    isModal = false;
    private accounts: AccountDTO[];

    //@ngInject
    constructor(
        protected $uibModal,
        private coreService: ICoreService,
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private scheduleService: IScheduleService,
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
            this.focusService.focusByName("ctrl_timeScheduleTaskType_name");
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.timeScheduleTaskTypeId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Schedule_StaffingNeeds_TaskTypes, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Schedule_StaffingNeeds_TaskTypes].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_StaffingNeeds_TaskTypes].modifyPermission;
    }

    // LOOKUPS
    private onDoLookups(): ng.IPromise<any> {
        return this.loadCompanySettings();
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.timeScheduleTaskTypeId > 0) {
            return this.progress.startLoadingProgress([
                () => this.loadData()
            ])
        } else {
            this.isNew = true;
            this.timeScheduleTaskTypeId = 0;
            this.timeScheduleTaskType = new TimeScheduleTaskTypeDTO;
        }
    }

    private loadData(): ng.IPromise<any> {
        return this.scheduleService.getTimeScheduleTaskType(this.timeScheduleTaskTypeId).then((x) => {
            this.isNew = false;
            this.timeScheduleTaskType = x;
        });
    }

    private loadAccountStringIdsByUserFromHierarchy(): ng.IPromise<any> {
        this.accounts = [];
        return this.coreService.getAccountsFromHierarchyByUserSetting(CalendarUtility.getDateNow(), CalendarUtility.getDateNow(),true, false, false, true).then(x => {
            this.accounts = x;
            //empty
            var empty: AccountDTO = new AccountDTO;
            empty.accountId = null;
            empty.name = '';
            this.accounts.splice(0, 0, empty);

            if (this.isNew && this.accounts.length == 2)
                this.timeScheduleTaskType.accountId = this.accounts.find(a => a.accountId).accountId;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);

            if (this.useAccountsHierarchy) {
                this.loadAccountStringIdsByUserFromHierarchy();
            }
        });
    }

    //ACTIONS

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveTimeScheduleTaskType(this.timeScheduleTaskType).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.timeScheduleTaskTypeId = result.integerValue;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.timeScheduleTaskType);
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

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteTimeScheduleTaskType(this.timeScheduleTaskType.timeScheduleTaskTypeId).then((result) => {
                if (result.success) {
                    completion.completed(this.timeScheduleTaskType, true);
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

    protected copy() {
        if (!this.timeScheduleTaskType)
            return;

        super.copy();

        this.isNew = true;
        this.timeScheduleTaskTypeId = 0;
        this.timeScheduleTaskType.timeScheduleTaskTypeId = 0;
        this.timeScheduleTaskType.name = "";
        this.timeScheduleTaskType.created = null;
        this.timeScheduleTaskType.createdBy = "";
        this.timeScheduleTaskType.modified = null;
        this.timeScheduleTaskType.modifiedBy = "";
        this.timeScheduleTaskType.accountId = null;

        this.dirtyHandler.setDirty();
        this.focusService.focusByName("ctrl_timeScheduleTaskType_name");
        this.translationService.translate("time.schedule.timescheduletasktype.new_type").then((term) => {
            this.messagingHandler.publishSetTabLabel(this.guid, term);
        });
    }

    //EVENTS

    public closeModal() {
        if (this.isModal) {
            if (this.timeScheduleTaskTypeId) {
                this.modal.close(this.timeScheduleTaskTypeId);
            } else {
                this.modal.dismiss();
            }
        }
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.timeScheduleTaskType) {
                if (!this.timeScheduleTaskType.name) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }

}
