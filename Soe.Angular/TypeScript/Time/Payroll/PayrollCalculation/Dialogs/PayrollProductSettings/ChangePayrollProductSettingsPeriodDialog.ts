import { DialogControllerBase } from "../../../../../Core/Controllers/DialogControllerBase";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IReportService } from "../../../../../Core/Services/ReportService";
import { ITranslationService } from "../../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../../Core/Services/NotificationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../../Core/Services/UrlHelperService";
import { IPayrollService } from "../../../PayrollService";
import { TermGroup_PayrollProductTaxCalculationType } from "../../../../../Util/CommonEnumerations";

export class ChangePayrollProductSettingsPeriodDialogControlller extends DialogControllerBase {

    isLoading: boolean = false;
    isPopulating: boolean = false;
    setting: any;

    private _selectedTaxType: number;
    get selectedTaxType(): number {
        return this._selectedTaxType;
    }
    set selectedTaxType(value: number) {
        this._selectedTaxType = value;
    }

    //@ngInject
    constructor(private $uibModalInstance,
        private payrollService: IPayrollService,
        translationService: ITranslationService,
        coreService: ICoreService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private employeeId,
        private timePeriodId,
        private productName,
        private payrollProductId) {

        super(null, translationService, coreService, notificationService, urlHelperService);

        this.loadLookups();
    }

    // EVENTS               

    buttonCancelClick() {
        this.$uibModalInstance.dismiss('cancel');
    }

    // LOOKUPS        

    private loadLookups() {
        this.startLoad();
        this.lookups = 1;
        this.isLoading = true;

        this.getEmployeeTimePeriodProductSettings();
    }

    protected lookupLoaded() {
        super.lookupLoaded();
        if (this.lookups == 0) {
            this.isLoading = false;
            this.readOnlyPermission = true;
            this.modifyPermission = true;
            this.populate();
        }
    }

    private populate() {
        this.isPopulating = true;
        if (!this.setting) {
            this.setting = {
                useSettings: false,
                employeeTimePeriodProductSettingId: 0,
                payrollProductId: this.payrollProductId,
                note: "",
            };
            this.selectedTaxType = TermGroup_PayrollProductTaxCalculationType.TableTax;

            this.isNew = true;
        } else {
            this.isNew = false;
            this.selectedTaxType = this.setting.taxCalculationType;
        }

        this.validate();
        this.isPopulating = false;
    }

    protected validate() {
    }


    // ACTIONS

    private save() {

        if (this.selectedTaxType == 0)
            this.setting.taxCalculationType = TermGroup_PayrollProductTaxCalculationType.TableTax;
        else if (this.selectedTaxType == 1)
            this.setting.taxCalculationType = TermGroup_PayrollProductTaxCalculationType.OneTimeTax;

        this.startSave();
        this.payrollService.saveEmployeeTimePeriodProductSetting(this.employeeId, this.timePeriodId, this.setting).then((result) => {
            if (result.success) {
                this.$uibModalInstance.close(true);
            } else {
                this.failedSave(result.errorMessage);
            }
        }, error => {
            this.failedSave(error.message);
        });
    }

    protected delete() {
        this.payrollService.deleteEmployeeTimePeriodProductSetting(this.setting.employeeTimePeriodProductSettingId).then((result) => {
            if (result.success) {
                this.$uibModalInstance.close(true);
            } else {
                this.failedDelete(result.errorMessage);
            }
        }, error => {
            this.failedDelete(error.message);
        });
    }

    private getEmployeeTimePeriodProductSettings() {
        this.payrollService.getEmployeeTimePeriodProductSetting(this.payrollProductId, this.employeeId, this.timePeriodId).then((x) => {
            this.setting = x;
            this.lookupLoaded();
        });
    }
}
