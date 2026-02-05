import { GridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPriceListTypeDTO, IPriceListDTO } from "../../../Scripts/TypeLite.Net4";
import { IProductService } from "../../../Shared/Billing/Products/ProductService";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { NumberUtility } from "../../../Util/NumberUtility";
import { PriceListDTO } from "../../../Common/Models/pricelistdto";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export class ProjectPriceListsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Billing/Projects/Directives/ProjectPriceLists.html'),
            scope: {
                priceListRows: '=',
                readOnly: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: ProjectPriceListsDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class ProjectPriceListsDirectiveController extends GridControllerBase {
    // Setup
    private priceListRows: any[];
    private readOnly: boolean;

    // Collections        
    allPriceLists: IPriceListTypeDTO[];

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        coreService: ICoreService,
        private productService: IProductService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService) {
        super("Billing.Projects.List.Directives.PriceLists", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    protected setupCustomToolBar() {
        this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("billing.product.pricelist.new", "billing.product.pricelist.new", IconLibrary.FontAwesome, "fa-plus",
            () => { this.addRow(); },
            null,
            () => { return this.readOnly; })));
    }

    public setupGrid() {
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.showGridFooter = false;
        this.soeGridOptions.setMinRowsToShow(4);
        this.setupTypeAhead();
        this.doubleClickToEdit = false;
        //
        const events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        this.soeGridOptions.subscribe(events);

        this.startLoad();
        this.$q.all([this.loadPriceLists()]).then(() => this.setupGridColumns());
    }

    private setupGridColumns() {
        const keys: string[] = [
            "billing.product.pricelist.name",
            "billing.product.pricelist.price",
            "core.delete"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.soeGridOptions.addColumnSelect("priceListTypeId", terms["billing.product.pricelist.name"], null, this.allPriceLists, false, true, "name", "priceListTypeId", "name", "rowChanged");

            var colDef3 = this.soeGridOptions.addColumnNumber("price", terms["billing.product.pricelist.price"], "30%", false, 2);
            colDef3.enableCellEdit = true;
            this.soeGridOptions.addColumnDelete(terms["core.delete"], "deleteRow");

            super.gridDataLoaded(this.priceListRows);
        });
    }


    // Lookups

    private loadPriceLists(): ng.IPromise<any> {
        return this.productService.getPriceLists().then(x => {
            this.allPriceLists = x;
            _.forEach(this.priceListRows, (row) => {
                row.name = _.filter(this.allPriceLists, a => a.priceListTypeId == row.priceListTypeId)[0].name;
            });
        });
    }

    //Events        
    private rowChanged(row: any) {
        var list = _.find(this.allPriceLists, p => p.priceListTypeId == row.priceListTypeId)
        row.name = list.name;
    }

    private afterCellEdit(row: IPriceListDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        // afterCellEdit will always be called, even if just tabbing through the columns.
        // No need to perform anything if value has not been changed.
        if (newValue === oldValue)
            return;

        switch (colDef.field) {
            case 'price':
                row.price = NumberUtility.parseNumericDecimal(row.price);
                break;
        }
    }

    // Actions
    private addRow() {
        var row: PriceListDTO = new PriceListDTO();
        row.productId = 0;
        row.price = 0;

        this.soeGridOptions.addRow(row);
        this.soeGridOptions.focusRowByRow(row, 0);
    }

    private deleteRow(row) {
        this.soeGridOptions.deleteRow(row);
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
    }
}
