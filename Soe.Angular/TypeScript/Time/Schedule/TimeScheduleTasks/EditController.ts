import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ShiftTypeDTO } from "../../../Common/Models/ShiftTypeDTO";
import { TimeScheduleTaskDTO } from "../../../Common/Models/StaffingNeedsDTOs";
import { Guid } from "../../../Util/StringUtility";
import { IFocusService } from "../../../Core/Services/FocusService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IScheduleService } from "../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { TaskAndDeliveryFunctions } from "../../../Util/Enumerations";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { DailyRecurrenceRangeDTO, DailyRecurrenceParamsDTO } from "../../../Common/Models/DailyRecurrencePatternDTOs";
import { DailyRecurrencePatternController } from "../../../Common/Dialogs/DailyRecurrencePattern/DailyRecurrencePatternController";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { Constants } from "../../../Util/Constants";
import { Feature, CompanySettingType } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { GeneratedNeedsDialogController } from "./Dialogs/GeneratedNeedsDialogController";

enum ValidationError {
    Unknown = 0,
    LengthIsLowerThanAllowed = 1,
    MinSplitLengthIsLowerThanAllowed = 2,
    StartLaterThanStop = 3,
    PlannedTimeLowerThanLength = 4,
    LengthLongerThanPlannedTime = 5
}

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Permissions
    private staffingNeedsReadPermission: boolean = false;

    // Company Settings
    private useAccountsHierarchy: boolean;

    // Init parameters
    private timeScheduleTaskId: number;
    private startTime: Date;
    private stopTime: Date;

    private accounts: AccountDTO[];
    terms: any;
    timeScheduleTask: TimeScheduleTaskDTO;
    private shiftTypes: ShiftTypeDTO[];
    private taskTypes: ISmallGenericType[] = [];
    isNew: boolean;
    deleteButtonTemplateUrl: string;
    saveButtonTemplateUrl: string;
    modifyPermission: boolean;
    readOnlyPermission: boolean;
    companySettingMinLength: number = 15;
    private modal;
    isModal = false;
    private edit: ng.IFormController;
    saveFunctions: any = [];

    private _selectedAccount: AccountDTO;
    public get selectedAccount(): AccountDTO {
        return this._selectedAccount;
    }
    public set selectedAccount(account: AccountDTO) {
        this._selectedAccount = account;
        if (account) {
            this.timeScheduleTask.accountId = account.accountId;
            this.timeScheduleTask.accountName = account.name;
        }
    }

    //@ngInject
    constructor(
        $scope: ng.IScope,
        protected $uibModal,
        private $timeout: ng.ITimeoutService,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
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
            this.focusService.focusByName("ctrl_timeScheduleTask_name");
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    // SETUP

    public onInit(parameters: any) {
        this.timeScheduleTaskId = parameters.id;
        if (parameters.startTime)
            this.startTime = parameters.startTime;
        if (parameters.stopTime)
            this.stopTime = parameters.stopTime;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Schedule_StaffingNeeds_Tasks, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Schedule_StaffingNeeds_Tasks].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_StaffingNeeds_Tasks].modifyPermission;
    }

    private doLookups() {
        return this.loadTerms().then(() => {
            return this.progress.startLoadingProgress([
                () => this.loadReadPermissions(),
                () => this.loadCompanySettings(),
                () => this.loadShiftTypes(),
                () => this.loadTaskTypes(),
                () => this.loadAccountStringIdsByUserFromHierarchy(),
            ]);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.timeScheduleTaskId, recordId => {
            if (!this.isNew && recordId !== this.timeScheduleTaskId) {
                this.timeScheduleTaskId = recordId;
                this.onLoadData();
            }
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "core.save",
            "core.saveandnew",
            "core.saveandclose",
            "core.notspecified",
            "time.schedule.timescheduletask.task"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.saveFunctions.push({ id: TaskAndDeliveryFunctions.Save, name: terms["core.save"] });
            this.saveFunctions.push({ id: TaskAndDeliveryFunctions.SaveAndNew, name: terms["core.saveandnew"] });
            this.saveFunctions.push({ id: TaskAndDeliveryFunctions.SaveAndClose, name: terms["core.saveandclose"] });
        });
    }

    private loadReadPermissions(): ng.IPromise<any> {
        let features: number[] = [];
        features.push(Feature.Time_Schedule_StaffingNeeds);

        return this.coreService.hasReadOnlyPermissions(features).then(x => {
            this.staffingNeedsReadPermission = x[Feature.Time_Schedule_StaffingNeeds];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength);
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.companySettingMinLength = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength);
            if (this.companySettingMinLength == 0)
                this.companySettingMinLength = 15;
        });
    }

    private loadShiftTypes(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypes(false, false, false, false, false, false).then(x => {
            this.shiftTypes = x;

            // Insert empty shift type
            let shiftType: ShiftTypeDTO = new ShiftTypeDTO();
            shiftType.shiftTypeId = 0;
            shiftType.name = this.terms["core.notspecified"];
            shiftType.color = Constants.SHIFT_TYPE_UNSPECIFIED_COLOR;
            this.shiftTypes.splice(0, 0, shiftType);
        });
    }

    private loadAccountStringIdsByUserFromHierarchy(): ng.IPromise<any> {
        this.accounts = [];
        return this.coreService.getAccountsFromHierarchyByUserSetting(CalendarUtility.getDateNow(), CalendarUtility.getDateNow(), true, false, false, true).then(x => {
            this.accounts = x;

            let empty: AccountDTO = new AccountDTO;
            empty.accountId = null;
            empty.name = '';
            this.accounts.splice(0, 0, empty);

            this.setDefaultAccount();
        });
    }

    private setDefaultAccount() {
        if (this.isNew && this.accounts.filter(a => a.accountId).length == 1)
            this.selectedAccount = this.accounts.find(a => a.accountId);
    }

    private loadTaskTypes(): ng.IPromise<any> {
        this.taskTypes = [];
        return this.scheduleService.getTimeScheduleTaskTypesGrid(true).then(x => {
            _.forEach(x, (row: any) => {
                this.taskTypes.push({ id: row.timeScheduleTaskTypeId, name: row.name });
            });
        });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.timeScheduleTaskId > 0) {
            return this.progress.startLoadingProgress([() => {
                return this.scheduleService.getTimeScheduleTask(this.timeScheduleTaskId, true, true, true).then((x) => {
                    this.isNew = false;
                    this.timeScheduleTask = x;
                    if (this.timeScheduleTask) {
                        // If no shift type is selected, set the unspecified, to get that description to appear
                        if (!this.timeScheduleTask.shiftTypeId)
                            this.timeScheduleTask.shiftTypeId = 0;

                        this.timeScheduleTask["minLength"] = this.companySettingMinLength;
                        this.selectedAccount = _.find(this.accounts, a => a.accountId == this.timeScheduleTask.accountId);
                        this.setRecurrenceInfo();
                    }
                    this.dirtyHandler.clean();
                    this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.schedule.timescheduletask.task"] + ' ' + this.timeScheduleTask.name);
                });
            }]);
        } else {
            this.new(false);
        }
    }

    private reloadNavigationRecords(selectedRecord, setSelected: boolean) {
        this.navigatorRecords = [];
        this.scheduleService.getTimeScheduleTasksGrid(false).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.timeScheduleTaskId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (setSelected && recordId !== this.timeScheduleTaskId) {
                    this.timeScheduleTaskId = recordId;
                    this.onLoadData();
                }
            });

            if (setSelected)
                this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    // ACTIONS

    private new(keepStartDate: boolean) {
        this.messagingHandler.publishSetTabLabelNew(this.guid);

        const startDate: Date = keepStartDate && this.timeScheduleTask ? this.timeScheduleTask.startDate : (this.startTime ? CalendarUtility.convertToDate(this.startTime).beginningOfDay() : CalendarUtility.getDateToday());
        const accountId = keepStartDate && this.timeScheduleTask ? this.timeScheduleTask.accountId : undefined;

        this.isNew = true;
        this.timeScheduleTaskId = 0;
        this.timeScheduleTask = new TimeScheduleTaskDTO;
        this.timeScheduleTask.isActive = true;
        this.timeScheduleTask.shiftTypeId = 0;
        this.timeScheduleTask.accountId = accountId;

        if (!accountId)
            this.setDefaultAccount();

        if (this.startTime && this.stopTime && this.startTime.isBeginningOfDay() && this.stopTime.isEndOfDay()) {
            this.timeScheduleTask.startTime = null;
            this.timeScheduleTask.stopTime = null;
            this.timeScheduleTask.length = 60;
        } else {
            this.timeScheduleTask.startTime = this.startTime ? this.startTime : null;
            this.timeScheduleTask.stopTime = this.stopTime ? this.stopTime : null;
            this.calculateLength();
        }
        this.timeScheduleTask.nbrOfPersons = 1;
        this.timeScheduleTask.startDate = startDate;
        this.timeScheduleTask.length = 0;
        this.timeScheduleTask.onlyOneEmployee = false;
        this.timeScheduleTask.allowOverlapping = false;
        this.timeScheduleTask.excludedDates = [];
        this.setRecurrenceInfo();
        this.timeScheduleTask.minSplitLength = this.companySettingMinLength;
        this.timeScheduleTask["minLength"] = this.companySettingMinLength;

        this.focusService.focusByName("ctrl_timeScheduleTask_name");
    }

    protected copy() {
        super.copy();

        this.isNew = true;
        this.timeScheduleTaskId = 0;
        this.timeScheduleTask.timeScheduleTaskId = 0;
        this.timeScheduleTask.created = null;
        this.timeScheduleTask.createdBy = "";
        this.timeScheduleTask.modified = null;
        this.timeScheduleTask.modifiedBy = "";

        this.dirtyHandler.setDirty();
        this.focusService.focusByName("ctrl_timeScheduleTask_name");
    }

    private executeSaveFunction(option) {
        switch (option.id) {
            case TaskAndDeliveryFunctions.Save:
                this.save(false, false);
                break;
            case TaskAndDeliveryFunctions.SaveAndNew:
                this.save(true, false);
                break;
            case TaskAndDeliveryFunctions.SaveAndClose:
                this.save(false, true);
                break;
        }
    }

    private save(newAfterSave: boolean, closeAfterSave: boolean) {
        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveTimeScheduleTask(this.timeScheduleTask).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.timeScheduleTaskId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.timeScheduleTask.name));
                                if (!newAfterSave && !closeAfterSave)
                                    this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue, !newAfterSave && !closeAfterSave);
                            }
                        }
                        this.timeScheduleTaskId = result.integerValue;
                        this.timeScheduleTask.timeScheduleTaskId = result.integerValue;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.timeScheduleTask);
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
                    this.closeModal(true);
                else {
                    if (newAfterSave) {
                        this.new(true);
                    } else if (closeAfterSave) {
                        this.closeMe(true);
                    } else {
                        this.onLoadData();
                    }
                }

            }, error => {

            });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteTimeScheduleTask(this.timeScheduleTask.timeScheduleTaskId).then((result) => {
                if (result.success) {
                    completion.completed(this.timeScheduleTask, true);
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

    // EVENTS

    private startTimeChanged() {
        this.$timeout(() => {
            this.calculateLength();
        });
    }

    private stopTimeChanged() {
        this.$timeout(() => {
            this.calculateLength();
        });
    }

    private lengthChanged() {
        this.$timeout(() => {
            if (this.timeScheduleTask.length < 0)
                this.timeScheduleTask.length = 0;

            this.timeScheduleTask.length = Math.round((this.timeScheduleTask.length / this.companySettingMinLength)) * this.companySettingMinLength;

            if (this.timeScheduleTask.length === 0)
                this.timeScheduleTask.minSplitLength = 0;
        });
    }

    private numberOfPersonsChanged() {
        this.$timeout(() => {
            if (this.timeScheduleTask.nbrOfPersons < 1)
                this.timeScheduleTask.nbrOfPersons = 1;
        });
    }

    private minSplitLengthChanged() {
        if (this.timeScheduleTask.minSplitLength < 0)
            this.timeScheduleTask.minSplitLength = 0;

        this.timeScheduleTask.minSplitLength = Math.round((this.timeScheduleTask.minSplitLength / this.companySettingMinLength)) * this.companySettingMinLength;
    }

    private onlyOneEmployeeChanged(item: any) {
        this.$timeout(() => {
            if (this.timeScheduleTask.onlyOneEmployee && this.timeScheduleTask.allowOverlapping)
                this.timeScheduleTask.allowOverlapping = false; //Not allowed same time with allowOverlapping
        });
    }

    private allowOverLappingChanged(item: any) {
        this.$timeout(() => {
            if (this.timeScheduleTask.onlyOneEmployee && this.timeScheduleTask.allowOverlapping)
                this.timeScheduleTask.onlyOneEmployee = false; //Not allowed same time with onlyoneperson
        });
    }

    // HELP-METHODS

    public closeModal(modified: boolean) {
        if (this.isModal) {
            if (modified && this.timeScheduleTaskId) {
                this.modal.close(this.timeScheduleTaskId);
            } else {
                this.modal.dismiss();
            }
        }
    }

    private calculateLength() {
        if (!this.timeScheduleTask || !this.timeScheduleTask.startTime || !this.timeScheduleTask.stopTime)
            return;

        while (this.timeScheduleTask.startTime >= this.timeScheduleTask.stopTime)
            this.timeScheduleTask.stopTime = this.timeScheduleTask.stopTime.addDays(1);

        while (this.timeScheduleTask.startTime.addDays(1) < this.timeScheduleTask.stopTime)
            this.timeScheduleTask.stopTime = this.timeScheduleTask.stopTime.addDays(-1);

        this.timeScheduleTask.length = this.timeScheduleTask.stopTime.diffMinutes(this.timeScheduleTask.startTime);
        if (this.timeScheduleTask.length < 0)
            this.timeScheduleTask.length = 0;
    }

    private setRecurrenceInfo() {
        if (this.timeScheduleTask) {
            DailyRecurrenceRangeDTO.setRecurrenceInfo(this.timeScheduleTask, this.translationService);
            this.scheduleService.getRecurrenceDescription(this.timeScheduleTask.recurrencePattern).then((x) => {
                this.timeScheduleTask["patternDescription"] = x;
            });
            if (this.timeScheduleTask.excludedDates && this.timeScheduleTask.excludedDates.length > 0)
                this.timeScheduleTask["excludedDatesDescription"] = _.map(this.timeScheduleTask.excludedDates, d => d.toLocaleDateString()).join(', ');
        }
    }

    private openRecurrencePatternDialog() {
        let params = new DailyRecurrenceParamsDTO(this.timeScheduleTask);
        let options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/DailyRecurrencePattern/Views/dailyRecurrencePattern.html"),
            controller: DailyRecurrencePatternController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                pattern: () => { return params.pattern },
                range: () => { return params.range },
                excludedDates: () => { return this.timeScheduleTask.excludedDates },
                date: () => { return params.date },
                hideRange: () => { return false }
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                params.parseResult(this.timeScheduleTask, result);
                this.setRecurrenceInfo();
                this.dirtyHandler.setDirty();
            }
        });
    }

    private openGeneratedNeedsDialog() {
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/TimeScheduleTasks/Dialogs/GeneratedNeedsDialog.html"),
            controller: GeneratedNeedsDialogController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                timeScheduleTaskId: () => { return this.timeScheduleTaskId },
                date: () => { return CalendarUtility.getDateToday() }
            }
        }

        this.$uibModal.open(options);
    }

    // VALIDATION

    private isDisabled() {
        return !this.dirtyHandler.isDirty || this.edit.$invalid;
    }

    private showValidationError() {
        const keys: string[] = [
            "core.warning",
            "common.name",
            "time.schedule.timescheduletask.lengthislowerthanallowed",
            "time.schedule.timescheduletask.startlaterthanstop",
            "time.schedule.timescheduletask.minsplitlengthislowerthanallowed",
            "time.schedule.timescheduletask.plannedtimelowerthanlength",
            "time.schedule.timescheduletask.shiftlengthlongerthanallowed",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
                if (this.timeScheduleTask) {

                    const errors = this['edit'].$error;

                    if (!this.timeScheduleTask.name) {
                        mandatoryFieldKeys.push("common.name");
                    }

                    if (errors['length'])
                        validationErrorStrings.push(terms["time.schedule.timescheduletask.lengthislowerthanallowed"]);
                    if (errors['stopTime'])
                        validationErrorStrings.push(terms["time.schedule.timescheduletask.startlaterthanstop"]);
                    if (errors['minSplitLength'])
                        validationErrorStrings.push(terms["time.schedule.timescheduletask.minsplitlengthislowerthanallowed"]);
                    if (errors['plannedTime'])
                        validationErrorStrings.push(terms["time.schedule.timescheduletask.plannedtimelowerthanlength"]);
                }
            });
        });
    }
}
