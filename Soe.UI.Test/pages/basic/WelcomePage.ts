import { expect, type Locator, type Page } from '@playwright/test';
import * as allure from "allure-js-commons";

export class WelcomePage {
    readonly page: Page;

    constructor(page: Page) {
        this.page = page;
    }

    async selectModule(moduleName: string) {
        await allure.step("Select " + moduleName, async () => {
            switch (moduleName) {
                case 'Sales': {
                    //await this.page.locator("xpath=//span[@data-ng-class='{'fa-chart-line': true}']").click();
                    //await this.page.waitForTimeout(20000);
                    await this.page.getByText(moduleName).click();
                    expect(await this.page.getByText('Dashboard - Sales')).toBeTruthy();
                    break;
                }
                case 'constant_expr2': {
                    //statements; 
                    break;
                }
                default: {
                    //statements; 
                    break;
                }
            }
        });
    }


}