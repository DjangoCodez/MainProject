import { selectors, type Locator, type Page } from '@playwright/test';
import { test, expect } from '../../utils/main-fixture.ts';

export class BasePage {
	readonly endpoint: string;
	readonly domain: string = 'devs1d1.softone.se';

	readonly page: Page;
	readonly angularButton: Locator;
	readonly rightMenu: Locator;

	//readonly reloadButton: Locator;


	private get fullEndpoint(): string {
		return `https://${this.domain}/${this.endpoint}/default.aspx`;
	}

	constructor(page: Page, endpoint: string) {
		this.page = page;
		this.endpoint = endpoint;

		this.angularButton = this.page.locator('.fab.fa-angular');
		this.rightMenu = this.page.locator('#information-menu-toggle');

		//this.reloadButton = this.page.locator('.fa-arrows-rotate');
	}

	async switchToAngular() {
		await this.angularButton.click();
	}

	async navigateTo() {
		const navigate = async () => {
			await this.page.goto(this.fullEndpoint);
			await this.rightMenu.isEnabled();
		};

		await navigate();

		if (!this.page.url().includes(this.endpoint)) {
			navigate();
		}
	}
	async getNumbersFromString(input: string) {
		// Returns array with all numbers in string
		var num = input.replace(/[^0-9]/g, '');
		return num
	}
	async getNumbersFromSelector(selector: Locator) {
		await selector.waitFor({ state: 'visible' });
		const textContent = await selector.textContent();
		if (textContent !== null && textContent !== undefined) {
			const text : string = textContent;
			const num = await this.getNumbersFromString(text); 
			const number : number = Number(num);
			return number;
		}
		return 0;
	}
	

	
}
