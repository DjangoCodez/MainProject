import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { ITimeService } from "../TimeService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { TimePeriodHeadDTO } from "../../../Common/Models/TimePeriodHeadDTO";
import { Feature, CompanySettingType, TermGroup_TimePeriodType, ActionResultSave } from "../../../Util/CommonEnumerations";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Constants } from "../../../Util/Constants";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { ModalUtility } from "../../../Util/ModalUtility";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Data       
    private timePeriodHeadId: number;
    private timePeriodHead: TimePeriodHeadDTO;

    private accounts: AccountDTO[] = [];
    private accountDim: AccountDimSmallDTO;

    private timePeriodHeads: SmallGenericType[] = [];
    private filteredTimePeriodHeads: SmallGenericType[] = [];

    // Company settings
    private useAccountHierarchy = false;
    private defaultEmployeeAccountDimId = 0;
    private useAveragingPeriod = false;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private timeService: ITimeService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private sharedAccountingService: IAccountingService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        this.messagingHandler.onSetDirty(() => { this.dirtyHandler.setDirty() })
    }

    public onInit(parameters: any) {
        this.timePeriodHeadId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_TimeSettings_PlanningPeriod, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    // LOOKUPS

    protected onDoLookups() {
        return this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings(),
            this.loadAccountsByUserFromHierarchy(),
            this.loadTimePeriodHeads()
        ]).then(() => {
            this.$q.all([
                this.loadAccountDim()
            ]);
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.timePeriodHeadId, recordId => {
            if (recordId !== this.timePeriodHeadId) {
                this.timePeriodHeadId = recordId;
                this.onLoadData();
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_TimeSettings_PlanningPeriod].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_TimeSettings_PlanningPeriod].modifyPermission;
    }

    private onLoadData() {
        if (this.timePeriodHeadId > 0) {
            return this.timeService.getTimePeriodHead(this.timePeriodHeadId).then(x => {
                this.timePeriodHead = x;
                this.isNew = false;
                this.setFilteredTimePeriodHeads();
                this.dirtyHandler.clean();
                this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.time.planningperiod.planningperiod"] + ' ' + this.timePeriodHead.name);
            });
        } else {
            this.new();
        }
    }

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.verifyquestion",
            "time.time.planningperiod.planningperiod"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        let settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.UseAccountHierarchy);
        settingTypes.push(CompanySettingType.DefaultEmployeeAccountDimEmployee);
        settingTypes.push(CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            this.defaultEmployeeAccountDimId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.DefaultEmployeeAccountDimEmployee);
            this.useAveragingPeriod = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.TimeCalculatePlanningPeriodScheduledTimeUseAveragingPeriod);
        });
    }

    private loadAccountsByUserFromHierarchy(): ng.IPromise<any> {
        return this.coreService.getAccountsFromHierarchyByUser(CalendarUtility.getDateToday(), CalendarUtility.getDateToday()).then(x => {
            this.accounts = x;
        });
    }

    private loadAccountDim(): ng.IPromise<any> {
        return this.sharedAccountingService.getAccountDimSmall(this.defaultEmployeeAccountDimId, true, false).then(x => {
            this.accountDim = x;

            let validAccountIds: number[] = _.map(this.accounts, a => a.accountId);
            this.accountDim.accounts = _.filter(this.accountDim.accounts, a => _.includes(validAccountIds, a.accountId));
        });
    }

    private loadTimePeriodHeads(): ng.IPromise<any> {
        return this.timeService.getTimePeriodHeadsDict(TermGroup_TimePeriodType.RuleWorkTime, true).then(x => {
            this.timePeriodHeads = x;
            this.setFilteredTimePeriodHeads();
        });
    }

    // ACTIONS
    protected copy() {
        super.copy();
        this.isNew = true;
        this.timePeriodHeadId = 0;
        this.timePeriodHead.timePeriodHeadId = 0;
    }

    private new() {
        this.isNew = true;
        this.timePeriodHeadId = 0;
        this.timePeriodHead = new TimePeriodHeadDTO();
        this.timePeriodHead.timePeriodType = TermGroup_TimePeriodType.RuleWorkTime;
        this.timePeriodHead.timePeriods = [];
        this.setFilteredTimePeriodHeads();
    }

    public save(removePeriodLinks: boolean = false) {
        this.progress.startSaveProgress((completion) => {
            this.timeService.saveTimePeriodHead(this.timePeriodHead, removePeriodLinks).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.timePeriodHeadId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.timePeriodHead.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                        this.timePeriodHeadId = result.integerValue;
                        this.timePeriodHead.timePeriodHeadId = result.integerValue;
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.timePeriodHead);
                    }
                } else {
                    if (result.errorNumber === ActionResultSave.TimePeriodHasEmployeeGroupRuleWork) {
                        const modal = this.notificationService.showDialogEx(this.terms["core.verifyquestion"], result.errorMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                        modal.result.then(val => {
                            if (val) {
                                completion.completed(null, null, true);
                                this.save(true);
                            }
                        }, () => {
                            completion.failed(null, true);
                        });
                    } else {
                        completion.failed(result.errorMessage);
                    }
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

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.timeService.getTimePeriodHeadsForGrid(TermGroup_TimePeriodType.RuleWorkTime, false, true, false).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.timePeriodHeadId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.timePeriodHeadId) {
                    this.timePeriodHeadId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    protected delete(removePeriodLinks: boolean = false) {
        this.progress.startDeleteProgress((completion) => {
            this.timeService.deleteTimePeriodHead(this.timePeriodHead.timePeriodHeadId, removePeriodLinks).then((result) => {
                if (result.success) {
                    completion.completed(this.timePeriodHead, true);
                } else {
                    if (result.errorNumber === ActionResultSave.TimePeriodHasEmployeeGroupRuleWork) {
                        const modal = this.notificationService.showDialogEx(this.terms["core.verifyquestion"], result.errorMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                        modal.result.then(val => {
                            if (val) {
                                completion.completed(null, true);
                                this.delete(true);
                            }
                        }, () => {
                            completion.failed(null, true);
                        });
                    } else {
                        completion.failed(result.errorMessage);
                    }
                }
            }, error => {
                completion.failed(error.message);
            });
        }, null, removePeriodLinks ? ModalUtility.MODAL_SKIP_CONFIRM : '').then(x => {
            super.closeMe(true);
        });
    }

    // HELP-METHODS

    private setFilteredTimePeriodHeads() {
        this.filteredTimePeriodHeads = this.timePeriodHeads.filter(t => t.id !== this.timePeriodHeadId || t.id === 0);
    }

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    public showValidationError() {
        let errors = this['edit'].$error;
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.timePeriodHead) {
                //if (this.useAccountHierarchy && !this.timePeriodHead.accountId && this.accountDim)
                //    validationErrorStrings.push(this.terms["common.missingrequired"].format(this.accountDim.name.toLocaleLowerCase()));

                if (!this.timePeriodHead.name)
                    mandatoryFieldKeys.push("common.name");

                if (errors['periodDatesValid'])
                    validationErrorKeys.push("time.time.planningperiod.invaliddates");
            }
        });
    }
}
