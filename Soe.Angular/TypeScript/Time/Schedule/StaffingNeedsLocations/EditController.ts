import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IScheduleService } from "../ScheduleService";
import { CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Constants } from "../../../Util/Constants";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ICoreService } from "../../../Core/Services/CoreService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Company settings
    private useAccountsHierarchy: boolean = false;

    // Terms
    private terms: any;

    // Data
    staffingNeedsLocationId: number;
    location: any;
    locationGroups: any = [];

    // Lookups

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private coreService: ICoreService,
        private scheduleService: IScheduleService,
        private translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.staffingNeedsLocationId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_NeedsSettings_Locations_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_NeedsSettings_Locations_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_NeedsSettings_Locations_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true,() => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.staffingNeedsLocationId, recordId => {
            if (recordId !== this.staffingNeedsLocationId) {
                this.staffingNeedsLocationId = recordId;
                this.load();
            }
        });
    }

    // LOOKUPS
    
    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadCompanySettings(),
            this.loadTerms()
        ]).then(() => {
            this.loadLocationGroups()
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
        });
    }

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "time.schedule.staffingneedslocation.staffingneedslocation"
        ];
        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private onLoadData() {
        if (this.staffingNeedsLocationId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    }

    private load(): ng.IPromise<any> {
        return this.scheduleService.getStaffingNeedsLocation(this.staffingNeedsLocationId).then((x) => {
            this.isNew = false;
            this.location = x;
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.schedule.staffingneedslocation.staffingneedslocation"] + ' ' + this.location.name);
        });
    }

    private loadLocationGroups(): ng.IPromise<any> {
        return this.scheduleService.getStaffingNeedsLocationGroupsDict(false, this.useAccountsHierarchy).then((x) => {
            this.locationGroups = x;
        });
    }

    // EVENTS
    protected copy() {
        super.copy();
        this.isNew = true;
        this.staffingNeedsLocationId = 0;
        this.location.staffingNeedsLocationId = 0;
    }

    // ACTIONS
    private save() {
        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveStaffingNeedsLocation(this.location).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {

                        if (this.staffingNeedsLocationId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.location.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }
                        this.staffingNeedsLocationId = result.integerValue;
                        this.location.staffingNeedsLocationId = result.integerValue;                       
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.location);
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
        this.scheduleService.getStaffingNeedsLocations().then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.staffingNeedsLocationId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.staffingNeedsLocationId) {
                    this.staffingNeedsLocationId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    protected delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteStaffingNeedsLocation(this.location.staffingNeedsLocationId).then((result) => {
                if (result.success) {
                    completion.completed(this.location, true);
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
    private new() {
        this.isNew = true;
        this.staffingNeedsLocationId = 0;
        this.location = {};
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.location) {
                // Mandatory fields
                if (!this.location.name)
                    mandatoryFieldKeys.push("common.name");
                if (!this.location.staffingNeedsLocationId)
                    mandatoryFieldKeys.push("common.group");
            }
        });
    }
}
