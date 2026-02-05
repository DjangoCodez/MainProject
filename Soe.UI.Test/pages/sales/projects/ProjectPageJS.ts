import { expect, Page } from "@playwright/test";
import { SalesBasePage } from "../SalesBasePage";
import * as allure from "allure-js-commons";
import { SingleGridPageJS } from "../../common/SingleGridPageJS";
import { SectionGridPageJS } from "pages/common/SectionGridPageJS";


export class ProjectPageJS extends SalesBasePage {
    readonly projectGrid: SingleGridPageJS;
    readonly projectAccountsGrid: SectionGridPageJS;
    readonly projectPriceListGrid: SectionGridPageJS;
    readonly projectBudgetGrid: SectionGridPageJS;
    readonly projectOverviewGrid: SingleGridPageJS;
    readonly projectLisGrid: SectionGridPageJS

    constructor(page: Page) {
        super(page);
        this.projectGrid = new SingleGridPageJS(page);
        this.projectAccountsGrid = new SectionGridPageJS(page, 'billing.projects.list.accounts', 'ctrl.gridAg.options.gridOptions');
        this.projectPriceListGrid = new SectionGridPageJS(page, 'billing.projects.list.pricelist', 'ctrl.priceListGridOptions.gridOptions');
        this.projectBudgetGrid = new SectionGridPageJS(page, 'billing.projects.list.budget', 'ctrl.gridAg.options.gridOptions');
        this.projectOverviewGrid = new SingleGridPageJS(page, -1, 'ctrl.gridHandler.gridAg.options.gridOptions');
        this.projectLisGrid = new SectionGridPageJS(page, '', 'ctrl.gridAg.options.gridOptions');
    }

    async waitForPageLoad() {
        await allure.step("Wait for page load", async () => {
            const pageLocator = this.page.getByRole('button', { name: 'Change project status' });
            await expect(pageLocator).toBeVisible({ timeout: 15_000 });
            await this.waitForDataLoad('/api/Billing/Invoice/ProjectList/2/false');
            await this.projectGrid.waitForPageLoad();
            await this.page.waitForTimeout(3_000);
        });
    }

    async filterProjectName(projectName: string) {
        await allure.step("Filter project name " + projectName, async () => {
            await this.projectGrid.filterByName('Name', projectName);
            await this.page.waitForTimeout(2_000);
        });
    }

    async goToPrjectOverview() {
        await allure.step("Go to project overview", async () => {
            await this.projectGrid.clickLinkByColumnId('projectId');
        });
    }

    async waitForCreatePageLoad() {
        await allure.step("Wait for Project page load", async () => {
            await this.waitForDataLoad("api/Core/UserGridState/Billing_Project_Budget");
        });
    }

    async addProjectName(projectName: string) {
        await allure.step("Enter Project name: " + projectName, async () => {
            await this.page.getByRole('textbox', { name: 'Name' }).fill(projectName);
        });
    }

    async addDescription(description: string) {
        await allure.step("Enter Project description: " + description, async () => {
            await this.page.getByRole('textbox', { name: 'Description' }).fill(description);
        });
    }

    async verifyProjectNumber(): Promise<string> {
        return await allure.step("Verify project number is generated", async () => {
            const projectNumberField = this.page.locator('//input[@id="ctrl_project_number"]');
            await projectNumberField.waitFor({ state: 'visible', timeout: 10000 });
            await this.page.waitForFunction(el => (el as HTMLInputElement).value && (el as HTMLInputElement).value.trim() !== "", await projectNumberField.elementHandle(), { timeout: 10000 });
            const projectNumber = await projectNumberField.inputValue();
            console.log('Captured Project Number:', projectNumber);
            expect(projectNumber?.trim(), "Project Number should not be empty").not.toBe("");
            return projectNumber;
        });
    }

    async expandParticipants() {
        await allure.step("Expand Participants tab", async () => {
            await this.page.getByRole('button', { name: 'Participants' }).scrollIntoViewIfNeeded();
            await this.page.getByRole('button', { name: 'Participants' }).click();
        });
    }

    async clickAdd() {
        await allure.step("Click Add button in Participants tab", async () => {
            await this.page.getByTitle('Add').scrollIntoViewIfNeeded();
            await this.page.getByTitle('Add').click();
        });
    }

    async verifyParticipantsDialogAppears() {
        await allure.step("Verify Participants dialog pop-up appears", async () => {
            const dialog = this.page.getByRole('dialog');
            await dialog.waitFor({ state: 'visible', timeout: 10000 });
            await expect(dialog, "Participants dialog pop-up should be visible").toBeVisible();
        });
    }

    async setParticipants(participantType: string, participantUser: string) {
        await allure.step("Set Participants Details in Participants tab", async () => {
            await this.setParticipantType(participantType);
            await this.setUser(participantUser);
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async setParticipantType(participantType: string) {
        await allure.step("Set Participant Type in Participants tab", async () => {
            const labelText = 'participant type';
            const select = this.page.locator(`//label[contains(translate(normalize-space(.),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'${labelText}')]/following::select[1]`);
            await select.waitFor({ state: 'visible', timeout: 2000 });
            await select.selectOption(participantType);
        });
    }

    async setUser(participantUser: string) {
        await allure.step("Set User in Participants tab", async () => {
            await this.page.getByLabel('Users').selectOption(participantUser);
        });
    }

    async expandAccounts() {
        await allure.step("Expand Accounts tab", async () => {
            await this.page.getByRole('button', { name: 'Accounts' }).scrollIntoViewIfNeeded();
            await this.page.getByRole('button', { name: 'Accounts' }).click();
        });
    }

    async setDetailsToAccounts() {
        await allure.step("Set details to accounts", async () => {
            await this.projectAccountsGrid.waitForPageLoad();
            await this.projectAccountsGrid.enterGridValueByColumnId("account1Nr", "4010", 0, true);
            await this.page.waitForTimeout(1000);
            await this.projectAccountsGrid.enterGridValueByColumnId("account1Nr", "3010", 1, true);
            await this.page.waitForTimeout(1000);
            await this.projectAccountsGrid.enterGridValueByColumnId("account1Nr", "3014", 3, true);
        });
    }

    async expandProjectBudget() {
        await allure.step("Expand Project Budget tab", async () => {
            await this.page.getByRole('button', { name: 'Project budget' }).scrollIntoViewIfNeeded();
            await this.page.getByRole('button', { name: 'Project budget' }).click();
        });
    }

    async setDetailsToProjectBudget(options?: {
        editRows?: { index: number; totalAmount: string; ibValue?: string }[];
        addRow?: { totalAmount: string; hours: string };
        materialRow?: { materialCode: string; budgetAmount: string };
    }) {
        await allure.step("Set details to Project Budget", async () => {
            await this.page.waitForTimeout(1_000);
            // staff income, income material, cost for expenses, overhead rate
            if (options?.editRows) {
                for (const row of options.editRows) {
                    await this.editBudgetRow(row.index, row.totalAmount, row.ibValue);
                }
            }
            // staff costs
            if (options?.addRow) {
                await this.addBudgetRowWithFirstTimeCode(options.addRow.totalAmount, options.addRow.hours);
            }
            // material costs
            if (options?.materialRow) {
                await this.addMaterialBudgetRow(options.materialRow.materialCode, options.materialRow.budgetAmount);
            }
        });
    }

    async editBudgetRow(index: number, totalAmount: string, ibValue?: string) {
        await allure.step(`Edit budget row ${index}`, async () => {
            const page = this.page;
            await page.getByRole('button', { name: '' }).nth(index).click();
            await page.locator("//input[@id='ctrl_row_totalAmount']").fill(totalAmount);
            if (ibValue) {
                await page.locator("//input[@id='ctrl_row_ib']").fill(ibValue);
            }
            await page.locator("//button[normalize-space()='OK']").click();
        });
    }

    async addBudgetRowWithFirstTimeCode(totalAmount: string, hours: string) {
        await allure.step("Add new budget row with first Time Code option", async () => {
            const page = this.page;
            await page.getByRole('button', { name: '+' }).first().click();
            await page.locator(".modal-body").waitFor({ state: 'visible', timeout: 5000 });
            const timeCodeSelect = page.locator("//select[@id='ctrl_row_timeCodeId']");
            await timeCodeSelect.click({ delay: 200 });
            const firstOptionValue = await timeCodeSelect.locator('option').first().getAttribute('value');
            if (firstOptionValue) {
                await timeCodeSelect.selectOption(firstOptionValue);
            }
            await page.locator("//input[@id='ctrl_row_totalAmount']").fill(totalAmount);
            await page.locator("//input[@id='ctrl_row_hours']").fill(hours);
            await page.locator("//button[normalize-space()='OK']").click();
        });
    }

    async addMaterialBudgetRow(materialCode: string, budgetAmount: string) {
        await allure.step("Add material budget row", async () => {
            const page = this.page;
            await page.getByRole('button', { name: '+' }).nth(1).click();
            await page.getByLabel('Material code').selectOption(materialCode);
            await page.getByRole('textbox', { name: 'Budget' }).fill(budgetAmount);
            await page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async expandPriceList() {
        await allure.step("Expand Price List tab", async () => {
            await this.page.getByRole('button', { name: 'Price List' }).scrollIntoViewIfNeeded();
            await this.page.getByRole('button', { name: 'Price List' }).click();
        });
    }

    async clickAddPriceList() {
        await allure.step("Click Add New Price List Button", async () => {
            await this.page.waitForTimeout(1_000);
            await this.page.getByTitle('New price list').scrollIntoViewIfNeeded();
            await this.page.getByTitle('New price list').click();
        });
    }

    async setNewPriceList(priceName: string = 'Swedish Krona') {
        await allure.step("Set New Price List", async () => {
            await this.page.waitForTimeout(3000);
            const uniqueNumber = Math.floor(10000 + Math.random() * 90000);
            await this.page.locator('#ctrl_priceList_name').fill(`Price List ${uniqueNumber} 78710`,);
            const price = this.page.locator('#ctrl_priceList_currencyId')
            await price.click();
            await price.selectOption({ label: priceName });
            await this.page.getByRole('button', { name: 'Save' }).click();
        });
    }

    async setPriceListDetails() {
        await allure.step("Set Price List Details", async () => {
            await this.projectPriceListGrid.waitForPageLoad();
            await this.projectPriceListGrid.enterGridValueByColumnId("price", "50", 0);
            await this.projectPriceListGrid.enterGridValueByColumnId("price", "2000", 1);
        });
    }

    async save() {
        await allure.step('Save', async () => {
            const saveBtn = this.page.getByRole('button', { name: 'Save' });
            await saveBtn.scrollIntoViewIfNeeded();
            await saveBtn.click();
        });
    }

    async verifyAccountDetails() {
        await this.verifyAccountValue(0, "4010- Inköp material tillv prod");
        await this.verifyAccountValue(1, "3010- Försäljning");
        await this.verifyAccountValue(3, "3014Försäljning momsfri");
    }

    async verifyAccountValue(rowIndex: number, expectedAccount: string) {
        await allure.step(`Verify Account value in row ${rowIndex}`, async () => {
            const account = await this.projectAccountsGrid.getRowColumnValue('account1Nr', rowIndex);
            expect(account).toBe(expectedAccount);
        });
    }

    async verifyPriceListDetails() {
        await this.verifyPriceValue(0, "50,00");
        await this.verifyPriceValue(1, "2 000,00");
    }

    async verifyPriceValue(rowIndex: number, expectedPrice: string) {
        await allure.step(`Verify Price value in row ${rowIndex}`, async () => {
            const price = await this.projectPriceListGrid.getRowColumnValue('price', rowIndex);
            expect(price).toBe(expectedPrice);
        });
    }

    async verifyProjectBudgetDetails() {
        await this.verifyBudgetValue(0, "2 000");
        await this.verifyIBValue(0, "1 000");

        await this.verifyBudgetValue(1, "3 000");
        await this.verifyIBValue(1, "500");

        await this.verifyBudgetValue(2, "1 000");
        await this.verifyHoursValue(2, "5");
    }

    async verifyBudgetValue(rowIndex: number, expectedBudget: string) {
        await allure.step(`Verify Budget value in row ${rowIndex}`, async () => {
            const budget = await this.projectBudgetGrid.getRowColumnValue('budget', rowIndex);
            expect(budget).toBe(expectedBudget);
        });
    }

    async verifyIBValue(rowIndex: number, expectedIB: string) {
        await allure.step(`Verify IB value in row ${rowIndex}`, async () => {
            const ib = await this.projectBudgetGrid.getRowColumnValue('ib', rowIndex);
            expect(ib).toBe(expectedIB);
        });
    }

    async verifyHoursValue(rowIndex: number, expectedHours: string) {
        await allure.step(`Verify Hours value in row ${rowIndex}`, async () => {
            const hours = await this.projectBudgetGrid.getRowColumnValue('totalHours', rowIndex);
            expect(hours).toBe(expectedHours);
        });
    }

    async closeTab(index: number = 0) {
        await allure.step("Close tab", async () => {
            const orderCloseButton = this.page.locator("//i[contains(@class, 'fa-times') and @title='Close']").nth(index)
            await orderCloseButton.click();
        });
    }

    async reloadPage() {
        await allure.step("Reload page", async () => {
            await this.page.getByRole('toolbar').getByTitle('Reload records').click();
            await this.page.waitForTimeout(1_000);
        });
    }

    async verifyFilteredProjectDetails(number: string, name: string, description: string, participantUser: string) {
        await allure.step("Filter by project number : " + number, async () => {
            await this.projectGrid.filterByName('Number', number);
            const filteredCount = await this.projectGrid.getFilteredAgGridRowCount();
            expect(filteredCount).toBeGreaterThan(0);
            const actualName = await this.projectGrid.getRowColumnValue('name', 0);
            expect(actualName).toBe(name);
            const actualDescription = await this.projectGrid.getRowColumnValue('description', 0);
            expect(actualDescription).toBe(description);
            const projectManager = await this.projectGrid.getRowColumnValue('managerName', 0);
            expect(projectManager).toBe(participantUser);
        });
    }

    async waitOverviewPageLoad() {
        await allure.step("Wait for Overview page load", async () => {
            await this.waitForDataLoad("api/Core/UserGridState/Common_Directives_TimeProjectReport_ProjectCentral");
        });
    }

    async searchProjectInOverview(projectNumber: string) {
        await allure.step("Select project : " + projectNumber, async () => {
            await this.page.getByRole('textbox', { name: 'Number Filter Input', exact: true }).fill(projectNumber);
            await this.page.waitForTimeout(3_000);
            await this.page.getByRole('button', { name: 'OK' }).click();
        });
    }

    async addCustomer(customer: string) {
        await allure.step("Enter Customer: " + customer, async () => {
            const customerField = this.page.getByLabel('Customer', { exact: true });
            await customerField.click();
            await customerField.fill(customer);
            const option = this.page.locator('.uib-typeahead-match', { hasText: customer });
            await option.waitFor({ state: 'visible' });
            await option.click();
        });
    }

    async clickSearchInMainProject() {
        await allure.step("Click Search in Main Project", async () => {
            await this.page.getByRole('button', { name: '' }).click();
        });
    }

    async selectMainProject(mainProjectNumber: number) {
        await allure.step("Search Main Project Number: " + mainProjectNumber, async () => {
            await this.page.getByRole('textbox', { name: 'Number Filter Input', exact: true }).first().fill(mainProjectNumber.toString());
            await this.waitForDataLoad("**/Billing/Invoice/Project/Search/**");
            await this.page.waitForTimeout(2_000);
            await this.page.locator('//div[@class="modal-footer"]//button').nth(1).click({ force: true });
        });
    }

    async clickLoadData() {
        await allure.step("Click Load Data button", async () => {
            await this.page.getByRole('button', { name: 'Load data' }).scrollIntoViewIfNeeded();
            await this.page.getByRole('button', { name: 'Load data' }).click();
            await this.page.waitForTimeout(2_000);
        });
    }

    async verifyProjectOverviewRow(rowIndex: number, expected: { budget?: string, deviation?: string, budgetTime?: string, deviationTime?: string, result?: string, resultTime?: string }) {
        await allure.step(`Verify Project Overview row ${rowIndex}`, async () => {
            if (expected.budget !== undefined) {
                const budget = await this.projectOverviewGrid.getRowColumnValue('budget', rowIndex);
                expect(budget).toBe(expected.budget);
            }
            if (expected.deviation !== undefined) {
                const diff = await this.projectOverviewGrid.getRowColumnValue('diff', rowIndex);
                expect(diff).toBe(expected.deviation);
            }
            if (expected.budgetTime !== undefined) {
                const time = await this.projectOverviewGrid.getRowColumnValue('budgetTimeFormatted', rowIndex);
                expect(time).toBe(expected.budgetTime);
            }
            if (expected.deviationTime !== undefined) {
                const time = await this.projectOverviewGrid.getRowColumnValue('diffTimeFormatted', rowIndex);
                expect(time).toBe(expected.deviationTime);
            }
            if (expected.result !== undefined) {
                const time = await this.projectOverviewGrid.getRowColumnValue('value', rowIndex);
                expect(time).toBe(expected.result);
            }
            if (expected.resultTime !== undefined) {
                const time = await this.projectOverviewGrid.getRowColumnValue('valueTimeFormatted', rowIndex);
                expect(time).toBe(expected.resultTime);
            }
        });
    }

    async clickNewOrder() {
        await allure.step("Click New Order button", async () => {
            await this.page.getByTitle('Create new order').scrollIntoViewIfNeeded();
            await this.page.getByTitle('Create new order').click();
        });
    }

    async verifyCopiedProjectNumber(projectNumber: string) {
        await allure.step(`Verify copied project number`, async () => {
            const projectNoField = this.page.getByRole('textbox', { name: 'Project no.' });
            const displayedValue = await projectNoField.inputValue();
            expect(displayedValue).toBe(projectNumber);
        });
    }

    async goToProjectTab() {
        await allure.step("Go to Project tab", async () => {
            await this.page.locator('#ng-app-bootstrap-element').getByRole('link', { name: "Projects", exact: true }).click();
        });
    }

    async verifyFilteredProjectDetailsByMainProject(mainProjectNumber: number, projectNumber: string, projectName: string) {
        await allure.step("Filter by Main Project number : " + mainProjectNumber, async () => {
            await this.projectGrid.filterByName('Number', mainProjectNumber.toString());
            const filteredCount = await this.projectGrid.getFilteredAgGridRowCount();
            expect(filteredCount).toBeGreaterThan(0);
            const actualName = await this.projectGrid.getRowColumnValue('childProjects', 0);
            expect(actualName).toBe(projectNumber + " - " + projectName);

        });
    }

    async filterProjectsByStatus(status: string) {
        await allure.step(`Filter projects by ${status}`, async () => {
            const dropDown = this.page.locator("#ctrl_projectStatusSelection")
            await dropDown.click({ delay: 1000 })
            await dropDown.selectOption({ label: status })
            await this.waitForDataLoad(`**/Billing/Invoice/ProjectList/**/false`)
        })
    }

    async verifyProjectExist(projectNumber: string, rowCount: number = 1) {
        await allure.step(`Filter project by ${projectNumber}`, async () => {
            await this.projectLisGrid.filterByName("Number", projectNumber)
            const count = await this.projectLisGrid.getAgGridRowCount()
            expect(count).toBe(rowCount)
        })
    }
}