# SOE.UI.TEST
Playwright Test Automation Project.

The UI test automation project for **SoftOne Go**.

<br>

# Setup

**Prerequisites:**  
Install Node.js latest (18 +)

**Do following steps on terminal**

1. Git clone the project https://softonedev@dev.azure.com/softonedev/XE/_git/Main
2. Move to folder 'Main'.
3. Execute script in '.\NewSource\Soe.UI.Test\scripts\CopyWebApiModels.ps1'.
4. Run 'npm install'
5. Run 'npx playwright install'
6. Run 'npx playwright test' to run the tests.
7. After the test execution fininsh run 'allure serve allure-results' , allure results will open in browser.

IMPORTANT : <br>
1. Better remmove test-results folder before each run if execution not start ( Uncomment the code "Remove test-results" in 'playwright.config.ts'  ).<br>
2. Auth context is saved in .auth/{username}.json. Remove if session is expired. 

<br>

# Configurations

TBA

<br>



# Run With VSCode
https://playwright.dev/docs/getting-started-vscode

1. Open the project with VScode
2. Select the "Show browser" and 'UAT' option from left nav and run the test



