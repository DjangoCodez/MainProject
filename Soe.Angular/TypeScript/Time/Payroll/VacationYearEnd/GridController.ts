import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { IPayrollService } from "../PayrollService";
import { CalendarUtility } from "../../../Util/CalendarUtility";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../Util/Enumerations";
import { HtmlUtility } from "../../../Util/HtmlUtility";
import { Feature, SoeReportTemplateType } from "../../../Util/CommonEnumerations";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/progresshandlerfactory";
import { IPermissionRetrievalResponse } from "../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../Core/Handlers/GridHandler";
import { VacationYearEndResultDialogController } from "../../Dialogs/VacationYearEndResult/VacationYearEndResultDialog";
import { IVacationYearEndEmployeeResultDTO } from "../../../Scripts/TypeLite.Net4";


export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Filters
    typeFilterOptions = [];
    vatTypeFilterOptions = [];

    //@ngInject
    constructor(
        protected $uibModal,
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private payrollService: IPayrollService,
        private notificationService: INotificationService,
        private $window: ng.IWindowService,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {
        super(gridHandlerFactory, "Time.Payroll.VacationYearEnd", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())

    }
    onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Payroll_VacationYearEnd, loadReadPermissions: true, loadModifyPermissions: true });
    }
    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Payroll_VacationYearEnd].readPermission;
        this.modifyPermission = response[Feature.Time_Payroll_VacationYearEnd].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }
    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }
    setUpGrid() {
        this.doubleClickToEdit = false;
        this.gridAg.options.enableRowSelection = false;

        // Columns
        var keys: string[] = [
            "common.date",
            "common.type",
            "common.content",
            "common.user",
            "common.created",
            "time.payroll.vacationyearend.pdf",
            "core.delete",
            "time.payroll.vacationyearend.vacationyearend.failed",
            "core.info"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnDate("date", terms["common.date"], 125);
            this.gridAg.addColumnText("contentTypeName", terms["common.type"], 124);
            this.gridAg.addColumnText("content", terms["common.content"], 100);
            this.gridAg.addColumnText("employeesFailed", terms["time.payroll.vacationyearend.vacationyearend.failed"], 100);
            this.gridAg.addColumnDate("created", terms["common.created"], 100, true);
            this.gridAg.addColumnText("createdBy", terms["common.user"], 100, true);
            this.gridAg.addColumnIcon(null, "", null, { toolTip: terms["core.info"], icon: "fal fa-info-circle infoColor", onClick: this.showInfo.bind(this) });
            this.gridAg.addColumnPdf(terms["time.payroll.vacationyearend.pdf"], this.showPdf.bind(this) );
            if (this.modifyPermission)
                this.gridAg.addColumnDelete(terms["core.delete"], this.initDeleteRow.bind(this));
            this.gridAg.finalizeInitGrid("time.payroll.vacationyearend.vacationyearendings", true);
        });
    }
    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.payrollService.getVacationYearEnds()
                .then((x) => {
                    _.forEach(x, (y: any) => {
                        var vacationDebtReportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.EmployeeVacationDebtReport + "&datastorageid=";

                        y.date = CalendarUtility.toFormattedDate(y.date);
                        y.created = CalendarUtility.toFormattedDate(y.created);
                        if (y.dataStorageId)
                            y.pdfUrl = vacationDebtReportUrl + y.dataStorageId;

                    });
                    return x;
                }).then(data => {
                    this.setData(data);
                    
                });
                
        }]);
    }
   
    private reloadData() {
        this.loadGridData();
    }
    protected initDeleteRow(row) {
        var id: number = row['vacationYearEndHeadId'];

        // Show verification dialog
        var keys: string[] = [
            "core.warning",
            "time.payroll.vacationyearend.deletewarning"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            var message: string = terms["time.payroll.vacationyearend.deletewarning"].format(row['employeenumber'], row['employeeName']);
            var modal = this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
            modal.result.then(val => {
                if (val) {

                    this.progress.startDeleteProgress((completion) => {

                       
                        this.payrollService.deleteVacationYearEnd(id).then((result) => {
                            if (result.success) {
                                completion.completed(null);
                            }
                            else {
                                completion.failed(result.errorMessage);
                            }
                        }, error => {
                            completion.failed(error.message);
                        });
                    });
                  
                }
            });
        });
    }
    private showInfo(row: any) {
        let employeeResults: IVacationYearEndEmployeeResultDTO[];
        this.payrollService.getVacationYearEndResult(row.vacationYearEndHeadId).then(x => {
            employeeResults = x.employeeResults;
           
            const options: angular.ui.bootstrap.IModalSettings = {
                templateUrl: this.urlHelperService.getGlobalUrl("Time/Dialogs/VacationYearEndResult/Views/VacationYearEndResultDialog.html"),
                controller: VacationYearEndResultDialogController,
                controllerAs: "ctrl",
                bindToController: true,
                backdrop: 'static',
                size: 'lg',
                windowClass: 'fullsize-modal',
                resolve: {
                    results: () => { return employeeResults },
                    date: () => { return row.date },
                    showVacationGroup: () => { return false },
                    header: () => { return '' }
                }
            }
            this.$uibModal.open(options);
        });
    }
    //PDF
    protected showPdf(row) {
        if (row && row.pdfUrl)
            HtmlUtility.openInSameTab(this.$window, row.pdfUrl);
        else {
            this.translationService.translate("time.payroll.vacationyearend.nopdfcreated").then((term) => {
                this.notificationService.showDialog("", term, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
        }
    }
}