import { Injectable } from '@angular/core';
import { debounceTime, Subject } from 'rxjs';

// Needs to be injected in the component which will use it.
@Injectable({
  providedIn: 'root',
})
export class AutoHeightService {
  /**
   * Calculates the height of element with content type 'content'.
   */
  public dynamicHeight$ = new Subject<number>();
  public cachedHeight: number | undefined;
  private readonly triggerCalc$ = new Subject<void>();

  private footerTop = 0;
  private footerHeight = 0;
  private contentTop = 0;
  private contentBottom = 0;

  constructor() {
    this.triggerCalc$.pipe(debounceTime(100)).subscribe(() => {
      this.calculateDynamicHeight();
    });
  }

  public setBottomContainer(height: number, top: number, bottom: number) {
    this.footerTop = top;
    this.footerHeight = height;
    this.triggerCalc$.next();
  }

  public setConentContainer(top: number, bottom: number) {
    this.contentTop = top;
    this.contentBottom = bottom;
    this.triggerCalc$.next();
  }

  private calculateDynamicHeight() {
    // Heights that we need to take into account:
    // 1. Footer height (static)
    // 2. Footer offset top (static), e.g. from the top of the dynamic height element to the top of the window.
    const occupiedHeight = this.contentTop + this.footerHeight;

    // 3. Eventual gap between the footer and the content (static).
    const adjustmentToContent =
      this.footerTop > 0 ? this.footerTop - this.contentBottom : 0;

    const marginToBottom = 5;

    // Then add the window height and subtract the occupied height and the adjustments.
    const dynamicHeight =
      window.innerHeight -
      occupiedHeight -
      adjustmentToContent -
      marginToBottom;

    this.cachedHeight = dynamicHeight;
    this.dynamicHeight$.next(dynamicHeight);
  }

  ngOnDestroy() {
    this.triggerCalc$.complete();
    this.dynamicHeight$.complete();
  }
}
