import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { AccountDistributionRowDTO } from "../../Models/AccountDistributionRowDTO";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ToolBarUtility, ToolBarButton, ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { NumberUtility } from "../../../Util/NumberUtility";
import { AccountDimSmallDTO } from "../../Models/AccountDimDTO";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { Constants } from "../../../Util/Constants";

export class DistributionRowsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl('Common/Directives/DistributionRows/Views/DistributionRows.html'),
            scope: {
                accountingRows: '=',
                showAmountSummary: '@',
                difference: '=?'
            },
            restrict: 'E',
            replace: true,
            controller: DistributionRowsController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

export class DistributionRowsController {

    private accountingRows: AccountDistributionRowDTO[];
    public accountDims: AccountDimSmallDTO[];
    private terms: any;
    private accountBalances: any;
    private showAmountSummary: boolean;
    private amountSummary: Summary;
    private difference: number;

    // Grid
    private soeGridOptions: ISoeGridOptionsAg;

    // ToolBar
    protected buttonGroups = new Array<ToolBarButtonGroup>();

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        this.$scope.$on('rowsChanged', (e, a, container) => {
            this.soeGridOptions.setData(this.accountingRows);
            this.updateAmountSummary();
        });
        this.$scope.$on('stopEditing', (e, a) => {
            this.soeGridOptions.stopEditing(false);
            this.$timeout(() => {
                a.functionComplete();
            }, 100)
        });
    }

    public $onInit() {
        this.soeGridOptions = new SoeGridOptionsAg("Common.Directives.DistributionRows", this.$timeout);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.setMinRowsToShow(8);
        this.soeGridOptions.customTabToCellHandler = (params) => this.navigateToNextCell(params);

        this.showAmountSummary = <any>this.showAmountSummary === 'true';

        this.$q.all([this.loadTerms(), this.loadAccounts()]).then(() => {
            this.setupCustomToolBar();
            this.gridAndDataIsReady();
        });
    }

    private getDim1ColumnIndex() {
        return this.soeGridOptions.getColumnIndex('dim1Nr');
    }

    protected setupCustomToolBar() {
        var isLoadingAccounts = false;
        this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.accountingrows.reloadaccounts", "common.accountingrows.reloadaccounts", IconLibrary.FontAwesome, "fa-sync", () => {
            isLoadingAccounts = true;
            this.loadAccounts().then(() => isLoadingAccounts = false);
        }, () => isLoadingAccounts)));

        this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.newrow", "common.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
            this.addRow();
            //this.soeGridOptions.focusRowByRow(row, this.getDim1ColumnIndex());
        })));
    }

    private loadTerms(): ng.IPromise<any> {
        // Columns
        var keys: string[] = [
            "common.number",
            "common.date",
            "common.text",
            "common.debit",
            "common.credit",
            "common.balance",
            "common.rownr",
            "economy.accounting.voucher.voucherseries",
            "economy.accounting.voucher.vatvoucher",
            "core.deleterow",
            "common.accountingrows.missingaccount",
            "common.accountingrows.invalidaccount",
            "economy.accounting.accountdistributionauto.keepaccount",
            "economy.accounting.accountdistributionauto.calculaterownbr",
            "economy.accounting.accountdistributionauto.samesign",
            "economy.accounting.accountdistributionauto.oppositesign",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private parceDecimalAndResetSameOppositeIfNeeded(colDef, entity) {

        if (colDef.field === 'oppositeBalance' && entity.oppositeBalance) {
            entity.oppositeBalance = NumberUtility.parseDecimal(entity.oppositeBalance.toString());
        }

        if (colDef.field === 'sameBalance' && entity.sameBalance) {
            entity.sameBalance = NumberUtility.parseDecimal(entity.sameBalance.toString());
        }

        if (colDef.field === 'oppositeBalance' && entity.oppositeBalance && entity.sameBalance > 0) {
            entity.sameBalance = 0;
        }
        if (colDef.field === 'sameBalance' && entity.sameBalance && entity.oppositeBalance > 0) {
            entity.oppositeBalance = 0;
        }
    }

    private validateAccountingRow(row: AccountDistributionRowDTO) {

        this.accountDims.forEach((ad, i) => {
            var prop = 'dim' + (i + 1) + 'Nr';

            var val = row[prop];
            var mandatory = row['dim' + (i + 1) + 'Mandatory'];

            if (!val && mandatory) {
                row['dim' + (i + 1) + 'Error'] = this.terms['common.accountingrows.missingaccount'];
            } else if (val && !row['dim' + (i + 1) + 'Name']) { //no name means we couldnt find the account name, which means this is invalid.
                row['dim' + (i + 1) + 'Error'] = this.terms['common.accountingrows.invalidaccount'];
            } else {
                row['dim' + (i + 1) + 'Error'] = null;
            }
        });

        row["selectOptions"] = this.giveMeTheSameListEveryTimeBasedOnInput(row.rowNbr);
    }

    private loadAccounts(): ng.IPromise<any> {
        return this.accountingService.getAccountDimsSmall(false, false, true, false).then(x => {
            this.accountDims = x;
        });
    }

    private gridAndDataIsReady() {
        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef) => {
            this.validateAccountingRow(entity);

            if (this.showAmountSummary)
                this.updateAmountSummary();

            this.parceDecimalAndResetSameOppositeIfNeeded(colDef, entity);

            this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
        }));
        this.soeGridOptions.subscribe(events);

        this.accountDims.forEach((ad, i) => {

            if (!ad.accounts)
                ad.accounts = [];

            if (ad.accounts.length === 0 || ad.accounts[0].accountId !== 0)
                (<any[]>ad.accounts).unshift({ accountId: 0, accountNr: '', name: '', numberName: '' });

            (<any>ad.accounts).push({ accountNr: '*', name: this.terms['economy.accounting.accountdistributionauto.keepaccount'], numberName: this.terms['economy.accounting.accountdistributionauto.keepaccount'] });
            
        });
        this.setAccountingRowsBaseAccountName();

        this.setupGridColumns();

        this.accountingRows.forEach(ar => this.validateAccountingRow(ar));

        this.soeGridOptions.setData(this.accountingRows);

        this.setupWatchers();
    }
    private setAccountingRowsBaseAccountName() {
        this.accountDims.forEach((ad, i) => {            
            this.accountingRows.forEach(ar => {
                if (ar['dim' + (i + 1) + 'Nr'] === '*')
                    ar['dim' + (i + 1) + 'Name'] = this.terms['economy.accounting.accountdistributionauto.keepaccount'];
            });

        });
    }

    //called AFTER all data is loaded.
    private setupWatchers() {
        this.$scope.$watch(() => this.accountingRows, () => {
            this.setAccountingRowsBaseAccountName();
            this.accountingRows.forEach(ar => this.validateAccountingRow(ar));
            this.accountingRows.sort((a, b) => (a.rowNbr > b.rowNbr) ? 1 : -1);
            this.soeGridOptions.setData(this.accountingRows);
            this.updateAmountSummary();
        });
    }

    private setupGridColumns() {
        this.soeGridOptions.addColumnNumber("rowNbr", this.terms["common.rownr"], 100, { enableHiding: false });
        this.accountDims.forEach((ad, i) => {

            let index = i + 1;

            const field = "dim" + index + "Nr";
            const secondRowField = "dim" + index + "Name";
            const errorField = "dim" + index + "Error";
            const editable = (data) => {
                var disabled = data["dim" + index + "Disabled"];

                return !disabled;
            };

            const onCellChanged = ({ data }) => {
                this.onAccountingDimChanged(data, index);
            };

            const allowNavigateFrom = (value, data) => {
                return this.allowNavigationFromTypeAhead(value, data, index);
            };

            const col = this.soeGridOptions.addColumnTypeAhead(field, ad.name, null, {
                editable: editable.bind(this),
                error: errorField,
                secondRow: secondRowField,
                onChanged: onCellChanged.bind(this),
                typeAheadOptions: {
                    source: (filter) => this.filterAccounts(i, filter),
                    updater: null,
                    allowNavigationFromTypeAhead: allowNavigateFrom.bind(this),
                    displayField: "numberName",
                    dataField: "accountNr",
                    minLength: 0
                }
            }, {
                dimIndex: index,
                });
        });

        this.soeGridOptions.addColumnSelect("calculateRowNbr", this.terms["economy.accounting.accountdistributionauto.calculaterownbr"], null,
        {
            selectOptions: [], 
            dynamicSelectOptions: {
                idField: "id",
                displayField: "value",
                options: "selectOptions",
            },
            enableHiding: false,
            editable: true,
            displayField: "calculateRowNbr",
            dropdownIdLabel: "id",
            dropdownValueLabel: "value",
            populateFilterFromGrid: true,
            //onChanged: this.typeChanged.bind(this),
        });
        /*var colDef = this.soeGridOptions.addColumnSelect('calculateRowNbr', this.terms["economy.accounting.accountdistributionauto.calculaterownbr"], "10%", null, false, true);

        colDef.cellTemplate = '<div class="ngCellText gridCellAlignLeft" ng-class="col.colIndex()"><span ng-cell-text ng-bind="MODEL_COL_FIELD || \'\'"></span></div>';

        colDef.editableCellTemplate = '<div>' +
            '<form name = "inputForm">' +
            '<select ng-class="\'colt\' + col.uid" ui-grid-edit-dropdown-with-focus-delay ng-model="MODEL_COL_FIELD" ' +
            'ng-options="nr.id as nr.value for nr in grid.appScope.directiveCtrl.getDropDownValuesForNumberRows(row.entity)">' +
            '</select>' +
            '</form>' +
            '</div>';*/

        this.soeGridOptions.addColumnNumber("sameBalance", this.terms["economy.accounting.accountdistributionauto.samesign"], null, { decimals: 2, enableHiding: false, editable: true, maxDecimals: 5 });
        this.soeGridOptions.addColumnNumber("oppositeBalance", this.terms["economy.accounting.accountdistributionauto.oppositesign"], null, { decimals: 2, enableHiding: false, editable: true, maxDecimals: 5 });
        this.soeGridOptions.addColumnText("description", this.terms["common.text"],null, { editable: true });
        this.soeGridOptions.addColumnDelete(this.terms["core.deleterow"], this.onDeleteEvent.bind(this));

        this.soeGridOptions.finalizeInitGrid();

        if (this.showAmountSummary) {
            this.amountSummary = new Summary();
            this.updateAmountSummary();
        }
    }

    private giveMeTheSameListEveryTimeBasedOnInput = _.memoize(function (nr) {
        var arr = [{ id: 0, value: '' }];

        for (var i = 1; i < nr; i++) {
            arr.push({ id: i, value: i.toString() });
        }

        return arr;
    });

    private findAccount(entity, dimIndex) {
        var idToFind = entity['dim' + dimIndex + 'Nr'];

        if (!idToFind)
            return null;

        var found = this.accountDims[dimIndex - 1].accounts.filter(acc => acc.accountNr === idToFind);

        if (found.length) {
            var acc = found[0];
            return acc;
        }

        return null;
    }

    protected onAccountingDimChanged(data, dimIndex) {
        var acc = this.findAccount(data, dimIndex);
        data['dim' + dimIndex + 'Id'] = acc ? acc.accountId : 0;
        data['dim' + dimIndex + 'Name'] = acc ? acc.name : "";
        data['dim' + dimIndex + 'KeepSourceRowAccount'] = data['dim' + dimIndex + 'Nr'] === '*';

        this.soeGridOptions.refreshRows(data);
        this.messagingService.publish(Constants.EVENT_ACCOUNTING_ROWS_MODIFIED, {});
    }

    protected getSecondRowValue(entity, colDef) {
        var acc = this.findAccount(entity, colDef);

        if (acc) {
            return acc.name;
        }

        return null;
    }

    protected allowNavigationFromTypeAhead(value, entity, dimIndex) {

        var currentValue = value;

        if (!currentValue) //if no value, allow it.
            return true;

        var valueHasMatchingAccount = this.accountDims[dimIndex - 1].accounts.filter(acc => acc.accountNr === currentValue);
        if (valueHasMatchingAccount.length) //if there is a value and it is valid, allow it.
            return true;

        return false;
    }

    public filterAccounts(dimIndex, filter) {
        return _.orderBy(this.accountDims[dimIndex].accounts.filter(acc => {
            if (parseInt(filter))
                return acc.accountNr.startsWithCaseInsensitive(filter);

            return acc.accountNr.startsWithCaseInsensitive(filter) || acc.name.contains(filter);
        }), 'accountNr');
    }

    private addRow() {
        var row = new AccountDistributionRowDTO();

        for (var i = 1, len = this.accountDims.length; i < len + 1; i++) {
            row['dim' + i + 'Id'] = 0;
            row['dim' + i + 'Name'] = "";
        };

        var maxRowNo = this.accountingRows.reduce((max, current) => Math.max(max, current.rowNbr), 0);
        row.rowNbr = maxRowNo + 1;

        this.accountingRows.push(row);
        this.validateAccountingRow(row);
        this.soeGridOptions.setData(this.accountingRows);

        return row;
    }

    protected onDeleteEvent(row: any) {
        _.remove(this.accountingRows, row);
        this.soeGridOptions.setData(this.accountingRows);
        this.soeGridOptions.reNumberRows();

        _.forEach(this.accountingRows, (row) => {
            this.validateAccountingRow(row);
        });

        this.updateAmountSummary();

        this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
    }

    //protected navigateToNextCell(coldef: uiGrid.IColumnDef) {
    protected navigateToNextCell(params: any): { rowIndex: number, column: any } {
        const { nextCellPosition, previousCellPosition, backwards } = params;
        let nextColumnCaller: (column: any) => any = backwards ? this.soeGridOptions.getPreviousVisibleColumn : this.soeGridOptions.getNextVisibleColumn;
        let { rowIndex, column } = nextCellPosition;
        let row: any = this.soeGridOptions.getVisibleRowByIndex(rowIndex).data;
        if (column.colId === 'delete') {
            const nextRowResult = this.soeGridOptions.getNextRow(row);
            if (nextRowResult && nextRowResult.rowNode) {
                this.soeGridOptions.startEditingCell(nextRowResult.rowNode.data, this.soeGridOptions.getColumnByField('dim1Nr'));
                return null;
            } else {
                this.soeGridOptions.stopEditing(false);
                this.addRow();

                const nextRowResult = this.soeGridOptions.getNextRow(row);
                if (nextRowResult && nextRowResult.rowNode) {
                    this.soeGridOptions.startEditingCell(nextRowResult.rowNode.data, this.soeGridOptions.getColumnByField('dim1Nr'));
                    return null;
                }
                return null;
            }
        }
        else {
            return { rowIndex, column };
        }
        /*var row = this.soeGridOptions.getCurrentRow();

        var colDefs = this.soeGridOptions.getColumnDefs();

        for (var i = 0; i < colDefs.length; i++) {
            if (colDefs[i] === coldef) {
                while (i < colDefs.length - 1) {
                    i++;

                    if (colDefs[i].enableCellEdit) {
                        if (i !== colDefs.length - 1) {
                            this.soeGridOptions.scrollToFocus(row, i);
                            return;
                        }
                    }
                }
            }
        }

        var nextRow = this.findNextRow(row);
        if (!nextRow)
            nextRow = this.addRow();

        this.soeGridOptions.scrollToFocus(<any>nextRow, 1);*/
    }

    private updateAmountSummary() {
        var data = this.accountingRows;

        this.amountSummary.reset();

        data.forEach(r => {
            this.amountSummary.sameSum += parseFloat(<any>r.sameBalance) || 0;
            this.amountSummary.oppositeSum += parseFloat(<any>r.oppositeBalance) || 0;
        });

        this.amountSummary.calculateDiff();

        this.difference = this.amountSummary.difference;
    }

    private startsWith(value, startsWith) {
        if (!startsWith)
            return true;

        return value.substr(0, startsWith.length).toLowerCase() === startsWith.toLowerCase();
    }
}

class Summary {
    sameSum: number;
    oppositeSum: number;
    difference: number;

    public calculateDiff() {
        this.difference = +(this.sameSum - this.oppositeSum).toFixed(2);
    }

    public reset() {
        this.difference = 0;
        this.sameSum = 0;
        this.oppositeSum = 0;
    }

    constructor() {
        this.reset();
    }
}