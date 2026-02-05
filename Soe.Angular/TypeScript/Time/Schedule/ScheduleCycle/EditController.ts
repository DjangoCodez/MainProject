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
import { ScheduleCycleDTO, ScheduleCycleRuleTypeDTO, ScheduleCycleRuleDTO } from "../../../Common/Models/ScheduleCycle";
import { IFocusService } from "../../../Core/Services/FocusService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IScheduleService } from "../ScheduleService";
import { ToolBarButtonGroup, ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary } from "../../../Util/Enumerations";
import { Constants } from "../../../Util/Constants";
import { CompanySettingType, Feature, SoeEntityState } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ICoreService } from "../../../Core/Services/CoreService";
import { AccountDTO } from "../../../Common/Models/AccountDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {
    // Company settings
    useAccountsHierarchy: boolean;

    private scheduleCycleId: number;
    private scheduleCycle: ScheduleCycleDTO;
    isNew = true;
    deleteButtonTemplateUrl: string;
    saveButtonTemplateUrl: string;
    modifyPermission: boolean;
    readOnlyPermission: boolean;
    private modal;
    isModal = false;
    private terms: any;
    selectedRow: ScheduleCycleRuleDTO;
    selectableScheduleCycleRuleTypes: ScheduleCycleRuleTypeDTO[]
    scheduleCycleRuleTypes: ScheduleCycleRuleTypeDTO[]

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();
    accounts: AccountDTO[];

    //@ngInject
    constructor(
        protected $uibModal,
        private $scope: ng.IScope,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private scheduleService: IScheduleService,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.deleteButtonTemplateUrl = urlHelperService.getCoreComponent("deleteButtonComposition.html");
        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButtonComposition.html");

        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.modal = parameters.modal;
            this.onInit(parameters);
            this.focusService.focusByName("ctrl_schedulecycleruletype_name");
        });

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.scheduleCycleId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Schedule_StaffingNeeds_ScheduleCycle, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveScheduleCycle(this.scheduleCycle).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.scheduleCycleId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.scheduleCycle);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.loadScheduleCycle();
            }, error => {

            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteScheduleCycle(this.scheduleCycle.scheduleCycleId).then((result) => {
                if (result.success) {
                    completion.completed(this.scheduleCycle);
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

    private doLookups() {
        this.loadCompanySettings();

        if (this.scheduleCycleId > 0) {

            return this.progress.startLoadingProgress([
                () => this.loadTerms(),
                () => this.loadScheduleCycleRuleTypes()
            ]).then(x => {

                this.loadScheduleCycle()
            });
        } else {
            this.loadScheduleCycleRuleTypes();
            this.loadTerms();
            this.isNew = true;
            this.scheduleCycleId = 0;
            this.scheduleCycle = new ScheduleCycleDTO;
            this.scheduleCycle.name = "";
            this.scheduleCycle.description = "";
            this.scheduleCycle.nbrOfWeeks = 0;
            this.scheduleCycle.scheduleCycleRuleDTOs = [];
            this.scheduleCycle.accountId = null;
        }
    }

    // LOOKUPS
    private loadAccountStringIdsByUserFromHierarchy(): ng.IPromise<any> {
        this.accounts = [];
        return this.coreService.getAccountsFromHierarchyByUserSetting(CalendarUtility.getDateNow(), CalendarUtility.getDateNow(),true, false, false, true).then(x => {
            this.accounts = x;
            // Insert Empty
            if (this.scheduleCycleId > 0) {
                var empty: AccountDTO = new AccountDTO;
                empty.accountId = null;
                empty.name = '';
                this.accounts.splice(0, 0, empty);
            }

        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.TimeSetEmploymentPercentManually);
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);

            if (this.useAccountsHierarchy)
                this.loadAccountStringIdsByUserFromHierarchy();
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.warning",
            "time.schedule.schedulecycle.rule.mingreaterthenmax"
        ];
        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadScheduleCycle(): ng.IPromise<any> {
        return this.scheduleService.getScheduleCycle(this.scheduleCycleId).then((x) => {
            this.isNew = false;
            this.scheduleCycle = x;
            // Convert to typed DTOs
            this.scheduleCycle.scheduleCycleRuleDTOs = x.scheduleCycleRuleDTOs.map(s => {
                var obj = new ScheduleCycleRuleDTO();
                angular.extend(obj, s);
                this.refreshRelations(obj);
                return obj;
            });
        });
    }

    private loadScheduleCycleRuleTypes(): ng.IPromise<any> {
        return this.scheduleService.getScheduleCycleRuleTypes().then((x) => {
            this.scheduleCycleRuleTypes = x;
        });
    }

    private selectionChanged() {
        this.dirtyHandler.setDirty();
    }

    private hasRows(): boolean {
        return this.scheduleCycle && this.scheduleCycle.scheduleCycleRuleDTOs && this.scheduleCycle.scheduleCycleRuleDTOs.length > 0;
    }

    private ruleOnFocus(row: ScheduleCycleRuleDTO) {
        this.selectableScheduleCycleRuleTypes = [];
        _.forEach(this.scheduleCycleRuleTypes, (item: ScheduleCycleRuleTypeDTO) => {
            if (!_.find(this.scheduleCycle.scheduleCycleRuleDTOs, x => x.scheduleCycleRuleTypeId === item.scheduleCycleRuleTypeId)) {
                this.selectableScheduleCycleRuleTypes.push(item);
            }
        });
    }

    protected copy() {
        super.copy();

        this.isNew = true;
        this.scheduleCycleId = 0;
        this.scheduleCycle.scheduleCycleId = 0;
        this.scheduleCycle.name = '';
        this.scheduleCycle.accountId = null;

        _.forEach(this.scheduleCycle.scheduleCycleRuleDTOs, row => {
            row.scheduleCycleRuleId = 0;
        });

        this.scheduleCycle.created = null;
        this.scheduleCycle.createdBy = '';
        this.scheduleCycle.modified = null;
        this.scheduleCycle.modifiedBy = '';

        this.dirtyHandler.setDirty();

        this.focusService.focusByName("ctrl_scheduleCycle_name");
    }

    private addRow() {
        if (this.scheduleCycle) {
            var row: ScheduleCycleRuleDTO = new ScheduleCycleRuleDTO();
            row.scheduleCycleRuleId = 0;
            row.scheduleCycleRuleTypeId = 0;
            row.maxOccurrences = 0;
            row.minOccurrences = 0;
            row.state = SoeEntityState.Active;

            if (!this.scheduleCycle.scheduleCycleRuleDTOs)
                this.scheduleCycle.scheduleCycleRuleDTOs = [];
            this.scheduleCycle.scheduleCycleRuleDTOs.push(row);

            this.dirtyHandler.setDirty();
        }
    }

    private deleteRow(row: ScheduleCycleRuleDTO) {
        if (this.scheduleCycle && this.scheduleCycle.scheduleCycleRuleDTOs && row) {
            _.pull(this.scheduleCycle.scheduleCycleRuleDTOs, row);
        }
        this.dirtyHandler.setDirty();
    }

    private refreshRelations(row: ScheduleCycleRuleDTO) {
        if (row) {
            if (this.scheduleCycleRuleTypes)
                row.selectedScheduleCycleRuleType = (_.filter(this.scheduleCycleRuleTypes, { scheduleCycleRuleTypeId: row.scheduleCycleRuleTypeId }))[0];
        }
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.scheduleCycle) {
                if (!this.scheduleCycle.name) {
                    mandatoryFieldKeys.push("common.name");
                }
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Schedule_StaffingNeeds_ScheduleCycle].readPermission;
        this.modifyPermission = response[Feature.Time_Schedule_StaffingNeeds_ScheduleCycle].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);

        if (this.modifyPermission && this.gridButtonGroups.length == 0) {
            this.gridButtonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("time.schedule.schedulecycle.addnewrule", "time.schedule.schedulecycle.addnewrule", IconLibrary.FontAwesome, "fa-plus", () => {
                this.addRow();
            })));
        }
    }
}
