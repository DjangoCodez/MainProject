import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { Feature } from "../../../../Util/CommonEnumerations";

export class ProjectProductsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Shared/Billing/Projects/Directives/ProjectProducts.html'),
            scope: {
                projectId: '=',
                readOnly: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: ProjectProductsDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class ProjectProductsDirectiveController extends GridControllerBase {
    // Setup
    private rows: any[];
    private readOnly: boolean;
    private projectId: number;

    // dims
    private dim2Header: any;
    private dim3Header: any;
    private dim4Header: any;
    private dim5Header: any;
    private dim6Header: any;

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        coreService: ICoreService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Billing.Projects.List.Directives.ProjectProducts", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = true;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.showGridFooter = false;
        this.soeGridOptions.setMinRowsToShow(8);
        this.setupTypeAhead();
    }

    protected setupCustomToolBar() {
        //this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.customer.customer.product.new", "common.customer.customer.product.new", IconLibrary.FontAwesome, "fa-plus",
        //    () => { this.addRow(); },
        //    null,
        //    () => { this.readOnly; })));
    }

    public setupGrid() {
        this.startLoad();
        this.setupWatchers();
    }

    private setupGridColumns() {
        //Product = 0,
        //    Dim1 = 1,
        //    Dim2 = 2,
        //    Dim3 = 3,
        //    Dim4 = 4,
        //    Dim5 = 5,
        //    Dim6 = 6,
        //    EmployeeName = 7,
        //    EmployeeChild = 8,
        //    Quantity = 9,
        //    Invoice = 10,
        //    InvoiceQuantity = 11,
        //    AttestState = 12,
        //    Comment = 13,

        const keys: string[] = [
            "billing.projects.list.product",
            "billing.projects.list.account",
            "billing.projects.list.employee",
            "billing.projects.list.quantity",
            "billing.projects.list.invoicedquantity",
            "billing.projects.list.atteststate"
        ];

        this.translationService.translateMany(keys).then(terms => {

            var colDef1 = this.soeGridOptions.addColumnText("productField", terms["billing.projects.list.product"], null);
            colDef1.allowCellFocus = false;
            var colDef2 = this.soeGridOptions.addColumnText("accountField", terms["billing.projects.list.account"], null);
            colDef2.allowCellFocus = false;
            if (this.dim2Header != null) {
                var colDef3 = this.soeGridOptions.addColumnText("dim2Field", this.dim2Header, null);
                colDef3.allowCellFocus = false;
            }
            if (this.dim3Header != null) {
                var colDef4 = this.soeGridOptions.addColumnText("dim3Field", this.dim3Header, null);
                colDef4.allowCellFocus = false;
            }
            if (this.dim4Header != null) {
                var colDef5 = this.soeGridOptions.addColumnText("dim4Field", this.dim4Header, null);
                colDef5.allowCellFocus = false;
            }
            if (this.dim5Header != null) {
                var colDef6 = this.soeGridOptions.addColumnText("dim5Field", this.dim5Header, null);
                colDef6.allowCellFocus = false;
            }
            if (this.dim6Header != null) {
                var colDef7 = this.soeGridOptions.addColumnText("dim6Field", this.dim6Header, null);
                colDef7.allowCellFocus = false;
            }
            var colDef8 = this.soeGridOptions.addColumnText("employeeName", terms["billing.projects.list.employee"], null);
            colDef8.enableCellEdit = false;
            var colDef9 = this.soeGridOptions.addColumnText("quantityString", terms["billing.projects.list.quantity"], null);
            colDef9.enableCellEdit = false;
            var colDef10 = this.soeGridOptions.addColumnText("invoiceQuantityString", terms["billing.projects.list.invoicedquantity"], null);
            colDef10.enableCellEdit = false;
            var colDef11 = this.soeGridOptions.addColumnText("attestState", terms["billing.projects.list.atteststate"], null);
            colDef11.enableCellEdit = false;

            super.gridDataLoaded(this.rows);


        });
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.projectId, () => {
            this.$q.all([this.loadAccountDims()]).then(() => {
                this.$q.all([
                    this.loadProducts()]).then(() => {
                        this.setupGridColumns();
                    })
            });
        });
    }

    protected onBlur(entity, colDef) {

    }

    public filterProducts(filter) {

    }

    // Lookups

    private loadProducts(): ng.IPromise<any> {
        //console.log(this.projectId);
        return this.coreService.getProjectTimeInvoiceTransactions(this.projectId).then(x => {
            _.forEach(x, (row: any) => {
                row.productField = row.productNr + " " + row.productName;
                row.accountField = row.dim1Nr + " " + row.dim1Name;
                row.dim2Field = row.dim2Nr ? row.dim2Nr + " " + row.dim2Name : "";
                row.dim3Field = row.dim3Nr ? row.dim3Nr + " " + row.dim3Name : "";
                row.dim4Field = row.dim4Nr ? row.dim4Nr + " " + row.dim4Name : "";
                row.dim5Field = row.dim5Nr ? row.dim5Nr + " " + row.dim5Name : "";
                row.dim6Field = row.dim6Nr ? row.dim6Nr + " " + row.dim6Name : "";
                if (row.isQuantityOrMerchandise == false) {
                    row.quantityString = CalendarUtility.minutesToTimeSpan(row.quantity);
                    row.invoiceQuantityString = CalendarUtility.minutesToTimeSpan(row.invoiceQuantity);
                }
                else {
                    row.quantityString = row.quantity;
                    row.invoiceQuantityString = row.invoiceQuantity;
                }

            });

            this.rows = x;
        });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.coreService.getAccountDimsSmall(false, false, true, true, false, false).then(x => {
                var counter: number = 1;
                _.forEach(x, y => {
                    switch (counter) {
                        case 1:
                            counter = counter + 1;
                            break;
                        case 2:
                            this.dim2Header = y.name;
                            counter = counter + 1;
                            break;
                        case 3:
                            this.dim3Header = y.name;
                            counter = counter + 1;
                            break;
                        case 4:
                            this.dim4Header = y.name;
                            counter = counter + 1;
                            break;
                        case 5:
                            this.dim5Header = y.name;
                            counter = counter + 1;
                            break;
                        case 6:
                            this.dim6Header = y.name;
                            counter = counter + 1;
                            break;
                    }
                });
                this.stopProgress();
            });
    }

    // Actions
    private addRow() {

    }

    protected initDeleteRow(row: any) {

    }
}
