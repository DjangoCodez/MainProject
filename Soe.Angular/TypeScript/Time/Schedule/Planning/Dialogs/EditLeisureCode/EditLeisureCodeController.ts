import { IScheduleService as ISharedScheduleService } from "../../../../../Shared/Time/Schedule/ScheduleService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ShiftDTO, TimeLeisureCodeSmallDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { SoeScheduleWorkRules, TermGroup_ShiftHistoryType } from "../../../../../Util/CommonEnumerations";

export class EditLeisureCodeController {
    // Flags
    private executing: boolean = false;

    // Data
    private readonly shift: ShiftDTO;

    //@ngInject
    constructor(private readonly $timeout: ng.ITimeoutService,
        private readonly $uibModalInstance,
        private readonly sharedScheduleService: ISharedScheduleService,
        private readonly translationService: ITranslationService,
        private readonly notificationService: INotificationService,
        private readonly $q: ng.IQService,
        private readonly isReadonly: boolean,
        private readonly modifyPermission: boolean,
        shift: ShiftDTO,
        private readonly leisureCodes: TimeLeisureCodeSmallDTO[],
        private readonly skipWorkRules: boolean
    ) {
        if (!this.modifyPermission)
            this.isReadonly = true;

        this.shift = new ShiftDTO();
        angular.extend(this.shift, shift);
    }

    // EVENTS

    private cancel() {
        //this.$uibModalInstance.close({ reload: true });
        // No need to reload the schedule at this point.
        // It might be in the future if we add support for drag and drop or other things that affect the UI.
        this.$uibModalInstance.dismiss('cancel');
    }

    private save() {
        //this.executing = true;

        this.$uibModalInstance.close({ save: true, shift: this.shift });
    //    this.initSave().then(val => {
    //        if (val) {
    //            this.$uibModalInstance.close({ save: true, shift: this.shift });
    //        } else {
    //            this.executing = false;
    //        }
    //    });
    }

    private initSave(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        // Validate work rules
        this.validateWorkRules().then(passedWorkRules => {
            deferral.resolve(passedWorkRules);
        });

        return deferral.promise;
    }

    private validateWorkRules(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        let rules: SoeScheduleWorkRules[] = null;
        if (this.skipWorkRules) {
            // The following rules should always be evaluated
            rules = [];
            rules.push(SoeScheduleWorkRules.OverlappingShifts);
        }

        this.sharedScheduleService.evaluatePlannedShiftsAgainstWorkRules([this.shift], rules, this.shift.employeeId, false, null).then(result => {
            this.notificationService.showValidateWorkRulesResult(TermGroup_ShiftHistoryType.Unknown, result, this.shift.employeeId).then(passed => {
                deferral.resolve(passed);
            });
        });

        return deferral.promise;
    }
}