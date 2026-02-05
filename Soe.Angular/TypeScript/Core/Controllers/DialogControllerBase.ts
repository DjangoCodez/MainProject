import { ICoreService } from "../Services/CoreService";
import { ITranslationService } from "../Services/TranslationService";
import { INotificationService } from "../Services/NotificationService";
import { IUrlHelperService } from "../Services/UrlHelperService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../Util/Enumerations";
import { Feature } from "../../Util/CommonEnumerations";

export class DialogControllerBase {

    // Progress bar
    protected progressMessage: string;
    protected progressBusy: boolean;
    protected lookups: number = 0;

    // Footer
    protected deleteButtonTemplateUrl: string;
    protected saveButtonTemplateUrl: string;

    // Permissions
    protected readOnlyPermission: boolean = false;
    protected modifyPermission: boolean = false;

    // Data
    protected isNew: boolean = true;

    // Validation
    protected mandatoryFieldKeys: string[] = [];
    protected validationErrorKeys: string[] = [];

    constructor(feature: Feature,
        protected translationService: ITranslationService,
        protected coreService: ICoreService,
        protected notificationService: INotificationService,
        protected urlHelperService: IUrlHelperService) {

        this.deleteButtonTemplateUrl = urlHelperService.getCoreComponent("deleteButton.html");
        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButton.html");

        if (feature)
            this.loadPermissions(feature);
    }

    // SETUP

    protected loadPermissions(feature: Feature) {
        var featureIds: number[] = [];
        featureIds.push(feature);

        this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            if (x[feature]) {
                this.readOnlyPermission = true;
                this.coreService.hasModifyPermissions(featureIds).then((y) => {
                    if (y[feature]) {
                        this.modifyPermission = true;
                    }
                    this.permissionsLoaded();
                });
            } else {
                this.permissionsLoaded();
            }
        });
    }

    protected permissionsLoaded() {
        this.setupLookups();
        // Override in child class to call other setup methods below after permissions are loaded
    }

    protected setupLookups() {
        this.startLoad();
        // Override in child class
    }

    // PUBLIC PROPERTIES

    protected parseInt(value: string) {
        return parseInt(value, 10);
    }

    // ACTIONS

    protected initDelete() {
        // Show verification dialog
        var keys: string[] = [
            "core.warning",
            "core.deletewarning"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            var modal = this.notificationService.showDialog(terms["core.warning"], terms["core.deletewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.startDelete();
                    this.delete();
                }
            });
        });
    }

    protected delete() {
    }

    // PROGRESS         

    protected lookupLoaded() {
        this.lookups = this.lookups - 1;
        this.progressBusy = this.lookups > 0;
    }

    protected startProgress(messageKey: string = undefined) {
        this.progressBusy = true;
        if (messageKey) {
            this.translationService.translate(messageKey).then((term) => {
                this.progressMessage = term;
            });
        }
    }

    protected startLoad() {
        this.startProgress("core.loading");
    }

    protected startSave() {
        this.startProgress("core.saving");
    }

    protected startDelete() {
        this.startProgress("core.deleting");
    }

    protected startWork() {
        this.startProgress("core.working");
    }

    protected stopProgress() {
        this.progressBusy = false;
    }

    protected completedSave(message?: string) {
        if (message)
            this.showCompletedDialog(message);
        else
            this.showCompletedDialogAfterTranslate("core.saved");
    }

    protected completedDelete() {
        this.showCompletedDialogAfterTranslate("core.deleted");
    }

    protected completedWork() {
        this.showCompletedDialogAfterTranslate("core.worked");
    }

    protected showCompletedDialogAfterTranslate(key) {
        this.translationService.translate(key).then((message) => {
            this.showCompletedDialog(message);
        });
    }

    protected showCompletedDialog(message) {
        this.notificationService.showDialog("", message, SOEMessageBoxImage.OK, SOEMessageBoxButtons.OK);
        this.stopProgress();
    }

    protected failedWork(message = "") {
        if (message === "") {
            this.showFailedDialogAfterTranslate("core.workfailed");
        }
        else {
            this.showFailedDialog(message);
        }
    }

    protected failedSave(message = "") {
        if (message === "") {
            this.showFailedDialogAfterTranslate("core.savefailed");
        }
        else {
            this.showFailedDialog(message);
        }
    }

    protected failedDelete(message = "") {
        if (message === "") {
            this.showFailedDialogAfterTranslate("core.deletefailed");
        }
        else {
            this.showFailedDialog(message);
        }
    }

    protected showFailedDialogAfterTranslate(key) {
        this.translationService.translate(key).then((message) => {
            this.showFailedDialog(message);
        });
    }

    protected showFailedDialog(message) {
        this.notificationService.showDialog("", message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
        this.stopProgress();
    }

    // VALIDATION

    protected validate() {
        // Override in EditController and set validationErrorKeys
    }

    protected showValidationError() {
        this.mandatoryFieldKeys = [];
        this.validationErrorKeys = [];
        this.validate();

        var keys: string[] = [];

        if (this.mandatoryFieldKeys.length > 0 || this.validationErrorKeys.length > 0) {
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

                this.notificationService.showDialog(terms["error.unabletosave_title"], message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
        }
    }
}