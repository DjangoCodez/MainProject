import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { PayrollGroupVacationGroupDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { PayrollGroupVacationGroupsDialogController } from "./PayrollGroupVacationGroupsDialogController";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";

export class PayrollGroupVacationGroupsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/PayrollGroups/Directives/PayrollGroupVacationGroups/Views/PayrollGroupVacationGroups.html'),
            scope: {
                payrollGroupId: '=',
                vacationGroups: '=',                
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: PayrollGroupVacationGroupsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class PayrollGroupVacationGroupsController {

    // Init parameters
    private payrollGroupId: number;
    private vacationGroups: PayrollGroupVacationGroupDTO[] = [];

    // Data
    private payrollVacationGroups: ISmallGenericType[] = [];
    

    // Events
    private onChange: Function;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private urlHelperService: IUrlHelperService,
        private payrollService: IPayrollService) {

        this.$q.all([
            this.loadVacationGroups()
        ]).then(() => {
        });
    }

    // SERVICE CALLS

    private loadVacationGroups(): ng.IPromise<any> {
        this.payrollVacationGroups = [];

        return this.payrollService.getVacationGroupsDict(false).then(x => {
            this.payrollVacationGroups = x;
        });
    }

    // EVENTS

    private editVacationGroup(vacationGroup: PayrollGroupVacationGroupDTO) {
        
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/PayrollGroups/Directives/PayrollGroupVacationGroups/Views/PayrollGroupVacationGroupsDialog.html"),
            controller: PayrollGroupVacationGroupsDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                vacationGroup: () => { return vacationGroup },
                payrollVacationGroups: () => { return this.getAvailablePayrollVacationGroups(vacationGroup) },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.vacationGroup) {
                if (!vacationGroup) {
                    // Add new formula to the original collection
                    vacationGroup = new PayrollGroupVacationGroupDTO();
                    this.vacationGroups.push(vacationGroup);
                }

                vacationGroup.vacationGroupId = result.vacationGroup.vacationGroupId;
                vacationGroup.isDefault = result.vacationGroup.isDefault;            
                if (vacationGroup.isDefault) {
                    _.forEach(this.vacationGroups, (group) => {
                        if (group.vacationGroupId !== vacationGroup.vacationGroupId)
                            group.isDefault = false;
                    });     
                }

                // Set name
                let payrollVacationGroup = _.find(this.payrollVacationGroups, p => p.id === vacationGroup.vacationGroupId);
                if (payrollVacationGroup) {
                    vacationGroup.name = payrollVacationGroup.name;                    
                }

                this.setAsDirty();
            }
        });
    }

    private deleteVacationGroup(vacationGroup: PayrollGroupVacationGroupDTO) {
        _.pull(this.vacationGroups, vacationGroup);

        this.setAsDirty();
    }

    // HELP-METHODS

    private getAvailablePayrollVacationGroups(vacationGroup: PayrollGroupVacationGroupDTO): ISmallGenericType[] {
        let groups = _.filter(this.payrollVacationGroups, pvg => !_.includes(_.map(this.vacationGroups, vg => vg.vacationGroupId), pvg.id));
        if (vacationGroup)
            groups.push({ id: vacationGroup.vacationGroupId, name: vacationGroup.name });

        return groups;
    }

    private setAsDirty() {
        if (this.onChange)
            this.onChange();
    }
}