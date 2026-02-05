import { ProductRowsProductDTO } from "../../../../../Common/Models/ProductDTOs";
import { PurchaseDeliveryRowDTO } from "../../../../../Common/Models/PurchaseDeliveryDTO";
import { GridControllerBase2Ag } from "../../../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { ICompositionGridController } from "../../../../../Core/ICompositionGridController";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { IProductSmallDTO } from "../../../../../Scripts/TypeLite.Net4";
import { CompanySettingType, Feature, SoeEntityState, UserSettingType } from "../../../../../Util/CommonEnumerations";
import { SoeGridOptionsEvent } from "../../../../../Util/Enumerations";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { GridEvent } from "../../../../../Util/SoeGridOptions";

class DeliveryRowsController extends GridControllerBase2Ag implements ICompositionGridController {

    private deliveryRows: PurchaseDeliveryRowDTO[] = [];
    private visibleRows: PurchaseDeliveryRowDTO[] = [];
    private products: IProductSmallDTO[] = [];
    private productList: ProductRowsProductDTO[] = [];
    private productUnits: any[];
    private deliveryId = 0;

    private readOnly: boolean;
    private parentGuid: any;

    // Company settings
    private defaultStockId = 0;
    private defaultProductUnitId = 0;
    private defaultVatCodeId = 0;
    
    //permissions
    private useStock = false;
    private showSalesPricePermission = false;

    //@ngInject
    constructor(
        private coreService: ICoreService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "billing.purchase.rows", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onDoLookUp(() => this.doLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.afterSetup());

        this.onInit({});

        this.productList = [];

        this.$scope.$on('refreshRows', (e, a) => {
            if (this.deliveryRows)
                this.gridAg.options.refreshRows();
        });
    }

    onInit(parameters: any) {
        this.parameters = parameters;

        this.flowHandler.start([
            { feature: Feature.Billing_Purchase_Purchase_Edit, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Stock, loadReadPermissions: true, loadModifyPermissions: true },
            { feature: Feature.Billing_Product_Products_ShowSalesPrice, loadReadPermissions: true, loadModifyPermissions: true }
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Billing_Purchase_Purchase_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Purchase_Purchase_Edit].modifyPermission;
        this.useStock = response[Feature.Billing_Stock].modifyPermission;
        this.showSalesPricePermission = response[Feature.Billing_Product_Products_ShowSalesPrice].modifyPermission;
    }

    private doLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadCompanySettings(),
        ]).then(() => {
            this.loadUserAndCompanySettings();
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            CompanySettingType.BillingDefaultInvoiceProductUnit,
            CompanySettingType.BillingDefaultStock,
        ];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultProductUnitId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultInvoiceProductUnit);
            this.defaultStockId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.BillingDefaultStock);
        });
    }

    private loadUserAndCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [
            UserSettingType.BillingDefaultStockPlace
        ];

        return this.coreService.getUserAndCompanySettings(settingTypes).then(x => {
            const userDefaultStockId = SettingsUtility.getIntUserSetting(x, UserSettingType.BillingDefaultStockPlace);
            if (userDefaultStockId) 
                this.defaultStockId = userDefaultStockId;
        });
    }

    private afterSetup() {
        this.setupWatchers();
    }

    private setupWatchers() {
        if (!this.deliveryRows)
            this.deliveryRows = [];
        
        this.$scope.$watch(() => this.deliveryRows, (newVal, oldVal) => {
            this.purchaseRowsUpdated();
        });
    }

    private setupGrid() {

        this.gridAg.options.setMinRowsToShow(6);
        this.gridAg.options.setAutoHeight(true);
        const defaultEditable = true;
        
        const gridEvents: GridEvent[] = [];
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.BeginCellEdit, (entity, colDef) => { this.beginCellEdit(entity, colDef); }));
        gridEvents.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        this.gridAg.options.subscribe(gridEvents);

        const keys: string[] = [
            "common.rownr",
            "billing.order.ordernr",
            "billing.purchase.purchasenr",
            "billing.purchase.delivery.finaldelivery",
            "billing.purchase.delivery.remainingqty",
            "billing.purchase.delivery.purchaseqty",
            "billing.productrows.stockcode",
            "billing.productrows.addtextrow",
            "billing.productrows.stockcode",
            "billing.purchaserows.productnr",
            "billing.purchaserows.purchaseprice",
            "billing.purchaserows.purchasepricecurrency",
            "billing.purchaserows.quantity",
            "billing.purchaserows.deliverydate",
            "billing.purchaserows.productnr",
            "billing.purchaserows.text",
            "billing.purchaserows.deliveredquantity",
            "billing.purchaserows.purchaseprice",
            "billing.purchaserows.purchaseunit",
            "billing.purchaserows.text",
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.gridAg.addColumnIsModified("isModified");
            this.gridAg.addColumnText("productNr", terms["billing.purchaserows.productnr"], 100, false, { editable: false });
            this.gridAg.addColumnText("productName", terms["billing.purchaserows.text"], 100, false, { editable: false });
            
            this.gridAg.addColumnNumber("deliveredQuantity", terms["billing.purchaserows.deliveredquantity"], 50, { editable: (data) => this.isRowEditable(data) });

            this.gridAg.addColumnNumber("purchaseQuantity", terms["billing.purchase.delivery.purchaseqty"], 50, { editable: false });
            this.gridAg.addColumnNumber("remainingQuantity", terms["billing.purchase.delivery.remainingqty"], 50, { editable: false, enableHiding: false });
            this.gridAg.addColumnBool("setRowAsDelivered", terms["billing.purchase.delivery.finaldelivery"], 50, false, null, "isLocked" );

            this.gridAg.addColumnText("stockCode", terms["billing.productrows.stockcode"], 100, false, { editable: false });

            this.gridAg.addColumnNumber("purchasePriceCurrency", terms["billing.purchaserows.purchasepricecurrency"], 80, { enableHiding: false, decimals: 2,  editable: (data) => this.isRowEditable(data), maxDecimals: 4 });
            this.gridAg.addColumnDate("deliveryDate", terms["billing.purchaserows.deliverydate"], null, null, null, { enableHiding: false, editable: (data) => this.isRowEditable(data) });
            this.gridAg.addColumnText("purchaseNr", terms["billing.purchase.purchasenr"], null, true, { editable: false });
            this.gridAg.finalizeInitGrid("billing.purchase.rows", false);
        });
    }

    private isRowEditable(row: PurchaseDeliveryRowDTO) {
        return !row.isLocked;
    }

    private beginCellEdit(row: PurchaseDeliveryRowDTO, colDef: uiGrid.IColumnDef) {
        switch (colDef.field) {
        
        }
    }

    private afterCellEdit(row: PurchaseDeliveryRowDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue && colDef.field !== 'productNr')
            return;

        switch (colDef.field) {
            case 'deliveredQuantity':
                row.deliveredQuantity = NumberUtility.parseNumericDecimal(newValue);
                row.remainingQuantity -= (NumberUtility.parseNumericDecimal(newValue) - NumberUtility.parseNumericDecimal(oldValue));
                row['setRowAsDelivered'] = row.remainingQuantity <= 0;
                this.setRowAsModified(row);
                this.purchaseRowsUpdated();
                break;
            case 'purchasePriceCurrency':
                row.purchasePriceCurrency = NumberUtility.parseNumericDecimal(newValue);
                this.setRowAsModified(row);
                break;
        }
    }

    public setRowAsModified(row: PurchaseDeliveryRowDTO) {
        if (row) {
            row.isModified = true;
            this.setParentAsModified();
        }
    }

    private setParentAsModified() {
        this.$scope.$applyAsync(() => this.messagingHandler.publishSetDirty(this.parentGuid));
    }
       
    private purchaseRowsUpdated() {
        if (this.deliveryRows) {
            this.visibleRows = _.orderBy(_.filter(this.deliveryRows, r => r.state === SoeEntityState.Active), 'rowNr');
            this.gridAg.setData(this.visibleRows);
        }
    }
}

export class DeliveryRowsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getGlobalUrl("Shared/Billing/Purchase/Directives/DeliveryRows/DeliveryRows.html"),
            scope: {
                parentGuid: "=",
                purchaseDeliveryId: "=",
                deliveryRows: '=',
            },
            restrict: 'E',
            replace: true,
            controller: DeliveryRowsController,
            controllerAs: 'directiveCtrl',
            bindToController: true,
        };
    }
}