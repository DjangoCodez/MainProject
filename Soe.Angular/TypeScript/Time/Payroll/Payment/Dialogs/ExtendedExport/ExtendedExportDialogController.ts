import { IProgressHandler } from "../../../../../Core/Handlers/ProgressHandler";
import { IProgressHandlerFactory } from "../../../../../Core/Handlers/progresshandlerfactory";
import { ICoreService } from "../../../../../Core/Services/CoreService";
import { IFocusService } from "../../../../../Core/Services/focusservice";
import { ISmallGenericType } from "../../../../../Scripts/TypeLite.Net4";
import { CompanySettingType, TermGroup, TermGroup_Currency } from "../../../../../Util/CommonEnumerations";
import { SettingsUtility } from "../../../../../Util/SettingsUtility";
import { PayrollService } from "../../../PayrollService";

export class ExtendedExportDialogController {

    // Data
    private agreementNumber: string;
    private senderIdentification: string;
    private currencyRate: number;
    public progress: IProgressHandler;
    private currencyType: ISmallGenericType;
    private exporting: boolean = false;
    private currencyDate: Date;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $uibModalInstance,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private progressHandlerFactory: IProgressHandlerFactory,
        private selection: any,
        private currency: TermGroup_Currency,
        private payrollService: PayrollService,
        private focusService: IFocusService) {
        this.progress = progressHandlerFactory.create()
        this.$q.all([
            this.loadData,
        ]);
    }
    $onInit() {
        this.loadData();
    }

    private loadData(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.loadCompanySettings(),
            () => this.loadCurrencys()
        ]).then(() =>
            this.$timeout(() => { this.focusService.focusByName("ctrl_currencyRate") }, 200));
    }

      // LOOKUPS

    private loadCurrencys(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.SysCurrency, false, false).then((x) => {
            this.currencyType = _.find(x, t => t.id === this.currency);
        });
    }    
   
    private loadCompanySettings(): ng.IPromise<any>  {
        const settingTypes = [CompanySettingType.SalaryPaymentExportExtendedAgreementNumber, CompanySettingType.SalaryPaymentExportExtendedSenderIdentification];
        
        return this.coreService.getCompanySettings(settingTypes).then(x => {
                this.agreementNumber = SettingsUtility.getStringCompanySetting(x, CompanySettingType.SalaryPaymentExportExtendedAgreementNumber);
                this.senderIdentification = SettingsUtility.getStringCompanySetting(x, CompanySettingType.SalaryPaymentExportExtendedSenderIdentification);
        });

    }

    // ACTIONS

    private create() {
        this.exporting = true;
        this.progress.startSaveProgress((completion) => {
            this.payrollService.exportSalaryPaymentExtendedSelection(this.selection.timeSalaryPaymentExportId, this.currencyDate, this.currencyRate, this.currency).then(result => {
                if (result.success) {
                    completion.completed(null, true);
                    this.exporting = false;
                    this.$uibModalInstance.close({ created: true });
                } else {
                    completion.failed(result.errorMessage);
                    this.exporting = false;
                }
            }, error => {
                completion.failed(error.message);
                this.exporting = false;
            });
        }, null).then(data => {
            this.exporting = false;
            
        }, error => {
            this.exporting = false;
        });
    }
    
    private cancel() {
        this.$uibModalInstance.close();
    }    
}  
