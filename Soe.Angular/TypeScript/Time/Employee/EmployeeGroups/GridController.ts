import { IEmployeeService } from "../EmployeeService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature } from "../../../Util/CommonEnumerations";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    //@ngInject
    constructor(
    private employeeService: IEmployeeService,
    private translationService: ITranslationService,
    controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
    progressHandlerFactory: IProgressHandlerFactory,
    messagingHandlerFactory: IMessagingHandlerFactory,
    gridHandlerFactory: IGridHandlerFactory) {
    super(gridHandlerFactory, "Time.Employee.EmployeeGroups", progressHandlerFactory, messagingHandlerFactory);

    this.flowHandler = controllerFlowHandlerFactory.createForGrid()
        .onPermissionsLoaded((feature, readOnly, modify) => {
            this.readPermission = readOnly;
            this.modifyPermission = modify

            if (this.modifyPermission) {
                // Send messages to TabsController
                this.messagingHandler.publishActivateAddTab();
            }
        })
        .onSetUpGrid(() => this.setupGrid())
        .onLoadGridData(() => this.loadGridData())
    }


    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.flowHandler.start({ feature: Feature.Time_Employee_Groups, loadReadPermissions: true, loadModifyPermissions: true });
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "time.employee.employeegroup.daytypes",
            "time.employee.employeegroup.timedeviationcauses",
            //"time.employee.employeegroup.autogentimeblocks",
            "time.employee.employeegroup.timereporttype",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("dayTypesNames", terms["time.employee.employeegroup.daytypes"], null);
            this.gridAg.addColumnText("timeDeviationCausesNames", terms["time.employee.employeegroup.timedeviationcauses"], null);
            //this.gridAg.addColumnText("autogenTimeblocks", terms["time.employee.employeegroup.autogentimeblocks"], null);
            this.gridAg.addColumnSelect("timeReportTypeName", terms["time.employee.employeegroup.timereporttype"], null, { displayField: "timeReportTypeName", selectOptions: null, populateFilterFromGrid: true });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));
            
            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.employee.employeegroup.employeegroups", true);
        });
    }

    public loadGridData() {
        // Load data
        this.employeeService.getEmployeeGroups().then((x) => {
            this.setData(x);
        });
    }
}
