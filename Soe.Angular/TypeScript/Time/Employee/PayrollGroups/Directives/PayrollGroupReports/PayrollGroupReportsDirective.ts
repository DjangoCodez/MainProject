import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { PayrollGroupReportDTO } from "../../../../../Common/Models/PayrollGroupDTOs";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { IReportService } from "../../../../../Core/Services/ReportService";
import { SoeReportTemplateType } from "../../../../../Util/CommonEnumerations";
import { ReportViewDTO } from "../../../../../Common/Models/ReportDTOs";

export class PayrollGroupReportsDirectiveFactory {
    //@ngInject
    public static create(urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Time/Employee/PayrollGroups/Directives/PayrollGroupReports/Views/PayrollGroupReports.html'),
            scope: {
                payrollGroupId: '=',
                reportIds: '=',
                readOnly: '=',
                onChange: '&'
            },
            restrict: 'E',
            replace: true,
            controller: PayrollGroupReportsController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

export class PayrollGroupReportsController {

    // Init parameters
    private payrollGroupId: number;
    private reportIds: number[];

    // Data
    private allReports: ReportViewDTO[] = [];

    // Flags
    private readOnly: boolean;

    // Events
    private onChange: Function;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private $uibModal,
        private $q: ng.IQService,
        private urlHelperService: IUrlHelperService,
        private reportService: IReportService,
        private payrollService: IPayrollService) {

        this.$q.all([
            this.loadReports()
        ]).then(() => {
            this.setupWatchers();
        });
    }

    private setupWatchers() {
        this.$scope.$watchCollection(() => this.reportIds, (newVal, oldVal) => {
            if (newVal !== oldVal)
                this.setSelectedReports();
        });
    }

    // SERVICE CALLS

    private loadReports(): ng.IPromise<any> {
        this.allReports = [];

        var reportTypes: number[] = [];
        reportTypes.push(SoeReportTemplateType.TimeEmploymentContract);
        reportTypes.push(SoeReportTemplateType.TimeEmploymentDynamicContract);

        return this.reportService.getReportsForType(reportTypes, true, false).then(x => {
            this.allReports = x;
            this.setSelectedReports();
        });
    }

    // EVENTS

    private reportSelected(report: ReportViewDTO) {
        if (!this.readOnly) {
            report['selected'] = !report['selected'];
            if (report['selected'])
                this.reportIds.push(report.reportId);
            else
                _.pull(this.reportIds, report.reportId);
            this.setAsDirty();
        }
    }

    // HELP-METHODS

    private setSelectedReports() {
        _.forEach(this.allReports, report => {
            report['selected'] = _.includes(this.reportIds, report.reportId);
        });

        // Set sort order once (on load)

        let i = 0;
        _.forEach(_.orderBy(this.allReports, ['selected', 'reportName'], ['desc', 'asc']), report => {
            report['sort'] = i++;
        });
    }

    private setAsDirty() {
        if (this.onChange)
            this.onChange();
    }
}