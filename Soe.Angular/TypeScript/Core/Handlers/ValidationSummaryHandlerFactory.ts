import { IValidationSummaryHandler, ValidationSummaryHandler } from "./ValidationSummaryHandler";
import { ITranslationService } from "../Services/TranslationService";
import { INotificationService } from "../Services/NotificationService";

export interface IValidationSummaryHandlerFactory {
    create(): IValidationSummaryHandler;
}

export class ValidationSummaryHandlerFactory implements IValidationSummaryHandlerFactory {
    //@ngInject
    constructor(private $q: ng.IQService, private translationService: ITranslationService, private notificationService: INotificationService) {
    }

    create(): IValidationSummaryHandler {
        return new ValidationSummaryHandler(this.$q, this.translationService, this.notificationService);
    }
}
