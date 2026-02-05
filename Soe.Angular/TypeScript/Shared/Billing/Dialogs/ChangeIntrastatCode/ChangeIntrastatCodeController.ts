import { ISoeGridOptionsAg, SoeGridOptionsAg, TypeAheadOptionsAg } from "../../../../Util/SoeGridOptionsAg";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IProductUnitDTO, ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { SoeEntityState, SoeOriginStatusClassification, SoeOriginType, TermGroup } from "../../../../Util/CommonEnumerations";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { IProductService } from "../../Products/ProductService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IntrastatTransactionDTO } from "../../../../Common/Models/CommodityCodesDTO";
import { SoeGridOptionsEvent } from "../../../../Util/Enumerations";

export class ChangeIntrastatCodeController {

    // Terms
    private terms: any;

    // Values
    private classification: SoeOriginStatusClassification;
    private selectedCustomerInvoice: number;
    private usedAmount: number;

    // Collections
    private intrastatCodes: any[];
    private codeDict: any[] = [];
    private productUnits: any[] = [];
    private transactionDict: ISmallGenericType[] = [];
    private countryDict: ISmallGenericType[] = [];
    private existingTransactions: IntrastatTransactionDTO[]

    // Grid
    private soeGridOptions: ISoeGridOptionsAg;

    // Properties
    private _selectedIntrastatCode: number = undefined;
    public get selectedIntrastatCode(): number {
        return this._selectedIntrastatCode;
    }
    public set selectedIntrastatCode(value: number) {
        this._selectedIntrastatCode = value;
        const code = _.find(this.codeDict, c => c.id === value);
        if (code) {
            const selectedRows = this.soeGridOptions.getSelectedRows();
            _.forEach(selectedRows, (r: IntrastatTransactionDTO) => {
                r.intrastatCodeId = value;
                r.instrastatCodeName = code.name;
                r.isModified = true;
            });
            this.soeGridOptions.refreshRows(...selectedRows);
        }
    }

    private _selectedTransactionType: number = undefined;
    public get selectedTransactionType(): number {
        return this._selectedTransactionType;
    }
    public set selectedTransactionType(value: number) {
        this._selectedTransactionType = value;
        const type = _.find(this.transactionDict, c => c.id === value);
        if (type) {
            const selectedRows = this.soeGridOptions.getSelectedRows();
            _.forEach(selectedRows, (r) => {
                r.intrastatTransactionType = value;
                r.transactionTypeName = type.name;
                r.isModified = true;
            });
            this.soeGridOptions.refreshRows(...selectedRows);
        }
    }

    private _selectedCountry: number = undefined;
    public get selectedCountry(): number {
        return this._selectedCountry;
    }
    public set selectedCountry(value: number) {
        this._selectedCountry = value;
        const country = _.find(this.countryDict, c => c.id === value);
        if (country) {
            const selectedRows = this.soeGridOptions.getSelectedRows();
            _.forEach(selectedRows, (r) => {
                r.sysCountryId = value;
                r.sysCountryName = country.name;
                r.isModified = true;
            });
            this.soeGridOptions.refreshRows(...selectedRows);
        }
    }

    public get restAmount(): number {
        return this.totalAmount - this.usedAmount;
    }

    public get isOrder(): boolean {
        return this.originType === SoeOriginType.Order;
    }

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private productService: IProductService,
        private $q: ng.IQService,
        private transactions: IntrastatTransactionDTO[],
        private originType: SoeOriginType,
        private originId: number,
        private totalAmount?: number) {
    }

    private $onInit() {
        this.soeGridOptions = new SoeGridOptionsAg("Billing.Dialogs.ChangeIntrastatCode", this.$timeout);
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.showAlignedFooterGrid = true;
        this.soeGridOptions.setMinRowsToShow(8);

        this.$q.all([
            this.loadIntrastatCodes(),
            this.loadTransactionTypes(),
            this.loadCountries(),
            this.loadProductUnits(),
            this.loadIntrastatTransactions()]).then(() => {
                this.generateRows();
                this.setupGrid();
            });
    }

    private loadIntrastatCodes(): ng.IPromise<any> {
        return this.productService.getCustomerCommodityCodes(true, true).then((x) => {
            this.intrastatCodes = x;
            this.codeDict = _.filter(x, c => c.isActive).map(r => { return { id: r.intrastatCodeId, name: `${r.code} ${r.text}` } });;
        })
    }

    private loadTransactionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.IntrastatTransactionType, false, false).then(x => {
            this.transactionDict = [];
            this.transactionDict.push({ id: 0, name: " " });
            _.forEach(_.orderBy(x, 'id'), (y) => {
                this.transactionDict.push({ id: y.id, name: y.id + " " + y.name });
            });
        });
    }

    private loadCountries(): ng.IPromise<any> {
        return this.coreService.getSysCountries(true, false).then(x => {
            this.countryDict = x;
        });
    }

    private loadProductUnits(): ng.IPromise<any> {
        return this.productService.getProductUnits().then((x: IProductUnitDTO[]) => {
            _.forEach(x, (y) => {
                this.productUnits.push({ id: y.productUnitId, name: y.code });
            });
        });
    }

    private loadIntrastatTransactions(): ng.IPromise<any> {
        return this.coreService.getIntrastatTransactions(this.originId).then(x => {
            console.log("transactions loaded", x);
            this.existingTransactions = x;
        });
    }

    private setupGrid() {
        var keys: string[] = [
            "common.customer.invoices.row",
            "common.customer.invoices.productnr",
            "common.customer.invoices.productname",
            "common.customer.invoices.quantity",
            "common.customer.invoices.unit",
            "common.commoditycodes.code",
            "economy.accounting.liquidityplanning.transactiontype",
            "common.commoditycodes.netweight",
            "common.commoditycodes.otherquantity",
            "common.countryoforigin",
            "common.commoditycodes.notintrastat",
            "billing.productrows.productunit",
            "billing.purchaserows.sumamount",
            "core.deleterow"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            if (this.originType === SoeOriginType.SupplierInvoice) {
                this.soeGridOptions.addColumnNumber("amount", terms["billing.purchaserows.sumamount"], null, { enableHiding: false, editable: (row) => this.isRowEditable(row), decimals: 2 });

                /*var unitOptions = new TypeAheadOptionsAg();
                unitOptions.source = (filter) => this.filter(filter, this.productUnits);
                unitOptions.displayField = "name"
                unitOptions.dataField = "name";
                unitOptions.minLength = 0;
                unitOptions.delay = 0;
                unitOptions.useScroll = true;
                unitOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef, this.productUnits);
                this.soeGridOptions.addColumnTypeAhead("productUnitCode", terms["billing.productrows.productunit"], null, { typeAheadOptions: unitOptions, editable: true, suppressSorting: true });*/

                this.soeGridOptions.addColumnNumber("quantity", terms["common.customer.invoices.quantity"], null, { enableHiding: false });
            }
            else {
                this.soeGridOptions.addColumnNumber("rowNr", terms["common.customer.invoices.row"], 50, { enableHiding: false, pinned: "left" });
                this.soeGridOptions.addColumnText("productNr", terms["common.customer.invoices.productnr"], null, { enableHiding: false });
                this.soeGridOptions.addColumnText("productName", terms["common.customer.invoices.productname"], null, { enableHiding: false });
                this.soeGridOptions.addColumnNumber("quantity", terms["common.customer.invoices.quantity"], null, { enableHiding: false });
                this.soeGridOptions.addColumnText("productUnitCode", terms["common.customer.invoices.unit"], null, { enableHiding: false });
            }

            var codeOptions = new TypeAheadOptionsAg();
            codeOptions.source = (filter) => this.filter(filter, this.codeDict);
            codeOptions.displayField = "name"
            codeOptions.dataField = "name";
            codeOptions.minLength = 0;
            codeOptions.delay = 0;
            codeOptions.useScroll = true;
            codeOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef, this.codeDict);
            this.soeGridOptions.addColumnTypeAhead("instrastatCodeName", terms["common.commoditycodes.code"], null, { typeAheadOptions: codeOptions, editable: (row) => this.isRowEditable(row), suppressSorting: true });

            var transactionOptions = new TypeAheadOptionsAg();
            transactionOptions.source = (filter) => this.filter(filter, this.transactionDict);
            transactionOptions.displayField = "name"
            transactionOptions.dataField = "name";
            transactionOptions.minLength = 0;
            transactionOptions.delay = 0;
            transactionOptions.useScroll = true;
            transactionOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef, this.transactionDict);
            this.soeGridOptions.addColumnTypeAhead("transactionTypeName", terms["economy.accounting.liquidityplanning.transactiontype"], null, { typeAheadOptions: transactionOptions, editable: (row) => this.isRowEditable(row), suppressSorting: true });

            this.soeGridOptions.addColumnNumber("netWeight", terms["common.commoditycodes.netweight"], 50, { enableHiding: false, decimals: 3, editable: (row) => this.isRowEditable(row), maxDecimals: 3 });
            this.soeGridOptions.addColumnText("otherQuantity", terms["common.commoditycodes.otherquantity"], null, { enableHiding: false, editable: (row) => this.isRowEditable(row) });

            if (this.originType === SoeOriginType.Order) {
                var countryOptions = new TypeAheadOptionsAg();
                countryOptions.source = (filter) => this.filter(filter, this.countryDict);
                countryOptions.displayField = "name"
                countryOptions.dataField = "name";
                countryOptions.minLength = 0;
                countryOptions.delay = 0;
                countryOptions.useScroll = true;
                countryOptions.allowNavigationFromTypeAhead = (value, entity, colDef) => this.allowNavigationFromTypeAhead(value, colDef, this.countryDict);
                this.soeGridOptions.addColumnTypeAhead("sysCountryName", terms["common.countryoforigin"], null, { typeAheadOptions: countryOptions, editable: (row) => this.isRowEditable(row), suppressSorting: true });
            }

            this.soeGridOptions.addColumnBool("notIntrastat", terms["common.commoditycodes.notintrastat"], 100, { enableEdit: true, onChanged: this.rowSelected.bind(this) });
            this.soeGridOptions.addColumnDelete(terms["core.deleterow"], (data) => this.deleteRow(data), null, null);

            this.soeGridOptions.subscribe([new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); })]);

            this.soeGridOptions.getColumnDefs().forEach(col => {
                var cellcls: string = col.cellClass ? col.cellClass.toString() : "";
                col.cellClass = (grid: any) => {
                    if (grid.data['notIntrastat'])
                        return cellcls + " closedRow";
                    else
                        return cellcls;
                }
            });

            this.soeGridOptions.finalizeInitGrid();

            this.setData();
        });
    }

    private isRowEditable(row: IntrastatTransactionDTO) {
        return !row.notIntrastat;
    }

    protected setData() {
        this.soeGridOptions.setData(this.filterRows());

        this.$timeout(() => _.forEach(this.filterRows(), (r) => {
            this.soeGridOptions.selectRow(r, true);
        }), null);
    }

    protected filter(filter, collection) {
        return _.orderBy(collection.filter(p => {
            return p.name.contains(filter);
        }), 'name');
    }

    protected allowNavigationFromTypeAhead(value, colDef, collection) {
        if (!value)
            return true;

        var matched = _.some(collection, { 'name': value });
        if (matched)
            return true;

        return false;
    }

    private rowSelected(transaction) {
        this.usedAmount = _.sum(_.map(_.filter(this.transactions, r => r.state != SoeEntityState.Deleted), t => t.amount));
        this.$timeout(() => {
            transaction.isModified = true;
        });
        this.soeGridOptions.refreshRows(transaction.data);
    }

    private afterCellEdit(transaction: IntrastatTransactionDTO, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        switch (colDef.field) {
            case 'instrastatCodeName':
                const code = _.find(this.codeDict, c => c.name === newValue);
                if (code) {
                    transaction.intrastatCodeId = code.id;
                    transaction.instrastatCodeName = code.name;
                }
                else {
                    transaction.intrastatCodeId = undefined;
                    transaction.instrastatCodeName = undefined;
                }
                this.$timeout(() => {
                    transaction.isModified = true;
                });
                break;
            case 'transactionTypeName':
                const type = _.find(this.transactionDict, c => c.name === newValue);
                if (type) {
                    transaction.intrastatTransactionType = type.id;
                    transaction.transactionTypeName = type.name;
                }
                else {
                    transaction.intrastatTransactionType = undefined;
                    transaction.transactionTypeName = undefined;
                }
                this.$timeout(() => {
                    transaction.isModified = true;
                });
                break;
            case 'sysCountryName':
                const country = _.find(this.countryDict, c => c.name === newValue);
                if (country) {
                    transaction.sysCountryId = country.id;
                    transaction.sysCountryName = country.name;
                }
                else {
                    transaction.sysCountryId = undefined;
                    transaction.sysCountryName = undefined;
                }
                this.$timeout(() => {
                    transaction.isModified = true;
                });
                break;
            case "productUnitCode":
                const productUnit = _.find(this.productUnits, c => c.name === newValue);
                if (productUnit) {
                    transaction.productUnitId = productUnit.id;
                    transaction.productUnitCode = productUnit.name;
                }
                else {
                    transaction.productUnitId = undefined;
                    transaction.productUnitCode = undefined;
                }
                this.$timeout(() => {
                    transaction.isModified = true;
                });
                break;
            case "amount":
                this.$timeout(() => {
                    this.usedAmount = _.sum(_.map(_.filter(this.transactions, r => r.state != SoeEntityState.Deleted), t => t.amount));
                    transaction.isModified = true;
                });
                break;
            default:
                console.log("default edited")
                this.usedAmount = _.sum(_.map(_.filter(this.transactions, r => r.state != SoeEntityState.Deleted), t => t.amount));
                this.$timeout(() => {
                    transaction.isModified = true;
                });
                break;
        }

        this.soeGridOptions.refreshRows(transaction);
    }

    protected addRow() {
        if (this.originType === SoeOriginType.SupplierInvoice && this.usedAmount >= this.totalAmount)
            return;

        const transaction = new IntrastatTransactionDTO();
        transaction.intrastatCodeId = this.selectedIntrastatCode;
        transaction.sysCountryId = this.selectedCountry;
        transaction.intrastatTransactionType = this.selectedTransactionType;
        transaction.originId = this.originId;
        transaction.amount = this.restAmount;
        transaction.quantity = 1;
        transaction.isModified = true;
        transaction.state = SoeEntityState.Active;
        this.setRowValues(transaction);
        this.transactions.push(transaction);

        this.setData();

        this.$timeout(() => {
            this.usedAmount = _.sum(_.map(_.filter(this.transactions, r => r.state != SoeEntityState.Deleted), t => t.amount));
        });
    }

    private deleteRow(row: IntrastatTransactionDTO) {
        if (row.intrastatTransactionId && row.intrastatTransactionId > 0) {
            row.state = SoeEntityState.Deleted;
            row.isModified = true;
        }
        else {
            var index = this.transactions.indexOf(row);
            this.transactions.splice(index, 1);
        }
        this.setData();

        this.$timeout(() => {
            this.usedAmount = _.sum(_.map(_.filter(this.transactions, r => r.state != SoeEntityState.Deleted), t => t.amount));
        });
    }

    private filterRows() {
        return _.filter(this.transactions, { state: SoeEntityState.Active });
    }

    protected generateRows() {
        if (this.originType == SoeOriginType.SupplierInvoice && this.existingTransactions && this.existingTransactions.length > 0) {
            this.transactions = this.existingTransactions;

            this.usedAmount = 0;
            _.forEach(_.filter(this.transactions, r => r.state != SoeEntityState.Deleted), (transaction) => {
                this.usedAmount += transaction.amount;
                this.setRowValues(transaction);
            });

            if (this.totalAmount !== this.usedAmount) {
                const transaction = new IntrastatTransactionDTO();
                transaction.originId = this.originId;
                transaction.amount = this.totalAmount - this.usedAmount;
                transaction.quantity = 1;
                transaction.state = SoeEntityState.Active;
                this.transactions.push(transaction);
            }
        }
        else {
            _.forEach(this.transactions, (transaction) => {
                if (transaction.intrastatTransactionId && transaction.intrastatTransactionId > 0) {
                    var existing = _.find(this.existingTransactions, t => t.intrastatTransactionId === transaction.intrastatTransactionId);
                    if (existing) {
                        transaction.intrastatCodeId = existing.intrastatCodeId;
                        transaction.intrastatTransactionType = existing.intrastatTransactionType;
                        transaction.netWeight = existing.netWeight;
                        transaction.notIntrastat = existing.notIntrastat;
                        transaction.originId = existing.originId;
                        transaction.otherQuantity = existing.otherQuantity;
                        transaction.sysCountryId = existing.sysCountryId;
                        transaction.state = existing.state;
                    }
                }
                this.setRowValues(transaction);
            });
        }

        this.usedAmount = _.sum(_.map(_.filter(this.transactions, r => r.state != SoeEntityState.Deleted), t => t.amount));
    }

    protected setRowValues(transaction: IntrastatTransactionDTO) {
        if (transaction.intrastatCodeId && transaction.intrastatCodeId > 0) {
            const code = _.find(this.intrastatCodes, c => c.intrastatCodeId === transaction.intrastatCodeId);
            if (code)
                transaction.instrastatCodeName = `${code.code} ${code.text}`;
        }
        if (transaction.intrastatTransactionType && transaction.intrastatTransactionType > 0) {
            const type = _.find(this.transactionDict, c => c.id === transaction.intrastatTransactionType);
            if (type)
                transaction.transactionTypeName = type.name;
        }
        if (transaction.sysCountryId && transaction.sysCountryId > 0) {
            const country = _.find(this.countryDict, c => c.id === transaction.sysCountryId);
            if (country)
                transaction.sysCountryName = country.name;
        }
    }

    buttonCancelClick() {
        this.close(null);
    }

    buttonOkClick() {
        return this.coreService.saveIntrastatTransactions(_.filter(this.transactions, t => t.isModified), this.originId, this.originType).then(result => {
            this.close(result.success);
        });
    }

    close(result: any) {
        if (!result) {
            this.$uibModalInstance.dismiss('cancel');
        } else {
            this.$uibModalInstance.close(result);
        }
    }

    buttonEnabled() {
        if (_.some(_.filter(this.transactions, r => r.isModified && !r.notIntrastat), t => (!t.intrastatCodeId || !t.intrastatTransactionType))) {
            return false;
        }
        else {
            return this.originType === SoeOriginType.SupplierInvoice ? _.some(_.filter(this.transactions, r => r.state != SoeEntityState.Deleted), t => t.isModified) && this.restAmount === 0 : _.some(_.filter(this.transactions, r => r.state != SoeEntityState.Deleted), t => t.isModified);
        }
    }

    buttonNewRowDisabled() {
        return this.usedAmount >= this.totalAmount;
    }

    isSupplierInvoice() {
        return this.originType === SoeOriginType.SupplierInvoice;
    }
}