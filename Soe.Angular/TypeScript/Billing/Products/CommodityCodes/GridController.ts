import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IProductService } from "../../../Shared/Billing/Products/ProductService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ISelectedItemsService } from "../../../Core/Services/SelectedItemsService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Collections
    private changedItems = [];

    // Flags
    disableSave = true;
    private loadAll = false;
    private onlyActive = true;

    //@ngInject
    constructor(
        $scope: ng.IScope,
        private productService: IProductService,
        private translationService: ITranslationService,
        private selectedItemsService: ISelectedItemsService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "common.commoditycodes.codes", progressHandlerFactory, messagingHandlerFactory)

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.selectedItemsService.setup($scope, "sysIntrastatCodeId", (items: number[]) => this.save(items));
    }

    public onInit(parameters: any) {
        this.messagingHandler.onGridDataReloadRequired(x => { this.loadGridData(); });

        this.flowHandler.start({ feature: Feature.Economy_Intrastat_Administer, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData(), true, () => this.selectedItemsService.Save(), () => { return !this.selectedItemsService.SelectedItemsExist() });
    }

    public loadGridData() {
        this.gridAg.clearData();
        this.progress.startLoadingProgress([() => {
            return this.productService.getCustomerCommodityCodes(this.onlyActive, false).then((x) => {
                this.setData(x);
                this.loadAll = true;
            });
        }]);
    }

    public save(items: number[]) {
        const dict: any = {};

        _.forEach(items, (id: number) => {
            const entity: any = this.gridAg.options.findInData((ent: any) => ent["sysIntrastatCodeId"] === id);

            if (entity !== undefined) 
                dict[id] = entity.isActive;
        });

        this.progress.startSaveProgress((completion) => {
            this.productService.saveCustomerCommodityCodes(dict).then((result) => {
                if (result.success)
                    completion.completed();
                else
                    completion.failed(result.errorMessage);
            });
        }, null).then(() => {
            this.loadGridData();
        });
    }

    private setupGrid() {
        this.gridAg.options.enableRowSelection = false;

        const translationKeys: string[] = [
            "common.active",
            "common.code",
            "common.description",
            "core.edit",
            "common.commoditycodes.otherquantity",
            "common.startdate",
            "common.stopdate"
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {
            this.gridAg.addColumnActive("sysIntrastatCodeId", terms["common.active"], 60, (params) => this.selectedItemsService.CellChanged(params));
            this.gridAg.addColumnText("code", terms["common.code"], null, false);
            this.gridAg.addColumnText("text", terms["common.description"], null, false);
            this.gridAg.addColumnBool("useOtherQuantity", terms["common.commoditycodes.otherquantity"], 30, false);
            this.gridAg.addColumnDate("startDate", terms["common.startdate"], null, false);
            this.gridAg.addColumnDate("endDate", terms["common.stopdate"], null, false);

            // Events
            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.FilterChanged, () => {
                const filterInstance = this.gridAg.options.getFilterValueModel('isActive');
                if (this.loadAll && this.onlyActive && filterInstance && Array.isArray(filterInstance) && filterInstance[0] === 'false') {
                    this.onlyActive = false;
                    this.loadGridData();
                }
            }));
            
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("common.commoditycodes.codes", true, undefined, true)
        });
    }

    
}