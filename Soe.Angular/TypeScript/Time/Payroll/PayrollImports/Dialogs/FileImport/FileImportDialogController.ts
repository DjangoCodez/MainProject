import { IReportDataService } from "../../../../../Core/RightMenu/ReportMenu/ReportDataService";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { FileUploadOptions, INotificationService } from "../../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { CalendarUtility } from "../../../../../Util/CalendarUtility";
import { TermGroup, TermGroup_PayrollImportHeadFileType } from "../../../../../Util/CommonEnumerations";
import { Constants } from "../../../../../Util/Constants";
import { CoreUtility } from "../../../../../Util/CoreUtility";

export class FileImportDialogController {

    // Data
    private importTypes: ISmallGenericType[];
    private paymentDates: any[] = [];

    // Options
    private importType: number;
    private paymentDate: Date;
    private comment: string;
    private skipMissingEmployeeValidation: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private coreService: ICoreService,
        private reportDataService: IReportDataService,
        private translationService: ITranslationService,
        private notificationService: INotificationService) {
    }

    $onInit() {
        this.loadImportTypes();
        this.loadPeriods();
    }

    private loadImportTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.PayrollImportHeadFileType, false, true).then(x => {
            this.importTypes = x;
            this.importType = _.find(this.importTypes, t => t.id == TermGroup_PayrollImportHeadFileType.SoftOneClassic).id;
        });
    }

    private loadPeriods() {
        this.reportDataService.getAllPayrollTimePeriods().then(x => {
            // Create a list of distinct dates for the dropdown
            _.forEach(x, y => {
                if (!CalendarUtility.includesDate(_.map(this.paymentDates, p => p.date), y.paymentDate))
                    this.paymentDates.push({ date: y.paymentDate, label: y.paymentDate.toFormattedDate() });
            });
        });
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private selectFile() {
        let cmt: string = this.comment;
        if (!cmt || cmt === 'undefined')
            cmt = Constants.WEBAPI_STRING_EMPTY;

        let options: FileUploadOptions = {
            url: CoreUtility.apiPrefix + Constants.WEBAPI_TIME_PAYROLL_PAYROLL_IMPORT_HEAD_IMPORT + this.importType + "/" + this.paymentDate.toDateTimeString() + "/" + cmt + "/" + this.skipMissingEmployeeValidation,
            allowMultipleFiles: false,
        };

        let modal = this.notificationService.showFileUploadEx(this.translationService.translateInstant("time.payroll.payrollimport.importfile"), options);
        modal.result.then(importResult => {
            this.$uibModalInstance.close(
                {
                    options: { importType: this.importType, paymentDate: this.paymentDate },
                    result: importResult.result,
                });
        }, (reason) => {
            // User closed file dialog
        });
    }
}
