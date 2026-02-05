import { ICoreService } from "../../Core/Services/CoreService";
import { CoreUtility } from "../../Util/CoreUtility";
import { HtmlUtility } from "../../Util/HtmlUtility";
import { Feature, SoeStatesAnalysis, SoeOriginStatusClassificationGroup } from "../../Util/CommonEnumerations";
import { ITranslationService } from "../../Core/Services/TranslationService";

export class StateAnalysisController {

    // Progress bars
    generalBusy  = false;
    generalBusyMessage: string;
    billingBusy = false;
    householdBusy = false;
    customerledgerBusy = false;
    supplierledgerBusy = false;

    // Permissions
    generalPermission = false;
    billingPermission = false;
    offerPermission = false;
    contractPermission = false;
    orderPermission = false;
    invoicePermission = false;
    remainingAmountPermission = false;
    showSalesPricePermission = false;
    householdPermission = false;
    customerledgerPermission = false;
    supplierledgerPermission = false;
    editRolesPermission = false;
    editUsersPermission = false;
    editEmployeesPermission = false;
    editBillingCustomersPermission = false;
    editEconomyCustomersPermission = false;
    editCustomersPermission = false;
    editSuppliersPermission = false;
    editInvoiceProductsPermission = false;

    // Data
    stateAnalysis: any = {};

    //@ngInject
    constructor(
        private $uibModalInstance,
        private $window: ng.IWindowService,
        private $filter,
        private coreService: ICoreService,
        private translationService: ITranslationService) {

        this.loadTerms();
        this.loadPermissions();
    }

    private loadTerms() {
        this.translationService.translate("core.loading").then(term => {
            this.generalBusyMessage = term;
        })
    }
    // PERMISSIONS

    private loadPermissions() {
        const featureIds: number[] = [];
        featureIds.push(Feature.Billing_Offer_Status);
        featureIds.push(Feature.Billing_Contract_Status);
        featureIds.push(Feature.Billing_Order_Status);
        featureIds.push(Feature.Billing_Invoice_Status);
        featureIds.push(Feature.Billing_Product_Products_ShowSalesPrice);
        featureIds.push(Feature.Billing_Invoice_Household);
        featureIds.push(Feature.Economy_Customer_Invoice);
        featureIds.push(Feature.Economy_Supplier_Invoice);
        featureIds.push(Feature.Manage_Roles);
        featureIds.push(Feature.Manage_Users);
        featureIds.push(Feature.Time_Employee_Employees);
        featureIds.push(Feature.Billing_Customer_Customers);
        featureIds.push(Feature.Economy_Customer_Customers);
        featureIds.push(Feature.Economy_Supplier_Suppliers);
        featureIds.push(Feature.Billing_Product_Products);

        this.coreService.hasReadOnlyPermissions(featureIds).then((x) => {
            // General (no permission needed)
            this.generalPermission = true;
            this.generalBusy = true;
            var analysisIds: number[] = [];
            analysisIds.push(SoeStatesAnalysis.Role);
            analysisIds.push(SoeStatesAnalysis.User);
            analysisIds.push(SoeStatesAnalysis.Employee);
            analysisIds.push(SoeStatesAnalysis.Customer);
            analysisIds.push(SoeStatesAnalysis.Supplier);
            analysisIds.push(SoeStatesAnalysis.InvoiceProduct);
            this.load(analysisIds);

            if (x[Feature.Billing_Product_Products_ShowSalesPrice])
                this.showSalesPricePermission = true;

            if (x[Feature.Manage_Roles])
                this.editRolesPermission = true;
            if (x[Feature.Manage_Users])
                this.editUsersPermission = true;
            if (x[Feature.Time_Employee_Employees])
                this.editEmployeesPermission = true;
            if (x[Feature.Billing_Customer_Customers])
                this.editBillingCustomersPermission = true;
            if (x[Feature.Economy_Customer_Customers])
                this.editEconomyCustomersPermission = true;
            if (this.editBillingCustomersPermission || this.editEconomyCustomersPermission)
                this.editCustomersPermission = true;
            if (x[Feature.Economy_Supplier_Suppliers])
                this.editSuppliersPermission = true;
            if (x[Feature.Billing_Product_Products])
                this.editInvoiceProductsPermission = true;

            // Billing
            analysisIds = [];
            if (x[Feature.Billing_Offer_Status]) {
                this.billingPermission = true;
                this.offerPermission = true;
                analysisIds.push(SoeStatesAnalysis.Offer);
            }
            if (x[Feature.Billing_Contract_Status]) {
                this.billingPermission = true;
                this.contractPermission = true;
                analysisIds.push(SoeStatesAnalysis.Contract);
            }
            if (x[Feature.Billing_Order_Status]) {
                this.billingPermission = true;
                this.orderPermission = true;
                this.remainingAmountPermission = true;
                analysisIds.push(SoeStatesAnalysis.Order);
                analysisIds.push(SoeStatesAnalysis.OrderRemaingAmount);
            }
            if (x[Feature.Billing_Invoice_Status]) {
                this.billingPermission = true;
                this.invoicePermission = true;
                analysisIds.push(SoeStatesAnalysis.Invoice);
            }
            if (analysisIds.length > 0) {
                this.billingBusy = true;
                this.load(analysisIds);
            }

            // Household tax deduction
            analysisIds = [];
            if (x[Feature.Billing_Invoice_Household]) {
                this.householdPermission = true;
                analysisIds.push(SoeStatesAnalysis.HouseHoldTaxDeductionApply);
                analysisIds.push(SoeStatesAnalysis.HouseHoldTaxDeductionApplied);
                analysisIds.push(SoeStatesAnalysis.HouseHoldTaxDeductionReceived);
                analysisIds.push(SoeStatesAnalysis.HouseHoldTaxDeductionDenied);
            }
            if (analysisIds.length > 0) {
                this.householdBusy = true;
                this.load(analysisIds);
            }

            // Customer ledger
            analysisIds = [];
            if (x[Feature.Economy_Customer_Invoice]) {
                this.customerledgerPermission = true;
                analysisIds.push(SoeStatesAnalysis.CustomerPaymentsUnpayed);
                analysisIds.push(SoeStatesAnalysis.CustomerInvoicesOverdued);
            }
            if (analysisIds.length > 0) {
                this.customerledgerBusy = true;
                this.load(analysisIds);
            }

            // Supplier ledger
            analysisIds = [];
            if (x[Feature.Economy_Supplier_Invoice]) {
                this.supplierledgerPermission = true;
                analysisIds.push(SoeStatesAnalysis.SupplierInvoicesUnpayed);
                analysisIds.push(SoeStatesAnalysis.SupplierInvoicesOverdued);
            }
            if (analysisIds.length > 0) {
                this.supplierledgerBusy = true;
                this.load(analysisIds);
            }
        });
    }

    // LOOKUPS

    private load(analysisIds: number[]) {
        this.coreService.getStateAnalysis(analysisIds).then((analysis) => {
            _.forEach(analysis, (analyse) => {

                let noOfItems: number = analyse['noOfItems'] || 0;
                let totalAmount: number = analyse['totalAmount'] || 0;
                let totalAmount2: number = analyse['totalAmount2'] || 0;
                let totalAmount3: number = analyse['totalAmount3'] || 0;
                let noOfActorsForItems: number = analyse['noOfActorsForItems'] || 0;
                
                switch (analyse['state']) {
                    // General
                    case SoeStatesAnalysis.Role:
                        this.stateAnalysis.roles = noOfItems;
                        this.generalBusy = false;
                        break;
                    case SoeStatesAnalysis.User:
                        this.stateAnalysis.users = noOfItems;
                        break;
                    case SoeStatesAnalysis.Employee:
                        this.stateAnalysis.employees = noOfItems;
                        break;
                    case SoeStatesAnalysis.Customer:
                        this.stateAnalysis.customers = noOfItems;
                        break;
                    case SoeStatesAnalysis.Supplier:
                        this.stateAnalysis.suppliers = noOfItems;
                        break;
                    case SoeStatesAnalysis.InvoiceProduct:
                        this.stateAnalysis.invoiceproducts = noOfItems;
                        break;
                    // Billing
                    case SoeStatesAnalysis.Offer:

                        this.stateAnalysis.offersNbrOf = noOfItems;
                        this.stateAnalysis.offersSumExclVat = 0;
                        this.stateAnalysis.offersAverage = 0;
                        if (this.showSalesPricePermission) {
                            this.stateAnalysis.offersSumExclVat = this.asAmount(totalAmount);
                            this.stateAnalysis.offersAverage = this.asAmount(this.getAverage(totalAmount, noOfItems));
                        }
                        this.stateAnalysis.offersCustomers = noOfActorsForItems;
                        this.billingBusy = false;
                        break;
                    case SoeStatesAnalysis.Contract:
                        this.stateAnalysis.contractsNbrOf = noOfItems;
                        this.stateAnalysis.contractsSumExclVat = 0;
                        this.stateAnalysis.contractsAverage = 0;
                        if (this.showSalesPricePermission) {
                            this.stateAnalysis.contractsSumExclVat = this.asAmount(totalAmount);
                            this.stateAnalysis.contractsAverage = this.asAmount(this.getAverage(totalAmount, noOfItems));
                        }
                        this.stateAnalysis.contractsCustomers = noOfActorsForItems;
                        this.billingBusy = false;
                        break;
                    case SoeStatesAnalysis.Order:
                        this.stateAnalysis.ordersNbrOf = noOfItems;
                        this.stateAnalysis.ordersSumExclVat = 0;
                        this.stateAnalysis.ordersAverage = 0;
                        if (this.showSalesPricePermission) {
                            this.stateAnalysis.ordersSumExclVat = this.asAmount(totalAmount);
                            this.stateAnalysis.ordersAverage = this.asAmount(this.getAverage(totalAmount, noOfItems));
                        }
                        this.stateAnalysis.ordersCustomers = noOfActorsForItems;
                        this.billingBusy = false;
                        break;
                    case SoeStatesAnalysis.Invoice:
                        this.stateAnalysis.invoicesNbrOf = noOfItems;
                        this.stateAnalysis.invoicesSumExclVat = 0;
                        this.stateAnalysis.invoicesAverage = 0;
                        if (this.showSalesPricePermission) {
                            this.stateAnalysis.invoicesSumExclVat = this.asAmount(totalAmount);
                            this.stateAnalysis.invoicesAverage = this.asAmount(this.getAverage(totalAmount, noOfItems));
                        }
                        this.stateAnalysis.invoicesCustomers = noOfActorsForItems;
                        this.billingBusy = false;
                        break;
                    case SoeStatesAnalysis.OrderRemaingAmount:
                        this.stateAnalysis.remainingAmountSumExclVat = 0;
                        if (this.showSalesPricePermission) {
                            this.stateAnalysis.remainingAmountSumExclVat = this.asAmount(totalAmount);
                        }
                        this.billingBusy = false;
                        break;
                    // Household tax deduction
                    case SoeStatesAnalysis.HouseHoldTaxDeductionApply:
                        this.stateAnalysis.householdApply = noOfItems;
                        this.householdBusy = false;
                        break;
                    case SoeStatesAnalysis.HouseHoldTaxDeductionApplied:
                        this.stateAnalysis.householdApplied = noOfItems;
                        break;
                    case SoeStatesAnalysis.HouseHoldTaxDeductionReceived:
                        this.stateAnalysis.householdReceived = noOfItems;
                        break;
                    case SoeStatesAnalysis.HouseHoldTaxDeductionDenied:
                        this.stateAnalysis.householdDenied = noOfItems;
                        break;
                    // Customer ledger
                    case SoeStatesAnalysis.CustomerPaymentsUnpayed:
                        this.stateAnalysis.customerledgerUnpaidNbrOf = noOfItems;
                        this.stateAnalysis.customerledgerUnpaidSum = this.asAmount(totalAmount);
                        this.stateAnalysis.customerledgerUnpaidSumExclVat = this.asAmount(totalAmount2);
                        this.stateAnalysis.customerledgerUnpaidSumToPay = this.asAmount(totalAmount3);
                        this.customerledgerBusy = false;
                        break;
                    case SoeStatesAnalysis.CustomerInvoicesOverdued:
                        this.stateAnalysis.customerledgerOverdueNbrOf = noOfItems;
                        this.stateAnalysis.customerledgerOverdueSum = this.asAmount(totalAmount);
                        this.stateAnalysis.customerledgerOverdueSumExclVat = this.asAmount(totalAmount2);
                        this.stateAnalysis.customerledgerOverdueSumToPay = this.asAmount(totalAmount3);
                        this.customerledgerBusy = false;
                        break;
                    // Supplier ledger
                    case SoeStatesAnalysis.SupplierInvoicesUnpayed:
                        this.stateAnalysis.supplierledgerUnpaidNbrOf = noOfItems;
                        this.stateAnalysis.supplierledgerUnpaidSum = this.asAmount(totalAmount);
                        this.stateAnalysis.supplierledgerUnpaidSumExclVat = this.asAmount(totalAmount2);
                        this.stateAnalysis.supplierledgerUnpaidSumToPay = this.asAmount(totalAmount3);
                        this.supplierledgerBusy = false;
                        break;
                    case SoeStatesAnalysis.SupplierInvoicesOverdued:
                        this.stateAnalysis.supplierledgerOverdueNbrOf = noOfItems;
                        this.stateAnalysis.supplierledgerOverdueSum = this.asAmount(totalAmount);
                        this.stateAnalysis.supplierledgerOverdueSumExclVat = this.asAmount(totalAmount2);
                        this.supplierledgerBusy = false;
                        break;
                }
            });
        });
    }

    // EVENTS
    editRoles() {
        HtmlUtility.openInSameTab(this.$window, "/soe/manage/roles/default.aspx?license=" + CoreUtility.licenseId + "&licenseNr=" + CoreUtility.licenseNr + "&company=" + CoreUtility.actorCompanyId);
    }

    editUsers() {
        HtmlUtility.openInSameTab(this.$window, "/soe/manage/users/default.aspx");
    }

    editEmployees() {
        HtmlUtility.openInSameTab(this.$window, "/soe/time/employee/employees/default.aspx");
    }

    editCustomers() {
        if (this.editBillingCustomersPermission)
            HtmlUtility.openInSameTab(this.$window, "/soe/billing/customer/customers/default.aspx");
        else if (this.editEconomyCustomersPermission)
            HtmlUtility.openInSameTab(this.$window, "/soe/economy/customer/customers/default.aspx");
    }

    editSuppliers() {
        HtmlUtility.openInSameTab(this.$window, "/soe/economy/supplier/suppliers/default.aspx")
    }

    editInvoiceProducts() {
        HtmlUtility.openInSameTab(this.$window, "/soe/billing/product/products/default.aspx");
    }

    editOffers() {
        HtmlUtility.openInSameTab(this.$window, "/soe/billing/offer/status/default.aspx?classificationgroup=" + SoeOriginStatusClassificationGroup.HandleOffers);
    }

    editContracts() {
        HtmlUtility.openInSameTab(this.$window, "/soe/billing/contract/status/default.aspx?classificationgroup=" + SoeOriginStatusClassificationGroup.HandleContracts);
    }

    editOrders() {
        HtmlUtility.openInSameTab(this.$window, "/soe/billing/order/status/default.aspx?classificationgroup=" + SoeOriginStatusClassificationGroup.HandleOrders);
    }

    editInvoices() {
        HtmlUtility.openInSameTab(this.$window, "/soe/billing/invoice/status/default.aspx?classificationgroup=" + SoeOriginStatusClassificationGroup.HandleCustomerInvoices);
    }

    editHouseholds() {
        HtmlUtility.openInSameTab(this.$window, "/soe/billing/invoice/household/default.aspx");
    }

    editUnpaidCustomerLedgers() {
        HtmlUtility.openInSameTab(this.$window, "/soe/economy/customer/invoice/status/default.aspx?classificationgroup=" + SoeOriginStatusClassificationGroup.HandleCustomerPayments);
    }

    editOverdueCustomerLedgers() {
        HtmlUtility.openInSameTab(this.$window, "/soe/economy/customer/invoice/status/default.aspx?classificationgroup=" + SoeOriginStatusClassificationGroup.HandleCustomerInvoices);
    }

    editUnpaidSupplierLedgers() {
        HtmlUtility.openInSameTab(this.$window, "/soe/economy/supplier/invoice/status/default.aspx?classificationgroup=" + SoeOriginStatusClassificationGroup.HandleSupplierPayments);
    }

    editOverdueSupplierLedgers() {
        HtmlUtility.openInSameTab(this.$window, "/soe/economy/supplier/invoice/status/default.aspx?classificationgroup=" + SoeOriginStatusClassificationGroup.HandleSupplierInvoices);
    }

    close() {
        this.$uibModalInstance.dismiss('cancel');
    }

    // HELP-METHODS
    getAverage(amount: number, items: number) {
        if (items > 0)
            return amount / items;
        else
            return 0;
    }

    asAmount(amount: number) {
        return this.$filter("number")(amount, 2);
    }
}