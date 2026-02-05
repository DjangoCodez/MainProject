import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ProjectTimeBlockDTO, ProjectTimeBlockSaveDTO } from "../../../Common/Models/ProjectDTO";
import { TimeProjectContainer, SOEMessageBoxSize, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { SmallGenericType } from "../../../Common/Models/smallgenerictype";
import { IProjectSmallDTO, IActionResult } from "../../../Scripts/TypeLite.Net4";
import { IProjectService } from "../../../Shared/Billing/Projects/ProjectService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { TermGroup_ProjectType, Feature, CompanySettingType, TermGroup_AttestEntity, UserSettingType, SettingMainType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ISoeGridOptionsAg } from "../../../Util/SoeGridOptionsAg";
import { AttestStateDTO } from "../../../Common/Models/AttestStateDTO";
import { AttestPayrollTransactionDTO } from "../../../Common/Models/AttestPayrollTransactionDTO";
import { TimePayrollUtility } from "../../../Util/TimePayrollUtility";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Guid } from "../../../Util/StringUtility";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Permissions
    private modifyOtherEmployeesPermission: boolean = false;

    // Properties
    private isLocked = true;
    private projectTimeBlockRows: ProjectTimeBlockDTO[];
    private projectType = TermGroup_ProjectType.TimeProject;
    private projectContainer = TimeProjectContainer.TimeSheet;
    private employeeId = 0;
    private timeProjectFrom: Date;
    private timeProjectTo: Date;
    private groupByDate = false;
    private dateRangeText: string;

    // User settings
    private userSettingTimeAttestDisableSaveAttestWarning = false;

    //Attest
    private userValidPayrollAttestStates: AttestStateDTO[] = [];
    private userValidPayrollAttestStatesOptions: any = [];

    // Flags
    private loadingTimeProjectRows = false;
    private loadTimeProjectRowsTimeout: any;
    private useExtendedTimeRegistration = false;
    private hasSelectedRows = false;
    private useProjectTimeBlocks = false;
    private usePayroll = false;
    private usedPayrollSince: Date;
    private resultLoaded = false;

    // Collections
    private terms: { [index: string]: string };
    private employees: SmallGenericType[];
    private customers: SmallGenericType[];
    private projects: IProjectSmallDTO[];
    //private projectInvoices: SoftOne.IProjectInvoiceSmallDTO[];

    //Project central
    projectId: number;
    includeChildProjects: boolean;
    orders: number[];

    // Migration
    currentGuid: Guid;
    timerToken: any;
    modal: angular.ui.bootstrap.IModalService;

    // Grid
    private soeGridOptions: ISoeGridOptionsAg;

    get showMigrateButton(): boolean {
        return CoreUtility.isSupportAdmin && this.useProjectTimeBlocks && this.usePayroll;
    }

    //@ngInject
    constructor(
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        urlHelperService: IUrlHelperService,
        private projectService: IProjectService,
        private coreService: ICoreService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private $timeout: ng.ITimeoutService,
        private notificationService: INotificationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);
        //super("Billing.Projects.TimeSheets", soeConfig.feature, $uibModal, translationService, messagingService, coreService, notificationService, urlHelperService);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups()) //this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        // Init parameters
        this.employeeId = soeConfig.employeeId;
        this.timeProjectFrom = CalendarUtility.getDateToday().beginningOfWeek();
        this.timeProjectTo = CalendarUtility.getDateToday().endOfWeek();

        messagingService.subscribe(Constants.EVENT_SEARCH_TIME_PROJECT_ROWS_TIMESHEET, (x) => {
            // Make sure event does not come from any other orders product rows
            if (x.guid === this.guid)
                this.loadTimeProjectRows(x.emps, x.projs, x.orders, x.employeeCategories, x.incPlannedAbsence, x.incInternOrderText, x.getAll, x.timeDeviationCauses);
        });
    }

    // SETUP
    public onInit(parameters: any) {
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([
            { feature: soeConfig.feature, loadModifyPermissions: true },
            { feature: Feature.Billing_Project_TimeSheetUser_OtherEmployees, loadModifyPermissions: true },
            { feature: Feature.Time_Time_TimeSheetUser_OtherEmployees, loadModifyPermissions: true }
        ]);

        if (parameters.isProjectCentral && parameters.isProjectCentral === true) {
            this.projectContainer = TimeProjectContainer.ProjectCentral;
            this.messagingService.subscribe(Constants.EVENT_LOAD_PROJECTCENTRALDATA, (x) => {
                this.projectId = x.projectId;
                this.includeChildProjects = x.includeChildProjects;
                this.orders = x.orders;

                if (x.fromDate)
                    this.timeProjectFrom = x.fromDate;

                if (x.toDate)
                    this.timeProjectTo = x.toDate;
            });
        }
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.modifyPermission = response[soeConfig.feature].modifyPermission;
        this.modifyOtherEmployeesPermission = response[Feature.Billing_Project_TimeSheetUser_OtherEmployees].modifyPermission || response[Feature.Time_Time_TimeSheetUser_OtherEmployees].modifyPermission;
        this.isLocked = !this.employeeId && !this.modifyOtherEmployeesPermission;
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [CompanySettingType.ProjectUseExtendedTimeRegistration, CompanySettingType.UseProjectTimeBlocks, CompanySettingType.UsePayroll, CompanySettingType.UsedPayrollSince];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useExtendedTimeRegistration = x[CompanySettingType.ProjectUseExtendedTimeRegistration];
            this.useProjectTimeBlocks = x[CompanySettingType.UseProjectTimeBlocks];
            this.usePayroll = x[CompanySettingType.UsePayroll];
            this.usedPayrollSince = x[CompanySettingType.UsedPayrollSince];

            if (this.usedPayrollSince)
                this.usedPayrollSince = new Date(<any>this.usedPayrollSince);
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.TimeDisableApplySaveAttestWarning];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.userSettingTimeAttestDisableSaveAttestWarning = SettingsUtility.getBoolUserSetting(x, UserSettingType.TimeDisableApplySaveAttestWarning);
        });
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadCompanySettings(),
            this.loadUserSettings(),
            this.loadEmployees()
        ]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false, null, null);
    }

    // LOOKUPS
    private loadEmployees(): ng.IPromise<any> {
        const deferral = this.$q.defer();
        // Only load employees if permitted to modify other
        if (this.modifyOtherEmployeesPermission) {
            return this.projectService.getEmployeesForProject(true, false, false, this.employeeId).then(x => {
                this.employees = x;
            });
        }

        deferral.resolve();
        return deferral.promise;
    }

    // Actions
    private loadTimeProjectRows(employees: number[], projects: number[], orders: number[], employeeCategories: number[], incPlannedAbsence = false, incInternOrderText = false, getAll = false, timeDeviationCause: number[] = []) {
        if (this.loadTimeProjectRowsTimeout)
            this.$timeout.cancel(this.loadTimeProjectRowsTimeout);

        this.loadTimeProjectRowsTimeout = this.$timeout(() => {
            this.loadingTimeProjectRows = true;
            this.progress.startLoadingProgress([() => {
                return this.projectService.getProjectTimeBlocksForTimeSheetFiltered(this.modifyOtherEmployeesPermission ? 0 : this.employeeId, getAll ? new Date(1900, 1, 1) : this.timeProjectFrom, getAll ? new Date(9999, 1, 1) : this.timeProjectTo, employees, projects, orders, employeeCategories, this.groupByDate, incPlannedAbsence, incInternOrderText, timeDeviationCause).then((data: ProjectTimeBlockDTO[]) => {
                    //this.projectTimeBlockRows = x;

                    this.projectTimeBlockRows = data.map(tb => {
                        const obj = new ProjectTimeBlockDTO();
                        angular.extend(obj, tb);
                        if (obj.date)
                            obj.date = CalendarUtility.convertToDate(obj.date);
                        if (obj.startTime)
                            obj.startTime = CalendarUtility.convertToDate(obj.startTime);
                        if (obj.stopTime)
                            obj.stopTime = CalendarUtility.convertToDate(obj.stopTime);
                        obj.showOrderButton = obj.customerInvoiceId !== undefined && obj.customerInvoiceId !== null && obj.customerInvoiceId !== 0;
                        obj.showCustomerButton = obj.customerId !== undefined && obj.customerId !== null && obj.customerId !== 0;
                        obj.showProjectButton = obj.projectId !== undefined && obj.projectId !== null && obj.projectId !== 0;
                        return obj;
                    });

                    if (getAll) {
                        var dates = this.projectTimeBlockRows.map(d => d.date);
                        if (dates.length > 0) {
                            this.timeProjectTo = new Date(Math.max.apply(null, dates));
                            this.timeProjectFrom = new Date(Math.min.apply(null, dates));
                        }
                    }
                })
            }])
        }, 500);
    }

    private reloadGrid() {
        this.$scope.$broadcast(Constants.EVENT_RELOAD_GRID, { guid: this.guid });
    }

    private recalculateWorkTime() {
        const selectedRows = this.soeGridOptions.getSelectedRows();

        if (selectedRows && selectedRows.length > 0) {
            this.progress.startSaveProgress((completion) => {

                const dtos: ProjectTimeBlockSaveDTO[] = [];
                _.forEach(selectedRows, (row: ProjectTimeBlockDTO) => {
                    var dto = new ProjectTimeBlockSaveDTO();
                    dto.projectTimeBlockId = row.projectTimeBlockId;
                    dto.employeeId = row.employeeId;
                    dto.timeBlockDateId = row.timeBlockDateId;
                    dtos.push(dto);
                });

                this.projectService.recalculateWorkTime(dtos).then((result: IActionResult) => {
                    if (result.success) {
                        completion.completed("");
                        this.reloadGrid();
                    }
                    else {
                        completion.failed(result.errorMessage);
                    }
                });
            }, this.guid);
        }
    }

    //Attest...
    private isAttestDisabled(): boolean {
        return !(this.projectTimeBlockRows &&
            (this.projectTimeBlockRows.length > 0) &&
            (this.hasSelectedRows) &&
            (!this.groupByDate) &&
            this.userValidPayrollAttestStatesOptions &&
            this.userValidPayrollAttestStatesOptions.length > 0);
    }

    private loadAttestStates(employeeGroupId: any) {
        this.userValidPayrollAttestStates = [];

        this.coreService.getUserValidAttestStates(TermGroup_AttestEntity.PayrollTime, this.timeProjectFrom, this.timeProjectTo, true, employeeGroupId).then((result) => {
            this.userValidPayrollAttestStates = result;
            this.userValidPayrollAttestStatesOptions.length = 0;
            _.forEach(this.userValidPayrollAttestStates, (attestState: AttestStateDTO) => {
                this.userValidPayrollAttestStatesOptions.push({ id: attestState.attestStateId, name: attestState.name });
            });
        });
    }

    private saveUserSettingDisableAttestWarning() {
        this.userSettingTimeAttestDisableSaveAttestWarning = true;
        this.coreService.saveBoolSetting(SettingMainType.User, UserSettingType.TimeDisableApplySaveAttestWarning, this.userSettingTimeAttestDisableSaveAttestWarning);
    }

    private getAttestState(attestStateId: number): AttestStateDTO {
        const filtered = this.userValidPayrollAttestStates.filter(x => x.attestStateId === attestStateId);
        return filtered && filtered.length > 0 ? filtered[0] : null;
    }

    private saveAttest(option: any) {
        const attestStateTo: AttestStateDTO = this.getAttestState(option.id);
        if (!attestStateTo)
            return;

        const transactionItems: AttestPayrollTransactionDTO[] = [];

        const selectedRows = this.soeGridOptions.getSelectedRows();

        this.progress.startSaveProgress((completion) => {

            _.forEach(selectedRows, (row: ProjectTimeBlockDTO) => {
                _.forEach(row.timePayrollTransactionIds, (timePayrollTransactionId: any) => {
                    const transactionItem: AttestPayrollTransactionDTO = new AttestPayrollTransactionDTO();
                    transactionItem.employeeId = row.employeeId;
                    transactionItem.timePayrollTransactionId = timePayrollTransactionId;
                    transactionItem.attestStateId = row.timePayrollAttestStateId;
                    transactionItem.date = row.date;
                    transactionItem.isScheduleTransaction = false;
                    transactionItem.isExported = false;
                    transactionItem.isPreliminary = false;
                    transactionItems.push(transactionItem);
                });
            });

            this.validateSaveAttestTransactions(selectedRows, transactionItems, attestStateTo).then((validItems: any[]) => {
                if (validItems) {
                    this.saveAttestForTransactions(validItems, attestStateTo).then((result: IActionResult) => {
                        if (result.success) {
                            completion.completed("");
                            this.reloadGrid();
                        }
                        else {
                            completion.failed(result.errorMessage);
                        }
                    });
                }
                else {
                    completion.failed("");
                }
            })
        }, this.guid);
    }

    private validateSaveAttestTransactions(selectedRows: any[], transactionItems: AttestPayrollTransactionDTO[], attestStateTo: AttestStateDTO): ng.IPromise<any[]> {

        const deferral = this.$q.defer<any[]>();

        this.projectService.saveAttestForTransactionsValidation(transactionItems, attestStateTo.attestStateId, false).then((validationResult) => {
            if (validationResult.success && this.userSettingTimeAttestDisableSaveAttestWarning) {
                deferral.resolve(validationResult.validItems);
            }
            else {
                this.translationService.translateMany(["billing.project.timesheet.timerowsstatuschange", "core.donotshowagain"]).then((terms) => {
                    let message = validationResult.success ? terms["billing.project.timesheet.timerowsstatuschange"].format(selectedRows.length.toString(), attestStateTo.name) : validationResult.message;
                    const modal = this.notificationService.showDialog(validationResult.title, message, TimePayrollUtility.getSaveAttestValidationMessageIcon(validationResult), TimePayrollUtility.getSaveAttestValidationMessageButton(validationResult), SOEMessageBoxSize.Medium, false, validationResult.success, terms["core.donotshowagain"]);
                    modal.result.then(result => {
                        if (validationResult.success) {
                            if (result) {
                                if (result.isChecked)
                                    this.saveUserSettingDisableAttestWarning();
                                deferral.resolve(validationResult.validItems);
                            }
                            else {
                                deferral.resolve(null);
                            }
                        }
                        else {
                            deferral.resolve(null);
                        }
                    });
                });
            }
        });

        return deferral.promise;
    }

    private saveAttestForTransactions(validItems: any, attestStateTo: AttestStateDTO): ng.IPromise<IActionResult> {
        //window.scrollTo(0, 0);
        return this.projectService.saveAttestForTransactions(validItems, attestStateTo.attestStateId, false);
    }

    private runMigrateTimesJob() {
        let terms;
        let company;
        const keys: string[] = [
            "billing.project.timesheet.converttimes",
            "common.licensename",
            "common.company",
            "billing.project.timesheet.payrollactivatedfrom",
            "billing.project.timesheet.migratewarning",
            "common.dailyrecurrencepattern.startdate"
        ];

        this.$q.all([
            this.translationService.translateMany(keys).then((result) => { terms = result }),
            this.coreService.getCompany(soeConfig.actorCompanyId).then((comp) => { company = comp })
        ]).then(() => {
            let message = terms["common.licensename"] + ": " + company.licenseNr + "</br>";
            message += terms["common.company"] + ": " + company.number + " - " + company.name + "</br>"
            message += terms["billing.project.timesheet.payrollactivatedfrom"] + ": " + this.usedPayrollSince.toLocaleDateString() + "</br></br>"
            message += terms["billing.project.timesheet.migratewarning"];

            const modal = this.notificationService.showDialog(terms["billing.project.timesheet.converttimes"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(result => {
                if (result) {
                    this.progress.showProgressDialog(terms["common.dailyrecurrencepattern.startdate"] + "...");
                    this.currentGuid = Guid.newGuid();

                    this.projectService.migrateToProjectTimeBlocks(this.currentGuid).then((x) => {
                        this.timerToken = setInterval(() => this.getProgress(), 500);
                        this.resultLoaded = false;
                    });
                }
            });
        });
    }

    private getProgress() {
        this.coreService.getProgressInfo(this.currentGuid.toString()).then((x) => {
            this.progress.updateProgressDialogMessage(x.message);
            if (x.abort === true && !this.resultLoaded)
                this.getProcessedResult();
        });
    }

    private getProcessedResult() {
        this.resultLoaded = true;
        clearInterval(this.timerToken);
        this.projectService.getMigrateToProjectTimeBlocksResult(this.currentGuid).then((result) => {
            this.progress.hideProgressDialog();
            this.notificationService.showDialog("Resultat", result[0], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
        });
    }
}