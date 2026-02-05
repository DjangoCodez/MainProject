import { IActionResult } from "../../../../../Scripts/TypeLite.Net4";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { Constants } from "../../../../../Util/Constants";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/ProgressHandlerFactory";
import { IProductService } from "../../../../../Shared/Billing/Products/ProductService";
import { SoeGridOptionsAg, ISoeGridOptionsAg } from "../../../../../Util/SoeGridOptionsAg";
import { GridEvent } from "../../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../../../Util/Enumerations";

export class ImportUnitConversionsDialogController {

    // Terms
    langId: number;
    terms: { [index: string]: string; };

    // File
    importFile: any;
    importFileName: string;

    // GUI
    progress: IProgressHandler;

    // Flags
    private hasSelectedRows: boolean = false;

    // Grid
    private soeGridOptions: ISoeGridOptionsAg;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private productService: IProductService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        private $q: ng.IQService,
        private selectedProducts: number[]) {

        this.progress = progressHandlerFactory.create();

        this.soeGridOptions = new SoeGridOptionsAg("common.dialogs.searchprojects", this.$timeout);
        this.setupGrid();
    }

    private setupGrid() {

        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableRowSelection = true;
        this.soeGridOptions.setMinRowsToShow(10);

        var keys: string[] = [
            "billing.productrows.productnr",
            "common.name",
            "billing.product.productunit.unitfrom",
            "billing.product.productunit.unitto",
            "billing.product.productunit.convertfactor",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.soeGridOptions.addColumnText("productNr", terms["billing.productrows.productnr"], null, { suppressFilter: true });
            this.soeGridOptions.addColumnText("productName", terms["common.name"], null, { suppressFilter: true });
            this.soeGridOptions.addColumnText("baseProductUnitName", terms["billing.product.productunit.unitfrom"], null, { suppressFilter: true });
            this.soeGridOptions.addColumnText("productUnitName", terms["billing.product.productunit.unitto"], null, { suppressFilter: true });
            this.soeGridOptions.addColumnNumber("convertFactor", terms["billing.product.productunit.convertfactor"], null, { enableHiding: false, decimals: 2 });

            this.soeGridOptions.finalizeInitGrid();

            var events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: any) => {
                this.$timeout(() => {
                    this.hasSelectedRows = this.soeGridOptions.getSelectedCount() > 0;
                });
            }));
            events.push(new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (rows: any[]) => {
                this.$timeout(() => {
                    this.hasSelectedRows = this.soeGridOptions.getSelectedCount() > 0;
                });
            }));
            this.soeGridOptions.subscribe(events);

            this.$timeout(() => {
                this.soeGridOptions.addTotalRow("#totals-grid", {
                    filtered: this.terms["core.aggrid.totals.filtered"],
                    total: this.terms["core.aggrid.totals.total"]
                });
            }, 10);
        });
    }

    private importValid() {
        return this.importFile && this.hasSelectedRows; // Add selected check
    }

    private addFile() {
        this.translationService.translate("core.fileupload.choosefiletoimport").then((term) => {
            var url = CoreUtility.apiPrefix + Constants.WEBAPI_ECONOMY_ACCOUNTING_PAYMENTFILEIMPORT;
            var modal = this.notificationService.showFileUpload(url, term, true, true, false);
            modal.result.then(res => {
                let result: IActionResult = res.result;
                if (result.success) {
                    this.importFile = result.value;
                    this.importFileName = result.value2;
                    this.parseImportedFile();
                } else {
                    //this.failedWork(result.errorMessage);
                }
            }, error => {
                //this.failedWork(error.message)
            });
        });
    }

    private parseImportedFile() {
        this.progress.startWorkProgress((completion) => {
            this.productService.parseUnitConversionFile(this.selectedProducts, this.importFile).then((importResult) => {
                this.soeGridOptions.setData(importResult);
                completion.completed(importResult, true);
            })
        });
    }

    private import() {
        this.progress.startWorkProgress((completion) => {
            this.productService.saveProductUnitConvert(this.soeGridOptions.getSelectedRows()).then((result: IActionResult) => {
                if (result.success) {
                    completion.completed(result, false);
                    this.$uibModalInstance.close();
                }
                else {
                    completion.failed(result.errorMessage);
                }
            })
        });
    }

    private close() {
        this.$uibModalInstance.close();
    }
}