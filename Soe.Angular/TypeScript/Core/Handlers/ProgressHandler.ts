import { IUrlHelperService } from "../Services/UrlHelperService";
import { ITranslationService } from "../Services/TranslationService";
import { IMessagingService } from "../Services/MessagingService";
import { INotificationService } from "../Services/NotificationService";
import { Guid } from "../../Util/StringUtility";
import { ProgressController } from "../Dialogs/Progress/ProgressController";
import { Constants } from "../../Util/Constants";

export interface IProgressHandler {
    startLoadingProgress(tasks: (() => ng.IPromise<any>)[], message?: string): ng.IPromise<any>;
    startSaveProgress(task: (completion: SaveProgressCompletion) => void, guid: Guid, onModalClosed?: () => void): ng.IPromise<any>;
    startDeleteProgress(task: (completion: DeleteProgressCompletion) => void, onModalClosed?: () => void, confirmMessage?: string): ng.IPromise<any>;
    startWorkProgress(task: (completion: WorkProgressCompletion) => void, onModalClosed?: () => void, text?: string): ng.IPromise<any>;
    showProgressDialog(text?: string);
    hideProgressDialog();
    updateProgressDialogMessage(message: string);
    setProgressBusy(busy: boolean);
}

export class ProgressHandler implements IProgressHandler {
    private _outstandingTasks: number;

    constructor(private $uibModal, private translationService: ITranslationService, private $q: ng.IQService,
        private messagingService: IMessagingService, private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService) {
    }

    private openModalProgress(progressModalMetaData: any, onModalClosed?: () => void): any {
        var progressModal = this.$uibModal.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Core/Dialogs/Progress/Views/Progress.html"),
            controller: ProgressController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                metadata: () => progressModalMetaData,
                progressParent: () => this
            }
        });

        if (onModalClosed) {
            progressModal.closed.then(() => {
                onModalClosed();
            });
        }
        this.progressModalBusy = true;
        return progressModal;
    }

    public startLoadingProgress(tasks: (() => ng.IPromise<any>)[], message?: string) {
        let deferred = this.$q.defer();
        this.translationService.translate('core.loading').then((term) => {
            this.progressMessage = message ? message : term;

            var progressModal = this.openModalProgress({ icon: 'far fa-spinner fa-pulse', text: this.progressMessage });

            if (tasks) {
                this._outstandingTasks = tasks.length;
                for (let i = 0; i < tasks.length; i++) {
                    tasks[i]().then(result => {
                    }, error => {
                    }).finally(() => {
                        this._outstandingTasks--;
                        if (this._outstandingTasks <= 0) {
                            if (progressModal) {
                                this.progressModalBusy = false;
                                progressModal.close();
                            }
                            deferred.resolve();
                        }
                    });
                }
            } else {
                deferred.resolve();
            }
        });

        return deferred.promise;
    }

    public startSaveProgress(task: (completion: SaveProgressCompletion) => void, guid: Guid, onModalClosed?: () => void): ng.IPromise<any> {
        let deferred = this.$q.defer();

        var keys: string[] = [
            "core.saving",
            "core.saved",
            'core.savefailed'
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.progressMessage = terms["core.saving"];

            let progressModalMetaData: any = { icon: 'far fa-spinner fa-pulse fa-fw', text: terms["core.saving"] };
            let progressModal = this.openModalProgress(progressModalMetaData, onModalClosed);


            task(new SaveProgressCompletion((event, data, skipDialog, message) => {
                if (skipDialog) {
                    this.progressBusy = false;
                    if (event)
                        this.messagingService.publish(event, { guid: guid, data: data });

                    progressModal.close();

                    deferred.resolve(data);
                } else {
                    progressModalMetaData.icon = 'fa-check-circle okColor';
                    progressModalMetaData.showclose = true;
                    if (message) {
                        progressModalMetaData.text = message;
                    } else {
                        progressModalMetaData.text = terms["core.saved"];
                    }

                    progressModal.closed.then(() => {
                        this.progressBusy = false;
                        if (event)
                            this.messagingService.publish(event, { guid: guid, data: data });
                        deferred.resolve(data);
                    });
                }
                deferred.resolve(data);
            }, (errorMessage, skipDialog) => {
                if (skipDialog) {
                    this.progressBusy = false;
                    this.progressModalBusy = false;
                    progressModal.close();
                } else {
                    progressModalMetaData.icon = 'fa-exclamation-triangle errorColor';
                    progressModalMetaData.showclose = true;

                    if (errorMessage === "") {
                        progressModalMetaData.text = terms["core.savefailed"];
                    } else {
                        progressModalMetaData.text = errorMessage;
                    }
                    progressModal.closed.then(() => {
                        this.progressBusy = false;
                        deferred.reject(errorMessage);
                    });
                }
            }));
        });

        return deferred.promise;
    }

    public startDeleteProgress(task: (completion: DeleteProgressCompletion) => void, onModalClosed?: () => void, confirmMessage?: string): ng.IPromise<any> {
        let deferred = this.$q.defer();

        var keys: string[] = [
            'core.deleting',
            'core.deleted',
            'core.deletefailed'
        ];

        this.notificationService.showConfirmOnDelete(confirmMessage).then((val: boolean) => {
            if (!val) {
                deferred.reject(null);
                return;
            }

            this.translationService.translateMany(keys).then((terms) => {

                let progressModalMetaData: any = { icon: 'far fa-spinner fa-pulse fa-fw', text: terms['core.deleting'] };
                let progressModal = this.openModalProgress(progressModalMetaData, onModalClosed);

                task(new DeleteProgressCompletion((data, skipDialog, message) => {
                    if (skipDialog) {
                        this.progressBusy = false;
                        //this.messagingService.publish(Constants.EVENT_EDIT_DELETED, data);

                        progressModal.close();

                        deferred.resolve(data);
                    } else {
                        progressModalMetaData.icon = 'fa-check-circle okColor';
                        progressModalMetaData.showclose = true;
                        if (message) {
                            progressModalMetaData.text = message;
                        } else {
                            progressModalMetaData.text = terms["core.deleted"];
                        }
                        progressModal.closed.then(() => {
                            this.progressModalBusy = false;
                            this.progressBusy = false;
                            this.messagingService.publish(Constants.EVENT_EDIT_DELETED, data);
                            deferred.resolve(data);
                        });
                    }
                }, (errorMessage, skipDialog) => {
                    if (skipDialog) {
                        this.progressBusy = false;
                        this.progressModalBusy = false;
                        progressModal.close();
                    } else {
                        progressModalMetaData.icon = 'fa-exclamation-triangle errorColor';
                        progressModalMetaData.showclose = true;

                        if (errorMessage === "") {
                            progressModalMetaData.text = terms["core.deletefailed"];
                        } else {
                            progressModalMetaData.text = errorMessage;
                        }
                        progressModal.closed.then(() => {
                            this.progressBusy = false;
                            this.progressModalBusy = false;
                            deferred.reject(errorMessage);
                        });
                    }
                }));
            });
        });

        return deferred.promise;
    }

    public startWorkProgress(task: (completion: WorkProgressCompletion) => void, onModalClosed?: () => void, text?: string): ng.IPromise<any> {
        let deferred = this.$q.defer();

        var keys: string[] = [
            "core.working",
            "core.workfailed",
            "core.worked"
        ];
        this.translationService.translateMany(keys).then(terms => {
            let progressModalMetaData: any = { icon: 'far fa-spinner fa-pulse fa-fw', text: text ? text : terms["core.working"] };
            let progressModal = this.openModalProgress(progressModalMetaData, onModalClosed);

            task(new WorkProgressCompletion((data, skipDialog, message) => {
                this.messagingService.publish(Constants.EVENT_EDIT_WORKED, data);
                if (skipDialog) {
                    this.progressBusy = false;
                    this.progressModalBusy = false;
                    progressModal.close();
                } else {
                    progressModalMetaData.icon = 'fa-check-circle okColor';
                    progressModalMetaData.showclose = true;
                    if (message) {
                        progressModalMetaData.text = message;
                    } else {
                        progressModalMetaData.text = terms["core.worked"];
                    }
                    this.progressBusy = false;
                }
                deferred.resolve(data);
            }, (errorMessage, skipDialog) => {
                if (skipDialog) {
                    this.progressBusy = false;
                    this.progressModalBusy = false;
                    progressModal.close();
                } else {
                    progressModalMetaData.icon = 'fa-exclamation-triangle errorColor';
                    progressModalMetaData.showclose = true;

                    if (errorMessage === "") {
                        progressModalMetaData.text = terms["core.workfailed"];
                    } else {
                        progressModalMetaData.text = errorMessage;
                    }
                    progressModal.closed.then(() => {
                        this.progressBusy = false;
                        this.progressModalBusy = false;
                        deferred.reject(errorMessage);
                    });
                }
            }));
        });

        return deferred.promise;
    }

    showProgressDialog(text: string) {
        this.progressModalMetaData = { icon: 'far fa-spinner fa-pulse fa-fw', text: text };
        this.progressModal = this.openModalProgress(this.progressModalMetaData, null);
    }

    hideProgressDialog() {
        this.progressBusy = false;
        this.progressModal.close();
    }

    updateProgressDialogMessage(message: string) {
        if (this.progressModal && this.progressModalMetaData)
            this.progressModalMetaData.text = message;
    }

    setProgressBusy(busy: boolean) {
        this.progressBusy = busy;
    }

    private progressModal: any;
    private progressModalMetaData: any;
    public progressMessage: string;
    public progressBusy: boolean;
    public progressModalBusy: boolean;
    public get outstandingTasks() {
        return this._outstandingTasks;
    }
}

export class SaveProgressCompletion {
    constructor(private completedCallback: (completeEvent?: string, data?: any, skipDialog?: boolean, message?: string) => void, private failedCallback: (errorMessage: string, skipDialog?: boolean) => void) { }

    completed(completeEvent?: string, data?: any, skipDialog: boolean = true, message?: string) {
        this.completedCallback(completeEvent, data, skipDialog, message);
    }
    failed(errorMessage: string, skipDialog?: boolean) {
        this.failedCallback(errorMessage, skipDialog);
    }
}

export class DeleteProgressCompletion {
    constructor(private completedCallback: (data: any, skipDialog: boolean, message?: string) => void, private failedCallback: (errorMessage: string, skipDialog?: boolean) => void) { }

    completed(data: any, skipDialog?: boolean, message?: string) {
        this.completedCallback(data, skipDialog, message);
    }
    failed(errorMessage: string, skipDialog?: boolean) {
        this.failedCallback(errorMessage, skipDialog);
    }
}

export class WorkProgressCompletion {
    constructor(private completedCallback: (data: any, skipDialog: boolean, message?: string) => void, private failedCallback: (errorMessage: string, skipDialog?: boolean) => void) { }

    completed(data: any, skipDialog?: boolean, message?: string) {
        this.completedCallback(data, skipDialog, message);
    }
    failed(errorMessage: string, skipDialog?: boolean) {
        this.failedCallback(errorMessage, skipDialog);
    }
}