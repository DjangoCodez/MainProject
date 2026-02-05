import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EmploymentVacationGroupDTO } from "../../../../../Common/Models/EmployeeUserDTO";
import { PayrollGroupVacationGroupDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { EmploymentVacationGroupDialogController } from "./EmploymentVacationGroupDialogController";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";

export class EmploymentVacationGroupsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/EmploymentVacationGroups/Views/EmploymentVacationGroups.html'),
            scope: {
                vacationGroups: '=',
                payrollGroupVacationGroups: '=',
                selectedVacationGroupId: '=?',
                selectedEmploymentDate: '=',
                readOnly: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: EmploymentVacationGroupsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmploymentVacationGroupsController {
    // Data
    private vacationGroups: EmploymentVacationGroupDTO[];
    private selectedVacationGroup: EmploymentVacationGroupDTO;
    private selectedVacationGroupId: number;
    private payrollGroupVacationGroups: PayrollGroupVacationGroupDTO[];
    private selectedEmploymentDate: Date;

    // Flags
    private readOnly: boolean;

    // Events
    private onChange: Function;

    //@ngInject
    constructor(
        private $uibModal,
        private $scope: ng.IScope,
        private urlHelperService: IUrlHelperService) {

        this.setupWatchers();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.vacationGroups, (newVal, oldVal) => {
            this.selectedVacationGroup = this.vacationGroups && this.vacationGroups.length > 0 ? _.orderBy(_.filter(this.vacationGroups, v => (!v.fromDate || v.fromDate.isSameOrBeforeOnDay(this.selectedEmploymentDate ?? CalendarUtility.getDateToday()))), 'sortableDate', 'desc')[0] : null;
            this.selectedVacationGroupId = this.selectedVacationGroup ? this.selectedVacationGroup.vacationGroupId : 0;
        });
    }

    // EVENTS

    private editGroup(group: EmploymentVacationGroupDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/EmploymentVacationGroups/Views/EmploymentVacationGroupDialog.html"),
            controller: EmploymentVacationGroupDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                vacationGroups: () => { return this.payrollGroupVacationGroups },
                group: () => { return group },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.group) {
                if (!group) {
                    // Add new group to the original collection
                    group = new EmploymentVacationGroupDTO();
                    group.fromDate = result.group.fromDate;
                    group.vacationGroupId = result.group.vacationGroupId;
                    group.name = this.getVacationGroupName(group.vacationGroupId);
                    if (!this.vacationGroups)
                        this.vacationGroups = [];
                    this.vacationGroups.push(group);
                } else {
                    // Update original group
                    var originalGroup = _.find(this.vacationGroups, v => v.employmentVacationGroupId === group.employmentVacationGroupId);
                    if (originalGroup) {
                        originalGroup.fromDate = result.group.fromDate;
                        originalGroup.vacationGroupId = result.group.vacationGroupId;
                        originalGroup.name = this.getVacationGroupName(originalGroup.vacationGroupId);
                    }
                }

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private deleteGroup(group: EmploymentVacationGroupDTO) {
        _.pull(this.vacationGroups, group);

        if (this.onChange)
            this.onChange();
    }

    // HELP-METHODS

    private getVacationGroupName(vacationGroupId): string {
        var group = _.find(this.payrollGroupVacationGroups, g => g.vacationGroupId === vacationGroupId);
        return group ? group.name : '';
    }
}