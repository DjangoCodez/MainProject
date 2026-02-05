import { expect, Page } from '@playwright/test';
import { GridPageJS } from './GridPageJS';

export class ModelGridPageJS extends GridPageJS {
    constructor(page: Page, agGrid: string = 'ctrl.soeGridOptions.gridOptions') {
        super(page, agGrid, true);
    }

    // Add ModelGridPageJS specific methods and properties here

    /**
         * Checks if a checkbox with the given name is ticked, and clicks it if not.
         * @param name The name of the checkbox to check.
         */
    async checkCheckboxTicked(name: string) {
        const modelCheck = this.page.locator('.modal-dialog');
        await modelCheck.waitFor({ state: 'attached', timeout: 10000 });
        const checkbox = modelCheck.getByRole('checkbox', { name });
        await expect(checkbox).toBeVisible();
        if (!(await checkbox.isChecked())) {
            console.log(`Checkbox with name "${name}" is not checked. Clicking to check it.`);
            await checkbox.click();
        }
        await expect(checkbox).toBeChecked();
    }
}