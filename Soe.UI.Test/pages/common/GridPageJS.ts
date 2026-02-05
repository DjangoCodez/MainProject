import { expect, Locator, type Page } from '@playwright/test';

export class GridPageJS {
    readonly page: Page;
    readonly rootLocator: Locator;
    readonly isModel: boolean = false;

    /**
     * Constructor for GridPageJS
     * @param page Playwright Page object
     * @param agGrid Optional ag-grid identifier, defaults to 'ctrl.gridAg.options.gridOptions'
     * @param modal Optional flag to indicate if the grid is inside a modal
     * @param labelKey Optional label key to locate the grid
     */
    constructor(page: Page, agGrid: string = 'ctrl.gridAg.options.gridOptions', modal: boolean = false, labelKey: string = '', tabIndex: number = 0) {
        this.page = page;
        if (labelKey) {
            this.rootLocator = this.page.locator(`//*[@label-key='${labelKey}']//div[@ag-grid='${agGrid}']`).nth(tabIndex);
        } else {
            this.rootLocator = this.page.locator(`//div[@ag-grid='${agGrid}']`).nth(tabIndex);
        }
        if (modal) {
            this.isModel = true;
            this.rootLocator = this.page.locator(`//div[@class= 'modal-content']//div[@ag-grid='${agGrid}']`).nth(tabIndex);
        }
    }

    async waitForPageLoad(timeout: number = 15000) {
        await this.rootLocator.waitFor({ state: 'attached', timeout });
        const isGridVisible = await this.rootLocator.isVisible();
        if (!isGridVisible) {
            throw new Error('AG Grid is not visible after waiting for the specified timeout.');
        }
    }

    /**
  * Fills a text input inside .grid-container using partial aria-label match
  * @param page Playwright Page object
  * @param columnName Substring to match in the aria-label (e.g. "Name Filter")
  * @param value The text value to input
  * @param timeout The timeout for wait for the input to be visible, default is 3000ms
  * @param rowIndex The index of the row to target, default is 0 (first row)
  */
    async filterByName(columnName: string, value: string, timeout: number = 5000, rowIndex: number = 0) {
        const selector = `//input[@type='text' and contains(@aria-label, '${columnName}')]`;
        const inputLocator = this.rootLocator.locator(selector);
        await this.page.waitForSelector(selector, { state: 'attached', timeout });
        const count = await inputLocator.count();
        if (count === 0 || count <= rowIndex) {
            throw new Error(`Input with aria-label containing "${columnName}" not found at rowIndex ${rowIndex}.`);
        }

        const input = inputLocator.nth(rowIndex);
        console.log(`Filling input at rowIndex ${rowIndex} with value: ${value}`);
        await input.waitFor({ state: 'visible', timeout });
        await input.fill('');
        await this.page.waitForTimeout(1000);
        const rowCount: number = await this.getAgGridRowCount();
        await input.fill(value);
        if (rowCount > 1) {
            let newRowCount = rowCount;
            const startTime = Date.now();
            while (Date.now() - startTime < 10000) {
                newRowCount = await this.getAgGridRowCount();
                if (newRowCount < rowCount) {
                    break;
                }
                await this.page.waitForTimeout(1000);
            }
            expect(newRowCount).toBeLessThanOrEqual(rowCount);
        } else {
            await this.page.waitForTimeout(1000); // allow grid to refresh
            try {
                await this.rootLocator
                    .locator('//div[contains(@class, "ag-cell-range-selected")]')
                    .waitFor({ state: 'attached', timeout: 3000 });
            } catch {
                console.log('No filter happened within 3 seconds, or only one available row.');
            }
        }
    }

    /**
    * Clicks Edit button when one raw is filtered.   
    */
    async edit() {
        const buttonLocator = this.rootLocator.locator(`//button[contains(@title, 'Edit')]`).last();

        if (await buttonLocator.count() === 0) {
            throw new Error(`Button with title containing Edit not found.`);
        }

        await buttonLocator.first().click();
    }

    /**
    * Returns the count of AG Grid rows inside the center columns container.
    * 
    * @returns Number of rows found
    */
    async getAgGridRowCount() {
        const rowSelector = this.rootLocator.locator(`//div[@class="ag-center-cols-container"]//div[@role="row"]`);
        const count = await rowSelector.count();
        return count;
    }

    /**
    * Returns the count of AG Grid Filtered rows inside the center columns container.
    * 
    * @returns Number of rows found
    */
    async getFilteredAgGridRowCount() {
        let locator = this.page.locator('.soe-ag-grid-totals-filtered-count');
        const isFilteredCountVisible: boolean = await locator.isVisible();
        if (!isFilteredCountVisible) {
            locator = this.page.locator("xpath=//div[contains(@class, 'soe-ag-totals-row-part')]").first();
        }

        await locator.waitFor({ state: 'attached', timeout: 20000 });
        const text = await locator.innerText();
        let match: RegExpMatchArray | null = null;
        if (!isFilteredCountVisible) {
            match = text.match(/Total\s+(\d+)/);
        } else {
            // Extract the number using regex
            match = text.match(/\(Filtered (\d+)\)/);
        }
        if (!match) {
            throw new Error(`Failed to parse filtered count from text: ${text}`);
        }
        const filteredCount = parseInt(match[1], 10);
        return filteredCount;
    }

    /**
     * Clicks a specific cell in a grid by identifying the column using its column ID and finding 
     * the corresponding cell based on its text content
     * @param colId The column ID (e.g., 'supplierName', 'supplierId')
     */
    async getCellvalueByColId(colId: string, isCellClick: boolean = true) {
        const xpath = `//div[@class="ag-center-cols-container"]//div[@role="gridcell" and @col-id='${colId}']`;
        const cellLocator = this.page.locator(xpath);
        await expect(cellLocator).toBeVisible({ timeout: 5000 });
        if (isCellClick) {
            await cellLocator.click();
        }
        return await cellLocator.innerText();
    }

    async selectAllCheckBox() {
        let checkBox: Locator;
        checkBox = this.page.getByRole('checkbox', { name: 'Press Space to toggle all' });
        while (!(await checkBox.nth(0).isChecked())) {
            await checkBox.nth(0).waitFor({ state: 'attached' });
            await checkBox.nth(0).click({ force: true });
        }

    }
    async unselectAllCheckBox() {
        let checkBox: Locator;
        checkBox = this.page.getByRole('checkbox', { name: 'Press Space to toggle all' });
        while (await checkBox.nth(0).isChecked()) {
            await checkBox.nth(0).waitFor({ state: 'visible' });
            await checkBox.nth(0).click();
        }
    }

    async unselectCheckBox(rowIndex: number = 0) {
        const checkBox = this.rootLocator.locator(`//input[@type='checkbox' and starts-with(@aria-label, 'Press Space to toggle row selection')]`).nth(rowIndex);
        await checkBox.waitFor({ state: 'visible' });
        if (await checkBox.isChecked()) {
            await checkBox.click();
        }
    }

    async selectCheckBox(rowIndex: number = 0) {
        let checkBox: Locator;
        checkBox = this.rootLocator.locator(`//input[@type='checkbox' and starts-with(@aria-label, 'Press Space to toggle row selection')]`);
        await checkBox.nth(rowIndex).waitFor({ state: 'visible' });
        return checkBox.nth(rowIndex).click();
    }

    async selectSubGridCheckBox(rowIndex: number = 0) {
        let checkBox: Locator;
        checkBox = this.rootLocator.locator(`//div[@aria-label="Press SPACE to select this row."]//input[@type='checkbox']`);
        await checkBox.nth(rowIndex).waitFor({ state: 'visible' });
        return checkBox.nth(rowIndex).click();
    }
    /**
     * Selected the row first and clicks a specific cell in a grid by identifying the column using its column ID and finding 
     * the corresponding cell based on its text content
     * @param colId The column ID (e.g., 'supplierName', 'supplierId')
     * @param gridIndex The gridIndex (e.g., '1', '2')
     */
    async getCellvalueByColIdandGrid(colId: string, rowIndex: number = 0) {
        const xpath = `//div[@row-index='${rowIndex}']//div[@role="gridcell" and @col-id="${colId}"]`;
        const cellLocator = this.rootLocator.locator(xpath);
        await expect(cellLocator).toBeVisible({ timeout: 5000 });
        return await cellLocator.innerText();
    }

    /**
     * click dropdown and select value
     * @param value The value to select from the dropdown
     */
    async enterDropDownValue(value: string) {
        const input = this.page.locator('#typeahead-editor');
        await input.fill(value);
        await this.page.waitForSelector('ul.typeahead.dropdown-menu >> text=' + value);
        await this.page.locator(`ul.typeahead.dropdown-menu >> text=${value}`).first().click();
    }

    /**
     * click dropdown and select value
     * @param value The value to select from the dropdown
     */
    async enterDropDownValueGrid(colId: string, value: string, rowIndex: number = 0) {
        const cellLocator = this.rootLocator.locator(`//div[@row-index='${rowIndex}']//div[@role="gridcell" and @col-id="${colId}"]`);
        await cellLocator.click();
        await this.enterDropDownValue(value);
    }

    /**
    * click dropdown and select value
    * @param optionText Enter the value to select from the dropdown
    */
    async enterDropDownValueGridRichSelecter(colId: string, optionText: string, rowIndex: number = 0) {
        const cellLocator = this.rootLocator.locator(`//div[@row-index='${rowIndex}']//div[@role="gridcell" and @col-id="${colId}"]`);
        await cellLocator.click();
        await this.page.waitForSelector('div.ag-rich-select-virtual-list-container');
        await this.page.locator('div.ag-rich-select-row', { hasText: optionText }).click();
    }

    /**
     * Click cell and enter value in the input field
     * @param value The value to enter in the input field
     * @param colId The column ID (e.g., 'supplierName', 'supplierId')
     */
    async enterGridValueByColumnId(colId: string, value: string, rowIndex: number = 0, isCelEditor: boolean = false) {
        const cellLocator = this.rootLocator.locator(`//div[@row-index='${rowIndex}']//div[@role="gridcell" and @col-id="${colId}"]`);
        await cellLocator.first().click();
        let inputLocator = cellLocator.locator('//input');
        if (isCelEditor) {
            inputLocator = this.page.locator(`//div[@aria-label='Cell Editor']//input[@id='typeahead-editor']`);
            await inputLocator.first().fill(value);
            const drop = this.page.locator(`ul.typeahead.dropdown-menu >> text=${value}`);
            //await this.page.waitForSelector(`ul.typeahead.dropdown-menu >> text=${value}`);
            await drop.waitFor({ state: 'visible' });
            await drop.click({ force: true, delay: 200 });
            //    await this.page.locator(`ul.typeahead.dropdown-menu >> text=${value}`).click();
            await drop.waitFor({ state: 'detached' });
        } else {
            await inputLocator.waitFor({ state: 'visible' });
            await inputLocator.clear();
            await inputLocator.fill(value);
        }
        await this.page.keyboard.press('Enter');
    }

    /**
     * Clicks a checkbox in a specific column by its column ID and row index.
     * @param colId The column ID where the checkbox is located.
     * @param doCheck Boolean indicating whether to check or uncheck the checkbox.
     * @param rowIndex The index of the row where the checkbox is located (default is 0).
     */
    async clickCheckBoxByColumnId(colId: string, doCheck: boolean, rowIndex: number = 0) {
        const cellLocator = this.rootLocator.locator(`//div[@row-index='${rowIndex}']//div[@role="gridcell" and @col-id="${colId}"]`);
        const checkBox = cellLocator.locator('//input[@type="checkbox"]');
        if (doCheck && !(await checkBox.isChecked())) {
            await checkBox.click();
        } else if (!doCheck && await checkBox.isChecked()) {
            await checkBox.click();
        }
        expect(await checkBox.isChecked()).toBe(doCheck);
    }

    /**
     * Clicks a link in a specific column by its column ID and row index.
     * @param colId The column ID where the link is located.
     * @param rowIndex The index of the row where the link is located (default is 0).
     * @throws Error if the link is not found.
     */
    async clickLinkByColumnId(colId: string, rowIndex: number = 0) {
        const cellLocator = this.rootLocator.locator(`//div[@row-index='${rowIndex}']//div[@role="gridcell" and @col-id="${colId}"]`);
        const linkLocator = cellLocator.locator('//a');
        if (await linkLocator.count() === 0) {
            throw new Error(`Link in column ${colId} at row ${rowIndex} not found.`);
        }
        await linkLocator.first().click();
    }

    async clickButtonByColumnId(colId: string, rowIndex: number = 0) {
        const cellLocator = this.rootLocator.locator(`//div[@row-index='${rowIndex}']//div[@role="gridcell" and @col-id="${colId}"]`);
        const linkLocator = cellLocator.locator('//button');
        await linkLocator.waitFor({ state: 'visible', timeout: 5000 });
        await linkLocator.first().click();
    }

    async checkRowIconExist(iconClass: string, rowIndex: number = 0) {
        const iconLocator = this.rootLocator.locator(`//div[@row-index='${rowIndex}']//button[contains(@class, '${iconClass}')]`);
        await expect(iconLocator).toBeVisible({ timeout: 5000 });
    }

    async checkRowIconCount(iconClass: string, count: number = 0) {
        const iconLocator = this.rootLocator.locator(`//button[contains(@class, '${iconClass}')]`);
        await expect(iconLocator).toHaveCount(count);
    }

    async getRowColumnValue(colId: string, rowNumber: number = 0) {
        return await this.rootLocator.locator(`//div[@row-index='${rowNumber}']//div[@role="gridcell" and @col-id="${colId}"]`).textContent();
    }

    async getCellValueFromGrid(colId: string, value: string, colIdToGet: string, isGrouped: boolean = false, returnRowIndex: boolean = false): Promise<string | null | { cellValue: string | null; rowIndex: number } | undefined> {
        const rowCount = await this.getAgGridRowCount();
        for (let i = 0; i < rowCount; i++) {
            let rowsStr = this.rootLocator.locator(`//div[@row-index='${i}']//div[@role="gridcell" and @col-id="${colId}"]`);
            if (isGrouped) {
                rowsStr = this.rootLocator.locator(`//div[@row-index='${i}']//div[@role="gridcell" and @col-id="${colId}"]//span[@ref='eValue']`);
            }
            if ((await rowsStr.textContent()) === value) {
                const cellValue = await this.rootLocator.locator(`//div[@row-index='${i}']//div[@role="gridcell" and @col-id="${colIdToGet}"]`).textContent();
                if (!returnRowIndex) {
                    return cellValue;
                }
                return {
                    cellValue, rowIndex: i
                };
            }
        }
    }

    async waitForEndPontResponse(endpoint: string, timeout: number = 15_000) {
        await this.page.waitForResponse(response =>
            response.url().includes(endpoint) && response.status() === 200
            , {
                timeout: timeout
            });
    }



    async setGridColumnHeaders(columnName: string) {
        const columnOptions = this.rootLocator.locator(`//div[@class="ag-pinned-right-header"]//div[contains(@class, "ag-header-row") and contains(@class, "ag-header-row-column")]//div[contains(@class, "ag-header-cell") and @col-id="soe-grid-menu-column"]//div[contains(@class, "ag-cell-label-container")]//span[@ref="eMenu"]//span`);
        await columnOptions.waitFor({ state: 'visible' });
        await columnOptions.click();
        const tabOption = this.page.locator(`//div[@class='ag-theme-balham ag-popup']//div[@aria-label='Column Menu']//div[@role='tablist']//span[@role='tab']`);
        await tabOption.nth(1).waitFor({ state: 'visible' });
        await tabOption.nth(1).click();
        const searchBox = this.page.locator(`//input[@aria-label='Filter Columns Input']`);
        await searchBox.waitFor({ state: 'visible' });
        await searchBox.click();
        await searchBox.fill(columnName);
        const columnHeader = this.page.locator('xpath=//div[@aria-label="Column List"]/div', { hasText: new RegExp(columnName, 'i') });
        await expect.poll(async () => await columnHeader.count(), { timeout: 5000 }).toBe(1);
        const checkbox = columnHeader.locator(`//input[@type='checkbox']`);
        //  const checkbox = this.page.locator(`//div[contains(@class, 'ag-column-select-list')]//span[contains(@class, 'ag-column-select-column-label') and normalize-space(text())='${columnName}']/preceding-sibling::div[contains(@class, 'ag-column-select-checkbox')]//input[@type='checkbox']`);
        await checkbox.waitFor({ state: 'visible' });
        await checkbox.scrollIntoViewIfNeeded();
        const isChecked = await checkbox.isChecked();
        if (!isChecked) {
            await checkbox.click();
        }
        await tabOption.nth(1).click(); // for close column menu
    }

}