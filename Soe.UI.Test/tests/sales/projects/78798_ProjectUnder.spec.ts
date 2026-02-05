import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';
import { ProjectUtils } from '../../../apis/utils/ProjectUtils';
import { generateRandomId } from 'utils/CommonUtil';

let testCaseId: string = '78798';
let envUrl: string;
let mainProjectName = `Auto MainProject ${Math.random().toString(36).substring(2, 7)}` + ' ' + testCaseId;
let projectName: string = `Auto ProjectUnder  ${Math.random().toString(36).substring(2, 7)}` + ' ' + testCaseId;
let projectUtils: ProjectUtils;
let customer: string = getEnvironmentValue('default_customer')?.toString() ?? '';

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });
test.beforeEach(async ({ accountEx, salesBasePage, page }, testInfo) => {
    const uniqueId = generateRandomId(testInfo,testCaseId);
    await allure.parentSuite("Sales");
    await allure.suite("Projects");
    await allure.subSuite("Project");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl,getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
    projectUtils = new ProjectUtils(page, envUrl);
    await projectUtils.addProject(mainProjectName, uniqueId);
});

test(testCaseId + ': Verify Project Under : SR', { tag: ['@Sales', '@Projects', '@Regression'] }, async ({ projectPageJS, salesBasePage }) => {
    await salesBasePage.goToMenu('Projects', 'Projects');
    await projectPageJS.waitForPageLoad();
    const mainProjectNumber = await projectUtils.getProjectNumberByName(mainProjectName);
    await salesBasePage.createItem();
    await projectPageJS.addProjectName(projectName);
    const projectNumber = await projectPageJS.verifyProjectNumber();
    await projectPageJS.addCustomer(customer);
    await projectPageJS.clickSearchInMainProject();
    await projectPageJS.selectMainProject(mainProjectNumber ?? 0);
    await projectPageJS.save();
    await projectPageJS.closeTab();
    await projectPageJS.reloadPage();
    await projectPageJS.verifyFilteredProjectDetailsByMainProject(mainProjectNumber ?? 0, projectNumber, projectName);
});