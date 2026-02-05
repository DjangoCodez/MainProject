import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { Feature, CompanySettingType, SoeCategoryType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IEmployeeService } from "../EmployeeService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { SearchEmployeeSkillDTO } from "../../../Common/Models/SkillDTOs";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { CalendarUtility } from "../../../Util/CalendarUtility";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Terms
    private terms: { [index: string]: string; };

    // Company settings
    private nbrOfSkillLevels: number = 0;
    private halfPrecision: boolean = false;
    private useAccountsHierarchy: boolean;

    // Data
    private positions: SmallGenericType[] = [];
    private skills: SmallGenericType[] = [];
    private categories: SmallGenericType[] = [];
    private employeeSkills: SearchEmployeeSkillDTO[] = [];
    private accounts: AccountDTO[];

    // Properties
    private selectedEmployeeNrFrom: string = '';
    private selectedEmployeeNrTo: string = '';
    private selectedPositionId: number = 0;
    private selectedPositionMissing: boolean = false;
    private selectedSkillId: number = 0;
    private selectedSkillMissing: boolean = false;
    private selectedCategoryId: number = 0;
    private selectedEndDate: Date;
    private accountId: number = 0;

    // Flags
    private loading: boolean = false;

    private gridHeaderComponentUrl: any;

    private _selectedAccount: AccountDTO;
    public get selectedAccount(): AccountDTO {
        return this._selectedAccount;
    }
    public set selectedAccount(account: AccountDTO) {
        this._selectedAccount = account;
        if (account) {
            this.accountId = account.accountId;
        }
    }
    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        private employeeService: IEmployeeService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Time.Employee.EmployeeSkills", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onBeforeSetUpGrid(() => this.loadTerms())
            .onBeforeSetUpGrid(() => this.loadCompanySettings())
            .onBeforeSetUpGrid(() => this.loadSkills())
            .onBeforeSetUpGrid(() => this.loadPositions())
            .onBeforeSetUpGrid(() => this.loadCategories())
            .onBeforeSetUpGrid(() => this.loadAccountStringIdsByUserFromHierarchy())
            .onSetUpGrid(() => this.setupGrid());

        this.gridHeaderComponentUrl = urlHelperService.getGlobalUrl("Time/Employee/EmployeeSkills/Views/gridHeader.html");
    }

    // SETUP

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        this.flowHandler.start([
            { feature: Feature.Time_Employee_Skills, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_Skills].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_Skills].modifyPermission;
    }
    public setupGrid() {
        // Rename empty row in selections to 'All'
        this.positions[0].name = this.terms["common.all"];
        this.selectedPositionId = this.positions[0].id;

        this.skills[0].name = this.terms["common.all"];
        this.selectedSkillId = this.skills[0].id;

        this.categories[0].name = this.terms["common.all"];
        this.selectedCategoryId = this.categories[0].id;

        this.doubleClickToEdit = false;

        this.gridAg.addColumnText("skillName", this.terms["time.schedule.skill.skill"], null);
        this.gridAg.addColumnText("employeeName", this.terms["common.employee"], null);
        if (this.useAccountsHierarchy)
            this.gridAg.addColumnText("accountName", this.terms["common.user.attestrole.accounthierarchy"], null, true);
        this.gridAg.addColumnText("positions", this.terms["time.employee.position.positions"], null);
        this.gridAg.addColumnDate("endDate", this.terms["common.stopdate"], 100);
        this.gridAg.addColumnText("skillRatingText", this.terms["common.skills.level"], 50);
        this.gridAg.addColumnText("diffSkillRating", this.terms["time.employee.employeeskills.diff"], 50);

        this.gridAg.options.enableRowSelection = false;
        this.gridAg.finalizeInitGrid("time.employee.employeeskills", true);
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "common.all",
            "common.employee",
            "common.stopdate",
            "common.skills.level",
            "time.schedule.skill.skill",
            "time.employee.position.positions",
            "common.user.attestrole.accounthierarchy",
            "time.employee.employeeskills.diff"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimeNbrOfSkillLevels);
        settingTypes.push(CompanySettingType.TimeSkillLevelHalfPrecision);
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.nbrOfSkillLevels = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeNbrOfSkillLevels);
            this.halfPrecision = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeNbrOfSkillLevels);
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadPositions(): ng.IPromise<any> {
        return this.employeeService.getPositionsDict(true, true).then(x => {
            this.positions = x;
        });
    }

    private loadSkills(): ng.IPromise<any> {
        return this.employeeService.getSkillsDict(true, true).then(x => {
            this.skills = x;
        });
    }

    private loadCategories(): ng.IPromise<any> {
        return this.coreService.getCategoriesDict(SoeCategoryType.Employee, true).then(x => {
            this.categories = x;
        });
    }
    private loadAccountStringIdsByUserFromHierarchy(): ng.IPromise<any> {
        this.accounts = [];
        return this.coreService.getAccountsFromHierarchyByUserSetting(CalendarUtility.getDateNow(), CalendarUtility.getDateNow(), true).then(x => {
            this.accounts = x;

                this._selectedAccount = this.accounts.find(a => a.accountId!=0);

        });
    }
    private loadData() {
        this.loading = true;

        this.progress.startLoadingProgress([
            () => this.employeeService.searchEmployeeSkills(this.selectedEmployeeNrFrom, this.selectedEmployeeNrTo, this.selectedCategoryId, this.selectedPositionId, this.selectedSkillId, this.selectedEndDate, this.selectedSkillMissing, this.selectedPositionMissing,this.accountId).then(x => {
                this.employeeSkills = x;
                _.forEach(this.employeeSkills, skill => {
                    skill.employeeSkillRating = this.convertToRating(skill.skillLevel);
                    skill.positionSkillRating = this.convertToRating(skill.skillLevelPosition);
                    skill.diffSkillRating = this.convertToRating(skill.skillLevelDifference);
                });

                this.setData(this.employeeSkills);
                this.loading = false;
            })]);
    }   

    // HELP-METHDS

    private convertToRating(level: number): number {
        return (this.nbrOfSkillLevels > 0 ? Math.round(level / (100 / this.nbrOfSkillLevels)) : 0);
    }
}