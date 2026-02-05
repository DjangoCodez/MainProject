import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { ISupplierService } from "../../../../Shared/Economy/Supplier/SupplierService";
import { TabMessage } from "../../../../Core/Controllers/TabsControllerBase1";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { Feature, CompanySettingType, TermGroup, TermGroup_CurrencyType, SoeInvoiceType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { EditController } from "../../../../Shared/Economy/Supplier/Invoices/EditController";

export class GridController extends GridControllerBase {
    public gridHeaderComponentUrl;
    public searchModel: any = {};
    currencyTypes: any[];
    private supplierInvoicePermission: boolean;

    private nbrOfIntervals: number;
    private interval1: number;
    private interval2: number;
    private interval3: number;
    private interval4: number;
    private interval5: number;
    private terms;

    get selectedCompareDate() {
        return this.searchModel.compareDate;
    }

    set selectedCompareDate(date: any) {
        this.searchModel.compareDate = date;
    }

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        coreService: ICoreService,
        private supplierService: ISupplierService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private uiGridGroupingConstants: uiGrid.grouping.IUiGridGroupingConstants,
        private $q: ng.IQService) {

        super("Soe.Economy.Supplier.Invoice.AgeDistribution", "economy.supplier.invoice.agedistribution.agedistribution", Feature.Economy_Supplier_Invoice_AgeDistribution, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
        this.gridHeaderComponentUrl = this.urlHelperService.getViewUrl("searchHeader.html");
        this.resetSearchModel();

        this.doubleClickToEdit = false;
    }

    public init() {
        this.$q.all([
            this.loadModifyPermissions(),
            this.loadCompanySettings(),
            this.loadCurrencyTypes()]).then(() => {
                this.setupAgeDistributionGrid();
                this.stopProgress();
            });
    }

    protected permissionsLoaded() {
        // Setup grid
        this.initSetupGrid();
    }

    public setupAgeDistributionGrid() {
        this.soeGridOptions.showColumnFooter = true;
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
            super.addColumnNumber("actorNr", terms["economy.supplier.invoice.liquidityplanning.suppliernr"], "5%", true, null, "").groupingShowAggregationMenu = false;
            var actor = super.addColumnText("actorName", terms["common.name"], null, true);
            actor.groupingShowAggregationMenu = false
            actor.grouping = { groupPriority: 0 }; // Default grouped
            super.addColumnNumber("seqNr", terms["economy.supplier.invoice.liquidityplanning.sequencenr"], "5%", true, null, "").groupingShowAggregationMenu = false;
            super.addColumnNumber("invoiceNr", terms["economy.supplier.invoice.liquidityplanning.invoicenr"], "10%", true, null, "").groupingShowAggregationMenu = false;
            super.addColumnDate("invoiceDate", terms["economy.supplier.invoice.liquidityplanning.invoicedate"], null, true).groupingShowAggregationMenu = false;
            super.addColumnDate("expiryDate", terms["economy.supplier.invoice.liquidityplanning.overduedate"], null, true).groupingShowAggregationMenu = false;
            var amount1Column = super.addColumnNumber("amount1", "< {0}".format(this.interval1.toString()), "5%", false, 2, "")
            amount1Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
            amount1Column.customTreeAggregationFinalizerFn = (aggregation) => {
                if (aggregation && aggregation.value)
                    aggregation.rendered = aggregation.value.toString();
            }
            this.addSumAggregationFooterToColumnsGrouping(amount1Column);
            var amount2Column = super.addColumnNumber("amount2", "{0}-{1}".format(this.interval1.toString(), (this.interval2 - 1).toString()), "5%", false, 2, "");
            amount2Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
            amount2Column.customTreeAggregationFinalizerFn = (aggregation) => {
                if (aggregation && aggregation.value)
                    aggregation.rendered = aggregation.value.toString();
            }
            this.addSumAggregationFooterToColumnsGrouping(amount2Column);
            if (this.nbrOfIntervals > 3) {
                var amount3Column = super.addColumnNumber("amount3", "{0}-{1}".format(this.interval2.toString(), (this.interval3 - 1).toString()), "5%", false, 2, "");
                amount3Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                amount3Column.customTreeAggregationFinalizerFn = (aggregation) => {
                    if (aggregation && aggregation.value)
                        aggregation.rendered = aggregation.value.toString();
                }
                this.addSumAggregationFooterToColumnsGrouping(amount3Column);
                if (this.nbrOfIntervals > 4) {
                    var amount4Column = super.addColumnNumber("amount4", "{0}-{1}".format(this.interval3.toString(), (this.interval4 - 1).toString()), "5%", false, 2, "");
                    amount4Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                    amount4Column.customTreeAggregationFinalizerFn = (aggregation) => {
                        if (aggregation && aggregation.value)
                            aggregation.rendered = aggregation.value.toString();
                    }
                    this.addSumAggregationFooterToColumnsGrouping(amount4Column);
                    if (this.nbrOfIntervals > 5) {
                        var amount5Column = super.addColumnNumber("amount5", "{0}-{1}".format(this.interval4.toString(), (this.interval5 - 1).toString()), "5%", false, 2, "");
                        amount5Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                        amount5Column.customTreeAggregationFinalizerFn = (aggregation) => {
                            if (aggregation && aggregation.value)
                                aggregation.rendered = aggregation.value.toString();
                        }
                        this.addSumAggregationFooterToColumnsGrouping(amount5Column);
                        var amount6Column = super.addColumnNumber("amount6", "> {0}".format((this.interval5 - 1).toString()), "5%", false, 2, "");
                        amount6Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                        amount6Column.customTreeAggregationFinalizerFn = (aggregation) => {
                            if (aggregation && aggregation.value)
                                aggregation.rendered = aggregation.value.toString();
                        }
                        this.addSumAggregationFooterToColumnsGrouping(amount6Column);
                    } else {
                        var amount5Column = super.addColumnNumber("amount5", "> {0}".format((this.interval4 - 1).toString()), "5%", false, 2, "");
                        amount5Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                        amount5Column.customTreeAggregationFinalizerFn = (aggregation) => {
                            if (aggregation && aggregation.value)
                                aggregation.rendered = aggregation.value.toString();
                        }
                        this.addSumAggregationFooterToColumnsGrouping(amount5Column);
                    }
                } else {
                    var amount4Column = super.addColumnNumber("amount4", "> {0}".format((this.interval3 - 1).toString()), "5%", false, 2, "");
                    amount4Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                    amount4Column.customTreeAggregationFinalizerFn = (aggregation) => {
                        if (aggregation && aggregation.value)
                            aggregation.rendered = aggregation.value.toString();
                    }
                    this.addSumAggregationFooterToColumnsGrouping(amount4Column);
                }
            } else {
                var amount3Column = super.addColumnNumber("amount3", "> {0}".format((this.interval2 - 1).toString()), "5%", false, 2, "");
                amount3Column.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
                amount3Column.customTreeAggregationFinalizerFn = (aggregation) => {
                    if (aggregation && aggregation.value)
                        aggregation.rendered = aggregation.value.toString();
                }
                this.addSumAggregationFooterToColumnsGrouping(amount3Column);
            }
            var amountSumColumn = super.addColumnNumber("sumAmount", terms["common.sum"], null, false, 2, "");
            amountSumColumn.treeAggregationType = this.uiGridGroupingConstants.aggregation.SUM;
            amountSumColumn.customTreeAggregationFinalizerFn = (aggregation) => {
                if (aggregation && aggregation.value)
                    aggregation.rendered = aggregation.value.toString();
            }
            this.addSumAggregationFooterToColumnsGrouping(amountSumColumn);
            if (this.supplierInvoicePermission)
                this.soeGridOptions.addColumnEdit(terms["core.edit"], "openSupplierInvoice");
        });
    }

    private addSumFooter(column: uiGrid.IColumnDefOf<any>) {
        column.aggregationType = this.uiGridConstants.aggregationTypes.sum;
        column.aggregationHideLabel = true;
        column.width = "100";
        this.addSumAggregationFooterToColumns(column);
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        const featureIds: number[] = [Feature.Economy_Supplier_Invoice];

        return this.coreService.hasReadOnlyPermissions(featureIds)
            .then((x) => {
                this.supplierInvoicePermission = x[Feature.Economy_Supplier_Invoice];
            });
    }

    private loadCompanySettings(): ng.IPromise<any> {
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

    public loadCurrencyTypes() {
        return this.coreService.getTermGroupContent(TermGroup.CurrencyType, false, false).then((currencyTypes: any[]) => {
            // Remove transaction currency from list
            this.currencyTypes = currencyTypes.filter(x => x.id !== TermGroup_CurrencyType.TransactionCurrency);
            this.searchModel.currencyType = TermGroup_CurrencyType.BaseCurrency;
        });
    }

    public search() {
        this.startWork();
        this.supplierService.getAgeDistribution(this.searchModel).then((x) => {
            super.gridDataLoaded(x);
            this.completedWork(null, true);
        }, error => {
            this.failedWork(null);
        });
    }

    public resetSearchModel() {
        this.searchModel = {};
        this.searchModel.compareDate = new Date();
        this.searchModel.type = SoeInvoiceType.SupplierInvoice;
        this.searchModel.currencyType = TermGroup_CurrencyType.BaseCurrency;
    }

    public openSupplierInvoice(row: any) {
        var message = new TabMessage(
            `${this.terms["economy.supplier.invoice.invoice"]} ${row.seqNr}`,
            row.invoiceId,
            EditController,
            { id: row.invoiceId },
            this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html")
        );
        this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
    }
}
