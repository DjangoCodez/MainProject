import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IMessagingHandler } from "../../../../Core/Handlers/MessagingHandler";
import { IGridControllerFlowHandler } from "../../../../Core/Handlers/ControllerFlowHandler";
import { BudgetHeadDTO, BudgetRowDTO, BudgetHeadSalesDTO, BudgetRowSalesDTO, BudgetPeriodSalesDTO } from "../../../../Common/Models/BudgetDTOs";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { IAccountingService } from "../../../../Shared/Economy/Accounting/AccountingService";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { ToolBarUtility, ToolBarButton } from "../../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { SalesBudgetPreviousPeriodResultController } from "./PreviousPeriodResultController";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { IProgressHandler } from "../../../../Core/Handlers/ProgressHandler";
import { IToolbar } from "../../../../Core/Handlers/Toolbar";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { Feature, TermGroup_AccountingBudgetType, TermGroup_AccountingBudgetSubType } from "../../../../Util/CommonEnumerations";
import { IGridHandlerAg } from "../../../../Core/Handlers/GridHandlerAg";
import { TypeAheadOptionsAg, IColumnAggregations, SoeGridOptionsAg, ISoeGridOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { ColumnAggregationFooterGrid } from "../../../../Util/ag-grid/ColumnAggregationFooterGrid";
import { DistributionCodeHeadDTO, DistributionCodePeriodDTO } from "../../../../Common/Models/DistributionCodeHeadDTO";
import { NumberUtility } from "../../../../Util/NumberUtility";
import { Guid } from "../../../../Util/StringUtility";
import { AccountDimSmallDTO } from "../../../../Common/Models/AccountDimDTO";
import { OpeningHoursDTO } from "../../../../Common/Models/OpeningHoursDTO";

export class SalesBudgetGridDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getViewUrl("budgetGrid.html"),
            controller: SalesBudgetGridController,
            controllerAs: "ctrlDirective",
            bindToController: true,
            scope: {
                budgetHead: "=",
                numberOfPeriods: "=",
                accountDims: "=",
                distributionCodesDict: "=",
                distributionCodes: "=",
                openingHours: "=",
                useDim2: "=",
                useDim3: "=",
                distributionCodeId: "=",
                dim2Id: "=",
                dim3Id: "=",
                showPassedPeriodsDialog: "=",
                isDisabled: "=",
                intervalType: "=",
                budgetType: "=",
                guid: "=",
                earliestOpeningHour: "=",
                latestClosingHour: "=",

            },
            link(scope: ng.IScope, element: JQuery, attributes: ng.IAttributes, ngModelController: any) {
                scope.$watch(() => (ngModelController.budgetHead), (newValue, oldValue, scope) => {
                    if (newValue) {
                        if (!oldValue) {
                            ngModelController.distributionCodeChanged();
                            if (ngModelController.budgetHead.rows) {
                                ngModelController.setRowsWithoutDeleted(false);
                            }
                        }
                    }
                }, true);
                scope.$watch(() => (ngModelController.numberOfPeriods), (newValue, oldValue, scope) => {
                    if (newValue && ngModelController.budgetHead && newValue !== ngModelController.budgetHead.noOfPeriods) {
                        ngModelController.setNumberOfPeriods(newValue);
                    }
                });
                scope.$watch(() => (ngModelController.useDim2), (newValue, oldValue, scope) => {
                    if (newValue != null) {
                        ngModelController.showColumnDim2();
                    }
                });
                scope.$watch(() => (ngModelController.useDim3), (newValue, oldValue, scope) => {
                    if (newValue != null) {
                        ngModelController.showColumnDim3();
                    }
                });
                scope.$watch(() => (ngModelController.budgetType), (newValue, oldValue, scope) => {
                    if (newValue != null) {
                        ngModelController.typeChanged();
                    }
                });
                scope.$watch(() => (ngModelController.distributionCodeId), (newValue, oldValue, scope) => {
                    if (newValue != null && newValue !== oldValue) {
                        ngModelController.distributionCodeChanged();
                    }
                });
                scope.$watch(() => (ngModelController.distributionCodes), (newValue, oldValue, scope) => {
                    if (newValue != null && newValue !== oldValue) {
                        ngModelController.distributionCodeChanged();
                    }
                });
            },
            restrict: "E",
            replace: true
        }
    }
}

export class SalesBudgetGridController implements ICompositionGridController {
    modal: angular.ui.bootstrap.IModalService;

    public guid: Guid;

    private fieldName: string = "amount";

    private messagingHandler: IMessagingHandler;
    private parameters: any;
    private isHomeTab: boolean;
    private doubleClickToEdit: boolean = true;
    private flowHandler: IGridControllerFlowHandler;

    // Data
    budgetHead: BudgetHeadSalesDTO;
    currentClonedRow: BudgetRowSalesDTO;
    currentClonedCell: BudgetPeriodSalesDTO;
    currentCellFirstLevel: BudgetPeriodSalesDTO;
    currentCellSecondLevel: BudgetPeriodSalesDTO;

    // Lookups 
    accountDims: AccountDimSmallDTO[];
    distributionCodesDict;
    distributionCodes: DistributionCodeHeadDTO[];
    openingHours: OpeningHoursDTO[];
    currentDistributionCode: DistributionCodeHeadDTO;
    private dayOfWeeks: any[];

    terms: any;
    numberOfPeriods: number;
    public sumCalculations: any[] = [];
    budgetType: number;
    private get isTime(): boolean {
        return this.budgetType === TermGroup_AccountingBudgetType.SalesBudgetTime;
    }
    distributionCodeId: number;
    dim2Id: number;
    dim3Id: number;
    showPassedPeriodsDialog: boolean;
    intervalType: number;
    earliestOpeningHour: number;
    latestClosingHour: number;

    // Flags
    loadingBudget: boolean;
    isDisabled: boolean;
    useDim2: boolean;
    useDim3: boolean;
    firstLevelInitialized: boolean = false;
    secondLevelInitialized: boolean = false;
    currentCellChanged: boolean = false;

    // Grid options
    firstLevelDetailsGridOption: ISoeGridOptionsAg;
    secondLevelDetailsGridOption: ISoeGridOptionsAg;

    // Aligned grid
    footerGrid: ColumnAggregationFooterGrid;

    //@ngInject
    constructor(private accountingService: IAccountingService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private messagingService: IMessagingService,
        $uibModal) {

        this.modal = $uibModal;
        this.messagingHandler = messagingHandlerFactory.create();

        this.gridAg = gridHandlerFactory.create("Economy.Accounting.SalesBudget.Edit", "agGrid");
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readOnlyPermission = readOnly;
                this.modifyPermission = modify
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    //this.messagingHandler.publishActivateAddTab();
                }

            })
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.setRowsWithoutDeleted(false))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))

        this.flowHandler.start({ feature: Feature.Economy_Accounting_SalesBudget, loadReadPermissions: true, loadModifyPermissions: true });

        this.$scope.$on('resetRows', (e, a) => {
            this.setRowsWithoutDeleted(false);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createEmpty()
        if (this.modifyPermission) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.newrow", "common.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
                this.addRow();
            }, () => { return !this.distributionCodeId || this.isDisabled })));
        }
    }

    onInit(parameters: any) {
        //Not called
    }

    edit(row) {
        /*
        // Send message to TabsController
        if (this.doubleClickToEdit && (this.readOnlyPermission || this.modifyPermission)) {
            this.messagingHandler.publishEditRow(row);
        }
        */
    }

    private setupDayOfWeeks() {
        this.dayOfWeeks = []
        _.forEach(CalendarUtility.getDayOfWeekNames(true), dayOfWeek => {
            this.dayOfWeeks.push({ id: dayOfWeek.id, label: dayOfWeek.name });
        });
    }

    private createRow(rowNr: number): BudgetRowSalesDTO {
        var row = new BudgetRowSalesDTO();
        row.totalAmount = 0;
        row.totalQuantity = 0;
        row.isDeleted = false;
        row.budgetRowNr = rowNr;

        // Set upp hierarchial periods (up to 3?)
        if (this.currentDistributionCode) {
            // Get opening hour
            var startHour = undefined;
            var closingHour = undefined;
            var openingHour = this.currentDistributionCode && this.currentDistributionCode.openingHoursId ? _.find(this.openingHours, { openingHoursId: this.currentDistributionCode.openingHoursId }) : undefined;
            if (openingHour) {
                startHour = new Date(openingHour.openingTime).getHours();
                closingHour = new Date(openingHour.closingTime).getHours();
            }

            var periodDate = new Date(1900, 1, 1);
            for (var i = 0; i < this.currentDistributionCode.periods.length; i++) {
                var newPeriod = new BudgetPeriodSalesDTO();
                newPeriod.budgetRowNr = row.budgetRowNr;
                newPeriod.guid = Guid.newGuid();
                if (openingHour) {
                    newPeriod["startHour"] = startHour;
                    newPeriod["closingHour"] = closingHour;
                }

                var period = this.currentDistributionCode.periods[i] as DistributionCodePeriodDTO;
                if (period && period.parentToDistributionCodePeriodId) {
                    var childCodeLevelOne = _.find(this.distributionCodes, { distributionCodeHeadId: period.parentToDistributionCodePeriodId });
                    if (childCodeLevelOne) {
                        // Get opening hour
                        var openingHourLevelOne = childCodeLevelOne && childCodeLevelOne.openingHoursId ? _.find(this.openingHours, { openingHoursId: childCodeLevelOne.openingHoursId }) : undefined;
                        if (openingHourLevelOne) {
                            newPeriod["startHour"] = new Date(openingHourLevelOne.openingTime).getHours();
                            newPeriod["closingHour"] = new Date(openingHourLevelOne.closingTime).getHours();
                        }

                        newPeriod.distributionCodeHeadId = childCodeLevelOne.distributionCodeHeadId;
                        if (childCodeLevelOne.subType !== TermGroup_AccountingBudgetSubType.Day) {
                            for (var ix = 0; ix < childCodeLevelOne.periods.length; ix++) {
                                var newPeriodLevelOne = new BudgetPeriodSalesDTO();
                                newPeriodLevelOne.budgetRowNr = row.budgetRowNr;
                                newPeriodLevelOne.guid = Guid.newGuid();
                                newPeriodLevelOne.parentGuid = newPeriod.guid;

                                var periodLevelOne = childCodeLevelOne.periods[ix] as DistributionCodePeriodDTO;
                                if (periodLevelOne && periodLevelOne.parentToDistributionCodePeriodId) {
                                    var childCodeLevelTwo = _.find(this.distributionCodes, { distributionCodeHeadId: periodLevelOne.parentToDistributionCodePeriodId });
                                    if (childCodeLevelTwo) {
                                        // Get opening hour
                                        var openingHourLevelTwo = childCodeLevelTwo && childCodeLevelTwo.openingHoursId ? _.find(this.openingHours, { openingHoursId: childCodeLevelTwo.openingHoursId }) : undefined;
                                        if (openingHourLevelTwo) {
                                            newPeriodLevelOne["startHour"] = new Date(openingHourLevelTwo.openingTime).getHours();
                                            newPeriodLevelOne["closingHour"] = new Date(openingHourLevelTwo.closingTime).getHours();
                                        }

                                        newPeriodLevelOne.distributionCodeHeadId = childCodeLevelTwo.distributionCodeHeadId;
                                        newPeriodLevelOne["showDay"] = childCodeLevelTwo.periods && childCodeLevelTwo.periods.length > 0;
                                    }
                                }
                                newPeriodLevelOne.amount = 0;
                                newPeriodLevelOne.quantity = 0;
                                newPeriodLevelOne.percent = periodLevelOne.percent;
                                newPeriodLevelOne.startDate = periodDate;
                                newPeriod.periods.push(newPeriodLevelOne);

                                // Update date
                                if (periodLevelOne)
                                    periodDate = this.getPeriodDate(childCodeLevelOne, periodDate);
                            }
                        } else {
                            newPeriod["showDay"] = childCodeLevelOne.periods && childCodeLevelOne.periods.length > 0;
                        }
                    }
                }

                newPeriod.amount = 0;
                newPeriod.quantity = 0;
                newPeriod.percent = period.percent;
                newPeriod.startDate = periodDate;
                row.periods.push(newPeriod);

                // Update date
                if (period)
                    periodDate = this.getPeriodDate(this.currentDistributionCode, periodDate);
            }
        }

        //Set empty accounts
        row.dim1Nr = "";
        row.dim1Name = "";
        row.dim2Nr = "";
        row.dim2Name = "";
        row.dim3Nr = "";
        row.dim3Name = "";
        row.dim4Nr = "";
        row.dim4Name = "";
        row.dim5Nr = "";
        row.dim5Name = "";
        row.dim6Nr = "";
        row.dim6Name = "";

        return row;
    }

    private addRow() {
        // Create clone
        var newRow = this.createRow(this.budgetHead.rows.length + 1);

        // Add to collection
        this.budgetHead.rows.push(newRow);

        // Update grid
        this.setRowsWithoutDeleted();
    }

    private getPeriodDate(distributionCode: DistributionCodeHeadDTO, currentDate: Date): Date {
        var date;
        if (distributionCode.subType === TermGroup_AccountingBudgetSubType.Year) {
            date = currentDate.addMonths(1);
        }
        else if (distributionCode.subType === TermGroup_AccountingBudgetSubType.YearWeek) {
            date = currentDate.addDays(7);
        }
        else if (distributionCode.subType === TermGroup_AccountingBudgetSubType.Day) {
            date = currentDate.addDays(1);
        }
        else {
            date = currentDate.addHours(1);
        }
        return date;
    }

    public setupGrid(): void {
        //Different layout depending on Interval selection
        this.initRowsGrid();

        var keys: string[] = [
            "core.deleterow",
            "common.sum",
            "common.total",
            "economy.accounting.distributioncode.distributioncode",
            "economy.accounting.budget.getresultperiod",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            // Set name
            this.gridAg.options.setName("budgetGrid");

            // Disable filtering
            this.gridAg.options.enableFiltering = false;

            // Enable details view
            this.firstLevelDetailsGridOption = SoeGridOptionsAg.create("firstLevelDetails", this.$timeout);
            this.firstLevelDetailsGridOption.keepApi = true;
            this.firstLevelDetailsGridOption.autoHeight = true;
            this.firstLevelDetailsGridOption.enableFiltering = false;
            this.gridAg.options.enableMasterDetail(this.firstLevelDetailsGridOption, 300);

            // Enable second level details view
            this.secondLevelDetailsGridOption = SoeGridOptionsAg.create("secondLevelDetails", this.$timeout);
            this.secondLevelDetailsGridOption.keepApi = true;
            this.secondLevelDetailsGridOption.autoHeight = true;
            this.secondLevelDetailsGridOption.enableFiltering = false;
            this.firstLevelDetailsGridOption.enableMasterDetail(this.secondLevelDetailsGridOption, 150);

            // Set up columns
            if (this.distributionCodeId && this.distributionCodeId > 0)
                this.setupGridColumns();

            _.forEach(this.gridAg.options.getColumnDefs(), (colDef: any) => {
                colDef.enableFiltering = false;
                colDef.enableSorting = false;
                /*colDef.cellEditableCondition = (scope: any) => { return this.isDisabled === false }
                if (this.modifyPermission) {
                    colDef.enableCellEdit = true;
                }*/
            });

            this.gridAg.options.customTabToCellHandler = (params) => this.navigateToNextCell(params);

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.addStandardMenuItems();
            this.gridAg.setExporterFilenamesAndHeader("economy.accounting.budget.budget");

            this.gridAg.options.addTotalRow("#totals-grid", {
                filtered: this.terms["core.aggrid.totals.filtered"],
                total: this.terms["core.aggrid.totals.total"]
            });

            this.showColumnDim2();
            this.showColumnDim3();
            this.calculateSum();
        });
    }

    private setupGridColumns() {
        if (!this.terms)
            return;

        if (!this.currentDistributionCode)
            return;

        // Clear column definitions
        this.gridAg.options.resetColumnDefs();
        if (this.intervalType > 0)  // Month
            this.gridAg.options.ignoreResizeToFit = true;

        // Handle dimension from distribution code (do not add dimensions above specified)
        var minIndex: number = 0;
        if (this.currentDistributionCode.accountDimId) {
            var dimension = _.find(this.accountDims, a => a.accountDimId === this.currentDistributionCode.accountDimId);
            if (dimension)
                minIndex = this.accountDims.indexOf(dimension)
        }
        this.addAccountDimColumns(minIndex);

        var aggregations = {};
        this.gridAg.addColumnNumber(this.isTime ? "totalQuantity" : "totalAmount", this.isTime ? this.terms["common.total"] : this.terms["common.sum"], 100, { enableHiding: false, minWidth: 50, decimals: 2, suppressSorting: true, editable: this.isCellEditable.bind(this) });
        aggregations[this.isTime ? "totalQuantity" : "totalAmount"] = "sum";

        if (this.budgetHead) {
            var openHour: number = 0;
            if (this.intervalType === 14) { //day - hours
                if (this.currentDistributionCode.openingHoursId) {
                    let openingHour = _.find(this.openingHours, { openingHoursId: this.currentDistributionCode.openingHoursId });
                    if (openingHour)
                        openHour = new Date(openingHour.openingTime).getHours();
                }
            }

            for (var i = 1; i <= this.budgetHead.noOfPeriods; i++) {
                var headerName: string;
                if (this.intervalType === 0)                                // Month
                    headerName = CalendarUtility.getMonthName(i - 1);
                else if (this.intervalType > 0 && this.intervalType < 12)
                    headerName = i.toString() + ".";
                else if (this.intervalType === 13)                          // Week (days)
                    headerName = CalendarUtility.getDayName(i);
                else if (this.intervalType === 14)                          // Day (hours)
                    headerName = (openHour + (i - 1)).toString().padLeft(2, '0') + ":00";
                else
                    headerName = i.toString();

                var numCol = this.gridAg.addColumnNumber(this.fieldName + i.toString(), headerName, 100, { enableHiding: false, minWidth: 50, decimals: 2, suppressSorting: true, editable: this.isCellEditable.bind(this) });
                numCol["periodNr"] = i;
                aggregations[this.fieldName + i.toString()] = "sum";
            }
        }

        if (!this.isDisabled)
            this.gridAg.addColumnDelete(this.terms["core.deleterow"], this.deleteBudgetRow.bind(this));

        if (this.footerGrid)
            this.footerGrid.setColumnDefs(this.gridAg.options.getColumnDefs(), aggregations);
        else
            this.footerGrid = this.gridAg.options.addAggregatedFooterRow("#sum-footer-grid", aggregations as IColumnAggregations);

        this.gridAg.options.finalizeInitGrid();
    }

    private addAccountDimColumns(minIndex: number) {
        this.accountDims.forEach((ad, i) => {
            var options = new TypeAheadOptionsAg();
            options.source = (filter) => this.filterAccounts(ad.accountDimNr, filter);
            options.displayField = "name"
            options.dataField = "name";
            options.minLength = 0;
            options.delay = 0;
            options.useScroll = true;
            options.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, entity, colDef);
            var dimColumn = this.gridAg.addColumnTypeAhead("dim" + (i + 1) + "Name", ad.name, 100, { error: 'dim' + (i + 1) + 'Error', typeAheadOptions: options, displayField: "name", editable: this.isCellEditable.bind(this), suppressSorting: true });
            dimColumn['additionalData'] = { dimIndex: i + 1, dimNr: ad.accountDimNr };
            if (this.accountDims.indexOf(ad) < minIndex)
                dimColumn.hide = true;
        });
    }

    private getPeriodCellValue(i: number, params: any) {
        if (params.data.startHour) {
            if (params.data.periods[i - params.data.startHour])
                return params.data.periods[i - params.data.startHour].amount;
            else
                return 0;
        }
        else {
            return params.data.periods[i].amount;
        }
    }

    private setPeriodCellValueDetails(i: number, params: any) {
        var newValue = params.newValue;
        var oldValue = params.oldValue;
        if (newValue != oldValue) {
            var entity = params.data;
            var currentPeriod = entity.periods[i] as BudgetPeriodSalesDTO;
            if (currentPeriod) {
                //Set amount
                currentPeriod.amount = newValue;
                this.distributeAmount(currentPeriod, newValue, oldValue, true);
            }
            entity.totalAmount = NumberUtility.parseDecimal((entity.totalAmount + (newValue - oldValue)).toFixed(2));

            //Set modified
        }
    }

    private initRowsGrid() {
        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.CellFocused, (rowIndex, column, rowPinned, forceBrowserFocus) => { this.cellFocused(rowIndex, column ? column.colDef : null); }));
        events.push(new GridEvent(SoeGridOptionsEvent.BeginCellEdit, (entity, colDef) => this.beginCellEdit(entity, colDef)));
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity: BudgetRowSalesDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) => this.afterCellEdit(entity, colDef, newValue, oldValue)));
        this.gridAg.options.subscribe(events);
    }

    private cellFocused(rowIndex, colDef: uiGrid.IColumnDef) {
        if (this.isDisabled && colDef && colDef['periodNr']) {
            let row = this.gridAg.options.getVisibleRowByIndex(rowIndex);
            if (row && row.data)
                this.beginCellEdit(row.data, colDef);
        }
    }

    private beginCellEdit(entity: any, colDef: uiGrid.IColumnDef) {
        // Close others
        if (this.currentClonedRow && entity && entity != this.currentClonedRow) {
            this.gridAg.options.expandMasterDetail(this.currentClonedRow, false);
        }

        // Set current row
        var rowChanged: boolean = false;
        this.currentClonedRow = entity;

        if (colDef['periodNr']) {
            let currentRow: BudgetRowSalesDTO = _.find(this.budgetHead.rows, (r) => r.budgetRowNr === this.currentClonedRow.budgetRowNr) as BudgetRowSalesDTO;
            this.currentCellFirstLevel = currentRow.periods[colDef['periodNr'] - 1];
            if (this.currentCellFirstLevel && this.currentCellFirstLevel.periods && this.currentCellFirstLevel.periods.length > 0) {
                // Create copy
                var period = {};
                period[this.fieldName] = this.currentCellFirstLevel[this.fieldName];

                // Create period properties
                var counter: number = 1;
                _.forEach(this.currentCellFirstLevel.periods, (p) => {
                    period[this.fieldName + counter.toString()] = p[this.fieldName];
                    counter++;
                });

                // Get code
                var distributionCode = _.find(this.distributionCodes, { distributionCodeHeadId: this.currentCellFirstLevel.distributionCodeHeadId });

                // Check if initialized
                if (!this.firstLevelInitialized) {
                    // Create callback
                    this.gridAg.options.setDetailCellDataCallback((params) => {
                        period["ag_node_id"] = 0;
                        params.successCallback([period]);

                        // Disconnect
                        this.gridAg.options.setDetailCellDataCallback(undefined);

                        // Start edit
                        var column = this.gridAg.options.getColumnByField(colDef.field);
                        this.gridAg.options.startEditingColumn(column);

                    });

                    // Initialize sub grid
                    this.initializeDetailsGrid(this.currentClonedRow, period, distributionCode, colDef, this.firstLevelDetailsGridOption);

                    // Update flag
                    this.firstLevelInitialized = true;

                    // Expand detail
                    this.gridAg.options.expandMasterDetail(this.currentClonedRow, true);
                } else {
                    if (rowChanged) {
                        // Create callback
                        this.gridAg.options.setDetailCellDataCallback((params) => {
                            period["ag_node_id"] = 0;
                            params.successCallback([period]);

                            // Disconnect
                            this.gridAg.options.setDetailCellDataCallback(undefined);

                            // Start edit
                            var column = this.gridAg.options.getColumnByField(colDef.field);
                            this.gridAg.options.startEditingColumn(column);
                        });

                        this.gridAg.options.expandMasterDetail(this.currentClonedRow, true);
                    } else {
                        this.firstLevelDetailsGridOption.setData([period]);
                        this.gridAg.options.expandMasterDetail(this.currentClonedRow, true);
                    }
                }
                this.currentCellChanged = true;
            }
        }
    }

    private cellFocusedDetails(rowIndex, colDef: uiGrid.IColumnDef) {
        if (this.isDisabled && colDef && colDef['periodNr']) {
            let row = this.gridAg.options.getVisibleRowByIndex(rowIndex);
            if (row && row.data)
                this.beginCellEditDetails(row.data, colDef);
        }
    }

    private beginCellEditDetails(entity: any, colDef: uiGrid.IColumnDef) {
        if (colDef['periodNr'] && this.currentCellFirstLevel && !entity.lowestLevel) {
            if (!this.currentCellSecondLevel || entity != this.currentCellSecondLevel) {
                // Get current period
                this.currentCellSecondLevel = this.currentCellFirstLevel.periods[colDef['periodNr'] - 1] as BudgetPeriodSalesDTO;
                if (this.currentCellSecondLevel) {
                    // Get code
                    var distributionCode = _.find(this.distributionCodes, { distributionCodeHeadId: this.currentCellSecondLevel.distributionCodeHeadId });
                    if (this.currentCellSecondLevel.periods && this.currentCellSecondLevel.periods.length > 0) {
                        var period = { startHour: this.currentCellSecondLevel.startHour ? this.currentCellSecondLevel.startHour : 0, closingHour: this.currentCellSecondLevel.closingHour ? this.currentCellSecondLevel.closingHour : 0, lowestLevel: (!this.currentCellSecondLevel.periods[0].periods || this.currentCellSecondLevel.periods[0].periods.length === 0) };
                        period[this.fieldName] = this.currentCellSecondLevel[this.fieldName];

                        // Create period properties
                        var counter: number = period.startHour ? period.startHour : 1;
                        _.forEach(this.currentCellSecondLevel.periods, (p) => {
                            period[this.fieldName + counter.toString()] = p[this.fieldName];
                            counter++;
                        });

                        if (!this.secondLevelInitialized) {
                            // Initialize sub grid
                            this.initializeDetailsGrid(entity, period, distributionCode, colDef, this.secondLevelDetailsGridOption);

                            // Set callback
                            this.firstLevelDetailsGridOption.setDetailCellDataCallback((params) => {
                                period["ag_node_id"] = 0;
                                params.successCallback([period]);
                            });

                            // Update flag
                            this.secondLevelInitialized = true;

                            // Expand detail
                            this.firstLevelDetailsGridOption.expandMasterDetail(entity, true);
                        } else {
                            if (this.currentCellChanged) {
                                // Set callback
                                this.firstLevelDetailsGridOption.setDetailCellDataCallback((params) => {
                                    dayPeriod["ag_node_id"] = 0;
                                    params.successCallback([dayPeriod]);

                                    // Detach
                                    this.firstLevelDetailsGridOption.setDetailCellDataCallback(undefined);
                                });

                                this.firstLevelDetailsGridOption.expandMasterDetail(entity, true);
                                this.currentCellChanged = false;
                            } else {
                                this.firstLevelDetailsGridOption.expandMasterDetail(entity, true);
                                this.resetDetailsGrid(entity, dayPeriod, distributionCode, colDef, this.secondLevelDetailsGridOption);
                            }
                        }
                    } else if (distributionCode && distributionCode.subType === TermGroup_AccountingBudgetSubType.Day) {
                        //Check for opening hour
                        var openingHour = distributionCode.openingHoursId ? _.find(this.openingHours, { openingHoursId: distributionCode.openingHoursId }) : undefined;

                        // Create period
                        var dayPeriod = { guid: this.currentCellSecondLevel.guid, distributionCodeHeadId: distributionCode.distributionCodeHeadId, distributionCodeName: distributionCode.name, fieldName: this.currentCellSecondLevel["fieldName"], startHour: openingHour ? new Date(openingHour.openingTime).getHours() : 0, closingHour: openingHour ? new Date(openingHour.closingTime).getHours() : 24, lowestLevel: true, locked: true };
                        dayPeriod[this.fieldName] = this.currentCellSecondLevel[this.fieldName];

                        // Create period properties
                        var dayCounter: number = openingHour ? new Date(openingHour.openingTime).getHours() : 1;
                        _.forEach(distributionCode.periods, (p) => {
                            dayPeriod[this.fieldName + dayCounter.toString()] = NumberUtility.parseDecimal(((this.currentCellSecondLevel[this.fieldName] * p.percent) / 100).toFixed(2));
                            dayCounter++;
                        });

                        if (!this.secondLevelInitialized) {
                            // Initialize sub grid
                            this.initializeDetailsGrid(entity, dayPeriod, distributionCode, colDef, this.secondLevelDetailsGridOption);

                            // Set callback
                            this.firstLevelDetailsGridOption.setDetailCellDataCallback((params) => {
                                dayPeriod["ag_node_id"] = 0;
                                params.successCallback([dayPeriod]);

                                // Detach
                                this.firstLevelDetailsGridOption.setDetailCellDataCallback(undefined);
                            });

                            // Update flag
                            this.secondLevelInitialized = true;

                            // Expand detail
                            this.firstLevelDetailsGridOption.expandMasterDetail(entity, true);
                        } else {
                            if (this.currentCellChanged) {
                                // Set callback
                                this.firstLevelDetailsGridOption.setDetailCellDataCallback((params) => {
                                    dayPeriod["ag_node_id"] = 0;
                                    params.successCallback([dayPeriod]);

                                    // Detach
                                    this.firstLevelDetailsGridOption.setDetailCellDataCallback(undefined);
                                });

                                this.firstLevelDetailsGridOption.expandMasterDetail(entity, true);
                                this.currentCellChanged = false;
                            } else {
                                this.firstLevelDetailsGridOption.expandMasterDetail(entity, true);
                                this.resetDetailsGrid(entity, dayPeriod, distributionCode, colDef, this.secondLevelDetailsGridOption);
                            }
                        }
                    }
                }
            }
        }
    }

    private afterCellEdit(entity: BudgetRowSalesDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue != oldValue) {
            if (colDef.field === "")
                return;

            var editingRow = _.find(this.budgetHead.rows, (r) => r.budgetRowNr === entity.budgetRowNr);
            if (!editingRow)
                return;

            if (colDef['additionalData']) {
                var accountDim = _.find(this.accountDims, a => a.accountDimNr === colDef['additionalData'].dimNr);
                if (accountDim) {
                    var account = accountDim.accounts.find(acc => acc.name === newValue);
                    let dimIdx = colDef['additionalData'].dimIndex;
                    entity[`dim${dimIdx}Id`] = editingRow[`dim${dimIdx}Id`] = entity.accountId = (account && account.accountId > 0 ? account.accountId : undefined);
                    entity[`dim${dimIdx}Nr`] = editingRow[`dim${dimIdx}Nr`] = (account && account.accountId > 0 ? account.accountNr : '');
                    entity[`dim${dimIdx}Name`] = editingRow[`dim${dimIdx}Name`] = (account && account.accountId > 0 ? account.name : '');
                }
            }

            if (this.isTime) {
                if (colDef.field === "totalQuantity" || (colDef.field === "distributionCodeHeadId")) {
                    // Set quantity in actual objects
                    // Set value on row
                    editingRow.totalQuantity = newValue;

                    //Set value on periods
                    for (var i = 1; i < editingRow.periods.length + 1; i++) {
                        var editingPeriod = editingRow.periods[i - 1] as BudgetPeriodSalesDTO;
                        if (editingPeriod) {
                            var quantity = NumberUtility.parseDecimal((newValue * editingPeriod.percent / 100).toFixed(2));
                            entity["quantity" + i] = quantity;
                            editingPeriod.quantity = quantity;
                            entity["amount" + i] = 0;
                            editingPeriod.amount = 0;
                            this.distributeAmount(editingPeriod, quantity, oldValue, true);
                        }
                    }
                }

                if (colDef.field.indexOf("quantity") > -1) {
                    // Set amount in actual objects
                    var editingPeriod = editingRow.periods[colDef["periodNr"] - 1] as BudgetPeriodSalesDTO;
                    if (editingPeriod) {
                        editingPeriod.quantity = newValue;
                        this.distributeAmount(editingPeriod, newValue, oldValue, true);
                    }
                    entity.totalQuantity = NumberUtility.parseDecimal((entity.totalQuantity + newValue - oldValue).toFixed(2));
                }

                this.calculateQuantitySum();
            } else {
                if (colDef.field === "totalAmount" || (colDef.field === "distributionCodeHeadId")) {
                    // Set amount in actual objects
                    // Set value on row
                    editingRow.totalAmount = newValue;

                    //Set value on periods
                    for (var i = 1; i < editingRow.periods.length + 1; i++) {
                        var editingPeriod = editingRow.periods[i - 1] as BudgetPeriodSalesDTO;
                        if (editingPeriod) {
                            var amount = NumberUtility.parseDecimal((newValue * editingPeriod.percent / 100).toFixed(2));
                            entity["amount" + i] = amount;
                            editingPeriod.amount = amount;
                            this.distributeAmount(editingPeriod, amount, oldValue, true);
                        }
                    }
                }

                if (colDef.field.indexOf("amount") > -1) {
                    // Set amount in actual objects
                    var editingPeriod = editingRow.periods[colDef["periodNr"] - 1] as BudgetPeriodSalesDTO;
                    if (editingPeriod) {
                        editingPeriod.amount = newValue;
                        this.distributeAmount(editingPeriod, newValue, oldValue, true);
                    }
                    entity.totalAmount = NumberUtility.parseDecimal((entity.totalAmount + newValue - oldValue).toFixed(2));
                }

                this.calculateSum();
            }

            // Refresh
            this.gridAg.options.refreshRows(entity);
            this.setDirty();
        }
    }

    private afterCellEditDetails(entity: any, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue != oldValue) {
            /*if (this.isTime) {
                if (colDef.field === "totalQuantity") {
                    if (entity.distributionCodeHeadId) {
                        var distributionCode = <any>(_.filter(this.distributionCodes, { distributionCodeHeadId: (entity.distributionCodeHeadId) }))[0];
                        for (var i = 1; i < this.numberOfPeriods + 1; i++) {
                            var period = (distributionCode.periods[i - 1]);
                            if (period) {
                                entity['quantity' + i] = NumberUtility.parseDecimal((entity.totalAmount * period.percent / 100).toFixed(2));
                            } else {
                                entity['quantity' + i] = 0;
                            }
                        }
                    } else {
                        var sum = entity.totalAmount / this.numberOfPeriods;
                        for (var i = 1; i < this.numberOfPeriods + 1; i++) {
                            entity['quantity' + i] = sum;
                        }
                    }
                }
                if (colDef.field.indexOf("quantity") > -1) {
                    entity.totalAmount = 0;
                    for (var i = 1; i < entity.periods.length; i++) {
                        entity.totalAmount = NumberUtility.parseDecimal((entity.totalAmount + entity.periods[i].amount).toFixed(2));
                    }
                }

                this.calculateQuantitySum();
            } else {*/
            if (colDef.field === "distributionCodeName") {
                var code = undefined;//_.find(this.distributionCodes, (c) => { c.name === newValue });
                _.forEach(this.distributionCodes, (c) => {
                    if (c.name === newValue) {
                        code = c;
                    }
                });

                if (code) {
                    // Set current period amount
                    var editingRow = _.find(this.currentCellFirstLevel.periods, { guid: entity.guid });
                    if (editingRow) {
                        // Set new distribution code
                        editingRow.distributionCodeHeadId = code.distributionCodeHeadId;

                        //Check for opening hour
                        var openingHour = code.openingHoursId ? _.find(this.openingHours, { openingHoursId: code.openingHoursId }) : undefined;

                        // Create period
                        var dayPeriod = { guid: this.currentCellSecondLevel.guid, distributionCodeHeadId: code.distributionCodeHeadId, distributionCodeName: code.name, startHour: openingHour ? new Date(openingHour.openingTime).getHours() : 0, closingHour: openingHour ? new Date(openingHour.closingTime).getHours() : 24, lowestLevel: true, locked: true };
                        dayPeriod[this.fieldName] = this.currentCellSecondLevel[this.fieldName];

                        // Create period properties
                        var counter: number = openingHour ? new Date(openingHour.openingTime).getHours() : 1;
                        _.forEach(code.periods, (p) => {
                            dayPeriod[this.fieldName + counter.toString()] = NumberUtility.parseDecimal(((this.currentCellSecondLevel[this.fieldName] * p.percent) / 100).toFixed(2));
                            counter++;
                        });

                        // Set data
                        this.resetDetailsGrid(entity, dayPeriod, code, colDef, this.secondLevelDetailsGridOption);
                    }
                } else {
                    entity.distributionCodeName = oldValue;
                }
            } else if (colDef.field === "amount" || colDef.field === "quantity") {
                this.distributeAmount(this.currentCellFirstLevel, newValue, oldValue, false);
            } else {
                // Set current period amount
                if (entity["startHour"]) {
                    var editingRow = this.currentCellFirstLevel.periods[(colDef["periodNr"] - entity["startHour"] - 1)];
                    if (editingRow) {
                        editingRow[this.fieldName] = newValue;
                        this.tryDistributeAmountToChildPeriods(newValue, editingRow, 24);

                        //Distribute upwards
                        if (editingRow.parentGuid)
                            this.tryDistributeAmountToParentPeriods(editingRow.parentGuid, editingRow.budgetRowNr, newValue, oldValue);
                    }
                } else {
                    var editingRow = this.currentCellFirstLevel.periods[colDef["periodNr"] - 1];
                    if (editingRow) {
                        editingRow[this.fieldName] = newValue;
                        this.tryDistributeAmountToChildPeriods(newValue, editingRow, 24);

                        //Distribute upwards
                        if (editingRow.parentGuid)
                            this.tryDistributeAmountToParentPeriods(editingRow.parentGuid, editingRow.budgetRowNr, newValue, oldValue);
                    }
                }

                //Set total
                entity[this.fieldName] = NumberUtility.parseDecimal((entity[this.fieldName] + newValue - oldValue).toFixed(2));
            }

            this.calculateSum();

            // Refresh
            this.$timeout(() => {
                this.firstLevelDetailsGridOption.refreshRows(entity);
            });
            //}
            this.setDirty();
        }
    }

    private initializeDetailsGrid(entity: any, period: any, distributionCode: DistributionCodeHeadDTO, colDef: any, gridOptions: any) {
        // Set up events
        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.CellFocused, (rowIndex, column, rowPinned, forceBrowserFocus) => { this.cellFocusedDetails(rowIndex, column ? column.colDef : null); }));
        events.push(new GridEvent(SoeGridOptionsEvent.BeginCellEdit, (entity, colDef) => this.beginCellEditDetails(entity, colDef)));
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity: BudgetRowDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) => this.afterCellEditDetails(entity, colDef, newValue, oldValue)));
        gridOptions.subscribe(events);

        //Setup columns
        var aggregations = {};
        if (!distributionCode || distributionCode.subType !== TermGroup_AccountingBudgetSubType.Day) {
            if (distributionCode.subType == TermGroup_AccountingBudgetSubType.Week) {
                // Setup total
                var sumAmountCol = gridOptions.addColumnNumber(this.fieldName, this.terms["common.sum"], null, { enableHiding: false, minWidth: 50, decimals: 2, suppressSorting: true, editable: this.isCellEditable.bind(this) });
                sumAmountCol["detailsName"] = "detail_" + (entity.budgetRowNr - 1);

                // Loops 7 days
                for (var i = 1; i < 8; i++) {
                    var numCol = gridOptions.addColumnNumber(this.fieldName + i.toString(), i.toString(), null, { enableHiding: false, minWidth: 50, decimals: 2, suppressSorting: true, editable: this.isCellEditable.bind(this) });
                    numCol["periodNr"] = i;
                    numCol["detailsName"] = "detail_" + (entity.budgetRowNr - 1);
                }
            }
            else {
                // Setup total
                var sumAmountCol = gridOptions.addColumnNumber(this.fieldName, this.terms["common.sum"], null, { enableHiding: false, minWidth: 50, decimals: 2, suppressSorting: true, editable: this.isCellEditable.bind(this) });
                sumAmountCol["detailsName"] = "detail_" + (entity.budgetRowNr - 1);

                // Loops a maximum of 31 days
                for (var i = 1; i < 32; i++) {
                    var numCol = gridOptions.addColumnNumber(this.fieldName + i.toString(), i.toString(), null, { enableHiding: false, minWidth: 50, decimals: 2, suppressSorting: true, editable: this.isCellEditable.bind(this) });
                    numCol["periodNr"] = i;
                    numCol["detailsName"] = "detail_" + (entity.budgetRowNr - 1);
                }
            }
        } else {
            // Create column for distributioncodes
            var options = new TypeAheadOptionsAg();
            options.source = (filter) => this.filterDistributionCodes(TermGroup_AccountingBudgetSubType.Day, filter);
            options.displayField = "name"
            options.dataField = "name";
            options.minLength = 50;
            options.delay = 0;
            options.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromCodeTypeAhead(value, entity, colDef);
            gridOptions.addColumnTypeAhead("distributionCodeName", this.terms["economy.accounting.distributioncode.distributioncode"], null, { typeAheadOptions: options, displayField: "name", editable: this.isCellEditable.bind(this), suppressSorting: true });

            // Setup total
            var sumAmountCol = gridOptions.addColumnNumber(this.fieldName, this.terms["common.sum"], null, { enableHiding: false, minWidth: 50, decimals: 2, suppressSorting: true, editable: this.isCellEditable.bind(this) });
            sumAmountCol["detailsName"] = "detail_" + (entity.budgetRowNr - 1);

            var openingHour = distributionCode && distributionCode.openingHoursId ? _.find(this.openingHours, { openingHoursId: distributionCode.openingHoursId }) : undefined;
            let openHour = openingHour ? openingHour.openingTime.getHours() : this.earliestOpeningHour;
            let closeHour = openingHour ? openingHour.closingTime.getHours() : this.latestClosingHour;

            // Loops a maximum of 24 hours
            for (var i = openHour; i < closeHour; i++) {
                var numCol = gridOptions.addColumnNumber(this.fieldName + i.toString(), i.toString() + ":00", null, { enableHiding: false, minWidth: 50, decimals: 2, suppressSorting: true, editable: this.isPeriodCellEditable.bind(this) });
                numCol["periodNr"] = i;
                numCol["detailsName"] = "detail_" + (entity.budgetRowNr - 1);
            }
        }

        // Finalize grid
        gridOptions.finalizeInitGrid();
    }

    private resetDetailsGrid(entity: any, period: any, distributionCode: any, colDef: any, gridOptions: ISoeGridOptionsAg) {
        gridOptions.setData([period]);
    }

    public distributeAmount(entity: any, newAmount: number, oldAmount: number, ignoreDistributeUpwards: boolean) {
        if (entity.periods) {
            for (var i = 1; i < entity.periods.length + 1; i++) {
                var period = entity.periods[i - 1];
                if (period) {
                    entity[this.fieldName + i] = period[this.fieldName] = NumberUtility.parseDecimal((newAmount * period.percent / 100).toFixed(2));
                    this.tryDistributeAmountToChildPeriods(entity[this.fieldName + i], period, 24);

                    //Distribute upwards
                    if (period.parentGuid && !ignoreDistributeUpwards)
                        this.tryDistributeAmountToParentPeriods(period.parentGuid, period.budgetRowNr, newAmount, oldAmount)
                } else {
                    entity[this.fieldName + i] = 0;
                }
            }
        }

        //Set total
        entity[this.fieldName] = newAmount;
    }

    public tryDistributeAmountToParentPeriods(parentGuid: Guid, rowNr: number, newAmount: number, oldAmount: number) {
        var periodFound: boolean = false;
        var currentRow = _.find(this.budgetHead.rows, (r) => r.budgetRowNr === rowNr);
        if (currentRow) {
            for (var i = 1; i < currentRow.periods.length + 1; i++) {
                if (periodFound)
                    break;

                var per_x = currentRow.periods[i - 1];
                if (per_x.guid === parentGuid) {
                    if (this.isTime) //this.fieldName
                        currentRow.totalQuantity = this.currentClonedRow["totalQuantity"] = NumberUtility.parseDecimal((currentRow.totalQuantity + newAmount - oldAmount).toFixed(2));
                    else
                        currentRow.totalAmount = this.currentClonedRow["totalAmount"] = NumberUtility.parseDecimal((currentRow.totalAmount + newAmount - oldAmount).toFixed(2));

                    this.currentClonedRow[this.fieldName + i] = NumberUtility.parseDecimal((this.currentClonedRow[this.fieldName + i] + newAmount - oldAmount).toFixed(2));
                    per_x[this.fieldName] = NumberUtility.parseDecimal((per_x[this.fieldName] + newAmount - oldAmount).toFixed(2));

                    // Refresh
                    this.gridAg.options.refreshRowsIgnoreFocus(this.currentClonedRow);
                    break;
                }
            }
        }
    }

    public tryDistributeAmountToChildPeriods(amount: number, period: any, maxPeriods: number) {
        // Loop periods of current period to distribute amount
        if (period.periods) {
            var startHour: number = period["startHour"] ? period["startHour"] : 1;
            for (var i = 1; i < maxPeriods + 1; i++) {
                var childPeriod = period.periods[i - startHour];
                period[this.fieldName + i] = childPeriod ? childPeriod[this.fieldName] = NumberUtility.parseDecimal(((amount * childPeriod.percent) / 100).toFixed(2)) : 0;
            }

            period["isFlattened"] = true;
        }
    }

    private isCellEditable(row) {
        return !this.isDisabled && this.modifyPermission;
    }

    private isPeriodCellEditable(row) {
        return !this.isDisabled && this.modifyPermission && !row.locked;
    }

    private deleteBudgetRow(row: BudgetRowDTO) {
        var budgetRow = _.find(this.budgetHead.rows, (r) => r.budgetRowNr === row.budgetRowNr);
        if (budgetRow)
            budgetRow.isDeleted = true;

        this.setRowsWithoutDeleted(false);

        this.setDirty();
    }

    private setRowsWithoutDeleted(setFocus: boolean = true) {
        if (this.budgetHead) {
            var rows = [];
            _.forEach(this.budgetHead.rows, (r) => {
                if (!r.isDeleted) {
                    // Create copy
                    var clone = { isModified: r.isModified, budgetRowNr: r.budgetRowNr, totalAmount: r.totalAmount, totalQuantity: r.totalQuantity };

                    // Set internal accounts
                    clone["dim1Id"] = r.dim1Id;
                    clone["dim1Nr"] = r.dim1Nr;
                    clone["dim1Name"] = r.dim1Name;
                    clone["dim2Id"] = r.dim2Id;
                    clone["dim2Nr"] = r.dim2Nr;
                    clone["dim2Name"] = r.dim2Name;
                    clone["dim3Id"] = r.dim3Id;
                    clone["dim3Nr"] = r.dim3Nr;
                    clone["dim3Name"] = r.dim3Name;
                    clone["dim4Id"] = r.dim4Id;
                    clone["dim4Nr"] = r.dim4Nr;
                    clone["dim4Name"] = r.dim4Name;
                    clone["dim5Id"] = r.dim5Id;
                    clone["dim5Nr"] = r.dim5Nr;
                    clone["dim5Name"] = r.dim5Name;
                    clone["dim6Id"] = r.dim6Id;
                    clone["dim6Nr"] = r.dim6Nr;
                    clone["dim6Name"] = r.dim6Name;

                    var counter: number = 1;
                    _.forEach(r.periods, (p) => {
                        clone["amount" + counter.toString()] = p.amount;
                        clone["quantity" + counter.toString()] = p.quantity;
                        counter++;
                    });
                    rows.push(clone);
                }
            });

            this.$timeout(() => {
                this.gridAg.options.setData(rows);
                if (setFocus) {
                    let colDefs = this.gridAg.options.getColumnDefs();
                    _.forEach(colDefs, colDef => {
                        let def = this.gridAg.options.getColumnByField(colDef.field);
                        if (def && def.isVisible()) {
                            this.gridAg.options.startEditingCell(rows[rows.length - 1], colDef);
                            return false;
                        }
                    });
                }
            }, 500);
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
                    this.gridAg.options.startEditingCell(nextRowResult.rowNode.data, this.isTime ? this.gridAg.options.getColumnByField('totalQuantity') : this.gridAg.options.getColumnByField('totalAmount'));
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

    public distributionChanged(row): void {
        var obj = (_.filter(this.distributionCodesDict, { id: row.distributionCodeHeadId }))[0];
        if (obj) {
            row.distributionCodeHeadId = obj["id"];
            row.distributionCodeHeadName = obj["name"];
        }
    }

    public filterAccounts(dimNr, filter) {
        var accountDim = _.find(this.accountDims, a => a.accountDimNr === dimNr);
        if (accountDim) {
            return _.orderBy(accountDim.accounts.filter(acc => {
                return acc.name.contains(filter);
            }), 'name');
        }
        else {
            return [];
        }
    }

    public filterDistributionCodes(subType, filter) {
        var codes = _.filter(this.distributionCodes, d => d.subType === subType);
        if (codes) {
            return _.orderBy(codes.filter(c => {
                return c.name.contains(filter);
            }), 'distributionCodeName');
        }
        else {
            return [];
        }
    }

    private openPreviousPeriodResultDialog(row: BudgetRowDTO) {
        var result: any = [];
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getViewUrl("PreviousPeriodResult.html"),
            controller: SalesBudgetPreviousPeriodResultController,
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

    protected allowNavigationFromTypeAhead(value, entity, colDef) {
        if (!value)
            return true;

        var accountDim = _.find(this.accountDims, a => a.accountDimNr === colDef['additionalData'].dimNr);
        if (accountDim) {
            var valueHasMatchingAccount = accountDim.accounts.filter(acc => acc.name === value);

            if (valueHasMatchingAccount.length) //if there is a value and it is valid, allow it.
                return true;
        }

        return false;
    }

    protected allowNavigationFromCodeTypeAhead(value, entity, colDef) {
        if (!value)
            return true;

        var code = _.find(this.distributionCodes, { name: value });
        if (code) {
            return true;
        }

        return false;
    }

    private setNumberOfPeriods() {
        if (this.budgetHead) {
            this.budgetHead.noOfPeriods = this.numberOfPeriods;
            this.setupGridColumns();
        }
    }

    private typeChanged() {
        if (this.budgetHead && this.budgetHead.rows) {
            this.setupGridColumns();
        }
        this.fieldName = this.isTime ? "quantity" : "amount";
    }

    private distributionCodeChanged() {

        if (this.budgetHead && this.budgetHead.rows) {
            var distributionCode = _.find(this.distributionCodes, c => c.distributionCodeHeadId === this.distributionCodeId)
            if (distributionCode) {
                this.currentDistributionCode = distributionCode;
                this.budgetHead.distributionCodeHeadId = this.currentDistributionCode.distributionCodeHeadId;
                this.budgetHead.noOfPeriods = this.numberOfPeriods = this.currentDistributionCode.noOfPeriods;
                this.intervalType = this.currentDistributionCode.subType;
                this.setupGridColumns();
            }
        }
    }

    private showColumnDim2() {
        this.gridAg.options.showColumn("dim2Nr");
    }

    private showColumnDim3() {
        this.gridAg.options.showColumn("dim3Nr");
    }

    private calculateSum() {
        if (this.budgetHead && this.budgetHead.rows) {
            this.sumCalculations = [];
            for (var j = 0; j < this.budgetHead.rows.length; j++) {
                for (var i = 1; i < this.numberOfPeriods + 1; i++) {
                    if (!this.sumCalculations[i - 1]) {
                        this.sumCalculations.push({ id: i, sum: 0 });
                    }
                    var sumrow = <any>(_.filter(this.sumCalculations, { id: i }))[0];
                    sumrow.sum += +this.budgetHead.rows[j]['amount' + i];
                }
            }
        }
    }

    private calculateQuantitySum() {
        if (this.budgetHead && this.budgetHead.rows) {
            this.sumCalculations = [];
            for (var j = 0; j < this.budgetHead.rows.length; j++) {
                for (var i = 1; i < this.numberOfPeriods + 1; i++) {
                    if (!this.sumCalculations[i - 1]) {
                        this.sumCalculations.push({ id: i, sum: 0 });
                    }
                    var sumrow = <any>(_.filter(this.sumCalculations, { id: i }))[0];
                    sumrow.sum += +this.budgetHead.rows[j]['quantity' + i];
                }
            }
        }
    }

    private setDirty() {
        this.$scope.$emit("rowUpdated", this.guid);
    }

    progress: IProgressHandler;
    toolbar: IToolbar;
    gridAg: IGridHandlerAg;
    grid: IGridHandler;
    modifyPermission: boolean;
    readOnlyPermission: boolean;
}


