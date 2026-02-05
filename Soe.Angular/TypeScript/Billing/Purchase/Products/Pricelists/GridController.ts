import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";
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
import { ISupplierProductService } from "../../../../Shared/Billing/Purchase/Purchase/SupplierProductService";
import { ISupplierService } from "../../../../Shared/Economy/Supplier/SupplierService";
import { Feature } from "../../../../Util/CommonEnumerations";


export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    //arrays
    private suppliers: SmallGenericType[] = [];

    //props
    private selectedSupplier: SmallGenericType;

    private modalInstance: any;


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
        $uibModal) {
        super(gridHandlerFactory, "Billing.Purchase.Pricelists", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onLoadGridData(() => this.loadGridData(true))
            .onDoLookUp(() => this.onDoLookups())
            .onSetUpGrid(() => this.setupGrid());

        this.modalInstance = $uibModal;
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.onTabActivetedAndModified(() => {
                this.loadGridData();
            });
        }

        this.flowHandler.start([
            { feature: Feature.Billing_Purchase_Pricelists, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Purchase_Pricelists].modifyPermission;
        this.modifyPermission = response[Feature.Billing_Purchase_Pricelists].modifyPermission;
        if (this.modifyPermission) {
            this.messagingHandler.publishActivateAddTab();
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadGridFromFilter());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([this.loadSuppliers()]);
    }

    private loadSuppliers(): ng.IPromise<any> {
        return this.supplierService.getSuppliersDict(true, false, true).then(data => {
            this.suppliers = data;
        });
    }


    public setupGrid() {
        // Columns
        const keys: string[] = [
            "billing.purchase.supplierno",
            "billing.purchase.suppliername",
            "billing.purchase.product.wholesellertype",
            "billing.purchase.product.wholeseller",
            "billing.purchase.product.pricestartdate",
            "billing.purchase.product.priceenddate",
            "common.currency",
            "core.created"
        ];

        this.translationService.translateMany(keys).then((terms) => {

            this.gridAg.addColumnText("supplierNr", terms["billing.purchase.supplierno"], null);
            this.gridAg.addColumnText("supplierName", terms["billing.purchase.suppliername"], null);
            this.gridAg.addColumnText("sysWholeSellerName", terms["billing.purchase.product.wholeseller"], null, false, { hide:true});
            this.gridAg.addColumnText("sysWholeSellerTypeName", terms["billing.purchase.product.wholesellertype"], null, false, { hide: true });
            this.gridAg.addColumnText("currencyCode", terms["common.currency"], null);
            this.gridAg.options.addColumnDate("startDate", terms["billing.purchase.product.pricestartdate"], null, true, null, null);
            this.gridAg.options.addColumnDate("endDate", terms["billing.purchase.product.priceenddate"], null, true, null, null);
            this.gridAg.addColumnDateTime("created", terms["core.created"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("billing.purchase.pricelists.list", true);

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

        this.progress.startLoadingProgress(
            [() => this.supplierProductService.getSupplierPricelists(this.selectedSupplier?.id || 0).then(data => {
                this.setData(data);
            })]
        )
    }
}