import { ElementRef } from '@angular/core';

export class BrowserUtil {
  static openInSameTab($window: any, url: string) {
    return $window.open(url, '_self');
  }

  static openInNewTab($window: any, url: string) {
    return $window.open(url, '_blank');
  }

  static openInNewWindow($window: any, url: string) {
    return $window.open(url, 'newwindow');
  }

  static downloadFile(url: string) {
    window.location.assign(url);
  }

  static getElementHeight(element: HTMLElement): number {
    return element
      ? element.clientHeight || element.getBoundingClientRect().height
      : 0;
  }

  static getElementWidth(element: HTMLElement): number {
    return element
      ? element.clientWidth || element.getBoundingClientRect().width
      : 0;
  }

  static elementHasContent(elementRef?: ElementRef): boolean {
    return (
      elementRef?.nativeElement.innerHTML &&
      elementRef?.nativeElement.innerHTML !== '<!--container-->' &&
      elementRef?.nativeElement.innerHTML !== '<!--container--><!--container-->'
    );
  }
}
