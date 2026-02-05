import { AttestStateDTO } from "../../../../../../Common/Models/AttestStateDTO";
import { EmployeeAttestResult } from "../../../../../../Common/Models/TimeEmployeeTreeDTO";
import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { ISoeGridOptions, SoeGridOptions } from "../../../../../../Util/SoeGridOptions";
import { StringUtility } from "../../../../../../Util/StringUtility";
export class AttestResultDialogController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private soeGridOptions: ISoeGridOptions;

    private title: string;

    //@ngInject
    constructor(private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private translationService: ITranslationService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private attestStateTo: AttestStateDTO,
        private employeeResults: EmployeeAttestResult[],
    ) {

        this.$q.all([
            this.loadTerms(),
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
        this.soeGridOptions.enableRowSelection = false;
        this.soeGridOptions.enableRowHeaderSelection = false;
        this.soeGridOptions.enableFullRowSelection = false;
        this.soeGridOptions.addColumnText("numberAndName", this.terms["common.employee"], "170");
        this.soeGridOptions.addColumnText("status", this.terms["common.status"], "270");
        this.soeGridOptions.addColumnText("datesFailedString", this.terms["common.date"], null);

        var failedEmployeeResults = _.filter(this.employeeResults, e => e.success === false);
        _.forEach(failedEmployeeResults, (employeeResult: EmployeeAttestResult) => {
            employeeResult.status = this.terms["time.time.attest.saveattestemployeesresultinvalid"].format(employeeResult.noOfDaysFailed.toString(), StringUtility.nullToEmpty(this.attestStateTo.name));
        });

        this.title = this.terms["time.time.attest.result"].format(failedEmployeeResults.length.toString(), StringUtility.nullToEmpty(this.attestStateTo?.name));
        this.soeGridOptions.setData(failedEmployeeResults);
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.employee",
            "common.days",
            "common.date",
            "common.status",
            "time.time.attest.result",
            "time.time.attest.saveattestemployeesresultinvalid"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    // ACTIONS

    // EVENTS

    private buttonOkClick() {
        this.$uibModalInstance.close({
            success: true,
        });
    }
    private cancel() {
        this.$uibModalInstance.close({
            success: true,
        });
    }
}

