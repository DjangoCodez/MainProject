import { IPromise } from "angular";
import { AccountDimSmallDTO } from "../../../../../../Common/Models/AccountDimDTO";
import { AccountYearLightDTO } from "../../../../../../Common/Models/AccountYear";
import { AccountIntervalSelectionDTO, BoolSelectionDTO, DateRangeSelectionDTO, IdListSelectionDTO, AccountFilterSelectionDTO, AccountFilterSelectionsDTO, YearAndPeriodSelectionDTO, IdSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IYearAndPeriodSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../../Util/Constants";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";
import { NamedFilterRange } from "../../../Selections/AccountRangeSelection/AccountRangeSelectionComponent";

export class GeneralLedgerReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: GeneralLedgerReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/GeneralLedgerReport/GeneralLedgerReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "distributionSelectionReport";

    private labelKey: string;
    //private items: SmallGenericType[];
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

    private rangeName: string;
    private rangeFrom: string;
    private rangeTo: string;

    private selectableRangeNames: AccountDimSmallDTO[];
    private rangeFilters: NamedFilterRange[];

    private projectSelected: BoolSelectionDTO;
    private separateAccountDimSelected: BoolSelectionDTO;
    private dateIsSelectedDTO: BoolSelectionDTO;
    private selectedDateRange: DateRangeSelectionDTO;

    private userSelectionPeriodRange: IdListSelectionDTO;
    private showRange: boolean;

    private accountPeriodFrom: AccountIntervalSelectionDTO;
    private accountPeriodTo: AccountIntervalSelectionDTO;
    private reportTitle = "Huvudrapport";
    private standardSelectionTitle = "Standardurval";

    private userSelectionInputVoucherSeries: IdListSelectionDTO;
    private isSetup = false;

    //@ngInject
    constructor(private $scope: ng.IScope, $timeout: ng.ITimeoutService, private translationService: ITranslationService, private reportDataService: IReportDataService) {
        this.rangeFilters = new Array<NamedFilterRange>();
        this.$scope.$watch(() => this.rangeFilters, () => {
            this.saveRangedFilters();
        }, true);

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;

            this.applySavedUserFilters(newVal);
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
        this.projectSelected = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_PROJECT_SELECTION_CHOOSEN);
        this.separateAccountDimSelected = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SEPARATE_ACCOUNT_DIM);
        this.userSelectionInputVoucherSeries = this.userSelection.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_VOUCHER_SERIES);

        let namedFilters = savedValues.getNamedRangeSelection();
        if (namedFilters.length > 0) {
            this.applySavedRanges(namedFilters);
        }

        this.accountPeriodFrom = savedValues.getIntervalFromSelection() ?? undefined;
        this.accountPeriodTo = savedValues.getIntervalToSelection() ?? undefined;
        
        const dateSelectionChoosen = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_DATE_SELECTION_CHOOSEN);
        this.dateIsSelectedDTO = dateSelectionChoosen;
        this.showRange = !dateSelectionChoosen.value;
        if (!this.showRange) {
            this.selectedDateRange = savedValues.getDateRangeSelection();
        }
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

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
    }

    public intervalToChanged(selection: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_ACCOUNT_PERIOD_TO, selection)
    }

    public onDateSelectionSelected(selection: IBoolSelectionDTO) {
        this.showRange = !selection.value;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_SELECTION_CHOOSEN, selection);
    }

    public onProjectSelected(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_SELECTION_CHOOSEN, selection);
    }

    public onSeparateAccountDimSelected(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SEPARATE_ACCOUNT_DIM, selection);
    }
    private onVoucherSeriesSelectionUpdated(selection: IdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_VOUCHER_SERIES, selection);
    }
}
