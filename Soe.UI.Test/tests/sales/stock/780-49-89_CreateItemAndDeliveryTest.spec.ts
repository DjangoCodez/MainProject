import { test } from '../../fixtures/sales-fixture';
import * as allure from "allure-js-commons";
import { getAccountExValue } from '../../../utils/properties';

let envUrl: string;
let productNumber: string = `${Math.floor(100 + Math.random() * 900)}_780-49-89`;
let productName: string = `AutoProduct_${Math.random().toString(36).substring(2, 7)}_780-49-89`;

test.beforeEach(async ({ accountEx, salesBasePage }) => {
    await allure.parentSuite("Sales");
    await allure.suite("Stock");
    await allure.subSuite("Stock");
    envUrl = accountEx.baseUrl;
    await salesBasePage.SetupSales(envUrl, getAccountExValue(accountEx, 'role')?.toString() ?? 'Admin');
});

test.describe.serial('Article in Stock and Delivery', () => {

    test('78049 : Create article in stock : SR', { tag: ['@Sales', '@Stock', '@Product', '@Regression'] }, async ({ salesBasePage, productsPageJS }) => {
        await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/78049', 'Test case : 78049');
        await salesBasePage.goToMenu('Product', 'Products');
        await productsPageJS.waitForPageLoad();
        await productsPageJS.createItem();
        await productsPageJS.waitforCreateProductPageLoad();
        await productsPageJS.setProductNumber(productNumber);
        await productsPageJS.setProductName(productName);
        await productsPageJS.expandPrice();
        await productsPageJS.setPurchasePrice('105');
        await productsPageJS.saveProduct();
        await productsPageJS.verifyPurchasePrice('105,00');
        await productsPageJS.expandStock();
        await productsPageJS.checkMarkAsStocked();
        await productsPageJS.clickAddRowInStock();
        await productsPageJS.setAveragePrice('500');
        await productsPageJS.saveProduct();
        await productsPageJS.verifyStockGridRowCount(1);
        await productsPageJS.verifyAveragePriceInStockGrid('500,00');
    });

    test('78089 : Stock in delivery : SR', { tag: ['@Sales', '@Stock', '@Product', '@Regression'] }, async ({ balancePage }) => {
        await allure.link('https://dev.azure.com/softonedev/XE/_workitems/edit/78089', 'Test case : 78089');
        await balancePage.goToMenu('Stock', 'Balance');
        await balancePage.waitforPageLoad();
        await balancePage.filterByProductNumber(productNumber);
        await balancePage.edit(0);
        await balancePage.selectType('Delivery');
        await balancePage.addQuantity('5');
        await balancePage.updatePrice('50');
        await balancePage.saveBalance();
        await balancePage.closeTab();
        await balancePage.clearAllFilters();
        await balancePage.releadRecords();
        await balancePage.waitforPageLoad();
        await balancePage.filterByProductNumber(productNumber);
        await balancePage.verifyInStock('5.00');
        await balancePage.verifyAveragePriceInGrid('50.00');
    });

});




