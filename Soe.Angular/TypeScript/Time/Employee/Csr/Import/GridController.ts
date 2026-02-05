import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { IEmployeeService } from "../../EmployeeService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { Feature, TermGroup } from "../../../../Util/CommonEnumerations";
import { EmployeeCSRExportDTO } from "../../../../Common/Models/EmployeeUserDTO";
import { IEmployeeTaxSEDTO } from "../../../../Scripts/TypeLite.Net4";

export class GridController extends GridControllerBase {
    // Lookups 
    itemsSelectionDict: any[];
    itemsLoaded: boolean;
    // Footer & header
    gridFooterComponentUrl: any;
    toolbarInclude: any;

    // Functions
    buttonFunctions: any = [];

    //Dataset
    allCsrRows: EmployeeCSRExportDTO[];

    //properties
    private _itemsSelection: any;
    get itemsSelection() {
        return this._itemsSelection;
    }
    set itemsSelection(item: any) {
        this._itemsSelection = item;
        if (this.itemsLoaded == true)
            this.updateItemsSelection();
    }

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private employeeService: IEmployeeService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants) {

        super("Time.Employee.Csr.Import", "time.employee.csr.imports", Feature.Time_Employee_Csr_Import, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        this.toolbarInclude = urlHelperService.getViewUrl("gridHeader.html");
        // Setup footer information
        this.gridFooterComponentUrl = this.urlHelperService.getViewUrl("gridFooter.html");
    }

    protected loadLookups() {
        this.loadSelectionTypes();
    }

    public setupGrid() {
        // Columns
        var keys: string[] = [
            "time.employee.employeenumber",
            "time.employee.csr.personnumber",
            "time.employee.name",
            "time.employee.csr.exportdate",
            "time.employee.csr.importdate"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            super.addColumnText("employeeNr", terms["time.employee.employeenumber"], "15%");
            super.addColumnText("employeeSocialSec", terms["time.employee.csr.personnumber"], null);
            super.addColumnText("employeeName", terms["time.employee.name"], "40%");
            super.addColumnDate("csrExportDate", terms["time.employee.csr.exportdate"], null);
            super.addColumnDate("csrImportDate", terms["time.employee.csr.importdate"], null);
        });
    }

    public loadGridData() {
        // Load data
        this.employeeService.getEmployeesForCsrExport(new Date().getFullYear()).then((x) => {
            this.allCsrRows = x;
            super.gridDataLoaded(x);
        });
    }

    private loadSelectionTypes(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.CsrGridSelection, true, false).then((x) => {
            this.itemsSelectionDict = x;
            this.itemsLoaded = true;
        });
    }

    public createFile() {
        // create exportfile
        var rows = this.soeGridOptions.getSelectedRows();
        var emploeyeeTaxSe: IEmployeeTaxSEDTO;

        if (rows.length >= 1) {
            _.forEach(rows, (y: any) => {
                if (y.employeeSocialSec) {
                    var employeeSocialSec = y.employeeSocialSec;
                    var employeeId = y.employeeId;

                    employeeSocialSec = employeeSocialSec.split("-").join("");
                    if (employeeSocialSec) {
                        this.employeeService.getEmployeeTax(employeeSocialSec).then((x) => {
                            emploeyeeTaxSe = x;
                            if (emploeyeeTaxSe) {
                                emploeyeeTaxSe.csrExportDate = new Date();
                                this.employeeService.saveEmployeeTaxSe(emploeyeeTaxSe.employeeTaxId, employeeId);
                            }
                        });
                    }
                }
            });
        }
    }

    public updateItemsSelection() {
        var filteredRows: any = [];
        var today = new Date();
        switch (this.itemsSelection) {
            case 0:
                super.gridDataLoaded(this.allCsrRows);
                break;
            case 1:
                _.forEach(this.allCsrRows, row => {
                    if (row.csrExportDate == null) {
                        filteredRows.push(row);
                    }
                });
                super.gridDataLoaded(filteredRows);
                break;
            case 2:
                _.forEach(this.allCsrRows, row => {
                    if (row.csrExportDate != null && row.year == today.getFullYear())
                        filteredRows.push(row);
                });
                super.gridDataLoaded(filteredRows);
                break;
            case 3:
                super.gridDataLoaded(this.allCsrRows);
                break;
            default:
                break;
        }
    }
}
