import { defineConfig, devices } from '@playwright/test';
import dotenv from "dotenv";
// import path from 'path';
// import fs from 'fs';

const username = process.env.BROWSERSTACK_USERNAME;
const accessKey = process.env.BROWSERSTACK_ACCESS_KEY;

//process.env.ENV = 'stage';

if (process.env.ENV) {
  console.log('./env/.env.' + process.env.ENV)
  dotenv.config({
    path: './env/.env.' + process.env.ENV,
  });
} else {
  dotenv.config({
    path: './env/.env',
    override: true,
  }
  );
}

//  dotenv.config({
//     path: './env/.env.' + 'prod',
//   });

if (!username || !accessKey) {
  console.warn("Warning: BrowserStack credentials are not set in the environment. BrowserStack tests will be skipped.");
}

const caps = {
  browser: 'chrome',
  os: 'osx',
  os_version: 'Catalina',
  name: 'Playwright Regression Test',
  build: 'Playwright-regression-build',
  'browserstack.username': username,
  'browserstack.accessKey': accessKey,
};

const wsEndpoint = `wss://cdp.browserstack.com/playwright?caps=${encodeURIComponent(JSON.stringify(caps))}`;


// Remove test-results
// const resultsDir = path.join(__dirname, 'test-results');
// if (fs.existsSync(resultsDir)) {
//   fs.rmSync(resultsDir, { recursive: true, force: true });
//   console.log('test-results folder was removed.');
// }

/**
 * Read environment variables from file.
 * https://github.com/motdotla/dotenv
 */
// import dotenv from 'dotenv';
// import path from 'path';
// dotenv.config({ path: path.resolve(__dirname, '.env') });

/**
 * See https://playwright.dev/docs/test-configuration.
 */
export default defineConfig({
  testDir: './tests',
  timeout: process.env.CI ? 150_000 : 150_000,
  /* Run tests in files in parallel */
  fullyParallel: true,
  /* Fail the build on CI if you accidentally left test.only in the source code. */
  forbidOnly: !!process.env.CI,
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  /* Opt out of parallel tests on CI. */
  workers: process.env.CI ? 2 : 1,
  /* Reporter to use. See https://playwright.dev/docs/test-reporters */
  //reporter: 'html',
  maxFailures: process.env.CI ? 25 : undefined,
  reporter: [
    ["line"],
    [
      "allure-playwright",
      {
        resultsDir: "allure-results", detail: true
      },
    ],
  ],

  /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
  use: {
    /* Base URL to use in actions like `await page.goto('/')`. */
    // baseURL: 'http://127.0.0.1:3000',

    /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
    screenshot: 'only-on-failure',
    trace: 'off',
    video: 'off',
    actionTimeout: 20_000
  },


  /* Configure projects for major browsers */
  projects: [
    {
      name: 'pipeline-chrome',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'browserstack-chrome',
      use: {
        browserName: 'chromium',
        connectOptions: {
          wsEndpoint,
        },
      },
    },
    // {
    //   name: 'firefox',
    //   use: { ...devices['Desktop Firefox']},
    // },

    // {
    //   name: 'webkit',
    //   use: { ...devices['Desktop Safari'] },
    // },

    /* Test against mobile viewports. */
    // {
    //   name: 'Mobile Chrome',
    //   use: { ...devices['Pixel 5'] },
    // },
    // {
    //   name: 'Mobile Safari',
    //   use: { ...devices['iPhone 12'] },
    // },

    /* Test against branded browsers. */
    // {
    //   name: 'Microsoft Edge',
    //   use: { ...devices['Desktop Edge'], channel: 'msedge' },
    // },
    // {
    //   name: 'Google Chrome',
    //   use: { ...devices['Desktop Chrome'], channel: 'chrome' },
    // },
  ],

  /* Run your local dev server before starting the tests */
  // webServer: {
  //   command: 'npm run start',
  //   url: 'http://127.0.0.1:3000',
  //   reuseExistingServer: !process.env.CI,
  // },

});
