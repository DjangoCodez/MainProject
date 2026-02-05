import { ICoreService } from "../../../../Core/Services/CoreService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { IUrlHelperService, IUrlHelperServiceProvider } from "../../../../Core/Services/UrlHelperService";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { Guid } from "../../../../Util/StringUtility";
import { EditControllerBase, IEditControllerBase } from "../../../../Core/Controllers/EditControllerBase";
import { AccountingRowDTO } from "../../../../Common/Models/AccountingRowDTO";
import { ISupplierService } from "../../../../Shared/Economy/Supplier/SupplierService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { GridControllerBase } from "../../../../Core/Controllers/GridControllerBase";
import { GridEvent } from "../../../../Util/SoeGridOptions";
import { SoeGridOptionsEvent, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { TabMessage } from "../../../../Core/Controllers/TabsControllerBase1";
import { EditController as CustomerInvoicesEditController } from "../../../../Common/Customer/Invoices/EditController";
import { EditController as CustomerPaymentsEditController } from "../../../../Common/Customer/Payments/EditController";
import { VoucherHeadDTO } from "../../../../Common/Models/VoucherHeadDTO";
import { Feature, CompanySettingType, TermGroup_Languages, SoeInvoiceMatchingType, SoeOriginType, TermGroup_BillingType, SupplierAccountType, AccountingRowType, SoeEntityState, SoeInvoiceType } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";
import { EditController as SupplierInvoicesEditController } from "../../../../Shared/Economy/Supplier/Invoices/EditController";
import { EditController as SupplierPaymentsEditController } from "../../../../Shared/Economy/Supplier/Payments/EditController";

export class EditController extends GridControllerBase implements IEditControllerBase {
    public guid: Guid;
    private supplierInvoicePermission: boolean;
    private terms: { [index: string]: string; };
    private accountYearId: number;
    public totalInvoice: string;
    public totalPayment: string;
    public totalSum: string;
    public selectedInvoice: string;
    public selectedPayment: string;
    public selectedSum: string;
    public voucherSeries: any;
    public selectedvoucherSeries;
    public selectedDate: Date = new Date();
    public comment: string;
    public accountVat: boolean = true;
    public matchCodes;
    private defaultCreditAccountId: number;
    private defaultDebitAccountId: number;
    private defaultVatAccountId: number;
    private selectedMatchCode: any;
    public accountingRows: AccountingRowDTO[] = [];
    private editValidation: ng.IFormController;
    private supplierId: number;

    //Styling
    private styleNonClicked: string = "btn btn-sm btn-default col-sm-2 margin-small-bottom margin-small-right";
    private styleClicked: string = "btn btn-sm btn-primary col-sm-2 margin-small-bottom margin-small-right";

    //@ngInject
    constructor($http,
        $templateCache,
        $timeout: ng.ITimeoutService,
        $uibModal,
        private $filter: ng.IFilterService,
        coreService: ICoreService,
        private supplierService: ISupplierService,
        translationService: ITranslationService,
        messagingService: IMessagingService,
        notificationService: INotificationService,
        urlHelperService: IUrlHelperService,
        uiGridConstants: uiGrid.IUiGridConstants,
        private $q: ng.IQService,
        private $scope: ng.IScope) {

        super("Soe.Economy.Supplier.Invoice.Matches", "economy.supplier.invoice.matches.supplier", Feature.Economy_Supplier_Invoice_Matching, $http, $templateCache, $timeout, $uibModal, coreService, translationService, urlHelperService, messagingService, notificationService, uiGridConstants);
        this.accountYearId = soeConfig.accountYearId;
    }

    protected init() {
        this.supplierId = this.parameters.id;
        this.$q.all([
            this.loadModifyPermissions(),
            this.loadVoucherSeries(),
            this.loadMatchCodes(),
            this.loadCompanyAccounts(),
            this.loadAccountPeriod(this.accountYearId)
        ]).then(x => this.setupMatchesGrid())
            .then(() => this.search());
    }

    protected edit(row) {
        this.openSupplierInvoice(row);
    }

    public setupMatchesGrid() {
        this.soeGridOptions.showColumnFooter = true;
        // Columns
        var keys: string[] = [
            "economy.supplier.invoice.matches.invoicenr",
            "economy.supplier.invoice.supplierordernumber",
            "economy.supplier.invoice.matches.paymentnr",
            "economy.supplier.invoice.matches.totalamount",
            "economy.supplier.invoice.matches.matchamount",
            "economy.supplier.invoice.matches.totalmatchamount",
            "economy.supplier.invoice.matches.ordernumber",
            "common.type",
            "core.edit",
            "economy.supplier.invoice.matches.debitinvoice",
            "economy.supplier.invoice.matches.creditinvoice",
            "economy.supplier.invoice.matches.interestinvoice",
            "economy.supplier.invoice.matches.demandinvoice",
            "economy.supplier.invoice.matches.payment",
            "economy.supplier.invoice.matches.paymentsuggestion",
            "core.warning",
            "economy.accounting.voucher.accountstandardmissing",
            "economy.accounting.voucher.accountinternalmissing",
            "economy.accounting.voucher.invalidrowamount",
            "economy.accounting.voucher.unbalancedrows," +
            "core.information",
            "economy.supplier.invoice.matches.supplierswillmatch",
            "economy.supplier.invoice.matches.vouchercreated",
            "economy.supplier.invoice.invoice",
            "economy.supplier.payment.payment",
            "common.customer.invoices.invoice",
            "common.customer.payment.payment"
        ];

        this.translationService.translateMany(keys).then((terms) => {
            this.terms = terms;
            super.addColumnNumber("invoiceNr", terms["economy.supplier.invoice.matches.invoicenr"], "10%", null, null, "");
            super.addColumnNumber("orderNr", terms["economy.supplier.invoice.matches.ordernumber"], "10%", null, null, "");
            super.addColumnNumber("paymentNr", terms["economy.supplier.invoice.matches.paymentnr"], "10%", null, null, "");
            super.addColumnNumber("invoiceTotalAmount", terms["economy.supplier.invoice.matches.totalamount"], "10%", null, 2, "");
            super.addColumnNumber("invoicePayedAmount", terms["economy.supplier.invoice.matches.totalmatchamount"], "10%", null, 2, "");
            super.addColumnText("billingTypeName", terms["common.type"], null);
            super.addColumnNumber("invoiceMatchAmount", terms["economy.supplier.invoice.matches.matchamount"], "10%", null, 2, "");
            if (this.supplierInvoicePermission)
                this.soeGridOptions.addColumnEdit(terms["core.edit"], "openSupplierInvoice");
            this.soeGridOptions.subscribe([
                new GridEvent(SoeGridOptionsEvent.RowSelectionChanged, row => {
                    this.updateSelectedInfo();
                    this.accountingRows = [];
                })
            ]);
        });
    }

    private loadModifyPermissions(): ng.IPromise<any> {
        var featureIds: number[] = [];
        featureIds.push(Feature.Economy_Supplier_Invoice);

        return this.coreService.hasReadOnlyPermissions(featureIds)
            .then((x) => {
                if (x[Feature.Economy_Supplier_Invoice]) {
                    this.supplierInvoicePermission = true;
                }
            });
    }

    private loadCompanyAccounts(): ng.IPromise<any> {
        var settingTypes: number[] = [];

        settingTypes.push(CompanySettingType.AccountSupplierDebt);
        settingTypes.push(CompanySettingType.AccountSupplierPurchase);
        settingTypes.push(CompanySettingType.AccountCommonVatReceivable);

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            this.defaultCreditAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountSupplierDebt);
            this.defaultDebitAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountSupplierPurchase);
            this.defaultVatAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCommonVatReceivable);

            // Load default VAT rate for the company
            this.loadVatRate(this.defaultVatAccountId);
        });
    }

    private loadVatRate(accountId: number) {
        if (accountId === 0) {
            this.setDefaultVatRate();
            return;
        }

        this.supplierService.getAccountSysVatRate(accountId).then(x => {
            this.defaultVatRate = x;
            this.setDefaultVatRate();
        });
    }

    private setDefaultVatRate() {
        if (this.defaultVatRate === 0)
            this.defaultVatRate = CoreUtility.sysCountryId == TermGroup_Languages.Finnish ? Constants.DEFAULT_VAT_RATE_FIN : Constants.DEFAULT_VAT_RATE;

        this.vatRate = this.defaultVatRate;
    }

    private loadMatchCodes(): ng.IPromise<any> {
        return this.supplierService.getMatchCodes(SoeInvoiceMatchingType.SupplierInvoiceMatching, false).then(
            x => {
                this.matchCodes = x;

                _.forEach(this.matchCodes, x => {
                    x.style = this.styleNonClicked;
                });
            });
    }

    private loadVoucherSeries(): ng.IPromise<any> {
        return this.supplierService.getVoucherSeriesByYear(this.accountYearId, false)
            .then((x) => {
                this.voucherSeries = x;
            }).then(this.loadDefaultVoucherSeriesId());
    }

    private loadDefaultVoucherSeriesId() {
        return this.supplierService.getDefaultVoucherSeriesId(this.accountYearId, CompanySettingType.SupplierInvoiceVoucherSeriesType).then((x) => {
            this.defaultvoucherSeriesIdId = x;
            this.selectedvoucherSeries = _.find(this.voucherSeries, { voucherSeriesId: this.defaultvoucherSeriesIdId });

        });
    }

    public openSupplierInvoice(row: any) {

        switch (row.type) {
            case SoeOriginType.SupplierInvoice:
                var message = new TabMessage(
                    `${this.terms["economy.supplier.invoice.invoice"]} ${row.invoiceNr}`,
                    row.invoiceId,
                    SupplierInvoicesEditController,
                    { id: row.invoiceId },
                    this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Invoices/Views/edit.html")
                );
                this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
                break;
            case SoeOriginType.CustomerInvoice:
                var message = new TabMessage(
                    `${this.terms["common.customer.invoices.invoice"]} ${row.invoiceNr}`,
                    row.invoiceId,
                    CustomerInvoicesEditController,
                    { id: row.invoiceId },
                    this.urlHelperService.getGlobalUrl("/Common/Customer/Invoices/Views/edit.html")
                );
                this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
                break;
            case SoeOriginType.SupplierPayment:
                var message = new TabMessage(
                    `${this.terms["economy.supplier.payment.payment"]} ${row.paymentNr}`,
                    row.invoiceId,
                    SupplierPaymentsEditController,
                    { id: row.invoiceId },
                    this.urlHelperService.getGlobalUrl("Shared/Economy/Supplier/Payments/Views/edit.html")
                );
                this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
                break;
            case SoeOriginType.CustomerPayment:
                var message = new TabMessage(
                    `${this.terms["common.customer.payment.payment"]} ${row.paymentNr}`,
                    row.invoiceId,
                    CustomerPaymentsEditController,
                    { id: row.invoiceId },
                    this.urlHelperService.getGlobalUrl("/Common/Customer/Payments/Views/edit.html")
                );
                this.messagingService.publish(Constants.EVENT_OPEN_TAB, message);
                break;
        }

    }

    private setTypeName(row: any) {
        if (row.type === SoeOriginType.CustomerInvoice || row.type === SoeOriginType.SupplierInvoice) {
            switch (row.billingType) {
                case TermGroup_BillingType.Credit: row.billingTypeName = this.terms["economy.supplier.invoice.matches.creditinvoice"]; break;
                case TermGroup_BillingType.Debit: row.billingTypeName = this.terms["economy.supplier.invoice.matches.debitinvoice"]; break;
                case TermGroup_BillingType.Interest: row.billingTypeName = this.terms["economy.supplier.invoice.matches.interestinvoice"]; break;
                case TermGroup_BillingType.Reminder: row.billingTypeName = this.terms["economy.supplier.invoice.matches.demandinvoice"]; break;
            }
            if (row.billingTypeName && !row.isEditable) {
                row.billingTypeName += ` (${this.terms["economy.supplier.invoice.matches.paymentsuggestion"]})`;
            }
            return;
        }
        row.billingTypeName = this.terms["economy.supplier.invoice.matches.payment"];
    }

    public vatChanged(vat) {
        //if (this.selectedMatchCode)
        //    this.match(this.selectedMatchCode);
    }

    public matching() {
        if (this.validate()) {
            var modal = this.notificationService.showDialog(this.terms["core.warning"], this.soeGridOptions.getSelectedRows().length + " " + this.terms["economy.supplier.invoice.matches.supplierswillmatch"], SOEMessageBoxImage.Warning, SOEMessageBoxButtons.OKCancel);
            modal.result.then(val => {
                if (val) {

                    var voucherHead: VoucherHeadDTO = new VoucherHeadDTO();
                    voucherHead.voucherHeadId = 0;
                    voucherHead.voucherNr = this.selectedvoucherSeries.voucherNrLatest;
                    voucherHead.date = this.selectedDate;
                    voucherHead.text = this.comment;
                    voucherHead.template = false;
                    voucherHead.vatVoucher = false;
                    voucherHead.status = 2;
                    voucherHead.voucherSeriesId = this.selectedvoucherSeries.voucherSeriesId;
                    voucherHead.accountPeriodId = this.accountPeriodId;

                    var invoicePaymentMatchAndVoucherDTO: any = {};
                    invoicePaymentMatchAndVoucherDTO.voucherHead = voucherHead;
                    invoicePaymentMatchAndVoucherDTO.accoutningsRows = this.accountingRows;
                    invoicePaymentMatchAndVoucherDTO.matchings = this.soeGridOptions.getSelectedRows();
                    invoicePaymentMatchAndVoucherDTO.matchCodeId = this.selectedMatchCode.matchCodeId;
                    this.startSave();
                    this.supplierService.InvoicePaymentMatchAndVoucher(invoicePaymentMatchAndVoucherDTO).then((result) => {
                        if (result.success) {
                            this.accountingRows = [];
                            this.notificationService.showDialog(this.terms["core.information"], this.terms["economy.supplier.invoice.matches.vouchercreated"].format(result.value.toString()), SOEMessageBoxImage.Information, SOEMessageBoxButtons.OK);
                            this.completedSave(this.supplierId);
                        } else {
                            this.failedSave(result.errorMessage);
                        }
                    }, error => {
                        this.failedSave(error.message);
                    });

                }
            })
        }

    }



    protected validate(): boolean {
        var errors = this['editValidation'].$error;
        // Accounting row validation
        if (errors['accountStandard'])
            this.showErrorDialog(this.terms["economy.accounting.voucher.accountstandardmissing"]);
        if (errors['accountInternal'])
            this.showErrorDialog(this.terms["economy.accounting.voucher.accountinternalmissing"]);
        if (errors['rowAmount'])
            this.showErrorDialog(this.terms["economy.accounting.voucher.invalidrowamount"]);
        if (errors['amountDiff'])
            this.showErrorDialog(this.terms["economy.accounting.voucher.unbalancedrows"]);
        return Object.keys(errors).length === 0;
    }

    public match(model: any): void {
        //Set style
        _.forEach(this.matchCodes, x => {
            if (x === model)
                x.style = this.styleClicked;
            else
                x.style = this.styleNonClicked;
        });

        this.accountingRows = [];
        this.selectedMatchCode = model;

        //var paymentAmount = this.soeGridOptions.getData().filter(f => f.type === SoeOriginType.SupplierPayment).map(f => f.invoiceTotalAmount);
        // Credit row
        var remainingAmount = this.soeGridOptions.getSelectedRows().filter(f => f.type === SoeOriginType.SupplierInvoice).map(f => f.invoiceMatchAmount).reduce((current, previous) => {
            return current + previous;
        }, 0);

        this.createAccountingRow(SupplierAccountType.Unknown, this.defaultCreditAccountId, remainingAmount, remainingAmount < 0, false, false);

        //var paymentmount = this.soeGridOptions.getSelectedRows().filter(f => f.type === SoeOriginType.SupplierPayment).map(f => f.invoiceMatchAmount).reduce((current, previous) => {
        //    return current + previous;
        //}, 0);
        if (remainingAmount !== 0) {
            if (remainingAmount > 0) {
                this.createAccountingRow(SupplierAccountType.Unknown, model.accountId || this.defaultDebitAccountId, remainingAmount, remainingAmount > 0, false, false);
            } else {
                if (this.accountVat) {
                    var vatRate = this.vatRate / (100 + this.vatRate);
                    var vatAmount = remainingAmount * vatRate;
                    var credAmount = remainingAmount * (1 - vatRate);
                    this.createAccountingRow(SupplierAccountType.Unknown, model.accountId || this.defaultDebitAccountId, credAmount, credAmount > 0, false, false);
                    this.createAccountingRow(SupplierAccountType.Unknown, model.vatAccountId || this.defaultVatAccountId, vatAmount, vatAmount > 0, true, false);
                } else {
                    this.createAccountingRow(SupplierAccountType.Unknown, model.accountId || this.defaultDebitAccountId, remainingAmount, remainingAmount > 0, false, false);
                }
            }
        }

        this.$timeout(() => {
            this.$scope.$broadcast('setRowItemAccountsOnAllRows');
            this.$scope.$broadcast('rowsAdded');
        });
    }

    private loadAccountPeriod(accountYearId: number): ng.IPromise<any> {
        return this.supplierService.getAccountPeriodId(accountYearId, this.selectedDate).then((id: number) => {
            this.accountPeriodId = id;
        });
    }
    private createAccountingRow(type: SupplierAccountType, accountId: number, amount: number, isDebitRow: boolean, isVatRow: boolean, isContractorVatRow: boolean): AccountingRowDTO {
        amount = Math.abs(amount);

        var row = new AccountingRowDTO();
        row.type = AccountingRowType.AccountingRow;
        row.invoiceAccountRowId = 0;
        row.tempRowId = 0;
        row.rowNr = AccountingRowDTO.getNextRowNr(this.accountingRows);
        row.debitAmountCurrency = isDebitRow ? amount : 0;
        row.creditAmountCurrency = isDebitRow ? 0 : amount;
        row.quantity = null;
        row.date = new Date().date();
        row.isCreditRow = !isDebitRow;
        row.isDebitRow = isDebitRow;
        row.isVatRow = isVatRow;
        row.isContractorVatRow = isContractorVatRow;
        row.isInterimRow = type === SupplierAccountType.Debit;
        row.state = SoeEntityState.Active;
        //row.invoiceId = this.invoice.invoiceId;
        row.isModified = false;


        // Set accounts
        if (type !== SupplierAccountType.Unknown) {
            row.dim1Id = this.getAccountId(type, 1);
            row.dim2Id = this.getAccountId(type, 2);
            row.dim3Id = this.getAccountId(type, 3);
            row.dim4Id = this.getAccountId(type, 4);
            row.dim5Id = this.getAccountId(type, 5);
            row.dim6Id = this.getAccountId(type, 6);
        }

        if (accountId !== 0)
            row.dim1Id = accountId;
        this.accountingRows.push(row);

        return row;
    }

    private getAccountId(type: SupplierAccountType, dimNr: number): number {
        // First try to get account from supplier
        //var accountId = this.getSupplierAccountId(type, dimNr);
        var accountId = 0;
        if (accountId === 0 && dimNr === 1) {
            // No account found on supplier, use base account
            switch (type) {
                case SupplierAccountType.Credit:
                    accountId = this.defaultCreditAccountId;
                    break;
                case SupplierAccountType.Debit:
                    accountId = this.defaultDebitAccountId;
                    break;
                case SupplierAccountType.VAT:
                    accountId = this.defaultVatAccountId;
                    break;
            }
        }

        return accountId;
    }

    public search() {
        this.supplierService.getInvoicePaymentsMatches(this.supplierId, SoeInvoiceType.SupplierInvoice).then((result: any[]) => {
            result.forEach(row => this.setTypeName(row));
            super.gridDataLoaded(result);
            this.setTotatlInfo();
        });
    }

    updateSelectedInfo(): void {
        var invoiceAmount = this.soeGridOptions.getSelectedRows().filter(f => f.type === SoeOriginType.SupplierInvoice).map(f => f.invoiceMatchAmount).reduce((current, previous) => {
            return current + previous;
        }, 0);
        var paymentAmount = this.soeGridOptions.getSelectedRows().filter(f => f.type === SoeOriginType.SupplierPayment).map(f => f.invoiceMatchAmount).reduce((current, previous) => {
            return current + previous;
        }, 0);
        var totalAmount = invoiceAmount + paymentAmount;
        this.selectedInvoice = invoiceAmount.toString();
        this.selectedPayment = paymentAmount.toString();
        this.selectedSum = totalAmount.toString();
    }

    setTotatlInfo(): void {
        var invoiceAmount = this.soeGridOptions.getData().filter(f => f.type === SoeOriginType.SupplierInvoice).map(f => f.invoiceTotalAmount).reduce((current, previous) => {
            return current + previous;
        }, 0);
        var paymentAmount = this.soeGridOptions.getData().filter(f => f.type === SoeOriginType.SupplierPayment).map(f => f.invoiceTotalAmount).reduce((current, previous) => {
            return current + previous;
        }, 0);
        var totalAmount = invoiceAmount + paymentAmount;
        this.totalInvoice = invoiceAmount.toString();
        this.totalPayment = paymentAmount.toString();
        this.totalSum = totalAmount.toString();
    }

    defaultVatRate;
    vatRate;
    accountPeriodId: number;
    defaultvoucherSeriesIdId;
}