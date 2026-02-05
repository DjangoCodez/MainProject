import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { CompTermsRecordType, Feature } from "../../../Util/CommonEnumerations";
import { lang } from "moment";
import { IProductService } from "../../../Shared/Billing/Products/ProductService";
import { Constants } from "../../../Util/Constants";
import { ProductUnitDTO } from "../../../Common/Models/ProductUnitDTO";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    private productUnitId: number;
    private productUnit: ProductUnitDTO;
    private compTermRecordType = CompTermsRecordType.ProductUnitName;
    private compTermRows: any[];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private productService: IProductService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);
        
        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onLoadData(() => this.loadData());
        
    }

    public onInit(parameters: any) {
        this.productUnitId = parameters.id || 0;
        this.guid = parameters.guid;

        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Billing_Product_Products_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Product_Products_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Product_Products_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false);
    }

    private loadData(): ng.IPromise<any> {
        const deferral = this.$q.defer();

        if (this.productUnitId > 0) {
            this.productService.getProductUnit(this.productUnitId).then((x) => {
                this.productUnit = x;
                this.isNew = false;
                deferral.resolve();
            });
        }
        else {
            this.new();
            deferral.resolve();
        }

        return deferral.promise;
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.productService.saveProductUnit(this.productUnit, this.compTermRows).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.productUnitId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.productUnit);
                } else {
                    completion.failed(result.errorMessage);
                }
            })
        }, this.guid).then(data => {
            this.dirtyHandler.clean();

            this.loadData();
        }, error => {

        });
    }

    public delete() {
        this.progress.startDeleteProgress((completion) => {
            this.productService.deleteProductUnit(this.productUnitId).then((result) => {
                if (result.success) {
                    completion.completed(this.productUnit);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        });
    }

    private new() {
        this.productUnit = new ProductUnitDTO();
        this.isNew = true;
    }

    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.productUnit) {
                if (!this.productUnit.code)
                    mandatoryFieldKeys.push("common.code");
                if (!this.productUnit.name)
                    mandatoryFieldKeys.push("common.name");
            }
        });
    }
}