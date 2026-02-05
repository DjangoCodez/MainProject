import { ITranslationService } from "../../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../../Core/Services/NotificationService";
import { ICoreService } from "../../../../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../../../../Core/Services/UrlHelperService";
import { ITimeService as ISharedTimeService } from "../../../TimeService";
import { IActionResult } from "../../../../../../Scripts/TypeLite.Net4";
import { IProgressHandlerFactory } from "../../../../../../Core/Handlers/progresshandlerfactory";
import { IProgressHandler } from "../../../../../../Core/Handlers/ProgressHandler";

export class AttestReminderDialogController {

    public progress: IProgressHandler;

    // Terms
    private terms: { [index: string]: string; };

    // Company settings

    // Data
    
    // Properties
    private sendAttestReminderToEmployee: boolean;
    private sendAttestReminderToExecutive: boolean;

    // Flags
    
    //@ngInject
    constructor(
        private $uibModal,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $q: ng.IQService,
        progressHandlerFactory: IProgressHandlerFactory,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private sharedTimeService: ISharedTimeService,
        private employeeIds: number[],
        private dateFrom: Date,
        private dateTo: Date) {
        
        this.progress = progressHandlerFactory.create();
        
        this.$q.all([
            this.loadTerms(),
            
        ]).then(() => {
            this.dataLoaded();  
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.errormessage",           
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            
        });
    }

    
    // EVENTS

    private ok() {

        this.progress.startSaveProgress((completion) => {

            this.sharedTimeService.sendAttestReminder(this.employeeIds, this.dateFrom, this.dateTo, this.sendAttestReminderToEmployee, this.sendAttestReminderToExecutive).then((result: IActionResult) => {
                if (result.success) {                    
                    completion.completed(null, true);
                    this.$uibModalInstance.close();
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null);
    }

    private cancel() {
        this.$uibModalInstance.dismiss('cancel');
    }

    // HELP-METHODS

    private dataLoaded() {

    }


}
