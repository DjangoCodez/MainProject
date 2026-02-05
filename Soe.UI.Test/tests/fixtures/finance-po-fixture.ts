import { test as base } from '@playwright/test';
import { SupplierInvoicePageJS } from '../../pages/finance/supplier/SupplierInvoicePageJS';
import { FinanceBasePage } from '../../pages/finance/FinanceBasePage';
import { AccountPage } from '../../pages/finance/accounting/AccountPage';
import { VoucherPageJS } from '../../pages/finance/accounting/voucher/VoucherPageJS';
import { AccuralsPageJS } from '../../pages/finance/accounting/AccuralsPageJS';
import { InvoicePageJS } from '../../pages/finance/customer/InvoicePageJS';
import { PeriodInvoicingPageJS } from '../../pages/finance/order/PeriodInvoicingPageJS';
import { DistributionCodePage } from '../../pages/finance/settings/DistributionCodePage';
import { BudgetPage } from '../../pages/finance/accounting/budget/BudgetPage';
import { ReportPage } from '../../pages/finance/report/ReportPage';
import { AccountPayableSettingsPage } from '../../pages/finance/settings/AccountPayableSettingsPage';
import { PaymentsPage } from '../../pages/finance/customer/PaymentsPage';
import { AccountsReceivableSettingsPage } from '../../pages/finance/settings/AccountsReceivableSettingsPage';
import { SuppliersPage } from 'pages/finance/supplier/index/SuppliersPage';
import { AngVersion } from 'enums/AngVersionEnums';
import { getVersion } from 'utils/CommonUtil';
import { BaseAccountsPage } from 'pages/finance/settings/products/BaseAccountsPage';
import { OverviewPage } from 'pages/finance/supplier/OverviewPage';
import { CategoriesPage } from 'pages/finance/supplier/index/CategoriesPage';


type Financefixtures = {
  paymentsPage: PaymentsPage;
  financeBasePage: FinanceBasePage
  supplierInvoicePageJS: SupplierInvoicePageJS;
  accountPage: AccountPage;
  voucherPageJS: VoucherPageJS;
  accuralsPageJS: AccuralsPageJS;
  financeInvoicePageJS: InvoicePageJS;
  periodInvoicingPageJS: PeriodInvoicingPageJS;
  distributionCodePage: DistributionCodePage;
  reportPage: ReportPage;
  budgetPage: BudgetPage;
  accountPayableSettingsPage: AccountPayableSettingsPage;
  accountsReceivableSettingsPage: AccountsReceivableSettingsPage;
  suppliersPage: SuppliersPage;
  baseAccountsPage: BaseAccountsPage;
  overviewPage: OverviewPage;
  categoriesPage: CategoriesPage;
}

export const test = base.extend<Financefixtures>({
  overviewPage: async ({ page }, use) => {
    await use(new OverviewPage(page));
  },
  categoriesPage: async ({ page }, use) => {
    await use(new CategoriesPage(page));
  },
  baseAccountsPage: async ({ page }, use) => {
    await use(new BaseAccountsPage(page));
  },
  periodInvoicingPageJS: async ({ page }, use) => {
    await use(new PeriodInvoicingPageJS(page));
  },
  suppliersPage: async ({ page }, use) => {
    await use(new SuppliersPage(page));
  },
  reportPage: async ({ page }, use) => {
    await use(new ReportPage(page));
  },
  paymentsPage: async ({ page }, use) => {
    await use(new PaymentsPage(page));
  },
  financeInvoicePageJS: async ({ page }, use) => {
    await use(new InvoicePageJS(page));
  },
  accuralsPageJS: async ({ page }, use) => {
    await use(new AccuralsPageJS(page));
  },
  voucherPageJS: async ({ page }, use) => {
    await use(new VoucherPageJS(page));
  },
  accountPage: async ({ page }, use) => {
    const ang_version: AngVersion = await getVersion('AccountPage');
    await use(new AccountPage(page, ang_version));
  },
  financeBasePage: async ({ page }, use) => {
    await use(new FinanceBasePage(page));
  },
  supplierInvoicePageJS: async ({ page }, use) => {
    await use(new SupplierInvoicePageJS(page));
  },
  distributionCodePage: async ({ page }, use) => {
    await use(new DistributionCodePage(page));
  },
  budgetPage: async ({ page }, use) => {
    await use(new BudgetPage(page));
  },
  accountPayableSettingsPage: async ({ page }, use) => {
    await use(new AccountPayableSettingsPage(page));
  },
  accountsReceivableSettingsPage: async ({ page }, use) => {
    await use(new AccountsReceivableSettingsPage(page));
  }
});