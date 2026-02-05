import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IScheduleService } from "../ScheduleService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { Feature, SoeTimeCodeType, TermGroup, SoeEntityState, CompanySettingType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Constants } from "../../../Util/Constants";
import { TimeScheduleTypeDTO, TimeScheduleTypeFactorDTO } from "../../../Common/Models/TimeScheduleTypeDTO";
import { FactorDialogController } from "./FactorDialogController";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { TimeDeviationCauseDTO } from "../../../Common/Models/TimeDeviationCauseDTOs";
import { ITimeService } from "../../Time/TimeService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private timeScheduleTypeId: number;
    private timeScheduleType: TimeScheduleTypeDTO;
    private selectedFactor: TimeScheduleTypeFactorDTO;
    private hideShowInTerminal: boolean = true;
    private timeDeviationCauses: TimeDeviationCauseDTO;

    //@ngInject
    constructor(
        private $uibModal,
        private $q: ng.IQService,
        private scheduleService: IScheduleService,
        private coreService: ICoreService,
        private timeService: ITimeService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private $timeout: ng.ITimeoutService) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.timeScheduleTypeId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_TimeSettings_TimeScheduleType_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_TimeSettings_TimeScheduleType_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_TimeSettings_TimeScheduleType_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.timeScheduleTypeId, recordId => {
            if (recordId !== this.timeScheduleTypeId) {
                this.timeScheduleTypeId = recordId;
                this.onLoadData();
            }
        });
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms()
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        this.loadCompanySettings();
        this.loadTimeDeviations();
        if (this.timeScheduleTypeId) {
            return this.load();
        } else {
            this.new();
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.delete",
            "common.all",
            "common.newrow",
            "time.schedule.scheduletype.scheduletype"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }
    private loadTimeDeviations() {
        return this.timeService.getTimeDeviationCausesDict(true, true).then(x => {
            this.timeDeviationCauses = x;
        });
    }
    private loadCompanySettings(): ng.IPromise<any> {
        var settingTypes: number[] = [];
        settingTypes.push(CompanySettingType.PossibilityToRegisterAdditionsInTerminal);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.hideShowInTerminal = !SettingsUtility.getBoolCompanySetting(x, CompanySettingType.PossibilityToRegisterAdditionsInTerminal);
        });
    }

    private load(): ng.IPromise<any> {
        return this.scheduleService.getScheduleType(this.timeScheduleTypeId, true).then(x => {
            this.isNew = false;
            this.timeScheduleType = x;
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.schedule.scheduletype.scheduletype"] + ' ' + this.timeScheduleType.name);
        });
    }

    protected copy() {
        super.copy();
        this.isNew = true;
        this.timeScheduleTypeId = 0;
        this.timeScheduleType.timeScheduleTypeId = 0;
    }

    private new() {
        this.isNew = true;
        this.timeScheduleTypeId = 0;
        this.timeScheduleType = new TimeScheduleTypeDTO();
        this.timeScheduleType.isActive = true;
        this.timeScheduleType.factors = [];
    }

    // EVENTS

    private isAllChanged() {
        this.$timeout(() => {
            this.timeScheduleType.name = (this.timeScheduleType.isAll ? this.terms["common.all"] : '');
        });        
    }

    private editFactor(factor: TimeScheduleTypeFactorDTO) {
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getGlobalUrl("Time/Schedule/ScheduleTypes/Views/FactorDialog.html"),
            controller: FactorDialogController,
            controllerAs: "ctrl",
            size: 'md',
            resolve: {
                factor: () => { return factor }
            }
        }
        this.$uibModal.open(options).result.then((result: any) => {
            if (result && result.factor) {
                if (!factor) {
                    // Add new factor to the original collection
                    factor = new TimeScheduleTypeFactorDTO();
                    this.updateFactor(factor, result.factor);
                    this.timeScheduleType.factors.push(factor);
                } else {
                    // Update original factor
                    var originalFactor = _.find(this.timeScheduleType.factors, f => f.timeScheduleTypeFactorId === factor.timeScheduleTypeFactorId);
                    if (originalFactor)
                        this.updateFactor(originalFactor, result.factor);
                }
            }
        });
    }

    private updateFactor(factor: TimeScheduleTypeFactorDTO, input: TimeScheduleTypeFactorDTO) {
        factor.factor = input.factor;
        factor.fromTime = input.fromTime;
        factor.toTime = input.toTime;
        this.setDirty();
    }

    private deleteFactor(factor: TimeScheduleTypeFactorDTO) {
        _.pull(this.timeScheduleType.factors, factor);
        this.setDirty();
    }

    // ACTIONS

    private initSave() {
        if (this.validateSave())
            this.save();
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveScheduleType(this.timeScheduleType).then(result => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.timeScheduleTypeId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.timeScheduleType.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                    this.timeScheduleTypeId = result.integerValue;
                    this.timeScheduleType.timeScheduleTypeId = this.timeScheduleTypeId;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.timeScheduleType);
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
        this.scheduleService.getScheduleTypes(true, false, false, false).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.timeScheduleTypeId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.timeScheduleTypeId) {
                    this.timeScheduleTypeId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteScheduleType(this.timeScheduleType.timeScheduleTypeId).then(result => {
                if (result.success) {
                    completion.completed(this.timeScheduleType);
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

    private setDirty() {
        this.dirtyHandler.setDirty();
    }

    // VALIDATION

    private validateSave(): boolean {
        return true;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.timeScheduleType) {
                if (!this.timeScheduleType.code)
                    mandatoryFieldKeys.push("common.code");
                    
                if (!this.timeScheduleType.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}