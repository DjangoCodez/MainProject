import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { SOEMessageBoxImage, SOEMessageBoxButtons, AbsenceRequestGuiMode, AbsenceRequestViewMode, AbsenceRequestParentMode } from "../../../../Util/Enumerations";
import { ShiftDTO, TimeScheduleScenarioHeadDTO } from "../../../../Common/Models/TimeSchedulePlanningDTOs";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { ExtendedAbsenceSettingDTO, EmployeeRequestDTO } from "../../../../Common/Models/EmployeeRequestDTO";
import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { EmployeeListDTO } from "../../../../Common/Models/EmployeeListDTO";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IScheduleService as ISharedScheduleService } from "../../Schedule/ScheduleService";
import { ITimeService as ISharedTimeService } from "../../Time/TimeService";
import { Guid } from "../../../../Util/StringUtility";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { Constants } from "../../../../Util/Constants";
import { ITimeDeviationCauseDTO } from "../../../../Scripts/TypeLite.Net4";
import { Feature, CompanySettingType, TermGroup, TermGroup_EmployeeRequestType, TermGroup_ShiftHistoryType, TermGroup_TimeScheduleTemplateBlockShiftUserStatus, TermGroup_YesNo, TermGroup_EmployeeRequestStatus, TermGroup_EmployeeRequestResultStatus } from "../../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";

//Enums

enum AbsenceType {
    Fulltime = 1,
    Parttime = 2,
    SelectedShifts = 3,
}

enum PercentageAbsenceType {
    BeginningOfDay = 0,
    EndOfDay = 1,
}

enum RestoreOptions {
    RestoreAbsence = 1,
    RestoreAbsenceAndSetRequestToPending = 2,
}

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private isForcedDefinitive: boolean = false;

    // Terms
    private terms: any;
    private title: string;
    private saveButtonLabel: string;
    private affectedShifts: ShiftDTO[];
    private historyShifts: any[];
    private isLendedLabel: string;
    private isOnDutyLabel: string;

    // Init parameters        
    private guiMode: AbsenceRequestGuiMode;
    private viewMode: AbsenceRequestViewMode;
    private timeScheduleScenarioHeadId?: number;
    private employeeRequestId: number;
    private employeeId: number;
    private employeeGroupId: number;
    private hideOptionSelectedShift: boolean = false;
    private skipXEMailOnShiftChanges: boolean = false;
    private loadRequestFromInterval: boolean = false;
    private wholeDaysSelectedWithoutHoles: boolean = false;
    private selectedDates: Date[];
    private parentMode: AbsenceRequestParentMode;
    private readOnly: boolean;

    private isModal: boolean = false;
    private modal: any;

    //Only regular absence
    private shiftId: number;
    private dateFrom: Date;
    private dateTo: Date;

    // Permissions - vacation
    private vacationPermission: boolean = false;

    private employeeRequest: EmployeeRequestDTO;
    private timeScheduleScenarioHead: TimeScheduleScenarioHeadDTO;
    private settingsIsDirty: boolean = false;

    // Company Settings
    private sendXEMailOnChange: boolean = false;
    private setApprovedYesAsDefault: boolean = false;
    private onlyNoReplacementIsSelectable: boolean = false;
    private includeNoteInMessages: boolean = false;
    private hiddenEmployeeId: number;

    //Collections
    private deviationCauses: ITimeDeviationCauseDTO[];
    private employeeChilds: SmallGenericType[];
    private approvalTypes: SmallGenericType[];
    private employeeList: EmployeeListDTO[];
    private employees: SmallGenericType[];
    private replaceAllWithEmployees: SmallGenericType[];
    private replaceWithEmployees: SmallGenericType[] = [];

    //Functions
    private restoreOptions: any = [];

    private selectedApproveAllApprovalTypeId: number = TermGroup_YesNo.Unknown;

    //GUI: GET/SET        
    private _selectedEmployee: SmallGenericType;
    get selectedEmployee() {
        return this._selectedEmployee;
    }
    set selectedEmployee(item: SmallGenericType) {
        this.dirtyHandler.setDirty();
        this._selectedEmployee = item;
        if (item) {
            this.employeeId = item.id;
            this.employeeRequest.employeeId = this.employeeId;
        }
        else {
            this.employeeId = 0;
            this.employeeRequest.employeeId = 0;
        }

        this.employeeChanged();
    }

    private _setAsDefinitive: boolean = false;
    get setAsDefinitive() {
        return this._setAsDefinitive;
    }
    set setAsDefinitive(value: boolean) {
        if (value && this.affectedShifts && this.affectedShifts.length == 0) {
            var modal = this.notificationService.showDialog(this.terms["core.info"], this.terms["time.schedule.absencerequests.setasdefinitivewarning"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                this._setAsDefinitive = value;
            }, (reason) => {
            });
        } else {
            this._setAsDefinitive = value;
        }
    }

    private _selectedDeviationCause: ITimeDeviationCauseDTO;
    get selectedDeviationCause() {
        return this._selectedDeviationCause;
    }
    set selectedDeviationCause(item: ITimeDeviationCauseDTO) {
        this.dirtyHandler.setDirty();
        this._selectedDeviationCause = item;
        if (this.selectedDeviationCause)
            this.employeeRequest.timeDeviationCauseId = this.selectedDeviationCause.timeDeviationCauseId;
        else
            this.employeeRequest.timeDeviationCauseId = 0;

        this.deviationCauseChanged();
    }

    private _selectedEmployeeChild: SmallGenericType;
    get selectedEmployeeChild() {
        return this._selectedEmployeeChild;
    }
    set selectedEmployeeChild(item: SmallGenericType) {
        this.dirtyHandler.setDirty();
        this._selectedEmployeeChild = item;
        if (this.selectedEmployeeChild)
            this.employeeRequest.employeeChildId = this.selectedEmployeeChild.id;
        else
            this.employeeRequest.employeeChildId = 0;

        this.employeeChildChanged();
    }

    private _selectedDateFrom: Date;
    get selectedDateFrom() {
        return this._selectedDateFrom;
    }
    set selectedDateFrom(date: Date) {
        this._selectedDateFrom = date;

        if (!date || !(date instanceof Date))
            return;

        this.employeeRequest.start = date;
        this.selectedDateFromChanged();
    }

    private _selectedDateTo: Date;
    get selectedDateTo() {
        return this._selectedDateTo;
    }
    set selectedDateTo(date: Date) {
        this._selectedDateTo = date;

        if (!date || !(date instanceof Date))
            return;

        this.employeeRequest.stop = date;
        this.selectedDateToChanged();
    }

    private _selectedPercentageAbsenceType: number;
    get selectedPercentageAbsenceType() {
        return this._selectedPercentageAbsenceType;
    }
    set selectedPercentageAbsenceType(value: number) {
        this._selectedPercentageAbsenceType = value;
        if (this.employeeRequest && this.employeeRequest.extendedSettings) {
            if (value === PercentageAbsenceType.BeginningOfDay) {
                this.employeeRequest.extendedSettings.percentalAbsenceOccursStartOfDay = true;
                this.employeeRequest.extendedSettings.percentalAbsenceOccursEndOfDay = false;
            }
            else {
                this.employeeRequest.extendedSettings.percentalAbsenceOccursStartOfDay = false;
                this.employeeRequest.extendedSettings.percentalAbsenceOccursEndOfDay = true;
            }
            this.setSettingsIsDirty();
        }
    }

    private _selectedAbsenceType: AbsenceType = AbsenceType.Fulltime;
    get selectedAbsenceType() {
        return this._selectedAbsenceType;
    }
    set selectedAbsenceType(value: number) {
        var previous = this._selectedAbsenceType
        this._selectedAbsenceType = value;
        if (previous !== this._selectedAbsenceType)
            this.selectedAbsenceTypeChanged();
    }

    private _selectedReplaceAllWithEmployee: SmallGenericType;
    get selectedReplaceAllWithEmployee() {
        return this._selectedReplaceAllWithEmployee;
    }
    set selectedReplaceAllWithEmployee(item: SmallGenericType) {

        this._selectedReplaceAllWithEmployee = item;
        this.replaceAllWithEmployeeChanged(this._selectedReplaceAllWithEmployee);
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private coreService: ICoreService,
        private notificationService: INotificationService,
        private translationService: ITranslationService,
        private sharedScheduleService: ISharedScheduleService,
        private sharedTimeService: ISharedTimeService,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;
            this.onInit(parameters);
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.employeeRequestId = parameters.id;
        this.employeeId = parameters.employeeId;
        this.skipXEMailOnShiftChanges = parameters.skipXEMailOnShiftChanges;
        this.guid = parameters.guid;
        this.guiMode = parameters.guiMode;
        this.parentMode = parameters.parentMode;
        this.readOnly = parameters.readOnly;
        this.timeScheduleScenarioHeadId = parameters.timeScheduleScenarioHeadId;

        if (this.guiMode === AbsenceRequestGuiMode.EmployeeRequest) {
            this.viewMode = parameters.viewMode;
            this.employeeGroupId = parameters.employeeGroupId;
            this.loadRequestFromInterval = parameters.loadRequestFromInterval;
            if (this.loadRequestFromInterval) {
                this.dateFrom = CalendarUtility.convertToDate(parameters.date);
                this.dateTo = CalendarUtility.convertToDate(parameters.date);
            }
        }
        else if (this.guiMode === AbsenceRequestGuiMode.AbsenceDialog) {
            this.employeeGroupId = 0;

            if (this.parentMode === AbsenceRequestParentMode.SchedulePlanning) {
                this.viewMode = AbsenceRequestViewMode.Attest; //only attest for the moment
                this.shiftId = parameters.shiftId;
                this.dateFrom = CalendarUtility.convertToDate(parameters.date);
                this.dateTo = CalendarUtility.convertToDate(parameters.date);
                //this.timeScheduleTemplatePeriodId = parameters.scheduleTemplatePeriodId;
                this.hideOptionSelectedShift = parameters.hideOptionSelectedShift;
            }
            else if (this.parentMode === AbsenceRequestParentMode.TimeAttest) {
                this.viewMode = parameters.viewMode; // can be both attest and employee     
                this.hideOptionSelectedShift = true;
                this.selectedDates = parameters.selectedDates;
                this.wholeDaysSelectedWithoutHoles = this.selectedDates.length > 0 && CalendarUtility.isCoherent(this.selectedDates);
                this.dateFrom = this.wholeDaysSelectedWithoutHoles ? CalendarUtility.convertToDate(_.min(this.selectedDates)) : CalendarUtility.getDateNow();
                this.dateTo = this.wholeDaysSelectedWithoutHoles ? CalendarUtility.convertToDate(_.max(this.selectedDates)) : CalendarUtility.getDateNow();

            }
        }

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        let featureId = soeConfig.feature;
        if (!featureId)
            featureId = Feature.None;

        this.flowHandler.start([{ feature: featureId, loadReadPermissions: false, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = true;
        this.modifyPermission = true;
    }

    private doLookups() {
        return this.progress.startLoadingProgress([
            () => this.loadEmployees(),
            () => this.loadAbsenceRequest(false, true, this.loadRequestFromInterval),
            () => this.loadDeviationCauses(),
            () => this.loadEmployeeChilds(),
            () => this.loadReadOnlyPermissions(),
            () => this.loadCompanySettings(),
            () => this.loadApprovalTypes(),
            () => this.loadHiddenEmployeeId(),
            () => this.loadTimeScheduleScenarioHead(),
            () => this.loadTerms(),
        ]).then(() => {
            this.lookUpsLoaded();
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    private lookUpsLoaded() {
        if (this.isAttestRoleMode()) {
            this.employees = [];
            this.replaceAllWithEmployees = [];
            _.forEach(this.employeeList, (emp: EmployeeListDTO) => {
                if (emp.employeeId !== Constants.NO_REPLACEMENT_EMPLOYEEID && emp.employeeId !== this.hiddenEmployeeId) {
                    let employee = new SmallGenericType(emp.employeeId, '({0}) {1}'.format(emp.employeeNr, emp.name));
                    this.employees.push(employee);
                }

                if (this.onlyNoReplacementIsSelectable === false && this.employeeId !== emp.employeeId) {
                    if (emp.hasEmployment(CalendarUtility.convertToDate(this.employeeRequest.start), CalendarUtility.convertToDate(this.employeeRequest.stop)) || emp.employeeId === Constants.NO_REPLACEMENT_EMPLOYEEID || emp.employeeId === this.hiddenEmployeeId) {
                        let employee = new SmallGenericType(emp.employeeId, (emp.employeeId === Constants.NO_REPLACEMENT_EMPLOYEEID || emp.employeeId === this.hiddenEmployeeId) ? emp.name : '({0}) {1}'.format(emp.employeeNr, emp.name));
                        this.replaceAllWithEmployees.push(employee);
                    }
                }
            });

            if (this.onlyNoReplacementIsSelectable) {
                var employeeNoReplacement = _.find(this.employeeList, e => e.employeeId === Constants.NO_REPLACEMENT_EMPLOYEEID);
                if (employeeNoReplacement) {
                    let emp = new SmallGenericType(employeeNoReplacement.employeeId, employeeNoReplacement.name);
                    this.replaceAllWithEmployees.push(emp);
                }
            }
        }

        if (this.isEmployeeRequestGuiMode())
            this.title = this.terms["time.schedule.absencerequests.absencerequest"];
        else if (this.isRegularAbsenceGuiMode())
            this.title = this.terms["time.schedule.absencerequests.absence"];

        if (this.timeScheduleScenarioHead)
            this.title = this.title + " (" + this.terms["time.schedule.absencerequests.scenario"] + this.timeScheduleScenarioHead.name + ") ";

        if (this.employeeRequest && this.employeeRequest.timeDeviationCauseId && !this.deviationCauses.find(d => d.timeDeviationCauseId === this.employeeRequest.timeDeviationCauseId)) {
            // If manager is not allowed to see the selected deviation cause, load it and add it to the list.
            // Then it will be visible in the dropdown and the manager can approve without changing the cause.
            this.loadSelectedDeviationCause().then(() => {
                this.populateRequest();
            });
        } else {
            this.populateRequest();
        }

        this.setupRestoreOptions();
    }

    //Service calls

    //------Collections-------//
    private loadEmployeeGroupId(): ng.IPromise<any> {
        return this.sharedTimeService.getEmployeeGroupId(this.employeeId, this.selectedDateFrom).then(x => {
            this.employeeGroupId = x;
        });
    }

    private loadDeviationCauses(): ng.IPromise<any> {
        if (this.isEmployeeRequestGuiMode())
            return this.loadTimeDeviationCauseRequests();
        else if (this.isRegularAbsenceGuiMode())
            return this.loadDeviationCauseForRegularAbsence();
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.warning",
            "core.info",
            "core.save",
            "core.send",
            "time.schedule.absencerequests.absencerequest",
            "time.schedule.absencerequests.absence",
            "time.schedule.absencerequests.updateaffectedshiftswarning",
            "time.schedule.absencerequests.restoreoptionsrestoreabsence",
            "time.schedule.absencerequests.restoreoptionsrestoreabsenceandsetrequesttopending",
            "time.schedule.absencerequests.shiftsmissingreplacements",
            "time.schedule.absencerequests.shiftsmissingapproval",
            "time.schedule.absencerequests.noneapprovedshifts",
            "time.schedule.absencerequests.restoresuccessinfo",
            "time.schedule.absencerequests.intersectattention",
            "time.schedule.absence.shiftsmissingapproval",
            "time.schedule.absencerequests.absencesavedmessage",
            "time.schedule.absencerequests.shiftsincludedinabsencerequestmessage",
            "time.schedule.absencerequests.invaliddates",
            "time.schedule.absence.noaffectedshifts",
            "time.schedule.absencerequests.performrequestwarning",
            "time.schedule.absencerequests.islended",
            "time.schedule.absencerequests.setasdefinitivewarning",
            "time.schedule.absencerequests.scenario",
            "time.schedule.planning.blocktype.onduty",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms

            this.isLendedLabel = this.terms["time.schedule.absencerequests.islended"];
            this.isOnDutyLabel = this.terms["time.schedule.planning.blocktype.onduty"];
            if (this.isEmployeeMode() && this.isEmployeeRequestGuiMode())
                this.saveButtonLabel = this.terms["core.send"];
            else
                this.saveButtonLabel = this.terms["core.save"];
        });
    }

    private loadEmployees(): ng.IPromise<any> {
        if (this.isAttestRoleMode() && this.parentMode !== AbsenceRequestParentMode.TimeAttest /*&& this.isEmployeeRequestGuiMode()*/)
            return this.loadAttestEmployees();
        else
            return this.loadEmployee();
    }

    private loadEmployee(): ng.IPromise<any> {
        return this.sharedTimeService.getEmployee(this.employeeId, null, null, false, true, true, true).then((result) => {
            let employee = new SmallGenericType(result.employeeId, '({0}) {1}'.format(result.employeeNr, result.name));
            let emp = new EmployeeListDTO();
            emp.employeeId = result.employeeId;
            emp.employeeNr = result.employeeNr;
            emp.name = result.name;

            this.employees = [];
            this.employees.push(employee);
            this.employeeList = [];
            this.employeeList.push(emp);

            this.selectedEmployee = employee;
        });
    }

    private loadAttestEmployees(): ng.IPromise<any> {
        // Prevent that the current user is in the list when creating a new request
        let excludeCurrentUserEmployee = this.isEmployeeRequestGuiMode() && (this.employeeRequestId == undefined || this.employeeRequestId == 0);
        return this.sharedTimeService.getEmployeesForAbsencePlanning(this.selectedDateFrom || this.dateFrom, this.selectedDateTo || this.dateTo, this.employeeId, excludeCurrentUserEmployee, this.timeScheduleScenarioHeadId ? this.timeScheduleScenarioHeadId : 0).then(x => {
            this.employeeList = x;
        });
    }

    private loadTimeDeviationCauseRequests(): ng.IPromise<any> {
        if (this.isEmployeeMode()) {
            return this.sharedTimeService.getTimeDeviationCauseRequests(this.employeeId, this.employeeGroupId).then((x) => {
                this.deviationCauses = x;
            });
        }
        else {
            if (this.employeeId > 0) {
                return this.sharedTimeService.getAbsenceTimeDeviationCausesFromEmployeeId(this.employeeId, this.selectedDateFrom ? this.selectedDateFrom : new Date(Date.now()), this.isEmployeeMode()).then((x) => {
                    this.deviationCauses = x
                });
            }
            else {
                return this.sharedTimeService.getAbsenceTimeDeviationCauses().then((x) => {
                    this.deviationCauses = x;
                });
            }
        }
    }

    private loadDeviationCauseForRegularAbsence(): ng.IPromise<any> {
        return this.sharedTimeService.getAbsenceTimeDeviationCausesFromEmployeeId(this.employeeId, this.selectedDateFrom ? this.selectedDateFrom : new Date(Date.now()), this.isEmployeeMode()).then((x) => {
            this.deviationCauses = x;
        });
    }

    private loadSelectedDeviationCause(): ng.IPromise<any> {
        return this.sharedTimeService.getTimeDeviationCause(this.employeeRequest.timeDeviationCauseId).then(x => {
            // Add the deviation cause to the list
            this.deviationCauses.push(x);
        });
    }

    private loadEmployeeChilds(): ng.IPromise<any> {
        return this.sharedTimeService.getEmployeeChildsDict(this.employeeId, false).then(x => {
            this.employeeChilds = x;
        });
    }

    private reLoadEmployeeChilds(): ng.IPromise<any> {
        return this.sharedTimeService.getEmployeeChildsDict(this.employeeId, false).then(x => {
            this.employeeChilds = x;
            if (this.employeeChilds.filter(ec => ec.id !== 0).length === 1) {
                this.selectedEmployeeChild = _.first(this.employeeChilds.filter(c => c.id !== 0));
            }
        });
    }

    private loadReadOnlyPermissions(): ng.IPromise<any> {
        var features: number[] = [];
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Vacation);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Vacation);

        return this.coreService.hasReadOnlyPermissions(features).then((x) => {
            let vacationPermissionMySelfRead = x[Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Vacation];
            let vacationPermissionOtherEmployeesRead = x[Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Vacation];

            this.vacationPermission = (this.isEmployeeMode() && vacationPermissionMySelfRead || vacationPermissionOtherEmployeesRead);
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.TimeSchedulePlanningSendXEMailOnChange);
        settingTypes.push(CompanySettingType.TimeSetApprovedYesAsDefault);
        settingTypes.push(CompanySettingType.TimeOnlyNoReplacementIsSelectable);
        settingTypes.push(CompanySettingType.AbsenceRequestPlanningIncludeNoteInMessages);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.sendXEMailOnChange = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSchedulePlanningSendXEMailOnChange);
            this.setApprovedYesAsDefault = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSetApprovedYesAsDefault);
            this.onlyNoReplacementIsSelectable = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeOnlyNoReplacementIsSelectable);
            this.includeNoteInMessages = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.AbsenceRequestPlanningIncludeNoteInMessages);
        });
    }

    private loadHiddenEmployeeId(): ng.IPromise<any> {
        return this.sharedScheduleService.getHiddenEmployeeId().then(x => {
            this.hiddenEmployeeId = x;
        });
    }

    private loadApprovalTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.YesNo, true, false).then(x => {
            this.approvalTypes = x;
        });
    }

    private loadTimeScheduleScenarioHead(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        if (this.timeScheduleScenarioHeadId && this.timeScheduleScenarioHeadId > 0) {
            this.sharedScheduleService.getScenarioHead(this.timeScheduleScenarioHeadId, false, false).then((x) => {
                this.timeScheduleScenarioHead = x;
                deferral.resolve();
            });
        } else {

            deferral.resolve();
        }
        return deferral.promise;
    }

    //--------------------------//

    //------Absencerequest-----//
    private loadAbsenceRequest(populateRequest: boolean, showIntersectMessage: boolean, loadRequestFromIntervall: boolean = false): ng.IPromise<any> {
        var deferral = this.$q.defer();
        if (this.employeeRequestId > 0) {
            this.sharedScheduleService.getAbsenceRequest(this.employeeRequestId).then((x) => {
                this.isNew = false;
                this.employeeRequest = x;
                if (populateRequest)
                    this.populateRequest();

                if (showIntersectMessage && this.employeeRequest.requestIntersectsWithCurrent) {
                    this.showRequestIntersectsWithExisting(this.employeeRequest.intersectMessage);
                }

                deferral.resolve();
            });
        } else if (loadRequestFromIntervall) {
            this.sharedScheduleService.getEmployeeRequestFromDateInterval(this.employeeId, this.dateFrom, this.dateTo, TermGroup_EmployeeRequestType.AbsenceRequest).then((x) => {
                this.employeeRequest = x;
                if (this.employeeRequest) {
                    this.isNew = false;
                    this.employeeRequestId = this.employeeRequest.employeeRequestId;
                    if (showIntersectMessage && this.employeeRequest.requestIntersectsWithCurrent) {
                        this.showRequestIntersectsWithExisting(this.employeeRequest.intersectMessage);
                    }
                }
                else {
                    this.isNew = true;
                    this.newRequest();
                }

                deferral.resolve();
            });
        } else {
            this.newRequest();
            deferral.resolve();
        }
        return deferral.promise;
    }

    private preSaveRequest() {
        if (this.isEmployeeMode()) {
            // Validate policy            
            this.sharedScheduleService.validateDeviationCausePolicy(this.employeeRequest, this.employeeId, TermGroup_EmployeeRequestType.AbsenceRequest).then(result => {
                if (result.success) {
                    if (!result.infoMessage)
                        this.saveRequest();
                    else {
                        var title: string = result.infoMessage ? result.infoMessage : '';
                        var message: string = result.errorMessage ? result.errorMessage : '';

                        var modal = this.notificationService.showDialog(title, message, SOEMessageBoxImage.Information, result.canUserOverride ? SOEMessageBoxButtons.OKCancel : SOEMessageBoxButtons.OK);
                        modal.result.then(val => {
                            if (result.canUserOverride) {
                                this.saveRequest();
                            }
                            else {
                                this.saveInProgress = false;
                            }
                        }, (reason) => {
                            this.saveInProgress = false;
                        });
                    }
                } else {
                    this.saveInProgress = false;
                    this.notificationService.showDialog("", result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                }
            });
        } else {
            this.saveRequest();
        }
    }

    private saveRequest() {
        this.progress.startSaveProgress((completion) => {
            this.employeeRequest.actorCompanyId = CoreUtility.actorCompanyId;
            this.sharedScheduleService.saveAbsenceRequest(this.employeeRequest, this.employeeId, TermGroup_EmployeeRequestType.AbsenceRequest, this.skipXEMailOnShiftChanges, this.isForcedDefinitive).then((result) => {

                this.saveInProgress = false;
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.employeeRequestId = result.integerValue;

                    this.employeeRequest.employeeRequestId = this.employeeRequestId;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.employeeRequest);
                    this.dirtyHandler.clean();

                    if (this.getShiftsToPlan().length > 0 && this.isAttestRoleMode()) {
                        if (result.booleanValue === true && this.isNew) {
                            this.showRequestIntersectsWithExisting(result.ErrorMessage);
                        }
                        this.prePerformRequest();
                    } else {
                        this.loadAbsenceRequest(true, this.isNew);
                    }
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
            }, error => {
            });
    }

    //private saveDeviations() {
    //    this.prePerformSaveDeviationsFromShifts();
    //}

    private evaluateWorkRules(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        var action: TermGroup_ShiftHistoryType = (this.guiMode === AbsenceRequestGuiMode.AbsenceDialog ? TermGroup_ShiftHistoryType.AbsencePlanning : TermGroup_ShiftHistoryType.AbsenceRequestPlanning);

        this.sharedScheduleService.evaluateAbsenceRequestPlanningAgainstWorkRules(this.employeeRequest.employeeId, this.getShiftsToPlan(), null, this.timeScheduleScenarioHeadId).then(result => {
            this.notificationService.showValidateWorkRulesResult(action, result, this.employeeRequest.employeeId).then(passed => {
                if (!passed)
                    this.saveInProgress = false;

                deferral.resolve(passed);
            });
        });

        return deferral.promise;
    }

    private performRequest() {
        var affectedEmployeeIds: number[] = this.getAffectedEmployeeIds();
        this.progress.startSaveProgress((completion) => {
            this.sharedScheduleService.performAbsenceRequestPlanningAction(this.employeeRequest.employeeRequestId, this.getShiftsToPlan(), this.skipXEMailOnShiftChanges, this.timeScheduleScenarioHeadId).then((result) => {
                if (result.success) {
                    if (this.isModal) {
                        completion.completed();
                        var modal = this.notificationService.showDialog(this.terms["core.info"], this.terms["time.schedule.absencerequests.absencesavedmessage"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                        modal.result.then(val => {
                            this.closeModal(true, affectedEmployeeIds);
                        }, (reason) => {
                            this.closeModal(true, affectedEmployeeIds);
                        });
                    } else {
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.employeeRequest);
                        this.loadAbsenceRequest(true, false);
                    }
                } else {
                    completion.failed(result.errorMessage);
                    this.saveInProgress = false;
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
        }, error => {
        });
    }

    private saveDeviationsFromShifts() {
        this.progress.startSaveProgress((completion) => {
            var affectedEmployeeIds: number[] = this.getAffectedEmployeeIds();

            this.sharedScheduleService.performAbsencePlanningAction(this.employeeRequest, this.getShiftsToPlan(), true, this.skipXEMailOnShiftChanges, this.timeScheduleScenarioHeadId).then((result) => {
                if (result.success) {
                    completion.completed();
                    this.closeModal(true, affectedEmployeeIds);
                    //var modal = this.notificationService.showDialog(this.terms["core.info"], this.terms["time.schedule.absencerequests.absencesavedmessage"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                    //modal.result.then(val => {
                    //    this.closeModal(true, affectedEmployeeIds);
                    //}, (reason) => {
                    //    this.closeModal(true, affectedEmployeeIds);
                    //});
                } else {
                    completion.failed(result.errorMessage);
                    this.saveInProgress = false;
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.saveInProgress = false;
            }, error => {
            });
    }

    private getShiftsIsIncludedInAbsenceRequestWarningMessage() {
        this.progress.startSaveProgress((completion) => {
            return this.sharedScheduleService.getShiftsIsIncludedInAbsenceRequestWarningMessage(this.employeeRequest.employeeId, this.getShiftsToPlan()).then(result => {
                completion.completed();
                if (result.success) {
                    // Validate work rules
                    this.evaluateWorkRules().then(passed => {
                        if (passed)
                            this.saveDeviationsFromShifts();
                    });
                } else {
                    var message = result.errorMessage + this.terms["time.schedule.absencerequests.shiftsincludedinabsencerequestmessage"];
                    var modal = this.notificationService.showDialog(this.terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                    modal.result.then(val => {
                        // Validate work rules
                        this.evaluateWorkRules().then(passed => {
                            if (passed)
                                this.saveDeviationsFromShifts();
                        });
                    }, (reason) => {
                        console.log("abort");
                    });
                }
            });
        }, this.guid)
            .then(data => {
            }, error => {
            });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.sharedScheduleService.deleteEmployeeRequest(this.employeeRequestId).then((result) => {
                if (result.success) {
                    completion.completed(this.employeeRequest);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.dirtyHandler.clean();
            this.closeMe(false);
        });
    }

    //------------------------//

    //------Affected Shifts---//
    private loadShiftsForAbsencePlanning(employeeId: number, shiftId: number, includeLinkedShifts: boolean, getAllshiftsForDay: boolean) {
        var deferral = this.$q.defer();

        this.sharedScheduleService.getShiftsForAbsencePlanning(employeeId, shiftId, includeLinkedShifts, getAllshiftsForDay, this.selectedDeviationCause ? this.selectedDeviationCause.timeDeviationCauseId : 0, this.timeScheduleScenarioHeadId).then(x => {

            this.affectedShiftsLoaded(x);
            deferral.resolve();
        });

        return deferral.promise;
    }

    private loadShiftsForAbsencePlanningFromSelectedDays(employeeId: number, days: Date[]) {
        var deferral = this.$q.defer();

        this.sharedScheduleService.getAbsenceRequestAffectedShiftsFromSelectedDays(employeeId, days, this.selectedDeviationCause ? this.selectedDeviationCause.timeDeviationCauseId : 0, this.timeScheduleScenarioHeadId).then(x => {

            this.affectedShiftsLoaded(x);
            deferral.resolve();
        });

        return deferral.promise;
    }

    private loadAbsenceAffectedShiftsFromAbsenceRequest() {
        if (!this.employeeRequest || this.employeeRequest.employeeRequestId == 0)
            return;

        var deferral = this.$q.defer();

        this.sharedScheduleService.getAbsenceRequestAffectedShifts(this.employeeRequest, this.employeeRequest.extendedSettings, TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested, this.timeScheduleScenarioHeadId).then(x => {

            this.affectedShiftsLoaded(x);

            deferral.resolve();
        });

        return deferral.promise;
    }

    private loadAbsenceAffectedShifts(employeeId: number, dateFrom: Date, dateTo: Date, timeDeviationCauseId: number, extendedSettings: ExtendedAbsenceSettingDTO, includeAlreadyAbsence: boolean) {
        var deferral = this.$q.defer();

        this.sharedScheduleService.getAbsenceAffectedShifts(employeeId, dateFrom, dateTo, timeDeviationCauseId, extendedSettings, includeAlreadyAbsence, this.timeScheduleScenarioHeadId).then(x => {

            this.affectedShiftsLoaded(x);
            deferral.resolve();
        });

        return deferral.promise;
    }

    //------------------------//

    //------History----------//
    private loadAbsenceRequestHistory() {
        var deferral = this.$q.defer();
        if (this.employeeRequest.employeeRequestId > 0) {
            this.sharedScheduleService.getAbsenceRequestHistory(this.employeeRequestId).then((x) => {
                this.historyShifts = x;
                _.forEach(this.historyShifts, (shift) => {
                    shift.toStart = CalendarUtility.convertToDate(shift.toStart);
                    shift.toStop = CalendarUtility.convertToDate(shift.toStop);
                });
                deferral.resolve();
            });
        } else {
            deferral.resolve();
        }
        return deferral.promise;
    }

    private restoreAbsenceRequestedShifts(setRequestAsPending: boolean) {
        this.progress.startSaveProgress((completion) => {

            this.sharedScheduleService.performRestoreAbsenceRequestedShifts(this.employeeRequest.employeeRequestId, setRequestAsPending).then((result) => {
                if (result.success) {
                    completion.completed(null, null, false, this.terms["time.schedule.absencerequests.restoresuccessinfo"].format(this.selectedEmployee.name));
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.loadAbsenceRequest(true, false);
            }, error => {
            });
    }

    //-----------------------//

    //GUI:Events
    private updateAffectedShifts() {
        this.dirtyHandler.setDirty();
        this.UpdateRequest();

        this.selectedApproveAllApprovalTypeId = (this.isTimeAttestMode() || this.setApprovedYesAsDefault) ? TermGroup_YesNo.Yes : TermGroup_YesNo.Unknown;
        this._selectedReplaceAllWithEmployee = undefined;

        if (!this.selectedEmployee || !this.selectedDeviationCause) {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["time.schedule.absencerequests.updateaffectedshiftswarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            return;
        } else {
            var extendsettings = (this.selectedAbsenceType == AbsenceType.Parttime) ? this.employeeRequest.extendedSettings : null;
            this.loadAbsenceAffectedShifts(this.employeeId, this.employeeRequest.start, this.employeeRequest.stop, this.employeeRequest.timeDeviationCauseId, extendsettings, this.isRegularAbsenceGuiMode());
        }

        if (this.showStartAndStopDate) {
            this.loadAttestEmployees();
        }
    }

    private replaceWithEmployeeOnFocus(shift: ShiftDTO) {
        this.replaceWithEmployees = [];
        if (this.onlyNoReplacementIsSelectable === false) {
            if (shift) {
                if ((shift.actualStartTime.diffMinutes(shift.actualStopTime) === 0) && _.find(this.employeeList, e => e.employeeId === Constants.NO_REPLACEMENT_EMPLOYEEID)) {
                    let emp = _.find(this.employeeList, e => e.employeeId === Constants.NO_REPLACEMENT_EMPLOYEEID)
                    let employee = new SmallGenericType(emp.employeeId, emp.name);
                    this.replaceWithEmployees.push(employee);
                } else {
                    _.forEach(this.employeeList, (emp: EmployeeListDTO) => {
                        if (this.employeeId !== emp.employeeId) {
                            if (emp.hasEmployment(CalendarUtility.convertToDate(shift.startTime.date()), CalendarUtility.convertToDate(shift.startTime.date())) || emp.employeeId === Constants.NO_REPLACEMENT_EMPLOYEEID || emp.employeeId === this.hiddenEmployeeId) {
                                let employee = new SmallGenericType(emp.employeeId, (emp.employeeId === Constants.NO_REPLACEMENT_EMPLOYEEID || emp.employeeId === this.hiddenEmployeeId) ? emp.name : '({0}) {1}'.format(emp.employeeNr, emp.name));
                                this.replaceWithEmployees.push(employee);
                            }
                        }
                    });
                }
            }
        } else {
            var employeeNoReplacement = _.find(this.employeeList, e => e.employeeId === Constants.NO_REPLACEMENT_EMPLOYEEID);
            this.replaceWithEmployees.push(new SmallGenericType(employeeNoReplacement.employeeId, employeeNoReplacement.name));
        }
    }

    private replaceWithEmployeeChanged() {
        this.dirtyHandler.setDirty();
    }

    private replaceWithAllApprovalTypeChanged(item: number) {
        var employeeNoReplacement = _.find(this.employeeList, e => e.employeeId === Constants.NO_REPLACEMENT_EMPLOYEEID);
        _.forEach(this.affectedShifts, (shift: ShiftDTO) => {
            if (!shift.isLended) {
                shift.approvalTypeId = item;
                if (shift.approvalTypeId === TermGroup_YesNo.Yes && shift.actualStartTime.diffMinutes(shift.actualStopTime) === 0) {
                    if (employeeNoReplacement) {
                        var emp = new SmallGenericType(employeeNoReplacement.employeeId, employeeNoReplacement.name);
                        shift.replaceWithEmployee = emp;
                    }
                }
            }
        });
    }

    private replaceAllWithEmployeeChanged(item: SmallGenericType) {
        this.dirtyHandler.setDirty();

        var employee = item ? _.find(this.employeeList, e => e.employeeId === item.id) : undefined;
        var employeeNoReplacement = _.find(this.employeeList, e => e.employeeId === Constants.NO_REPLACEMENT_EMPLOYEEID);

        _.forEach(this.affectedShifts, (shift: ShiftDTO) => {
            if (!shift.isLended) {
                if (shift.actualStartTime.diffMinutes(shift.actualStopTime) === 0) {
                    if (employeeNoReplacement) {
                        let emp = new SmallGenericType(employeeNoReplacement.employeeId, employeeNoReplacement.name);
                        shift.replaceWithEmployee = emp;
                    }
                } else {
                    if (employee && (employee.hasEmployment(CalendarUtility.convertToDate(shift.startTime.date()), CalendarUtility.convertToDate(shift.startTime.date())) || employee.employeeId === Constants.NO_REPLACEMENT_EMPLOYEEID || employee.employeeId === this.hiddenEmployeeId)) {
                        let emp = new SmallGenericType(employee.employeeId, (employee.employeeId === Constants.NO_REPLACEMENT_EMPLOYEEID || employee.employeeId === this.hiddenEmployeeId) ? employee.name : '({0}) {1}'.format(employee.employeeNr, employee.name));
                        shift.replaceWithEmployee = emp;
                    } else {
                        shift.replaceWithEmployee = null;
                    }
                }
            }
        });
    }

    private save() {
        this.saveInProgress = true;
        if (this.settingsIsDirty) {
            this.saveInProgress = false;
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["time.schedule.absencerequests.performrequestwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);

            return;
        }

        this.UpdateRequest();

        if (this.employeeRequest.stop.date().isBeforeOnDay(this.employeeRequest.start.date())) {
            this.saveInProgress = false;
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["time.schedule.absencerequests.invaliddates"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);

            return;
        } else {
            if (this.isEmployeeRequestGuiMode())
                this.preSaveRequest();
            else if (this.isRegularAbsenceGuiMode())
                this.prePerformSaveDeviationsFromShifts();
        }
    }

    //------HISTORY------------//

    private executeRestoreFunction(option) {
        switch (option.id) {
            case RestoreOptions.RestoreAbsence:
                this.restoreAbsenceRequestedShifts(false);
                break;
            case RestoreOptions.RestoreAbsenceAndSetRequestToPending:
                this.restoreAbsenceRequestedShifts(true);
                break;
        }
    }

    //------------------------//

    //------EXTENDED SETTINGS--------//

    private percentalAbsenceIsClicked() {
        if (this.employeeRequest.extendedSettings.percentalAbsence === true)
            this.employeeRequest.extendedSettings.adjustAbsencePerWeekDay = false;

        this.selectedPercentageAbsenceType = PercentageAbsenceType.BeginningOfDay;
        this.setSettingsIsDirty();
    }

    private adjustAbsencePerWeekDayIsClicked() {
        if (this.employeeRequest.extendedSettings.adjustAbsencePerWeekDay === true)
            this.employeeRequest.extendedSettings.percentalAbsence = false;

        this.setSettingsIsDirty();
    }

    private setExtendSettingsHasChanged() {
        this.setSettingsIsDirty();
    }

    //--------------------------------//

    //Help methods

    private setSettingsIsDirty() {
        if (this.showUpdateShiftsButton() && !this.onlySelectedWholeDaysMode())
            this.settingsIsDirty = true;
    }

    private setSettingsIsNotDirty() {
        this.settingsIsDirty = false;
    }

    private affectedShiftsLoaded(shifts: any) {

        // Convert to typed DTOs
        this.affectedShifts = shifts.map(s => {
            var obj = new ShiftDTO(s.type);
            angular.extend(obj, s);
            obj.employeeId = 0;
            obj.fixDates();
            obj.setTimesForSave();
            return obj;
        });

        _.forEach(this.affectedShifts, (shift: ShiftDTO) => {
            if (!shift.isLended) {
                if (this.isTimeAttestMode() || this.setApprovedYesAsDefault)
                    shift.approvalTypeId = TermGroup_YesNo.Yes;

                if (this.isTimeAttestMode() || this.onlyNoReplacementIsSelectable) {
                    var employeeNoReplacement = _.find(this.employeeList, e => e.employeeId === Constants.NO_REPLACEMENT_EMPLOYEEID);
                    if (employeeNoReplacement) {
                        let emp = new SmallGenericType(employeeNoReplacement.employeeId, employeeNoReplacement.name);
                        shift.replaceWithEmployee = emp;
                    }
                    else {
                        //only role employee will end up in here
                        let emp = new SmallGenericType(Constants.NO_REPLACEMENT_EMPLOYEEID, "");
                        shift.replaceWithEmployee = emp;
                    }
                }
            }
        });
        this.setSettingsIsNotDirty();
    }

    private tryUpdateSilent() {
        if (this.onlySelectedWholeDaysMode())
            this.loadShiftsForAbsencePlanningFromSelectedDays(this.employeeId, this.selectedDates);
        else if (this.selectedAbsenceType === AbsenceType.SelectedShifts)
            this.loadShiftsForAbsencePlanning(this.employeeId, this.shiftId, true, this.hideOptionSelectedShift);
    }

    private UpdateRequest() {

        if (!this.employeeRequest)
            this.employeeRequest = new EmployeeRequestDTO;

        if (this.selectedAbsenceType === AbsenceType.Parttime)
            this.updateExtendSettings();
        else
            this.employeeRequest.extendedSettings = null;

        this.employeeRequest.start = this.employeeRequest.start.beginningOfDay();
        this.employeeRequest.stop = this.employeeRequest.stop.beginningOfDay().addDays(1).addSeconds(-1);

        if (this.employeeRequest.employeeChildId && this.employeeRequest.employeeChildId === 0)
            this.employeeRequest.employeeChildId = null;

        if (this.employeeRequest.employeeRequestId === 0 || this.employeeRequest.status === TermGroup_EmployeeRequestStatus.None)
            this.employeeRequest.status = TermGroup_EmployeeRequestStatus.RequestPending;

        if (this.selectedDeviationCause)
            this.employeeRequest.timeDeviationCauseName = this.selectedDeviationCause.name;

        if (this.showSetAsDefinitive() && this.setAsDefinitive) {
            this.employeeRequest.status = TermGroup_EmployeeRequestStatus.Definate;
            this.isForcedDefinitive = true;
        }
    }

    private updateExtendSettings() {
        if (this.employeeRequest.extendedSettings.absenceFirstDayStart)
            this.employeeRequest.extendedSettings.absenceFirstDayStart = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.absenceFirstDayStart.clearSeconds());
        else
            this.employeeRequest.extendedSettings.absenceFirstDayStart = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.absenceLastDayStart)
            this.employeeRequest.extendedSettings.absenceLastDayStart = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.absenceLastDayStart.clearSeconds());
        else
            this.employeeRequest.extendedSettings.absenceLastDayStart = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceAllDaysStart)
            this.employeeRequest.extendedSettings.adjustAbsenceAllDaysStart = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceAllDaysStart.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceAllDaysStart = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceAllDaysStop)
            this.employeeRequest.extendedSettings.adjustAbsenceAllDaysStop = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceAllDaysStop.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceAllDaysStop = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceMonStart)
            this.employeeRequest.extendedSettings.adjustAbsenceMonStart = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceMonStart.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceMonStart = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceMonStop)
            this.employeeRequest.extendedSettings.adjustAbsenceMonStop = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceMonStop.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceMonStop = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceTueStart)
            this.employeeRequest.extendedSettings.adjustAbsenceTueStart = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceTueStart.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceTueStart = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceTueStop)
            this.employeeRequest.extendedSettings.adjustAbsenceTueStop = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceTueStop.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceTueStop = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceWedStart)
            this.employeeRequest.extendedSettings.adjustAbsenceWedStart = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceWedStart.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceWedStart = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceWedStop)
            this.employeeRequest.extendedSettings.adjustAbsenceWedStop = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceWedStop.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceWedStop = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceThuStart)
            this.employeeRequest.extendedSettings.adjustAbsenceThuStart = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceThuStart.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceThuStart = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceThuStop)
            this.employeeRequest.extendedSettings.adjustAbsenceThuStop = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceThuStop.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceThuStop = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceFriStart)
            this.employeeRequest.extendedSettings.adjustAbsenceFriStart = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceFriStart.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceFriStart = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceFriStop)
            this.employeeRequest.extendedSettings.adjustAbsenceFriStop = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceFriStop.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceFriStop = Constants.DATETIME_DEFAULT.beginningOfDay()

        if (this.employeeRequest.extendedSettings.adjustAbsenceSatStart)
            this.employeeRequest.extendedSettings.adjustAbsenceSatStart = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceSatStart.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceSatStart = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceSatStop)
            this.employeeRequest.extendedSettings.adjustAbsenceSatStop = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceSatStop.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceSatStop = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceSunStart)
            this.employeeRequest.extendedSettings.adjustAbsenceSunStart = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceSunStart.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceSunStart = Constants.DATETIME_DEFAULT.beginningOfDay();

        if (this.employeeRequest.extendedSettings.adjustAbsenceSunStop)
            this.employeeRequest.extendedSettings.adjustAbsenceSunStop = Constants.DATETIME_DEFAULT.mergeTime(this.employeeRequest.extendedSettings.adjustAbsenceSunStop.clearSeconds());
        else
            this.employeeRequest.extendedSettings.adjustAbsenceSunStop = Constants.DATETIME_DEFAULT.beginningOfDay();
    }

    public trySetAbsenceFirstAndLastDayAsChecked() {
        if (this.employeeRequest && this.employeeRequest.extendedSettings && this.employeeRequest.extendedSettings.extendedAbsenceSettingId && this.employeeRequest.extendedSettings.extendedAbsenceSettingId !== 0)
            return;

        if ((this.employeeRequest.extendedSettings.absenceFirstAndLastDay === true) ||
            (this.employeeRequest.extendedSettings.adjustAbsencePerWeekDay === true) ||
            (this.employeeRequest.extendedSettings.percentalAbsence === true)) {
            return;
        }

        this.employeeRequest.extendedSettings.absenceFirstAndLastDay = true;
        this.employeeRequest.extendedSettings.absenceWholeFirstDay = false;
        this.employeeRequest.extendedSettings.absenceWholeLastDay = false;
    }

    private tryReLoadEmployeeChilds() {
        if (this.employeeId <= 0)
            return;

        if (!this.selectedDeviationCause || (this.selectedDeviationCause && this.selectedDeviationCause.specifyChild === false))
            return;

        this.reLoadEmployeeChilds();
    }

    private tryReloadDeviationCauses() {
        return this.loadDeviationCauses();
    }

    private selectedDateFromChanged() {
        if (!this.selectedDateTo || this.selectedDateTo.isBeforeOnDay(this._selectedDateFrom))
            this.selectedDateTo = this.selectedDateFrom;

        if (this.isRegularAbsenceGuiMode())
            this.loadDeviationCauses();
        else {
            this.loadEmployeeGroupId().then(() => {
                this.loadDeviationCauses();
            });
        }

        this.setSettingsIsDirty();
    }

    private selectedDateToChanged() {
        this.setSettingsIsDirty();
    }

    private deviationCauseChanged() {
        if (this.selectedDeviationCause && this.selectedDeviationCause.onlyWholeDay) {
            this.selectedAbsenceType = AbsenceType.Fulltime;
        }
        this.tryReLoadEmployeeChilds();
        this.setSettingsIsDirty();
        if (this.selectedDeviationCause)
            this.tryUpdateSilent();
    }

    private employeeChanged() {
        this.tryReloadDeviationCauses();
        this.tryReLoadEmployeeChilds();
        this.setSettingsIsDirty();
    }

    private selectedAbsenceTypeChanged() {
        this.setSettingsIsDirty();
        if (this.selectedAbsenceType === AbsenceType.Fulltime) {

            if (this.isRegularAbsenceGuiMode() && this.selectedDeviationCause && this.selectedDateFrom && this.selectedDateTo)
                this.updateAffectedShifts();

        } else if (this.selectedAbsenceType === AbsenceType.Parttime) {

            if (!this.employeeRequest.extendedSettings)
                this.employeeRequest.extendedSettings = new ExtendedAbsenceSettingDTO;

            if (this.selectedDateFrom.date().isSameMinuteAs(this.selectedDateTo.date()))
                this.trySetAbsenceFirstAndLastDayAsChecked();

        } else if (this.selectedAbsenceType === AbsenceType.SelectedShifts) {
            this.loadShiftsForAbsencePlanning(this.employeeId, this.shiftId, true, this.hideOptionSelectedShift);
        }
    }

    private employeeChildChanged() {
    }

    private populateRequest() {
        if (this.employeeRequest) {
            if (this.employeeRequest.employeeRequestId > 0) {
                this._selectedDateFrom = this.employeeRequest.start;
                this._selectedDateTo = this.employeeRequest.stop;
                this._selectedEmployee = _.find(this.employees, e => e.id === this.employeeRequest.employeeId);
                this.employeeId = this.employeeRequest.employeeId;
                this._selectedDeviationCause = _.find(this.deviationCauses, e => e.timeDeviationCauseId === this.employeeRequest.timeDeviationCauseId);
                this._selectedEmployeeChild = _.find(this.employeeChilds, e => e.id === this.employeeRequest.employeeChildId);

                if (this.employeeRequest.extendedSettings) {
                    this.selectedAbsenceType = AbsenceType.Parttime;
                    this.populateExtendedSettings();
                }
            } else {
                this._selectedEmployee = _.find(this.employees, e => e.id === this.employeeRequest.employeeId);

                if (this.isRegularAbsenceGuiMode()) {
                    this._selectedDateFrom = this.dateFrom;
                    this._selectedDateTo = this.dateTo;
                    this.loadDeviationCauses();
                    if (this.hideOptionSelectedShift === false)
                        this._selectedAbsenceType = AbsenceType.SelectedShifts;
                } else {
                    this._selectedDateFrom = CalendarUtility.getDateNow();
                    this._selectedDateTo = CalendarUtility.getDateNow();
                }
            }
        }

        if (this.isEmployeeRequestGuiMode()) {
            this.setupAbsenceRequestPlanning();
            this.setupAbsenceRequestHistory();
        } else if (this.isRegularAbsenceGuiMode()) {
            this.setupAbsencePlanning();
        }
    }

    private setupAbsenceRequestPlanning() {
        if (!this.employeeRequest || this.employeeRequest.employeeRequestId == 0)
            return;

        if (this.employeeRequest.status == TermGroup_EmployeeRequestStatus.RequestPending)
            this.loadAbsenceAffectedShifts(this.employeeId, this.employeeRequest.start, this.employeeRequest.stop, this.employeeRequest.timeDeviationCauseId, this.employeeRequest.extendedSettings, false);
        else
            this.loadAbsenceAffectedShiftsFromAbsenceRequest();
    }

    private setupAbsencePlanning() {
        if (this.isTimeAttestMode()) {
            if (this.onlySelectedWholeDaysMode())
                this.loadShiftsForAbsencePlanningFromSelectedDays(this.employeeId, this.selectedDates);
            else {
                //no initial load
            }
        }
        else
            this.loadShiftsForAbsencePlanning(this.employeeId, this.shiftId, true, this.hideOptionSelectedShift);
    }

    private setupAbsenceRequestHistory() {
        this.loadAbsenceRequestHistory();
    }

    private newRequest() {
        this.isNew = true;
        this.employeeRequestId = 0;
        this.employeeRequest = new EmployeeRequestDTO;
        this.employeeRequest.employeeRequestId = 0;
        this.employeeRequest.employeeId = this.employeeId;
        this.employeeRequest.status = TermGroup_EmployeeRequestStatus.RequestPending;
        this.employeeRequest.resultStatus = TermGroup_EmployeeRequestResultStatus.None;
        this.employeeRequest.start = this.isRegularAbsenceGuiMode() ? this.dateFrom : CalendarUtility.getDateNow();
        this.employeeRequest.stop = this.isRegularAbsenceGuiMode() ? this.dateTo : CalendarUtility.getDateNow();
        this.employeeRequest.comment = ""
        this.employeeRequest.type = TermGroup_EmployeeRequestType.AbsenceRequest;

        this.employeeRequest.extendedSettings = new ExtendedAbsenceSettingDTO;
        this.employeeRequest.extendedSettings.absenceFirstDayStart = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.absenceLastDayStart = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceAllDaysStart = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceAllDaysStop = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceFriStart = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceFriStop = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceMonStart = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceMonStop = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceSatStart = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceSatStop = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceSunStart = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceSunStop = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceThuStart = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceThuStop = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceTueStart = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceTueStop = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceWedStart = Constants.DATETIME_DEFAULT.beginningOfDay();
        this.employeeRequest.extendedSettings.adjustAbsenceWedStop = Constants.DATETIME_DEFAULT.beginningOfDay();
    }

    private populateExtendedSettings() {
        if (this.employeeRequest) {
            if (!this.employeeRequest.extendedSettings) {
                this.employeeRequest.extendedSettings = new ExtendedAbsenceSettingDTO;
                this.employeeRequest.extendedSettings.absenceFirstAndLastDay = true;
            }

            //fix dates
            this.employeeRequest.extendedSettings.absenceFirstDayStart = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.absenceFirstDayStart);
            this.employeeRequest.extendedSettings.absenceLastDayStart = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.absenceLastDayStart);

            this.employeeRequest.extendedSettings.adjustAbsenceAllDaysStart = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceAllDaysStart);
            this.employeeRequest.extendedSettings.adjustAbsenceAllDaysStop = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceAllDaysStop);

            this.employeeRequest.extendedSettings.adjustAbsenceMonStart = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceMonStart);
            this.employeeRequest.extendedSettings.adjustAbsenceMonStop = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceMonStop);
            this.employeeRequest.extendedSettings.adjustAbsenceTueStart = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceTueStart);
            this.employeeRequest.extendedSettings.adjustAbsenceTueStop = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceTueStop);
            this.employeeRequest.extendedSettings.adjustAbsenceWedStart = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceWedStart);
            this.employeeRequest.extendedSettings.adjustAbsenceWedStop = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceWedStop);
            this.employeeRequest.extendedSettings.adjustAbsenceThuStart = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceThuStart);
            this.employeeRequest.extendedSettings.adjustAbsenceThuStop = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceThuStop);
            this.employeeRequest.extendedSettings.adjustAbsenceFriStart = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceFriStart);
            this.employeeRequest.extendedSettings.adjustAbsenceFriStop = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceFriStop);
            this.employeeRequest.extendedSettings.adjustAbsenceSatStart = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceSatStart);
            this.employeeRequest.extendedSettings.adjustAbsenceSatStop = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceSatStop);
            this.employeeRequest.extendedSettings.adjustAbsenceSunStart = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceSunStart);
            this.employeeRequest.extendedSettings.adjustAbsenceSunStop = CalendarUtility.convertToDate(this.employeeRequest.extendedSettings.adjustAbsenceSunStop);

            if (this.employeeRequest.extendedSettings.percentalAbsenceOccursStartOfDay)
                this.selectedPercentageAbsenceType = PercentageAbsenceType.BeginningOfDay
            else if (this.employeeRequest.extendedSettings.percentalAbsenceOccursEndOfDay)
                this.selectedPercentageAbsenceType = PercentageAbsenceType.EndOfDay
        }
    }

    private isRegularAbsenceGuiMode(): boolean {
        return this.guiMode === AbsenceRequestGuiMode.AbsenceDialog;
    }

    private isEmployeeRequestGuiMode(): boolean {
        return this.guiMode === AbsenceRequestGuiMode.EmployeeRequest;
    }

    private isEmployeeMode(): boolean {
        return this.viewMode === AbsenceRequestViewMode.Employee;
    }

    private isAttestRoleMode(): boolean {
        return this.viewMode === AbsenceRequestViewMode.Attest;
    }

    private isTimeAttestMode(): boolean {
        return this.parentMode == AbsenceRequestParentMode.TimeAttest;
    }

    private isSchedulePlanningMode(): boolean {
        return this.parentMode == AbsenceRequestParentMode.SchedulePlanning;
    }

    private isAbsenceInScenario(): boolean {
        if (this.timeScheduleScenarioHead)
            return true;
        else
            return false;
    }

    private onlySelectedWholeDaysMode(): boolean {
        return this.isRegularAbsenceGuiMode() && this.wholeDaysSelectedWithoutHoles === false && this.selectedDates && this.selectedDates.length > 0
    }

    private showStartAndStopDate(): boolean {
        if (this.selectedAbsenceType === AbsenceType.SelectedShifts)
            return false;

        if (this.onlySelectedWholeDaysMode())
            return false;

        //if (this.isRegularAbsenceGuiMode() && this.hideOptionSelectedShift === false)
        //    return false;

        return true;
    }

    private showOptionPartTime(): boolean {

        if (this.onlySelectedWholeDaysMode())
            return false;

        //if saved as parttime, then we have to show parttime option
        if (this.employeeRequest && this.employeeRequest.employeeRequestId > 0 && this.employeeRequest.extendedSettings)
            return true;

        if (this.selectedDeviationCause && this.selectedDeviationCause.onlyWholeDay === true)
            return false;

        return true;
    }

    private showOptionSelectedShifts(): boolean {

        if (this.onlySelectedWholeDaysMode())
            return false;

        if (this.selectedDeviationCause && this.selectedDeviationCause.onlyWholeDay === true)
            return false;

        if (this.isRegularAbsenceGuiMode() && this.hideOptionSelectedShift === false) {
            return true;
        }

        return false;
    }

    private showEmployeeChild(): boolean {
        if (this.selectedDeviationCause) {
            return (this.selectedDeviationCause.specifyChild === true);
        }
    }
    private showReactive(): boolean {
        return (this.isAttestRoleMode() && this.employeeRequest && this.employeeRequest.employeeRequestId != 0 && this.employeeRequest.status === TermGroup_EmployeeRequestStatus.RequestPending);
    }

    private showSkipXEMailOnShiftChanges(): boolean {
        return (!this.isAbsenceInScenario() && this.isAttestRoleMode() && this.sendXEMailOnChange === true);
    }

    private showSetAsDefinitive(): boolean {
        return (this.isAttestRoleMode() && this.employeeRequest && this.employeeRequest.employeeRequestId != 0 && this.employeeRequest.status !== TermGroup_EmployeeRequestStatus.Definate && this.employeeRequest.status !== TermGroup_EmployeeRequestStatus.Restored && this.affectedShifts && this.affectedShifts.length === 0)
    }

    private disableDeleteButton(): boolean {
        return (this.employeeRequest && (this.employeeRequest.status !== TermGroup_EmployeeRequestStatus.RequestPending && this.employeeRequest.status !== TermGroup_EmployeeRequestStatus.Restored))
    }

    private isRequestNotPending(): boolean {
        return (this.employeeRequest && this.employeeRequest.status !== TermGroup_EmployeeRequestStatus.RequestPending)
    }

    private isRequestDefinate(): boolean {
        return (this.employeeRequest && this.employeeRequest.status === TermGroup_EmployeeRequestStatus.Definate)
    }

    private isNoteRequired(): boolean {
        return (this.isEmployeeMode() && this.selectedDeviationCause && this.selectedDeviationCause.mandatoryNote === true)
    }

    private isNoteInvalid(): boolean {

        return (this.isNoteRequired() && (!this.employeeRequest.comment || this.employeeRequest.comment.length == 0))
    }

    private showUpdateShiftsButton(): boolean {
        return this.selectedAbsenceType !== AbsenceType.SelectedShifts && !this.readOnly;
    }

    private showApprovalTypeAndReplacewithEmployee() {
        return (this.isAttestRoleMode() && this.isSchedulePlanningMode())
    }

    private lockDownRequestPlanning(): boolean {
        return this.settingsIsDirty === true;
    }

    private getDayName(shiftHistory: any) {
        return CalendarUtility.getDayName(shiftHistory.toStart.dayOfWeek()).toUpperCaseFirstLetter();
    }

    private setupRestoreOptions() {
        this.restoreOptions.push({ id: RestoreOptions.RestoreAbsence, name: this.terms["time.schedule.absencerequests.restoreoptionsrestoreabsence"] });
        this.restoreOptions.push({ id: RestoreOptions.RestoreAbsenceAndSetRequestToPending, name: this.terms["time.schedule.absencerequests.restoreoptionsrestoreabsenceandsetrequesttopending"] });
    }

    private getShiftsToPlan(): ShiftDTO[] {
        var shifts: ShiftDTO[] = [];

        _.forEach(this.affectedShifts, (shift: ShiftDTO) => {
            if (!shift.isLended && (shift.approvalTypeId === TermGroup_YesNo.Yes && shift.employeeId !== 0) || shift.approvalTypeId === TermGroup_YesNo.No)
                shifts.push(shift)
        });

        return shifts;
    }

    private getAllAffectedShiftIds(): number[] {
        var ids: number[] = [];

        _.forEach(this.affectedShifts, (shift: ShiftDTO) => {
            if (!shift.isLended)
                ids.push(shift.timeScheduleTemplateBlockId);
        });

        return ids;
    }

    private getAffectedEmployeeIds(): number[] {
        var affectedEmployeeIds: number[] = _.uniqBy(_.map(_.filter(this.getShiftsToPlan(), s => s.employeeId > 0), b => b.employeeId), s => s);
        affectedEmployeeIds.push(this.employeeRequest.employeeId);
        return affectedEmployeeIds;
    }

    private prePerformRequest() {
        var result: boolean = true;
        var msg: string = "";

        _.forEach(this.affectedShifts, (shift: ShiftDTO) => {
            if (shift.approvalTypeId === TermGroup_YesNo.Yes && shift.employeeId === 0) {
                result = false;
                msg = this.terms["time.schedule.absencerequests.shiftsmissingreplacements"];
                return { result, msg };
            }

            if (shift.approvalTypeId === TermGroup_YesNo.Unknown && shift.employeeId !== 0) {
                result = false;
                msg = this.terms["time.schedule.absencerequests.shiftsmissingapproval"];
                return { result, msg };
            }
        });

        if (_.filter(this.affectedShifts, r => r.approvalTypeId === TermGroup_YesNo.Unknown).length == this.affectedShifts.length) {
            result = false;
            msg = this.terms["time.schedule.absencerequests.noneapprovedshifts"];
            return { result, msg };
        }

        if (result === true) {
            if (this.getShiftsToPlan().length === 0)
                return;

            // Validate work rules
            this.evaluateWorkRules().then(passed => {
                if (passed)
                    this.performRequest();
            });
        } else {
            this.notificationService.showDialog(this.terms["core.warning"], msg, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            return;
        }
    }

    private prePerformSaveDeviationsFromShifts() {

        var planShifts = this.getShiftsToPlan();
        var affectedShiftIds = this.getAllAffectedShiftIds();
        if (this.isRegularAbsenceGuiMode()) {

            if (affectedShiftIds.length == 0) {
                this.notificationService.showDialog(this.terms["core.warning"], this.terms["time.schedule.absence.noaffectedshifts"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                this.saveInProgress = false;
                return;
            }
        }

        if (planShifts.length == affectedShiftIds.length) {
            if (_.filter(planShifts, x => x.shiftUserStatus === TermGroup_TimeScheduleTemplateBlockShiftUserStatus.AbsenceRequested).length > 0) {
                this.getShiftsIsIncludedInAbsenceRequestWarningMessage();
            } else {
                // Validate work rules
                this.evaluateWorkRules().then(passed => {
                    if (passed)
                        this.saveDeviationsFromShifts();
                });
            }
        } else {
            this.notificationService.showDialog(this.terms["core.warning"], this.terms["time.schedule.absence.shiftsmissingapproval"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
            this.saveInProgress = false;
            return;
        }

    }

    private showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.employeeRequest) {
                if (!this.selectedDeviationCause) {
                    mandatoryFieldKeys.push("common.time.timedeviationcause");
                }
                if (!this.selectedEmployee) {
                    mandatoryFieldKeys.push("common.employee");
                }
                if (this.showEmployeeChild() && !this.selectedEmployeeChild) {
                    mandatoryFieldKeys.push("time.schedule.absencerequests.employeechild");
                }
                if (this.isNoteInvalid()) {
                    mandatoryFieldKeys.push("common.note");
                }
            }
        });
    }

    private showRequestIntersectsWithExisting(message: string) {
        this.notificationService.showDialog(this.terms["time.schedule.absencerequests.intersectattention"], message, SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
    }

    private closeModal(success: boolean, ids: number[]) {
        if (this.isModal) {
            if (success) {
                this.modal.close(ids);
            } else {
                this.modal.dismiss();
            }
        }
    }
    private getStyle(shift: ShiftDTO, checkStart: boolean): string {
        if (checkStart) {
            if (shift.actualStartTime.clearSeconds() < shift.absenceStartTime.clearSeconds() || shift.actualStartTime.clearSeconds() > shift.absenceStartTime.clearSeconds())
                return 'red';
        } else {
            if (shift.actualStopTime.clearSeconds() < shift.absenceStopTime.clearSeconds() || shift.actualStopTime.clearSeconds() > shift.absenceStopTime.clearSeconds())
                return 'red';
        }

        return '';
    }


    public showAbsenceDateAndTime(): boolean {
        return _.filter(this.affectedShifts, (shift) => (shift.actualStartTime.getDate() != shift.actualStopTime.getDate())).length > 0;
    }
}
