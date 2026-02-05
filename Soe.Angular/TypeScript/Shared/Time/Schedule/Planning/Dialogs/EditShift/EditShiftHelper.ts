import { ICoreService } from "../../../../../../Core/Services/CoreService";
import { IScheduleService as ISharedScheduleService } from "../../../../../../Shared/Time/Schedule/ScheduleService";
import { ShiftDTO } from "../../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { CompanySettingType, Feature, TimeSchedulePlanningDisplayMode, UserSettingType } from "../../../../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../../../../Util/SettingsUtility";
import { IUrlHelperService } from "../../../../../../Core/Services/UrlHelperService";
import { ShiftTypeDTO } from "../../../../../../Common/Models/ShiftTypeDTO";
import { AccountingSettingsRowDTO } from "../../../../../../Common/Models/AccountingSettingsRowDTO";
import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { TimeScheduleTypeSmallDTO } from "../../../../../../Common/Models/TimeScheduleTypeDTO";
import { ITimeCodeBreakSmallDTO, ITimeScheduleTypeSmallDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../../Util/Constants";
import { EditShiftController } from "./EditShiftController";
import { EmployeeListDTO } from "../../../../../../Common/Models/EmployeeListDTO";
import { AccountDimDTO, AccountDimSmallDTO } from "../../../../../../Common/Models/AccountDimDTO";

export class EditShiftHelper {
    // Data
    private hiddenEmployeeId = 0;
    private vacantEmployeeIds: number[] = [];
    private allEmployees: EmployeeListDTO[];
    private validAccountIds: number[] = [];

    // Read only permissions
    private showTotalCostPermission = false;

    // Modify permissions
    private placementPermission = false;
    private hasStaffingByEmployeeAccount = false;
    private standbyModifyPermission = false;
    private onDutyModifyPermission = false;
    private attestPermission = false;

    // Company settings
    private useAccountHierarchy = false;
    private skillCantBeOverridden = false;
    private clockRounding = 0;
    private shiftTypeMandatory = false;
    private allowHolesWithoutBreaks = false;
    private keepShiftsTogether = false;
    private useShiftRequestPreventTooEarly = false
    private sendXEMailOnChange = false;
    private possibleToSkipWorkRules = false;
    private maxNbrOfBreaks = 1;
    private showGrossTimeSetting = false;
    private showExtraShift = false;
    private showSubstitute = false;
    private useMultipleScheduleTypes = false;
    private inactivateLending = false;
    private extraShiftAsDefaultOnHidden = false;

    // User settings
    private accountHierarchyId: string;
    private showAvailability = false;
    private disableBreaksWithinHolesWarning = false;

    // Lookups
    private allShiftTypes: ShiftTypeDTO[];
    private shiftTypes: ShiftTypeDTO[];
    private shiftTypeIds: number[];
    private timeScheduleTypes: ITimeScheduleTypeSmallDTO[];
    private breakTimeCodes: ITimeCodeBreakSmallDTO[];
    private accountDim: AccountDimSmallDTO;
    private accountDims: AccountDimSmallDTO[];
    private shiftTypeAccountDim: AccountDimDTO;

    // Flags
    private showSkills = false;
    private skipXEMailOnChanges = false;
    private skipWorkRules = false;

    // Modal
    private editShiftModal: any;

    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private sharedScheduleService: ISharedScheduleService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private isAdmin: boolean,
        private dateFrom: Date,
        private dateTo: Date,
        private employeeId: number,
        private isReadOnly: boolean,
        private onLoadComplete: Function) {

        $q.all([
            //this.loadReadOnlyPermissions(),
            this.loadModifyPermissions(),
            this.loadCompanySettings(),
            this.loadUserSettings(),
            this.loadUserAndCompanySettings(),
            this.loadEmployeesSmall(),
            this.loadShiftTypes(),
            this.loadUserShiftTypes(),
            this.loadShiftTypeAccountDim(),
            this.loadAccountDims(),
            this.loadTimeScheduleTypes(),
            this.loadBreakTimeCodes()
        ]).then(() => {
            this.loadEmployeeAvailability().then(() => {
                this.setShiftTypes();

                if (this.useAccountHierarchy) {
                    this.loadDefaultEmployeeAccountDimAndSelectableAccounts().then(() => {
                        this.onLoadComplete();
                    });
                } else {
                    this.onLoadComplete();
                }
            })
        });
    }

    // SERVICE CALLS

    //private loadReadOnlyPermissions(): ng.IPromise<any> {
    //    var features: number[] = [];

    //    features.push(Feature.Time_Schedule_SchedulePlanning_ShowCosts);

    //    return this.coreService.hasReadOnlyPermissions(features).then((x) => {
    //        this.showTotalCostPermission = x[Feature.Time_Schedule_SchedulePlanning_ShowCosts];
    //    });
    //}

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];

        features.push(Feature.Time_Schedule_Placement);
        features.push(Feature.Time_Schedule_SchedulePlanning_Placement);
        features.push(Feature.Time_Schedule_SchedulePlanning_StandbyShifts);
        features.push(Feature.Time_Schedule_SchedulePlanning_OnDutyShifts);
        features.push(Feature.Time_Time_Attest);

        return this.coreService.hasReadOnlyPermissions(features).then((x) => {
            this.placementPermission = x[Feature.Time_Schedule_Placement] || x[Feature.Time_Schedule_SchedulePlanning_Placement];
            this.standbyModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_StandbyShifts];
            this.onDutyModifyPermission = x[Feature.Time_Schedule_SchedulePlanning_OnDutyShifts];
            this.attestPermission = x[Feature.Time_Time_Attest];
        });
    }

    private getHasStaffingByEmployeeAccount(date: Date): ng.IPromise<any> {
        return this.sharedScheduleService.getHasStaffingByEmployeeAccount(date).then(result => {
            this.hasStaffingByEmployeeAccount = result;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.TimeUseVacant);
        settingTypes.push(CompanySettingType.TimeSkillCantBeOverridden);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningClockRounding);
        settingTypes.push(CompanySettingType.TimeShiftTypeMandatory);
        settingTypes.push(CompanySettingType.TimeEditShiftAllowHoles);
        settingTypes.push(CompanySettingType.TimeDefaultDoNotKeepShiftsTogether);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarly);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningSendXEMailOnChange);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningSkipWorkRules);
        settingTypes.push(CompanySettingType.TimeMaxNoOfBrakes);
        //settingTypes.push(CompanySettingType.PayrollAgreementUseGrossNetTimeInStaffing);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningSetShiftAsExtra);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningSetShiftAsSubstitute);
        settingTypes.push(CompanySettingType.UseMultipleScheduleTypes);
        settingTypes.push(CompanySettingType.TimeSchedulePlanningInactivateLending);
        settingTypes.push(CompanySettingType.ExtraShiftAsDefaultOnHidden);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.getHiddenEmployeeId();
            if (SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeUseVacant))
                this.getVacantEmployeeIds();
            this.skillCantBeOverridden = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSkillCantBeOverridden);
            this.clockRounding = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningClockRounding);
            this.shiftTypeMandatory = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeShiftTypeMandatory);
            this.allowHolesWithoutBreaks = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeEditShiftAllowHoles);
            this.keepShiftsTogether = !SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeDefaultDoNotKeepShiftsTogether);
            this.useShiftRequestPreventTooEarly = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningShiftRequestPreventTooEarly);
            this.sendXEMailOnChange = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningSendXEMailOnChange);
            this.possibleToSkipWorkRules = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningSkipWorkRules);
            this.maxNbrOfBreaks = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeMaxNoOfBrakes, this.maxNbrOfBreaks);
            //this.showGrossTimeSetting = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PayrollAgreementUseGrossNetTimeInStaffing);
            //if (!this.showGrossTimeSetting)
            //    this.showTotalCostPermission = false;
            this.showExtraShift = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningSetShiftAsExtra);
            this.showSubstitute = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningSetShiftAsSubstitute);
            this.useMultipleScheduleTypes = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseMultipleScheduleTypes);
            this.inactivateLending = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningInactivateLending);
            this.extraShiftAsDefaultOnHidden = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.ExtraShiftAsDefaultOnHidden);
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
            this.disableBreaksWithinHolesWarning = SettingsUtility.getBoolUserSetting(x, UserSettingType.TimeSchedulePlanningDisableBreaksWithinHolesWarning);
        });
    }

    private loadUserAndCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(UserSettingType.AccountHierarchyId);

        return this.coreService.getUserAndCompanySettings(settingTypes).then(x => {
            this.accountHierarchyId = SettingsUtility.getStringUserSetting(x, UserSettingType.AccountHierarchyId, '0');
        });
    }

    private loadAccountsByUserFromHierarchy(dateFrom: Date, dateTo: Date): ng.IPromise<any> {
        return this.coreService.getAccountIdsFromHierarchyByUser(dateFrom, dateTo, false, false, false, true, true, true).then(x => {
            this.validAccountIds = x;
        });
    }

    private loadEmployeesSmall(): ng.IPromise<any> {
        return this.sharedScheduleService.getEmployeesForPlanning([this.employeeId], null, true, true, false, false, false, this.dateFrom, this.dateTo, true, TimeSchedulePlanningDisplayMode.Admin).then(x => {
            this.allEmployees = x.map(e => {
                var obj = new EmployeeListDTO();
                obj.isVisible = true;
                angular.extend(obj, e);
                return obj;
            });
        });
    }

    private loadEmployeeAvailability(): ng.IPromise<any> {
        return this.sharedScheduleService.getEmployeeAvailability(_.map(this.allEmployees, e => e.employeeId)).then(x => {
            _.forEach(x, employee => {
                var existingEmployee = _.find(this.allEmployees, e => e.employeeId === employee.employeeId);
                if (existingEmployee) {
                    existingEmployee.available = employee.available;
                    existingEmployee.unavailable = employee.unavailable;
                }
            });
        });
    }

    private loadShiftTypes(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypes(true, true, false, false, false, true).then(x => {
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
                    this.shiftTypes.push(shiftType);
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

    private loadBreakTimeCodes(): ng.IPromise<any> {
        return this.sharedScheduleService.getTimeCodeBreaks(false).then((x) => {
            this.breakTimeCodes = x;
        });
    }

    private loadDefaultEmployeeAccountDimAndSelectableAccounts(): ng.IPromise<any> {
        return this.sharedScheduleService.getDefaultEmployeeAccountDimAndSelectableAccounts(this.employeeId, this.dateFrom).then(x => {
            this.accountDim = x;
        });
    }

    private loadShiftTypeAccountDim(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypeAccountDim(false).then(x => {
            this.shiftTypeAccountDim = x;
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.sharedScheduleService.getAccountDimsForPlanning(false, true, TimeSchedulePlanningDisplayMode.Admin).then(x => {
            this.accountDims = x;
        });
    }

    // PUBLIC METHODS

    public loadShift(timeScheduleTemplateBlockId: number): ng.IPromise<ShiftDTO> {
        return this.sharedScheduleService.getShift(timeScheduleTemplateBlockId, true);
    }

    public loadLinkedShifts(shift: ShiftDTO): ng.IPromise<ShiftDTO[]> {
        return this.sharedScheduleService.getShiftsForDay(shift.employeeId, shift.actualStartDate, [shift.type], false, false, shift.link, true, false, false, true);
    }

    public openEditShiftDialog(shift: ShiftDTO, date: Date, employeeId: number, loadAllDayShifts: boolean, dayHasDeviations: boolean, onDialogClosed?: Function): ng.IPromise<any> {
        var deferral = this.$q.defer();

        this.getHasStaffingByEmployeeAccount(date).then(() => {
            if (this.useAccountHierarchy && shift) {
                this.loadAccountsByUserFromHierarchy((shift ? shift.actualStartTime : date).beginningOfDay(), (shift ? shift.actualStopTime : date).endOfDay()).then(() => {
                    // Shift account is not valid for current user
                    if (this.validAccountIds.length > 0 && shift.accountId && !this.inactivateLending) {
                        if (!_.includes(this.validAccountIds, shift.accountId))
                            shift.isLended = true;
                    }

                    deferral.resolve(this.doOpenEditShiftDialog(shift, date, employeeId, loadAllDayShifts, dayHasDeviations, onDialogClosed));
                });
            } else {
                deferral.resolve(this.doOpenEditShiftDialog(shift, date, employeeId, loadAllDayShifts, dayHasDeviations, onDialogClosed));
            }
        });

        return deferral.promise;
    }

    private doOpenEditShiftDialog(shift: ShiftDTO, date: Date, employeeId: number, loadAllDayShifts: boolean, dayHasDeviations: boolean, onDialogClosed?: Function) {
        // Show edit shift dialog
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Planning/Dialogs/EditShift/Views/editShift.html"),
            controller: EditShiftController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                isAdmin: () => { return this.isAdmin },
                currentEmployeeId: () => { return this.employeeId },
                templateHelper: () => { return null },
                isScheduleView: () => { return true },
                isTemplateView: () => { return false },
                isEmployeePostView: () => { return false },
                isScenarioView: () => { return false },
                isStandbyView: () => { return false },
                isReadonly: () => { return this.isReadOnly },
                template: () => { return null },
                standby: () => { return shift ? shift.isStandby : false },
                onDuty: () => { return shift ? shift.isOnDuty : false },
                shift: () => { return shift },
                shifts: () => { return loadAllDayShifts || !shift ? [] : [shift] },
                loadTasks: () => { return false },
                date: () => { return shift ? shift.actualStartDate : date },
                employeeId: () => { return shift ? shift.employeeId : employeeId },
                shiftTypes: () => { return this.shiftTypes; },
                shiftTypeAccountDim: () => { return this.shiftTypeAccountDim; },
                timeScheduleTypes: () => { return this.timeScheduleTypes; },
                allBreakTimeCodes: () => { return this.breakTimeCodes; },
                singleEmployeeMode: () => { return true; },
                employees: () => { return this.allEmployees; },
                hiddenEmployeeId: () => { return this.hiddenEmployeeId; },
                vacantEmployeeIds: () => { return this.vacantEmployeeIds; },
                showSkills: () => { return this.showSkills; },
                standbyModifyPermission: () => { return this.standbyModifyPermission; },
                onDutyModifyPermission: () => { return this.onDutyModifyPermission; },
                attestPermission: () => { return this.attestPermission; },
                hasStaffingByEmployeeAccount: () => { return this.hasStaffingByEmployeeAccount; },
                placementPermission: () => { return this.placementPermission; },
                showTotalCost: () => { return this.showTotalCostPermission; },
                showTotalCostIncEmpTaxAndSuppCharge: () => { return this.showTotalCostPermission; },
                showGrossTime: () => { return this.showGrossTimeSetting; },
                showExtraShift: () => { return this.showExtraShift; },
                showSubstitute: () => { return this.showSubstitute; },
                useMultipleScheduleTypes: () => { return this.useMultipleScheduleTypes; },
                showAvailability: () => { return this.showAvailability; },
                maxNbrOfBreaks: () => { return this.maxNbrOfBreaks; },
                clockRounding: () => { return this.clockRounding; },
                useAccountHierarchy: () => { return this.useAccountHierarchy; },
                accountDim: () => { return this.accountDim; },
                accountDims: () => { return this.accountDims; },
                accountHierarchyId: () => { return this.accountHierarchyId; },
                validAccountIds: () => { return this.validAccountIds; },
                showSecondaryAccounts: () => { return false; },
                shiftTypeMandatory: () => { return this.shiftTypeMandatory; },
                keepShiftsTogether: () => { return this.keepShiftsTogether; },
                disableBreaksWithinHolesWarning: () => { return this.disableBreaksWithinHolesWarning; },
                disableSaveAndActivateCheck: () => { return false; },
                autoSaveAndActivate: () => { return false; },
                allowHolesWithoutBreaks: () => { return this.allowHolesWithoutBreaks; },
                skillCantBeOverridden: () => { return this.skillCantBeOverridden; },
                useShiftRequestPreventTooEarly: () => { return this.useShiftRequestPreventTooEarly; },
                skipWorkRules: () => { return this.skipWorkRules; },
                skipXEMailOnChanges: () => { return this.skipXEMailOnChanges; },
                dayHasDeviations: () => { return dayHasDeviations; },
                timeScheduleScenarioHeadId: () => { return 0; },
                scenarioDateFrom: () => { return null; },
                scenarioDateTo: () => { return null; },
                loadedRangeDateFrom: () => { return this.dateFrom; },
                loadedRangeDateTo: () => { return this.dateTo; },
                inactivateLending: () => { return this.inactivateLending; },
                extraShiftAsDefaultOnHidden: () => { return this.extraShiftAsDefaultOnHidden; },
                planningPeriodStartDate: () => { return null; },
                planningPeriodStopDate: () => { return null; },
            }
        }
        this.editShiftModal = this.$uibModal.open(options);

        this.editShiftModal.result.then((result: any) => {
            if (onDialogClosed)
                onDialogClosed(result);
        });

        return this.editShiftModal;
    }
}