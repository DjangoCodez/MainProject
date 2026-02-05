import { Page } from "@playwright/test";
import * as allure from "allure-js-commons";
import { BasePage } from "pages/common/BasePage";
import { GridPage } from "pages/common/GridPage";

export class SuppliersPage extends BasePage {

    readonly page: Page;
    readonly paymentDetailsGrid: GridPage;
    readonly supplierGrid: GridPage;

    constructor(page: Page) {
        super(page);
        this.page = page;
        this.paymentDetailsGrid = new GridPage(page, "Common.Directives.PaymentInformation.Domestic");
        this.supplierGrid = new GridPage(page, "Economy.Supplier.Suppliers");
    }

    async waitForNewSupplierLoad() {
        allure.step("Wait For New Supplier Load", async () => {
            await this.waitForDataLoad('/api/V2/Economy/Supplier/Supplier/NextSupplierNr/');
        });
    }

    async waitForSupplierGridLoaded() {
        allure.step("Wait For Supplier Grid Loaded", async () => {
            await this.waitForDataLoad('/api/V2/Economy/Supplier/Supplier/?onlyActive=false&supplierId=undefined');
        });
    }

    async enterSupplierName(supplierName: string) {
        allure.step("Enter Supplier Name: " + supplierName, async () => {
            const supplierNameInput = this.page.getByTestId("name");
            await supplierNameInput.waitFor({ state: 'visible' });
            await supplierNameInput.fill('');
            await supplierNameInput.fill(supplierName);
        });
    }

    async enterSupplierNumber(supplierNumber: string) {
        allure.step("Enter Supplier Number: " + supplierNumber, async () => {
            const supplierNumberInput = this.page.getByTestId("supplierNr");
            await supplierNumberInput.waitFor({ state: 'visible' });
            await supplierNumberInput.fill('');
            await supplierNumberInput.fill(supplierNumber);
            await supplierNumberInput.press('Tab').then(() => this.page.waitForTimeout(500));
        });
    }

    async expandSettings() {
        await allure.step("Expand Settings", async () => {
            const settingsExpand = this.page.getByTestId("common.settings");
            await settingsExpand.waitFor({ state: 'visible' });
            await settingsExpand.click();
        });
    }

    async expandPaymentDetails() {
        await allure.step("Expand Payment Details", async () => {
            const paymentDetailsExpand = this.page.getByTestId("economy.supplier.supplier.paymentinformation-container");
            await paymentDetailsExpand.waitFor({ state: 'visible' });
            await paymentDetailsExpand.click();
        });
    }

    async addRowInPaymentDetails(index: number) {
        await allure.step("Add Row In Payment Details", async () => {
            const addRowButton = this.page.getByTestId("standard");
            await addRowButton.nth(index).waitFor({ state: 'visible' });
            await addRowButton.nth(index).scrollIntoViewIfNeeded();
            await addRowButton.nth(index).click();
        });
    }

    async addPaymentType(paymentType: string) {
        await allure.step("Add Payment Type: " + paymentType, async () => {
            await this.paymentDetailsGrid.selectDropdownValueFromGrid_2("Payment Type", paymentType);
        });
    }

    async addAccountOrIBAN(accountOrIBAN: string) {
        await allure.step("Add Account Or IBAN: " + accountOrIBAN, async () => {
            await this.paymentDetailsGrid.enterValueToGrid("Account/IBAN", accountOrIBAN);
        });
    }

    async verifySupplierCreatedSuccessfully() {
        await allure.step("Verify Supplier Created Successfully: ", async () => {
            await this.waitForDataLoad('/api/V2/Economy/Supplier/Supplier');
        });
    }

    async filterBySupplierName(supplierName: string) {
        await allure.step("Filter By Supplier Name: " + supplierName, async () => {
            await this.supplierGrid.filterByColumnNameAndValue("Name", supplierName);
        });
    }

    async editSupplier() {
        await allure.step("Edit Supplier: ", async () => {
            await this.page.locator("//span[contains(@title,'Edit')]").first().click();
            await this.waitForDataLoad('/api/V2/Economy/Supplier/Supplier/');
        });
    }

    async deleteSupplier() {
        await allure.step("Delete Supplier: ", async () => {
            const confirmDelete = this.page.getByTestId('delete');
            await confirmDelete.waitFor({ state: 'visible' });
            await confirmDelete.click();
            await this.clickAlertMessage(' OK');
            await this.waitForDataLoad('/api/V2/Economy/Supplier/Supplier/');
        });
    }

    async reload() {
        await allure.step("Reload Suppliers Page: ", async () => {
            await this.page.getByTestId('core.reload_data').click();
            await this.waitForDataLoad('/api/V2/Economy/Supplier/Supplier/?onlyActive');
        });
    }

    async verifyDeletedSupplierSuccessfully() {
        await allure.step("Verify Supplier Deleted Successfully: ", async () => {
            await this.supplierGrid.verifyFilteredItemCount('0');
        });
    }

    async getSupplierNumber() {
        return await allure.step("Get Supplier Number: ", async () => {
            return this.supplierGrid.getRowColumnValue("Number", 0);
        });
    }

    async getPaymentAccount() {
        return await allure.step("Get Payment Account: ", async () => {
            return this.supplierGrid.getRowColumnValue("Payment Account", 0);
        });
    }

    async getSupplierName() {
        return await allure.step("Get Supplier Name: ", async () => {
            return this.supplierGrid.getRowColumnValue("Name", 0);
        });
    }

    async enterCompanyRegistrationNumber(companyRegNumber: string) {
        allure.step("Enter Company Registration Number: " + companyRegNumber, async () => {
            const companyReg = this.page.getByTestId("orgNr");
            await companyReg.waitFor({ state: 'visible' });
            await companyReg.fill('');
            await companyReg.fill(companyRegNumber);
        });
    }

    async save() {
        await allure.step("Save Supplier", async () => {
            const save = this.page.getByTestId("save");
            await save.waitFor({ state: 'visible' });
            await save.click();
        });
    }

    async waitForSaveComplete() {
        await allure.step("Wait For Save Complete", async () => {
            await this.waitForDataLoad('/api/V2/Economy/Supplier/Supplier/?onlyActive=false&supplierId');
        });
    }
}