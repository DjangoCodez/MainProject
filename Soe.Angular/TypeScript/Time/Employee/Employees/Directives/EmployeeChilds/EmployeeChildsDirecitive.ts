import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { EmployeeChildDTO } from "../../../../../Common/Models/EmployeeChildDTOs";
import { EmployeeChildDialogController } from "./EmployeeChildDialogController";
import { Constants } from "../../../../../Util/Constants";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { SoeEntityState } from "../../../../../Util/CommonEnumerations";

export class EmployeeChildsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/Employees/Directives/EmployeeChilds/Views/EmployeeChilds.html'),
            scope: {
                employeeChilds: '=',
                childCares: '=',
                readOnly: '=',
                openingBalancePermission: '='
            },
            restrict: 'E',
            replace: true,
            controller: EmployeeChildsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class EmployeeChildsController {

    // Data
    private employeeChilds: EmployeeChildDTO[];
    private selectedChild: EmployeeChildDTO;

    // Flags
    private readOnly: boolean;
    private openingBalancePermission: boolean;

    //@ngInject
    constructor(
        private $uibModal,
        private urlHelperService: IUrlHelperService,
        private messagingService: IMessagingService) {
    }

    // EVENTS

    private editChild(child: EmployeeChildDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Directives/EmployeeChilds/Views/EmployeeChildDialog.html"),
            controller: EmployeeChildDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                child: () => { return child },
                openingBalancePermission: () => { return this.openingBalancePermission; }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.child) {
                if (!child) {
                    // Add new child
                    child = new EmployeeChildDTO();
                    child.firstName = result.child.firstName;
                    child.lastName = result.child.lastName;
                    child.birthDate = result.child.birthDate;
                    child.singleCustody = result.child.singleCustody;
                    child.openingBalanceUsedDays = result.child.openingBalanceUsedDays;
                    child.usedDays = result.child.usedDays;
                    child.state = SoeEntityState.Active;
                    if (!this.employeeChilds)
                        this.employeeChilds = [];
                    this.employeeChilds.push(child);
                } else {
                    // Update child
                    var originalChild = this.getOriginalChild();
                    if (originalChild) {
                        originalChild.firstName = result.child.firstName;
                        originalChild.lastName = result.child.lastName;
                        originalChild.birthDate = result.child.birthDate;
                        originalChild.singleCustody = result.child.singleCustody;
                        originalChild.openingBalanceUsedDays = result.child.openingBalanceUsedDays;
                        originalChild.usedDays = result.child.usedDays;
                    }
                }
                this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
            }
        });
    }

    private deleteChild(child: EmployeeChildDTO) {
        child.state = SoeEntityState.Deleted;
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
    }

    // HELP-METHODS

    private getOriginalChild(): EmployeeChildDTO {
        // Get selected child type from originally bound collection
        return _.find(this.employeeChilds, c => c.employeeChildId === this.selectedChild.employeeChildId);
    }
}