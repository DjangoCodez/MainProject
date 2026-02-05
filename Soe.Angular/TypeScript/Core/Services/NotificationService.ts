import { ICoreService } from "./CoreService";
import { UrlHelperService } from "./UrlHelperService";
import { TranslationService } from "./TranslationService";
import { IMessagingService } from "./MessagingService";
import { IHttpService } from "./HttpService";
import { CoreUtility } from "../../Util/CoreUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons, SOEMessageBoxSize, SOEMessageBoxButton } from "../../Util/Enumerations";
import { IEvaluateWorkRulesActionResult, ISmallGenericType } from "../../Scripts/TypeLite.Net4";
import { MessageBoxController } from "../Controllers/MessageBoxController";
import { FileUploadController } from "../Controllers/FileUploadController";
import { TermGroup_ShiftHistoryType, SoeEntityType, SoeEntityImageType } from "../../Util/CommonEnumerations";
import { Constants } from "../../Util/Constants";
import { ModalUtility } from "../../Util/ModalUtility";

export interface INotificationService {
    showDialog(title: string, message: string, image: SOEMessageBoxImage, buttons: SOEMessageBoxButtons);
    showDialog(title: string, message: string, image: SOEMessageBoxImage, buttons: SOEMessageBoxButtons, size: SOEMessageBoxSize);
    showDialog(title: string, message: string, image: SOEMessageBoxImage, buttons: SOEMessageBoxButtons, size: SOEMessageBoxSize, isFromGrid: boolean);
    showDialog(title: string, message: string, image: SOEMessageBoxImage, buttons: SOEMessageBoxButtons, size: SOEMessageBoxSize, isFromGrid: boolean, showCheckBox: boolean, checkBoxLabel: string);
    showDialog(title: string, message: string, image: SOEMessageBoxImage, buttons: SOEMessageBoxButtons, size: SOEMessageBoxSize, isFromGrid: boolean, showCheckBox: boolean, checkBoxLabel: string, isChecked: boolean);
    showDialog(title: string, message: string, image: SOEMessageBoxImage, buttons: SOEMessageBoxButtons, size: SOEMessageBoxSize, isFromGrid: boolean, showCheckBox: boolean, checkBoxLabel: string, isChecked: boolean, buttonOkLabelKey: string, buttonYesLabelKey: string, buttonNoLabelKey: string, buttonCancelLabelKey: string, initialFocusButton: SOEMessageBoxButton);
    showDialog(title: string, message: string, image: SOEMessageBoxImage, buttons: SOEMessageBoxButtons, size: SOEMessageBoxSize, isFromGrid: boolean, showCheckBox: boolean, checkBoxLabel: string, isChecked: boolean, buttonOkLabelKey: string, buttonYesLabelKey: string, buttonNoLabelKey: string, buttonCancelLabelKey: string, initialFocusButton: SOEMessageBoxButton, returnNullIfCancel: boolean);
    showDialogDefButton(title: string, message: string, image: SOEMessageBoxImage, buttons: SOEMessageBoxButtons, initialFocusButton: SOEMessageBoxButton);

    showDialogEx(title: string, message: string, image: SOEMessageBoxImage);
    showDialogEx(title: string, message: string, image: SOEMessageBoxImage, buttons: SOEMessageBoxButtons);
    showDialogEx(title: string, message: string, image: SOEMessageBoxImage, buttons: SOEMessageBoxButtons, config: IDialogConfig);

    showConfirmOnClose(multipleTabs: boolean): ng.IPromise<boolean>;
    showConfirmOnExit(): ng.IPromise<boolean>;
    showConfirmOnDelete(confirmMessage?: string): ng.IPromise<boolean>;
    showConfirmOnContinue(): ng.IPromise<boolean>;

    showServiceError(reason: any, title?: string);
    showErrorDialog(title: string, message: string, hiddenMessage: string);

    showFileUpload(url: string, title: string, showDropZone: boolean, showQueue: boolean, allowMultipleFiles: boolean, noMaxSize?: boolean, showRoles?: boolean, rolesMandatory?: boolean);
    showFileUploadEx(title: string, options?: FileUploadOptions);
    showValidateWorkRulesResult(action: TermGroup_ShiftHistoryType, result: IEvaluateWorkRulesActionResult, employeeId: number, showCancelAll?: boolean, dialogTitleKey?: string): ng.IPromise<boolean>;
    showValidateShortenEmploymentResult(result): ng.IPromise<boolean>;
    showValidateSaveEmployee(result): ng.IPromise<boolean>;
}

export interface IDialogConfig {
    customIcon?: string;
    hiddenMessage?: string;
    size?: SOEMessageBoxSize;
    isFromGrid?: boolean;
    showCheckBox?: boolean;
    checkBoxLabel?: string;
    isChecked?: boolean;
    showTextBox?: boolean;
    textBoxLabel?: string;
    textBoxValue?: string;
    textBoxRows?: number;
    textBoxType?: string;
    showDatePicker?: boolean;
    datePickerLabel?: string;
    datePickerValue?: Date;
    showButtonCancelAll?: boolean;
    buttonOkLabelKey?: string;
    buttonYesLabelKey?: string;
    buttonNoLabelKey?: string;
    buttonCancelLabelKey?: string;
    buttonCancelAllLabelKey?: string;
    initialFocusButton?: SOEMessageBoxButton;
    returnNullIfCancel?: boolean;
    autoCloseDelay?: number;
    useTextValidation?: boolean;
}

export class NotificationService implements INotificationService {
    private defaultConfig: IDialogConfig = {
        customIcon: '', hiddenMessage: '', size: SOEMessageBoxSize.Medium, isFromGrid: false, showCheckBox: false, checkBoxLabel: '', isChecked: false, showTextBox: false, textBoxLabel: '', textBoxValue: '', textBoxRows: 1, showDatePicker: false, datePickerLabel: '', datePickerValue: null,
        buttonOkLabelKey: '', buttonYesLabelKey: '', buttonNoLabelKey: '', buttonCancelLabelKey: '', initialFocusButton: SOEMessageBoxButton.Cancel, returnNullIfCancel: false,
    };

    //@ngInject
    constructor(
        private httpService: IHttpService,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private $uibModal,
        private urlHelperService: UrlHelperService,
        private translationService: TranslationService,
        private messagingService: IMessagingService,
        private coreService: ICoreService) {
    }

    public showDialogDefButton(title: string, message: string, image: SOEMessageBoxImage, buttons: SOEMessageBoxButtons, initialFocusButton: SOEMessageBoxButton): any {
        return this.showDialogEx(title, message, image, buttons, { hiddenMessage: "", isFromGrid: false, showCheckBox: false, checkBoxLabel: "", isChecked: false, buttonOkLabelKey: "", buttonYesLabelKey: "", buttonNoLabelKey: "", buttonCancelLabelKey: "", initialFocusButton: initialFocusButton });
    }

    public showDialog(title: string,
        message: string,
        image: SOEMessageBoxImage,
        buttons: SOEMessageBoxButtons,
        size: SOEMessageBoxSize = SOEMessageBoxSize.Medium,
        isFromGrid: boolean = false,
        showCheckBox: boolean = false,
        checkBoxLabel: string = '',
        isChecked: boolean = false,
        buttonOkLabelKey: string = '',
        buttonYesLabelKey: string = '',
        buttonNoLabelKey: string = '',
        buttonCancelLabelKey: string = '',
        initialFocusButton: SOEMessageBoxButton = SOEMessageBoxButton.Cancel,
        returnNullIfCancel: boolean = false,

    ): any {
        return this.showDialogEx(title, message, image, buttons, { customIcon: '', hiddenMessage: '', isFromGrid: isFromGrid, showCheckBox: showCheckBox, checkBoxLabel: checkBoxLabel, isChecked: isChecked, buttonOkLabelKey: buttonOkLabelKey, buttonYesLabelKey: buttonYesLabelKey, buttonNoLabelKey: buttonNoLabelKey, buttonCancelLabelKey: buttonCancelLabelKey, initialFocusButton: initialFocusButton, returnNullIfCancel: returnNullIfCancel });
    }

    public showDialogEx(title: string, message: string, image: SOEMessageBoxImage, buttons: SOEMessageBoxButtons = null, config: IDialogConfig = null) {
        if (!buttons) {
            buttons = SOEMessageBoxButtons.OK;
        }

        var cfg: IDialogConfig = {};
        angular.extend(cfg, this.defaultConfig);
        angular.extend(cfg, config || {});

        var modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getCoreViewUrl("ChildWindows/MessageBox.html"),
            controller: MessageBoxController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: CoreUtility.getSOEMessageBoxSizeString(cfg.size),
            resolve: {
                translationService: () => { return this.translationService },
                title: () => { return title },
                text: () => { return message },
                hiddenText: () => { return cfg.hiddenMessage },
                image: () => { return image },
                buttons: () => { return buttons },
                customIcon: () => { return cfg.customIcon },
                isFromGrid: () => { return cfg.isFromGrid },
                showCheckBox: () => { return cfg.showCheckBox },
                checkBoxLabel: () => { return cfg.checkBoxLabel },
                isChecked: () => { return cfg.isChecked },
                showTextBox: () => { return cfg.showTextBox },
                textBoxLabel: () => { return cfg.textBoxLabel },
                textBoxValue: () => { return cfg.textBoxValue },
                textBoxRows: () => { return cfg.textBoxRows },
                textBoxType: () => { return cfg.textBoxType },
                showDatePicker: () => { return cfg.showDatePicker },
                datePickerLabel: () => { return cfg.datePickerLabel },
                datePickerValue: () => { return cfg.datePickerValue },
                showButtonCancelAll: () => { return cfg.showButtonCancelAll },
                buttonOkLabelKey: () => { return cfg.buttonOkLabelKey },
                buttonYesLabelKey: () => { return cfg.buttonYesLabelKey },
                buttonNoLabelKey: () => { return cfg.buttonNoLabelKey },
                buttonCancelLabelKey: () => { return cfg.buttonCancelLabelKey },
                buttonCancelAllLabelKey: () => { return cfg.buttonCancelAllLabelKey },
                initialFocusButton: () => { return cfg.initialFocusButton },
                returnNullIfCancel: () => { return cfg.returnNullIfCancel },
                useTextValidation: () => {return cfg.useTextValidation }
            }
        });

        if (cfg.autoCloseDelay && cfg.autoCloseDelay > 0) {
            this.$timeout(() => {
                modal.dismiss('cancel');
            }, cfg.autoCloseDelay);
        }

        return modal;
    }

    public showServiceError(reason: any, title: string = '') {
        var message: string = '';
        var hiddenMessage: string = '';
        if (reason.message)
            message = reason.message + '<br/>';

        if (reason.error) {
            var error = reason.error;
            while (error.innerException) {
                if (error.exceptionMessage)
                    hiddenMessage += error.exceptionMessage + '<br/>';
                error = error.innerException;
            }
            if (error.exceptionMessage)
                hiddenMessage += error.exceptionMessage + '<br/>';
        }

        if (!title) {
            this.translationService.translate("error.default_error").then(term => {
                this.showErrorDialog(term, message, hiddenMessage);
            });
        } else {
            this.showErrorDialog(title, message, hiddenMessage);
        }
    }

    public showErrorDialog(title: string, message: string, hiddenMessage: string) {
        return this.showDialogEx(title, message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK, { hiddenMessage: hiddenMessage });
    }

    public showConfirmOnClose(multipleTabs: boolean): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        var keys: string[] = [
            "core.warning",
            "core.confirmonclosetab",
            "core.confirmonclosetabs"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.showDialog(terms["core.warning"], multipleTabs ? terms["core.confirmonclosetabs"] : terms["core.confirmonclosetab"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo).result
                .then(val => {
                    deferral.resolve(val);
                });
        });

        return deferral.promise;
    }

    public showConfirmOnContinue(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        var keys: string[] = [
            "core.warning",
            "common.unsavedchanges",
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.showDialog(terms["core.warning"], terms["common.unsavedchanges"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo).result
                .then(val => {
                    deferral.resolve(val);
                });
        });

        return deferral.promise;
    }


    public showConfirmOnDelete(confirmMessage: string = ''): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        if (confirmMessage === ModalUtility.MODAL_SKIP_CONFIRM) {
            deferral.resolve(true);
        } else {
            var keys: string[] = [
                "core.warning",
                "core.deletewarning"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.showDialogEx(terms["core.warning"], confirmMessage ? confirmMessage : terms["core.deletewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    deferral.resolve(val);
                },
                    (cancel) => {
                        deferral.resolve(false);
                    }
                );
            });
        }

        return deferral.promise;
    }

    public showConfirmOnExit(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        var keys: string[] = [
            "core.warning",
            "core.confirmonexit"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.showDialog(terms["core.warning"], terms["core.confirmonexit"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel).result
                .then((val) => {
                    deferral.resolve(val);
                },
                    (cancel) => {
                        deferral.resolve(false);
                    }
                );
        });

        return deferral.promise;
    }

    public showFileUpload(url: string, title: string, showDropZone: boolean, showQueue: boolean, allowMultipleFiles: boolean, noMaxSize: boolean = false, showRoles: boolean = false, rolesMandatory: boolean = false, showMessageGroups: boolean = false): any {
        return this.showFileUploadEx(title, { url: url, showDropZone: showDropZone, showQueue: showQueue, allowMultipleFiles: allowMultipleFiles, noMaxSize: noMaxSize, showRoles: showRoles, rolesMandatory: rolesMandatory, showMessageGroups: showMessageGroups });
    }

    public showFileUploadEx(title: string, options?: FileUploadOptions): any {
        options = FileUploadOptions.setDefaultOptionsIfNeeded(options, FileUploadOptions.default());

        if (!options.url) {
            options.url = CoreUtility.apiPrefix + `${Constants.WEBAPI_CORE_FILES_UPLOAD}${options.entity ? options.entity : SoeEntityType.None}/${options.imageType ? options.imageType : SoeEntityImageType.Unknown}`;
            if (options.recordId)
                options.url += "/" + options.recordId;
        }

        const modal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getCoreViewUrl("ChildWindows/fileUpload.html"),
            controller: FileUploadController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                translationService: () => { return this.translationService },
                url: () => { return options.url },
                title: () => { return title },
                showDropZone: () => { return options.showDropZone },
                showQueue: () => { return options.showQueue },
                allowMultipleFiles: () => { return options.allowMultipleFiles },
                noMaxSize: () => { return options.noMaxSize },
                showRoles: () => { return options.showRoles },
                rolesMandatory: () => { return options.rolesMandatory },
                showMessageGroups: () => { return options.showMessageGroups },
                showSelect: () => { return options.showSelect },
                selectLabel: () => { return options.selectLabel },
                selectOptions: () => { return options.selectOptions },
                defaultOption: () => { return options.defaultOption },
                showDate: () => { return options.showDate },
                dateLabel: () => { return options.dateLabel },
                defaultDate: () => { return options.defaultDate },
                showCheckBox: () => { return options.showCheckBox },
                checkBoxLabel: () => { return options.checkBoxLabel },
                defaultChecked: () => { return options.defaultChecked }
            }
        });

        if (options.allowMultipleFiles) {
            modal.result = modal.result.then(
                (resolvedData: any) => {
                    if (resolvedData && resolvedData.result && Array.isArray(resolvedData.result)) {
                        resolvedData.result = this.flattenArray(resolvedData.result);
                    }
                    return resolvedData;
                }
            );
        }

        return modal;
    }

    public showValidateWorkRulesResult(action: TermGroup_ShiftHistoryType, result: IEvaluateWorkRulesActionResult, employeeId: number, showCancelAll: boolean = false, dialogTitleKey: string = ''): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        if (result.result.success) {
            if (result.allRulesSucceded) {
                // Success
                deferral.resolve(true);
            } else {
                // Warning
                const keys: string[] = [
                    "time.schedule.planning.evaluateworkrules.warning",
                    "core.continue"
                ];
                if (dialogTitleKey)
                    keys.push(dialogTitleKey);

                this.translationService.translateMany(keys).then(terms => {
                    let text: string = result.errorMessage ? result.errorMessage : '';
                    if (result.canUserOverrideRuleViolation)
                        text += "\n" + terms["core.continue"];

                    const config: IDialogConfig = {};
                    if (showCancelAll)
                        config.showButtonCancelAll = true;

                    const modal = this.showDialogEx(dialogTitleKey ? terms[dialogTitleKey] : terms["time.schedule.planning.evaluateworkrules.warning"], text, SOEMessageBoxImage.Warning, result.canUserOverrideRuleViolation ? SOEMessageBoxButtons.OKCancel : SOEMessageBoxButtons.OK, config);
                    modal.result.then(val => {
                        if (result.canUserOverrideRuleViolation) {
                            _.forEach(_.filter(result.evaluatedRuleResults, r => r.success === false), res => {
                                res.action = action;
                            });

                            // Log override warning
                            if (employeeId)
                                this.coreService.saveEvaluateAllWorkRulesByPass(result, employeeId);
                        }

                        deferral.resolve(result.canUserOverrideRuleViolation);
                    }, (reason) => {
                        deferral.resolve(reason === 'cancelAll' ? null : false);
                    });
                });
            }
        } else {
            // Failure
            this.translationService.translate(dialogTitleKey ? dialogTitleKey : "time.schedule.planning.evaluateworkrules.failed").then(term => {
                this.showDialog(term, result.result.errorMessage, SOEMessageBoxImage.Forbidden, SOEMessageBoxButtons.OK);
                deferral.resolve(false);
            });
        }

        return deferral.promise;
    }

    public showValidateShortenEmploymentResult(result): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        if (result.success) {
            deferral.resolve(true);
        } else {
            // Warning
            var keys: string[] = [
                "core.warning",
                "core.error",
                "time.employee.emloyment.shortenemploymentnotallowed",
                "time.employee.emloyment.shortenemployment",
            ];

            this.translationService.translateMany(keys).then(terms => {
                var invalidChange: boolean = result.booleanValue;
                var message: string = result.errorMessage;
                var title: string = invalidChange ? terms["core.error"] : terms["core.warning"];
                title += ": ";
                title += invalidChange ? terms["time.employee.emloyment.shortenemploymentnotallowed"] : terms["time.employee.emloyment.shortenemployment"];
                var image: SOEMessageBoxImage = invalidChange ? SOEMessageBoxImage.Error : SOEMessageBoxImage.Warning;
                var modal = this.showDialogEx(title, message, image, SOEMessageBoxButtons.OK);

                modal.result.then(val => {
                    deferral.resolve(!invalidChange);
                });
            });
        }

        return deferral.promise;
    }

    public showValidateSaveEmployee(result): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        if (result.success) {
            deferral.resolve(true);
        } else {
            // Warning
            var keys: string[] = [
                "core.warning",
                "core.error",
                "core.continue",
            ];

            this.translationService.translateMany(keys).then(terms => {
                var invalidChange: boolean = result.booleanValue;
                var message: string = invalidChange ? result.errorMessage : result.errorMessage + ". " + terms["core.continue"];
                var title: string = invalidChange ? terms["core.error"] : terms["core.warning"];
                var image: SOEMessageBoxImage = invalidChange ? SOEMessageBoxImage.Error : SOEMessageBoxImage.Warning;
                var button: SOEMessageBoxButtons = invalidChange ? SOEMessageBoxButtons.OK : SOEMessageBoxButtons.YesNo;
                var modal = this.showDialogEx(title, message, image, button);

                if (invalidChange) {
                    deferral.resolve(false);
                }
                else {
                    modal.result.then(val => {
                        deferral.resolve(!!val);
                    }, (reason) => {
                        deferral.resolve(false);
                    });
                }
            });
        }

        return deferral.promise;
    }

    private flattenArray<T>(input: T[]): T[] {
        return input.reduce((acc: T[], item) => {
            if (Array.isArray(item)) {
                // Put items from nested array into flattened array
                return acc.concat(item);
            }
            // Add non-array member to flattened array
            acc.push(item as T);
            return acc;
        }, []);
    }
}

export class FileUploadOptions {
    url?: string;

    showDropZone?: boolean;
    showQueue?: boolean;
    allowMultipleFiles?: boolean;

    noMaxSize?: boolean;
    showRoles?: boolean;
    rolesMandatory?: boolean;
    showMessageGroups?: boolean;

    showSelect?: boolean;
    selectLabel?: string;
    selectOptions?: ISmallGenericType[];
    defaultOption?: number;

    showDate?: boolean;
    dateLabel?: string;
    defaultDate?: Date;

    showCheckBox?: boolean;
    checkBoxLabel?: string;
    defaultChecked?: boolean;

    entity?: SoeEntityType;
    imageType?: SoeEntityImageType;
    recordId?: number;

    public static default(): FileUploadOptions {
        let options = new FileUploadOptions();

        options.showDropZone = true;
        options.showQueue = true;
        options.allowMultipleFiles = true;

        return options;
    }

    public static setDefaultOptionsIfNeeded<T extends FileUploadOptions>(options: T, defaultOptions: T): T {
        return _.assignWith<T, T>(options, defaultOptions, (destinationValue, srcValue) => _.isUndefined(destinationValue) ? srcValue : destinationValue);
    }
}