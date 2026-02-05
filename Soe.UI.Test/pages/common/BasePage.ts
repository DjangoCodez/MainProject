import { type Locator, type Page, expect } from '@playwright/test';
import * as allure from "allure-js-commons";
import { getMenuID } from '../../enums/MenuIdEnums';
import { AngVersion } from '../../enums/AngVersionEnums';
import fs from 'fs';

export class BasePage {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async GoTo(baseUrl: string) {
    await allure.step("Go to sales " + baseUrl, async () => {
      await this.page.goto(baseUrl);
    });

  }

  async SetFinancialYear() {
    await allure.step("Set financial year ", async () => {
      const currentYear = new Date().getFullYear().toString();
      const financialYearContainer = this.page.locator("xpath=//div[@id='ctl00_ctl00_baseMasterBody_soeTopMenu_AccountYearSelector_Container']");
      const financialYear = financialYearContainer.locator("xpath=//a[@title='Financial Year']");
      let isFiancialNotYearMatching = false;
      //await financialYear.click();
      await financialYear.textContent().then(option => {
        if (!option?.includes(currentYear)) {
          isFiancialNotYearMatching = true;
        }
      });
      if (isFiancialNotYearMatching) {
        await financialYear.click();
        await financialYearContainer.locator('xpath=//ul//li/a').filter({ hasText: currentYear }).click();
      }

    });
  }

  async setLastFinancialYear() {
    await allure.step("Select last financial year", async () => {
      const dropdown = this.page.locator('[data-testid="accountYearId"]');
      const currentYear = new Date().getFullYear().toString();
      const lastYear = (new Date().getFullYear() - 1).toString();
      const option = dropdown.locator('option').filter({ hasText: lastYear }).first();
      const value = await option.getAttribute("value");
      if (!value) throw new Error(`No financial year found for ${lastYear}`);

      await dropdown.selectOption(value);
      console.log(`Selected financial year for ${lastYear}`);
    });
  }

  async SetRole(role: string) {
    await allure.step("Set the role ", async () => {
      const roleSelectorContainer = this.page.locator("xpath=//div[@id='ctl00_ctl00_baseMasterBody_soeTopMenu_RoleSelector_Container']");
      const roleUser = roleSelectorContainer.locator("xpath=/a");
      // Find all role options under the role selector container

      let isOptionMatching = false;
      await roleUser.textContent().then(option => {
        if (!option?.match(role)) {
          isOptionMatching = true;
        }
      });


      if (isOptionMatching) {
        await roleUser.click();
        const roleOptions = await roleSelectorContainer.locator('ul > li > a').all();
        for (const option of roleOptions) {
          const text = await option.textContent();
          if (text?.trim() === role) {
            await option.click();
            break;
          }
        }
      }
    });
  }

  async changeLanguage() {
    await allure.step("Set the language to english ", async () => {
      const element = this.page.locator('#ctl00_ctl00_baseMasterBody_soeTopMenu_UserSelector_Container');
      await element.locator('xpath=//a').first().click();
      const languageOptions = await element.locator('xpath=//ul//li/a').all();
      for (const option of languageOptions) {
        const text = await option.textContent();
        if (text && (text.includes('Vaihda') || text.includes('Byt') || text.includes('Endre'))) {
          await option.click();
          await this.page.getByRole('link', { name: 'En' }).or(this.page.getByRole('link', { name: 'en' })).click();
          break;
        }
      }
    });
  }

  async goToMenu(menuName: string, subMenu: string, isMainSubMenu: boolean = false, mainSubMenu: string = "Index") {
    await allure.step("Go to menu " + menuName + ">" + subMenu, async () => {
      let menuId = await getMenuID(menuName);
      if (!menuId) {
        throw new Error(`Menu ID not found for menu: ${menuName}`);
      }
      if (menuId.includes('_')) {
        const tempVal = menuId.split('_');
        const mainSubMenuName = `${tempVal[0]}_${tempVal[1]}`;
        const mainMenuLocator = this.page.locator(`#${menuId}`);
        await mainMenuLocator.hover();
        const subMenuLocator = this.page.locator(`xpath=//div[@id='${mainSubMenuName.toLowerCase()}']`);
        await subMenuLocator.waitFor({ state: 'visible' });
        if (isMainSubMenu) {
          await subMenuLocator.locator(' div > h4 > a', { hasText: new RegExp(mainSubMenu, 'i') }).first().click();
          const leafMenuLocator = subMenuLocator
            .locator(`//a[text()="${mainSubMenu}"]/../../following-sibling::div//ul/li/a`, { hasText: new RegExp(subMenu, 'i') })
          await leafMenuLocator.waitFor({ state: 'visible' })
          await leafMenuLocator.click()
        }
        else {
          await subMenuLocator.locator('div.panel-body > ul > li > a', { hasText: new RegExp(subMenu, 'i') })
            .first()
            .click();
        }
      } else {
        throw new Error(`Invalid: ${menuName}`);
      }

    });
  }

  async switchMenu(menuName: string) {
    await allure.step("Switch to menu " + menuName, async () => {
      await this.page.locator('#ActiveHeader').click();
      await this.page.getByRole('link', { name: menuName }).click();
    })
  }

  async createItem() {
    await allure.step("Create Item", async () => {
      const oldLink = this.page.getByRole('link', { name: '+' });
      const newLink = this.page.getByTestId('add-tab-button');
      await expect(oldLink.or(newLink).first()).toBeVisible({ timeout: 50_000 });
      if (await newLink.isVisible()) {
        await newLink.click();
      }
      else {
        await oldLink.click();
      }
    })
  }

  async goToPageVersion(ang_version: AngVersion) {
    await allure.step("Change to Angular page", async () => {
      const angularJsIcon = this.page.locator('.fa-js-square');
      const angularIcon = this.page.locator('.fa-angular');

      await Promise.all([
        angularJsIcon.first().waitFor({ state: 'visible', timeout: 5000 }).catch(() => { }),
        angularIcon.first().waitFor({ state: 'visible', timeout: 5000 }).catch(() => { })
      ]);

      const [angularIconsCount, angularNewIconsCount] = await Promise.all([
        angularJsIcon.count(),
        angularIcon.count()
      ]);

      if (angularIconsCount === 0 && angularNewIconsCount === 0) {
        return
      }
      try {
        await angularJsIcon.or(angularIcon).first().waitFor({ state: 'visible', timeout: 15_000 });
        switch (ang_version) {
          case AngVersion.JS:
            if (await angularJsIcon.isVisible()) {
              await angularJsIcon.click();
            }
            return ang_version;
          case AngVersion.NEW:
            if (await angularIcon.isVisible()) {
              await angularIcon.click();
            }
            return ang_version;
          case AngVersion.NA:
            throw new Error(`Page version is not available`);
          default:
            return ang_version;
        }
      } catch {
        console.warn(`Angular page version change is not available. Defaulting to ${ang_version}`);
      }
    });
  }

  async clickTabByTestId(dataTestId: string) {
    await allure.step("Go tab by data test id", async () => {
      await this.page.getByTestId(dataTestId).click();
    })
  }

  async goToTabByName(name: string) {
    await allure.step("Go tab by data test id", async () => {
      await this.page.getByRole('link', { name: name, exact: true }).click();
    })
  }

  async save() {
    await allure.step("Save", async () => {
      await this.page.getByTestId('save').click();
    })
  }

  async closeTab() {
    await allure.step("Close tab", async () => {
      await this.page.getByTestId('tab-1-close').click();
      const confirmClose = this.page.getByTestId('primary');
      if (await confirmClose.isVisible({ timeout: 2000 })) {
        await confirmClose.click();
      }
    })
  }

  async reloadPage() {
    await allure.step("Reload page", async () => {
      await this.page.getByTestId('reload').click();
    })
  }

  async waitForDataLoad(endpoint: string | RegExp = 'Core/Dialogs/Progress/Views/Progress.html', timeout: number = 15_000) {
    await allure.step(`Wait for data load: ${endpoint}`, async () => {
      console.log(`Waiting for data load: ${endpoint}`);
      await this.page.waitForResponse((response) => {
        const url = response.url();
        const ok = response.status() === 200;
        // ✅ If endpoint is RegExp → use .test()
        if (endpoint instanceof RegExp) {
          return endpoint.test(url) && ok;
        }
        // ✅ If endpoint is string → use includes()
        if (typeof endpoint === 'string') {
          return url.includes(endpoint) && ok;
        }
        return false;
      }, { timeout }).catch(() => {
        console.warn(`No response from the endpoint: ${endpoint}`);
      });
    })
  }

  async clickAlertMessage(message: string) {
    await allure.step(`Alert Message ${message}`, async () => {
      const okButton = this.page.getByRole('button', { name: message, exact: true });
      await okButton.waitFor({ state: 'visible' });
      await expect(okButton).toBeEnabled();
      await okButton.scrollIntoViewIfNeeded();
      await okButton.first().click();
    });
  }

  async downloadFile(savePath: string, downloadButton: Locator, alertMessage: string | null = null) {
    await allure.step(`Download File ${savePath}`, async () => {
      const [download] = await Promise.all([
        this.page.waitForEvent('download'),
        (async () => {
          await this.page.waitForTimeout(500);
          await downloadButton.first().click();
          if (alertMessage) {
            await this.clickAlertMessage(alertMessage);
          }
        })(),
      ]);
      await download.saveAs(savePath);
    });
  }

  async deleteFile(filePath: string) {
    await allure.step(`Delete File ${filePath}`, async () => {
      if (fs.existsSync(filePath)) {
        fs.unlinkSync(filePath);
      }
    });
  }

  async clearAllFilters(tabIndex: number = 0) {
    await allure.step("Clear all filters", async () => {
      const clearAll = this.page.getByRole('toolbar').getByTitle('Clear all filters').nth(tabIndex);
      if (await clearAll.isVisible()) {
        await clearAll.click();
      }
      await this.page.waitForLoadState("load");
    });
  }

  async toggleMainMenu() {
    await allure.step("Toggle main menu", async () => {
      const menuToggle = this.page.locator("//div[contains(@class,'menu-icons-container')]//span[@id='ActiveHeader']");
      await menuToggle.waitFor({ state: 'visible' });
      await menuToggle.click();
    });
  }

  async goToModule(moduleName: string) {
    await allure.step(`Click ${moduleName} module from main menu`, async () => {
      await this.toggleMainMenu();
      const module = this.page.locator(`//div[contains(@class,'module-icon-text-container')][span[contains(@class,'module-text') and normalize-space()='${moduleName}']]`);
      await module.waitFor({ state: 'visible' });
      await module.click();
    });
  }

  async clickYes() {
    await allure.step("Click yes button", async () => {
      const yesButton = this.page.locator('button.btn.btn-primary', { hasText: 'Yes' });
      await yesButton.waitFor({ state: 'visible' });
      if (await yesButton.isVisible() && await yesButton.isEnabled()) {
        await yesButton.click();
      } else {
        console.log('Yes button is not ready to be clicked.');
      }
    });
  }
}