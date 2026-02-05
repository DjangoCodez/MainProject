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
import { IncomingDeliveryTypeDTO } from "../../../Common/Models/StaffingNeedsDTOs";
import { IFocusService } from "../../../Core/Services/FocusService";
import { IScheduleService } from "../ScheduleService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { CoreUtility } from "../../../Util/CoreUtility";
import { Feature, CompanySettingType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { AccountDTO } from "../../../Common/Models/AccountDTO";

enum ValidationError {
    Unknown = 0,
    Name = 1,
    LengthIsLowerThanAllowed = 2,
}

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private incomingDeliveryTypeId: number;
    private useAccountsHierarchy: boolean;

    private accounts: AccountDTO[];

    incomingDeliveryType: IncomingDeliveryTypeDTO;
    isNew = true;
    deleteButtonTemplateUrl: string;
    saveButtonTemplateUrl: string;
    modifyPermission: boolean;
    readOnlyPermission: boolean;
    companySettingMinLength: number = 15;
    lengthChanged: boolean = false;

    minLengthInfoLabel: string;

    //@ngInject
    constructor(
        private $timeout: ng.ITimeoutService,
        private $scope: ng.IScope,
        private focusService: IFocusService,
        private scheduleService: IScheduleService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private notificationService: INotificationService,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.deleteButtonTemplateUrl = urlHelperService.getCoreComponent("deleteButtonComposition.html");
        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButtonComposition.html");

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    // SETUP

    public onInit(parameters: any) {
        this.incomingDeliveryTypeId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Time_Preferences_ScheduleSettings_IncomingDeliveryType, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_ScheduleSettings_IncomingDeliveryType].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_ScheduleSettings_IncomingDeliveryType].modifyPermission;
    }

    // LOOKUPS

    private doLookups() {
        return this.loadCompanySettings();
    }

    private onLoadData() {
        if (this.incomingDeliveryTypeId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
            
        }
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength);
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.companySettingMinLength = SettingsUtility.getIntCompanySetting(x, CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength);

            this.translationService.translate('common.min').then(term => {
                this.minLengthInfoLabel = '{0} {1}'.format(term, this.companySettingMinLength.toString());
            });

            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);
            if (this.useAccountsHierarchy) {
                this.loadAccountStringIdsByUserFromHierarchy();

            }
        });
    }

    private loadAccountStringIdsByUserFromHierarchy(): ng.IPromise<any> {
        this.accounts = [];
        return this.coreService.getAccountsFromHierarchyByUserSetting(CalendarUtility.getDateNow(), CalendarUtility.getDateNow(),true, false, false, true).then(x => {
            this.accounts = x;
            
            var empty: AccountDTO = new AccountDTO;
            empty.accountId = null;
            empty.name = '';
            this.accounts.splice(0, 0, empty);

            if (this.isNew && this.accounts.length == 2)
                this.incomingDeliveryType.accountId = this.accounts.find(a => a.accountId).accountId;

        });
    }

    private load(): ng.IPromise<any> {
        return this.scheduleService.getIncomingDeliveryType(this.incomingDeliveryTypeId).then((x) => {
            this.isNew = false;
            this.incomingDeliveryType = x;
        });
    }

    // ACTIONS

    public save() {
        if (this.lengthChanged && this.incomingDeliveryType && this.incomingDeliveryType.incomingDeliveryTypeId > 0) {
            // Show verification dialog
            var keys: string[] = [
                "core.warning",
                "time.schedule.incomingdeliverytype.recalculatewarning",
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["time.schedule.incomingdeliverytype.recalculatewarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val) {
                        this.saveDeliveryType();
                    }
                });
            });
        }
        else {
            this.saveDeliveryType();
        }
    }

    private saveDeliveryType() {
        this.progress.startSaveProgress((completion) => {
            this.incomingDeliveryType.actorCompanyId = CoreUtility.actorCompanyId;
            this.scheduleService.saveIncomingDeliveryType(this.incomingDeliveryType).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.incomingDeliveryTypeId = result.integerValue;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.incomingDeliveryType);
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

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteIncomingDeliveryType(this.incomingDeliveryType.incomingDeliveryTypeId).then((result) => {
                if (result.success) {
                    completion.completed(this.incomingDeliveryType, true);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(false);
        });
    }

    protected copy() {
        if (!this.incomingDeliveryTypeId)
            return;

        super.copy();

        this.isNew = true;
        this.incomingDeliveryTypeId = 0;
        this.incomingDeliveryType.incomingDeliveryTypeId = 0;
        this.incomingDeliveryType.name = "";
        this.incomingDeliveryType.created = null;
        this.incomingDeliveryType.createdBy = "";
        this.incomingDeliveryType.modified = null;
        this.incomingDeliveryType.modifiedBy = "";

        this.dirtyHandler.setDirty();
        this.focusService.focusByName("ctrl_incomingDeliveryType_name");
        this.translationService.translate("time.schedule.incomingdeliverytype.new_incomingdeliverytype").then((term) => {
            this.messagingHandler.publishSetTabLabel(this.guid, term);
        });
    }

    // HELP-METHODS
    private new() {
        this.isNew = true;
        this.incomingDeliveryTypeId = 0;
        this.incomingDeliveryType = new IncomingDeliveryTypeDTO();
    }

    //EVENTS
    private numberChanged(id: string) {
        this.$timeout(() => {
            if (id === 'length') {
                this.lengthChanged = true;
                if (this.incomingDeliveryType.length < 0)
                    this.incomingDeliveryType.length = 0;
            }
            if (id === 'nbrOfPersons') {
                if (this.incomingDeliveryType.nbrOfPersons < 0)
                    this.incomingDeliveryType.nbrOfPersons = 0;
            }
        });
    }

    // VALIDATION
    private validate(): ValidationError[] {
        //var dirty: boolean = false;
        var validationErrors: ValidationError[] = [];

        if (this.incomingDeliveryType) {

            if (this.incomingDeliveryType.length < this.companySettingMinLength) {
                validationErrors.push(ValidationError.LengthIsLowerThanAllowed);
            }
        }
        return validationErrors;
    }

    protected showValidation() {
        var validationErrors: ValidationError[] = this.validate();
        if (validationErrors.length == 0)
            return;

        var keys: string[] = [
            "core.warning",
            "common.name",
            "time.schedule.incomingdeliverytype.validation.lengthminutesislowerthanallowed",
        ];

        return this.translationService.translateMany(keys).then(terms => {
            var message: string = '';

            _.forEach(validationErrors, validationError => {
                if (validationError == ValidationError.LengthIsLowerThanAllowed) {
                    message += terms["time.schedule.incomingdeliverytype.validation.lengthminutesislowerthanallowed"];
                    message += "<br />";
                }
            });
            this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
        });
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.incomingDeliveryType) {
                if (!this.incomingDeliveryType.name) {
                    mandatoryFieldKeys.push("common.name");
                }
                var hasErrorLengthIsLowerThanAllowed: boolean = false;
                var validationErrors: ValidationError[] = this.validate();
                _.forEach(validationErrors, validationError => {
                    if (validationError == ValidationError.LengthIsLowerThanAllowed)
                        hasErrorLengthIsLowerThanAllowed = true;
                });
                if (hasErrorLengthIsLowerThanAllowed)
                    validationErrorKeys.push("time.schedule.incomingdeliverytype.validation.lengthminutesislowerthanallowed");
            }
        });
    }
}
