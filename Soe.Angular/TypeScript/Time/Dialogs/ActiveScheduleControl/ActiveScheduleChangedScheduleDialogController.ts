import { ActivateScheduleControlHeadDTO } from "../../../Common/Models/EmployeeScheduleDTOs";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { UrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ShiftHistoryController } from "../../../Shared/Time/Schedule/Planning/Dialogs/ShiftHistory/ShiftHistoryController";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class ActiveScheduleChangedScheduleDialogController {
    public progress: IProgressHandler;
    private terms: { [index: string]: string; };
    private headerInfo: string;

    //@ngInject
    constructor(private $uibModalInstance,
        private $uibModal,
        private $q: ng.IQService,
        private urlHelperService: UrlHelperService,
        private scheduleChanges: ActivateScheduleControlHeadDTO) {

        this.setHeader();

    }

    private setHeader() {
        this.headerInfo = this.scheduleChanges.employeeNrAndName + " " + CalendarUtility.toFormattedDate(this.scheduleChanges.startDate) + " - " + CalendarUtility.toFormattedDate(this.scheduleChanges.stopDate);
    }

    private viewHistory(timeScheduleTemplateBlockId: number, timeScheduleTemplateBlockType: number) {
        
        // Show shifthistory dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Time/Schedule/Planning/Dialogs/ShiftHistory/Views/shiftHistory.html"),
            controller: ShiftHistoryController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            resolve: {
                shiftType: () => { return timeScheduleTemplateBlockType },
                timeScheduleTemplateBlockId: () => { return timeScheduleTemplateBlockId }
            }
        }
        this.$uibModal.open(options);
    }

    private ok() {
        this.$uibModalInstance.dismiss('cancel');
    }

}