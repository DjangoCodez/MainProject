import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IPayrollService } from "../PayrollService";
import { Feature } from "../../../Util/CommonEnumerations";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Flags
    private showInactive: boolean = false;

    // Toolbar
    private toolbarInclude: any;

    private startupFilter: any = {
        isActive: ['true']
    };

    //@ngInject
    constructor(
        private payrollService: IPayrollService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private $timeout: ng.ITimeoutService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory    ) {

        super(gridHandlerFactory, "Time.Payroll.PayrollPriceFormulas", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('payrollPriceFormulaId', 'name');
        this.toolbarInclude = urlHelperService.getViewUrl("gridHeader.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData());

    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Preferences_SalarySettings_PriceFormula, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Preferences_SalarySettings_PriceFormula].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_SalarySettings_PriceFormula].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
        this.toolbar.addInclude(this.toolbarInclude);
    }

    public setUpGrid() {
        // Columns
        var keys: string[] = [
            "common.code",
            "common.name",
            "common.description",
            "common.active",
            "time.payroll.payrollpriceformula.formula",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnBool("isActive", terms["common.active"], 50, false, null, null, true);
            this.gridAg.addColumnText("code", terms["common.code"], null);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null);
            this.gridAg.addColumnText("formulaPlain", terms["time.payroll.payrollpriceformula.formula"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.payroll.payrollpriceformula.payrollpriceformulas", true);
        });
    }

    private reloadData() {
        this.loadGridData();
    }
    private showInactiveChanged() {
        this.$timeout(() => {
            this.reloadData();
        });
    }
    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.payrollService.getPayrollPriceFormulas(this.showInactive).then(data => {
                this.setData(data);
            });
        }]);
        // Load data
    }
}
