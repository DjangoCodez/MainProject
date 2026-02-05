import {
  Directive,
  ElementRef,
  HostListener,
  inject,
  input,
} from '@angular/core';
import { AutoHeightService } from './auto-height.service';
import { Subject, takeUntil } from 'rxjs';

// Conditionally applying directives are tricky.
// Therefore the ignore option can be used to "disable" the directive.
export type ElementType = 'ignore' | 'content' | 'bottom';

@Directive({
  selector: '[soeAutoHeight]',
  standalone: true,
})
export class AutoHeightDirective {
  private readonly heightService = inject(AutoHeightService);
  private readonly el = inject(ElementRef);
  private readonly destroy$ = new Subject<void>();

  soeAutoHeight = input<ElementType>('ignore');

  ngOnInit() {
    if (this.heightService.cachedHeight) {
      this.el.nativeElement.style.height = `${this.heightService.cachedHeight}px`;
    }
  }

  ngAfterViewInit() {
    if (this.soeAutoHeight() === 'ignore') {
      return;
    }

    const el = this.el.nativeElement as HTMLElement;

    const observer = new IntersectionObserver(entries => {
      const entry = entries[0];

      if (entry.isIntersecting && el.offsetParent !== null) {
        observer.disconnect();
        this.waitForStableHeight(() => {
          this.setPlacement();
          if (this.soeAutoHeight() === 'content') {
            // Start listening to height updates only after content is visible
            this.heightService.dynamicHeight$
              .pipe(takeUntil(this.destroy$))
              .subscribe(height => {
                el.style.height = `${height}px`;
              });
          }
        });
      }
    });

    observer.observe(el);
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setPlacement() {
    if (this.soeAutoHeight() === 'bottom') {
      this.setBottomContainer();
    } else if (this.soeAutoHeight() === 'content') {
      this.setContentPlacement();
    }
  }

  private setBottomContainer() {
    const boundRect = this.el.nativeElement.getBoundingClientRect();
    this.heightService.setBottomContainer(
      boundRect.height,
      boundRect.top,
      boundRect.bottom
    );
  }

  private setContentPlacement() {
    const boundRect = this.el.nativeElement.getBoundingClientRect();
    this.heightService.setConentContainer(boundRect.top, boundRect.bottom);
  }

  private waitForStableHeight(cb: () => void) {
    // We need to wait for the height to stabilize before we can set it.
    let lastHeight = this.el.nativeElement.offsetHeight;
    let stableCounter = 0;
    let maxFrames = 100; // 100 frames ~1.6s

    const check = () => {
      if (maxFrames-- <= 0) return;

      const currentHeight = this.el.nativeElement.offsetHeight;
      if (currentHeight === lastHeight) {
        stableCounter++;
        if (stableCounter > 10) {
          // 10 frames stable ~166ms
          cb();
          return;
        }
      } else {
        stableCounter = 0;
        lastHeight = currentHeight;
      }
      requestAnimationFrame(check);
    };

    check();
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: Event) {
    this.setPlacement();
  }
}
