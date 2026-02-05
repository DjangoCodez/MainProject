import { Page } from '@playwright/test';
import { GridPageJS } from './GridPageJS';

export class SingleGridPageJS extends GridPageJS {
    constructor(page: Page, tabIndex: number = 0, agGrid: string = 'ctrl.gridAg.options.gridOptions') {
        super(page, agGrid, false, '', tabIndex);
    }

    // Add SingleGridPageJS specific methods and properties here
}