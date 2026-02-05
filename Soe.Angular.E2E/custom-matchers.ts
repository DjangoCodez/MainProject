import { expect, Locator } from "@playwright/test";

export async function someCustomMatcher(received: number, floor: number, ceiling: number) {
  const pass = received >= floor && received <= ceiling;
    if (pass) {
      return {
        message: () => 'passed',
        pass: true,
      };
    } else {
      return {
        message: () => 'failed',
        pass: false,
      };
    }
  
};


