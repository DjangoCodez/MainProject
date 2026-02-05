import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IPayrollService } from "../PayrollService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Feature, CompanySettingType, TermGroup_TimeSalaryPaymentExportType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IIdListSelectionDTO, IIdSelectionDTO } from "../../../Scripts/TypeLite.Net4";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private timePeriodHeadId: number;
    private timePeriodId: number;
    private employeeIds: number[] = [];
    private publishPayrollSlipWhenLockingPeriod: boolean = false;
    private usePaymentDateAsExecutionDate: boolean = false;
    private exportType: TermGroup_TimeSalaryPaymentExportType = TermGroup_TimeSalaryPaymentExportType.Undefined;

    // Flags
    private exporting: boolean = false;

    private _salarySpecificationPublishDate: Date;
    get salarySpecificationPublishDate() {
        return this._salarySpecificationPublishDate;
    }
    set salarySpecificationPublishDate(date: Date) {
        if (!date) {
            this._salarySpecificationPublishDate = null;
            return;
        }
        this._salarySpecificationPublishDate = new Date(<any>date.toString());
    }

    private _debitDate: Date;
    get debitDate() {
        return this._debitDate;
    }
    set debitDate(date: Date) {
        if (!date) {
            this._debitDate = null;
            return;
        }
        this._debitDate = new Date(<any>date.toString());
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private payrollService: IPayrollService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.guid = parameters.guid;

        this.flowHandler.start([{ feature: Feature.Time_Payroll_Payment, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Payroll_Payment].readPermission;
        this.modifyPermission = response[Feature.Time_Payroll_Payment].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
        ]).then(() => {
            this.$q.all([
            ]).then(() => {
                this.lookupsDone();
            })
        });
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.warning",
            "core.exporting"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.PublishPayrollSlipWhenLockingPeriod);
        settingTypes.push(CompanySettingType.SalaryPaymentExportType);
        settingTypes.push(CompanySettingType.SalaryPaymentExportUsePaymentDateAsExecutionDate);

        return this.coreService.getCompanySettings(settingTypes).then(x => {

            this.publishPayrollSlipWhenLockingPeriod = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PublishPayrollSlipWhenLockingPeriod);
            this.usePaymentDateAsExecutionDate = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SalaryPaymentExportUsePaymentDateAsExecutionDate);
            this.exportType = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SalaryPaymentExportType);
        });
    }

    private lookupsDone() {
        if(!this.publishPayrollSlipWhenLockingPeriod)
            this.salarySpecificationPublishDate = CalendarUtility.getDateNow();
    }

    // ACTIONS

    private initExport() {
        this.validateExport().then(passed => {
            if (passed)
                this.export();
        });
    }

    private export() {
        this.exporting = true;

        this.progress.startWorkProgress((completion) => {
            this.payrollService.exportSalaryPayment(this.timePeriodHeadId, this.timePeriodId, this.employeeIds, this.salarySpecificationPublishDate, this.debitDate).then(result => {
                if (result.success) {
                    completion.completed(null, true);
                } else {
                    completion.failed(result.errorMessage);
                    this.exporting = false;
                }
            }, error => {
                completion.failed(error.message);
                this.exporting = false;
            });
        }, null, this.terms["core.exporting"] + '...').then(data => {
            this.messagingHandler.publishReloadGrid(this.guid);
            super.closeMe(true);
        }, error => {
            this.exporting = false;
        });
    }

    // EVENTS

    private onPeriodHeadSelectionUpdated(selection: IIdSelectionDTO) {
        this.timePeriodHeadId = selection.id;
    }

    private onPeriodSelectionUpdated(selection: IIdListSelectionDTO) {
        this.timePeriodId = selection.ids.length > 0 ? selection.ids[0] : 0;
    }

    private onEmployeesSelectionUpdated(selection: IIdListSelectionDTO) {
        this.employeeIds = selection.ids;
    }

    private showDebitDate() {
        return this.exportType == TermGroup_TimeSalaryPaymentExportType.ISO20022 && !this.usePaymentDateAsExecutionDate
    }

    // VALIDATION

    private validateExport(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        this.payrollService.exportSalaryPaymentWarnings(this.timePeriodId, this.employeeIds).then(message => {            
            if (!message || (message && message.length === 0)) {
                deferral.resolve(true);
            } else {            
                var modal = this.notificationService.showDialogEx(this.terms["core.warning"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    deferral.resolve(true);
                }, (reason) => {
                    deferral.resolve(false);
                });
            }
        });

        return deferral.promise;
    }
}