import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ICommonCustomerService } from "../../../../Common/Customer/CommonCustomerService";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { Feature, CompanySettingType, TermGroup, TermGroup_CurrencyType, SoeInvoiceType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    private invoiceType: SoeInvoiceType;

    public gridHeaderComponentUrl;
    public gridFooterComponentUrl;
    public searchModel: any = {};
    currencyTypes: any[];
    private customerInvoicePermission: boolean;
    private supplierInvoicePermission: boolean;

    private nbrOfIntervals: number;
    private interval1: number;
    private interval2: number;
    private interval3: number;
    private interval4: number;
    private interval5: number;
    private terms;

    private get isSupplierInvoice() {
        return this.invoiceType === SoeInvoiceType.SupplierInvoice;
    }

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private coreService: ICoreService,
        private commonCustomerService: ICommonCustomerService,
        private translationService: ITranslationService,
        private messagingService: IMessagingService,
        notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private uiGridConstants: uiGrid.IUiGridConstants,
        private uiGridGroupingConstants: uiGrid.grouping.IUiGridGroupingConstants,
        private $q: ng.IQService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory    ) {
        super(gridHandlerFactory, "Soe.Economy.Customer.Invoice.AgeDistribution", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify;
                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.doLookup())
            .onLoadGridData(() => this.setData(""));

        this.doubleClickToEdit = false;
    }

    private doLookup(): ng.IPromise<any> {
        return this.$q.all([
            this.loadModifyPermissions(),
            this.loadCompanySettings(),
            this.loadCurrencyTypes()]).then(() => {
                this.resetSearchModel();

                if (this.invoiceType === SoeInvoiceType.CustomerInvoice)
                    this.setupCustomerAgeDistributionGrid();
                else
                    this.setupSupplierAgeDistributionGrid();
            });
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;
        this.invoiceType = parameters.invoiceType;

        this.flowHandler.start(this.invoiceType === SoeInvoiceType.CustomerInvoice ? { feature: Feature.Economy_Customer_Invoice_AgeDistribution, loadReadPermissions: true, loadModifyPermissions: true } : [{ feature: Feature.Economy_Supplier_Suppliers_Edit, loadReadPermissions: true, loadModifyPermissions: true }, { feature: Feature.Manage_ContactPersons_Edit, loadModifyPermissions: true }]);
        
        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => {  });
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, null);
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("searchHeader.html"));
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [
            Feature.Economy_Customer_Invoice_Invoices_Edit,
            Feature.Economy_Supplier_Invoice_Invoices_Edit
        ];

        return this.coreService.hasModifyPermissions(featureIds)
            .then((x) => {
                this.customerInvoicePermission = x[Feature.Economy_Customer_Invoice_Invoices_Edit];
                this.supplierInvoicePermission = x[Feature.Economy_Supplier_Invoice_Invoices_Edit];
            });
    }

    public setupCustomerAgeDistributionGrid() {
        // Columns
        const keys: string[] = [
            "common.customer.invoices.customerinvoice",
            "common.customer.customer.customernr",
            "economy.supplier.invoice.liquidityplanning.sequencenr",
            "common.name",
            "economy.supplier.invoice.liquidityplanning.invoicedate",
            "economy.supplier.invoice.liquidityplanning.overduedate",
            "economy.supplier.invoice.liquidityplanning.invoicenr",
            "economy.supplier.invoice.invoice",
            "common.sum",
            "core.edit"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            this.gridAg.options.addGroupAverageAggFunction();
            var actor = this.gridAg.addColumnText("actorName", terms["common.name"], null, true, { enableRowGrouping: true });
            actor.groupingShowAggregationMenu = false;
            actor.grouping = { groupPriority: 0 }; // Default grouped
            this.gridAg.options.groupRowsByColumn(actor, true);
            this.gridAg.addColumnNumber("actorNr", terms["common.customer.customer.customernr"], 70, { decimals: null, enableHiding: true, onChanged: null, enableRowGrouping: true }).groupingShowAggregationMenu = false;
            this.gridAg.addColumnNumber("seqNr", terms["economy.supplier.invoice.liquidityplanning.sequencenr"], 60, { decimals: null, enableHiding: true, onChanged: null, enableRowGrouping: true  }).groupingShowAggregationMenu = false;
            this.gridAg.addColumnNumber("invoiceNr", terms["economy.supplier.invoice.liquidityplanning.invoicenr"], 60, { decimals: null, enableHiding: true, onChanged: null, enableRowGrouping: true }).groupingShowAggregationMenu = false;
            this.gridAg.addColumnDate("invoiceDate", terms["economy.supplier.invoice.liquidityplanning.invoicedate"], 100, true, null, { enableRowGrouping: true }).groupingShowAggregationMenu = false;
            this.gridAg.addColumnDate("expiryDate", terms["economy.supplier.invoice.liquidityplanning.overduedate"], 100, true, null, { enableRowGrouping: true }).groupingShowAggregationMenu = false;
            var amount1Column = this.gridAg.addColumnNumber("amount1", "< {0}".format(this.interval1.toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum' })
            amount1Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
            amount1Column.customTreeAggregationFinalizerFn = (aggregation) => {
                if (aggregation && aggregation.value)
                    aggregation.rendered = aggregation.value.toString();
            }
           
            var amount2Column = this.gridAg.addColumnNumber("amount2", "{0}-{1}".format(this.interval1.toString(), (this.interval2 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum'});
            amount2Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
            amount2Column.customTreeAggregationFinalizerFn = (aggregation) => {
                if (aggregation && aggregation.value)
                    aggregation.rendered = aggregation.value.toString();
            }
            
            if (this.nbrOfIntervals > 3) {
                var amount3Column = this.gridAg.addColumnNumber("amount3", "{0}-{1}".format(this.interval2.toString(), (this.interval3 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
                amount3Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                amount3Column.customTreeAggregationFinalizerFn = (aggregation) => {
                    if (aggregation && aggregation.value)
                        aggregation.rendered = aggregation.value.toString();
                }
                
                if (this.nbrOfIntervals > 4) {
                    var amount4Column = this.gridAg.addColumnNumber("amount4", "{0}-{1}".format(this.interval3.toString(), (this.interval4 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
                    amount4Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                    amount4Column.customTreeAggregationFinalizerFn = (aggregation) => {
                        if (aggregation && aggregation.value)
                            aggregation.rendered = aggregation.value.toString();
                    }
                 
                    if (this.nbrOfIntervals > 5) {
                        var amount5Column = this.gridAg.addColumnNumber("amount5", "{0}-{1}".format(this.interval4.toString(), (this.interval5 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum'});
                        amount5Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                        amount5Column.customTreeAggregationFinalizerFn = (aggregation) => {
                            if (aggregation && aggregation.value)
                                aggregation.rendered = aggregation.value.toString();
                        }
                       
                        var amount6Column = this.gridAg.addColumnNumber("amount6", "> {0}".format((this.interval5 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
                        amount6Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                        amount6Column.customTreeAggregationFinalizerFn = (aggregation) => {
                            if (aggregation && aggregation.value)
                                aggregation.rendered = aggregation.value.toString();
                        }
                       
                    } else {
                        var amount5Column = this.gridAg.addColumnNumber("amount5", "> {0}".format((this.interval4 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum'});
                        amount5Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                        amount5Column.customTreeAggregationFinalizerFn = (aggregation) => {
                            if (aggregation && aggregation.value)
                                aggregation.rendered = aggregation.value.toString();
                        }
                      
                    }
                } else {
                    var amount4Column = this.gridAg.addColumnNumber("amount4", "> {0}".format((this.interval3 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum'});
                    amount4Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                    amount4Column.customTreeAggregationFinalizerFn = (aggregation) => {
                        if (aggregation && aggregation.value)
                            aggregation.rendered = aggregation.value.toString();
                    }
                 
                }
            } else {
                var amount3Column = this.gridAg.addColumnNumber("amount3", "> {0}".format((this.interval2 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum'});
                amount3Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                amount3Column.customTreeAggregationFinalizerFn = (aggregation) => {
                    if (aggregation && aggregation.value)
                        aggregation.rendered = aggregation.value.toString();
                }
               
            }
            var amountSumColumn = this.gridAg.addColumnNumber("sumAmount", terms["common.sum"], null, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum'});
            amountSumColumn.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
            amountSumColumn.customTreeAggregationFinalizerFn = (aggregation) => {
                if (aggregation && aggregation.value)
                    aggregation.rendered = aggregation.value.toString();
            }

            if (this.customerInvoicePermission)
                this.gridAg.options.addColumnEdit(terms["core.edit"], (row) => this.openCustomerInvoice(row), null, this.showEditButton.bind(this));

            this.gridAg.options.useGrouping();
            this.gridAg.finalizeInitGrid("economy.supplier.invoice.agedistribution.agedistribution", true);
        });
    }

    public setupSupplierAgeDistributionGrid() {
        // Columns
        var keys: string[] = [
            "economy.supplier.invoice.liquidityplanning.suppliernr",
            "economy.supplier.invoice.liquidityplanning.sequencenr",
            "common.name",
            "economy.supplier.invoice.liquidityplanning.invoicedate",
            "economy.supplier.invoice.liquidityplanning.overduedate",
            "economy.supplier.invoice.liquidityplanning.invoicenr",
            "economy.supplier.invoice.invoice",
            "common.sum",
            "core.edit"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;

            this.gridAg.options.addGroupAverageAggFunction(); var actor = this.gridAg.addColumnText("actorName", terms["common.name"], null, true, { enableRowGrouping: true });
            actor.groupingShowAggregationMenu = false;
            actor.grouping = { groupPriority: 0 }; // Default grouped
            this.gridAg.options.groupRowsByColumn(actor, true);
            this.gridAg.addColumnNumber("actorNr", terms["economy.supplier.invoice.liquidityplanning.suppliernr"], 70, { decimals: null, enableHiding: true, onChanged: null, enableRowGrouping: true }).groupingShowAggregationMenu = false;
            this.gridAg.addColumnNumber("seqNr", terms["economy.supplier.invoice.liquidityplanning.sequencenr"], 60, { decimals: null, enableHiding: true, onChanged: null, enableRowGrouping: true }).groupingShowAggregationMenu = false;
            this.gridAg.addColumnNumber("invoiceNr", terms["economy.supplier.invoice.liquidityplanning.invoicenr"], 60, { decimals: null, enableHiding: true, onChanged: null, enableRowGrouping: true }).groupingShowAggregationMenu = false;
            this.gridAg.addColumnDate("invoiceDate", terms["economy.supplier.invoice.liquidityplanning.invoicedate"], 100, true, null, { enableRowGrouping: true }).groupingShowAggregationMenu = false;
            this.gridAg.addColumnDate("expiryDate", terms["economy.supplier.invoice.liquidityplanning.overduedate"], 100, true, null, { enableRowGrouping: true }).groupingShowAggregationMenu = false;
            
            var amount1Column = this.gridAg.addColumnNumber("amount1", "< {0}".format(this.interval1.toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum' })
            amount1Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
            amount1Column.customTreeAggregationFinalizerFn = (aggregation) => {
                if (aggregation && aggregation.value)
                    aggregation.rendered = aggregation.value.toString();
            }

            var amount2Column = this.gridAg.addColumnNumber("amount2", "{0}-{1}".format(this.interval1.toString(), (this.interval2 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
            amount2Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
            amount2Column.customTreeAggregationFinalizerFn = (aggregation) => {
                if (aggregation && aggregation.value)
                    aggregation.rendered = aggregation.value.toString();
            }

            if (this.nbrOfIntervals > 3) {
                var amount3Column = this.gridAg.addColumnNumber("amount3", "{0}-{1}".format(this.interval2.toString(), (this.interval3 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
                amount3Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                amount3Column.customTreeAggregationFinalizerFn = (aggregation) => {
                    if (aggregation && aggregation.value)
                        aggregation.rendered = aggregation.value.toString();
                }

                if (this.nbrOfIntervals > 4) {
                    var amount4Column = this.gridAg.addColumnNumber("amount4", "{0}-{1}".format(this.interval3.toString(), (this.interval4 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
                    amount4Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                    amount4Column.customTreeAggregationFinalizerFn = (aggregation) => {
                        if (aggregation && aggregation.value)
                            aggregation.rendered = aggregation.value.toString();
                    }

                    if (this.nbrOfIntervals > 5) {
                        var amount5Column = this.gridAg.addColumnNumber("amount5", "{0}-{1}".format(this.interval4.toString(), (this.interval5 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
                        amount5Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                        amount5Column.customTreeAggregationFinalizerFn = (aggregation) => {
                            if (aggregation && aggregation.value)
                                aggregation.rendered = aggregation.value.toString();
                        }

                        var amount6Column = this.gridAg.addColumnNumber("amount6", "> {0}".format((this.interval5 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
                        amount6Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                        amount6Column.customTreeAggregationFinalizerFn = (aggregation) => {
                            if (aggregation && aggregation.value)
                                aggregation.rendered = aggregation.value.toString();
                        }

                    } else {
                        var amount5Column = this.gridAg.addColumnNumber("amount5", "> {0}".format((this.interval4 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
                        amount5Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                        amount5Column.customTreeAggregationFinalizerFn = (aggregation) => {
                            if (aggregation && aggregation.value)
                                aggregation.rendered = aggregation.value.toString();
                        }

                    }
                } else {
                    var amount4Column = this.gridAg.addColumnNumber("amount4", "> {0}".format((this.interval3 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
                    amount4Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                    amount4Column.customTreeAggregationFinalizerFn = (aggregation) => {
                        if (aggregation && aggregation.value)
                            aggregation.rendered = aggregation.value.toString();
                    }

                }
            } else {
                var amount3Column = this.gridAg.addColumnNumber("amount3", "> {0}".format((this.interval2 - 1).toString()), 55, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
                amount3Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                amount3Column.customTreeAggregationFinalizerFn = (aggregation) => {
                    if (aggregation && aggregation.value)
                        aggregation.rendered = aggregation.value.toString();
                }

            }
            var amountSumColumn = this.gridAg.addColumnNumber("sumAmount", terms["common.sum"], null, { decimals: 2, enableHiding: false, onChanged: null, enableRowGrouping: true, aggFuncOnGrouping: 'sum' });
            amountSumColumn.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
            amountSumColumn.customTreeAggregationFinalizerFn = (aggregation) => {
                if (aggregation && aggregation.value)
                    aggregation.rendered = aggregation.value.toString();
            }

            if (this.supplierInvoicePermission)
                this.gridAg.options.addColumnEdit(terms["core.edit"], (row) => this.openCustomerInvoice(row), null, this.showEditButton.bind(this));

            this.gridAg.options.useGrouping();
            this.gridAg.finalizeInitGrid("economy.supplier.invoice.agedistribution.agedistribution", true);
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        if (this.invoiceType === SoeInvoiceType.CustomerInvoice) {
            const settingTypes: number[] = [
                CompanySettingType.CustomerInvoiceAgeDistributionNbrOfIntervals,
                CompanySettingType.CustomerInvoiceAgeDistributionInterval1,
                CompanySettingType.CustomerInvoiceAgeDistributionInterval1,
                CompanySettingType.CustomerInvoiceAgeDistributionInterval2,
                CompanySettingType.CustomerInvoiceAgeDistributionInterval3,
                CompanySettingType.CustomerInvoiceAgeDistributionInterval4,
                CompanySettingType.CustomerInvoiceAgeDistributionInterval5,
            ];
            return this.coreService.getCompanySettings(settingTypes).then(x => {
                this.nbrOfIntervals = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerInvoiceAgeDistributionNbrOfIntervals);
                this.interval1 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerInvoiceAgeDistributionInterval1);
                this.interval2 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerInvoiceAgeDistributionInterval2);
                this.interval3 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerInvoiceAgeDistributionInterval3);
                this.interval4 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerInvoiceAgeDistributionInterval4);
                this.interval5 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.CustomerInvoiceAgeDistributionInterval5);
            });
        }
        else {
            const settingTypes: number[] = [
                CompanySettingType.SupplierInvoiceAgeDistributionNbrOfIntervals,
                CompanySettingType.SupplierInvoiceAgeDistributionInterval1,
                CompanySettingType.SupplierInvoiceAgeDistributionInterval2,
                CompanySettingType.SupplierInvoiceAgeDistributionInterval3,
                CompanySettingType.SupplierInvoiceAgeDistributionInterval4,
                CompanySettingType.SupplierInvoiceAgeDistributionInterval5
            ];
            return this.coreService.getCompanySettings(settingTypes).then(x => {
                this.nbrOfIntervals = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SupplierInvoiceAgeDistributionNbrOfIntervals);
                this.interval1 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SupplierInvoiceAgeDistributionInterval1);
                this.interval2 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SupplierInvoiceAgeDistributionInterval2);
                this.interval3 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SupplierInvoiceAgeDistributionInterval3);
                this.interval4 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SupplierInvoiceAgeDistributionInterval4);
                this.interval5 = SettingsUtility.getIntCompanySetting(x, CompanySettingType.SupplierInvoiceAgeDistributionInterval5);
            });
        }
    }

    private showEditButton(row) {
        return row != undefined;
    }

    public loadCurrencyTypes() {
        return this.coreService.getTermGroupContent(TermGroup.CurrencyType, false, false).then((currencyTypes: any[]) => {
            // Remove transaction currency from list
            this.currencyTypes = currencyTypes.filter(x => x.id !== TermGroup_CurrencyType.TransactionCurrency);
            this.searchModel.currencyType = TermGroup_CurrencyType.BaseCurrency;
        });
    }

    public search() {
        this.progress.startLoadingProgress([() => {
            return this.commonCustomerService.getAgeDistribution(this.searchModel).then((x) => {
                this.setData(x);
            });
        }]);
    }

    public loadData() {
        this.gridAg.clearData();
        this.resetSearchModel();

        this.progress.startLoadingProgress([() => {
            return this.commonCustomerService.getAgeDistribution(this.searchModel).then((x) => {
                this.setData(x);
            });
        }]);
    }

    public resetSearchModel() {
        this.searchModel = {};
        this.searchModel.compareDate = new Date();
        this.searchModel.type = this.invoiceType;
        this.searchModel.currencyType = TermGroup_CurrencyType.BaseCurrency;
    }

    public openCustomerInvoice(row: any) {
        this.messagingService.publish(Constants.EVENT_OPEN_EDITCUSTOMERINVOICE, {
            id: row.invoiceId,
            name: (this.invoiceType === SoeInvoiceType.CustomerInvoice ? this.terms["common.customer.invoices.customerinvoice"] : this.terms["economy.supplier.invoice.invoice"]) + " " + row.seqNr,
            registrationType: row.registrationType
        });
    }
}
