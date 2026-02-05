import { ICoreService } from "../Services/CoreService";
import { ITranslationService } from "../Services/TranslationService";
import { IMessagingService } from "../Services/MessagingService";
import { INotificationService } from "../Services/NotificationService";
import { IUrlHelperService } from "../Services/UrlHelperService";
import { ToolBarButtonGroup, ToolBarUtility } from "../../Util/ToolBarUtility";
import { Guid } from "../../Util/StringUtility";
import { ProgressController } from "../Dialogs/Progress/ProgressController";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../Util/Enumerations";
import { Constants } from "../../Util/Constants";
import { Feature } from "../../Util/CommonEnumerations";

export interface IEditControllerBase {
    guid: Guid;
}

export class EditControllerBase {
    public guid: Guid;
    protected parameters: any;

    // Progress bar
    protected progressMessage: string;
    protected progressBusy: boolean = true;
    protected progressModalBusy: boolean = false;
    protected lookups: number = 0;

    // ToolBar
    protected buttonGroups = new Array<ToolBarButtonGroup>();

    //Modal
    public isModal: boolean = false;
    protected progressModalMetaData;
    protected progressModal;

    // Footer
    protected cancelButtonTemplateUrl: string;
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
    protected validationErrorStrings: string[] = [];

    // Navigation
    protected navigationMenuButtons = new Array<ToolBarButtonGroup>();

    // Dirty checking
    private dirty: boolean = false;
    get isDirty() {
        return this.dirty;
    }
    set isDirty(dirty: boolean) {
        this.dirty = dirty;
        this.messagingService.publish(Constants.EVENT_SET_TAB_MODIFIED, {
            guid: this.guid,
            dirty: this.dirty
        });
    }

    constructor(private editName: string,
        feature: Feature,
        protected $uibModal,
        protected translationService: ITranslationService,
        protected messagingService: IMessagingService,
        protected coreService: ICoreService,
        protected notificationService: INotificationService,
        protected urlHelperService: IUrlHelperService,) {

        this.cancelButtonTemplateUrl = urlHelperService.getCoreComponent("cancelButton.html");
        this.deleteButtonTemplateUrl = urlHelperService.getCoreComponent("deleteButton.html");
        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButton.html");

        if (feature)
            this.loadPermissions(feature);

        // Events
        this.messagingService.subscribe(Constants.EVENT_SET_DIRTY, (x) => {
            if (x && x.guid) {
                if (x.guid === this.guid)
                    this.isDirty = true;
            }
            else {
                this.isDirty = true;
            }
        });

    }

    // called by tabs controller
    protected onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = this.parameters.guid;
        this.init();
    }
    protected init() { }

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

    protected setupDefaultToolBar(showCopy: boolean = false, disableCopy = () => { return this.isNew; }): boolean {
        if (this.buttonGroups && this.buttonGroups.length > 0)
            return false;

        if (showCopy === true) {
            this.buttonGroups.push(ToolBarUtility.createGroup(ToolBarUtility.createCopyButton(() => {
                this.copy();
            }, disableCopy)));
        }

        return true;
    }

    protected setupLookups() {
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
            var modal = this.notificationService.showDialogEx(terms["core.warning"], terms["core.deletewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
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
        } else {
            this.progressMessage = '';
        }
    }

    protected startLoad() {
        this.startProgress("core.loading");
    }

    private openModalProgress(showAbort: boolean = false, abortCallback?: () => void) {
        this.progressModalMetaData.showabort = showAbort;

        // Check if dialog alreday open
        if (this.progressModal)
            return;

        this.progressModalBusy = true;

        var options = {
            templateUrl: this.urlHelperService.getGlobalUrl("Core/Dialogs/Progress/Views/Progress.html"),
            controller: ProgressController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                metadata: () => this.progressModalMetaData,
                progressParent: () => this
            }
        };

        this.progressModal = this.$uibModal.open(options);
        this.progressModal.result.then(result => {
            if (this.progressModal)
                this.progressModal = null;
        }, function () {
            // Cancelled
            if (showAbort && abortCallback)
                abortCallback();
            if (this.progressModal)
                this.progressModal = null;
        });
    }

    protected startModalProgress(messageKey: string = undefined, showAbort: boolean = false, abortCallback?: () => void) {
        if (!messageKey)
            messageKey = "core.working";

        this.translationService.translate(messageKey).then(s => {
            if (!this.progressModalMetaData)
                this.progressModalMetaData = {};
            this.progressModalMetaData.icon = 'far fa-spinner fa-pulse fa-fw';
            this.progressModalMetaData.text = s;
            this.progressModalMetaData.showabort = showAbort;

            // Check if dialog alreday open
            if (this.progressModal)
                return;

            this.progressModalBusy = true;

            var options = {
                templateUrl: this.urlHelperService.getGlobalUrl("Core/Dialogs/Progress/Views/Progress.html"),
                controller: ProgressController,
                controllerAs: 'ctrl',
                backdrop: 'static',
                size: 'lg',
                resolve: {
                    metadata: () => this.progressModalMetaData,
                    progressParent: () => this
                }
            };

            this.progressModal = this.$uibModal.open(options);
            this.progressModal.result.then(result => {
                if (this.progressModal)
                    this.progressModal = null;
            }, function () {
                // Cancelled
                if (showAbort && abortCallback)
                    abortCallback();
                if (this.progressModal)
                    this.progressModal = null;
            });
        });
    }

    protected stopModalProgress(data: any, skipDialog: boolean = true, message?: string) {
        if (skipDialog) {
            this.stopProgress();

            if (this.progressModal)
                this.progressModal.close();
        } else {
            this.progressModalMetaData.icon = 'fa-check-circle okColor';
            this.progressModalMetaData.showclose = true;
            if (message) {
                this.progressModalMetaData.text = message;
            } else {
                this.translationService.translate('core.saved').then(s => {
                    this.progressModalMetaData.text = s;
                });
            }
            this.stopProgress();
        }
    }

    protected startSave(useProgress: boolean = true, showAbort: boolean = false, abortCallback?: () => void) {
        if (useProgress)
            this.startProgress("core.saving");

        this.translationService.translate('core.saving').then(s => {
            if (!this.progressModalMetaData)
                this.progressModalMetaData = {};
            this.progressModalMetaData.icon = 'far fa-spinner fa-pulse fa-fw';
            this.progressModalMetaData.text = s;

            this.openModalProgress(showAbort, abortCallback);

            this.progressModal.closed.then(() => {
                this.saveMessageClosed();
                this.progressModalMetaData = null;
            });
        });
    }

    protected saveMessageClosed() {
        // Override in child class for setting focus etc.
    }

    protected startDelete(showAbort: boolean = false, abortCallback?: () => void) {
        this.startProgress("core.deleting");

        this.translationService.translate('core.deleting').then(s => {
            if (!this.progressModalMetaData)
                this.progressModalMetaData = {};
            this.progressModalMetaData.icon = 'far fa-spinner fa-pulse fa-fw';
            this.progressModalMetaData.text = s;

            this.openModalProgress(showAbort, abortCallback);

            this.progressModal.closed.then(() => {
                this.deleteMessageClosed();
                this.progressModalMetaData = null;
            });
        });
    }

    protected deleteMessageClosed() {
        // Override in child class for setting focus etc.
    }

    protected startWork(messageKey?: string, showAbort: boolean = false, abortCallback?: () => void) {
        if (!messageKey)
            messageKey = "core.working";
        this.startProgress(messageKey);

        this.translationService.translate(messageKey).then(s => {
            if (!this.progressModalMetaData)
                this.progressModalMetaData = {};
            this.progressModalMetaData.icon = 'far fa-spinner fa-pulse fa-fw';
            this.progressModalMetaData.text = s;

            this.openModalProgress(showAbort, abortCallback);

            this.progressModal.closed.then(() => {
                this.workMessageClosed();
                this.progressModalMetaData = null;
            });
        });
    }

    protected workMessageClosed() {
        // Override in child class for setting focus etc.
    }

    protected stopProgress() {
        this.progressBusy = false;
        this.progressMessage = '';
    }

    protected closeMe(reloadGrid: boolean) {
        // Send messages to TabsController
        this.messagingService.publish(Constants.EVENT_CLOSE_TAB, this.guid);
        if (reloadGrid) {
            this.messagingService.publish(Constants.EVENT_RELOAD_GRID, this.guid);
        }
    }

    protected completedSave(data: any, skipDialog: boolean = true, message?: string, asHtml: boolean = false) {
        if (this.isNew) {
            this.messagingService.publish(Constants.EVENT_EDIT_ADDED, {
                guid: this.guid,
                data: data
            });
        }
        else {
            this.messagingService.publish(Constants.EVENT_EDIT_SAVED, {
                guid: this.guid,
                data: data
            });
        }
        this.isDirty = false;
        if (skipDialog) {
            this.stopProgress();

            if (this.progressModal)
                this.progressModal.close();
        } else {
            if (this.progressModalMetaData) {
                this.progressModalMetaData.icon = 'fa-check-circle okColor';
                this.progressModalMetaData.showclose = true;
                if (message) {
                    if (asHtml) {
                        this.progressModalMetaData.text = '';
                        this.progressModalMetaData.html = message;
                    } else {
                        this.progressModalMetaData.html = '';
                        this.progressModalMetaData.text = message;
                    }
                } else {
                    this.translationService.translate('core.saved').then(s => {
                        this.progressModalMetaData.text = s;
                    });
                }
            }
            this.stopProgress();
        }
    }

    protected completedDelete(data, skipDialog?: boolean, message?: string) {
        this.messagingService.publish(Constants.EVENT_EDIT_DELETED, data);
        if (skipDialog) {
            this.stopProgress();

            if (this.progressModal)
                this.progressModal.close();
        } else {
            if (this.progressModalMetaData) {
                this.progressModalMetaData.icon = 'fa-check-circle okColor';
                this.progressModalMetaData.showclose = true;
                if (message) {
                    this.progressModalMetaData.text = message;
                } else {
                    this.translationService.translate('core.deleted').then(s => {
                        this.progressModalMetaData.text = s;
                    });
                }
            }
            this.stopProgress();
        }
    }

    protected completedWork(data, skipDialog?: boolean, message?: string) {
        this.messagingService.publish(Constants.EVENT_EDIT_WORKED, data);

        if (skipDialog) {
            this.stopProgress();

            if (this.progressModal)
                this.progressModal.close();
        } else {
            if (this.progressModalMetaData) {
                this.progressModalMetaData.icon = 'fa-check-circle okColor';
                this.progressModalMetaData.showclose = true;
                if (message) {
                    this.progressModalMetaData.text = message;
                } else {
                    this.translationService.translate('core.worked').then(s => {
                        this.progressModalMetaData.text = s;
                    });
                }
            }
            this.stopProgress();
        }
    }

    protected failedSave(message = "", closeDialog: boolean = false) {
        if (closeDialog) {
            this.stopProgress();

        if (this.progressModal)
            this.progressModal.close();
        } else {
            this.progressModalMetaData.icon = 'fa-exclamation-triangle errorColor';
            this.progressModalMetaData.showclose = true;

            if (message === "") {
                this.translationService.translate('core.savefailed').then(s => this.progressModalMetaData.text = s);
            } else {
                this.progressModalMetaData.text = message;
            }
            this.stopProgress();
        }
    }

    protected failedDelete(message = "") {
        this.progressModalMetaData.icon = 'fa-exclamation-triangle errorColor';
        this.progressModalMetaData.showclose = true;

        if (message === "") {
            this.translationService.translate('core.deletefailed').then(s => this.progressModalMetaData.text = s);
        } else {
            this.progressModalMetaData.text = message;
        }
        this.stopProgress();
    }

    protected failedWork(message = "", title = "", closeDialog: boolean = false) {
        if (closeDialog) {
            this.stopProgress();

            if (this.progressModal)
                this.progressModal.close();
        } else {
            this.progressModalMetaData.icon = 'fa-exclamation-triangle errorColor';
            this.progressModalMetaData.showclose = true;

            if (message === "") {
                this.translationService.translate('core.workfailed').then(s => this.progressModalMetaData.text = s);
            } else {
                this.progressModalMetaData.text = message;
            }

            if (title)
                this.progressModalMetaData.title = title;

            this.stopProgress();
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

    // TOOLBAR

    protected copy() {
        // Override in EditController to perform actual copy
    }

    protected setupNavigationGroup(disabled = () => { }, hidden = () => { }) {
        var group = ToolBarUtility.createNavigationGroup(
            () => {
                this.messagingService.publish(Constants.EVENT_NAVIGATE_FIRST, { guid: this.guid });
            },
            () => {
                this.messagingService.publish(Constants.EVENT_NAVIGATE_LEFT, { guid: this.guid });
            },
            () => {
                this.messagingService.publish(Constants.EVENT_NAVIGATE_RIGHT, { guid: this.guid });
            },
            () => {
                this.messagingService.publish(Constants.EVENT_NAVIGATE_LAST, { guid: this.guid });
            },
            disabled,
            hidden
        );
        this.navigationMenuButtons.push(group);
    }

    // VALIDATION

    protected validate() {
        // Override in EditController and set validationErrorKeys
    }

    protected showValidationError() {
        this.mandatoryFieldKeys = [];
        this.validationErrorKeys = [];
        this.validationErrorStrings = [];
        this.validate();

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
            });
        }
    }
}