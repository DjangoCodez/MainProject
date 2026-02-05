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
import { IFocusService } from "../../../Core/Services/FocusService";
import { IScheduleService } from "../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { Feature, CompanySettingType, SoeEntityState, SoeTimeCodeType, SoeTimeCodeBreakTimeType } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { TimeScheduleTemplateBlockSlim, TimeScheduleTemplateHeadDTO, TimeScheduleTemplatePeriodDTO } from "../../../Common/Models/TimeScheduleTemplateDTOs";
import { IEmployeeSchedulePlacementGridViewDTO, ISmallGenericType, ITimeCodeDTO, ITimeScheduleTemplateBlockDTO } from "../../../Scripts/TypeLite.Net4";
import { IEmployeeService } from "../../Employee/EmployeeService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Constants } from "../../../Util/Constants";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { ITimeService } from "../../Time/TimeService";
import { Guid } from "../../../Util/StringUtility";
import { CoreUtility } from "../../../Util/CoreUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    private timeScheduleTemplateHeadId: number;
    private templateHead: TimeScheduleTemplateHeadDTO;
    private placement: IEmployeeSchedulePlacementGridViewDTO;
    private employees: ISmallGenericType[] = [];
    private employee: ISmallGenericType;
    private shiftTypes: ISmallGenericType[] = [];
    private dayNumbers: ISmallGenericType[] = [];
    private timeCodeBreaks: ITimeCodeDTO[];
    private blocks: TimeScheduleTemplateBlockSlim[] = [];
    private selectedBlock: TimeScheduleTemplateBlockSlim;

    // Permissions
    private modifyAccountPermission: boolean = false;
    private lockTemplatePermission: boolean = false;
    private readOnlyMode: boolean = false;

    // Company settings
    private timeCodeLoaded = false;
    private defaultTimeCodeId = 0;
    private maxNoOfBrakes = 0;
    private defaultStartOnFirstDayOfWeek = false;
    private useStopDate = false;
    private shiftTypeMandatory = false;
    private clockRounding = 0;

    // Properties
    private get startDate(): Date {
        return this.templateHead?.startDate;
    }
    private set startDate(date: Date) {
        if (this.templateHead) {
            if (date && this.templateHead.startOnFirstDayOfWeek) {
                date = date.beginningOfWeek();
            }

            this.templateHead.startDate = date;
        }
    }

    // Flags
    private templateIsInUse: boolean = false;
    private hasPlacements: boolean = false;
    private isUpdated: boolean = false;
    private hasOverlappingBreakWindows: boolean = false;

    // Sums
    private totalWorkMinutes = 0;
    private workMinutes = 0;
    private breakMinutes = 0;
    private weekAverageMinutes = 0;

    private startDateOptions = {
        dateDisabled: this.disabledStartDates,
        customClass: this.getStartDateDayClass,
        controller: this,
    };

    private modal;
    private isModal = false;
    private modalInstance: any;
    private newForEmployeeId: number;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        protected $uibModal,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private employeeService: IEmployeeService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private timeService: ITimeService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private notificationService: INotificationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;

            this.onInit(parameters);
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    // SETUP

    public onInit(parameters: any) {
        this.timeScheduleTemplateHeadId = parameters.id;
        this.guid = parameters.guid;

        if (parameters.source) {
            if (parameters.source === 'employeeTemplate')
                this.newForEmployeeId = parameters.employeeId;
            else if (parameters.source === 'templateGroup')
                this.readOnlyMode = true;
        }

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Schedule_Templates_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        if (!this.isModal)
            this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => !this.timeScheduleTemplateHeadId);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Schedule_Templates_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_Templates_Edit].modifyPermission && !this.readOnlyMode;
    }

    // LOOKUPS

    private doLookups() {
        return this.progress.startLoadingProgress([
            () => this.loadModifyPermissions(),
            () => this.loadCompanySettings(),
            () => this.loadEmployees(true),
            () => this.loadShiftTypes(),
            () => this.loadTimeCodeBreaks()
        ]).then(() => {
            if (this.timeScheduleTemplateHeadId)
                this.onLoadData();
            else
                this.new();
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        let features: number[] = [];
        features.push(Feature.Time_Schedule_SchedulePlanning_LockTemplateSchedule);
        features.push(Feature.Time_Schedule_Templates_Edit_ChangeAccount);

        return this.coreService.hasModifyPermissions(features).then((x) => {
            this.modifyAccountPermission = x[Feature.Time_Schedule_Templates_Edit_ChangeAccount];
            this.lockTemplatePermission = x[Feature.Time_Schedule_SchedulePlanning_LockTemplateSchedule];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimeDefaultTimeCode);
        settingTypes.push(CompanySettingType.TimeMaxNoOfBrakes);
        settingTypes.push(CompanySettingType.TimeDefaultStartOnFirstDayOfWeek);
        settingTypes.push(CompanySettingType.TimeUseStopDateOnTemplate);
        settingTypes.push(CompanySettingType.TimeShiftTypeMandatory);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningClockRounding);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultTimeCodeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultTimeCode);
            this.timeCodeLoaded = true;

            this.maxNoOfBrakes = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeMaxNoOfBrakes);
            this.defaultStartOnFirstDayOfWeek = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeDefaultStartOnFirstDayOfWeek);
            this.useStopDate = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeUseStopDateOnTemplate);
            this.shiftTypeMandatory = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeShiftTypeMandatory);
            this.clockRounding = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningClockRounding);
        });
    }

    private loadEmployees(useCache: boolean): ng.IPromise<any> {
        return this.employeeService.getEmployeesDict(false, true, false, true, useCache).then(x => {
            this.employees = x;
        });
    }

    private loadShiftTypes(): ng.IPromise<any> {
        return this.scheduleService.getShiftTypesDict(true).then(x => {
            this.shiftTypes = x;
        });
    }

    private loadTimeCodeBreaks(): ng.IPromise<any> {
        return this.timeService.getTimeCodes(SoeTimeCodeType.Break, true, false).then(x => {
            this.timeCodeBreaks = x;
            this.timeCodeBreaks.splice(0, 0, <ITimeCodeDTO>{ timeCodeId: 0, name: " " });
        });
    }

    private onLoadData(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.sharedScheduleService.getTimeScheduleTemplate(this.timeScheduleTemplateHeadId, true, false).then(x => {
                this.isNew = false;
                this.templateHead = x;

                if (this.templateHead.employeeId)
                    this.loadPlacement();

                this.employee = this.templateHead && this.templateHead.employeeId ? this.employees.find(e => e.id === this.templateHead.employeeId) : null;

                if (this.templateHead && this.templateHead.employeeSchedules.length > 0) {
                    this.templateIsInUse = true;
                    this.hasPlacements = true;
                }

                this.setupDayNumbers();
                this.setupBlocks();
                this.createPeriods();
                this.sortBlocks();
                this.calculateTotalLengths();

                this.isUpdated = false;
            })
        ]);
    }

    private loadPlacement(): ng.IPromise<any> {
        return this.sharedScheduleService.getLastPlacementForEmployee(this.templateHead.employeeId, this.templateHead && this.templateHead.timeScheduleTemplateHeadId ? this.templateHead.timeScheduleTemplateHeadId : 0).then(x => {
            this.placement = x;
        });
    }

    // ACTIONS

    private new() {
        this.isNew = true;
        this.timeScheduleTemplateHeadId = 0;
        this.templateHead = new TimeScheduleTemplateHeadDTO();
        this.templateHead.isActive = true;
        this.templateHead.startOnFirstDayOfWeek = this.defaultStartOnFirstDayOfWeek;
        this.templateHead.setTypes();
        this.templateHead.noOfDays = 7;
        this.templateHead.simpleSchedule = true;

        this.templateIsInUse = false;

        this.setupDayNumbers();
        this.createPeriods();

        if (this.newForEmployeeId) {
            this.setEmployeeIdFromEmployee();
            if (!this.employee) {
                this.loadEmployees(false).then(() => {
                    this.setEmployeeIdFromEmployee();
                });
            }
        }
    }

    private setEmployeeIdFromEmployee() {
        if (!this.newForEmployeeId)
            return;

        this.employee = this.employees.find(e => e.id === this.newForEmployeeId);
        if (this.employee)
            this.templateHead.employeeId = this.newForEmployeeId;
    }

    protected copy() {
        super.copy();

        // Clear template head
        this.timeScheduleTemplateHeadId = this.templateHead.timeScheduleTemplateHeadId = 0;
        this.templateHead.name = this.templateHead.description = undefined;
        this.employee = undefined;
        this.templateHead.employeeId = 0;
        this.templateHead.startOnFirstDayOfWeek = this.defaultStartOnFirstDayOfWeek;
        this.templateHead.locked = false;
        this.templateHead.isActive = true;

        this.templateHead.created = null;
        this.templateHead.createdBy = null;
        this.templateHead.modified = null;
        this.templateHead.modifiedBy = null;

        this.templateIsInUse = false;

        // Clear template period data
        _.forEach(this.templateHead.timeScheduleTemplatePeriods, period => {
            period.timeScheduleTemplateHeadId = 0;
            period.timeScheduleTemplatePeriodId = 0;

            // Clear template block data
            _.forEach(period.timeScheduleTemplateBlocks, block => {
                block.timeScheduleTemplatePeriodId = 0;
            });
        });

        // Clear template block data
        _.forEach(this.blocks, block => {
            block.timeScheduleTemplatePeriodId = 0;
        });

        this.focusService.focusByName("ctrl_templateHead_name");
    }

    private validateExistingPlacement(): ng.IPromise<boolean> {
        let deferral = this.$q.defer<boolean>();

        // Check if any placement exists on current start date
        if (this.templateHead.employeeId && this.templateHead.startDate) {
            this.scheduleService.hasEmployeeSchedule(this.templateHead.employeeId, this.templateHead.startDate).then(result => {
                if (result) {
                    const keys: string[] = [
                        "core.warning",
                        "time.schedule.planning.templateschedule.placementexistsendafter"
                    ];

                    this.translationService.translateMany(keys).then(terms => {
                        const modal = this.notificationService.showDialogEx(terms["core.warning"], terms["time.schedule.planning.templateschedule.placementexistsendafter"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                        modal.result.then(val => {
                            if (val)
                                deferral.resolve(true);
                        }, (reason) => {
                            deferral.resolve(false);
                        });
                    });
                } else {
                    deferral.resolve(true);
                }
            });
        } else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private save(checkExistingPlacement: boolean = true) {
        if (checkExistingPlacement) {
            this.validateExistingPlacement().then(passed => {
                if (passed) {
                    this.save(false);
                    return;
                }
            });
            return;
        }

        this.templateHead.employeeId = (this.employee ? this.employee.id : undefined);
        this.templateHead.firstMondayOfCycle = this.templateHead.startDate.beginningOfWeek();

        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveTimeScheduleTemplate(this.templateHead, this.blocks).then(result => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        this.timeScheduleTemplateHeadId = result.integerValue;
                        this.isUpdated = true;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.templateHead);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                if (this.isModal)
                    this.closeModal(true, false);
                else {
                    this.dirtyHandler.clean();
                    this.onLoadData();
                }
            }, error => {

            });
    }

    private delete() {
        if (!this.templateHead.timeScheduleTemplateHeadId)
            return;

        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteTimeScheduleTemplateHead(this.templateHead.timeScheduleTemplateHeadId).then(result => {
                if (result.success) {
                    completion.completed(this.templateHead, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            if (this.isModal)
                this.closeModal(true, true);
            else
                super.closeMe(true);
        });
    }

    // EVENTS

    public closeModal(modified: boolean, deleted: boolean) {
        if (this.isModal) {
            if (this.timeScheduleTemplateHeadId) {
                this.modal.close({ modified: modified, deleted: deleted, id: this.timeScheduleTemplateHeadId });
            } else {
                this.modal.dismiss();
            }
        }
    }

    private startOnFirstDayOfWeekChanged() {
        this.$timeout(() => {
            if (this.templateHead && this.templateHead.startOnFirstDayOfWeek && this.templateHead.startDate && !this.templateHead.startDate.isBeginningOfWeek())
                this.templateHead.startDate = this.templateHead.startDate.beginningOfWeek();
        });
    }

    private noOfDaysChanged() {
        let prevNbrOfDays: number = this.templateHead.noOfDays;

        this.$timeout(() => {
            // Check that "number of days" is not changed to a value that is lower than any registered period
            // Perids with a length of zero are ignored
            if (prevNbrOfDays > this.templateHead.noOfDays) {
                if (_.filter(this.blocks, b => b.dayNumber > this.templateHead.noOfDays && b.shiftLength > 0).length > 0) {
                    let keys: string[] = [
                        "time.schedule.template.cantlowernoofdays.title",
                        "time.schedule.template.cantlowernoofdays.message"
                    ];

                    this.translationService.translateMany(keys).then(terms => {
                        this.notificationService.showDialogEx(terms["time.schedule.template.cantlowernoofdays.title"], terms["time.schedule.template.cantlowernoofdays.message"], SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                    });

                    this.templateHead.noOfDays = prevNbrOfDays;
                    return;
                }
            }

            // Warn if user entered more than 366 days
            if (this.templateHead.noOfDays > 366) {
                let keys: string[] = [
                    "core.warning",
                    "time.schedule.template.morethanoneyear.message"
                ];

                this.translationService.translateMany(keys).then(terms => {
                    const modal = this.notificationService.showDialogEx(terms["core.warning"], terms["time.schedule.template.morethanoneyear.message"].format(this.templateHead.noOfDays.toString()), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(val => {
                        if (val) {
                            this.setupDayNumbers();
                            this.createPeriods();
                        } else {
                            this.templateHead.noOfDays = prevNbrOfDays;
                        }
                    }, (reason) => {
                        this.templateHead.noOfDays = prevNbrOfDays;
                    });
                });
            } else {
                this.setupDayNumbers();
                this.createPeriods();
            }
        });
    }

    private dayNumberChanged(block: TimeScheduleTemplateBlockSlim, breakNr: number) {
        this.$timeout(() => {
            if (block.timeScheduleTemplatePeriodId) {
                let period = this.getPeriod(block.dayNumber);
                if (period)
                    block.timeScheduleTemplatePeriodId = period.timeScheduleTemplatePeriodId;
            }
            this.validateOverlappingBreaks(block.dayNumber);
        });
    }

    private timeChanged(block: TimeScheduleTemplateBlockSlim) {
        this.$timeout(() => {
            if (block.startTime && block.stopTime) {
                // Handle over midnight
                if (block.stopTime.isBeforeOnMinute(block.startTime))
                    block.stopTime = block.stopTime.addDays(1);
                // Handle switch back (if end is set to less than start, and then back again)
                // So, if shift ends more than 24 hours after it starts, reduce end by 24 hours
                while (block.stopTime.isSameOrAfterOnMinute(block.startTime.addDays(1))) {
                    block.stopTime = block.stopTime.addDays(-1);
                }
            }

            this.validateOverlappingBreaks(block.dayNumber);
            this.calculateTotalLengths();
        });
    }

    private breakTimeCodeChanged(block: TimeScheduleTemplateBlockSlim, breakNr: number) {
        this.$timeout(() => {
            this.setBreakTimeCode(block, block[`break${breakNr}TimeCodeId`], breakNr);
            this.calculateTotalLengths();
        });
    }

    private deleteBlock(block: TimeScheduleTemplateBlockSlim) {
        _.pull(this.blocks, block);
        this.setDirty();
        this.selectedBlock = null;
        this.validateOverlappingBreaks(block.dayNumber);
        this.calculateTotalLengths();
    }

    private addBlock(copyFrom: TimeScheduleTemplateBlockSlim) {
        let block: TimeScheduleTemplateBlockSlim = new TimeScheduleTemplateBlockSlim();
        block.timeCodeId = copyFrom ? copyFrom.timeCodeId : this.defaultTimeCodeId;
        block.startTime = copyFrom ? CalendarUtility.convertToDate(copyFrom.stopTime) : CalendarUtility.DefaultDateTime();
        block.stopTime = copyFrom ? CalendarUtility.convertToDate(copyFrom.stopTime) : CalendarUtility.DefaultDateTime();

        if (copyFrom) {
            // Copy from selected row
            block.timeScheduleTemplatePeriodId = copyFrom.timeScheduleTemplatePeriodId;
            block.dayNumber = copyFrom.dayNumber;
            block.shiftTypeId = copyFrom.shiftTypeId;
        } else {
            // Add after last row
            let lastBlock = this.blocks.length > 0 ? _.last(this.blocks) : null;

            let dayNumber = lastBlock ? lastBlock.dayNumber : 0;
            if (dayNumber < this.templateHead.noOfDays)
                dayNumber++;
            block.dayNumber = dayNumber;

            let period = this.getPeriod(dayNumber);
            if (period) {
                block.timeScheduleTemplatePeriodId = period.timeScheduleTemplatePeriodId;
            }

            // Copy breaks from last row
            if (lastBlock) {
                for (let i = 1; i <= this.maxNoOfBrakes; i++) {
                    if (lastBlock[`break${i}TimeCodeId`])
                        this.setBreakTimeCode(block, lastBlock[`break${i}TimeCodeId`], i);
                }
            }
        }

        this.blocks.push(block);
        this.sortBlocks();
        this.setDirty();

        this.calculateTotalLengths();

        this.selectedBlock = block;
    }

    private sortBlocks() {
        this.blocks = _.orderBy(this.blocks, ['dayNumber', 'startTime']);
    }

    // HELP-METHODS

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    private disabledStartDates(data) {
        let self: EditController = this['datepickerOptions']['controller'];
        let limit: boolean = self && self.templateHead && self.templateHead.startOnFirstDayOfWeek;

        // Only mondays are valid
        return limit && data.mode === 'day' && data.date.getDay() !== 1;
    }

    private getStartDateDayClass(data) {
        let self: EditController = this['datepickerOptions']['controller'];
        let limit: boolean = self && self.templateHead && self.templateHead.startOnFirstDayOfWeek;

        // Only mondays are valid
        return (limit && data.mode === 'day' && data.date.getDay() !== 1) ? 'disabledDate' : '';
    }

    private setupDayNumbers() {
        this.dayNumbers = [];
        if (this.templateHead && this.templateHead.noOfDays) {
            for (let i = 1; i <= this.templateHead.noOfDays; i++) {
                this.dayNumbers.push({ id: i, name: i.toString() });
            }
        }
    }

    private getPeriod(dayNumber: number): TimeScheduleTemplatePeriodDTO {
        return _.find(this.templateHead.timeScheduleTemplatePeriods, p => p.dayNumber === dayNumber);
    }

    private getBlocksForPeriod(dayNumber: number): TimeScheduleTemplateBlockSlim[] {
        return _.filter(this.blocks, b => b.dayNumber === dayNumber);
    }

    private createPeriods() {
        for (let i = 1; i <= this.templateHead.noOfDays; i++) {
            let period = this.getPeriod(i);
            if (!period) {
                period = new TimeScheduleTemplatePeriodDTO();
                period.dayNumber = i;
                period.timeScheduleTemplateBlocks = [];
                period.blocks = [];
                this.templateHead.timeScheduleTemplatePeriods.push(period);
            }

            if (period && this.getBlocksForPeriod(i).length === 0) {
                let block: TimeScheduleTemplateBlockSlim = new TimeScheduleTemplateBlockSlim();
                block.timeScheduleTemplatePeriodId = period.timeScheduleTemplatePeriodId;
                block.timeCodeId = this.defaultTimeCodeId;
                block.dayNumber = period.dayNumber;
                block.startTime = CalendarUtility.DefaultDateTime();
                block.stopTime = CalendarUtility.DefaultDateTime();
                if (!period.blocks)
                    period.blocks = [];
                period.blocks.push(block);
                this.blocks.push(block);
            }
        }

        _.pullAll(this.templateHead.timeScheduleTemplatePeriods, _.filter(this.templateHead.timeScheduleTemplatePeriods, p => p.dayNumber > this.templateHead.noOfDays));
        _.pullAll(this.blocks, _.filter(this.blocks, b => b.dayNumber > this.templateHead.noOfDays));
    }

    private setupBlocks() {
        this.blocks = [];

        _.forEach(_.orderBy(_.filter(this.templateHead.timeScheduleTemplatePeriods, p => p.isActive), p => p.dayNumber), period => {
            // Shifts
            _.forEach(_.orderBy(_.filter(period.timeScheduleTemplateBlocks, b => !b.isBreak && b.state === SoeEntityState.Active), b => b.startTime), block => {
                this.blocks.push(this.createBlock(block));
            });

            // Breaks
            let breakNr: number = 0;
            _.forEach(_.orderBy(_.filter(period.timeScheduleTemplateBlocks, b => b.isBreak && b.state === SoeEntityState.Active), b => b.startTime), brk => {
                breakNr++;
                brk.startTime = CalendarUtility.convertToDate(brk.startTime);
                brk.stopTime = CalendarUtility.convertToDate(brk.stopTime);

                let workRow: TimeScheduleTemplateBlockSlim = null;
                let workRows: TimeScheduleTemplateBlockSlim[] = _.orderBy(_.filter(this.blocks, b => b.timeScheduleTemplatePeriodId === brk.timeScheduleTemplatePeriodId), b => b.startTime);
                if (workRows.length > 0) {
                    _.forEach(workRows, row => {
                        if (CalendarUtility.getIntersectingDuration(brk.startTime, brk.stopTime, row.startTime, row.stopTime) > 0) {
                            workRow = row;
                            return false;
                        }
                    });

                    if (!workRow)
                        workRow = workRows[0];

                    this.setBreakTimeCode(workRow, brk.timeCodeId, breakNr);
                }
            });
        });
    }

    private createBlock(block: ITimeScheduleTemplateBlockDTO): TimeScheduleTemplateBlockSlim {
        let slim: TimeScheduleTemplateBlockSlim = new TimeScheduleTemplateBlockSlim();

        slim.timeScheduleTemplateBlockId = block.timeScheduleTemplateBlockId;
        slim.timeScheduleTemplatePeriodId = block.timeScheduleTemplatePeriodId;
        slim.timeCodeId = block.timeCodeId;
        slim.dayNumber = block.dayNumber;
        slim.startTime = CalendarUtility.convertToDate(block.startTime);
        slim.stopTime = CalendarUtility.convertToDate(block.stopTime);
        slim.shiftTypeId = block.shiftTypeId;

        return slim;
    }

    private setBreakTimeCode(block: TimeScheduleTemplateBlockSlim, timeCodeId: number, breakNr: number) {
        if (block) {
            block[`break${breakNr}TimeCodeId`] = timeCodeId;
            let timeCode = this.timeCodeBreaks.find(t => t.timeCodeId === timeCodeId);
            block[`break${breakNr}Length`] = timeCode ? timeCode.defaultMinutes : 0;

            this.validateOverlappingBreaks(block.dayNumber);
        }
    }

    private calculateTotalLengths() {
        this.totalWorkMinutes = 0;
        this.workMinutes = 0;
        this.breakMinutes = 0;

        _.forEach(this.blocks, block => {
            this.totalWorkMinutes += block.duration + block.breakLength;
            this.workMinutes += block.duration;
            this.breakMinutes += block.breakLength;
        });

        let nbrOfDays = this.templateHead.noOfDays
        if (nbrOfDays < 7)
            nbrOfDays = 7;
        this.weekAverageMinutes = this.workMinutes / (nbrOfDays / 7);
    }

    isAfterToday(date: Date): boolean {
        return date.isAfterOnDay(CalendarUtility.DefaultDateTime());
    }

    // VALIDATION

    private validateOverlappingBreaks(dayNumber: number) {
        let dayBlocks = _.orderBy(_.filter(this.blocks, b => b.dayNumber === dayNumber), ['startTime', 'stopTime'], ['asc', 'desc']);
        if (dayBlocks.length > 0) {
            let breaks: ITimeCodeDTO[] = [];
            _.forEach(dayBlocks, block => {
                for (let i = 1; i <= this.maxNoOfBrakes; i++) {
                    if (block[`break${i}TimeCodeId`]) {
                        let timeCode = _.find(this.timeCodeBreaks, t => t.timeCodeId === block[`break${i}TimeCodeId`]);
                        if (timeCode)
                            breaks.push(timeCode);
                    }
                }
            });

            this.hasOverlappingBreakWindows = this.isTimeCodeBreaksOverlapping(dayBlocks[0].startTime, _.last(dayBlocks).stopTime, breaks);
        }
    }

    private isTimeCodeBreaksOverlapping(scheduleStart: Date, scheduleStop: Date, breaks: ITimeCodeDTO[]): boolean {
        if (breaks.length === 0)
            return false;

        let isOverlapping: boolean = false;
        let today: Date = CalendarUtility.getDateToday();
        let breakWindows: Date[][] = [];

        let scheduleStartMinutes = (scheduleStart.getHours() * 60 + scheduleStart.getMinutes());
        let scheduleStopMinutes = (scheduleStop.getHours() * 60 + scheduleStop.getMinutes());

        // Loop through all breaks and calculate a start/stop date for each of them
        _.forEach(breaks, timeCodeBreak => {
            let startMinutes = 0;
            let stopMinutes = 0;

            switch (timeCodeBreak.startType) {
                case SoeTimeCodeBreakTimeType.Clock:
                    startMinutes = timeCodeBreak.startTimeMinutes;
                    break;
                case SoeTimeCodeBreakTimeType.ScheduleIn:
                    startMinutes = scheduleStartMinutes + timeCodeBreak.startTimeMinutes;
                    break;
                case SoeTimeCodeBreakTimeType.ScheduleOut:
                    startMinutes = scheduleStopMinutes + timeCodeBreak.startTimeMinutes;
                    break;
            }
            switch (timeCodeBreak.stopType) {
                case SoeTimeCodeBreakTimeType.Clock:
                    stopMinutes = timeCodeBreak.stopTimeMinutes;
                    break;
                case SoeTimeCodeBreakTimeType.ScheduleIn:
                    stopMinutes = scheduleStartMinutes + timeCodeBreak.stopTimeMinutes;
                    break;
                case SoeTimeCodeBreakTimeType.ScheduleOut:
                    stopMinutes = scheduleStopMinutes + timeCodeBreak.stopTimeMinutes;
                    break;
            }

            let arr: Date[] = [];
            arr.push(today.addMinutes(startMinutes));
            arr.push(today.addMinutes(stopMinutes));
            breakWindows.push(arr);
        });

        // Compare all break windows to see if any overlap
        let i = 0;
        let j;
        _.forEach(breakWindows, outerArr => {
            i++;
            j = 0;
            _.forEach(breakWindows, innerArr => {
                // Do not compare the same range from outer and inner
                j++;
                if (i != j) {
                    if (CalendarUtility.getIntersectingDuration(outerArr[0], outerArr[1], innerArr[0], innerArr[1])) {
                        isOverlapping = true;
                        return false;
                    }
                }
            });
        });

        return isOverlapping;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.templateHead) {
                const errors = this['edit'].$error;

                // Mandatory fields
                if (!this.templateHead.name)
                    mandatoryFieldKeys.push("common.name");
                if (!this.templateHead.noOfDays)
                    mandatoryFieldKeys.push("time.schedule.template.noofdays");

                // Dates
                if (errors['startDateMandatory'])
                    validationErrorKeys.push("time.schedule.template.startdatemandatory");
                if (errors['mustStartOnMonday'])
                    validationErrorKeys.push("time.schedule.template.muststartonmonday");
                if (errors['stopDateAfterStartDate'])
                    validationErrorKeys.push("time.schedule.template.startdateafterstopdate");
                if (errors['validStopDateForPlacement'])
                    validationErrorKeys.push("time.schedule.template.stopdatebeforeplacementend");

                // Blocks
                if (errors['shiftTypeMandatory'])
                    validationErrorKeys.push("time.schedule.template.shifttypemandatory");
                if (errors['duration'])
                    validationErrorKeys.push("time.schedule.template.negativeduration");
                if (errors['overlapping'])
                    validationErrorKeys.push("time.schedule.template.overlappingtimes");
                if (errors['overlappingBreaks'])
                    validationErrorKeys.push("time.schedule.template.overlappingbreaks");
                if (errors['overlappingBreakWindows'])
                    validationErrorKeys.push("time.schedule.template.overlappingbreakwindows");
            }
        });
    }
}
