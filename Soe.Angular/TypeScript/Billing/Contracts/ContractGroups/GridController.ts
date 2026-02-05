import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IContractService } from "../ContractService";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { Feature } from "../../../Util/CommonEnumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        private contractService: IContractService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Billing.Contracts.ContractGroups", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('contractGroupId', 'name');
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
            .onLoadGridData(() => this.loadGridData());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });
        }

        this.flowHandler.start({ feature: Feature.Billing_Contract_Groups_Edit, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private setupGrid() {

        var translationKeys: string[] = [
            "common.name",
            "common.description",
            "common.period",
            "billing.contract.contractgroups.pricemanagement",
            "billing.contract.contractgroups.interval",
            "billing.contract.contractgroups.dayinmonth",
            "core.edit",
        ];

        this.translationService.translateMany(translationKeys).then(terms => {
            this.gridAg.addColumnText("name", terms["common.name"], null, false);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnSelect("periodText", terms["common.period"], null, { displayField: "periodText", selectOptions: null, populateFilterFromGrid: true });
            this.gridAg.addColumnSelect("priceManagementText", terms["billing.contract.contractgroups.pricemanagement"], null, { displayField: "priceManagementText", selectOptions: null, populateFilterFromGrid: true });
            this.gridAg.addColumnNumber("interval", terms["billing.contract.contractgroups.interval"], 40, { enableHiding: true });
            this.gridAg.addColumnNumber("dayInMonth", terms["billing.contract.contractgroups.dayinmonth"], 40, { enableHiding: true });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("billing.contract.contractgroups.contractgroups", true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
    }

    private loadGridData() {
        this.gridAg.clearData();

        this.progress.startLoadingProgress([() => {
            return this.contractService.getContractGroups().then(data => {
                this.setData(data);
            });
        }]);

    }
}
