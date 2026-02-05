import { IPromise } from "angular";
import { AccountDimSmallDTO } from "../../../../../../Common/Models/AccountDimDTO";
import { AccountIntervalSelectionDTO, BoolSelectionDTO, DateRangeSelectionDTO, AccountFilterSelectionDTO, AccountFilterSelectionsDTO, IdSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType } from "../../../../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../../../../Util/Constants";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";
import { NamedFilterRange } from "../../../Selections/AccountRangeSelection/AccountRangeSelectionComponent";

export class BalanceReport {
    public static component(): ng.IComponentOptions {
        return {
            controller: BalanceReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Economy/BalanceReport/BalanceReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<",
                showRowsByAccountInput: "<"
            }
        } as ng.IComponentOptions;
    }

    public static componentKey = "balanceReport";

    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;
    private selectableRangeNames: AccountDimSmallDTO[];
    private selectableRangeNamesWithoutAccount: ISmallGenericType[];
    private rangeFilters: NamedFilterRange[];
    private projectSelected: BoolSelectionDTO;
    private separateAccountDimSelected: BoolSelectionDTO;
    private dateIsSelectedDTO: BoolSelectionDTO;
    private selectedDateRange: DateRangeSelectionDTO;
    private includeyearendvouchers: BoolSelectionDTO;
    private includeexternalvouchers: BoolSelectionDTO;
    private selectedPostdim: IdSelectionDTO;
    private showRange: boolean;
    private accountPeriodFrom: AccountIntervalSelectionDTO;
    private accountPeriodTo: AccountIntervalSelectionDTO;
    private showRowsByAccountInput: boolean;
    private showRowsByAccount: BoolSelectionDTO;
    private isSetup = false;

    //@ngInject
    constructor(private $scope: ng.IScope, private reportDataService: IReportDataService) {
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
        this.selectedPostdim = savedValues.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_POSTING_DIMENSION);
        this.includeexternalvouchers = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_lNCLUDE_EXTERNAL_VOUCHERS);
        this.includeyearendvouchers = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_lNCLUDE_YEAREND_VOUCHERS);
        this.showRowsByAccount = savedValues.getBoolSelection(Constants.REPORTMENU_SELECTION_KEY_SHOW_ROW_BY_ACCOUNT);

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

    private loadPostingDimension(accountDims: AccountDimSmallDTO[]) {
        this.selectableRangeNamesWithoutAccount = [];
        this.selectableRangeNamesWithoutAccount.push(new SmallGenericType(0, ""));
        accountDims.forEach((dim) => {
            if (dim.accountDimNr === 1) return;
            this.selectableRangeNamesWithoutAccount.push(new SmallGenericType(dim.accountDimId, dim.name));
        });
    }

    private addFilter(filters: AccountDimSmallDTO[]) {
        const row = new NamedFilterRange(filters);
        if (this.rangeFilters.length === 0) {
            row.selectedSelection = this.selectableRangeNames[0];
        }
        this.rangeFilters.push(row);
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

    public onPostingDimensionSelectionChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_POSTING_DIMENSION, selection);
    }

    public onBoolSelectionIncludeExternalVoucherSeries(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_lNCLUDE_EXTERNAL_VOUCHERS, selection);
    }

    public onBoolSelectionShowRowsByAccount(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SHOW_ROW_BY_ACCOUNT, selection);
    }

    public onBoolSelectionInclncludeYearEndVoucherSeries(selection: IBoolSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_lNCLUDE_YEAREND_VOUCHERS, selection);
    }
}