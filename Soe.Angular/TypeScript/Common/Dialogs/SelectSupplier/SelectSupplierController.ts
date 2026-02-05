import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ISelectSupplierService } from "./SelectSupplierService";
import { IShortCutService } from "../../../Core/Services/ShortCutService";
import { SoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { SoeGridOptionsEvent } from "../../../Util/Enumerations";
import { GridEvent } from "../../../Util/SoeGridOptions";

export class SelectSupplierController {

    private search;
    private suppliers: any;
    private soeGridOptions: SoeGridOptionsAg;
    private searching: boolean;
    private rowSelected;

    //@ngInject
    constructor(private $uibModalInstance,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        shortCutService: IShortCutService,
        private $scope: ng.IScope,
        private selectSupplierService: ISelectSupplierService) {
        shortCutService.bindEnterCloseDialog(this.$scope, () => { this.buttonOkClick(); })

    }

    private $onInit() {
        this.soeGridOptions = new SoeGridOptionsAg("common.dialogs.searchsupplier", this.$timeout);
        this.setupGrid();
        this.$timeout(() => {
            this.soeGridOptions.setFilterFocus();
        }, 500);
    }


    public setupGrid() {
        // Columns
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.ignoreResetFilterModel = true;
        this.soeGridOptions.enableSingleSelection();
        this.soeGridOptions.enableRowSelection = false;
        this.soeGridOptions.setMinRowsToShow(10);

        const keys: string[] = [
            "common.number",
            "common.name"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            //super.addColumnText("supplierNr", terms["common.number"], "25%");
            this.soeGridOptions.addColumnText("supplierNr", terms["common.number"], null)
            //super.addColumnText("name", terms["common.name"], null
            this.soeGridOptions.addColumnText("name", terms["common.name"], null)


            // Events
            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.FilterChanged, () => {
                if (!this.searching)
                    this.loadSuppliersFromFilter();
            }))

            events.push(new GridEvent(SoeGridOptionsEvent.RowClicked, (row) => {
                if (!row) return
                this.rowSelected = row;
                this.$scope.$applyAsync();

                events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.edit(row); }));
            }))


            this.soeGridOptions.subscribe(events);

            this.soeGridOptions.finalizeInitGrid();
        })
    }

    public loadSuppliersFromFilter = _.debounce(() => {
        this.loadSuppliers();
    }, 800, { leading: false, trailing: true });


    private loadSuppliers() {
        const filterModels = this.soeGridOptions.getFilterModels();

        if (!filterModels) {
            return;
        }
        this.searching = true;
        const supplierNr = filterModels["supplierNr"] ? filterModels["supplierNr"].filter : "";
        const name = filterModels["name"] ? filterModels["name"].filter : "";
        this.search = {SupplierNUmber: supplierNr, SupplierName: name}

        if (!this.search && !this.search.supplierNr && !this.search.name) {
            this.searching = false;
            return;
        }

        this.selectSupplierService.getSuppliersBySearch(this.search).then((x) => {
            this.searching = false;
            this.suppliers = x;
            this.soeGridOptions.setData(x);
            this.selectFirstRow();
        });
    }

    private selectFirstRow() {
        if (this.suppliers.length > 0 && !this.searching) {
            this.soeGridOptions.selectRowByVisibleIndex(0)
            this.rowSelected = this.soeGridOptions.getSelectedRows()[0];
            this.$scope.$applyAsync();
        }
    }

    buttonOkClick() {
        if (this.rowSelected)
            this.$uibModalInstance.close(this.rowSelected.actorSupplierId);
    }

    buttonCancelClick() {
        this.$uibModalInstance.close(null);
    }

    public edit(row) {
        this.$uibModalInstance.close(row.actorSupplierId);
    }
}