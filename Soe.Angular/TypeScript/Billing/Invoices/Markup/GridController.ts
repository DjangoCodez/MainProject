import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IInvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { ICommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { TypeAheadOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { Feature, SoeCategoryType, SoeEntityState } from "../../../Util/CommonEnumerations";
import { IconLibrary, SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { MarkupDTO } from "../../../Common/Models/InvoiceDTO";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    private terms: { [index: string]: string; };
    private isCustomerDiscount: boolean;
    private sysWholesellersDict: any[] = [];
    private customerCategories: any[] = [];
    private customers: any[] = [];
    private materialClasses: any[] = [];
    private markupRows: MarkupDTO[] = [];
    private classColumn: any;
    //modal
    private modalInstance: any;

    //@ngInject
    constructor(private invoiceService: IInvoiceService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private commonCustomerService: ICommonCustomerService,
        $uibModal,
        private $q: ng.IQService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
    ) {
        super(gridHandlerFactory, "Billing.Invoices.Markup", progressHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })            
            .onBeforeSetUpGrid(() => this.loadLookups())     
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    onInit(parameters: any) {
        this.parameters = parameters;  
        this.isCustomerDiscount = soeConfig.isCustomerDiscount || false;
        this.flowHandler.start({ feature: Feature.Billing_Preferences_InvoiceSettings_Markup, loadReadPermissions: true, loadModifyPermissions: true });  

        this.gridAg.options.customTabToCellHandler = (params) => this.handleNavigateToNextCell(params);
    }

    private loadLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadCustomerCategories()]).then(() => {
                this.loadWholesellers();
                this.loadMaterialClasses();
                this.loadCustomers();
            });
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.date",
            "billing.invoices.markup.newcustomerdiscount",
            "billing.invoices.markup.newmarkup",
            "billing.invoices.markup.wholeseller",
            "billing.invoices.markup.materialclass",
            "billing.invoices.markup.customercategory",
            "billing.invoices.markup.supplieragreemmentpercent",
            "billing.invoices.markup.productgroup",
            "billing.invoices.markup.markuppercent",
            "billing.invoices.markup.customerdiscount",
            "billing.invoices.markup.discountpercent",
            "billing.invoices.markup.customer",
            "common.all",
            "common.personal",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadWholesellers(): ng.IPromise<any> {
        return this.invoiceService.getSysWholesellersDict(true).then((x) => {
            this.sysWholesellersDict = x;
            this.sysWholesellersDict.push({ id: 65, name: "Comfort" });
            if (this.isCustomerDiscount) {
                this.sysWholesellersDict.splice(0, 0, { id: -2, name: this.terms["common.personal"] });
                this.sysWholesellersDict.splice(0, 0, { id: -1, name: this.terms["common.all"] });
            }
        })
    }

    private loadCustomerCategories(): ng.IPromise<any> {
        return this.coreService.getCategoriesDict(SoeCategoryType.Customer, true).then((x) => {
            this.customerCategories = x;
        });
    }

    private loadCustomers(): ng.IPromise<any> {
        return this.commonCustomerService.getCustomersDict(true, true, true).then((x) => {
            this.customers = x;
        });
    }

    private loadMaterialClasses(): ng.IPromise<any> {
        return this.invoiceService.getSysPricelistCodeBySysWholesellerId(_.map(this.sysWholesellersDict, 'id')).then((x) => {
            this.materialClasses = x;
        })
    }  

    private loadDiscount(row: MarkupDTO) {
        var sysWholesellerId = row.sysWholesellerId;
        if (sysWholesellerId === 14 || sysWholesellerId === 15)
            sysWholesellerId = 2;

        return this.invoiceService.getDiscount(sysWholesellerId, row.code).then((x) => {
            row.wholesellerDiscountPercent = x;
        })
    }

    edit(row) {
        
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        var newLabel = this.isCustomerDiscount ? "billing.invoices.markup.newcustomerdiscount" : "billing.invoices.markup.newmarkup";
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.reloadData(), true, () => this.save());
        var group = ToolBarUtility.createGroup(new ToolBarButton(newLabel, newLabel, IconLibrary.FontAwesome, "fa-plus", () => {
            this.addRow();
        }, null, () => {
            return !this.modifyPermission;
        }));

        this.toolbar.addButtonGroup(group);
    }

    public setupGrid() {

        this.gridAg.options.enableRowSelection = false;

        this.gridAg.addColumnIsModified("isModified", "", 20);        

        const wholesellerOptions = new TypeAheadOptionsAg();
        wholesellerOptions.source = (filter) => this.filterWholesellers(filter);
        wholesellerOptions.minLength = 0;
        wholesellerOptions.delay = 0;
        wholesellerOptions.displayField = "name"
        wholesellerOptions.dataField = "name";
        wholesellerOptions.allowNavigationFromTypeAhead = this.allowNavigationFromTypeAhead.bind(this);        

        this.gridAg.addColumnTypeAhead("wholesellerName", this.terms["billing.invoices.markup.wholeseller"], null, { typeAheadOptions: wholesellerOptions, editable: true });

        const classOptions = new TypeAheadOptionsAg();
        //categoriesOptions.source = (filter) => this.filterCategories(filter);*/
        classOptions.displayField = "name"
        classOptions.dataField = "name";
        classOptions.minLength = 0;
        classOptions.delay = 0;
        classOptions.allowNavigationFromTypeAhead = this.allowNavigationFromTypeAhead.bind(this);

        this.classColumn = this.gridAg.addColumnTypeAhead("code", this.terms["billing.invoices.markup.materialclass"], null, { typeAheadOptions: classOptions, editable: true });

        const categoriesOptions = new TypeAheadOptionsAg();
        categoriesOptions.source = (filter) => this.filterCategories(filter);
        categoriesOptions.displayField = "name"
        categoriesOptions.dataField = "name";
        categoriesOptions.minLength = 0;
        categoriesOptions.delay = 0;
        categoriesOptions.allowNavigationFromTypeAhead = this.allowNavigationFromTypeAhead.bind(this);

        var categoriesColumn = this.gridAg.addColumnTypeAhead("categoryName", this.terms["billing.invoices.markup.customercategory"], null, { error: 'productError', typeAheadOptions: categoriesOptions, editable: true });
        categoriesColumn['navigationCollection'] = this.customerCategories;

        var customerOptions = new TypeAheadOptionsAg();
        customerOptions.source = (filter) => this.filterCustomers(filter);
        customerOptions.minLength = 0;
        customerOptions.delay = 0;
        customerOptions.displayField = "name"
        customerOptions.dataField = "name";
        customerOptions.allowNavigationFromTypeAhead = this.allowNavigationFromTypeAhead.bind(this);

        this.gridAg.addColumnTypeAhead("customerName", this.terms["billing.invoices.markup.customer"], null, { typeAheadOptions: customerOptions, editable: true });

        this.gridAg.addColumnNumber("wholesellerDiscountPercent", this.terms["billing.invoices.markup.supplieragreemmentpercent"], null, { enableHiding: true, decimals: 2 });
        this.gridAg.addColumnText("productIdFilter", this.terms["billing.invoices.markup.productgroup"], null, false, { editable: true });
        if (this.isCustomerDiscount)
            this.gridAg.addColumnNumber("discountPercent", this.terms["billing.invoices.markup.discountpercent"], null, { editable: true, decimals: 2 });
        else
            this.gridAg.addColumnNumber("markupPercent", this.terms["billing.invoices.markup.markuppercent"], null, { editable: true, decimals: 2 });
        this.gridAg.addColumnDate("created", this.terms["common.date"], null);
        //this.gridAg.addColumnIcon(null, "", null, { icon: "iconDelete fal fa-times", onClick: this.deleteRow.bind(this) });
        this.gridAg.addColumnDelete(this.terms["core.deleterow"], (data) => this.deleteRow(data), null, null);

        var events: GridEvent[] = [];
        events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
        this.gridAg.options.subscribe(events);

        this.gridAg.options.getColumnDefs()
            .forEach(f => {
                if (f.field === "code") {
                    f.editable = (node, column, colDef, context, api, columnApi) => {
                        return !(node.data.sysWholesellerId === -1);
                    }
                }
            });

        this.gridAg.finalizeInitGrid(this.isCustomerDiscount ? "billing.invoices.markup.customerdiscount" : "billing.invoices.markup.markup", true);
    }
    
    protected allowNavigationFromTypeAhead(value, entity, colDef) {
        if (!value)  // If no value, allow it.
            return true;

        if (!colDef['navigationCollection'])
            return true;


        var matched = _.some(colDef['navigationCollection']  , (p) => p.name === value);
        if (matched) {
            return true;
        }
        else {
            return false;
        }
    }

    private filterWholesellers(filter) {
        return this.sysWholesellersDict.filter(wholeseller => {
            return wholeseller.name.contains(filter);
        });
    }    

    private filterCustomers(filter) {
        return this.customers.filter(customer => {
            return customer.name.contains(filter);
        });
    }    

    private filterCategories(filter) {
        return this.customerCategories.filter(category => {
            return category.name.contains(filter);
        });
    }

    private afterCellEdit(row: MarkupDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue !== oldValue && newValue) {
            switch (colDef.field) {
                case 'wholesellerName':
                    var wholeseller = _.find(this.sysWholesellersDict, { name: newValue });
                    if (wholeseller) {
                        row.sysWholesellerId = wholeseller.id;

                        //Load discount
                        this.loadDiscount(row);

                        //Set data in classcolumn
                        var options: TypeAheadOptionsAg = this.classColumn['cellEditorParams']['typeAheadOptions'];
                        var classes = this.materialClasses[row.sysWholesellerId];
                        if (classes) {
                            var collection = [];
                            _.forEach(classes, x => {
                                collection.push({ id: x, name: x });
                            })
                            options.source = (filter) => {
                                return collection.filter(materialClass => {
                                    return materialClass.name.contains(filter);
                                });
                            };
                            this.classColumn['navigationCollection'] = collection;
                        }
                        else {
                            options.source = (filter) => [];
                            this.classColumn['navigationCollection'] = undefined;
                        }

                        row['isModified'] = true;
                        this.gridAg.options.refreshRows(row);
                    }
                    else {
                        row.wholesellerName = oldValue;
                        this.gridAg.options.refreshRows(row);
                    }
                    break;
                case 'code':
                    //Load discount
                    this.loadDiscount(row);

                    row['isModified'] = true;
                    this.gridAg.options.refreshRows(row);
                    break;
                case 'categoryName':
                    var category = _.find(this.customerCategories, { name: newValue });
                    if (category) {
                        row.categoryId = category.id;

                        //clear customer: discount is either for category or for customer
                        row.actorCustomerId = 0;
                        row.customerName = "";

                        row['isModified'] = true;
                        this.gridAg.options.refreshRows(row);
                    }
                    else {
                        row.categoryName = oldValue;
                        this.gridAg.options.refreshRows(row);
                    }
                    break;
                case 'customerName':
                    var customer = _.find(this.customers, { name: newValue });
                    if (customer) {
                        row.actorCustomerId = customer.id;

                        //clear customercategory: discount is either for category or for customer
                        row.categoryId = 0;
                        row.categoryName = "";

                        row['isModified'] = true;
                        this.gridAg.options.refreshRows(row);
                    }
                    else {
                        row.customerName = oldValue;
                        this.gridAg.options.refreshRows(row);
                    }
                    break;
                case 'categoryIdFilter':
                case 'discountPercent':
                case 'markupPercent':
                    row['isModified'] = true;
                    this.gridAg.options.refreshRows(row);
                    break;
            }
        }
    }

    protected handleNavigateToNextCell(params: any): { rowIndex: number, column: any } {
        const { nextCellPosition, previousCellPosition, backwards } = params;
        let nextColumnCaller: (column: any) => any = backwards ? this.gridAg.options.getPreviousVisibleColumn : this.gridAg.options.getNextVisibleColumn;
        let { rowIndex, column } = nextCellPosition;
        let row: MarkupDTO = this.gridAg.options.getVisibleRowByIndex(rowIndex).data;

        if (column.colId === 'created') {
            const nextRowResult = this.gridAg.findNextRowInfo(row);

            if (nextRowResult) {
                    return {
                        rowIndex: nextRowResult.rowIndex,
                        column: this.gridAg.options.getColumnByField('wholesellerName')
                    };
            } else {
                this.gridAg.options.stopEditing(false);
                this.addRow();
                return null;
            }
        }
        else {
            return { rowIndex, column };
        }
    } 

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.invoiceService.getMarkup(this.isCustomerDiscount).then(x => {
                this.markupRows = x
                this.markupRows.forEach((row) => {
                    if (row.code) {
                        var options: TypeAheadOptionsAg = this.classColumn['cellEditorParams']['typeAheadOptions'];
                        var classes = this.materialClasses[row.sysWholesellerId];
                        if (classes) {
                            var collection = [];
                            _.forEach(classes, x => {
                                collection.push({ id: x, name: x });
                            })
                            options.source = (filter) => {
                                return collection.filter(materialClass => {
                                    return materialClass.name.contains(filter);
                                });
                            };
                            this.classColumn['navigationCollection'] = collection;
                        }
                        else {
                            options.source = (filter) => [];
                            this.classColumn['navigationCollection'] = undefined;
                        }
                    }
                    if (row.categoryId && row.categoryId > 0) {
                        var category = this.customerCategories.find(c => c.id === row.categoryId);
                        row.categoryName = category?.name;
                    }
                    if (row.sysWholesellerId && row.sysWholesellerId < 0) {
                        var wholeSeller = this.sysWholesellersDict.find(i => i.id === row.sysWholesellerId);
                        row.wholesellerName = wholeSeller?.name;
                    }
                    if (row.actorCustomerId && row.actorCustomerId > 0) {
                        var customer = this.customers.find(i => i.id === row.actorCustomerId);
                        row.customerName = customer?.name;
                    }
                });
                return this.markupRows;
            }).then(data => {
                this.setData(this.filterRows());
            });
        }]);
    }

    private filterRows() {
        return _.filter(this.markupRows, { state: SoeEntityState.Active });
    }

    private addRow() {
        var row = new MarkupDTO();
        row.code = "";
        row.state = SoeEntityState.Active;
        this.markupRows.push(row);
        this.gridAg.setData(this.filterRows());

        //Set focus and edit
        var column = this.gridAg.options.getColumnByField('wholesellerName');
        this.gridAg.options.startEditingCell(row, column);
    }

    private deleteRow(row: MarkupDTO) {
        if (row.markupId && row.markupId > 0) {
            row.state = SoeEntityState.Deleted;
            this.gridAg.setData(this.filterRows());
        }
        else {
            var index = this.markupRows.indexOf(row);
            this.markupRows.splice(index, 1);
            this.gridAg.setData(this.filterRows());
        }
    }

    private save() {
        var rowsToSave = [];
        for (let row of this.markupRows) {
            if (row["isModified"] || row.state === SoeEntityState.Deleted)
                rowsToSave.push(row);
        }

        this.gridAg.options.stopEditing(false);

        this.progress.startSaveProgress((completion) => {
            this.invoiceService.saveMarkup(rowsToSave).then((result) => {
                if (result.success)
                    completion.completed();
                else {
                    completion.failed(result.errorMessage);
                }
            });

        }, null)
            .then(data => {
                this.reloadData();
            }, error => {
            });
    }

    private reloadData() {        
        this.loadGridData();
    }
}