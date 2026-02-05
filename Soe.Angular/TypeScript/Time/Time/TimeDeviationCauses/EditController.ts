import { ICoreService } from "../../../Core/Services/CoreService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITimeService } from "../TimeService";
import { Feature, CompanySettingType } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IFocusService } from "../../../Core/Services/focusservice";
import { IScheduleService } from "../../Schedule/ScheduleService";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IActionResult } from "../../../Scripts/TypeLite.Net4";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { Constants } from "../../../Util/Constants";
import { TimeDeviationCauseDTO } from "../../../Common/Models/TimeDeviationCauseDTOs";


export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    private terms: { [index: string]: string; };
    private timeDeviationCauseId: number;
    timeDeviationCause: TimeDeviationCauseDTO;
    types: any = [];
    timeCodes: any = [];
    private scheduleService: IScheduleService;
    showFields = false;
    useExtendedTimeRegistration = false;
    //@ngInject
    constructor(
        $uibModal,
        private $q: ng.IQService,
        private coreService: ICoreService,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private timeService: ITimeService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.timeDeviationCauseId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_TimeSettings_TimeDeviationCause_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }
    // SETUP
    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_TimeSettings_TimeDeviationCause_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_TimeSettings_TimeDeviationCause_Edit].modifyPermission;
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [CompanySettingType.ProjectUseExtendedTimeRegistration];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.useExtendedTimeRegistration = x[CompanySettingType.ProjectUseExtendedTimeRegistration]; 
        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => !this.timeDeviationCauseId);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.timeDeviationCauseId, recordId => {
            if (recordId !== this.timeDeviationCauseId) {
                this.timeDeviationCauseId = recordId;
                this.onLoadData();
            }
        });
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadCompanySettings()
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.timeDeviationCauseId) {
            return this.progress.startLoadingProgress([
                () =>this.loadCodes(),
                () => this.load(),
               
            ]);
        } else {
            this.new();
        }
    }
    private load(): ng.IPromise<any> {
        return this.timeService.getTimeDeviationCause(this.timeDeviationCauseId).then(x => {
            this.isNew = false;
            this.setTypes();
            this.timeDeviationCause = x;
            this.dirtyHandler.clean();
            this.showFields = this.timeDeviationCause.changeDeviationCauseAccordingToPlannedAbsence;
          
          
        });
    }
    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "time.time.timecodeworks.timecodework",
            "common.absence",
            "time.time.attest.presence",
            "time.time.timedeviationcause.absencepresence",

        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
         
        });
    }
    private new() {

        this.isNew = true;
        this.timeDeviationCauseId = 0;
        this.timeDeviationCause = new TimeDeviationCauseDTO();
        this.timeDeviationCause.timeDeviationCauseId = 0;
        this.timeDeviationCause.name = "";
        this.timeDeviationCause.description = "";
        this.setTypes();
        this.timeDeviationCause.type = 3;
        this.loadCodes();
        this.focusService.focusByName("ctrl_timeDeviationCause_name");
    }
    private setTypes() {
        
        this.types.length = 0;
        this.types.push({ id: 1, name: this.terms["common.absence"] });
        this.types.push({ id: 3, name: this.terms["time.time.timedeviationcause.absencepresence"] });
        this.types.push({ id: 2, name: this.terms["time.time.attest.presence"] });

    }

    private loadCodes(): ng.IPromise<any> {
        this.timeCodes = [];
        return this.timeService.getTimeCodesDict(0, true, false, true).then((x) => {
            this.timeCodes = x;
        });
    }
    
    protected copy() {
        super.copy();
        this.timeDeviationCauseId = 0;
        this.timeDeviationCause.timeDeviationCauseId = 0;
        this.timeDeviationCause.name = "";
        this.timeDeviationCause.description = "";
        this.focusService.focusByName("ctrl_timeDeviationCause_name");
    }

    private initSave() {
        if (this.validateSave())
            this.save();
    }
    private checkFields() {

        this.showFields = !this.timeDeviationCause.changeDeviationCauseAccordingToPlannedAbsence;
    }
    private save() {
        this.progress.startSaveProgress((completion) => {
            this.timeService.saveTimeDeviationCauses(this.timeDeviationCause).then(result => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.timeDeviationCause) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.timeDeviationCause.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                        this.timeDeviationCauseId = result.integerValue;
                        this.timeDeviationCause.timeDeviationCauseId = this.timeDeviationCauseId;
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.timeDeviationCauseId);
                    }
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.onLoadData();
        }, error => {
        });
    }
    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.timeService.getTimeDeviationCausesDict(false,false).then(x => {
            this.navigatorRecords = x;
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.timeDeviationCauseId) {
                    this.timeDeviationCauseId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    private delete() {
            this.progress.startDeleteProgress((completion) => {
                this.timeService.deleteTimeDeviationCause(this.timeDeviationCause).then((result: IActionResult) => {
                    if (result.success) {
                        completion.completed(this.timeDeviationCause, false, null);
                        this.closeMe(true);
                    }
                    else {
                        if (result.errorMessage) {
                            completion.failed(result.errorMessage);
                        }
                       
                    }
                }, error => {
                    completion.failed(error.message);
                });
        });
        
    }
    // VALIDATION

    private validateSave(): boolean {
        return true;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.timeDeviationCause) {
                if (!this.timeDeviationCause.name)
                    mandatoryFieldKeys.push("common.name");

            }
        });
    }
}
