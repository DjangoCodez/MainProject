import { of } from 'rxjs';
import { delay, take } from 'rxjs/operators';

export function focusOnElementBySelector(
  selector: string,
  delayInMs = 0
): void {
  of(true)
    .pipe(delay(delayInMs), take(1))
    .subscribe(() => {
      const el: HTMLElement | null = document.querySelector(selector);
      el
        ? el.focus()
        : console.error(`No selector was found for "${selector}"`);
    });
}

export function focusOnElement(element: HTMLElement, delayInMs = 0): void {
  of(true)
    .pipe(delay(delayInMs), take(1))
    .subscribe(() => {
      element ? element.focus() : console.error(`No element was provided`);
    });
}
