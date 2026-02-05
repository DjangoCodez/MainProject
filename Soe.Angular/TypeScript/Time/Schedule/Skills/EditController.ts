import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IScheduleService } from "../ScheduleService";
import { Feature } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Constants } from "../../../Util/Constants";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    skill: any;
    skillTypes: any = [];
    private skillId: number;
    terms: any = [];

    //@ngInject
    constructor(
        private scheduleService: IScheduleService,
        urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.doLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    // SETUP
    public onInit(parameters: any) {
        this.skillId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_ScheduleSettings_Skill_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.skillId, recordId => {
            if (recordId !== this.skillId) {
                this.skillId = recordId;
                this.onLoadData();
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_ScheduleSettings_Skill_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_ScheduleSettings_Skill_Edit].modifyPermission;
    }

    private load(): ng.IPromise<any> {
        return this.scheduleService.getSkill(this.skillId).then((x) => {
            this.isNew = false;
            this.skill = x;
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.schedule.skill.skill"] + ' ' + this.skill.name);
            
        });
    }

    private onLoadData() {
        if (this.skillId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    }

    private loadTypes(): ng.IPromise<any> {
        return this.scheduleService.getSkillTypesDict(false).then((x) => {
            this.skillTypes = x;
        });
    }

    protected doLookups() {
        return this.loadTerms().then(() => {
            return this.loadTypes();
        });
    }

    private loadTerms() {
        var keys: string[] = [
            "time.schedule.skill.skill"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveSkill(this.skill).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.skillId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.skill.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                            
                        }
                        this.skillId = result.integerValue;
                        this.skill.skillId = result.integerValue;
                        
                        completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.skill);
                    }
                    
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
            }, error => {

            });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
       this.scheduleService.getSkills(false).then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.skillId, row.name));
            });            
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {                    
               if (recordId !== this.skillId) {
                   this.skillId = recordId;
                   this.onLoadData();
               }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteSkill(this.skill.skillId).then((result) => {
                if (result.success) {
                    completion.completed(this.skill, true);
                } else {
                    completion.failed('');
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(false);
        });
    }

    // HELP-METHODS

    protected copy() {
        super.copy();
        this.isNew = true;
        this.skillId = 0;
        this.skill.skillId = 0;
    }

    private new() {
        this.isNew = true;
        this.skillId = 0;
        this.skill = {};
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.skill) {
                // Mandatory fields
                if (!this.skill.name)
                    mandatoryFieldKeys.push("common.name");
                if (!this.skill.skillTypeId)
                    mandatoryFieldKeys.push("common.type");
            }
        });
    }
}
