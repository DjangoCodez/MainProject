import '@analogjs/vitest-angular/setup-zone';
import { getTestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import {
  BrowserTestingModule,
  platformBrowserTesting,
} from '@angular/platform-browser/testing';
import { vi } from 'vitest';

// Suppress console warnings from focus utility and Angular
const originalError = console.error;
global.console = {
  ...console,
  error: vi.fn((...args: any[]) => {
    // Suppress NG0912 CdkVirtualScrollViewport collision warning
    const message = args[0]?.toString() || '';
    if (message.includes('NG0912') || message.includes('CdkVirtualScrollViewport')) {
      return;
    }
    // Allow other errors through for debugging if needed
    // originalError.apply(console, args);
  }),
  warn: vi.fn(),
  log: vi.fn(),
};

// Mock indexedDB for Node.js test environment
const mockIDBRequest = {
  result: null,
  error: null,
  onsuccess: null,
  onerror: null,
  onupgradeneeded: null,
  onblocked: null,
  readyState: 'pending',
  transaction: null,
  source: null,
};

global.indexedDB = {
  open: vi.fn(() => ({ ...mockIDBRequest })),
  deleteDatabase: vi.fn(() => ({ ...mockIDBRequest })),
  databases: vi.fn(() => Promise.resolve([])),
  cmp: vi.fn(),
} as any;

// Initialize the Angular testing environment.
getTestBed().initTestEnvironment(
  [BrowserTestingModule, SoftOneTestBed],
  platformBrowserTesting()
);
