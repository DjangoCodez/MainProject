import { IPromise } from "angular";
import { AccountDimSmallDTO } from "../../../../../../Common/Models/AccountDimDTO";
import { AccountYearLightDTO } from "../../../../../../Common/Models/AccountYear";
import { AccountIntervalSelectionDTO, BoolSelectionDTO, DateRangeSelectionDTO, IdListSelectionDTO, AccountFilterSelectionDTO, AccountFilterSelectionsDTO, YearAndPeriodSelectionDTO, IdSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType, IYearAndPeriodSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../../Util/Constants";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";
import { NamedFilterRange } from "../../../Selections/AccountRangeSelection/AccountRangeSelectionComponent";

export class SruReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: SruReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/SruReport/SruReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "sruReport";

    private labelKey: string;
    private onSelected: (_: { selection: IYearAndPeriodSelectionDTO }) => void = angular.noop;
    private userSelectionInput: YearAndPeriodSelectionDTO;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private accountYears: AccountYearLightDTO[];

    private selectedFromYear: AccountYearLightDTO;
    private selectedToYear: AccountYearLightDTO;

    private selectableFromIntervals: SmallGenericType[];
    private selectableToIntervals: SmallGenericType[];

    private selectedAccountYearIntervals: SmallGenericType[];

    private selectedFromInterval: SmallGenericType;
    private selectedToInterval: SmallGenericType;

    private selectableRangeNames: AccountDimSmallDTO[];
    private selectableRangeNamesWithoutAccount: ISmallGenericType[];
    private rangeFilters: NamedFilterRange[];

    private projectSelected: BoolSelectionDTO;
    private separateAccountDimSelected: BoolSelectionDTO;

    private userSelectionPeriodRange: IdListSelectionDTO;

    private accountPeriodFrom: AccountIntervalSelectionDTO;
    private accountPeriodTo: AccountIntervalSelectionDTO;
    private projectReportTitle = ""; 
    private userSelectionInputPostingDimension: IdSelectionDTO;
    private includeexternalvouchers: BoolSelectionDTO;
    private includeyearendvouchers: BoolSelectionDTO;
    private isSetup = false;

    //@ngInject
    constructor(private $scope: ng.IScope, $timeout: ng.ITimeoutService, private translationService: ITranslationService, private reportDataService: IReportDataService) {
        var userInputs = this.selections;  
        this.rangeFilters = new Array<NamedFilterRange>();
        this.$scope.$watch(() => this.rangeFilters, () => {
            this.saveRangedFilters();
        }, true);

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.applySavedUserFilters(newVal);
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

    public $onInit() {
        this.reportDataService.getAccountDimsSmall(false, false, true, false).then(accountDims => {
            this.isSetup = true;
            this.selectableRangeNames = accountDims;
            this.addFilter(accountDims);

            // If report is loaded from Printed (queue).
            if (this.userSelection)
                this.applySavedUserFilters(this.userSelection);
        });
    }

    private saveRangedFilters() {
        const transformed = new Array<AccountFilterSelectionDTO>();
        this.rangeFilters.forEach(filter => {
            if (filter.selectedSelection) {
                const filterDTO = new AccountFilterSelectionDTO(filter.selectedSelection.accountDimId, filter.selectionFrom, filter.selectionTo);
                transformed.push(filterDTO);
            }
        });
        const filtersDTO = new AccountFilterSelectionsDTO(transformed);
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_NAMED_FILTER_RANGES, filtersDTO);
    }

    private applySavedUserFilters(savedValues: ReportUserSelectionDTO) {
        if (!this.isSetup) return;
        let namedFilters = savedValues.getNamedRangeSelection();
        if (namedFilters.length > 0) {
            this.applySavedRanges(namedFilters);
        }

        this.accountPeriodFrom = savedValues.getIntervalFromSelection() ?? undefined;
        this.accountPeriodTo = savedValues.getIntervalToSelection() ?? undefined;
    }

    private applySavedRanges(namedFilters: AccountFilterSelectionDTO[]) {
        const savedFilters = new Array<NamedFilterRange>();
        namedFilters.forEach(x => {
            const filter = new NamedFilterRange(this.selectableRangeNames);
            if (this.selectableRangeNames) {
                filter.selectedSelection = this.selectableRangeNames.find(y => y.accountDimId === x.id);
            }
            filter.selectionFrom = x.from;
            filter.selectionTo = x.to;
            filter.accountFrom = _.find(filter.selectedSelection.accounts, (account) => account.accountNr == x.from);
            filter.accountTo = _.find(filter.selectedSelection.accounts, (account) => account.accountNr == x.to);
            savedFilters.push(filter);

        });
        this.rangeFilters = savedFilters;
    }

    private addFilter(filters: AccountDimSmallDTO[]) {
        const row = new NamedFilterRange(filters);
        if (this.rangeFilters.length === 0) {
            row.selectedSelection = this.selectableRangeNames[0];
        }
        this.rangeFilters.push(row);
    }

    private removeFilter(selection: number) {
        this.rangeFilters.splice(selection, 1);
    }

    public intervalFromChanged(selection: AccountIntervalSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_PERIOD_FROM, selection);
    }

    public intervalToChanged(selection: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_PERIOD_TO, selection)
    }
        
   
}