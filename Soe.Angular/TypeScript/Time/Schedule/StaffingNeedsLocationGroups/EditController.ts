import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IScheduleService } from "../ScheduleService";
import { CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Constants } from "../../../Util/Constants";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ICoreService } from "../../../Core/Services/CoreService";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    terms: any;
    // Data
    staffingNeedsLocationGroupId: number;
    locationGroup: any;
    private useAccountsHierarchy: boolean;
    
    // Lookups
    timeScheduleTasks: any = [];
    accounts: AccountDTO[];

    //@ngInject
    constructor(
        private scheduleService: IScheduleService,
        urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private $q: ng.IQService,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.staffingNeedsLocationGroupId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_NeedsSettings_LocationGroups_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_NeedsSettings_LocationGroups_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_NeedsSettings_LocationGroups_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.staffingNeedsLocationGroupId, recordId => {
            if (recordId !== this.staffingNeedsLocationGroupId) {
                this.staffingNeedsLocationGroupId = recordId;
                this.onLoadData();
            }
        });
    }

    // LOOKUPS
    protected doLookups() {
        return this.loadTerms().then(() => {
            return this.$q.all([
                this.loadCompanySettings(),
                this.loadTimeScheduleTasks()
            ]);

        });
    }

    private onLoadData() {
        if (this.staffingNeedsLocationGroupId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "time.schedule.staffingneedslocationgroup.staffingneedslocationgroup"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
           
        });
    }
    private loadAccountStringIdsByUserFromHierarchy(): ng.IPromise<any> {
        this.accounts = [];
        return this.coreService.getAccountsFromHierarchyByUserSetting(CalendarUtility.getDateNow(), CalendarUtility.getDateNow(), true, false, false, true).then(x => {
            this.accounts = x;
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

    private load(): ng.IPromise<any> {
        return this.scheduleService.getStaffingNeedsLocationGroup(this.staffingNeedsLocationGroupId).then((x) => {
            this.isNew = false;
            this.locationGroup = x;
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.schedule.staffingneedslocationgroup.staffingneedslocationgroup"] + ' ' + this.locationGroup.name);

            if (this.locationGroup.accountId > 0) {
                // Insert empty
                var account: AccountDTO = new AccountDTO();
                account.accountId = 0;
                account.name = "";
                this.accounts.splice(0, 0, account);
            }
        });
    }

    private loadTimeScheduleTasks(): ng.IPromise<any> {
        return this.scheduleService.getTimeScheduleTasksDict().then((x) => {
            this.timeScheduleTasks = [];
            this.timeScheduleTasks = x;
        });
    }

    // ACTIONS

    private save() {
        var ids: number[] = [];
        
        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveStaffingNeedsLocationGroup(this.locationGroup, ids).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.staffingNeedsLocationGroupId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.locationGroup.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }
                        this.staffingNeedsLocationGroupId = result.integerValue;
                        this.locationGroup.staffingNeedsLocationGroupId = result.integerValue;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.locationGroup);
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
            });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.scheduleService.getStaffingNeedsLocationGroups().then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.staffingNeedsLocationGroupId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.staffingNeedsLocationGroupId) {
                    this.staffingNeedsLocationGroupId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    private delete() {
        if (!this.locationGroup.staffingNeedsLocationGroupId)
            return;

        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteStaffingNeedsLocationGroup(this.locationGroup.staffingNeedsLocationGroupId).then((result) => {
                if (result.success) {
                    completion.completed(this.locationGroup, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(true);
        });
    }

    // HELP-METHODS

    protected copy() {
        super.copy();
        this.isNew = true;
        this.staffingNeedsLocationGroupId = 0;
        this.locationGroup.staffingNeedsLocationGroupId = 0;
    }


    private new() {
        this.isNew = true;
        this.staffingNeedsLocationGroupId = 0;
        this.locationGroup = {};
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.locationGroup) {
                // Mandatory fields
                if (!this.locationGroup.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}
