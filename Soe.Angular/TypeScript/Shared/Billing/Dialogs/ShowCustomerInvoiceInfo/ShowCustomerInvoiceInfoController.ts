import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { TermGroup } from "../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IOrderService } from "../../Orders/OrderService";

export class ShowCustomerInvoiceInfoController {

    private summary: any;
    private progressBusy = true;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private translationService: ITranslationService,
        private orderService: IOrderService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
        private customerInvoiceId: number,
        private projectId: number,
        private title: string
    ) {
        
    }

    private $onInit() {
        this.loadCustomerInvoiceInfo();
    }

    private loadCustomerInvoiceInfo() {
        this.orderService.getOrderSummary(this.customerInvoiceId, this.projectId).then((x) => {
            this.summary = x;
            this.progressBusy = false;
        });
    }

    buttonOkClick() {
        this.$uibModalInstance.close('ok');
    }



    
}