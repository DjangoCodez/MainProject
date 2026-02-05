import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";
import { IPriceListTypeDTO } from "../../../../../Scripts/TypeLite.Net4";
import { IProductService } from "../../../../../Shared/Billing/Products/ProductService";
import { SoeGridOptionsEvent } from "../../../../../Util/Enumerations";
import { PriceListDTO } from "../../../../../Common/Models/PriceListDTO";
import { NumberUtility } from "../../../../../Util/NumberUtility";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { CompanySettingType, Feature, SoeEntityState } from "../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../Util/Constants";
import { GridControllerBase2Ag } from "../../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../../../Core/Handlers/gridhandlerfactory";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { Guid } from "../../../../../Util/StringUtility";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { ICoreService } from "../../../../../Core/Services/CoreService";

export class PriceListsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Billing/Products/Products/Directives/Views/PriceLists.html'),
            scope: {
                priceListRows: '=',
                readOnly: '=?',
                parentGuid: '=?'
            },
            restrict: 'E',
            replace: true,
            controller: PriceListsDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class PriceListsDirectiveController extends GridControllerBase2Ag implements ICompositionGridController {
    // Setup
    private priceListRows: any[] = [];
    private readOnly: boolean;
    private useQuantityPrices: boolean;

    private parentGuid: Guid;

    // Collections        
    allPriceLists: IPriceListTypeDTO[];

    // Flags
    priceListsLoaded: boolean = false;
    finishedGridSetup: boolean = false;

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private productService: IProductService,
        protected messagingService: IMessagingService,
        private coreService: ICoreService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $scope: ng.IScope) {
        super(gridHandlerFactory, "Billing.Products.Products.Views.PriceLists", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify
            })
            .onBeforeSetUpGrid(() => this.loadPriceLists())
            .onDoLookUp(() => this.ondoLookup())
            .onSetUpGrid(() => this.setupGrid())

        this.setupWatchers();

        this.flowHandler.start({ feature: Feature.Billing_Product_Products_Edit, loadReadPermissions: true, loadModifyPermissions: true });
    }

    public onInit(parameters: any) {
    }

    private ondoLookup(): ng.IPromise<any> {
        return this.loadCompanySettings();
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.priceListRows, () => {   
            if (this.priceListsLoaded) {
                this.setPriceListNames();
                if (this.finishedGridSetup) {
                    this.setGridData();
                }
            }
        });
    }


    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [CompanySettingType.BillingUseQuantityPrices];
        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useQuantityPrices = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.BillingUseQuantityPrices,false);
        });
    }

    public setupGrid() {
        this.gridAg.options.enableRowSelection = false;
        this.gridAg.options.setMinRowsToShow(10);
        this.gridAg.options.enableGridMenu = false;

        const keys: string[] = [
            "billing.product.pricelist.name",
            "billing.product.pricelist.price",
            "billing.products.pricelists.startdate",
            "billing.products.pricelists.stopdate",
            "core.delete",
            "common.quantity"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.gridAg.addColumnIsModified("isModified");
            this.gridAg.addColumnSelect("priceListTypeId", terms["billing.product.pricelist.name"], null, { editable: true, selectOptions: this.allPriceLists, populateFilterFromGrid: true, dropdownValueLabel: "name", dropdownIdLabel: "priceListTypeId", displayField: "name" });
            if (this.useQuantityPrices) {
                this.gridAg.addColumnNumber("quantity", terms["common.quantity"], null, { editable: true, decimals: 2, maxDecimals: 2 });
            }
            this.gridAg.addColumnNumber("price", terms["billing.product.pricelist.price"], null, { editable: true, decimals: 2, maxDecimals: 4 });
            this.gridAg.addColumnDate("startDate", terms["billing.products.pricelists.startdate"], null, true, null, { editable: true });
            this.gridAg.addColumnDate("stopDate", terms["billing.products.pricelists.stopdate"], null, true, null, { editable: true });
            this.gridAg.addColumnDelete(terms["core.delete"], this.deleteRow.bind(this));

            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("billing.invoices.pricelists.pricelist", false);

            this.setPriceListNames();
            this.setGridData();
            this.finishedGridSetup = true;
        });
    }

    // Lookups

    private loadPriceLists(): ng.IPromise<any> {
        return this.productService.getPriceListsGrid().then((x: any[]) => {
            x.forEach(x => x.name = x.name + " (" + x.currency + ")");
            this.allPriceLists = x;
            this.priceListsLoaded = true;
        });
    }

    //Events        
    private rowChanged(row: any) {
        const list = _.find(this.allPriceLists, p => p.priceListTypeId == row.priceListTypeId)
        row.name = list.name;
        this.setParentAsModified();
    }

    private afterCellEdit(row: PriceListDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        // afterCellEdit will always be called, even if just tabbing through the columns.
        // No need to perform anything if value has not been changed.
        if (newValue === oldValue)
            return;

        switch (colDef.field) {
            case 'price':
                row.price = NumberUtility.parseNumericDecimal(row.price);
                break;
            case 'quantity':
                row.quantity = NumberUtility.parseNumericDecimal(row.quantity);
                break;
        }
        row.isModified = true;
        this.gridAg.options.refreshRows(row);
        this.setParentAsModified();
    }

    // Actions
    private setGridData() {
        this.gridAg.setData(_.orderBy(this.priceListRows.filter(r => r.state == SoeEntityState.Active), ['name', 'quantity', 'startDate']));
    }

    private addRow() {
        const row = new PriceListDTO();
        row.isModified = true;
        this.priceListRows.push(row);
        this.setGridData();

        this.gridAg.options.startEditingCell(row, "priceListTypeId");

        this.setParentAsModified();
    }

    private deleteRow(row: PriceListDTO) {
        row.state = SoeEntityState.Deleted;
        row.isModified = true;

        this.setGridData();

        this.setParentAsModified();
    }

    private setPriceListNames() {
        _.forEach(this.priceListRows, (row) => {
            const priceList = _.find(this.allPriceLists, a => a.priceListTypeId == row.priceListTypeId)
            row.name = priceList ? priceList.name : " ";
        });
    }

    private setParentAsModified() {
        this.$scope.$applyAsync(() => this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.parentGuid }));
    }
}
