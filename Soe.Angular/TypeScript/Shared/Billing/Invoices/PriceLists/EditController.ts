import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { InvoiceService } from "../../../../Shared/Billing/Invoices/InvoiceService";
import { Feature, ActionResultSave } from "../../../../Util/CommonEnumerations";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IValidationSummaryHandlerFactory } from "../../../../Core/Handlers/validationsummaryhandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IDirtyHandlerFactory } from "../../../../Core/Handlers/DirtyHandlerFactory";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { ICompositionEditController } from "../../../../Core/ICompositionEditController";
import { EditControllerBase2 } from "../../../../Core/Controllers/EditControllerBase2";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { Constants } from "../../../../Util/Constants";
import { Guid } from "../../../../Util/StringUtility";
import { IFocusService } from "../../../../Core/Services/focusservice";

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Modal
    modal: any;
    isModal: boolean = false;

    // Data
    priceListId: number;
    priceList: any;

    incVat: boolean;
    currencies: any[];
    baseCurrency: number;

    // Modal title
    modalTitle: string;

    //@ngInject
    constructor(
        private $q,
        private coreService: ICoreService,
        private invoiceService: InvoiceService,
        private translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory,
        private focusService: IFocusService,
        private $scope: ng.IScope) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onLoadData(() => this.onLoadData()) //this.doLookups())
            .onDoLookUp(() => this.onDoLookUps()) //this.doLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));

        //Event from pages where controller is opened as dialog.
        $scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {

            if (parameters && parameters.sourceGuid === this.guid) {
                return;
            }

            parameters.guid = Guid.newGuid();
            this.isModal = true;
            this.onInit(parameters);
            this.modal = parameters.modal;
            this.modalTitle = parameters.title;
            this.focusService.focusByName("ctrl_priceList_name");
        });
    }

    public onInit(parameters: any) {
        this.priceListId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);
        
        this.flowHandler.start([{ feature: Feature.Billing_Preferences_InvoiceSettings_Pricelists_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Preferences_InvoiceSettings_Pricelists_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Preferences_InvoiceSettings_Pricelists_Edit].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(false, null, () => this.isNew);
    }

    // SETUP

    private onDoLookUps() {
        return this.$q.all([
            this.loadCurrencies()
        ]);
    }

    private loadCurrencies(): ng.IPromise<any> {
        return this.coreService.getCompCurrenciesSmall().then(x => {
            this.currencies = x;
        });
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.priceListId > 0) {
            return this.invoiceService.getPriceList(this.priceListId).then((x) => {
                this.isNew = false;
                this.priceList = x;
            });
        }
        else {
            this.new();
        }
    }

    private save() {

        this.progress.startSaveProgress((completion) => {
            this.invoiceService.savePriceList(this.priceList).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.priceListId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.priceList);
                } else {
                    completion.failed(result.errorMessage);
                }
            }, error => {
                completion.failed(error.message);
            });
        }, this.guid)
            .then(data => {
                if (this.isModal) {
                    this.closeModal();
                }
                else {
                    this.dirtyHandler.clean();
                    this.onLoadData();
                }
            }, error => {

            });
    }

    protected delete() {

        this.progress.startDeleteProgress((completion) => {
            this.invoiceService.deletePriceList(this.priceList.priceListTypeId).then((result) => {
                if (result.success) {
                    completion.completed(this.priceList);
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
        this.priceListId = 0;
        this.priceList = {};
        this.priceList.currencyId = this.currencies && this.currencies.length > 0 ? this.currencies[0].currencyId : 0;
        if (this.isModal) {
            this.priceList.isProjectPriceList = true;
        }
    }

    public closeModal() {
        if (this.isModal) {
            if (this.priceListId) {
                this.modal.close({ id: this.priceListId });
            } else {
                this.modal.dismiss();
            }
        }
    }

    // VALIDATION

    protected validate() {
        if (this.priceList) {
            this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
                if (this.priceList) {
                    if (!this.priceList.name) {
                        mandatoryFieldKeys.push("common.name");
                    }
                }
            });
        }
    }
}
