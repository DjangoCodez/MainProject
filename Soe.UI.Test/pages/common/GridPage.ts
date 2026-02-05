import { expect, type Locator, type Page } from '@playwright/test';
import * as allure from "allure-js-commons";

export class GridPage {
    readonly page: Page;
    private rootGrid: Locator;
    private headerRows!: Locator;
    private headerRightPin!: Locator;
    private bodyRows!: Locator;


    constructor(page: Page, agGridTestDataId: string) {
        this.page = page;
        this.rootGrid = this.page.getByTestId(agGridTestDataId);
        this.getHeaderRows();
        this.getHeaderRightPin();
        this.getBodyRows();
    }

    private async getHeaderRows() {
        this.headerRows = this.rootGrid.locator("xpath=//div[contains(@class, 'ag-header-viewport')]//div[@data-ref='eCenterContainer']//div[@role='row']");
    }

    private async getHeaderRightPin() {
        this.headerRightPin = this.rootGrid.locator("xpath=//div[contains(@class, 'ag-header-viewport')]//div[contains(@class, 'ag-pinned-right-header')]//div[@role='row']");
    }

    private async getBodyRows() {
        this.bodyRows = this.rootGrid.locator("xpath=//div[@data-ref='eBody']//div[@role='row']");
    }

    private async getColumnIdByName(columnName: string) {
        let colid: string = "col-id";
        await this.headerRows.locator("xpath=//div[@role='columnheader']").first().waitFor({ state: 'visible' });
        for (const li of await this.headerRows.locator("xpath=//div[@role='columnheader']").all()) {
            if (await li.getByText(columnName, { exact: true }).count() == 1) {
                colid = await li.getAttribute('col-id') ?? "Not found";
            }
        }
        return colid;
    }

    async waitForPageLoad(timeout: number = 15000) {
        await this.rootGrid.waitFor({ state: 'attached', timeout });
        const isGridVisible = await this.rootGrid.isVisible();
        if (!isGridVisible) {
            throw new Error('AG Grid is not visible after waiting for the specified timeout.');
        }
    }

    /**
   * Enter the value for given column and row of the grid.
   *
   * @remarks
   * If row number not given enter the value for next available row for given column.
   *
   * @param {string} columnName - Column name
   * @param {string} value - Value to enter
   * @param {number} rowNumber - Row number (Optional)
   * @returns void
   *
   */
    async enterValueToGrid(columnName: string, value: string, rowNumber: number = 0) {
        await allure.step("Enter Value to Grid for col:" + columnName + " value:" + value, async () => {
            const colId = await this.getColumnIdByName(columnName);
            const rowsStr: string = "xpath=//div[@col-id='" + colId + "']";
            const rowCount = await this.bodyRows.locator(rowsStr).count();
            const rowNumberEdit = rowNumber === 0 ? rowCount - 1 : rowNumber
            const rowToEnterValue = this.bodyRows.locator(rowsStr).nth(rowNumberEdit);
            await rowToEnterValue.click();
            await rowToEnterValue.locator('xpath=//input').fill(value);
            await this.page.keyboard.press('Enter');
        });
    }

    /**
  * Enter the value for a given column (by columnId) and row of the grid.
  *
  * @remarks
  * If two column names headers have the same name, use this method to enter value. 
  *
  * @param columnId - Column ID
  * @param value - Value to enter
  * @param rowNumber - Row number (optional)
  */
    async enterValueToGridByColumnId(columnId: string, value: string, rowNumber = 0) {
        await allure.step(`Enter value "${value}" to grid column "${columnId}"`, async () => {
            const cellLocator = this.bodyRows.locator(`//div[@col-id='${columnId}']`);
            const rowCount = await cellLocator.count();
            const targetRowIndex = rowNumber === 0 ? rowCount - 1 : rowNumber;
            const targetCell = cellLocator.nth(targetRowIndex);
            await targetCell.click();
            await targetCell.locator('input').fill(value);
            await this.page.keyboard.press('Enter');
        }
        );
    }


    /**
   * Filter the grid records by given column and value.
   *
   * @param {string} columnName - Column name
   * @param {string} valueToFilter - Value to filter
   *
   */
    async filterByColumnNameAndValue(columnName: string, valueToFilter: string) {
        await allure.step("Filter by col name & val :" + columnName + "," + valueToFilter, async () => {
             await this.headerRows.getByLabel(columnName + ' Filter Input').nth(0).fill(valueToFilter);
            const isFilteredVisible: boolean = await this.page.getByTestId('filtered').waitFor({ state: 'attached', timeout: 3000 }).then(() => true).catch(() => false);
            if (!isFilteredVisible) {
                console.log(`No filter  happened within 3 seconds, or only one avaialable row.`);
            }
        });
    }

    /**
  * This will verify whether it will show correct filtered count in bottom. It can validate as 0 for clear filter
  * 
  * @param {string} count - Count to verify
  *
  */
    async verifyFilteredItemCount(count: string) {
        await allure.step("Verify filter item count as " + count, async () => {
            let countN: number = +count;
            if (countN === 0) {
                await expect(this.page.getByTestId('filtered')).toHaveText('(Filtered 0)');
            } else {
                const isFiltered: string = await this.page.getByTestId('filtered').waitFor({ state: 'attached', timeout: 3000 }).then(() => 'Filtered').catch(
                    async () => await this.page.getByTestId('grid-totals').waitFor({ state: 'attached', timeout: 3000 }).then(() => 'Total').catch(() => 'Not found'));
                if (isFiltered === 'Filtered') {
                    expect(this.page.getByTestId('filtered')).toHaveText('(Filtered ' + count + ') ');
                } else if (isFiltered === 'Total') {
                    expect(this.page.getByTestId('grid-totals')).toHaveText(' Total ' + count + ' ');
                } else {
                    throw new Error('Filtered count is not found');
                }
            }
        });
    }

    async verifyFilteredIsCleared() {
        await allure.step("Verify filter is cleared", async () => {
            await expect(this.page.getByTestId('filtered')).toHaveCount(0);
        });
    }

    /**
  * Verify filtered records have only count 1 for given value.
  *
  * @param {string} value - value
  * @param {boolean} exactMatch - whether it should look for exact match of value, default is true
  *
  */
    async verifyFilteredItem(value: string, exactMatch: boolean = true) {
        await allure.step("Verify filtered Item", async () => {
            await expect(this.bodyRows.getByRole('gridcell', { name: value, exact: exactMatch })).toHaveCount(1);
        });
    }

    /**
  * Double click filtered records macth grid cell value. Click on first element if multiple found
  *
  * @param {string} value - value
  * @param {boolean} exactMatch - whether it should look for exact match of value, default is true
  *
  */
    async doubleClickGridCellItem(value: string, exactMatch: boolean = true) {
        await allure.step("Double click on item " + value, async () => {
            await this.bodyRows.getByRole('gridcell', { name: value, exact: exactMatch }).nth(0).dblclick();
        });
    }

    /**
   * Enter the value for given column and row of the grid.
   *
   * @remarks
   * If row number not given enter the value for next available row for given column.
   *
   * @param {string} columnName - Column name
   * @param {string} value - Value to enter
   * @param {number} rowNumber - Row number (Optional)
   * @returns void
   *
   */
    async verifyCellValueFromGrid(columnName: string, value: string, rowNumber: number = 0) {
        await allure.step("Verify cell from grid col & value " + columnName + ", " + value, async () => {
            const colId = await this.getColumnIdByName(columnName);
            const rowsStr: string = "xpath=//div[@col-id='" + colId + "']";
            const rowCount = await this.bodyRows.locator(rowsStr).count();
            const rowNumberEdit = rowNumber === 0 ? rowCount - 1 : rowNumber - 1;
            const rowToEnterValue = this.bodyRows.locator(rowsStr).nth(rowNumberEdit);
            await expect(rowToEnterValue).toHaveText(value);
        });
    }


    /**
  * Enter the value for given column and row of the grid.
  *
  * @remarks
  * If row number not given enter the value for next available row for given column.
  *
  * @param {string} columnName - Column name
  * @returns void
  *
  */
    async getRowCount(columnName: string) {
        return await allure.step("Get row count", async () => {
            const colId = await this.getColumnIdByName(columnName);
            const rowsStr: string = "xpath=//div[@col-id='" + colId + "']";
            return await this.bodyRows.locator(rowsStr).count();
        });
    }

    /**
  * TBA
  *
  * @remarks
  * TBA
  *
  * @param {string} columnName - Column name
  * @param {number} rowNumber - Row number (Optional)
  * @returns void
  *
  */
    async clickGridRow(columnName: string, rowNumber: number = 0) {
        return await allure.step("Click on row col & row " + columnName + "," + rowNumber, async () => {
            const colId = await this.getColumnIdByName(columnName);
            const rowsStr: string = "xpath=//div[@col-id='" + colId + "']";
            const rowCount = await this.bodyRows.locator(rowsStr).count();
            const rowNumberEdit = rowNumber == 0 ? rowCount - 1 : rowNumber - 1;
            await this.bodyRows.locator(rowsStr).nth(rowNumberEdit).dblclick();
            return rowCount;
        });
    }

    /**
  * TBA
  *
  * @remarks
  * If row number not given enter the value for next available row for given column.
  *
  * @param {string} columnName - Column name
  * @param {string} value - Value to enter
  * @param {number} rowNumber - Row number (Optional)
  * @returns void
  *
  */
    async getCellValueFromGrid(columnName: string, value: string, columnNameToGet: string) {
        return await allure.step("Get value from grid " + columnName + "," + value + "," + columnNameToGet, async () => {
            const colId = await this.getColumnIdByName(columnName);
            const rowsStr: string = "xpath=//div[@col-id='" + colId + "']";
            const colIdExtract = await this.getColumnIdByName(columnNameToGet);
            const rowsStrExtract: string = "xpath=//div[@col-id='" + colIdExtract + "']";
            const rowCount: number = await this.bodyRows.locator(rowsStr).count();
            //const rowNumberEdit = rowNumber === 0 ? rowCount - 1 : rowNumber - 1;
            for (let i = 0; i < rowCount; i++) {
                if (await this.bodyRows.locator(rowsStr).nth(i).textContent() == value) {
                    return await this.bodyRows.locator(rowsStrExtract).nth(i).textContent();
                }
            }
        });
    }

    /**
* TBA
*
* @remarks
* TBA
*
* @param {string} columnName - Column name
* @param {number} rowNumber - Row number (Optional)
* @returns void
*
*/
    async getRowColumnValue(columnName: string, rowNumber: number = 0) {
        return await allure.step("Get value from grid " + columnName + "," + rowNumber, async () => {
            const colId = await this.getColumnIdByName(columnName);
            const rowsStr: string = "xpath=//div[@col-id='" + colId + "']";
            const rowCount = await this.bodyRows.locator(rowsStr).count();
            const rowNumberEdit = rowNumber == 0 ? rowCount - 1 : rowNumber - 1;
            return await this.bodyRows.locator(rowsStr).nth(rowNumberEdit).textContent();
        });
    }


    /**
 * TBA
 *
 * @remarks
 * If row number not given enter the value for next available row for given column.
 *
 * @param {string} columnName - Column name
 * @param {string} value - Value to enter
 * @param {number} rowNumber - Row number (Optional)
 * @returns void
 *
 */
    async enterCellValuetoGrid(columnName: string, value: string, columnNameToEnter: string, valueToEnter: string) {
        await allure.step("Enter value to grid for " + columnName + "," + value, async () => {
            const colId = await this.getColumnIdByName(columnName);
            const rowsStr: string = "xpath=//div[@col-id='" + colId + "']";
            const colIdExtract = await this.getColumnIdByName(columnNameToEnter);
            const rowsStrExtract: string = "xpath=//div[@col-id='" + colIdExtract + "']";
            const rowCount: number = await this.bodyRows.locator(rowsStr).count();
            //const rowNumberEdit = rowNumber === 0 ? rowCount - 1 : rowNumber - 1;
            for (let i = 0; i < rowCount; i++) {
                if (await this.bodyRows.locator(rowsStr).nth(i).textContent() == value) {
                    const rowToEnterValue = this.bodyRows.locator(rowsStrExtract).nth(i);
                    await rowToEnterValue.click();
                    await rowToEnterValue.locator('xpath=//input').fill(valueToEnter);
                    await this.page.keyboard.press('Enter');
                    break;
                }
            }
        });
    }

    /**
 * Select the value for given column and row of the grid.
 *
 * @remarks
 * If row number not given enter the value for next available row for given column.
 *
 * @param {string} columnName - Column name
 * @param {string} value - Value to enter
 * @param {number} rowIndex - Row number (Optional)
 * @returns void
 *
 */
    async selectDropdownValueFromGrid_1(columnName: string, value: string, rowIndex: number = 0) {
        const colId = await this.getColumnIdByName(columnName);
        const rowsStr: string = "xpath=//div[@col-id='" + colId + "']";
        const cellLocator = this.bodyRows.locator(rowsStr).nth(rowIndex).getByText(value);
        await cellLocator.scrollIntoViewIfNeeded();
        await cellLocator.click();
        await this.page.keyboard.press('Enter');
    }


    /**
  * Select the value for given drop down of the grid.
  *
  * @param {string} columnName - Column name
  * @param {string} value - Value to enter
  * @param {number} rowIndex - Row number (Optional)
  * @returns void
  *
  */

    async selectDropdownValueFromGrid(columnName: string, value: string, rowIndex: number = 0) {
        const colId = await this.getColumnIdByName(columnName);
        const cellLocator = this.bodyRows.locator(`xpath=//div[@col-id='${colId}']`).nth(rowIndex);
        await cellLocator.click();
        const input = cellLocator.locator("input[role='combobox']");
        await input.waitFor({ state: 'visible' });
        await input.fill(value);
        const option = this.page.locator(`mat-option >> text="${value}"`);
        await option.waitFor({ state: 'visible' });
        await option.click();
    }

    async selectDropdownValueFromGrid_2(columnName: string, value: string, rowIndex: number = 0) {
        const colId = await this.getColumnIdByName(columnName);
        const cellLocator = await this.bodyRows.locator(`xpath=//div[@col-id='${colId}']`).nth(rowIndex);
        await cellLocator.click();
        const combo = cellLocator.locator('.ag-wrapper.ag-picker-field-wrapper[role="combobox"]');
        const listId = await combo.getAttribute('aria-controls');
        const list = this.page.locator(`#${listId}`);
        await list.waitFor({ state: 'visible' });
        await list.locator(`.ag-rich-select-row:has-text("${value}")`).click();
    }

    async clickGridActiveInactiveCheckbox() {
        await allure.step("Click Active/Inactive checkbox in grid", async () => {
            let checkBox: Locator;
            checkBox = this.rootGrid.locator(`//input[@type='checkbox' and contains(@class, 'form-check-input is-active-column')]`);
            await checkBox.nth(0).waitFor({ state: 'visible' });
            await checkBox.nth(0).click();
        });
    }
}       