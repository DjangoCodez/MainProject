import { test as base } from '@playwright/test';
import { SalesBasePage } from '../../pages/sales/SalesBasePage';
import { PurchacePage } from '../../pages/sales/purchase/PurchasePage';
import { InventoryPage } from '../../pages/sales/stock/InventoryPage';
import { WarehousePage } from '../../pages/sales/stock/WarehousePage';
import { PurchaseProposalPage } from '../../pages/sales/stock/PurchaseProposalPage';
import { BalancePage } from '../../pages/sales/stock/BalancePage';
import { BasePage } from '../../pages/common/BasePage';
import { PurchaseItemsPage } from '../../pages/sales/purchase/PurchaseItemsPage';
import { ProductsPageJS } from '../../pages/sales/product/ProductsPageJS';
import { PurchasePriceListPage } from '../../pages/sales/purchase/PurchasePriceListPage';
import { OrderPageJS } from '../../pages/sales/order/OrderPageJS';
import { AgreementsGroupsPage } from '../../pages/sales/agreements/AgreementGroupsPage';
import { AgreementsPageJS } from '../../pages/sales/agreements/AgreementsPageJS';
import { ProjectPageJS } from '../../pages/sales/projects/ProjectPageJS';
import { ProjectOverviewPageJS } from '../../pages/sales/projects/ProjectOverviewPageJS';
import { TimeReportPage } from '../../pages/sales/projects/TimeReportPage';
import { InvoicePageJS } from '../../pages/sales/invoice/InvoicePageJS';
import { ProductGroupsPage } from '../../pages/sales/settings/ProductGroupsPage';
import { ProductUnitsPage } from '../../pages/sales/settings/ProductUnitsPage';
import { getVersion } from '../../utils/CommonUtil';
import { AngVersion } from '../../enums/AngVersionEnums';
import { PlanningPageJS } from '../../pages/sales/planning/PlanningPageJS';
import { CustomersPage } from 'pages/sales/customer/CustomersPage';
import { ProcessingHistoryPage } from 'pages/finance/supplier/index/ProcessingHistoryPage';
import { AssignmentTypesPage } from 'pages/sales/settings/sales/AssignmentTypesPage';
import { ProductSettingsPage } from 'pages/sales/settings/products/ProductSettingsPage';
import { PaymentPage } from 'pages/sales/invoice/PaymentPage';
import { SettingsPage } from 'pages/sales/settings/sales/SettingsPage';

type SalesFixtures = {
  processingHistoryPage: ProcessingHistoryPage;
  assignmentTypesPage: AssignmentTypesPage;
  planningPageJS: PlanningPageJS;
  timeReportPage: TimeReportPage;
  projectOverviewPageJS: ProjectOverviewPageJS;
  projectPageJS: ProjectPageJS;
  orderPageJS: OrderPageJS;
  invoicePageJS: InvoicePageJS;
  agreementsPage: AgreementsPageJS;
  agreementsGroupsPage: AgreementsGroupsPage;
  purchasePriceListPage: PurchasePriceListPage;
  customersPage: CustomersPage;
  productsPageJS: ProductsPageJS;
  purchaseItemsPage: PurchaseItemsPage;
  salesBasePage: SalesBasePage;
  purchasePage: PurchacePage;
  inventoryPage: InventoryPage;
  wareHousePage: WarehousePage;
  purchaseProposalPage: PurchaseProposalPage;
  balancePage: BalancePage;
  productGroupsPage: ProductGroupsPage;
  productUnitsPage: ProductUnitsPage;
  productSettingsPage: ProductSettingsPage;
  paymentPage: PaymentPage;
  settingsPage: SettingsPage;
  basePage: BasePage;
};

export const test = base.extend<SalesFixtures>({
  processingHistoryPage: async ({ page }, use) => {
    await use(new ProcessingHistoryPage(page));
  },

  assignmentTypesPage: async ({ page }, use) => {
    const ang_version: AngVersion = await getVersion('AssignmentTypesPage');
    await use(new AssignmentTypesPage(page, ang_version));
  },

  planningPageJS: async ({ page }, use) => {
    await use(new PlanningPageJS(page));
  },

  timeReportPage: async ({ page }, use) => {
    await use(new TimeReportPage(page));
  },

  projectOverviewPageJS: async ({ page }, use) => {
    await use(new ProjectOverviewPageJS(page));
  },

  projectPageJS: async ({ page }, use) => {
    await use(new ProjectPageJS(page));
  },

  orderPageJS: async ({ page }, use) => {
    await use(new OrderPageJS(page));
  },

  invoicePageJS: async ({ page }, use) => {
    await use(new InvoicePageJS(page));
  },

  agreementsPage: async ({ page }, use) => {
    await use(new AgreementsPageJS(page));
  },

  agreementsGroupsPage: async ({ page }, use) => {
    await use(new AgreementsGroupsPage(page));
  },

  purchasePriceListPage: async ({ page }, use) => {
    const ang_version: AngVersion = await getVersion('PurchasePriceListPageJS');
    await use(new PurchasePriceListPage(page, ang_version));
  },

  productsPageJS: async ({ page }, use) => {
    await use(new ProductsPageJS(page));
  },

  customersPage: async ({ page }, use) => {
    const ang_version: AngVersion = await getVersion('CustomersPage');
    await use(new CustomersPage(page, ang_version));
  },

  purchaseItemsPage: async ({ page }, use) => {
    await use(new PurchaseItemsPage(page));
  },

  salesBasePage: async ({ page }, use) => {
    await use(new SalesBasePage(page));
  },

  purchasePage: async ({ page }, use) => {
    const ang_version: AngVersion = await getVersion('PurchasePage');
    await use(new PurchacePage(page, ang_version));
  },

  inventoryPage: async ({ page }, use) => {
    await use(new InventoryPage(page));
  },

  wareHousePage: async ({ page }, use) => {
    await use(new WarehousePage(page));
  },

  purchaseProposalPage: async ({ page }, use) => {
    await use(new PurchaseProposalPage(page));
  },

  balancePage: async ({ page }, use) => {
    await use(new BalancePage(page));
  },

  basePage: async ({ page }, use) => {
    await use(new BasePage(page));
  },
  productGroupsPage: async ({ page }, use) => {
    await use(new ProductGroupsPage(page));
  },

  productUnitsPage: async ({ page }, use) => {
    await use(new ProductUnitsPage(page));
  },

  productSettingsPage: async ({ page }, use) => {
    await use(new ProductSettingsPage(page));
  },

  paymentPage: async ({ page }, use) => {
    await use(new PaymentPage(page));
  },
  settingsPage: async ({ page }, use) => {
    await use(new SettingsPage(page));
  }

});
export { expect } from '@playwright/test';