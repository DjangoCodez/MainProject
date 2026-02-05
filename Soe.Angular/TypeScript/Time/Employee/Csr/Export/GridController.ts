import { IEmployeeService } from "../../EmployeeService";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { Feature, TermGroup } from "../../../../Util/CommonEnumerations";
import { EmployeeCSRExportDTO } from "../../../../Common/Models/EmployeeUserDTO";
import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/controllerflowhandlerfactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { IGridHandlerFactory } from "../../../../Core/Handlers/gridhandlerfactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/messaginghandlerfactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/progresshandlerfactory";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Lookups 
    itemsSelectionDict: any[];
    itemsLoaded: boolean;
    // Footer & header
    private gridFooterComponentUrl: any;
    private gridHeaderComponentUrl: any;
    selectedYear: number = 0;
    // Functions
    buttonFunctions: any = [];
    //Dataset
    allCsrRows: EmployeeCSRExportDTO[];
    //properties
    private executing: boolean;
    private _itemsSelection: any;

    get itemsSelection() {
        return this._itemsSelection;
    }
    set itemsSelection(item: any) {
        this._itemsSelection = item;
        if (this.itemsLoaded == true)
            this.updateItemsSelection();
    }
    private get year(): number {
        return new Date().getFullYear() + (this.selectedYear === 0 ? 0 : 1);
    }

    //@ngInject
    constructor(
        private urlHelperService: IUrlHelperService,
        private translationService: ITranslationService,
        private employeeService: IEmployeeService,
        private notificationService: INotificationService,
        private coreService: ICoreService,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,

        controllerFlowHandlerFactory: IControllerFlowHandlerFactory) {

        super(gridHandlerFactory, "Time.Employee.Csr.Export", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onDoLookUp(() => this.loadSelectionTypes())
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())

        //If december then default = next year
        if (new Date().getMonth() === 11)
            this.selectedYear = 1;
        // Setup footer information
        this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("gridFooter.html");
        this.gridHeaderComponentUrl = this.urlHelperService.getViewUrl("gridHeader.html");
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.guid = parameters.guid;
        this.isHomeTab = !!parameters.isHomeTab;

        if (this.isHomeTab) {
            this.messagingHandler.onGridDataReloadRequired(x => { this.reloadData(); });
        }

        this.flowHandler.start({ feature: Feature.Time_Employee_Csr_Export, loadReadPermissions: true, loadModifyPermissions: true });
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Time_Employee_Csr_Export].readPermission;
        this.modifyPermission = response[Feature.Time_Employee_Csr_Export].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    public setUpGrid() {
        // Columns
        var keys: string[] = [
            "time.employee.employeenumber",
            "time.employee.csr.personnumber",
            "time.employee.name",
            "time.employee.csr.exportdate",
            "time.employee.csr.importdate",
            "common.message"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.gridAg.addColumnText("employeeNr", terms["time.employee.employeenumber"], 55);
            this.gridAg.addColumnText("employeeSocialSec", terms["time.employee.csr.personnumber"], 85);
            this.gridAg.addColumnText("employeeName", terms["time.employee.name"], 125);
            this.gridAg.addColumnDate("csrExportDate", terms["time.employee.csr.exportdate"], 85);
            this.gridAg.addColumnDate("csrImportDate", terms["time.employee.csr.importdate"], 85);
            this.gridAg.addColumnText("message", terms["common.message"], 125);

            this.gridAg.options.enableRowSelection = true;
            this.gridAg.finalizeInitGrid("time.employee.csr.csrs", true); //.csrs

        });
    }
    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.CsrGridSelection, false, false).then((x) => {
            this.itemsSelectionDict = x;
            this.itemsLoaded = true;
            this.itemsSelection = 3;
        });
    }

    public loadGridData() {
        this.progress.startLoadingProgress([() => {
            return this.employeeService.getEmployeesForCsrExport(this.year)
                .then((x) => {
                    //this.allCsrRows = x;
                    this.setData(x);
                });
        }]);
    }
    public updateGridData(csrResponses: any[]) {
        // Load data
        this.employeeService.getEmployeesForCsrExport(this.year).then((x) => {
            this.allCsrRows = x;
            _.forEach(this.allCsrRows, row => {
                var csrResponse = _.find(csrResponses, e => e.employeeId === row.employeeId);
                if (csrResponse) {
                    row["message"] = csrResponse.errorMessage;
                }
            });

            this.setData(x);

        });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg as IGridHandler, () => this.reloadData());
    }

    private reloadData() {
        this.loadGridData();
    }
    protected initDeleteRow(row) {
        var id: number = row['employeeId'];

        // Show verification dialog
        var keys: string[] = [
            "core.warning",
            "time.employee.csr.deletewarning"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            var message: string = terms["time.employee.csr.deletewarning"].format(row['employeenumber'], row['employeeName']);
            var modal = this.notificationService.showDialog(terms["core.warning"], message, SOEMessageBoxImage.Warning, SOEMessageBoxButtons.YesNo);
            modal.result.then(val => {
                if (val) {

                    this.progress.startDeleteProgress((completion) => {
                        this.employeeService.deleteCardNumber(id).then((result) => {
                            if (result.success) {
                                completion.completed(null);
                            } else {
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

    public getCsrInquiries() {
        var ids: number[] = [];
        var unvalid: number = 0;

        var rows = this.gridAg.options.getSelectedRows();
        if (rows.length >= 1) {
            _.forEach(rows, (y: any) => {
                if (y.employeeSocialSec) {
                    var employeeSocialSec: string = y.employeeSocialSec;
                    var employeeId = y.employeeId;

                    employeeSocialSec = employeeSocialSec.split("-").join("");
                    if (employeeSocialSec.length == 12) {
                        ids.push(employeeId);
                    } else {
                        unvalid++;
                    }
                } else {
                    unvalid++;
                }
            });

            if (unvalid > 0) {
                var keys: string[] = [
                    "time.employee.csr.unvalidpersonnr",
                    "time.employee.csr.unvalidpersonnrquantity"
                ];

                this.translationService.translateMany(keys).then((terms) => {
                    this.notificationService.showDialog(terms["core.information"], terms["time.employee.csr.unvalidpersonnr"] + unvalid.toString() + terms["time.employee.csr.unvalidpersonnrquantity"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OK);
                });
            }

            this.employeeService.getCsrResponses(ids, this.year).then(result => {
                if (result && result.length > 0) {
                    let msg: string = '';
                    _.forEach(_.filter(result, r => r.errorMessage), res => {
                        msg += res.errorMessage + '\n';
                    });

                    if (msg)
                        this.notificationService.showDialogEx("", msg, SOEMessageBoxImage.Error);

                    this.updateGridData(result);
                }
            });
        }
    }
    private updateItemsSelection() {
        var filteredRows: any = [];
        var today = new Date();
        switch (this.itemsSelection) {
            case 0:
                this.setData(this.allCsrRows);
                break;
            case 1:
                _.forEach(this.allCsrRows, row => {
                    if (row.csrExportDate == null) {
                        filteredRows.push(row);
                    }
                });
                this.setData(filteredRows);
                break;
            case 2:
                _.forEach(this.allCsrRows, row => {
                    if (row.csrExportDate != null && row.year == today.getFullYear())
                        filteredRows.push(row);
                });
                this.setData(filteredRows);
                break;
            case 3:
                this.setData(this.allCsrRows);
                break;
            default:
                break;
        }
    }



}
