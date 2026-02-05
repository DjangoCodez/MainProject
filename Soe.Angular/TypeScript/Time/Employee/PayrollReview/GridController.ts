import { ICoreService } from "../../../Core/Services/CoreService";
import { IEmployeeService } from "../EmployeeService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    //@ngInject
    constructor($http,
        private $window,
        private coreService: ICoreService,
        private employeeService: IEmployeeService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Time.Employee.PayrollReview.PayrollReview", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('payrollReviewHeadId', 'name');

        this.onTabActivetedAndModified(() => this.reloadGridFromFilter());

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.reloadGridFromFilter());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadGridFromFilter(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Employee_PayrollReview, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadGridFromFilter());
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "common.name",
            "time.employee.payrollgroup.payrollgroups",
            "time.payroll.payrollpricetype.payrollpricetypes",
            "time.employee.payrolllevel.payrolllevels",
            "common.datefrom",
            "common.status",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("payrollGroupNames", terms["time.employee.payrollgroup.payrollgroups"], null);
            this.gridAg.addColumnText("payrollPriceTypeNames", terms["time.payroll.payrollpricetype.payrollpricetypes"], null);
            this.gridAg.addColumnText("payrollLevelNames", terms["time.employee.payrolllevel.payrolllevels"], null);
            this.gridAg.addColumnDate("dateFrom", terms["common.datefrom"], null);
            this.gridAg.addColumnText("statusName", terms["common.status"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("time.employee.payrollreview.payrollreview", true);
        });

    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.employeeService.getPayrollReviewHeads(false, true, true, true, true).then(x => {
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 1000, { leading: false, trailing: true });
}
