import { ComponentRef } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { By } from '@angular/platform-browser';
import { ShapeCellRenderer, ShapeType } from './shape-cell-renderer.component';
import { ShapeCellRendererParams } from '@ui/grid/interfaces/cell-renderer.interface';
import { vi } from 'vitest';

describe('ShapeCellRenderer', () => {
  let component: ShapeCellRenderer;
  let componentRef: ComponentRef<ShapeCellRenderer>;
  let fixture: ComponentFixture<ShapeCellRenderer>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, ShapeCellRenderer],
    }).compileComponents();

    fixture = TestBed.createComponent(ShapeCellRenderer);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    component.params = {} as ShapeCellRendererParams;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should set the default values', () => {
      expect(component.isVisible).toBe(true);
      expect(component.shape).toBe('circle');
      expect(component.width).toBe(20);
      expect(component.color).toBe('transparent');
      expect(component.showShapeField).toBe('');
      expect(component.useGradient).toBe(false);
      expect(component.gradientField).toBe('');
      expect(component.tooltip).toBe('');
      expect(component.shapeTooltip).toBe('');
      expect(component.isText).toBe(false);
      expect(component.isSelect).toBe(false);
    });
  });
  describe('methods', () => {
    describe('agInit', () => {
      it('should initialize with basic parameters', () => {
        const params = {
          data: { colorField: 'red' },
          shape: 'circle' as ShapeType,
          width: 100,
          color: 'blue',
          tooltip: 'Sample Tooltip',
          shapeTooltip: 'Shape Tooltip',
          isText: true,
          isSelect: false,
        } as ShapeCellRendererParams;

        component.agInit(params);
        expect(component.params).toEqual(params);
        expect(component.data).toEqual(params.data);
        expect(component.shape).toBe('circle');
        expect(component.width).toBe(100);
        expect(component.color).toBe('blue');
        expect(component.tooltip).toBe('Sample Tooltip');
        expect(component.shapeTooltip).toBe('Shape Tooltip');
        expect(component.isText).toBe(true);
        expect(component.isSelect).toBe(false);
      });

      it('should override color with colorField value from data', () => {
        component.color = 'blue';

        const params = {
          data: { colorField: 'green' },
          colorField: 'colorField',
          color: 'blue',
          isSelect: false,
        } as ShapeCellRendererParams;

        component.agInit(params);

        expect(component.color).toBe('green');
      });

      it('should set isVisible based on gradientField', () => {
        component.isVisible = false;

        const params = {
          data: { gradientField: true },
          gradientField: 'gradientField',
          isSelect: false,
        } as ShapeCellRendererParams;

        component.agInit(params);

        expect(component.isVisible).toBe(true);
      });

      it('should set isVisible based on showShapeField', () => {
        component.isVisible = false;

        const params = {
          data: { showShapeField: true },
          showShapeField: 'showShapeField',
          isSelect: false,
        } as ShapeCellRendererParams;

        component.agInit(params);

        expect(component.isVisible).toBe(true);
      });

      it('should handle missing data gracefully', () => {
        const params = {
          data: null,
          isSelect: false,
        } as ShapeCellRendererParams;

        component.agInit(params);

        expect(component.data).toBeNull();
        expect(component.isVisible).toBe(true);
      });
      it('should set isSelect correctly', () => {
        component.isSelect = false;

        const params = {
          isSelect: true,
        } as ShapeCellRendererParams;

        component.agInit(params);

        expect(component.isSelect).toBe(true);
      });
    });
    describe('refresh', () => {
      it('should return false', () => {
        expect(component.refresh({} as ShapeCellRendererParams)).toBe(false);
      });
    });
    describe('getters', () => {
      it('should return true if circle, else false', () => {
        component.shape = 'circle';
        expect(component.isCircle).toBe(true);

        component.shape = 'square';
        expect(component.isCircle).toBe(false);
      });
      it('should return true if square, else false', () => {
        component.shape = 'square';
        expect(component.isSquare).toBe(true);

        component.shape = 'circle';
        expect(component.isSquare).toBe(false);
      });
      it('should return true if triangle up, else false', () => {
        component.shape = 'triangle-up';
        expect(component.isTriangleUp).toBe(true);

        component.shape = 'circle';
        expect(component.isTriangleUp).toBe(false);
      });
      it('should return true if triangle down, else false', () => {
        component.shape = 'triangle-down';
        expect(component.isTriangleDown).toBe(true);

        component.shape = 'circle';
        expect(component.isTriangleDown).toBe(false);
      });
      it('should return true if triangle right, else false', () => {
        component.shape = 'triangle-right';
        expect(component.isTriangleRight).toBe(true);

        component.shape = 'circle';
        expect(component.isTriangleRight).toBe(false);
      });
      it('should return true if triangle left, else false', () => {
        component.shape = 'triangle-left';
        expect(component.isTriangleLeft).toBe(true);

        component.shape = 'circle';
        expect(component.isTriangleLeft).toBe(false);
      });
    });
    describe('getValue', () => {
      it('should return value', () => {
        component.params = {
          value: 'value',
          valueFormatted: 'valueFormatted',
        } as ShapeCellRendererParams;
        component.isText = true;
        expect(component.getValue()).toBe('value');
      });
      it('should return valueformatted', () => {
        component.params = {
          value: 'value',
          valueFormatted: 'valueFormatted',
        } as ShapeCellRendererParams;
        component.isText = false;
        component.isSelect = true;
        expect(component.getValue()).toBe('valueFormatted');
      });
    });
    describe('isColorInFilter', () => {
      it('should return true if isFilter and color and color includes #', () => {
        component.isFilter = true;
        component.color = '#ffffff';
        expect(component.isColorInFilter()).toBe(true);
      });
      it('should return false if isFilter and color and color does not include #', () => {
        component.isFilter = true;
        component.color = 'ffffff';
        expect(component.isColorInFilter()).toBe(false);
      });
      it('should return false if isFilter is false', () => {
        component.isFilter = false;
        component.color = '#ffffff';
        expect(component.isColorInFilter()).toBe(false);
      });
    });
  });
  describe('DOM', () => {
    beforeEach(() => {
      const params = {
        data: { colorField: 'red' },
        shape: 'circle' as ShapeType,
        width: 100,
        color: 'blue',
        tooltip: 'Sample Tooltip',
        shapeTooltip: 'Shape Tooltip',
        isText: true,
        isSelect: false,
      } as ShapeCellRendererParams;
      component.params = params;
    });
    describe('div', () => {
      let divElement: HTMLDivElement;
      beforeEach(() => {
        fixture.detectChanges();
        divElement = fixture.debugElement.query(By.css('div')).nativeElement;
      });
      it('should not render the div when params.data is null', () => {
        component.params = {
          data: null,
          shape: 'circle',
          useGradient: false,
        } as ShapeCellRendererParams;
        fixture.detectChanges();
        const divElement = fixture.debugElement.query(
          By.css('div')
        )?.nativeElement;
        expect(divElement).toBeFalsy();
      });
      it('should render the div when params.data is truthy', () => {
        expect(divElement).toBeTruthy();
      });
      it('should set display none if isVisible is false', () => {
        component.isVisible = false;
        fixture.detectChanges();
        expect(divElement.style.display).toBe('none');
      });
      it('should not set display none if isVisible is true', () => {
        component.isVisible = true;
        fixture.detectChanges();
        expect(divElement.style.display).not.toBe('none');
      });
      it('should render params.value when isFilter is true and isColorInFilter is false', () => {
        const params = {
          data: { colorField: 'red' },
          shape: 'circle' as ShapeType,
          width: 100,
          color: 'blue',
          tooltip: 'Sample Tooltip',
          shapeTooltip: 'Shape Tooltip',
          isText: true,
          isSelect: false,
          value: 'test value',
        } as ShapeCellRendererParams;
        component.params = params;
        component.isFilter = true;
        component.isColorInFilter = () => false;
        fixture.detectChanges();
        divElement = fixture.debugElement.query(By.css('div')).nativeElement;
        expect(divElement.textContent?.trim()).toBe('test value');
      });
    });
    describe('shapes', () => {
      it('should render a circle without gradient when isCircle is true and useGradient is false', () => {
        component.shape = 'circle';
        component.useGradient = false;
        component.color = 'red';
        fixture.detectChanges();
        const circleElement = fixture.debugElement.query(
          By.css('circle')
        ).nativeElement;
        expect(circleElement).toBeTruthy();
        expect(circleElement.style.fill).toBe('red');
      });
      it('should render a square when isSquare is true', () => {
        component.shape = 'square';
        component.useGradient = false;
        component.color = 'blue';
        fixture.detectChanges();
        const rectElement = fixture.debugElement.query(
          By.css('rect')
        ).nativeElement;
        expect(rectElement).toBeTruthy();
        expect(rectElement.style.fill).toBe('blue');
      });
      it('should render a rectangle when isRectangle is true', () => {
        component.shape = 'rectangle';
        component.useGradient = false;
        component.color = 'green';
        fixture.detectChanges();
        const rectElement = fixture.debugElement.query(
          By.css('rect')
        ).nativeElement;
        expect(rectElement).toBeTruthy();
        expect(rectElement.style.fill).toBe('green');
      });
      it('should render a triangle up when isTriangleUp is true', () => {
        component.shape = 'triangle-up';
        component.useGradient = false;
        component.color = 'yellow';
        fixture.detectChanges();
        const polygonElement = fixture.debugElement.query(
          By.css('polygon')
        ).nativeElement;
        expect(polygonElement).toBeTruthy();
        expect(polygonElement.style.fill).toBe('yellow');
      });
      it('should render a triangle right when isTriangleRight is true', () => {
        component.shape = 'triangle-right';
        component.useGradient = false;
        component.color = 'purple';
        fixture.detectChanges();
        const polygonElement = fixture.debugElement.query(
          By.css('polygon')
        ).nativeElement;
        expect(polygonElement).toBeTruthy();
        expect(polygonElement.style.fill).toBe('purple');
      });
      it('should render a triangle down when isTriangleDown is true', () => {
        component.shape = 'triangle-down';
        component.useGradient = false;
        component.color = 'orange';
        fixture.detectChanges();
        const polygonElement = fixture.debugElement.query(
          By.css('polygon')
        ).nativeElement;
        expect(polygonElement).toBeTruthy();
        expect(polygonElement.style.fill).toBe('orange');
      });
      it('should render a triangle left when isTriangleLeft is true', () => {
        component.shape = 'triangle-left';
        component.useGradient = false;
        component.color = 'pink';
        fixture.detectChanges();
        const polygonElement = fixture.debugElement.query(
          By.css('polygon')
        ).nativeElement;
        expect(polygonElement).toBeTruthy();
        expect(polygonElement.style.fill).toBe('pink');
      });
    });
    describe('div span', () => {
      it('should render filterText when isFilter is true', () => {
        component.params.data = 'some data';
        component.isColorInFilter = () => true;
        component.isFilter = true;
        component.filterText = 'Test Filter';
        fixture.detectChanges();
        const spanElements = fixture.nativeElement.querySelectorAll('div span');
        const spanElement = spanElements[1];
        expect(spanElement.textContent).toBe('Test Filter');
      });
      it('should render value from getValue() when isText', () => {
        component.params.data = 'some data';
        component.isText = true;
        vi.spyOn(component, 'getValue').mockReturnValue('Mock Value');
        fixture.detectChanges();
        const spanElements = fixture.nativeElement.querySelectorAll('div span');
        const spanElement = spanElements[1];
        expect(spanElement.textContent).toBe('Mock Value');
      });
      it('should render value from getValue() when isSelect is true', () => {
        component.params.data = 'some data';
        component.isSelect = true;
        vi.spyOn(component, 'getValue').mockReturnValue('Mock Value');
        fixture.detectChanges();
        const spanElements = fixture.nativeElement.querySelectorAll('div span');
        const spanElement = spanElements[1];
        expect(spanElement.textContent).toBe('Mock Value');
      });
    });
  });
});
