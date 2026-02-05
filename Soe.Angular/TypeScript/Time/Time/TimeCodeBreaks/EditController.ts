import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../TimeService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { TimeCodeBreakDTO, TimeCodeRuleDTO, TimeCodeBreakGroupGridDTO } from "../../../Common/Models/TimeCode";
import { Feature, SoeTimeCodeType, TermGroup, SoeEntityState, TermGroup_TimeCodeRuleType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";
import { IFocusService } from "../../../Core/Services/focusservice";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };
    private minutesLabel: string;
    private timeLabel: string;

    // Permissions
    private breakGroupPermission: boolean = false;

    // Data
    private timeCodeId: number;
    private timeCode: TimeCodeBreakDTO;
    private startStopTypes: ISmallGenericType[] = [];
    private timeCodes: ISmallGenericType[] = [];
    private breakGroups: TimeCodeBreakGroupGridDTO[] = [];

    // Properties
    private _timeCodeEarlierThanStart: number;
    private get timeCodeEarlierThanStart(): number {
        return this._timeCodeEarlierThanStart;
    }
    private set timeCodeEarlierThanStart(timeCodeId: number) {
        this._timeCodeEarlierThanStart = timeCodeId;
        this.setTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeEarlierThanStart, timeCodeId);
    }

    private _timeCodeLaterThanStop: number;
    private get timeCodeLaterThanStop(): number {
        return this._timeCodeLaterThanStop;
    }
    private set timeCodeLaterThanStop(timeCodeId: number) {
        this._timeCodeLaterThanStop = timeCodeId;
        this.setTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeLaterThanStop, timeCodeId);
    }

    private _timeCodeMoreThanMax: number;
    private get timeCodeMoreThanMax(): number {
        return this._timeCodeMoreThanMax;
    }
    private set timeCodeMoreThanMax(timeCodeId: number) {
        this._timeCodeMoreThanMax = timeCodeId;
        this.setTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeMoreThanMax, timeCodeId);
    }

    private _timeCodeBetweenStdAndMax: number;
    private get timeCodeBetweenStdAndMax(): number {
        return this._timeCodeBetweenStdAndMax;
    }
    private set timeCodeBetweenStdAndMax(timeCodeId: number) {
        this._timeCodeBetweenStdAndMax = timeCodeId;
        this.setTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeBetweenStdAndMax, timeCodeId);
    }

    private _timeCodeStd: number;
    private get timeCodeStd(): number {
        return this._timeCodeStd;
    }
    private set timeCodeStd(timeCodeId: number) {
        this._timeCodeStd = timeCodeId;
        this.setTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeStd, timeCodeId);
    }

    private _timeCodeBetweenMinAndStd: number;
    private get timeCodeBetweenMinAndStd(): number {
        return this._timeCodeBetweenMinAndStd;
    }
    private set timeCodeBetweenMinAndStd(timeCodeId: number) {
        this._timeCodeBetweenMinAndStd = timeCodeId;
        this.setTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeBetweenMinAndStd, timeCodeId);
    }

    private _timeCodeLessThanMin: number;
    private get timeCodeLessThanMin(): number {
        return this._timeCodeLessThanMin;
    }
    private set timeCodeLessThanMin(timeCodeId: number) {
        this._timeCodeLessThanMin = timeCodeId;
        this.setTimeCodeRule(TermGroup_TimeCodeRuleType.TimeCodeLessThanMin, timeCodeId);
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private timeService: ITimeService,
        private coreService: ICoreService,
        private focusService:IFocusService,
        private translationService: ITranslationService,
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
        this.timeCodeId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_TimeSettings_TimeCodeBreak_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_TimeSettings_TimeCodeBreak_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_TimeSettings_TimeCodeBreak_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => !this.timeCodeId);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.timeCodeId, recordId => {
            if (recordId !== this.timeCodeId) {
                this.timeCodeId = recordId;
                this.onLoadData();
            }
        });
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadReadOnlyPermissions(),
            this.loadTerms(),
            this.loadStartStopTypes(),
            this.loadTimeCodes(),
            this.loadBreakGroups()
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.timeCodeId) {
            return this.load();
        } else {
            this.new();
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "core.time.minutes",
            "common.datetime.short",
            "time.time.timecodebreaks.timecodebreak"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.minutesLabel = this.terms["core.time.minutes"];
            this.timeLabel = this.terms["common.datetime.short"];
        });
    }

    private loadReadOnlyPermissions(): ng.IPromise<any> {
        var features: number[] = [];

        features.push(Feature.Time_Preferences_TimeSettings_TimeCodeBreakGroup);

        return this.coreService.hasReadOnlyPermissions(features).then((x) => {
            this.breakGroupPermission = x[Feature.Time_Preferences_TimeSettings_TimeCodeBreakGroup];
        });
    }

    private loadStartStopTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeCodeBreakStartStopTypes, true, false).then(x => {
            this.startStopTypes = x;
        });
    }

    private loadTimeCodes(): ng.IPromise<any> {
        return this.timeService.getTimeCodesDict(SoeTimeCodeType.WorkAndAbsense, true, false).then(x => {
            this.timeCodes = x;
        });
    }

    private loadBreakGroups(): ng.IPromise<any> {
        return this.timeService.getTimeCodeBreakGroups().then(x => {
            var empty = new TimeCodeBreakGroupGridDTO();
            empty.timeCodeBreakGroupId = 0;
            empty.name = ' ';
            this.breakGroups.push(empty);
            _.forEach(x, (b) => {
                this.breakGroups.push(b);
            });
        });
    }

    private load(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([() => {
        return this.timeService.getTimeCode(SoeTimeCodeType.Break, this.timeCodeId, false, false, true, true).then(x => {
            this.isNew = false;
            this.timeCode = x;

            this.timeCodeEarlierThanStart = this.getTimeCodeRuleId(TermGroup_TimeCodeRuleType.TimeCodeEarlierThanStart);
            this.timeCodeLaterThanStop = this.getTimeCodeRuleId(TermGroup_TimeCodeRuleType.TimeCodeLaterThanStop);
            this.timeCodeMoreThanMax = this.getTimeCodeRuleId(TermGroup_TimeCodeRuleType.TimeCodeMoreThanMax);
            this.timeCodeBetweenStdAndMax = this.getTimeCodeRuleId(TermGroup_TimeCodeRuleType.TimeCodeBetweenStdAndMax);
            this.timeCodeStd = this.getTimeCodeRuleId(TermGroup_TimeCodeRuleType.TimeCodeStd);
            this.timeCodeBetweenMinAndStd = this.getTimeCodeRuleId(TermGroup_TimeCodeRuleType.TimeCodeBetweenMinAndStd);
            this.timeCodeLessThanMin = this.getTimeCodeRuleId(TermGroup_TimeCodeRuleType.TimeCodeLessThanMin);

            this.dirtyHandler.clean();
                this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.time.timecodebreaks.timecodebreak"] + ' ' + this.timeCode.name);
        });
        }]);
    }

    private new() {
        this.isNew = true;
        this.timeCodeId = 0;
        this.timeCode = new TimeCodeBreakDTO();
        this.timeCode.type = SoeTimeCodeType.Break;
        this.timeCode.state = SoeEntityState.Active;
    }

    // ACTIONS

    protected copy() {
        super.copy();

        this.timeCodeId = this.timeCode.timeCodeId = 0;
        
        _.forEach(this.timeCode.timeCodeDeviationCauses, c => {
            c.timeCodeBreakTimeCodeDeviationCauseId = 0;
        });
        this.focusService.focusByName("ctrl_timeCode_code");
    }

    private initSave() {
        if (this.validateSave())
            this.save();
    }

    private prepareToSave() {
        this.timeCode.timeCodeRules = [];
        if (this.timeCodeEarlierThanStart)
            this.timeCode.timeCodeRules.push(new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.TimeCodeEarlierThanStart, this.timeCodeEarlierThanStart));
        if (this.timeCodeLaterThanStop)
            this.timeCode.timeCodeRules.push(new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.TimeCodeLaterThanStop, this.timeCodeLaterThanStop));

        if (this.timeCodeLessThanMin)
            this.timeCode.timeCodeRules.push(new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.TimeCodeLessThanMin, this.timeCodeLessThanMin));
        if (this.timeCodeBetweenMinAndStd)
            this.timeCode.timeCodeRules.push(new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.TimeCodeBetweenMinAndStd, this.timeCodeBetweenMinAndStd));
        if (this.timeCodeStd)
            this.timeCode.timeCodeRules.push(new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.TimeCodeStd, this.timeCodeStd));
        if (this.timeCodeBetweenStdAndMax)
            this.timeCode.timeCodeRules.push(new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.TimeCodeBetweenStdAndMax, this.timeCodeBetweenStdAndMax));
        if (this.timeCodeMoreThanMax)
            this.timeCode.timeCodeRules.push(new TimeCodeRuleDTO(TermGroup_TimeCodeRuleType.TimeCodeMoreThanMax, this.timeCodeMoreThanMax));

        if (this.timeCode.timeCodeBreakGroupId === 0)
            this.timeCode.timeCodeBreakGroupId = null;
    }

    private save() {
        this.prepareToSave();
        this.progress.startSaveProgress((completion) => {
            this.timeService.saveTimeCode(this.timeCode).then(result => {
                if (result.success) {
                    if (this.timeCodeId == 0) {
                        if (this.navigatorRecords) {
                            this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.timeCode.name));
                            this.toolbar.setSelectedRecord(result.integerValue);
                        } else {
                            this.reloadNavigationRecords(result.integerValue);
                        }

                    }                    
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.timeCode);
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
        this.timeService.getTimeCodesGrid(SoeTimeCodeType.Break, false, true).then(data => {
            _.forEach(data, (row) => {
                if (row.isActive) {
                    this.navigatorRecords.push(new SmallGenericType(row.timeCodeId, row.name));
                }
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.timeCodeId) {
                    this.timeCodeId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.timeService.deleteTimeCode(this.timeCode.timeCodeId).then(result => {
                if (result.success) {
                    completion.completed(this.timeCode, true);
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

    private getTimeCodeRule(type: TermGroup_TimeCodeRuleType): TimeCodeRuleDTO {
        if (!this.timeCode || !this.timeCode.timeCodeRules)
            return null;

        return _.find(this.timeCode.timeCodeRules, r => r.type == type);
    }

    private getTimeCodeRuleId(type: TermGroup_TimeCodeRuleType): number {
        var rule = this.getTimeCodeRule(type);
        return rule ? rule.value : 0;
    }

    private setTimeCodeRule(type: TermGroup_TimeCodeRuleType, timeCodeId: number) {
        if (!this.timeCode)
            return;

        var rule = this.getTimeCodeRule(type);
        if (rule)
            rule.value = timeCodeId;
        else {
            let newRule = new TimeCodeRuleDTO(type, timeCodeId);
            if (!this.timeCode.timeCodeRules)
                this.timeCode.timeCodeRules = [];
            this.timeCode.timeCodeRules.push(newRule);
        }
    }


    // VALIDATION

    private validateSave(): boolean {
        return true;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.timeCode) {
                if (!this.timeCode.code)
                    mandatoryFieldKeys.push("common.code");

                if (!this.timeCode.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}