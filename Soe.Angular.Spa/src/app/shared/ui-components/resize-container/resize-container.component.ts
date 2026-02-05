
import {
  AfterViewInit,
  Component,
  ElementRef,
  HostListener,
  input,
  NgZone,
  OnChanges,
  OnDestroy,
  output,
  signal,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { IconModule } from '@ui/icon/icon.module';

type HandlePosition = 'center' | 'left' | 'right';

@Component({
  selector: 'soe-resize-container',
  imports: [IconModule],
  templateUrl: './resize-container.component.html',
  styleUrls: ['./resize-container.component.scss'],
})
export class ResizeContainerComponent
  implements OnChanges, OnDestroy, AfterViewInit
{
  height = input(300);
  minHeight = input(100);
  maxHeight = input(1000);
  dividerHeight = input(32);
  handlePosition = input<HandlePosition>('center');
  handleLeftMargin = input(8);
  handleRightMargin = input(8);

  heightChanging = output<number>();
  heightChanged = output<number>();

  localHeight = signal(0.5);
  offset = signal(0);
  isGrabbing = signal(false);

  @ViewChild('content') contentElementRef!: ElementRef<HTMLElement>;
  @ViewChild('divider') dividerElementRef!: ElementRef<HTMLElement>;

  resizeObserver = new ResizeObserver(() => {
    this.zone.run(() => {
      this.setHandlePosition();
    });
  });

  constructor(private zone: NgZone) {}

  ngAfterViewInit(): void {
    this.localHeight.set(this.height());
    this.setHeight();
    this.setHandlePosition();
    this.resizeObserver.observe(this.dividerElementRef.nativeElement);
  }

  ngOnDestroy(): void {
    this.resizeObserver.unobserve(this.dividerElementRef.nativeElement);
  }

  ngOnChanges(changes: SimpleChanges) {
    const { localHeight } = changes;
    if (localHeight) this.setHeight();
  }

  dividerMouseDown() {
    this.isGrabbing.set(true);
  }

  @HostListener('window:mouseup') onMouseUp() {
    if (!this.isGrabbing()) return;

    this.isGrabbing.set(false);

    this.heightChanged.emit(this.localHeight());
  }

  @HostListener('window:mousemove', ['$event']) onMouseMove(event: MouseEvent) {
    if (!this.isGrabbing()) return;

    event.preventDefault();
    const boundsTop =
      this.contentElementRef.nativeElement.getBoundingClientRect().top;
    if (event.clientY <= boundsTop) return;

    const dividerHeight = this.dividerElementRef.nativeElement.offsetHeight;
    this.localHeight.set(event.clientY - boundsTop - dividerHeight / 2);
    this.setHeight();

    this.heightChanging.emit(this.localHeight());
  }

  @HostListener('window:scroll') onWindowScroll() {
    this.setHandlePosition();
  }

  @HostListener('window:resize') onWindowResize() {
    this.setHandlePosition();
  }

  setHeight() {
    if (!this.contentElementRef?.nativeElement) return;

    const height =
      this.localHeight() < this.minHeight()
        ? this.minHeight()
        : this.localHeight() > this.maxHeight()
          ? this.maxHeight()
          : this.localHeight();

    this.contentElementRef.nativeElement.style.height = `${height}px`;
  }

  setHandlePosition() {
    let offset = 0;

    if (this.handlePosition() === 'left') {
      // Position the handle at the left of the divider, with specified margin
      offset = this.handleLeftMargin();
    } else {
      const boundingBox =
        this.dividerElementRef?.nativeElement?.getBoundingClientRect();
      if (
        !boundingBox ||
        boundingBox.left < 0 ||
        boundingBox.right > window.innerWidth
      ) {
        // Out of view
        return;
      }

      const visibleLeft = boundingBox.left < 0 ? 0 : boundingBox.left;
      const visibleRight =
        boundingBox.right > window.innerWidth
          ? window.innerWidth
          : boundingBox.right;

      if (this.handlePosition() === 'right') {
        // Position the handle at the right of the divider, with specified margin (64 is the width of the handle)
        offset = visibleRight - visibleLeft - this.handleRightMargin() - 64;
      } else if (this.handlePosition() === 'center') {
        // Center the handle in the visible area of the divider
        offset = (visibleRight - visibleLeft) / 2 - 32; // 32 is half the width of the handle
        if (offset < 0) offset = this.handleLeftMargin();
      }

      if (offset < 0) offset = 0;
    }

    this.offset.set(offset);
  }
}
