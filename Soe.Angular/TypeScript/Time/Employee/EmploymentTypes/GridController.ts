import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IEmployeeService } from "../EmployeeService";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ISelectedItemsService } from "../../../Core/Services/SelectedItemsService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //@ngInject
    constructor(
        $scope: ng.IScope,
        private employeeService: IEmployeeService,
        private selectedItemsService: ISelectedItemsService,
        private translationService: ITranslationService,
        private $filter: ng.IFilterService,
        private $timeout: ng.ITimeoutService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        
        super(gridHandlerFactory, "Time.Employee.EmploymentTypes", progressHandlerFactory, messagingHandlerFactory);
        
        this.selectedItemsService.setup($scope, "employmentTypeId", (items: number[]) => this.save(items));

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

        this.flowHandler.start({ feature: Feature.Time_Employee_EmploymentTypes, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_EmploymentTypes ].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_EmploymentTypes].modifyPermission;
         
        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData(), true, () => this.selectedItemsService.Save(), () => { return this.saveButtonIsDisabled() });
    }

    private saveButtonIsDisabled(): boolean {
        return !this.selectedItemsService.SelectedItemsExist();
    }
    public setUpGrid() {
        // Columns
        const keys: string[] = [
            "common.active",
            "common.name",
            "common.description",
            "common.code",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnActive("employmentTypeId", terms["common.active"], 50, (params) => this.selectedItemsService.CellChanged(params));
            this.gridAg.addColumnText("code", terms["common.code"], 20);
            this.gridAg.addColumnText("name", terms["common.name"], null);            
            this.gridAg.addColumnText("description", terms["common.description"], null);
            this.gridAg.addColumnIcon(null, "", 40, { icon: "fal fa-pencil iconEdit", onClick: this.edit.bind(this), showIcon: (row) => !row.hideEdit })
            this.gridAg.options.enableRowSelection = false;
            this.gridAg.finalizeInitGrid("time.employee.employmenttype.employmenttype", true, undefined, true);
        });
    }

    //Overide edit in base class to include some special functionality
    edit(row) {
        if (row !== null && row["standard"] !== null && row["standard"] === true)
            return;

        this.messagingHandler.publishEditRow(row);
    }

    private reloadData() {
        this.loadGridData();
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {            
            return this.employeeService.getEmploymentTypes().then(data => {
                var dd = _.filter(data, function (y) { return y["employmentTypeId"] != 0; });

                this.setData(dd);
            });
        }]);         
    }

    protected save(items: number[]) {
        var dict: any = {};
        _.forEach(items, (id: number) => {
            // Find entity
            var entity: any = this.gridAg.options.findInData((ent: any) => ent["employmentTypeId"] === id);
            // Push id and active flag to array
            if (entity !== undefined) {                
                    dict[id] = entity.isActive;
            }
        });
        
        if ((dict !== undefined) && (Object.keys(dict).length > 0)) {
            this.progress.startLoadingProgress([() => {
                return this.employeeService.updateEmploymentTypesState(dict).then(result => {
                    this.reloadData();
                });
            }]);
        }
    }


}