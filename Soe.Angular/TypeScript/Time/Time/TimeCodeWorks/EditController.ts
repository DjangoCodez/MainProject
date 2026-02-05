import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../TimeService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { TimeCodeWorkDTO } from "../../../Common/Models/TimeCode";
import { Feature, SoeTimeCodeType, TermGroup, TermGroup_TimeCodeRuleType, SoeEntityState, TermGroup_TimeCodeClassification } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";
import { IFocusService } from "../../../Core/Services/focusservice";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";
import { IScheduleService } from "../../Schedule/ScheduleService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };
    private minutesLabel: string;

    // Data
    private timeCodeId: number;
    private timeCode: TimeCodeWorkDTO;
    private roundingTypes: ISmallGenericType[] = [];
    private timeCodeWorks: ISmallGenericType[] = [];
    private adjustQuantityByBreakTimeTypes: ISmallGenericType[] = [];
    private adjustQuantityTimeScheduleTypes: ISmallGenericType[] = [];
    private timeCodeRuleTypesQuantity: ISmallGenericType[] = [];
    private classifications: ISmallGenericType[] = [];    
    private classificationInfo: string;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $timeout: ng.ITimeoutService,
        private timeService: ITimeService,
        private scheduleService: IScheduleService,
        private coreService: ICoreService,
        private focusService: IFocusService,
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
        this.flowHandler.start([{ feature: Feature.Time_Preferences_TimeSettings_TimeCodeWork_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_TimeSettings_TimeCodeWork_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_TimeSettings_TimeCodeWork_Edit].modifyPermission;
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

    private roundingValueChange() {
        this.$timeout(() => {
            if (!this.timeCode.roundingValue || this.timeCode.roundingValue === 0)
                this.timeCode.roundingInterruptionTimeCodeId = null;
        });
    }

    private classificationChange() {        
        this.$timeout(() => {
            this.setClassificationInfo();
        });        
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadRoundingTypes(),
            this.loadAdjustQuantityByBreakTimeTypes(),
            this.loadAdjustQuantityTimeCodes(),
            this.loadAdjustQuantityTimeScheduleTypes(),
            this.loadTimeCodeRuleTypesQuantity(),
            this.loadTimeCodeClassification()
        ]);
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.timeCodeId) {
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
            "core.time.minutes",
            "time.time.timecodeworks.timecodework",
            "time.time.timecode.classification.message1",
            "time.time.timecode.classification.message2"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
            this.minutesLabel = this.terms["core.time.minutes"];
            this.setClassificationInfo();
        });
    }

    private loadRoundingTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeCodeRoundingType, true, false).then(x => {
            this.roundingTypes = x;
        });
    }

    private loadAdjustQuantityByBreakTimeTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AdjustQuantityByBreakTime, true, false).then(x => {
            this.adjustQuantityByBreakTimeTypes = x;
        });
    }

    private loadAdjustQuantityTimeCodes(): ng.IPromise<any> {
        this.timeCodeWorks = [];
        this.timeCodeWorks.push(new SmallGenericType(0, ''));
        return this.timeService.getTimeCodes(SoeTimeCodeType.Work, false, false).then((x) => {
            _.forEach(x, (t) => {
                this.timeCodeWorks.push(new SmallGenericType(t.timeCodeId, t.name));
            });
        });
    }

    private loadAdjustQuantityTimeScheduleTypes(): ng.IPromise<any> {
        this.adjustQuantityTimeScheduleTypes = [];
        return this.scheduleService.getTimeScheduleTypesDict(false, true).then(x => {
            this.adjustQuantityTimeScheduleTypes = x;
        });
    }

    private loadTimeCodeRuleTypesQuantity(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeCodeRuleType, true, false).then(x => {
            this.timeCodeRuleTypesQuantity = [];
            _.forEach(x, y => {
                if (y.id === TermGroup_TimeCodeRuleType.Unknown ||
                    y.id === TermGroup_TimeCodeRuleType.AdjustQuantityOnTime ||
                    y.id === TermGroup_TimeCodeRuleType.AdjustQuantityOnScheduleInNextDay)
                    this.timeCodeRuleTypesQuantity.push(y);
            });
        });
    }

    private loadTimeCodeClassification(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeCodeClassification, false, false).then(x => {
            this.classifications = x;            
        });
    }

    private load(): ng.IPromise<any> {
        return this.timeService.getTimeCode(SoeTimeCodeType.Work, this.timeCodeId, true, true).then(x => {
            this.isNew = false;
            this.timeCode = x;
            this.dirtyHandler.clean();
            this.setClassificationInfo();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.time.timecodeworks.timecodework"] + ' ' + this.timeCode.name);
        });
    }

    private new() {
        this.isNew = true;
        this.timeCodeId = 0;
        this.timeCode = new TimeCodeWorkDTO();
        this.timeCode.type = SoeTimeCodeType.Work;
        this.timeCode.state = SoeEntityState.Active;
        this.timeCode.classification = TermGroup_TimeCodeClassification.None;
    }

    // ACTIONS

    protected copy() {
        super.copy();

        this.timeCodeId = 0;
        this.timeCode.timeCodeId = 0;
        //this.timeCode.code = undefined;
        //this.timeCode.name = undefined;
        _.forEach(this.timeCode.payrollProducts, p => {
            p.timeCodePayrollProductId = 0;
            p.timeCodeId = 0;
        });
        _.forEach(this.timeCode.invoiceProducts, p => {
            p.timeCodeInvoiceProductId = 0;
            p.timeCodeId = 0;
        });

        this.focusService.focusByName("ctrl_timeCode_code");
    }

    private initSave() {
        if (this.validateSave())
            this.save();
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.timeService.saveTimeCode(this.timeCode).then(result => {                
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.timeCodeId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.timeCode.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                    this.timeCodeId = result.integerValue;
                    this.timeCode.timeCodeId = this.timeCodeId;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.timeCode);
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
        this.timeService.getTimeCodesGrid(SoeTimeCodeType.Work, false, true).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.timeCodeId, row.name));
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

    private setClassificationInfo() {
        this.classificationInfo = this.terms["time.time.timecode.classification.message1"];
        if (this.timeCode) {
            if (this.timeCode.classification !== TermGroup_TimeCodeClassification.None) {
                this.classificationInfo += this.terms["time.time.timecode.classification.message2"];
            }
        }
    }

    private setDirty() {
        this.dirtyHandler.setDirty();
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