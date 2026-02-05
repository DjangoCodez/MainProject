import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { IAttestService } from "../../AttestService";
import { Feature, SoeModule } from "../../../../Util/CommonEnumerations";
import { ISelectedItemsService } from "../../../../Core/Services/SelectedItemsService";
import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Lookups
    private dayTypes: SmallGenericType[] = [];

    //@ngInject
    constructor(
        $scope: ng.IScope,
        private attestService: IAttestService,
        private translationService: ITranslationService,
        private selectedItemsService: ISelectedItemsService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Manage.Attest.Time", progressHandlerFactory, messagingHandlerFactory);

        this.selectedItemsService.setup($scope, "attestRuleHeadId", (items: number[]) => this.save(items));

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
            .onDoLookUp(() => this.loadDayTypes())
            .onLoadGridData(() => this.loadGridData(false));
    }

    // SETUP

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Manage_Attest_Time_AttestRules, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private setupGrid() {
        var keys: string[] = [
            "common.active",
            "common.name",
            "common.description",
            "manage.attest.time.daytype",
            "common.employeegroups",
            "core.edit",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.gridAg.addColumnActive("isActive", terms["common.active"], 50, (params) => this.selectedItemsService.CellChanged(params));
            this.gridAg.addColumnText("name", terms["common.name"], null);
            this.gridAg.addColumnText("description", terms["common.description"], null, true);
            this.gridAg.addColumnText("dayTypeName", terms["manage.attest.time.daytype"], null, true);
            this.gridAg.addColumnText("employeeGroupNames", terms["common.employeegroups"], null, true);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this), false);

            this.gridAg.finalizeInitGrid("manage.attest.time.rules", true, undefined, true);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData(), true, () => this.selectedItemsService.Save(), () => { return this.saveButtonIsDisabled() });
    }

    private saveButtonIsDisabled(): boolean {
        return !this.selectedItemsService.SelectedItemsExist();
    }

    // SERVICE CALLS   

    private loadDayTypes(): ng.IPromise<any> {
        return this.attestService.getDayTypesDict(false).then(x => {
            this.dayTypes = x;
        });
    }

    public loadGridData(useCache: boolean) {
        this.progress.startLoadingProgress([() => {
            return this.attestService.getAttestRuleHeadsGrid(SoeModule.Time).then(x => {
                return x;
            }).then(data => {
                this.setData(data);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData(false);
    }

    private save(items: number[]) {
        var dict: any = {};

        _.forEach(items, (id: number) => {
            // Find entity
            var entity: any = this.gridAg.options.findInData((ent: any) => ent["attestRuleHeadId"] === id);

            // Push id and active flag to array
            if (entity !== undefined) {
                dict[id] = entity.isActive;
            }
        });

        if ((dict !== undefined) && (Object.keys(dict).length > 0)) {
            this.attestService.updateAttestRuleState(dict).then(result => {
                this.reloadData();
            });
        }
    }

    // EVENTS   

    edit(row) {
        // Send message to TabsController        
        if (this.readPermission || this.modifyPermission)
            this.messagingHandler.publishEditRow(row);
    }
}