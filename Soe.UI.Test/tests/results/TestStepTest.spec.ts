import test from "@playwright/test";
import { checkTestCaseStepsUpdated, getTestIdsFromTestFilesRecursive } from "./TestStepCheck";
import * as path from 'path';
import * as fs from 'fs';

let testCaseId: string = 'Regression';
let patToken = '';

test(testCaseId + ': TEST STEP CHECK', { tag: ['@TestStepCheck'] }, async () => {
  patToken = process.env.PAT_TOKEN ?? '';
  const testFolder = path.join(__dirname, '../');
  const testIds = await getTestIdsFromTestFilesRecursive(testFolder);

  const ORG = "softonedev"; 
  const PROJECT = "XE";   
  const PAT = patToken;
  const now = new Date();
  const oneWeekAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
  let sinceIso = oneWeekAgo.toISOString();
  console.log('Checking for test step changes since: ' + sinceIso);

  const updatedTest: Record<string, string> = {};

  for (const TEST_CASE_ID of testIds) {
    const result = await checkTestCaseStepsUpdated(ORG, PROJECT, TEST_CASE_ID, PAT, {
      sinceIso,
    });
    if (result.stepsChangedSinceGivenTime) {
      if (result.stepsLastChangedAt !== null) {
        updatedTest[TEST_CASE_ID] = result.stepsLastChangedAt;
      }
    }
  }

  let output: { updatedTest: Record<string, string> } | { message: string } = { updatedTest };
  if (Object.keys(updatedTest).length === 0) {
    output = { message: 'No test steps updated in the last week.' };
  }

  console.log('Output:', JSON.stringify(output, null, 2));
  fs.writeFileSync(
    path.join(__dirname, '../../updatedTest.json'),
    JSON.stringify(output, null, 2),
    'utf-8'
  );

});