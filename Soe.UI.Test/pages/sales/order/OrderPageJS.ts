import { expect, type Page } from '@playwright/test';
import { SalesBasePage } from '../SalesBasePage';
import * as allure from "allure-js-commons";
import { ModelGridPageJS } from '../../common/ModelGridPageJS';
import { SectionGridPageJS } from '../../common/SectionGridPageJS';
import { SingleGridPageJS } from '../../common/SingleGridPageJS';
import path from 'path/win32';
import { getEnvironmentValue } from '../../../utils/properties';
import { getFormattedDateMMDDYY } from 'utils/CommonUtil';


export class OrderPageJS extends SalesBasePage {

    readonly projectDialogGrid: ModelGridPageJS;
    readonly productRowsGrid: SectionGridPageJS;
    readonly orderGrid: SingleGridPageJS;
    readonly registerTimeGrid: ModelGridPageJS;
    readonly codingRowGrid: SectionGridPageJS;
    readonly checklistGrid: SectionGridPageJS;
    readonly documentGrid: SectionGridPageJS;
    readonly timesRowGrid: SectionGridPageJS;
    readonly showLinkedTimeRowsGrid: ModelGridPageJS;
    readonly selectRowMoveGrid: ModelGridPageJS;

    constructor(page: Page) {
        super(page);
        this.projectDialogGrid = new ModelGridPageJS(page);
        this.productRowsGrid = new SectionGridPageJS(page, 'billing.order.productrows');
        this.orderGrid = new SingleGridPageJS(page);
        this.registerTimeGrid = new ModelGridPageJS(page, 'ctrl.gridHandler.gridAg.options.gridOptions');
        this.codingRowGrid = new SectionGridPageJS(page, 'common.customer.invoices.accountingrows');
        this.checklistGrid = new SectionGridPageJS(page, 'common.checklists');
        this.documentGrid = new SectionGridPageJS(page, 'core.document', 'directiveCtrl.gridAg.options.gridOptions');
        this.timesRowGrid = new SectionGridPageJS(page, 'billing.order.times');
        this.showLinkedTimeRowsGrid = new ModelGridPageJS(page, 'directiveCtrl.soeGridOptions.gridOptions');
        this.selectRowMoveGrid = new ModelGridPageJS(page, 'ctrl.soeGridOptions.gridOptions');
    }

    async waitForPageLoad() {
        await allure.step("Wait for Order page to load", async () => {
            await this.waitForDataLoad('/api/Core/CustomerInvoices/');
            await this.page.waitForTimeout(3000);
        });
    }

    async addCustomer(customerName: string = 'first') {
        await allure.step(`Add customer: ${customerName}`, async () => {
            const customerInput = this.page.locator('#ctrl_selectedCustomer');
            await customerInput.waitFor({ state: 'visible', timeout: 10000 });
            await customerInput.click();
            await customerInput.clear()
            await customerInput.fill(customerName);
            await this.page.getByRole('link', { name: customerName }).first().click();
        });
    }

    async setVatType(vatType: string) {
        await allure.step(`Set VAT type: ${vatType}`, async () => {
            await this.page.locator('//select[@id="ctrl_invoice_vatType"]').click({ force: true, delay: 200 })
            await this.page.getByLabel('VAT Type').selectOption({ label: vatType }, { force: true });
        });
    }

    async setAgreementType(agreementType: string) {
        await allure.step(`Set agreement type: ${agreementType}`, async () => {
            const dropDown = this.page.locator('//select[@id="ctrl_selectedFixedPriceOrder"]')
            await dropDown.click({ force: true, delay: 2000 })
            await dropDown.selectOption({ label: agreementType }, { force: true, timeout: 1000 });
        });
    }

    async verifyVatType(vatType: string) {
        await allure.step(`Verify VAT type: ${vatType}`, async () => {
            const dropDown = this.page.getByLabel('VAT Type');
            const isSelected = await dropDown.locator(`option[label="${vatType}"]`).getAttribute('selected');
            expect(isSelected).toBe('selected');
        });
    }

    async linkProject(projectName: string, isProjWithCustomer: boolean = true, isShowMine: boolean = false) {
        await allure.step(`Link project: ${projectName}`, async () => {
            await this.page.locator("//div[@options='ctrl.projectFunctions']//button[contains(@class,'dropdown-toggle')]").click();
            const responsePromise = this.page.waitForResponse('**/Billing/Invoice/Project/Search/');
            await this.page.getByRole('link', { name: 'Connect to project' }).click();
            await responsePromise;
            await this.projectDialogGrid.waitForPageLoad();
            await this.projectDialogGrid.unselectAllCheckBox();
            await this.page.waitForTimeout(1000);
            if (isProjWithCustomer) {
                await this.projectDialogGrid.checkCheckboxTicked("Show projects without customer");
            }
            if (isShowMine) {
                await this.projectDialogGrid.checkCheckboxTicked("Show mine");
            }
            await this.projectDialogGrid.filterByName('Name', projectName);
            await this.page.waitForLoadState('load');
            await this.page.waitForTimeout(2000);
            await this.page.getByRole('button', { name: 'OK' }).click();
            await this.page.waitForTimeout(1000);
        });
    }

    async addInternalText(internalText: string) {
        await allure.step(`Add internal text: ${internalText}`, async () => {
            const internalTextInput = this.page.locator('#ctrl_invoice_originDescription')
            await internalTextInput.click();
            await internalTextInput.fill(internalText);
        });
    }

    async addFirstAccountProject(): Promise<string> {
        return await allure.step(`Add firstaccount project:`, async () => {
            const projectInput = this.page.locator('#ctrl_selectedAccount3');
            await projectInput.fill('1');
            const ariaOwns = await projectInput.getAttribute('aria-owns');
            const firstOption = this.page.locator(`//ul[@id="${ariaOwns}"]//li[@role='option']//a`).first();
            //await firstOption.waitFor({ state: 'visible', timeout: 5000 });
            let firstOptionText: string = await firstOption.textContent() ?? '';
            await firstOption.click();
            return firstOptionText.trim();
        });
    }

      async addProduct(productName: string, productPrice?: string) {
        await allure.step(`Add product: ${productName}${productPrice ? ` with price: ${productPrice}` : ''}`, async () => {
            const newRowButton = this.page.getByRole('button', { name: 'New product row' });
            await newRowButton.scrollIntoViewIfNeeded();
            await newRowButton.click();
            await this.productRowsGrid.enterGridValueByColumnId('productNr', productName, 0, true);
            await this.page.waitForTimeout(2000);
            if (productPrice !== undefined) {
                await this.productRowsGrid.enterGridValueByColumnId('amountCurrency', productPrice);
                await this.page.waitForTimeout(3000);
            }
        });
    }

    async saveOrder() {
        return await allure.step("Save order", async () => {
            const responsePromise = this.page.waitForResponse('**/Billing/Order/');
            await this.page.getByRole('button', { name: 'Save' }).click();
            const response = await responsePromise;
            const responseBody = await response.json();
            const { stringValue, value2 } = responseBody
            return { orderId: stringValue, projectId: value2?.number }
        });
    }

    async copyOrder() {
        await allure.step("Copy order", async () => {
            await this.page.getByTitle("Copy").click({ force: true })
        })
    }
    async handlePopUp(option: "Yes" | "No" | "OK", message: string = "") {
        await allure.step("Handle warning popup", async () => {
            const popup = await this.page.locator('.modal-content').
                waitFor({ state: 'visible', timeout: 3000 })
                .then(() => true)
                .catch(() => false);
            if (message) {
                const alertMessage = await this.page.locator('.modal-body').innerText();
                expect(alertMessage).toContain(message);
            }
            if (popup) {
                await this.page.getByRole('button', { name: option }).click();
            }
        });
    }

    /**
     * @param deleteProject Yes or No - Delete the associated project
     * @param deleteProduct OK or Cancel - Confirm the product deletion
     */
    async deleteOrder(deleteProject: string, deleteProduct: string) {
        await allure.step("Delete order", async () => {
            await this.page.getByRole('button', { name: 'Remove' }).click()
            await this.page.locator(`//button[text()="${deleteProject}"]`).click()
            await this.page.getByRole('button', { name: `${deleteProduct}` }).click()
            await this.page.getByRole('button', { name: 'OK' }).click()
        })
    }

    async setEmployee(employeeName: string) {
        const employeeInput = this.page.locator('#typeahead-editor');
        await employeeInput.waitFor({ state: 'visible' });
        employeeInput.fill(employeeName);
        await this.page.waitForTimeout(200);
        await this.selectFromList(employeeName);
        await this.page.waitForTimeout(1000);
    }
    async setChargingType(chargingType: string) {
        await this.page.locator('//div[@class="modal-body"]//div[@col-id="timeCodeName" and @role="gridcell"]').click();
        await this.selectFromList(chargingType);
        await this.page.waitForTimeout(1000);
    }
    async setTimeWorked(time: string) {
        const ele = this.page.locator('//div[@class="modal-body"]//div[@col-id="timePayrollQuantityFormattedEdit" and @role="gridcell"]')
        await ele.click();
        await ele.locator('input').fill(time);
        await this.page.waitForTimeout(1000);
    }
    async setBillableTime(billableTime: string) {
        const ele = this.page.locator('//div[@class="modal-body"]//div[@col-id="invoiceQuantityFormatted" and @role="gridcell"]')
        await ele.click();
        await ele.locator('input').fill(billableTime);
        await this.page.waitForTimeout(1000);
    }
    async setExternalNote(externalNote: string) {
        const ele = this.page.locator('//div[@class="modal-body"]//div[@col-id="externalNote" and @role="gridcell"]')
        await ele.click();
        await ele.locator('input').fill(externalNote);
        await this.page.waitForTimeout(1000);
    }

    async setInternalNote(internalNote: string) {
        const ele = this.page.locator('//div[@class="modal-body"]//div[@col-id="internalNote" and @role="gridcell"]')
        await ele.click();
        await ele.locator('input').fill(internalNote);
        await this.page.waitForTimeout(1000);
    }


    // Temporary method to handle OK button in popup
     async clickOkButton() {
          await allure.step("Click OK button in popup if visible", async () => {
              await this.page.waitForTimeout(500);
              const popup = this.page.locator('//div[@class="modal-header"]/h6[text()="Warning"]')
              const isOkButtonVisible = await Promise.race([
                  await popup.evaluate(el => {
                      const style = window.getComputedStyle(el);
                      return (
                          style.display !== 'none' &&
                          style.visibility !== 'hidden' &&
                          style.opacity !== '0' &&
                          el.getBoundingClientRect().width > 0 &&
                          el.getBoundingClientRect().height > 0
                      );
                  }).catch(() => false),
                  await this.page.waitForTimeout(1500).then(() => false)
              ])
              if (isOkButtonVisible) {
                  const okButton = this.page.getByRole('button', { name: 'OK' });
                  await okButton.click();
              }
          });
      }

    async addWork({ externalNote = 'External note', internalNote = 'Internal note', employeeName = 'Playwright Employee', chargingType = 'Arbetad tid', timeWorked = '2', billableTime = '2' } = {}) {
        await allure.step("Add work time", async () => {
            const timeSection = this.page.locator("//*[@label-key='billing.order.times']");
            await timeSection.getByRole('button', { name: 'Add Row' }).click();
            await this.registerTimeGrid.waitForPageLoad();
            const modalContent = this.page.locator("//div[@class='modal-content']");
            await expect(modalContent.getByRole('heading', { name: 'Register time' })).toBeVisible();
            await this.page.waitForTimeout(1000);
            await this.setEmployee(employeeName);
            await this.setChargingType(chargingType);
            await this.setTimeWorked(timeWorked);
            await this.setBillableTime(billableTime);
            await this.setExternalNote(externalNote);
            await this.setInternalNote(internalNote);
            await this.page.locator('//div[@class="modal-footer"]//button[text()="Save"]').click();
            await this.clickOkButton();
            await this.waitForDataLoad('/api/Core/Project/ProjectTimeBlockSaveDTO/', 5000);
        })
    }

    async addWorkTime({ externalNote = 'External note', internalNote = 'Internal note', employeeName = 'Playwright Employee', chargingType = 'Arbetad tid', timeWorked = '2', billableTime = '2' } = {}) {
        await allure.step("Add work time", async () => {
            const timeSection = this.page.locator("//*[@label-key='billing.order.times']");
            await timeSection.getByRole('button', { name: 'Add Row' }).click();
            await this.registerTimeGrid.waitForPageLoad();
            const modalContent = this.page.locator("//div[@class='modal-content']");
            await expect(modalContent.getByRole('heading', { name: 'Register time' })).toBeVisible();
            await this.page.waitForTimeout(1000);
            await this.setEmployee(employeeName);
            await this.registerTimeGrid.enterGridValueByColumnId('externalNote', 'External note');
            await this.page.waitForTimeout(1000);
            await this.registerTimeGrid.enterGridValueByColumnId('externalNote', externalNote);
            await this.page.waitForTimeout(1000);
            await this.registerTimeGrid.enterGridValueByColumnId('employeeName', employeeName, 0, true);
            await this.page.waitForTimeout(2000);
            await this.registerTimeGrid.enterGridValueByColumnId('timePayrollQuantityFormattedEdit', timeWorked);
            await this.page.waitForTimeout(1000);
            await this.registerTimeGrid.enterGridValueByColumnId('invoiceQuantityFormatted', billableTime);
            await this.page.waitForTimeout(1000);
            await this.registerTimeGrid.enterGridValueByColumnId('timeCodeName', chargingType, 0, true);
            await this.page.waitForTimeout(1000);
            await this.registerTimeGrid.enterGridValueByColumnId('internalNote', internalNote);
            await this.page.getByRole('button', { name: 'Save', exact: true }).click();
            await this.clickOkButton();
            await this.waitForDataLoad('api/Core/Project/TimeBlock/');
        });
    }

    async updateWorkTime({ externalNote, internalNote, employeeName, chargingType, timeWorked, billableTime }: { externalNote?: string; internalNote?: string; employeeName?: string; chargingType?: string; timeWorked?: string; billableTime?: string; } = {}) {
        await allure.step("Update work time", async () => {
            await this.registerTimeGrid.waitForPageLoad();
            const modalContent = this.page.locator("//div[@class='modal-content']");
            await expect(modalContent.getByRole('heading', { name: 'Register time' })).toBeVisible();
            if (externalNote !== undefined) {
                await this.registerTimeGrid.enterGridValueByColumnId('externalNote', externalNote);
            }
            if (employeeName !== undefined) {
                await this.registerTimeGrid.enterGridValueByColumnId('employeeName', employeeName, 0, true);
            }
            if (timeWorked !== undefined) {
                await this.registerTimeGrid.enterGridValueByColumnId('timePayrollQuantityFormattedEdit', timeWorked);
            }
            if (billableTime !== undefined) {
                await this.registerTimeGrid.enterGridValueByColumnId('invoiceQuantityFormatted', billableTime);
            }
            if (chargingType !== undefined) {
                await this.registerTimeGrid.enterGridValueByColumnId('timeCodeName', chargingType, 0, true);
            }
            if (internalNote !== undefined) {
                await this.registerTimeGrid.enterGridValueByColumnId('internalNote', internalNote);
            }
            await this.page.getByRole('button', { name: 'Save', exact: true }).click();
            await this.clickOkButton();
            await this.waitForDataLoad('api/Core/Project/TimeBlock/', 30000);
        });
    }

    async getOrdersTab() {
        await allure.step("Get Orders tab", async () => {
            this.page.getByRole('link', { name: 'Orders' });
        });
    }

    async filterByInternalText(internalText: string, rowIndex: number = 0) {
        await allure.step("Filter by internal  text ", async () => {
            await this.orderGrid.filterByName('Internal Text', internalText, 15000, rowIndex);
        });
    }

    async filterByCustomerName(customerName: string, rowIndex: number = 0) {
        await allure.step("Filter by customer name", async () => {
            await this.orderGrid.filterByName('Customer Name', customerName, 15000, rowIndex);
        });
    }

    async Reload() {
        await allure.step("Reload", async () => {
            await this.page.locator("//a[contains(@class, 'fa-sync') and @title='Reload records']").click();
        });
    }

    async filterAllOrders() {
        await allure.step("Filter all orders", async () => {
            const dropdown = this.page.locator('#ctrl_allItemsSelection');
            await dropdown.waitFor({ state: 'visible' });
            await expect(dropdown).toBeEnabled();
            await dropdown.selectOption({ label: 'All' });
            await this.page.waitForTimeout(200);
            // await this.waitForDataLoad('api/Core/CustomerInvoices/');
        });
    }

    async filterByOrderNo(orderNumber: string, rowIndex: number = 0) {
        await allure.step("Filter by order number", async () => {
            await this.orderGrid.filterByName('Order No.', orderNumber, 15000, rowIndex);
        });
    }

    async editOrder(rowIndex: number = 0) {
        await allure.step("Edit order", async () => {
            const editOrderButton = this.page.locator('//div[@col-id="edit"]/button').nth(rowIndex);
            await editOrderButton.waitFor({ state: 'visible' });
            await editOrderButton.click();
            await this.waitForDataLoad('api/Core/Customer/Customer/GLN/');
            await this.page.waitForTimeout(2000);
        });
    }

    async expandDocument() {
        await allure.step("Expand document tab", async () => {
            const documentTab = this.page.locator("//div[@class='soe-accordion-heading ng-scope']//label[@class='control-label' and normalize-space(text())='Document']");
            await documentTab.waitFor({ state: 'visible' });
            await documentTab.click();
        });
    }

    async clickSelectFilesToUpload() {
        await allure.step("Click select files to upload", async () => {
            const uploadButton = this.page.locator("//button[@type='button' and contains(text(), 'Select Files to Upload')]");
            await uploadButton.waitFor({ state: 'visible' });
            await uploadButton.click();
        });
    }

    async uploadFile(filePath: string, fileName: string) {
        await allure.step(`Upload file: ${filePath}`, async () => {
            const fileInput = this.page.locator('//input[@type="file" and @uploader="ctrl.uploader" and @multiple]');
            const resolvedFilePath = path.resolve(filePath);
            await fileInput.setInputFiles(resolvedFilePath);
            await this.page.getByRole('button', { name: ' Upload', exact: true }).click();
            await this.waitForDataLoad('api/Core/Files', 30000);
            const uploadedFileName = await this.page.locator('//div[@col-id="fileName" and @role="gridcell"]').innerText()
            expect(uploadedFileName).toBe(fileName)
        });
    }

    async tickAttachWhenSendingColumn(isChecked: boolean = false) {
        await allure.step("Tick 'Attach when sending'", async () => {
            const checkbox = this.page.locator('//div[@ref="eFloatingFilterBody"]//input[@type="checkbox"]');
            await checkbox.nth(0).waitFor({ state: 'attached' });
            if (isChecked) {
                if (!(await checkbox.nth(0).isChecked())) {
                    await checkbox.nth(0).click();
                }
            } else {
                if (await checkbox.nth(0).isChecked()) {
                    await checkbox.nth(0).click();
                }
            }
        });
    }

    async tickAttachWhenSending(rowIndex: number = 0) {
        await allure.step("Tick 'Attach when sending'", async () => {
            await this.documentGrid.clickCheckBoxByColumnId('includeWhenDistributed', true, rowIndex);
        });
    }

    async tickAttachToInvoiceColumn(isChecked: boolean = false) {
        await allure.step("Tick 'Attach to invoice'", async () => {
            const checkbox = this.page.locator(`//input[@type='checkbox' and @id='directiveCtrl_distributeAll']`);
            await checkbox.waitFor({ state: 'attached' });
            if (isChecked) {
                if (!(await checkbox.isChecked())) {
                    await checkbox.click();
                }
            } else {
                if (await checkbox.isChecked()) {
                    await checkbox.click();
                }
            }
        });
    }

    async tickAttachToInvoice(rowIndex: number = 0) {
        await allure.step("Tick 'Attach to invoice'", async () => {
            await this.documentGrid.clickCheckBoxByColumnId('includeWhenTransfered', true, rowIndex);
        });
    }

    async closeOrder(index: number = 0) {
        await allure.step("Close order", async () => {
            const orderCloseButton = this.page.locator("//i[contains(@class, 'fa-times') and @title='Close']").nth(index)
            await orderCloseButton.click();
        });
    }

    /**
    * Pass the row index number of the item to transfer to final invoice. 
    * The default value is -1, which means all items will be selected and
    * they status will be transferd to final invoice.
    * @param rowIndex Row number of the item 
    */
    async transferToFinalInvoice(rowIndex: number = -1) {
        return await allure.step("Transfer to final invoice", async () => {
            if (rowIndex > -1) {
                await this.productRowsGrid.selectCheckBox(rowIndex)
            } else {
                await this.productRowsGrid.selectAllCheckBox();
            }
            const transferResponse = this.page.waitForResponse("**/Core/CustomerInvoices/Transfer/")
            await this.page.locator("//div[@class='tab-pane ng-scope active']//div[@form='ctrl.edit']//button[contains(text(),'invoice')]/following-sibling::button").click();
            const transferToFinalInvoice = this.page.locator(`//li[@data-ng-repeat='option in directiveCtrl.options']//a[contains(text(), 'Transfer to the final invoice')]`);
            await transferToFinalInvoice.waitFor({ state: 'visible' });
            await transferToFinalInvoice.click();
            await this.clickAlertMessage('OK');
            await this.clickAlertMessage('OK');
            const tranferResponseData = await (await transferResponse).json()
            const { strDict } = tranferResponseData
            const keys = Object.keys(strDict)
            return { invoiceNumber: strDict[keys[0]] }
        });
    }

    async tranferToPreliminaryMergedInvoice() {
        await allure.step("Select Transfer to Preliminary Merged Invoice", async () => {
            await this.page.locator('//button[@data-toggle="dropdown"]').click({ delay: 500 })
            await this.page.locator('//a[contains(text(),"Transfer to preliminary merged invoice")]').click()
            await this.clickAlertMessage('OK');
            await this.clickAlertMessage('OK');
        })
    }

    /**
    * Pass the row index number of the item to transfer to preliminary invoice. 
    * The default value is -1, which means all items will be selected and
    * they status will be transferd to preliminary invoice.
    * @param rowIndex Row number of the item 
    */
    async tranferToPreliminaryInvoice(rowIndex: number = -1) {
        await allure.step("Select Transfer to Preliminary Invoice", async () => {
            if (process.env.ENV === 'prod') { // This popup appears only in prod environment
                const isCreateInvoicePopupVisible = await this.page.locator('//div[@class="modal-header"]/h6').
                    waitFor({ state: 'visible', timeout: 5000 }).
                    catch(() => false).
                    then(() => true);
                if (isCreateInvoicePopupVisible) {
                    await this.page.getByRole('button', { name: 'Preliminary' }).click();
                }
            } else {
                if (rowIndex > -1) {
                    await this.productRowsGrid.selectCheckBox(rowIndex)
                } else {
                    await this.productRowsGrid.selectAllCheckBox();
                }
                await this.page.locator('//div[@selected-option="ctrl.transferButtonSelectedOption"]/button[@data-toggle="dropdown"]').click({ delay: 500 })
                await this.page.locator(`//div[@selected-option="ctrl.transferButtonSelectedOption"]/ul/li/a[contains(text(), 'Transfer to preliminary invoice')]`).click();
                await this.clickAlertMessage('OK');
            }

        })
    }

    async expandProducts() {
        await allure.step("Expand products tab", async () => {
            const productsTab = this.page.locator('[label-key="billing.order.productrows"]');
            await productsTab.scrollIntoViewIfNeeded();
            await productsTab.click({ force: true });
            await this.page.locator(`#article-rows-grid`).waitFor({ state: 'visible' });
        });
    }

    async expandProjectOrder() {
        await allure.step("Expand project order", async () => {
            const projectTab = this.page.getByRole('button', { name: 'Project order' });
            await projectTab.scrollIntoViewIfNeeded();
            await projectTab.click();
        });
    }

    async expandTimes() {
        await allure.step("Expand times", async () => {
            const timesTab = this.page.getByRole('button', { name: 'Times' });
            await timesTab.scrollIntoViewIfNeeded();
            await timesTab.click();
        });
    }

    async expandCodingRows() {
        await allure.step("Expand coding rows", async () => {
            const codingRows = this.page.getByRole('button', { name: 'Coding Rows' });
            await codingRows.scrollIntoViewIfNeeded();
            await codingRows.click();
            await this.page.locator('//div[@id="accounting-sum-footer-grid"]').isVisible({ timeout: 2000 })
        })
    }

    async expandChecklist() {
        await allure.step("Expand checklist", async () => {
            const checklist = this.page.getByRole('button', { name: 'Checklists' });
            await checklist.scrollIntoViewIfNeeded();
            await checklist.click();
            await this.page.locator('#directiveCtrl_selectedChecklistHeadId').isVisible({ timeout: 2000 })
        })
    }

    /**
     * Select the given checklist from the dropdown
     * @param checklist Created checklist
     */
    async addChecklist(checklist: string) {
        await allure.step(`Add selected checklist ${checklist}`, async () => {
            const checklistSelector = this.page.locator('#directiveCtrl_selectedChecklistHeadId')
            await checklistSelector.click()
            await checklistSelector.selectOption({ label: `${checklist}` })
            await this.page.locator('//button[@title="Add"]').click()
            await this.page.locator('//soe-accordion[@label-value="head.checklistHeadName"]//span[contains(@class,"toggle")]').click()
        })
    }

    private async getQuestionRowId(question: string) {
        const checklistRows = await this.page.locator(`//soe-accordion//table//tr/td[2]/div`).all()
        for (let i = 0; i < checklistRows.length; i++) {
            const q = await checklistRows[i].innerText()
            if (question === q) {
                return i + 1
            }
        }
    }

    async answerChecklistYesNO(question: string, answer: string) {
        await allure.step(`Answer yes no question ${answer}`, async () => {
            const index = await this.getQuestionRowId(question)
            const rootLocator = `//soe-accordion//tbody[${index}]//td[4]`
            const dateLocator = `//soe-accordion//tbody[${index}]//td[6]`
            await this.page.locator(rootLocator).locator('select').selectOption({ label: `${answer}` })
            const date = await this.page.locator(dateLocator).locator(`input`).inputValue()
            const checklistQuestion = await this.page.locator(`//soe-accordion//tbody[${index}]//td[2]`).locator(`div`).innerText()
            expect(checklistQuestion).toBe(question)
            expect(date.trim()).toBe(getFormattedDateMMDDYY().trim())
        })
    }

    async answerChecklistCheckbox(question: string, answer: boolean) {
        await allure.step(`Answer checkbox question ${answer}`, async () => {
            const index = await this.getQuestionRowId(question)
            const rootLocator = this.page.locator(`//soe-accordion//tbody[${index}]//td[4]`)
            const dateLocator = this.page.locator(`//soe-accordion//tbody[${index}]//td[6]`)
            if (answer) {
                await rootLocator.locator(`input`).check()
                const date = await dateLocator.locator(`input`).inputValue()
                const checklistQuestion = await this.page.locator(`//soe-accordion//tbody[${index}]//td[2]`).locator(`div`).innerText()
                expect(checklistQuestion).toBe(question)
                expect(date.trim()).toBe(getFormattedDateMMDDYY().trim())
            }
        })
    }

    async answerChecklistFreeText(question: string, answer: string) {
        await allure.step(`Answer checkbox question ${answer}`, async () => {
            const index = await this.getQuestionRowId(question)
            const rootLocator = `//soe-accordion//tbody[${index}]//td[5]`
            if (answer) {
                await this.page.locator(rootLocator).locator(`input`).fill(answer)
                const checklistQuestion = await this.page.locator(`//soe-accordion//tbody[${index}]//td[2]`).locator(`div`).innerText()
                expect(checklistQuestion).toBe(question)
            }
        })
    }

    async answerChecklistUploadImage(question: string) {
        await allure.step(`Upload an image`, async () => {
            await this.page.locator(`//soe-accordion[@label-key="common.checklists"]`).click({ delay: 300 })
            await this.page.locator('//soe-accordion[@label-value="head.checklistHeadName"]//span[contains(@class,"toggle")]').click({ force: true })
            const index = await this.getQuestionRowId(question)
            const rootLocator = this.page.locator(`//soe-accordion//tbody[${index}]//td[7]`)
            await rootLocator.locator(`span`).click()
            await this.clickSelectFilesToUpload()
            await this.uploadFile(`test-data/construction_bill.png`, `construction_bill.png`);
            await this.page.getByRole('button', { name: 'Close', exact: true }).click({ delay: 2000 });
        })
    }

    async selectAllProducts() {
        await allure.step("Select all products", async () => {
            await this.productRowsGrid.selectAllCheckBox();
        });
    }

    async selectAllProductsOrderRowsGrid() {
        await allure.step("Select all products from order grid", async () => {
            await this.orderGrid.selectAllCheckBox()
        })
    }

    async selectProductRow(rowIndex: number = 0) {
        await allure.step("Select product row", async () => {
            await this.productRowsGrid.selectCheckBox(rowIndex);
        });
    }

    async unselectProductRow(rowIndex: number = 0) {
        await allure.step("Select product row", async () => {
            await this.productRowsGrid.unselectCheckBox(rowIndex);
        });
    }

    /**
     * Pass the row index number of the item to change its status to Klar. 
     * The default value is -1, which means all items will be selected and their status will be changed to Klar.
     * @param rowIndex Row number of the item 
     */
    async addToKlar(rowIndex: number = -1) {
        await allure.step("Add to Klar", async () => {
            if (rowIndex > -1) {
                await this.productRowsGrid.selectCheckBox(rowIndex)
            } else {
                await this.productRowsGrid.selectAllCheckBox();
            }
            await this.page.selectOption('#directiveCtrl_selectedAttestState', 'Klar');
            await this.page.locator("//button[@title='Change row status']").click();
            await this.waitForDataLoad('/api/Billing/Order/');
            if (process.env.ENV !== 'prod') { // This popup appears only in prod environment
                const productsTab = this.page.locator('[label-key="billing.order.productrows"]');
                await productsTab.waitFor({ state: 'visible' });
                await productsTab.click();
            }
        });
    }

    async reloadOrders() {
        await allure.step("Reload orders", async () => {
            await this.page.locator(`//a[@data-ng-repeat='button in group.buttons' and @title='Reload records']`).click();
            await this.waitForDataLoad('/api/Core/CustomerInvoices/');
        });
    }

    async addColumnToProductRows(columnName: string) {
        await allure.step("Add column to product rows", async () => {
            await this.productRowsGrid.setGridColumnHeaders(columnName);
        });
    }

    async changeSalesPrice(price: string, addingPrice: number, rowIndex: number) {
        await allure.step("Change sales price", async () => {
            let normalizedSalesPrice = price.replace(/[^\d,]+/g, '').trim();
            let normalizedPrice = normalizedSalesPrice.replace(',', '.');
            let new_price: string = Math.round((parseFloat(normalizedPrice.replace(/\s/g, '')) + addingPrice)).toString();
            await this.productRowsGrid.enterGridValueByColumnId('amountCurrency', new_price, rowIndex);
        });
    }

    async getValueFromProductRows(columnId: string, rowIndex: number) {
        return await allure.step("Get value from product row", async () => {
            return await this.productRowsGrid.getCellvalueByColIdandGrid(columnId, rowIndex);
        });
    }

    async getOrderDetails(expectedValue: string, colId: string, row: number = 0) {
        await allure.step(`Get values from product row ${colId}`, async () => {
            const value = await this.orderGrid.getCellvalueByColIdandGrid(colId, row)
            expect(value, { message: `Order value is not equal to ${expectedValue}` }).toEqual(expectedValue)
        })
    }

    async clickOnFunctions() {
        await allure.step("Click on Functions", async () => {
            const productRows = this.page.locator('[label-key="billing.order.productrows"]');
            await productRows.waitFor({ state: 'visible' });
            const functions = this.page.locator("//div[@form='ctrl.edit']//button[contains(normalize-space(.), 'Functions')]");
            await functions.waitFor({ state: 'visible' });
            await functions.click();
        });
    }

    async deleteSelectedRow() {
        await allure.step("Delete selected row", async () => {
            const deleteOption = this.page.getByRole('link', { name: 'Delete selected rows' });
            await deleteOption.waitFor({ state: 'visible' });
            await deleteOption.click();
        });
    }

    async selectRecalculatePrice() {
        await allure.step("Select Recalculate Price", async () => {
            const recalculatePricesOption = this.page.locator("//a[contains(., 'Recalculate prices on selected rows')]");
            await recalculatePricesOption.waitFor({ state: 'visible' });
            await recalculatePricesOption.click();
            await this.page.waitForTimeout(1000);
            await this.clickAlertMessage('Yes');
            await this.waitForDataLoad('/api/Billing/Product/Prices/Collection/');
            await this.page.waitForTimeout(1000);
            await this.clickAlertMessage('OK');
        });
    }

    async getProductValuesFromRows(rowIndex: number) {
        return await allure.step("Verify item details", async () => {
            const itemQuantity = await this.productRowsGrid.getCellvalueByColIdandGrid('quantity', rowIndex)
            const itemNumber = await this.productRowsGrid.getCellvalueByColIdandGrid('productNr', rowIndex)
            const itemPrice = await this.productRowsGrid.getCellvalueByColIdandGrid('amountCurrency', rowIndex)
            const itemTitle = await this.productRowsGrid.getCellvalueByColIdandGrid('text', rowIndex)
            return {
                itemNumber,
                itemPrice,
                itemQuantity,
                itemTitle
            }
        })
    }

    private async selectFromList(item: string) {
        await expect(this.page.locator('//ul[@class="typeahead dropdown-menu"]')).toBeVisible({ timeout: 5000 });
        const fprducts = await this.page.locator('//ul[@class="typeahead dropdown-menu"]/li/a').all()
        for (let prd of fprducts) {
            const name = await prd.innerText()
            if (name.includes(item)) {
                await prd.click()
                return;
            }
        }
    }

    async addFixedPriceProduct(productName: string, quantity: string = '0', productPrice: string = '0', rowIndex: number = 0) {
        await this.selectFromList(productName)
        await this.page.waitForTimeout(2000);
        await this.setProductQuantity(quantity, rowIndex);
        await this.page.waitForTimeout(2000);
        await this.setProductPrice(productPrice, rowIndex);
    }

    async setProductQuantity(quantity: string, rowIndex: number = 0) {
        await allure.step("Set product quantity", async () => {
            const quantityLocator = `//div[@id="article-rows-grid"]//div[@row-index="${rowIndex}"]//div[@col-id="quantity" and @role="gridcell"]`
            const quantityElement = this.page.locator(quantityLocator).first()
            await quantityElement.click({ force: true });
            const input = quantityElement.locator('.ag-cell-edit-input')
            await input.waitFor({ state: 'visible' });
            await input.clear()
            await input.fill(quantity)
        });
    }

    async setProductPrice(price: string, rowIndex: number = 0) {
        await allure.step("Set product price", async () => {
            const amountCurrencyLocator = `//div[@id="article-rows-grid"]//div[@row-index="${rowIndex}"]//div[@col-id="amountCurrency" and @role="gridcell"]`
            const tooalPriceLocator = `//div[@id="article-rows-grid"]//div[@row-index="${rowIndex}"]//div[@col-id="sumAmountCurrency" and @role="gridcell"]`
            const totalPriceElement = this.page.locator(tooalPriceLocator)
            const priceElement = this.page.locator(amountCurrencyLocator)
            await priceElement.click({ force: true });
            const input = priceElement.locator('.ag-cell-edit-input')
            await input.waitFor({ state: 'visible' });
            await input.clear()
            await input.fill(price)
            await totalPriceElement.click() // To close the input field
        })
    }

    async addNewProduct(productName: string, productPrice: string, quantity: string = '1', rowIndex: number = 0, deleteEmptyRow: boolean = false) {
        await allure.step(`Add new product: ${productName} with price: ${productPrice}`, async () => {
            await this.clickButtonToggle('billing.order.productrows');
            await this.page.getByRole('link', { name: 'New product row' }).click();
            await this.selectFromList(productName)
            await this.page.waitForTimeout(2000);
            await this.setProductQuantity(quantity, rowIndex);
            await this.page.waitForTimeout(2000);
            await this.setProductPrice(productPrice, rowIndex);
            await this.page.waitForTimeout(2000);
            if (deleteEmptyRow) {
                await this.productRowsGrid.clickButtonByColumnId('delete', rowIndex);
            }
        });
    }

    async newProductRow(productName: string = '', productPrice: string = '0', quantity: string = '0', title: string = "", rowIndex: number = 0, deleteEmptyRow: boolean = true) {
        await allure.step("Add new product row", async () => {
            await this.clickButtonToggle('billing.order.productrows');
            await this.page.getByRole('link', { name: 'New product row' }).click();
            await this.productRowsGrid.enterGridValueByColumnId('quantity', quantity, rowIndex);
            if (productName !== '') {
                await this.productRowsGrid.enterGridValueByColumnId('productNr', productName, rowIndex, true);
            };
            await this.page.waitForTimeout(2000);
            await this.productRowsGrid.enterGridValueByColumnId('amountCurrency', productPrice, rowIndex);
            if (title !== '') {
                await this.productRowsGrid.enterGridValueByColumnId('text', title, rowIndex);
            }
            if (deleteEmptyRow) {
                await this.productRowsGrid.clickButtonByColumnId('delete', rowIndex + 1);
            }
        });
    }

    async addNewLineOfText(text: string, rowIndex: number = 0, deleteEmptyRow: boolean = true) {
        await allure.step("Add new line of text", async () => {
            await this.clickButtonToggle('billing.order.productrows');
            await this.page.getByRole('link', { name: 'New line of text' }).click();
            await this.productRowsGrid.enterGridValueByColumnId('soe-ag-single-value-column', text, rowIndex);
            if (deleteEmptyRow) {
                await this.productRowsGrid.clickButtonByColumnId('delete', rowIndex + 1);
            }
        });
    }

    async addPageBreak() {
        await allure.step("Add new page break", async () => {
            await this.clickButtonToggle('billing.order.productrows');
            await this.page.getByRole('link', { name: 'Page break' }).click();
        });
    }

    async addSubTotal() {
        await allure.step("Add new subtotal", async () => {
            await this.clickButtonToggle('billing.order.productrows');
            await this.page.getByRole('link', { name: 'Subtotal' }).click();
        });
    }

    private async clickButtonToggle(labelKeySection: string) {
        await allure.step("Click button toggle", async () => {
            const buttonToggle = this.page.locator(`//*[@label-key='${labelKeySection}']//div[@form='ctrl.edit']//button[contains(@class, 'dropdown-toggle')]`);
            await buttonToggle.nth(0).scrollIntoViewIfNeeded();
            await buttonToggle.nth(0).waitFor({ state: 'visible' });
            await buttonToggle.nth(0).click();
        });
    }

    async selectTextLine(rowIndex: number) {
        await allure.step("Select text line", async () => {
            await this.productRowsGrid.selectCheckBox(rowIndex);
        });
    }

    async expandPlanning() {
        await allure.step("Expand planning tab", async () => {
            const planningTab = this.page.locator("//div[contains(@class, 'soe-accordion-heading')]//label[contains(text(), 'Planning')]");
            await planningTab.waitFor({ state: 'visible' });
            await planningTab.click();
        });
    }

    async selectAssignmentType() {
        await allure.step("Select assignment type", async () => {
            const assignmentType = this.page.locator("#ctrl_selectedShiftType");
            await assignmentType.waitFor({ state: 'visible' });
            await assignmentType.click();
            const dropdown = await this.page.locator("//ul[contains(@class, 'dropdown-menu')]//li[contains(@class, 'uib-typeahead-match')]//a[normalize-space(text())='SeleniumOrderType']");
            await dropdown.waitFor({ state: 'visible' });
            await dropdown.click();
        });
    }

    async planStartDate(date: string) {
        await allure.step("Plan start date", async () => {
            const startDate = this.page.locator("#ctrl_invoice_plannedStartDate");
            await startDate.waitFor({ state: 'visible' });
            await startDate.fill(date);
            await startDate.click();
        });
    }

    async waitForOrderLoading() {
        await allure.step("Wait for order loading", async () => {
            await this.waitForDataLoad('/api/Core/CustomerInvoices/');
        });
    }

    async verrifyOrderNumber(orderNumner: string) {
        await allure.step(`Verify order number: ${orderNumner}`, async () => {
            expect(this.page.getByRole('button', { name: 'Service order ' + orderNumner })).toBeTruthy();
        });
    }

    async verifySalesPrice(price: string, addedPrice: number, rowIndex: number) {
        await allure.step("Verify sales price", async () => {
            let salesPrice = await this.getValueFromProductRows('amountCurrency', rowIndex);
            let normalizedSalesPrice = salesPrice.replace(/[^\d,]+/g, '').trim();
            let normalizedPrice = price.replace(',', '.');
            let expectedPrice = (parseFloat(normalizedPrice.replace(/\s/g, '')) + addedPrice).toFixed(2);
            expectedPrice = expectedPrice.replace(/\s+/g, '').replace('.', ',');
            expect(normalizedSalesPrice, `Sales Price for row ${rowIndex} is not correct`).toBe(expectedPrice);
        });
    }

    async verifyProductSubTotal(total: string, rowIndex: number = 0) {
        await allure.step("Verify product sub total", async () => {
            const productTotal = await this.getValueFromProductRows('soe-ag-single-value-column', rowIndex);
            const [label, amount] = productTotal.split(/\r?\n+/).map(t => t.trim());
            const amountTotal = amount.replace(/[^\d,]+/g, ' ').trim();
            expect(label, `Expected product title to be 'Subtotal', but found '${label}'`).toBe('Subtotal');
            expect(amountTotal, `Expected product sub total to be '${total}', but found '${amountTotal}'`).toBe(total);
            await this.productRowsGrid.checkRowIconExist('fa-calculator-alt', rowIndex);
        });
    }

    async verifyProductPageBreak(rowIndex: number = 0) {
        await allure.step("Verify product page break", async () => {
            const pageBreakText = await this.getValueFromProductRows('soe-ag-single-value-column', rowIndex);
            expect(pageBreakText, `Expected product title to be 'Page break', but found '${pageBreakText}'`).toBe('Page break');
            await this.productRowsGrid.checkRowIconExist('fa-cut', rowIndex);
        });
    }

    async verifyPurchasePriceAndSalesPriceDifference() {
        await allure.step("Verify all items have purchase price", async () => {
            const priceColumn_1 = await this.productRowsGrid.getCellvalueByColIdandGrid('amountCurrency', 0);
            const priceColumn_2 = await this.productRowsGrid.getCellvalueByColIdandGrid('amountCurrency', 1);
            expect(priceColumn_1, `Price column 1 is not present ${priceColumn_1}`).toBeTruthy();
            expect(priceColumn_2, `Price column 2 is not present ${priceColumn_2}`).toBeTruthy();
            const purchasePriceColumn_1 = await this.productRowsGrid.getCellvalueByColIdandGrid('purchasePriceCurrency', 0);
            const purchasePriceColumn_2 = await this.productRowsGrid.getCellvalueByColIdandGrid('purchasePriceCurrency', 1);
            expect(purchasePriceColumn_1, `Purchase Price column 1 is not present ${purchasePriceColumn_1}`).toBeTruthy();
            expect(purchasePriceColumn_2, `Purchase Price column 2 is not present ${purchasePriceColumn_2}`).toBeTruthy();
            // Assume this is not same for according to test case
            expect(priceColumn_1, `Price column 1 is not equal to Purchase Price column 1`).not.toBe(purchasePriceColumn_1);
            expect(priceColumn_2, `Price column 2 is not equal to Purchase Price column 2`).not.toBe(purchasePriceColumn_2);
        });
    }

    async verifyRecalculatePriceDisabled() {
        await allure.step("Verify Recalculate Price button is disabled", async () => {
            const recalculatePricesOption = this.page.locator("//a[contains(., 'Recalculate prices on selected rows')]");
            await recalculatePricesOption.waitFor({ state: 'visible' });
            const isDisabled = await recalculatePricesOption.evaluate(el => el.classList.contains('disabled-link'));
            expect(isDisabled).toBeTruthy();
        });
    }

    /**
     * Verify the number of orders returned based on the filter.
     * @param rowCount Exptected order count
     */
    async verifyOrder(rowCount: number = 1) {
        await allure.step("Verify row count ", async () => {
            const count = await this.orderGrid.getFilteredAgGridRowCount();
            expect(count, `Expected ${rowCount} row, but found ${count} rows.`).toBe(rowCount);
        });
    }

    async verifyStatus(status: string) {
        await allure.step("Verify status documentation ", async () => {
            const statusName = await this.orderGrid.getCellvalueByColIdandGrid('statusName');
            expect(statusName, `Expected Documentation row, but found ${statusName} rows.`).toBe(status);
        });
    }

    async verifyTimeRow(quantity: string, rowIndex: number = 0) {
        await allure.step("Verify time rows", async () => {
            await this.productRowsGrid.checkRowIconExist('fa-clock', rowIndex);
            const quantityText = await this.getValueFromProductRows('quantity', rowIndex);
            expect(quantityText).toBe(quantity);
        });
    }

    async verifyTimeRowCount(count: number = 0) {
        await allure.step("Verify time rows", async () => {
            await this.productRowsGrid.checkRowIconCount('fa-clock', count);
        });
    }

    async verifyTextRow(rowText: string, rowIndex: number = 0) {
        await allure.step("Verify text rows", async () => {
            const rowText = await this.getValueFromProductRows('soe-ag-single-value-column', rowIndex);
            expect(rowText, `Expected product title to be 'Page break', but found '${rowText}'`).toBe(rowText);
            await this.productRowsGrid.checkRowIconExist('fa-text', rowIndex);
        });
    }

    async verifyProductSumAmount(amount: string) {
        await allure.step("Verify sum amount", async () => {
            const sumAmount = await this.page.locator("//div[@id='article-sum-footer-grid']//div[@col-id='sumAmountCurrency']").nth(1).innerText();
            const normalizedSumAmount = sumAmount.replace(/[^\d,]+/g, '').trim();
            const normalizedAmount = amount.replace(',', '.');
            const expectedAmount = (parseFloat(normalizedAmount)).toFixed(2).replace(/\s+/g, '').replace('.', ',');
            expect(normalizedSumAmount, `Sum Amount is not correct`).toBe(expectedAmount);
        });
    }

    /**
     * Verify the number of product items added to an order
     * @param rowCount - The number of expected product items
     */
    async verifyProductRowCount(rowCount: number = 1) {
        await allure.step("Verify product row count", async () => {
            const actualRowCount = this.page.locator('//div[@id="article-rows-grid"]//div[@name="center"]//div[@comp-id and @role="row"]');
            await expect.poll(async () => await actualRowCount.count(), { timeout: 10000, message: `Expected ${rowCount} product rows, but found a different count.` },).toBe(rowCount);
        });
    }

    /**
     * Verify the number of ledger entries of an order
     * @param rowCount - The number of expected ledger entries
     */
    async verifyCodingRowCount(rowCount: number) {
        await allure.step(`Verify the affected number of accounts`, async () => {
            const loc = this.page.locator('#directiveCtrl_voucherSeriesId')
            await expect(loc).toBeVisible()
            const actualRowCount = await this.codingRowGrid.getAgGridRowCount()
            expect(actualRowCount, `Expected ${rowCount} is not equals to ${actualRowCount}`).toBe(rowCount)
        })
    }

    async verifyAccount(accountId: string, colId: string, rowId: number) {
        await allure.step(`Verify account details ${accountId}`, async () => {
            const account = await this.codingRowGrid.getCellvalueByColIdandGrid(colId, rowId)
            expect(account).toContain(accountId)
        })
    }

    async verifyAccountBalance(balance: string, colId: string, rowId: number) {
        await allure.step(`Verify account balance ${colId}`, async () => {
            const account = await this.codingRowGrid.getCellvalueByColIdandGrid(colId, rowId)
            expect(account).toContain(balance)
        })
    }

    async verifyInternalText(internalText: string) {
        await allure.step(`Verify internal text: ${internalText}`, async () => {
            const internalTextValue = await this.page.locator("#ctrl_invoice_originDescription").inputValue();
            expect(internalTextValue).toBe(internalText);
        });
    }

    async verifyAccountProject(projectName: string) {
        await allure.step(`Verify account project: ${projectName}`, async () => {
            const labels = this.page.locator("//div[@model='ctrl.selectedAccount3']//label");
            const count = await labels.count();
            let found = false;
            for (let i = 0; i < count; i++) {
                let text: string = await labels.nth(i).textContent() ?? '';
                text = text.trim();
                if (text && text === projectName) {
                    found = true;
                    break;
                }
            }
            expect(found).toBeTruthy();
        });
    }

    async createItem() {
        await allure.step("Create Item", async () => {
            await this.page.waitForLoadState('networkidle');
            const newItemButton = this.page.locator(`//ul/li[@index!="tab.index"]`).nth(0)
            const tabs = this.page.locator(`//li[@index="tab.index"]`)
            const initTabCount = await tabs.count();
            await newItemButton.click({ force: true });
            const finalTabCount = await tabs.count();
            expect(finalTabCount, 'New tab was not created').toBe(initTabCount + 1);
        })
    }

    async selectCustomerByNumber(customerNumber: string, customerName: string) {
        await allure.step(`Select customer by number: ${customerNumber}`, async () => {
            await this.waitForDataLoad('/api/Core/Customer/Customer/?onlyActive=true');
            const customerField = this.page.locator('#ctrl_selectedCustomer');
            await customerField.fill(customerNumber);
            const customerOption = this.page.getByRole('link', { name: new RegExp(customerName, 'i') });
            await expect(customerOption).toBeVisible({ timeout: 10_000 });
            await customerOption.click();
        });
    }

    async clickNewProductRow() {
        await allure.step("Click 'New Product Row' button", async () => {
            const newProductBtn = this.page.getByRole('button', { name: /New product row/i });
            await expect(newProductBtn).toBeVisible({ timeout: 60000 });
            await newProductBtn.click();
        });
    }

    async addProductForCustomer(productName: string) {
        await allure.step(`Add product "${productName}" under this customer`, async () => {
            await this.page.waitForSelector('[role="option"]', { state: 'visible', timeout: 60000 });
            const productOption = this.page.getByRole('option', { name: new RegExp(productName, 'i') });
            await expect(productOption).toBeVisible({ timeout: 60000 });
            await productOption.click();
        });
    }

    async verifyTaxROTDetails(propertyType: string, SSN: string, propertyName: string, tax: string) {
        await allure.step("Wait for confirmation message popup", async () => {
            await this.page.locator('.modal-content').waitFor({ state: 'visible' });
            await this.waitForDataLoad("**/Billing/Invoice/HouseholdTaxDeduction/Customer/**/**/**");
            const [property, socialSecNr, name, taxDeduction] = await Promise.all([
                this.page.locator("#ctrl_row_householdProperty").inputValue(),
                this.page.locator("#ctrl_row_householdSocialSecNbr").inputValue(),
                this.page.locator("#ctrl_row_householdName").inputValue(),
                this.page.locator("#ctrl_row_householdAmountCurrency").inputValue()
            ]);
            await this.clickAlertMessage('OK');
            expect(property).toBe(propertyType);
            expect(socialSecNr).toBe(SSN);
            expect(name).toBe(propertyName);
            expect(taxDeduction).toBe(tax);
        });
    }

    async addQuantity(quantity: string) {
        await allure.step(`Add quantity ${quantity}`, async () => {
            const quantityCell = this.page.locator('xpath=//*[@id="article-rows-grid"]/div/div[2]/div[2]/div[3]/div[2]/div/div/div/div[4]');
            await quantityCell.scrollIntoViewIfNeeded();
            await quantityCell.waitFor({ state: 'visible' });
            await quantityCell.click({ clickCount: 2, delay: 200 });
            const input = quantityCell.locator('xpath=.//input');
            await input.waitFor({ state: 'visible' });
            await input.fill(quantity);
            await this.page.keyboard.press('Enter');
        });
    }

    async verifyProductPrice(row: { expectedPrice: string }[]) {
        await allure.step(`Verify product price `, async () => {
            await this.page.waitForTimeout(2_000);
            await this.page.keyboard.press('Tab');
            let price = await this.productRowsGrid.getRowColumnValue('amountCurrency', 0);
            console.log('Price:', price);
            expect((price ?? '').replace(/\s/g, '')).toBe(row[0].expectedPrice.replace(/\s/g, ''));
        });
    }

    async showClosedOrders() {
        await allure.step("Show closed orders", async () => {
            const responsePromise = this.page.waitForResponse('**/Core/CustomerInvoices/');
            await this.page.locator('#ctrl_loadClosed').click()
            const response = await responsePromise
            return response.json()
        })
    }

    async updateQuantity(quantity: string, rowIndex: number) {
        await allure.step(`Update quantity to ${quantity}`, async () => {
            await this.page.waitForTimeout(1_000);
            await this.productRowsGrid.enterGridValueByColumnId('quantity', quantity, rowIndex);
        });
    }

    async updatePrice(price: string, rowIndex: number) {
        await allure.step(`Update price to ${price}`, async () => {
            await this.productRowsGrid.enterGridValueByColumnId('amountCurrency', price, rowIndex);
        });
    }

    async updatePurchasePrice(purchasePrice: string, rowIndex: number) {
        await allure.step(`Update purchase price to ${purchasePrice}`, async () => {
            await this.productRowsGrid.enterGridValueByColumnId('purchasePriceCurrency', purchasePrice, rowIndex);
        });
    }

    async getProjectNumber(): Promise<string> {
        return await allure.step("Get created project number", async () => {
            const projectNumberField = this.page.getByRole('textbox', { name: 'Project no.' });
            await projectNumberField.waitFor({ state: 'visible', timeout: 10000 });
            await this.page.waitForFunction((el) => (el as HTMLInputElement).value && (el as HTMLInputElement).value.trim() !== "", await projectNumberField.elementHandle(), { timeout: 10000 });
            const projectNumber = await projectNumberField.inputValue();
            console.log("Captured Project Number:", projectNumber);
            return projectNumber;
        });
    }

    async verifyBalanceLeftToInvoice(balance: string) {
        await allure.step("Verify available balance for invoice", async () => {
            const value = await this.page.locator("#directiveCtrl_remainingAmountExVat").inputValue()
            expect(value).toBe(balance)
        })
    }

    async verifyVATexcludedTotalValue(totalVatExcludeValue: string) {
        await allure.step("Verify VAT excluded total value", async () => {
            const total = await this.page.locator("#directiveCtrl_sumAmountCurrency").inputValue()
            expect(total, `Expected total VAT exclude amount ${totalVatExcludeValue} is not equal to ${total}`).toBe(totalVatExcludeValue)
        })
    }


    async verifyVATexcludedLeftValue(totalVatExcludeValue: string) {
        await allure.step("Verify VAT excluded left value", async () => {
            const total = await this.page.locator("#directiveCtrl_remainingAmountExVat").inputValue()
            expect(total, `Expected left VAT exclude amount ${totalVatExcludeValue} is not equal to ${total}`).toBe(totalVatExcludeValue)
        })
    }

    async getOrderNumber(): Promise<string> {
        return await allure.step("Get created order number", async () => {
            const orderNumberField = this.page.locator('//input[@id="ctrl_invoice_invoiceNr"]');
            await orderNumberField.waitFor({ state: 'visible', timeout: 10000 });
            await this.page.waitForFunction((el) => (el as HTMLInputElement).value && (el as HTMLInputElement).value.trim() !== "", await orderNumberField.elementHandle(), { timeout: 10000 });
            const orderNumber = await orderNumberField.inputValue();
            console.log('Captured Order Number:', orderNumber);
            return orderNumber;
        });
    }

    async verifyToInvoiceGreaterThanZero() {
        await allure.step("Verify 'To invoice' is greater than zero", async () => {
            const value = await this.page.getByRole('textbox', { name: 'Left to invoice excl. VAT' }).inputValue()
            expect(parseFloat(value.replace(',', '.'))).toBeGreaterThan(0)
        });
    }

    async verifyTotalHoursInTimesSection(totalHours: string) {
        await allure.step("Verify total hours in times section", async () => {
            const value = await this.page.getByRole('textbox', { name: 'Total hours worked' }).inputValue();
            expect(value).toBe(totalHours);
        });
    }

    async clickEditButton() {
        await allure.step("Click Edit button", async () => {
            await this.timesRowGrid.clickButtonByColumnId('edit', 0);
            await this.page.waitForTimeout(3000);
        });
    }

    async deleteTimeEntryInEditMode() {
        await allure.step("Delete time entry in edit mode", async () => {
            await this.registerTimeGrid.clickButtonByColumnId('delete', 0);
            await this.page.getByRole('button', { name: 'Save' }).click();
            await this.clickOkButton();
            await this.waitForDataLoad('/api/Core/Project/ProjectTimeBlockSaveDTO/', 30000);
            const timeRows = this.page.locator('//div[@id="time-block-rows-grid"]//div[@name="center"]//div[@comp-id and @role="row"]');
            await expect.poll(() => timeRows.count(), { timeout: 20000 }).toBe(0);
        });
    }

    async verifyRowCountTimeGrid(actualRowCount: number = 1) {
        await allure.step("Verify row count in times grid", async () => {
            const rowCount = await this.timesRowGrid.getAgGridRowCount();
            expect(rowCount).toBe(actualRowCount);
        });
    }

    async externalProductSearch(productName: string) {
        await allure.step("Click external product search", async () => {
            const searchButton = this.page.locator('//div[@name="root"]//button');
            await searchButton.waitFor({ state: 'visible' });
            await searchButton.click();
            await expect(this.page.locator('#ctrl_searchText')).toBeVisible({ timeout: 10000 });
            await this.page.locator('#ctrl_searchText').fill(productName);
            await this.page.getByRole('button', { name: 'Search' }).click();
            const selectedProduct = this.page.locator(`//div[@ag-grid="ctrl.soeGridOptions.gridOptions"]//div[@col-id="number" and @role="gridcell"]`);
            await expect.poll(() => selectedProduct.count(), { timeout: 10000 }).toBe(1);
            const priceRow = this.page.locator(`//div[@price-list-type-id="ctrl.priceListTypeId"]//div[@name="center"]//div[@role="row"]`);
            await expect.poll(() => priceRow.count(), { timeout: 10000 }).toBeGreaterThanOrEqual(1);
            await this.page.locator('//div[@class="modal-footer"]/button').nth(1).click();
            await this.waitForDataLoad('/api/Billing/Product/Accounts/');
        });
    }
    async rightClickSelectedProductRow() {
        await allure.step("Right click selected product row", async () => {
            const selectedCheckbox = this.page.getByRole('checkbox', { name: 'Press Space to toggle row selection (checked)' });
            await selectedCheckbox.waitFor({ state: 'visible' });
            await selectedCheckbox.click({ button: 'right' });
        });
    }

    async clickShowLinkedTimeRows() {
        await allure.step("Click Show linked time rows", async () => {
            const showLinkedTimeRows = this.page.getByRole('link', { name: 'Show linked time rows' });
            await showLinkedTimeRows.waitFor({ state: 'visible' });
            await showLinkedTimeRows.click();
            await this.page.waitForTimeout(1000);
        });
    }

    async verifyDialogPopupVisible() {
        await allure.step("Verify dialog popup is visible", async () => {
            await this.page.waitForTimeout(1000);
            const dialog = this.page.getByRole('dialog');
            await expect(dialog).toBeVisible();
        });
    }

    async verifyWorkingTimeInPopupGrid(row: { expectedTime: string }[], rowIndex: number) {
        await allure.step("Verify working time in popup grid", async () => {
            await this.page.waitForTimeout(2000);
            const time = await this.showLinkedTimeRowsGrid.getRowColumnValue('timePayrollQuantityFormatted', rowIndex);
            console.log('Worked Time:', time);
            expect(time).toBe(row[0].expectedTime);
        });
    }

    async verifyInvoicedTimeInPopupGrid(row: { expectedTime: string }[], rowIndex: number) {
        await allure.step("Verify invoiced time in popup grid", async () => {
            await this.page.waitForTimeout(2000);
            const time = await this.showLinkedTimeRowsGrid.getRowColumnValue('invoiceQuantityFormatted', rowIndex);
            console.log('Invoiced Time:', time);
            expect(time).toBe(row[0].expectedTime);
        });
    }

    async selectTimeRowInPopupGrid(rowIndex: number = 0) {
        await allure.step("Select product row", async () => {
            await this.showLinkedTimeRowsGrid.selectCheckBox(rowIndex);
        });
    }

    async selectRowToMoveToPopupGrid(rowIndex: number = 0) {
        await allure.step("Select product row", async () => {
            await this.selectRowMoveGrid.selectCheckBox(rowIndex);
        });
    }

    async clickMoveTimeRowsToNewButton() {
        await allure.step("Click Move time rows to new product row button", async () => {
            const splitButton = this.page.locator('div.btn-group').filter({ has: this.page.locator('ul.dropdown-menu a:has-text("Move time rows to new"):not(.disabled-link)') });
            await splitButton.locator('button.dropdown-toggle').click();
            await splitButton.locator('a:has-text("Move time rows to new")').click();
            await this.waitForDataLoad('/api/Core/Project/TimeBlock/InvoiceRow/');
        });
    }

    async clickMoveTimeRowsToExistingButton() {
        await allure.step("Click Move time rows to existing product row button", async () => {
            const moveExistingSplitButton = this.page.locator('div.btn-group').filter({ has: this.page.locator('ul.dropdown-menu a:has-text("Move time rows to existing"):not(.disabled-link)') });
            await moveExistingSplitButton.locator('button.dropdown-toggle').click();
            await moveExistingSplitButton.locator('a:has-text("Move time rows to existing")').click();
            await this.page.waitForTimeout(1000);
        });
    }

    async verifyRowCountShowLinkedPopup(actualRowCount: number) {
        await allure.step("Verify row count in times grid", async () => {
            await this.page.waitForTimeout(1000);
            const rowCount = await this.showLinkedTimeRowsGrid.getAgGridRowCount();
            expect(rowCount).toBe(actualRowCount);
        });
    }

    async closeDialogPopup() {
        await allure.step("Close dialog popup", async () => {
            await this.page.getByRole('button', { name: '×' }).click();
            await this.waitForDataLoad('/api/Core/Project/TimeBlock/');
        });
    }

    async clickCancelButtonInPopup() {
        await allure.step("Click Cancel button in show linked time rows popup", async () => {
            const cancelButton = this.page.getByRole('button', { name: 'Cancel' });
            await cancelButton.waitFor({ state: 'visible' });
            await cancelButton.click();
        });
    }

    async clickOkButtonInMoveToExistingPopup() {
        await allure.step("Click OK button in Move to existing popup", async () => {
            const okButton = this.page.getByRole('button', { name: 'OK' });
            await okButton.waitFor({ state: 'visible' });
            await okButton.click();
            await this.waitForDataLoad('/api/Core/Project/TimeBlock/InvoiceRow/');
        });
    }

    async clickOkButtonInPopup() {
        await allure.step("Click OK button in show linked time rows popup", async () => {
            const okButton = this.page.getByRole('button', { name: 'OK' });
            await okButton.waitFor({ state: 'visible' });
            await okButton.click();
            await this.waitForDataLoad('/api/Core/Project/TimeBlock/');
        });
    }

    async verifyProductPurchasePrice(price: string, rowIndex: number) {
        await allure.step("Verify product purchase price", async () => {
            let purchasePrice = await this.getValueFromProductRows('purchasePriceCurrency', rowIndex);
            let normalizedPurchasePrice = purchasePrice.replace(/[^\d,]+/g, '').trim();
            let normalizedPrice = price.replace(',', '.');
            let expectedPrice = (parseFloat(normalizedPrice.replace(/\s/g, ''))).toFixed(2);
            expectedPrice = expectedPrice.replace(/\s+/g, '').replace('.', ',');
            expect(normalizedPurchasePrice, `Purchase Price for row ${rowIndex} is not correct`).toBe(expectedPrice);
        });
    }
}