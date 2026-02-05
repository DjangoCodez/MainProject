import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { EmployeeUserDTO, EmployeeMeetingDTO, EmploymentDTO, EmployeeTaxSEDTO, UserRolesDTO, TimeWorkAccountDTO } from "../../../Common/Models/EmployeeUserDTO";
import { EmployeeListSmallDTO } from "../../../Common/Models/EmployeeListDTO";
import { IEmployeeSmallDTO, IAccountingSettingsRowDTO, IImagesDTO, ISmallGenericType, IFollowUpTypeGridDTO } from "../../../Scripts/TypeLite.Net4";
import { EmployeeService } from "../EmployeeService";
import { EmployeeService as SharedEmployeeService } from "../../../Shared/Time/Employee/EmployeeService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { NotificationService } from "../../../Core/Services/NotificationService";
import { IFocusService } from "../../../Core/Services/FocusService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Feature, TermGroup, CompanySettingType, UserSettingType, SoeEntityType, SoeEntityImageType, TermGroup_EmployeeDisbursementMethod, TermGroup_Sex, EmploymentAccountType, SaveEmployeeUserResult, TermGroup_SysContactEComType, ContactAddressItemType, DeleteEmployeeAction, SoeTimeCodeType, UserReplacementType, SoeEntityState, SoeDataStorageRecordType, LicenseSettingType, MatrixDataType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { EmployeePositionDTO } from "../../../Common/Models/EmployeePositionDTO";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { Constants } from "../../../Util/Constants";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { CoreUtility } from "../../../Util/CoreUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons, IconLibrary, EditUserFunctions } from "../../../Util/Enumerations";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { ContactAddressItemDTO } from "../../../Common/Models/ContactAddressDTOs";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { DeleteEmployeeController } from "./Dialogs/DeleteEmployee/DeleteEmployeeController";
import { ExportUtility } from "../../../Util/ExportUtility";
import { Guid } from "../../../Util/StringUtility";
import { UserReplacementDTO } from "../../../Common/Models/UserDTO";
import { IScopeWatcherService } from "../../../Core/Services/ScopeWatcherService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { FilesHelper } from "../../../Common/Files/FilesHelper";
import { IPayrollService } from "../../Payroll/PayrollService";
import { ExtraFieldRecordDTO } from "../../../Common/Models/ExtraFieldDTO";
import { PayrollLevelDTO } from "../../../Common/Models/PayrollLevelDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    private toolbarInclude: any;

    private employeeId: number;
    private employee: EmployeeUserDTO;
    private showInactive: boolean = false;
    private allEmployees: IEmployeeSmallDTO[];
    private filteredEmployees: IEmployeeSmallDTO[];
    private employeesForMeeting: EmployeeListSmallDTO[];
    private selectedEmployee: IEmployeeSmallDTO;

    private _selectedEmployment: EmploymentDTO;
    private set selectedEmployment(employment: EmploymentDTO) {
        // Accounting settings for previously selected employment has been modified,
        // copy settings to employment before selecting the new employment.
        if (this.isAccountingSettingsModified && this._selectedEmployment) {
            this._selectedEmployment.accountingSettings = this.settings;
        }

        this._selectedEmployment = employment;
        this.selectedVacationGroupId = employment.employmentVacationGroup && employment.employmentVacationGroup.length > 0 ? employment.employmentVacationGroup[0].vacationGroupId : 0;

        this.setupAccountingSettings();
    }
    private get selectedEmployment(): EmploymentDTO {
        return this._selectedEmployment
    }
    private selectedVacationGroupId: number = 0;

    private employeeTax: EmployeeTaxSEDTO;

    private userIsMySelf: boolean = false;
    private skillMatcherDate = CalendarUtility.getDateToday();
    private skillMatcherInvalid: boolean = false;
    private nbrOfSkills: number = 8;

    // Lookups
    private replacementUsers: any[];
    private replacementUser: UserReplacementDTO;
    private timeDeviationCauses: any[] = [];
    private timeCodes: any[];
    private payrollReportsPersonalCategories: any[];
    private payrollReportsWorkTimeCategories: any[];
    private payrollReportsPayrollExportSalaryTypes: any[];
    private payrollReportsAFACategories: any[];
    private payrollReportsAFASpecialAgreements: any[];
    private payrollReportsCollectumITPplans: any[];
    private kpaBelongings: ISmallGenericType[] = [];
    private bygglosenSalaryType: ISmallGenericType[] = [];
    private kpaEndCodes: ISmallGenericType[] = [];
    private employeePositions: EmployeePositionDTO[] = [];
    private employeePositionIds: number[] = [];
    private contactAddressItems: ContactAddressItemDTO[];
    private disbursementPaymentMethods: any[];
    private followUpTypes: IFollowUpTypeGridDTO[];
    private payrollLevels: PayrollLevelDTO[];
    private attestRolesForMeeting: any[];
    private userRoles: UserRolesDTO[];
    private userRolesHasChanges: boolean;
    private userAttestRolesHasChanges: boolean;
    private userHasChanges: boolean;
    private payrollPriceFormulas: ISmallGenericType[] = [];
    private kpaAgreementTypes: ISmallGenericType[] = [];
    private gtpAgreementNumbers: ISmallGenericType[] = [];
    private timeWorkAccounts: TimeWorkAccountDTO[];
    private ifPaymentCodes: ISmallGenericType[] = [];
    // License settings
    private showSso: boolean = false;

    // Company settings
    private useVacant: boolean = false;
    private defaultTimeCodeId: number = 0;
    private forceSocialSecNbr: boolean = false;
    private dontValidateSocialSecNbr: boolean = false;
    private usePayroll: boolean = false;
    private payrollGroupMandatory: boolean = false;
    private setNextEmployeeNumberAutomatically: boolean = false;
    private suggestEmployeeNrAsUsername: boolean = false;
    private useAccountsHierarchy: boolean = false;
    private defaultEmployeeAccountDimId: number = 0;
    private useLimitedEmployeeAccountDimLevels: boolean = false;
    private useExtendedEmployeeAccountDimLevels: boolean = false;
    private useHibernatingEmployment: boolean = false;
    private useSalaryPaymentExportExtendedSelection: boolean = false;
    private dontAllowIdenticalSSN: boolean = false

    private showLifetime: boolean;

    // User settings
    private sendEmailScheduleChanged: boolean = false;

    // Permissions
    private readPermissions: any[];
    private modifyPermissions: any[];

    private socialSecReadPermission = false;
    private socialSecModifyPermission = false;
    private cardNumberReadPermission: boolean = false;
    private cardNumberModifyPermission: boolean = false;
    private contactReadPermission: boolean = false;
    private contactModifyPermission: boolean = false;
    private hasAttestRoles: boolean = false;
    private allowSecretContactInfoPermission: boolean = false;
    private contactDisbursementAccountReadPermission: boolean = false;
    private contactDisbursementAccountModifyPermission: boolean = false;
    private userReadPermission: boolean = false;
    private userModifyPermission: boolean = false;
    private userMappingReadPermission: boolean = false;
    private userMappingModifyPermission: boolean = false;
    private attestRoleMappingReadPermission: boolean = false;
    private attestRoleMappingModifyPermission: boolean = false;
    private userReplacementReadPermission: boolean = false;
    private userReplacementModifyPermission: boolean = false;
    private noExtraShiftModifyPermission: boolean = false;

    private employmentDataReadPermission: boolean = false;
    private employmentDataModifyPermission: boolean = false;
    private employmentReadPermission: boolean = false;
    private employmentModifyPermission: boolean = false;
    private employmentPayrollReadPermission: boolean = false;
    private employmentPayrollModifyPermission: boolean = false;
    private employmentPayrollSalaryReadPermission: boolean = false;
    private employmentPayrollSalaryModifyPermission: boolean = false;
    private employmentAccountsReadPermission: boolean = false;
    private employmentAccountsModifyPermission: boolean = false;

    private workTimeAccountReadPermission: boolean = false;
    private workTimeAccountModifyPermission: boolean = false;

    private payrollAdditionsReadPermission: boolean = false;
    private payrollAdditionsModifyPermission: boolean = false;

    private scheduleDataReadPermission: boolean = false;
    private scheduleDataModifyPermission: boolean = false;

    private taxReadPermission: boolean = false;
    private taxModifyPermission: boolean = false;
    private employeeUnionFeeReadPermission: boolean = false;
    private employeeUnionFeeModifyPermission: boolean = false;

    private absenceVacationVacationReadPermission: boolean = false;
    private absenceVacationVacationModifyPermission: boolean = false;
    private absenceVacationAbsenceReadPermission: boolean = false;
    private absenceVacationAbsenceModifyPermission: boolean = false;
    private employeeChildReadPermission: boolean = false;
    private employeeChildModifyPermission: boolean = false;
    private openingBalanceUsedDaysModifyPermission: boolean = false;

    private reportsReadPermission: boolean = false;
    private reportsModifyPermission: boolean = false;

    private workRulesReadPermission: boolean = false;
    private workRulesModifyPermission: boolean = false;

    private categoriesReadPermission: boolean = false;
    private categoriesModifyPermission: boolean = false;

    private timeReadPermission: boolean = false;
    private timeModifyPermission: boolean = false;
    private timeCalculatedCostPerHourReadPermission: boolean = false;
    private timeCalculatedCostPerHourModifyPermission: boolean = false;

    private skillsReadPermission: boolean = false;
    private skillsModifyPermission: boolean = false;
    private employeeMeetingReadPermission: boolean = false;
    private employeeMeetingModifyPermission: boolean = false;

    private noteReadPermission: boolean = false;
    private noteModifyPermission: boolean = false;

    private hasExtraFieldPermission = false;

    private filesReadPermission: boolean = false;
    private filesModifyPermission: boolean = false;

    private gdprLogsReadPermission: boolean = false;
    private gdprLogsModifyPermission: boolean = false;

    private settings: IAccountingSettingsRowDTO[] = [];
    private settingTypes: SmallGenericType[] = [];
    private baseAccounts: SmallGenericType[] = [];

    // Attest role settings
    private hasAllowToAddOtherEmployeeAccounts = false;

    // Properties
    private searchMode: boolean = false;
    private isManuallyNew: boolean = false;
    private searchEmployeeCondition: string;
    private selectedEmployeeMeeting: EmployeeMeetingDTO;

    // Flags
    private usePayrollLevels: boolean = false;
    private showNavigation: boolean = true;
    private personalDataAccordionInitiallyOpen: boolean = false;
    private contactInfoAccordionInitiallyOpen: boolean = false;
    private employmentDataAccordionInitiallyOpen: boolean = false;
    private employmentAccordionInitiallyOpen: boolean = false;
    private documentAccordionOpened: boolean = false;
    private employeeTaxInitiallyOpen: boolean = false;
    private isLoginNameValid: boolean = false;
    private isSocialSecurityNumberValid: boolean = true;
    private isCheckingCardNumber: boolean = false;
    private externalAuthIdModified: boolean = false;
    private lifetimeSecondsModified: boolean = false;
    private isBankAccountValid: boolean = true;
    private isAccountingSettingsValid: boolean = true;
    private isEmploymentPriceTypesValid: boolean = true;
    private isAccountingSettingsModified: boolean;
    private isEmployeeVacationValid: boolean = true;
    private employeeVacationValidationErrors: string;
    private isContactAddressesValid: boolean = true;
    private contactAddressesValidationErrors: string;
    private scheduleDataIsOpen: boolean = false;
    private scheduleDataOpened: boolean = false;
    private employeeMeetingIsOpen: boolean = false;
    private employeeMeetingOpened: boolean = false;
    private employeeMiscIsOpen: boolean = false;
    private timeWorkAccountInitiallyOpen: boolean = false;

    // Files
    private filesHelper: FilesHelper;

    // Portrait
    public portrait: IImagesDTO;
    private portraitChanged: boolean = false;

    private modal;
    private isModal = false;
    private modalInstance: any;

    // Extra fields
    private extraFieldRecords: ExtraFieldRecordDTO[];
    get showExtraFieldsExpander() {
        return this.hasExtraFieldPermission;
    }
    extraFieldsExpanderRendered = false;

    //@ngInject
    constructor(
        private $scope: ng.IScope,
        protected $uibModal,
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private employeeService: EmployeeService,
        private sharedEmployeeService: SharedEmployeeService,
        private payrollService: IPayrollService,
        private coreService: ICoreService,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private notificationService: NotificationService,
        private messagingService: IMessagingService,
        private focusService: IFocusService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private scopeWatcherService: IScopeWatcherService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.toolbarInclude = urlHelperService.getGlobalUrl("Time/Employee/Employees/Views/editHeader.html");

        this.modalInstance = $uibModal;

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            if (parameters.source && parameters.source === 'employeeTemplate')
                return;

            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;

            this.showNavigation = false;

            this.onInit(parameters);
        });

        this.messagingService.subscribe(Constants.EVENT_CLOSE_DIALOG, (params) => {
            if (this.guid === params)
                this.closeModal(false);
        });
        this.messagingService.subscribe(Constants.EVENT_EMPLOYMENTS_RELOADED, (data: { employments: EmploymentDTO[], date: Date }) => {
            this.employmentsReloaded(data.employments, data.date);
        }, this.$scope);

        this.messagingService.subscribe('employeeGroupChanged', (data) => {
            if (data.employeeId !== this.employeeId || !data.employeeGroupId)
                return;

            if (this.employee.currentEmployeeGroupId !== <number>data.employeeGroupId || this.timeDeviationCauses.length === 0)
                this.loadTimeDeviationCauses(<number>data.employeeGroupId);
        }, this.$scope);

        this.messagingService.subscribe('employmentDateChanged', (data) => {
            if (this.employee.blockedFromDate && data) {

                const { newDateFrom, newDateTo } = data;

                if (newDateFrom.isAfterOnDay(this.employee.blockedFromDate) || (newDateTo && newDateTo.isAfterOnDay(this.employee.blockedFromDate)) || !newDateTo) {
                    this.translationService.translateMany(["time.employee.removeblockedfromdate.title", "time.employee.removeblockedfromdate.message"]).then(terms => {

                        const modal = this.notificationService.showDialogEx(terms["time.employee.removeblockedfromdate.title"], terms["time.employee.removeblockedfromdate.message"].format(this.employee.blockedFromDate.toFormattedDate()), SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
                        modal.result.then(val => {
                            if (val) {
                                this.employee.blockedFromDate = null;
                                this.employee.saveUser = true;
                                this.setDirty();
                            }
                        });
                    });
                }

            }
        }, this.$scope);

        this.messagingService.subscribe('reloadFieldsChangedByNewEmploymentFromTemplate', (data) => {
            if (data.employeeId !== this.employeeId)
                return;

            // If extra fields expander is open, close it.
            // It will refetch records on open.
            if (this.extraFieldsExpanderRendered)
                this.$scope.$broadcast('reloadExtraFields', { guid: this.guid, recordId: this.employeeId });

            this.loadEmployeePositions();
        }, this.$scope);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.messagingService.subscribe('employeePositionChanged', (data) => {
            if (data.employeeId !== this.employeeId)
                return;

            this.employeePositionIds = _.map(this.employeePositions, p => p.positionId);
        }, this.$scope);

        this.setTabCallbacks(this.onTabActivated, this.onTabDeActivated);
    }

    public onInit(parameters: any) {
        this.employeeId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.isManuallyNew = parameters.isManuallyNew;
        if (parameters.searchMode)
            this.searchMode = true;
        if (parameters.showInactive)
            this.showInactive = parameters.showInactive;

        this.filesHelper = new FilesHelper(this.coreService, this.$q, this.dirtyHandler, false, SoeEntityType.Employee, SoeEntityImageType.EmployeeFile, () => this.employeeId);

        this.flowHandler.start([{ feature: Feature.Time_Employee_Employees, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    public save() {
        this.saveInProgress = true;
        this.initSaveEmployeeUser();
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Employee_Employees].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_Employees].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false, null, () => this.isNew);
        if (CoreUtility.isSupportAdmin)
            this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("", "Debug (console.log)", IconLibrary.FontAwesome, "fa-debug", () => {
                console.log(this.employee)
            })));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("time.employee.employee.delete", "time.employee.employee.delete.title", IconLibrary.FontAwesome, "fa-user-secret", () => {
            this.openDeleteDialog();
        }, () => { return this.dirtyHandler.isDirty }, () => {
            return !this.gdprLogsModifyPermission || !this.employee || !this.employee.employeeId;
        })));
        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("common.download", "common.download", IconLibrary.FontAwesome, "fal fa-download", () => {
            this.exportEmployee();
        }, null, () => {
            return (!this.userIsMySelf && !this.gdprLogsModifyPermission) || !this.employeeId || this.employeeId === 0;
        })));
        this.toolbar.addInclude(this.toolbarInclude);

        if (this.showNavigation)
            this.loadAllEmployees();
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadLicenseSettings(),
            this.loadCompanySettings(),
            this.loadUserSettings(),
            this.loadReadOnlyPermissions(),
            this.getHasAttestRoles(),
            this.loadPayrollPriceFormulas(),
            this.loadKpaAgreementTypes(),
            this.loadGtpAgreementNumbers(),
            this.loadIFPaymentCodes(),
            this.loadModifyPermissions(),
            this.loadPayrollLevels(),
            this.loadPayrollReportsPersonalCategories(),
            this.loadPayrollReportsWorkTimeCategories(),
            this.loadPayrollReportsPayrollExportSalaryTypes(),
            this.loadPayrollReportsAFACategories(),
            this.loadPayrollReportsAFASpecialAgreements(),
            this.loadPayrollReportsCollectumITPplans(),
            this.loadKpaBelongings(),
            this.loadBygglosenSalaryType(),
            this.loadEndCodes()
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.employeeId) {
            return this.loadEmployee();
        } else if (!this.searchMode) {
            this.new();
        }
    }

    private setupAccountingSettings() {
        if (!this.selectedEmployment)
            return;

        this.settings = [];
        this.settingTypes = [];

        if (this.selectedEmployment.fixedAccounting) {
            // Calculate nbrOfFixedAccounts based on how many that exists (min 2)
            let nbrOfFixedAccounts: number = 2;
            if (this.hasEmploymentAccountingSetting(EmploymentAccountType.Fixed8))
                nbrOfFixedAccounts = 8;
            else if (this.hasEmploymentAccountingSetting(EmploymentAccountType.Fixed7))
                nbrOfFixedAccounts = 7;
            else if (this.hasEmploymentAccountingSetting(EmploymentAccountType.Fixed6))
                nbrOfFixedAccounts = 6;
            else if (this.hasEmploymentAccountingSetting(EmploymentAccountType.Fixed5))
                nbrOfFixedAccounts = 5;
            else if (this.hasEmploymentAccountingSetting(EmploymentAccountType.Fixed4))
                nbrOfFixedAccounts = 4;
            else if (this.hasEmploymentAccountingSetting(EmploymentAccountType.Fixed3))
                nbrOfFixedAccounts = 3;

            for (var i = 1; i <= nbrOfFixedAccounts; i++) {
                this.settingTypes.push(new SmallGenericType(i + 2, '{0} {1}'.format(this.terms["common.accountingsettings.fixed"], i.toString())));
                var acc = this.getEmploymentAccountingSetting(i + 2);
                if (acc)
                    this.settings.push(acc);
            }
        } else {
            this.settingTypes.push(new SmallGenericType(1, this.terms["time.employee.accounting.cost"]));
            this.settingTypes.push(new SmallGenericType(2, this.terms["time.employee.accounting.income"]));
            var cost = this.getEmploymentAccountingSetting(EmploymentAccountType.Cost);
            if (cost)
                this.settings.push(cost);
            var income = this.getEmploymentAccountingSetting(EmploymentAccountType.Income);
            if (income)
                this.settings.push(income);
        }
    }

    private onTabActivated() {
        this.scopeWatcherService.resumeWatchers(this.$scope);
    }

    private onTabDeActivated() {
        this.flowHandler.starting().finally(() => {
            if (this.isTabActivated === false) {
                this.scopeWatcherService.suspendWatchers(this.$scope);
            }
        });
    }

    private getEmploymentAccountingSetting(type: EmploymentAccountType): IAccountingSettingsRowDTO {
        return _.find(this.selectedEmployment.accountingSettings, a => a.type == type);
    }

    private hasEmploymentAccountingSetting(type: EmploymentAccountType): boolean {
        var setting = this.getEmploymentAccountingSetting(type);
        if (setting && (setting.account1Id || setting.account2Id || setting.account3Id || setting.account4Id || setting.account5Id || setting.account6Id))
            return true;

        return false;
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "time.employee.accounting.cost",
            "time.employee.accounting.income",
            "common.accountingsettings.fixed",
            "core.warning"
        ];
        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadReadOnlyPermissions(): ng.IPromise<any> {
        var features: number[] = [];

        features.push(Feature.Time_Employee_Employees_Edit_MySelf);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Contact);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Contact_DisbursementAccount);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_User);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Employments);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Employments_Employment);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Employments_Payroll);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Employments_Accounts);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Employments_Additions);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Tax);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_UnionFee);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Vacation);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Absence);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Contact_Children);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Contact_NoExtraShift);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Reports);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_WorkRules);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Categories);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Time);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Time_CalculatedCostPerHour);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Schedule);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Skills);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_EmployeeMeeting);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Note);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_WorkTimeAccount);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_DisbursementAccount);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_User);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_User_CardNumber);
        features.push(Feature.Manage_Users_Edit_UserMapping);
        features.push(Feature.Manage_Users_Edit_AttestRoleMapping);
        features.push(Feature.Manage_Users_Edit_AttestReplacementMapping);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Additions);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Tax);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_UnionFee);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Vacation);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Absence);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_Children);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Reports);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_WorkRules);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Categories);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Time);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Time_CalculatedCostPerHour);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Schedule);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Skills);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_EmployeeMeeting);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Note);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_WorkTimeAccount);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Files);
        features.push(Feature.Manage_GDPR_Logs);

        return this.coreService.hasReadOnlyPermissions(features).then(x => {
            this.readPermissions = x;
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var features: number[] = [];

        features.push(Feature.Time_Employee_Employees_Edit_MySelf);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Contact);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Contact_DisbursementAccount);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_User);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Employments);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Employments_Employment);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Employments_Payroll);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Employments_Accounts);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Employments_Additions);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Tax);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_UnionFee);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Vacation);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Absence);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Contact_Children);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Contact_NoExtraShift);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Reports);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_WorkRules);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Categories);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Time);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Time_CalculatedCostPerHour);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Schedule);

        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Skills);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_EmployeeMeeting);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_Note);
        features.push(Feature.Time_Employee_Employees_Edit_MySelf_WorkTimeAccount);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_DisbursementAccount);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_User);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_User_CardNumber);
        features.push(Feature.Manage_Users_Edit_UserMapping);
        features.push(Feature.Manage_Users_Edit_AttestRoleMapping);
        features.push(Feature.Manage_Users_Edit_AttestReplacementMapping);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Additions);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Tax);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_UnionFee);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Vacation);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Absence);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_Children);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Reports);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_WorkRules);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Categories);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Time);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Time_CalculatedCostPerHour);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Schedule);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Skills);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_EmployeeMeeting);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Note);
        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_WorkTimeAccount);

        features.push(Feature.Time_Employee_Employees_Edit_OtherEmployees_Files);
        features.push(Feature.Common_ExtraFields_Employee_Edit);
        features.push(Feature.Manage_GDPR_Logs);

        return this.coreService.hasModifyPermissions(features).then(x => {
            this.modifyPermissions = x;
        });
    }

    private setPermissions() {
        // Read

        this.readOnlyPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees];

        // Personal
        this.socialSecReadPermission = this.userIsMySelf || this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec];
        this.cardNumberReadPermission = this.userIsMySelf || this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_User_CardNumber];
        this.contactReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Contact] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact];
        this.allowSecretContactInfoPermission = this.contactReadPermission && (this.hasAttestRoles || this.userIsMySelf);
        this.contactDisbursementAccountReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Contact_DisbursementAccount] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_DisbursementAccount];
        this.userReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_User] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_User];
        this.userMappingReadPermission = this.readPermissions[Feature.Manage_Users_Edit_UserMapping];
        this.attestRoleMappingReadPermission = this.readPermissions[Feature.Manage_Users_Edit_AttestRoleMapping];
        this.userReplacementReadPermission = this.readPermissions[Feature.Manage_Users_Edit_AttestReplacementMapping];

        // Employment
        this.employmentDataReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Employments] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments];
        this.employmentReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Employments_Employment] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment];
        this.employmentPayrollReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Employments_Payroll] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll];
        this.employmentPayrollSalaryReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Employments_Payroll] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary];
        this.employmentAccountsReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Employments_Accounts] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts];
        this.taxReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Tax] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Tax];
        this.employeeUnionFeeReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_UnionFee] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_UnionFee];
        this.absenceVacationVacationReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Vacation] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Vacation];
        this.absenceVacationAbsenceReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Absence] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Absence];
        this.employeeChildReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Contact_Children] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_Children];
        this.reportsReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Reports] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Reports];
        this.workRulesReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_WorkRules] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_WorkRules];
        this.categoriesReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Categories] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Categories];
        this.timeReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Time] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Time];
        this.timeCalculatedCostPerHourReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Time_CalculatedCostPerHour] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Time_CalculatedCostPerHour];
        this.workTimeAccountReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_WorkTimeAccount] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_WorkTimeAccount];
        this.payrollAdditionsReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Employments_Additions] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Additions];

        // Schedule
        this.scheduleDataReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Schedule] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Schedule];

        // HR
        this.skillsReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Skills] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Skills];
        this.employeeMeetingReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_EmployeeMeeting] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_EmployeeMeeting];
        this.noteReadPermission = this.userIsMySelf ? this.readPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Note] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Note];
        this.hasExtraFieldPermission = this.modifyPermissions[Feature.Common_ExtraFields_Employee_Edit];
        // Files
        this.filesReadPermission = this.userIsMySelf ? true : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Files];

        // Log
        this.gdprLogsReadPermission = this.readPermissions[Feature.Manage_GDPR_Logs];

        // Modify

        this.modifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees];

        // Personal
        this.socialSecModifyPermission = this.userIsMySelf || this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_SocialSec];
        this.cardNumberModifyPermission = this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_User_CardNumber];
        this.contactModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Contact] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact];
        this.contactDisbursementAccountModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Contact_DisbursementAccount] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_DisbursementAccount];
        this.userModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_User] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_User];
        this.userMappingModifyPermission = this.modifyPermissions[Feature.Manage_Users_Edit_UserMapping];
        this.attestRoleMappingModifyPermission = this.modifyPermissions[Feature.Manage_Users_Edit_AttestRoleMapping];
        this.userReplacementModifyPermission = this.modifyPermissions[Feature.Manage_Users_Edit_AttestReplacementMapping];
        this.noExtraShiftModifyPermission = this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Contact_NoExtraShift];

        // Employment
        this.employmentDataModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Employments] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments];
        this.employmentModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Employments_Employment] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Employment];
        this.employmentPayrollModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Employments_Payroll] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll];
        this.employmentPayrollSalaryModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Employments_Payroll] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Payroll_Salary];
        this.employmentAccountsModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Employments_Accounts] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Accounts];
        this.taxModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Tax] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Tax];
        this.employeeUnionFeeModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_UnionFee] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_UnionFee];
        this.absenceVacationVacationModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Vacation] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Vacation];
        this.absenceVacationAbsenceModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_AbsenceVacation_Absence] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_AbsenceVacation_Absence];
        this.employeeChildModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Contact_Children] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_Children];
        this.openingBalanceUsedDaysModifyPermission = !this.userIsMySelf && this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Contact_Children];
        this.reportsModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Reports] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Reports];
        this.workRulesModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_WorkRules] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_WorkRules];
        this.categoriesModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Categories] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Categories];
        this.timeModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Time] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Time];
        this.timeCalculatedCostPerHourModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Time_CalculatedCostPerHour] : this.readPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Time_CalculatedCostPerHour];
        this.workTimeAccountModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_WorkTimeAccount] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_WorkTimeAccount];
        this.payrollAdditionsModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Employments_Additions] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Employments_Additions];

        // Schedule
        this.scheduleDataModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Schedule] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Schedule];

        // HR
        this.skillsModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Skills] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Skills];
        this.employeeMeetingModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_EmployeeMeeting] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_EmployeeMeeting];
        this.noteModifyPermission = this.userIsMySelf ? this.modifyPermissions[Feature.Time_Employee_Employees_Edit_MySelf_Note] : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Note];

        // Files
        this.filesModifyPermission = this.userIsMySelf ? false : this.modifyPermissions[Feature.Time_Employee_Employees_Edit_OtherEmployees_Files];

        // Log
        this.gdprLogsModifyPermission = this.modifyPermissions[Feature.Manage_GDPR_Logs];

        if (this.userReplacementReadPermission)
            this.loadReplacementUsers();
    }

    private loadLicenseSettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(LicenseSettingType.SSO_Key);
        settingTypes.push(LicenseSettingType.LifetimeSecondsEnabledOnUser);

        return this.coreService.getLicenseSettings(settingTypes).then(x => {
            let setting = SettingsUtility.getStringLicenseSetting(x, LicenseSettingType.SSO_Key);
            if (setting || setting.length > 2)
                this.showSso = true;

            let lts = SettingsUtility.getBoolLicenseSetting(x, LicenseSettingType.LifetimeSecondsEnabledOnUser);
            if (lts)
                this.showLifetime = true;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimeUseVacant);
        settingTypes.push(CompanySettingType.TimeDefaultTimeCode);
        settingTypes.push(CompanySettingType.UsePayroll);
        settingTypes.push(CompanySettingType.PayrollGroupMandatory);
        settingTypes.push(CompanySettingType.TimeForceSocialSecNbr);
        settingTypes.push(CompanySettingType.TimeDontValidateSocialSecNbr);
        settingTypes.push(CompanySettingType.TimeSetNextFreePersonNumberAutomatically);
        settingTypes.push(CompanySettingType.TimeSuggestEmployeeNrAsUsername);
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.DefaultEmployeeAccountDimEmployee);
        settingTypes.push(CompanySettingType.UseLimitedEmployeeAccountDimLevels);
        settingTypes.push(CompanySettingType.UseExtendedEmployeeAccountDimLevels);
        settingTypes.push(CompanySettingType.SalaryPaymentExportUseExtendedCurrencyNOK);
        settingTypes.push(CompanySettingType.UseHibernatingEmployment);
        settingTypes.push(CompanySettingType.DontAllowIdenticalSSN);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            // To do add missing ones
            this.useVacant = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeUseVacant);
            this.defaultTimeCodeId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeDefaultTimeCode);
            this.dontValidateSocialSecNbr = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeDontValidateSocialSecNbr);
            this.forceSocialSecNbr = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeForceSocialSecNbr);
            this.usePayroll = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UsePayroll);
            this.payrollGroupMandatory = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PayrollGroupMandatory);
            this.setNextEmployeeNumberAutomatically = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSetNextFreePersonNumberAutomatically);
            this.suggestEmployeeNrAsUsername = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeSuggestEmployeeNrAsUsername);
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.defaultEmployeeAccountDimId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.DefaultEmployeeAccountDimEmployee);
            this.useLimitedEmployeeAccountDimLevels = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseLimitedEmployeeAccountDimLevels);
            this.useExtendedEmployeeAccountDimLevels = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseExtendedEmployeeAccountDimLevels);
            this.useHibernatingEmployment = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseHibernatingEmployment);
            this.useSalaryPaymentExportExtendedSelection = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SalaryPaymentExportUseExtendedCurrencyNOK);
            this.dontAllowIdenticalSSN = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.DontAllowIdenticalSSN);

            this.loadDisbursementPaymentMethods();
            this.loadTimeCodes();

            if (this.useAccountsHierarchy)
                this.getHasAllowToAddOtherEmployeeAccounts();
        });
    }

    private loadUserSettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(UserSettingType.TimeSchedulePlanningDontSendXEMailOnChange);

        return this.coreService.getUserSettings(settingTypes, true).then(x => {
            this.sendEmailScheduleChanged = SettingsUtility.getBoolUserSetting(x, UserSettingType.TimeSchedulePlanningDontSendXEMailOnChange);
        });
    }

    private getHasAttestRoles(): ng.IPromise<any> {
        return this.employeeService.getHasAttestRoles(CalendarUtility.getDateToday(), CalendarUtility.getDateToday()).then(x => {
            this.hasAttestRoles = x;
        });
    }

    private loadReplacementUsers(): ng.IPromise<any> {
        return this.coreService.getUsersDict(true, false, false, false).then(x => {
            this.replacementUsers = x;
        });
    }

    private loadUserReplacement() {
        this.replacementUser = null;
        if (this.employee.userId && this.userReplacementReadPermission) {
            this.coreService.getUserReplacement(UserReplacementType.AttestFlow, this.employee.userId).then(x => {
                this.replacementUser = x;
            });
        }
    }

    private loadTimeDeviationCauses(employeeGroupId: number): ng.IPromise<any> {
        return this.employeeService.getTimeDeviationCausesGrid(employeeGroupId).then(x => {
            x.splice(0, 0, { id: 0, name: '' });
            this.timeDeviationCauses = x;
        });
    }

    private loadTimeCodes(): ng.IPromise<any> {
        return this.employeeService.getTimeCodesDict(SoeTimeCodeType.WorkAndAbsense, true, false).then(x => {
            this.timeCodes = x;
        });
    }

    private getHasAllowToAddOtherEmployeeAccounts(): ng.IPromise<any> {
        return this.employeeService.getHasAllowToAddOtherEmployeeAccounts(null).then(x => {
            this.hasAllowToAddOtherEmployeeAccounts = x;
        });
    }

    private loadPayrollReportsPersonalCategories(): ng.IPromise<any> {
        this.payrollReportsPersonalCategories = [];
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsPersonalCategory, true, true).then((x) => {
            this.payrollReportsPersonalCategories = x;
        });
    }

    private loadPayrollReportsWorkTimeCategories(): ng.IPromise<any> {
        this.payrollReportsWorkTimeCategories = [];
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsWorkTimeCategory, true, true).then((x) => {
            this.payrollReportsWorkTimeCategories = x;
        });
    }

    private loadPayrollReportsPayrollExportSalaryTypes(): ng.IPromise<any> {
        this.payrollReportsPayrollExportSalaryTypes = [];
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsSalaryType, true, true).then((x) => {
            this.payrollReportsPayrollExportSalaryTypes = x;
        });
    }

    private loadPayrollReportsAFACategories(): ng.IPromise<any> {
        this.payrollReportsAFACategories = [];
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsAFACategory, true, true).then((x) => {
            this.payrollReportsAFACategories = x;
        });
    }

    private loadPayrollReportsAFASpecialAgreements(): ng.IPromise<any> {
        this.payrollReportsAFASpecialAgreements = [];
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsAFASpecialAgreement, true, true).then((x) => {
            this.payrollReportsAFASpecialAgreements = x;
        });
    }

    private loadPayrollReportsCollectumITPplans(): ng.IPromise<any> {
        this.payrollReportsCollectumITPplans = [];
        return this.coreService.getTermGroupContent(TermGroup.PayrollReportsCollectumITPplan, true, true).then((x) => {
            this.payrollReportsCollectumITPplans = x;
        });
    }

    private loadKpaBelongings(): ng.IPromise<any> {
        this.kpaBelongings = [];

        return this.coreService.getTermGroupContent(TermGroup.KPABelonging, true, true).then(x => {
            this.kpaBelongings = x;
        });
    }
    private loadBygglosenSalaryType(): ng.IPromise<any> {
        this.bygglosenSalaryType = [];

        return this.coreService.getTermGroupContent(TermGroup.BygglosenSalaryType, true, true).then(x => {
            this.bygglosenSalaryType = x;
        });
    }
    private loadEndCodes(): ng.IPromise<any> {
        this.kpaEndCodes = [];

        return this.coreService.getTermGroupContent(TermGroup.KPAEndCode, true, true).then(x => {
            this.kpaEndCodes = x;
        });
    }

    private loadPayrollPriceFormulas(): ng.IPromise<any> {
        this.payrollPriceFormulas = [];

        return this.payrollService.getPayrollPriceFormulasDict(true).then(x => {
            this.payrollPriceFormulas = x;
        });
    }

    private loadKpaAgreementTypes(): ng.IPromise<any> {
        this.kpaAgreementTypes = [];

        return this.coreService.getTermGroupContent(TermGroup.KPAAgreementType, true, true).then(x => {
            this.kpaAgreementTypes = x;
        });
    }

    private loadGtpAgreementNumbers(): ng.IPromise<any> {
        this.gtpAgreementNumbers = [];

        return this.coreService.getTermGroupContent(TermGroup.GTPAgreementNumber, true, true, true).then(x => {
            x.forEach(y => {
                this.gtpAgreementNumbers.push({ id: y.id, name: y.id > 0 ? "({0}) {1}".format(y.id.toString(), y.name) : y.name });
            });
        });
    }

    private loadIFPaymentCodes(): ng.IPromise<any> {
        this.ifPaymentCodes = [];

        return this.coreService.getTermGroupContent(TermGroup.IFPaymentCode, true, true, true).then(x => {
            x.forEach(y => {
                this.ifPaymentCodes.push({ id: y.id, name: y.id > 0 ? "({0}) {1}".format(y.id.toString(), y.name) : y.name });
            });
        });
    }

    private loadDisbursementPaymentMethods(): ng.IPromise<any> {
        this.disbursementPaymentMethods = [];
        return this.coreService.getTermGroupContent(TermGroup.EmployeeDisbursementMethod, true, false).then((x) => {
            var disbursementPaymentMethodsAll = x;
            disbursementPaymentMethodsAll.forEach(y => {
                if (y.id === TermGroup_EmployeeDisbursementMethod.SE_NorweiganAccount) {
                    if (this.useSalaryPaymentExportExtendedSelection)
                        this.disbursementPaymentMethods.push(y);
                }
                else {
                    this.disbursementPaymentMethods.push(y);
                }
            });
            this.disbursementPaymentMethods = _.orderBy(this.disbursementPaymentMethods, ['name'], ['asc']);
        });
    }

    private loadEmployeePositions(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        this.employeePositions = [];
        this.employeePositionIds = [];

        if (!this.employeeId)
            deferral.resolve();
        else {
            return this.employeeService.getEmployeePositions(this.employeeId, true).then((x) => {
                this.employeePositions = x;
                this.employeePositionIds = _.map(this.employeePositions, p => p.positionId);
            });
        }

        return deferral.promise;
    }

    private loadEmployee(): ng.IPromise<any> {
        // Clear meetings while loading, otherwise wrong meetings will be displayed for a fraction while binding updates.
        if (this.employeeMeetingIsOpen && this.employee && this.employee.employeeMeetings.length > 0)
            this.employee.employeeMeetings = [];

        if (this.scheduleDataIsOpen && this.employee && (!this.employee.templateGroups || this.employee.templateGroups.length > 0))
            this.employee.templateGroups = [];

        return this.employeeService.getEmployeeForEdit(this.employeeId, this.employeeMeetingIsOpen, this.scheduleDataIsOpen).then(x => {
            this.isNew = false;

            // Must set userIsMySelf before setting permissions
            this.userIsMySelf = (x && x.userId === CoreUtility.userId);
            this.setPermissions();

            this.employee = x;
            this.messagingHandler.publishSetTabLabel(this.guid, "{0} - {1}".format(this.employee.employeeNr, this.employee.name));
            this.setHibernatingAbsenceText();
            if (this.extraFieldsExpanderRendered)
                this.$scope.$broadcast('reloadExtraFields', { guid: this.guid, recordId: this.employeeId });
            if (this.scheduleDataIsOpen && !this.scheduleDataOpened)
                this.expandScheduleData();
            if (this.employeeMiscIsOpen)
                this.loadCalculatedCosts();
            if (this.timeWorkAccountInitiallyOpen)
                this.loadEmployeeTimeWorkAccount();

            this.selectFirstMeeting();
            this.filesHelper.filesRendered = false;
            this.documentAccordionOpened = false;
            this.$timeout(() => {
                this.reloadFiles();
            });
            this.documentExpanderOpened();

            this.$q.all([
                this.validateLoginName(),
                this.validateSocialSec(),
                this.validateBankAccount(),
                this.loadContactAddressItems(),
                this.loadEmployeePortrait(),
                this.loadEmployeePositions(),
                this.loadUserReplacement(),
                this.loadTimeDeviationCauses(this.employee.currentEmployeeGroupId)
            ]).then(() => {
                this.$scope.$applyAsync(() => {
                    this.dirtyHandler.clean();
                    this.showGui();
                });
            });
        });
    }

    private reloadEmployments(date: Date) {
        var keys: string[] = [];
        keys.push("core.warning");
        keys.push("time.employee.employment.refresh.warning");

        var isEdited: boolean = false;
        _.forEach(this.employee.employments, employment => {
            if (employment.isEdited)
                isEdited = true;
        });

        if (isEdited) {
            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialogEx(terms["core.warning"], terms["time.employee.employment.refresh.warning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.employeeService.getEmployments(this.employeeId, date).then(employments => {
                        this.employmentsReloaded(employments, date);
                    });
                }, (reason) => {
                    // Cancel
                });
            });
        }
        else {
            this.employeeService.getEmployments(this.employeeId, date).then(employments => {
                this.employmentsReloaded(employments, date);
            });
        }
    }

    private employmentsReloaded(employments: EmploymentDTO[], date: Date) {
        if (!this.employee)
            return;
        this.employee.employments = employments;
        this.setHibernatingAbsenceText(date);
    }

    private reloadFiles() {
        this.filesHelper.loadFiles(true, null, false);
    }

    private _firstLoad: boolean = true;
    private showGui() {
        if (!this._firstLoad)
            return;

        if (this._firstLoad)
            this._firstLoad = false;

        this.contactInfoAccordionInitiallyOpen = true;
        this.employmentAccordionInitiallyOpen = true;
    }

    private loadContactAddressItems(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        this.contactAddressItems = [];
        if (!this.employeeId)
            deferral.resolve();
        else {
            return this.employeeService.getContactAddressItems(this.employee.actorContactPersonId).then((x) => {
                this.contactAddressItems = x;
            });
        }

        return deferral.promise;
    }

    private loadEmployeePortrait(): ng.IPromise<any> {
        return this.coreService.getImages(SoeEntityImageType.EmployeePortrait, SoeEntityType.Employee, this.employeeId, false).then(x => {
            if (x && x.length > 0)
                this.portrait = x[0];
            else
                this.portrait = null;
        });
    }

    private deleteEmployeePortrait() {
        if (this.portrait) {
            this.portrait = null;
            this.portraitChanged = true;
            this.setDirty();

            var keys: string[] = [
                "core.info",
                "time.employee.employee.deleteimage.message"
            ];

            this.translationService.translateMany(keys).then(terms => {
                this.notificationService.showDialogEx(terms["core.info"], terms["time.employee.employee.deleteimage.message"], SOEMessageBoxImage.Information);
            });
        }
    }

    public loadAllEmployees() {
        this.sharedEmployeeService.getEmployeesForGridSmall(this.showInactive).then(x => {
            this.allEmployees = x;
            _.forEach(this.allEmployees, employee => {
                employee['description'] = "{0} - {1}".format(employee.employeeNr, employee.name);
            });
            this.copyEmployees(this.allEmployees);
            if (this.employeeId)
                this.selectedEmployee = _.find(this.allEmployees, e => e.employeeId === this.employeeId);

            this.dirtyHandler.clean();
        });
    }

    private expandScheduleData() {
        if (!this.isNew && !this.scheduleDataOpened) {
            this.scheduleDataOpened = true;
            this.loadTemplateGroups();
            this.$scope.$broadcast('employeeScheduleInitialLoad', { guid: this.guid });
        }
    }

    private expandMeetings() {
        if (!this.isNew && !this.employeeMeetingOpened && (!this.employee.employeeMeetings || this.employee.employeeMeetings.length === 0)) {
            this.employeeMeetingOpened = true;
            this.loadAttestRolesForMeetings();
            this.loadFollowUpTypes();
            this.loadEmployeeMeetings();
            this.copyEmployeesforMeeting(this.allEmployees);
        }
    }

    private expandMisc() {
        if (!this.isNew && !this.employee.calculatedCosts) {
            this.loadCalculatedCosts();
        }
    }

    private expandEmployeeTimeWorkAccount() {
        if (!this.isNew && (!this.employee.employeeTimeWorkAccounts || this.employee.employeeTimeWorkAccounts.length === 0))
            this.loadEmployeeTimeWorkAccount();
    }

    private loadFollowUpTypes(): ng.IPromise<any> {
        this.followUpTypes = [];
        return this.employeeService.getFollowUpTypes().then((x) => {
            this.followUpTypes = x;
        });
    }

    private loadPayrollLevels(): ng.IPromise<any> {
        return this.payrollService.getPayrollLevels().then(x => {
            this.payrollLevels = x;
            if (_.size(this.payrollLevels) > 1) {
                this.usePayrollLevels = true;
                let empty: PayrollLevelDTO = new PayrollLevelDTO();
                empty.payrollLevelId = 0;
                empty.name = "";
                empty.description = "";
                empty.nameAndDesc = "";
                this.payrollLevels.push(empty);
            }
        });
    }

    private loadAttestRolesForMeetings(): ng.IPromise<any> {
        this.attestRolesForMeeting = [];
        return this.employeeService.getAttestRolesForMeetings().then((x) => {
            this.attestRolesForMeeting = x;
        });
    }

    private loadTemplateGroups(): ng.IPromise<any> {
        return this.employeeService.getTimeScheduleTemplateGroupsForEmployee(this.employeeId, true, true).then(x => {
            this.employee.templateGroups = x;
        });
    }

    private loadEmployeeMeetings(): ng.IPromise<any> {
        return this.employeeService.getEmployeeMeetings(this.employee.employeeId, CoreUtility.userId).then(x => {
            this.employee.employeeMeetings = x;
            this.selectFirstMeeting();
        });
    }

    private loadCalculatedCosts(): ng.IPromise<any> {
        return this.employeeService.getCalculatedCosts(this.employee.employeeId).then(x => {
            this.employee.calculatedCosts = x;
        });
    }

    private loadEmployeeTimeWorkAccount(): ng.IPromise<any> {
        return this.payrollService.getEmployeeTimeWorkAccount(this.employee.employeeId, true).then(x => {
            this.employee.employeeTimeWorkAccounts = x;

        });
    }

    private getNextEmployeeNumber() {
        this.employeeService.getLastUsedEmployeeSequenceNumber().then(number => {
            this.employee.employeeNr = (number + 1).toString();
            if (this.suggestEmployeeNrAsUsername) {
                this.employee.loginName = this.employee.employeeNr;
                this.employee.saveUser = true;
            }
        });
    }

    private new() {
        this.isNew = true;

        this.userIsMySelf = false;
        this.setPermissions();

        this.employeeId = 0;
        this.employee = new EmployeeUserDTO();
        this.employee.licenseId = CoreUtility.licenseId;
        this.employee.actorCompanyId = CoreUtility.actorCompanyId;
        this.employee.active = true;
        this.portrait = null;

        if (this.setNextEmployeeNumberAutomatically)
            this.getNextEmployeeNumber();

        this.personalDataAccordionInitiallyOpen = !this.searchMode;
        this.contactInfoAccordionInitiallyOpen = false;
        this.employmentDataAccordionInitiallyOpen = false;
        this.employmentAccordionInitiallyOpen = false;

        this.employee.employments = [];
        this.employee.factors = [];
        this.employee.unionFees = [];
        this.employee.employeeTimeWorkAccounts = [];
        this.employee.categoryRecords = [];
        this.employee.calculatedCosts = [];
        this.employee.employeeSkills = [];

        this.employee.disbursementMethod = TermGroup_EmployeeDisbursementMethod.Unknown;
        this.employee.timeDeviationCauseId = 0;
        this.employee.timeCodeId = this.defaultTimeCodeId;
        this.employee.langId = CoreUtility.languageId;
        this.employee.isMobileUser = true;
        this.employee.bygglosenMunicipalCode = '';
        this.employee.bygglosenProfessionCategory = '';
        this.employee.bygglosenLendedToOrgNr = '';

        this.filesHelper.reset();

        this.validateLoginName();
        this.validateSocialSec();
        this.validateBankAccount();

        if (this.searchMode) {
            this.searchMode = false;

            this.translationService.translate('time.employee.employee.searchemployee').then(term => {
                this.messagingHandler.publishSetTabLabel(this.guid, term);
            });
            this.focusService.focusById("ctrl_searchEmployeeCondition");
        } else {
            this.focusService.focusById("ctrl_employee_firstName");
        }
    }

    // TOOLBAR

    private openDeleteDialog() {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Employee/Employees/Dialogs/DeleteEmployee/deleteEmployee.html"),
            controller: DeleteEmployeeController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'lg',
            resolve: {
                employeeId: () => { return this.employeeId; },
                employeeText: () => { return '({0}) {1}'.format(this.employee.employeeNr, this.employee.name); }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            this.messagingHandler.publishReloadGrid(this.guid);
            var action: DeleteEmployeeAction = result.action;
            if (action == DeleteEmployeeAction.Inactivate || action == DeleteEmployeeAction.RemoveInfo)
                this.loadEmployee();
            else
                this.closeMe(true);
        });
    }

    // EVENTS

    public closeModal(modified: boolean) {
        if (this.isModal) {
            if (this.employeeId) {
                this.modal.close({ modified: modified, id: this.employeeId });
            } else {
                this.modal.dismiss();
            }
        }
    }

    private showInactiveChanged() {
        this.$timeout(() => {
            if (this.showNavigation)
                this.loadAllEmployees();
        });
    }

    private addDefaultContactAddresRow() {

    }

    private emailCopyChanged() {
        this.employee.saveUser = true;
        this.setDirty();
    }

    private contactAddressesChanged() {
        this.employee.saveUser = true;
        this.setDirty();
    }

    private selectContactAddress() {
        this.$scope.$broadcast('selectContactAddresRow', { index: 0 });
    }

    private searchEmployee() {
        this.validateSearchEmployee().then(ok => {
            if (ok) {
                // First check if employee number matches exactly, secondly match on part of name
                var emp = _.find(this.allEmployees, e => e.employeeNr === this.searchEmployeeCondition);
                if (!emp) {
                    var emps = _.filter(this.allEmployees, e => e.name.contains(this.searchEmployeeCondition));
                    if (emps.length > 0) {
                        // Set first match
                        emp = emps[0];
                        // Replace complete list with matches
                        this.copyEmployees(emps);
                    }
                }

                this.setSelectedEmployee(emp);
            }
        });
    }

    private clearSearchEmployee() {
        this.$timeout(() => {
            // Triggered on every change in search field, so we need to check if it is empty
            if (!this.searchEmployeeCondition) {
                this.copyEmployees(this.allEmployees);
                this.focusService.focusById("ctrl_searchEmployeeCondition");
            }
        });
    }

    private validateSearchEmployee(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        if (this.dirtyHandler.isDirty) {
            this.notificationService.showConfirmOnExit().then(ok => {
                deferral.resolve(ok);
            });
        } else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private copyEmployees(source: IEmployeeSmallDTO[]) {
        this.filteredEmployees = [];
        _.forEach(source, employee => {
            this.filteredEmployees.push(employee);
        });
    }

    private copyEmployeesforMeeting(source: IEmployeeSmallDTO[]) {
        this.employeesForMeeting = [];
        _.forEach(source, employee => {
            var employeeToAdd: EmployeeListSmallDTO = new EmployeeListSmallDTO();
            employeeToAdd.employeeId = employee.employeeId
            employeeToAdd.name = employee.name;
            this.employeesForMeeting.push(employeeToAdd);
        });
    }

    private portraitConsentChanged() {
        this.$timeout(() => {
            this.employee.portraitConsentDate = (this.employee.portraitConsent ? new Date() : null);
        });
    }

    private userInfoChanged(result) {
        this.$timeout(() => {
            this.validateLoginName();

            if (result.result.function == EditUserFunctions.ConnectUser) {
                // Get contact info from selected user
                this.employeeService.getContactAddressItemsByUser(this.employee.userId).then(items => {
                    let itemsAdded: boolean = false;
                    if (items.length > 0) {
                        // Check for duplicates
                        _.forEach(items, item => {
                            let exists: boolean = false;
                            _.forEach(_.filter(this.contactAddressItems, a => a.sysContactAddressTypeId === item.sysContactAddressTypeId && a.sysContactEComTypeId === item.sysContactEComTypeId), existing => {
                                if (existing.displayAddress === item.displayAddress) {
                                    exists = true;
                                    return;
                                }
                            });
                            if (!exists) {
                                // Add contact address item from user to employee
                                ContactAddressItemDTO.setIcon(item);
                                this.contactAddressItems.push(item);
                                itemsAdded = true;
                            }
                        });
                    }

                    if (itemsAdded) {
                        // Notify user about added contact addresses
                        var keys: string[] = [
                            "time.employee.employee.contactaddressesadded.title",
                            "time.employee.employee.contactaddressesadded.message"
                        ];
                        this.translationService.translateMany(keys).then(terms => {
                            this.notificationService.showDialogEx(terms["time.employee.employee.contactaddressesadded.title"], terms["time.employee.employee.contactaddressesadded.message"], SOEMessageBoxImage.Information);
                        });
                    }
                });
            } else if (result.result.function == EditUserFunctions.DisconnectUser) {
                this.employee.disconnectExistingUser = true;
            }

            this.employee.saveUser = true;
            this.setDirty();
        });
    }

    private validateLoginName() {
        this.isLoginNameValid = !!(this.employee && this.employee.loginName && this.employee.loginName.length > 0) || !this.employee.userId || this.employee.vacant;
    }

    private userRolesChanged() {
        this.employee.saveUser = true;
        this.setDirty();
    }

    private socialSecChanged() {
        this.$timeout(() => {
            if (this.employee.disbursementMethod == TermGroup_EmployeeDisbursementMethod.SE_PersonAccount)
                this.paymentMethodChanged(this.employee.disbursementMethod);

            this.validateSocialSecExists();
            this.validateSocialSec();
        });
    }

    private validateSocialSec(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        var doNotValidate = this.dontValidateSocialSecNbr || (!this.socialSecReadPermission && !this.socialSecModifyPermission) || (!this.employee.socialSec && (!this.forceSocialSecNbr || this.employee.vacant));
        if (doNotValidate) {
            this.isSocialSecurityNumberValid = true;
            deferral.resolve();
        } else {
            return this.coreService.validSocialSecurityNumber(this.employee.socialSec, true, this.forceSocialSecNbr, false, TermGroup_Sex.Unknown).then(isValid => {
                this.isSocialSecurityNumberValid = isValid;
            });
        }

        return deferral.promise;
    }

    private validateSocialSecExists() {
        if (!this.employee || !this.employee.socialSec)
            return;

        this.employeeService.validateEmployeeSocialSecNumberNotExists(this.employee.socialSec, this.employeeId).then(result => {
            if (!result.success) {
                if (!this.dontAllowIdenticalSSN) {
                    this.translationService.translate("time.employee.employee.socialsecnumber.exists.title").then(term => {
                        this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Warning);
                    });
                } else {
                    this.translationService.translate("time.employee.employee.socialsecnumber.exists.companysetting").then(term => {
                        this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Warning);
                    });
                }
            }
        });
    }

    private vacantChanged() {
        this.$timeout(() => {
            this.validateSocialSec();
            this.validateBankAccount();
        });
    }

    private employeeNrChanged() {
        this.$timeout(() => {
            this.employeeNumberExists();
        });
    }

    private bankAccountChanged() {
        this.validateBankAccount();
    }

    private paymentMethodChanged(method: TermGroup_EmployeeDisbursementMethod) {
        if (!this.employee)
            return;

        if (method == TermGroup_EmployeeDisbursementMethod.SE_NorweiganAccount) {
            this.employee.dontValidateDisbursementAccountNr = true;
        } else if (method == TermGroup_EmployeeDisbursementMethod.SE_CashDeposit) {
            this.employee.disbursementClearingNr = '';
            this.employee.disbursementAccountNr = '';
            this.employee.dontValidateDisbursementAccountNr = false;
        } else if (method == TermGroup_EmployeeDisbursementMethod.SE_PersonAccount && this.employee.socialSec) {
            if (CalendarUtility.isValidSocialSecurityNumber(this.employee.socialSec.trim(), true, this.forceSocialSecNbr, false)) {
                this.employee.disbursementClearingNr = '3300';

                // Remove all but numbers
                let socialSec: string = this.employee.socialSec.replace(/\D/g, '');
                // Remove century
                if (socialSec.length === 12)
                    socialSec = socialSec.substring(2);

                this.employee.disbursementAccountNr = socialSec;
            }
        }
        this.validateBankAccount();
    }

    private employeeAccountsChanged() {
        this.validateEmployeeAccounts();
        this.setDirty();
    }

    private highRiskProtectionChanged() {
        this.$timeout(() => {
            if (!this.employee.highRiskProtection)
                this.employee.highRiskProtectionTo = null;
        });
    }

    private medicalCertificateReminderChanged() {
        this.$timeout(() => {
            if (!this.employee.medicalCertificateReminder)
                this.employee.medicalCertificateDays = null;
        });
    }

    private absence105DaysExcludedChanged() {
        this.$timeout(() => {
            if (!this.employee.absence105DaysExcluded)
                this.employee.absence105DaysExcludedDays = null;
        });
    }

    private showMedicalCertificateInfo() {
        var keys: string[] = [
            "core.info",
            "time.employee.employee.absence.medicalcertificate.reminder.days.info"
        ];
        this.translationService.translateMany(keys).then(terms => {
            this.notificationService.showDialogEx(terms["core.info"], terms["time.employee.employee.absence.medicalcertificate.reminder.days.info"].format('7'), SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
        });
    }

    private cardNumberChanged() {
        // 2019-09-23 Håkan removed this check due to task #41533
        //this.$timeout(() => {
        //    this.employee.cardNumber = this.employee.cardNumber.replace(/\D/g, '');
        //});
    }

    private checkCardNumberExists() {
        this.$timeout(() => {
            this.cardNumberExists();
        });
    }

    private deleteCardNumber() {
        this.employee.cardNumber = '';
        this.setDirty();

        var keys: string[] = [
            "time.employee.employee.cardnumber.delete.title",
            "time.employee.employee.cardnumber.delete.message"
        ];

        this.translationService.translateMany(keys).then(terms => {
            this.notificationService.showDialogEx(terms["time.employee.employee.cardnumber.delete.title"], terms["time.employee.employee.cardnumber.delete.message"], SOEMessageBoxImage.Information);
        });
    }

    private scheduleDataChanged() {
        this.setDirty();
        this.employee.isTemplateGroupsChanged = true;
    }

    private meetingTypeChanged() {
        this.$timeout(() => {
            let type = _.find(this.followUpTypes, t => t.followUpTypeId === this.selectedEmployeeMeeting.followUpTypeId);
            this.selectedEmployeeMeeting.followUpTypeName = (type ? type.name : '');
            this.employee.isEmployeeMeetingsChanged = true;
        });
    }

    private documentExpanderOpened() {
        this.documentAccordionOpened = true;
    }

    private _fileUploadedCounter: number = 0;
    private fileUploaded(result) {
        this._fileUploadedCounter++;
        this.filesHelper.fileUploadedCallback(result, false);
        this.notifyFileUploaded();
    }

    private notifyFileUploaded = _.debounce(() => {
        let keys: string[] = [
            "time.employee.employee.document.uploadedone.title",
            "time.employee.employee.document.uploadedone.message",
            "time.employee.employee.document.uploadedmultiple.title",
            "time.employee.employee.document.uploadedmultiple.message"];

        this.translationService.translateMany(keys).then(terms => {
            let title: string = (this._fileUploadedCounter === 1 ? terms["time.employee.employee.document.uploadedone.title"] : terms["time.employee.employee.document.uploadedmultiple.title"]);
            let msg: string = (this._fileUploadedCounter === 1 ? terms["time.employee.employee.document.uploadedone.message"] : terms["time.employee.employee.document.uploadedmultiple.message"].format(this._fileUploadedCounter.toString()));

            this.notificationService.showDialogEx(title, msg, SOEMessageBoxImage.OK);
            this._fileUploadedCounter = 0;
        });
    }, 1000, { leading: false, trailing: true });

    // ACTIONS

    private cardNumberExists() {
        if (!this.employee.cardNumber)
            return;

        this.isCheckingCardNumber = true;

        this.employeeService.cardNumberExists(this.employee.cardNumber, this.employeeId).then(result => {
            if (result.booleanValue) {
                var keys: string[] = [
                    "time.employee.employee.cardnumber.exists.title",
                    "time.employee.employee.cardnumber.exists.message"
                ];

                this.translationService.translateMany(keys).then(terms => {
                    this.notificationService.showDialogEx(terms["time.employee.employee.cardnumber.exists.title"], terms["time.employee.employee.cardnumber.exists.message"].format(result.stringValue), SOEMessageBoxImage.Forbidden);
                });

                this.employee.cardNumber = '';
                this.isCheckingCardNumber = false;
            } else {
                this.isCheckingCardNumber = false;
            }
        });
    }

    private employeeNumberExists() {
        if (!this.employee.employeeNr)
            return;

        this.employeeService.employeeNumberExists(this.employee.employeeNr, false, this.employeeId).then(exists => {
            if (exists) {
                var keys: string[] = [
                    "time.employee.employee.employeenr",
                    "time.employee.employee.employeenr.exists"
                ];

                this.translationService.translateMany(keys).then(terms => {
                    this.notificationService.showDialogEx(terms["time.employee.employee.employeenr"], terms["time.employee.employee.employeenr.exists"].format(this.employee.employeeNr), SOEMessageBoxImage.Forbidden);
                    this.employee.employeeNr = '';
                });

            } else {
                if (this.suggestEmployeeNrAsUsername) {
                    this.employee.loginName = this.employee.employeeNr;
                    this.employee.saveUser = true;
                }
            }
        });
    }

    private setHibernatingAbsenceText(date?: Date) {
        if (!this.useHibernatingEmployment)
            return;

        _.forEach(this.employee.employments, employment => {
            if (employment.hibernatingPeriods && employment.hibernatingPeriods.length > 0) {
                if (!date) {
                    var today = CalendarUtility.getDateToday();
                    if (today >= employment.dateFrom && (!employment.dateTo || today <= employment.dateTo))
                        date = today;
                    else
                        date = employment.dateFrom;

                }
                var hibernating = employment.hibernatingPeriods.filter(p => p.start <= date && p.stop >= date)[0];
                if (hibernating)
                    employment.hibernatingTimeDeviationCauseName = hibernating.comment;
            }
        });
    }

    private initSaveEmployeeUser() {
        if (this.isCheckingCardNumber) {
            this.saveInProgress = false;
            return;
        }

        this.employee.saveEmployee = true;

        this.validateUser().then(passedUser => {
            if (this.employee.saveUser && !this.employee.loginName && !this.employee.disconnectExistingUser)
                this.employee.saveUser = false;

            if (passedUser) {
                this.validateEmployeeAccounts().then(passedAccounts => {
                    if (passedAccounts) {
                        this.validateSaveEmployee().then(passedEmployee => {
                            if (passedEmployee) {
                                this.saveEmployeeUser();
                            } else {
                                this.saveInProgress = false;
                            }
                        });
                    } else {
                        this.saveInProgress = false;
                    }
                });
            } else {
                this.saveInProgress = false;
            }
        });
    }

    private employeeActiveChanged() {
        this.employee.saveUser = true;
    }

    private saveEmployeeUser() {
        if (this.isCheckingCardNumber) {
            this.saveInProgress = false;
            return;
        }

        if (!this.employee.name)
            this.employee.name = this.employee.firstName + " " + this.employee.lastName;

        // Accounting settings for selected employment has been modified,
        // copy settings to employment before saving.
        if (this.isAccountingSettingsModified && this.selectedEmployment) {
            this.selectedEmployment.accountingSettings = this.settings;
        }

        // New employee tax with no type selected should not be saved
        if (this.employeeTax && !this.employeeTax.employeeTaxId && !this.employeeTax.type)
            this.employeeTax = null;

        // Attestflow replacement user
        if (this.replacementUser) {
            this.replacementUser.actorCompanyId = CoreUtility.actorCompanyId;
            this.replacementUser.originUserId = this.employee.userId;
            this.replacementUser.type = UserReplacementType.AttestFlow;
            if (!this.replacementUser.replacementUserId)
                this.replacementUser.state = SoeEntityState.Deleted;
        }

        // Only save modified employee settings
        // Remember all settings to be able to restore on error
        const allEmployeeSettings = CoreUtility.cloneDTOs(this.employee.employeeSettings);
        if (this.employee.employeeSettings != null)
            this.employee.employeeSettings = this.employee.employeeSettings.filter(s => s.isModified);

        this.progress.startSaveProgress((completion) => {
            const files = this.filesHelper.getAsDTOs();
            this.employeeService.saveEmployeeUser(this.employee, this.contactAddressItems, this.employeePositions, this.employee.employeeSkills, this.replacementUser, this.employeeTax, this.userRolesHasChanges, this.userAttestRolesHasChanges, this.userRoles, files, _.filter(this.extraFieldRecords, (r) => r.isModified === true).map(r => r.toPlainObject())).then(result => {
                if (result.success) {
                    if (result.intDict) {
                        this.employeeId = result.intDict[SaveEmployeeUserResult.EmployeeId];
                        this.employee.employeeId = this.employeeId;
                    }

                    if (this.extraFieldsExpanderRendered)
                        this.$scope.$broadcast('reloadExtraFields', { guid: this.guid, recordId: this.employeeId });

                    if (this.employeeTax)
                        this.$scope.$broadcast('reloadYears', this.employeeTax.year);

                    if (this.userHasChanges)
                        this.$scope.$broadcast('reloadUserInfo');

                    if (this.userRoles && (this.userRolesHasChanges || this.userAttestRolesHasChanges))
                        this.$scope.$broadcast('reloadUserRoles');

                    if (files && files.length > 0 && this.filesHelper.filesLoaded)
                        this.reloadFiles();

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.employee);

                    if (this.showNavigation)
                        this.loadAllEmployees();
                    this.saveEmployeePortrait();
                    this.loadEmployeePositions();
                    this.setHibernatingAbsenceText();
                    this.dirtyHandler.clean();

                    if (result.infoMessage && result.infoMessage.length > 0)
                        this.notificationService.showDialogEx(this.terms["core.warning"], result.infoMessage, SOEMessageBoxImage.Warning);
                } else {
                    completion.failed(result.errorMessage);
                    // Restore employee settings
                    this.employee.employeeSettings = allEmployeeSettings;
                    this.saveInProgress = false;
                }
            }, error => {
                completion.failed(error.message);
                // Restore employee settings
                this.employee.employeeSettings = allEmployeeSettings;
                this.saveInProgress = false;
            });
        }, this.guid).then(data => {
            if (this.isModal)
                this.closeModal(true);
            else
                this.onLoadData();

            this.saveInProgress = false;
        });
    }

    private saveEmployeePortrait() {
        if (this.portraitChanged) {
            if (this.portrait) {
                // New uploaded image, connect it to employee
                this.coreService.connectDataStorageToEntity(this.portrait.imageId, this.employeeId, true, SoeEntityType.Employee, SoeDataStorageRecordType.EmployeePortrait).then(result => {
                    if (!result.success) {
                        this.translationService.translate("time.employee.employee.saveimage.error").then(term => {
                            this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Error);
                        });
                    }
                });
            } else {
                // Image deleted
                this.coreService.deleteDataStorage(this.employeeId, SoeDataStorageRecordType.EmployeePortrait).then(result => {
                    //this.coreService.saveImage(SoeEntityImageType.EmployeePortrait, ImageFormatType.NONE, SoeEntityType.Employee, this.employeeId, null, null).then(result => {
                    if (!result.success) {
                        this.translationService.translate("time.employee.employee.deleteimage.error").then(term => {
                            this.notificationService.showDialogEx(term, result.errorMessage, SOEMessageBoxImage.Error);
                        });
                    }
                });
            }
        }
    }

    private exportEmployee() {
        this.progress.startLoadingProgress([() => {
            return this.employeeService.getEmployeeForExport(this.employeeId).then((employee) => {
                ExportUtility.Export(employee, employee.name + '.json');
            });
        }]);
    }

    // HELP-METHODS

    private setDirty(force: boolean = false) {
        if (force) {
            this.$scope.$applyAsync(() => {
                this['edit'].$pristine = false;
                this['edit'].$dirty = true;
            });
        }
        this.dirtyHandler.setDirty();
    }

    private employeeTaxChanged() {
        if (!this.employeeTaxInitiallyOpen) {
            this.employmentDataAccordionInitiallyOpen = true;
            this.employmentAccordionInitiallyOpen = false;
            this.employeeTaxInitiallyOpen = true;
        }
        this.setDirty();
    }

    private employeeTimeWorkAccountChanged() {
        if (!this.timeWorkAccountInitiallyOpen) {
            this.employmentDataAccordionInitiallyOpen = true;
            this.employmentAccordionInitiallyOpen = false;
            this.timeWorkAccountInitiallyOpen = true;
        }
        this.setDirty();
    }

    private selectFirstMeeting() {
        if (!this.employee || !this.employee.employeeMeetings)
            return;

        let activeMeetings = _.filter(this.employee.employeeMeetings, m => m.state === SoeEntityState.Active);
        this.selectedEmployeeMeeting = activeMeetings.length > 0 ? _.orderBy(activeMeetings, 'startTime', 'desc')[0] : null;
    }

    private deleteEmployeeMeeting(meeting: EmployeeMeetingDTO) {
        meeting.state = SoeEntityState.Deleted;
        this.selectFirstMeeting();
        this.employee.isEmployeeMeetingsChanged = true;
        this.setDirty();
    }

    private addMeeting() {
        if (!this.employee.employeeMeetings)
            this.employee.employeeMeetings = [];
        var meeting: EmployeeMeetingDTO = new EmployeeMeetingDTO();
        meeting.employeeId = this.employeeId || 0;
        meeting.participantIds = [];
        if (this.employeeId)
            meeting.participantIds.push(this.employeeId);
        meeting.attestRoleIds = [];
        meeting.otherParticipants = '';
        meeting.note = '';
        this.employee.employeeMeetings.push(meeting);
        this.selectedEmployeeMeeting = meeting;
        this.employee.isEmployeeMeetingsChanged = true;
        this.setDirty();
    }

    private undoMeeting() {
        if (this.selectedEmployeeMeeting && !this.selectedEmployeeMeeting.employeeMeetingId) {
            _.pull(this.employee.employeeMeetings, this.selectedEmployeeMeeting);
            this.selectedEmployeeMeeting = null;
        }
    }

    private setSelectedEmployee(record: IEmployeeSmallDTO) {
        if (!record || record.employeeId === this.employeeId)
            return;

        this.employeeId = record.employeeId;
        this.portrait = null;
        this.selectedEmployee = _.find(this.allEmployees, e => e.employeeId === this.employeeId);
        this.searchMode = false;
        this.onLoadData();
    }

    public employeePortraitUploaded(result) {
        if (result) {
            this.portrait = result;
            this.portraitChanged = true;
            this.setDirty();
        }
    }

    // VALIDATION

    private validateBankAccount(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        var doNotValidate = ((!this.contactDisbursementAccountReadPermission && !this.contactDisbursementAccountModifyPermission) || this.employee.vacant);
        if (doNotValidate) {
            this.isBankAccountValid = true
            deferral.resolve(true);
        } else {
            this.$timeout(() => {
                if (!this.contactDisbursementAccountReadPermission || this.employee.dontValidateDisbursementAccountNr || this.employee.disbursementMethod === TermGroup_EmployeeDisbursementMethod.SE_CashDeposit || (!this.usePayroll && this.employee.disbursementMethod === TermGroup_EmployeeDisbursementMethod.Unknown)) {
                    this.isBankAccountValid = true
                    deferral.resolve(true);
                } else {
                    this.coreService.validBankNumberSE(this.employee.disbursementClearingNr, this.employee.disbursementAccountNr, null).then(isValid => {
                        this.isBankAccountValid = isValid;
                        deferral.resolve(isValid);
                    });
                }
            });
        }

        return deferral.promise;
    }

    private validateUser(): ng.IPromise<boolean> {
        var deferral = this.$q.defer<boolean>();

        if (!this.employee.loginName && !this.employee.disconnectExistingUser) {
            var keys: string[] = [
                "time.employee.employee.nouserwarning.title",
                "time.employee.employee.nouserwarning.message"
            ];
            this.translationService.translateMany(keys).then(terms => {
                var modal = this.notificationService.showDialogEx(terms["time.employee.employee.nouserwarning.title"], terms["time.employee.employee.nouserwarning.message"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    deferral.resolve(true);
                }, (reason) => {
                    deferral.resolve(false);
                });
            });
        } else {
            deferral.resolve(true);
        }

        return deferral.promise;
    }

    private validateEmployeeAccounts(): ng.IPromise<boolean> {
        const deferral = this.$q.defer<boolean>();

        if (this.useAccountsHierarchy) {
            // TODO: Set 'mustHaveMainAllocation' to true after job has been run
            this.employeeService.validateEmployeeAccounts(this.employee.accounts, false, this.usePayroll && !this.employee.excludeFromPayroll).then(result => {
                if (result.success) {
                    deferral.resolve(true);
                } else {
                    this.translationService.translate("time.employee.employee.accounthierarchy").then(term => {
                        this.notificationService.showErrorDialog(term, result.errorMessage, null);
                    });
                    deferral.resolve(false);
                }
            });
        } else {
            deferral.resolve(true);
        }

        return deferral.promise
    }

    private validateSaveEmployee(): ng.IPromise<boolean> {
        return this.employeeService.validateSaveEmployee(this.employee, this.contactAddressItems).then(result => {
            return this.notificationService.showValidateSaveEmployee(result);
        });
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.employee) {
                var errors = this['edit'].$error;

                var noPermission: boolean = false;

                if (!this.employee.firstName)
                    mandatoryFieldKeys.push("common.firstname");

                if (!this.employee.lastName)
                    mandatoryFieldKeys.push("common.lastname");

                if (!this.employee.employeeNr)
                    mandatoryFieldKeys.push("time.employee.employee.employeenr");

                if (this.socialSecModifyPermission) {
                    if (this.forceSocialSecNbr && !this.employee.socialSec && !this.employee.vacant) {
                        mandatoryFieldKeys.push("time.employee.employee.socialsec");
                        if (!this.socialSecModifyPermission)
                            noPermission = true;
                    }

                    if (errors['socialSecurityNumber']) {
                        validationErrorKeys.push("time.employee.employee.socialsecnotvalid");
                        if (!this.socialSecModifyPermission)
                            noPermission = true;
                    }
                }

                if (this.selectedEmployeeMeeting && !this.selectedEmployeeMeeting.followUpTypeId)
                    mandatoryFieldKeys.push("time.employee.employeeconversation.type");

                if (this.selectedEmployeeMeeting && !this.selectedEmployeeMeeting.startTime)
                    mandatoryFieldKeys.push("time.employee.employeeconversation.startdate");

                if (errors['contactAddress'])
                    validationErrorStrings.push(this.contactAddressesValidationErrors);

                if (!this.employee.vacant && errors['bankAccount']) {
                    validationErrorKeys.push("time.employee.employee.bankaccountnotvalid");
                    if (!this.contactDisbursementAccountModifyPermission)
                        noPermission = true;
                }

                if (this.contactDisbursementAccountModifyPermission) {
                    if (errors['bankAccountMandatory']) {
                        validationErrorKeys.push("time.employee.employee.paymentmethodmandatory");
                        validationErrorKeys.push("time.employee.employee.selectcashinfo");
                        if (!this.contactDisbursementAccountModifyPermission)
                            noPermission = true;
                    }

                    if (errors['bankAccountMandatoryIfMethodSelected']) {
                        validationErrorKeys.push("time.employee.employee.bankaccountmandatorywithpaymentmethod");
                        if (!this.contactDisbursementAccountModifyPermission)
                            noPermission = true;
                    }
                }

                //if (!this.employee.loginName && !this.employee.vacant) {
                //    mandatoryFieldKeys.push("common.user.loginid");
                //    if (!this.userModifyPermission)
                //        noPermission = true;
                //}

                if (errors['role']) {
                    validationErrorKeys.push("time.employee.employee.missingrole");
                    if (!this.userModifyPermission)
                        noPermission = true;
                }

                if (errors['defaultRole']) {
                    validationErrorKeys.push("time.employee.employee.missingdefaultrole");
                    if (!this.userModifyPermission)
                        noPermission = true;
                }

                if (errors['accountingSettings']) {
                    validationErrorKeys.push("time.employee.employee.accountingsettingsinvalid");
                    if (!this.employmentAccountsModifyPermission)
                        noPermission = true;
                }
                if (errors['employmentPriceTypes']) {
                    validationErrorKeys.push("time.employee.employee.employmentpriceptypemissinglevel");
                }

                if (errors['empVacation']) {
                    validationErrorStrings.push(this.employeeVacationValidationErrors);
                    if (!this.absenceVacationVacationModifyPermission)
                        noPermission = true;
                }

                if (errors['accountMandatory']) {
                    validationErrorKeys.push("time.employee.employee.accountmandatory");
                    if (!this.categoriesModifyPermission)
                        noPermission = true;
                }

                if (errors['categoryMandatory']) {
                    validationErrorKeys.push("time.employee.employee.categorymandatory");
                    if (!this.categoriesModifyPermission)
                        noPermission = true;
                }

                if (errors['payrollGroupMandatory']) {
                    mandatoryFieldKeys.push("time.employee.payrollgroup.payrollgroup");
                }

                if (errors['employmentMandatory']) {
                    mandatoryFieldKeys.push("time.employee.employee.employment");
                }

                if (errors['templateGroupDates'])
                    validationErrorKeys.push("time.employee.employee.invalidtemplategroupdates");

                if (noPermission) {
                    if ((mandatoryFieldKeys.length + validationErrorKeys.length + validationErrorStrings.length) > 1)
                        validationErrorKeys.push("time.employee.employee.nomodifypermissions");
                    else
                        validationErrorKeys.push("time.employee.employee.nomodifypermission");
                }
            }
        });
    }

    public onExtraFieldsExpanderOpenClose() {
        this.extraFieldRecords = [];
        this.extraFieldsExpanderRendered = !this.extraFieldsExpanderRendered;
    }
}