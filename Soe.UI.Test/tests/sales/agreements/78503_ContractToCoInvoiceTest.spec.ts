import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { CustomerUtil } from '../../../apis/utils/CustomerUtil';
import { AgreementUtil } from '../../../apis/utils/AgreementUtil';
import { ProductUtils } from '../../../apis/utils/ProductUtil';

let testCaseId: string = '78503';
let envUrl: string;
let customerUtil: CustomerUtil;
let customers: any;
let agreementUtil: AgreementUtil;
let groups: any;
let productUtil: ProductUtils;
let products: any;
let internalText_1: string = `Auto_1 ${Math.random().toString(36).substring(2, 7)} ` + ' ' + testCaseId;
let internalText_2: string = `Auto_2 ${Math.random().toString(36).substring(2, 7)} ` + ' ' + testCaseId;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
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
    await agreementUtil.createAgreement(groups[0].contractGroupId, customers[0].actorCustomerId, products[0].productId, internalText_1);
    await agreementUtil.createAgreement(groups[0].contractGroupId, customers[0].actorCustomerId, products[0].productId, internalText_2);
});

test(testCaseId + ': Agreements to co-invoice : SR', { tag: ['@Sales', '@Agreements', '@Regression'] }, async ({ salesBasePage, agreementsPage, invoicePageJS }) => {
    await salesBasePage.goToMenu('Agreements ', 'Agreements');
    await agreementsPage.waitForPageLoad();
    await agreementsPage.filterByInternalTextCurrent(internalText_1);
    const totalAmount1 = await agreementsPage.getTotalAmount();
    await agreementsPage.selectAgreement();
    await agreementsPage.filterByInternalTextCurrent(internalText_2);
    const totalAmount2 = await agreementsPage.getTotalAmount();
    await agreementsPage.selectAgreement();
    await agreementsPage.clearAllFilters();
    await agreementsPage.createPreliminaryMergedInvoicefromAgreement();
    await salesBasePage.goToMenu('Invoice', 'Invoices');
    await invoicePageJS.filterByInternalText(internalText_1);
    const totalMerged = totalAmount1 + totalAmount2;
    await invoicePageJS.verifyTotalAmount(totalMerged);
});
