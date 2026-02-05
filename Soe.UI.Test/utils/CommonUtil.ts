import { TestInfo } from '@playwright/test';
import { AngVersion } from '../enums/AngVersionEnums';
import { getEnvironmentValue, getPages } from '../utils/properties';
import { PdfReader } from 'pdfreader';


/**
 * @param days - (number): The number of days to add or subtract from the current date.
 * @param add -(boolean, optional): Determines whether to add or subtract days.
 * @returns * - A string representing the calculated date in MM/DD/YYYY format
 * @description - This function calculates a date based on the current date by adding or subtracting a given number of days.
 */

export async function getDateUtil(days: number, add = false) {
    const currentDate = new Date();

    if (add) {
        currentDate.setDate(currentDate.getDate() + days); // Add days
    } else {
        currentDate.setDate(currentDate.getDate() - days); // Subtract days
    }

    const month = String(currentDate.getMonth() + 1).padStart(2, '0'); // 0-based month
    const day = String(currentDate.getDate()).padStart(2, '0');
    const year = currentDate.getFullYear();
    return `${month}/${day}/${year}`;
}

export async function getCurrentDateUtilWithFormat() {
    const today = new Date();
    const mm = String(today.getMonth() + 1).padStart(2, '0');
    const dd = String(today.getDate()).padStart(2, '0');
    const yyyy = today.getFullYear();
    const formattedDate = `${mm}/${dd}/${yyyy}`;
    return formattedDate;
}

/**
 * @returns - A string representing the 1st date of current month in MM/DD/YYYY format
 */
export async function getFirstDateOfCurrentMonth() {
    const currentDate = new Date();
    const firstDay = new Date(currentDate.getFullYear(), currentDate.getMonth(), 1);
    const month = String(firstDay.getMonth() + 1).padStart(2, '0'); // 0-based month
    const day = String(firstDay.getDate()).padStart(2, '0');
    const year = firstDay.getFullYear();
    return `${month}/${day}/${year}`;
}

/**
 * @returns- A string representing the last date of current month in MM/DD/YYYY format
 */

export async function getLastDateOfCurrentMonth() {
    const currentDate = new Date();
    const lastDay = new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 0);
    const month = String(lastDay.getMonth() + 1).padStart(2, '0'); // 0-based month
    const day = String(lastDay.getDate()).padStart(2, '0');
    const year = lastDay.getFullYear();
    return `${month}/${day}/${year}`;
}

/**
 * @returns- Returns version for given page
 */
export async function getVersion(pageName: string) {
    const ang_run_version: string = getEnvironmentValue('angular_run_version') ?? 'release';
    return getPages(ang_run_version).some(item => item.includes(pageName + 'JS')) ? AngVersion.JS : AngVersion.NEW;
}

/**
 * @returns - A string representing the current date in YYMMDD format
 */
export function getFormatYYMMDD(): string {
    const currentDate = new Date();
    const year = String(currentDate.getFullYear()).slice(-2);
    const month = String(currentDate.getMonth() + 1).padStart(2, '0');
    const day = String(currentDate.getDate()).padStart(2, '0');
    return `${year}${month}${day}`;
}


export const generateRandomId = (testInfo: TestInfo,testCaseId?: string): number => {
    const id =
        testInfo.workerIndex * 1_000 +
        ((Date.now() % 1_000_000) * 1000) +
        Math.floor(Math.random() * 1000) + (testCaseId ? parseInt(testCaseId) : 0);
    return id;
}

export const getFormattedDateMMDDYY = (date: Date = new Date()) => {
    let month = (1 + date.getMonth()).toString().padStart(2);
    let day = date.getDate().toString().padStart(2, '');
    let year = date.getFullYear().toString().slice(-2);
    return `${month}/${day}/${year}`.trim();
}

/**
 * @param dateString - A string representing a date in MM/DD/YYYY format
 * @returns - A string representing the date in ISO format (YYYY-MM-DDTHH:mm:ss)
 */
export function convertToISOStringDate(dateString: string): string {
    const [month, day, year] = dateString.split('/');
    return `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}T00:00:00`;
}

/**
 * Extracts the numeric total value from a PDF containing a line like: "Total -272 257 233,99"
 * 
 * @param pdfPath - The absolute or relative path to the PDF file
 * @param regexToGetValue - example /Total\s-?([\d\s]+,\d{2})/
 * @returns - The extracted PDF value as a string (e.g., "272 257 233,99"), or null if not found
 */
export async function getValueFromPDF(pdfPath: string, regex: RegExp): Promise<string | null> {
    const text = await extractWithPdfReader(pdfPath);
    const m = text.match(regex);
    return m ? m[1] : null;
}

type TextItem = { text: string; x: number; y: number };
type AnyItem = | TextItem | { page?: number; width?: number; height?: number } | { text?: string; file?: { path?: string; buffer?: string } } | null;

function isTextItem(i: AnyItem): i is TextItem {
    return !!i && typeof (i as any).text === "string" && typeof (i as any).x === "number" && typeof (i as any).y === "number";
}

/**
 * Universal layout stitcher (no keywords):
 * - Dynamic LINE_EPS from local y-deltas (prevents near-overlap merges)
 * - Backtrack break when X decreases by > BACKTRACK_EPS (new row)
 * - Gap-based spacing (space/tab) purely from ΔX
 */
function stitchWithLayout(items: TextItem[], { gapSpace = 3, gapTab = 20, yDeltaCap = 20, backtrackEps = 6 }: { gapSpace?: number; gapTab?: number; yDeltaCap?: number; backtrackEps?: number; } = {}): string {
    if (!items.length) return "";

    // 🔁 Sort top→bottom (Y asc in pdfreader), then left→right (X asc)
    items.sort((a, b) => {
        const dy = a.y - b.y;
        if (Math.abs(dy) > 0.5) return dy;
        return a.x - b.x;
    });

    // Dynamic line threshold from local Y deltas
    const dYs: number[] = [];
    for (let i = 1; i < items.length; i++) {
        const dy = Math.abs(items[i].y - items[i - 1].y);
        if (dy > 0 && dy < yDeltaCap) dYs.push(dy);
    }
    const base = dYs.sort((x, y) => x - y)[Math.floor((dYs.length - 1) * 0.4)] || 3;
    const LINE_EPS = Math.max(2, Math.min(8, base));

    const out: string[] = [];
    let curY = Number.NEGATIVE_INFINITY;
    let prevX = 0;
    let line = "";

    for (const it of items) {
        const { x, y, text } = it;

        const startsNewLine =
            curY === Number.NEGATIVE_INFINITY ||
            Math.abs(y - curY) > LINE_EPS ||
            x < prevX - backtrackEps;

        if (startsNewLine) {
            if (line) out.push(line);
            line = "";
            curY = y;
            prevX = x;
        }

        const dx = x - prevX;
        if (dx > gapTab) line += "\t";
        else if (dx > gapSpace) line += " ";

        line += text ?? "";
        prevX = x;
    }
    if (line) out.push(line);

    return out.join("\n").replace(/\u00A0/g, " ").replace(/[ \t]+/g, " ");
}

export async function extractWithPdfReader(path: string): Promise<string> {
    return new Promise((resolve, reject) => {
        const items: TextItem[] = [];
        new PdfReader().parseFileItems(path, (err, item: AnyItem) => {
            if (err) return reject(err);
            if (!item) {
                const text = stitchWithLayout(items);
                return resolve(text);
            }
            if (isTextItem(item)) items.push({ text: item.text, x: item.x, y: item.y });
        });
    });
}

function randInt(min: number, max: number) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}
function pad(n: number, w = 2) {
    return n.toString().padStart(w, '0');
}

/** compute Swedish Luhn check digit on 9-digit core (YYMMDDNNN) */
function swedishLuhnCheck(core9: string): number {
    const multipliers = [2, 1, 2, 1, 2, 1, 2, 1, 2];
    let sum = 0;
    for (let i = 0; i < core9.length; i++) {
        let prod = Number(core9[i]) * multipliers[i];
        if (prod > 9) prod -= 9;
        sum += prod;
    }
    return (10 - (sum % 10)) % 10;
}

function isValidDate(y: number, m: number, d: number) {
    const dt = new Date(y, m - 1, d);
    return dt.getFullYear() === y && dt.getMonth() === m - 1 && dt.getDate() === d;
}

/**
* Generate a valid Swedish social security with year <= 1999 (YYYYMMDD-XXXX)
* options:
*   gender?: 'M'|'F'|'random'  // optional gender parity (3rd digit of NNN: odd=M, even=F)
*   minYear?: number (default 1900)
*/
export function generateSocialSecNumber(options?: { gender?: 'M' | 'F' | 'random', minYear?: number }): string {
    const gender = options?.gender ?? 'random';
    const minYear = options?.minYear ?? 1900;
    const maxYear = 1999;

    // pick a valid random date between minYear and 1999
    let y: number, m: number, d: number;
    do {
        y = randInt(minYear, maxYear);
        m = randInt(1, 12);
        d = randInt(1, 31);
    } while (!isValidDate(y, m, d));

    const YYYY = y.toString();
    const YY = YYYY.slice(-2);
    const MM = pad(m);
    const DD = pad(d);

    // pick NNN satisfying gender parity if requested
    let nnn: string;
    for (; ;) {
        nnn = pad(randInt(1, 999), 3); // 001..999 (avoid 000)
        const thirdDigit = Number(nnn[2]);
        if (gender === 'M' && thirdDigit % 2 === 1) break;
        if (gender === 'F' && thirdDigit % 2 === 0) break;
        if (gender === 'random') break;
    }

    const core9 = `${YY}${MM}${DD}${nnn}`; // 9 digits for Luhn
    const check = swedishLuhnCheck(core9);

    return `${YYYY}${MM}${DD}-${nnn}${check}`;
}

export const setJsonValues = (obj: any, key: any, value: any) => {
    const keys = key.split(".");
    let current = obj;
    while (keys.length > 1) {
        const part = keys.shift();
        if (!current[part]) current[part] = {};
        current = current[part];
    }
    current[keys[0]] = value;
}
