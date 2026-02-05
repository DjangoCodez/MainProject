import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { ITimeService } from "../TimeService";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { TimePeriodHeadDTO } from "../../../Common/Models/TimePeriodHeadDTO";
import { Feature, TermGroup, CompanySettingType, TermGroup_TimePeriodType, ActionResultSave } from "../../../Util/CommonEnumerations";
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
import { SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ModalUtility } from "../../../Util/ModalUtility";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    timePeriodHeadId: number;
    timePeriod: TimePeriodHeadDTO;

    // Lookups
    types: any[];
    usePayroll: boolean = false;
    payrollStartDate: Date;

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    //@ngInject
    constructor(
        private timeService: ITimeService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        private $q: ng.IQService,
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
        this.flowHandler.start([{ feature: Feature.Time_Preferences_TimeSettings_TimePeriodHead_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    // LOOKUPS
    protected onDoLookups() {
        return this.$q.all([
            this.loadTerms(),
            this.loadPeriodTypes(),
            this.loadCompanySettings()
        ]);
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
        this.readOnlyPermission = response[Feature.Time_Preferences_TimeSettings_TimePeriodHead_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_TimeSettings_TimePeriodHead_Edit].modifyPermission;
    }

    private onLoadData() {
        if (this.timePeriodHeadId > 0) {
            return this.timeService.getTimePeriodHead(this.timePeriodHeadId).then(x => {
                this.timePeriod = x;
                this.isNew = false;
                this.dirtyHandler.clean();
                this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.time.timeperiod.timeperiod"] + ' ' + this.timePeriod.name);
            });
        } else {
            this.new();
        }
    }

    private loadTerms(): ng.IPromise<any> {
        let keys: string[] = [
            "core.verifyquestion",
            "time.time.timeperiod.timeperiod"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadPeriodTypes(): ng.IPromise<any> {
        this.types = [];
        return this.coreService.getTermGroupContent(TermGroup.TimePeriodHeadType, false, false).then(x => {
            this.types = _.filter(x, y => y.id != TermGroup_TimePeriodType.RuleWorkTime);
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.UsePayroll);
        settingTypes.push(CompanySettingType.UsedPayrollSince);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.usePayroll = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.UsePayroll);
            this.payrollStartDate = CalendarUtility.convertToDate(SettingsUtility.getStringCompanySetting(x, CompanySettingType.UsedPayrollSince));
        });
    }

    // ACTIONS

    public save(removePeriodLinks: boolean = false) {
        this.progress.startSaveProgress((completion) => {
            this.timeService.saveTimePeriodHead(this.timePeriod, removePeriodLinks).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.timePeriodHeadId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.timePeriod.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }

                        this.timePeriodHeadId = result.integerValue;


                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.timePeriod);
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
        this.timeService.getTimePeriodHeadsForGrid(TermGroup_TimePeriodType.Unknown, true, false, false).then(data => {
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
            this.timeService.deleteTimePeriodHead(this.timePeriod.timePeriodHeadId, removePeriodLinks).then((result) => {
                if (result.success) {
                    completion.completed(this.timePeriod, true);
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

    protected copy() {
        super.copy();
        this.isNew = true;
        this.timePeriodHeadId = 0;
        this.timePeriod.timePeriodHeadId = 0;
    }

    private new() {
        this.isNew = true;
        this.timePeriodHeadId = 0;
        this.timePeriod = new TimePeriodHeadDTO();
        this.timePeriod.timePeriods = [];
    }

    public showValidationError() {
        var errors = this['edit'].$error;
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.timePeriod) {
                if (!this.timePeriod.timePeriodType)
                    mandatoryFieldKeys.push("time.time.timeperiod.type");
                if (!this.timePeriod.name)
                    mandatoryFieldKeys.push("common.name");
                if (errors['periodsValid'])
                    validationErrorKeys.push("time.time.timeperiod.periodsnotvalid");
            }
        });
    }
}
