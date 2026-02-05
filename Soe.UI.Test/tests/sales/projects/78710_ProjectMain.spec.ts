import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';

let testCaseId: string = '78710';
let envUrl: string;
const projectName = `Auto Project ${Math.random().toString(36).substring(2, 7)} 78710`;
const projectDescription = `Regtest Project_Main ${new Date().toISOString().slice(2, 10).replace(/-/g, '')} 78710`;
let participantType: string = getEnvironmentValue('default_participantType') ?? '';
let participantUser: string = getEnvironmentValue('default_participantUser') ?? '';

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });

test.beforeEach(async ({ accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Projects");
    await allure.subSuite("Project");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Verify project main details : SR', { tag: ['@Sales', '@Projects', '@Regression'] }, async ({ projectPageJS, salesBasePage }) => {
    await salesBasePage.goToMenu('Projects', 'Projects');
    await projectPageJS.waitForPageLoad();
    await salesBasePage.createItem();
    await projectPageJS.waitForCreatePageLoad();
    await projectPageJS.addProjectName(projectName);
    await projectPageJS.addDescription(projectDescription);
    const projectNumber = await projectPageJS.verifyProjectNumber();
    await projectPageJS.expandParticipants();
    await projectPageJS.clickAdd();
    await projectPageJS.verifyParticipantsDialogAppears();
    await projectPageJS.setParticipants(participantType, participantUser);
    await projectPageJS.expandAccounts();
    await projectPageJS.setDetailsToAccounts();
    await projectPageJS.expandProjectBudget();
    await projectPageJS.setDetailsToProjectBudget({
        editRows: [
            { index: 0, totalAmount: '2000', ibValue: '1000' },
            { index: 1, totalAmount: '3000', ibValue: '500' }
        ],
        addRow: { totalAmount: '1000', hours: '5' }
    });
    await projectPageJS.expandPriceList();
    await projectPageJS.clickAddPriceList();
    await projectPageJS.setNewPriceList();
    await projectPageJS.setPriceListDetails();
    await projectPageJS.save();
    await projectPageJS.verifyAccountDetails();
    await projectPageJS.verifyPriceListDetails();
    await projectPageJS.verifyProjectBudgetDetails();
    await projectPageJS.closeTab();
    await projectPageJS.reloadPage();
    await projectPageJS.verifyFilteredProjectDetails(projectNumber, projectName, projectDescription, participantUser);
});