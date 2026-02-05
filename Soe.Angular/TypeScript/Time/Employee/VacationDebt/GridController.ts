import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { CompanySettingType, Feature, SettingMainType, SoeReportTemplateType, TermGroup_ReportExportType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IEmployeeService } from "../EmployeeService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { EmployeeCalculateVacationResultFlattenedDTO } from "../../../Common/Models/EmployeeCalculateVacationResultFlattenedDTO";
import { IReportDataService } from "../../../Core/RightMenu/ReportMenu/ReportDataService";
import { ReportJobDefinitionFactory } from "../../../Core/Handlers/ReportJobDefinitionFactory";
import { IReportService } from "../../../Core/Services/ReportService";
import { GroupDisplayType } from "../../../Util/SoeGridOptionsAg";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms:
    private terms: any;

    // Data
    private vacationDebtCalculations: EmployeeCalculateVacationResultFlattenedDTO[];

    //@ngInject
    constructor(
        private notificationService: INotificationService,
        private $timeout: ng.ITimeoutService,
        private reportDataService: IReportDataService,
        private reportService: IReportService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private employeeService: IEmployeeService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
    ) {
        super(gridHandlerFactory, "Time.Employee.VacationDebt.VacationDebt", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    // SETUP

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.flowHandler.start([
            { feature: Feature.Time_Employee_VacationDebt, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_VacationDebt].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_VacationDebt].modifyPermission;
    }

    public setupGrid() {

        //this.gridAg.options.enableRowSelection = true;
        // this.gridAg.options.groupHideOpenParents = true;

        //Set name
        this.gridAg.options.setName("time.employee.vacationdebt.vacationdebt");

        // Prevent double click
        this.doubleClickToEdit = false;

        // Hide filters
        this.gridAg.options.enableFiltering = false;

        // Enable auto column for grouping
        this.gridAg.options.groupDisplayType = GroupDisplayType.Custom;

        // Details
        this.gridAg.enableMasterDetail(false);
        this.gridAg.options.setDetailCellDataCallback((params) => {
            this.loadEmployeeResults(params);
        });

        this.gridAg.detailOptions.addColumnText("name", this.terms["common.name"], 40, { cellClassRules: { "strike-through": (gridRow: any) => gridRow && gridRow.data.isDeleted } });
        this.gridAg.detailOptions.addColumnNumber("value", this.terms["common.value"], 40, { decimals: 2, cellClassRules: { "strike-through": (gridRow: any) => gridRow && gridRow.data.isDeleted } });
        this.gridAg.detailOptions.addColumnText("formulaPlain", this.terms["common.formulabuilder.formula"], 70, { cellClassRules: { "strike-through": (gridRow: any) => gridRow && gridRow.data.isDeleted }, toolTipField: "formulaPlain" });
        this.gridAg.detailOptions.addColumnText("formulaExtracted", this.terms["time.employee.vacationdebt.formulaextracted"], 70, { cellClassRules: { "strike-through": (gridRow: any) => gridRow && gridRow.data.isDeleted }, toolTipField: "formulaExtracted" });
        this.gridAg.detailOptions.addColumnText("formulaNames", this.terms["time.employee.vacationdebt.formulanames"], null, { cellClassRules: { "strike-through": (gridRow: any) => gridRow && gridRow.data.isDeleted }, toolTipField: "formulaNames" });
        this.gridAg.detailOptions.addColumnText("formulaOrigin", this.terms["time.employee.vacationdebt.formulaorigin"], 80, { cellClassRules: { "strike-through": (gridRow: any) => gridRow && gridRow.data.isDeleted }, toolTipField: "formulaOrigin" });
        this.gridAg.detailOptions.addColumnText("error", this.terms["time.employee.vacationdebt.error"], 60, { cellClassRules: { "strike-through": (gridRow: any) => gridRow && gridRow.data.isDeleted }, toolTipField: "error" });
        this.gridAg.detailOptions.addColumnDateTime("created", this.terms["core.created"], 60, false, null, null, { cellClassRules: { "strike-through": (gridRow: any) => gridRow && gridRow.data.isDeleted } });
        this.gridAg.detailOptions.finalizeInitGrid();

        // Master
        var groupCol1 = this.gridAg.addColumnDate("date", this.terms["common.dateto"], 160, false, null, { suppressSizeToFit: true, minWidth: 80, maxWidth: 80 });
        var groupCol2 = this.gridAg.addColumnDateTime("created", this.terms["core.created"], 130, false, null, { suppressSizeToFit: true, minWidth: 130, maxWidth: 130 });
        var groupCol3 = this.gridAg.addColumnText("employeeNrAndName", this.terms["common.employee"], null);
        groupCol3.cellRenderer = 'agGroupCellRenderer';

        //this.gridAg.addColumnText("employeeName", this.terms["common.name"], null,);
        this.gridAg.addColumnNumber("vidValue", "Värde intjänade dagar", null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum', alignLeft: true });
        this.gridAg.addColumnNumber("vbdValue", "Värde återstående betalda dagar", null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum', alignLeft:true });
        this.gridAg.addColumnNumber("vsdValue", "Värde återstående sparade dagar", null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum', alignLeft: true });
        this.gridAg.addColumnNumber("vfdValue", "Värde förskottsdagar", null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum', alignLeft: true });
        this.gridAg.addColumnNumber("totValue", "Totalt värde dagar", null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum', alignLeft: true });
        this.gridAg.addColumnNumber("totVSTRValue", "Varav semestertillägg rörligt", null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum', alignLeft: true });

        this.gridAg.addColumnIcon("delete", null, null, { toolTip: this.terms["core.delete"], icon: 'fal fa-times iconDelete', onClick: this.deleteRow.bind(this), getNodeOnClick: true, showIcon: this.showDeleteRowIcon.bind(this) });
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.openEdit.bind(this), false, this.showEditIcon.bind(this));
        this.gridAg.addColumnIcon(null, null, 20, { icon: "fal fa-print", onClick: this.print.bind(this), getNodeOnClick: true, showIcon: this.showPrintIcon.bind(this) });

        this.gridAg.options.enableRowSelection = false;
        this.gridAg.options.groupRowsByColumnAndHide(groupCol1, 'agGroupCellRenderer', 0, true, null, null, true);
        this.gridAg.options.groupRowsByColumnAndHide(groupCol2, 'agGroupCellRenderer', 1, true, null, null, true);
        //this.gridAg.options.groupSelectsChildren = true;
        this.gridAg.options.noRowGroupIndent = true;

        this.gridAg.finalizeInitGrid("time.employee.vacationdebt.vacationdebt", true); 
        this.setData("");
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    private openEdit(row) {
        if (row) {
            // Send message to TabsController
            if (this.readPermission || this.modifyPermission)
                this.messagingHandler.publishEditRow({ row: row });
        }
    }


    private print(row) {
        let employeeIds: number[] = [];
        if (row.group) {
            if (row.key) {
                if (row.field === 'created') {
                    let date: Date = CalendarUtility.convertToDate(row.key);
                    if (date) {
                        let rows: EmployeeCalculateVacationResultFlattenedDTO[] = _.filter(this.vacationDebtCalculations, v => v.created.isSameDayAs(date));
                        if (rows && rows.length > 0) {
                            employeeIds = _.uniqBy(_.map(rows, b => b.employeeId), s => s); 
                            this.reportService.getStandardReportId(SettingMainType.Company, CompanySettingType.DefaultEmployeeVacationDebtReport, SoeReportTemplateType.EmployeeVacationDebtReport).then(reportId => {
                                this.reportDataService.createReportJob(ReportJobDefinitionFactory.createEmployeeVacationDebtReportDefinition(reportId, employeeIds, rows[0].employeeCalculateVacationResultHeadId, TermGroup_ReportExportType.Pdf), true)
                            });
                        }
                    }
                }
            }
        }
        else {
            if (row) {
                employeeIds.push(row.data.employeeId);
                this.reportService.getStandardReportId(SettingMainType.Company, CompanySettingType.DefaultEmployeeVacationDebtReport, SoeReportTemplateType.EmployeeVacationDebtReport).then(reportId => {
                    this.reportDataService.createReportJob(ReportJobDefinitionFactory.createEmployeeVacationDebtReportDefinition(reportId, employeeIds, row.data.employeeCalculateVacationResultHeadId, TermGroup_ReportExportType.Pdf), true)
                });
            }
        }
    }

    private showDeleteRowIcon(node) {
        if (node) {
            if (node.group) {
                if (node.key) {
                    if (node.field === 'date') {
                        return false;
                    } else if (node.field === 'created') {
                        return true;
                    }
                }
            } else {
                return true;
            }
        }
    }

    private deleteRow(node) {
        if (node.group) {
            if (node.key) {
                //if (node.field === 'date') {
                //    let date: Date = CalendarUtility.convertToDate(node.key);
                //    if (date) {
                //        let row: EmployeeCalculateVacationResultFlattenedDTO = _.find(this.vacationDebtCalculations, v => v.date.isSameDayAs(date));
                //        if (row)
                //            this.deleteHead(row.employeeCalculateVacationResultHeadId);
                //    }
                //} else 
                if (node.field === 'created') {
                    let date: Date = CalendarUtility.convertToDate(node.key);
                    if (date) {
                        let row: EmployeeCalculateVacationResultFlattenedDTO = _.find(this.vacationDebtCalculations, v => v.created.isSameDayAs(date));
                        if (row) {
                            this.deleteHead(row.employeeCalculateVacationResultHeadId);
                        }
                    }
                }
            }
        } else {
            let data: EmployeeCalculateVacationResultFlattenedDTO = node.data;
            if (data)
                this.deleteEmployee(data.employeeCalculateVacationResultHeadId, data.employeeId);
        }
    }

    private showEditIcon(row): boolean {
        return !!row;
    }

    private showPrintIcon(gridRow) {
        if (this.showDeleteRowIcon(gridRow))
            return true;
        else
            return false;
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.edit",
            "core.delete",
            "core.print",
            "common.date",
            "common.formulabuilder.formula",
            "common.value",
            "common.dateto",
            "common.modified",
            "common.modifiedby",
            "common.name",
            "common.employee",
            "core.created",
            "core.createdby",
            "time.employee.employee.employeenrshort",
            "time.employee.vacationdebt.formulaextracted",
            "time.employee.vacationdebt.formulanames",
            "time.employee.vacationdebt.formulaorigin",
            "time.employee.vacationdebt.error",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.employeeService.getVacationDebtCalculations().then(x => {
                this.vacationDebtCalculations = x;
                this.setData(this.vacationDebtCalculations);
            });
        }]);
    }

    public loadEmployeeResults(params) {
        if (!params.data['rowsLoaded']) {
            this.progress.startLoadingProgress([() => {
                return this.employeeService.getEmployeeVacationDebtCalculationResults(params.data.employeeCalculateVacationResultHeadId, params.data.employeeId, false).then((x) => {
                    params.data['rows'] = x;
                    params.data['rowsLoaded'] = true;
                });
            }]).then(() => {
                params.successCallback(params.data['rows']);
            });
        }
        else {
            params.successCallback(params.data['rows']);
        }
    }

    private reloadData() {
        this.loadGridData();
    }

    private deleteHead(employeeCalculateVacationResultHeadId: number) {
        this.progress.startDeleteProgress((completion) => {
            this.employeeService.deleteEmployeeCalculateVacationResultHead(employeeCalculateVacationResultHeadId).then(result => {
                if (result.success) {
                    completion.completed(null, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.reloadData();
        });
    }

    private deleteEmployee(employeeCalculateVacationResultHeadId: number, employeeId: number) {
        this.progress.startDeleteProgress((completion) => {
            this.employeeService.deleteEmployeeCalculateVacationResultsForEmployee(employeeCalculateVacationResultHeadId, employeeId).then(result => {
                if (result.success) {
                    completion.completed(null, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.reloadData();
        });
    }

    private printEmployeeVacationDebtReport(reportId: number, employeeIds: number[], employeeCalculateVacationResultHeadId?: number, exportType: TermGroup_ReportExportType = TermGroup_ReportExportType.Unknown) {
        this.reportDataService.createReportJob(ReportJobDefinitionFactory.createEmployeeVacationDebtReportDefinition(reportId, employeeIds, employeeCalculateVacationResultHeadId, exportType), true);
    }

    // EVENTS
}