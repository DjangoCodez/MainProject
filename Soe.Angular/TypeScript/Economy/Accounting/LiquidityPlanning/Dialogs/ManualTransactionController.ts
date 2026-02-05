import { ISmallGenericType } from "../../../../Scripts/TypeLite.Net4";
import { IAccountingService } from "../../../../Shared/Economy/Accounting/AccountingService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { Constants } from "../../../../Util/Constants";
import { IProgressHandler } from "../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { LiquidityPlanningDTO } from "../../../../Common/Models/LiquidityPlanningDTO";

export class ManualTransactionController {

    progress: IProgressHandler;

    transaction: LiquidityPlanningDTO

    get validToSave() {
        return (this.transaction.specification && this.transaction.specification !== this.trans.specification) ||
            (this.transaction.date && this.transaction.date !== this.trans.date) ||
            (this.transaction.total && this.transaction.total !== 0 && this.transaction.total !== this.trans.total);
    }

    get amount(): number {
        return this.transaction.total;
    }
    set amount(item: number) {
        if (item > 0) {
            this.transaction.valueIn = item;
            this.transaction.valueOut = 0;
        }
        else {
            this.transaction.valueOut = item;
            this.transaction.valueIn = 0;
        }
        this.transaction.total = item;
    }

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private trans: LiquidityPlanningDTO) {

        this.transaction = new LiquidityPlanningDTO();
        angular.extend(this.transaction, trans);
    }

    private save() {
        this.$uibModalInstance.close({ item: this.transaction });
    }

    private delete() {
        this.$uibModalInstance.close({ item: this.transaction, delete: true });
    }

    private close() {
        this.$uibModalInstance.close();
    }
}