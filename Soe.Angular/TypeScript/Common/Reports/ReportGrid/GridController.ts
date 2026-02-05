import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IReportService } from "../../../Core/Services/ReportService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { CoreUtility } from "../../../Util/CoreUtility";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { ReportTransferButtonFunctions, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { Feature, SoeModule, SoeReportTemplateType, SoeEntityType, SoeEntityImageType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IActionResult } from "../../../Scripts/TypeLite.Net4";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Permissions
    reportTransferPermission = false;
    private language = 2;
    // Functions
    buttonFunctions: any = [];

    // Grid header and footer        
    gridFooterComponentUrl: any;

    //@ngInject
    constructor(private reportService: IReportService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        private $window: ng.IWindowService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory) {
        super(gridHandlerFactory, "Common.Reports.ReportGrid", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded((response) => this.onAllPermissionsLoaded(response))
            .onSetUpGrid(() => this.setupGrid())
            .onLoadGridData(() => this.loadGridData())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
    }

    onInit(parameters: any) {
        this.parameters = parameters;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(() => { this.loadGridData(); });
        }

        
        this.setInfoText();
        this.flowHandler.start([{ feature: Feature.Economy_Distribution_Reports_Selection, loadReadPermissions: true, loadModifyPermissions: true },
        { feature: Feature.Economy_Distribution_ReportTransfer, loadReadPermissions: false, loadModifyPermissions: true }]);
    }

    private setInfoText() {
        if (soeConfig.sysCountryId == 1) {
            this.language = 1;
        }
        else if (soeConfig.sysCountryId == 3) {
            this.language = 3;
        }
        else {
            this.language = 2;
        }
    }

    private onAllPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Economy_Distribution_Reports_Selection].readPermission;
        this.modifyPermission = response[Feature.Economy_Distribution_Reports_Selection].modifyPermission;
        this.reportTransferPermission = response[Feature.Economy_Distribution_ReportTransfer].modifyPermission;

        if (this.modifyPermission) {
            // Send messages to TabsController
            this.messagingHandler.publishActivateAddTab();
        }

        if (this.reportTransferPermission && soeConfig.module == SoeModule.Economy && !soeConfig.reportPackageId) {
            this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("gridFooter.html");


            const keys: string[] = [
                "common.reports.reporttransfer.export",
                "common.reports.reporttransfer.import"
            ];

            return this.translationService.translateMany(keys).then((terms) => {
                this.buttonFunctions.push({ id: ReportTransferButtonFunctions.Export, name: terms["common.reports.reporttransfer.export"] });
                this.buttonFunctions.push({ id: ReportTransferButtonFunctions.Import, name: terms["common.reports.reporttransfer.import"] });
            });
        }
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
        this.toolbar.addInclude(this.urlHelperService.getViewUrl("gridHeader.html"));
    }

    public setupGrid() {
        // Columns
        const keys: string[] = [
            "common.number",
            "common.type",
            "common.name",
            "common.description",
            "common.report.report.selectionname",
            "common.report.report.standard",
            "common.report.report.roles",
            "common.report.report.print",
            "common.report.selection.exporttype",
            "core.edit",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("reportNr", terms["common.number"], 100);
            this.gridAg.addColumnText("sysReportTypeName", terms["common.type"], null);
            this.gridAg.addColumnText("reportName", terms["common.name"], null);
            this.gridAg.addColumnText("reportDescription", terms["common.description"], null);
            this.gridAg.addColumnText("reportSelectionText", terms["common.report.report.selectionname"], null);
            this.gridAg.addColumnBool("standard", terms["common.report.report.standard"], 60);
            this.gridAg.addColumnText("roleNames", terms["common.report.report.roles"], null);
            this.gridAg.addColumnText("exportTypeName", terms["common.report.selection.exporttype"], null);

            if (soeConfig.reportSelectionPermission)
                this.gridAg.addColumnIcon(null, null, 60, { icon: "fal fa-print", onClick: this.print.bind(this), showIcon: this.showPrintIcon.bind(this) });// "print", "selectionType != 0");

            if (this.modifyPermission)
                this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.finalizeInitGrid("common.report.report.reports", true);
        });

    }

    private showPrintIcon(row): boolean {
        return true;
    }

    public loadGridData() {
        // Load data            
        this.progress.startLoadingProgress([() => {
            if (!soeConfig.reportPackageId) {
                return this.reportService.getReportViewsForModule(soeConfig.module, false, false).then((data => {
                    _.forEach(data, (x: any) => {
                        x.name = x.reportName;
                    });
                    this.setData(_.orderBy(data, 'reportNr'));
                }));
            } else {
                return this.reportService.getReportViewsInPackage(soeConfig.reportPackageId).then((data) => {
                    _.forEach(data, (x: any) => {
                        x.name = x.reportName;
                    });
                    this.setData(_.orderBy(data, 'reportNr'));
                });
            }
        }]);
    }

    private reloadData() {
        this.loadGridData();
    }

    private executeButtonFunction(option) {

        switch (option.id) {
            case ReportTransferButtonFunctions.Export:
                this.exportReports();
                break;
            case ReportTransferButtonFunctions.Import:
                this.importReports();
                break;
        }
    }

    private exportReports() {

        const keys: string[] = [
            "common.reports.reporttransfer.noreportsselectedmessage",
            "common.reports.reporttransfer.beginimport",
            "core.warning"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            if (this.gridAg.options.getSelectedCount() === 0) {
                this.notificationService.showDialog(terms["core.warning"], terms["common.reports.reporttransfer.noreportsselectedmessage"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            } else {
                const modal = this.notificationService.showDialog(terms["core.warning"], terms["common.reports.reporttransfer.beginimport"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OKCancel);
                modal.result.then(result => {
                    const selectedItems = this.gridAg.options.getSelectedRows();

                    const reportIds: number[] = [];
                    _.forEach(selectedItems, (item) => {
                        reportIds.push(item.reportId);
                    })

                    const ediPdfReportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.ReportTransfer + "&reportidlist=" + reportIds;
                    window.open(ediPdfReportUrl, '_blank');
                });
            }
        });

    }

    private importReports() {

        const keys: string[] = [
            "core.fileupload.fileupload",
            "common.reports.reporttransfer.importfailedmessage",
        ];

        this.translationService.translateMany(keys).then((terms) => {
            const url = CoreUtility.apiPrefix + `${Constants.WEBAPI_CORE_FILES_UPLOAD}${SoeEntityType.None}/${SoeEntityImageType.Unknown}`;
            const modal = this.notificationService.showFileUpload(url, terms["core.fileupload.fileupload"], false, false, false);
            modal.result.then(res => {
                let result: IActionResult = res.result;
                this.progress.startWorkProgress((completion) => {
                    this.reportService.getReportsFromFile(result.integerValue).then((importResult) => {
                        if (importResult.success) {
                            completion.completed(importResult, false);
                            this.loadGridData();
                        }
                        else
                            completion.failed(terms["common.reports.reporttransfer.importfailedmessage"]);
                    })
                });

            });
        });
    }

    edit(row) {
        if (this.readPermission || this.modifyPermission)
            HtmlUtility.openInSameTab(this.$window, "/soe/" + SoeModule[soeConfig.module].toLowerCase() + "/distribution/reports/edit/?report=" + row.reportId);
    }

    rowDoubleClicked(row) {
        this.print(row);
    }

    print(row) {
        if (row.sysReportTemplateTypeId == SoeReportTemplateType.ProjectTransactionsReport) {
            this.messagingHandler.publishEditRow(row);
        } else {
            HtmlUtility.openInSameTab(this.$window, "/soe/" + SoeModule[soeConfig.module].toLowerCase() + "/distribution/reports/selection/?report=" + row.reportId);
        }
    }
}