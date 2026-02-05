import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IVacationYearEndEmployeeResultDTO } from "../../../Scripts/TypeLite.Net4";
import { TermGroup_VacationYearEndStatus } from "../../../Util/CommonEnumerations";
import { ExportUtility } from "../../../Util/ExportUtility";

export class VacationYearEndResultDialogController {
    public progress: IProgressHandler;
    private terms: { [index: string]: string; };
    private nr: string = '';
    
    //@ngInject
    constructor(private $uibModalInstance,
        private results: IVacationYearEndEmployeeResultDTO[],
        private date: Date,
        private showVacationGroup: boolean,
        private header: string,
        private translationService: ITranslationService) {
        this.loadTerms();
    }

    private ok() {
        this.$uibModalInstance.dismiss();
    }
    private loadTerms() {
        var keys: string[] = [
            "common.employee",
            "time.payroll.vacationgroup.vacationgroup",
            "common.message",
            "time.payroll.vacationyearend.vacationyearend",
            "common.status",
            "time.payroll.vacationyearend.ok",
            "time.payroll.vacationyearend.failed"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.setResult();
        });
    }

    private setResult() {
        this.nr = this.results.filter(f => f.status === TermGroup_VacationYearEndStatus.Succeded).length + ' ' + this.terms["time.payroll.vacationyearend.ok"];
        this.nr += ' '+this.results.filter(f => f.status === TermGroup_VacationYearEndStatus.Failed).length + ' ' + this.terms["time.payroll.vacationyearend.failed"];
    }

    private exportToEcxcel() {
       
        let headers: string[] = [];
        headers.push(this.terms["common.employee"]);
        if (this.showVacationGroup)
            headers.push(this.terms["time.payroll.vacationgroup.vacationgroup"]);
        headers.push(this.terms["common.status"]);
        headers.push(this.terms["common.message"]);
       
        let content: string = headers.join(';') + '\r\n';
        let fileName: string = this.terms["time.payroll.vacationyearend.vacationyearend"] + "_"+this.date;
        _.forEach(this.results, row => {
           
            let rowContent: string[] = [];
            rowContent.push(row.employeeNrAndName);
            if (this.showVacationGroup)
                rowContent.push(row.vacationGroupName);
            rowContent.push(row.statusName);
            rowContent.push(row.message);
               
            content += rowContent.join(';') + '\r\n'
           
        });
       
        ExportUtility.ExportToCSV('\uFEFF' + content, fileName + '.csv');
    }

}