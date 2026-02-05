import { Constants } from "../../../../../Util/Constants";
import { SelectionCollection } from "../../../../../Core/RightMenu/ReportMenu/SelectionCollection";
import { IEmployeeSelectionDTO, IIdListSelectionDTO, ISelectableTimePeriodDTO } from "../../../../../Scripts/TypeLite.Net4";
import { EmployeeSelectionDTO, SelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { IReportDataService } from "../../../../../Core/RightMenu/ReportMenu/ReportDataService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";

export class AddRowsDialogController {

    private selections: SelectionCollection;
    private userSelectionInput: EmployeeSelectionDTO;
    private timePeriodIds: number[];
    private paymentDate: Date;
    private duplicatePaymentDates: boolean = false;

    private get selectedEmployeeIds(): number[] {
        let selections: SelectionDTO[] = this.selections.materialize();
        let empSelection: EmployeeSelectionDTO = <EmployeeSelectionDTO>_.find(selections, s => s.key === 'employees');
        if (empSelection)
            return empSelection.employeeIds;

        return [];
    }

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private reportDataService: IReportDataService,
        private allTimePeriods: ISelectableTimePeriodDTO[],
        private existingPaymentDates: Date[]) {
    }

    $onInit() {
        this.selections = new SelectionCollection();
    }

    private onPeriodSelectionUpdated(selection: IIdListSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PERIODS, selection);
        this.timePeriodIds = selection.ids;
        this.paymentDate = null;
        this.duplicatePaymentDates = false;

        _.forEach(this.timePeriodIds, timePeriodId => {
            let timePeriod = _.find(this.allTimePeriods, t => t.id === timePeriodId);
            if (timePeriod) {
                if (!this.paymentDate) {
                    this.paymentDate = timePeriod.paymentDate;
                } else if (!this.paymentDate.isSameDayAs(timePeriod.paymentDate)) {
                    this.duplicatePaymentDates = true;
                }
            }
        });
    }

    private onEmployeeSelectionUpdated(selection: IEmployeeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_EMPLOYEES, selection);
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private initOk() {
        if (this.existingPaymentDates.length > 0 && !CalendarUtility.includesDate(this.existingPaymentDates, this.paymentDate)) {
            let keys: string[] = [
                "core.warning",
                "time.payroll.massregistration.addrows.adddifferentpaymentdate"
            ];

            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialogEx(terms["core.warning"], terms["time.payroll.massregistration.addrows.adddifferentpaymentdate"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val) {
                        this.ok();
                    };
                });
            });
        } else {
            this.ok();
        }
    }

    private ok() {
        this.$uibModalInstance.close({ paymentDate: this.paymentDate, employeeIds: this.selectedEmployeeIds });
    }
}
