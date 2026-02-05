import { AccountDimSmallDTO } from "../../../../../../Common/Models/AccountDimDTO";
import { DateRangeSelectionDTO, IdSelectionDTO, TextSelectionDTO } from "../../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../../Common/Models/ReportDTOs";
import {  IIdSelectionDTO, ISmallGenericType, ITextSelectionDTO } from "../../../../../../Scripts/TypeLite.Net4";
import { TermGroup, TermGroup_StockTransactionType } from "../../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../../Util/Constants";
import { ICoreService } from "../../../../../Services/CoreService";
import { ITranslationService } from "../../../../../Services/TranslationService";
import { IReportDataService } from "../../../ReportDataService";
import { SelectionCollection } from "../../../SelectionCollection";

export class StockHistoryReport {
    selectableSorting: ISmallGenericType[];
    selectableInventoryBalanceList: any[];
    selectableCodeList: any[];
    selectableStockTransactionTypes: ISmallGenericType[];
    public static component(): ng.IComponentOptions {
        return {
            controller: StockHistoryReport,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/ReportTypes/Billing/StockHistoryReport/StockHistoryReportView.html",
            bindings: {
                userSelection: "=",
                selections: "<",
            }
        } as ng.IComponentOptions;
    }

    public static componentKey = "stockHistoryReport";
    private userSelection: ReportUserSelectionDTO;
    private selections: SelectionCollection;

    private rangeCodeListFilters: NamedFilterRange[];
    private rangeInventoryBalanceListFilters: NamedFilterRange[];

    private projectReportTitle = "";
    
    private articleNumberFrom: TextSelectionDTO;
    private articleNumberTo: TextSelectionDTO;

    private inventoryBalanceFrom: IdSelectionDTO;
    private inventoryBalanceTo: IdSelectionDTO;

    private codeSeriesFrom: IdSelectionDTO;
    private codeSeriesTo: IdSelectionDTO;

    private selectedDateRange: DateRangeSelectionDTO;
    private selectedSortItem: IdSelectionDTO;
    private selectedStockTransactionType: IdSelectionDTO;

    private articleNumberHandler: boolean = true;
    private inventoryBalanceHandler: boolean = true;
    private codeSeriesHandler: boolean = true;

    //@ngInject
    constructor(private $scope: ng.IScope, $timeout: ng.ITimeoutService, private translationService: ITranslationService, private reportDataService: IReportDataService, private coreService: ICoreService) {

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
        this.getStockTransactionTypes();
        this.getSortOrder();
    }

    private setSavedUserFilters(savedValues: ReportUserSelectionDTO) {

        this.selectedDateRange = savedValues.getDateRangeSelection();
        this.inventoryBalanceFrom = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_BALANCE_FROM);
        this.inventoryBalanceHandler = true;
        this.inventoryBalanceTo = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_BALANCE_TO);
        this.codeSeriesFrom = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_CODE_SERIES_FROM);
        this.codeSeriesHandler = true;
        this.codeSeriesTo = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_CODE_SERIES_TO);
        this.articleNumberFrom = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM);
        this.articleNumberHandler = true;
        this.articleNumberTo = this.userSelection.getTextSelection(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO);
        this.selectedSortItem = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER);
        this.selectedStockTransactionType = this.userSelection.getIdSelection(Constants.REPORTMENU_SELECTION_KEY_STOCK_TRANSACTION_TYPE);
    }
    private getStockTransactionTypes() {
        let skipTransactionTypes = [TermGroup_StockTransactionType.Correction, TermGroup_StockTransactionType.Reserve, TermGroup_StockTransactionType.StockTransfer];
        this.selectableStockTransactionTypes = [];
       
        return this.coreService.getTermGroupContent(TermGroup.StockTransactionType, true, false, false).then(data => {
            data.forEach(d => {
                if (!skipTransactionTypes.find(f => f == d.id))
                    this.selectableStockTransactionTypes.push(d);
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
        return this.reportDataService.getStocksDict(true).then(data => {
            this.selectableInventoryBalanceList = _.sortBy(data, 'name');
        });
    }

    private getCodeList() {
        this.selectableCodeList = [];
        return this.reportDataService.getStockPlaces(true, 0).then(data => {
            data.forEach(filter => {
                this.selectableCodeList.push({ id: filter.stockShelfId, name: filter.name });
            });
            this.selectableCodeList.push({ id: 0, name: '' });
            this.selectableCodeList = _.sortBy(this.selectableCodeList, 'name');
        });
    }

    public onDateRangeSelected(dateRange: DateRangeSelectionDTO) {
        dateRange.useMinMaxIfEmpty = true;
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_DATE_RANGE, dateRange);
    }

    public onSortOrderSelectionChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_SORT_ORDER, selection);
    }

    public onStockTransactionTypesChanged(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_STOCK_TRANSACTION_TYPE, selection);
    }

    public articleNumberChangedFrom(selection: ITextSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_FROM, selection);
        if (!this.articleNumberTo || !this.articleNumberTo.text) {
            this.articleNumberHandler = true;
            this.articleNumberTo = new TextSelectionDTO(selection.text);
        }
        this.addInventoryBalanceListFilter();
    }

    private articleNumberChangedTo(selection: ITextSelectionDTO) {
        if (this.articleNumberHandler) {
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVOICE_NUMBER_TO, selection);
            this.articleNumberHandler = false;
            this.articleNumberTo = new TextSelectionDTO(selection.text);
        } else {
            this.articleNumberHandler = true;
        }
    }

    private inventoryBalanceChangedFrom(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_BALANCE_FROM, selection);
        if (!this.inventoryBalanceTo || !this.inventoryBalanceTo.id || this.inventoryBalanceTo.id == 0) {
            this.inventoryBalanceHandler = true;
            this.inventoryBalanceTo = new IdSelectionDTO(selection.id);
        }
        this.addInventoryBalanceListFilter();
    }

    private inventoryBalanceChangedTo(selection: IIdSelectionDTO) {
        if (this.inventoryBalanceHandler) {   
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_INVENTORY_BALANCE_TO, selection);
            this.inventoryBalanceHandler = false;
            this.inventoryBalanceTo = new IdSelectionDTO(selection.id);
        } else {
            this.inventoryBalanceHandler = true;
        }
    }

    private codeSeriesChangedFrom(selection: IIdSelectionDTO) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CODE_SERIES_FROM, selection);
        if (!this.codeSeriesTo || !this.codeSeriesTo.id || this.codeSeriesTo.id == 0) {
            this.codeSeriesHandler = true;
            this.codeSeriesTo = new IdSelectionDTO(selection.id);
        }
    }

    private codeSeriesChangedTo(selection: IIdSelectionDTO) {        
        if (this.codeSeriesHandler) {
            this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_CODE_SERIES_TO, selection);
            this.codeSeriesHandler = false;
            this.codeSeriesTo = new IdSelectionDTO(selection.id);
        } else {
            this.codeSeriesHandler = true;
        }
    }

    private onProductGroupSelectedFrom(selection: any) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PRODUCT_GROUPS_FROM, new TextSelectionDTO(selection ? selection.value : ''));
    }

    private onProductGroupSelectedTo(selection: any) {
        this.selections.upsert(Constants.REPORTMENU_SELECTION_KEY_PRODUCT_GROUPS_TO, new TextSelectionDTO(selection ? selection.value : ''));
    }

    private addInventoryBalanceListFilter() {
        const row = new NamedFilterRange(this.selectableInventoryBalanceList); 
        if (this.rangeInventoryBalanceListFilters.length === 0) {
            row.selectedSelection = this.selectableInventoryBalanceList[0];
            this.rangeInventoryBalanceListFilters.push(row);
            this.selectableInventoryBalanceList.push({id:0,name:" "});

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