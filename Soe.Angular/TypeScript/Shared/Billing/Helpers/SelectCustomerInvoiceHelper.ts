import { SelectCustomerInvoiceController } from "../../../Common/Dialogs/SelectCustomerInvoice/SelectCustomerInvoiceController";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IGridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { GridControllerBaseAg } from "../../../Core/Controllers/GridControllerBaseAg";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SoeOriginType, SoeProjectRecordType } from "../../../Util/CommonEnumerations";
import { OrderEditProjectFunctions, SOEMessageBoxButton, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { IOrderService } from "../Orders/OrderService";

type InvoiceSearchResult = {
    number: string
    customerInvoiceId: number
    customerId: number
    customerName: string
    customerNumber: string
    projectId: number
    projectName: string
    projectNr: string
}

export class SelectCustomerInvoiceHelper {
    public orderId: number;

    //@ngInject
    constructor(private parent: EditControllerBase2 | GridControllerBaseAg | GridControllerBase2Ag,
        private orderService: IOrderService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private $q: ng.IQService,
        private $uibModal: any,
        private customerInvoiceChanged: (invoice: InvoiceSearchResult, data: any) => void,
        private getProject: () => { projectName: string, projectId: number },
    ) {
    }

    public openOrderSearch(data: any) {
        const { projectName, projectId } = this.getProject();
        this.translationService.translate("core.search").then(term => {

            const modal = this.$uibModal.open({
                templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/selectcustomerinvoice", "selectcustomerinvoice.html"),
                controller: SelectCustomerInvoiceController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    title: () => { return term },
                    isNew: () => { return true },
                    ignoreChildren: () => { return false },
                    originType: () => { return SoeOriginType.Order },
                    customerId: () => { return null },
                    projectId: () => { return projectId || null },
                    invoiceId: () => { return null },
                    currentMainInvoiceId: () => { return null },
                    selectedProjectName: () => { return projectName || "" },
                    userId: () => { return null },
                    includePreliminary: () => { return null },
                    includeVoucher: () => { return null },
                    fullyPaid: () => { return null },
                    useExternalInvoiceNr: () => { return null },
                    importRow: () => { return null },
                }
            });

            modal.result.then(result => {
                if (result && result.invoice) {
                    this.customerInvoiceChanged(result.invoice, data);
                }
            });
        })

    }
}