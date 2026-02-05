import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';

import { SplitContainerComponent } from './split-container.component';
import { ComponentRef, SimpleChange } from '@angular/core';
import { random } from 'lodash';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('SplitContainerComponent', () => {
  let component: SplitContainerComponent;
  let fixture: ComponentFixture<SplitContainerComponent>;
  let componentRef: ComponentRef<SplitContainerComponent>;

  beforeEach(() => {
    // Mock ResizeObserver
    global.ResizeObserver = vi.fn().mockImplementation(() => ({
      observe: vi.fn(),
      unobserve: vi.fn(),
      disconnect: vi.fn(),
    }));

    TestBed.configureTestingModule({
      imports: [SoftOneTestBed, SplitContainerComponent],
    });
    fixture = TestBed.createComponent(SplitContainerComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should set up with default values', () => {
      expect(component.ratio()).toEqual(0.5);
      expect(component.minRatio()).toEqual(0.1);
      expect(component.maxRatio()).toEqual(0.9);
      expect(component.isGrabbing()).toBe(false);
    });
  });
  describe('methods', () => {
    describe('ngAfterViewInit', () => {
      it('should set width, handle position and observe divider element', () => {
        vi.spyOn(component, 'setWidth');
        vi.spyOn(component, 'setHandlePosition');
        vi.spyOn(component.resizeObserver, 'observe');
        component.ngAfterViewInit();
        expect(component.setWidth).toHaveBeenCalled();
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
      it('should set width if ratio changes', () => {
        vi.spyOn(component, 'setWidth');
        component.ngOnChanges({
          localRatio: new SimpleChange(null, true, false),
        });
        expect(component.setWidth).toHaveBeenCalled();
      });
    });
    describe('dividerMouseDown', () => {
      it('should set isGrabbing to true and add grabbing class to divider element', () => {
        component.dividerMouseDown();
        fixture.detectChanges();
        expect(component.isGrabbing()).toBe(true);
        expect(
          component.dividerElementRef.nativeElement.classList.contains(
            'grabbing'
          )
        ).toBe(true);
      });
    });
    describe('onMouseUp', () => {
      it('should emit ratioChanged event if grabbing', () => {
        vi.spyOn(component.ratioChanged, 'emit');
        component.isGrabbing.set(true);
        fixture.detectChanges(); // Apply the class binding first
        document.body.style.cursor = 'e-resize';
        component.onMouseUp({} as MouseEvent);
        fixture.detectChanges(); // Update after isGrabbing changes
        expect(component.isGrabbing()).toBe(false);
        expect(
          component.dividerElementRef.nativeElement.classList.contains(
            'grabbing'
          )
        ).toBe(false);
        expect(component.ratioChanged.emit).toHaveBeenCalledWith(
          component.localRatio()
        );
      });
      it('should not do anything if grabbing is false', () => {
        vi.spyOn(component.ratioChanged, 'emit');
        component.isGrabbing.set(false);
        component.onMouseUp({} as MouseEvent);
        expect(component.isGrabbing()).toBe(false);
        expect(component.ratioChanged.emit).not.toHaveBeenCalled();
      });
    });
    describe('onMouseMove', () => {
      it('should set ratio and call setWidth if grabbing', () => {
        const mockNativeElement = {
          get offsetWidth() {
            return 20;
          },
        };
        vi.spyOn(component, 'setWidth');
        const event = new MouseEvent('mousemove', { clientX: 100 });
        component.isGrabbing.set(true);
        component.leftElementRef.nativeElement.getBoundingClientRect = vi.fn(
          () => ({
            left: 0,
            height: 0,
            width: 0,
            x: 0,
            y: 0,
            top: 0,
            right: 0,
            bottom: 0,
            toJSON: vi.fn(),
          })
        );
        component.rightElementRef.nativeElement.getBoundingClientRect = vi.fn(
          () => ({
            height: 0,
            width: 0,
            x: 0,
            y: 0,
            top: 0,
            right: 200,
            bottom: 0,
            left: 0,
            toJSON: vi.fn(),
          })
        );
        component.dividerElementRef.nativeElement =
          mockNativeElement as HTMLElement;
        component.onMouseMove(event);
        expect(component.localRatio()).toBe(0.45);
        expect(component.setWidth).toHaveBeenCalled();
      });
      it('should not do anything if grabbing is false', () => {
        vi.spyOn(component, 'setWidth');
        const event = new MouseEvent('mousemove', { clientX: 100 });
        component.isGrabbing.set(false);
        component.onMouseMove(event);
        expect(component.localRatio()).toBe(0.5);
        expect(component.setWidth).not.toHaveBeenCalled();
      });
    });
    describe('setWidth', () => {
      it('should not set width if leftElementRef not available', () => {
        component.leftElementRef.nativeElement = null as unknown as HTMLElement;
        const leftElement = component.leftElementRef.nativeElement;
        const rightElement = component.rightElementRef.nativeElement.style.flex;
        component.setWidth();
        expect(component.leftElementRef.nativeElement).toBe(leftElement);
        expect(component.rightElementRef.nativeElement.style.flex).toBe(
          rightElement
        );
      });
      it('should not set width if rightElementRef not available', () => {
        component.rightElementRef.nativeElement =
          null as unknown as HTMLElement;
        const leftElement = component.leftElementRef.nativeElement.style.flex;
        const rightElement = component.rightElementRef.nativeElement;
        component.setWidth();
        expect(component.leftElementRef.nativeElement.style.flex).toBe(
          leftElement
        );
        expect(component.rightElementRef.nativeElement).toBe(rightElement);
      });
      it('should set ratio to minratio if ratio less than minratio', () => {
        const randomNum = random(0, component.minRatio());
        component.localRatio.set(randomNum);
        component.setWidth();
        expect(component.leftElementRef.nativeElement.style.flex).toBe(
          `0 5 ${100 * component.minRatio()}%`
        );
        expect(component.rightElementRef.nativeElement.style.flex).toContain(
          '1 5'
        );
      });
      it('should set ratio to maxratio if ratio greater than maxratio', () => {
        const randomNum = random(
          component.maxRatio() + 0.01,
          component.maxRatio() + 1
        );
        component.localRatio.set(randomNum);
        component.setWidth();
        expect(component.leftElementRef.nativeElement.style.flex).toBe(
          `0 5 ${100 * component.maxRatio()}%`
        );
        expect(component.rightElementRef.nativeElement.style.flex).toContain(
          '1 5'
        );
      });
      it('should set ratio to ratio if ratio between minratio and maxratio', () => {
        const randomNum = random(component.minRatio(), component.maxRatio());
        component.localRatio.set(randomNum);
        component.setWidth();
        expect(component.leftElementRef.nativeElement.style.flex).toBe(
          `0 5 ${100 * component.localRatio()}%`
        );
        expect(component.rightElementRef.nativeElement.style.flex).toContain(
          '1 5'
        );
      });
    });
    describe('onWindowScroll', () => {
      it('should call setHandlePosition', () => {
        vi.spyOn(component, 'setHandlePosition');
        component.onWindowScroll();
        expect(component.setHandlePosition).toHaveBeenCalled();
      });
    });
    describe('onWindowResize', () => {
      it('should call setHandlePosition', () => {
        vi.spyOn(component, 'setHandlePosition');
        component.onWindowResize();
        expect(component.setHandlePosition).toHaveBeenCalled();
      });
    });
    describe('setHandlePosition', () => {
      it('should not set handle position if dividerElementRef not available', () => {
        component.dividerElementRef.nativeElement =
          null as unknown as HTMLElement;
        component.setHandlePosition();
      });
      it('should set marginTop', () => {
        const mockNativeElement = {
          style: {
            marginTop: '30px',
            top: '20px',
          },
          getBoundingClientRect: vi.fn(() => ({
            left: 10,
            height: 10,
            width: 10,
            x: 5,
            y: 5,
            top: -10,
            right: 10,
            bottom: 10,
            toJSON: vi.fn(),
          })),
        };
        component.dividerElementRef.nativeElement =
          mockNativeElement as unknown as HTMLElement;
        component.setHandlePosition();
        // The method calculates offset and sets it to the signal, which is bound to style in template
        // With top: -10, bottom: 10, the visible height is 10 (from 0 to 10)
        // offset = (10 - 0) / 2 - 32 + 10 = 5 - 32 + 10 = -17, which gets clamped to handleTopMargin (8)
        expect(component.offset()).toBe(8);
      });
      it('should not set marginTop if offset is less than 0', () => {
        const mockNativeElement = {
          style: {
            marginTop: '30px',
            top: '20px',
          },
          getBoundingClientRect: vi.fn(() => ({
            left: 10,
            height: 10,
            width: 10,
            x: 5,
            y: 5,
            top: 10,
            right: 10,
            bottom: 10,
            toJSON: vi.fn(),
          })),
        };
        component.dividerElementRef.nativeElement =
          mockNativeElement as unknown as HTMLElement;
        component.dividerElementRef.nativeElement =
          mockNativeElement as unknown as HTMLElement;
        component.setHandlePosition();
        expect(component.dividerElementRef.nativeElement.style.marginTop).toBe(
          '30px'
        );
      });
      it('should not set marginTop if GetBoundingClient null', () => {
        const mockNativeElement = {
          style: {
            marginTop: '30px',
            top: '20px',
          },
          getBoundingClientRect: vi.fn(() => null),
        };
        component.dividerElementRef.nativeElement =
          mockNativeElement as unknown as HTMLElement;
        component.dividerElementRef.nativeElement =
          mockNativeElement as unknown as HTMLElement;
        component.setHandlePosition();
        expect(component.dividerElementRef.nativeElement.style.marginTop).toBe(
          '30px'
        );
      });
    });
  });
  describe('DOM', () => {
    it('should render the flex-container, left and right flex-items, and divider', () => {
      const container = fixture.debugElement.query(By.css('.flex-container'));
      const leftItem = fixture.debugElement.query(
        By.css('.flex-item:first-child')
      );
      const rightItem = fixture.debugElement.query(
        By.css('.flex-item:last-child')
      );
      const divider = fixture.debugElement.query(By.css('.divider'));

      expect(container).toBeTruthy();
      expect(leftItem).toBeTruthy();
      expect(rightItem).toBeTruthy();
      expect(divider).toBeTruthy();
    });
    it('should call dividerMouseDown when the divider is clicked', () => {
      vi.spyOn(component, 'dividerMouseDown');

      const divider = fixture.debugElement.query(By.css('.divider'));
      divider.triggerEventHandler('mousedown', null);

      expect(component.dividerMouseDown).toHaveBeenCalled();
    });
  });
});
