export {};

declare global {
 namespace PlaywrightTest {
    interface Matchers<R, T> {
        someCustomMatcher(a: number, b: number): R;
    }
  }
}