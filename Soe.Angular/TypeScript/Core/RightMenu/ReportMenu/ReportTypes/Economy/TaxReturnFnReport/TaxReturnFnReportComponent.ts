import { AccountYearLightDTO } from "../../../../../../Common/Models/AccountYear";
import { AccountIntervalSelectionDTO, BoolSelectionDTO, DateRangeSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType } from "../../../../../../Scripts/TypeLite.Net4";
import { TermGroup } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";

export class TaxReturnFnReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: TaxReturnFnReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/TaxReturnFnReport/TaxReturnFnReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "taxReturnFnReport";

    private selections: SelectionCollection;
    private userSelection: ReportUserSelectionDTO;
    private projectReportTitle = ""; 

    private accountPeriodFrom: AccountIntervalSelectionDTO;
    private accountPeriodTo: AccountIntervalSelectionDTO;

    private createVatVoucher: BoolSelectionDTO;
    private selectedFromInterval: SmallGenericType;
    private selectedToInterval: SmallGenericType;
    private selectedFromYear: AccountYearLightDTO;
    private selectedToYear: AccountYearLightDTO;

    //@ngInject
    constructor(private $scope: ng.IScope, $timeout: ng.ITimeoutService, private translationService: ITranslationService, private reportDataService: IReportDataService) {

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.setIntervals(newVal); 
        });

        const keys = [
            "billing.project.central.projectreports",
            "common.report.daterangeselection",
            "common.report.seperatereport",
            "common.report.accountselection",
            "common.report.distributionreport",
            "common.report.standardselection"
        ]

        this.translationService.translateMany(keys).then(terms => {
            this.projectReportTitle = terms["billing.project.central.projectreports"];
        });
    }

    private setIntervals(savedValues: ReportUserSelectionDTO) {

        const accountPeriodFrom = savedValues.getIntervalFromSelection();
        if (accountPeriodFrom) {
            this.accountPeriodFrom = accountPeriodFrom;
        }
        const accountPeriodTo = savedValues.getIntervalToSelection();
        if (accountPeriodTo) {
            this.accountPeriodTo = accountPeriodTo;
        }
    }

    public intervalFromChanged(selection: AccountIntervalSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_PERIOD_FROM, selection);
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
    }

    public intervalToChanged(selection: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_PERIOD_TO, selection)
    }

}
