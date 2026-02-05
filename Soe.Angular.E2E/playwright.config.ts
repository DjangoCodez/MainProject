import { defineConfig, devices, expect } from '@playwright/test';
import path from 'path';
import dotenv from 'dotenv';
import * as customMatchers from './custom-matchers';

dotenv.config();

/**
 * Read environment variables from file.
 * https://github.com/motdotla/dotenv
 */
// require('dotenv').config();

/**
 * See https://playwright.dev/docs/test-configuration.
 */

expect.extend(customMatchers);

export default defineConfig({
	testDir: './tests',
	/* Run tests in files in parallel */
	fullyParallel: true,
	/* Fail the build on CI if you accidentally left test.only in the source code. */
	forbidOnly: !!process.env.CI,
	/* Retry on CI only */
	retries: process.env.CI ? 2 : 0,
	/* Opt out of parallel tests on CI. */
	workers: process.env.CI ? 1 : undefined,
	timeout: process.env.CI ? 60000 : 30000,
	/* Reporter to use. See https://playwright.dev/docs/test-reporters */
	reporter: 'html',
	globalSetup: './global-setup',
	globalTeardown: './global-teardown',
	/* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
	use: {
		/* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
		trace: 'on-first-retry'
	},
	expect: {
		timeout: 30 * 1000
	},

	/* Configure projects for major browsers */
	projects: [
		// {
		// 	name: 'setup',
		// 	testMatch: /global.setup.ts/
		// },
		{
			name: 'chromium',
			use: {
				...devices['Desktop Chrome']
				// storageState: storage_state()
			}
		}
		// {
		// 	name: 'teardown',
		// 	testMatch: /global\.teardown\.ts/
		// }
		/* Test against branded browsers. */
		// {
		//   name: 'Microsoft Edge',
		//   use: { ...devices['Desktop Edge'], channel: 'msedge' },
		// },
		// {
		//   name: 'Google Chrome',
		//   use: { ...devices['Desktop Chrome'], channel: 'chrome' },
		// },
	]
});
