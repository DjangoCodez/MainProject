import { type Page } from '@playwright/test';
import { BasePage } from '../common/BasePage';
import * as allure from "allure-js-commons";


export class FinanceBasePage extends BasePage {

  readonly page: Page;

  constructor(page: Page) {
    super(page);
    this.page = page;
  }


  async SetupFinance(envUrl: string, role: string) {
    await this.GoTo(envUrl);
    await this.SetRole(role);
    await this.changeLanguage();
    await this.GoToFinance(envUrl);
    await this.SetFinancialYear();
  }

  async GoToFinance(baseUrl: string) {
    await allure.step("Go to finance ", async () => {
      await this.page.goto(baseUrl + '/soe/economy');
      let isPriceListPopUpVisible: boolean = false;
      try {
        const isPriceListPopUp = await this.page.waitForSelector("xpath=//form//button[@id='submit']", {
          state: 'visible',
          timeout: 5000 // waits up to 10 seconds
        });
        isPriceListPopUpVisible = await isPriceListPopUp.isVisible();
        if (isPriceListPopUpVisible) {
          await isPriceListPopUp.click();
          await this.page.waitForTimeout(3000);
        }
      } catch (error) {
        console.log('Element was not visible within timeout.');
      }

      // Check if the welcome page is visible and navigate to the billing page if it is
      const isWelComePage = this.page.getByText('Welcome to SoftOne GO');
      if (await isWelComePage.isVisible()) {
        await this.page.goto(baseUrl + '/soe/billing');
      }
    });

  }

  /**
   * Toggle a right-side menu by its type.
   * @param menuType one of: information, help, academy, message, report, document
   */
  async toggleRightSideMenu(menuType: 'information' | 'help' | 'academy' | 'message' | 'report' | 'document', subMenu: string) {
    const locator = `#${menuType}-menu-toggle`;
    await this.page.waitForSelector(locator, { state: 'visible' });
    await this.page.click(locator);
    await this.selectSubMenu(subMenu);
  }

  async selectSubMenu(menuName: string) {
    const subMenuLocator = this.page.locator(`//li[span[normalize-space()='${menuName}']]`);
    await subMenuLocator.waitFor({ state: 'visible' });
    await subMenuLocator.click();
  }

}