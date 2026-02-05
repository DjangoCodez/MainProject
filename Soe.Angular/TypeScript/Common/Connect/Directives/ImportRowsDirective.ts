import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { ICoreService } from "../../../Core/Services/CoreService";
import { IConnectService } from "../ConnectService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { GridControllerBaseAg } from "../../../Core/Controllers/GridControllerBaseAg";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { SoeGridOptionsEvent, SOEMessageBoxButtons, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { ImportGridColumnDTO } from "../../Models/ImportGridColumnDTO";
import { CustomerIODTO } from "../../Models/CustomerIODTO";
import { CustomerInvoiceIODTO } from "../../Models/CustomerInvoiceIODTO";
import { CustomerInvoiceRowIODTO } from "../../Models/CustomerInvoiceRowIODTO";
import { SupplierIODTO } from "../../Models/SupplierIODTO";
import { SupplierInvoiceHeadIODTO } from "../../Models/SupplierInvoiceHeadIODTO";
import { VoucherHeadIODTO } from "../../Models/VoucherHeadIODTO";
import { ProjectIODTO } from "../../Models/ProjectIODTO";
import { Feature, TermGroup_IOImportHeadType, TermGroup, TermGroup_IOStatus } from "../../../Util/CommonEnumerations";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { IColumnAggregations } from "../../../Util/SoeGridOptionsAg";
import { IRequestReportService } from "../../../Shared/Reports/RequestReportService";
import { IDownloadFileDTO } from "../../../Scripts/TypeLite.Net4";

export class ImportRowsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        
        return {
            
            templateUrl: urlHelperService.getGlobalUrl('Common/Connect/Directives/ImportRows.html'),
            scope: {                
                batchId: '=',
                importHeadType: '=',
                useAccountDistribution: '=',
                useAccountDimensions: '=',
                defaultDim1Account: '=?',
                defaultDim2Account: '=?',
                defaultDim3Account: '=?',
                defaultDim4Account: '=?',
                defaultDim5Account: '=?',
                defaultDim6Account: '=?'
            },
            restrict: 'E',
            replace: true,
            controller: ImportRowsDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true            
        };
    }
}

export class ImportRowsDirectiveController extends GridControllerBaseAg {
    // Setup    
    batchId: string;
    importHeadType: number;
    useAccountDistribution: boolean;
    useAccountDimensions: boolean;
    defaultDim1Account?: number;
    defaultDim2Account?: number;
    defaultDim3Account?: number;
    defaultDim4Account?: number;
    defaultDim5Account?: number;
    defaultDim6Account?: number;
    updateExistingInvoice: boolean;
    statusList: any[] = [];

    // Flags
    showPrintButton = false;

    // Data
    importRows: any[];
    
    // Collections
    gridColumns: ImportGridColumnDTO[];            
    

    protected isPrinting: boolean = false;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        protected coreService: ICoreService,        
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        private connectService: IConnectService,
        urlHelperService: IUrlHelperService,
        private $window: ng.IWindowService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope,
        private readonly requestReportService: IRequestReportService,) {

        super("Common.Connect.ImportRows", "Common.Connect.ImportRows", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);                                              
    }

    // SETUP    
    public $onInit() {                
        this.soeGridOptions.enableFiltering = true;        
        this.soeGridOptions.enableGridMenu = true;
        this.soeGridOptions.enableRowSelection = true;  
        this.soeGridOptions.ignoreResizeToFit = true;

        if (this.importHeadType == TermGroup_IOImportHeadType.CustomerInvoice ||            
            this.importHeadType == TermGroup_IOImportHeadType.Voucher)
            this.showPrintButton = true;  
    }


    private setupWatchers() {        
        this.$scope.$watch(() => this.batchId, () => {                        
        });       
    }

    protected setupCustomToolBar() {
                                
        //var group = ToolBarUtility.createGroup(new ToolBarButton("core.print", "core.print", IconLibrary.FontAwesome, "fa-print", () => {
        //    this.print();
        //}));

        //this.buttonGroups.push(group);                    
    }

    public setupGrid() {
        this.$q.all([this.loadStatus(), this.loadColumns()]).then(() => { this.loadImportIOResult() })        
    }

    private loadStatus(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.IOStatus, false, false).then((x) => {            
            this.statusList = x;            
        });
    }

    private loadColumns(): ng.IPromise<any> {
        return this.connectService.getImportGridColumns(this.importHeadType).then((x) => {
            this.gridColumns = x;
            this.setupGridColumns();
        });
    }

    private setupGridColumns() {
        const keys: string[] = [
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "core.aggrid.totals.selected"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            const ignoredColumns: string[] = ["status", "statusName", "errorMessage", "actorCustomerId", "customerIoId", "customerInvoiceHeadIoId", "invoiceId", "supplierIoId", "supplierInvoiceHeadIoId", "supplierId", "voucherHeadIoId", "actorCompanyId", "import", "importId", "type", "source", "batchId", "state", "created", "createdBy", "modified", "modifiedBy", "entityState", "entityKey"];
            
            this.soeGridOptions.addColumnIsModified("isModified", "", 20);
            this.soeGridOptions.addColumnText("statusName", "Statusname", 40);
            this.soeGridOptions.addColumnText("errorMessage", "Errormessage", 40);
            
            const events: GridEvent[] = [];
            events.push(new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef, newValue, oldValue) => { this.afterCellEdit(entity, colDef, newValue, oldValue); }));
            this.soeGridOptions.subscribe(events);

            var aggregations = {};
            for (var i = 0; i < this.gridColumns.length; i++) {
                var gridColumn = this.gridColumns[i];   
                var column: any;                
                                
                if (ignoredColumns.indexOf(gridColumn.columnName) > -1)
                    continue;
                
                if (gridColumn.columnType == "decimal") {
                    column = this.addColumnNumber(gridColumn.columnName, gridColumn.headerName, 20, { decimals: 2 });
                    aggregations[gridColumn.columnName] = "sum";
                }
                else if (gridColumn.columnType == "int32") 
                    column = this.addColumnNumber(gridColumn.columnName, gridColumn.headerName, 20);                                                                    
                else if (gridColumn.columnType == "datetime") 
                    column = this.addColumnDate(gridColumn.columnName, gridColumn.headerName, 20);                                                                            
                else if (gridColumn.columnType == "boolean") 
                    column = this.addColumnBool(gridColumn.columnName, gridColumn.headerName, 5);                                                                            
                else 
                    column = this.addColumnText(gridColumn.columnName, gridColumn.headerName, 40);                                              
                
                column.editable = this.isEditable(gridColumn.columnName);                    
            } 

           
            var rows = this.importRows ? this.importRows.length : 0;
            if (rows < 10)
                rows = 10;
            if (rows > 30)
                rows = 30;

            this.soeGridOptions.setMinRowsToShow(rows);

            this.soeGridOptions.getColumnDefs().forEach(col => {
                var cellcls: string = col.cellClass ? col.cellClass.toString() : "";
                col.cellClass = (grid: any) => {
                    if (grid.data['status'] === TermGroup_IOStatus.Error)
                        return cellcls + " errorRow";
                    else
                        return cellcls;
                }
            });

            this.soeGridOptions.finalizeInitGrid();   

            this.$timeout(() => {
                this.soeGridOptions.addTotalRow("#totals-grid", {
                    filtered: terms["core.aggrid.totals.filtered"],
                    total: terms["core.aggrid.totals.total"],
                    selected: terms["core.aggrid.totals.selected"]
                });

                this.soeGridOptions.addFooterRow("#sum-footer-grid", aggregations as IColumnAggregations);
            });                    
        });
    }

    private afterCellEdit(row: any, colDef: uiGrid.IColumnDef, newValue, oldValue) {
        if (newValue === oldValue)
            return;

        row['isModified'] = true;
        this.soeGridOptions.refreshRows(row);
    }

    private isEditable(columnName: string): boolean {

        var isEditable: boolean = true;
        var readOnlyColumns: string[] = ["status", "statusName", "errorMessage"];               
        
        if (readOnlyColumns.indexOf(columnName) > -1)
            isEditable = false;

        return isEditable;
    }    

    private loadImportIOResult(): ng.IPromise<any> {
        const deferral = this.$q.defer();        
        // Load data
        this.connectService.getImportIOResult(this.importHeadType, this.batchId).then((x) => {
            for (var i = 0; i < x.length; i++) {                
                var status = _.find(this.statusList, { id: x[i].status });                
                x[i].statusName = status.name;
            }
            this.importRows = x;            
            this.soeGridOptions.setData(this.importRows);       
            this.progressBusy = false;
            deferral.resolve();
        })

        return deferral.promise;  
    }

    private gridAndDataIsReady() {
        this.setupGridColumns();
        this.setupWatchers();
        
    }

    public reloadData() {
        this.loadImportIOResult();
    }

    public save() {        

        const keys: string[] = [
            "common.connect.savenotsuccess",
            "core.error"
        ];

        this.translationService.translateMany(keys).then((terms) => {

            var modifiedRows = _.filter(this.importRows, { isModified: true });

            if (this.importHeadType == TermGroup_IOImportHeadType.Customer) {
                //Customers
                var customerDTO: CustomerIODTO[] = [];

                for (var i = 0; i < modifiedRows.length; i++) {
                    var row = modifiedRows[i];
                    customerDTO.push(row);
                }

                this.connectService.SaveCustomerIODTO(customerDTO).then((result) => {
                    if (result.success) {
                        this.reloadData();
                    }
                    else {
                        this.notificationService.showDialog(terms["core.error"], terms["common.connect.savenotsuccess"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    }
                });
            }
            else if (this.importHeadType == TermGroup_IOImportHeadType.CustomerInvoice) {
                //Customer invoice heads
                var customerInvoiceDTO: CustomerInvoiceIODTO[] = [];

                for (var i = 0; i < modifiedRows.length; i++) {
                    var row = modifiedRows[i];
                    customerInvoiceDTO.push(row);
                }

                this.connectService.SaveCustomerInvoiceHeadIODTO(customerInvoiceDTO).then((result) => {
                    if (result.success) {
                        this.reloadData();
                    }
                    else {
                        this.notificationService.showDialog(terms["core.error"], terms["common.connect.savenotsuccess"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    }
                })
            }
            else if (this.importHeadType == TermGroup_IOImportHeadType.CustomerInvoiceRow) {
                //Customer invoice rows
                var customerInvoiceRowDTO: CustomerInvoiceRowIODTO[] = [];

                for (var i = 0; i < modifiedRows.length; i++) {
                    var row = modifiedRows[i];
                    customerInvoiceRowDTO.push(row);
                }

                this.connectService.SaveCustomerInvoiceRowIODTO(customerInvoiceRowDTO).then((result) => {
                    if (result.success) {
                        this.reloadData();
                    }
                    else {
                        this.notificationService.showDialog(terms["core.error"], terms["common.connect.savenotsuccess"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    }
                })
            }
            else if (this.importHeadType == TermGroup_IOImportHeadType.Supplier) {
                //Suppliers
                var supplierDTO: SupplierIODTO[] = [];

                for (var i = 0; i < modifiedRows.length; i++) {
                    var row = modifiedRows[i];
                    supplierDTO.push(row);
                }

                this.connectService.SaveSupplierIODTO(supplierDTO).then((result) => {
                    if (result.success) {
                        this.reloadData();
                    }
                    else {
                        this.notificationService.showDialog(terms["core.error"], terms["common.connect.savenotsuccess"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    }
                })
            }
            else if (this.importHeadType == TermGroup_IOImportHeadType.SupplierInvoice || this.importHeadType == TermGroup_IOImportHeadType.SupplierInvoiceAnsjo) {
                //Supplier invoice heads
                var supplierInvoiceDTO: SupplierInvoiceHeadIODTO[] = [];

                for (var i = 0; i < modifiedRows.length; i++) {
                    var row = modifiedRows[i];
                    supplierInvoiceDTO.push(row);
                }

                this.connectService.SaveSupplierInvoiceHeadIODTO(supplierInvoiceDTO).then((result) => {
                    if (result.success) {
                        this.reloadData();
                    }
                    else {
                        this.notificationService.showDialog(terms["core.error"], terms["common.connect.savenotsuccess"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    }
                })
            }
            else if (this.importHeadType == TermGroup_IOImportHeadType.Voucher) {
                //Vouchers
                var voucherDTO: VoucherHeadIODTO[] = [];

                for (var i = 0; i < modifiedRows.length; i++) {
                    var row = modifiedRows[i];
                    voucherDTO.push(row);
                }

                this.connectService.SaveVoucherHeadIODTO(voucherDTO).then((result) => {
                    if (result.success) {
                        this.reloadData();
                    }
                    else {
                        this.notificationService.showDialog(terms["core.error"], terms["common.connect.savenotsuccess"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    }
                })
            }
            else if (this.importHeadType == TermGroup_IOImportHeadType.Project) {
                //Projects
                var projectDTO: ProjectIODTO[] = [];

                for (var i = 0; i < modifiedRows.length; i++) {
                    var row = modifiedRows[i];
                    projectDTO.push(row);
                }

                this.connectService.SaveProjectIODTO(projectDTO).then((result) => {
                    if (result.success) {
                        this.reloadData();
                    }
                    else {
                        this.notificationService.showDialog(terms["core.error"], terms["common.connect.savenotsuccess"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    }
                })
            }

        });
    }

    private initImportSelectedRows() {
        if (this.useAccountDimensions) {
            var keys: string[] = [
                "core.warning",
                "common.connect.accountdimwarning",
            ];
            this.translationService.translateMany(keys).then((terms) => {
                var modal = this.notificationService.showDialog(terms[""], terms["common.connect.accountdimwarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
                modal.result.then(
                    (val) => {
                        this.importSelectedRows();
                    },
                    (cancel) => {
                        return;
                    })
            });
        }
        else {
            this.importSelectedRows();
        }
    }

    private importSelectedRows() {               

        const keys: string[] = [
            "common.obs",
            "common.connect.unsavedchangesmessage",
            "common.connect.importdata",
            "common.connect.importsuccess",
            "common.connect.importnotsuccess",
            "core.info",
            "core.error"
        ];

        this.translationService.translateMany(keys).then((terms) => {             

            var modifiedRows = _.filter(this.importRows, { isModified: true });

            if (modifiedRows.length > 0) {
                this.notificationService.showDialog(terms["common.obs"], terms["common.connect.unsavedchangesmessage"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            }
            else {
                this.startWork();

                var selectedRows = this.soeGridOptions.getSelectedRows();

                var ioIds: any[] = [];
                for (var i = 0; i < selectedRows.length; i++) {
                    var row = selectedRows[i];

                    if (this.importHeadType == TermGroup_IOImportHeadType.Customer)
                        ioIds.push(row.customerIOId);
                    else if (this.importHeadType == TermGroup_IOImportHeadType.CustomerInvoice)
                        ioIds.push(row.customerInvoiceHeadIOId);
                    else if (this.importHeadType == TermGroup_IOImportHeadType.CustomerInvoiceRow)
                        ioIds.push(row.customerInvoiceRowIOId);
                    else if (this.importHeadType == TermGroup_IOImportHeadType.Supplier)
                        ioIds.push(row.supplierIOId);
                    else if (this.importHeadType == TermGroup_IOImportHeadType.SupplierInvoice ||
                        this.importHeadType == TermGroup_IOImportHeadType.SupplierInvoiceAnsjo)
                        ioIds.push(row.supplierInvoiceHeadIOId);
                    else if (this.importHeadType == TermGroup_IOImportHeadType.Voucher)
                        ioIds.push(row.voucherHeadIOId);
                    else if (this.importHeadType == TermGroup_IOImportHeadType.Project)
                        ioIds.push(row.projectIOId);
                }

                this.connectService.ImportIO(this.importHeadType, ioIds, this.useAccountDistribution, this.useAccountDimensions, this.defaultDim1Account, this.defaultDim2Account, this.defaultDim3Account, this.defaultDim4Account, this.defaultDim5Account, this.defaultDim6Account).then((result) => {
                    if (result.success) {
                        this.completedWork(result);
                        this.notificationService.showDialog(terms["core.info"], terms["common.connect.importsuccess"], SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                        this.reloadData();
                    }
                    else {
                        this.completedWork(result);
                        this.notificationService.showDialog(terms["core.error"], terms["common.connect.importnotsuccess"] + "\n" + result.errorMessage, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                    }
                });

            }

        });
    }

    private print(): void {

        const selectedRows = this.soeGridOptions.getSelectedRows();

        if (selectedRows.length) {
            if (this.importHeadType === TermGroup_IOImportHeadType.CustomerInvoice) {
                const ioIds: any[] = selectedRows.map(row => row.customerInvoiceHeadIOId);
                this.isPrinting = true;
                this.requestReportService
                    .printIOCustomerInvoice(ioIds)
                    .then((fileDataResult: IDownloadFileDTO) => {
                        this.handlePrintResponse(fileDataResult);
                    });

            } else if (this.importHeadType === TermGroup_IOImportHeadType.Voucher) {
                const ioIds: any[] = selectedRows.map(row => row.voucherHeadIOId);
                this.isPrinting = true;
                this.requestReportService
                    .printIOVoucher(ioIds)
                    .then((fileDataResult: IDownloadFileDTO) => {
                        this.handlePrintResponse(fileDataResult);
                    });
            }
        }
    }

    private handlePrintResponse(fileDataResult: IDownloadFileDTO): void {
        if (!fileDataResult.success && !fileDataResult.errorMessage) {
            this.printError();
        }

        this.isPrinting = false;
    }

    private printError(): void {
        const keys: string[] = [
            "common.connect.printerror",
            "core.error"
        ];

        this.translationService.translateMany(keys).then((terms) => {

            this.notificationService.showDialog(
                terms["core.error"], 
                terms["common.connect.printerror"], 
                SOEMessageBoxImage.Error, 
                SOEMessageBoxButtons.OK);
        });
    }
}