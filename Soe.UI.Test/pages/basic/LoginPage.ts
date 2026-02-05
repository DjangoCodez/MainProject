import { expect, type Locator, type Page } from '@playwright/test';
import * as allure from "allure-js-commons";

export class LoginPage {
    readonly page: Page;
    readonly inputUserName: Locator;
    readonly inputPaswword: Locator;
    readonly btnLogin: Locator;

    constructor(page: Page) {
        this.page = page;
        this.inputUserName = page.locator('#login-username');
        this.inputPaswword = page.locator('#login-password');
        this.btnLogin = page.getByRole('button', { name: 'Login' });
    }

    async Login(userName: string, password: string) {
        await allure.step("Login " + userName, async () => {
            //await expect(this.btnLogin).toBeVisible();
            await this.inputUserName.fill(userName);
            await this.inputPaswword.fill(password)
            //await this.page.getByRole('button', { name: 'Login' }).click();
            await this.page.locator("//*[@id='btn-login']").click();
        });
    }

}