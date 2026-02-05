import { ICoreService } from "../../../Core/Services/CoreService";
import { IReportService } from "../../../Core/Services/ReportService";
import { ISoeGridOptions } from "../../../Util/SoeGridOptions";
import { CoreUtility } from "../../../Util/CoreUtility";
import { TermGroup, SoeReportTemplateType, SoeModule } from "../../../Util/CommonEnumerations";
import { ReportViewDTO } from "../../Models/ReportDTOs";

export class SelectReportController {
    private soeGridOptions: ISoeGridOptions;
    private languages: any[];
    private languageId: number;
    private isCopy: boolean = true;
    private isReminder: boolean = false;
    private loadingReports: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private coreService: ICoreService,
        private reportService: IReportService,
        private module: SoeModule,
        private reportTypes: SoeReportTemplateType[],
        private showCopy: boolean,
        private showEmail: boolean,
        private copyValue: boolean,
        private reports: ReportViewDTO[],
        private defaultReportId: number,
        private langId: number,
        private showReminder: number,
        private showLangSelection: boolean = true,
        private showSavePrintout: boolean,
        private savePrintout: boolean) {

        this.loadLanguages();

        if (!this.reports || this.reports.length === 0)
            this.loadReports();
        else
            this.setDefaultReport();

        if (this.showCopy)
            this.isCopy = this.copyValue;
    }

    private loadLanguages() {
        this.languages = [];

        return this.coreService.getTermGroupContent(TermGroup.Language, true, false).then(data => {
            this.languages = data;
            if (this.langId !== -1) {
                this.languageId = this.langId ? this.langId : CoreUtility.sysCountryId;
            }
        });
    }

    private loadReports() {
        this.loadingReports = true;
        this.reports = [];

        return this.reportService.getReportsForType(this.reportTypes, true, false, this.module).then(x => {
            this.reports = x;
            this.setDefaultReport();
            this.loadingReports = false;
        });
    }

    private setDefaultReport() {
        if (this.defaultReportId) {
            const report = _.find(this.reports, r => r.reportId === this.defaultReportId);
            if (report)
                report['default'] = true;
        }
    }

    printClick(report: ReportViewDTO) {
        this.close(report, false);
    }

    emailClick(report: ReportViewDTO) {
        this.close(report, true);
    }

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }

    close(report: ReportViewDTO, email: boolean) {
        if (!report) {
            this.buttonCancelClick();
        } else {
            this.$uibModalInstance.close({ reportId: report.reportId, reportType: report.sysReportTemplateTypeId, languageId: this.languageId, createCopy: this.isCopy, email: email, reminder: this.isReminder, savePrintout: this.savePrintout, employeeTemplateId: report.employeeTemplateId });
        }
    }
}