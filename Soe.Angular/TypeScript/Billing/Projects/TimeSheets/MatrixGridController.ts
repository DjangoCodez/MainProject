import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { Feature, CompanySettingType, SoeTimeCodeType, UserSettingType, SettingMainType } from "../../../Util/CommonEnumerations";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { TimeColumnOptions, IColumnAggregations, IColumnAggregate } from "../../../Util/SoeGridOptionsAg";
import { IEmployeeTimeCodeDTO, ITimeDeviationCauseDTO, ITimeCodeDTO, IActionResult } from "../../../Scripts/TypeLite.Net4";
import { IProjectService } from "../../../Shared/Billing/Projects/ProjectService";
import { ProjectTimeBlockDTO } from "../../../Common/Models/ProjectDTO";
import { EditNoteController } from "../../../Common/Directives/TimeProjectReport/EditNoteController";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { DayOfWeek, SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage, SOEMessageBoxSize, TimeProjectButtonFunctions } from "../../../Util/Enumerations";
import { EditTimeHelper } from "../../../Common/Directives/TimeProjectReport/EditTimeHelper";
import { ProjectTimeMatrixDTO, ProjectTimeMatrixSaveDTO } from "../../../Common/Models/ProjectTimeMatrixDTO";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IShortCutService } from "../../../Core/Services/ShortCutService";
import { SelectChildController } from "../Dialogs/SelectChild/SelectChildController";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class MatrixGridController extends GridControllerBase2Ag implements ICompositionGridController {

    private employeeId: number;
    private projectTimeBlockRows: ProjectTimeMatrixDTO[] = [];

    //gui
    private gridFooterComponentUrl: any;
    private buttonFunctionsHeader: any = [];
    private buttonFunctionsFooter: any = [];
    private invoicedQuantityTotalFormated: string;
    private timePayrollQuantityTotalFormatted: string;
    private hasSelectedRows: boolean;
    private hasFetchedData: boolean;

    //settings
    private useExtendedTimeRegistration = false;
    private invoiceTimeAsWorkTime = false;

    //permissions
    private invoiceTimePermission = false;
    private workTimePermission = false;
    private modifyOtherEmployeesPermission = false;

    private timeProjectFrom: Date;
    private timeProjectTo: Date;
    private employee: IEmployeeTimeCodeDTO;
    private employeesDict: any[] = [];
    private employees: IEmployeeTimeCodeDTO[] = [];

    private selectedEmployee: IEmployeeTimeCodeDTO;
    private weekNr: number = undefined;

    private allProjects: any[] = [];
    private allProjectsAndInvoices: any[] = [];
    private allOrders: any[] = [];
    private filteredProjectsDict: any[] = [];
    private filteredOrdersDict: any[] = [];
    private projectInvoices: any[] = [];
    private isProjectsLoaded = false;
    private isOrdersLoaded = false;

    private _selectedEmployeeId = 0;
    get selectedEmployeeId() {
        return this._selectedEmployeeId;
    }
    set selectedEmployeeId(id: any) {
        if (this._selectedEmployeeId !== id) {
            this.employeeChange(id);
        }
    }

    private _showWeekend = false;
    get showWeekend() {
        return this._showWeekend;
    }
    set showWeekend(item: boolean) {
        if (this._showWeekend !== item) {
            this._showWeekend = item;
            this.toggleWeekendColumns(true, this.showWeekend);
        }
    }

    private timeCodes: ITimeCodeDTO[] = [];
    private timeCodesDict: any[] = [];
    private timeDeviationCauses: ITimeDeviationCauseDTO[] = [];

    private editTimeHelper: EditTimeHelper;
    private activated = false;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $uibModal,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private projectService: IProjectService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        shortCutService: IShortCutService,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Billing.Projects.Project.Matrix", progressHandlerFactory, messagingHandlerFactory);

        this.gridFooterComponentUrl = urlHelperService.getViewUrl("matrixGridFooter.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.doLookup())
            .onSetUpGrid(() => this.setupGrid())
            .onBeforeSetUpGrid(() => this.onBeforeSetUpGrid())
            //.onLoadGridData(() => this.loadGridData());
        
        this.employeeId = soeConfig.employeeId;
        shortCutService.bindSave($scope, () => { this.save(); });
        shortCutService.bindNew($scope, () => { this.addRow(); });

        this.editTimeHelper = new EditTimeHelper(coreService, this.$q, (employeeId: number) => { return this.getEmployee(employeeId) });

        this.onTabActivated(() => this.tabActivated());
    }
        

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.loadGridData());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("matrixGridHeader.html"));
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;
    }
    
    private tabActivated() {
        if (!this.activated) {
            this.timeProjectFrom = CalendarUtility.getDateToday().beginningOfWeek();
            this.timeProjectTo = this.timeProjectFrom.endOfWeek();
            this.weekNr = this.timeProjectFrom.week();
            this.setupFunctions();
            this.setupWatches();
            this.flowHandler.start([
                { feature: Feature.Billing_Project_List, loadReadPermissions: false, loadModifyPermissions: true },
                { feature: Feature.Time_Project_Invoice_WorkedTime, loadReadPermissions: false, loadModifyPermissions: true },
                { feature: Feature.Time_Project_Invoice_InvoicedTime, loadReadPermissions: false, loadModifyPermissions: true },
                { feature: Feature.Billing_Project_TimeSheetUser_OtherEmployees, loadReadPermissions: false, loadModifyPermissions: true }
            ]
            );
            this.activated = true;
        }
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.workTimePermission = response[Feature.Time_Project_Invoice_WorkedTime].modifyPermission;
        this.invoiceTimePermission = response[Feature.Time_Project_Invoice_InvoicedTime].modifyPermission;
        this.modifyOtherEmployeesPermission = response[Feature.Billing_Project_TimeSheetUser_OtherEmployees].modifyPermission;
    }

    private doLookup(): ng.IPromise<any> {
        return this.$q.all([this.loadEmployee(), this.loadUserSettings()]).then(() => {
            this.loadEmployees().then(() => {
                if (this.employeeId) {
                    this.selectedEmployeeId = this.employeeId;
                }
            })
        });
    }

    private onBeforeSetUpGrid(): ng.IPromise<any> {
        return this.loadCompanySettings().then(() => { this.$q.all([this.loadTimeCodes()]) });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            CompanySettingType.ProjectUseExtendedTimeRegistration,
            CompanySettingType.ProjectInvoiceTimeAsWorkTime
        ];
        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useExtendedTimeRegistration = x[CompanySettingType.ProjectUseExtendedTimeRegistration];
            this.invoiceTimeAsWorkTime = x[CompanySettingType.ProjectInvoiceTimeAsWorkTime];
        });
    }

    protected loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.TimeSheetShowWeekend];
        
        return this.coreService.getUserSettings(settingTypes).then(x => {
            this._showWeekend = SettingsUtility.getBoolUserSetting(x, UserSettingType.TimeSheetShowWeekend, false);
        });
    }

    private skipDateChange = false;
    private setupWatches() {    
        this.$scope.$watch(() => this.filteredProjectsDict, (newValue: any, oldValue: any) => {
            if (newValue.length > 0)
                this.isProjectsLoaded = true;
            else
                this.isProjectsLoaded = false;
        });
        this.$scope.$watch(() => this.filteredOrdersDict, (newValue: any, oldValue: any) => {
            if (newValue.length > 0)
                this.isOrdersLoaded = true;
            else 
                this.isOrdersLoaded = false;
        });


        this.$scope.$watch(() => this.timeProjectFrom, (newValue: Date, oldValue: Date) => {
            if ((newValue && !oldValue || !(newValue.isSameDayAs(oldValue)))) {
                if (this.skipDateChange) {
                    this.skipDateChange = false;
                    return;
                }

                if (this.isDirty) {
                    this.showDirtyConfirmationDialog().then((result: boolean) => {
                        if (result) {
                            this.projectDateChanged(newValue, true);
                        }
                        else {
                            this.skipDateChange = true;
                            this.projectDateChanged(oldValue, false);
                        }
                    });
                }
                else {
                    this.projectDateChanged(newValue, true);
                }
            }
        });
    }

    private projectDateChanged(newDate: Date, clearData: boolean) {
        this.timeProjectFrom = newDate.beginningOfWeek();
        this.timeProjectTo = this.timeProjectFrom.endOfWeek();
        this.weekNr = this.timeProjectFrom.week();
        if (clearData) {
            this.updateGrid(true, false);
            if (this.modifyOtherEmployeesPermission) {
                this.loadEmployees();
            }
        }
    }

    private setupFunctions() {
        const keys: string[] = [
            "core.save",
            "common.newrow",
            "core.deleterow",
            "billing.project.timesheet.copylastweek"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            this.buttonFunctionsFooter.push({ id: TimeProjectButtonFunctions.Save, name: terms["core.save"] + " (Ctrl+S)"});

            this.buttonFunctionsHeader.push({ id: TimeProjectButtonFunctions.AddRow, name: terms["common.newrow"] + " (Ctrl+R)", icon: 'fal fa-plus'});
            this.buttonFunctionsHeader.push({ id: TimeProjectButtonFunctions.DeleteRow, name: terms["core.deleterow"], icon: 'fal fa-times iconDelete', disabled: () => { return !this.hasSelectedRows } });

            this.buttonFunctionsHeader.push({ id: TimeProjectButtonFunctions.CopyLastWeek, name: terms["billing.project.timesheet.copylastweek"], icon: 'fal fa-clone', disabled: () => { return this.projectTimeBlockRows.length > 0 } });
        })
    }

    private setupGrid() {
        const weekdays = CalendarUtility.getDayOfWeekNames(true);
        
        const keys: string[] = [
            "billing.project.timesheet.chargingtype",
            "billing.project.timesheet.invoice",
            "common.time.timedeviationcause",
            "billing.project.project",
            "billing.project.timesheet.workedtime",
            "billing.project.timesheet.invoicedtime",
            "common.customer.customer.customer",
            "common.total"
        ];

        const timeColumnOptions: TimeColumnOptions = {
            cellClassRules: {
                "excelTime": () => true,
            },
            enableRowGrouping: true,
            editable: (data: ProjectTimeMatrixDTO, field: string) => data.isEditable(field),
            cellStyle: (data: ProjectTimeMatrixDTO, field: string) => data.getCellStyle(field),
            suppressMovable: true,
            clearZero: false, alignLeft: false, minDigits: 5 //, aggFuncOnGrouping: "sumTimeSpan"
        };

        const timeSpanColumnAggregate = {
            getSeed: () => 0,
            accumulator: (acc, next) => CalendarUtility.sumTimeSpan(acc, next),
            cellRenderer: this.timeSpanAggregateRenderer.bind(this)
        } as IColumnAggregate;

        this.translationService.translateMany(keys).then((terms) => {

            this.gridAg.addColumnIsModified("isGridRowModified", "");
            this.gridAg.options.addColumnIcon("childIcon", "", null, {suppressFilter:true, pinned:"left", cellClassRules: { "warningColor": () => true }, onClick: this.selectChildren.bind(this) });

            const colHeaderMainInfo = this.gridAg.options.addColumnHeader("mainInfo", "", null);

            const invoiceOptions = this.editTimeHelper.createTypeAheadOptions();
            invoiceOptions.source = (filter) => this.filterOrders(filter);
            this.gridAg.addColumnTypeAhead("invoiceNr", terms["billing.project.timesheet.invoice"], null, { typeAheadOptions: invoiceOptions, editable: (data: ProjectTimeMatrixDTO) => data.isOrderEditable(), suppressSorting: false, suppressMovable: true }, null, colHeaderMainInfo);

            const projectOptions = this.editTimeHelper.createTypeAheadOptions();
            projectOptions.source = (filter) => this.filterProjects(filter);
            this.gridAg.addColumnTypeAhead("projectName", terms["billing.project.project"], null, { typeAheadOptions: projectOptions, editable: (data: ProjectTimeMatrixDTO) => data.isOrderEditable(), suppressSorting: false,suppressMovable:true }, null, colHeaderMainInfo);

            this.gridAg.options.addColumnText("customerName", terms["common.customer.customer.customer"], null, { suppressMovable: true, editable: false}, colHeaderMainInfo);

            if (this.useExtendedTimeRegistration) {
                const timeDevationCauseOptions = this.editTimeHelper.createTypeAheadOptions("name");
                timeDevationCauseOptions.source = (filter) => this.filterTimeDeviationCauses(filter);

                this.gridAg.addColumnTypeAhead("timeDeviationCauseName", terms["common.time.timedeviationcause"], null, { typeAheadOptions: timeDevationCauseOptions, editable: (data: ProjectTimeMatrixDTO) => data.isTimeDevationCauseEditable(), suppressSorting: false, suppressMovable: true }, null, colHeaderMainInfo);
            }

            const timeCodeOptions = this.editTimeHelper.createTypeAheadOptions("name");
            timeCodeOptions.source = (filter) => this.filterTimeCodes(filter);
            this.gridAg.addColumnTypeAhead("timeCodeName", terms["billing.project.timesheet.chargingtype"], null, { typeAheadOptions: timeCodeOptions, editable: (data: ProjectTimeMatrixDTO) => data.isTimeCodeEditable(), suppressSorting: false, suppressMovable: true }, null, colHeaderMainInfo);

            const footerAggregations: any = {};

            weekdays.forEach((day) => {
                const columnSuffix = day.id.toString(); 
                const colHeaderDay = this.gridAg.options.addColumnHeader("day" + columnSuffix, day.name, null);
                colHeaderDay.marryChildren = true;

                if (this.workTimePermission) {
                    this.gridAg.options.addColumnTimeSpan("timePayrollQuantityFormatted_" + columnSuffix, terms["billing.project.timesheet.workedtime"], null, timeColumnOptions, colHeaderDay);
                    footerAggregations["timePayrollQuantityFormatted_" + columnSuffix] = timeSpanColumnAggregate;
                }
                if (this.invoiceTimePermission) {
                    this.gridAg.options.addColumnTimeSpan("invoiceQuantityFormatted_" + columnSuffix, terms["billing.project.timesheet.invoicedtime"], null, timeColumnOptions, colHeaderDay);
                    footerAggregations["invoiceQuantityFormatted_" + columnSuffix] = timeSpanColumnAggregate;
                }
                
                if (day.id === DayOfWeek.Sunday) { day.id = 7 }
                this.gridAg.options.addColumnIcon("noteIcon_" + columnSuffix, "", null, { onClick: this.showNote.bind(this, day.id) }, colHeaderDay);
            });

            //Row totals
            const timeTotalColumnOptions: TimeColumnOptions = {
                cellClassRules: {
                    "excelTime": () => true,
                },
                enableRowGrouping: true,
                editable: false,
                suppressMovable: true,
                clearZero: false, alignLeft: false, minDigits: 5 
            };

            const colHeaderTotal = this.gridAg.options.addColumnHeader("total", terms["common.total"], null);
            if (this.workTimePermission) {
                const column = this.gridAg.options.addColumnTimeSpan("timePayrollQuantityFormatted_Total", terms["billing.project.timesheet.workedtime"], null, timeTotalColumnOptions, colHeaderTotal);
                column.cellClass = "indiscreet";
                footerAggregations["timePayrollQuantityFormatted_Total"] = timeSpanColumnAggregate;
            }
            if (this.invoiceTimePermission) {
                const column = this.gridAg.options.addColumnTimeSpan("invoiceQuantityFormatted_Total", terms["billing.project.timesheet.invoicedtime"], null, timeTotalColumnOptions, colHeaderTotal);
                column.cellClass = "indiscreet";
                footerAggregations["invoiceQuantityFormatted_Total"] = timeSpanColumnAggregate;
            }

            this.gridAg.options.addFooterRow("#sum-footer-grid", footerAggregations as IColumnAggregations);

            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.IsRowSelectable, (rowNode) => {
                return rowNode.data && rowNode.data.isEditableAll();
            }));
            events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rows: ProjectTimeMatrixDTO[]) => {
                this.hasSelectedRows = (Array.isArray(rows) && rows.length > 0);
                this.$scope.$applyAsync();
            }));

            this.gridAg.options.subscribe(events);

            this.gridAg.options.customTabToCellHandler = (params) => this.handleNavigateToNextCell(params);

            this.gridAg.finalizeInitGrid("billing.project.timesheet.weekreport", true);

            this.$timeout(() => { this.toggleWeekendColumns(false, this.showWeekend); }, 250 )
        })
    }

    private timeSpanAggregateRenderer({ data, colDef }) {
        let value: string = data[colDef.field];
        if (!value)
            value = "00:00";
        if (value.length === 4)
            value = "0" + value;

        return "<b>" + value + "<b>";
    }

    private afterCellEdit(row: ProjectTimeMatrixDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;
        
        switch (colDef.field) {
            case 'projectName':
                {
                    const project = (_.find(this.filteredProjectsDict, ['label', newValue]));
                    if (project) {
                        if (project.id !== row.projectId) {
                            row.projectChanged(project.id, project.label);
                            const projectOrders = this.allOrders.filter(x => x.projectId === project.id);
                            if (projectOrders.length > 0) {
                                const order = projectOrders[0];
                                row.orderChanged(order.id, order.invoiceNr);
                                this.gridAg.options.refreshRows(row);
                            }
                        }
                    }
                    else {
                        row.projectChanged(0, "");
                    }
                }
                break;
            case 'invoiceNr':
                {
                    const order = (_.find(this.filteredOrdersDict, ['label', newValue]));

                    if (order) {
                        if (order.id !== row.customerInvoiceId) {
                            row.orderChanged(order.id, order.label);
                            const order2 = this.allOrders.find(x => x.id === order.id);
                            if (order2 && order2.projectId && order2.projectId !== row.projectId) {
                                const project = (_.find(this.filteredProjectsDict, ['id', order2.projectId]));
                                if (project) {
                                    row.projectChanged(project.id, project.label);
                                }
                            }
                            this.gridAg.options.refreshRows(row);
                        }
                    }
                    else {
                        row.orderChanged(0, "");
                    }
                }
                break;
            case 'timeCodeName':
                {
                    const timeCode = this.timeCodes.find(t => t.name === newValue);
                    row.timeCodeChanged(timeCode);
                    this.gridAg.options.refreshRows(row);
                }
                break;
            case 'timeDeviationCauseName':
                {
                    const deviationCause = this.timeDeviationCauses.find(t => t.name === newValue);
                    row.timeDeviationCauseChanged(deviationCause);
                    if (deviationCause && !row.timeCodeReadOnly) {
                        this.setDefaultTimeCode(row, deviationCause);
                    }
                    this.gridAg.options.refreshRows(row);
                }
                break;
        }

        if (!this.isDirty && row.isGridRowModified) {
            this.isDirty = true;
            this.$scope.$applyAsync();
        }
    }

    private handleNavigateToNextCell(params: any, skipChecks?: boolean): { rowIndex: number, column: any } {
        let { nextCellPosition, previousCellPosition } = params;

        if (!nextCellPosition) {
            nextCellPosition = previousCellPosition;
            this.$timeout(() => { this.addRow();  } )
        }

        return { rowIndex: nextCellPosition.rowIndex, column: nextCellPosition.column };
    }

    private filterProjects(filter) {
        return _.orderBy(this.filteredProjectsDict.filter(p => {
            return p.label.contains(filter);
        }), 'label');
    }

    private filterOrders(filter) {
        return _.orderBy(this.filteredOrdersDict.filter(p => {
            return p.label.contains(filter);
        }), 'label');
    }

    private filterTimeCodes(filter) {
        return _.orderBy(this.timeCodes.filter(p => {
            return p.name.contains(filter);
        }), 'name');
    }

    private filterTimeDeviationCauses(filter) {
        return _.orderBy(this.timeDeviationCauses.filter(p => {
            return p.name.contains(filter);
        }), 'name');
    }

    private getTimeCode(timeCodeId: number): ITimeCodeDTO {
        return (_.find(this.timeCodes, ['timeCodeId', timeCodeId]));
    }

    private getTimeDeviationCause(timeDeviationCauseId: number): ITimeDeviationCauseDTO {
        return (_.find(this.timeDeviationCauses, ['timeDeviationCauseId', timeDeviationCauseId]));
    }

    private getEmployee(employeeId: number): any {
        return this.employees.find(e => e.employeeId === employeeId);
    }

    private setDefaultTimeCode(row: ProjectTimeMatrixDTO, deviationCause: ITimeDeviationCauseDTO) {
        if (!deviationCause && row.timeDeviationCauseId) {
            deviationCause = this.getTimeDeviationCause(row.timeDeviationCauseId);
        }
        let timeCode: ITimeCodeDTO = null; 
        if (!row.timeCodeId && deviationCause) {
            if (deviationCause.timeCodeId) {
                timeCode = this.getTimeCode(deviationCause.timeCodeId);
            }
        }

        if (!timeCode && this.selectedEmployee.defaultTimeCodeId) {
            timeCode = this.getTimeCode(this.selectedEmployee.defaultTimeCodeId);
        }

        if (timeCode) {
            row.timeCodeChanged(timeCode);
        }
    }

    private setDefaultTimeDeviationCause(row: ProjectTimeMatrixDTO) {
        if (this.selectedEmployee.timeDeviationCauseId) {
            const timeDeviationCause = this.getTimeDeviationCause(this.selectedEmployee.timeDeviationCauseId);
            row.timeDeviationCauseChanged(timeDeviationCause);
        }
    }

    private showDirtyConfirmationDialog(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();
        this.notificationService.showConfirmOnContinue().then(close => {
            if (close) {
                this.isDirty = false;
                deferral.resolve(true);
            } else {
                deferral.resolve(false);
                return;
            }
        });

        return deferral.promise;
    }

    private loadGridData() {
        if (!this.selectedEmployeeId) {
            this.updateGrid(true);
            return;
        }

        if (this.isDirty) {
            this.showDirtyConfirmationDialog().then( (ok:boolean) => {
                if (ok) {
                    this.loadGridData();
                }
            });
            return;
        }

        this.progress.startLoadingProgress([() => {
            return this.projectService.getProjectTimeBlocksForMatrix(this.employeeId, this.selectedEmployeeId, this.timeProjectFrom, this.timeProjectTo, false).then((data: ProjectTimeMatrixDTO[]) => {

                this.projectTimeBlockRows = data.map(tb => {
                    const obj = new ProjectTimeMatrixDTO();
                    angular.extend(obj, tb);
                    return obj;
                });
                this.updateGrid();
                this.checkWeekends();
            })
        }])
    }

    private updateGrid(clearRows=false, hasFetchedData = true) {
        if (clearRows) {
            this.projectTimeBlockRows = [];
        }
        this.setData(this.projectTimeBlockRows);
        this.updateTotals();
        this.hasSelectedRows = false;
        this.setValuesAfterLoad();
        this.hasFetchedData = hasFetchedData;
    }

    private setValuesAfterLoad() {
        if (this.useExtendedTimeRegistration) {
            this.projectTimeBlockRows.forEach(p => {
                const deviationCause = this.timeDeviationCauses.find(t => t.timeDeviationCauseId === p.timeDeviationCauseId);
                p.timeDeviationCauseChanged(deviationCause);
            });
            this.gridAg.options.refreshRows();
        }
    }

    private updateTotals() {
        let invoiceQuantity = 0;
        let timePayrollQuantity = 0;
        for (const element of this.projectTimeBlockRows) {
            const row = element;
            timePayrollQuantity += row.getTimePayrollQuantity_Total();
            invoiceQuantity += row.getInvoiceQuantity_Total();
        }
        if (this.workTimePermission) {
            this.invoicedQuantityTotalFormated = CalendarUtility.minutesToTimeSpan(invoiceQuantity, false, false, true);
        }
        if (this.invoiceTimePermission) {
            this.timePayrollQuantityTotalFormatted = CalendarUtility.minutesToTimeSpan(timePayrollQuantity,false,false,true);
        }
    }

    private loadEmployee(): ng.IPromise<any> {
        return this.projectService.getEmployeeForUserWithTimeCode(this.timeProjectFrom).then(x => {
            this.employee = x;
        });
    }

    private loadEmployees(): ng.IPromise<any> {
        const deferral = this.$q.defer();
        this.employees = [];
        this.employeesDict.length = 0;

        if (this.modifyOtherEmployeesPermission) {
            this.projectService.getEmployeesForProjectTimeCode(false, false, false, this.employeeId, this.timeProjectFrom, this.timeProjectTo).then(x => {
                this.employees = x;
                this.employeesDict.length = 0;
                _.forEach(x, (e) => {
                    this.employeesDict.push({ id: e.employeeId, name: e.name + " (" + e.employeeNr + ")" });
                });

                if (this.selectedEmployeeId && !this.employees.find(x => x.employeeId === this.selectedEmployeeId)) {
                    this._selectedEmployeeId = 0;
                    this.selectedEmployee = undefined;
                }
                deferral.resolve();
            });
        } else {
            //this.employees.push({ employeeId: this.employee.employeeId, name: this.employee.name, employeeNr: this.employee.employeeNr, defaultTimeCodeId: this.employee.defaultTimeCodeId, timeDeviationCauseId: this.employee.timeDeviationCauseId, employeeGroupId: this.employee.employeeGroupId, autoGenTimeAndBreakForProject: this.employee.autoGenTimeAndBreakForProject });
            this.employeesDict.push({ id: this.employee.employeeId, name: this.employee.name });
            this.employees.push(this.employee);
            deferral.resolve();
        }

        return deferral.promise;
    }

    private loadTimeCodes(): ng.IPromise<any> {
        const type = this.useExtendedTimeRegistration ? SoeTimeCodeType.Work : SoeTimeCodeType.WorkAndAbsense;
        const onlyWithProducts = this.useExtendedTimeRegistration ? true : false;
        return this.projectService.getTimeCodesByType(type, true, false, onlyWithProducts).then((x) => {
            this.timeCodes = x;
            _.forEach(this.timeCodes, (t) => {
                this.timeCodesDict.push({ value: t.timeCodeId, label: t.name });
            });
        });
    }

    private loadTimeDeviationCausesForEmployee(employeeGroupId: number) {
        this.projectService.getTimeDeviationCauses(employeeGroupId, false, true).then((x:ITimeDeviationCauseDTO[]) => {
            this.timeDeviationCauses = x;
        });
    }

    private loadProjectInvoices(employeeId: number): ng.IPromise<any[]> {
        //var employeeIds = _.map(this.employees, e => e.employeeId);
        const deferral = this.$q.defer<any[]>();
        const employeeIds = [employeeId];
        this.projectService.getProjectsForTimeSheetEmployees(employeeIds).then((result: any[]) => {
            this.projectInvoices = this.projectInvoices.concat(result);
            this.allProjectsAndInvoices = [];
            this.allProjects = [];
            this.filteredProjectsDict = [];
            this.allOrders = [];
            this.filteredOrdersDict = [];

            this.filteredOrdersDict.push({ id: undefined, label: " " });
            this.filteredProjectsDict.push({ id: undefined, label: " " });

            for (let e of this.projectInvoices) {
                //Filter projects
                for (let p of e.projects) {
                    if (_.filter(this.allProjects, x => x.id === p.projectId).length === 0) {
                        this.allProjectsAndInvoices.push(p);
                        this.allProjects.push({ id: p.projectId, label: p.numberName });
                        this.filteredProjectsDict.push({ id: p.projectId, label: p.numberName });
                    }
                }
                //Filter invoices
                for (let i of e.invoices) {
                    if (_.filter(this.allOrders, x => x.id === i.invoiceId).length === 0) {
                        this.allOrders.push({ id: i.invoiceId, label: i.numberName, projectId: i.projectId, invoiceNr: i.invoiceNr});
                        this.filteredOrdersDict.push({ id: i.invoiceId, label: i.numberName });
                    }
                }
            }

            deferral.resolve(this.projectInvoices);
        });

        return deferral.promise;
    }

    private employeeChange(newEmployeeId: number) {
        if (!newEmployeeId) {
            return;
        }

        if (this.isDirty) {
            this.showDirtyConfirmationDialog().then((ok: boolean) => {
                if (ok) {
                    this._selectedEmployeeId = newEmployeeId;
                    this.employeeChanged();
                }
            });
            return;
        }
        else {
            this._selectedEmployeeId = newEmployeeId;
            this.employeeChanged();
        }
    }

    private employeeChanged() {
        if (this.useExtendedTimeRegistration) {

            this.editTimeHelper.loadEmployeeTimesAndSchedule(this.selectedEmployeeId, this.timeProjectFrom).then((employeGroupId: number) => {
                this.loadTimeDeviationCausesForEmployee(employeGroupId);
            });
        }

        this.selectedEmployee = this.employees.find(e => e.employeeId === this.selectedEmployeeId);
        this.loadProjectInvoices(this.selectedEmployeeId);
        this.updateGrid(true,false);
    }

    private executeButtonFunction(option) {
        switch (option.id) {
            case TimeProjectButtonFunctions.Save:
                this.save();
                break;
            case TimeProjectButtonFunctions.AddRow:
                this.addRow();
                break;
            case TimeProjectButtonFunctions.DeleteRow:
                this.deleteRows();
                break;
            case TimeProjectButtonFunctions.CopyLastWeek:
                this.copyLastWeek();
                break;
        }
    }

    private addRow() {
        if (!this.hasFetchedData) {
            return;
        }
        const row = new ProjectTimeMatrixDTO();
        row.employeeId = this.selectedEmployeeId;
        row.rows = [];
        if (this.useExtendedTimeRegistration) {
            this.setDefaultTimeDeviationCause(row);
        }
        this.setDefaultTimeCode(row, null);
        this.projectTimeBlockRows.push(row);
        this.gridAg.setData(this.projectTimeBlockRows);
        this.gridAg.options.startEditingCell(row,"invoiceNr")
    }

    private deleteRows() {
        const selectedRows: ProjectTimeMatrixDTO[] = this.gridAg.options.getSelectedRows();
        const existingRows = selectedRows.filter(x => x.hasDbRows());
        const newRows = selectedRows.filter(x => !x.hasDbRows());

        if (newRows.length > 0) {
            newRows.forEach(x => {
                const index = this.projectTimeBlockRows.findIndex(y => y === x);
                if (index >= 0) {
                    this.projectTimeBlockRows.splice(index, 1);
                }
            })
        }
        if (existingRows.length > 0) {

            this.progress.startDeleteProgress((completion) => {
                const dbBlocksToDelete: ProjectTimeMatrixSaveDTO[] = [];

                _.forEach(existingRows, (row: ProjectTimeMatrixDTO) => {
                    dbBlocksToDelete.push(row.toSaveDTOForDelete(this.timeProjectFrom));
                });

                return this.projectService.saveProjectTimeMatrixBlocks(dbBlocksToDelete).then((result: IActionResult) => {
                    if (result.success) {
                        completion.completed("", null);
                    } else {
                        completion.failed(result.errorMessage);
                    }

                }, error => {
                    completion.failed(error.message);
                });
            }).then((x) => {
                this.loadGridData();
            });
        }
        else if (newRows.length > 0) {
            this.updateGrid(false, this.hasFetchedData);
            this.isDirty = this.projectTimeBlockRows.filter(x => x.hasChanges()).length > 0;
        }
    }

    private copyLastWeek() {
        const lastWeekStart = this.timeProjectFrom.addDays(-1).beginningOfWeek();
        const lastWeekStop = this.timeProjectFrom.endOfWeek();

        this.projectService.getProjectTimeBlocksForMatrix(this.employeeId, this.selectedEmployeeId, lastWeekStart, lastWeekStop, true).then((data: ProjectTimeMatrixDTO[]) => {
            this.projectTimeBlockRows = data.map(tb => {
                const obj = new ProjectTimeMatrixDTO();
                angular.extend(obj, tb);
                return obj;
            });

            if (this.projectTimeBlockRows && this.projectTimeBlockRows.length > 0) {
                this.projectTimeBlockRows.forEach((p) => {
                    p.clearRowKeys();
                });
                
                this.updateGrid();
                this.isDirty = true;
            }
            else {
                this.showMessage("billing.project.timesheet.notimerowsfound", SOEMessageBoxImage.Information);
            }
        });
    }

    private showMessage(messageKey: string, type: SOEMessageBoxImage) {
        this.translationService.translate(messageKey).then((term) => {
            this.notificationService.showDialog("", term, type, SOEMessageBoxButtons.OK, SOEMessageBoxSize.Small);
        });
    }

    private decreaseDate() {
        this.timeProjectFrom = this.timeProjectFrom.addDays(-7);
        //this.loadGridDataDelayed();
    }

    private increaseDate() {
        this.timeProjectFrom = this.timeProjectFrom.addDays(7);
        //this.loadGridDataDelayed();
    }

    private hasWeekendTimes() {
        let weekendTimes: ProjectTimeMatrixDTO | null = null;
        const rows = this.gridAg.options.getData();

        if (rows.length > 0) {
            weekendTimes =
                rows.find(
                        row =>
                        row['timePayrollQuantityFormatted_6'] && (row['timePayrollQuantityFormatted_6']) != "" ||
                        row['timePayrollQuantityFormatted_0'] && row['timePayrollQuantityFormatted_0'] != "" ||
                        row['invoiceQuantityFormatted_6'] && (row['invoiceQuantityFormatted_6']) != "" ||
                        row['invoiceQuantityFormatted_0'] && row['invoiceQuantityFormatted_0'] != ""
                    ) || null;
        }
        this._showWeekend = !!weekendTimes;
        return !!weekendTimes;
    }

    private checkWeekends() { 
        this.toggleWeekendColumns(false, this.hasWeekendTimes());
    }

    private toggleWeekendColumns(saveSettting: boolean, showWeekend: boolean) {
        if (showWeekend) {
                this.gridAg.options.showColumn("timePayrollQuantityFormatted_6");
                this.gridAg.options.showColumn("invoiceQuantityFormatted_6");
                this.gridAg.options.showColumn("noteIcon_6");

                this.gridAg.options.showColumn("timePayrollQuantityFormatted_0");
                this.gridAg.options.showColumn("invoiceQuantityFormatted_0");
                this.gridAg.options.showColumn("noteIcon_0");
            }
            else {
                this.gridAg.options.hideColumn("timePayrollQuantityFormatted_6");
                this.gridAg.options.hideColumn("invoiceQuantityFormatted_6");
                this.gridAg.options.hideColumn("noteIcon_6");

                this.gridAg.options.hideColumn("timePayrollQuantityFormatted_0");
                this.gridAg.options.hideColumn("invoiceQuantityFormatted_0");
                this.gridAg.options.hideColumn("noteIcon_0");
            }
            this.gridAg.options.sizeColumnToFit();

            if (saveSettting) {
                this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeSheetShowWeekend, this.showWeekend).then((result: IActionResult) => {
                    if (!result.success) {
                        console.log("Error when saving setting", result);
                    }
                });
            }
    }

    private getEmployeeChilds(employeeId: number): ng.IPromise<SmallGenericType[]> {

        return this.projectService.getEmployeeChilds(employeeId);
    }

    public selectChildren(row: ProjectTimeMatrixDTO) {

        this.translationService.translate("billing.project.timesheet.choosechild").then((term) => {

            this.getEmployeeChilds(this.selectedEmployeeId).then((childs: SmallGenericType[]) => {

                const modal = this.$uibModal.open({
                    templateUrl: this.urlHelperService.getGlobalUrl("Billing/Projects/Dialogs/SelectChild/Views/SelectChild.html"),
                    controller: SelectChildController,
                    controllerAs: 'ctrl',
                    backdrop: 'static',
                    size: 'sm',
                    resolve: {
                        title: () => { return term },
                        weekFrom: () => { return this.timeProjectFrom },
                        childs: () => { return childs },
                        rows: () => { return _.orderBy(row.rows, r => r.weekDay) }
                    }
                });

                modal.result.then(result => {
                    if (result && result.rows) {
                        row.rows = result.rows;
                        if (result.modified) {
                            this.isDirty = row.isGridRowModified = true;
                            this.gridAg.options.refreshRows(row);
                        }
                    }
                    else {

                    }
                });
            });
        });
    }

    public showNote(weekDay: number, gridRow: ProjectTimeMatrixDTO) {

        const dayRow = gridRow.getRow(weekDay, false);
        const projectTimeBlock = new ProjectTimeBlockDTO();

        if (dayRow) {
            projectTimeBlock.externalNote = dayRow.externalNote;
            projectTimeBlock.internalNote = dayRow.internalNote;
            projectTimeBlock.invoiceQuantityFormatted = gridRow.getInvoiceQuantityFormatted(weekDay);
            projectTimeBlock.timePayrollQuantityFormatted = gridRow.getTimePayrollQuantityFormatted(weekDay);
        }
        projectTimeBlock.date = this.timeProjectFrom.addDays(weekDay - 1);
        projectTimeBlock.isEditable = true;
        const employee = _.find(this.employeesDict, ["id", this.selectedEmployeeId]);
        projectTimeBlock.employeeName = employee ? employee.name : "";

        // Show edit note dialog
        const options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Directives/TimeProjectReport/Views/editNote.html"),
            controller: EditNoteController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: '',
            resolve: {
                rows: () => { return [projectTimeBlock] },
                row: () => { return projectTimeBlock },
                isReadonly: () => { return false },
                saveDirect: () => { return false },
                workTimePermission: () => { return this.workTimePermission },
                invoiceTimePermission: () => { return this.invoiceTimePermission },
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result.rowIsModified) {
                this.isDirty = true;
                gridRow.setExternalNote(weekDay, projectTimeBlock.externalNote);
                gridRow.setInternalNote(weekDay, projectTimeBlock.internalNote);
                this.gridAg.options.refreshRows();
            }
        });
    }

    private save() {
        const modifiedBlocks = this.projectTimeBlockRows.filter(p => p.hasChanges());
        if (modifiedBlocks.length === 0) {
            return;
        }

        if (!this.validateSave(modifiedBlocks)) {
            return;
        }

        this.gridAg.options.stopEditing(false);
        this.progress.startSaveProgress((completion) => {
            const blockToSave: ProjectTimeMatrixSaveDTO[] = [];

            _.forEach(modifiedBlocks, block => {
                blockToSave.push(block.toSaveDTO(this.timeProjectFrom));
            });
            
            return this.projectService.saveProjectTimeMatrixBlocks(blockToSave).then((result: IActionResult) => {
                if (result.success) {
                    completion.completed("", null);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(() => {
                this.isDirty = false;
                this.loadGridData();
            }, error => {
                    console.log("Error when saving:" + error);
            });
    }

    private validateSave(blocks: ProjectTimeMatrixDTO[]): boolean {
        let result = true;
        blocks.forEach(p => {
            p.rows.forEach(r => {
                if (result) {
                    if (p.timeDeviationCauseNeedChild && !r.employeeChildId) {
                        this.showMessage("billing.project.timesheet.mustsetchild", SOEMessageBoxImage.Error);
                        result = false;
                        return;
                    }
                }
            })
        })

        return result;
    }
}