
import {
  AfterViewInit,
  Component,
  effect,
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

type HandlePosition = 'center' | 'top' | 'bottom';

@Component({
  selector: 'soe-split-container',
  imports: [IconModule],
  templateUrl: './split-container.component.html',
  styleUrls: ['./split-container.component.scss'],
})
export class SplitContainerComponent
  implements OnChanges, OnDestroy, AfterViewInit
{
  ratio = input(0.5);
  minRatio = input(0.1);
  maxRatio = input(0.9);
  dividerWidth = input(32);
  dividerNoMargins = input(false);
  handlePosition = input<HandlePosition>('center');
  handleTopMargin = input(8);
  handleBottomMargin = input(8);

  hideRightContent = input(false);
  hideLeftContent = input(false);

  ratioChanging = output<number>();
  ratioChanged = output<number>();

  localRatio = signal(0.5);
  offset = signal(0);
  isGrabbing = signal(false);

  @ViewChild('leftElement') leftElementRef!: ElementRef<HTMLElement>;
  @ViewChild('divider') dividerElementRef!: ElementRef<HTMLElement>;
  @ViewChild('rightElement') rightElementRef!: ElementRef<HTMLElement>;

  resizeObserver = new ResizeObserver(() => {
    this.zone.run(() => {
      this.setHandlePosition();
    });
  });
  component: any;

  private prevHideLeftContent = false;
  private prevHideRightContent = false;

  constructor(private zone: NgZone) {
    effect(() => {
      if (
        this.hideLeftContent() === this.prevHideLeftContent &&
        this.hideRightContent() === this.prevHideRightContent
      ) {
        return;
      }

      if (this.hideRightContent() && !this.hideLeftContent()) {
        this.localRatio.set(1);
      } else if (this.hideLeftContent() && !this.hideRightContent()) {
        this.localRatio.set(0);
      } else {
        this.localRatio.set(this.ratio());
      }
      this.prevHideLeftContent = this.hideLeftContent();
      this.prevHideRightContent = this.hideRightContent();
      this.setWidth();
      this.ratioChanged.emit(this.localRatio());
    });
  }

  ngAfterViewInit(): void {
    if (!this.hideLeftContent() && !this.hideRightContent()) {
      this.localRatio.set(this.ratio());
    }

    this.setWidth();
    this.setHandlePosition();
    this.resizeObserver.observe(this.dividerElementRef.nativeElement);
  }

  ngOnDestroy(): void {
    this.resizeObserver.unobserve(this.dividerElementRef.nativeElement);
  }

  ngOnChanges(changes: SimpleChanges) {
    const { localRatio } = changes;
    if (localRatio) this.setWidth();
  }

  dividerMouseDown() {
    this.isGrabbing.set(true);
  }

  @HostListener('window:mouseup') onMouseUp(event: MouseEvent) {
    if (!this.isGrabbing()) return;

    this.isGrabbing.set(false);

    this.ratioChanged.emit(this.localRatio());
  }

  @HostListener('window:mousedown') onMouseDown(event: MouseEvent) {
    if (!this.isGrabbing()) return;

    if (event?.preventDefault) event.preventDefault();
    if (event?.stopPropagation) event.stopPropagation();
  }

  @HostListener('window:mousemove', ['$event']) onMouseMove(event: MouseEvent) {
    if (!this.isGrabbing()) return;

    if (event.preventDefault) event.preventDefault();
    if (event.stopPropagation) event.stopPropagation();

    const boundsLeft =
      this.leftElementRef.nativeElement.getBoundingClientRect().left;
    const boundsRight =
      this.rightElementRef.nativeElement.getBoundingClientRect().right;
    if (event.clientX <= boundsLeft || event.clientX >= boundsRight) return;

    const dividerWidth = this.dividerElementRef.nativeElement.offsetWidth;
    const leftSize = event.clientX - boundsLeft - dividerWidth / 2;
    const totalSize = boundsRight - boundsLeft;
    this.localRatio.set(leftSize / totalSize);
    this.setWidth();

    this.ratioChanging.emit(this.localRatio());
  }

  @HostListener('window:scroll') onWindowScroll() {
    this.setHandlePosition();
  }

  @HostListener('window:resize') onWindowResize() {
    this.setHandlePosition();
  }

  setWidth() {
    if (
      !this.leftElementRef?.nativeElement ||
      !this.rightElementRef?.nativeElement
    ) {
      return;
    }

    const ratio =
      this.localRatio() < this.minRatio()
        ? this.minRatio()
        : this.localRatio() > this.maxRatio()
          ? this.maxRatio()
          : this.localRatio();

    this.leftElementRef.nativeElement.style.flex = `0 5 ${100 * ratio}%`;
    this.rightElementRef.nativeElement.style.flex = `1 5`;
  }

  setHandlePosition() {
    let offset = 0;

    if (this.handlePosition() === 'top') {
      // Position the handle at the top of the divider, with specified margin
    } else {
      const boundingBox =
        this.dividerElementRef?.nativeElement?.getBoundingClientRect();

      if (
        !boundingBox ||
        boundingBox.bottom < 0 ||
        boundingBox.top > window.innerHeight
      ) {
        // Out of view
        return;
      }

      const visibleTop = boundingBox.top < 0 ? 0 : boundingBox.top;
      const visibleBottom =
        boundingBox.bottom > window.innerHeight
          ? window.innerHeight
          : boundingBox.bottom;

      const overshootingTop = boundingBox.top < 0 ? -boundingBox.top : 0;

      if (this.handlePosition() === 'bottom') {
        // Position the handle at the bottom of the divider, with specified margin (64 is the height of the handle)
        offset =
          visibleBottom -
          visibleTop -
          this.handleBottomMargin() -
          64 +
          overshootingTop;
      } else if (this.handlePosition() === 'center') {
        // Position the handle in center of the divider
        offset = (visibleBottom - visibleTop) / 2 - 32 + overshootingTop; // 32 is half the height of the handle
      }
    }

    if (offset < this.handleTopMargin()) offset = this.handleTopMargin();
    this.offset.set(offset);
  }
}
