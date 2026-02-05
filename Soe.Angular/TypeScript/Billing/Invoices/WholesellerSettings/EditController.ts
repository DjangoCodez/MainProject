import { EditControllerBase2 } from "../../../Core/Controllers/EditControllerBase2";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { ICompositionEditController } from "../../../Core/ICompositionEditController";
import { CoreUtility } from "../../../Util/CoreUtility";
import { InvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { SupplierService } from "../../../Shared/Economy/Supplier/SupplierService";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IValidationSummaryHandlerFactory } from "../../../Core/Handlers/ValidationSummaryHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IDirtyHandlerFactory } from "../../../Core/Handlers/DirtyHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { CompanyWholesellerDTO } from "../../../Common/Models/CompanyWholesellerDTO"; 

export class EditController extends EditControllerBase2 implements ICompositionEditController {

    // Data params
    companySysWholesellerId: number;
    companyWholeseller: CompanyWholesellerDTO;
    suppliersList: any[];
    sysWholesellersList: any[];
    customerNrs: string = "";
    supplierId: number;

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private invoiceService: InvoiceService,
        private supplierService: SupplierService,
        translationService: ITranslationService,
        urlHelperService: IUrlHelperService,
        progressHandlerFactory: IProgressHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        validationSummaryHandlerFactory: IValidationSummaryHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private dirtyHandlerFactory: IDirtyHandlerFactory) {
        super(urlHelperService, progressHandlerFactory, validationSummaryHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForEdit()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onDoLookUp(() => this.onDoLookups())
            .onLoadData(() => this.onLoadData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.companySysWholesellerId = parameters.id;
        this.guid = parameters.guid;
        this.dirtyHandler = this.dirtyHandlerFactory.create(this.guid);

        this.flowHandler.start([{ feature: Feature.Billing_Preferences_Wholesellers_Edit, loadReadPermissions: true, loadModifyPermissions: true }]);
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readOnlyPermission = response[Feature.Billing_Preferences_Wholesellers_Edit].readPermission;
        this.modifyPermission = response[Feature.Billing_Preferences_Wholesellers_Edit].modifyPermission;
        if (CoreUtility.isSupportAdmin)
            this.modifyPermission = true;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultEditToolbar(true, null, () => this.isNew);
    }

    // LOOKUPS
    private onDoLookups() {
        return this.$q.all([
            this.loadSuppliersList(),
            this.loadWholesellersList(),
        ]);            
    }

    private loadWholesellersList(): ng.IPromise<any> {
        return this.invoiceService.getSysWholesellersByCompanyDict(true, true).then((x) => {
            this.sysWholesellersList = x;
        })
    }

    private loadSuppliersList(): ng.IPromise<any> {
        return this.supplierService.getSuppliersDict(true, true, true).then((x) => {
            this.suppliersList = x;            
        })
    }

    private loadSupplierByWholesellerId() {
        this.invoiceService.getSupplierBySysWholeseller(this.companyWholeseller.sysWholesellerId).then((x) => {
            if (x) {
                this.supplierId = x.actorSupplierId;
            }
        })
    }

    private onLoadData(): ng.IPromise<any> {
        if (this.companySysWholesellerId > 0) {
            return this.invoiceService.getCompanyWholeseller(this.companySysWholesellerId).then((x) => {
                this.isNew = false;
                this.companyWholeseller = x;
                
                //set list of customer numbers
                this.customerNrs = "";
                _.forEach(this.companyWholeseller.ediConnections, (y) => {
                    if (this.customerNrs.length > 0)
                        this.customerNrs += "\n";
                    this.customerNrs += y.wholesellerCustomerNr;
                });

                //get supplier by syswholesellerid
                this.loadSupplierByWholesellerId();
            });
        }
        else {
            this.new();
        }
    }

    //EVENTS
    private sysWholesellerChanged(sysWholesellerId) {
        if (sysWholesellerId === 0) {
            this.companyWholeseller.messageTypes = undefined;
            this.companyWholeseller.sysWholesellerEdiId = undefined;
            this.companyWholeseller.hasEdiFeature = false;
            return;
        }

        this.invoiceService.getSysWholeseller(sysWholesellerId, true, true, true).then((x) => {
            this.companyWholeseller.messageTypes = x.messageTypes;
            this.companyWholeseller.sysWholesellerEdiId = x.sysWholesellerEdiId;
            this.companyWholeseller.hasEdiFeature = x.hasEdiFeature;
        })
    }


    public save() {
        this.progress.startSaveProgress((completion) => {
                                    
            this.invoiceService.saveCompanyWholesellerSetting(this.companyWholeseller, this.customerNrs.split('\n'), this.supplierId).then((result) => {
                if (result.success) {
                    if (result.integerValue && result.integerValue > 0)
                        this.companySysWholesellerId = result.integerValue;

                    completion.completed(this.isNew ? Constants.EVENT_EDIT_ADDED : Constants.EVENT_EDIT_SAVED, this.companyWholeseller);
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
            this.invoiceService.deleteCompanyWholeseller(this.companyWholeseller.sysWholesellerId).then((result) => {
                if (result.success) {
                    completion.completed(this.companyWholeseller);
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
    private new() {
        this.isNew = true;
        this.companySysWholesellerId = 0;
        this.companyWholeseller = new CompanyWholesellerDTO(); 
        this.companyWholeseller.sysWholesellerId = 0;
    }

    // VALIDATION
    public showValidationError() {
        this.validationHandler.showValidationSummary((mandatoryFieldKeys, validationErrorKeys, validationErrorStrings) => {
            if (this.companyWholeseller) {
                //if (!this.companyWholeseller.code) {
                //    mandatoryFieldKeys.push("common.code");
                //}
                //if (!this.companyWholeseller.name) {
                //    mandatoryFieldKeys.push("common.name");
                //}
            }
        });
    }
}
