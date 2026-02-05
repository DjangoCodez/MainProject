import { ICoreService } from "../../../Core/Services/CoreService";
import { ITranslationService } from "../../../Core/Services/TranslationService";
import { IUrlHelperService } from "../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../Core/Services/MessagingService";
import { INotificationService } from "../../../Core/Services/NotificationService";
import { GridControllerBaseAg } from "../../../Core/Controllers/GridControllerBaseAg";
import { IColumnAggregate, IColumnAggregations } from "../../../Util/SoeGridOptionsAg";
import { ToolBarUtility, ToolBarButton } from "../../../Util/ToolBarUtility";
import { NumberUtility } from "../../../Util/NumberUtility";
import { GridEvent } from "../../../Util/SoeGridOptions";
import { SettingsUtility } from "../../../Util/SettingsUtility";
import { DistributionHelper } from "./Helpers/DistributionHelper";
import { AddAccountController } from "./AddAccountController";
import { CoreUtility } from "../../../Util/CoreUtility";
import { AccountingRowsContainers, IconLibrary, SoeGridOptionsEvent, CurrencyEvent, SOEMessageBoxButtons, SOEMessageBoxButton, SOEMessageBoxImage } from "../../../Util/Enumerations";
import { AccountingRowDTO } from "../../Models/AccountingRowDTO";
import { IVoucherSeriesDTO, IAccountDTO, ISysAccountStdDTO, IAccountEditDTO, IActionResult } from "../../../Scripts/TypeLite.Net4";
import { CurrencyHelper, CurrencyHelperEvent } from "../Helpers/CurrencyHelper";
import { AccountDTO } from "../../Models/AccountDTO";
import { Constants } from "../../../Util/Constants";
import { Feature, TermGroup_CurrencyType, TermGroup_AccountDistributionTriggerType, CompanySettingType, AccountingRowType, SupplierInvoiceAccountRowAttestStatus, SoeEntityState, MapItemType } from "../../../Util/CommonEnumerations";
import { IAccountingService } from "../../../Shared/Economy/Accounting/AccountingService";
import { EditController as InventoryEditController } from "../../../Shared/Economy/Inventory/Inventories/EditController";
import { GridController as TransactionGridController } from "../../../Shared/Economy/Accounting/VoucherSearch/GridController"
import { AccountDimSmallDTO } from "../../Models/AccountDimDTO";
import { AccountingRowsContainer } from "../../Models/Enums";

export class AccountingRowsDirectiveFactory {
    //@ngInject
    public static create(translationService: ITranslationService, urlHelperService: IUrlHelperService): ng.IDirective {
        return {
            templateUrl: urlHelperService.getCommonDirectiveUrl('AccountingRows/Views', 'AccountingRows.html'),
            scope: {
                registerControl: '&',
                container: '@',
                progressBusy: '=?',
                isReadonly: '=?',
                accountingRows: '=',
                actorId: '=?',
                currencyId: '=?',
                currencyDate: '=?',
                currencyRateDate: '=?',
                baseCurrencyCode: '=?',
                enterpriseCurrencyCode: '=?',
                ledgerCurrencyCode: '=?',
                transactionCurrencyCode: '=?',
                transactionCurrencyRate: '=?',
                isBaseCurrency: '=?',
                isLedgerCurrency: '=?',
                onAmountConverted: '&',
                onEditCompletedForBalancedAccount: '&',
                showRegenerateDetailedButton: '=?',
                showSortButtons: '@',
                showRegenerateButton: '@',
                showReloadAccountsButton: '@',
                showRowNr: '@',
                showText: '@',
                showTransactionCurrency: '=?',
                showEnterpriseCurrency: '=?',
                showLedgerCurrency: '=?',
                showAttestUser: '@',
                showBalance: '@',
                showAmountSummary: '@',
                showInstructions: '@',
                allowZeroAmount: '=',
                oneColumnAmount: '@',
                minRowsToShow: '@',
                defaultAttestRowDebitAccountId: '=',
                defaultAttestRowAmount: '=',
                purchaseDate: '=?',
                showGrouping: '=?',
                showVoucherSeries: '=?',
                voucherSeriesId: '=?',
                voucherDate: '=?',
                overrideDiffValidation: '=?',
                lockVoucherSeries: '=?',
                parentGuid: '=?',
                parentRecordId: '=?',
                currencyHelper: '=?',
            },
            restrict: 'E',
            replace: true,
            controller: AccountingRowsController,
            controllerAs: 'directiveCtrl',
            bindToController: true
        };
    }
}

export class AccountingRowsController extends GridControllerBaseAg {

    private registerControl: Function;
    private container: AccountingRowsContainers;
    private isReadonly: boolean;
    private terms: any;
    private accountingRows: AccountingRowDTO[];
    private get activeAccountingRows(): AccountingRowDTO[] {
        return _.sortBy(_.filter(this.accountingRows, r => !r.isDeleted || this.container == AccountingRowsContainers.SupplierInvoiceAttest || this.debugMode), ['rowNr', 'tempRowId']);
    }
    public accountDims: AccountDimSmallDTO[];
    private voucherSeries: IVoucherSeriesDTO[];
    private currencyHelper: CurrencyHelper;
    private delaySetCurrency: boolean;
    private delayedCurrencyId: number;
    private delayedCurrencyDate: Date;
    private delayedCurrencyRateDate: Date;
    private deleayIsBaseCurrency: boolean = undefined;
    private deleayIsLedgerCurrency: boolean = undefined;
    private accountDistributionHelper: DistributionHelper;
    private accountBalances: any;
    private delayedSetRowItemAccountsOnAllRows: boolean = false;
    private delayedSetRowItemAccountsOnAllRowsIfMissing: boolean = false;

    private modalInstance: any;
    private pendingAutobalance: boolean = false;
    // Currency properties
    private _actorId: number;
    get actorId(): number {
        return this._actorId;
    }
    set actorId(value: number) {
        if (this._actorId !== value) {
            this._actorId = value;
            if (this.currencyHelper) {
                this.currencyHelper.loadLedgerCurrency(this._actorId);
            }
        }
    }

    public get currencyId(): number {
        if (this.delaySetCurrency) {
            return this.delayedCurrencyId;
        }

        return this.currencyHelper ? this.currencyHelper.currencyId : 0;
    }
    public set currencyId(value: number) {
        if (this.currencyHelper)
            this.currencyHelper.currencyId = value;
        else {
            this.delayedCurrencyId = value;
            this.delaySetCurrency = true;
        }
    }
    public get currencyDate(): Date {
        return this.currencyHelper ? this.currencyHelper.currencyDate : null;
    }
    public set currencyDate(date: Date) {
        if (this.currencyHelper) {
            if (date instanceof Date) {
                this.currencyHelper.currencyDate = date;
            }
            else {
                this.currencyHelper.currencyDate = date ? new Date(<any>date) : undefined;
            }
        }
        else {
            this.delayedCurrencyDate = date;
            this.delaySetCurrency = true;
        }
    }
    public get currencyRateDate(): Date {
        return this.currencyHelper ? this.currencyHelper.currencyRateDate : null;
    }
    public set currencyRateDate(date: Date) {
        if (typeof date === 'string') {
            date = new Date(date);
        }
        if (this.currencyHelper)
            this.currencyHelper.currencyRateDate = date;
        else {
            this.delayedCurrencyRateDate = date;
            this.delaySetCurrency = true;
        }
    }

    private get baseCurrencyCode(): string {
        return this.currencyHelper ? this.currencyHelper.getBaseCurrencyCode() : '';
    }
    private set baseCurrencyCode(code: string) { /* Not actually a setter, just to make binding work */ }
    private get enterpriseCurrencyCode(): string {
        return this.currencyHelper ? this.currencyHelper.getEnterpriseCurrencyCode() : '';
    }
    private set enterpriseCurrencyCode(code: string) { /* Not actually a setter, just to make binding work */ }
    private get ledgerCurrencyCode(): string {
        return this.currencyHelper ? this.currencyHelper.getLedgerCurrencyCode() : '';
    }
    private set ledgerCurrencyCode(code: string) { /* Not actually a setter, just to make binding work */ }
    private get transactionCurrencyCode(): string {
        return this.currencyHelper ? this.currencyHelper.getTransactionCurrencyCode() : '';
    }
    private set transactionCurrencyCode(code: string) { /* Not actually a setter, just to make binding work */ }
    private get transactionCurrencyRate(): number {
        return this.currencyHelper ? this.currencyHelper.transactionCurrencyRate : 1;
    }
    private set transactionCurrencyRate(rate: number) { /* Not actually a setter, just to make binding work */ }
    private get isBaseCurrency(): boolean {
        if (!this.currencyHelper?.isInitialized) {
            return this.deleayIsBaseCurrency ?? true;
        }
        else {
            return this.currencyHelper.getIsBaseCurrency();
        }
    }
    private set isBaseCurrency(value: boolean) { this.deleayIsBaseCurrency = value; /* Not actually a setter, just to make binding work */ }
    private get isLedgerCurrency(): boolean {
        if (!this.currencyHelper?.isInitialized) {
            return this.deleayIsLedgerCurrency ?? true;
        }
        else {
            return this.currencyHelper.getIsLedgerCurrency();
        }
    }
    private set isLedgerCurrency(value: boolean) {
        this.deleayIsLedgerCurrency = value;
    }

    public onAmountConverted: (item: any) => void;
    public onEditCompletedForBalancedAccount: () => void;

    // Init parameters
    private parentGuid: string;
    private parentRecordId: number;
    private showRegenerateDetailedButton: boolean;
    private showSortButtons: boolean;
    private showRegenerateButton: boolean;
    private showReloadAccountsButton: boolean;
    private showRowNr: boolean;
    private showText: boolean;
    private showQuantity: boolean;
    private showAttestUser: boolean;
    private showBalance: boolean;
    private showTransactionCurrency: boolean;
    private showEnterpriseCurrency: boolean;
    private showLedgerCurrency: boolean;
    private showAmountSummary: boolean;
    private showInstructions: boolean;
    private allowZeroAmount: boolean;
    private oneColumnAmount: boolean;
    private minRowsToShow: number;
    private defaultAttestRowDebitAccountId: number;
    private defaultAttestRowAmount: number;
    private purchaseDate: Date;
    private showGrouping: boolean;
    private showVoucherSeries: boolean;
    private lockVoucherSeries: boolean;
    private _voucherSeriesId: number;
    public get ignoreInternalAccounts(): boolean {
        return this.container == AccountingRowsContainers.CustomerInvoice;
    }
    public get voucherSeriesId(): number {
        return this._voucherSeriesId;
    }
    public set voucherSeriesId(value: number) {
        this._voucherSeriesId = value;

        if (this.messagingService && this.setupDone)
            this.messagingService.publish(Constants.EVENT_SET_DIRTY, this.parentGuid ? { guid: this.parentGuid } : null);
    }
    private _voucherDate: Date;
    public get voucherDate(): Date {
        return this._voucherDate;
    }
    public set voucherDate(value: Date) {
        this._voucherDate = value;

        if (this.messagingService && this.setupDone && this.showVoucherSeries)
            this.messagingService.publish(Constants.EVENT_SET_DIRTY, this.parentGuid ? { guid: this.parentGuid } : null);
    }
    private overrideDiffValidation: boolean;

    private _collapseAllRowGroups: boolean;
    public get collapseAllRowGroups(): boolean {
        return this._collapseAllRowGroups;
    }
    public set collapseAllRowGroups(value: boolean) {
        this._collapseAllRowGroups = value;
        this.setRowGroupExpension();
    }

    // Converted init parameters
    private showSortButtonsValue: boolean;
    private showRegenerateButtonValue: boolean;
    private showReloadAccountsButtonValue: boolean;
    private showRowNrValue: boolean;
    private showTextValue: boolean;
    private showQuantityValue: boolean;
    private showAttestUserValue: boolean;
    private showBalanceValue: boolean;
    private showAmountSummaryValue: boolean;
    private showInstructionsValue: boolean;
    private oneColumnAmountValue: boolean;

    // Company settings
    private allowUnbalancedRows: boolean = false;
    private useDimsInRegistration: boolean = false;
    private useAutomaticAccountDistribution: boolean = false;
    private steppingRules: any;

    // Grouping
    private groupColumn: any;

    //ui stuff
    private setupDone: boolean = false;
    private gridHeightStyle;
    private controllIsReady: boolean = false;

    private internalIdCounter = 1;

    private debugMode: boolean = false;

    private inventoryTriggerAccounts: any[] = [];

    private oppositeColumns: any = {};

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        protected $uibModal,
        protected coreService: ICoreService,
        private accountingService: IAccountingService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Common.Directives.AccountingRows", "", Feature.None, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);

        //var gridOptions = (this.soeGridOptions as any).gridOptions;
        //gridOptions.enableHorizontalScrollbar = 0;

        ////beging fix for scrolling issue
        ////TODO: Able to set row heights.
        //gridOptions.headerRowHeight = 20;//partial fix for scrolling issue
        //gridOptions.rowHeight = 22;//partial fix for scrolling issue

        //var height = gridOptions.minRowsToShow * gridOptions.rowHeight + 92;//This causes the canvas to fit 8.5 rows, which makes a scrolling bug on the first row that causes a scrollbar to go away. The important part is that the canvas need to a be a bit bigger than the rows the grid thinks its rendering.
        //this.gridHeightStyle = { height: height + "px" };
        ////end fix scrolling issue

        this.$scope.$on('focusRow', (e, a) => {
            this.soeGridOptions.startEditingCell(a.row - 1, this.getDim1Column());
        });
        this.$scope.$on('setRowItemAccountsOnRow', (e, a) => {
            if (this.controllIsReady)
                this.setRowItemAccountsOnRow(a, false);
            else
                this.delayedSetRowItemAccountsOnAllRows = true;
        });
        this.$scope.$on('setRowItemAccountsOnRowIfMissing', (e, a) => {
            if (this.controllIsReady)
                this.setRowItemAccountsOnRowIfMissing(a);
            else
                this.delayedSetRowItemAccountsOnAllRowsIfMissing = true;
        });
        this.$scope.$on('setRowItemAccountsOnAllRows', (e, a) => {

            if (this.controllIsReady) {
                if (a && a.setInternalAccountFromAccount) {
                    this.setRowItemAccountsOnAllRows(true);
                    this.sendAccountRowsModified();
                }
                else
                    this.setRowItemAccountsOnAllRows(false);
            }
            else
                this.delayedSetRowItemAccountsOnAllRows = true;
        });
        this.$scope.$on('setRowItemAccountsOnAllRowsIfMissing', (e, a) => {
            if (this.controllIsReady) {
                this.setRowItemAccountsOnAllRowsIfMissing();
            }
            else
                this.delayedSetRowItemAccountsOnAllRowsIfMissing = true;
        });
        this.$scope.$on('dimChanged', (e, a) => {
            if (this.controllIsReady) {
                const rowItem = _.find(this.activeAccountingRows, { rowNr: a[0] });
                if (rowItem && this.accountDims) {
                    if (this.accountDims.length >= (a[1])) {
                        const acc = _.find(this.accountDims[a[1] - 1].accounts, acc => acc.accountId === rowItem['dim' + a[1] + 'Id']);
                        if (acc) {
                            rowItem['dim' + a[1] + 'Nr'] = acc.accountNr;
                            rowItem['dim' + a[1] + 'Name'] = acc.name;
                            rowItem['dim' + a[1] + 'Error'] = null;
                            this.soeGridOptions.refreshRows(rowItem);
                        }
                    }
                }
            }
        });
        this.$scope.$on('amountChanged', (e, a) => {
            this.convertAmount(a.field, a.amount, a.sourceCurrencyType);
        });
        this.$scope.$on('rowAdded', (e, a, container) => {
            if (this.container == container) {
                this.calculateAccountBalances();
                this.calculateRowAllCurrencyAmounts(a, TermGroup_CurrencyType.TransactionCurrency, true);
                this.gridDataLoaded(this.activeAccountingRows);

                this.internalIdCounter = this.activeAccountingRows.length;
            }
        });
        this.$scope.$on('rowsAdded', (e, a) => {
            if (a && a.setRowItemAccountsOnAllRows) {
                this.setRowItemAccountsOnAllRows(false);
            }

            this.calculateAccountBalances();
            this.calculateAllRowsAllCurrencyAmounts(TermGroup_CurrencyType.TransactionCurrency, false).then(() => {
                this.gridDataLoaded(this.activeAccountingRows);
                this.soeGridOptions.refreshRows();
            });

            this.internalIdCounter = this.activeAccountingRows.length;
        });
        this.$scope.$on('checkAccountDistribution', (e, a, container) => {
            if (this.container == container) {
                this.calculateRowCurrencyAmounts(a, TermGroup_CurrencyType.TransactionCurrency, TermGroup_CurrencyType.BaseCurrency);
                this.$timeout(() => {
                    this.checkAccountDistribution(a);
                });
            }
        });
        this.$scope.$on('checkInventoryAccounts', (e, a, container) => {
            if (this.container == container) {
                this.checkInventoryAccounts(a);
            }
        });
        this.$scope.$on('stopEditing', (e, a) => {
            this.soeGridOptions.stopEditing(false);
            this.$timeout(() => {
                a.functionComplete();
            }, 100)
        });
    }

    public $onInit() {
        this.modalInstance = this.$uibModal;

        if (!this.currencyHelper) {
            this.currencyHelper = new CurrencyHelper(this.coreService, this.$timeout, this.$q);
        }
        else {
            if (this.actorId && this.actorId !== 0 && !this.currencyHelper.hasLedgerCurrency)
                this.currencyHelper.loadLedgerCurrency(this.actorId);
        }

        this.currencyHelper.notifyCurrencyChanged = false;
        if (this.delaySetCurrency) {
            this.currencyId = this.delayedCurrencyId;
            this.currencyDate = this.delayedCurrencyDate;
            this.currencyRateDate = this.delayedCurrencyRateDate;
            this.delaySetCurrency = false;
        }
        this.showSortButtonsValue = <any>this.showSortButtons === 'true';
        this.showRegenerateButtonValue = <any>this.showRegenerateButton === 'true';
        this.showReloadAccountsButtonValue = <any>this.showReloadAccountsButton === 'true';
        this.showRowNrValue = <any>this.showRowNr === 'true';
        this.showTextValue = <any>this.showText === 'true';
        this.showQuantityValue = <any>this.showQuantity === 'true';
        this.showAttestUserValue = <any>this.showAttestUser === 'true';
        this.showBalanceValue = <any>this.showBalance === 'true';
        this.showAmountSummaryValue = <any>this.showAmountSummary === 'true';
        this.showInstructionsValue = <any>this.showInstructions === 'true';
        this.oneColumnAmountValue = <any>this.oneColumnAmount === 'true';

        this.soeGridOptions.enableRowSelection = false;

        //Changed to get the same behaviour as in the product rows directive
        var rows = this.accountingRows ? this.accountingRows.length : 0;
        if (rows < 8)
            rows = 8;
        if (rows > 30)
            rows = 30;
        this.minRowsToShow = rows;
        /*if (!this.minRowsToShow)
            this.minRowsToShow = 8;*/
        this.soeGridOptions.setMinRowsToShow(this.minRowsToShow);

        if (this.container === AccountingRowsContainers.Voucher) {
            this.soeGridOptions.getRowId = (row: AccountingRowDTO) => row.tempInvoiceRowId; //seems like invoideRowId is the same as tempInvoiceRowId
        }

        this.setupSteppingRules();

        if (this.registerControl)
            this.registerControl({ control: this });

        this.setupDone = true;
    }

    private setupSteppingRules() {
        var mappings =
        {
            dim1Nr(row: AccountingRowDTO) { return row.dim1Stop || row.dim1Mandatory || this.useDimsInRegistration },
            dim2Nr(row: AccountingRowDTO) { return row.dim2Stop || row.dim2Mandatory || (this.useDimsInRegistration && !row.dim2Disabled) },
            dim3Nr(row: AccountingRowDTO) { return row.dim3Stop || row.dim3Mandatory || (this.useDimsInRegistration && !row.dim3Disabled) },
            dim4Nr(row: AccountingRowDTO) { return row.dim4Stop || row.dim4Mandatory || (this.useDimsInRegistration && !row.dim4Disabled) },
            dim5Nr(row: AccountingRowDTO) { return row.dim5Stop || row.dim5Mandatory || (this.useDimsInRegistration && !row.dim5Disabled) },
            dim6Nr(row: AccountingRowDTO) { return row.dim6Stop || row.dim6Mandatory || (this.useDimsInRegistration && !row.dim6Disabled) },
            text(row: AccountingRowDTO) { return row.rowTextStop },
            quantity(row: AccountingRowDTO) { return row.quantityStop },
            unit(row: AccountingRowDTO) { return row.quantityStop },
            debitAmount(row: AccountingRowDTO) { return row.amountStop === 1 },
            creditAmount(row: AccountingRowDTO) { return row.amountStop === 2 || !row.debitAmount }
        };

        this.steppingRules = mappings;
    }

    private getDim1ColumnIndex() {
        return this.soeGridOptions.getColumnIndex('dim1Nr');
    }

    private getDim1Column() {
        return this.soeGridOptions.getColumnByField('dim1Nr');
    }

    private getCreditAmountColumn() {
        return this.soeGridOptions.getColumnByField('creditAmount');
    }

    protected setupCustomToolBar() {
        if (this.showRegenerateButtonValue) {
            const regenerateRowsButtonGroup = ToolBarUtility.createGroup();

            if (this.showRegenerateDetailedButton) {

                regenerateRowsButtonGroup.buttons.push(new ToolBarButton("common.accountingrows.regeneraterows.detailed", "", IconLibrary.FontAwesome, "fa-sync", () => {
                    this.initRegenerateRows(true);
                }, null, () => { return this.isReadonly /*|| this.groupedReadOnly*/ }));
            }

            regenerateRowsButtonGroup.buttons.push(new ToolBarButton("common.accountingrows.regeneraterows", "common.accountingrows.regeneraterowstooltip", IconLibrary.FontAwesome, "fa-sync", () => {
                this.initRegenerateRows();
            }, null, () => { return this.isReadonly /*|| this.groupedReadOnly*/ }));

            this.buttonGroups.push(regenerateRowsButtonGroup);
        }

        var isLoadingAccounts = false;
        if (this.showReloadAccountsButtonValue) {
            this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.accountingrows.reloadaccounts", "common.accountingrows.reloadaccounts", IconLibrary.FontAwesome, "fa-sync", () => {
                isLoadingAccounts = true;
                this.loadAccounts(false).then(() => isLoadingAccounts = false);
            }, () => isLoadingAccounts, () => { return this.isReadonly })));
        }

        this.buttonGroups.push(ToolBarUtility.createGroup(new ToolBarButton("common.newrow", "common.newrow", IconLibrary.FontAwesome, "fa-plus", () => {
            var row = this.addRow(null, null, null, true, true).row;
            this.soeGridOptions.startEditingCell(row, this.getDim1Column());
        }, null, () => { return this.isReadonly })));

        if (this.showSortButtonsValue) {
            this.setupSortGroup("rowNr", null, () => { return this.isReadonly /* || this.groupedReadOnly*/ });
        }
    }

    private afterCellEdit(entity, colDef) {
        const field: string = colDef.field;
        var sourceCurrencyType: TermGroup_CurrencyType = TermGroup_CurrencyType.BaseCurrency;
        var calculateCurrencyAmounts: boolean = false;
        var amountChanged: boolean = false;

        if (field.startsWithCaseInsensitive('debit') || field.startsWithCaseInsensitive('credit') || field === 'amount' || field === 'amountCurrency') {
            var opposite: string;
            if (!this.oneColumnAmountValue)
                opposite = field.startsWithCaseInsensitive('debit') ? field.replace("debit", "credit") : field.replace("credit", "debit");

            // Parse string
            entity[field] = NumberUtility.parseDecimal(entity[field]);

            if (!this.oneColumnAmountValue) {
                // Clear opposite amount
                if (entity[field] !== 0)
                    entity[opposite] = 0;
            }

            // If the amount is negative, transpose credit and debit
            this.transposingCreditAndDebit(field, entity);

            // Get currency type
            if (field === 'amount' || field.endsWithCaseInsensitive('Amount'))
                sourceCurrencyType = TermGroup_CurrencyType.BaseCurrency;
            else if (field === 'amountCurrency' && field.endsWithCaseInsensitive('AmountCurrency'))
                sourceCurrencyType = TermGroup_CurrencyType.TransactionCurrency;
            else if (field.endsWithCaseInsensitive('AmountEntCurrency'))
                sourceCurrencyType = TermGroup_CurrencyType.EnterpriseCurrency;
            else if (field.endsWithCaseInsensitive('AmountLedgerCurrency'))
                sourceCurrencyType = TermGroup_CurrencyType.LedgerCurrency;

            var isCreditEdited: boolean = field.startsWithCaseInsensitive('credit');
            var hasAmount: boolean = entity[field] !== 0;

            entity.isCreditRow = isCreditEdited && hasAmount;
            entity.isDebitRow = !entity.isCreditRow;

            calculateCurrencyAmounts = true;
            amountChanged = true;

        } else if (field === "dim1Nr") {
            var acc = this.findAccount(entity, colDef.soeData.dimIndex);
            this.setRowItemAccounts(entity, acc, true, false, false);
        }

        this.gridRowChanged(entity, sourceCurrencyType, calculateCurrencyAmounts);

        if (amountChanged && this.pendingAutobalance) {
            this.pendingAutobalance = false;
            this.$timeout(() => this.handleCheckForAutoBalance(entity, colDef));
        }
    }

    private transposingCreditAndDebit(field: string, rowItem: AccountingRowDTO) {
        if (field == 'creditAmount' && rowItem.creditAmount < 0) {
            rowItem.debitAmount = Math.abs(rowItem.creditAmount);
            rowItem.creditAmount = 0;
        } else if (field == 'debitAmount' && rowItem.debitAmount < 0) {
            rowItem.creditAmount = Math.abs(rowItem.debitAmount);
            rowItem.debitAmount = 0;
        } else {
            return;
        }
    }

    private handleCheckForAutoBalance(entity, colDef) {
        var autoBalancedColField = this.checkForAndExecuteAutoBalancing(entity, colDef);
        if (autoBalancedColField) {
            this.pendingAutobalance = false;
            this.gridRowChanged(entity, TermGroup_CurrencyType.BaseCurrency, true);
        }
    }

    protected gridRowChanged(row: any, sourceCurrencyType: TermGroup_CurrencyType, calculateCurrencyAmounts: boolean) {
        this.validateAccountingRow(row);
        this.calculateAccountBalances();

        if (calculateCurrencyAmounts) {
            this.calculateAllRowsAllCurrencyAmounts(sourceCurrencyType);
        }

        this.soeGridOptions.refreshRows(row);
        this.$scope.$applyAsync(() => {
            this.messagingService.publish(Constants.EVENT_SET_DIRTY, this.parentGuid ? { guid: this.parentGuid } : null);
            if (this.parentGuid && (row.dim1Id > 0) && (row.debitAmount > 0 || row.creditAmount > 0)) {
                this.sendAccountRowsModified();
            }
        }
        );
    }

    private sendAccountRowsModified() {
        this.messagingService.publish(Constants.EVENT_ACCOUNTING_ROWS_MODIFIED, this.parentGuid);
    }

    public setupGrid() {

        const events: GridEvent[] = [
            new GridEvent(SoeGridOptionsEvent.AfterCellEdit, (entity, colDef) => { this.afterCellEdit(entity, colDef); })
        ];

        this.soeGridOptions.subscribe(events);

        this.soeGridOptions.customTabToCellHandler = (params) => this.tabToNextCell(params);

        this.startLoad();
        this.$q.all([this.loadTerms(),
        this.loadCompanySettings(),
        this.loadAccounts(false),
        this.loadAccountBalances(),
        this.loadVoucherSeries()]).then(() => {
            // Currency helper
            var currencyChanged: CurrencyHelperEvent = new CurrencyHelperEvent(CurrencyEvent.CurrencyChanged, () => {
                this.calculateAllRowsAllCurrencyAmounts(TermGroup_CurrencyType.TransactionCurrency);
            });
            this.currencyHelper.subscribe([currencyChanged]);
            //this.currencyHelper.init(); Moved

            if (this.container.toString() == AccountingRowsContainers.SupplierInvoice.toString() ||
                this.container.toString() == AccountingRowsContainers.SupplierInvoiceAttest.toString())
                this.loadInventoryTriggerAccounts();

            if (!this.isReadonly) {
                // Account distribution helper must me setup after company settings are loaded                        
                this.$q.all([this.loadAccountDistributions(true)]).then(() => {
                    this.gridAndDataIsReady();
                });
            } else {
                this.gridAndDataIsReady();
            }
        });
    }

    private tabToNextCell(params: any): { rowIndex: number, column: any } {
        const { nextCellPosition, previousCellPosition, backwards } = params;
        let { rowIndex, column } = nextCellPosition;

        return this.handleNavigateToNextCell(params, false);
    }

    private loadAccountDistributions(useCache: boolean) {
        var useInVoucher: boolean = null;  //must send null or true, should be fixed on the serverside that false could be sent to
        var useInSupplierInvoice: boolean = null;
        var useInCustomerInvoice: boolean = null;
        var useInImport: boolean = null;

        //Is the accountingrowsdirective inside a voucher, supplierinvoice or customerinvoice
        switch (this.container.toString()) {
            case AccountingRowsContainers.Voucher.toString(): {
                useInVoucher = true;
                break;
            }
            case AccountingRowsContainers.SupplierInvoice.toString(): {
                useInSupplierInvoice = true;
                break;
            }
            case AccountingRowsContainers.CustomerInvoice.toString(): {
                useInCustomerInvoice = true;
                useInImport = true;
                break;
            }
        }

        return this.accountingService.getAccountDistributionHeadsUsedIn(null, TermGroup_AccountDistributionTriggerType.Registration, null, null, null, null, null, null, null, true).then(x => {
            _.forEach(x, (y) => {
                if (y.startDate)
                    y.startDate = new Date(y.startDate);
                if (y.endDate)
                    y.endDate = new Date(y.endDate);
            });

            let accountDistributionHeads;
            let accountDistributionHeadsForImport;
            switch (this.container.toString()) {
                case AccountingRowsContainers.Voucher.toString(): {
                    accountDistributionHeads = _.filter(x, (y) => y.useInVoucher);
                    accountDistributionHeadsForImport = _.filter(x, (y) => !y.useInVoucher && y.useInImport);
                    break;
                }
                case AccountingRowsContainers.SupplierInvoice.toString(): {
                    accountDistributionHeads = _.filter(x, (y) => y.useInSupplierInvoice);
                    accountDistributionHeadsForImport = _.filter(x, (y) => !y.useInSupplierInvoice && y.useInImport);
                    break;
                }
                case AccountingRowsContainers.CustomerInvoice.toString(): {
                    accountDistributionHeads = _.filter(x, (y) => y.useInCustomerInvoice);
                    accountDistributionHeadsForImport = _.filter(x, (y) => !y.useInCustomerInvoice && y.useInImport);
                    break;
                }
                default:
                    accountDistributionHeads = [];
                    accountDistributionHeadsForImport = [];
                    break;
            }

            this.accountDistributionHelper = new DistributionHelper(accountDistributionHeads, accountDistributionHeadsForImport, this.translationService, this.notificationService, this.messagingService, this.accountingService, this.addRow.bind(this), this.deleteRow.bind(this), this.distributionHelperDoneCallback.bind(this), this.container, this.useAutomaticAccountDistribution, this.$q, this.$scope, this.$uibModal, this.urlHelperService);
        });
    }

    private distributionHelperDoneCallback(normalMoveNext: boolean = true) {
        this.updateMandatoryAndAccountNamesOnAllRows();
        this.calculateAccountBalances();
        this.calculateAllRowsAllCurrencyAmounts(TermGroup_CurrencyType.BaseCurrency);
        this.activeAccountingRows.forEach(row => this.validateAccountingRow(row));

        var emptyRows = this.activeAccountingRows.filter(i => i.dim1Id == 0);
        if (emptyRows && emptyRows.length > 0) {
            _.forEach(emptyRows, (row) => { this.deleteRow(row, true); })
        }

        if (!normalMoveNext) {
            //var lastRow = _.last(this.activeAccountingRows);

            //if (!lastRow || lastRow.isModified) {
            //    lastRow = this.addRow().row;
            //}

            var lastRow = this.addRow().row;
            this.soeGridOptions.startEditingCell(lastRow, this.getDim1Column());
        }

        this.soeGridOptions.refreshRows();
    }

    private validateAccountingRow(row: AccountingRowDTO) {
        this.accountDims.forEach((ad, i) => {
            var prop = 'dim' + (i + 1) + 'Nr';
            var val = row[prop];
            var mandatory = row['dim' + (i + 1) + 'Mandatory'];

            if (!val && mandatory) {
                row['dim' + (i + 1) + 'Error'] = this.terms['common.accountingrows.missingaccount'];
            } else if (val && !row['dim' + (i + 1) + 'Name']) { //no name means we couldnt find the account name, which means this is invalid.
                row['dim' + (i + 1) + 'Error'] = this.terms['common.accountingrows.invalidaccount'];
            } else {
                row['dim' + (i + 1) + 'Error'] = null;
            }
        });
    }

    private loadTerms(): ng.IPromise<any> {
        // Columns
        const keys: string[] = [
            "common.accountingrows.rownr",
            "common.date",
            "common.quantity",
            "common.unit",
            "common.text",
            "common.amount",
            "common.amountcurrency",
            "common.debit",
            "common.credit",
            "common.debitcurrency",
            "common.creditcurrency",
            "common.debitentcurrency",
            "common.creditentcurrency",
            "common.debitledgercurrency",
            "common.creditledgercurrency",
            "common.user",
            "common.balance",
            "economy.accounting.voucher.voucherseries",
            "economy.accounting.voucher.vatvoucher",
            "core.deleterow",
            "common.accountingrows.missingaccount",
            "common.accountingrows.invalidaccount",
            "core.aggrid.totals.filtered",
            "core.aggrid.totals.total",
            "common.showtransactions"
        ];

        return this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
        });
    }

    private loadCompanySettings(): ng.IPromise<any> {
        const settingTypes: number[] = [];

        // Common settings
        settingTypes.push(CompanySettingType.AccountingUseDimsInRegistration);

        // Container specific settings            
        switch (this.container.toString()) {
            case AccountingRowsContainers.Voucher.toString(): {
                settingTypes.push(CompanySettingType.AccountingUseQuantityInVoucher);
                settingTypes.push(CompanySettingType.AccountingAllowUnbalancedVoucher);
                settingTypes.push(CompanySettingType.AccountingAutomaticAccountDistribution);
                break;
            }
            case AccountingRowsContainers.SupplierInvoice.toString(): {
                settingTypes.push(CompanySettingType.SupplierInvoiceAutomaticAccountDistribution);
                settingTypes.push(CompanySettingType.SupplierInvoiceUseQuantityInAccountingRows);
                break;
            }
            case AccountingRowsContainers.CustomerInvoice.toString(): {
                settingTypes.push(CompanySettingType.CustomerInvoiceAutomaticAccountDistribution);
                settingTypes.push(CompanySettingType.CustomerInvoiceApplyQuantitiesDuringInvoiceEntry);
                break;
            }
        }

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            // Common settings
            this.useDimsInRegistration = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.AccountingUseDimsInRegistration);

            // Container specific settings           
            switch (this.container.toString()) {
                case AccountingRowsContainers.Voucher.toString(): {
                    this.showQuantityValue = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.AccountingUseQuantityInVoucher);
                    this.allowUnbalancedRows = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.AccountingAllowUnbalancedVoucher);
                    this.useAutomaticAccountDistribution = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.AccountingAutomaticAccountDistribution);
                    break;
                }
                case AccountingRowsContainers.SupplierInvoice.toString(): {
                    this.showQuantityValue = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceUseQuantityInAccountingRows);
                    this.useAutomaticAccountDistribution = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.SupplierInvoiceAutomaticAccountDistribution);
                    break;
                }
                case AccountingRowsContainers.CustomerInvoice.toString(): {
                    this.showQuantityValue = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceApplyQuantitiesDuringInvoiceEntry);
                    this.useAutomaticAccountDistribution = SettingsUtility.getBoolCompanySetting(x, CompanySettingType.CustomerInvoiceAutomaticAccountDistribution);
                    if (this.overrideDiffValidation)
                        this.allowUnbalancedRows = true;
                    break;
                }
                case AccountingRowsContainers.SupplierInvoiceAttest.toString(): {
                    this.allowUnbalancedRows = true;
                }
            }
        });
    }

    private loadAccounts(useCache: boolean): ng.IPromise<any> {
        const getAccounts = useCache ?
            this.accountingService.getAccountDimsSmallMemoryCache(false, false, true, true, true) :
            this.accountingService.getAccountDimsSmall(false, false, true, true, false, true);

        return getAccounts.then(x => {
            this.accountDims = x;

            this.accountDims.forEach(ad => {
                if (!ad.accounts)
                    ad.accounts = [];

                if (ad.accounts.length === 0 || ad.accounts[0].accountId !== 0) {
                    (<any[]>ad.accounts).unshift({ accountId: 0, accountNr: '', name: '', numberName: ' ', state: SoeEntityState.Active });
                }
            });
            this.setRowItemAccountsOnAllRows(false);
        });
    }

    private loadAccountBalances(): ng.IPromise<any> {
        return this.accountingService.getAccountBalances(soeConfig.accountYearId ? soeConfig.accountYearId : 0).then(x => {
            this.accountBalances = x;
        });
    }

    public reloadAccountBalances(): ng.IPromise<any> {
        return this.loadAccountBalances().then(() => {
            this.calculateAccountBalances(false);
            this.soeGridOptions.refreshRows();
        });
    }

    private gridAndDataIsReady() {
        this.controllIsReady = true;

        // actorId is set before constructor has run, therefore the currencyHelper is not available then
        if (this.actorId && this.actorId !== 0 && !this.currencyHelper.hasLedgerCurrency)
            this.currencyHelper.loadLedgerCurrency(this.actorId);

        this.calculateAccountBalances(true);

        if (this.accountingRows) {
            var accIds: number[] = [];
            this.activeAccountingRows.forEach((ar) => {
                this.validateAccountingRow(ar);
            });

            this.setupGridColumns();
            this.syncDistributionRows();
        }
        else {
            this.setupGridColumns();
        }

        this.setupWatchers();

        const debitCreditColumnAggregate = {
            getSeed: () => 0,
            accumulator: (acc, next) => acc + next,
            cellRenderer: this.debitCreditAggregateRenderer.bind(this)
        } as IColumnAggregate;

        if (this.showAmountSummaryValue) {
            this.soeGridOptions.addFooterRow("#accounting-sum-footer-grid", {
                "debitAmount": debitCreditColumnAggregate,
                "creditAmount": debitCreditColumnAggregate,
                "debitAmountCurrency": debitCreditColumnAggregate,
                "creditAmountCurrency": debitCreditColumnAggregate,
                "debitAmountEntCurrency": debitCreditColumnAggregate,
                "creditAmountEntCurrency": debitCreditColumnAggregate,
                "debitAmountLedgerCurrency": debitCreditColumnAggregate,
                "creditAmountLedgerCurrency": debitCreditColumnAggregate
            } as IColumnAggregations);
        }

        this.soeGridOptions.addTotalRow("#accounting-totals-grid", {
            filtered: this.terms["core.aggrid.totals.filtered"],
            total: this.terms["core.aggrid.totals.total"]
        });

        this.currencyHelper.notifyCurrencyChanged = true;
        this.currencyHelper.init();

        if (this.delayedSetRowItemAccountsOnAllRowsIfMissing) {
            this.delayedSetRowItemAccountsOnAllRowsIfMissing = false;
            this.setRowItemAccountsOnAllRowsIfMissing();
        } else if (this.delayedSetRowItemAccountsOnAllRows) {
            this.delayedSetRowItemAccountsOnAllRows = false;
            this.setRowItemAccountsOnAllRows(false);
        }

        this.gridDataLoaded(this.activeAccountingRows);

        // Set grouping     
        if (this.showGrouping && this.groupColumn) {
            this.collapseAllRowGroups = true;
            this.soeGridOptions.groupRowsByColumnAndHide(this.groupColumn, 'agGroupCellRenderer', 1, true, true);
        }

        this.soeGridOptions.finalizeInitGrid();

        this.messagingService.publish(Constants.EVENT_ACCOUNTING_ROWS_READY, this.parentGuid);
    }

    private loadInventoryTriggerAccounts() {
        return this.coreService.getInventoryTriggerAccounts().then(x => {
            this.inventoryTriggerAccounts = x;
        });
    }

    private loadVoucherSeries(): ng.IPromise<any> {
        if (!this.showVoucherSeries || (!soeConfig.accountYearId || soeConfig.accountYearId === 0)) {
            this.showVoucherSeries = false;
            return null;
        }

        if (this.voucherDate) {
            return this.accountingService.getVoucherSeriesByYearDate(this.voucherDate, false, true).then((x: IVoucherSeriesDTO[]) => {
                this.voucherSeries = x;
            });
        }
        else {
            return this.accountingService.getVoucherSeriesByYear(soeConfig.accountYearId, false, true).then((x: IVoucherSeriesDTO[]) => {
                this.voucherSeries = x;
            });
        }
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.accountingRows, () => {
            _.forEach(this.accountingRows, (r) => {
                if (this.showGrouping) {
                    r["dim1NrName"] = r.dim1Nr + " - " + r.dim1Name;
                }
                // Fail safe
                if (!r["dim2Name"])
                    r["dim2Name"] = "";
                if (!r["dim3Name"])
                    r["dim3Name"] = "";
                if (!r["dim4Name"])
                    r["dim4Name"] = "";
                if (!r["dim5Name"])
                    r["dim5Name"] = "";
                if (!r["dim6Name"])
                    r["dim6Name"] = "";
            });

            this.calculateAccountBalances(true);
            this.gridDataLoaded(this.activeAccountingRows);//it is always the same rows or no rows, so no more changes needed?
            this.syncDistributionRows();

        });
        this.$scope.$watch(() => this.isReadonly, () => {
            // When switching between read only and not, some things need to be fixed.
            // Happens for example if you copy a read only voucher into a new voucher that should be editable.
            if (!this.isReadonly) {
                if (!this.accountDistributionHelper)
                    this.loadAccountDistributions(true);

                this.setRowItemAccountsOnAllRows(false);
            }

            _.forEach(this.activeAccountingRows, row => {
                // Use built in read only functionality in soeGridOptions
                row['isReadOnly'] = (this.isReadonly || row['isAttestReadOnly']);
            });
        });
    }

    private setupGridColumns() {

        // Enable auto column for grouping
        this.addColumnIsModified();

        if (this.showGrouping) {
            this.groupColumn = this.soeGridOptions.addColumnText("dim1NrName", "", 30, { enableRowGrouping: true, enableHiding: false, enableResizing: false });
            this.groupColumn.name = "namecolumn";
        }

        this.addColumnNumber("rowNr", this.terms["common.accountingrows.rownr"], 80, { editable: !this.isReadonly, onChanged: this.onRowNrChanged.bind(this) });

        this.accountDims.forEach((ad, i) => {
            let index = i + 1;

            const field = "dim" + index + "Nr";
            const secondRowField = "dim" + index + "Name";
            const errorField = "dim" + index + "Error";
            const editable = (data) => {
                const disabled = data["dim" + index + "Disabled"] || this.isReadonly;

                return !disabled;
            };

            const onCellChanged = ({ data }) => {
                this.onAccountingDimChanged(data, index);
            };

            const allowNavigateFrom = (value, data) => {
                return this.allowNavigationFromTypeAhead(value, data, index);
            };

            this.addColumnTypeAhead(field, ad.name, null, {
                editable: editable.bind(this),
                error: errorField,
                secondRow: secondRowField,
                onChanged: onCellChanged.bind(this),
                typeAheadOptions: {
                    source: (filter) => this.filterAccounts(i, filter),
                    updater: null,
                    allowNavigationFromTypeAhead: allowNavigateFrom.bind(this),
                    displayField: "numberName",
                    dataField: "accountNr",
                    minLength: 0,
                    useScroll: true
                }
            }, {
                dimIndex: index,
            });
        });

        if (this.showTextValue) {
            var colText = this.addColumnText("text", this.terms["common.text"], null);
            colText.name = "columnText";
        }
        if (this.showQuantityValue) {
            var colQua = this.addColumnNumber("quantity", this.terms["common.quantity"], null, { enableHiding: true, decimals: 2, aggFuncOnGrouping: 'sum' });
            colQua.name = "columnQuantity";
            var colUnit = this.addColumnText("unit", this.terms["common.unit"], null, { enableHiding: true });
            colUnit.name = "columnUnit";
        }

        if (this.oneColumnAmountValue || this.debugMode) {
            this.addColumnNumber("amount", this.terms["common.amount"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
        }
        if (!this.oneColumnAmountValue || this.debugMode) {
            this.addColumnNumber("debitAmount", this.terms["common.debit"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.addColumnNumber("creditAmount", this.terms["common.credit"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
        }

        if ((!this.oneColumnAmountValue && this.showTransactionCurrency) || this.debugMode) {
            this.addColumnNumber("debitAmountCurrency", this.terms["common.debitcurrency"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.addColumnNumber("creditAmountCurrency", this.terms["common.creditcurrency"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
        }
        if (this.showEnterpriseCurrency || this.debugMode) {
            this.addColumnNumber("debitAmountEntCurrency", this.terms["common.debitentcurrency"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.addColumnNumber("creditAmountEntCurrency", this.terms["common.creditentcurrency"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
        }
        if (this.showLedgerCurrency || this.debugMode) {
            this.addColumnNumber("debitAmountLedgerCurrency", this.terms["common.debitledgercurrency"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
            this.addColumnNumber("creditAmountLedgerCurrency", this.terms["common.creditledgercurrency"], null, { enableHiding: false, decimals: 2, aggFuncOnGrouping: 'sum' });
        }
        if (this.showAttestUserValue)
            this.addColumnText("attestUserName", this.terms["common.user"], null);
        if (this.showBalanceValue) {
            this.addColumnNumber("balance", this.terms["common.balance"], null, { enableHiding: false, decimals: 2 });
            this.addColumnIcon(null, "", null, { icon: "iconEdit fal fa-search", suppressFilter: true, onClick: this.showTransactions.bind(this), toolTip: this.terms["common.showtransactions"] });
        }

        const col = this.addColumnDelete(this.terms["core.deleterow"], this.initDeleteRow.bind(this), false, (data) => data && !data.isReadOnly);

        const editable = (data) => {
            var disabled = this.isReadonly;
            return !disabled;
        };

        var defs = this.soeGridOptions.getColumnDefs();
        _.forEach(defs, (colDef) => {
            if (colDef.field !== "dim1Nr" &&
                colDef.field !== "dim2Nr" &&
                colDef.field !== "dim3Nr" &&
                colDef.field !== "dim4Nr" &&
                colDef.field !== "dim5Nr" &&
                colDef.field !== "dim6Nr" &&
                colDef.field !== "rowNr" &&
                colDef.field !== "attestUserName" &&
                colDef.field !== "balance" &&
                colDef.field !== 'isModified' &&
                colDef.field !== 'delete' &&
                colDef.field !== 'icon') {
                colDef.editable = editable.bind(this);
            }

            // Add strike through on deleted or processed rows
            let cellClass: string = colDef.cellClass ? colDef.cellClass.toString() : "";
            colDef.cellClass = ({ data }) => {
                if (colDef.field === 'amount' || colDef.field.startsWith('debit') || colDef.field.startsWith('credit')) {
                    cellClass += " grid-text-right";
                }

                if (!data) {
                    return cellClass;
                }

                let cls = cellClass + (data.isDeleted ? " deleted" : "") + (data.isProcessed ? " processed" : "");

                return cls;
            };
        });

        if (this.showAmountSummaryValue)
            this.setupAmountSummary();
    }

    private debitCreditAggregateRenderer({ data, colDef, formatValue }) {
        const field = colDef.field;
        const aggregateDiff = this.calculateAggregateDiff(data, field);

        let secondRow = "";
        if (aggregateDiff) {
            secondRow = "<div class='pull-right errorColor'>" + formatValue(aggregateDiff) + "</div>";
        }

        return "<div class='pull-right' style='vertical-align: top;'>" + formatValue(data[field]) + "</div><br/>" + secondRow;
    }

    private calculateAggregateDiff(data, field): number {
        const oppositeField = this.oppositeColumns[field];
        if (!oppositeField) {
            return null;
        }

        const diff = (data[field] - data[oppositeField]).round(2);

        return diff >= 0 ? null : Math.abs(diff.round(2));
    }

    protected getSecondRowBindingValue(entity, dimIndex) {
        var acc = this.findAccount(entity, dimIndex);
        return acc ? acc.name : null;
    }

    protected allowNavigationFromTypeAhead(value, entity, dimIndex) {

        var currentValue = value;

        if (!currentValue) //if no value, allow it.
            return true;

        var valueHasMatchingAccount = this.accountDims[dimIndex - 1].accounts.filter(acc => acc.state === SoeEntityState.Active && acc.accountNr === currentValue);
        if (valueHasMatchingAccount.length) //if there is a value and it is valid, allow it.
            return true;

        if (dimIndex === 1) {
            // Account number not found, open add account dialog
            this.openAddAccountDialog(entity, currentValue);
        }

        return false;
    }

    private onRowNrChanged(changedInfo: any) {
        const { data, newValue, oldValue } = changedInfo;

        const matches = this.accountingRows.filter(r => r.rowNr === newValue);

        if (matches.length > 1) {
            //move the old row with same number down one spot so the new row will be above....
            matches[0].rowNr = newValue + 1;
        }

        this.soeGridOptions.clearFocusedCell();

        this.accountingRows.sort((a, b) => {
            if (a.rowNr === b.rowNr)
                return 0;
            return (a.rowNr > b.rowNr) ? 1 : -1
        });

        this.reNumberGridRows(this.activeAccountingRows, "rowNr");
        this.soeGridOptions.setData(this.activeAccountingRows);

        this.soeGridOptions.refreshRows();
        this.soeGridOptions.startEditingCell(data, "rowNr");
    }

    protected onAccountingDimChanged(data, dimIndex) {

        var acc = this.findAccount(data, dimIndex);
        data['dim' + dimIndex + 'Id'] = acc ? acc.accountId : 0;
        data['dim' + dimIndex + 'Name'] = acc ? acc.name : "";

        if (dimIndex === 1) {
            this.setRowItemAccounts(data, acc, false);
            if (data.accountDistributionHeadId > 0 && data.isAccrualAccount)
                this.accountDistributionHelper.deleteAccountDistributionEntries(data);
        }

        this.soeGridOptions.refreshRows(data);
        //TODO: find out if its a VatRow.
        //rowItem.IsVatRow = account != null ? vatAccounts.Contains(account.AccountId) : false;
    }

    protected findAccount(entity: AccountingRowDTO, dimIndex: number) {
        var nrToFind = entity['dim' + dimIndex + 'Nr'];

        if (!nrToFind)
            return null;

        const found = this.accountDims[dimIndex - 1].accounts.filter(acc => acc.accountNr === nrToFind && acc.state === SoeEntityState.Active);
        return found.length ? found[0] : null;
    }

    private getAccount(rowItem: AccountingRowDTO, dimIndex: number): IAccountDTO {
        var account = null;
        if (this.accountDims && this.accountDims[dimIndex - 1])
            account = _.find(this.accountDims[dimIndex - 1].accounts, (acc: IAccountDTO) => acc.accountId === rowItem['dim' + dimIndex + 'Id']);

        return account;
    }

    private updateMandatoryAndAccountNamesOnAllRows() {
        this.activeAccountingRows.forEach(ar => {
            if (ar.dim1Id) {
                this.updateMandatoryAndAccountNames(ar, this.getAccount(ar, 1), true, true);
            }
        });
    }

    private updateMandatoryAndAccountNames(rowItem: AccountingRowDTO, account: IAccountDTO, isSalesRow: boolean, setInternalAccount: boolean) {
        // Set standard account
        rowItem.dim1Id = account != null ? account.accountId : 0;
        rowItem.dim1Nr = account != null ? account.accountNr : '';
        rowItem.dim1Name = account != null ? account.name : '';
        rowItem.dim1Disabled = false;
        rowItem.dim1Mandatory = true;
        rowItem.dim1Stop = true;
        rowItem.quantityStop = account != null ? account.unitStop : false;
        rowItem.unit = account != null ? account.unit : '';
        rowItem.amountStop = account != null ? account.amountStop : 1;
        rowItem.rowTextStop = account != null ? account.rowTextStop : true;
        rowItem.isAccrualAccount = account != null ? account.isAccrualAccount : false;

        // Set internal accounts
        if (account != null) {

            this.accountDims.forEach((accDim, i) => {
                var index = i + 1;
                var acc = _.find(accDim.accounts, acc => acc.accountId === rowItem['dim' + index + 'Id']);
                if (acc) {
                    rowItem['dim' + index + 'Nr'] = acc.accountNr;
                    rowItem['dim' + index + 'Name'] = acc.name;
                }
            });

            if (account.accountInternals != null) {
                account.accountInternals.forEach(ai => {
                    var index = _.findIndex(this.accountDims, ad => ad.accountDimNr === ai.accountDimNr) + 1;//index is 0 based, our dims are 1 based

                    rowItem['dim' + index + 'Disabled'] = ai.mandatoryLevel === 1;
                    rowItem['dim' + index + 'Mandatory'] = ai.mandatoryLevel === 2;
                    rowItem['dim' + index + 'Stop'] = ai.mandatoryLevel === 3;
                });
            }
        }
    }

    private setRowItemAccountsOnAllRows(setInternalAccountFromAccount: boolean) {
        if (this.accountDims && this.activeAccountingRows) {
            this.activeAccountingRows.forEach(ar => {
                this.setRowItemAccountsOnRow(ar, setInternalAccountFromAccount);
            });
        }
    }

    private setRowItemAccountsOnAllRowsIfMissing() {
        if (this.accountDims && this.activeAccountingRows) {
            this.activeAccountingRows.forEach(ar => {
                this.setRowItemAccountsOnRowIfMissing(ar);
            });
        }
    }

    private setRowItemAccountsOnRow(rowItem: AccountingRowDTO, setInternalAccountFromAccount: boolean) {
        if (rowItem.dim1Id) {
            this.setRowItemAccounts(rowItem, this.getAccount(rowItem, 1), setInternalAccountFromAccount);
        }
    }

    private setRowItemAccountsOnRowIfMissing(rowItem: AccountingRowDTO) {
        if (rowItem.dim1Id) {
            this.setRowItemAccounts(rowItem, this.getAccount(rowItem, 1), false, true);
        }
    }

    private setRowItemAccounts(rowItem: AccountingRowDTO, account: IAccountDTO, setInternalAccountFromAccount: boolean, internalsFromStdIfMissing: boolean = false, resetInternals = true) {
        // Set standard account
        rowItem.dim1Id = account != null ? account.accountId : 0;
        rowItem.dim1Nr = account != null ? account.accountNr : '';
        rowItem.dim1Name = account != null ? account.name : '';
        rowItem.dim1Disabled = false;
        rowItem.dim1Mandatory = true;
        rowItem.dim1Stop = true;
        rowItem.quantityStop = account != null ? account.unitStop : false;
        rowItem.unit = account != null ? account.unit : '';
        rowItem.amountStop = account != null ? account.amountStop : 1;
        rowItem.rowTextStop = account != null ? account.rowTextStop : true;
        rowItem.isAccrualAccount = account != null ? account.isAccrualAccount : false;

        if (setInternalAccountFromAccount) {
            if (resetInternals) {
                // Clear internal accounts
                rowItem.dim2Id = 0;
                rowItem.dim2Nr = '';
                rowItem.dim2Name = '';
                rowItem.dim2Disabled = false;
                rowItem.dim2Mandatory = false;
                rowItem.dim2Stop = false;
                rowItem.dim3Id = 0;
                rowItem.dim3Nr = '';
                rowItem.dim3Name = '';
                rowItem.dim3Disabled = false;
                rowItem.dim3Mandatory = false;
                rowItem.dim3Stop = false;
                rowItem.dim4Id = 0;
                rowItem.dim4Nr = '';
                rowItem.dim4Name = '';
                rowItem.dim4Disabled = false;
                rowItem.dim4Mandatory = false;
                rowItem.dim4Stop = false;
                rowItem.dim5Id = 0;
                rowItem.dim5Nr = '';
                rowItem.dim5Name = '';
                rowItem.dim5Disabled = false;
                rowItem.dim5Mandatory = false;
                rowItem.dim5Stop = false;
                rowItem.dim6Id = 0;
                rowItem.dim6Nr = '';
                rowItem.dim6Name = '';
                rowItem.dim6Disabled = false;
                rowItem.dim6Mandatory = false;
                rowItem.dim6Stop = false;

                // Set internal accounts
                if (account != null && account.accountInternals != null) {
                    // Get internal accounts from the account
                    account.accountInternals.forEach(ai => {
                        if (ai.accountDimNr > 1) {
                            var index = _.findIndex(this.accountDims, ad => ad.accountDimNr === ai.accountDimNr) + 1;//index is 0 based, our dims are 1 based
                            rowItem[`dim${index}Id`] = ai.accountId || 0;
                            rowItem[`dim${index}Nr`] = ai.accountNr || '';
                            rowItem[`dim${index}Name`] = ai.name || '';
                            rowItem[`dim${index}Disabled`] = ai.mandatoryLevel === 1;
                            rowItem[`dim${index}Mandatory`] = ai.mandatoryLevel === 2;
                            rowItem[`dim${index}Stop`] = ai.mandatoryLevel === 3;
                        }
                    });
                }
            }
            else {
                // Set internal accounts
                if (account != null && account.accountInternals != null) {
                    // Get internal accounts from the account
                    account.accountInternals.forEach(ai => {
                        if (ai.accountDimNr > 1) {
                            var index = _.findIndex(this.accountDims, ad => ad.accountDimNr === ai.accountDimNr) + 1;//index is 0 based, our dims are 1 based
                            if (ai.accountId > 0) {
                                rowItem[`dim${index}Id`] = ai.accountId || 0;
                                rowItem[`dim${index}Nr`] = ai.accountNr || '';
                                rowItem[`dim${index}Name`] = ai.name || '';
                            }

                            rowItem[`dim${index}Disabled`] = ai.mandatoryLevel === 1;
                            rowItem[`dim${index}Mandatory`] = ai.mandatoryLevel === 2;
                            rowItem[`dim${index}Stop`] = ai.mandatoryLevel === 3;
                        }
                    });
                }
            }
        }
        else if (internalsFromStdIfMissing) {
            if (account != null && account.accountInternals != null) {
                // Get internal accounts from the account
                account.accountInternals.forEach(ai => {
                    if (ai.accountDimNr > 1) {
                        var index = _.findIndex(this.accountDims, ad => ad.accountDimNr === ai.accountDimNr) + 1;//index is 0 based, our dims are 1 based
                        if (!rowItem[`dim${index}Id`] || ai.mandatoryLevel === 1) {
                            rowItem[`dim${index}Id`] = ai.accountId || 0;
                            rowItem[`dim${index}Nr`] = ai.accountNr || '';
                            rowItem[`dim${index}Name`] = ai.name || '';
                            rowItem[`dim${index}Disabled`] = ai.mandatoryLevel === 1;
                            rowItem[`dim${index}Mandatory`] = ai.mandatoryLevel === 2;
                            rowItem[`dim${index}Stop`] = ai.mandatoryLevel === 3;
                        }
                    }
                });
            }
        }
        else {
            // Keep internal accounts, just set number and names
            // If not found, keep values from server since it can be an account that has been inactivated but we should 
            // always show choosen account dims...
            var index = 1;
            _.forEach(_.filter(this.accountDims, d => d.accountDimNr !== 1), dim => {
                index = index + 1;
                var internalAccount = _.find(dim.accounts, a => a.accountId === rowItem[`dim${index}Id`]);
                rowItem[`dim${index}Nr`] = internalAccount ? internalAccount.accountNr : rowItem[`dim${index}Nr`] ? rowItem[`dim${index}Nr`] : "";
                rowItem[`dim${index}Name`] = internalAccount ? internalAccount.name : rowItem[`dim${index}Name`] ? rowItem[`dim${index}Name`] : "";

                let dimAccount = account ? _.find(account.accountInternals, (ai) => ai.accountDimNr === dim.accountDimNr) : undefined;
                rowItem[`dim${index}Disabled`] = dimAccount ? dimAccount.mandatoryLevel === 1 : false;
                rowItem[`dim${index}Mandatory`] = dimAccount ? dimAccount.mandatoryLevel === 2 : false;
                rowItem[`dim${index}Stop`] = dimAccount ? dimAccount.mandatoryLevel === 3 : false;
            });
        }
    }

    private openAddAccountDialog(row: AccountingRowDTO, accountNr: string) {

        //Show add account dialog
        var result: any;
        var options: angular.ui.bootstrap.IModalSettings = {
            templateUrl: this.urlHelperService.getCommonDirectiveUrl('AccountingRows/Views', 'AddAccount.html'),
            controller: AddAccountController,
            controllerAs: "ctrl",
            size: 'lg',
            resolve: {
                coreService: () => { return this.coreService },
                accountingService: () => { return this.accountingService },
                $q: () => { return this.$q },
                $timeout: () => { return this.$timeout },
                accountNr: () => { return accountNr },
                buttons: () => { return SOEMessageBoxButtons.OKCancel },
                initialFocusButton: () => { return SOEMessageBoxButton.OK },
                isFromGrid: () => { return true }
            }
        }

        this.$uibModal.open(options).result.then((result: any) => {
            if (result) {
                if (result['type'] === 'copy') {
                    // Copy sys account
                    var sysAccount: ISysAccountStdDTO = result['data'];
                    if (sysAccount)
                        this.accountingService.copySysAccountStd(sysAccount.sysAccountStdId).then((copiedAccount: AccountDTO) => {
                            if (copiedAccount) {
                                this.accountDims[0].accounts.push(copiedAccount);
                                row.dim1Id = copiedAccount.accountId;
                                row.dim1Error = null;
                                this.setRowItemAccounts(row, copiedAccount, true);
                                this.navigateToNextCell(this.soeGridOptions.getColumnDefs()[this.getDim1ColumnIndex()], true);
                            }
                        });
                } else if (result['type'] === 'new') {
                    // Create new account
                    var account: IAccountEditDTO = result['data'];
                    if (account) {
                        this.accountingService.saveAccountSmall(account).then((postResult: IActionResult) => {
                            const newAccount = postResult?.value as AccountDTO
                            if (postResult.success && newAccount && newAccount.accountId > 0) {
                                this.accountDims[0].accounts.push(newAccount);
                                row.dim1Id = newAccount.accountId;
                                row.dim1Error = null;
                                this.setRowItemAccounts(row, newAccount, true);
                                this.navigateToNextCell(this.soeGridOptions.getColumnDefs()[this.getDim1ColumnIndex()], true);
                            } else {
                                this.translationService.translateMany(["common.savefailed", "economy.accounting.account"]).then(terms => {
                                    this.showErrorDialog(postResult.errorMessage)
                                })
                            }
                        });
                    }
                }
            }
        });
    }

    private initRegenerateRows(detailedCodingRows: boolean = false) {

        // Show verification dialog
        const keys: string[] = [
            "core.warning",
            "common.accountingrows.regeneraterowswarning"
        ];
        this.translationService.translateMany(keys).then((terms) => {
            const modal = this.notificationService.showDialog(terms["core.warning"], terms["common.accountingrows.regeneraterowswarning"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {
                    this.messagingService.publish(Constants.EVENT_REGENERATE_ACCOUNTING_ROWS, { detailedCodingRows });
                    this.messagingService.publish(Constants.EVENT_SET_DIRTY, this.parentGuid ? { guid: this.parentGuid } : null);
                }
            }, (reason) => {
                // User cancelled
            });
        });
    }

    public filterAccounts(dimIndex, filter) {
        return _.orderBy(this.accountDims[dimIndex].accounts.filter(acc => {
            if (parseInt(filter))
                return acc.state === SoeEntityState.Active && acc.accountNr.startsWithCaseInsensitive(filter);

            return acc.state === SoeEntityState.Active && (acc.accountNr.startsWithCaseInsensitive(filter) || acc.name.contains(filter));
        }), 'accountNr');
    }

    private calculateAccountBalances(onReady: boolean = false) {
        if (this.activeAccountingRows) {
            var accountBalances = angular.extend({}, this.accountBalances);
            this.activeAccountingRows.forEach(ar => {
                ar.balance = this.getAccountRowBalance(ar, accountBalances, onReady);
                if (onReady) {
                    this.setOrgDebetCreditAmount(ar);
                }
            });
        }
    }

    public setOrgDebetCreditAmount(ar: AccountingRowDTO) {
        if (!ar["orgDebetAmount"] && (ar.voucherRowId || ar.invoiceRowId)) {
            ar["orgDebetAmount"] = ar.debitAmount;
        }

        if (!ar["orgCreditAmount"] && (ar.voucherRowId || ar.invoiceRowId)) {
            ar["orgCreditAmount"] = ar.creditAmount;
        }
    }

    private getAccountRowBalance(ar: AccountingRowDTO, accountBalances, onReady: boolean = false) {
        var accountId = ar.dim1Id;
        if (!accountId)
            return null;

        if (accountBalances[accountId] === undefined)
            accountBalances[accountId] = 0;

        if (ar["orgCreditAmount"] && ar.isModified) {
            var orgCreditAmount: number = ar["orgCreditAmount"];
            accountBalances[accountId] += orgCreditAmount;
        }

        if (ar["orgDebetAmount"] && ar.isModified) {
            var orgDebetAmount: number = ar["orgDebetAmount"];
            accountBalances[accountId] -= orgDebetAmount;
        }

        if (ar.debitAmount && !onReady && ar.isModified)
            accountBalances[accountId] += parseFloat(<any>ar.debitAmount) || 0;

        if (ar.creditAmount && !onReady && ar.isModified)
            accountBalances[accountId] -= parseFloat(<any>ar.creditAmount) || 0;

        return accountBalances[accountId];
    }

    protected handleNavigateToNextCell(params: any, skipChecks?: boolean): { rowIndex: number, column: any } {
        const { nextCellPosition, previousCellPosition, backwards } = params;
        const nextColumnCaller: (column: any) => any = backwards ? this.soeGridOptions.getPreviousVisibleColumn : this.soeGridOptions.getNextVisibleColumn;

        let { rowIndex, column } = nextCellPosition;
        let row: AccountingRowDTO = this.soeGridOptions.getVisibleRowByIndex(rowIndex).data;

        if (rowIndex > 0 && previousCellPosition.field === this.getDim1Column().field) {
            this.tryCompleteEditOnBalancedAccount(row);
        }

        while (!!column && !!this.steppingRules) {
            const { colDef } = column;
            if (this.soeGridOptions.isCellEditable(row, colDef)) {
                const steppingRule = this.steppingRules[colDef.field];
                const stop = !!steppingRule ? steppingRule.call(this, row) : false;

                if (stop) {
                    return { rowIndex, column };
                }
            }

            column = nextColumnCaller(column);
        }

        column = previousCellPosition.column;

        if (!skipChecks) {
            this.pendingAutobalance = true;
            this.$timeout(() => this.handleCheckForAutoBalance(row, column.colDef));

            //let accdistHandlesTheRest = false;

            if (row.isModified)
                this.$timeout(() => {
                    const accdistHandlesTheRest = this.accountDistributionHelper.checkAccountDistribution(row, this.parentGuid);
                    if (accdistHandlesTheRest) {
                        this.soeGridOptions.clearFocusedCell();
                    }
                });

            //if (accdistHandlesTheRest) {
            //    console.log("accdistHandlesTheRest");
            //    return previousCellPosition;
            //}
        }

        const isInventoryDialogOpen = this.checkInventoryAccounts(row);

        if (isInventoryDialogOpen) {
            return null;
        }

        const nextRowResult = backwards ? this.findPreviousRow(row) : this.findNextRow(row);
        const newRowIndex = nextRowResult ? nextRowResult.rowIndex : this.addRow().rowIndex;

        return { rowIndex: newRowIndex, column: backwards ? this.getCreditAmountColumn() : this.getDim1Column() };
    }

    protected navigateToNextCell(coldef: uiGrid.IColumnDef, skipChecks = false) {
        const startColumn = this.soeGridOptions.getColumnByField(coldef.field);
        const nextColumn = this.soeGridOptions.getNextVisibleColumn(startColumn) || this.getDim1Column();
        const rowIndex = this.soeGridOptions.getRowIndexFor(this.soeGridOptions.getCurrentRow());

        const params = {
            backwards: false,
            nextCellPosition: {
                column: nextColumn,
                rowIndex
            },
            previousCellPosition: {
                column: startColumn,
                rowIndex
            }
        };
        //TODO: Wont' work, need to call "tabToNextCell".
        this.handleNavigateToNextCell(params, skipChecks);
    }

    private checkInventoryAccounts(row: AccountingRowDTO): boolean {
        const inventoryId: number = row.inventoryId ? row.inventoryId : 0;
        const invAccount = _.filter(this.inventoryTriggerAccounts, i => i.key == row.dim1Id && inventoryId == 0)[0];
        let isInventoryDialogOpen = false;
        if (invAccount) {
            this.openInventoryDialog(row, invAccount.value);
            isInventoryDialogOpen = true;
        }

        return isInventoryDialogOpen;
    }

    private tryCompleteEditOnBalancedAccount(row: AccountingRowDTO) {
        if (row.dim1Nr || this.getCurrentDiff() != 0) {
            return;
        }

        this.onEditCompletedForBalancedAccount();
        this.$timeout(() => {
            //this.initDeleteRow(row); //looks nice if the row is empty
            this.soeGridOptions.clearFocusedCell();
        });
    }

    private openInventoryDialog(row: AccountingRowDTO, writeOffTemplateId: number) {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getGlobalUrl("Shared/Economy/Inventory/Inventories/Views/edit.html"),
            controller: InventoryEditController,
            controllerAs: 'ctrl',
            bindToController: true,
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope
        });
        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, amount: row.amount, writeOffTemplateId: writeOffTemplateId, purchaseDate: this.purchaseDate });
        });
        modal.result.then((result) => {
            row.inventoryId = result.inventoryId;
        });
    }

    private getCurrentDiff(): any {
        var sums = this.activeAccountingRows.reduce((aggr: any, curr: AccountingRowDTO) => {
            aggr.credit += parseFloat(<any>curr.creditAmount) || 0;
            aggr.debit += parseFloat(<any>curr.debitAmount) || 0;
            return aggr;
        }, { credit: 0, debit: 0 });

        var diff = sums.credit - sums.debit;
        return parseFloat(diff.toFixed(2));//js cant represent some numbers, so we convert it to fixed and back again, which seems to fix some issues. should probably lower 5 to 2 if possible, but i dont know how many decimals you use.
    }

    private checkForAndExecuteAutoBalancing(row: AccountingRowDTO, colDef: uiGrid.IColumnDef): boolean {
        if (row.creditAmount !== undefined && !row.creditAmount && !row.debitAmount) {
            var diff = this.getCurrentDiff();

            if (diff === 0)
                return false;

            var idx = colDef.field.indexOf("Amount");
            var currencyType = colDef.field.substr(idx);

            if (diff > 0) {
                row.debitAmount = diff;
                row.isDebitRow = true;
                row.isCreditRow = false;
                row.amountCurrency = Math.abs(diff);
                return true;
            } else if (diff < 0) {
                row.creditAmount = Math.abs(diff);
                row.isDebitRow = false;
                row.isCreditRow = true;
                row.amountCurrency = diff;
                return true;
            }
        }

        return null;
    }

    private scrollToColumn(row: AccountingRowDTO, fieldName: string) {
        var colDef = this.soeGridOptions.getColumnDefByField(fieldName);
        this.soeGridOptions.startEditingCell(<any>row, colDef);
    }

    private syncDistributionRows() {
        if (this.accountDistributionHelper)
            this.accountDistributionHelper.setAccountingRows(this.activeAccountingRows);
    }

    private checkAccountDistribution(row: AccountingRowDTO) {
        if (this.accountDistributionHelper)
            this.accountDistributionHelper.checkAccountDistribution(row, this.parentGuid);
    }

    public addRow(accountId?: number, amount?: number, isDebit?: boolean, setFocus = false, insertAtCurrentRow = false): { rowIndex: number, row: any } {

        var row = new AccountingRowDTO();
        let reNumberRows = false;
        row.tempRowId = this.internalIdCounter;
        row.tempInvoiceRowId = this.internalIdCounter;
        this.internalIdCounter++;

        row.type = AccountingRowType.AccountingRow;
        row.rowNr = AccountingRowDTO.getNextRowNr(this.activeAccountingRows);
        row.state = SoeEntityState.Active;

        let selectedRow = undefined;

        var account = null;
        if (this.container === AccountingRowsContainers.SupplierInvoiceAttest) {
            row.type = AccountingRowType.SupplierInvoiceAttestRow;
            row.attestUserId = CoreUtility.userId;
            row.attestStatus = SupplierInvoiceAccountRowAttestStatus.New;
            row.isDebitRow = true;
            row.isCreditRow = false;
            row.isModified = true;

            if (this.accountingRows.length === 0) {
                // Add default account and amount
                if (this.accountDims && this.accountDims.length > 0 && this.accountDims[0].accounts)
                    account = _.find(this.accountDims[0].accounts, a => a.accountId === this.defaultAttestRowDebitAccountId);

                row.amount = this.defaultAttestRowAmount;
                this.calculateRowCurrencyAmounts(row, TermGroup_CurrencyType.BaseCurrency, TermGroup_CurrencyType.TransactionCurrency);
            }
        }

        if (this.container == AccountingRowsContainers.Voucher || this.container == AccountingRowsContainers.CustomerInvoice || this.container == AccountingRowsContainers.SupplierInvoice) {
            row.date = this.voucherDate ?? new Date();
        }

        if (this.container == AccountingRowsContainers.Voucher && this.accountingRows.length > 0) {
            var prevRow = this.accountingRows[this.accountingRows.length - 1];
            if (prevRow && prevRow.text) {
                row.text = prevRow.text;
            }
        }

        if (insertAtCurrentRow) {
            selectedRow = this.soeGridOptions.getCurrentRow();
            if (!selectedRow) {
                var selectRows = this.soeGridOptions.getSelectedRows();
                if (selectRows.length > 0) {
                    selectedRow = selectRows[0];
                }
            }

            if (selectedRow) {
                reNumberRows = true;
                var childRows = this.accountingRows.filter((r) => r.parentRowId === selectedRow.tempRowId);
                if (childRows && childRows.length > 0) {
                    var childRow = _.last(_.orderBy(childRows, 'rowNr'));
                    if (childRow)
                        row.rowNr = childRow.rowNr;
                    else
                        row.rowNr = selectedRow.rowNr;
                }
                else {
                    row.rowNr = selectedRow.rowNr + 1;
                }

                // Switch id's if necessary
                var matchedRow = _.find(this.activeAccountingRows, (r) => r.rowNr === row.rowNr && r.tempRowId < 100);
                if (matchedRow) {
                    const rowId = row.tempRowId;
                    row.tempRowId = row.tempInvoiceRowId = matchedRow.tempRowId;
                    matchedRow.tempRowId = matchedRow.tempInvoiceRowId = rowId;
                }
            }
            else {
                row.rowNr = AccountingRowDTO.getNextRowNr(_.filter(this.accountingRows, r => r.state === SoeEntityState.Active));
            }
        } else {
            row.rowNr = AccountingRowDTO.getNextRowNr(this.accountingRows);
        }

        let focusColumn: number = this.getRowNrColumn();

        this.setRowItemAccounts(row, account, true);
        this.validateAccountingRow(row);

        // Add the row to the collection
        if (!this.accountingRows)
            this.accountingRows = [];

        this.accountingRows.push(row);

        const rowIndx = this.soeGridOptions.addRow(row);

        if (reNumberRows) {
            this.reNumberGridRows(this.activeAccountingRows, "rowNr");
            this.resetRows(true);
            _.forEach(_.filter(this.activeAccountingRows, r => r.rowNr > row.rowNr), accountRow => {
                this.setRowAsModified(accountRow, false);
            });
        }

        if (setFocus && focusColumn) {
            this.soeGridOptions.startEditingCell(row, focusColumn);
        }

        this.syncDistributionRows();
        return { rowIndex: rowIndx, row: row };
    }

    public setRowAsModified(row: AccountingRowDTO, notify: boolean = true) {
        if (row) {
            row.isModified = true;
            if (notify)
                this.setParentAsModified();
        }
    }

    private resetRows( keepSelectedRows = false) {
        const selectedRowIds = keepSelectedRows ? this.soeGridOptions.getSelectedIds("tempRowId") : [];

        super.gridDataLoaded(this.activeAccountingRows);
        this.soeGridOptions.refreshRows();

        if (keepSelectedRows && selectedRowIds && selectedRowIds.length > 0) {
            const selectedRows = this.activeAccountingRows.filter(p => selectedRowIds.some(s => s === p.tempRowId));
            this.soeGridOptions.selectRows(selectedRows);
        }
    }

    private setParentAsModified() {
        this.$scope.$applyAsync(() => this.messagingService.publish(Constants.EVENT_SET_DIRTY, { guid: this.parentGuid }));
    }
    private getRowNrColumn() {
        return this.soeGridOptions.getColumnByField('rowNr');
    }
    public createDefaultAccountingRow(accountId?: number, amount?: number, isDebit?: boolean): { rowIndex: number, row: any } {
        var row = new AccountingRowDTO();

        row.tempRowId = this.internalIdCounter;
        row.tempInvoiceRowId = this.internalIdCounter;
        this.internalIdCounter++;

        row.type = AccountingRowType.AccountingRow;
        row.rowNr = AccountingRowDTO.getNextRowNr(this.activeAccountingRows);

        var account = accountId ? _.find(this.accountDims[0].accounts, a => a.accountId === accountId) : null;
        row.isDebitRow = isDebit ? isDebit : true;
        row.isCreditRow = isDebit ? !isDebit : false;
        row.isModified = true;

        row.setDebitAmount(TermGroup_CurrencyType.TransactionCurrency, isDebit ? amount : 0);
        row.setCreditAmount(TermGroup_CurrencyType.TransactionCurrency, isDebit ? 0 : amount);
        this.calculateRowAllCurrencyAmounts(row, TermGroup_CurrencyType.TransactionCurrency, true);

        if (this.container == AccountingRowsContainers.Voucher && this.accountingRows.length > 0) {
            var prevRow = this.accountingRows[this.accountingRows.length - 1];
            if (prevRow && prevRow.text) {
                row.text = prevRow.text;
            }
        }

        this.setRowItemAccounts(row, account, true);
        this.validateAccountingRow(row);
        const rowIndex = this.soeGridOptions.addRow(row);
        this.accountingRows.push(row);
        this.syncDistributionRows();

        return { rowIndex, row };
    }

    //protected onDeleteEvent(row: AccountingRowDTO) {
    //    if (this.accountDistributionHelper) {
    //        this.accountDistributionHelper.checkDeleteAccountDistribution(row).then((result) => {
    //            // User cancelled
    //            if (result === null)
    //                return;

    //            // Child rows are deleted in helper
    //            // Delete selected row
    //            this.initDeleteRow(row);
    //        });
    //    } else {
    //        this.initDeleteRow(row);
    //    }
    //}

    protected showTransactions(row: AccountingRowDTO) {
        const modal = this.modalInstance.open({
            templateUrl: this.urlHelperService.getCoreViewUrl("gridCompositionAg.html"),
            controller: TransactionGridController,
            controllerAs: 'ctrl',
            backdrop: 'static',
            size: 'xl',
            windowClass: 'fullsize-modal',
            scope: this.$scope,
            resolve: {
                coreService: () => { return this.coreService },
                accountingService: () => { return this.accountingService },
                translationService: () => { return this.translationService },
                urlHelperService: () => { return this.urlHelperService },
                messagingService: () => { return this.messagingService },
                notificationService: () => { return this.notificationService },
            }
        });

        modal.rendered.then(() => {
            this.$scope.$broadcast(Constants.EVENT_ON_INIT_MODAL, { modal: modal, parameters: { transactionMode: true, accountId: row.dim1Id } });
        });

        modal.result.then((result: any) => {
            if (result && result.reload)
                this.loadGridData();
        });
    }

    protected initDeleteRow(row: AccountingRowDTO, renumber: boolean = true) {
        if (this.accountDistributionHelper) {
            this.accountDistributionHelper.checkDeleteAccountDistribution(row).then((result) => {
                // User cancelled
                if (result === null)
                    return;

                // Child rows are deleted in helper
                // Delete selected row
                this.deleteRow(row, renumber);
            });
        } else {
            this.deleteRow(row, renumber);
        }
    }

    protected deleteRow(row: AccountingRowDTO, renumber: boolean = true) {
        // Get row number of row above the specified
        var rowNbr: number = row.rowNr - 1;

        // If a saved row, mark it as deleted, otherwise just remove it from collection
        if (row.invoiceRowId || row.invoiceAccountRowId) {
            row.isDeleted = true;
            row.isModified = true;
            row.state = SoeEntityState.Deleted;
            if (this.container == AccountingRowsContainers.SupplierInvoiceAttest) {
                row.attestStatus = SupplierInvoiceAccountRowAttestStatus.Deleted;
                row['isReadOnly'] = true;
            } else {
                this.soeGridOptions.deleteRow(row);
            }
        } else {
            var index = this.accountingRows.findIndex(r => r.rowNr == row.rowNr && !r.isDeleted);
            if (index >= 0)
                this.accountingRows.splice(index, 1);

            this.soeGridOptions.deleteRow(row);
            this.syncDistributionRows();
        }

        this.calculateAccountBalances();

        if (renumber) {
            this.reNumberGridRows(this.activeAccountingRows, "rowNr");
        }

        this.soeGridOptions.refreshRows();

        this.messagingService.publish(Constants.EVENT_ROW_DELETED, row);
        this.messagingService.publish(Constants.EVENT_SET_DIRTY, this.parentGuid ? { guid: this.parentGuid } : null);
    }

    private calculateAllRowsAllCurrencyAmounts(sourceCurrencyType: TermGroup_CurrencyType, refreshGrid = true): ng.IPromise<any> {
        if (this.isReadonly)
            return;

        let promises: ng.IPromise<any>[] = [];

        _.forEach(this.activeAccountingRows, (row) => {
            promises.push(this.calculateRowAllCurrencyAmounts(row, sourceCurrencyType, false));
        });

        return this.$q.all(promises).then(vals => {
            if (this.soeGridOptions && refreshGrid)
                this.soeGridOptions.refreshRows();
        });
    }

    private calculateRowAllCurrencyAmounts(row: AccountingRowDTO, sourceCurrencyType: TermGroup_CurrencyType, refreshGrid: boolean): ng.IPromise<any> {
        if (this.isReadonly)
            return;

        return this.$q.all([
            this.calculateRowCurrencyAmounts(row, sourceCurrencyType, TermGroup_CurrencyType.BaseCurrency),
            this.calculateRowCurrencyAmounts(row, sourceCurrencyType, TermGroup_CurrencyType.EnterpriseCurrency),
            this.calculateRowCurrencyAmounts(row, sourceCurrencyType, TermGroup_CurrencyType.LedgerCurrency),
            this.calculateRowCurrencyAmounts(row, sourceCurrencyType, TermGroup_CurrencyType.TransactionCurrency)]).then(() => {
                if (refreshGrid && this.soeGridOptions)
                    this.soeGridOptions.refreshRows();
            });
    }

    private calculateRowCurrencyAmounts(row: AccountingRowDTO, sourceCurrencyType: TermGroup_CurrencyType, targetCurrencyType: TermGroup_CurrencyType): ng.IPromise<any> {
        if (sourceCurrencyType === targetCurrencyType)
            return;

        if (this.oneColumnAmountValue) {
            return this.$q.all([this.currencyHelper.getCurrencyAmount(row.getAmount(sourceCurrencyType), sourceCurrencyType, targetCurrencyType).then(amount => { row.setAmount(targetCurrencyType, amount); })]);
        } else {
            return this.$q.all([
                this.currencyHelper.getCurrencyAmount(row.getDebitAmount(sourceCurrencyType), sourceCurrencyType, targetCurrencyType).then(amount => { row.setDebitAmount(targetCurrencyType, amount); }),
                this.currencyHelper.getCurrencyAmount(row.getCreditAmount(sourceCurrencyType), sourceCurrencyType, targetCurrencyType).then(amount => { row.setCreditAmount(targetCurrencyType, amount) })
            ])
        }
    }

    private convertAmount(field: string, amount: number, sourceCurrencyType: TermGroup_CurrencyType) {
        const item = {
            field: field,
            baseCurrencyAmount: amount,
            enterpriceCurrencyAmount: amount,
            ledgerCurrencyAmount: amount,
            transactionCurrencyAmount: amount,
            parentRecordId: this.parentRecordId
        };

        this.$q.all([
            this.currencyHelper.getCurrencyAmount(amount, sourceCurrencyType, TermGroup_CurrencyType.BaseCurrency).then(amount => { item.baseCurrencyAmount = amount; }),
            this.currencyHelper.getCurrencyAmount(amount, sourceCurrencyType, TermGroup_CurrencyType.EnterpriseCurrency).then(amount => { item.enterpriceCurrencyAmount = amount; }),
            this.currencyHelper.getCurrencyAmount(amount, sourceCurrencyType, TermGroup_CurrencyType.LedgerCurrency).then(amount => { item.ledgerCurrencyAmount = amount; }),
            this.currencyHelper.getCurrencyAmount(amount, sourceCurrencyType, TermGroup_CurrencyType.TransactionCurrency).then(amount => { item.transactionCurrencyAmount = amount; })]).then(() => {
                this.onAmountConverted({ item: item });
            });
    }

    private setupAmountSummary() {

        const groups = [
            new AmountSummaryGroup('creditAmount', 'debitAmount'),
            new AmountSummaryGroup('creditAmountCurrency', 'debitAmountCurrency'),
            new AmountSummaryGroup('creditAmountEntCurrency', 'debitAmountEntCurrency'),
            new AmountSummaryGroup('creditAmountLedgerCurrency', 'debitAmountLedgerCurrency')
        ];

        groups.forEach(g => {
            this.oppositeColumns[g.creditProp] = g.debitProp;
            this.oppositeColumns[g.debitProp] = g.creditProp;
        });
    }

    private debug() {
        this.debugMode = true;
        this.soeGridOptions.clearColumnDefs();
        this.setupGridColumns();
        this.gridDataLoaded(this.activeAccountingRows);
        console.log(this.accountingRows);
    }

    private setRowGroupExpension() {
        this.soeGridOptions.setAllGroupExpended(!this.collapseAllRowGroups);
    }
}

class AmountSummaryGroup {
    public creditProp: string;
    public debitProp: string;

    constructor(creditProp: string, debitProp: string) {
        this.creditProp = creditProp;
        this.debitProp = debitProp;
    }
}