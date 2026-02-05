import { AccountDimSmallDTO } from "../../../../../../Common/Models/AccountDimDTO";
import { BoolSelectionDTO, DateRangeSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../../Common/Models/SmallGenericType";
import { IBoolSelectionDTO, IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO, IYearAndPeriodSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { IStockService } from "../../../../../../Shared/Billing/Stock/StockService";
import { TermGroup } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";

export class InventeringsunderlagReport {
    selectableSorting: ISmallGenericType[];
    selectableInventoryBalanceList: any[];
    selectableStockInventoryList: any[];
    selectableCodeList: any[];
    public static component(): ng.IComponentOptions {
        return {
            controller: InventeringsunderlagReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/InventeringsunderlagReport/InventeringsunderlagReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<",
            }
        } as ng.IComponentOptions;
    }

    public static componentKey = "inventeringsunderlagReport";

    private onSelected: (_: { selection: IYearAndPeriodSelectionDTO }) => void = angular.noop;
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;

    private projectSelected: BoolSelectionDTO;
    private dateIsSelectedDTO: BoolSelectionDTO;
    private selectedDateRange: DateRangeSelectionDTO;

    private showRange: boolean;
    private rangeCodeListFilters: NamedFilterRange[];
    private rangeInventoryBalanceListFilters: NamedFilterRange[];
    

    private projectReportTitle = "";

    private voucherSeriesFrom: SmallGenericType;
    private inventoryBalanceFrom: IdSelectionDTO;
    private selectedSortItem: IdSelectionDTO;
    private selectedStockInventoryItem: IdSelectionDTO;

    private inventoryBalanceTo: IdSelectionDTO;
    private codeSeriesFrom: IdSelectionDTO;
    private codeSeriesTo: IdSelectionDTO;
    private articleNumberFrom: TextSelectionDTO;
    private articleNumberTo: TextSelectionDTO;

    private onInventoryBalanceSelectedFrom: (_: { selection: IdSelectionDTO }) => void = angular.noop;
    private onInventoryBalanceSelectedTo: (_: { selection: IdSelectionDTO }) => void = angular.noop;
    private onCodeSeriesSelectedFrom: (_: { selection: IdSelectionDTO }) => void = angular.noop;
    private onCodeSeriesSelectedTo: (_: { selection: IdSelectionDTO }) => void = angular.noop;

    //@ngInject
    constructor(private $scope: ng.IScope, $timeout: ng.ITimeoutService, private translationService: ITranslationService, private reportDataService: IReportDataService, private coreService: ICoreService, private stockService: IStockService) {

        this.$scope.$watch(() => this.selections, (newVal, oldVal) => {
            if (!newVal)
                return;
        });

        this.$scope.$watch(() => this.userSelection, (newVal, oldVal) => {
            if (!newVal)
                return;
            this.setSavedUserFilters(newVal);
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
        this.rangeInventoryBalanceListFilters = new Array<NamedFilterRange>();
        this.rangeCodeListFilters = new Array<NamedFilterRange>();
        this.getInventoryBalanceList();
        this.getCodeList();
        this.getSortOrder();
        this.getStockInventories();
    }

    private setSavedUserFilters(savedValues: ReportUserSelectionDTO) {

        this.selectedDateRange = savedValues.getDateRangeSelection();
        this.inventoryBalanceFrom = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_BALANCE_FROM);
        this.inventoryBalanceTo = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_BALANCE_TO);
        this.codeSeriesFrom = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_CODE_SERIES_FROM);
        this.codeSeriesTo = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_CODE_SERIES_TO);
        this.articleNumberFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM);
        this.articleNumberTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO);
        this.selectedSortItem = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);
    }

    private getStockInventories() {
        this.selectableStockInventoryList = [];
        return this.stockService.getStockInventories().then((x) => {
            this.selectableStockInventoryList.push({ id: 0, name: ' ' });
            x.forEach(y => {
                this.selectableStockInventoryList.push({ id: y.stockInventoryHeadId, name: y.headerText });
            });
        });
    }

    private getSortOrder() {
        this.selectableSorting = [];
        return this.coreService.getTermGroupContent(TermGroup.ReportBillingStockSortOrder, false, false, false).then(data => {
            this.selectableSorting = data;
            this.selectedSortItem = new IdSelectionDTO(this.selectableSorting[0].id);
        });
    }

    private getInventoryBalanceList() {
        this.selectableInventoryBalanceList = [];
        return this.stockService.getStocksDict(true).then(data => {
            this.selectableInventoryBalanceList = data;
        });
    }

    private getCodeList() {
        this.selectableCodeList = [];
        return this.stockService.getStockPlaces(true, 0).then(data => {

            data.forEach(filter => {
                this.selectableCodeList.push({ id: filter.stockShelfId, name: filter.name });
            });

        });
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
    }

    public onDateSelectionSelected(selection: IBoolSelectionDTO) {
        this.showRange = !selection.value;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_SELECTION_CHOOSEN, selection);
    }

    public onSortOrderSelectionChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER, selection);
    }

    public articleNumberChangedFrom(selection: ITextSelectionDTO) {
        var selectionFrom = new TextSelectionDTO(selection.text);
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM, selection);

        if (!this.articleNumberTo || !this.articleNumberTo.text) {
            this.articleNumberTo = selection;
        }
    }
    public onStockInventorySelectionChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_STOCK_INVENTORY, selection);
    }

    private articleNumberChangedTo(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO, selection);
    }

    private inventoryBalanceChangedFrom(selection: IIdSelectionDTO) {

        let selectionsFrom = new IdSelectionDTO(selection.id);
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_BALANCE_FROM, selection);

        if (!this.inventoryBalanceTo || !this.inventoryBalanceTo.id) {
            this.inventoryBalanceTo = selectionsFrom;
        }
        this.onInventoryBalanceSelectedFrom({ selection: selectionsFrom });

        //this.addInventoryBalanceListFilter();
    }

    private inventoryBalanceChangedTo(selection: IIdSelectionDTO) {

        let selections = new IdSelectionDTO(selection.id);
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_BALANCE_TO, selections);

        this.onInventoryBalanceSelectedTo({ selection: selections });
    }

    private codeSeriesChangedFrom(selection: IIdSelectionDTO) {

        let selectionFrom = new IdSelectionDTO(selection.id);
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CODE_SERIES_FROM, selection);

        if (!this.codeSeriesTo || !this.codeSeriesTo.id) {
            this.codeSeriesTo = selectionFrom;
        }
        this.onCodeSeriesSelectedFrom({ selection: selectionFrom });

        this.addCodeListFilter();
    }

    private codeSeriesChangedTo(selection: IIdSelectionDTO) {

        let selections = new IdSelectionDTO(selection.id);
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CODE_SERIES_TO, selections);

        this.onCodeSeriesSelectedTo({ selection: selections });
    }

    private addInventoryBalanceListFilter() {
        const row = new NamedFilterRange(this.selectableInventoryBalanceList);
        if (this.rangeInventoryBalanceListFilters.length === 0) {
            row.selectedSelection = this.selectableInventoryBalanceList[0];
            this.rangeInventoryBalanceListFilters.push(row);
            this.selectableInventoryBalanceList.push({ id: 0, name: " " });

            // sort by name
            this.selectableInventoryBalanceList.sort((a, b) => {
                if (a.id < b.id) { return -1; }
                if (a.id > b.id) { return 1; }

                // else names must be equal
                return 0;
            });
        }
    }

    private addCodeListFilter() {
        const row = new NamedFilterRange(this.selectableCodeList);
        if (this.rangeCodeListFilters.length === 0) {
            row.selectedSelection = this.selectableCodeList[0];
            this.rangeCodeListFilters.push(row);
            this.selectableCodeList.push({ id: 0, name: " " });

            // sort by name
            this.selectableCodeList.sort((a, b) => {
                if (a.id < b.id) { return -1; }
                if (a.id > b.id) { return 1; }

                // else names must be equal
                return 0;
            });
        }
    }

}

export class NamedFilterRange {

    constructor(private availableSelectionNames: AccountDimSmallDTO[]) {
        this.selectionFrom = "";
        this.selectionTo = "";
        this.selectedSelection = null;
    }

    public selectedSelection: AccountDimSmallDTO;
    public selectionFrom: string;
    public selectionTo: string;

    public getIndexOfSelected(): number {
        if (!this.selectedSelection) return -1;
        return this.availableSelectionNames.findIndex(x => x.accountDimId === this.selectedSelection.accountDimId);
    }
}