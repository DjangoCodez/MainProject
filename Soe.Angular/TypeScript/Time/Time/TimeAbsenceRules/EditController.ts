import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITimeService } from "../TimeService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { TimeAbsenceRuleHeadDTO } from "../../../Common/Models/TimeAbsenceRuleHeadDTO";
import { Feature, TermGroup, SoeEntityState, TermGroup_TimeAbsenceRuleType, SoeTimeCodeType } from "../../../Util/CommonEnumerations";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";
import { IFocusService } from "../../../Core/Services/focusservice";
import { SmallGenericType } from "../../../Common/Models/smallgenerictype";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Terms
    private terms: { [index: string]: string; };

    // Data
    private timeAbsenceRuleHeadId: number;
    private timeAbsenceRuleHead: TimeAbsenceRuleHeadDTO;
    private types: ISmallGenericType[] = [];
    private timeCodes: ISmallGenericType[] = [];
    private employeeGroups: ISmallGenericType[] = [];

    // Properties
    private selectedEmployeeGroups: ISmallGenericType[] = [];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private timeService: ITimeService,
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
        this.timeAbsenceRuleHeadId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;


        this.flowHandler.start([{ feature: Feature.Time_Preferences_TimeSettings_TimeAbsenceRule_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_TimeSettings_TimeAbsenceRule_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_TimeSettings_TimeAbsenceRule_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => !this.timeAbsenceRuleHeadId);

        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.timeAbsenceRuleHeadId, recordId => {
            if (recordId !== this.timeAbsenceRuleHeadId) {
                this.timeAbsenceRuleHeadId = recordId;
                this.onLoadData();
            }
        });
    }

    private onDoLookups(): ng.IPromise<any> {
        return this.loadTerms().then(() => {
            this.loadTypes();
            this.loadTimeCodes();
            this.loadEmployeeGroups();
        });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.timeAbsenceRuleHeadId) {
            return this.load();
        } else {
            this.new();
        }
    }

    // SERVICE CALLS

    private loadTerms(): ng.IPromise<any> {
        var keys: string[] = [
            "common.all"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.terms = terms;
        });
    }

    private loadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.TimeAbsenceRuleType, false, false).then(x => {
            this.types = x;
        });
    }

    private loadTimeCodes(): ng.IPromise<any> {
        return this.timeService.getTimeCodesDict(SoeTimeCodeType.Absense, false, false).then(x => {
            this.timeCodes = x;
        });
    }

    private loadEmployeeGroups(): ng.IPromise<any> {
        this.employeeGroups = [];
        return this.coreService.getEmployeeGroupsDict(false).then((x) => {
            this.employeeGroups = x;
            this.employeeGroups.splice(0, 0, new SmallGenericType(0, this.terms["common.all"]));
        });
    }

    private load(): ng.IPromise<any> {
        return this.timeService.getTimeAbsenceRule(this.timeAbsenceRuleHeadId).then(x => {
            this.isNew = false;
            this.timeAbsenceRuleHead = x;
            this.selectedEmployeeGroups = _.filter(this.employeeGroups, g => (this.timeAbsenceRuleHead.employeeGroupIds && this.timeAbsenceRuleHead.employeeGroupIds.length > 0 ? _.includes(this.timeAbsenceRuleHead.employeeGroupIds, g.id) : g.id === 0));
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.timeAbsenceRuleHead.name);
        });
    }

    private new() {
        this.isNew = true;
        this.timeAbsenceRuleHeadId = 0;
        this.timeAbsenceRuleHead = new TimeAbsenceRuleHeadDTO();
        this.timeAbsenceRuleHead.timeAbsenceRuleRows = [];
        this.timeAbsenceRuleHead.type = TermGroup_TimeAbsenceRuleType.None;
        this.timeAbsenceRuleHead.state = SoeEntityState.Active;
    }

    // ACTIONS

    protected copy() {
        super.copy();

        this.timeAbsenceRuleHeadId = this.timeAbsenceRuleHead.timeAbsenceRuleHeadId = 0;
        if (this.timeAbsenceRuleHead.timeAbsenceRuleRows) {
            _.forEach(this.timeAbsenceRuleHead.timeAbsenceRuleRows, timeAbsenceRuleRow => {
                timeAbsenceRuleRow.timeAbsenceRuleRowId = 0;
            });
        }
        this.focusService.focusByName("ctrl_timeAbsenceRuleHead_name");
    }

    private initSave() {
        if (this.validateSave())
            this.save();
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.timeAbsenceRuleHead.employeeGroupIds = this.selectedEmployeeGroups.map(e => e.id);
            this.timeService.saveTimeAbsenceRule(this.timeAbsenceRuleHead).then(result => {
                if (result.success) {
                    if (this.timeAbsenceRuleHeadId == 0) {
                        if (this.navigatorRecords) {
                            this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.timeAbsenceRuleHead.name));
                            this.toolbar.setSelectedRecord(result.integerValue);
                        } else {
                            this.reloadNavigationRecords(result.integerValue);
                        }
                    }

                    this.timeAbsenceRuleHeadId = result.integerValue;
                    this.timeAbsenceRuleHead.timeAbsenceRuleHeadId = this.timeAbsenceRuleHeadId;
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.timeAbsenceRuleHead);

                    this.toolbar.setSelectedRecord(this.timeAbsenceRuleHead.timeAbsenceRuleHeadId);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid).then(data => {
            this.onLoadData();
        });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.timeService.getTimeAbsenceRules().then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.timeAbsenceRuleHeadId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.timeAbsenceRuleHeadId) {
                    this.timeAbsenceRuleHeadId = recordId;
                    this.load();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    private delete() {
        this.progress.startDeleteProgress((completion) => {
            this.timeService.deleteTimeAbsenceRule(this.timeAbsenceRuleHead.timeAbsenceRuleHeadId).then(result => {
                if (result.success) {
                    completion.completed(this.timeAbsenceRuleHead, true);
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

    // EVENTS

    private selectedEmployeeGroupsChanged() {
        if (!this.selectedEmployeeGroups)
            return;
        if (this.selectedEmployeeGroups.length === 0)
            this.selectedEmployeeGroups = _.filter(this.employeeGroups, g => g.id === 0);
        else if (_.filter(this.selectedEmployeeGroups, g => g.id === 0).length > 0 && _.filter(this.selectedEmployeeGroups, g => g.id > 0).length > 0)
            this.selectedEmployeeGroups = _.filter(this.selectedEmployeeGroups, g => g.id > 0);
        this.setDirty();
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
            if (this.timeAbsenceRuleHead) {
                if (!this.timeAbsenceRuleHead.name)
                    mandatoryFieldKeys.push("common.name");
                if (!this.timeAbsenceRuleHead.type)
                    mandatoryFieldKeys.push("common.type");
                if (!this.timeAbsenceRuleHead.timeCodeId || this.timeAbsenceRuleHead.timeCodeId < 0)
                    mandatoryFieldKeys.push("common.timecode");
            }
        });
    }
}
