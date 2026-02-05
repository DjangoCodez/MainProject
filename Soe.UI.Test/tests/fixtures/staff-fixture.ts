import { mergeTests } from '@playwright/test';
import { test as admin } from './main-fixture';
import { test as sales } from './sales-po-fixture';
import { test as finance } from './finance-po-fixture';
import { test as staff } from './staff-po-fixture';
 
export const test = mergeTests(admin, sales, finance, staff);
export { expect } from '@playwright/test';
export { type Page } from '@playwright/test';