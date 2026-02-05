import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { AttestEmployeeDayDTO } from "../../../Common/Models/TimeEmployeeTreeDTO";
import { Constants } from "../../../Util/Constants";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { PayrollImportEmployeeTransactionDTO } from "../../../Common/Models/PayrollImport";

export class RowDetailDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Directives/AttestEmployee/Views/RowDetail.html'),
            scope: {
                isModal: '=?',
                showMultipleDays: '=?',
                data: '=',
                rowNodeId: '=',
                resizeCallback: '=?',
                showTimeStamp: '=?',
                showProjectTimeBlock: "=?",
                isDirty: '=?',
                
            },
            restrict: 'E',
            replace: true,
            controller: RowDetailController,
            controllerAs: 'ctrl',
            bindToController: true,
        };
    }
}

export class RowDetailController {

    // Init parameters
    private isModal: boolean;
    private showMultipleDays: boolean;
    private data: AttestEmployeeDayDTO;
    private rowNodeId: string;
    private resizeCallback: (height: number, rowId: string) => void;
    private showTimeStamp: boolean;
    private showProjectTimeBlock: boolean;
    private isDirty: boolean;
    private timeStampIsModified: boolean;
    private timeBlockIsModified: boolean;
    private contentEmployeeTimeCodeTransactionIdSelected: number = 1;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private messagingService: IMessagingService) { }

    // EVENTS

    public $onInit() {
        this.showTimeStamp = false;
        this.showProjectTimeBlock = false;

        this.messagingService.subscribe(Constants.EVENT_ATTESTEMPLOYEE_TIMECODETRANSACTION_SELECTED, (timeCodeTransactionId: number) => {
            this.contentEmployeeTimeCodeTransactionIdSelected = timeCodeTransactionId;
        }, this.$scope);
    }

    private timeStampSelected(id: number) {
        this.$scope.$broadcast('TimeStampSelected', id);
    }

    private timeStampChanged() {
        this.$scope.$broadcast('TimeStampChanged', this.data.date);
        this.isDirty = true;
        this.timeStampIsModified = true;
    }

    private selectedTimeStampChanged(timeStampEntry) {
        this.$scope.$broadcast('SelectedTimeStampChanged', timeStampEntry);
    }

    private createTimeBlockFromPayrollImport(trans: PayrollImportEmployeeTransactionDTO) {
        this.$scope.$broadcast('CreateTimeBlockFromPayrollImport', trans);
    }

    // HELP-METODS
    private setHeight() {
        let scrollHeight = 0;
        const elems = document.getElementsByClassName('timeattest-rowdetail-container');
        for (let i = 0; i < elems.length; i++) {
            const elem = elems[i];
            for (let y = 0; y < elems.length; y++) {
                if (elem.getAttribute("grid-node-Id") === this.rowNodeId) {
                    scrollHeight = elem.scrollHeight;
                    break;
                }
            }
        }

        if (this.resizeCallback && scrollHeight)
            this.resizeCallback(scrollHeight + 20, this.rowNodeId);
    }
}

