import { expect, test } from '../../fixtures/staff-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { FinanceUtil } from 'apis/utils/FinanceUtil';
import { getEnvironmentValue } from '../../../utils/properties';


const testCaseId: string = '118149';
let envUrl: string;
const randomString = Math.random().toString(36).substring(2, 7);
const customerNumber = testCaseId + '_CUST_' + randomString;
const customerName = 'AutoCustomer_' + randomString;
const deliveryAddress = '123 Auto St, Test City';
const productId = '0268605';
const reportPath = 'test-data/temp-download/118149_invoice.pdf'

test.use({ account: { domain: "mtest", user: 'admin' } });

test.beforeEach(async ({ accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Invoice");
    await allure.subSuite("Invoice");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    const finacialUtil = new FinanceUtil(salesBasePage.page, envUrl);
    await finacialUtil.enableFinancialPeriods();
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test.describe.serial(testCaseId + ': Invoice Final : DS', { tag: ['@Sales', '@Invoice', '@Smoke'] }, () => {
    const internalText = Math.random().toString(36).substring(2, 7);

    test('Create Customer', async ({ salesBasePage, customersPage }) => {
        await salesBasePage.goToMenu('Customer_Sale', 'Customers');
        await customersPage.waitForPageLoad();
        await customersPage.createNewCustomer();
        await customersPage.setNumber(customerNumber);
        await customersPage.addCustomerName(customerName);
        await customersPage.addDeliveryAddress(deliveryAddress)
        await customersPage.addNote();
        await customersPage.save();
        await customersPage.verifyTabChanged(customerName, customerNumber);
        await customersPage.closeTab();
        await customersPage.reloadPage();
        await customersPage.verifyFilterByNumber(customerNumber);
    })
    test('Create order with the customer', async ({ salesBasePage, orderPageJS }) => {
        await salesBasePage.goToMenu('Order', 'Order');
        await orderPageJS.waitForPageLoad();
        await orderPageJS.createItem();
        await orderPageJS.addCustomer(customerName);
        await orderPageJS.addInternalText(internalText);
        await orderPageJS.setVatType('Subject to VAT');
        await orderPageJS.expandProducts();
        await orderPageJS.clickNewProductRow()
        await orderPageJS.externalProductSearch(productId)
        await orderPageJS.verifyProductRowCount(1);
        await orderPageJS.saveOrder();
        await orderPageJS.closeOrder()
        await orderPageJS.filterByInternalText(internalText);
        await orderPageJS.editOrder();
        await orderPageJS.expandTimes();
        await orderPageJS.addWork();
        await orderPageJS.expandProducts()
        await orderPageJS.addToKlar()
        await orderPageJS.tranferToPreliminaryInvoice()
    })
    test('Finalize the invoice', async ({ salesBasePage, invoicePageJS }) => {
        await salesBasePage.goToMenu('Invoice', 'Invoices');
        await invoicePageJS.waitForPageLoad()
        await invoicePageJS.filterByInternalText(internalText)
        await invoicePageJS.selectInvoiceRowFromTheGrid();
        await invoicePageJS.transferInvoice();
        await invoicePageJS.filterByInternalText(internalText)
        await invoicePageJS.selectInvoiceRowFromTheGrid();
        await invoicePageJS.editInvoice()
        await invoicePageJS.expandProductRowTab()
        const totalAmount = await invoicePageJS.getInvoiceTotalAmount();
        await invoicePageJS.expandTracking()
        const { debitAmount } = await invoicePageJS.verifyVoucher()
        await invoicePageJS.closeInvoice();
        await invoicePageJS.InvoiceJournal(testCaseId);
        await invoicePageJS.verifyMultipleValueInPDF(reportPath, `Total2610Utgående moms, oreducerad 1 509,00 377,25 -0,25 1 886,00 377,25`)
        expect(debitAmount).toBe(totalAmount);
    })
});