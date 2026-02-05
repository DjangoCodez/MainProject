import { Directive, HostListener, input } from '@angular/core';

@Directive({
  selector: '[soeUnsavedChanges]',
})
export class UnsavedChangesDirective {
  hasChanged = input(false);

  @HostListener('window:beforeunload', ['$event'])
  onBeforeUnload(event: BeforeUnloadEvent) {
    if (!this.hasChanged()) return;

    event.preventDefault(); // Needed for some browsers
    return '';
  }
}
