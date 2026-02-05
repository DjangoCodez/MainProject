import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IGridControllerFlowHandler } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IAccountingService } from "../../../../Shared/Economy/Accounting/AccountingService";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { BudgetHeadFlattenedDTO, BudgetRowFlattenedDTO } from "../../../../Common/Models/BudgetDTOs";
import { PreviousPeriodResultController } from "./PreviousPeriodResultController";
import { IProgressHandler } from "../../../../Core/Handlers/ProgressHandler";
import { IToolbar } from "../../../../Core/Handlers/Toolbar";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { Feature } from "../../../../Util/CommonEnumerations";
import { TypeAheadOptionsAg, IColumnAggregations } from "../../../../Util/SoeGridOptionsAg";
import { ColumnAggregationFooterGrid } from "../../../../Util/ag-grid/ColumnAggregationFooterGrid";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { IGridHandlerAg } from "../../../../Core/Handlers/GridHandlerAg";
import { Guid } from "../../../../Util/StringUtility";
import { DistributionCodeHeadDTO } from "../../../../Common/Models/DistributionCodeHeadDTO";
import { GridEvent } from "../../../../Util/SoeGridOptions";

export class BudgetGridDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("Economy/Accounting/Budget/Views/budgetGrid.html"),
            controller: BudgetGridController,
            controllerAs: "ctrlDirective",
            bindToController: true,
            scope: {
                budgetHead: "=",
                numberOfPeriods: "=",
                accountDims: "=",
                distributionCodesDict: "=",
                distributionCodes: "=",
                useDim2: "=",
                useDim3: "=",
                distributionCodeId: "=",
                dim2Id: "=",
                dim3Id: "=",
                showPassedPeriodsDialog: "=",
                isDisabled: "=",
                guid : "=",

            },
            link(scope: ng.IScope, element: JQuery, attributes: ng.IAttributes, ngModelController: any) {
                scope.$watch(() => (ngModelController.budgetHead), (newValue, oldValue, scope) => {
                    if (newValue) {
                        if (!oldValue) 
                            ngModelController.updateBudgetRows();
                    }
                }, true);
                scope.$watch(() => (ngModelController.numberOfPeriods), (newVAlue, oldvalue, scope) => {
                    if (newVAlue && ngModelController.budgetHead) {
                        ngModelController.setNumberOfPeriods(newVAlue);
                    }
                });
                scope.$watch(() => (ngModelController.useDim2), (newVAlue, oldvalue, scope) => {
                    if (newVAlue != null) {
                        ngModelController.showColumnDim2();
                    }
                });
                scope.$watch(() => (ngModelController.useDim3), (newVAlue, oldvalue, scope) => {
                    if (newVAlue != null) {
                        ngModelController.showColumnDim3();
                    }
                });
            },
            restrict: "E",
            replace: true
        }
    }
}

export class BudgetGridController implements ICompositionGridController {
    modal: angular.ui.bootstrap.IModalService;

    public guid: Guid;

    // Data
    budgetHead: BudgetHeadFlattenedDTO;

    // Lookups 
    accountDims;
    distributionCodesDict;
    distributionCodes: DistributionCodeHeadDTO[];

    terms: any;
    numberOfPeriods: number;
    public sumCaluculations: any[] = [];
    useDim2: boolean;
    useDim3: boolean;
    distributionCodeId: number;
    dim2Id: number;
    dim3Id: number;
    showPassedPeriodsDialog: boolean;
    isDisabled: boolean;
    gridSetupComplete: boolean = false;

    progress: IProgressHandler;
    toolbar: IToolbar;
    gridAg: IGridHandlerAg;
    grid: IGridHandler;
    modifyPermission: boolean;
    readPermission: boolean;
    private flowHandler: IGridControllerFlowHandler;

    // Aligned grid
    footerGrid: ColumnAggregationFooterGrid;

    //@ngInject
    constructor(private accountingService: IAccountingService,
        private translationService: ITranslationService,
        protected messagingService: IMessagingService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        $uibModal) {

        this.modal = $uibModal;
        this.gridAg = gridHandlerFactory.create("Economy.Accounting.Budget.Edit", "agGrid");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    //this.messagingHandler.publishActivateAddTab();
                }
                
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
                .onLoadGridData(() => this.setRowsWithoutDeleted())

        this.flowHandler.start({ feature: Feature.Economy_Accounting_Budget_Edit, loadReadPermissions: true, loadModifyPermissions: true });

        this.$scope.$on('resultLoaded', (e, a) => {
            if (a.guid === this.guid)
                this.updateBudgetRows();
        });

        this.$scope.$on('resetRows', (e, a) => {
            if (a.guid === this.guid)
                this.setRowsWithoutDeleted();
        });

        //workaround to make it possible to select TypeheadDD when rowcount > gridview....
        //var gridOptions = (this.grid.options as any).gridOptions;
        //gridOptions.rowHeight = 25;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createEmpty();
        if (this.modifyPermission) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.newrow", "common.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
                this.addRow();
            })));
        }

    }

    onInit(parameters: any) {
    }

    edit(row) {
        /*
        // Send message to TabsController
        if (this.doubleClickToEdit && (this.readOnlyPermission || this.modifyPermission)) {
            this.messagingHandler.publishEditRow(row);
        }
        */
    }

    private addRow(): { rowIndex: number, newRow: any } {
        var newRow = new BudgetRowFlattenedDTO();
        newRow.totalAmount = 0;
        newRow.isDeleted = false;
        newRow.budgetRowId = undefined;
        newRow.budgetRowNr = this.budgetHead.rows.length - 1;
        for (var i = 1; i < this.numberOfPeriods + 1; i++) {
            newRow['amount' + i] = 0;
        }
        if (this.distributionCodeId) {
            var obj = (_.filter(this.distributionCodesDict, { id: this.distributionCodeId }))[0];
            if (obj) {
                newRow.distributionCodeHeadId = <number>obj["id"];
                newRow.distributionCodeHeadName = obj["name"];
            }
        }

        // Intialize account names
        newRow.dim1Nr = "";
        newRow.dim1Name = "";
        newRow.dim2Nr = "";
        newRow.dim2Name = "";
        newRow.dim3Nr = "";
        newRow.dim3Name = "";

        if (this.dim2Id) {
            var dim2 = (_.filter(this.accountDims[1].accounts, { accountId: this.dim2Id }))[0];
            if (dim2) {
                newRow.dim2Id = this.dim2Id;
                newRow.dim2Nr = dim2["accountNr"];
                newRow.dim2Name = dim2["name"];
            }
        }
        if (this.dim3Id) {
            var dim3 = (_.filter(this.accountDims[2].accounts, { accountId: this.dim3Id }))[0];
            if (dim3) {
                newRow.dim3Id = this.dim3Id;
                newRow.dim3Nr = dim3["accountNr"];
                newRow.dim3Name = dim3["name"];
            }
        }

        const rowIndex = this.gridAg.options.addRow(newRow);
        this.budgetHead.rows.push(newRow);

        //this.setRowsWithoutDeleted();

        this.gridAg.options.startEditingCell(newRow, this.gridAg.options.getColumnByField('dim1Nr'));

        return { rowIndex, newRow };
    }

    public setupGrid(): void {
        this.initRowsGrid();
        //this.grid.setupTypeAhead();
        var keys: string[] = [
            "core.delete",
            "common.sum",
            "economy.accounting.distributioncode.distributioncode",
            "economy.accounting.budget.getresultperiod",
        ];
        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            // Set name
            this.gridAg.options.setName("budgetGrid");

            // Disable filtering
            this.gridAg.options.enableFiltering = false;

            this.gridAg.options.customTabToCellHandler = (params) => this.navigateToNextCell(params);

            // Create columns
            this.setupGridColumns();

            this.gridAg.finalizeInitGrid("economy.accounting.budget.budget", true);
        });
    }

    private setupGridColumns() {
        if (!this.terms)
            return;

        // Clear column definitions
        this.gridAg.options.resetColumnDefs();
        
        this.accountDims.forEach((ad, i) => { //TODO: its wrong to use the index here since they can come out of order?
            if (ad.accountDimNr <= 3) {
                var options = new TypeAheadOptionsAg();
                options.source = (filter) => this.filterAccounts(i, filter);
                options.displayField = "numberName"
                options.dataField = "accountNr";
                options.minLength = 0;
                options.delay = 0;
                options.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, entity, colDef);
                var accountCol = this.gridAg.addColumnTypeAhead("dim" + (i + 1) + "Nr", ad.name, null, { secondRow: 'dim' + (i + 1) + 'Name', error: 'dim' + (i + 1) + 'Error', typeAheadOptions: options, displayField: "numberName", editable: this.isCellEditable.bind(this), suppressSorting: true });
                accountCol['additionalData'] = { dimIndex: i + 1, dimNr: ad.accountDimNr };
            }
        });

        var distributionCodeOptions = new TypeAheadOptionsAg();
        distributionCodeOptions.source = (filter) => this.filterDistributionCodes(filter);
        distributionCodeOptions.minLength = 0;
        distributionCodeOptions.delay = 0;
        distributionCodeOptions.displayField = "name"
        distributionCodeOptions.dataField = "name";
        distributionCodeOptions.allowNavigationFromTypeAhead = () => { return true };
        this.gridAg.addColumnTypeAhead("distributionCodeHeadName", this.terms["economy.accounting.distributioncode.distributioncode"], null, { typeAheadOptions: distributionCodeOptions, displayField: "name", editable: this.isCellEditable.bind(this) });
 
        this.gridAg.addColumnNumber("totalAmount", this.terms["common.sum"], null, { enableHiding: false, decimals: 2, suppressSorting: true, editable: this.isCellEditable.bind(this) });
        var aggregations = { "totalAmount": "sum" };
        if (this.budgetHead) {
            for (var i = 1; i <= this.budgetHead.noOfPeriods; i++) {
                this.gridAg.addColumnNumber("amount" + i.toString(), i.toString(), null, { enableHiding: false, decimals: 2, suppressSorting: true, editable: this.isCellEditable.bind(this) });
                aggregations["amount" + i.toString()] = "sum";
            }
        }

        _.forEach(this.gridAg.options.getColumnDefs(), (colDef: any) => {
            colDef.enableFiltering = false;
            colDef.enableSorting = false;
            colDef.cellEditableCondition = (scope: any) => { return this.isDisabled === false }
            if (this.modifyPermission) {
                colDef.enableCellEdit = true;
            }

        });
        this.gridAg.addColumnDelete(this.terms["core.delete"], this.deleteBudgetRow.bind(this), null, () => !this.isDisabled );

        if (this.footerGrid) {
            this.footerGrid.setColumnDefs(this.gridAg.options.getColumnDefs(), aggregations);
        }
        else {
            this.footerGrid = this.gridAg.options.addAggregatedFooterRow("#sum-footer-grid", aggregations as IColumnAggregations);
        }

        this.showColumnDim2();
        this.showColumnDim3();

        this.gridAg.options.finalizeInitGrid(true);

        this.updateBudgetRows();
    }

    private isCellEditable(row) {
        return !this.isDisabled && this.modifyPermission;
    }

    private deleteBudgetRow(row: BudgetRowFlattenedDTO) {
        row.isDeleted = true;
        row.isModified = true;
        if (!row.budgetRowId) {
            _.remove(this.budgetHead.rows, row => row.isDeleted && !row.budgetRowId);
        }

        this.setParentAsModified();

        this.setRowsWithoutDeleted();
    }

    private setRowsWithoutDeleted() {
        if (this.budgetHead) {
            this.gridAg.options.setData(_.filter(this.budgetHead.rows, { isDeleted: false }));
        }
    }

    private findAccount(entity, colDef) {
        var idToFind = entity['dim' + colDef.soeData.additionalData.dimIndex + 'Nr'];
        if (!idToFind)
            return null;

        var found = this.accountDims[colDef.soeData.additionalData.dimIndex - 1].accounts.filter(acc => acc.accountNr === idToFind);
        if (found.length) {
            var acc = found[0];
            return acc;
        }

        return null;
    }

    protected onBlur(entity, colDef) {
        var acc = this.findAccount(entity, colDef);
        if (acc) {
            entity['dim' + colDef.soeData.additionalData.dimIndex + 'Id'] = acc.accountId;
            if (colDef.soeData.additionalData.dimIndex === 0) {
                entity.accountId = acc.accountId;
            }
        } else {
            entity['dim' + colDef.soeData.additionalData.dimIndex + 'Id'] = 0;
        }
    }

    protected navigateToNextCell(params: any): { rowIndex: number, column: any } {
        const { nextCellPosition, previousCellPosition, backwards } = params;
        if (nextCellPosition) {
            let nextColumnCaller: (column: any) => any = backwards ? this.gridAg.options.getPreviousVisibleColumn : this.gridAg.options.getNextVisibleColumn;
            let { rowIndex, column } = nextCellPosition;
            let row: any = this.gridAg.options.getVisibleRowByIndex(rowIndex).data;
            if (column.colId === 'delete') {
                const nextRowResult = this.gridAg.findNextRowInfo(row, true);

                if (nextRowResult && nextRowResult.rowNode) {
                    this.gridAg.options.startEditingCell(nextRowResult.rowNode.data, this.gridAg.options.getColumnByField('dim2Nr'));
                    return null;
                } else {
                    this.gridAg.options.stopEditing(false);
                    this.addRow();
                    return null;
                }
            } else {
                return { rowIndex, column };
            }
        }
    }

    private validateAccountingRow(row: BudgetRowFlattenedDTO) {

        this.accountDims.forEach((ad, i) => {
            var prop = 'dim' + (i + 1) + 'Nr'; //TODO:still wrong to use i, fix it.

            var val = row[prop];
            var mandatory = row['dim' + (i + 1) + 'Mandatory'];

            if (!val && mandatory) {
                row['dim' + (i + 1) + 'Error'] = this.terms['common.accountingrows.missingaccount'];
            } else if (!row['dim' + (i + 1) + 'Name']) { //no name means we couldnt find the account name, which means this is invalid.
                row['dim' + (i + 1) + 'Error'] = this.terms['common.accountingrows.invalidaccount'];
            } else {
                row['dim' + (i + 1) + 'Error'] = null;
            }
        });
    }

    public distributionChanged(row): void {
        var obj = (_.filter(this.distributionCodesDict, { id: row.distributionCodeHeadId }))[0];
        if (obj) {
            row.distributionCodeHeadId = obj["id"];
            row.distributionCodeHeadName = obj["name"];
        }
    }

    public filterAccounts(dimIndex, filter) {
        return _.orderBy(this.accountDims[dimIndex].accounts.filter(acc => {
            if (parseInt(filter))
                return acc.accountNr.startsWithCaseInsensitive(filter);

            return acc.accountNr.startsWithCaseInsensitive(filter) || acc.name.contains(filter);
        }), 'accountNr');
    }

    protected filterDistributionCodes(filter) {
        return this.distributionCodesDict.filter(distributionCode => {
            return distributionCode.name.contains(filter);
        });
    }

    private openPreviousPeriodResultDialog(row: BudgetRowFlattenedDTO) {
        var result: any = [];
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getViewUrl("PreviousPeriodResult.html"),
            controller: PreviousPeriodResultController,
            controllerAs: "ctrl",
            resolve: {
                result: () => result,
                terms: () => this.terms
            }
        }
        this.modal.open(options).result.then((result: any) => {
            if (result && result.getPreviousPeriodResult)
                this.$scope.$emit("getPreviousPeriodResult", row);
        });
    }

    private startsWith(value, startsWith) {
        return value.substr(0, startsWith.length).toLowerCase() === startsWith.toLowerCase();
    }

    protected getSecondRowValue(entity, colDef) {
        var idToFind = entity['dim' + colDef.soeData.additionalData.dimIndex + 'Nr'];

        if (!idToFind)
            return null;

        //TODO: this use of dimindex is wrong, since they dont need to be in order or sequence
        var found = this.accountDims[colDef.soeData.additionalData.dimIndex - 1].accounts.filter(acc => acc.accountNr === idToFind);

        if (found.length) {
            var acc = found[0];
            return acc.name;
        }

        return null;
    }

    protected allowNavigationFromTypeAhead(value, entity, colDef) {
        if (!value)
            return true;

        var accountDim = _.find(this.accountDims, a => a.accountDimNr === colDef['additionalData'].dimNr);
        if (accountDim) {
            var valueHasMatchingAccount = accountDim.accounts.filter(acc => acc.accountNr === value);

            if (valueHasMatchingAccount.length) //if there is a value and it is valid, allow it.
                return true;
        }
        return false;
    }

    private initRowsGrid() {
        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.CellFocused, (rowIndex, column, rowPinned, forceBrowserFocus) => { this.cellFocused(rowIndex, column ? column.colDef : null); }));
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity: BudgetRowFlattenedDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) => this.afterCellEdit(entity, colDef, newValue, oldValue)));
        this.gridAg.options.subscribe(events);
    }

    private cellFocused(rowIndex, colDef: uiGrid.IColumnDef) {
        if (this.isDisabled && colDef) {
            let row = this.gridAg.options.getVisibleRowByIndex(rowIndex);
            if (row) {
                var column = this.gridAg.options.getColumnByField(colDef.field);
                this.gridAg.options.startEditingColumn(column);
            }
        }
    }

    private afterCellEdit(entity: BudgetRowFlattenedDTO, colDef: uiGrid.IColumnDef, newValue, oldValue){
    if (newValue != oldValue) {
        if (colDef.field === "")
            return;

        this.validateAccountingRow(entity);
        if (colDef === this.gridAg.options.getColumnDefs()[0] && this.showPassedPeriodsDialog && entity.showModalGetPreviousPeriodResult) {
            entity.showModalGetPreviousPeriodResult = false;

            this.$timeout(() => { this.openPreviousPeriodResultDialog(entity) }, 20);
        }
        if (colDef['additionalData']) {
            var accountDim = _.find(this.accountDims, a => a.accountDimNr === colDef['additionalData'].dimNr);
            if (accountDim) {
                var account = accountDim.accounts.find(acc => acc.accountNr === newValue);
                if (account && account.accountId > 0) {
                    if (colDef['additionalData'].dimIndex === 1)
                        entity.accountId = account.accountId;

                    entity['dim' + colDef['additionalData'].dimIndex + 'Id'] = account.accountId;
                    entity['dim' + colDef['additionalData'].dimIndex + 'Nr'] = account.accountNr;
                    entity['dim' + colDef['additionalData'].dimIndex + 'Name'] = account.name;
                }
                else {
                    if (colDef['additionalData'].dimIndex === 1)
                        entity.accountId = undefined;

                    entity['dim' + colDef['additionalData'].dimIndex + 'Id'] = entity.accountId = undefined;
                    entity['dim' + colDef['additionalData'].dimIndex + 'Id'] = entity.accountId = undefined;
                    entity['dim' + colDef['additionalData'].dimIndex + 'Nr'] = "";
                    entity['dim' + colDef['additionalData'].dimIndex + 'Name'] = "";
                }
            }
        }
        if (colDef.field === "distributionCodeHeadName") {
            var distributionCode: DistributionCodeHeadDTO = _.find(this.distributionCodes, { name: newValue });
            if (distributionCode) {
                entity.distributionCodeHeadId = distributionCode.distributionCodeHeadId as number;
                entity.distributionCodeHeadName = distributionCode.name as string;
                if (entity.totalAmount) {
                    for (var i = 1; i < this.numberOfPeriods + 1; i++) {
                        var periodC = (distributionCode.periods[i - 1]);
                        if (periodC) {
                            entity['amount' + i] = entity.totalAmount * periodC.percent / 100;
                        } else {
                            entity['amount' + i] = 0;
                        }
                    }
                }
            }
        }
        if (colDef.field === "totalAmount") {
            if (entity.distributionCodeHeadId) {
                var distributionCodeAmount: DistributionCodeHeadDTO = <any>(_.filter(this.distributionCodes, { distributionCodeHeadId: (entity.distributionCodeHeadId) }))[0];
                for (var ii = 1; ii < this.numberOfPeriods + 1; ii++) {
                    var periodA = (distributionCodeAmount.periods[ii - 1]);
                    if (periodA) {
                        entity['amount' + ii] = entity.totalAmount * periodA.percent / 100;
                    } else {
                        entity['amount' + ii] = 0;
                    }
                }
            } else {
                var sum = entity.totalAmount / this.numberOfPeriods;
                for (var iii = 1; iii < this.numberOfPeriods + 1; iii++) {
                    entity['amount' + iii] = sum;
                }
            }
        }
        if (colDef.field.indexOf("amount") > -1) {
            entity.totalAmount = 0;
            for (var ix = 1; ix < this.numberOfPeriods + 1; ix++) {
                entity.totalAmount += +entity['amount' + ix];
            }
        }

        // Refresh
        this.gridAg.options.refreshRows(entity);

        // Set dirty
        this.setParentAsModified();

        // Summarize
        this.calculateSum();
    }
}

    private setParentAsModified() {
        this.$scope.$emit("rowUpdated", this.guid);
    }

    private setNumberOfPeriods() {
        if (this.numberOfPeriods <= 18) {
            if (this.budgetHead) {
                this.budgetHead.noOfPeriods = this.numberOfPeriods;
                this.setupGridColumns();
            }
        }
    }

    private showColumnDim2() {
        this.$timeout(() => {
            if (this.useDim2) {
                this.gridAg.options.showColumn("dim2Nr");
                this.gridAg.options.sizeColumnToFit();
            }
            else {
                this.gridAg.options.hideColumn("dim2Nr");
                this.gridAg.options.sizeColumnToFit();
            }
        });
    }

    private showColumnDim3() {
        this.$timeout(() => {
            if (this.useDim3) {
                this.gridAg.options.showColumn("dim3Nr");
                this.gridAg.options.sizeColumnToFit();
            }
            else {
                this.gridAg.options.hideColumn("dim3Nr");
                this.gridAg.options.sizeColumnToFit();
            }
        });
    }

    private calculateSum() {
        this.sumCaluculations = [];
        for (var j = 0; j < this.budgetHead.rows.length; j++) {
            for (var i = 1; i < this.numberOfPeriods + 1; i++) {
                if (!this.sumCaluculations[i - 1]) {
                    this.sumCaluculations.push({ id: i, sum: 0 });
                }
                var sumrow = <any>(_.filter(this.sumCaluculations, { id: i }))[0];
                sumrow.sum += +this.budgetHead.rows[j]['amount' + i];
            }
        }
    }

    public updateBudgetRows() {
        if (this.budgetHead && this.budgetHead.rows) {
            _.forEach(this.budgetHead.rows, (paymentInformationRow: BudgetRowFlattenedDTO) => {
                _.forEach(this.distributionCodesDict, (distributionCode: any) => {
                    if (paymentInformationRow.distributionCodeHeadId === distributionCode.id) {
                        paymentInformationRow.distributionCodeHeadName = distributionCode.name;
                    }
                });
            });
            this.setRowsWithoutDeleted();
        }
    }
}