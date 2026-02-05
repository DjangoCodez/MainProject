import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { Guid } from "../../../Util/StringUtility";
import { ScheduleCycleRuleTypeDTO } from "../../../Common/Models/ScheduleCycle";
import { IFocusService } from "../../../Core/Services/FocusService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IScheduleService } from "../ScheduleService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Constants } from "../../../Util/Constants";
import { CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICoreService } from "../../../Core/Services/CoreService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { AccountDTO } from "../../../Common/Models/AccountDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Company settings
    private useAccountsHierarchy: boolean;

    private scheduleCycleRuleTypeId: number;
    scheduleCycleRuleType: ScheduleCycleRuleTypeDTO;
    isNew = true;
    deleteButtonTemplateUrl: string;
    saveButtonTemplateUrl: string;
    modifyPermission: boolean;
    readOnlyPermission: boolean;
    private modal;
    isModal = false;
    private accounts: AccountDTO[];

    private dayOfWeeks: any = [];
    private selectedDayOfWeeks: any = [];

    get selectedDaysString() {
        var str: string = "";
        var first: boolean = true;
        var sortStr: any = this.selectedDayOfWeeks;
        sortStr = _.sortBy(sortStr, 'id');
        _.forEach(sortStr, (day: any) => {
            if (day.id > 0) {
                let obj = _.find(this.dayOfWeeks, { id: day.id });
                if (obj) {
                    if (first === true) {
                        first = false;
                        str += obj['label'];
                    } else {
                        str += ", " + obj['label'];
                    }
                }
            }
        });

        //Handle sunday
        var sunday = _.find(this.selectedDayOfWeeks, { id: 0 });
        if (sunday) {
            let obj = _.find(this.dayOfWeeks, { id: 0 });
            if (obj) {
                if (first === true) {
                    first = false;
                    str += obj['label'];
                } else {
                    str += ", " + obj['label'];
                }
            }
        }

        return str;
    }
    set selectedDaysString(item: any) {
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        protected $uibModal,
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private coreService: ICoreService,
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
            this.focusService.focusByName("ctrl_schedulecycleruletype_name");
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.scheduleCycleRuleTypeId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Schedule_StaffingNeeds_ScheduleCycleRuleType, loadReadPermissions: true, loadModifyPermissions: true }]);
        this.setupDayOfWeeks();
    }

    public save() {
        this.scheduleCycleRuleType.dayOfWeekIds = [];
        _.forEach(this.selectedDayOfWeeks, (day: any) => {
            this.scheduleCycleRuleType.dayOfWeekIds.push(day.id);
        });

        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveScheduleCycleRuleType(this.scheduleCycleRuleType).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.scheduleCycleRuleTypeId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.scheduleCycleRuleType);
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
    // LOOKUPS
    private loadAccountStringIdsByUserFromHierarchy(): ng.IPromise<any> {
        this.accounts = [];
        return this.coreService.getAccountsFromHierarchyByUserSetting(CalendarUtility.getDateNow(), CalendarUtility.getDateNow(),true, false, false, true).then(x => {
            this.accounts = x;
            // Insert Empty
            if (this.scheduleCycleRuleTypeId > 0) {
                var empty: AccountDTO = new AccountDTO;
                empty.accountId = null;
                empty.name = '';
                this.accounts.splice(0, 0, empty);
            }
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);

            if (this.useAccountsHierarchy) {
                return this.loadAccountStringIdsByUserFromHierarchy();
            }
        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteScheduleCycleRuleType(this.scheduleCycleRuleType.scheduleCycleRuleTypeId).then((result) => {
                if (result.success) {
                    completion.completed(this.scheduleCycleRuleType, true);
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

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.scheduleCycleRuleType) {
                if (!this.scheduleCycleRuleType.name) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Schedule_StaffingNeeds_ScheduleCycleRuleType].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_StaffingNeeds_ScheduleCycleRuleType].modifyPermission;
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.scheduleCycleRuleTypeId > 0) {
            return this.progress.startLoadingProgress([
                () => this.loadData()
            ]);
        } else {
            this.isNew = true;
            this.scheduleCycleRuleTypeId = 0;
            this.scheduleCycleRuleType = new ScheduleCycleRuleTypeDTO;
            this.scheduleCycleRuleType.startTime = Constants.DATETIME_DEFAULT.beginningOfDay();
            this.scheduleCycleRuleType.stopTime = Constants.DATETIME_DEFAULT.beginningOfDay();
        }
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.loadCompanySettings();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
    }

    private setupDayOfWeeks() {
        this.dayOfWeeks = []
        _.forEach(CalendarUtility.getDayOfWeekNames(true), dayOfWeek => {
            this.dayOfWeeks.push({ id: dayOfWeek.id, label: dayOfWeek.name.toUpperCaseFirstLetter() });
        });
    }

    private loadData(): ng.IPromise<any> {
        return this.scheduleService.getScheduleCycleRuleType(this.scheduleCycleRuleTypeId).then((x) => {
            this.isNew = false;
            this.scheduleCycleRuleType = x;
            this.selectedDayOfWeeks = [];
            _.forEach(x.dayOfWeekIds, id => {
                this.selectedDayOfWeeks.push({ id: id });
            });
            if (this.scheduleCycleRuleType.startTime)
                this.scheduleCycleRuleType.startTime = new Date(<any>x.startTime);
            if (this.scheduleCycleRuleType.stopTime)
                this.scheduleCycleRuleType.stopTime = new Date(<any>x.stopTime);
        });
    }

    private selectionChanged() {
        this.dirtyHandler.setDirty();
    }

    protected copy() {
        if (!this.scheduleCycleRuleType)
            return;

        this.isNew = true;
        this.scheduleCycleRuleTypeId = 0;
        this.scheduleCycleRuleType.scheduleCycleRuleTypeId = 0;
        this.scheduleCycleRuleType.created = null;
        this.scheduleCycleRuleType.createdBy = "";
        this.scheduleCycleRuleType.modified = null;
        this.scheduleCycleRuleType.modifiedBy = "";
        this.scheduleCycleRuleType.name = "";
        this.scheduleCycleRuleType.accountId = null;

        this.dirtyHandler.setDirty();
        this.focusService.focusByName("ctrl_scheduleCycleRuleType_name");
        this.translationService.translate("time.schedule.schedulecycleruletype.new").then((term) => {
            this.messagingHandler.publishSetTabLabel(this.guid, term);
        });
    }
}
