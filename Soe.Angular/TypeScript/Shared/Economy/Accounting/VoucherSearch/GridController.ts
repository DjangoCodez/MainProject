import { GridControllerBase2Ag } from "../../../../Core/Controllers/GridControllerBase2Ag";
import { ICompositionGridController } from "../../../../Core/ICompositionGridController";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { IAccountingService } from "../../../../Shared/Economy/Accounting/AccountingService";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { Feature } from "../../../../Util/CommonEnumerations";
import { IGridHandlerFactory } from "../../../../Core/Handlers/GridHandlerFactory";
import { IControllerFlowHandlerFactory } from "../../../../Core/Handlers/ControllerFlowHandlerFactory";
import { IProgressHandlerFactory } from "../../../../Core/Handlers/ProgressHandlerFactory";
import { IMessagingHandlerFactory } from "../../../../Core/Handlers/MessagingHandlerFactory";
import { IPermissionRetrievalResponse } from "../../../../Core/Handlers/ControllerFlowHandler";
import { Constants } from "../../../../Util/Constants";
import { IToolbarFactory } from "../../../../Core/Handlers/ToolbarFactory";
import { IGridHandler } from "../../../../Core/Handlers/GridHandler";
import { IAccountDimSmallDTO } from "../../../../Scripts/TypeLite.Net4";

export class GridController extends GridControllerBase2Ag implements ICompositionGridController {

    // Modal
    modal: any;

    //Terms
    terms: { [index: string]: string; };
    // Filter options                
    userFilterOptions: Array<any> = [];
    fromVoucherSeriesFilterOptions: Array<any> = [];
    toVoucherSeriesFilterOptions: Array<any> = [];
    fromAccountFilterOptions: Array<any> = [];
    toAccountFilterOptions: Array<any> = [];
    toAccountDimFilterOptions: Array<any> = [];
    fromAccountDimFilterOptions: Array<any> = [];

    // Selected index
    setselectedfromvoucherseriesindex: number;
    setselectedtovoucherseriesindex: number;
    setselecteduserindex: number;
    setselectedfromaccountindex: number;
    setselectedtoaccountindex: number;
    setselectedfromaccountdimindex: number;
    setselectedtoaccountdimindex: number;

    // Search filters from 
    searchFilterFromVoucherDate: any;
    searchFilterVoucherSeriesFrom: any;
    searchFilterFromCreatedDate: any;
    searchFilterAccountFrom: any;
    searchFilterAccountDimFrom: any;
    get searchFilterDebetFrom() {
        return this.search ? this.search.debitFrom : undefined;
    }
    set searchFilterDebetFrom(item: any) {
        if (this.search) {
            this.search.debitFrom = item;
            /*if (item && !this.searchFilterDebetTo) {
                this.$timeout(() => {
                    this.searchFilterDebetTo = item;
                }, 300);
            }*/
        }
    }
    get searchFilterCreditFrom() {
        return this.search ? this.search.creditFrom : undefined;
    }
    set searchFilterCreditFrom(item: any) {
        if (this.search) {
            this.search.creditFrom = item;
            /*if (item && !this.searchFilterCreditTo) {
                this.$timeout(() => {
                    this.searchFilterCreditTo = item;
                }, 300);
            }*/
        }
    }
    get searchFilterAmountFrom() {
        return this.search ? this.search.amountFrom : undefined;
    }
    set searchFilterAmountFrom(item: any) {
        if (this.search) {
            this.search.amountFrom = item;
            /*if (item && !this.searchFilterAmountTo) {
                this.$timeout(() => {
                    this.searchFilterAmountTo = item;
                }, 300);
            }*/
        }
    }

    // Search filters to
    searchFiltertoVoucherDate: any;
    searchFilterVoucherSeriesTo: any;
    searchFilterToCreatedDate: any;
    searchFilterVoucherText: any;
    searchFilterAccountTo: any;
    searchFilterAccountDimTo: any;
    get searchFilterDebetTo() {
        return this.search ? this.search.debitTo : undefined;
    }
    set searchFilterDebetTo(item: any) {
        if (this.search) 
            this.search.debitTo = item;
    }
    get searchFilterCreditTo() {
        return this.search ? this.search.creditTo : undefined;
    }
    set searchFilterCreditTo(item: any) {
        if (this.search) 
            this.search.creditTo = item;
    }
    get searchFilterAmountTo() {
        return this.search ? this.search.amountTo : undefined;
    }
    set searchFilterAmountTo(item: any) {
        if (this.search) 
            this.search.amountTo = item;
    }

    // Grid header and footer
    gridHeaderComponentUrl: any;
    gridFooterComponentUrl: any;

    //Sums after search
    totalDebit: number;
    totalCredit: number;
    totalBalance: number;

    //AccountDims
    accountDimsTo: Array<any> = [];
    accountDimsFrom: Array<any> = [];

    //AccountStd
    accountStd: Array<any> = [];
    private _selectedAccount: any;
    get selectedAccount() {
        return this._selectedAccount;
    }
    set selectedAccount(item: any) {
        if (item && item.accountId !== 0) {
            this._selectedAccount = item;
        }
        else {
            this._selectedAccount = undefined;
        }
    }

    //Account Years
    accountYears: any[] = [];
    selectedAccountYear: any;

    //AccountPeriods
    accountPeriodsFrom: any[] = [];
    accountPeriodsTo: any[] = [];
    selectedAccountPeriodFrom: any;
    selectedAccountPeriodTo: any;

    //Search dto
    search: any;

    // Flags
    public isModal: boolean = false;
    public transactionMode: boolean = false;
    public headerExpanderOpen: boolean = true;

    //@ngInject
    constructor(
        private accountingService: IAccountingService,
        private translationService: ITranslationService,
        private urlHelperService: IUrlHelperService,
        controllerFlowHandlerFactory: IControllerFlowHandlerFactory,
        progressHandlerFactory: IProgressHandlerFactory,
        messagingHandlerFactory: IMessagingHandlerFactory,
        gridHandlerFactory: IGridHandlerFactory,
        private $q: ng.IQService,
        private $scope: ng.IScope) {
        super(gridHandlerFactory, "Economy.Accounting.VoucherSearch", progressHandlerFactory, messagingHandlerFactory);

        this.flowHandler = controllerFlowHandlerFactory.createForGrid()
            .onAllPermissionsLoaded(x => this.onPermissionsLoaded(x))
            .onCreateToolbar((toolbarFactory) => this.onCreateToolbar(toolbarFactory))
            .onBeforeSetUpGrid(() => this.loadLookups())
            .onSetUpGrid(() => this.setupGrid())

        this.$scope.$on(Constants.EVENT_ON_INIT_MODAL, (e, parameters) => {
            this.isModal = true;
            this.modal = parameters.modal;
            this.onInit(parameters.parameters);
        });
    }

    public onInit(parameters: any) {
        this.parameters = parameters;
        this.transactionMode = parameters.transactionMode || false;

        if(this.transactionMode)
            this.gridHeaderComponentUrl = this.urlHelperService.getGlobalUrl("Shared/Economy/Accounting/VoucherSearch/Views/transactionHeader.html");
        else
            this.gridHeaderComponentUrl = this.urlHelperService.getGlobalUrl("Shared/Economy/Accounting/VoucherSearch/Views/filterHeader.html");

        this.gridFooterComponentUrl = this.urlHelperService.getGlobalUrl("Shared/Economy/Accounting/VoucherSearch/Views/gridFooter.html");

        this.searchFilterFromVoucherDate = null;
        this.searchFilterVoucherSeriesFrom = null;
        this.searchFilterDebetFrom = 0;
        this.searchFilterCreditFrom = 0;
        this.searchFilterAmountFrom = 0;
        this.searchFilterFromCreatedDate = null;
        this.searchFilterAccountFrom = null;
        this.searchFilterAccountDimFrom = null;

        this.searchFiltertoVoucherDate = null;
        this.searchFilterVoucherSeriesTo = null;
        this.searchFilterDebetTo = 0;
        this.searchFilterCreditTo = 0;
        this.searchFilterAmountTo = 0;
        this.searchFilterToCreatedDate = null;
        this.searchFilterVoucherText = null;
        this.searchFilterAccountTo = null;
        this.searchFilterAccountDimTo = null;

        this.totalDebit = 0;
        this.totalCredit = 0;

        this.accountDimsTo = null;

        this.search = {
            voucherDateFrom: null,
            voucherDateTo: null,
            voucherSeriesIdFrom: 0,
            voucherSeriesIdTo: 0,
            debitFrom: 0,
            debitTo: 0,
            creditFrom: 0,
            creditTo: 0,
            amountFrom: 0,
            amountTo: 0,
            voucherText: "",
            createdFrom: null,
            createdTo: null,
            createdBy: "",
            dim1AccountId: 0,
            dim1AccountFr: "",
            dim1AccountTo: "",
            dim2AccountId: 0,
            dim2AccountFr: "",
            dim2AccountTo: "",
            dim3AccountId: 0,
            dim3AccountFr: "",
            dim3AccountTo: "",
            dim4AccountId: 0,
            dim4AccountFr: "",
            dim4AccountTo: "",
            dim5AccountId: 0,
            dim5AccountFr: "",
            dim5AccountTo: "",
            dim6AccountId: 0,
            dim6AccountFr: "",
            dim6AccountTo: ""
        }

        this.setupWatchers();

        this.flowHandler.start([
            { feature: Feature.Economy_Accounting_Vouchers, loadReadPermissions: true, loadModifyPermissions: true }
        ]);
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.search.voucherDateFrom, (newVal, oldVal) => {
            if (newVal && !this.search.voucherDateTo) {
                this.search.voucherDateTo = this.search.voucherDateFrom;
            }
            else if (this.search.voucherDateFrom > this.search.voucherDateTo)
                this.search.voucherDateFrom = oldVal;
        });
        this.$scope.$watch(() => this.search.voucherDateTo, (newVal, oldVal) => {
            if (this.search.voucherDateTo < this.search.voucherDateFrom)
                this.search.voucherDateTo = oldVal;
        });
        this.$scope.$watch(() => this.search.voucherSeriesIdFrom, (newVal, oldVal) => {
            if (newVal && !this.search.voucherSeriesIdTo) {
                this.search.voucherSeriesIdTo = this.search.voucherSeriesIdFrom;
            }
        });
        this.$scope.$watch(() => this.search.createdFrom, (newVal, oldVal) => {
            if (newVal && !this.search.createdTo) {
                this.search.createdTo = this.search.createdFrom;
            }
            else if (this.search.createdFrom > this.search.createdTo)
                this.search.createdFrom = oldVal;
        });
        this.$scope.$watch(() => this.search.createdTo, (newVal, oldVal) => {
            if (this.search.createdTo < this.search.createdFrom)
                this.search.createdTo = oldVal;
        });
    }

    private debetBlur(item) {
        if (this.search)
            this.search.debitTo = this.search.debitFrom ? this.search.debitFrom : 0;
    }

    private creditBlur(item) {
        if (this.search)
            this.search.creditTo = this.search.creditFrom ? this.search.creditFrom : 0;
    }

    private amountBlur(item) {
        if (this.search)
            this.search.amountTo = this.search.amountFrom ? this.search.amountFrom : 0;
    }

    private onPermissionsLoaded(response: IPermissionRetrievalResponse) {
        this.readPermission = response[Feature.Economy_Accounting_Vouchers].readPermission;
        this.modifyPermission = response[Feature.Economy_Accounting_Vouchers].modifyPermission;

        if (this.modifyPermission)
            this.messagingHandler.publishActivateAddTab();
    }

    private onCreateToolbar(toolbarFactory: IToolbarFactory) {
        this.toolbar = toolbarFactory.createDefaultGridToolbar(<IGridHandler>this.gridAg, () => this.loadGridData());
    }

    protected setupGrid() {

        // Columns
        const keys: string[] = [
            "common.number",
            "common.date",
            "common.text",
            "economy.accounting.vouchersearch.debet",
            "economy.accounting.vouchersearch.credit",
            "economy.accounting.vouchersearch.createddate",
            "economy.accounting.vouchersearch.createdby",
            "core.edit",
            "economy.accounting.voucher.voucher",
            "economy.accounting.voucherseriestype"
        ];

        this.translationService.translateMany(keys)
            .then((terms) => {
                this.terms = terms;
                this.gridAg.addColumnNumber("voucherNr", terms["common.number"], null, { enableHiding: true });
                this.gridAg.addColumnDate("voucherDate", terms["common.date"], null, true);
                this.gridAg.addColumnText("voucherText", terms["common.text"], null, true);

                //if (this.transactionMode) {
                    this.gridAg.addColumnText("voucherSeriesName", terms["economy.accounting.voucherseriestype"], null, true);
                    this.accountDimsFrom.forEach((ad, i) => {
                        let index = i + 1;
                        this.gridAg.addColumnText("dim" + index + "AccountName", ad.name, null, true);
                    });
                //}

                this.gridAg.addColumnNumber("debit", terms["economy.accounting.vouchersearch.debet"], null, { enableHiding: true, decimals: 2  });
                this.gridAg.addColumnNumber("credit", terms["economy.accounting.vouchersearch.credit"], null, { enableHiding: true, decimals: 2 });
                this.gridAg.addColumnDate("created", terms["economy.accounting.vouchersearch.createddate"], null, true);
                this.gridAg.addColumnText("createdBy", terms["economy.accounting.vouchersearch.createdby"], null, true);
                //super.addColumnIcon(null, "fal fa-pencil", terms["core.edit"], "openEdit");
                this.gridAg.addColumnEdit(terms["core.edit"], this.edit.bind(this));
                
                this.gridAg.finalizeInitGrid("economy.accounting.vouchersearch", true);
            });
    }

    public loadGridData() {
    }

    protected loadLookups(): ng.IPromise<any> {
        if(this.transactionMode)
            return this.$q.all([this.loadAccounts(), this.loadAccountYears(), this.loadAccountDims()]);
        else
            return this.$q.all([this.loadVoucherSeries(), this.loadUsers(), this.loadAccountDims()]);
    }

    private loadVoucherSeries(): ng.IPromise<any> {
        return this.accountingService.getVoucherSeriesTypes()
            .then((x) => {
                this.fromVoucherSeriesFilterOptions = [];
                this.toVoucherSeriesFilterOptions = [];
                _.forEach(x,
                    (y: any) => {
                        this.toVoucherSeriesFilterOptions.push({ id: y.voucherSeriesTypeId, name: y.name });
                        this.fromVoucherSeriesFilterOptions.push({ id: y.voucherSeriesTypeId, name: y.name });
                    });

                this.searchFilterVoucherSeriesFrom = this.fromVoucherSeriesFilterOptions[0].id;
                this.searchFilterVoucherSeriesTo = this.toVoucherSeriesFilterOptions[0].id;
            });
    }

    private loadUsers(): ng.IPromise<any> {
        return this.accountingService.getUserNamesWithLogin()
            .then((x) => {
                this.userFilterOptions = [{ id: "", name: "" }];
                _.forEach(x,
                    (y: any) => {
                        this.userFilterOptions.push({ id: y.loginName, name: y.userNameAndLogin });
                    });
                this.search.createdBy = this.userFilterOptions[0].id;
            });
    }

    private loadAccountDims(): ng.IPromise<any> {
        return this.accountingService.getAccountDimsSmall(false, false, true, true).then((dims: IAccountDimSmallDTO[]) => {
            this.accountDimsFrom = [];
            this.accountDimsTo = [];
            dims.forEach((y: any) => {
                const from = {
                    name: y.name,
                    accounts: y.accounts ? y.accounts : [],
                    accountDimId: y.accountDimId,
                    accountDimNr: y.accountDimNr,
                    selected: undefined,
                }

                from.accounts.splice(0, 0, { accountId: 0, name: ' ', numberName: ' ' });

                this.$scope.$watch(() => from.selected, (newVal) => {
                    if (newVal) {
                        const dimTo = _.find(this.accountDimsTo, { 'accountDimId': newVal.accountDimId });
                        if (dimTo) {
                            if (!dimTo.selected || dimTo.selected.accountNrSort < newVal.accountNrSort)
                                dimTo.selected = newVal;
                        }

                        from['prevSelected'] = newVal;
                    }
                    else {
                        from.selected = undefined;
                    }
                });
                this.accountDimsFrom.push(from);

                const to = {
                    name: y.name,
                    accounts: y.accounts ? y.accounts : [],
                    accountDimId: y.accountDimId,
                    accountDimNr: y.accountDimNr,
                    selected: undefined,
                }

                to.accounts.splice(0, 0, { accountId: 0, name: ' ', numberName: ' ' });

                this.$scope.$watch(() => to.selected, (newVal) => {
                    if (newVal) {
                        const dimFrom = _.find(this.accountDimsFrom, { 'accountDimId': newVal.accountDimId });
                        if (dimFrom) {
                            if (!dimFrom.selected || dimFrom.selected && dimFrom.selected.accountNrSort > newVal.accountNrSort)
                                dimFrom.selected = newVal;
                        }

                        to['prevSelected'] = newVal;
                    }
                    else {
                        to.selected = undefined;
                    }
                });
                this.accountDimsTo.push(to);
            });
        });
    }

    private loadAccounts(): ng.IPromise<any> {
        this.accountStd = [];
        return this.accountingService.getAccountStdsDict(true).then((x) => {
            this.accountStd = x;
            if(this.parameters.accountId)
                this.selectedAccount = _.find(this.accountStd, { id: this.parameters.accountId });
        });
    }

    private loadAccountYears(): ng.IPromise<any> {
        return this.accountingService.getAccountYearDict(true).then((x) => {
            this.accountYears = x;
            if (soeConfig.accountYearId) {
                var year = _.find(this.accountYears, { id: soeConfig.accountYearId });
                if (year) {
                    this.selectedAccountYear = year.id;
                    this.accountYearChanged(year.id);
                }
            }
        });
    }

    private accountYearChanged(accountYearId: number) {
        this.accountPeriodsFrom = [];
        this.accountPeriodsTo = [];
        this.progress.startLoadingProgress([() => {
            return this.accountingService.getAccountPeriods(accountYearId).then((x) => {
                this.accountPeriodsFrom.push({ id: 0, name: ' ' });
                this.accountPeriodsTo.push({ id: 0, name: ' ' });
                _.forEach(x, (y) => {
                    this.accountPeriodsFrom.push({ id: y.accountPeriodId, name: y.startValue });
                    this.accountPeriodsTo.push({ id: y.accountPeriodId, name: y.startValue });
                });
            });
        }]);
    }

    private startVoucherSearch() {
        this.headerExpanderOpen = false;
        this.progress.startLoadingProgress([() => {
            if (this.accountDimsFrom) { 
            this.accountDimsFrom.forEach((account, i) => {
                this.search[`dim${i + 1}AccountId`] = account.accountDimId;
                if (account.selected) {
                    this.search[`dim${i + 1}AccountFr`] = account.selected.accountNr;
                } else {
                    this.search[`dim${i + 1}AccountFr`] = "";
                }
            });
            }
            if (this.accountDimsTo) {
                this.accountDimsTo.forEach((account, i) => {
                    this.search[`dim${i + 1}AccountId`] = account.accountDimId;
                    if (account.selected) {
                        this.search[`dim${i + 1}AccountTo`] = account.selected.accountNr;
                    } else {
                        this.search[`dim${i + 1}AccountTo`] = "";
                    }
                });
            }

            return this.accountingService.getSearchedVoucherRows(this.search)
                .then((x) => {
                    _.forEach(x,
                        (y) => {
                            y.voucherDate = CalendarUtility.toFormattedDate(y.voucherDate);
                            y.created = CalendarUtility.toFormattedDate(y.created);

                            if (y.dim1AccountNr)
                                y.dim1AccountName = y.dim1AccountNr + " - " + y.dim1AccountName;
                            if (y.dim2AccountNr)
                                y.dim2AccountName = y.dim2AccountNr + " - " + y.dim2AccountName;
                            if (y.dim3AccountNr)
                                y.dim3AccountName = y.dim3AccountNr + " - " + y.dim3AccountName;
                            if (y.dim4AccountNr)
                                y.dim4AccountName = y.dim4AccountNr + " - " + y.dim4AccountName;
                            if (y.dim1AccountNr)
                                y.dim5AccountName = y.dim5AccountNr + " - " + y.dim5AccountName;
                            if (y.dim6AccountNr)
                                y.dim6AccountName = y.dim6AccountNr + " - " + y.dim6AccountName;
                        });
                    return x;
                }).then(data => {
                    this.setData(data);
                    this.Summarize(data);
                });
        }]);
    }

    private getTransactions() {
        this.progress.startLoadingProgress([() => {
            return this.accountingService.getVoucherTransactions(this.selectedAccount ? this.selectedAccount.id : 0, this.selectedAccountYear ? this.selectedAccountYear : 0, this.selectedAccountPeriodFrom ? this.selectedAccountPeriodFrom.id : 0, this.selectedAccountPeriodTo ? this.selectedAccountPeriodTo.id : 0)
                .then((x) => {
                    _.forEach(x,
                        (y) => {
                            y.voucherDate = CalendarUtility.toFormattedDate(y.voucherDate);
                            y.created = CalendarUtility.toFormattedDate(y.created);
                        });
                    return x;
                }).then(data => {
                    this.gridAg.setData(data);
                    this.Summarize(data);
                });
        }]);
    }

    private Summarize(x:any[]) {
        this.totalCredit = 0;
        this.totalDebit = 0;
        x.forEach( (y: any) => {
                this.totalCredit += y.credit;
                this.totalDebit += y.debit;
            });
        this.totalCredit = Math.abs(this.totalCredit)
        this.totalDebit = Math.abs(this.totalDebit)
        this.totalBalance = this.totalDebit - this.totalCredit
    }

    public edit(row) {
        // Send message to TabsController
        if (this.readPermission || this.modifyPermission) {
            this.messagingHandler.publishEditRow(row);
            this.closeModal();
        }

    }

    public closeModal() {
        if (this.isModal) {
            this.modal.close(null);
        }
    }
}
