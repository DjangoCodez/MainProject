import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SmallGenericType } from "../../Models/SmallGenericType";
import { Feature, CompanySettingType, TermGroup_EmployeeRequestType, SoeEntityState } from "../../../Util/CommonEnumerations";
import { ICoreService } from "../../../Core/Services/CoreService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { EmployeeRequestDTO } from "../../Models/EmployeeRequestDTO";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { IValidationSummaryHandler } from "../../../Core/Handlers/ValidationSummaryHandler";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { EmployeeListDTO } from "../../Models/EmployeeListDTO";

export class EditEmployeeAvailabilityDialogController {

    // Terms
    private terms: { [index: string]: string; };

    // Permissions
    private availablePermission: boolean = false;
    private notAvailablePermission: boolean = false;

    // Company settings
    private lockDaysBefore: number;

    // Data
    private intervals: SmallGenericType[] = [];
    private selectedInterval: EmployeeRequestInterval;
    private requests: EmployeeRequestDTO[] = [];

    // Flags
    private loading: boolean = false;
    private sameCommentForAllRequests: boolean = false;
    private comment: string;

    private validationHandler: IValidationSummaryHandler;
    private dialogform: ng.IFormController;
    private today: Date = CalendarUtility.getDateToday().beginningOfDay();
    private yesterday: Date = CalendarUtility.getDateToday().beginningOfDay().addDays(-1);

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        private readOnly: boolean,
        private employeeId: number,
        private dateFrom: Date,
        private dateTo: Date,
        private date: Date,
        private employeeInfo: EmployeeListDTO,
        private commentMandatory: boolean) {

        this.loading = true;

        this.validationHandler = validationSummaryHandlerFactory.create();

        this.$q.all([
            this.loadTerms(),
            this.loadModifyPermissions(),
            this.loadCompanySettings()
        ]).then(() => {
            this.loadEmployeeRequests().then(() => {
                this.loading = false;
            });
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.comment",
            "common.created",
            "common.modified",
            "common.by",
            "common.dashboard.myschedule.availability.addcomment",
            "common.dashboard.myschedule.availability.interval.partofday",
            "common.dashboard.myschedule.availability.interval.wholeday",
            "common.dashboard.myschedule.availability.interval.period",
            "common.dashboard.myschedule.availability.saveerror"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Time_Schedule_AvailabilityUser_Available);
        featureIds.push(Feature.Time_Schedule_AvailabilityUser_NotAvailable);

        return this.coreService.hasModifyPermissions(featureIds).then((x) => {
            this.availablePermission = x[Feature.Time_Schedule_AvailabilityUser_Available];
            this.notAvailablePermission = x[Feature.Time_Schedule_AvailabilityUser_NotAvailable];
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimeAvailabilityLockDaysBefore);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.lockDaysBefore = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeAvailabilityLockDaysBefore);
        });
    }

    private loadEmployeeRequests(): ng.IPromise<any> {
        return this.coreService.getEmployeeRequests(this.employeeId, this.dateFrom, this.dateTo).then(x => {
            this.requests = x;

            if (this.requests.length > 0) {
                let distinctDates: Date[] = [];
                _.forEach(this.requests, req => {
                    if (!CalendarUtility.includesDate(distinctDates, req.start.beginningOfDay()))
                        distinctDates.push(req.start.beginningOfDay());
                    if (!CalendarUtility.includesDate(distinctDates, req.stop.beginningOfDay()))
                        distinctDates.push(req.stop.beginningOfDay());
                });

                if (distinctDates.length > 1) {
                    this.date = null;
                    this.dateFrom = _.min(distinctDates);
                    this.dateTo = _.max(distinctDates);
                }
            }

            if (!this.date) {
                this.selectedInterval = EmployeeRequestInterval.Period;
            } else {
                if (this.requests.length > 0) {
                    this.selectedInterval = _.filter(this.requests, r => !r.start.isBeginningOfDay()).length > 0 ? EmployeeRequestInterval.PartOfDay : EmployeeRequestInterval.WholeDay;
                } else {
                    this.selectedInterval = EmployeeRequestInterval.WholeDay;
                }
            }

            // Add new whole day request
            if (this.requests.length === 0 && this.selectedInterval === EmployeeRequestInterval.WholeDay)
                this.addRequest();

            this.sameCommentForAllRequests = this.requests.length < 2 || _.uniqBy(this.requests, r => r.comment).length === 1;
            if (this.sameCommentForAllRequests && this.requests.length > 0)
                this.comment = this.requests[0].comment;
        });
    }

    // EVENTS

    private addRequest() {
        var request: EmployeeRequestDTO = new EmployeeRequestDTO();
        request.state = SoeEntityState.Active;
        request.type = TermGroup_EmployeeRequestType.InterestRequest;
        if (this.date) {
            if (this.selectedInterval === EmployeeRequestInterval.WholeDay) {
                request.start = this.date.beginningOfDay();
                request.stop = this.date.endOfDay();
            } else if (this.selectedInterval === EmployeeRequestInterval.PartOfDay) {
                request.start = request.stop = this.date.beginningOfDay();

                if (this.requests.length > 0) {
                    let last = _.last(_.orderBy(this.requests, ['start', 'stop']));
                    request.start = request.stop = request.start.mergeTime(last.stop);
                }
            }
        } else {
            if (this.requests.length > 0) {
                let last = _.last(_.orderBy(this.requests, ['start', 'stop']));
                request.start = last.stop.addDays(1).beginningOfDay();
                request.stop = request.start.endOfDay();
            } else {
                request.start = this.dateFrom ? this.dateFrom : CalendarUtility.getDateToday().beginningOfDay();
                request.stop = this.dateTo ? this.dateTo.endOfDay() : CalendarUtility.getDateToday().endOfDay();
            }
        }
        this.requests.push(request);
    }

    private commentChanged() {
        this.$timeout(() => {
            _.forEach(this.requests, req => {
                req.comment = this.comment;
            });
        });
    }

    private editRequestComment(request: EmployeeRequestDTO) {
        if (this.readOnly) {
            this.notificationService.showDialogEx(this.terms["common.comment"], request.comment, SOEMessageBoxImage.Information);
        } else {
            let modal = this.notificationService.showDialogEx(this.terms["common.dashboard.myschedule.availability.addcomment"], "", SOEMessageBoxImage.None, SOEMessageBoxButtons.OKCancel, { showTextBox: true, textBoxValue: request.comment });
            modal.result.then(val => {
                modal.result.then(result => {
                    if (result.result) {
                        request.comment = result.textBoxValue;
                    }
                });
            });
        }
    }

    private showRequestInfo(request: EmployeeRequestDTO) {
        let msg: string = '';
        if (request.created) {
            msg += "{0} {1}".format(this.terms["common.created"], request.created.toFormattedDateTime());
            if(request.createdBy)
                msg += " {0} {1}".format(this.terms["common.by"], request.createdBy);
            msg += "\n";
        }
        if (request.modified) {
            msg += "{0} {1}".format(this.terms["common.modified"], request.modified.toFormattedDateTime());
            if (request.modifiedBy)
                msg += " {0} {1}".format(this.terms["common.by"], request.modifiedBy);
        }
        this.notificationService.showDialogEx(this.terms["core.info"], msg, SOEMessageBoxImage.Information);
    }

    private deleteRequest(request: EmployeeRequestDTO) {
        if (request.employeeRequestId)
            request.state = SoeEntityState.Deleted;
        else
            _.pull(this.requests, request);
    }

    private toggleType(request: EmployeeRequestDTO) {
        if (this.readOnly || (request.start < this.today && request.employeeRequestId != undefined) )
            return;

        if (request.type === TermGroup_EmployeeRequestType.InterestRequest)
            request.type = TermGroup_EmployeeRequestType.NonInterestRequest;
        else
            request.type = TermGroup_EmployeeRequestType.InterestRequest;
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private ok() {
        this.setTimesForSave();

        this.coreService.saveEmployeeRequests(this.employeeId, _.filter(this.requests, r => r.state === SoeEntityState.Deleted && r.start >= this.today), _.filter(this.requests, r => r.state === SoeEntityState.Active && (r.employeeRequestId !== undefined ? r.stop >= this.yesterday : r.start >= this.today && r.stop >= this.today))).then(result => {
            if (result.success)
                this.$uibModalInstance.close({ succeess: true });
            else {
                this.notificationService.showDialogEx(this.terms["common.dashboard.myschedule.availability.saveerror"], result.errorMessage, SOEMessageBoxImage.Error);
                this.resetTimesForSave();
            }
        });
    }

    // HELP-METHODS

    private get filteredRequests(): EmployeeRequestDTO[] {
        return _.filter(this.requests, r => r.state === SoeEntityState.Active);
    }

    private get panelLabel(): string {
        switch (this.selectedInterval) {
            case EmployeeRequestInterval.WholeDay:
                return this.terms["common.dashboard.myschedule.availability.interval.wholeday"];
            case EmployeeRequestInterval.PartOfDay:
                return this.terms["common.dashboard.myschedule.availability.interval.partofday"];
            case EmployeeRequestInterval.Period:
                return this.terms["common.dashboard.myschedule.availability.interval.period"];
        }

        return '';
    }

    private getRequestCommentTooltip(request: EmployeeRequestDTO): string {
        if (!this.terms)
            return '';

        return request.comment ? request.comment : this.terms["common.dashboard.myschedule.availability.addcomment"];
    }

    private setTimesForSave() {
        // Convert times to whole day
        if (this.selectedInterval !== EmployeeRequestInterval.PartOfDay) {
            _.forEach(this.requests, request => {
                request['originalStart'] = request.start;
                request.start = request.start.beginningOfDay();
                request['originalStop'] = request.stop;
                request.stop = request.stop.endOfDay();
            });
        }
    }

    private resetTimesForSave() {
        // Convert times to whole day
        if (this.selectedInterval !== EmployeeRequestInterval.PartOfDay) {
            _.forEach(this.requests, request => {
                if (request['originalStart'])
                    request.start = request['originalStart'];
                if (request['originalStop'])
                    request.stop = request['originalStop'];
            });
        }
    }


    private showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            var errors = this['dialogform'].$error;

            if (errors['requestComment'])
                mandatoryFieldKeys.push("common.comment");
        });
    }
}

export enum EmployeeRequestInterval {
    WholeDay = 1,
    PartOfDay = 2,
    Period = 3
}
