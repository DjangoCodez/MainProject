import { test } from '../../fixtures/finance-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';
import { getFormatYYMMDD } from "utils/CommonUtil";

let testCaseId: string = '78612';
let envUrl: string;
let assignmentTypeName: string = 'Planning_AssignmentTypeCreate Order' + ' ' + Math.floor(100000 + Math.random() * 900000) + '_' + testCaseId;
let assignmentTypeReservationName: string = 'Planning_AssignmentTypeCreate Booking' + ' ' + Math.floor(100000 + Math.random() * 900000) + '_' + testCaseId;

test.beforeEach(async ({ accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Orders");
    await allure.subSuite("Planning");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': AssignmentType Create Order : AP', { tag: ['@Sales', '@Orders', '@Regression'] }, async ({ page, salesBasePage, assignmentTypesPage, purchasePriceListPage }) => {
    assignmentTypeName = await getFormatYYMMDD() + '_' + assignmentTypeName;
    assignmentTypeReservationName = await getFormatYYMMDD() + '_' + assignmentTypeReservationName;
    await salesBasePage.goToMenu('Settings_Sales', 'Assignment Types', true, 'Sales');
    await assignmentTypesPage.goToPageVersion(assignmentTypesPage.ang_version);
    await salesBasePage.createItem();
    await assignmentTypesPage.setType('Order');
    await assignmentTypesPage.name(assignmentTypeName);
    await assignmentTypesPage.color('#FF0000');
    await assignmentTypesPage.length('4');
    await assignmentTypesPage.saveAssignmentType();
    await assignmentTypesPage.close();
    await assignmentTypesPage.filterByName(assignmentTypeName);
    await assignmentTypesPage.edit();
    await assignmentTypesPage.waitForPageLoad();
    await page.waitForTimeout(800);
    await assignmentTypesPage.verifyType('Order');
    await assignmentTypesPage.verifyName(assignmentTypeName);
    await assignmentTypesPage.verifyColor('#FF0000');
    await assignmentTypesPage.verifyLength('4:00');
    await assignmentTypesPage.close();
    await salesBasePage.createItem();
    await assignmentTypesPage.setType('Booking');
    await assignmentTypesPage.name(assignmentTypeReservationName);
    await assignmentTypesPage.color('#00EAFF');
    await assignmentTypesPage.length('2');
    await assignmentTypesPage.saveAssignmentType();
    await assignmentTypesPage.close();
    await assignmentTypesPage.filterByName(assignmentTypeReservationName);
    await assignmentTypesPage.edit();
    await assignmentTypesPage.waitForPageLoad();
    await page.waitForTimeout(800);
    await assignmentTypesPage.verifyType('Booking');
    await assignmentTypesPage.verifyName(assignmentTypeReservationName);
    await assignmentTypesPage.verifyColor('#00EAFF');
    await assignmentTypesPage.verifyLength('2:00');
});
