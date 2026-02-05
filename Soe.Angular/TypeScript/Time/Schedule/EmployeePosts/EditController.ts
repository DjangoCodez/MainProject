import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IDirtyHandler } from "../../../Core/Handlers/DirtyHandler";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { SmallGenericType } from "../../../Common/Models/smallgenerictype";
import { EmployeePostSkillDTO } from "../../../Common/Models/SkillDTOs";
import { EmployeePostDTO } from "../../../Common/Models/EmployeePostDTO";
import { IScheduleService } from "../ScheduleService";
import { IFocusService } from "../../../Core/Services/FocusService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { Guid } from "../../../Util/StringUtility";
import { Constants } from "../../../Util/Constants";
import { CompanySettingType, TermGroup, SoeEmployeePostStatus, Feature } from "../../../Util/CommonEnumerations";
import { IEmployeeService } from "../../Employee/EmployeeService";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Company settings
    private useAccountsHierarchy: boolean;

    // Modal
    modal: any;
    private isModal: boolean = false;

    // Data
    private employeePostId: number;
    private employeePost: EmployeePostDTO;
    private employeeGroups: ISmallGenericType[] = [];
    private accounts: AccountDTO[];

    // Skills
    private employeePostSkills: EmployeePostSkillDTO[] = [];

    // Days
    private daysInWeek: any[];
    private selectedDayOfWeeks: any = [];

    // Lookups
    private scheduleCycles: any[];
    private dayOfWeeks: any[];
    private ruleWorkTimeWeek: number = 0;
    private allowEditEmploymentPercent: boolean = false;
    private terms: any;
    private employeePostWeekendTypes: SmallGenericType[] = [];

    private modalInstance: any;
    dirtyHandler: IDirtyHandler;

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
    set selectedDaysString(item: any) { }

    private _selectedAccount: AccountDTO;
    public get selectedAccount(): AccountDTO {
        return this._selectedAccount;
    }
    public set selectedAccount(account: AccountDTO) {
        this._selectedAccount = account;
        if (account) {
            this.employeePost.accountId = account.accountId;
            this.employeePost.accountName = account.name;
        }
    }

    //@ngInject
    constructor(
        $uibModal,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private employeeService: IEmployeeService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private focusService: IFocusService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.modalInstance = $uibModal;

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;

            this.onInit(parameters);
        });
    }

    // SETUP

    public onInit(parameters: any) {
        this.employeePostId = parameters.id || 0;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Schedule_StaffingNeeds_EmployeePost, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Schedule_StaffingNeeds_EmployeePost].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_StaffingNeeds_EmployeePost].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => { return this.isNew });
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all(
            [this.loadCompanySettings(),
            this.loadTerms(),
            this.loadEmployeeGroups(),
            this.loadscheduleCycles(),
            this.loadEmployeePostWeekendType(),
            this.loadAccountStringIdsByUserFromHierarchy(),
            ]).then(() => {
                this.loadNbrOfDays();
                this.setupDayOfWeeks();
            });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.employeePostId) {
            return this.load();
        } else {
            this.new();
        }
    }

    // LOOKUPS
    private loadAccountStringIdsByUserFromHierarchy(): ng.IPromise<any> {
        this.accounts = [];
        return this.coreService.getAccountsFromHierarchyByUserSetting(CalendarUtility.getDateNow(), CalendarUtility.getDateNow(), true, false, false, true).then(x => {
            this.accounts = x;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimeSetEmploymentPercentManually);
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.allowEditEmploymentPercent = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSetEmploymentPercentManually);
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "time.schedule.employeepost.employeegroupname"
        ];
        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadEmployeeGroups(): ng.IPromise<any> {
        this.employeeGroups = [];
        return this.coreService.getEmployeeGroupsDict(false).then((x) => {
            this.employeeGroups = x;
        });
    }

    private loadscheduleCycles(): ng.IPromise<any> {
        this.scheduleCycles = [];
        return this.scheduleService.getScheduleCyclesDict(false).then((x) => {
            this.scheduleCycles = x;
        });
    }

    private loadEmployeePostWeekendType(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.EmployeePostWeekendType, false, false).then(x => {
            this.employeePostWeekendTypes = x;
        });
    }

    private load(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        if (this.employeePostId !== 0) {
            this.scheduleService.getEmployeePost(this.employeePostId).then(x => {
                this.employeePost = x;

                this.selectedDayOfWeeks = [];
                _.forEach(this.employeePost.dayOfWeekIds, id => {
                    this.selectedDayOfWeeks.push({ id: id });
                });

                this.employeePostSkills = this.employeePost.employeePostSkillDTOs;

                if (this.employeePost.employeeGroupId) {
                    this.employeeService.getEmployeeGroup(this.employeePost.employeeGroupId).then(eg => {
                        this.ruleWorkTimeWeek = eg.ruleWorkTimeWeek || 0;
                        this.setWorkTimePercent();
                    });
                } else {
                    this.ruleWorkTimeWeek = 0;
                    this.setWorkTimePercent();
                }

                this.selectedAccount = _.find(this.accounts, a => a.accountId == this.employeePost.accountId);
                this.isNew = false;

                if (this.employeePost.accountId > 0) {
                    // Insert empty
                    var account: AccountDTO = new AccountDTO();
                    account.accountId = 0;
                    account.name = "";
                    this.accounts.splice(0, 0, account);
                }

                deferral.resolve();
            });
        } else {
            this.new();
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadNbrOfDays() {
        this.daysInWeek = [];
        for (var i = 1; i <= 5; i++) {
            this.daysInWeek.push({ id: i, name: i });
        }
    }

    private setupDayOfWeeks() {
        this.dayOfWeeks = []
        _.forEach(CalendarUtility.getDayOfWeekNames(true), dayOfWeek => {
            this.dayOfWeeks.push({ id: dayOfWeek.id, label: dayOfWeek.name });
        });
    }

    // EVENTS

    private groupChanged() {
        this.$timeout(() => {
            if (this.employeePost.employeeGroupId > 0) {
                //Groupchanged, get workinghours and percent from employeegroup
                this.employeeService.getEmployeeGroup(this.employeePost.employeeGroupId).then(x => {
                    if (x.ruleWorkTimeWeek)
                        this.ruleWorkTimeWeek = this.employeePost.workTimeWeek = x.ruleWorkTimeWeek;
                    this.setWorkTimePercent();
                });
            } else {
                this.ruleWorkTimeWeek = this.employeePost.workTimeWeek = 0
                this.setWorkTimePercent();
            }
        });
    }

    private workTimeWeekChanged() {
        this.$timeout(() => {
            if (this.employeePost.workTimeWeek < 0)
                this.employeePost.workTimeWeek = 0;
            this.setWorkTimePercent();
        });
    };

    private workTimePercentChanged() {
        this.$timeout(() => {
            if (this.employeePost.workTimePercent < 0)
                this.employeePost.workTimePercent = 0;
            if (this.employeePost.workTimePercent > 100)
                this.employeePost.workTimePercent = 100;
        });
    }

    private selectionChanged() {
        this.dirtyHandler.setDirty();
    }

    // ACTIONS

    public closeModal(deleted: boolean) {
        if (this.isModal) {
            if (this.employeePostId) {
                this.modal.close({ employeePostId: this.employeePostId, employeePostName: this.employeePost.name, deleted: deleted });
            } else {
                this.modal.dismiss();
            }
        }
    }

    private save() {
        this.employeePost.dayOfWeekIds = [];
        _.forEach(this.selectedDayOfWeeks, (day: any) => {
            this.employeePost.dayOfWeekIds.push(day.id);
        });

        if (this.employeePost.isLocked)
            this.employeePost.status = SoeEmployeePostStatus.Locked;
        else
            this.employeePost.status = SoeEmployeePostStatus.None;
        this.employeePost.employeePostSkillDTOs = this.employeePostSkills;

        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveEmployeePost(this.employeePost).then((result) => {
                if (result.success) {
                    this.employeePostId = result.integerValue;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.employeePost);
                    this.dirtyHandler.clean();
                    if (this.isModal)
                        this.closeModal(false);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.onLoadData();
        }, error => {
        });
    }

    protected initDelete() {
        if (!this.employeePost)
            return;

        return this.scheduleService.getEmployeePostStatus(this.employeePost.employeePostId).then(status => {
            // Show verification dialog
            var keys: string[] = [
                "time.schedule.employeepost.deletetitle",
                "time.schedule.employeepost.deletewarninghasschedule",
                "time.schedule.employeepost.cannotdeletehasemployee",
                "time.schedule.employeepost.cannotdeleteislocked",
                "core.continue",
            ];

            this.translationService.translateMany(keys).then((terms) => {
                if (status == SoeEmployeePostStatus.HasSchedule) {
                    let modal = this.notificationService.showDialog(terms["time.schedule.employeepost.deletetitle"], terms["time.schedule.employeepost.deletewarninghasschedule"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(val => {
                        if (val) {
                            this.delete();
                        }
                    });
                } else if (status == SoeEmployeePostStatus.HasEmployee) {
                    this.notificationService.showDialog(terms["time.schedule.employeepost.deletetitle"], terms["time.schedule.employeepost.cannotdeletehasemployee"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                } else if (status == SoeEmployeePostStatus.Locked) {
                    this.notificationService.showDialog(terms["time.schedule.employeepost.deletetitle"], terms["time.schedule.employeepost.cannotdeleteislocked"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                } else {
                    let modal = this.notificationService.showDialog(terms["time.schedule.employeepost.deletetitle"], terms["core.continue"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(val => {
                        if (val) {
                            this.delete();
                        }
                    });
                }
            });
        });
    }

    protected delete() {
        if (!this.employeePost)
            return;

        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteEmployeePost(this.employeePost.employeePostId).then((result) => {
                if (result.success) {
                    completion.completed(this.employeePost, true);
                    if (this.isModal)
                        this.closeModal(true);
                    else
                        this.closeMe(true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }

    // HELP-METHODS

    private new() {
        this.isNew = true;
        this.employeePostId = 0;
        this.employeePost = new EmployeePostDTO();
        this.employeePost.workDaysWeek = 5;
        this.employeePost.employeePostWeekendType = 0;
    }

    protected copy() {
        this.isNew = true;
        this.employeePostId = 0;
        this.employeePost.employeePostId = 0;
        this.employeePost.created = null;
        this.employeePost.createdBy = "";
        this.employeePost.modified = null;
        this.employeePost.modifiedBy = "";
        this.employeePost.name = "";

        this.dirtyHandler.setDirty();
        this.focusService.focusByName("ctrl_employeePost_name");
        this.translationService.translate("time.schedule.employeepost.new_employeepost").then((term) => {
            this.messagingService.publish(Constants.EVENT_SET_TAB_LABEL, {
                guid: this.guid,
                label: term,
            });
        });
    }

    private setWorkTimePercent() {
        this.employeePost.workTimePercent = this.ruleWorkTimeWeek > 0 ? (this.employeePost.workTimeWeek / this.ruleWorkTimeWeek * 100).round(2) : 0;
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.employeePost) {
                if (!this.employeePost.name)
                    mandatoryFieldKeys.push("common.name");

                if (!this.employeePost.scheduleCycleId)
                    mandatoryFieldKeys.push("time.schedule.employeepost.schedulecycle");

                if (!this.employeePost.employeeGroupId)
                    mandatoryFieldKeys.push("time.schedule.employeepost.employeegroupname");
            }
        });
    }
}
