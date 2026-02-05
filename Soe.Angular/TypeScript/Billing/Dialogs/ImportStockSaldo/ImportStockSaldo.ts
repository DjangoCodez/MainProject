import { ISmallGenericType, IActionResult } from "../../../Scripts/TypeLite.Net4";
import { ICommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Constants } from "../../../Util/Constants";
import { IProgressHandler } from "../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IStockService } from "../../../Shared/Billing/Stock/StockService";

export class ImportStockSaldoDialogController {

    langId: number;
    terms: { [index: string]: string; };

    createVoucher: boolean = true;
    wholesellers: ISmallGenericType[] = [];
    selectedWholeseller: ISmallGenericType;

    stocksDict: ISmallGenericType[] = [];
    selectedStock: any;

    importFile: any;
    importFileName: string;

    fromInventory: boolean = false;
    progress: IProgressHandler;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private stockService: IStockService,
        private commonCustomerService: ICommonCustomerService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        private stockInventoryHeadId: number,
        private $q: ng.IQService) {

        this.progress = progressHandlerFactory.create()
        if (stockInventoryHeadId) {
            this.fromInventory = true;
            this.$q.all([
                this.loadTerms()
            ]);
        }
        else {
            this.fromInventory = false;
            this.$q.all([
                this.loadTerms(),
                this.loadWholesellers(),
                this.loadStocks()
            ]);
        }
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "billing.productrows.productnr",
            "billing.productrows.text",
            "billing.order.syswholeseller",
            "billing.productrows.dialogs.changingwholeseller",
            "billing.productrows.dialogs.failedwholesellerchange",
            "billing.stock.stocksaldo.importstockbalance"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private save() {
        this.importStockBalances().then(() => {
            this.$uibModalInstance.close({ result: true });
        })
    }

    private close() {
        this.$uibModalInstance.close();
    }

    public loadWholesellers(): ng.IPromise<any> {
        return this.commonCustomerService.getSysWholesellersDict(true).then(x => {
            this.wholesellers = x;
        });
    }

    public loadStocks(): ng.IPromise<any> {
        // Load data
        return this.stockService.getStocks(false).then((x) => {
            this.stocksDict = x;
        });
    }

    private importStockBalances(): ng.IPromise<any> {
        if (this.stockInventoryHeadId) {
            return this.progress.startWorkProgress((completion) => {
                return this.stockService.importStockInventory(this.stockInventoryHeadId, this.importFileName, this.importFile).then((x) => {
                    if (x.success) {
                        completion.completed(x, true, "Stock inventory file imported: " + this.importFileName);
                        //this.notificationService.showDialog(this.terms["billing.stock.stocksaldo.importstockbalance"], "Stock inventory file imported: " + this.importFileName, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                    }
                    else {
                        completion.failed(x.errorMessage);
                        //this.notificationService.showDialog(this.terms["billing.stock.stocksaldo.importstockbalance"], x.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    }
                });
            });
        }
        else {
            return this.progress.startWorkProgress((completion) => {
                return this.stockService.importStockBalances(this.selectedWholeseller.id, this.selectedStock.stockId, this.createVoucher, this.importFileName, this.importFile).then((x) => {
                    if (x.success) {
                        completion.completed(x, true, "Stock file imported: " + this.importFileName);
                        //this.notificationService.showDialog(this.terms["billing.stock.stocksaldo.importstockbalance"], "Stock file imported: " + this.importFileName, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                    }
                    else {
                        completion.failed(x.errorMessage);
                        //this.notificationService.showDialog(this.terms["billing.stock.stocksaldo.importstockbalance"], x.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    }
                });
            });
        }
    }

    private okValid() {
        if (this.fromInventory) {
            return (this.importFile);
        }
        else {
            return ((this.importFile) && (this.selectedWholeseller) && (this.selectedStock));
        }
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
                } else {
                    //this.failedWork(result.errorMessage);
                }
            }, error => {
                //this.failedWork(error.message)
            });
        });
    }
}