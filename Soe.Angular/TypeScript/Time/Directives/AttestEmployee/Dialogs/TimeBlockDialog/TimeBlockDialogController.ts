import { AttestEmployeeDayTimeBlockDTO } from "../../../../../Common/Models/TimeEmployeeTreeDTO";
import { TimeDeviationCauseGridDTO } from "../../../../../Common/Models/TimeDeviationCauseDTOs";
import { PayrollImportEmployeeTransactionDTO } from "../../../../../Common/Models/PayrollImport";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../../../Util/Enumerations";
import { IPayrollService } from "../../../../Payroll/PayrollService";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { AccountingSettingsRowDTO } from "../../../../../Common/Models/AccountingSettingsRowDTO";
import { SmallGenericType } from "../../../../../Common/Models/SmallGenericType";
import { Feature, TimeAttestMode } from "../../../../../Util/CommonEnumerations";
import { ICoreService } from "../../../../../Core/Services/CoreService";


export class TimeBlockDialogController {

    // Init parameters
    private timeBlock: AttestEmployeeDayTimeBlockDTO;
    private isNew: boolean;

    private settings: AccountingSettingsRowDTO[];
    private settingTypes: SmallGenericType[] = [];
    private terms: { [index: string]: string; };
    private showStdAccount = false;
    private progress: IProgressHandler;
    private accountSetting: AccountingSettingsRowDTO;

    // Flags
    private showDates: boolean = false;

    private dateOptions = {
        minDate: this.date,
        maxDate: this.date.addDays(1),
    };

    private _selectedDeviationCause: TimeDeviationCauseGridDTO;
    private readPermission: boolean = false;
    private editPermission: boolean = false;
 
    get selectedDeviationCause() {
        return this._selectedDeviationCause;
    }
    set selectedDeviationCause(item: TimeDeviationCauseGridDTO) {

        this._selectedDeviationCause = item;
        if (this.selectedDeviationCause)
            this.timeBlock.timeDeviationCauseStartId = this.selectedDeviationCause.timeDeviationCauseId;
        else
            this.timeBlock.timeDeviationCauseStartId = 0;
    }
    private isMyTime: TimeAttestMode;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $timeout: ng.ITimeoutService,
        progressHandlerFactory: IProgressHandlerFactory,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private payrollService: IPayrollService,
        private deviationCauses: TimeDeviationCauseGridDTO[],
        timeBlock: AttestEmployeeDayTimeBlockDTO,
        private coreService: ICoreService,
        private date: Date,
        private changingStartTime: boolean,
        private changingStopTime: boolean,
        private trans: PayrollImportEmployeeTransactionDTO,
        accountSetting: AccountingSettingsRowDTO ) {

        if (progressHandlerFactory)
            this.progress = progressHandlerFactory.create();

        this.isNew = !timeBlock;
    
        this.timeBlock = new AttestEmployeeDayTimeBlockDTO();
        angular.extend(this.timeBlock, timeBlock);
        this.loadTerms().then(() => {
            this.isMyTime = soeConfig.attestMode ? soeConfig.attestMode : TimeAttestMode.Time;
            this.loadReadPermissions();
            this.loadModifyPermissions();
            this.settingTypes.push(new SmallGenericType(0, this.terms["common.accountingsettings.account"]));
            this.init();
        });
    }
    private loadReadPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Time_Time_Attest_SpecifyAccountingOnDeviations);
        featureIds.push(Feature.Time_Time_AttestUser_SpecifyAccountingOnDeviations);
        return this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            if (this.isMyTime == TimeAttestMode.Time) {
                this.readPermission = x[Feature.Time_Time_Attest_SpecifyAccountingOnDeviations];
            } else {
                this.readPermission = x[Feature.Time_Time_AttestUser_SpecifyAccountingOnDeviations];
            }
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Time_Time_Attest_SpecifyAccountingOnDeviations);
        featureIds.push(Feature.Time_Time_AttestUser_SpecifyAccountingOnDeviations);
        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            if (this.isMyTime == TimeAttestMode.Time) {
                this.editPermission = x[Feature.Time_Time_Attest_SpecifyAccountingOnDeviations];
            } else {
                this.editPermission = x[Feature.Time_Time_AttestUser_SpecifyAccountingOnDeviations];
            }
        });
    }
    private init() {
        if (this.isNew) {
            this.timeBlock.startTime = this.timeBlock.stopTime = this.date;
        } else if (this.changingStartTime && this.timeBlock.startTimeDuringMove) {
            this.timeBlock.startTime = this.timeBlock.startTimeDuringMove;
        } else if (this.changingStopTime && this.timeBlock.stopTimeDuringMove) {
            this.timeBlock.stopTime = this.timeBlock.stopTimeDuringMove;
        }
       
        if (this.timeBlock.startTime.getDate() != this.timeBlock.stopTime.getDate())
            this.showDates = true;

        if (this.trans) {
            this.showDates = true;
            if (this.trans.startTime)
                this.timeBlock.startTime = this.trans.startTime;
            if (this.trans.stopTime)
                this.timeBlock.stopTime = this.trans.stopTime;
            if (this.trans.timeDeviationCauseId && _.find(this.deviationCauses, e => e.timeDeviationCauseId === this.trans.timeDeviationCauseId))
                this.timeBlock.timeDeviationCauseStartId = this.trans.timeDeviationCauseId;
            if (this.trans.note)
                this.timeBlock.comment = this.trans.note;
        }

        if (this.timeBlock.deviationAccounts.length > 0) {
            let row = new AccountingSettingsRowDTO(0);
            this.settings = [];

            for (var i = 0; i < 5; i++) {
                row[`account${i + 1}Id`] = this.timeBlock.deviationAccounts[i]?.accountId ?? 0;
                row[`account${i + 1}Nr`] = this.timeBlock.deviationAccounts[i]?.accountNr ?? '';
                row[`account${i + 1}Name`] = this.timeBlock.deviationAccounts[i]?.name ?? '';
                row[`accountDim${i + 1}Nr`] = this.timeBlock.deviationAccounts[i]?.accountDimNr ?? 0;
            }
            this.settings.push(row);
            this.accountingChanged();
        }

        this._selectedDeviationCause = _.find(this.deviationCauses, e => e.timeDeviationCauseId === this.timeBlock.timeDeviationCauseStartId);
    }
    private loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.accountingsettings.account",
        ];

        return this.translationService.translateMany(keys).then(x => {
            this.terms = x;
        });
    }

    // EVENTS

    private startDateChanged() {
        // Remember times (changing date will reset time to 00:00)
        var startTime = this.timeBlock.startTime;

        this.$timeout(() => {
            // Limit date range from today to tomorrow
            if (this.timeBlock.startTime.isBeforeOnDay(this.date))
                this.timeBlock.startTime = this.date;
            if (this.timeBlock.startTime.isAfterOnDay(this.date.addDays(1)))
                this.timeBlock.startTime = this.date.addDays(1);

            // Restore times
            this.timeBlock.startTime = this.timeBlock.startTime.mergeTime(startTime);
        });
    }

    private stopDateChanged() {
        // Remember times (changing date will reset time to 00:00)
        var stopTime = this.timeBlock.stopTime;

        this.$timeout(() => {
            // Limit date range from today to tomorrow
            if (this.timeBlock.stopTime.isBeforeOnDay(this.date))
                this.timeBlock.stopTime = this.date;
            if (this.timeBlock.stopTime.isAfterOnDay(this.date.addDays(1)))
                this.timeBlock.stopTime = this.date.addDays(1);

            // Restore times
            this.timeBlock.stopTime = this.timeBlock.stopTime.mergeTime(stopTime);
        });
    }

    private setAsProcessed() {
        let keys: string[] = [
            "time.time.attest.timeblocks.setasprocessed",
            "time.time.attest.timeblocks.setasprocessed.info"
        ];

        this.translationService.translateMany(keys).then(terms => {
            let modal = this.notificationService.showDialogEx(terms["time.time.attest.timeblocks.setasprocessed"], terms["time.time.attest.timeblocks.setasprocessed.info"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.progress.startSaveProgress((completion) => {
                        this.payrollService.setPayrollImportEmployeeTransactionAsProcessed(this.trans.payrollImportEmployeeTransactionId).then(result => {
                            if (result.success) {
                                completion.completed(null, null, true);
                            } else {
                                completion.failed(result.errorMessage);
                            }
                        }, error => {
                            completion.failed(error.message);
                        });
                    }, null).then(data => {
                        this.$uibModalInstance.close({ reload: true });
                    });
                }
            }, (reason) => {
                // User cancelled
            });
        });
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        this.$uibModalInstance.close({ timeBlock: this.timeBlock, trans: this.trans, accountSetting: this.accountSetting });
    }

    private isNoteRequired(): boolean {
        if (!this.timeBlock)
            return false;

        let deviationCause = _.find(this.deviationCauses, a => a.timeDeviationCauseId == this.timeBlock.timeDeviationCauseStartId);

        return (deviationCause && deviationCause.mandatoryNote === true)
    }

    private isNoteInvalid(): boolean {
        return (this.isNoteRequired() && (!this.timeBlock.comment || this.timeBlock.comment.length == 0))
    }

    private get isLengthSameAsTrans(): boolean {
        return this.timeBlock.getBlockLength() === Math.floor(this.trans.quantity);
    }

    private accountingChanged() {
        this.accountSetting = this.settings[0];
    }

}
