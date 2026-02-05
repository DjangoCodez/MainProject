import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IProductSmallDTO, IInvoiceProductCopyResult, IActionResult } from "../../../Scripts/TypeLite.Net4";
import { IProductService } from "../../../Shared/Billing/Products/ProductService";
import { IReportService } from "../../../Core/Services/ReportService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { CoreUtility } from "../../../Util/CoreUtility";
import { ProductSearchResult, InvoiceProductDTO } from "../../../Common/Models/ProductDTOs";
import { SearchInvoiceProductController } from "../../../Shared/Billing/Dialogs/SearchInvoiceProduct/SearchInvoiceProductController";
import { SelectReportController } from "../../../Common/Dialogs/SelectReport/SelectReportController";
import { Feature, CompanySettingType, SoeReportTemplateType, SoeEntityType } from "../../../Util/CommonEnumerations";
import { SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons, IconLibrary } from "../../../Util/Enumerations";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ISelectedItemsService } from "../../../Core/Services/SelectedItemsService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { ImportUnitConversionsDialogController } from "./Dialogs/ImportUnitConversions/ImportUnitConversions";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { BatchUpdateController } from "../../../Common/Dialogs/BatchUpdate/BatchUpdateDirective";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private terms: any;
    private modalInstance: any;
    private defaultPriceListTypeId: number;
    private customerId = 0;
    private currencyId: number;
    private products: IProductSmallDTO[];
    private selectedCount = 0;
    private hasStockPermission: boolean;
    private hasBatchUpdatePermission: boolean;
    private useExtendSearchInfo = false;

    //@ngInject
    constructor($uibModal,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private productService: IProductService,
        private reportService: IReportService,
        private urlHelperService: IUrlHelperService,
        private $window: ng.IWindowService,
        private $scope: ng.IScope,
        private selectedItemsService: ISelectedItemsService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Billing.Products.Product", progressHandlerFactory, messagingHandlerFactory);
        this.setIdColumnNameOnEdit("productId");
        super.onTabActivetedAndModified(() => {
            this.loadGridData();
        });

        this.selectedItemsService.setup($scope, "productId", (items: number[]) => this.save(items));

        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onLoadSettings(() => this.loadCompanySettings())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [CompanySettingType.BillingDefaultPriceListType, CompanySettingType.CoreBaseCurrency, CompanySettingType.BillingShowExtendedInfoInExternalSearch];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultPriceListTypeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultPriceListType);
            this.currencyId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CoreBaseCurrency);
            this.useExtendSearchInfo = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingShowExtendedInfoInExternalSearch, false);//true; 
        });
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;
        this.flowHandler.start([
            { feature: Feature.Billing_Product_Products, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Stock, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Product_Products_BatchUpdate, loadReadPermissions: true, loadModifyPermissions: true }
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Product_Products].readPermission;
        this.modifyPermission = response[Feature.Billing_Product_Products].modifyPermission;
        this.hasStockPermission = response[Feature.Billing_Stock].readPermission;
        this.hasBatchUpdatePermission = response[Feature.Billing_Product_Products_BatchUpdate].modifyPermission;

        if (this.modifyPermission) {
            // Send messages to TabsController
            this.messagingHandler.publishActivateAddTab();
        }
    }

    public setupGrid() {
        const keys: string[] = [
            "common.active",
            "common.number",
            "common.name",
            "billing.products.productgroupcode",
            "billing.products.productgroupname",
            "billing.products.productcategories",
            "billing.products.eancode",
            "billing.products.external",
            "core.edit",
            "core.yes",
            "core.no",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.info",
            "billing.products.print.productlist.noselectedproducts",
            "common.batchupdate",
            "billing.product.materialcode"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.gridAg.addColumnActive("isActive", terms["common.active"], 60, (params) => this.selectedItemsService.CellChanged(params));
            this.gridAg.addColumnText("number", terms["common.number"], null, true, { filterOptions: ["startsWith", "contains", "endsWith"] });
            this.gridAg.addColumnText("name", terms["common.name"], null, true);
            this.gridAg.addColumnText("productGroupCode", terms["billing.products.productgroupcode"], null, true);
            this.gridAg.addColumnText("productGroupName", terms["billing.products.productgroupname"], null, true);
            this.gridAg.addColumnText("productCategories", terms["billing.products.productcategories"], null, true);
            this.gridAg.addColumnText("eanCode", terms["billing.products.eancode"], null, true);
            this.gridAg.addColumnText("isExternal", terms["billing.products.external"], null, true);
            this.gridAg.addColumnText("timeCodeName", terms["billing.product.materialcode"], null, true, { hide: true });
            this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rowNode) => { this.selectionChanged() }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (rowNode) => { this.selectionChanged() }));
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("billing.products.products", true, undefined, true);
        });
    }

    selectionChanged() {
        this.$timeout(() => {
            this.selectedCount = this.gridAg.options.getSelectedCount();
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.loadGridData(), true, () => this.selectedItemsService.Save(), () => !(this.selectedItemsService.SelectedItemsExist()));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("core.print", "core.print", IconLibrary.FontAwesome, "fa-print", () => { this.openReportDialog(); })));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("billing.products.searchfromexternalproducts", "billing.products.searchfromexternalproducts", IconLibrary.FontAwesome, "fa-search",
            () => { this.searchProduct(); }
        )));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("billing.products.product.unitconversion", "billing.products.product.unitconversion", IconLibrary.FontAwesome, "fa-divide",
            () => { this.importUnitConversion(); }, () => { return this.selectedCount === 0; }, () => { return !this.hasStockPermission; }
        )));

        if (this.hasBatchUpdatePermission) {
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.batchupdate.title", "common.batchupdate.title", IconLibrary.FontAwesome, "fa-pencil",
                () => { this.openBatchUpdate(); }, () => { return this.selectedCount === 0; }, () => { return false }
            )));
        }
    }

    public loadGridData() {
        this.productService.getInvoiceProductsForGrid(false, true, false, true, true).then((x) => {
            x.forEach(row => {
                if (row.external === true)
                    row.isExternal = this.terms["core.yes"];
                else if (row.external === false)
                    row.isExternal = this.terms["core.no"];
            });
            this.setData(x);
            this.selectedCount = 0;
        });
    }

    protected searchProduct(info: string = '') {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Dialogs/SearchInvoiceProduct/SearchInvoiceProduct.html"),
            controller: SearchInvoiceProductController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg', //(this.useExtendSearchInfo ? 'xl' : 'lg'),
            windowClass: (this.useExtendSearchInfo ? 'fullsize-modal' : ''),
            resolve: {
                translationService: () => { return this.translationService },
                coreService: () => { return this.coreService },
                productService: () => { return this.productService },
                hideProducts: () => { return false },
                priceListTypeId: () => { return 0 },
                customerId: () => { return this.customerId },
                currencyId: () => { return this.currencyId },
                sysWholesellerId: () => { return undefined },
                number: () => { return '' },
                name: () => { return '' },
                quantity: () => { return 1 },
                info: () => { return info }
            }
        });

        modal.result.then((result: ProductSearchResult) => {
            if (result) {
                this.copyInvoiceProduct(result, true);
            }
        }, function () {
            // Cancelled
        });

        return modal;
    }

    private openBatchUpdate() {

        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/BatchUpdate/Views/BatchUpdate.html"),
            controller: BatchUpdateController,
            controllerAs: "ctrl",
            bindToController: true,
            backdrop: 'static',
            size: 'md',
            resolve: {
                entityType: () => { return SoeEntityType.InvoiceProduct },
                selectedIds: () => { return _.map(this.gridAg.options.getSelectedRows(), 'productId') }
            }
        });

        modal.result.then(data => {
            // Reset cache
            this.loadGridData();
        }, function () {
            // Cancelled
        });
        this.$scope.$applyAsync();
        return modal;
    }

    protected importUnitConversion() {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Billing/Products/Products/Dialogs/ImportUnitConversions/Views/ImportUnitConversions.html"),
            controller: ImportUnitConversionsDialogController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                productService: () => { return this.productService },
                selectedProducts: () => { return _.map(this.gridAg.options.getSelectedRows(), 'productId') }
            }
        });

        modal.result.then((result: ProductSearchResult) => {
            // Reset cache
            this.productService.getProductUnits(true);
        }, function () {
            // Cancelled
        });

        return modal;
    }

    private openReportDialog() {

        if (this.gridAg.options.getSelectedCount() === 0) {
            this.notificationService.showDialog(this.terms["core.info"], this.terms["billing.products.print.productlist.noselectedproducts"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
        } else {
            const reportTypes: number[] = [SoeReportTemplateType.ProductListReport];

            const modal = this.modalInstance.open({
                templateUrl: this.urlHelperService.getGlobalUrl("Common/Dialogs/SelectReport/SelectReport.html"),
                controller: SelectReportController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    module: () => { return null },
                    reportTypes: () => { return reportTypes },
                    showCopy: () => { return false },
                    showEmail: () => { return false },
                    copyValue: () => { return false },
                    reports: () => { return null },
                    defaultReportId: () => { return null },
                    langId: () => { return CoreUtility.sysCountryId },
                    showReminder: () => { return false },
                    showLangSelection: () => { return false },
                    showSavePrintout: () => { return false },
                    savePrintout: () => { return false }
                }
            });

            modal.result.then((result: any) => {

                if ((result) && (result.reportId)) {
                    this.printReport(result.reportId);
                }
            });
        }

    }

    private printReport(reportId: number) {

        const selectedRows = this.gridAg.options.getSelectedRows();

        const productIds: any[] = [];

        for (let i = 0; i < selectedRows.length; i++) {
            const row = selectedRows[i];
            productIds.push(row.productId);
        }

        this.reportService.getProductListReportUrl(productIds, reportId, SoeReportTemplateType.ProductListReport).then((url) => {
            HtmlUtility.openInSameTab(this.$window, url);
        });
    }

    private copyInvoiceProduct(searchResult: ProductSearchResult, askMerge: boolean = false) {
        this.productService.copyInvoiceProduct(searchResult.productId, searchResult.purchasePrice, searchResult.salesPrice, searchResult.productUnit, searchResult.priceListTypeId, searchResult.sysPriceListHeadId, searchResult.sysWholesellerName, 0, searchResult.priceListOrigin).then((result: IInvoiceProductCopyResult) => {
            const product: InvoiceProductDTO = result.product;
            if (product) {
                this.loadGridData();
            }
        });
    }

    private save(items: number[]) {
        let dict: any = {};

        _.forEach(items, (id: number) => {
            // Find entity
            const entity: any = this.gridAg.options.findInData((ent: any) => ent["productId"] === id);

            // Push id and active flag to array
            if (entity !== undefined) {
                dict[id] = entity.isActive;
            }
        });

        if (dict !== undefined) {
            this.productService.updateProductState(dict).then((result: IActionResult) => {
                if (!result.success) {
                    this.notificationService.showDialog("", result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                }

                this.loadGridData();
            });
        }
    }
}
