import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../../Core/Services/NotificationService";
import { ISoeGridOptions, SoeGridOptions } from "../../../../../../Util/SoeGridOptions";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../../../Util/Enumerations";
import { TimeAttestCalculationFunctionDTO } from "../../../../../../Common/Models/TimeEmployeeTreeDTO";
import { ActionResultSave, TermGroup_TimePeriodType } from "../../../../../../Util/CommonEnumerations";
import { ITimeService } from "../../../../../../Time/Time/TimeService";
import { TimePeriodDTO } from "../../../../../../Common/Models/TimePeriodDTO";
import { CoreUtility } from "../../../../../../Util/CoreUtility";

export class PeriodCalculationController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private soeGridOptions: ISoeGridOptions;
    private generatedEmployeeIds: number[] = [];
    private timePeriods: any;
    private currentPeriodName: string = "";
    private parentPeriodName: string = "";

    private _selectedPeriodId: any;
    private onlyCurrentPeriod = false;

    get selectedPeriodId() {
        return this._selectedPeriodId;
    }
    set selectedPeriodId(item: any) {
        this._selectedPeriodId = item;
        this.loadPeriodData();
     
    }
    private title: string;
    // Flags
    private executing: boolean = false;

    //@ngInject
    constructor(private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private timeService: ITimeService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private employees: any[],
        private setAsSelected: boolean,
        private dateFrom: Date,
        private dateTo: Date,
       ) {

        this.$q.all([
            this.loadTerms(),
            this.loadTimePeriods()        
        ]).then(() => {
            this.setupGrid();
        });
    }

    private $onInit() {
        this.selectedPeriodId = 0;
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
        this.title = this.terms["time.time.attest.calculate.overtime"];
        this.soeGridOptions.addColumnText("numberAndName", this.terms["common.employee"], "170");
        this.soeGridOptions.addColumnText("status", this.terms["common.status"], null, false, "status");
        this.soeGridOptions.addColumnIcon("statusIcon", null, null, "showStatusDialog");
        this.soeGridOptions.addColumnText("currentPeriod", this.currentPeriodName, null, false, "");
        if (!this.onlyCurrentPeriod && this.parentPeriodName != "")
            this.soeGridOptions.addColumnText("parentPeriod", this.parentPeriodName, null, false, "");
        
        this.soeGridOptions.setData(this.employees);
        if (this.setAsSelected)
            this.$timeout(() => this.soeGridOptions.selectAllRows());
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.employee",
            "common.status",
            "time.time.attest.calculate.overtime",
            "core.verifyquestion",
            "time.time.attest.calculationfunctions.pending",
            "time.time.attest.calculationfunctions.ongoing",
            "time.time.attest.caculationfunctions.done",
            "time.time.attest.caculationfunctions.aborted",
            "time.time.attest.caculationfunctions.error",
            "time.time.attest.calculate.overtime.validate"

        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadTimePeriods(): ng.IPromise<any> {

        return this.timeService.getPeriodsForCalculation(TermGroup_TimePeriodType.RuleWorkTime, this.dateFrom, this.dateTo, true).then((x) => {
            _.forEach(x.periods, (o: TimePeriodDTO) => {
                o.name = o.name + '. ' + new Date(o.startDate).toLocaleDateString(CoreUtility.language) + ' - ' + new Date(o.stopDate).toLocaleDateString(CoreUtility.language);
              
            });
            this.timePeriods = x.periods;
            if (x.currentPeriod == x.parentPeriod)
                this.onlyCurrentPeriod = true;

            this.currentPeriodName = x.currentPeriod;
            this.parentPeriodName = x.parentPeriod;

        });
         
    }
    private loadPeriodData(): ng.IPromise<any> {
        this.cleanEmployees();
        if (this.selectedPeriodId === undefined || this.selectedPeriodId == 0)
            return;
        var ids = this.employees.map(obj => obj.employeeId); 
        return this.timeService.getCalculationsFromPeriod(ids, this.selectedPeriodId).then((x) => {
            _.forEach(x, y => {
                var employee = this.employees.find(e => e.employeeId == y.employeeId);
                employee.currentPeriod = y.currentPeriod;
                employee.parentPeriod = y.parentPeriod;
            });
        });

    }
    // ACTIONS
    private initSave() {
        var modal = this.notificationService.showDialogEx(this.terms["core.verifyquestion"], this.terms["time.time.attest.calculate.overtime.validate"], SOEMessageBoxImage.Question, SOEMessageBoxButtons.OKCancel);
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
            this.loadPeriodData();
        }
    }

    private save(employee: TimeAttestCalculationFunctionDTO): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();
        this.timeService.createTransactionsForPlannedPeriodCalculation(employee.employeeId, this.selectedPeriodId).then(result => {
            if (result.success) {
                employee.status = this.terms["time.time.attest.caculationfunctions.done"] + ".";
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

    private cleanEmployees() {
         _.forEach(this.employees, (employee: any) => {
            employee.currentPeriod = "";
            employee.parentPeriod = "";
        });
       
    }
}
