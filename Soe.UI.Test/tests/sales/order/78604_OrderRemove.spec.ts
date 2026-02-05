import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { ManageUtils } from 'apis/utils/ManageUtils';

let testCaseId: string = '78604';
let envUrl: string;

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Orders");
    await allure.subSuite("Order");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await new ManageUtils(page, envUrl).verifyOrderCheckLists();
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Order remove : DS', { tag: ['@Sales', '@Order', '@Regression'] }, async ({ salesBasePage, orderPageJS, projectPageJS }) => {
    // Create and delete a product without deleting the associated project.
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    let internalText = `Manual_Test_Order_Copy_${Math.random().toString(36).substring(2, 7)}`
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.setVatType('Subject to VAT');
    await orderPageJS.expandProducts();
    await orderPageJS.newProductRow("999", '1', '100');
    const res1 = await orderPageJS.saveOrder();
    let projectId = res1.projectId;
    await orderPageJS.deleteOrder("No", "OK")
    await salesBasePage.goToMenu('Projects', 'Projects');
    await projectPageJS.waitForPageLoad()
    await projectPageJS.filterProjectsByStatus("Hidden")
    await projectPageJS.verifyProjectExist(projectId.toString())

    // Create and delete a product with the associated project.
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.waitForPageLoad();
    await orderPageJS.createItem();
    await orderPageJS.addCustomer(getEnvironmentValue('default_customer'));
    let internalText1 = `Manual_Test_Order_Copy_${Math.random().toString(36).substring(2, 7)}`
    await orderPageJS.addInternalText(internalText1);
    await orderPageJS.setVatType('Subject to VAT');
    await orderPageJS.expandProducts();
    await orderPageJS.newProductRow("999", '1', '100');
    const res2 = await orderPageJS.saveOrder();
    projectId = res2.projectId
    await orderPageJS.deleteOrder("Yes", "OK")
    await salesBasePage.goToMenu('Projects', 'Projects');
    await projectPageJS.waitForPageLoad()
    await projectPageJS.filterProjectsByStatus("Hidden")
    await projectPageJS.verifyProjectExist(projectId.toString(),0)
});
