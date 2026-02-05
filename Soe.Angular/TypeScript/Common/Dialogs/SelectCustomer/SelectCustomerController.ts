import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICommonCustomerService } from "../../Customer/CommonCustomerService";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { ISoeGridOptionsAg, SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { IShortCutService } from "../../../Core/Services/ShortCutService";

export class SelectCustomerController {

    searching = false;
    search: any;
    name: string;
    number: number;
    selectedCustomerId: number;
    timeout = null;
    private customers = [];
    private soeGridOptions: ISoeGridOptionsAg;
    private rowSelected;

    //@ngInject
    constructor(private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        shortCutService: IShortCutService,
        private $scope: ng.IScope,
        private commonCustomerService: ICommonCustomerService) {
        shortCutService.bindEnterCloseDialog(this.$scope, () => { this.buttonOkClick(); }) 

    }

    private $onInit() {
        this.soeGridOptions = new SoeGridOptionsAg("common.dialogs.searchcustomer", this.$timeout);
        this.setupGrid();
        this.$timeout(() => {
            this.soeGridOptions.setFilterFocus();
        }, 500);
    }

    public setupGrid() {
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableRowSelection = false;
        this.soeGridOptions.ignoreResetFilterModel = true;
        this.soeGridOptions.enableSingleSelection();
        this.soeGridOptions.setMinRowsToShow(10);

        // Columns
        const keys: string[] = [
            "common.number",
            "common.name",
            "common.contactaddresses.addressmenu.billing",
            "common.customer.invoices.deliveryaddress",
            "common.note"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.soeGridOptions.addColumnText("customerNr", terms["common.number"], null, { suppressFilter: true });
            this.soeGridOptions.addColumnText("name", terms["common.name"], null, { suppressFilter: true });
            this.soeGridOptions.addColumnText("billingAddress", terms["common.contactaddresses.addressmenu.billing"], null, { suppressFilter: true });
            this.soeGridOptions.addColumnText("deliveryAddress", terms["common.customer.invoices.deliveryaddress"], null, { suppressFilter: true });
            this.soeGridOptions.addColumnText("note", terms["common.note"], null, { suppressFilter: true });
            
            // Events
            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.FilterChanged, () => {
                if (!this.searching)
                    this.loadCustomersFromFilter();
            }))

            events.push(new GridEvent(SoeGridOptionsEvent.RowClicked, (row) => {
                if (!row) return
                this.rowSelected = row;
                this.$scope.$applyAsync();

             events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.edit(row); }));
            }))
            this.soeGridOptions.subscribe(events);

            this.soeGridOptions.finalizeInitGrid();
        });
    }

    public customerOnChanging(item) {
        this.selectedCustomerId = item.id;
    }

    public edit(row) {
        this.$uibModalInstance.close(row);
    }

    public loadCustomersFromFilter = _.debounce(() => {
        this.loadCustomers();
    }, 800, { leading: false, trailing: true });

    private loadCustomers() {

        const filterModels = this.soeGridOptions.getFilterModels();

        if (!filterModels) {
            return;
        }

        this.searching = true;
        this.soeGridOptions.setData([]);
        const customerNr = filterModels["customerNr"] ? filterModels["customerNr"].filter : null
        const name = filterModels["name"] ? filterModels["name"].filter : null;
        const billingAddress = filterModels["billingAddress"] ? filterModels["billingAddress"].filter : null;
        const deliveryAddress = filterModels["deliveryAddress"] ? filterModels["deliveryAddress"].filter : null;
        const note = filterModels["note"] ? filterModels["note"].filter : null;

        if (!customerNr && !name && !billingAddress && !deliveryAddress && !note) {
            this.searching = false;
            return;
        }

        this.commonCustomerService.getCustomersBySearch(customerNr, name, billingAddress, deliveryAddress, note, 0).then((result) => {
            this.searching = false;
            this.customers = result;
            this.soeGridOptions.setData(this.customers);
            this.selectFirstRow();
        });    
    }

    selectFirstRow() {
        if (this.customers.length > 0 && !this.searching) {
            this.soeGridOptions.selectRowByVisibleIndex(0)
            this.rowSelected = this.soeGridOptions.getSelectedRows()[0];
            this.$scope.$applyAsync();
        }
    }

    buttonOkClick() {
        if (this.rowSelected)
            this.$uibModalInstance.close(this.rowSelected);
    }

    buttonCancelClick() {
        this.$uibModalInstance.close(null);
    }
}