import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IEmployeeService } from "../EmployeeService";
import { Feature} from "../../../Util/CommonEnumerations";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    vacationGroupTypes: any[];
    dayRanges: any[];

    //@ngInject
    constructor(
        private employeeService: IEmployeeService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory    ) {

        super(gridHandlerFactory, "Time.Employee.VacationGroups", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('vacationGroupId', 'name');
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
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

        this.flowHandler.start({ feature: Feature.Time_Employee_VacationGroups, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_VacationGroups].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_VacationGroups].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData(), true);
    }


    public setUpGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "time.employee.vacationgroup.fromdate",
            "time.employee.vacationgroup.type",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnSelect("fromDateName", terms["time.employee.vacationgroup.fromdate"], null, { displayField: "fromDateName", selectOptions: null, populateFilterFromGrid: true });
            this.gridAg.addColumnText("name", terms["common.name"], 125);
            this.gridAg.addColumnSelect("typeName", terms["time.employee.vacationgroup.type"], null, { displayField: "typeName", selectOptions: null, populateFilterFromGrid: true });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.employee.vacationgroup.vacationgroups", true);
        });
    }

    private reloadData() {
        this.loadGridData();
    }

    public loadGridData() {
  
        var locale = "sv-se";
        // Load data
        this.employeeService.getVacationGroups(true, false).then((data) => {
            _.forEach(data, (row) => {
                var dateToConvert = new Date(row.fromDate);
                var days = new Date(1900, dateToConvert.getMonth(), 0).getDate();
                var fromMonthName = new Date(1900, dateToConvert.getMonth(), 0).toLocaleString(locale, { month: "long" });
                var toMonthName = new Date(1900, dateToConvert.getMonth() + 1, 0).toLocaleString(locale, { month: "long" });
                var displayValue = "1 " + toMonthName + " - " + days + " " + fromMonthName;
                row.fromDateName = displayValue;
            });
            this.setData(data);
        });
    }
}
