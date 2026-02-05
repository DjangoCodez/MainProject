import { ICoreService } from "../../../../Core/Services/CoreService";
import { IOrderService } from "../../Orders/OrderService";
import { ITranslationService } from "../../../../Core/Services/TranslationService";
import { INotificationService } from "../../../../Core/Services/NotificationService";
import { IUrlHelperService } from "../../../../Core/Services/UrlHelperService";
import { EditProductRowModes, SOEMessageBoxImage, SOEMessageBoxButtons } from "../../../../Util/Enumerations";
import { ProductRowDTO } from "../../../../Common/Models/InvoiceDTO";
import { CalendarUtility } from "../../../../Util/CalendarUtility";
import { ToolBarUtility, ToolBarButtonGroup } from "../../../../Util/ToolBarUtility";
import { SplitAccountingRowDTO } from "../../../../Common/Models/AccountingRowDTO";
import { CoreUtility } from "../../../../Util/CoreUtility";
import { SplitAccountingDialogController } from "./SplitAccountingDialogController";
import { HouseholdTaxDeductionApplicantDTO } from "../../../../Common/Models/householdtaxdeductionapplicantdto";
import { SoeInvoiceRowType, SoeInvoiceRowDiscountType, SoeReportTemplateType, SoeOriginType, Feature, TermGroup_HouseHoldTaxDeductionType, CompanySettingType } from "../../../../Util/CommonEnumerations";
import { IMessagingService } from "../../../../Core/Services/MessagingService";
import { Guid } from "../../../../Util/StringUtility";
import { TimeRowsHelper } from "../../Helpers/TimeRowsHelper";
import { SettingsUtility } from "../../../../Util/SettingsUtility";
import { StockDTO } from "../../../../Common/Models/StockDTO";
import { IProductService } from "../../Products/ProductService";

export class EditProductRowDialogController {

    //Variables
    private title: string;
    private householdTitle: string;
    private guid:Guid;

    private timeRowsHelper: TimeRowsHelper;

    // Lookups
    private applicants: HouseholdTaxDeductionApplicantDTO[] = [];

    // Navigation
    protected navigationMenuButtons = new Array<ToolBarButtonGroup>();

    // Collections
    protected productRows: ProductRowDTO[];

    // Properties
    private _selectedProduct: string;
    get selectedProduct() {
        return this._selectedProduct;
    }
    set selectedProduct(item: string) {
        this._selectedProduct = item;
    }

    private _householdExpanderInitiallyOpened: boolean;
    set householdExpanderInitiallyOpened(value: boolean) {
        this._householdExpanderInitiallyOpened = value;
    }
    get householdExpanderInitiallyOpened(): boolean {
        return this._householdExpanderInitiallyOpened !== undefined ? this._householdExpanderInitiallyOpened : false;
    }

    private keepValuesWhenClearingApplicant: boolean = false;
    private _selectedApplicant: HouseholdTaxDeductionApplicantDTO;
    get selectedApplicant() {
        return this._selectedApplicant;
    }
    set selectedApplicant(item: HouseholdTaxDeductionApplicantDTO) {
        this._selectedApplicant = item;

        this.setValuesFromApplicant();
    }
    
    private _showAllApplicants: boolean = false;
    get showAllApplicants() {
        return this._showAllApplicants;
    }
    set showAllApplicants(item: boolean) {
        this._showAllApplicants = item;
        this.loadApplicants();
    }

    get isHouseholdMode() {
        return this.mode === EditProductRowModes.EditHousehold;
    }

    get isReadOnlyCheckTime() {
        return this.isReadonly || (this.row && this.row.isTimeProjectRow);
    }

    get showSplitAccounting() {
        return this.rowsCtrl.originType === SoeOriginType.CustomerInvoice ? !this.isReadonly : !this.isReadonly && this.row && !this.row.isLiftProduct;
    }

    // Flags
    private isReadonly: boolean = false;
    private isRowTransferred: boolean = false;
    private showNavigationButtons: boolean = false;

    private edit: ng.IFormController;
    private ediEntry: any;

    private socialSecurityNumberFormat = "YYYYMMDD-XXXX";

    //@ngInject
    constructor(private $scope: ng.IScope,
        $q: ng.IQService,
        private $uibModal,
        private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
        private $timeout: ng.ITimeoutService,
        private coreService: ICoreService,
        private orderService: IOrderService,
        private productService: IProductService,
        private translationService: ITranslationService,
        private notificationService: INotificationService,
        private urlHelperService: IUrlHelperService,
        messagingService: IMessagingService,
        private mode: EditProductRowModes,
        private titleTerm: string,
        private householdTitleTerm: string,
        private rutTitleTerm: string,
        private isCredit: boolean,
        private row: ProductRowDTO,
        private rowsCtrl: any,
        private priceListTypeInclusiveVat: boolean,
        invoiceId: number,
        private purchasePriceEnabled: boolean,
        private calculateMarginalIncomeOnZeroPurchase: boolean,
        private usePartialInvoicingOnOrderRow: boolean,
        private isTaxDeductionRow: boolean) {

        this.guid = Guid.newGuid();
        this.timeRowsHelper = new TimeRowsHelper(this.guid, $q, $uibModal, $scope, messagingService, urlHelperService, translationService, orderService, coreService, invoiceId, row.customerInvoiceRowId);

        // Setup
        this.timeRowsHelper.loadPermissions().then(() => {
            this.setup();
            this.setupWatchers();
            this.setupNavigationGroup();
        });
    }

        // SETUP
    protected setup() {
        this.title = this.titleTerm + ' ' + this.row.rowNr;
        this.selectedProduct = this.row.productNr;
        this.isRowTransferred = this.rowsCtrl.isRowTransferred(this.row, false, true);
        this.isReadonly = this.rowsCtrl.readOnly || this.rowsCtrl.isRowLocked(this.row) || this.rowsCtrl.isRowClosed(this.row) || this.isRowTransferred;

        if (this.row.date)
            this.row.date = CalendarUtility.convertToDate(this.row.date);
        if (this.row.dateTo)
            this.row.dateTo = CalendarUtility.convertToDate(this.row.dateTo);

        // Household
        this.householdExpanderInitiallyOpened = (this.mode === EditProductRowModes.EditHousehold);
        this.householdTitle = this.row.householdTaxDeductionType === TermGroup_HouseHoldTaxDeductionType.RUT ? this.rutTitleTerm : this.householdTitleTerm;

        if (this.mode === EditProductRowModes.EditHousehold) {
            this.loadApplicants();
        }

        if (this.row.stockId && (this.row.stocksForProduct || this.row.stocksForProduct.length === 0)) {
            
            this.row.stocksForProduct.push({ id: 0, name: "" });
            this.productService.getStocksByProduct(this.row.productId).then((stockList: StockDTO[]) => {

                stockList.forEach((stock) => {
                    this.row.stocksForProduct.push({ id: stock.stockId, name: stock.code + ' ' + stock.saldo });
                });
            });
        }
                

        this.showNavigationButtons = this.mode === EditProductRowModes.EditProductRow;

        if (this.rowsCtrl['productRows']) {
            this.productRows = [];
            _.forEach(this.rowsCtrl.productRows, (row: ProductRowDTO) => {
                if (row.type === SoeInvoiceRowType.ProductRow)
                    this.productRows.push(row);
            });
        }
        this.loadEdiEntry();
    }

    private loadEdiEntry() {
        if (this.row.ediEntryId) {
            this.orderService.getEdiEntryInfo(this.row.ediEntryId).then((result) => {
                this.ediEntry = result;
            })
        }
    }

    private showEdiPdf(ediEntryId: number, hasPdf: boolean) {
        if (hasPdf) {
            var ediPdfReportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.SymbrioEdiSupplierInvoice + "&edientryid=" + ediEntryId;
            window.open(ediPdfReportUrl, '_blank');
        }
        else {
            this.coreService.generateReportForEdi([ediEntryId]).then((result) => {
                if (result.success) {
                    var ediPdfReportUrl: string = "/ajax/downloadReport.aspx?templatetype=" + SoeReportTemplateType.SymbrioEdiSupplierInvoice + "&edientryid=" + ediEntryId;
                    window.open(ediPdfReportUrl, '_blank');
                }
            }, error => {
                var keys = ["core.warning", "common.pdferror"];
                keys.push("error.unabletosave_title");
                this.translationService.translateMany(keys).then((terms) => {
                    this.notificationService.showDialogEx(terms["core.warning"], terms["common.pdferror"], SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
                });
            });
        }
    }

    private setupWatchers() {
        this.$scope.$watch(() => this.row.productNr, (newVal, oldVal) => {
            // If product change is reverted, also revert selectedProduct
            if (newVal !== oldVal) {
                this.selectedProduct = newVal;
            }
        });
    }

    protected setupNavigationGroup() {
        var group = ToolBarUtility.createNavigationGroup(
            () => {
                if (this.productRows && this.productRows.length > 0) {
                    this.row = this.productRows[0];
                    this.rowChanged();
                }
            },
            () => {
                var i: number = 0;
                var found: boolean = false;

                _.forEach(this.productRows, (row) => {
                    if (row.tempRowId === this.row.tempRowId) {
                        return false;
                    }
                    i++;
                });

                if (i > 0) {
                    this.row = this.productRows[i - 1];
                    this.rowChanged();
                }
            },
            () => {
                var i: number = 0;
                var found: boolean = false;

                _.forEach(this.productRows, (row) => {
                    if (row.tempRowId === this.row.tempRowId) {
                        return false;
                    }
                    i++;
                });

                if (i < this.productRows.length - 1) {
                    this.row = this.productRows[i + 1];
                    this.rowChanged();
                }
            },
            () => {
                if (this.productRows && this.productRows.length > 0) {
                    this.row = this.productRows[this.productRows.length - 1];
                    this.rowChanged();
                }
            },
            null,
            null
        );

        this.navigationMenuButtons.push(group);
    }

    private rowChanged() {
        this.title = this.titleTerm + ' ' + this.row.rowNr;
        this.selectedProduct = this.row.productNr;
        this.isRowTransferred = this.rowsCtrl.isRowTransferred(this.row);
        this.isReadonly = this.rowsCtrl.readOnly || this.rowsCtrl.isRowLocked(this.row);
        if (this.timeRowsHelper.timeProjectRendered) {
            this.timeRowsHelper.loadTimeProjectRows(true, this.row.customerInvoiceRowId);
        }
    }

    // LOOKUPS
    private loadApplicants() {
        if (this.rowsCtrl.customer) {
            this.coreService.getHouseholdTaxDeductionRowsByCustomer(this.rowsCtrl.customer.actorCustomerId, true, this.showAllApplicants).then(x => {
                this.applicants = x;
                _.forEach(this.applicants, (app) => {
                    if (app.socialSecNr)
                        app['text'] = app.socialSecNr + " - " + app.name + " (" + (app.property ? app.property : app.apartmentNr + " : " + app.cooperativeOrgNr) + ")";
                    else
                        app['text'] = " ";
                });

                // If only one result select that item, otherwise select empty
                this.selectedApplicant = this.applicants[this.applicants.length === 2 ? 1 : 0];
            });
        }
    }

    private loadSplitAccountingRows(): ng.IPromise<any> {
        return this.orderService.getSplitAccountingRows(this.row.customerInvoiceRowId, true).then(x => {
            this.row.splitAccountingRows = x;
        });
    }

    // EVENTS

    private productFocus() {
        this.rowsCtrl.prevProductId = this.row.productId;
    }

    private productChanged() {
        this.$timeout(() => {
            this.row.productNr = this.selectedProduct;
            this.rowsCtrl.initChangeProduct(this.row);
        });
    }

    private searchProduct() {
        this.$timeout(() => {
            this.row.productNr = this.selectedProduct;
            this.rowsCtrl.searchProduct(this.row);
        });
    }

    private quantityFocus() {
        this.rowsCtrl.prevQuantity = this.row.quantity;
    }

    private quantityChanged(model) {
        this.$timeout(() => {
            this.rowsCtrl.initChangeQuantity(this.row);
        });
    }

    private invoiceQuantityChanged(model) {
        this.$timeout(() => {
            this.rowsCtrl.initChangeInvoiceQuantity(this.row);
        });
    }

    private amountFocus() {
        this.rowsCtrl.prevAmount = this.row.amountCurrency;
    }

    private amountChanged(model) {
        this.$timeout(() => {
            this.rowsCtrl.initChangeAmount(this.row);
        });
    }

    private discountValueChanged(model) {
        this.$timeout(() => {
            this.rowsCtrl.initChangeDiscount(this.row);
        });
    }

    private discountTypeChanged(model) {
        this.$timeout(() => {
            this.rowsCtrl.initChangeDiscount(this.row);
        });
    }

    private vatCodeChanged(model) {
        this.$timeout(() => {
            this.rowsCtrl.vatCodeChanged(this.row);
        });
    }

    private vatAccountChanged(model) {
        this.$timeout(() => {
            this.rowsCtrl.vatAccountChanged(this.row);
        });
    }

    private supplementChargeChanged(model) {
        this.$timeout(() => {
            this.rowsCtrl.supplementChargeChanged(this.row);
        });
    }

    private purchasePriceChanged(model) {
        this.$timeout(() => {
            this.rowsCtrl.purchasePriceChanged(this.row);
        });
    }

    private marginalIncomeChanged(model) {
        this.$timeout(() => {
            this.rowsCtrl.marginalIncomeChanged(this.row);
        });
    }

    private marginalIncomeRatioChanged(model) {
        this.$timeout(() => {
            this.rowsCtrl.marginalIncomeRatioChanged(this.row);
        });
    }

    private setValuesFromApplicant() {
        var selected: boolean = !!this.selectedApplicant;

        this.row.householdSocialSecNbr = selected ? this.selectedApplicant.socialSecNr : '';
        this.row.householdName = selected && this.selectedApplicant.name ? this.selectedApplicant.name.trim() : '';

        //if (!this.keepValuesWhenClearingApplicant) {
        this.row.householdProperty = selected ? this.selectedApplicant.property : '';
        this.row.householdApartmentNbr = selected ? this.selectedApplicant.apartmentNr : '';
        this.row.householdCooperativeOrgNbr = selected ? this.selectedApplicant.cooperativeOrgNr : '';
        //}
    }

    // ACTIONS

    private splitAccounting() {
        if (!this.row.splitAccountingRows || this.row.splitAccountingRows.length === 0) {
            if (this.row && this.row.customerInvoiceRowId && this.row.customerInvoiceRowId > 0) {
                this.loadSplitAccountingRows().then(() => {
                    this.openSplitAccounting();
                });
            }
            else {
                this.row.splitAccountingRows = [];
                this.openSplitAccounting();
            }
        } else {
            this.openSplitAccounting();
        }
    }

    private openSplitAccounting() {
        const settingTypes: number[] = [
            CompanySettingType.AccountCustomerDiscount,
            CompanySettingType.AccountCustomerDiscountOffset,
        ];

        return this.coreService.getCompanySettings(settingTypes).then(x => {
            const discountAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerDiscount, 0);
            const discountOffsetAccountId = SettingsUtility.getIntCompanySetting(x, CompanySettingType.AccountCustomerDiscountOffset, 0);
            if (this.row.splitAccountingRows.length === 0) {
                var splitRow = new SplitAccountingRowDTO();
                splitRow.splitValue = 100;
                splitRow.splitType = SoeInvoiceRowDiscountType.Percent;
                splitRow.amountCurrency = this.row.sumAmountCurrency;
                splitRow.excludeFromSplit = false;
                this.row.splitAccountingRows.push(splitRow);
            }
            else {
                _.forEach(this.row.splitAccountingRows, (r) => {
                    r.excludeFromSplit = (r.dim1Id === discountAccountId || r.dim1Id === discountOffsetAccountId);
                });
            }

            // Copy rows to be able to undo if user cancel the dialog
            const originalRows: SplitAccountingRowDTO[] = CoreUtility.cloneDTOs(this.row.splitAccountingRows);

            let amount = 0;
            if (this.row.sumAmountCurrency < 0)
                amount = (this.priceListTypeInclusiveVat ? (this.row.sumAmountCurrency - this.row.vatAmountCurrency).round(2) : this.row.sumAmountCurrency) * -1;
            else
                amount = this.priceListTypeInclusiveVat ? (this.row.sumAmountCurrency - this.row.vatAmountCurrency).round(2) : this.row.sumAmountCurrency;

            const options: angular.ui.bootstrap.IModalSettings = {
                templateUrl: this.urlHelperService.getGlobalUrl("Shared/Billing/Directives/ProductRows/Views/SplitAccountingDialog.html"),
                controller: SplitAccountingDialogController,
                controllerAs: "ctrl",
                size: 'lg',
                resolve: {
                    isReadonly: () => { return this.isReadonly; },
                    accountingRows: () => { return this.row.splitAccountingRows; },
                    productRowAmount: () => { return amount; },
                    isCredit: () => { return this.isCredit; },
                    multipleRowSplit: () => { return false; },
                    negativeRow: () => { return this.row.sumAmountCurrency < 0;}
                }
            }

            const modal = this.$uibModal.open(options);
            modal.result.then((result: any) => {
                if (result) this.row.splitAccountingRows = result;
                if (this.row.splitAccountingRows !== originalRows)
                    this.row.isModified = true;
                // OK
            }, (result: any) => {
                // Undo row modifications - always empty to cause reload
                this.row.splitAccountingRows = [];
            });
        });
    }

    private close() {
        if (this.validate()) {
            if (this['edit'].$dirty)
                this.row.isModified = true;

            this.$uibModalInstance.close( { reloadInvoiceAfterClose: this.timeRowsHelper.reloadInvoiceAfterClose} );
        }
    }

    private cancel() {
        this.$uibModalInstance.dismiss();
    }

    private validate(): boolean {
        var errors = this['edit'].$error;
        var keys: string[] = [];

        // Mandatory fields
        // Validate property or apartment
        let validateOrgNr = true;
        if (this.isHouseholdMode && this.row.householdTaxDeductionType !== TermGroup_HouseHoldTaxDeductionType.RUT) {
            if (!((this.row.householdProperty && this.row.householdProperty.length > 0) || (this.row.householdApartmentNbr && this.row.householdApartmentNbr.length > 0))) {
                keys.push("billing.productrows.houseorapartmentrequired");
                validateOrgNr = false;
            }
        }

        if (errors['required']) {
            _.forEach(errors['required'], err => {
                if (err['$name'] === 'ctrl_row_householdName')
                    keys.push("billing.productrows.missinghouseholdname");
            });
        }

        // Validate social security number and cooperative org. number
        if (errors['socialSecurityNumber']) {
            _.forEach(errors['socialSecurityNumber'], err => {
                if (err['$name'] === 'ctrl_row_householdSocialSecNbr')
                    keys.push("billing.productrows.invalidsocialsecuritynumber");
                else if ((!this.row.householdProperty || this.row.householdProperty.length === 0) && validateOrgNr && err['$name'] === 'ctrl_row_householdCooperativeOrgNbr')
                    keys.push("billing.productrows.invalidcooperativeorgnumber");
            });
        }

        if (keys.length > 0) {
            keys.push("error.unabletosave_title");
            this.translationService.translateMany(keys).then((terms) => {
                var message: string = "";
                _.forEach(terms, term => {
                    if (term !== terms["error.unabletosave_title"])
                        message += term + ".\\n";
                });

                this.notificationService.showDialog(terms["error.unabletosave_title"], message, SOEMessageBoxImage.Error, SOEMessageBoxButtons.OK);
            });
            return false;
        }

        return true;
    }

    public selectProductRow(): ng.IPromise<number> {
        return this.timeRowsHelper.selectProductRow(this.row.productId, this.productRows);
    }
}
