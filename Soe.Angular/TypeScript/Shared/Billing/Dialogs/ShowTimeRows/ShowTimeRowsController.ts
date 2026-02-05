import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { ProductRowDTO } from "../../../../Common/Models/InvoiceDTO";
import { IOrderService } from "../../Orders/OrderService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { TimeRowsHelper } from "../../Helpers/TimeRowsHelper";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { Guid } from "../../../../Util/StringUtility";
import { IMessagingService } from "../../../../Core/Services/MessagingService";

export class ShowTimeRowsController {

    // Terms
    private terms: any;
    private guid: Guid;
    // Values
    private timeRowsHelper: TimeRowsHelper;

    progressBusy: boolean = false;

    //@ngInject
    constructor(
        orderService: IOrderService,
        coreService: ICoreService,
        $scope: ng.IScope,
        $q: ng.IQService,
        $uibModal,
        private $uibModalInstance,
        urlHelperService: IUrlHelperService,
        messagingService: IMessagingService,
        invoiceId: number,
        customerInvoiceRowId: number,
        private isReadOnly: boolean,
        private productId : number,
        private translationService: ITranslationService,
        private productRows: ProductRowDTO[]) {

        this.guid = Guid.newGuid();
        this.timeRowsHelper = new TimeRowsHelper(this.guid, $q, $uibModal, $scope, messagingService, urlHelperService, translationService, orderService, coreService, invoiceId, customerInvoiceRowId);
    }

    private $onInit() {
        this.timeRowsHelper.loadTimeProjectRows(false);
    }

    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.rownr",
            "common.name",
            "core.succeeded",
            "core.failed",
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    buttonCancelClick() {
        this.close(this.timeRowsHelper.reloadInvoiceAfterClose);
    }

    buttonOkClick() {
        this.close(this.timeRowsHelper.reloadInvoiceAfterClose);
    }

    close(result: any) {
        if (!result) {
            this.$uibModalInstance.dismiss('cancel');
        } else {
            this.$uibModalInstance.close(result); 
        }
    }

    public selectProductRow(): ng.IPromise<number> {
        return this.timeRowsHelper.selectProductRow(this.productId, this.productRows);
    }
}