import { type Locator, type Page } from '@playwright/test';
import { BasePage } from '../base.page';
import { AgGridComponent } from '../../components/ag-grid.component';
import { ListAndFormPage } from '../list-and-form.page';

export class VatPage extends ListAndFormPage {


	constructor(page: Page) {
		super(page, 'soe/economy/preferences/vouchersettings/vatcodes');
	}

}