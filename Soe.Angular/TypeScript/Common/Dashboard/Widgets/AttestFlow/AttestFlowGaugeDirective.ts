import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { EditController as SupplierInvoiceEditController } from "../../../../Shared/Economy/Supplier/Invoices/EditController";
import { Constants } from "../../../../Util/Constants";
import { ISoeGridOptions } from "../../../../Util/SoeGridOptions";
import { WidgetControllerBase } from "../../Base/WidgetBase";

export class AttestFlowGaugeDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return WidgetControllerBase.getWidgetDirective(urlHelperService.getWidgetUrl('AttestFlow', 'AttestFlowGauge.html'), AttestFlowGaugeController);
    }
}

class AttestFlowGaugeController extends WidgetControllerBase {

    private soeGridOptions: ISoeGridOptions;
    private modalInstance: any;

    //@ngInject
    constructor(
        $timeout: ng.ITimeoutService,
        $q: ng.IQService,
        private $uibModal,
        private $scope: ng.IScope,
        private urlHelperService: IUrlHelperService,
        private coreService: ICoreService,
        private translationService: ITranslationService,
        uiGridConstants: uiGrid.IUiGridConstants) {
        super($timeout, $q, uiGridConstants);

        this.soeGridOptions = this.createGrid();
        this.modalInstance = $uibModal
    }
    
    protected setup(): ng.IPromise<any> {
        this.widgetCss = 'col-sm-6';
        var keys: string[] = [
            "common.dashboard.attestflow.title",
            "common.dashboard.attestflow.invoicedate",
            "common.dashboard.attestflow.invoicenr",
            "common.dashboard.attestflow.duedate",
            "common.dashboard.attestflow.atteststate",
            "common.dashboard.attestflow.supplier",
            "common.dashboard.attestflow.amount",
            "common.dashboard.attestflow.invoicenr",
            "core.show"
        ];

        return this.translationService.translateMany(keys).then(terms => {
            this.widgetTitle = terms["common.dashboard.attestflow.title"];

            this.soeGridOptions.addColumnText("invoiceNr", terms["common.dashboard.attestflow.invoicenr"], "20%");
            this.soeGridOptions.addColumnDate("invoiceDate", terms["common.dashboard.attestflow.invoicedate"], "25%");
            this.soeGridOptions.addColumnDate("dueDate", terms["common.dashboard.attestflow.duedate"], "25%");
            this.soeGridOptions.addColumnText("supplierName", terms["common.dashboard.attestflow.supplier"], null);
            this.soeGridOptions.addColumnNumber("amount", terms["common.dashboard.attestflow.amount"], null);
            this.soeGridOptions.addColumnText("attestStateName", terms["common.dashboard.attestflow.atteststate"], null);
            this.soeGridOptions.addColumnIcon(null, "fal fa-file-search", terms["core.show"], "showInvoice");
        });
    }

    protected load() {
        super.load();
        this.coreService.getAttestFlowWidgetData().then(x => {
            this.soeGridOptions.setData(x);
            super.loadComplete(x.length);
        });
    }

    private showInvoice(item) {
        var modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html"),
            controller: SupplierInvoiceEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope

        });
        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, id: item.invoiceId });
        });

        modal.result.then(id => {
            
        });
    }
}