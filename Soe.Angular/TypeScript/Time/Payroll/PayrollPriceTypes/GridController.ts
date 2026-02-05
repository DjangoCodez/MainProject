import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IPayrollService } from "../PayrollService";
import { Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Filters
    typeFilterOptions = [];

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private payrollService: IPayrollService,
        private translationService: ITranslationService,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,) {

        super(gridHandlerFactory, "time.payroll.payrollpricetype.payrollpricetypes", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('payrollPriceTypeId', 'name');
        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.onDoLookUp())
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

        this.flowHandler.start({ feature: Feature.Time_Preferences_SalarySettings_PriceType, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Preferences_SalarySettings_PriceType].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_SalarySettings_PriceType].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    public setUpGrid() {
        var keys: string[] = [
            "common.type",
            "common.code",
            "common.name",
            "common.description",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnSelect("typeName", terms["common.type"], 75, { displayField: "typeName", selectOptions: null, populateFilterFromGrid: true });
            this.gridAg.addColumnText("code", terms["common.code"], 75);
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.payroll.payrollpricetype.payrollpricetypes", true);
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.payrollService.getPayrollPriceTypesGrid(false)
                .then(data => {
                    this.setData(data);
                });
        }]);
    }

    protected onDoLookUp(): ng.IPromise<any> {
        return this.loadTypes();
    }

    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollPriceTypes, false, false).then((x) => {
            _.forEach(x, (y: any) => {
                this.typeFilterOptions.push({ value: y.name, label: y.name })
            });
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    private reloadData() {
        this.loadGridData();
    }
}
