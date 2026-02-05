import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../../Core/Services/NotificationService";
import { ISoeGridOptions, SoeGridOptions } from "../../../../../../Util/SoeGridOptions";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../../Util/Enumerations";
import { TimeAttestCalculationFunctionDTO, AttestEmployeesDaySmallDTO } from "../../../../../../Common/Models/TimeEmployeeTreeDTO";
import { IAttestFunctionOptionDescription } from "../../../../../../Scripts/TypeLite.Net4";
import { ActionResultSave, SoeTimeAttestFunctionOption} from "../../../../../../Util/CommonEnumerations";
import { ITimeService } from "../../../../../../Time/Time/TimeService";

export class TimeAttestCalculationController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private soeGridOptions: ISoeGridOptions;
    private generatedEmployeeIds: number[] = [];    
    private description: IAttestFunctionOptionDescription;

    // Flags
    private executing: boolean = false;

    //@ngInject
    constructor(private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,        
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private timeService: ITimeService,
        private messagingService: IMessagingService,
        private uiGridConstants: uiGrid.IUiGridConstants,                
        private employees: TimeAttestCalculationFunctionDTO[],
        private setAsSelected: boolean,
        private dateFrom: Date,
        private dateTo: Date,
        private calculationFunction: SoeTimeAttestFunctionOption,
        private calculationText: string,
        private timeScheduleScenarioHeadId: number) {
        
        this.$q.all([
            this.loadDescription(),
            this.loadTerms()
        ]).then(() => {
            this.setupGrid();
        });
    }

    private $onInit() {
        this.soeGridOptions = new SoeGridOptions("", this.$timeout, this.uiGridConstants);
        this.soeGridOptions.enableFiltering = true;
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.showGridFooter = true;
        this.soeGridOptions.enableFullRowSelection = false;
        this.soeGridOptions.enableRowHeaderSelection = true;
        this.soeGridOptions.setMinRowsToShow(12);        
    }

    private setupGrid() {
        this.soeGridOptions.addColumnText("numberAndName", this.terms["common.employee"], "170");
        this.soeGridOptions.addColumnText("status", this.terms["common.status"], null, false, "status");
        this.soeGridOptions.addColumnIcon("statusIcon", null, null, "showStatusDialog");

        this.soeGridOptions.setData(this.employees);
        if (this.setAsSelected)
            this.$timeout(() => this.soeGridOptions.selectAllRows());
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [            
            "common.employee",
            "common.status",            
            "time.time.attest.caculationfunctions.done",
            "time.time.attest.calculationfunctions.pending",      
            "time.time.attest.caculationfunctions.error",
            "time.time.attest.caculationfunctions.aborted",
            "time.time.attest.calculationfunctions.ongoing",
            "core.verifyquestion",
            "time.time.attest.caculationfunctions.executequestion"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }    

    // ACTIONS

    private loadDescription() {
        this.timeService.getTimeAttestFunctionOptionDescription(this.calculationFunction).then(x => {
            this.description = x;
        });
    }

    private initSave() {
        var modal = this.notificationService.showDialogEx(this.terms["core.verifyquestion"], this.terms["time.time.attest.caculationfunctions.executequestion"].format(this.calculationText), SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
        modal.result.then(val => {
            if (val === true) {
                this.executing = true;

                // Prepare selected employees
                var selectedEmployees = this.getSelectedEmployees();
                _.forEach(selectedEmployees, employee => {
                    employee.status = this.terms["time.time.attest.calculationfunctions.pending"];
                    employee.isProcessed = false;
                });

                this.saveOneEmployee();
            }
        });  
    }

    private saveOneEmployee() {
        var employee = this.getNextEmployee();
        if (employee) {   
            employee['statusIcon'] = "far fa-spinner fa-pulse fa-fw";            
            employee.status = this.terms["time.time.attest.calculationfunctions.ongoing"];
            this.save(employee).then(success => {
                employee.resultSuccess = success;
                employee.isProcessed = true;
                this.saveOneEmployee();
            });                
        } else {
            this.executing = false;            
            this.soeGridOptions.clearSelectedRows();
        }
    }

    private save(employee: TimeAttestCalculationFunctionDTO): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();
        let dto = new AttestEmployeesDaySmallDTO();
        dto.employeeId = employee.employeeId;
        dto.dateFrom = this.dateFrom;
        dto.dateTo = this.dateTo;

        this.timeService.applyAttestCalculationFunctionEmployees([dto], this.calculationFunction, this.timeScheduleScenarioHeadId).then(result => {
            if (result.success) {
                employee.status = this.terms["time.time.attest.caculationfunctions.done"] + ". " + result.infoMessage;
                if (result.successNumber == ActionResultSave.CompletedWithWarnings)
                    employee['statusIcon'] = "fal fa-exclamation-triangle warningColor";
                else
                    employee['statusIcon'] = "fal fa-check okColor";
                this.generatedEmployeeIds.push(employee.employeeId);
                deferral.resolve(true);
            } else {
                employee.status = "{0}. {1}".format(this.terms["time.time.attest.caculationfunctions.aborted"], result.errorMessage);
                employee['statusIcon'] = "fal fa-exclamation-triangle errorColor";
                deferral.resolve(false);
            }
        }).catch(reason => {
            employee.status = "{0}. {1}".format(this.terms["time.time.attest.caculationfunctions.error"], reason);
            deferral.resolve(false);
        });
        
        return deferral.promise;
    }

    // EVENTS

    private cancel() {
        //this.$uibModalInstance.dismiss('cancel');
        this.$uibModalInstance.close({
            success: true,
            reloadEmployeeIds: this.generatedEmployeeIds
        });
    }

    // HELP-METHODS

    private getSelectedEmployees() {
        var ids: number[] = this.soeGridOptions.getSelectedIds('employeeId');
        return _.filter(this.employees, e => _.includes(ids, e.employeeId));
    }

    private getNextEmployee() {
        var selectedEmployees = this.getSelectedEmployees();
        return _.find(selectedEmployees, e => !e.isProcessed);
    }

    private showStatusDialog(employee: any) {
        this.notificationService.showDialogEx(this.terms["common.status"], employee.status, SOEMessageBoxImage.Information);
    }
}
