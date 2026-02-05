import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { OrderUtil } from '../../../apis/utils/OrderUtil';

let testCaseId: string = '78097';
let envUrl: string;
let orderNumber: string;
let orderUtils: OrderUtil;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });
test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Orders");
  await allure.subSuite("Order");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  orderUtils = new OrderUtil(page, envUrl);
  orderNumber = await orderUtils.CreateOrder();
});

test(testCaseId + ': Order Document Transfer to Invoice : AP', { tag: ['@Sales', '@Order', '@Regression'] }, async ({ salesBasePage, orderPageJS, invoicePageJS }) => {
  await salesBasePage.goToMenu('Order', 'Order');
  await orderPageJS.filterAllOrders();
  await orderPageJS.reloadOrders();
  await orderPageJS.filterByOrderNo(orderNumber.toString());
  await orderPageJS.editOrder();
  await orderPageJS.expandDocument();
  await orderPageJS.tickAttachWhenSendingColumn();
  await orderPageJS.tickAttachToInvoiceColumn();
  await orderPageJS.clickSelectFilesToUpload();
  await orderPageJS.uploadFile(`test-data/Påminnelsemedfakturakopia123429SelTestCustomer.pdf`, "Påminnelsemedfakturakopia123429SelTestCustomer.pdf");
  await orderPageJS.tickAttachWhenSending();
  await orderPageJS.tickAttachToInvoice();
  await orderPageJS.saveOrder();
  await orderPageJS.closeOrder();
  await orderPageJS.filterByOrderNo(orderNumber.toString());
  await orderPageJS.editOrder();
  await orderPageJS.expandProducts();
  await orderPageJS.addToKlar();
  await orderPageJS.expandProducts();
  await orderPageJS.transferToFinalInvoice();
  await salesBasePage.goToMenu('Invoice', 'Invoices');
  await invoicePageJS.waitForGridLoaded();
  await invoicePageJS.filterAllInvoices();
  await invoicePageJS.filterByOrderNumber(orderNumber.toString());
  await invoicePageJS.editInvoice();
  await invoicePageJS.expandDocumentTab();
  await invoicePageJS.verifyFileExist('Påminnelsemedfakturakopia123429SelTestCustomer.pdf');
});