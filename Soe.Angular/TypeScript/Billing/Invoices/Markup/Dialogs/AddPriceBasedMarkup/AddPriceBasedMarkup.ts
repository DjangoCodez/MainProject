import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/ProgressHandlerFactory";
import { PriceBasedMarkupDTO } from "../../../../../Common/Models/InvoiceDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";

export class AddPriceBasedMarkupController {

    progress: IProgressHandler;

    get validToSave() {
        return this.priceBasedMarkup && (this.priceBasedMarkup.minPrice || this.priceBasedMarkup.maxPrice) && this.priceBasedMarkup.markupPercent > 0;
    }

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        progressHandlerFactory: IProgressHandlerFactory,
        private priceBasedMarkup: PriceBasedMarkupDTO,
        private priceLists: SmallGenericType[]) {

        this.progress = progressHandlerFactory.create();
    }

    private save() {
        this.$uibModalInstance.close({ item: this.priceBasedMarkup });
    }

    private delete() {
        this.$uibModalInstance.close({ item: this.priceBasedMarkup, delete: true });
    }

    private close() {
        this.$uibModalInstance.close();
    }
}