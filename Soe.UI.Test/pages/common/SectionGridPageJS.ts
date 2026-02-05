import { Page } from '@playwright/test';
import { GridPageJS } from './GridPageJS';

export class SectionGridPageJS extends GridPageJS {
    constructor(page: Page, labelKey: string = '', agGrid: string = 'directiveCtrl.soeGridOptions.gridOptions', tabIndex: number = 0) {
        super(page, agGrid, false, labelKey, tabIndex);
    }

    // Add SectionGridPageJS specific methods and properties here
}