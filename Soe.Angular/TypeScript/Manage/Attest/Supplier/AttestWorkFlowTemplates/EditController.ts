import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { Constants } from "../../../../Util/Constants";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IAttestService } from "../../AttestService";
import { IAttestWorkFlowTemplateHeadDTO } from "../../../../Scripts/TypeLite.Net4";
import { Feature, TermGroup, TermGroup_AttestEntity } from "../../../../Util/CommonEnumerations";
import { AttestWorkFlowTemplateRowDirectiveController } from "./Directives/AttestWorkFlowTemplateRowDirective";
import { AttestWorkFlowTemplateRowDTO } from "../../../../Common/Models/AttestWorkFlowDTOs";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data       
    attestTemplate: IAttestWorkFlowTemplateHeadDTO;
    templateTypes: any;
    attestWorkFlowTemplateRows: AttestWorkFlowTemplateRowDTO[] = [];
    attestTemplateId: number = 0;

    // ToolBar
    //protected gridButtonGroups = new Array<ToolBarButtonGroup>();    
    public parameters: any;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,        
        private coreService: ICoreService,
        private attestService: IAttestService,
        private attestWorkFlowTemplateRowDirective: AttestWorkFlowTemplateRowDirectiveController
        ) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            // .onLoadData(() => this.loadDefaultChecklist())
            .onLoadData(() => this.onLoadData())           
    }

    public onInit(parameters: any) {
        this.attestTemplateId = parameters.id;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Manage_Attest_Supplier_WorkFlowTemplate_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }


    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Manage_Attest_Supplier_WorkFlowTemplate_Edit].readPermission;
        this.modifyPermission = response[Feature.Manage_Attest_Supplier_WorkFlowTemplate_Edit].modifyPermission;
    }

    // LOOKUPS
    private onDoLookups() {
        return this.progress.startLoadingProgress([            
            () => this.loadTemplateTypes(),            
        ]);
    }   

    
    private setAttestTemplateId(id: number) {
        this.attestTemplateId = id;
        this.attestWorkFlowTemplateRows.forEach(row => row.attestWorkFlowTemplateHeadId = id);
    }

    // SERVICE CALLS

    private loadTemplateTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.AttestWorkFlowType, false, false).then(templateTypes => {
            this.templateTypes = templateTypes;
        });
    }

    private onLoadData(): ng.IPromise<any> {
        var deferral = this.$q.defer();
        if (this.attestTemplateId > 0) {
            this.attestService.getAttestWorkFlowTemplate(this.attestTemplateId)
                .then((x) => {
                    this.attestTemplate = x;
                    this.isNew = false;
                    deferral.resolve();
                });
        } else {
            this.new();
            deferral.resolve();
        }

        return deferral.promise;
    }

    // ACTIONS

    private save() {

        this.progress.startSaveProgress((completion) => {            

            this.attestService.saveAttestWorkFlowTemplate(this.attestTemplate).then((result) => {
                if (result.success) {
                    if (!this.attestTemplateId) {
                        this.setAttestTemplateId(result.integerValue);
                    }

                    var rows = this.attestWorkFlowTemplateRows.filter(item => item.checked)
                        .map((x, index) => {
                            x.sort = index + 1;
                            return x;
                        });
                    //if (rows.length)                         
                    this.attestService.saveAttestWorkFlowTemplateRows(rows, this.attestTemplateId).then((resultRows) => {
                        if (!resultRows.success)
                            completion.failed(resultRows.errorMessage);
                        else
                            completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.attestTemplate);
                    });
                                        
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

    protected delete() {
        this.progress.startDeleteProgress((completion) => {
            this.attestService.deleteAttestWorkFlowTemplate(this.attestTemplateId).then((result) => {
                if (result.success) {
                    completion.completed(this.attestTemplate);
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

    // HELP-METHODS   
    private new() {
        this.isNew = true;
        this.attestTemplateId = 0;
        this.attestTemplate = ({} as IAttestWorkFlowTemplateHeadDTO);
        this.attestTemplate.attestEntity = TermGroup_AttestEntity.SupplierInvoice
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.attestTemplate) {
                if (!this.attestTemplate.name) {
                    mandatoryFieldKeys.push("common.name");
                }                
            }
        });
    }
   
}
