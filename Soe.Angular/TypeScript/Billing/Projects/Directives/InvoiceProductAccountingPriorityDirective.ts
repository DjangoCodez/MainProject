import { GridControllerBase } from "../../../Core/Controllers/GridControllerBase";
import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { Feature, TermGroup } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export class InvoiceProductAccountingPriorityDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {

        return {
            templateUrl: urlHelperService.getGlobalUrl('Billing/Projects/Directives/InvoiceProductAccountingPriority.html'),
            scope: {
                invoiceProductAccountPriorityRows: '=',
                readOnly: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: AccountingPriorityDirectiveController,
            controllerAs: 'ctrl',
            bindToController: true
        };
    }
}

class AccountingPriorityDirectiveController extends GridControllerBase {
    // Setup
    private invoiceProductAccountPriorityRows: any[];
    private readOnly: boolean;

    // Collections
    accountingPrios: any[] = [];
    accountDims: any[] = [];

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Billing.Projects.List.InvoiceProductAccountingPriority", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        this.soeGridOptions.enableGridMenu = false;
        this.soeGridOptions.enableColumnMenus = false;
        this.soeGridOptions.enableFiltering = false;
        this.soeGridOptions.enableSorting = false;
        this.soeGridOptions.showGridFooter = false;
        this.soeGridOptions.setMinRowsToShow(5);
        this.soeGridOptions.enableDoubleClick = false;
        this.setupTypeAhead();
        //this.doubleClickToEdit = false;
    }

    protected setupCustomToolBar() {

    }

    public setupGrid() {
        this.startLoad();
        this.$q.all([this.loadAccountingPriority()]).then(() => this.setupGridColumns());
    }

    private setupGridColumns() {
        var keys: string[] = [
            "billing.products.products.dimname",
            "billing.products.products.priority"
        ];

        this.translationService.translateMany(keys).then(terms => {
            //var colDef = this.soeGridOptions.addColumnText("dimName", terms["billing.products.products.dimname"], null);                
            //colDef.allowCellFocus = false;
            this.soeGridOptions.addColumnSelect("prioNr", terms["billing.products.products.priority"], "50%", this.accountingPrios, false, true, "prioName", "id", "name", "prioChanged");

            super.gridDataLoaded(this.invoiceProductAccountPriorityRows);
        });
    }

    protected onBlur(entity, colDef) {
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, {});
    }

    // Lookups
    private loadAccountingPriority(): ng.IPromise<any> {
        return this.coreService.getTermGroupContent(TermGroup.InvoiceProductAccountingPrio, true, false).then(x => {
            this.accountingPrios = x;
        });
    }

    // Actions
    public prioChanged(row) {
        var obj = _.find(this.accountingPrios, { id: row.prioNr });
        if (obj) {
            row.prioNr = obj["id"];
            row.prioName = obj["name"];
        }
    }
}
