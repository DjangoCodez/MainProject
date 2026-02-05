import { ITranslationService } from "../../../Core/Services/TranslationService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IInvoiceService } from "../../../Shared/Billing/Invoices/InvoiceService";
import { ICommonCustomerService } from "../../../Common/Customer/CommonCustomerService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { Feature } from "../../../Util/CommonEnumerations";
import { IconLibrary } from "../../../Util/Enumerations";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { PriceBasedMarkupDTO } from "../../../Common/Models/InvoiceDTO";
import { ISmallGenericType } from "../../../Scripts/TypeLite.Net4";
import { AddPriceBasedMarkupController } from "./Dialogs/AddPriceBasedMarkup/AddPriceBasedMarkup";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";

export class PriceBasedGridController extends GridControllerBase2Ag implements ICompositionGridController {
    private priceLists: ISmallGenericType[];
    private markupRows: any[] = [];

    //modal
    private modalInstance: any;

    //@ngInject
    constructor(private invoiceService: IInvoiceService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        private commonCustomerService: ICommonCustomerService,
        $uibModal,
        private $q: ng.IQService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService,
    ) {
        super(gridHandlerFactory, "Billing.Invoices.Markup", progressHandlerFactory, messagingHandlerFactory);

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
            .onDoLookUp(() => this.loadPricelists())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData());
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.flowHandler.start({ feature: Feature.Billing_Preferences_InvoiceSettings_Markup, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());

        var groupAdd = ToolBarUtility.createGroup(new ToolBarButton("common.createnew", "common.createnew", IconLibrary.FontAwesome, "fa-plus", () => {
            this.openAddPriceBasedMarkupDialog();
        }));
        this.toolbar.addButtonGroup(groupAdd);
    }

    private loadPricelists(): ng.IPromise<any> {
        return this.commonCustomerService.getPriceListsDict(true, true).then(x => {
            this.priceLists = x;
        });
    }

    edit(row) {

    }

    protected setupGrid() {
        // Columns
        var keys: string[] = [
            "economy.supplier.invoice.matches.amountfrom",
            "economy.customer.invoice.matches.amountto",
            "billing.projects.list.pricelist",
            "billing.invoices.markup.markuppercent"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnNumber("minPrice", terms["economy.supplier.invoice.matches.amountfrom"], null);
            this.gridAg.addColumnNumber("maxPrice", terms["economy.customer.invoice.matches.amountto"], null);
            this.gridAg.addColumnText("priceListName", terms["billing.projects.list.pricelist"], null);
            this.gridAg.addColumnNumber("markupPercent", terms["billing.invoices.markup.markuppercent"], null);
            this.gridAg.addColumnEdit(terms["core.edit"], this.openAddPriceBasedMarkupDialog.bind(this));

            this.gridAg.finalizeInitGrid("economy.accounting.companygroup.mappings", true);
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.invoiceService.getPriceBasedMarkup().then(x => {
                this.markupRows = x
                return this.markupRows;
            }).then(data => {
                this.setData(this.markupRows);
            });
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }

    protected openAddPriceBasedMarkupDialog(row?: PriceBasedMarkupDTO) {
        // handle excluded
        if (!row) {
            row = new PriceBasedMarkupDTO();
            row.priceListTypeId = 0;
        }

        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Billing/Invoices/Markup/Dialogs/AddPriceBasedMarkup/AddPriceBasedMarkup.html"),
            controller: AddPriceBasedMarkupController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'md',
            resolve: {
                translationService: () => { return this.translationService },
                priceBasedMarkup: () => { return row },
                priceLists: () => { return this.priceLists },
            }
        });

        modal.result.then((dialogResult: any) => {
            if (dialogResult) {
                if (dialogResult.delete) {
                    this.progress.startDeleteProgress((completion) => {
                        this.invoiceService.deletePriceBasedMarkup(dialogResult.item.priceBasedMarkupId).then((result) => {
                            if (result.success)
                                completion.completed(null);
                            else
                                completion.failed(result.errorMessage);
                        });
                    }, null).then(() => {
                        this.loadGridData();
                    });
                }
                else if (dialogResult.item) {
                    this.progress.startSaveProgress((completion) => {
                        this.invoiceService.savePriceBasedMarkup(dialogResult.item).then((result) => {
                            if (result.success)
                                completion.completed();
                            else
                                completion.failed(result.errorMessage);
                        });
                    }, null).then(() => {
                        this.loadGridData();
                    });
                }
            }
        });
    }
}