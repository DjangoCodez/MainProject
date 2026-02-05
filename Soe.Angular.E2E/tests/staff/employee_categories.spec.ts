import { BasePage } from "../../ui/pages/base.page.ts";
import { test, expect } from '../../utils/main-fixture.ts';
import { UserPage } from "../../ui/pages/manage/user.page.ts";
import { EmployeeCategoryPage } from "../../ui/pages/Staff/Employee/employee_category.page.ts";

test.use({
	account: {
		userName: 'seleniumsys237',
		password: 'Summer2022',
		domain: 's1d1'
	}
});

test('create employee_category and delete it', async ({ page }) => {
	var name: string = 'Markus';
	var code: string = '12345'
	

    const employeeCategoryPage = new EmployeeCategoryPage(page);
    await employeeCategoryPage.navigateTo();
	//const rowAmount : number = await employeeCategoryPage.getTotalPostsAmount();
    await employeeCategoryPage.addNew();
	await employeeCategoryPage.setInput(employeeCategoryPage.name, name);
	await employeeCategoryPage.setInput(employeeCategoryPage.code, code);
	await employeeCategoryPage.editUndercategoryCombobox.selectOption( { index: 1})
	await employeeCategoryPage.clickSubmit();
	await employeeCategoryPage.navigateTo();
	//const newRowAmount : number = await employeeCategoryPage.getTotalPostsAmount();
	//await employeeCategoryPage.validatePostAmount(rowAmount + 1);
	await employeeCategoryPage.filterSearch('code', code);
	await employeeCategoryPage.filterSearch('name', name);
	//await employeeCategoryPage.validateVisibleRowAmount();
	expect(await employeeCategoryPage.getRowName()).toBe(name);
	expect(await employeeCategoryPage.getRowCode()).toBe(code);
	await employeeCategoryPage.clickRow();
	await employeeCategoryPage.clickDelete();
	await employeeCategoryPage.navigateTo();

	//await employeeCategoryPage.validatePostAmount(rowAmount);
});


test('search code emp cat', async ({ page }) => {
	

    const employeeCategoryPage = new EmployeeCategoryPage(page);
    await employeeCategoryPage.navigateTo();
	await employeeCategoryPage.filterSearch('code', 'hello');
    
});