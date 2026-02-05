import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { EmbeddedGridController } from "../../../../../Core/Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { EmployeeListDTO } from "../../../../../Common/Models/EmployeeListDTO";
import { IScheduleService } from "../../../ScheduleService";
import { AutomaticAllocationEmployeeResultDTO, AutomaticAllocationResultDTO } from "../../../../../Common/Models/TimeSchedulePlanningDTOs";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { LeisureCodeAllocationEmployeeStatus } from "../../../../../Util/CommonEnumerations";
import { IActionResult } from "../../../../../Scripts/TypeLite.Net4";

export class AllocateLeisureCodesController {

    // Terms
    private terms: { [index: string]: string; };
    private infoText: string;

    // Lookups
    private progress: IProgressHandler;

    // Flags
    private isValid = false;
    private executing = false;
    private hasExecuted = false;
    private evaluateWorkRules = true;

    // Properties
    private selectedEmployeeIds: number[] = [];

    // Grid
    private gridHandler: EmbeddedGridController;

    private result: AutomaticAllocationResultDTO;
    private reloadEmployeeIds: number[] = [];

    //@ngInject
    constructor(private $uibModalInstance,
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private scheduleService: IScheduleService,
        progressHandlerFactory: IProgressHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private sourceEmployees: EmployeeListDTO[],
        private dateFrom: Date,
        private dateTo: Date,
        private deleteMode: boolean) {

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();

        this.gridHandler = new EmbeddedGridController(gridHandlerFactory, "AllocateLeisureCodes.Employees");
        this.gridHandler.gridAg.options.enableGridMenu = false;
        this.gridHandler.gridAg.options.setMinRowsToShow(15);

        // Clear any previous status
        this.sourceEmployees.forEach(e => {
            e['allocateLeisureCodesStatusIcon'] = undefined;
            e['allocateLeisureCodesStatus'] = undefined;
        });

        this.progress.startLoadingProgress([() => {
            return this.loadTerms().then(() => {
                this.setupGrid();
            });
        }]);
    }

    private setupGrid() {
        this.gridHandler.gridAg.addColumnText("employeeNr", this.terms["time.employee.employeenumber"], 100);
        this.gridHandler.gridAg.addColumnText("name", this.terms["common.name"], null);
        this.gridHandler.gridAg.addColumnIcon("allocateLeisureCodesStatusIcon", this.terms["common.status"], 75, { showIcon: this.showStatusIcon.bind(this), onClick: this.showStatus.bind(this) });
        this.gridHandler.gridAg.options.setStandardSubscriptions((rows: any[]) => this.onEmployeeGridRowSelected(rows));
        this.gridHandler.gridAg.finalizeInitGrid(null, true, "employee-totals-grid");
        this.gridHandler.gridAg.setData(this.sourceEmployees);
    }

    private showStatusIcon(employee: EmployeeListDTO): boolean {
        return employee['allocateLeisureCodesStatusIcon'];
    }

    private showStatus(employee: EmployeeListDTO) {
        const eResult: AutomaticAllocationEmployeeResultDTO = employee['allocateLeisureCodesStatus'];
        if (eResult.status === LeisureCodeAllocationEmployeeStatus.AllSuccess)
            return;

        let message = eResult.message;
        if (eResult && eResult.dayResults.length > 0) {
            eResult.dayResults.filter(d => !d.success).forEach(d => {
                if (message)
                    message += '\n';
                message += `${d.date.toFormattedDate()}: ${d.message}`
            });
        }

        if (message)
            this.notificationService.showDialogEx(this.terms["common.status"], message, SOEMessageBoxImage.Information);
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.name",
            "common.status",
            "time.employee.employeenumber",
            "time.schedule.planning.allocateleisurecodes.info",
            "time.schedule.planning.deleteleisurecodes.info"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.infoText = this.deleteMode ? terms["time.schedule.planning.deleteleisurecodes.info"] : terms["time.schedule.planning.allocateleisurecodes.info"];
        });
    }

    private setResult() {
        if (this.result && this.result.employeeResults.length > 0) {
            this.result.employeeResults.forEach((eResult: AutomaticAllocationEmployeeResultDTO) => {
                const sourceEmployee = this.sourceEmployees.find(e => e.employeeId === eResult.employeeId);
                if (sourceEmployee) {
                    let icon = 'fal ';
                    switch (eResult.status) {
                        case LeisureCodeAllocationEmployeeStatus.AllSuccess:
                            icon += 'fa-check-circle okColor';
                            break;
                        case LeisureCodeAllocationEmployeeStatus.AllFailed:
                            icon += 'fa-exclamation-triangle errorColor';
                            break;
                        case LeisureCodeAllocationEmployeeStatus.PartialSuccess:
                            icon += 'fa-exclamation-circle warningColor';
                            break;
                        case LeisureCodeAllocationEmployeeStatus.ProcessedWithInformation:
                            icon += 'fa-info-circle infoColor';
                            break;
                    }

                    sourceEmployee['allocateLeisureCodesStatusIcon'] = icon;
                    sourceEmployee['allocateLeisureCodesStatus'] = eResult;

                    if (eResult.status !== LeisureCodeAllocationEmployeeStatus.AllFailed)
                        this.reloadEmployeeIds.push(eResult.employeeId);
                }
            });
            this.gridHandler.gridAg.setData(this.sourceEmployees);
        }
    }

    // EVENTS

    private onEmployeeGridRowSelected(rows: any[]) {
        this.$scope.$applyAsync();
        this.selectedEmployeeIds = this.gridHandler.gridAg.options.getSelectedIds('employeeId');
        this.isValid = this.selectedEmployeeIds.length > 0;
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    private close() {
        this.$uibModalInstance.close({ reloadEmployeeIds: this.reloadEmployeeIds, evaluateWorkRules: this.evaluateWorkRules });
    }

    private create() {
        this.executing = true;
        this.progress.startWorkProgress((completion) => {
            return this.scheduleService.allocateLeisureDays(this.dateFrom.beginningOfDay(), this.dateTo.beginningOfDay(), this.selectedEmployeeIds).then((result: AutomaticAllocationResultDTO) => {
                this.result = result;
                this.executing = false;
                this.hasExecuted = true;
                this.setResult();
                completion.completed(null, true);
            });
        });
    }

    private delete() {
        this.executing = true;
        this.progress.startWorkProgress((completion) => {
            return this.scheduleService.deleteLeisureDays(this.dateFrom.beginningOfDay(), this.dateTo.beginningOfDay(), this.selectedEmployeeIds).then((result: IActionResult) => {
                this.executing = false;
                this.hasExecuted = true;
                if (result.success) {
                    this.evaluateWorkRules = false;
                    this.reloadEmployeeIds = this.selectedEmployeeIds;
                    this.close();
                    completion.completed(null, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            });
        });
    }
}
