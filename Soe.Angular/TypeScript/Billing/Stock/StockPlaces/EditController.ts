import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IStockService } from "../../../Shared/Billing/Stock/StockService";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data
    stockShelfId: number;
    stockPlace: any;
    stocksDict: any[];

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private stockService: IStockService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {

        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData())
            .onDoLookUp(() => this.onDoLookUp())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.stockShelfId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Billing_Stock_Place, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Stock_Place].readPermission;
        this.modifyPermission = response[Feature.Billing_Stock_Place].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    // LOOKUPS

    private onLoadData(): ng.IPromise<any> {
        if (this.stockShelfId > 0) {
            return this.stockService.getStockPlace(this.stockShelfId).then((x) => {
                this.isNew = false;
                this.stockPlace = x;
            });
        }
        else {
            this.new();
        }
    }
    private onDoLookUp() {
        return this.$q.all([this.loadStocks()]);
    }
    public loadStocks(): ng.IPromise<any> {
        // Load data
        return this.stockService.getStocks(false).then((x) => {
            this.stocksDict = x;
        });
    }

    public save() {
        this.progress.startSaveProgress((completion) => {
            this.stockService.saveStockPlace(this.stockPlace).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.stockShelfId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.stockPlace);
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
            this.stockService.deleteStockPlace(this.stockPlace.stockShelfId).then((result) => {
                if (result.success) {
                    completion.completed(this.stockPlace);
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
        this.stockShelfId = 0;
        this.stockPlace = {};
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.stockPlace) {
                if (!this.stockPlace.code) {
                    mandatoryFieldKeys.push("common.code");
                }
                if (!this.stockPlace.name) {
                    mandatoryFieldKeys.push("common.name");
                }
                if (!this.stockPlace.stockId) {
                    mandatoryFieldKeys.push("billing.stock.stocks.stock");
                }
            }
        });
    }
}