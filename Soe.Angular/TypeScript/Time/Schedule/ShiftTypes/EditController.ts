import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { Guid } from "../../../Util/StringUtility";
import { AccountingSettingsRowDTO } from "../../../Common/Models/AccountingSettingsRowDTO";
import { SmallGenericType } from "../../../Common/Models/smallgenerictype";
import { ShiftTypeDTO, ShiftTypeSkillDTO } from "../../../Common/Models/ShiftTypeDTO";
import { EmployeePostSkillDTO } from "../../../Common/Models/SkillDTOs";
import { IFocusService } from "../../../Core/Services/FocusService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IScheduleService } from "../ScheduleService";
import { IScheduleService as ISharedScheduleService } from "../../../Shared/Time/Schedule/ScheduleService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IShiftTypeSkillDTO, ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { Feature, TermGroup_TimeScheduleTemplateBlockType, TermGroup, CompanySettingType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { AccountDimDTO } from "../../../Common/Models/AccountDimDTO";
import { AccountDTO } from "../../../Common/Models/AccountDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: any;

    // Permissions
    private hasSchedulePermission: boolean;
    private hasOrderPermission: boolean;
    private hasBookingPermission: boolean;
    private hasStandbyPermission: boolean;
    private hasPlanningPermission: boolean;
    private hasEmployeeStatisticsPermission: boolean;

    // Company settings
    private useAccountHierarchy: boolean = false;
    private defaultEmployeeAccountDimId: number = 0;

    private shiftTypeAccountDim: AccountDimDTO;

    private settings: AccountingSettingsRowDTO[];
    private settingTypes: SmallGenericType[] = [];
    private baseAccounts: SmallGenericType[] = [];

    // Data
    private shiftTypeId: number = 0;
    private shiftType: ShiftTypeDTO;
    private shiftTypeIds: number[];
    private skills: EmployeePostSkillDTO[];
    private convertedSkills: EmployeePostSkillDTO[] = [];
    private linkedToInactivatedAccountInfo: string;

    // Lookups
    private timeScheduleTemplateBlockTypes: ISmallGenericType[];
    private timeScheduleTypes: ISmallGenericType[];

    // Flags
    private modal;
    private isModal = false;
    private isOrder: boolean = false;
    private timeScheduleTypeVisible: boolean = false;
    private timeScheduleTemplateBlockTypeVisible: boolean = false;

    //@ngInject
    constructor(
        protected $uibModal,
        private $scope: ng.IScope,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private scheduleService: IScheduleService,
        private sharedScheduleService: ISharedScheduleService,
        private coreService: ICoreService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.deleteButtonTemplateUrl = urlHelperService.getCoreComponent("deleteButtonComposition.html");
        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButtonComposition.html");

        // Init parameters
        if (soeConfig.type === 'order') {
            this.isOrder = true;
        }

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;
            this.onInit(parameters);
            this.focusService.focusByName("ctrl_shiftType_name");
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[this.isOrder ? Feature.Billing_Preferences_InvoiceSettings_ShiftType_Edit : Feature.Time_Preferences_ScheduleSettings_ShiftType_Edit].readPermission;
        this.modifyPermission = response[this.isOrder ? Feature.Billing_Preferences_InvoiceSettings_ShiftType_Edit : Feature.Time_Preferences_ScheduleSettings_ShiftType_Edit].modifyPermission;
    }

    public onInit(parameters: any) {
        this.shiftTypeId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: this.isOrder ? Feature.Billing_Preferences_InvoiceSettings_ShiftType_Edit : Feature.Time_Preferences_ScheduleSettings_ShiftType_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true,() => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.shiftTypeId, recordId => {
            if (recordId !== this.shiftTypeId) {
                this.shiftTypeId = recordId;
                this.load();
            }
        });
    }

    private doLookups() {
        return this.progress.startLoadingProgress([
            () => this.loadTerms(),
            () => this.loadModifyPermissions(),
            () => this.loadCompanySettings(),
            () => this.loadShiftTypeAccountDim(),
            () => this.loadTimeScheduleTypes()
        ]).then(x => {
            if (this.shiftTypeId > 0) {
                this.load();
            } else {
                this.new();
            }
        });
    }

    // LOOKUPS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.accountingsettings.account",
            "time.schedule.shifttype.shifttype",
            "time.schedule.shifttype.nolinkedaccount",
            "time.schedule.shifttype.addnewaccount",
            "time.schedule.shifttype.linkedtoinactivatedaccountinfo",
        ];
        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;

            this.settingTypes = [];
            this.settingTypes.push(new SmallGenericType(0, this.terms["common.accountingsettings.account"]));
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];

        featureIds.push(Feature.Time_Schedule_SchedulePlanning);
        featureIds.push(Feature.Billing_Order_Planning);
        featureIds.push(Feature.Time_Schedule_SchedulePlanning_Bookings);
        featureIds.push(Feature.Time_Schedule_SchedulePlanning_StandbyShifts);
        featureIds.push(Feature.Time_Schedule_Needs_Planning);
        featureIds.push(Feature.Time_Employee_Statistics);
        featureIds.push(Feature.Billing_Order_Planning_Bookings);
        featureIds.push(Feature.Time_Schedule_Needs_Shifts);

        return this.coreService.hasModifyPermissions(featureIds).then((response) => {
            this.hasSchedulePermission = response[Feature.Time_Schedule_SchedulePlanning];
            this.hasOrderPermission = response[Feature.Billing_Order_Planning];
            this.hasBookingPermission = response[Feature.Time_Schedule_SchedulePlanning_Bookings] || response[Feature.Billing_Order_Planning_Bookings];
            this.hasStandbyPermission = response[Feature.Time_Schedule_SchedulePlanning_StandbyShifts];
            this.hasPlanningPermission = response[Feature.Time_Schedule_Needs_Planning] || response[Feature.Time_Schedule_Needs_Shifts];
            this.hasEmployeeStatisticsPermission = response[Feature.Time_Employee_Statistics];
            this.loadTimeScheduleTemplateBlockTypes();
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.DefaultEmployeeAccountDimEmployee);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.defaultEmployeeAccountDimId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.DefaultEmployeeAccountDimEmployee);
        });
    }

    private loadShiftTypeAccountDim(): ng.IPromise<any> {
        return this.sharedScheduleService.getShiftTypeAccountDim(true).then(x => {
            this.shiftTypeAccountDim = x;

            if (this.shiftTypeAccountDim) {
                //var keys: string[] = [
                //    "time.schedule.shifttype.nolinkedaccount",
                //    "time.schedule.shifttype.addnewaccount"
                //];

                //this.translationService.translateMany(keys).then(terms => {
                    var account: AccountDTO = new AccountDTO();
                    account.accountId = 0;
                    account.numberName = this.terms["time.schedule.shifttype.nolinkedaccount"];
                    this.shiftTypeAccountDim.accounts.splice(0, 0, account);

                    account = new AccountDTO();
                    account.accountId = -1;
                    account.numberName = this.terms["time.schedule.shifttype.addnewaccount"];
                    this.shiftTypeAccountDim.accounts.splice(0, 0, account);
                //});
            }
        });
    }

    private loadTimeScheduleTemplateBlockTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeScheduleTemplateBlockType, true, false).then((x) => {
            this.timeScheduleTemplateBlockTypes = [];
            this.timeScheduleTemplateBlockTypes.push({ id: -1, name: " " })
            _.forEach(x, (type: any) => {
                if (type.id == TermGroup_TimeScheduleTemplateBlockType.Schedule && this.hasSchedulePermission)
                    this.timeScheduleTemplateBlockTypes.push(type);
                if (type.id == TermGroup_TimeScheduleTemplateBlockType.Order && this.hasOrderPermission)
                    this.timeScheduleTemplateBlockTypes.push(type);
                if (type.id == TermGroup_TimeScheduleTemplateBlockType.Booking && this.hasBookingPermission)
                    this.timeScheduleTemplateBlockTypes.push(type);
                if (type.id == TermGroup_TimeScheduleTemplateBlockType.Standby && this.hasStandbyPermission)
                    this.timeScheduleTemplateBlockTypes.push(type);
            });
            if (_.size(this.timeScheduleTemplateBlockTypes) > 2)
                this.timeScheduleTemplateBlockTypeVisible = true;
        });
    }

    private loadTimeScheduleTypes(): ng.IPromise<any> {
        return this.scheduleService.getTimeScheduleTypesDict(false, true).then((x) => {
            this.timeScheduleTypes = x;
            if (_.size(this.timeScheduleTypes) > 1)
                this.timeScheduleTypeVisible = true;
        });
    }

    private load(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([() => {
            return this.scheduleService.getShiftType(this.shiftTypeId, true, true, this.hasEmployeeStatisticsPermission, this.hasEmployeeStatisticsPermission, true, this.useAccountHierarchy).then((x) => {
                this.isNew = false;
                this.shiftType = x;
                
                if (this.shiftTypeAccountDim && !this.shiftType.accountId)
                    this.shiftType.accountId = 0;

                // Convert to employeepost skill for skills directive
                this.convertedSkills = [];
                _.forEach(this.shiftType.shiftTypeSkills, (y: IShiftTypeSkillDTO) => {
                    let skillToAdd = new EmployeePostSkillDTO;
                    skillToAdd.employeePostSkillId = y.shiftTypeSkillId;
                    skillToAdd.skillId = y.skillId;
                    skillToAdd.skillLevel = y.skillLevel;
                    skillToAdd.skillLevelStars = y.skillLevelStars;
                    skillToAdd.skillName = y.skillName;
                    skillToAdd.skillTypeName = y.skillTypeName;
                    this.convertedSkills.push(skillToAdd);
                });
                this.skills = this.convertedSkills;

                this.settings = [];
                this.settings.push(this.shiftType.accountingSettings);

                this.linkedToInactivatedAccountInfo = "";
                if(this.shiftType.accountIsNotActive)
                    this.linkedToInactivatedAccountInfo = this.terms["time.schedule.shifttype.linkedtoinactivatedaccountinfo"].format(this.shiftType.accountNrAndName);

                this.dirtyHandler.clean();
                this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.schedule.shifttype.shifttype"] + ' ' + this.shiftType.name);
            });
        }]);
    }

    // EVENTS

    private linkedAccountChanged(accountId: number) {
        if (accountId === -1 || accountId === 0) {
            this.shiftType.needsCode = '';
            this.setShiftTypeAccount(null);
        } else {
            var account = _.find(this.shiftTypeAccountDim.accounts, a => a.accountId === accountId);
            if (account) {
                this.shiftType.needsCode = account.accountNr;
                this.setShiftTypeAccount(account);
            }
        }
    }

    // ACTIONS

    private save() {
        if (this.skills) {
            this.shiftType.shiftTypeSkills = [];
            _.forEach(this.skills, (y: EmployeePostSkillDTO) => {
                var skillToAdd = new ShiftTypeSkillDTO;
                skillToAdd.shiftTypeSkillId = y.employeePostSkillId;
                skillToAdd.skillId = y.skillId;
                skillToAdd.skillLevel = y.skillLevel;
                skillToAdd.skillLevelStars = y.skillLevelStars;
                skillToAdd.skillName = y.skillName;
                skillToAdd.skillTypeName = y.skillTypeName;
                this.shiftType.shiftTypeSkills.push(skillToAdd);
            });
        }

        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveShiftType(this.shiftType).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.shiftTypeId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.shiftType.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }
                        this.shiftTypeId = result.integerValue;
                        this.shiftType.shiftTypeId = result.integerValue;
                       completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.shiftType);
                    }

                    this.loadShiftTypeAccountDim().then(() => {
                        this.$scope.$broadcast('reloadAccounts', null);
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.shiftType);
                    });
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.load();
            }, error => {

            });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.scheduleService.getShiftTypesGrid(false, false, false, true, true, true, true, true, false).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.shiftTypeId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.shiftTypeId) {
                    this.shiftTypeId = recordId;
                    this.load();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    protected delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteShiftType(this.shiftType.shiftTypeId).then((result) => {
                if (result.success) {
                    completion.completed(this.shiftType, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(true);
        });
    }

    // HELP-METHODS
    protected copy() {
        super.copy();
        this.isNew = true;
        this.shiftTypeId = 0;
        this.shiftType.shiftTypeId = 0;
    }

    private new() {
        this.isNew = true;
        this.shiftTypeId = 0;
        this.shiftType = new ShiftTypeDTO;
        this.shiftType.name = "";
        this.shiftType.description = "";
        this.shiftType.needsCode = "";
        this.shiftType.color = Constants.SHIFT_TYPE_UNSPECIFIED_COLOR;
        this.shiftType.categoryIds = [];

        if (this.shiftTypeAccountDim)
            this.shiftType.accountId = -1;

        this.settings = [];
        this.shiftType.accountingSettings = new AccountingSettingsRowDTO(0);
        this.settings.push(this.shiftType.accountingSettings);
    }

    private setShiftTypeAccount(account: AccountDTO) {
        if (this.settings && this.settings.length > 0 && this.shiftTypeAccountDim) {
            this.$scope.$broadcast('setAccount', {
                rowIndex: 0,
                accountDimId: this.shiftTypeAccountDim.accountDimId,
                accountId: account ? account.accountId : 0
            });
        }
    }

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.shiftType) {
                if (!this.shiftType.name) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }
}
