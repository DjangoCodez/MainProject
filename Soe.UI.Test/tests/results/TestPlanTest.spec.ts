import { test } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';

let testCaseId: string = 'Regression';
let patToken = '';



test(testCaseId + ': TEST PLAN UPDATE', { tag: ['@Results'] }, async ({ page }) => {

  patToken = process.env.PAT_TOKEN ?? '';

  // Type for the expected structure of the file
  interface Label {
    name: string;
    value: string;
  }

  interface TestResult {
    name?: string;
    labels?: Label[];
    status?: string; // Add the status property to match the expected structure
  }

  interface ExtractedInfo {
    subSuite?: string;
    testMethod?: string;
    testId?: string;
    suiteId?: number;
    testPointId?: number | null;
    outcome?: string;
  }

  // Go up two directories to get to project root, then to allure-results
  const allureResultsFolder = process.env.ALLURE_HISTORY_PATH
    ? path.resolve(process.env.ALLURE_HISTORY_PATH)
    : path.resolve(__dirname, '..', '..', 'allure-results'); // fallback

  const files = fs.readdirSync(allureResultsFolder);
  const resultFiles = files.filter(file =>
    file.endsWith('-result.json')
  );
  let resultFilesUnique;
  const latestByTest = new Map();
  if (resultFiles.length === 0) {
    console.error('No matching result files found in allure-results.');
    process.exit(1);
  } else {
    //filter latest when have multiple
    for (const f of resultFiles) {
      const full = path.join(allureResultsFolder, f);
      const stat = fs.statSync(full);

      let json;
      try {
        json = JSON.parse(fs.readFileSync(full, 'utf8'));
      } catch (e) {
        console.warn(`Skipping unreadable ${f}: ${(e as Error).message}`);
        continue;
      }

      // Stable key for "same test" across retries
      const key =
        json.historyId ||
        json.fullName ||
        json.name ||
        f.replace(/-result\.json$/, '');

      // Prefer Allure timestamps; fall back to file mtime
      const stop = json.stop ?? json.time?.stop ?? 0;
      const start = json.start ?? json.time?.start ?? 0;
      const ts = stop || start || stat.mtimeMs;

      const prev = latestByTest.get(key);
      if (!prev || ts > prev.ts) {
        latestByTest.set(key, { ts, file: full, json });
      }
    }

    // If you only want file paths:
    resultFilesUnique = Array.from(latestByTest.values()).map(v => v.file);

  }

  const results: ExtractedInfo[] = [];

  for (const file of resultFilesUnique) {
    const s = String(file).trim();
    const fileName: string = s.split(/[\\/]/).filter(Boolean).pop() ?? '';
    const filePath = path.join(allureResultsFolder, fileName);
    const content = fs.readFileSync(filePath, 'utf-8');

    try {
      const result: TestResult = JSON.parse(content);

      const subSuite = result.labels?.find(label => label.name === 'subSuite')?.value;
      let testMethod = result.labels?.find(label => label.name === 'testMethod')?.value;
      if (!testMethod) {
        testMethod = result.name?.toString();
      }
      const testIdMatch = testMethod?.match(/_(\d+)$/);
      let testId = testIdMatch ? testIdMatch[1] : undefined;
      if (!testId) {
        const testIdMatchPlay = testMethod?.match(/^(\d+):/);
        testId = testIdMatchPlay ? testIdMatchPlay[1] : undefined;
      }
      if (testId) {
        testId = testId.trim();
      }
      const outcome = result.status === 'passed' || result.status === 'failed' ? result.status : 'notExecuted';
      const isFailUpdate: boolean = process.env.isFailUpdate === 'True' || process.env.test_plan_id === '1';

      if (outcome === 'failed' && isFailUpdate) {
        results.push({ subSuite, testMethod, testId, outcome });
      }
      if (outcome === 'passed') {
        results.push({ subSuite, testMethod, testId, outcome });
      }
    } catch (error) {
      console.warn(`Skipping invalid JSON file: ${file}`);
    }
  }
  // Output result Debug
  // console.log('Extracted subSuite and testMethod values:');
  // results.forEach((r, index) => {
  //   // console.log(`Result ${index + 1}:`);
  //   // console.log(`  subSuite: ${r.subSuite}`);
  //   console.log(`  testMethod: ${r.testMethod}`);
  //   console.log(`  testId: ${r.testId}`);
  //   console.log(`  outcome: ${r.outcome}`);
  // });

  const response = await page.request.get(`https://dev.azure.com/softonedev/XE/_apis/testplan/Plans/${process.env.test_plan_id}/suites?api-version=7.1`, {
    headers: {
      Authorization: `Basic ${Buffer.from(':' + patToken).toString('base64')}`
    }
  });

  // Check if the response is OK
  if (!response.ok()) {
    throw new Error(`Failed to fetch test suites: ${response.status()} ${response.statusText()}`);
  }

  const responseData = await response.json();
  // console.log('Data:', JSON.stringify(responseData));

  const enrichedResults = results.map(result => {
    const suiteMatch = responseData.value.find(
      (suite: { name: string }) => suite.name === result.subSuite
    );
    return {
      ...result,
      suiteId: suiteMatch?.id
    };
  });

  // Log enriched results
  // console.log('Final results with suiteId:');
  // enrichedResults.forEach((r, index) => {
  //   console.log(`  testMethod: ${r.testMethod}`);
  //   console.log(`  testId: ${r.testId}`);
  //   console.log(`  outcome: ${r.outcome}`);
  // });


  for (const result of enrichedResults) {
    if (!result.suiteId || !result.testId) {
      console.warn(`Skipping result due to missing suiteId or testId:`, result);
      continue;
    }

    const testPointUrl = `https://dev.azure.com/softonedev/XE/_apis/test/Plans/${process.env.test_plan_id}/Suites/${result.suiteId}/points?api-version=7.1-preview.2&testCaseId=${result.testId}`;

    const testPointResponse = await page.request.get(testPointUrl, {
      headers: {
        Authorization: `Basic ${Buffer.from(':' + patToken).toString('base64')}`
      }
    });

    if (!testPointResponse.ok()) {
      console.error(`Failed to fetch test point for suiteId ${result.suiteId} and testId ${result.testId}: `, testPointResponse.status());
      continue;
    }

    const testPointData = await testPointResponse.json();
    result.testPointId = testPointData.value?.[0]?.id ?? null;
  }

  // Log final results with testPointId - Debug
  // console.log('Final enriched results with testPointId:');
  // enrichedResults.forEach((r, index) => {
  //   console.log(`Result ${index + 1}:`);
  //   console.log(`  subSuite: ${r.subSuite}`);
  //   console.log(`  testMethod: ${r.testMethod}`);
  //   console.log(`  testId: ${r.testId}`);
  //   console.log(`  suiteId: ${r.suiteId}`);
  //   console.log(`  testPointId: ${r.testPointId ?? 'Not found'}`);
  //   console.log(`  outcome: ${r.outcome}`);
  // });

  // Filter out results with null testPointId or undefined outcome
  // and map to the desired payload structure
  // Group enrichedResults by suiteId
  const groupedBySuiteId: { [suiteId: number]: ExtractedInfo[] } = {};

  for (const result of enrichedResults) {
    if (result.suiteId && result.testPointId !== null && result.outcome !== undefined) {
      if (!groupedBySuiteId[result.suiteId]) {
        groupedBySuiteId[result.suiteId] = [];
      }
      groupedBySuiteId[result.suiteId].push(result);
    }
  }

  // Send PATCH request per suiteId
  for (const suiteIdStr in groupedBySuiteId) {
    const suiteId = parseInt(suiteIdStr, 10);
    const resultsGroup = groupedBySuiteId[suiteId];

    const payload = resultsGroup.map(r => ({
      id: r.testPointId,
      results: {
        outcome: r.outcome
      }
    }));

    const patchUrl = `https://dev.azure.com/softonedev/XE/_apis/testplan/plans/${process.env.test_plan_id}/suites/${suiteId}/testpoint?api-version=7.1-preview.2`;

    const patchResponse = await page.request.patch(patchUrl, {
      headers: {
        Authorization: `Basic ${Buffer.from(':' + patToken).toString('base64')}`,
        'Content-Type': 'application/json'
      },
      data: JSON.stringify(payload)
    });

    if (!patchResponse.ok()) {
      console.log(payload);
      console.error(`❌ Failed to update test points for suiteId ${suiteId}: ${patchResponse.status()} ${patchResponse.statusText()}`);
    } else {
      console.log(`✅ Successfully updated test points for suiteId ${suiteId}`);
    }
  }


});