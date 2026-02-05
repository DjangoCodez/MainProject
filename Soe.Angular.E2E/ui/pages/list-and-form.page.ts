import { type Locator, type Page } from '@playwright/test';
import { BasePage } from './base.page';
import { AgGridComponent } from '../components/ag-grid.component';
import { test, expect } from '../../utils/main-fixture.ts';

export class ListAndFormPage extends BasePage {
	readonly reloadButton: Locator;
	readonly grid: AgGridComponent;
	readonly upperLeftCell: Locator;
	readonly showInactiveCheckbox: Locator;
	readonly addNewButton: Locator;
	readonly editButton: Locator;
	readonly submitButton: Locator;
    readonly deleteButton: Locator;
	readonly filterParent: Locator;
	readonly rowTotalAmountLabel: Locator;
	readonly rowFilteredAmountLabel: Locator;
	

	constructor(page: Page, endpoint: string) {
		super(page, endpoint);
		//this.reloadButton = this.page.locator('.fa-arrows-rotate');
		this.showInactiveCheckbox = this.page.locator('#ctrl_showInactive')
		this.reloadButton = this.page.locator('btn.btn-default.fal.fa-sync') //Needs better locator
		//this.reloadButton = this.page.getByRole('toolbar').getByTitle('Hämta om poster') //This works
		this.grid = new AgGridComponent(this.page, 'soe-grid');
		this.upperLeftCell = this.grid.getCell(0, 0);
		this.addNewButton = this.page.locator('.fal.fa-plus.addTabIcon') // find with class
		this.editButton = this.page.locator('.fal.fa-pencil.iconEdit')
		this.submitButton = this.page.locator('#submit')
        this.deleteButton = this.page.locator('#deletepost')
		this.rowTotalAmountLabel = this.page.locator('.soe-ag-totals-row-part.soe-ag-grid-totals-all-count')
		this.rowFilteredAmountLabel = this.page.locator('.soe-ag-totals-row-part.soe-ag-grid-totals-filtered-count')
	}
	async reload() {
		//await this.reloadButton.isEnabled();
		//await this.reloadButton.waitFor({ state: 'visible' });
    	await this.reloadButton.isEnabled();
		await this.reloadButton.click();
	}
	async toggleShowInactive() {
		await this.showInactiveCheckbox.isEnabled();
		await this.showInactiveCheckbox.click();
	}
	async getTotalPostsAmount() {
		const amount = await this.getNumbersFromSelector(this.rowTotalAmountLabel);
		return amount
	}
	async getFilteredPostsAmount(): Promise<number> {
		const amount = await this.getNumbersFromSelector(this.rowFilteredAmountLabel);
		return amount
	}
	async validatePostAmount(n : number) {
		var total = await this.getTotalPostsAmount();
		expect(total).toBe(n);
	}
	async addNew() {
		await this.addNewButton.isEnabled();
		await this.addNewButton.click();
	}
	// async getVisibleRowAmount() {
	// 	await this.editButton.waitFor( { state : 'visible'});
	// 	const buttonCount = await this.editButton.count();
	// 	return buttonCount;
	// }
	async validateVisibleRowAmount(n : number = 1) {
		var filtered = await this.getFilteredPostsAmount();
		expect (filtered).toBe(n);
	}
	async clickRow(n: number = 0) {
		const nthEditButton = this.editButton.nth(n);
		await nthEditButton.isEnabled();
		await nthEditButton.click();
	}
	async clickSubmit() {
        await this.submitButton.isEnabled();
        await this.submitButton.click();
    }
    async clickDelete() {
        await this.deleteButton.isEnabled();
        await this.deleteButton.click();
    }
	async setInput(selector: Locator, value : string) {
		await selector.isEnabled();
		await selector.fill(value);
		await expect(selector).toHaveValue(value);
	}
	async getFilterSelector(type : string) : Promise<Locator> {
		var header = this.page.locator('.ag-header-row.ag-header-row-column').locator(`[col-id="${type}"]`);
		await header.isVisible();
		var colIndex = await header.getAttribute('aria-colindex');
		var filterParent = this.page.locator(`.ag-header-cell.ag-floating-filter.ag-focus-managed[aria-colindex="${colIndex}"]`)
		var filterSelector = filterParent.locator('.ag-input-field-input.ag-text-field-input')
		return filterSelector
	}
	async filterSearch(type : string, input : string) {
		var filterSelector = await this.getFilterSelector(type);
		await filterSelector.isEnabled();
		await filterSelector.fill(input);
		await expect(filterSelector).toHaveValue(input);
	}
	async openAccordion(selector: Locator) {
		const accordionOpen: Locator = selector.locator('.fal.fa-chevron-down')
		const accordionClose: Locator = selector.locator('.fal.fa-chevron-up')
		await selector.isEnabled();
		if (await accordionOpen.isVisible()) {
			await selector.click();
			await expect(accordionClose).toBeVisible();
		}
	}
	async closeAccordion(accordionLocator: Locator) {
		const accordionOpen: Locator = accordionLocator.locator('.fal.fa-chevron-down')
		const accordionClose: Locator = accordionLocator.locator('.fal.fa-chevron-up')
		await accordionLocator.isEnabled();
		if (await accordionClose.isVisible()) {
			await accordionLocator.click();
			await expect(accordionOpen).toBeVisible();
		}
	}
}
