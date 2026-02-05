import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ToolBarButtonGroup } from "../../../Util/ToolBarUtility";
import { ITimeService } from "../TimeService";
import { Feature } from "../../../Util/CommonEnumerations";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Constants } from "../../../Util/Constants";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { SmallGenericType } from "../../../Common/Models/SmallGenericType";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data       
    breakGroup: any;
    timeCodeBreakGroupId: any;
    terms: any = [];

    // ToolBar
    protected gridButtonGroups = new Array<ToolBarButtonGroup>();

    // Lookups

    //@ngInject
    constructor(
        private timeService: ITimeService,
        urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
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
        this.timeCodeBreakGroupId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        this.navigatorRecords = parameters.navigatorRecords;
        this.flowHandler.start([{ feature: Feature.Time_Preferences_TimeSettings_TimeCodeBreakGroup_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
        this.toolbar.setupNavigationRecords(this.navigatorRecords, this.timeCodeBreakGroupId, recordId => {
            if (recordId !== this.timeCodeBreakGroupId) {
                this.timeCodeBreakGroupId = recordId;
                this.onLoadData();
            }
        });
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Time_Preferences_TimeSettings_TimeCodeBreakGroup_Edit].readPermission;
        this.modifyPermission = response[Feature.Time_Preferences_TimeSettings_TimeCodeBreakGroup_Edit].modifyPermission;
    }

    protected doLookups() {
        return this.loadTerms();
    }

    private onLoadData() {
        if (this.timeCodeBreakGroupId > 0) {
            return this.progress.startLoadingProgress([
                () => this.load()
            ]);
        }
        else {
            this.new();
        }
    }

    private loadTerms() {
        var keys: string[] = [
            "time.time.timecodebreakgroup.timecodebreakgroup"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private load(): ng.IPromise<any> {
        return this.timeService.getTimeCodeBreakGroup(this.timeCodeBreakGroupId).then((x) => {
            this.isNew = false;
            this.breakGroup = x;
            this.dirtyHandler.clean();
            this.messagingHandler.publishSetTabLabel(this.guid, this.terms["time.time.timecodebreakgroup.timecodebreakgroup"] + ' ' + this.breakGroup.name);
        });
    }

    // ACTIONS
    private new() {
        this.isNew = true;
        this.timeCodeBreakGroupId = 0;
        this.breakGroup = {
        };
    }

    protected copy() {
        super.copy();
        this.isNew = true;
        this.timeCodeBreakGroupId = 0;
        this.breakGroup.timeCodeBreakGroupId = 0;
    }

    private save() {
        this.progress.startSaveProgress((completion) => {
            this.timeService.saveTimeCodeBreakGroup(this.breakGroup).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0) {
                        if (this.timeCodeBreakGroupId == 0) {
                            if (this.navigatorRecords) {
                                this.navigatorRecords.push(new SmallGenericType(result.integerValue, this.breakGroup.name));
                                this.toolbar.setSelectedRecord(result.integerValue);
                            } else {
                                this.reloadNavigationRecords(result.integerValue);
                            }

                        }
                        this.timeCodeBreakGroupId = result.integerValue;
                        this.breakGroup.timeCodeBreakGroupId = result.integerValue;
                    }
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.breakGroup);
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
            });
    }

    private reloadNavigationRecords(selectedRecord) {
        this.navigatorRecords = [];
        this.timeService.getTimeCodeBreakGroups().then(data => {
            _.forEach(data, (row) => {
                this.navigatorRecords.push(new SmallGenericType(row.timeCodeBreakGroupId, row.name));
            });
            this.toolbar.setupNavigationRecords(this.navigatorRecords, selectedRecord, recordId => {
                if (recordId !== this.timeCodeBreakGroupId) {
                    this.timeCodeBreakGroupId = recordId;
                    this.onLoadData();
                }
            });
            this.toolbar.setSelectedRecord(selectedRecord);
        });
    }

    private delete() {
        if (!this.breakGroup.timeCodeBreakGroupId)
            return;

        this.progress.startDeleteProgress((completion) => {
            this.timeService.deleteTimeCodeBreakGroup(this.breakGroup.timeCodeBreakGroupId).then((result) => {
                if (result.success) {
                    completion.completed(this.breakGroup, true);
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
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.breakGroup) {
                // Mandatory fields
                if (!this.breakGroup.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }


}
