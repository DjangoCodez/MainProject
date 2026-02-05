import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { ResizeContainerComponent } from './resize-container.component';
import { ComponentRef, DebugElement, ElementRef } from '@angular/core';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('ResizeContainerComponent', () => {
  let component: ResizeContainerComponent;
  let fixture: ComponentFixture<ResizeContainerComponent>;
  let componentRef: ComponentRef<ResizeContainerComponent>;

  beforeEach(async () => {
    // Mock ResizeObserver
    global.ResizeObserver = vi.fn().mockImplementation(() => ({
      observe: vi.fn(),
      unobserve: vi.fn(),
      disconnect: vi.fn(),
    }));

    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, ResizeContainerComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ResizeContainerComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should set up with default values', () => {
      expect(component.height()).toEqual(300);
      expect(component.minHeight()).toEqual(100);
      expect(component.maxHeight()).toEqual(1000);
      expect(component.isGrabbing()).toBe(false);
    });
  });
  describe('methods', () => {
    describe('ngAfterViewInit', () => {
      it('should set height, handle position and observe divider element', () => {
        vi.spyOn(component, 'setHeight');
        vi.spyOn(component, 'setHandlePosition');
        vi.spyOn(component.resizeObserver, 'observe');
        component.ngAfterViewInit();
        expect(component.setHeight).toHaveBeenCalled();
        expect(component.setHandlePosition).toHaveBeenCalled();
        expect(component.resizeObserver.observe).toHaveBeenCalledWith(
          component.dividerElementRef.nativeElement
        );
      });
    });
    describe('ngOnDestroy', () => {
      it('should unobserve divider element', () => {
        vi.spyOn(component.resizeObserver, 'unobserve');
        component.ngOnDestroy();
        expect(component.resizeObserver.unobserve).toHaveBeenCalledWith(
          component.dividerElementRef.nativeElement
        );
      });
    });
    describe('ngOnChanges', () => {
      it('should call setHeight if input changes', () => {
        vi.spyOn(component, 'setHeight');
        component.ngOnChanges({
          localHeight: {
            currentValue: 200,
            previousValue: 100,
            firstChange: true,
            isFirstChange: () => true,
          },
        });
        expect(component.setHeight).toHaveBeenCalled();
      });
    });
    describe('dividerMouseDown', () => {
      it('should set isGrabbing to true and add grabbing class', () => {
        component.dividerMouseDown();
        fixture.detectChanges();
        expect(component.isGrabbing()).toBe(true);
        console.log(component.dividerElementRef.nativeElement.classList);
        expect(
          component.dividerElementRef.nativeElement.classList.contains(
            'grabbing'
          )
        ).toBe(true);
      });
    });
    describe('onMouseUp', () => {
      it('should do nothing if not grabbing', () => {
        component.onMouseUp();
        expect(component.isGrabbing()).toBe(false);
      });
      it('should emit height and reset grabbing if grabbing', () => {
        vi.spyOn(component.heightChanged, 'emit');
        component.isGrabbing.set(true);
        component.onMouseUp();
        expect(component.heightChanged.emit).toHaveBeenCalledWith(
          component.localHeight()
        );
        expect(component.isGrabbing()).toBe(false);
      });
    });
    describe('onMouseMove', () => {
      it('should do nothing if not grabbing', () => {
        component.onMouseMove(new MouseEvent('mousemove'));
        expect(component.isGrabbing()).toBe(false);
      });
      it('should prevent default and set height if grabbing and mouse is below content', () => {
        vi.spyOn(component, 'setHeight');
        vi.spyOn(
          component.contentElementRef.nativeElement,
          'getBoundingClientRect'
        ).mockReturnValue({
          top: 0,
          height: 0,
          width: 0,
          x: 0,
          y: 0,
          bottom: 0,
          left: 0,
          right: 0,
          toJSON: () => {},
        });
        component.isGrabbing.set(true);
        component.onMouseMove(new MouseEvent('mousemove', { clientY: 10 }));
        expect(component.setHeight).toHaveBeenCalled();
      });
      it('should not set height if mouse is above content', () => {
        vi.spyOn(component, 'setHeight');
        component.isGrabbing.set(true);
        component.onMouseMove(new MouseEvent('mousemove', { clientY: 0 }));
        expect(component.setHeight).not.toHaveBeenCalled();
      });
    });
    describe('setHeight', () => {
      it('should not set height if content element is not available', () => {
        component.contentElementRef =
          null as unknown as ElementRef<HTMLDivElement>;
        component.setHeight();
        expect(component.contentElementRef).toBeNull();
      });
      it('should set height to min if height is less than min', () => {
        component.localHeight.set(component.minHeight() - 1);
        component.setHeight();
        expect(component.contentElementRef.nativeElement.style.height).toEqual(
          component.minHeight() + 'px'
        );
      });
      it('should set height to max if the height is more than max', () => {
        component.localHeight.set(component.maxHeight() + 1);
        component.setHeight();
        expect(component.contentElementRef.nativeElement.style.height).toEqual(
          component.maxHeight() + 'px'
        );
      });
      it('should set height to the given height if height is more than minheight', () => {
        component.localHeight.set(component.minHeight() + 1);
        component.setHeight();
        expect(component.contentElementRef.nativeElement.style.height).toEqual(
          component.localHeight() + 'px'
        );
      });
    });
  });
  describe('DOM', () => {
    it('should render a divider with a handle icon', () => {
      const divider: HTMLElement = fixture.debugElement.query(
        By.css('.divider')
      ).nativeElement;
      expect(divider).toBeTruthy();

      const handleIcon: HTMLElement = fixture.debugElement.query(
        By.css('.handle fa-icon')
      ).nativeElement;
      expect(handleIcon).toBeTruthy();
    });
    it('should execute dividerMouseDown on mousedown event', () => {
      const dividerDebugElement: DebugElement = fixture.debugElement.query(
        By.css('.divider')
      );
      dividerDebugElement.triggerEventHandler('mousedown', {});
    });
  });
});
