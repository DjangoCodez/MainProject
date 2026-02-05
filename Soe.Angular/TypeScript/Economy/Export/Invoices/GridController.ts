import { GridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { IExportService } from "../ExportService";
import { Feature } from "../../../Util/CommonEnumerations";

export class GridController extends GridControllerBase {
    //
    public exportType: number;
    // Collections
    termsArray: any;
    // Subgrid rows
    subGridRows: any;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        $filter: ng.IFilterService,
        private type: number,
        coreService: ICoreService,
        private accountingService: IAccountingService,
        private exportService: IExportService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants) {

        super("Economy.Export.Invoices", "economy.export.invoice.invoices", Feature.Economy_Export_Invoices, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
    }

    protected setupCustomToolBar() {
        super.setupDefaultToolBar(true);
    }

    public setupGrid() {
        this.stopProgress();
        // Columns
        const keys: string[] = [
            "economy.export.payments.username",
            "economy.export.payments.exportdate",
            "economy.export.payments.cancelled",
            "economy.export.payments.download",
            "economy.export.invoices.description",
            "economy.export.invoices.info"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.termsArray = terms;
            super.addColumnText("createdBy", terms["economy.export.payments.username"], "25%");
            super.addColumnDate("exportDate", terms["economy.export.payments.created"], null);
            super.addColumnText("description", terms["economy.export.invoices.description"], null);
            super.addColumnText("information", terms["economy.export.invoices.info"], null);
            super.addColumnIcon(null, "fal fa-download", terms["economy.export.payments.download"], "doDownload", null, "showDownloadIcon", terms["economy.export.payments.download"]);
            super.addColumnIcon(null, "fal fa-times iconDelete", terms["economy.export.payments.cancelpayment"], "doCancel", null, "showCancelIcon", terms["economy.export.payments.cancelpayment"]);

        });
    }

    public loadGridData() {
        this.stopProgress();
        // Load data
        this.accountingService.getDataStorages(this.type).then((x) => {
            super.gridDataLoaded(x);
        });

    }

    protected showDownloadIcon(row) {
        return true;
    }

    protected showCancelIcon(row) {
        return true;
    }

    private doDownload(row) {
        //Show downloadlink            
        //string url = HtmlPage.Document.DocumentUri.ToString().Replace(HtmlPage.Document.DocumentUri.Query, "");
        //string query = "?c=" + actorCompanyId + "&r=" + roleId + "&storageType=" + (int)storageType + "&dataStorageId=" + dataStorage.DataStorageId;
        //url += query;
        //link.NavigateUri = new Uri(url, UriKind.Absolute);

        var uri = window.location.protocol + "//" + window.location.host;
        uri = uri + "/soe/economy/export/invoices/sop/default.aspx" + "?c=" + CoreUtility.actorCompanyId + "&r=" + CoreUtility.roleId + "&storageType=" + this.type + "&dataStorageId=" + row.dataStorageId;

        window.open(uri, '_blank');
    }

    private doCancel(row) {
        if (row.dataStorageId && row.dataStorageId != 0) {
            this.accountingService.undoDataStorage(row.dataStorageId).then((result) => {
                if (result.success) {
                    //LoadGridData
                    this.loadGridData();
                }
                else {
                    this.failedDelete(result.errorMessage);
                }
            }, error => {
                this.failedDelete(error.message);
            });
        }
    }

}
