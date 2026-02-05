import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EmployeeCalculatedCostDTO } from "../../../../../Common/Models/EmployeeCalculatedCostDTO";
import { EmployeeCalculatedDialogController } from "./EmployeeCalculatedCostDialogController";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";

export class EmployeeCalculatedCostsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/EmployeeCalculatedCosts/Views/EmployeeCalculatedCosts.html'),
            scope: {
                employeeId: '=',
                calculatedCosts: '=',
                readOnly: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: EmployeeCalculatedCostsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmployeeCalculatedCostsController {
    // Flags
    private readOnly: boolean;
    private selectedVacationGroup: EmployeeCalculatedCostDTO;
    private calculatedCosts: EmployeeCalculatedCostDTO[];
    // Events
    private onChange: Function;

    get activeCalculatedCosts(): EmployeeCalculatedCostDTO[] {
        return _.filter(this.calculatedCosts, r => !r.isDeleted);
    }

    //@ngInject
    constructor(
        private $uibModal,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        private translationService: ITranslationService) {
    }

    private editGroup(group: EmployeeCalculatedCostDTO) {
        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/EmployeeCalculatedCosts/Views/EmployeeCalculatedCostDialog.html"),
            controller: EmployeeCalculatedDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                group: () => { return group },
            }
        });

        modal.result.then(result => {
            if (result && result.group) {
                group = result.group;
                if (this.calculatedCosts.filter(x => ( (x.employeeCalculatedCostId !== group.employeeCalculatedCostId) || (group.employeeCalculatedCostId === 0 && x.employeeCalculatedCostId === 0) ) && x.fromDate.getTime() === group.fromDate.getTime() && !x.isDeleted ).length > 0) {
                    this.translationService.translate("time.employee.employee.calculatedcostexistfordate").then(term => {
                        this.notificationService.showDialogEx("", term + " " + CalendarUtility.toFormattedDate(group.fromDate), SOEMessageBoxImage.Error);
                    });
                    return;
                }

                if (group.employeeCalculatedCostId) {
                    const existing = this.calculatedCosts.filter(x => x.employeeCalculatedCostId === group.employeeCalculatedCostId);
                    if (existing && existing.length > 0) {
                        const existingGroup: EmployeeCalculatedCostDTO = existing[0];
                        existingGroup.fromDate = group.fromDate;
                        existingGroup.calculatedCostPerHour = group.calculatedCostPerHour;
                        existingGroup.isModified = true;
                    }
                }
                else {
                    this.calculatedCosts.push(group);
                }

                if (this.onChange)
                    this.onChange();
            }
        });
    }

    private deleteGroup(group: EmployeeCalculatedCostDTO) {
        group.isDeleted = true;

        if (this.onChange)
            this.onChange();
    }
}