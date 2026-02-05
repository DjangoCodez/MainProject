import { errors, type Locator, type Page } from "@playwright/test";
import { BasePage } from "../../base.page";
import { ListAndFormPage } from '../../list-and-form.page';

export class EmployeeCategoryPage extends ListAndFormPage {
    readonly code: Locator;
    readonly name: Locator;
    readonly editGroupCombobox: Locator;
    readonly editUndercategoryCombobox: Locator;

    constructor (page: Page) {
        super(page, 'soe/time/employee/categories');
        this.code = this.page.locator('#Code');
        this.name = this.page.locator('#Name');
        this.editGroupCombobox = this.page.locator('#CategoryGroup');
        this.editUndercategoryCombobox = this.page.locator('#ParentCategory');
    }
} 