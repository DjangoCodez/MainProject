import * as fs from 'fs';
import * as path from 'path';

type WitRevision = {
  id: number;
  rev: number;
  fields: Record<string, any>;
};

type StepsChangeResult = {
  testCaseId: number;
  stepsLastChangedRevision: number | null;
  stepsLastChangedAt: string | null; // ISO
  stepsChangedSinceGivenTime: boolean | null;
};

const ADO_API_VERSION = "7.1";

/** Build the base URL like: https://dev.azure.com/{org}/{project}/_apis */
const baseUrl = (org: string, project: string) =>
  `https://dev.azure.com/${encodeURIComponent(org)}/${encodeURIComponent(project)}/_apis`;

const authHeader = (pat: string) =>
  "Basic " + Buffer.from(`:${pat}`).toString("base64");

/**
 * Fetch all revisions for a Work Item (paged with $top/$skip).
 * We only need the fields bag, so no need for $expand=All.
 */
async function getWorkItemRevisions(
  org: string,
  project: string,
  workItemId: number,
  pat: string
): Promise<WitRevision[]> {
  const headers = { Authorization: authHeader(pat) };
  const all: WitRevision[] = [];
  let skip = 0;
  const top = 200;

  while (true) {
    const url =
      `${baseUrl(org, project)}/wit/workItems/${workItemId}/revisions` +
      `?$top=${top}&$skip=${skip}&api-version=${ADO_API_VERSION}`;
    const res = await fetch(url, { headers });
    if (!res.ok) {
      const text = await res.text();
      throw new Error(`Revisions fetch failed: ${res.status} ${res.statusText} — ${text}`);
    }
    const json = await res.json() as { value: WitRevision[] };
    const batch = json.value ?? [];
    all.push(...batch);
    if (batch.length < top) break;
    skip += top;
  }
  return all;
}

/**
 * Given a list of revisions, find the last revision where Steps changed.
 * Steps field name: "Microsoft.VSTS.TCM.Steps" (XML string).
 */
function findStepsLastChange(revisions: WitRevision[]) {
  const fieldName = "Microsoft.VSTS.TCM.Steps";
  let prev: string | undefined = undefined;
  let lastChangedRev: number | null = null;
  let lastChangedAt: string | null = null;

  for (const rev of revisions) {
    const steps: string | undefined = rev.fields?.[fieldName];
    if (prev === undefined) {
      prev = steps;
      continue;
    }
    if (steps !== prev) {
      lastChangedRev = rev.rev;
      lastChangedAt = rev.fields?.["System.ChangedDate"] ?? null;
      prev = steps;
    }
  }
  return { lastChangedRev, lastChangedAt };
}

/**
 * Main helper: checks if Steps changed since a given ISO timestamp (optional).
 */
export async function checkTestCaseStepsUpdated(
  org: string,
  project: string,
  testCaseId: number,
  pat: string,
  options?: { sinceIso?: string }
): Promise<StepsChangeResult> {
  const revisions = await getWorkItemRevisions(org, project, testCaseId, pat);
  if (!revisions.length) {
    throw new Error("No revisions found for this work item.");
  }

  const { lastChangedRev, lastChangedAt } = findStepsLastChange(revisions);

  let changedSince: boolean | null = null;
  if (options?.sinceIso && lastChangedAt) {
    const since = new Date(options.sinceIso);
    const last = new Date(lastChangedAt);
    changedSince = last >= since;
  }

  return {
    testCaseId,
    stepsLastChangedRevision: lastChangedRev,
    stepsLastChangedAt: lastChangedAt,
    stepsChangedSinceGivenTime: changedSince,
  };
}


export async function getTestIdsFromTestFilesRecursive(dir: string): Promise<number[]> {
  let testIds: number[] = [];
  const files = fs.readdirSync(dir);

  for (const file of files) {
    const fullPath = path.join(dir, file);
    const stat = fs.statSync(fullPath);

    if (stat.isDirectory()) {
      // Recursively search subdirectories
      testIds = testIds.concat(await getTestIdsFromTestFilesRecursive(fullPath));
    } else {
      // Match filenames like 77734_something.spec.ts
      const match = file.match(/^(\d+)_/);
      if (match) {
        testIds.push(Number(match[1]));
      }
    }
  }
  return testIds;
}
