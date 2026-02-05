import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { EditController } from "./EditController";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    currentAccountDimId: number;
    currentAccountYearId: number;

    currentAccountYearFromDate: any;
    currentAccountYearToDate: any;

    fromDateFilterOptions: Array<any> = [];
    toDateFilterOptions: Array<any> = [];
    fromAccountFilterOptions: Array<any> = [];
    toAccountFilterOptions: Array<any> = [];

    searchFilterFromDate: any;
    searchFilterToDate: any;
    searchFilterFromAccount: any;
    searchFilterToAccount: any;

    gridHeaderComponentUrl: any;

    get selectedFromDate() {
        return this.searchFilterFromDate;
    }

    set selectedFromDate(date: any) {
        this.searchFilterFromDate = date;
        if (!(date instanceof Date)) {
            return;
        }
    }

    get selectedToDate() {
        return this.searchFilterToDate;
    }

    set selectedToDate(date: any) {
        this.searchFilterToDate = date;
        if (!(date instanceof Date)) {
            return;
        }
    }

    //@ngInject
    constructor(
        private $q: ng.IQService,
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private coreService: ICoreService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Economy.Accounting.Reconciliation", progressHandlerFactory, messagingHandlerFactory);

        this.gridHeaderComponentUrl = urlHelperService.getViewUrl("filterHeader.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onSetUpGrid(() => this.setUpGrid())
            .onLoadGridData(() => this.loadGridData())
            .onDoLookUp(() => this.onDoLookups())
    }

    public onInit(parameters: any) {
        this.flowHandler.start({ feature: Feature.Economy_Accounting_Reconciliation, loadReadPermissions: true, loadModifyPermissions: true });
    }

    public edit(row: any) {

    }

    private onDoLookups(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.loadCurrentAccountYear(),
            () => this.loadAccountDimStd()
        ]).then(() => { this.loadAccounts() });
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, () => this.loadGridData());
    }

    private showDetailsGrid(row) {
        var params = {
            accountId: (row.accountId != null) ? row.accountId : 0,
            accountYearId: this.currentAccountYearId,
            fromDate: row.fromDate,
            toDate: row.toDate
        };

        var message = new TabMessage(
            row.account.split(" ")[0],
            (row.accountId != null) ? row.accountId + " " + row.account : 0,
            EditController,
            params,
            this.urlHelperService.getGlobalUrl('Economy/Accounting/Reconciliation/Views/edit.html')
        );

        this.messagingHandler.publishEvent(Constants.EVENT_OPEN_TAB, message);
    }

    public loadGridData() {
        this.gridAg.clearData();

        //this.progress.startLoadingProgress([() => this.loadAccounts()]);  //Will not load on page refresh       
    }

    private searchReconciliations() {
        this.gridAg.clearData();

        this.progress.startWorkProgress((completion) => {

            this.accountingService.getReconciliationRows(
                this.currentAccountDimId,
                this.searchFilterFromAccount,
                this.searchFilterToAccount,
                this.searchFilterFromDate,
                this.searchFilterToDate).then((data) => {
                    if (data) {
                        this.setData(data);

                        completion.completed(data, true);
                    }
                }, error => {

                    completion.failed(error.message);
                });
        });
    }

    private loadAccounts(): ng.IPromise<any> {
        var deferral = this.$q.defer();

        if (this.currentAccountDimId > 0 && this.currentAccountYearId > 0) {
            this.accountingService.getAccounts(this.currentAccountDimId, this.currentAccountYearId).then((x) => {

                _.forEach(x, (y: any) => {
                    this.toAccountFilterOptions.push({ id: y.accountNr, name: y.accountNr + " - " + y.name });
                    this.fromAccountFilterOptions.push({ id: y.accountNr, name: y.accountNr + " - " + y.name });
                });

                this.fromAccountFilterOptions = _.sortBy(this.fromAccountFilterOptions, 'id');
                this.toAccountFilterOptions = _.sortBy(this.toAccountFilterOptions, 'id');

                this.searchFilterFromAccount = this.fromAccountFilterOptions[0].id;
                this.searchFilterToAccount = this.toAccountFilterOptions[this.toAccountFilterOptions.length - 1].id;

                deferral.resolve();
            });
        } else { deferral.resolve(); }

        return deferral.promise;
    }

    private loadCurrentAccountYear(): ng.IPromise<any> {
        return this.coreService.getCurrentAccountYear().then((x) => {
            this.currentAccountYearId = x.accountYearId;

            this.currentAccountYearFromDate = new Date(x.from);
            this.currentAccountYearToDate = new Date(x.to);
            this.searchFilterFromDate = this.currentAccountYearFromDate;
            this.searchFilterToDate = this.currentAccountYearToDate;
        });
    }

    private loadAccountDimStd(): ng.IPromise<any> {
        return this.accountingService.getAccountDimStd().then((x) => {
            this.currentAccountDimId = x.accountDimId;
        });
    }

    private setUpGrid() {
        var translationKeys: string[] = [
            "economy.accounting.reconciliation.account",
            "economy.accounting.reconciliation.customeramount",
            "economy.accounting.reconciliation.supplieramount",
            "economy.accounting.reconciliation.paymentamount",
            "economy.accounting.reconciliation.ledgeramount",
            "economy.accounting.reconciliation.diffamount",
            "core.aggrid.totals.total",
            "core.aggrid.totals.filtered"
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {
            this.gridAg.addColumnText("account", terms["economy.accounting.reconciliation.account"], 25, true);
            this.gridAg.addColumnNumber("customerAmount", terms["economy.accounting.reconciliation.customeramount"], 15, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnNumber("supplierAmount", terms["economy.accounting.reconciliation.supplieramount"], 15, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnNumber("paymentAmount", terms["economy.accounting.reconciliation.paymentamount"], 15, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnNumber("ledgerAmount", terms["economy.accounting.reconciliation.ledgeramount"], 15, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnNumber("diffAmount", terms["economy.accounting.reconciliation.diffamount"], 15, { enableHiding: true, decimals: 2 });
            this.gridAg.addColumnIcon("info", "", 30, {
                toolTip: terms["economy.accounting.reconciliation.diffamount"],
                icon: "fal fa-info-circle infoColor",
                onClick: this.showDetailsGrid.bind(this),
                suppressSorting: true,
                enableResizing: false
            });

            this.gridAg.finalizeInitGrid("economy.accounting.reconciliation.reconciliation", true);
            this.setData('')
        });
    }
}
