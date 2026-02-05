import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent } from "../../../../Util/Enumerations";
import { Feature, TermGroup_BillingType } from "../../../../Util/CommonEnumerations";

//@ngInject
export function exportDetailsDirective(urlHelperService: IUrlHelperService): ng.IDirective {
    return {
        templateUrl: urlHelperService.getViewUrl("exportDetails.html"),
        replace: true,
        restrict: "E",
        controller: ExportDetailsController,
        controllerAs: "ctrl",
        bindToController: true,
        scope: {
            invoices: "=",
            exportPaymentId: "=",
            selectedTotal: "="
        },
        link(scope: ng.IScope, element: JQuery, attributes: ng.IAttributes, ngModelController: any) {
            scope.$watch(() => (ngModelController.invoices),
                (newValue) => {
                    if (newValue && newValue.length > 0 && ngModelController.exportPaymentId === 0) {
                        ngModelController.updateData();
                    }
                }, true);
        }
    }
}

export class ExportDetailsController extends GridControllerBase {
    public invoices: any;
    public exportPaymentId: number;
    public selectedTotal: number;
    private terms: any;
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
        private $scope: ng.IScope) {

        super("economy.export.paymentservices.exportdetails",
            "economy.export.paymentservices.exportdetails",
            Feature.Economy_Export_Invoices_PaymentService,
            $http,
            $templateCache,
            $timeout,
            $uibModal,
            coreService,
            translationService,
            urlHelperService,
            messagingService,
            notificationService,
            uiGridConstants);

        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.multiSelect = true;
        this.soeGridOptions.enableRowHeaderSelection = true;
        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.showColumnFooter = true;
        this.soeGridOptions.enableRowSelection = true;
        this.soeGridOptions.setMinRowsToShow(8);
    }

    $onInit() {
        this.$scope.$watch(() => this.invoices, (newVal, oldVal) => {
            this.updateData();
        });
    }
    protected setupGrid() {
        var keys: string[] = [
            "common.type",
            "economy.export.payments.invoicenr",
            "common.customer",
            "economy.export.payments.invoiceamount",
            "economy.export.payments.invoicedate",
            "economy.export.payments.paydate",
            "economy.export.payments.bankaccount",
            "common.send"
        ];

        this.translationService.translateMany(keys)
            .then(terms => {
                this.terms = terms;
                this.soeGridOptions.addColumnText("invoiceTypeName", terms["common.type"], null);
                this.soeGridOptions.addColumnNumber("invoiceNr", terms["economy.export.payments.invoicenr"], null);
                this.soeGridOptions.addColumnText("customerName", terms["common.customer"], null);
                this.soeGridOptions.addColumnNumber("invoiceAmount", terms["economy.export.payments.invoiceamount"], null, null, 2);
                this.soeGridOptions.addColumnDate("invoiceDate", terms["economy.export.payments.invoicedate"], null);
                this.soeGridOptions.addColumnDate("dueDate", terms["economy.export.payments.paydate"], null);
                this.soeGridOptions.addColumnText("bankAccount", terms["economy.export.payments.bankaccount"], null);
                this.soeGridOptions.subscribe([new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, (row: uiGrid.IGridRow) => {
                    row.entity.isSelected = row.isSelected;
                    this.selectedTotal = 0;
                    this.soeGridOptions.getSelectedRows().forEach(row => this.selectedTotal += row.invoiceAmount);
                })]);
                this.updateData();
            });
    }

    // prevent dubble click in grid
    public edit() { }

    public updateData() {
        this.invoices = this.invoices || [];
        this.translationService.translateMany(["common.debit", "common.credit"])
            .then(terms => {
                this.invoices.forEach(invoice => {
                    switch (invoice.invoiceType) {
                        case TermGroup_BillingType.Debit:
                            invoice.invoiceTypeName = terms["common.debit"];
                            break;
                        case TermGroup_BillingType.Credit:
                            invoice.invoiceTypeName = terms["common.credit"];
                            break;
                    }
                });

            });
        this.soeGridOptions.setData(this.invoices);
    }
}