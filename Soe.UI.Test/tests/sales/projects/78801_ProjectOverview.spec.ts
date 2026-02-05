import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue, getEnvironmentValue } from '../../../utils/properties';

let testCaseId: string = '78801';
let envUrl: string;
const projectName = `Auto Project ${Math.random().toString(36).substring(2, 7)} 78801`;
const internalText = `Regtest Project_Overview ${new Date().toISOString().slice(2, 10).replace(/-/g, '')} 78801`;
let materialCodeValue: string = getEnvironmentValue('default_materialCode') ?? '';
let customer: string = getEnvironmentValue('default_customer') ?? '';

test.use({ account: { domain: process.env.defualt_domain!, user: 'adminERP' } });
test.beforeEach(async ({ accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Projects");
    await allure.subSuite("Project");
    await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test(testCaseId + ': Verify Project Overview : SR', { tag: ['@Sales', '@Projects', '@Regression'] }, async ({ projectPageJS, salesBasePage, orderPageJS }) => {
    await salesBasePage.goToMenu('Projects', 'Projects');
    await projectPageJS.waitForPageLoad();
    await salesBasePage.createItem();
    await projectPageJS.addProjectName(projectName);
    const projectNumber = await projectPageJS.verifyProjectNumber();
    await projectPageJS.expandProjectBudget();
    await projectPageJS.setDetailsToProjectBudget({
        editRows: [
            { index: 0, totalAmount: '2000' },
            { index: 1, totalAmount: '3000' },
            { index: 2, totalAmount: '200' },
            { index: 3, totalAmount: '400' }
        ],
        addRow: { totalAmount: '1000', hours: '5' },
        materialRow: { materialCode: materialCodeValue, budgetAmount: '2000' },
    });
    await projectPageJS.save();
    await salesBasePage.goToMenu('Projects', 'Overview');
    await projectPageJS.waitOverviewPageLoad();
    await projectPageJS.searchProjectInOverview(projectNumber);
    await projectPageJS.clickLoadData();
    await projectPageJS.verifyProjectOverviewRow(1, { budget: "5 000,00", deviation: "-5 000,00" });
    await projectPageJS.verifyProjectOverviewRow(2, { budget: "2 000,00", deviation: "-2 000,00" });
    await projectPageJS.verifyProjectOverviewRow(3, { budget: "1 000,00", deviation: "-1 000,00", budgetTime: "05:00", deviationTime: "-5:00" });
    await projectPageJS.verifyProjectOverviewRow(4, { budget: "200,00", deviation: "-200,00" });
    await projectPageJS.verifyProjectOverviewRow(5, { budget: "400,00", deviation: "-400,00" });
    await projectPageJS.clickNewOrder();
    await projectPageJS.verifyCopiedProjectNumber(projectNumber);
    await orderPageJS.addCustomer(customer);
    await orderPageJS.addInternalText(internalText);
    await orderPageJS.expandProducts();
    await orderPageJS.newProductRow("999", "2300", "10", "Ströartikel", 0, false);
    await orderPageJS.saveOrder();
    await orderPageJS.expandTimes();
    await orderPageJS.addWork();
    await orderPageJS.addColumnToProductRows("Purchase Price");
    await orderPageJS.updateQuantity("5", 0);
    await orderPageJS.updatePrice("1000", 0);
    await orderPageJS.updatePurchasePrice("800", 0);
    await orderPageJS.updatePrice("500", 1);
    await orderPageJS.saveOrder();
    await projectPageJS.goToProjectTab();
    await projectPageJS.clickLoadData();
    await projectPageJS.verifyProjectOverviewRow(0, { result: "6 000,00" });
    await projectPageJS.verifyProjectOverviewRow(1, { budget: "5 000,00", deviation: "-5 000,00" });
    await projectPageJS.verifyProjectOverviewRow(2, { budget: "2 000,00", result: "4 000,00", deviation: "2 000,00" });
    //await projectPageJS.verifyProjectOverviewRow(3, { budget: "1 000,00", budgetTime: "05:00", result: "1 494,00", resultTime: "02:00", deviation: "494,00", deviationTime: "-3:00" });
    await projectPageJS.verifyProjectOverviewRow(3, { budget: "1 000,00", budgetTime: "05:00", deviation: "-1 000,00", deviationTime: "-3:00" });
    await projectPageJS.verifyProjectOverviewRow(4, { budget: "200,00", deviation: "-200,00" });
    await projectPageJS.verifyProjectOverviewRow(5, { budget: "400,00", deviation: "-400,00" });
});