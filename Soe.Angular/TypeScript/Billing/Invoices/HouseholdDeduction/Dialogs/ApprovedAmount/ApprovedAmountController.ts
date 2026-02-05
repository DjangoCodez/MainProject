import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../../Core/Services/UrlHelperService";

export class ApprovedAmountController {

    private createInvoice = false;

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $window: ng.IWindowService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private approvedAmount: number, 
    ) {
    }

    buttonCancelClick() {
        this.$uibModalInstance.close(null);
    }

    buttonOkClick() {
        this.$uibModalInstance.close({ approvedAmount: this.approvedAmount, createInvoice: this.createInvoice });
    }
}
