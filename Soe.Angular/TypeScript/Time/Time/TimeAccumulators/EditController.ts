import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../TimeService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { TimeAccumulatorDTO, TimeAccumulatorTimeCodeDTO, TimeWorkReductionEarningDTO } from "../../../Common/Models/TimeAccumulatorDTOs";
import { Feature, SoeTimeCodeType, TermGroup, SoeEntityState, TermGroup_TimePeriodType, TermGroup_TimeAccumulatorType, TermGroup_TimeWorkReductionPeriodType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";
import { IFocusService } from "../../../Core/Services/focusservice";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private timeAccumulatorId: number;
    private timeAccumulator: TimeAccumulatorDTO;
    private accumulatorTypes: ISmallGenericType[];
    private timePeriodHeads: ISmallGenericType[];
    private timeWorkReductionPeriodTypes: ISmallGenericType[];
    private timeCodes: ISmallGenericType[];
    private showTimeCodes: boolean = false;
    private useTimeWorkReductionEarning: boolean = false;
    private useTimeWorkReductionEarningOnload: boolean = false;
    //@ngInject
    constructor(
        private $q: ng.IQService,
        private timeService: ITimeService,
        private coreService: ICoreService,
        private notificationService: INotificationService,
        private focusService: IFocusService,
        private translationService: ITranslationService,
        private $timeout: ng.ITimeoutService,
        private urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.timeAccumulatorId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_TimeSettings_TimeAccumulator_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_TimeSettings_TimeAccumulator_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_TimeSettings_TimeAccumulator_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => !this.timeAccumulatorId);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.timeAccumulatorId, recordId => {
            if (recordId !== this.timeAccumulatorId) {
                this.timeAccumulatorId = recordId;
                this.onLoadData();
            }
        });
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadAccumulatorTypes(),
            this.loadTimePeriodHeads(),
            this.loadTimeCodes(),
            this.loadTimeWorkReductionPeriodType(),
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.timeAccumulatorId) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "time.time.timeaccumulator.timeaccumulator",
            "time.time.timeworkreduction.removeearningwarning",
            "core.warning"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadAccumulatorTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeAccumulatorType, false, true).then(x => {
            this.accumulatorTypes = x;
        });
    }
    private loadTimeWorkReductionPeriodType(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeWorkReductionPeriodType, false, true).then(x => {
            this.timeWorkReductionPeriodTypes = x;
        });
    }

    private loadTimePeriodHeads(): ng.IPromise<any> {
        return this.timeService.getTimePeriodHeadsDict(TermGroup_TimePeriodType.RuleWorkTime, true).then(x => {
            this.timePeriodHeads = x;
        });
    }

    private loadTimeCodes(): ng.IPromise<any> {
        return this.timeService.getTimeCodesDict(SoeTimeCodeType.WorkAndAbsenseAndAdditionDeduction, false, false).then(x => {
            this.timeCodes = x;
        });
    }

    private load(): ng.IPromise<any> {
        return this.timeService.getTimeAccumulator(this.timeAccumulatorId, false, true, true).then(x => {
            this.isNew = false;
            this.timeAccumulator = x;
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.time.timeaccumulator.timeaccumulator"] + ' ' + this.timeAccumulator.name);
            this.showHideTimeCodes(true);
            this.useTimeWorkReductionEarning = false;
            this.useTimeWorkReductionEarningOnload = false;

            if (this.timeAccumulator.timeWorkReductionEarningId !== undefined) {
                this.useTimeWorkReductionEarning = true;
                this.useTimeWorkReductionEarningOnload = true;
                this.onCheckboxChanged('useTimeWorkReductionEarning',true);
            }

        });
    }

    private new() {
        this.isNew = true;
        this.timeAccumulatorId = 0;
        this.timeAccumulator = new TimeAccumulatorDTO();
        this.timeAccumulator.state = SoeEntityState.Active;
        this.timeAccumulator.type = TermGroup_TimeAccumulatorType.Rolling;
        this.timeAccumulator = new TimeAccumulatorDTO();
    }

    // EVENTS

    private timeCodeChanged(timeCodeId) {
        if (!this.timeAccumulator)
            return;

        this.timeAccumulator.timeCodes = _.filter(this.timeAccumulator.timeCodes, { isHeadTimeCode: false });

        if (timeCodeId > 0 && !this.useTimeWorkReductionEarning) {
            var timeAccumulatorTimeCode = new TimeAccumulatorTimeCodeDTO();
            timeAccumulatorTimeCode.timeCodeId = timeCodeId;
            timeAccumulatorTimeCode.factor = 1;
            timeAccumulatorTimeCode.importDefault = false;
            timeAccumulatorTimeCode.isHeadTimeCode = true;
            this.timeAccumulator.timeCodes.push(timeAccumulatorTimeCode);
        } 
    }

    // ACTIONS

    protected copy() {
        super.copy();

        this.timeAccumulatorId = this.timeAccumulator.timeAccumulatorId = 0;
        this.timeAccumulator.timeWorkReductionEarningId = 0;
        if (this.timeAccumulator.timeWorkReductionEarning != null) {
            this.timeAccumulator.timeWorkReductionEarning.timeWorkReductionEarningId = 0;

            if (this.timeAccumulator.timeWorkReductionEarning.timeAccumulatorTimeWorkReductionEarningEmployeeGroup) {
                this.timeAccumulator.timeWorkReductionEarning.timeAccumulatorTimeWorkReductionEarningEmployeeGroup.forEach(group => {
                    group.timeAccumulatorTimeWorkReductionEarningEmployeeGroupId = 0;
                });
            }

        }

        this.focusService.focusByName("ctrl_timeAccumulator_name");
    }

    private save() {
        if (this.validateSave())
            this.peformSave();
        else
            this.showValidationError();
    }

    private peformSave() {
        if (this.useTimeWorkReductionEarningOnload && !this.useTimeWorkReductionEarning) {
            if (this.timeAccumulatorId != 0) {
                const modal = this.notificationService.showDialogEx(this.terms["core.warning"], this.terms["time.time.timeworkreduction.removeearningwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    this.doSave();
                });
            } else {
                this.doSave();
            }
        } else {
            this.doSave();
        }
    }


    private doSave() {
        if (!this.useTimeWorkReductionEarning) {
            if (this.timeAccumulator.timeWorkReductionEarning) 
                this.timeAccumulator.timeWorkReductionEarning = null;
            
            if (this.timeAccumulator.timeWorkReductionEarningId > 0)
                this.timeAccumulator.timeWorkReductionEarningId = null;
        }

        this.progress.startSaveProgress((completion) => {
            this.timeService.saveTimeAccumulator(this.timeAccumulator).then(result => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.timeAccumulatorId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.timeAccumulator.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                        if (result.infoMessage && result.infoMessage.length > 0) {
                            let infoMessage = result.infoMessage;
                            if (result.strings != null)
                                infoMessage += "\n" + result.strings.join("\n");

                            this.notificationService.showDialog(this.terms["core.warning"], infoMessage, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                        }
                        this.timeAccumulatorId = result.integerValue;
                        this.timeAccumulator.timeAccumulatorId = this.timeAccumulatorId;
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.timeAccumulator);
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
        this.timeService.getTimeAccumulators(false).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.timeAccumulatorId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.timeAccumulatorId) {
                    this.timeAccumulatorId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.timeService.deleteTimeAccumulator(this.timeAccumulator.timeAccumulatorId).then(result => {
                if (result.success) {
                    completion.completed(this.timeAccumulator, true);
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

    private showHideTimeCodes(load: boolean= false) {
        this.$timeout(() => {
            this.showTimeCodes = this.timeAccumulator.useTimeWorkAccount ||
                this.timeAccumulator.finalSalary ||
                this.useTimeWorkReductionEarning;
            
            if (!this.showTimeCodes) {
                this.timeAccumulator.timeCodeId = undefined;
                this.timeCodeChanged(undefined);
            } else if (this.timeAccumulator.timeCodeId !== undefined && !load) {
                    this.timeCodeChanged(this.timeAccumulator.timeCodeId);
            }
        });
    }

    public onCheckboxChanged(changed: string, load: boolean = false) {
        this.$timeout(() => { 
            if (changed === 'useTimeWorkAccount' && this.timeAccumulator.useTimeWorkAccount) {
                this.useTimeWorkReductionEarning = false;
                this.timeAccumulator.useTimeWorkReductionWithdrawal = false;
            }
            if (changed === 'useTimeWorkReductionEarning' && this.useTimeWorkReductionEarning) {
                this.timeAccumulator.useTimeWorkAccount = false;
                this.timeAccumulator.useTimeWorkReductionWithdrawal = false;
                
                if (!this.timeAccumulator.timeWorkReductionEarning) {
                    this.timeAccumulator.timeWorkReductionEarning = new TimeWorkReductionEarningDTO();
                    this.timeAccumulator.timeWorkReductionEarning.timeWorkReductionEarningId = 0;
                    this.timeAccumulator.timeWorkReductionEarning.periodType = TermGroup_TimeWorkReductionPeriodType.Week;
                }
            }
            if (changed === 'useTimeWorkReductionWithdrawal' && this.timeAccumulator.useTimeWorkReductionWithdrawal) {
                this.timeAccumulator.useTimeWorkAccount = false;
                this.useTimeWorkReductionEarning = false;
            }
            this.showHideTimeCodes(load);

        }, 100);
    }
    // VALIDATION
    private validateSave(): boolean {
        if (!this.timeAccumulator.employeeGroupRules == null && this.timeAccumulator.employeeGroupRules.find(f => f.employeeGroupId === undefined || f.employeeGroupId == 0))
            return false;
        else
            return true;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.timeAccumulator) {
                if (!this.timeAccumulator.name)
                    mandatoryFieldKeys.push("common.name");
                if (this.timeAccumulator.employeeGroupRules && this.timeAccumulator.employeeGroupRules.find(f => f.employeeGroupId === undefined || f.employeeGroupId == 0))
                    mandatoryFieldKeys.push("time.time.timeaccumulator.employeegroupmissing");

                if (this.showTimeCodes && this.timeAccumulator.timeCodeId === undefined)
                    mandatoryFieldKeys.push("common.timecode");
            }
        });
    }
}