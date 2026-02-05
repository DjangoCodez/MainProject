import { Insight, MatrixDefinitionColumnOptions, MatrixLayoutColumn } from "../../../../../Common/Models/MatrixResultDTOs";
import { MatrixColumnSelectionDTO, MatrixColumnsSelectionDTO } from "../../../../../Common/Models/ReportDataSelectionDTO";
import { ReportUserSelectionDTO } from "../../../../../Common/Models/ReportDTOs";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { AnalysisMode, Feature, ReportUserSelectionType, SoeModule, TermGroup, TermGroup_InsightChartTypes } from "../../../../../Util/CommonEnumerations";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { SoeGridOptionsEvent } from "../../../../../Util/Enumerations";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { EmbeddedGridController } from "../../../../Controllers/EmbeddedGridController";
import { IGridHandlerFactory } from "../../../../Handlers/gridhandlerfactory";
import { ICoreService } from "../../../../Services/CoreService";
import { IMessagingService } from "../../../../Services/MessagingService";
import { ITranslationService } from "../../../../Services/TranslationService";
import { IUrlHelperService } from "../../../../Services/UrlHelperService";
import { IReportDataService } from "../../ReportDataService";
import { SelectionCollection } from "../../SelectionCollection";
import { MatrixColumnOptionsController } from "./MatrixColumnOptionsController";

export class MatrixColumnSelection {
    public static component(): ng.IComponentOptions {
        return {
            controller: MatrixColumnSelection,
            templateUrl: soeConfig.baseUrl + "Core/RightMenu/ReportMenu/Selections/MatrixColumnSelection/MatrixColumnSelectionView.html",
            bindings: {
                onSelected: "&",
                module: "<",
                sysReportTemplateTypeId: "<",
                reportId: "<",
                userSelectionInput: "=",
                selections: "<",
                showInsights: "<"
            }
        };
    }

    public static componentKey = "matrixColumnSelection";

    // Binding properties
    private onSelected: (_: { selection: MatrixColumnsSelectionDTO }) => void = angular.noop;
    private module: SoeModule;
    private sysReportTemplateTypeId: number;
    private reportId: number;
    private userSelectionInput: MatrixColumnsSelectionDTO;
    private selections: SelectionCollection;
    private showInsights: boolean;

    // Terms
    private terms: { [index: string]: string; };

    // Permissions
    private hasInsightsReadPermission: boolean = false;
    private hasInsightsModifyPermission: boolean = false;

    private delaySetSavedUserSelection: boolean = false;

    // Saved user columns selections
    private reportUserSelections: SmallGenericType[] = [];
    private selectedReportUserSelectionId: number;
    private selectedReportUserSelection: ReportUserSelectionDTO;

    private matrixPossibleColumns: MatrixLayoutColumn[] = [];
    private possibleColumns: MatrixLayoutColumn[] = [];
    private selectedColumns: MatrixColumnSelectionDTO[] = [];
    private selectedColumn: MatrixColumnSelectionDTO;
    private selection: MatrixColumnsSelectionDTO;

    private possibleColumnsTextFilter: string;

    private _selectedMode: AnalysisMode = AnalysisMode.Analysis;
    private get selectedMode(): AnalysisMode {
        return this._selectedMode;
    }
    private set selectedMode(mode: AnalysisMode) {
        this._selectedMode = mode;
        if (mode === AnalysisMode.Insights) {
            if (this.insights.length === 0) {
                this.loadChartTypes().then(() => {
                    this.setupValueTypes();
                    this.loadInsights().then(() => {
                        if (this.delaySetSavedUserSelection)
                            this.setSavedUserSelection(true);
                        this.modeChanged();
                    });
                });
            } else {
                this.selectedInsight = this.insights[0];
                this.modeChanged();
            }
        } else {
            this.possibleColumns = this.matrixPossibleColumns;
            this.modeChanged();
        }
    }

    private get selectedModeIsAnalysis(): boolean {
        return this.selectedMode === AnalysisMode.Analysis;
    }
    private get selectedModeIsInsights(): boolean {
        return this.selectedMode === AnalysisMode.Insights;
    }

    private _selectedInsight: Insight;
    private get selectedInsight(): Insight {
        return this._selectedInsight;
    }
    private set selectedInsight(insight: Insight) {
        let prevInsight = this._selectedInsight;
        this._selectedInsight = insight;

        this.possibleColumns = insight.possibleColumns;
        this.possibleChartTypes = this.allChartTypes.filter(c => _.includes(insight.possibleChartTypes, c.id));

        // Set default chart type
        if (this.possibleChartTypes.length > 0) {
            if (insight.isCustom) {
                this.selectedChartType = TermGroup_InsightChartTypes.Pie;
            } else {
                if (_.includes(this.possibleChartTypes.map(c => c.id), insight.defaultChartType))
                    this.selectedChartType = insight.defaultChartType;
                else
                    this.selectedChartType = this.possibleChartTypes[0].id;
            }

        } else {
            this.selectedChartType = undefined;
        }

        // Add all columns if fixed
        if (!insight.isCustom) {
            this.clearSelectedColumns(false);
            this.addAllColumns();
        } else {
            if (prevInsight && !prevInsight.isCustom)
                this.selectedColumns = [];
            this.updateGrids();
        }
    }

    private get selectionType(): ReportUserSelectionType {
        return this.selectedModeIsAnalysis ? ReportUserSelectionType.AnalysisColumnSelection : ReportUserSelectionType.InsightsColumnSelection;
    }

    private insights: Insight[] = [];
    private allChartTypes: SmallGenericType[] = [];
    private possibleChartTypes: SmallGenericType[] = [];
    private selectedChartType: TermGroup_InsightChartTypes = TermGroup_InsightChartTypes.Pie;
    private selectedValueType: number;
    private valueTypes: SmallGenericType[] = [];
    private dateFormats: SmallGenericType[] = [];
    private groupAggOptions: SmallGenericType[] = [];

    // Grid
    private gridHandlerPossibleColumns: EmbeddedGridController;
    private gridHandlerSelectedColumns: EmbeddedGridController;

    private modalInstance: any;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        gridHandlerFactory: IGridHandlerFactory,
        private reportDataService: IReportDataService) {

        this.gridHandlerPossibleColumns = new EmbeddedGridController(gridHandlerFactory, "MatrixColumnSelection.PossibleColumns");
        this.gridHandlerPossibleColumns.gridAg.options.enableGridMenu = false;
        this.gridHandlerPossibleColumns.gridAg.options.enableRowSelection = false;
        this.gridHandlerPossibleColumns.gridAg.options.setMinRowsToShow(10);

        this.gridHandlerSelectedColumns = new EmbeddedGridController(gridHandlerFactory, "MatrixColumnSelection.SelectedColumns");
        this.gridHandlerSelectedColumns.gridAg.options.enableGridMenu = false;
        this.gridHandlerSelectedColumns.gridAg.options.enableSingleSelection();
        this.gridHandlerSelectedColumns.gridAg.options.setMinRowsToShow(10);

        this.$scope.$watch(() => this.userSelectionInput, () => {
            this.setSavedUserSelection(false);
        });

        this.modalInstance = $uibModal;
    }

    // SETUP

    public $onInit() {
        if (this.showInsights) {
            this.loadReadPermissions();
            this.loadModifyPermissions();
        }

        this.$q.all([
            this.loadTerms(),
            this.loadDateFormats(),
            this.loadGroupingOptions()
        ]).then(() => {
            this.reportDataService.getMatrixLayoutColumns(this.sysReportTemplateTypeId, this.module).then(x => {
                this.matrixPossibleColumns = x;
                this.possibleColumns = x;
                this.setupGrids();
                this.resetPossibleColumnsData();

                if (this.delaySetSavedUserSelection)
                    this.setSavedUserSelection(false);
            });

            this.selectedColumns = [];

            this.propagateSelection();
        });
    }

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.delete",
            "core.reportmenu.selection.insight.readonly",
            "core.reportmenu.selection.matrix.possiblecolumns",
            "core.reportmenu.selection.matrix.selectedcolumns",
            "core.reportmenu.selection.matrix.option.groupby",
            "common.percent",
            "common.quantity",
            "common.settings"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadReadPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [];
        if (this.module === SoeModule.Billing)
            featureIds.push(Feature.Billing_Insights);
        else if (this.module === SoeModule.Economy)
            featureIds.push(Feature.Economy_Insights);
        else if (this.module === SoeModule.Time)
            featureIds.push(Feature.Time_Insights);

        return this.coreService.hasReadOnlyPermissions(featureIds).then(x => {
            if (this.module === SoeModule.Billing)
                this.hasInsightsReadPermission = x[Feature.Billing_Insights];
            else if (this.module === SoeModule.Economy)
                this.hasInsightsReadPermission = x[Feature.Economy_Insights];
            else if (this.module === SoeModule.Time)
                this.hasInsightsReadPermission = x[Feature.Time_Insights];
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [];
        if (this.module === SoeModule.Billing)
            featureIds.push(Feature.Billing_Insights);
        else if (this.module === SoeModule.Economy)
            featureIds.push(Feature.Economy_Insights);
        else if (this.module === SoeModule.Time)
            featureIds.push(Feature.Time_Insights);

        return this.coreService.hasModifyPermissions(featureIds).then(x => {
            if (this.module === SoeModule.Billing)
                this.hasInsightsModifyPermission = x[Feature.Billing_Insights];
            else if (this.module === SoeModule.Economy)
                this.hasInsightsModifyPermission = x[Feature.Economy_Insights];
            else if (this.module === SoeModule.Time)
                this.hasInsightsModifyPermission = x[Feature.Time_Insights];
        });
    }

    private loadChartTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InsightChartTypes, false, false, false).then(x => {
            this.allChartTypes = x;
        });
    }

    private loadInsights(): ng.IPromise<any> {
        return this.reportDataService.getInsights(this.sysReportTemplateTypeId, this.module).then(x => {
            this.insights = x;
            this.insights.filter(i => i.readOnly).forEach(insight => {
                insight.name += ' ({0})'.format(this.terms["core.reportmenu.selection.insight.readonly"]);
            });

            if (this.insights.length > 0) {
                if (this.hasInsightsReadPermission) {
                    this.selectedInsight = this.insights[0];
                } else if (this.insights.filter(i => !i.readOnly).length > 0) {
                    this.selectedInsight = this.insights.filter(i => !i.readOnly)[0];
                }
            }
        });
    }

    private setupValueTypes() {
        this.valueTypes = [];
        this.valueTypes.push(new SmallGenericType(0, this.terms["common.quantity"]));
        this.valueTypes.push(new SmallGenericType(1, this.terms["common.percent"]));
        this.selectedValueType = 0;
    }

    private loadDateFormats(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.MatrixDateFormatOption, false, true, true).then(x => {
            this.dateFormats = x;
        });
    }

    private loadGroupingOptions(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.MatrixGroupAggOption, false, true, true).then(x => {
            this.groupAggOptions = x;
        });
    }

    private setupGrids() {
        this.gridHandlerPossibleColumns.gridAg.addColumnText("title", this.terms["core.reportmenu.selection.matrix.possiblecolumns"], null, false, { suppressMovable: true, cellStyle: (row) => { return { 'cursor': 'pointer' } } });

        const possibleColumnsEvents: GridEvent[] = [];
        possibleColumnsEvents.push(new GridEvent(SoeGridOptionsEvent.RowClicked, (row) => {
            this.addColumn(row, row.options);
            this.$scope.$applyAsync();
        }));
        this.gridHandlerPossibleColumns.gridAg.options.subscribe(possibleColumnsEvents);
        this.gridHandlerPossibleColumns.gridAg.finalizeInitGrid(null, false);

        this.gridHandlerSelectedColumns.gridAg.addColumnText("title", this.terms["core.reportmenu.selection.matrix.selectedcolumns"], null, false, { suppressMovable: true, suppressSorting: true });
        this.gridHandlerSelectedColumns.gridAg.addColumnIcon(null, null, null, { suppressFilter: true, icon: 'fal fa-poll', showIcon: (row) => row.options && row.options.groupBy, toolTip: this.terms["core.reportmenu.selection.matrix.option.groupby"], noPointer: true });
        this.gridHandlerSelectedColumns.gridAg.addColumnIcon(null, null, null, { suppressFilter: true, icon: 'fal fa-cog', pinned: 'right', toolTip: this.terms["common.settings"], onClick: this.editSelectedColumnOption.bind(this) });
        this.gridHandlerSelectedColumns.gridAg.addColumnDelete(this.terms["core.delete"], this.deleteColumn.bind(this), false, (row) => this.selectedModeIsAnalysis || this.hasInsightsModifyPermission);

        const selectedColumnsEvents: GridEvent[] = [];
        selectedColumnsEvents.push(new GridEvent(SoeGridOptionsEvent.RowClicked, (row) => {
            this.selectedColumn = row;
            this.$scope.$applyAsync();
        }));
        selectedColumnsEvents.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => {
            this.editSelectedColumnOption(row);
        }));
        this.gridHandlerSelectedColumns.gridAg.options.subscribe(selectedColumnsEvents);
        this.gridHandlerSelectedColumns.gridAg.finalizeInitGrid(null, false);
    }

    // USER SELCTIONS

    private setSavedUserSelection(keepMode: boolean) {
        if (!this.userSelectionInput)
            return;

        if (this.possibleColumns.length === 0) {
            this.delaySetSavedUserSelection = true;
            return;
        }

        if (!keepMode)
            this.selectedMode = this.userSelectionInput.analysisMode;

        if (this.userSelectionInput.analysisMode === AnalysisMode.Insights) {
            if (this.insights.length === 0) {
                this.delaySetSavedUserSelection = true;
                return;
            }
        }

        if (this.userSelectionInput.analysisMode === AnalysisMode.Insights) {
            this.selectedInsight = this.insights.find(i => i.insightId === this.userSelectionInput.insightId);
            this.selectedChartType = this.userSelectionInput.chartType;
            this.selectedValueType = this.userSelectionInput.valueType;
        }

        this.selectedColumns = [];
        if (this.userSelectionInput.columns && this.userSelectionInput.columns.length > 0) {
            _.forEach(_.orderBy(this.userSelectionInput.columns, c => c.sort), col => {
                let pCol = _.find(this.possibleColumns, c => c.field === col.field);
                if (pCol)
                    this.addColumn(pCol, col.options);
            });
        }

        this.resetSelectedColumnsData(true);
        this.propagateSelection();
    }

    private propagateSelection() {
        this.selection = new MatrixColumnsSelectionDTO(this.selectedColumns);
        this.selection.analysisMode = this.selectedMode;
        this.selection.insightId = this.selectedModeIsInsights && this.selectedInsight ? this.selectedInsight.insightId : 0;
        this.selection.insightName = this.selectedModeIsInsights && this.selectedInsight ? this.selectedInsight.name : '';
        this.selection.chartType = this.selectedModeIsInsights && this.selectedChartType ? this.selectedChartType : 0;
        this.selection.valueType = this.selectedModeIsInsights && this.selectedValueType ? this.selectedValueType : 0;

        this.onSelected({ selection: this.selection });
    }

    // EVENTS

    private reportUserSelectionChanged(reportUserSelection: ReportUserSelectionDTO) {
        this.selectedReportUserSelection = reportUserSelection;
        if (this.selectedReportUserSelection && this.selectedReportUserSelection.selections && this.selectedReportUserSelection.selections.length > 0) {
            let matrixSelection = this.selectedReportUserSelection.getMatrixColumnSelection();

            if (this.selectedModeIsInsights) {
                this.selectedInsight = this.insights.find(i => i.insightId === matrixSelection.insightId);
                this.selectedChartType = matrixSelection.chartType;
                this.selectedValueType = matrixSelection.valueType;
            }

            this.clearSelectedColumns(false);
            _.orderBy(matrixSelection.columns, col => col.sort).forEach(sc => {
                let col = this.possibleColumns.find(pc => pc.field === sc.field);
                this.addColumn(col, sc.options, false);
            });

            this.updateGrids();
        }
    }

    private modeChanged() {
        this.publishMatrixModeChanged();
    }

    private insightChanged() {
        this.$timeout(() => {
            this.propagateSelection();
            this.publishInsightChanged();
        });
    }

    private propagateSelectionFromGUI() {
        this.$timeout(() => {
            this.propagateSelection();
        });
    }

    private editSelectedColumnOption(column: MatrixColumnSelectionDTO) {
        let otherColumnsGrouped: boolean = this.selectedColumns.filter(c => c.options && c.options.groupBy && c.field !== column.field).length > 0;

        let modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Core/RightMenu/ReportMenu/Selections/MatrixColumnSelection/MatrixColumnOptions.html"),
            controller: MatrixColumnOptionsController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                mode: () => { return this.selectedMode },
                dateFormats: () => { return this.dateFormats },
                groupAggOptions: () => { return this.groupAggOptions },
                column: () => { return column }
            }
        });

        modal.result.then(result => {
            if (result && result.column) {
                let col = this.selectedColumns.find(c => c.field === column.field);
                if (col) {
                    col.options = CoreUtility.cloneDTO(result.column.options);
                    this.resetSelectedColumnsData(false);
                }
            }
        }, (reason) => {
            // User closed
        });
    }

    // HELP-METHODS

    private get showValueTypeSelector(): boolean {
        return this.selectedModeIsInsights &&
            (this.selectedChartType === TermGroup_InsightChartTypes.Pie ||
                this.selectedChartType === TermGroup_InsightChartTypes.Doughnut ||
                this.selectedChartType === TermGroup_InsightChartTypes.Bar ||
                this.selectedChartType === TermGroup_InsightChartTypes.Column);
    }

    private resetPossibleColumnsData() {
        this.gridHandlerPossibleColumns.gridAg.setData(this.visiblePossibleColumns);
    }

    private resortSelectedColumns() {
        let sort = 1;
        _.forEach(_.orderBy(this.selectedColumns, ['isGroupedOnSort', 'sort'], ['desc', 'asc']), col => {
            col.sort = sort++;
        });
    }

    private resetSelectedColumnsData(selectFirst: boolean) {
        this.resortSelectedColumns();
        this.gridHandlerSelectedColumns.gridAg.setData(_.orderBy(this.selectedColumns, c => c.sort));
        this.selectColumn(selectFirst);
        this.publishMatrixModeChanged();
    }

    private get visiblePossibleColumns(): MatrixLayoutColumn[] {
        return _.filter(this.possibleColumns, c => !c.isHidden && !c.visible);
    }

    private selectColumn(first: boolean) {
        if (this.selectedColumns.length > 0) {
            if (first)
                this.selectedColumn = this.selectedColumns[0];
            this.gridHandlerSelectedColumns.gridAg.options.selectRow(this.selectedColumn);
        } else {
            this.selectedColumn = undefined;
        }
    }

    private addColumn(column: MatrixLayoutColumn, options: MatrixDefinitionColumnOptions, update: boolean = true) {
        if (!column)
            return;

        let newColumn = new MatrixColumnSelectionDTO(column.field, this.selection.maxSort + 1, column.title, options);
        newColumn.matrixDataType = column.matrixDataType;
        this.selectedColumns.push(newColumn);
        column.visible = true;
        this.selectedColumn = newColumn;

        if (update)
            this.updateGrids(false);
    }

    private addAllColumns() {
        this.clearSelectedColumns(false);
        _.forEach(this.possibleColumns, pCol => {
            if (!pCol.isHidden) this.addColumn(pCol, pCol.options, false);
        });

        this.updateGrids();
    }

    private deleteColumn(column: MatrixColumnSelectionDTO) {
        if (!column || (this.selectedModeIsInsights && !this.hasInsightsModifyPermission))
            return;

        _.pullAll(this.selection.columns, _.filter(this.selection.columns, c => c.field === column.field));

        let pCol = _.find(this.possibleColumns, c => c.field === column.field);
        if (pCol)
            pCol.visible = false;

        this.reNumberRows();
        this.updateGrids();
    }

    private deleteAllColumns() {
        if (this.selectedModeIsInsights && !this.hasInsightsModifyPermission)
            return;

        this.clearSelectedColumns(true);
    }

    private clearSelectedColumns(update: boolean) {
        this.selectedColumns = [];
        this.selection.columns = [];
        this.possibleColumns.filter(pc => pc.visible).forEach(c => {
            c.visible = false;
        });

        if (update)
            this.updateGrids();
    }

    private sortFirst() {
        if (this.selectedColumn && this.selectedColumn.sort > 1) {
            this.selectedColumn.sort = -1;
            this.reNumberRows();
            this.resetSelectedColumnsData(false);
        }
    }

    private sortUp() {
        if (this.selectedColumn && this.selectedColumn.sort > 1) {
            let prevColumn = _.find(this.selection.columns, c => c.sort === this.selectedColumn.sort - 1);
            if (prevColumn) {
                this.selectedColumn.sort--;
                prevColumn.sort++;
                this.resetSelectedColumnsData(false);
            }
        }
    }

    private sortDown() {
        if (this.selectedColumn && this.selectedColumn.sort < this.selection.columns.length) {
            let nextColumn = _.find(this.selection.columns, c => c.sort === this.selectedColumn.sort + 1);
            if (nextColumn) {
                this.selectedColumn.sort++;
                nextColumn.sort--;
                this.resetSelectedColumnsData(false);
            }
        }
    }

    private sortLast() {
        if (this.selectedColumn && this.selectedColumn.sort < this.selection.columns.length) {
            this.selectedColumn.sort = this.selection.maxSort + 1;
            this.reNumberRows();
            this.resetSelectedColumnsData(false);
        }
    }

    private reNumberRows() {
        this.selection.columns.sort((r1, r2) => {
            if (r1.sort < r2.sort)
                return -1;
            else if (r1.sort > r2.sort)
                return 1;
            else
                return 0;
        });

        let i: number = 0;
        _.forEach(this.selection.columns, c => {
            i++;
            if (c.sort !== i)
                c.sort = i;
        });
    }

    private updateGrids(selectFirst: boolean = true) {
        this.resetPossibleColumnsData();
        this.resetSelectedColumnsData(selectFirst);
        this.propagateSelection();
    }

    private get selectedColumnIsFirst(): boolean {
        return this.selectedColumn && this.selectedColumn.sort === 1;
    }

    private get selectedColumnIsLast(): boolean {
        return this.selectedColumn && this.selectedColumn.sort === this.selection.columns.length;
    }

    private publishMatrixModeChanged() {
        this.messagingService.publish('matrixModeChanged', { mode: this.selectedMode, hasColumns: this.selectedColumns.length > 0, insightReadOnly: this.selectedModeIsInsights && this.selectedInsight && this.selectedInsight.readOnly });
    }

    private publishInsightChanged() {
        this.messagingService.publish('insightChanged', { insightId: this.selectedInsight.insightId });
    }
}
