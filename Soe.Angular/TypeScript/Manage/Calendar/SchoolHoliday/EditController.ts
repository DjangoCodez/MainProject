import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IEditControllerFlowHandler, IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IDirtyHandler } from "../../../Core/Handlers/DirtyHandler";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandler } from "../../../Core/Handlers/ValidationSummaryHandler";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { Guid } from "../../../Util/StringUtility";
import { ICalendarService } from "../CalendarService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { CompanySettingType, Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { AccountDTO } from "../../../Common/Models/AccountDTO";
import { ICoreService } from "../../../Core/Services/CoreService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SchoolHolidayDTO } from "../../../Common/Models/SchoolHoliday";
import { IFocusService } from "../../../Core/Services/focusservice";
import { ITranslationService } from "../../../Core/Services/TranslationService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {
    //Company Settings
    private useAccountsHierarchy: boolean;
    
    private accounts: AccountDTO[];
    private schoolHolidayId: number;
    private schoolHoliday: SchoolHolidayDTO;
    isNew = false;
    deleteButtonTemplateUrl: string;
    saveButtonTemplateUrl: string;
    modifyPermission: boolean;
    readOnlyPermission: boolean;

    private _selectedAccount: AccountDTO;
    public get selectedAccount(): AccountDTO {
        return this._selectedAccount;
    }
    public set selectedAccount(account: AccountDTO) {
        this._selectedAccount = account;
        if (account) {
           this.schoolHoliday.accountId = account.accountId;
           this.schoolHoliday.accountName = account.name;
        }
    }

    //@ngInject
    constructor(
        private calendarService: ICalendarService,
        urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.deleteButtonTemplateUrl = urlHelperService.getCoreComponent("deleteButtonComposition.html");
        this.saveButtonTemplateUrl = urlHelperService.getCoreComponent("saveButtonComposition.html");

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.loadAccount())
            .onLoadData(() => this.doLoad())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.schoolHolidayId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Preferences_Registry_SchoolHoliday, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.TimeSchedulePlanningDayViewMinorTickLength);
        settingTypes.push(CompanySettingType.UseAccountHierarchy);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useAccountsHierarchy = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UseAccountHierarchy);

           });
    }
    public save() {
        this.progress.startSaveProgress((completion) => {
            this.schoolHoliday.actorCompanyId = CoreUtility.actorCompanyId;
            this.calendarService.saveSchoolHoliday(this.schoolHoliday).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.schoolHolidayId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.schoolHoliday);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.loadData();
            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.calendarService.deleteSchoolHoliday(this.schoolHoliday.schoolHolidayId).then((result) => {
                if (result.success) {
                    completion.completed(this.schoolHoliday);
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

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.schoolHoliday) {
                if (!this.schoolHoliday.name) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (!this.schoolHoliday.dateFrom) {
                    mandatoryFieldKeys.push("common.fromdate");
                }
                if (!this.schoolHoliday.dateTo) {
                    mandatoryFieldKeys.push("common.todate");
                }
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Preferences_Registry_SchoolHoliday].readPermission;
        this.modifyPermission = response[Feature.Manage_Preferences_Registry_SchoolHoliday].modifyPermission;
    }
    private loadAccount(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.loadCompanySettings(),
            () => this.loadAccountStringIdsByUserFromHierarchy()]);

    }
   
    private doLoad() {
        if (this.schoolHolidayId > 0) {
            return this.progress.startLoadingProgress([
                () => this.loadData()
            ])
        } else {
            this.loadCompanySettings();
            this.loadAccountStringIdsByUserFromHierarchy();
            this.selectedAccount = _.find(this.accounts, a => a.accountId == this.schoolHoliday.accountId);
            this.isNew = true;
            this.schoolHolidayId = 0;
            this.schoolHoliday = new SchoolHolidayDTO;
          
        }
    }
    private loadAccountStringIdsByUserFromHierarchy(): ng.IPromise<any> {
        this.accounts = [];
         return this.coreService.getAccountsFromHierarchyByUserSetting(CalendarUtility.getDateNow(), CalendarUtility.getDateNow(),true,false,false,true).then(x => {
            this.accounts = x;
             // Insert Empty
            
            var empty: AccountDTO = new AccountDTO;
            empty.accountId = null;
            empty.name = '';
            this.accounts.splice(0, 0, empty);
            if (this.isNew && this.accounts.length == 2) {
                 this._selectedAccount = this.accounts.find(a => a.accountId);
             }
             
        
        });
    }
    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, ()=> this.copy(), () => this.isNew);
    }

    private loadData(): ng.IPromise<any> {
        return this.calendarService.getSchoolHoliday(this.schoolHolidayId).then((x) => {
            this.isNew = false;
            this.schoolHoliday = x;
            this.selectedAccount = _.find(this.accounts, a => a.accountId == this.schoolHoliday.accountId);
            
            if (this.schoolHoliday.dateFrom)
                this.schoolHoliday.dateFrom = new Date(this.schoolHoliday.dateFrom);
            if (this.schoolHoliday.dateTo)
                this.schoolHoliday.dateTo = new Date(this.schoolHoliday.dateTo);
            
        });
    }

    protected copy() {
        if (!this.schoolHolidayId)
            return;

        super.copy();

        this.isNew = true;
        this.schoolHolidayId = 0;
        this.schoolHoliday.schoolHolidayId = 0;
        this.schoolHoliday.name = "";
        this.schoolHoliday.created = null;
        this.schoolHoliday.createdBy = "";
        this.schoolHoliday.modified = null;
        this.schoolHoliday.modifiedBy = "";

        this.dirtyHandler.setDirty();
        this.focusService.focusByName("ctrl_schoolHoliday_name");
        this.translationService.translate("manage.calendar.schoolholiday.new_schoolholiday").then((term) => {
            this.messagingHandler.publishSetTabLabel(this.guid, term);
        });
    }
}
