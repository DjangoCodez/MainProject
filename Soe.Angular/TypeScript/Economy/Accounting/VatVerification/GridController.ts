import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { IControllerFlowHandlerFactory } from "../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../Core/Handlers/MessagingHandlerFactory";
import { IGridHandlerFactory } from "../../../Core/Handlers/GridHandlerFactory";
import { IToolbarFactory } from "../../../Core/Handlers/ToolbarFactory";
import { ICompositionGridController } from "../../../Core/ICompositionGridController";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { TabMessage } from "../../../Core/Controllers/TabsControllerBase1";
import { EditController as VouchersEditController } from "../../../Shared/Economy/Accounting/Vouchers/EditController";
import { Feature } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { GridControllerBase2Ag } from "../../../Core/Controllers/GridControllerBase2Ag";
import { IColumnAggregations } from "../../../Util/SoeGridOptionsAg";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {
    setselectedyearindex: number;
    setselectedfromperiodindex: number;
    setselectedtoperiodindex: number;

    accountYearFilterOptions: Array<any> = [];
    fromPeriodFilterOptions: Array<any> = [];
    toPeriodFilterOptions: Array<any> = [];

    searchFilterAccountYear: any;
    searchFilterFromDate: any;
    searchFilterToDate: any;
    searchFilterExcludeDiffAmountLimit = 0;

    totalVat25 = 0;
    totalVat12 = 0;
    totalVat6 = 0;
    totalTax25 = 0;

    gridHeaderComponentUrl: any;

    voucherEditPrefixTerm: string = null;

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
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        private messagingService: IMessagingService,
        private urlHelperService: IUrlHelperService) {
        super(gridHandlerFactory, "Economy.Accounting.VatVerification", progressHandlerFactory, messagingHandlerFactory);

        this.gridHeaderComponentUrl = urlHelperService.getViewUrl("filterHeader.html");

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onPermissionsLoaded((feature, readOnly, modify) => {
                this.readPermission = readOnly;
                this.modifyPermission = modify

                if (this.modifyPermission) {
                    this.messagingHandler.publishActivateAddTab();
                }
            })
            .onSetUpGrid(() => this.onSetUpGrid())
            .onDoLookUp(() => this.onDoLookups())
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory));
    }

    public onInit(parameters: any) {
        this.flowHandler.start({ feature: Feature.Economy_Accounting_Vouchers, loadReadPermissions: true, loadModifyPermissions: true });
    }

    public edit(row: any) {
        this.messagingService.publish(Constants.EVENT_OPEN_TAB,
            new TabMessage(this.voucherEditPrefixTerm + " " + row.voucherNr, row.voucherHeadId, VouchersEditController, { id: row.voucherHeadId }, this.urlHelperService.getGlobalUrl('Economy/Accounting/Vouchers/Views/edit.html')));

    }

    private onDoLookups(): ng.IPromise<any> {
        return this.progress.startLoadingProgress([
            () => this.loadAccountYearDict()
        ]);
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(this.gridAg, null);
    }

    private onSetUpGrid() {
        const translationKeys: string[] = [
            "economy.accounting.vatverification.period",                //Period
            "economy.accounting.vatverification.voucherserie",          //Verifikatserie                 
            "economy.accounting.vatverification.vouchernumber",         //Verifikatnummer                
            "economy.accounting.vatverification.vat25",                 //Utgående moms 25%               
            "economy.accounting.vatverification.vat12",                 //Utgående moms 12%               
            "economy.accounting.vatverification.vat6",                  //Utgående moms 6%              
            "economy.accounting.vatverification.tax25",                 //Utgående skatt 25%                
            "economy.accounting.vatverification.vatsales",              //Momspliktig försäljning
            "economy.accounting.vatverification.diffvatsales",          //Diff momspliktig försäljning              
            "economy.accounting.vatverification.taxsalesaccordingtovat",//Skattepliktig försäljning enligt moms                                    
        ];

        this.translationService.translateMany(translationKeys).then((terms) => {
            this.gridAg.addColumnNumber("periodNr", terms["economy.accounting.vatverification.period"], 80);
            this.gridAg.addColumnText("voucherSeriesName", terms["economy.accounting.vatverification.voucherserie"], null, true);
            this.gridAg.addColumnText("voucherNr", terms["economy.accounting.vatverification.vouchernumber"], null, true);
            this.gridAg.addColumnNumber("vat25Amount", terms["economy.accounting.vatverification.vat25"], null, { enableHiding: false, decimals: 2});
            this.gridAg.addColumnNumber("vat12Amount", terms["economy.accounting.vatverification.vat12"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.addColumnNumber("vat6Amount", terms["economy.accounting.vatverification.vat6"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.addColumnNumber("tax25Amount", terms["economy.accounting.vatverification.tax25"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.addColumnNumber("vatSalesSumAmount", terms["economy.accounting.vatverification.vatsales"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.addColumnNumber("diffAmount", terms["economy.accounting.vatverification.diffvatsales"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.addColumnNumber("taxSalesDueVATAmount", terms["economy.accounting.vatverification.taxsalesaccordingtovat"], null, { enableHiding: false, decimals: 2 });
            this.gridAg.options.addColumnEdit(terms["core.edit"], this.edit.bind(this));

            this.gridAg.options.addFooterRow("#sum-footer-grid", {
                "vat25Amount": "sum",
                "vat12Amount": "sum",
                "vat6Amount": "sum",
                "tax25Amount": "sum",
                "vatSalesSumAmount": 'sum',
                "diffAmount": "sum",
                "taxSalesDueVATAmount": 'sum'
            } as IColumnAggregations);

            this.gridAg.finalizeInitGrid("economy.accounting.vatverification", true)

            this.voucherEditPrefixTerm = terms["economy.accounting.vatverification.vouchernumber"];
        });
    }

    public accountYearOnChanging(selectedYearId) {
        this.loadAccountPeriodDict(selectedYearId);
    }

    private startVatVerification() {
        this.progress.startWorkProgress((completion) => {

            const fromDate = new Date(this.searchFilterFromDate.slice(0, 4), this.searchFilterFromDate.slice(4, 6), 1, 0, 0, 0, 0);
            let toDate = new Date(this.searchFilterToDate.slice(0, 4), this.searchFilterToDate.slice(4, 6), 1, 0, 0, 0, 0);

            fromDate.setMonth(fromDate.getMonth() - 1);
            toDate.setMonth(toDate.getMonth() - 1);
            toDate = toDate.endOfMonth();

            const limit = this.searchFilterExcludeDiffAmountLimit.toString();
            const limitNumber = +limit.replace(",", ".");

            this.accountingService.getVatVerifyVoucherRows(fromDate, toDate, limitNumber).then((data) => {
                data = _.sortBy(data, 'periodNr');

                this.setData(data);

                this.Summarize(data);

                completion.completed(data, true);

            }, error => {

                completion.failed(error.message);
            });
        });
    }

    private Summarize(data) {
        this.totalVat25 = 0;
        this.totalVat12 = 0;
        this.totalVat6 = 0;
        this.totalTax25 = 0;
        _.forEach(data, (y: any) => {
            this.totalVat25 += y.vat25Amount;
            this.totalVat12 += y.vat12Amount;
            this.totalVat6 += y.vat6Amount;
            this.totalTax25 += y.tax25Amount;
        });
    }

    private loadAccountYearDict(): ng.IPromise<any> {
        return this.accountingService.getAccountYearDict(false).then((x) => {
            _.forEach(x, (y: any) => {
                this.accountYearFilterOptions.push({ id: y.id, name: y.name })
            });

            this.accountYearFilterOptions.reverse();
            this.searchFilterAccountYear = this.accountYearFilterOptions[0].id;

            this.loadAccountPeriodDict(this.accountYearFilterOptions[0].id);
        });
    }

    private loadAccountPeriodDict(yearId: number): ng.IPromise<any> {
        return this.accountingService.getAccountPeriodDict(yearId, false).then((i) => {
            this.fromPeriodFilterOptions = [];
            this.toPeriodFilterOptions = [];

            _.forEach(i, (j: any) => {
                this.fromPeriodFilterOptions.push({ id: j.name, name: j.name })
                this.toPeriodFilterOptions.push({ id: j.name, name: j.name })
            });

            this.selectedFromDate = this.fromPeriodFilterOptions[0].id;
            this.selectedToDate = this.toPeriodFilterOptions[this.toPeriodFilterOptions.length - 1].id;
        });
    }
}
