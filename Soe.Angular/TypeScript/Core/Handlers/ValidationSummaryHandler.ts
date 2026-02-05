import { ITranslationService } from "../Services/TranslationService";
import { INotificationService } from "../Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../Util/Enumerations";

export interface IValidationSummaryHandler {
    showValidationSummary(validationCallback: (mandatoryFieldKeys: string[], validationErrorKeys: string[], validationErrorStrings: string[]) => void): ng.IPromise<boolean>;

}

export class ValidationSummaryHandler implements IValidationSummaryHandler {
    private mandatoryFieldKeys: string[] = [];
    private validationErrorKeys: string[] = [];
    private validationErrorStrings: string[] = [];

    constructor(private $q: ng.IQService, private translationService: ITranslationService, private notificationService: INotificationService) { }

    showValidationSummary(validationCallback: (mandatoryFieldKeys: string[], validationErrorKeys: string[], validationErrorStrings: string[]) => void) {
        this.mandatoryFieldKeys = [];
        this.validationErrorKeys = [];
        this.validationErrorStrings = [];

        var deferral = this.$q.defer<boolean>();

        validationCallback(this.mandatoryFieldKeys, this.validationErrorKeys, this.validationErrorStrings);

        var keys: string[] = [];

        if (this.mandatoryFieldKeys.length > 0 || this.validationErrorKeys.length > 0 || this.validationErrorStrings.length > 0) {
            keys.push("error.unabletosave_title");

            // Mandatory fields
            if (this.mandatoryFieldKeys.length > 0) {
                keys.push("core.missingmandatoryfield");
                _.forEach(this.mandatoryFieldKeys, (key) => {
                    keys.push(key);
                });
            }

            // Other messages
            if (this.validationErrorKeys.length > 0) {
                _.forEach(this.validationErrorKeys, (key) => {
                    keys.push(key);
                });
            }
        }

        if (keys.length > 0) {
            this.translationService.translateMany(keys).then((terms) => {
                var message: string = "";

                // Mandatory fields
                if (this.mandatoryFieldKeys.length > 0) {
                    _.forEach(this.mandatoryFieldKeys, (key) => {
                        message = message + terms["core.missingmandatoryfield"] + " " + terms[key].toLocaleLowerCase() + ".\\n";
                    });
                }

                // Other messages
                if (this.validationErrorKeys.length > 0) {
                    _.forEach(this.validationErrorKeys, (key) => {
                        message = message + terms[key] + ".\\n";
                    });
                }

                // Predefined messages
                if (this.validationErrorStrings.length > 0) {
                    _.forEach(this.validationErrorStrings, (str) => {
                        message = message + str + ".\\n";
                    });
                }

                this.notificationService.showDialog(terms["error.unabletosave_title"], message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);

                deferral.reject();
            });
        } else {
            deferral.resolve();
        }

        return deferral.promise;
    }
}
