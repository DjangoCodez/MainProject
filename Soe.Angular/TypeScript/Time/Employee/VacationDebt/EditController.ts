import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IFocusService } from "../../../Core/Services/FocusService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { Feature, CompanySettingType, TermGroup_ReportExportType, SettingMainType, SoeReportTemplateType } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IEmployeeCalculateVacationResultDTO } from "../../../Scripts/TypeLite.Net4";
import { IEmployeeService } from "../../Employee/EmployeeService";
import { Constants } from "../../../Util/Constants";
import { ReportJobDefinitionFactory } from "../../../Core/Handlers/ReportJobDefinitionFactory";
import { IReportDataService } from "../../../Core/RightMenu/ReportMenu/ReportDataService";
import { IReportService } from "../../../Core/Services/ReportService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Init parameters
    private resultHeadId: number;
    private employeeId: number;
    private employeeNr: string;
    private employeeName: string;
    private dateStr: string;

    //Data
    private results: IEmployeeCalculateVacationResultDTO[] = [];
    private selectedResult: IEmployeeCalculateVacationResultDTO;

    // Company settings

    // Properties
  

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private employeeService: IEmployeeService,                
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private reportDataService: IReportDataService,
        private reportService: IReportService,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private notificationService: INotificationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    // SETUP

    public onInit(parameters: any) {
        this.resultHeadId = parameters.resultHeadId;
        this.employeeId = parameters.employeeId;
        this.employeeNr = parameters.employeeNr;
        this.employeeName = parameters.employeeName;
        this.dateStr = parameters.dateStr;

        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Employee_VacationDebt, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_VacationDebt].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_VacationDebt].modifyPermission;
    }

    // LOOKUPS

    private doLookups() {
        return this.progress.startLoadingProgress([
            () => this.loadModifyPermissions(),
            () => this.loadCompanySettings(),            
        ]).then(() => {            
            this.onLoadData();            
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];
        
        return this.coreService.hasModifyPermissions(features).then((x) => {
        
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        
        return this.coreService.getCompanySettings(settingTypes).then(x => {
        
        });
    }


    private onLoadData(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.employeeService.getEmployeeVacationDebtCalculationResults(this.resultHeadId, this.employeeId, true).then(x => {                
                this.results = x;
            })
        ]);
    }

    // ACTIONS

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.employeeService.saveEmployeeVacationCalculationResultValues(this.resultHeadId, this.employeeId, this.results).then(result => {
                if (result.success) {                    
                    completion.completed(Constants.EVENT_EDIT_SAVED);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.onLoadData();
            }, error => {

            });
    }

    private print(): ng.IPromise<any> {
        return this.reportService.getStandardReportId(SettingMainType.Company, CompanySettingType.DefaultEmployeeVacationDebtReport, SoeReportTemplateType.EmployeeVacationDebtReport).then(reportId => {
            let employeeIds: number[] = [];
            employeeIds.push(this.employeeId);
            this.reportDataService.createReportJob(ReportJobDefinitionFactory.createEmployeeVacationDebtReportDefinition(reportId, employeeIds, this.resultHeadId, TermGroup_ReportExportType.Pdf), true);
        });
    }
    // EVENTS

    // HELP-METHODS


    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
        });
    }
}
