import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { OrderUtil } from '../../../apis/utils/OrderUtil';
import { AngVersion } from '../../../enums/AngVersionEnums';
import { getDateUtil } from '../../../utils/CommonUtil';

let testCaseId: string = '116556';
let envUrl: string;
let orderNumber: string;
let orderUtils: OrderUtil;
const employeeName = 'Playwright Employee';


test.beforeEach(async ({ page, accountEx, salesBasePage }, testInfo) => {
  await allure.parentSuite("Sales");
  await allure.suite("Orders");
  await allure.subSuite("Period Invoicing");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  orderUtils = new OrderUtil(page, envUrl);
  orderNumber = await orderUtils.CreateOrder();
});

test(testCaseId + ': Period Invoicing : AP', { tag: ['@Sales', '@Order', '@Regression'] }, async ({ salesBasePage, periodInvoicingPageJS }) => {
  await salesBasePage.goToMenu('Order', 'Period Invoicing');
  await periodInvoicingPageJS.goToPageVersion(AngVersion.JS);
  await periodInvoicingPageJS.untickOnlyReadyForInvoice();
  await periodInvoicingPageJS.search();
  await periodInvoicingPageJS.desOrderNo();
  await periodInvoicingPageJS.clickEditIconOnOrder(orderNumber.toString());
  await periodInvoicingPageJS.expandTimes();
  await periodInvoicingPageJS.loadAllTimes();
  await periodInvoicingPageJS.addTimeRow();
  await periodInvoicingPageJS.addRegisteredTime(employeeName, '02:00', '02:00', await getDateUtil(3));
  await periodInvoicingPageJS.addRegisteredTime(employeeName, '01:00', '01:00', await getDateUtil(2), 1, true, true);
  await periodInvoicingPageJS.loadAllTimes();
  await periodInvoicingPageJS.verifyGridValueUpdated('03:00', '03:00');
  await periodInvoicingPageJS.closeOrder();
  await periodInvoicingPageJS.reloadPeriodInvoicingGrid();
  await periodInvoicingPageJS.clearAllFilters();
  await periodInvoicingPageJS.filterByOrderNo(orderNumber.toString());
  await periodInvoicingPageJS.transferToTimeRowsToProductRows();
  const entries = await periodInvoicingPageJS.selectedAllTimeEntries();
  await periodInvoicingPageJS.changeKlar();
  await periodInvoicingPageJS.clickRun();
  await periodInvoicingPageJS.reloadPeriodInvoicingGrid();
  await periodInvoicingPageJS.verifychangeToKlar(entries);
  await periodInvoicingPageJS.selectedAllTimeEntries();
  await periodInvoicingPageJS.transferToPreliminaryInvoice();
  await periodInvoicingPageJS.moveToCustomerInvoicesTab()
  await periodInvoicingPageJS.reloadCustomerInvoicesGrid();
  await periodInvoicingPageJS.filterByOrderNoInvoice(orderNumber.toString());
  await periodInvoicingPageJS.editSelectedInvoice();
  await periodInvoicingPageJS.expandProductRows();
  await periodInvoicingPageJS.verifyProductQuantityEqualsTimeQuantity("3");
});
