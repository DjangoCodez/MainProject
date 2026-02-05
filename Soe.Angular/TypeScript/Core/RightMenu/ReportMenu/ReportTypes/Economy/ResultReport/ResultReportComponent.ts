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
import { IReportService } from "../../../../../Services/reportservice";

export class ResultReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: ResultReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/ResultReport/ResultReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<",
                includeBudget: "<",
                showRowsByAccountInput: "<"
            }
        } as ng.IComponentOptions;
    }
    public static componentKey = "resultReport";

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
    private budgetHeadsDicts: ISmallGenericType[];
    private rangeFilters: NamedFilterRange[];

    private projectSelected: BoolSelectionDTO;
    private separateAccountDimSelected: BoolSelectionDTO;
    private dateIsSelectedDTO: BoolSelectionDTO;
    private selectedDateRange: DateRangeSelectionDTO;

    private userSelectionPeriodRange: IdListSelectionDTO;

    private accountPeriodFrom: AccountIntervalSelectionDTO;
    private accountPeriodTo: AccountIntervalSelectionDTO;
    private projectReportTitle = ""; 
    private userSelectionInputPostingDimension: IdSelectionDTO;
    private userSelectionInputBudgetHeadsDict: IdSelectionDTO;
    private includeexternalvouchers: BoolSelectionDTO;
    private includeyearendvouchers: BoolSelectionDTO;
    private includeBudget: boolean;
    private showRowsByAccountInput: boolean;
    private showRowsByAccount: BoolSelectionDTO;
    private showRange: boolean;

    private isSetup = false;

    //@ngInject
    constructor(private $scope: ng.IScope, $timeout: ng.ITimeoutService, private translationService: ITranslationService, private reportDataService: IReportDataService) {
        this.includeexternalvouchers = new BoolSelectionDTO(true);
        this.includeyearendvouchers = new BoolSelectionDTO(true);

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
        this.showRowsByAccount = new BoolSelectionDTO(this.showRowsByAccountInput);
        this.reportDataService.getAccountDimsSmall(false, false, true, false).then(accountDims => {
            this.isSetup = true;
            this.selectableRangeNames = accountDims;
            this.addFilter(accountDims);
            this.loadPostingDimension(accountDims);

            // If report is loaded from Printed (queue).
            if (this.userSelection)
                this.applySavedUserFilters(this.userSelection);
        });
        this.loadBudgets();
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
        this.userSelectionInputPostingDimension = savedValues.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_POSTING_DIMENSION);
        this.userSelectionInputBudgetHeadsDict = savedValues.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_BUDGET);
        this.userSelectionPeriodRange = savedValues.getIdListSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM);
        this.includeexternalvouchers = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_lNCLUDE_EXTERNAL_VOUCHERS);
        this.includeyearendvouchers = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_lNCLUDE_YEAREND_VOUCHERS);
        this.showRowsByAccount = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_ROW_BY_ACCOUNT);

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

    private loadPostingDimension(accountDims: AccountDimSmallDTO[]) {
        this.selectableRangeNamesWithoutAccount = [];
        this.selectableRangeNamesWithoutAccount.push(new SmallGenericType(0, ""));
        accountDims.forEach((dim) => {
            if (dim.accountDimNr === 1) return;
            this.selectableRangeNamesWithoutAccount.push(new SmallGenericType(dim.accountDimId, dim.name));
        });
    }

    private loadBudgets() {
        this.budgetHeadsDicts = [];
        this.reportDataService.getBudgetHeadsDict().then(bd => {
            this.budgetHeadsDicts.push(new SmallGenericType(0, ""));
            _.forEach(bd, (b) => {
                this.budgetHeadsDicts.push(new SmallGenericType(b.id, b.name));
            });
        });
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
        
    public onProjectSelected(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PROJECT_SELECTION_CHOOSEN, selection);
    }

    public onSeparateAccountDimSelected(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SEPARATE_ACCOUNT_DIM, selection);
    }

    public onPostingDimensionSelectionChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_POSTING_DIMENSION, selection);
    }

    public onBudgetSelectionChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_BUDGET, selection);
    }

    public onBoolSelectionIncludeExternalVoucherSeries(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_lNCLUDE_EXTERNAL_VOUCHERS, selection);
    }

    public onBoolSelectionInclncludeYearEndVoucherSeries(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_lNCLUDE_YEAREND_VOUCHERS, selection);
    }

    public onBoolSelectionShowRowsByAccount(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_ROW_BY_ACCOUNT, selection);
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
    }

    public onDateSelectionSelected(selection: IBoolSelectionDTO) {
        this.showRange = !selection.value;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_SELECTION_CHOOSEN, selection);
    }
}