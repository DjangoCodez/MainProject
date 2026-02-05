import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { CustomerUtil } from '../../../apis/utils/CustomerUtil';
import { AgreementUtil } from '../../../apis/utils/AgreementUtil';
import { ProductUtils } from '../../../apis/utils/ProductUtil';

let testCaseId: string = '78495';
let envUrl: string;
let customerUtil: CustomerUtil;
let customers: any;
let agreementUtil: AgreementUtil;
let groups: any;
let productUtil: ProductUtils;
let products: any;
let internalText_1: string;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ page, accountEx, basePage, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Agreements");
    await allure.subSuite("Agreement");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
    customerUtil = new CustomerUtil(page, envUrl);
    customers = await customerUtil.getCustomers();
    agreementUtil = new AgreementUtil(page, envUrl);
    groups = await agreementUtil.getAgreementGroups();
    productUtil = new ProductUtils(page, envUrl);
    products = await productUtil.getProducts();
    internalText_1 = 'Agreement_' + testCaseId + Math.floor(100000 + Math.random() * 900000) + '_1';
    await agreementUtil.createAgreement(groups[0].contractGroupId, customers[0].actorCustomerId, products[0].productId, internalText_1);
});

test(testCaseId + ': Create Agreement To Final Invoice : AP', { tag: ['@Sales', '@Agreements', '@Regression'] }, async ({ salesBasePage, agreementsPage, invoicePageJS }) => {
    await salesBasePage.goToMenu('Agreements ', 'Agreements');
    await agreementsPage.waitForPageLoad();
    await agreementsPage.moveToAgreementTab();
    await agreementsPage.filterByInternalTextAgreements(internalText_1);
    await agreementsPage.editAgreement();
    await agreementsPage.transferToFinalInvoice();
    await salesBasePage.goToMenu('Invoice', 'Invoices');
    await invoicePageJS.waitForGridLoaded();
    await invoicePageJS.filterByInternalText(internalText_1);
    await invoicePageJS.verifyInvoiceExsit();
    await invoicePageJS.verifyInvoiceNo();
    await invoicePageJS.verifyStatus();
    await invoicePageJS.verifyInvoiceDateInCustomerInvoiceGrid();
});
