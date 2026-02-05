import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { CustomerUtil } from '../../../apis/utils/CustomerUtil';
import { AgreementUtil } from '../../../apis/utils/AgreementUtil';
import { ProductUtils } from '../../../apis/utils/ProductUtil';

let testCaseId: string = '78490';
let envUrl: string;
let customerUtil: CustomerUtil;
let customers: any;
let agreementUtil: AgreementUtil;
let groups: any;
let productUtil: ProductUtils;
let products: any;
let internalText: string = `Auto ${Math.random().toString(36).substring(2, 7)} ` + ' ' + testCaseId;
let internalText_updated: string = `Auto Updated ${Math.random().toString(36).substring(2, 7)} ` + ' ' + testCaseId;
let productName: string = 'Auto Product' + testCaseId;

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
    await agreementUtil.createAgreement(groups[0].contractGroupId, customers[0].actorCustomerId, products[0].productId, internalText);
    await productUtil.createProduct(testCaseId, productName);
});

test(testCaseId + ': Agreements edit : SR', { tag: ['@Sales', '@Agreements', '@Regression'] }, async ({ salesBasePage, agreementsPage }) => {
    await salesBasePage.goToMenu('Agreements ', 'Agreements');
    await agreementsPage.waitForPageLoad();
    await agreementsPage.moveToAgreementTab();
    await agreementsPage.filterByInternalTextAgreements(internalText);
    await agreementsPage.editAgreement();
    await agreementsPage.clickUnlockButton();
    await agreementsPage.expandHeaderSection();
    await agreementsPage.setInternalText(internalText_updated);
    await agreementsPage.expandProductRows();
    await agreementsPage.clickNewProductRow();
    await agreementsPage.addProductNo(productName);
    await agreementsPage.save();
    await agreementsPage.close();
    await agreementsPage.moveToAgreementTab();
    await agreementsPage.filterByInternalTextAgreements(internalText_updated);
    await agreementsPage.editAgreement();
    await agreementsPage.expandProductRows();
    await agreementsPage.verifyAddedProductInAgreement(productName);              
});
