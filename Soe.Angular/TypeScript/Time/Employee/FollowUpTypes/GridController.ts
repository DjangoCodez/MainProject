import { IEmployeeService } from "../EmployeeService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ISelectedItemsService } from "../../../Core/Services/SelectedItemsService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    selectedItems: any[];
    
    //@ngInject
    constructor(
        $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private employeeService: IEmployeeService,
        private translationService: ITranslationService,
        private selectedItemsService: ISelectedItemsService,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,) {

        super(gridHandlerFactory, "time.employee.followuptype.followuptype", progressHandlerFactory, messagingHandlerFactory);
        this.useRecordNavigatorInEdit('followUpTypeId', 'name');

        this.selectedItemsService.setup($scope, "followUpTypeId", (items: number[]) => this.save(items));

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

        this.flowHandler.start({ feature: Feature.Time_Employee_FollowUpTypes, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_FollowUpTypes].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_FollowUpTypes].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    public setUpGrid() {
        // Columns
        var keys: string[] = [
            "common.active",
            "common.name",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnActive("followUpTypeId", terms["common.active"], null, (params) => this.selectedItemsService.CellChanged(params));
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.employee.followuptype.followuptypes", true, undefined, true);
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.employeeService.getFollowUpTypes()
                .then(data => {
                    this.setData(data);
                });
        }]);
    }

    private save(items: number[]) {
        const dict: any = {};

        _.forEach(items, (id: number) => {
            // Find entity
            var entity: any = this.gridAg.options.findInData((ent: any) => ent["followUpTypeId"] === id);
            // Push id and active flag to array
            if (entity !== undefined) {
                dict[id] = entity.isActive;
            }
        });

        if ((dict !== undefined) && (Object.keys(dict).length > 0)) {
            this.employeeService.updateFollowUpTypesState(dict).then(() => {
                this.reloadData();
            });
        }
    }
    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData(), true, () => this.selectedItemsService.Save(), () => !this.selectedItemsService.SelectedItemsExist());
    }

    private reloadData() {
        this.loadGridData();
    }
}
