import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IPayrollService } from "../../Payroll/PayrollService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { PayrollStartValueHeadDTO, PayrollStartValueRowDTO } from "../../../Common/Models/PayrollImport";
import { ITimeoutService } from "angular";
import { SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Toolbar
    private toolbarInclude: any;
    private allEmployees: SmallGenericType[] = [];
    private employees: SmallGenericType[] = [];
    private selectedEmployees: SmallGenericType[] = [];

    // Data
    private heads: PayrollStartValueHeadDTO[];
    private filteredHeads: PayrollStartValueHeadDTO[];

    //@ngInject
    constructor(
        private $timeout: ITimeoutService,
        private payrollService: IPayrollService,
        private translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,) {

        super(gridHandlerFactory, "time.import.payrollstartvalue.payrollstartvalues", progressHandlerFactory, messagingHandlerFactory);

        this.toolbarInclude = urlHelperService.getViewUrl("gridHeader.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.loadEmployees())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Import_PayrollStartValuesImported, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Import_PayrollStartValuesImported].readPermission;
        this.modifyPermission = response[Feature.Time_Import_PayrollStartValuesImported].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    public setUpGrid() {
        var keys: string[] = [
            "core.edit",
            "common.amount",
            "common.date",
            "common.datefrom",
            "common.dateto",
            "common.employee",
            "common.quantity",
            "time.import.payrollstartvalue.importedfrom",
            "time.payrollproduct.payrollproduct"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            // Details
            this.gridAg.enableMasterDetail(false, null, null, true);
            this.gridAg.options.setDetailCellDataCallback(params => {
                this.gridAg.detailOptions.enableRowSelection = false;
                this.gridAg.detailOptions.sizeColumnToFit();

                let rows: PayrollStartValueRowDTO[] = this.filterRows(params.data['rows']);
                rows.forEach(row => {
                    row['employeeName'] = this.employees.find(e => e.id === row.employeeId)?.name;
                });
                params.successCallback(rows);
            });

            this.gridAg.detailOptions.addColumnText("employeeName", terms["common.employee"], null);
            this.gridAg.detailOptions.addColumnText("productNrAndName", terms["time.payrollproduct.payrollproduct"], null);
            this.gridAg.detailOptions.addColumnDate("date", terms["common.date"], 100);
            this.gridAg.detailOptions.addColumnNumber("quantity", terms["common.quantity"], 100);
            this.gridAg.detailOptions.addColumnNumber("amount", terms["common.amount"], 100, { decimals: 2 });
            this.gridAg.detailOptions.enableFiltering = false;
            this.gridAg.detailOptions.finalizeInitGrid();

            // Master
            let colDef = this.gridAg.addColumnDate("dateFrom", terms["common.datefrom"], 75);
            colDef.cellRenderer = 'agGroupCellRenderer';
            this.gridAg.addColumnDate("dateTo", terms["common.dateto"], 75);
            this.gridAg.addColumnText("importedFrom", terms["time.import.payrollstartvalue.importedfrom"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            let events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.IsRowMaster, (rowNode) => {
                return rowNode ? rowNode.rows.length > 0 : false;
            }));
            this.gridAg.options.subscribe(events);

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.options.noDetailPadding = true;

            this.gridAg.finalizeInitGrid("time.import.payrollstartvalue.payrollstartvalues", true);
        });
    }

    private loadEmployees(): ng.IPromise<any> {
        return this.payrollService.getEmployeesDict(false, true, false, false).then(x => {
            this.allEmployees = x;
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.payrollService.getPayrollStartValueHeads(true, true).then(x => {
                this.heads = x;
                this.setupEmployeeFilter();
                this.filterHeads();
            });
        }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
        this.toolbar.addInclude(this.toolbarInclude);
    }

    private reloadData() {
        this.loadGridData();
    }

    private setupEmployeeFilter() {
        let employeeIds: number[] = [];
        this.heads.forEach(head => {
            employeeIds.push(...head.rows.map(r => r.employeeId));
        });
        employeeIds = _.uniq(employeeIds);

        this.employees = this.allEmployees.filter(e => _.includes(employeeIds, e.id));
    }

    private filterHeads() {
        this.filteredHeads = [];
        this.$timeout(() => {
            let employeeIds: number[] = this.selectedEmployees.map(e => e.id);
            if (employeeIds.length === 0) {
                this.filteredHeads = this.heads;
            } else {
                this.heads.forEach(head => {
                    if (_.intersection(employeeIds, head.rows.map(r => r.employeeId)).length > 0)
                        this.filteredHeads.push(head);
                });
            }

            this.setData(this.filteredHeads);
        });
    }

    private filterRows(rows: PayrollStartValueRowDTO[]) {
        let filteredRows: PayrollStartValueRowDTO[] = [];
        let employeeIds: number[] = this.selectedEmployees.map(e => e.id);
        if (employeeIds.length === 0) {
            filteredRows = rows;
        } else {
            rows.forEach(row => {
                if (_.includes(employeeIds, row.employeeId))
                    filteredRows.push(row);
            });
        }

        return filteredRows;
    }
}
