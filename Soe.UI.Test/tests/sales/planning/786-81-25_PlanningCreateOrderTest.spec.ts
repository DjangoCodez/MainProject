import { test, expect } from '../../fixtures/finance-fixture';
import * as allure from 'allure-js-commons';
import { getAccountExValue } from '../../../utils/properties';
import { OrderUtil } from '../../../apis/utils/OrderUtil';
import { getDateUtil } from '../../../utils/CommonUtil';

let envUrl: string;
let orderUtils: OrderUtil;
let orderNumber: number;

test.beforeEach(async ({ page, accountEx, staffBasePage }) => {
  await allure.parentSuite('Sales');
  await allure.suite('Orders');
  await allure.subSuite('Planning');
  envUrl = accountEx.baseUrl;
  await staffBasePage.SetupStaff(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
  orderUtils = new OrderUtil(page, envUrl);
  orderNumber = await orderUtils.CreateOrder();
});

test.describe('Planning create order', () => {

  test('78625: Planning create order - Ignore breaks OFF : AP', { tag: ['@Sales', '@Orders', '@Regression'] }, async ({ staffBasePage, staffPlanningPage, salesBasePage, orderPageJS, planningPageJS }) => {
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/78625', 'Test case : 78625');
    const date = await getDateUtil(0);
    await staffBasePage.goToMenu('Settings_Staff ', 'Company Settings');
    await staffPlanningPage.waitforPageLoad();
    await staffPlanningPage.goToPlanningTab();
    await staffPlanningPage.toggleIgnoreBreaksWhenPlanning(false);
    await staffPlanningPage.savePlanningSettings();
    await staffBasePage.goToModule('Sales');
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.waitForOrderLoading();
    await orderPageJS.filterAllOrders();
    await orderPageJS.filterByOrderNo(orderNumber.toString());
    await orderPageJS.editOrder();
    await orderPageJS.expandPlanning();
    await orderPageJS.selectAssignmentType();
    await orderPageJS.planStartDate(date);
    await orderPageJS.saveOrder();
    await salesBasePage.goToMenu('Order', 'Planning');
    await planningPageJS.waitOrderLoading();
    await planningPageJS.filterOrder(orderNumber.toString());
  }
  );

  test('78681: Planning create order - Ignore breaks ON : AP', { tag: ['@Sales', '@Orders', '@Regression'] }, async ({ staffBasePage, staffPlanningPage, salesBasePage, orderPageJS, planningPageJS }) => {
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/78681', 'Test case : 78681');
    const date = await getDateUtil(0);
    await staffBasePage.goToMenu('Settings_Staff ', 'Company Settings');
    await staffPlanningPage.waitforPageLoad();
    await staffPlanningPage.goToPlanningTab();
    await staffPlanningPage.toggleIgnoreBreaksWhenPlanning(true);
    await staffPlanningPage.savePlanningSettings();
    await staffBasePage.goToModule('Sales');
    await salesBasePage.goToMenu('Order', 'Order');
    await orderPageJS.waitForOrderLoading();
    await orderPageJS.filterAllOrders();
    await orderPageJS.filterByOrderNo(orderNumber.toString());
    await orderPageJS.editOrder();
    await orderPageJS.expandPlanning();
    await orderPageJS.selectAssignmentType();
    await orderPageJS.planStartDate(date);
    await orderPageJS.saveOrder();
    await salesBasePage.goToMenu('Order', 'Planning');
    await planningPageJS.waitOrderLoading();
    await planningPageJS.filterOrder(orderNumber.toString());
  }
  );

});
