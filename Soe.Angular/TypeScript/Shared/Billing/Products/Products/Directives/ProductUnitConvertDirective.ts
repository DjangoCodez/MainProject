import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IProductService } from "../../ProductService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { IControllerFlowHandlerFactory } from "../../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IProductUnitSmallDTO } from "../../../../../Scripts/TypeLite.Net4";
import { ICompositionGridController } from "../../../../../Core/ICompositionGridController";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { Feature } from "../../../../../Util/CommonEnumerations";
import { ProductUnitConvertDTO } from "../../../../../Common/Models/ProductUnitConvertDTO";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../../Util/Enumerations";
import { GridControllerBase2Ag } from "../../../../../Core/Controllers/GridControllerBase2Ag";


export class ProductUnitConvertDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Billing/Products/Products/Directives/Views/ProductUnitConvert.html'),
            scope: {
                productId: '=',
                productUnitConverts: '=',
                productUnitId: '=',
                readOnly: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: ProductUnitConvertDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class ProductUnitConvertDirectiveController extends GridControllerBase2Ag implements ICompositionGridController {
    // Setup
    private productId: number;
    private productUnitId: number;
    private readOnly: boolean;
    private productUnitConverts: any[] = [];
    private productUnits: IProductUnitSmallDTO[];
    private productUnitsDDL: IProductUnitSmallDTO[];

    //@ngInject
    constructor(
        private productService: IProductService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private notificationService: INotificationService,
        private $q: ng.IQService,
        private $scope: ng.IScope) {
        super(gridHandlerFactory, "Billing.Products.Products.Views.Stocks", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify
            })
            .onSetUpGrid(() => this.setupGrid())

        this.setupWatches();
        
        this.flowHandler.start({ feature: Feature.Billing_Product_Products_Edit, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private setupWatches() {
        this.$scope.$watch(() => this.productUnitConverts, (newVal, oldVal) => {
            this.setGridData();
        });
        this.$scope.$watch(() => this.productUnitId, (newVal, oldVal) => {
            if (this.productUnitsDDL) {
                var itemIndex = _.findIndex(this.productUnitsDDL, x => x.productUnitId === this.productUnitId);
                this.productUnitsDDL.splice(itemIndex, 1);
            }
        });
    }

    public onInit(parameters: any) {
        // not called!
    }

    private loadProductUnits(): ng.IPromise<any> {
        return this.productService.getProductUnits().then((x) => {
            this.productUnits = x;
            this.productUnitsDDL = this.productUnits;
        });
    }

    private setGridData() {
        this.gridAg.setData(this.productUnitConverts.filter(x => !x.isDeleted));
    }

    public setupGrid() {

        this.gridAg.options.enableRowSelection = false;
        this.gridAg.options.setMinRowsToShow(8);
        this.gridAg.options.enableGridMenu = false;

        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        this.gridAg.options.subscribe(events);

        this.$q.all([this.loadProductUnits()]).then(() => {
            this.setupGridColumns();
        });
    }

    private setupGridColumns() {
        
        const keys: string[] = [
            "billing.product.productunit",
            "billing.product.productunit.convertfactor",
            "core.delete"
        ];
        
        this.translationService.translateMany(keys).then(terms => {
            this.gridAg.addColumnSelect("productUnitId", terms["billing.product.productunit"], null, { editable:true, selectOptions: this.productUnitsDDL, populateFilterFromGrid: true, dropdownValueLabel: "name", dropdownIdLabel: "productUnitId", displayField: "productUnitName" });
            this.gridAg.addColumnNumber("convertFactor", terms["billing.product.productunit.convertfactor"], null, {editable:true});
            this.gridAg.addColumnDelete(terms["core.delete"], this.deleteRow.bind(this) );
            this.gridAg.finalizeInitGrid("billing.product.productunit", false);
            this.setGridData();
        });
    }



    private afterCellEdit(row: ProductUnitConvertDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;
        
       if (colDef.field === "convertFactor" && newValue <= 0) {
            const keys: string[] = [
                "billing.product.productunit.convertfactornotzero",
                "core.error"
            ];

            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialog(terms["core.error"], terms["billing.product.productunit.convertfactornotzero"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
            row.convertFactor = oldValue;
            return;
        }
        
        if (row.convertFactor) {
            row.isModified = true;
            this.messagingHandler.publishSetDirty();
        }
    }

    // Actions
    
    private addRow() {
        var row: ProductUnitConvertDTO = new ProductUnitConvertDTO();
        row.productId = this.productId;
        //row.isModified = true;
        this.productUnitConverts.push(row);
        this.setGridData();
        this.gridAg.options.startEditingCell(row, "productUnitId");
        //this.messagingHandler.publishSetDirty();
    }

    private deleteRow(row: ProductUnitConvertDTO) {
        row.isDeleted = true;
        row.isModified = true;
        this.setGridData();
        this.messagingHandler.publishSetDirty();
    }
}
