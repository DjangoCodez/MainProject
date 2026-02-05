import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { Feature, TermGroup, SoeReportTemplateType, SoeEntityState } from "../../../Util/CommonEnumerations";
import { IRegistryService } from "../RegistryService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { Constants } from "../../../Util/Constants";
import { ChecklistHeadDTO } from "../../../Common/Models/ChecklistHeadDTO"
import { ChecklistRowDTO } from "../../../Common/Models/ChecklistRowDTO"
import { ChecklistRowsDirectiveController } from "./Directives/ChecklistRowsDirective";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private checklistHeadId: number;
    private checklistHead: ChecklistHeadDTO;
    checklistRows: ChecklistRowDTO[] = [];
    private checklistHeadTypes: any[];
    private checklistRowTypes: any[];
    private checklistReports: any[];
    private active = true;
    private yesNoDict: any[] = []; 

    //@ngInject
    constructor(
        private readonly $q: ng.IQService,
        private readonly translationService: ITranslationService,
        urlHelperService: IUrlHelperService,        
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,        
        messagingHandlerFactory: IMessagingHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        private readonly dirtyHandlerFactory: IDirtyHandlerFactory,
        private readonly registryService: IRegistryService,
        private checklistRowsDirective: ChecklistRowsDirectiveController,
        private readonly coreService: ICoreService
    ) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData()) 
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.checklistHeadId = parameters.id;
        this.guid = parameters.guid;
        
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);        

        this.flowHandler.start([{ feature: Feature.Manage_Preferences_Registry_Checklists_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, () => this.copy(), () => this.isNew);
    }


    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Preferences_Registry_Checklists_Edit].readPermission;
        this.modifyPermission = response[Feature.Manage_Preferences_Registry_Checklists_Edit].modifyPermission;
    }

    // LOOKUPS
    private onDoLookups() {
        return this.progress.startLoadingProgress([
            () => this.loadChecklistHeadTypes(),
            () => this.loadChecklistReports(),   
            () => this.loadChecklistRowAnswerTypes(),
            () => this.loadYesNoDict(),
        ]);
    }

    private loadChecklistHeadTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChecklistHeadType, false, false).then(x => {
            this.checklistHeadTypes = x;            
        });
    }

    private loadChecklistRowAnswerTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.ChecklistRowType, false, false).then(x => {
            this.checklistRowTypes = x;
        });
    }

    private loadChecklistReports(): ng.IPromise<any> {
        return this.registryService.getChecklistReports(SoeReportTemplateType.OrderChecklistReport, false, false, false, false).then((x) => {
            this.checklistReports = x;            
        })
    }    

    private loadYesNoDict(): ng.IPromise<any> {
        const keys: string[] = [
            "core.yes",
            "core.no",            
        ];

        return this.translationService.translateMany(keys).then((terms) => {            
            this.yesNoDict.push({ id: 1, name: terms["core.yes"] })
            this.yesNoDict.push({ id: 2, name: terms["core.no"] })
        });
    }
      
    private onLoadData(): ng.IPromise<any> {
        if (this.checklistHeadId > 0) {
            return this.registryService.getChecklistHead(this.checklistHeadId, true).then((x) => {
                this.isNew = false;
                this.checklistHead = x;
                this.checklistRows = _.orderBy(this.checklistHead.checklistRows, 'rowNr');
                this.active = (this.checklistHead.state == SoeEntityState.Active);

                _.forEach(this.checklistRows, (row) => {                    
                    row["mandatoryName"] = row.mandatory ? this.yesNoDict[0].name : this.yesNoDict[1].name;
                    const type = _.find(this.checklistRowTypes, { id: row["type"] })
                    row["typeName"] = type.name;
                })
            });
        }
        else {
            this.new();
        }
    }

    public save() {
        this.progress.startSaveProgress((completion) => {

            this.checklistHead.state = (this.active) ? SoeEntityState.Active : SoeEntityState.Inactive;
            this.checklistHead.checklistRows = this.checklistRows;

            this.registryService.saveChecklistHead(this.checklistHead).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.checklistHeadId = result.integerValue;
                    this.onLoadData();
                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.checklistHead);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                
            }, error => {

            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.registryService.deleteChecklistHead(this.checklistHeadId).then((result) => {
                if (result.success) {
                    completion.completed(this.checklistHead);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.closeMe(true);
        });
    }

    public closeMe(reloadGrid: boolean) {
        // Send messages to TabsController
        this.messagingHandler.publishCloseTab(this.guid);
        if (reloadGrid) {
            this.messagingHandler.publishReloadGrid(this.guid);
        }
    }

    private setModified() {
        this.dirtyHandler.setDirty();
    }

    // HELP-METHODS
    private new() {
        this.isNew = true;
        this.checklistHeadId = 0;
        this.checklistHead = new ChecklistHeadDTO();        
    }

    protected copy() {
        super.copy();

        this.checklistHeadId = 0;
        this.checklistHead.checklistHeadId = 0;
        this.isNew = true;

        _.forEach(this.checklistHead.checklistRows, (row) => {
            row.checklistHeadId = 0;
            row.checklistRowId = 0;
            row.isModified = true;
        });

        this.dirtyHandler.isDirty = true;
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.checklistHead) {
                if (!this.checklistHead.name) {
                    mandatoryFieldKeys.push("manage.registry.checklists.name");
                }
                if (!this.checklistHead.type) {
                    mandatoryFieldKeys.push("manage.registry.checklists.type");
                }
                
            }
        });
    }
}