import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IScheduleService as ISharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { IAvailableEmployeesDTO } from "../../../Scripts/TypeLite.Net4";
import { HtmlUtility } from "../../../Util/HtmlUtility";

export class AvailableEmployeesDirectiveFactory {

    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Common/Directives/AvailableEmployees/Views/AvailableEmployees.html'),
            scope: {
                employeeIds: '=',
                shiftIds: '=',
                showAvailability: '=',
                showMessageGroups: '=',
                messageGroups: '=',
                editMessageGroupPermission: '=',
                autoFilter: '=',
                isFiltered: '=',
                filtering: '=',
                filteringDone: '&'
            },
            restrict: 'E',
            replace: true,
            controller: AvailableEmployeesController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class AvailableEmployeesController {
    // Init parameters
    private employeeIds: number[];
    private shiftIds: number[];
    private showAvailability: boolean;
    private showMessageGroups: boolean;
    private messageGroups: any[];
    private editMessageGroupPermission: boolean;
    private autoFilter: boolean;
    private set isFiltered(value: boolean) { /* Not actually a setter, just to make binding work */ }
    private get isFiltered(): boolean {
        return this.filterOnShiftType || (this.showAvailability && this.filterOnAvailability) || (this.showMessageGroups && !!this.filterOnMessageGroupId) || this.filterOnSkills || this.filterOnWorkRules;
    }
    private filtering: boolean;
    private filteringDone: (employees: any) => void;

    // Filter properties
    private filterOnShiftType: boolean = true;
    private filterOnAvailability: boolean = true;
    private filterOnSkills: boolean = true;
    private filterOnWorkRules: boolean = true;
    private filterOnMessageGroupId: number = undefined;

    //@ngInject
    constructor(
        private sharedScheduleService: ISharedScheduleService,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $window: ng.IWindowService) {

        this.$scope.$on('getAvailableEmployees', (e, a) => {
            this.filter();
        });
        this.$scope.$on('clearEmployeeFilters', (e, a) => {
            this.clearFilters();
        });

        this.$scope.$watchCollection(() => this.shiftIds, (newVal, oldVal) => {
            if (this.autoFilter)
                this.filter();
        });
    }

    // ACTIONS

    private filter() {
        this.filtering = true;
        this.getAvailableEmployees().then((employees: IAvailableEmployeesDTO[]) => {
            this.filtering = false;
            if (this.filteringDone)
                this.filteringDone({ employees: employees });
        });
    }

    private clearFilters() {
        this.filterOnShiftType = false;
        this.filterOnAvailability = false;
        this.filterOnSkills = false;
        this.filterOnWorkRules = false;
        this.filterOnMessageGroupId = 0;
        this.filter();
    }

    private getAvailableEmployees(): ng.IPromise<IAvailableEmployeesDTO[]> {
        var deferral = this.$q.defer<IAvailableEmployeesDTO[]>();

        if (this.shiftIds.length === 0) {
            // No shift selected, return all employees
            deferral.resolve(null);
        } else {
            this.sharedScheduleService.getAvailableEmployees(this.shiftIds, this.employeeIds, this.filterOnShiftType, this.showAvailability && this.filterOnAvailability, this.filterOnSkills, this.filterOnWorkRules, this.filterOnMessageGroupId).then(e => {
                deferral.resolve(e);
            });
        }

        return deferral.promise;
    }

    private openMessageGroups() {
        HtmlUtility.openInNewTab(this.$window, "/soe/manage/preferences/registry/eventreceivergroup");
    }
}