import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { Feature } from "../../../Util/CommonEnumerations";
import { IPayrollService } from "../PayrollService";
import { MassRegistrationGridDTO } from "../../../Common/Models/MassRegistrationDTOs";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private rows: MassRegistrationGridDTO[];

    //@ngInject
    constructor(
        $scope: ng.IScope,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $window,
        private payrollService: IPayrollService,
        private translationService: ITranslationService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Time.Payroll.MassRegistration.MassRegistrations", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onBeforeSetUpGrid(() => this.loadLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData(false));

        super.onTabActivetedAndModified(() => {
            this.loadGridData(false);
        });
    }

    // SETUP

    onInit(parameters: any) {
        this.guid = parameters.guid;
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.flowHandler.start({ feature: Feature.Time_Payroll_MassRegistration, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private setupGrid() {
        this.gridAg.addColumnBool("isActive", this.terms["common.active"], 50);
        this.gridAg.addColumnText("name", this.terms["common.name"], null);
        this.gridAg.addColumnBoolEx("isRecurring", this.terms["time.payroll.massregistration.isrecurring"], 50, { enableHiding: true });
        this.gridAg.addColumnDate("recurringDateTo", this.terms["time.payroll.massregistration.recurringdateto"], 100, true);
        this.gridAg.addColumnEdit(this.terms["core.edit"], this.edit.bind(this), false);

        this.gridAg.finalizeInitGrid("time.payroll.massregistration.massregistrations", true, undefined, true);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData());
    }

    // SERVICE CALLS   

    private loadLookups(): ng.IPromise<any> {
        return this.$q.all([this.loadTerms()]);
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.edit",
            "common.active",
            "common.name",
            "time.payroll.massregistration.isrecurring",
            "time.payroll.massregistration.recurringdateto"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    public loadGridData(useCache: boolean) {
        this.progress.startLoadingProgress([() => {
            return this.payrollService.getMassRegistrationsGrid(useCache).then(x => {
                this.rows = x;
                return this.rows;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData(false);
    }

    // EVENTS   

    edit(row) {
        // Send message to TabsController        
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }
}