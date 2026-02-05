import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { Feature } from "../../../Util/CommonEnumerations";
import { IconLibrary, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { ExportUtility } from "../../../Util/ExportUtility";
import { StringUtility } from "../../../Util/StringUtility";
import { ToolBarButton, ToolBarUtility } from "../../../Util/ToolBarUtility";
import { IExportService } from "../ExportService";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    // Selections
    private selectedDateFrom: Date;
    private selectedDateTo: Date;
    
    //@ngInject
    constructor(
        private exportService: IExportService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private notificationService: INotificationService) {
        super(gridHandlerFactory, "economy.export.saft", progressHandlerFactory, messagingHandlerFactory)

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(response => this.onPermissionsLoaded(response))
            .onSetUpGrid(() => this.setupGrid())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onLoadGridData(() => this.search());


        this.selectedDateFrom = CalendarUtility.getDateToday().beginningOfMonth();
        this.selectedDateTo = CalendarUtility.getDateToday().endOfMonth();
    }

    public onInit(parameters: any) {

        this.flowHandler.start([
            { feature: Feature.Economy_Export_SAFT, loadReadPermissions: true, loadModifyPermissions: true },
        ]);
    }
    private onPermissionsLoaded(permissions: IPermissionRetrievalResponse) {
        this.readPermission = permissions[Feature.Economy_Export_SAFT].readPermission;
        this.modifyPermission = permissions[Feature.Economy_Export_SAFT].modifyPermission;
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.search());

        this.toolbar.addButtonGroup(ToolBarUtility.createGroup(new ToolBarButton("economy.reports.createfile", "economy.reports.createfile", IconLibrary.FontAwesome, "fal fa-download", () => {
            this.createExportFile();
        }, () => {
            return (!this.selectedDateFrom || !this.selectedDateTo);
        })));

        this.toolbar.addInclude(this.urlHelperService.getViewUrl("searchHeader.html"));
    }

    private setupGrid() {
        this.gridAg.options.enableRowSelection = false;
        const translationKeys: string[] = [
            "economy.export.saft.vouchernr",
            "common.date",
            "economy.export.saft.accountno",
            "common.text",
            "economy.accounting.account",
            "common.debit",
            "common.credit",
            "economy.accounting.account.vatdeduction",
            "economy.export.saft.vatcode",
            "economy.common.paymentmethods.customernr",
            "economy.supplier.invoice.liquidityplanning.suppliernr",
            "economy.export.saft.vatamount",
            "common.sum",
            "common.name"
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {
            this.gridAg.addColumnNumber("voucherNr", terms["economy.export.saft.vouchernr"], null, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnDate("date", terms["common.date"], null, false, null, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("accountNr", terms["economy.export.saft.accountno"], null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("accountName", terms["economy.accounting.account"], null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("voucherText", terms["common.text"], null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnNumber("debetAmount", terms["common.debit"], null, { enableRowGrouping: true, enableHiding: false, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("creditAmount", terms["common.credit"], null, { enableRowGrouping: true, enableHiding: false, aggFuncOnGrouping: 'sum' });
            this.gridAg.addColumnNumber("vatRate", terms["economy.accounting.account.vatdeduction"], null, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("customerId", terms["economy.common.paymentmethods.customernr"], null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("supplierId", terms["economy.supplier.invoice.liquidityplanning.suppliernr"], null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnText("supplierCustomerName", terms["common.name"], null, false, { enableRowGrouping: true, enableHiding: false });
            
            this.gridAg.addColumnText("vatCode", terms["economy.export.saft.vatcode"], null, false, { enableRowGrouping: true, enableHiding: false });
            this.gridAg.addColumnNumber("taxAmount", terms["economy.export.saft.vatamount"], null, { enableRowGrouping: true, enableHiding: false, aggFuncOnGrouping: 'sum' });

            this.gridAg.options.useGrouping(true, true, {
                keepColumnsAfterGroup: false, selectChildren: true, keepGroupState: true, groupSelectsFiltered: true, totalTerm: terms["common.sum"]
            });

            this.gridAg.finalizeInitGrid("economy.export.saft", true);
        });
    }

    public search() {
        if (!this.selectedDateFrom || !this.selectedDateTo)
            return;

        this.progress.startLoadingProgress([() => {
            return this.exportService.getSAFTTransactions(this.selectedDateFrom, this.selectedDateTo).then((x) => {
                this.setData(x);
                this.gridAg.options.setAllGroupExpended(true, 0);
            });
        }]);
    }

    private createExportFile() {
        this.progress.startWorkProgress((completion) => {
            this.exportService.getSAFTExportFile(this.selectedDateFrom, this.selectedDateTo).then((result) => {
                if (result.success) {
                    if (!StringUtility.isEmpty(result.stringValue)) {
                        ExportUtility.Export(result.stringValue, "SAFT.xml");
                    }
                    else {
                        const keys: string[] = [
                            "core.info",
                            "core.noresultfromselection"
                        ];

                        this.translationService.translateMany(keys).then((terms) => {
                            this.notificationService.showDialog(terms["core.info"], terms["core.noresultfromselection"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                        });
                    }

                    completion.completed("", true, result.infoMessage);
                }
                else {
                    completion.failed(result.errorMessage);
                }
            });
        });
    }
}