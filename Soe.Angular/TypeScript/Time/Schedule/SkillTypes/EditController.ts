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
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { Constants } from "../../../Util/Constants";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    skillType: any;
    skillTypeId: number;
    skillTypes: any = [];
    terms: any = [];

    //@ngInject
    constructor(
        private $q: ng.IQService,
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
        this.skillTypeId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_ScheduleSettings_SkillType_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true,() => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.skillTypeId, recordId => {
            if (recordId !== this.skillTypeId) {
                this.skillTypeId = recordId;
                this.onLoadData();
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_ScheduleSettings_SkillType_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_ScheduleSettings_SkillType_Edit].modifyPermission;
    }

    private onLoadData() {
        if (this.skillTypeId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        } else {
            this.new();
        }
    }

    protected doLookups() {
        return this.loadTerms().then(() => {
            return this.$q.all([
                this.loadTypes()
            ]);
        });
    }

    private loadTypes(): ng.IPromise<any> {
        return this.scheduleService.getSkillTypesDict(false).then((x) => {
            this.skillType = x;
        });
    }

    private load(): ng.IPromise<any> {
        return this.scheduleService.getSkillType(this.skillTypeId).then((x) => {
            this.isNew = false;
            this.skillType = x;

            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.schedule.skilltype.skilltype"] + ' ' + this.skillType.name);

            
        });
    }

    private loadTerms() {
        var keys: string[] = [
            "time.schedule.skilltype.skilltype"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    // HELP-METHODS
    protected copy() {
        super.copy();
        this.isNew = true;
        this.skillTypeId = 0;
        if (this.skillType) {
            this.skillType.skillTypeId = 0;
        }
        
    }

    private new() {
        this.isNew = true;
        this.skillTypeId = 0;
        this.skillType = {};
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.scheduleService.saveSkillType(this.skillType).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.skillTypeId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.skillType.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }
                        }
                        this.skillTypeId = result.integerValue;
                        this.skillType.skillTypeId = result.integerValue;                     
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.skillType);
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
        this.scheduleService.getSkillTypes().then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.skillTypeId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.skillTypeId) {
                    this.skillTypeId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }


    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.scheduleService.deleteSkillType(this.skillType.skillTypeId).then((result) => {
                if (result.success) {
                    completion.completed(this.skillType, true);
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

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.skillType) {
                // Mandatory fields
                if (!this.skillType.name)
                    mandatoryFieldKeys.push("common.name");
                if (!this.skillType.skillTypeId)
                    mandatoryFieldKeys.push("common.type");
            }
        });
    }
}
