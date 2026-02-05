import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IInvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { Feature, PriceListOrigin } from "../../../Util/CommonEnumerations";
import { CompanyWholesellerPriceListViewDTO } from "../../../Common/Models/CompanyWholeSellerPriceListViewDTO";
import { updatePriceListsController } from "../../../Common/Dialogs/updatePriceLists/updatePriceListsController";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    private wholesellerPriceLists: CompanyWholesellerPriceListViewDTO[];    
    
    //modal
    private modalInstance: any;

    //@ngInject
    constructor(private invoiceService: IInvoiceService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private $timeout: ng.ITimeoutService,
        $uibModal,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService,
    ) {
        super(gridHandlerFactory, "Billing.Invoices.WholesellerPricelists", progressHandlerFactory, messagingHandlerFactory);

        this.modalInstance = $uibModal;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })            
            .onBeforeSetUpGrid(() => this.loadWholesellerPriceLists())            
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    onInit(parameters: any) {
        this.parameters = parameters;        
        this.flowHandler.start({ feature: Feature.Billing_Preferences_InvoiceSettings_WholeSellerPriceList, loadReadPermissions: true, loadModifyPermissions: true });        
    }

    public setupGrid() {

        // Columns
        const keys: string[] = [            
            "common.active",
            "billing.invoices.wholesellerpricelists",
            "billing.invoice.wholesellerpricelist.name",
            "billing.invoice.wholesellerpricelist.date",
            "billing.invoice.wholesellerpricelist.version",
            "billing.invoice.wholesellerpricelist.delete",
            "common.customer.customer.wholesellername",
            "common.type"
        ];

        this.translationService.translateMany(keys).then((terms) => {            
            this.gridAg.addColumnBool("isUsed", terms["common.active"], 20, true);                                    
            this.gridAg.addColumnSelect("priceListName", terms["billing.invoice.wholesellerpricelist.name"], null, { displayField: "priceListName",  selectOptions:null, populateFilterFromGrid:true });                
            this.gridAg.addColumnText("sysWholesellerName", terms["common.customer.customer.wholesellername"], null);
            this.gridAg.addColumnText("typeName", terms["common.type"], null);

            this.gridAg.addColumnDate("date", terms["billing.invoice.wholesellerpricelist.date"], null, null, null, {minWidth:20});
            this.gridAg.addColumnNumber("version", terms["billing.invoice.wholesellerpricelist.version"], null, { minWidth: 20 });            
            this.gridAg.addColumnIcon(null, "", 40, { icon: "fal fa-chevron-up", onClick: this.upgrade.bind(this), showIcon: (row) => row.hasNewerVersion })
            this.gridAg.addColumnDelete(terms["billing.invoice.wholesellerpricelist.delete"], this.deletePriceList.bind(this), false, (row) => row.priceListOrigin == PriceListOrigin.CompDbPriceList);

            this.gridAg.finalizeInitGrid("billing.invoices.wholesellerpricelists", true, undefined);

            this.$timeout(() => {
                this.gridAg.options.setFilter('isUsed', { values: ['true'] });
            }, 10);
        });
    }
    

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData(), true, () => this.savePriceLists());
    }

    private loadWholesellerPriceLists(): ng.IPromise<any> {
        return this.invoiceService.getCompanyWholesellerPriceLists(false).then((data) => {
            _.forEach(data, y => {
                y.date = y.date ? new Date(<any>y.date).date() : null;
            })

            this.wholesellerPriceLists = data;         
        });
    }        

    public loadGridData() {
        this.setData(this.wholesellerPriceLists);
    }

    private savePriceLists() { 
        
        this.progress.startSaveProgress((completion) => {                
            this.invoiceService.saveCompanyWholesellerPriceLists(this.wholesellerPriceLists).then((result) => {                
                if (result.success)
                    completion.completed();
                else {
                    completion.failed(result.errorMessage);
                }
            });

        }, null)
            .then(data => {
                this.loadGridData();
            }, error => {
            });
        
    }    

    private deletePriceList(row) {
        
        if (row.priceListOrigin != PriceListOrigin.CompDbPriceList)
            return;

        const keys: string[] = [
            "billing.invoice.wholesellerpricelist.delete.success",            
            "billing.invoice.wholesellerpricelist.delete.notsuccess"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.progress.startDeleteProgress((completion) => {
                this.invoiceService.deleteCompPriceList(row.priceListImportedHeadId).then((result) => {
                    if (result.success) {
                        this.loadWholesellerPriceLists();
                        completion.completed(this.wholesellerPriceLists, false, (terms["billing.invoice.wholesellerpricelist.delete.success"]).format(result.integerValue.toString(), result.integerValue2.toString()));
                    } else {
                        completion.failed(terms["billing.invoice.wholesellerpricelist.delete.notsuccess"]+"\n"+result.errorMessage);
                    }
                }, error => {
                    completion.failed(terms["billing.invoice.wholesellerpricelist.delete.notsuccess"] + "\n" +error.message);
                });
            }).then(x => {
                this.loadGridData();
                });

        });
    }

    private upgrade(row) {        
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getCommonViewUrl("Dialogs/UpdatePriceLists", "UpdatePriceLists.html"),
            controller: updatePriceListsController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'sm',
            resolve: {
                sysPriceListHeadId: () => { return row.sysPriceListHeadId },
                sysWholesellerId: () => { return row.sysWholesellerId },                
            }
        });

        modal.result.then((result: any) => {
            if (result) {
                this.loadGridData();
            }
        });
    }    
}