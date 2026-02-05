import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { CoreUtility } from "../../../Util/CoreUtility";
import { IReportService } from "../../../Core/Services/ReportService";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { IconLibrary, SoeGridOptionsEvent, } from "../../../Util/Enumerations";
import { Feature, SettingMainType, CompanySettingType, SoeReportTemplateType, UserSettingType } from "../../../Util/CommonEnumerations";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IVoucherRowDTO } from "../../../Scripts/TypeLite.Net4";
import { AccountDimSmallDTO } from "../../../Common/Models/AccountDimDTO";
import { IRequestReportService } from "../../../Shared/Reports/RequestReportService";


export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Config
    isTemplates = false;
    accountYearId = 0;

    // Filters
    voucherSeriesTypes: any[] = [];
    accountYears: any[] = [];

    accountingOrderReportId: number;

    //Terms
    terms: { [index: string]: string; };

    gridHasSelectedRows = false;
    isPrinting = false;

    accountDims: AccountDimSmallDTO[] = [];

    // Grid header
    toolbarInclude: any;

    // Flags
    loading = false;

    get selectedAccountYear() {
        return this.accountYearId;
    }
    set selectedAccountYear(item: any) {
        this.accountYearId = item;
        this.loadVoucherSeriesTypes();
    }

    private _selectedVoucherSerie: any;
    get selectedVoucherSerie() {
        return this._selectedVoucherSerie;
    }
    set selectedVoucherSerie(item: any) {
        this._selectedVoucherSerie = item;
        if (!this.loading)
            this.updateVoucherSelection();
    }

    //@ngInject
    constructor(
        private $window,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private accountingService: IAccountingService,
        private reportService: IReportService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $scope: ng.IScope,
        private $q: ng.IQService,
        private readonly requestReportService: IRequestReportService,
    ) {
        super(gridHandlerFactory, "Economy.Accounting.Vouchers", progressHandlerFactory, messagingHandlerFactory);

        this.setIdColumnNameOnEdit("voucherHeadId");

        // Config parameters
        if (soeConfig.isTemplates)
            this.isTemplates = true;
        this.accountYearId = soeConfig.accountYearId;

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    // Send messages to TabsController
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onBeforeSetUpGrid(() => this.loadLookups())
            .onSetUpGrid(() => this.setupGrid())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.loading = true;
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.onTabActivetedAndModified(() => {
                this.loadGridData();
            });
        }

        this.flowHandler.start({ feature: Feature.Economy_Accounting_Vouchers_Edit, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.loadGridData());

        if (!this.isTemplates) {
            // Print
            const group = ToolBarUtility.createGroup(new ToolBarButton("", "economy.accounting.voucher.printselectedvouchers", IconLibrary.FontAwesome, "fa-print", () => {
                this.printSelectedVouchers();
            }, () => {
                return !this.gridHasSelectedRows || this.isPrinting
            }));

            // Delete
            if (CoreUtility.isSupportSuperAdmin) {
                group.buttons.push(new ToolBarButton("", "economy.accounting.voucher.deleteselectedvouchers", IconLibrary.FontAwesome, "fa-times", () => {
                    this.initDeleteSelectedVouchers();
                }, () => {
                    return !this.gridHasSelectedRows
                }));
            }

            this.toolbar.addButtonGroup(group);

            this.toolbarInclude = this.urlHelperService.getGlobalUrl("economy/accounting/vouchers/views/gridHeader.html");
            this.toolbar.addInclude(this.toolbarInclude);
        }
    }

    public setupGrid() {

        this.gridAg.options.subscribe([new GridEvent(SoeGridOptionsEvent.RowSelectionChangedBatch, (row: uiGrid.IGridRow) => { this.gridSelectionChanged(); })]);

        const keys: string[] = [
            "common.number",
            "common.date",
            "common.text",
            "economy.accounting.voucher.voucherseries",
            "economy.accounting.voucher.vatvoucher",
            "economy.accounting.voucher.vouchermodified",
            "economy.accounting.voucher.sourcetype",
            "core.edit",
            "common.debit",
            "common.credit",
            "common.accountingrows.rownr"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            //rows...
            this.gridAg.enableMasterDetail(true);
            this.gridAg.detailOptions.enableRowSelection = false;
            this.gridAg.options.setDetailCellDataCallback((params) => {
                this.loadAccountingRows(params);
                this.$scope.$applyAsync(() => {
                    this.gridAg.detailOptions.enableRowSelection = false;
                    this.gridAg.detailOptions.sizeColumnToFit();
                });
            });

            this.gridAg.detailOptions.addColumnNumber("rowNr", terms["common.accountingrows.rownr"], 50, { maxWidth: 50});


            this.accountDims.forEach((ad, i) => {
                let index = i + 1;
                this.gridAg.detailOptions.addColumnText("dim" + index + "Name", ad.name, null);
            });
            
            this.gridAg.detailOptions.addColumnText("text", terms["common.text"],null);
            this.gridAg.detailOptions.addColumnNumber("amountDebet", terms["common.debit"], 80, { enableHiding: false, decimals: 2, minWidth: 80 });
            this.gridAg.detailOptions.addColumnNumber("amountCredit", terms["common.credit"], 80, { enableHiding: false, decimals: 2, minWidth: 80 });
            
            this.gridAg.detailOptions.finalizeInitGrid();

            // Columns
            this.gridAg.addColumnNumber("voucherNr", terms["common.number"], 30);
            this.gridAg.addColumnDate("date", terms["common.date"], 60);
            this.gridAg.addColumnText("text", terms["common.text"], null);
            this.gridAg.addColumnSelect("voucherSeriesTypeName", terms["economy.accounting.voucher.voucherseries"], 60, { displayField: "voucherSeriesTypeName", selectOptions: this.voucherSeriesTypes, populateFilterFromGrid: true });
            this.gridAg.addColumnBool("vatVoucher", terms["economy.accounting.voucher.vatvoucher"], 30);
            this.gridAg.addColumnText("sourceTypeName", terms["economy.accounting.voucher.sourcetype"], 60, true, { hide: true, enableHiding: true });
            this.gridAg.addColumnIcon("hasDocumentsIconValue", null, null, { maxWidth:40, toolTipField: "hasDocumentsTooltip", showIcon: (row) => row.hasDocuments, showTooltipFieldInFilter: true });
            this.gridAg.addColumnIcon("accRowsIconValue", null, null, { maxWidth: 40, toolTipField: "accRowsText", showTooltipFieldInFilter: true });
            this.gridAg.addColumnIcon("modifiedIconValue", null, null, { maxWidth: 40, toolTipField: "modifiedTooltip", showTooltipFieldInFilter: true, showIcon: this.showStatusIcon.bind(this) });

            if (this.readPermission)
                this.gridAg.addColumnEdit(terms["core.edit"], this.editRow.bind(this), false);

            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.RowDoubleClicked, (row) => { this.editRow(row); }));
            this.gridAg.options.subscribe(events);

            this.gridAg.finalizeInitGrid("economy.accounting.voucher.voucher", true);
        });
    }

    public editRow(row) {
        row['selectedYear'] = _.find(this.accountYears, { 'accountYearId': this.selectedAccountYear });
        this.edit(row);
    }

    public showStatusIcon(row: any): boolean {
        return row.hasHistoryRows;
    }

    public reloadGridFromFilter = _.debounce(() => {
        this.loadGridData();
    }, 100, { leading: false, trailing: true });

    public loadGridData() {
        // Load data
        if (this.isTemplates) {
            this.progress.startLoadingProgress([() => {
                return this.accountingService.getVoucherTemplates(this.accountYearId).then((x) => {
                    this.setData(x);
                });
            }]);
        } else {
            if (!this.accountYearId)
                return; 

            this.progress.startLoadingProgress([() => {
                return this.accountingService.getVouchersBySeries(this.accountYearId, this.selectedVoucherSerie).then((items) => {
                    for (const element of items) {
                        const voucher = element;
                        voucher['expander'] = "";

                        if (voucher.date)
                            voucher.date = new Date(voucher.date).date();
                        if (voucher.modified) {
                            voucher.modified = new Date(voucher.modified).date();
                            voucher["modifiedTooltip"] = this.terms["economy.accounting.voucher.vouchermodified"];
                            voucher["modifiedIconValue"] = "fal fa-exclamation-circle warningColor";
                        }
                        if (voucher.hasDocuments) {
                            voucher["hasDocumentsTooltip"] = this.terms["core.attachments"];
                            voucher["hasDocumentsIconValue"] = "fal fa-paperclip";
                            
                        }

                        if (voucher.hasNoRows) {
                            voucher["accRowsIconValue"] = "fal fa-ban errorColor";
                            voucher["accRowsText"] = this.terms["economy.accounting.voucher.missingrows"];
                        }
                        else if (voucher.hasUnbalancedRows) {
                            voucher["accRowsIconValue"] = "fal fa-siren-on errorColor";
                            voucher["accRowsText"] = this.terms["economy.accounting.voucher.unbalancedrowswarning"];
                        }
                        else {
                            voucher["accRowsIconValue"] = undefined;
                            voucher["accRowsText"] = undefined;
                        }
                    }
                    this.setData(items);
                });
            }]);
            
        }
    }

    public loadAccountingRows(params: any) {
        return this.accountingService.getVoucherRows(params.data.voucherHeadId).then((rows: IVoucherRowDTO[]) => {

            for (let i = 0; i < rows.length; i++) {
                let row = rows[i];
                
                if (row.dim1Nr)
                    row.dim1Name = row.dim1Nr + " - " + row.dim1Name;
                if (row.dim2Nr)
                    row.dim2Name = row.dim2Nr + " - " + row.dim2Name;
                if (row.dim3Nr)
                    row.dim3Name = row.dim3Nr + " - " + row.dim3Name;
                if (row.dim4Nr)
                    row.dim4Name = row.dim4Nr + " - " + row.dim4Name;
                if (row.dim5Nr)
                    row.dim5Name = row.dim5Nr + " - " + row.dim5Name;
                if (row.dim6Nr)
                    row.dim6Name = row.dim6Nr + " - " + row.dim6Name;
                
                if (row.amount < 0) {
                    row["amountCredit"] = Math.abs(row.amount);
                    row["amountDebet"] = 0;
                }
                else {
                    row["amountDebet"] = row.amount;
                    row["amountCredit"] = 0;
                }
            }

            params.data['rows'] = _.orderBy(rows, (r) => r.rowNr);
            params.data['rowsLoaded'] = true;
            params.successCallback(params.data['rows']);
        });
    }

    protected loadLookups(): ng.IPromise<any> {
        return this.$q.all([
            this.loadTerms(),
            this.loadUserSettings(),
            this.loadAccountingOrderReportId(),
            this.loadAccountDims(),
            this.loadAccountYears()
        ]).then(() => {
            this.loadVoucherSeriesTypes();
            this.loading = false;
        });
    }

    protected loadTerms(): ng.IPromise<any> {
        const keys: string[] = [
            "common.all",
            "common.number",
            "economy.accounting.voucher.voucherseries",
            "economy.accounting.voucher.missingrows",
            "economy.accounting.voucher.unbalancedrowswarning",
            "economy.accounting.voucher.vouchermodified",
            "core.attachments"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    protected loadUserSettings(): ng.IPromise<any> {
        const settingTypes: number[] = [UserSettingType.VoucherSeriesSelection];

        return this.coreService.getUserSettings(settingTypes).then(x => {
            this.selectedVoucherSerie = SettingsUtility.getIntUserSetting(x, UserSettingType.VoucherSeriesSelection, 0, false);
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.accountingService.getAccountDimsSmall(false, false, true, true, true).then(x => {
            this.accountDims = x;
        });
    }

    private loadAccountYears(): ng.IPromise<any> {
        return this.accountingService.getAccountYears(false, true).then((x) => {
            this.accountYears = _.reverse(x);
        });
    }

    private loadVoucherSeriesTypes(): ng.IPromise<any> {
        this.voucherSeriesTypes = [];
        return this.accountingService.getVoucherSeriesByYear(this.accountYearId, false, true).then((data: any[]) => {

            data = _.sortBy(data, function (item) { return item.voucherSeriesTypeNr; });

            this.voucherSeriesTypes.push({ value: 0, label: this.terms["common.all"] });

            data.forEach(x => {
                this.voucherSeriesTypes.push({
                    value: x.voucherSeriesTypeId, label: x.voucherSeriesTypeNr + ". " + x.voucherSeriesTypeName });
            });
            
            const serie = this.selectedVoucherSerie;
            if (serie) {
                const serieToSelect = this.voucherSeriesTypes.find(s => s.value === serie);
                this.$timeout(() => {
                    this.selectedVoucherSerie = serieToSelect ? serieToSelect.value : (this.voucherSeriesTypes.length > 0 ? this.voucherSeriesTypes[0].value : undefined);
                });
            }
            else {
                this.$timeout(() => {
                    this.selectedVoucherSerie = 0; 
                });
            }
        });
    }

    private loadAccountingOrderReportId(): ng.IPromise<any> {
        return this.reportService.getStandardReportId(SettingMainType.Company, CompanySettingType.AccountingDefaultAccountingOrder, SoeReportTemplateType.VoucherList).then((x) => {
            this.accountingOrderReportId = x;
        });
    }

    private printSelectedVouchers(): void {
        if (this.gridAg.options.getSelectedCount() > 0) {
            const ids = this.gridAg.options.getSelectedIds("voucherHeadId");
            this.isPrinting = true;
            this.requestReportService.printVoucherList(ids)
            .then(() => {
                this.isPrinting = false;
            });

        }
    }

    private initDeleteSelectedVouchers() {
        // Removed after discussion in sprint 81-1
        this.deleteSelectedVouchers();

        /*if (this.gridAg.options.getSelectedCount() > 0) {
            // Show verification dialog
            var keys: string[] = [
                "core.warning",
                "economy.accounting.voucher.deleteselectedvoucherswarning"
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms["core.warning"], terms["economy.accounting.voucher.deleteselectedvoucherswarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(val => {
                    if (val) {
                        this.deleteSelectedVouchers();
                    }
                });
            });
        }*/
    }

    private deleteSelectedVouchers() {
        this.progress.startDeleteProgress((completion) => {
            const rows = this.gridAg.options.getSelectedRows();
            const ids = [];
            _.forEach(rows, (row) => {
                ids.push(row.voucherHeadId);
            });

            this.accountingService.deleteVouchersOnlySuperSupport(ids).then((result) => {
                completion.completed(null, false, result.stringValue);
            }, error => {
                completion.failed(error.message);
            });
        }).then(x => {
            this.loadGridData();
        });
    }

    public updateVoucherSelection() {
        this.coreService.saveIntSetting(SettingMainType.User, UserSettingType.VoucherSeriesSelection, this.selectedVoucherSerie).then((x) => {
            this.reloadGridFromFilter();
        });
    }

    private gridSelectionChanged() {
        this.$scope.$applyAsync(() => {
            this.gridHasSelectedRows = (this.gridAg.options.getSelectedCount() > 0);
        });
    }
}
