import { Page, type Locator } from '@playwright/test';

export class AgGridComponent {
	readonly page: Page;
	readonly gridLocator: Locator;

	constructor(page: Page, selector: string) {
		this.page = page;
		this.gridLocator = this.page.locator(selector);
	}

	getCell(row: number, column: number, additionalSelector?: string): Locator {
		return this.gridLocator.locator(
			'grid-value'
			//@ts-ignore
			//agUtility.getLocatorForCell(row, column, additionalSelector)
		);
	}
}
