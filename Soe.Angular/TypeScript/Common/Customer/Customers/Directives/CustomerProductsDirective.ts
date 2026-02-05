import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ICommonCustomerService } from "../../CommonCustomerService";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { ToolBarUtility, ToolBarButton, ToolBarButtonGroup } from "../../../../Util/ToolBarUtility";
import { CustomerProductDTO } from "../../../Models/CustomerDTO";
import { ProductSmallDTO } from "../../../Models/ProductDTOs";
import { IconLibrary, SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { Constants } from "../../../../Util/Constants";
import { NumberUtility } from "../../../../Util/NumberUtility";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { TypeAheadOptionsAg } from "../../../../Util/SoeGridOptionsAg";

export class CustomerProductsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Common/Customer/Customers/Directives/CustomerProducts.html'),
            scope: {
                rows: '=',
                readOnly: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: CustomerProductsDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class CustomerProductsDirectiveController extends GridControllerBase2Ag {
    // Setup
    private rows: CustomerProductDTO[];
    private readOnly: boolean;

    // Collections
    products: ProductSmallDTO[] = [];

    // ToolBar
    protected buttonGroups = new Array<ToolBarButtonGroup>();

    //@ngInject
    constructor(
        private $filter: ng.IFilterService,
        private commonCustomerService: ICommonCustomerService,
        private translationService: ITranslationService,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super(gridHandlerFactory, "Common.Customer.Customers.Directives.CustomerProducts", progressHandlerFactory, messagingHandlerFactory);

        this.loadLookups();
    }

    protected setupCustomToolBar() {
        this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.customer.customer.product.new", "common.customer.customer.product.new", IconLibrary.FontAwesome, "fa-plus",
            () => { this.addRow(); },
            null,
            () => { return this.readOnly; })));
    }

    public loadLookups() {
        this.$q.all([this.loadProducts()]).then(() => {
            this.setupCustomToolBar();
            //this.setupTypeAhead();
            this.setupGrid();
        });
    }

    private setupGrid() {
        this.gridAg.options.enableGridMenu = false;
        this.gridAg.options.enableFiltering = false;
        this.gridAg.options.setMinRowsToShow(8);

        const keys: string[] = [
            "common.customer.customer.product.productnr",
            "common.customer.customer.product.name",
            "common.customer.customer.product.price",
            "core.delete"
        ];

        this.translationService.translateMany(keys).then(terms => {
            const options = new TypeAheadOptionsAg();
            options.source = (filter) => this.filterProducts(filter);
            options.displayField = "numberName";
            options.dataField = "number";
            options.minLength = 0;
            options.delay = 0;
            options.useScroll = true;
            options.allowNavigationFromTypeAhead = this.allowNavigationFromTypeAhead.bind(this);

            var colDef = this.gridAg.addColumnTypeAhead("number", terms["common.customer.customer.product.productnr"], null, { typeAheadOptions: options, editable: !this.readOnly });
            colDef.enableCellEdit = true;
            var colDef2 = this.gridAg.addColumnText("name", terms["common.customer.customer.product.name"], null, false, { editable: false });
            colDef2.allowCellFocus = false;
            var colDef3 = this.gridAg.addColumnNumber("price", terms["common.customer.customer.product.price"], null, { enableHiding: false, decimals: 2, editable: !this.readOnly });
            colDef3.enableCellEdit = true;
            this.gridAg.addColumnDelete(terms["core.delete"], this.deleteRow.bind(this));

            this.gridAg.finalizeInitGrid("Common.Customer.Customers.Directives.CustomerProducts", false);

            var events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => {
                this.afterCellEdit(entity, colDef, newValue, oldValue);
            }));
            this.gridAg.options.subscribe(events);

            this.gridAg.setData(this.rows);


        });
    }

    private afterCellEdit(row, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        switch (colDef.field) {
            case 'number':
                var product = _.find(this.products, { number: newValue });
                if (product) {
                    row.name = product.name;
                    row.productId = product.productId;
                    this.gridAg.options.refreshRows(row);
                }
                break;
            case 'price':
                row.price = NumberUtility.parseDecimal(newValue);
                break;
        }

        this.messagingHandler.publishEvent(Constants.EVENT_SET_DIRTY, {});
    }

    protected allowNavigationFromTypeAhead(value, entity: CustomerProductDTO, colDef) {
        if (!value) // If no value, allow it.
            return true;

        var product = _.some(this.products, { number: value });
        if (product) {
            return true;
        }

        return false;
    }

    public filterProducts(filter) {
        return _.orderBy(this.products.filter(p => {
            return p.number.contains(filter) || p.name.contains(filter);
        }), 'number');
    }

    // Lookups

    private loadProducts(): ng.IPromise<any> {
        return this.commonCustomerService.getInvoiceProductsSmall().then(x => {
            this.products = x;
        });
    }

    // Actions
    private addRow() {
        let row: CustomerProductDTO = new CustomerProductDTO();
        row.productId = 0;
        row.price = 0;

        this.rows.push(row);
        this.gridAg.setData(this.rows);

        var colDef = this.gridAg.options.getColumnDefByField('number');
        this.gridAg.options.startEditingCell(<any>row, colDef);

        this.messagingHandler.publishEvent(Constants.EVENT_SET_DIRTY, {});
    }

    protected deleteRow(row: any) {
        this.rows.splice(this.gridAg.options.getRowIndex(row), 1);
        this.gridAg.setData(this.rows);
        this.messagingHandler.publishEvent(Constants.EVENT_SET_DIRTY, {});
    }
}