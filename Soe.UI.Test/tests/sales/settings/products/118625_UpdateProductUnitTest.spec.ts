import { test } from '../../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from "../../../../utils/properties";
import { ProductSettingsUtil } from "../../../../apis/utils/ProductSettingsUtil";

let productUnitUtil: ProductSettingsUtil;
let testCaseId: string = "118625";
let envUrl: string;
let code: string;
let name: string;

test.beforeEach(async ({ page, accountEx, salesBasePage }) => {
  await allure.parentSuite("Sales");
  await allure.suite("Settings");
  await allure.subSuite("Product unit (NEW)");
  await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/' + testCaseId, "Test case : " + testCaseId);
  envUrl = accountEx.baseUrl;
  await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, "role")?.toString() ?? "Admin");
  productUnitUtil = new ProductSettingsUtil(page, envUrl);
  code = "ProUnit" + Math.floor(100000 + Math.random() * 900000);
  name = "ProUName" + Math.floor(100000 + Math.random() * 900000);
  await productUnitUtil.createProductUnit(name, code);
});

test(testCaseId + ": Update/Delete product unit : DK", { tag: ["@Sales", "@Settings", "@Regression"] },
  async ({ salesBasePage, productUnitsPage }) => {
    {
      await salesBasePage.goToMenu("Settings_Sales", "Product Units", true, "Products");
      console.log("New Product unit is created (API)" + name);
      await productUnitsPage.filterByProductUnitName(name);
      await productUnitsPage.VerifyRowCount("1");
      await productUnitsPage.editProductUnit();
      await productUnitsPage.updateProductUnit(name + " EDIT");
      await salesBasePage.save();
      await salesBasePage.closeTab();
      await productUnitsPage.filterByProductUnitName(name + " EDIT");
      console.log("Product unit is edited successfully" + name);
      await productUnitsPage.editProductUnit();
      await productUnitsPage.deleteProductUnit();
      await productUnitsPage.filterByProductUnitName(name + " EDIT");
      await productUnitsPage.VerifyRowCount("0");
      console.log("Product unit is deleted successfully" + name);
    }
  }
);
