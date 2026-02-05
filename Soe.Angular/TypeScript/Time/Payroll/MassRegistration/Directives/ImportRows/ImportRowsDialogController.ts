import { ICoreService } from "../../../../../Core/Services/CoreService";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { TermGroup, TermGroup_MassRegistrationImportType } from "../../../../../Util/CommonEnumerations";
import { FileUploadOptions, INotificationService } from "../../../../../Core/Services/NotificationService";
import { CoreUtility } from "../../../../../Util/CoreUtility";
import { Constants } from "../../../../../Util/Constants";
import { MassRegistrationTemplateRowDTO } from "../../../../../Common/Models/MassRegistrationDTOs";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";

export class ImportRowsDialogController {

    // Data
    private importTypes: ISmallGenericType[];

    // Options
    private importType: number;
    private paymentDate: Date;
    private clearRows: boolean = false;

    //@ngInject
    constructor(
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private paymentDates: any[]) {
    }

    $onInit() {
        this.loadImportTypes();
    }

    private loadImportTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.MassRegistrationImportType, false, true).then(x => {
            this.importTypes = x;
            this.importType = _.find(this.importTypes, t => t.id == TermGroup_MassRegistrationImportType.Excel).id;
        });
    }

    private cancel() {
        this.$uibModalInstance.close();
    }

    private selectFile() {
        var options: FileUploadOptions = {
            url: CoreUtility.apiPrefix + Constants.WEBAPI_TIME_PAYROLL_MASS_REGISTRATION_IMPORT + this.importType + "/" + (this.paymentDate ? this.paymentDate.toDateTimeString() : 'null') + "/" + this.clearRows,
            allowMultipleFiles: false,
        };

        var modal = this.notificationService.showFileUploadEx(this.translationService.translateInstant("time.payroll.massregistration.rowfunctions.importrows"), options);
        modal.result.then(result => {
            let rows: MassRegistrationTemplateRowDTO[] = result.result

            this.$uibModalInstance.close({ rows: rows, options: { importType: this.importType, paymentDate: this.paymentDate, clearRows: this.clearRows } });
        }, (reason) => {
            // User closed file dialog
        });
    }
}
