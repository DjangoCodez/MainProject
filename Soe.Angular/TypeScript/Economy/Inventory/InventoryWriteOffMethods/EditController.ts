import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { InventoryService } from "../../../Shared/Economy/Inventory/InventoryService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature, TermGroup, TermGroup_InventoryWriteOffMethodType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    inventoryWriteOffMethod: any;
    inventoryWriteOffMethodId: number

    // Lookups     
    periodTypes: any;
    writeOffTypes: any;

    //@ngInject  
    constructor(
        private $q: ng.IQService,
        private inventoryService: InventoryService,
        private coreService: ICoreService,
        translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        private controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData()) //this.doLookups())
            .onDoLookUp(() => this.onDoLookups()) //this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.inventoryWriteOffMethodId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Economy_Inventory_WriteOffMethods_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Economy_Inventory_WriteOffMethods_Edit].readPermission;
        this.modifyPermission = response[Feature.Economy_Inventory_WriteOffMethods_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false, null, () => this.isNew);
    }

    // SETUP

    private onDoLookups() {
        return this.$q.all([this.loadPeriodTypes(), this.loadWriteOffTypes()]);
    }

    // LOOKUPS

    private onLoadData(): ng.IPromise<any> {
        if (this.inventoryWriteOffMethodId > 0) {
            return this.inventoryService.getInventoryWriteOffMethod(this.inventoryWriteOffMethodId).then((x) => {
                this.isNew = false;
                this.inventoryWriteOffMethod = x;
            });
        }
        else {
            this.new();
        }
    }

    private loadPeriodTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InventoryWriteOffMethodPeriodType, false, false).then((x) => {
            this.periodTypes = x;
        });
    }

    private loadWriteOffTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InventoryWriteOffMethodType, false, false).then((x) => {
            this.writeOffTypes = x;
        });
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.inventoryService.saveInventoryWriteOffMethod(this.inventoryWriteOffMethod).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.inventoryWriteOffMethodId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.inventoryWriteOffMethod);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                this.dirtyHandler.clean();
                this.onLoadData();
            }, error => {

            });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.inventoryService.deleteInventoryWriteOffMethod(this.inventoryWriteOffMethod.inventoryWriteOffMethodId).then((result) => {
                if (result.success) {
                    completion.completed(this.inventoryWriteOffMethod);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            super.closeMe(false);
        });
    }

    // HELP-METHODS
    private new() {
        this.isNew = true;
        this.inventoryWriteOffMethodId = 0;
        this.inventoryWriteOffMethod = {};
        this.inventoryWriteOffMethod.yearPercent = 30;
        this.inventoryWriteOffMethod.type = TermGroup_InventoryWriteOffMethodType.Immediate;
    }

    // VALIDATION

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.inventoryWriteOffMethod) {
                if (!this.inventoryWriteOffMethod.name) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (!this.inventoryWriteOffMethod.description) {
                    mandatoryFieldKeys.push("common.description");
                }
            }
        });
    }
}