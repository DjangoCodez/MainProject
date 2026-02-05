import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { SysJobSettingDTO } from "../../../../../Common/Models/SysJobDTO";
import { SoeGridOptionsAg } from "../../../../../Util/SoeGridOptionsAg";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { ISystemService } from "../../../SystemService";



export class TestCaseDialogController {

    private testCaseGrid: SoeGridOptionsAg;
    private terms: { [index: string]: string; };
    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $timeout,
        private systemService: ISystemService,
        private translationService: ITranslationService) {
            this.testCaseGrid = SoeGridOptionsAg.create("TestCases", this.$timeout);
            this.testCaseGrid.setMinRowsToShow(25);
            this.setup();
        }


    private setup() {
        this.loadTerms().then(() => {
            this.setupGrid()
        })
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.number",
            "common.name",
            "common.description",
            "common.type",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private setupGrid() {
        this.testCaseGrid.enableRowSelection = true;
        this.testCaseGrid.addColumnNumber("testCaseId", this.terms["common.number"], null);
        this.testCaseGrid.addColumnText("name", this.terms["common.name"], null);
        this.testCaseGrid.addColumnText("description", this.terms["common.description"], null);
        this.testCaseGrid.addColumnText("testCaseType", this.terms["common.type"], null);
        this.testCaseGrid.finalizeInitGrid();
        this.loadTestCases();
    }

    private loadTestCases() {
        this.systemService.getTestCases().then(data => {
            this.testCaseGrid.setData(data);
        })
    }

    public cancel() {
        this.$uibModalInstance.close();
    }

    public ok() {
        this.$uibModalInstance.close({ testCaseIds: this.testCaseGrid.getSelectedIds("testCaseId") });
    }
}
