import { FileUploadOptions, INotificationService } from "../../../../../Core/Services/NotificationService";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { Constants } from "../../../../../Util/Constants";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { PayrollStartValueHeadDTO } from "../../../../../Common/Models/PayrollImport";
import { PayrollStartValueUpdateType } from "../../../../../Util/CommonEnumerations";

export class ImportFileDialogController {

    // Terms
    private insertInfo: string;
    private overwriteInfo: string;

    // Options
    private selectedUpdateType: PayrollStartValueUpdateType = PayrollStartValueUpdateType.Insert;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private payrollStartValueHead: PayrollStartValueHeadDTO,
        private isNew: boolean) {
    }

    $onInit() {
        this.loadTerms();
    }

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "time.import.payrollstartvalue.insert.info",
            "time.import.payrollstartvalue.overwrite.info",
        ];
        return this.translationService.translateMany(keys).then(terms => {
            this.insertInfo = terms["time.import.payrollstartvalue.insert.info"];
            this.overwriteInfo = terms["time.import.payrollstartvalue.overwrite.info"];
        });
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private selectFile() {
        let url = CoreUtility.apiPrefix;
        if (this.isNew)
            url += Constants.WEBAPI_TIME_PAYROLL_START_VALUE_HEAD_IMPORT_ADD + this.payrollStartValueHead.dateFrom.toDateTimeString() + "/" + this.payrollStartValueHead.dateTo.toDateTimeString() + "/" + this.payrollStartValueHead.importedFrom;
        else
            url += Constants.WEBAPI_TIME_PAYROLL_START_VALUE_HEAD_IMPORT_UPDATE + this.payrollStartValueHead.payrollStartValueHeadId + "/" + this.selectedUpdateType

        let options: FileUploadOptions = {
            url: url,
            allowMultipleFiles: false,
        };

        let modal = this.notificationService.showFileUploadEx(this.translationService.translateInstant("time.import.payrollstartvalue.new"), options);
        modal.result.then(result => {
            this.$uibModalInstance.close(result);
        }, (reason) => {
            // User closed file dialog
        });
    }
}
