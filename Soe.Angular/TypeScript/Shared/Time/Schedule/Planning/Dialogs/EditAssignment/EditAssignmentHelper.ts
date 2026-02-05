import { ICoreService } from "../../../../../../Core/Services/CoreService";
import { IScheduleService as ISharedScheduleService } from "../../../../../../Shared/Time/Schedule/ScheduleService";
import { ShiftDTO } from "../../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { CompanySettingType, Feature, UserSettingType } from "../../../../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../../../../Util/SettingsUtility";
import { IUrlHelperService } from "../../../../../../Core/Services/UrlHelperService";
import { ShiftTypeDTO } from "../../../../../../Common/Models/ShiftTypeDTO";
import { AccountingSettingsRowDTO } from "../../../../../../Common/Models/AccountingSettingsRowDTO";
import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { TimeScheduleTypeFactorSmallDTO, TimeScheduleTypeSmallDTO } from "../../../../../../Common/Models/TimeScheduleTypeDTO";
import { CalendarUtility } from "../../../../../../Util/CalendarUtility";
import { ITimeScheduleTypeSmallDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../../Util/Constants";
import { EditAssignmentController } from "./EditAssignmentController";
import { EmployeeListDTO } from "../../../../../../Common/Models/EmployeeListDTO";

export class EditAssignmentHelper {
    // Data
    private hiddenEmployeeId: number = 0;
    private vacantEmployeeIds: number[] = [];
    private allEmployees: EmployeeListDTO[];

    // Company settings
    private skillCantBeOverridden: boolean = false;
    private clockRounding: number = 0;
    private shiftTypeMandatory: boolean = false;
    private keepShiftsTogether: boolean = false;
    private orderPlanningIgnoreScheduledBreaksOnAssignment = false;
    public dayViewStartTime: number = 0;   // Minutes from midnight
    public dayViewEndTime: number = 0;     // Minutes from midnight

    // User settings
    private showAvailability: boolean = false;

    // Lookups
    private allShiftTypes: ShiftTypeDTO[];
    private shiftTypes: any[];
    private shiftTypeIds: number[];
    private timeScheduleTypes: ITimeScheduleTypeSmallDTO[];

    // Flags
    private showSkills: boolean = false;
    private skipXEMailOnChanges: boolean = false;
    private skipWorkRules: boolean = false;

    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private sharedScheduleService: ISharedScheduleService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private isAdmin: boolean,
        private employeeId: number,
        private isReadOnly: boolean,
        private onLoadComplete: Function) {

        $q.all([
            this.loadCompanySettings(),
            this.loadUserSettings(),
            this.loadEmployeesSmall(),
            this.loadShiftTypes(),
            this.loadUserShiftTypes(),
            this.loadTimeScheduleTypes()
        ]).then(() => {
            this.setShiftTypes();
            this.onLoadComplete();
        })
    }

    // SERVICE CALLS

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        // Common
        settingTypes.push(CompanySettingType.TimeUseVacant);
        settingTypes.push(CompanySettingType.TimeSkillCantBeOverridden);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningDayViewStartTime);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningDayViewEndTime);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningClockRounding);
        settingTypes.push(CompanySettingType.TimeShiftTypeMandatory);
        settingTypes.push(CompanySettingType.TimeDefaultDoNotKeepShiftsTogether);
        settingTypes.push(CompanySettingType.OrderPlanningIgnoreScheduledBreaksOnAssignment);

        return this.coreService.getCompanySettings(settingTypes).then(x => {

            // Common
            this.getHiddenEmployeeId();
            if (SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeUseVacant))
                this.getVacantEmployeeIds();
            this.skillCantBeOverridden = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSkillCantBeOverridden);
            this.dayViewStartTime = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningDayViewStartTime);
            this.dayViewEndTime = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningDayViewEndTime);
            this.clockRounding = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningClockRounding);
            this.shiftTypeMandatory = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeShiftTypeMandatory);
            this.keepShiftsTogether = !SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeDefaultDoNotKeepShiftsTogether);
            this.orderPlanningIgnoreScheduledBreaksOnAssignment = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.OrderPlanningIgnoreScheduledBreaksOnAssignment);
        });
    }

    private getHiddenEmployeeId() {
        this.sharedScheduleService.getHiddenEmployeeId().then((id) => {
            this.hiddenEmployeeId = id;
        });
    }

    private getVacantEmployeeIds() {
        this.sharedScheduleService.getVacantEmployeeIds().then((ids) => {
            this.vacantEmployeeIds = ids;
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(UserSettingType.TimeSchedulePlanningShowAvailability);
        settingTypes.push(UserSettingType.TimeSchedulePlanningDisableBreaksWithinHolesWarning);

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.showAvailability = SettingsUtility.getBoolUserSetting(x, UserSettingType.TimeSchedulePlanningShowAvailability);
        });
    }

    private loadEmployeesSmall() {
        var dateFrom: Date = CalendarUtility.getDateToday().beginningOfWeek();
        var dateTo: Date = CalendarUtility.getDateToday().endOfWeek();
        this.sharedScheduleService.getEmployeesSmallForPlanning(dateFrom, dateTo, true, true).then(x => {
            this.allEmployees = (<any>x).map(e => {
                var obj = new EmployeeListDTO();
                obj.isVisible = true;
                angular.extend(obj, e);
                return obj;
            });
        });
    }

    private loadShiftTypes(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypes(true, true, false, false, false, false).then(x => {
            // Convert to typed DTOs
            this.allShiftTypes = x.map(s => {
                var obj = new ShiftTypeDTO();
                angular.extend(obj, s);

                var aobj = new AccountingSettingsRowDTO(0);
                angular.extend(aobj, obj.accountingSettings);
                obj.accountingSettings = aobj;

                return obj;
            });

            // Insert empty shift type
            this.translationService.translate("core.notselected").then((term) => {
                var shiftType: ShiftTypeDTO = new ShiftTypeDTO();
                shiftType.shiftTypeId = 0;
                shiftType.name = term;
                shiftType.color = Constants.SHIFT_TYPE_UNSPECIFIED_COLOR;
                this.allShiftTypes.splice(0, 0, shiftType);
            });
        });
    }

    private loadUserShiftTypes(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypeIdsForUser(this.employeeId, this.isAdmin, false).then(x => {
            this.shiftTypeIds = x;
        });
    }

    private setShiftTypes() {
        this.shiftTypes = [];
        _.forEach(this.allShiftTypes, shiftType => {
            if (_.includes(this.shiftTypeIds, shiftType.shiftTypeId) || shiftType.shiftTypeId === 0) {
                var isValid: boolean = true;


                if (isValid) {
                    this.shiftTypes.push({ id: shiftType.shiftTypeId, label: shiftType.name, timeScheduleTypeId: shiftType.timeScheduleTypeId });
                    if (!this.showSkills && shiftType.shiftTypeSkills && shiftType.shiftTypeSkills.length > 0)
                        this.showSkills = true;
                }
            }
        });
    }

    private loadTimeScheduleTypes(): ng.IPromise<any> {
        return this.sharedScheduleService.getTimeScheduleTypes(false, true, true).then(x => {
            this.timeScheduleTypes = x;

            // Add empty row
            var t = new TimeScheduleTypeSmallDTO();
            t.timeScheduleTypeId = 0;
            t.name = '';
            this.timeScheduleTypes.splice(0, 0, t);
        });
    }

    // PUBLIC METHODS

    public loadShift(timeScheduleTemplateBlockId: number): ng.IPromise<ShiftDTO> {
        return this.sharedScheduleService.getShift(timeScheduleTemplateBlockId, true);
    }

    public loadLinkedShifts(shift: ShiftDTO): ng.IPromise<ShiftDTO[]> {
        return this.sharedScheduleService.getShiftsForDay(shift.employeeId, shift.actualStartDate, [shift.type], false, false, shift.link, true, false, false, true);
    }

    public openEditAssignmentDialog(shift: ShiftDTO, onDialogClosed: Function) {
        if (!shift)
            return;

        // Read only
        var readOnly: boolean = this.isReadOnly;
        if (!readOnly && shift)
            readOnly = (shift.isReadOnly || shift.isAbsence || shift.isAbsenceRequest);

        // Show edit order dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Planning/Dialogs/EditAssignment/Views/editAssignment.html"),
            controller: EditAssignmentController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                currentEmployeeId: () => { return this.employeeId },
                isReadonly: () => { return readOnly },
                modifyPermission: () => { return true },
                shift: () => { return shift },
                date: () => { return shift.startTime },
                employeeId: () => { return shift.employeeId },
                shiftTypes: () => { return this.shiftTypes; },
                employees: () => { return this.allEmployees; },
                hiddenEmployeeId: () => { return this.hiddenEmployeeId; },
                vacantEmployeeIds: () => { return this.vacantEmployeeIds; },
                showSkills: () => { return this.showSkills; },
                dayStartTime: () => { return this.dayViewStartTime; },
                dayEndTime: () => { return this.dayViewEndTime; },
                showAvailability: () => { return this.showAvailability; },
                clockRounding: () => { return this.clockRounding; },
                shiftTypeMandatory: () => { return this.shiftTypeMandatory; },
                keepShiftsTogether: () => { return this.keepShiftsTogether; },
                skillCantBeOverridden: () => { return this.skillCantBeOverridden; },
                skipWorkRules: () => { return this.skipWorkRules; },
                skipXEMailOnChanges: () => { return this.skipXEMailOnChanges; },
                ignoreScheduledBreaksOnAssignment: () => { return this.orderPlanningIgnoreScheduledBreaksOnAssignment; }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            onDialogClosed(result);
        });
    }
}