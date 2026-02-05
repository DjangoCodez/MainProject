
import { DestroyRef, ElementRef, Inject, Injectable, DOCUMENT } from '@angular/core';
import { EventManager } from '@angular/platform-browser';

export type Key =
  | 'Control'
  | 'Alt'
  | 'Enter'
  | 'Escape'
  | 'Insert'
  | 'Delete'
  | 'Home'
  | 'End'
  | 'PageUp'
  | 'PageDown'
  | 'ArrowRight'
  | 'ArrowLeft'
  | 'ArrowUp'
  | 'ArrowDown'
  | 'a'
  | 'b'
  | 'c'
  | 'd'
  | 'e'
  | 'f'
  | 'g'
  | 'h'
  | 'i'
  | 'j'
  | 'k'
  | 'l'
  | 'm'
  | 'n'
  | 'o'
  | 'p'
  | 'q'
  | 'r'
  | 's'
  | 't'
  | 'u'
  | 'v'
  | 'w'
  | 'x'
  | 'y'
  | 'z';

@Injectable({
  providedIn: 'root',
})
export class ShortcutService {
  constructor(
    private eventManager: EventManager,
    @Inject(DOCUMENT) private document: Document
  ) {}

  /**
   * Add keyboard binding
   * @param element Component ElementRef, for checking that the callback is only triggered if component is visible
   * @param destroyRef Component DestroyRef, for removing the event listener when component is destroyed
   * @param keys The combination of keys to be pressed for the event to trigger
   * @param callback The function to be called. Pass an arrow function to correctly bind 'this'
   */
  bindShortcut(
    element: ElementRef,
    destroyRef: DestroyRef,
    keys: Key[],
    callback: (event: KeyboardEvent) => void,
    onlyWhenFocused: boolean = true
  ) {
    for (const binding of this.getPlatformBindings(keys)) {
      const removeListenerFn = this.eventManager.addEventListener(
        this.document.documentElement,
        `keydown.${binding}`,
        (e: KeyboardEvent) => {
          if (element.nativeElement.offsetParent === null) {
            // Invisible
            return;
          }
          if (
            onlyWhenFocused &&
            !element.nativeElement.contains(document.activeElement)
          ) {
            // Not focused
            return;
          }
          e.preventDefault();
          callback(e);
        }
      );
      destroyRef.onDestroy(() => {
        removeListenerFn();
      });
    }
  }

  private getPlatformBindings(keys: Key[]) {
    const bindings = [keys.join('.')];
    if (keys.indexOf('Control') >= 0) {
      bindings.push(keys.join('.').replace('Control', 'Meta')); // Mac
    }
    return bindings;
  }
}
