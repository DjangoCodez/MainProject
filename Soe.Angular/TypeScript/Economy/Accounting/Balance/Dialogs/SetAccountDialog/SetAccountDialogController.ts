import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { IAccountingService } from "../../../../../Shared/Economy/Accounting/AccountingService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { Constants } from "../../../../../Util/Constants";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/ProgressHandlerFactory";

export class SetAccountDialogController {

    private selectedAccount;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        progressHandlerFactory: IProgressHandlerFactory,
        private amount: number,
        private accounts: []) {
    }

    private save() {
        //this.importStockBalances().then(() => {
            this.$uibModalInstance.close(this.selectedAccount);
        //})
    }

    private close() {
        this.$uibModalInstance.close();
    }
}