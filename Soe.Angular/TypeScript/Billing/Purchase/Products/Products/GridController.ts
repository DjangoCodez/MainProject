import { SupplierProductSearchDTO } from "../../../../Common/Models/SupplierProductDTO";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ISmallGenericType, ISupplierProductGridDTO } from "../../../../Scripts/TypeLite.Net4";
import { ISupplierProductService } from "../../../../Shared/Billing/Purchase/Purchase/SupplierProductService";
import { ISupplierService } from "../../../../Shared/Economy/Supplier/SupplierService";
import { Feature } from "../../../../Util/CommonEnumerations";
import { IconLibrary } from "../../../../Util/Enumerations";
import { ToolBarButton, ToolBarUtility } from "../../../../Util/ToolBarUtility";
import { ImportProductsDialog } from "./ImportProductsDialog/ImportProductsDialog";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //Filter
    private suppliersDict: any[] = [];
    private selectedSuppliersDict: any[] = [];
    private supplierProductNr: string;
    private supplierProductName: string;
    private productNr: string;
    private productName: string;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private supplierProductService: ISupplierProductService,
        private supplierService: ISupplierService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $uibModal) {
        super(gridHandlerFactory, "Billing.Purchase.Products", progressHandlerFactory, messagingHandlerFactory);
        this.setIdColumnNameOnEdit("supplierProductId");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onLoadGridData(() => this.loadGridData(true))
            .onDoLookUp(() => this.onDoLookups())
            .onSetUpGrid(() => this.setupGrid());
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;


        this.flowHandler.start([
            { feature: Feature.Billing_Purchase_Products, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Purchase_Products].modifyPermission;
        this.modifyPermission = response[Feature.Billing_Purchase_Products].modifyPermission;
        if (this.modifyPermission) {
            this.messagingHandler.publishActivateAddTab();
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadGridFromFilter());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton(
            "billing.purchase.product.importpricelist",
            "billing.purchase.product.importpricelist",
            IconLibrary.FontAwesome, "fa-file-import",
            () => this.importProductsDialog(),
        )));

    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([this.loadSuppliers()]);
    }

    private loadSuppliers(): ng.IPromise<any> {
        return this.supplierService.getSuppliersDict(true, false, true).then((x: ISmallGenericType[]) => {
            x.forEach(s => {
                this.suppliersDict.push({ id: s.id, label: s.name });
            } )
        });
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "billing.purchase.supplierno",
            "billing.purchase.suppliername",
            "billing.purchase.product.supplieritemno",
            "billing.purchase.product.supplieritemname",
            "billing.product.number",
            "billing.product.name"
        ];

        this.translationService.translateMany(keys).then((terms) => {

            this.gridAg.addColumnText("supplierNr", terms["billing.purchase.supplierno"], null);
            this.gridAg.addColumnText("supplierName", terms["billing.purchase.suppliername"], null);
            this.gridAg.addColumnText("supplierProductNr", terms["billing.purchase.product.supplieritemno"], null, null, { filterOptions: ["startsWith", "contains", "endsWith"] });
            this.gridAg.addColumnText("supplierProductName", terms["billing.purchase.product.supplieritemname"], null);
            this.gridAg.addColumnText("productNr", terms["billing.product.number"], null, null, { filterOptions: ["startsWith", "contains", "endsWith"] });
            this.gridAg.addColumnText("productName", terms["billing.product.name"], null);

            if (this.modifyPermission) {
                this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));
            }

            this.gridAg.finalizeInitGrid("billing.purchase.products.list", true);
        });
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 1000, { leading: false, trailing: true });

    public loadGridData(firstLoad = false) {
        if (firstLoad) {
            this.setData([]);
            return;
        }
        const searchModel = new SupplierProductSearchDTO();
        searchModel.supplierIds = [];
        searchModel.supplierProduct = this.supplierProductNr;
        searchModel.supplierProductName = this.supplierProductName;
        searchModel.product = this.productNr;
        searchModel.productName = this.productName;

        if (this.selectedSuppliersDict) {
            this.selectedSuppliersDict.forEach(s => searchModel.supplierIds.push(s.id));
        }

        this.supplierProductService.getSupplierProducts(searchModel).then((data) => {
            this.setData(data);
        });
        
    }

    private importProductsDialog() {
        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Billing/Purchase/Products/Products/ImportProductsDialog/ImportProductsDialog.html"),
            controller: ImportProductsDialog,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {}
        });

        modal.result.then((result: any) => {
        });
    }

    private supplierSelectionComplete() {
        console.log("supplierSelectionComplete");
        //this.sortSelectedRows();
    }
}