import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { InstructionComponent } from './instruction.component';
import { ComponentRef, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('InstructionComponent', () => {
  let component: InstructionComponent;
  let componentRef: ComponentRef<InstructionComponent>;
  let fixture: ComponentFixture<InstructionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, InstructionComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(InstructionComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should set the default values', () => {
      expect(component.type()).toBe('info');
      expect(component.text()).toBe('');
      expect(component.iconPrefix()).toBe('fal');
      expect(component.iconName()).toBe('info-circle');
      expect(component.iconClass()).toBe('');
      expect(component.iconSpin()).toBe(false);
      expect(component.showIcon()).toBe(false);
      expect(component.showClose()).toBe(false);
      expect(component.fitToContent()).toBe(false);
    });
  });
  describe('methods', () => {
    describe('ngOnInit', () => {
      it('should set the default icon name', () => {
        componentRef.setInput('iconName', '');
        componentRef.setInput('type', 'success');
        vi.spyOn(component, 'setDefaultIconName' as any);
        component.ngOnInit();
        expect(component['setDefaultIconName']).toHaveBeenCalled();
      });
      it('should set the icon', () => {
        componentRef.setInput('iconName', 'info-circle');
        componentRef.setInput('iconPrefix', 'fal');
        component.ngOnInit();
        expect(component.icon).toStrictEqual(['fal', 'info-circle']);
      });
    });
    describe('setDefaultIconName', () => {
      it('should set the icon name to check-circle', () => {
        componentRef.setInput('type', 'success');
        component['setDefaultIconName']();
        expect(component.iconName()).toBe('check-circle');
      });
      it('should set the icon name to info-circle', () => {
        componentRef.setInput('type', 'info');
        component['setDefaultIconName']();
        expect(component.iconName()).toBe('info-circle');
      });
      it('should set the icon name to exclamation-circle', () => {
        componentRef.setInput('type', 'warning');
        component['setDefaultIconName']();
        expect(component.iconName()).toBe('exclamation-circle');
      });
      it('should set the icon name to exclamation-triangle', () => {
        componentRef.setInput('type', 'error');
        component['setDefaultIconName']();
        expect(component.iconName()).toBe('exclamation-triangle');
      });
    });
  });
  describe('DOM', () => {
    describe('alert show', () => {
      let alertDebugElement: DebugElement;
      let alertElement: HTMLElement;
      beforeEach(() => {
        alertDebugElement = fixture.debugElement.query(By.css('.alert'));
        alertElement = alertDebugElement.nativeElement;
      });
      it('should render with classes depending on values', () => {
        componentRef.setInput('inline', true);
        componentRef.setInput('fitToContent', true);
        componentRef.setInput('showClose', true);
        componentRef.setInput('type', '');
        fixture.detectChanges();
        expect(alertElement).toBeTruthy();
        expect(alertElement.classList).toContain('inline');
        expect(alertElement.classList).toContain('fit-content');
        expect(alertElement.classList).toContain('alert-dismissible');
        expect(alertElement.classList).not.toContain('alert-success');
        expect(alertElement.classList).not.toContain('alert-info');
        expect(alertElement.classList).not.toContain('alert-warning');
        expect(alertElement.classList).not.toContain('alert-danger');
        expect(alertElement.classList).not.toContain('alert-secondary');
      });
      it('should not render with classes depending on values', () => {
        componentRef.setInput('inline', false);
        componentRef.setInput('fitToContent', false);
        componentRef.setInput('showClose', false);
        fixture.detectChanges();
        expect(alertElement).toBeTruthy();
        expect(alertElement.classList).not.toContain('inline');
        expect(alertElement.classList).not.toContain('fit-content');
        expect(alertElement.classList).not.toContain('alert-dismissible');
      });
      it('should render class depending on value of type', () => {
        componentRef.setInput('type', 'success');
        fixture.detectChanges();
        expect(alertElement).toBeTruthy();
        expect(alertElement.classList).toContain('alert-success');

        componentRef.setInput('type', 'info');
        fixture.detectChanges();
        expect(alertElement).toBeTruthy();
        expect(alertElement.classList).toContain('alert-info');

        componentRef.setInput('type', 'warning');
        fixture.detectChanges();
        expect(alertElement).toBeTruthy();
        expect(alertElement.classList).toContain('alert-warning');

        componentRef.setInput('type', 'error');
        fixture.detectChanges();
        expect(alertElement).toBeTruthy();
        expect(alertElement.classList).toContain('alert-danger');

        componentRef.setInput('type', 'plain');
        fixture.detectChanges();
        expect(alertElement).toBeTruthy();
        expect(alertElement.classList).toContain('alert-secondary');
      });
    });
    describe('fa-icon', () => {
      it('should render <fa-icon> when showIcon() is true and icon is provided', () => {
        component.icon = 'info-circle';
        vi.spyOn(component, 'showIcon').mockReturnValue(true);
        fixture.detectChanges();

        const iconElement = fixture.debugElement.query(
          By.css('fa-icon')
        ).componentInstance;

        expect(iconElement).toBeTruthy();
        expect(iconElement.icon()).toBe('info-circle');
      });

      it('should not render <fa-icon> when showIcon() is false', () => {
        vi.spyOn(component, 'showIcon').mockReturnValue(false);
        fixture.detectChanges();

        const iconElement = fixture.debugElement.query(By.css('fa-icon'));
        expect(iconElement).toBeNull();
      });
    });
    describe('close button', () => {
      it('should render close button when showClose() is true', () => {
        vi.spyOn(component, 'showClose').mockReturnValue(true);
        fixture.detectChanges();
        const closeButtonElement = fixture.debugElement.query(
          By.css('.btn-close')
        );
        expect(closeButtonElement).toBeTruthy();
      });
      it('should not render close button when showClose() is false', () => {
        vi.spyOn(component, 'showClose').mockReturnValue(false);
        fixture.detectChanges();
        const closeButton = fixture.debugElement.query(By.css('.btn-close'));
        expect(closeButton).toBeNull();
      });
    });
  });
});
