import {
  Directive,
  ElementRef,
  HostListener,
  input,
  output,
} from '@angular/core';

@Directive({
  selector: '[soeClickOutside]',
})
export class ClickOutsideDirective {
  isOpen = input(true);
  clickOutside = output<void>();

  constructor(private elementRef: ElementRef) {}

  @HostListener('document:click', ['$event'])
  public onClick(event: MouseEvent) {
    if (!this.isOpen()) return;

    const target = event.target as Node | null;
    if (!target) return;

    const clickedInside = this.elementRef.nativeElement.contains(target);
    if (!clickedInside) {
      this.clickOutside.emit();
    }
  }
}
