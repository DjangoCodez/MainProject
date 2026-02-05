import { IUrlHelperService } from "../../../../../../Core/Services/UrlHelperService";
import { IScheduleService as ISharedScheduleService } from "../../../ScheduleService";
import { HandleShiftController } from "./HandleShiftController";
import { ShiftDTO } from "../../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { ShiftTypeDTO } from "../../../../../../Common/Models/ShiftTypeDTO";
import { CompanySettingType } from "../../../../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../../../../Util/SettingsUtility";
import { ICoreService } from "../../../../../../Core/Services/CoreService";

export class HandleShiftHelper {

    // Company settings
    private useAccountHierarchy: boolean = false;

    // Lookups
    private allShiftTypes: ShiftTypeDTO[];
    private shiftTypes: any[];
    private shiftTypeIds: number[];

    // Flags
    private showSkills: boolean = false;

    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private sharedScheduleService: ISharedScheduleService,
        private urlHelperService: IUrlHelperService,
        private isSchedulePlanningMode: boolean,
        private isOrderPlanningMode: boolean,
        private employeeId: number,
        private employeeGroupId: number,
        private onLoadComplete: Function) {

        $q.all([
            this.loadCompanySettings(),
            this.loadShiftTypes(),
            this.loadUserShiftTypes()
        ]).then(() => {
            this.setShiftTypes();
            this.onLoadComplete();
        })
    }

    // SERVICE CALLS

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadShiftTypes(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypes(false, true, false, false, false, false).then(x => {
            this.allShiftTypes = x;
        });
    }

    private loadUserShiftTypes(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypeIdsForUser(this.employeeId, false, false).then(x => {
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

    // PUBLIC METHODS

    public loadShift(timeScheduleTemplateBlockId: number): ng.IPromise<ShiftDTO> {
        return this.sharedScheduleService.getShift(timeScheduleTemplateBlockId, true);
    }

    public loadLinkedShifts(shift: ShiftDTO): ng.IPromise<ShiftDTO[]> {
        var deferral = this.$q.defer<ShiftDTO[]>();

        if (!shift)
            deferral.resolve([]);
        else
            this.sharedScheduleService.getShiftsForDay(shift.employeeId, shift.actualStartDate, [shift.type], true, false, shift.link, true, false, false, false).then(x => {
                deferral.resolve(x);
            });

        return deferral.promise;
    }

    public openHandleShiftDialog(shifts: ShiftDTO[], onDialogClosed: Function) {
        if (!shifts || shifts.length === 0)
            return;

        // Show handle shift dialog
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Planning/Dialogs/HandleShift/Views/handleShift.html"),
            controller: HandleShiftController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            windowClass: 'fullsize-modal',
            resolve: {
                isSchedulePlanningMode: () => { return this.isSchedulePlanningMode },
                isOrderPlanningMode: () => { return this.isOrderPlanningMode },
                shifts: () => { return shifts },
                employeeId: () => { return this.employeeId },
                employeeGroupId: () => { return this.employeeGroupId },
                showSkills: () => { return this.showSkills },
                useAccountHierarchy: () => { return this.useAccountHierarchy }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            onDialogClosed(result);
        });
    }
}