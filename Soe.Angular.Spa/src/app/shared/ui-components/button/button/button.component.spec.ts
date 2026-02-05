import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { ButtonComponent } from './button.component';
import { ShortcutService } from '@core/services/shortcut.service';
import { IconUtil } from '@shared/util/icon-util';
import { ComponentRef, ElementRef } from '@angular/core';
import { IconProp } from '@fortawesome/fontawesome-svg-core';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('ButtonComponent', () => {
  let component: ButtonComponent;
  let fixture: ComponentFixture<ButtonComponent>;
  let componentRef: ComponentRef<ButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, ButtonComponent],
      providers: [],
    }).compileComponents();

    fixture = TestBed.createComponent(ButtonComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  describe('Setup', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize default inputs', () => {
      expect(component.behaviour()).toBe('standard');
      expect(component.caption()).toBe('');
      expect(component.tooltip()).toBe('');
      expect(component.iconPrefix()).toBe('fal');
      expect(component.inline()).toBe(false);
      expect(component.disabled()).toBe(false);
      expect(component.inProgress()).toBe(false);
    });

    it('should initialize typeClass based on behaviour', () => {
      componentRef.setInput('behaviour', 'primary');
      fixture.detectChanges();
      expect(component.typeClass()).toBe('btn btn-primary');
    });

    it('should compute isDisabled based on inProgress', () => {
      componentRef.setInput('inProgress', true);
      fixture.detectChanges();
      expect(component.isDisabled()).toBe(true);

      componentRef.setInput('inProgress', false);
      fixture.detectChanges();
      expect(component.isDisabled()).toBe(false);
    });

    it('should set icon if iconName is provided on init', () => {
      const mockIcon: IconProp = { prefix: 'fal', iconName: 'check' };
      vi.spyOn(IconUtil, 'createIcon').mockReturnValue(mockIcon);

      componentRef.setInput('iconName', 'check');

      expect(component.icon()).toEqual(mockIcon);
    });
  });

  describe('Methods', () => {
    describe('onAction', () => {
      it('should emit action on onAction', () => {
        const actionSpy = vi.spyOn(component.action, 'emit');
        const mockEvent = new Event('click');

        component.onAction(mockEvent);
        expect(actionSpy).toHaveBeenCalledWith(mockEvent);
      });
    });

    describe('getTypeClass', () => {
      it('should return correct class based on behaviour in getTypeClass', () => {
        componentRef.setInput('behaviour', 'standard');
        expect(component['getTypeClass']()).toBe('btn btn-secondary');

        componentRef.setInput('behaviour', 'primary');
        expect(component['getTypeClass']()).toBe('btn btn-primary');

        componentRef.setInput('behaviour', 'danger');
        expect(component['getTypeClass']()).toBe('btn btn-danger');
      });
    });

    describe('DOM', () => {
      describe('Standard Button', () => {
        it('should display the standard button when not invalid', () => {
          componentRef.setInput('invalid', false);
          componentRef.setInput('behaviour', 'primary');
          fixture.detectChanges();

          const button = fixture.debugElement.query(
            By.css('button[type="button"]')
          );
          expect(button).toBeTruthy();
        });

        it('should apply inline style if "inline" input is true', () => {
          componentRef.setInput('inline', true);
          fixture.detectChanges();

          const button = fixture.debugElement.query(
            By.css('button[type="button"]')
          );
          expect(button.styles['margin-top']).toBe('1.5rem');
        });

        it('should display icon if "icon" is set', () => {
          const mockIcon: IconProp = { prefix: 'fal', iconName: 'check' };
          componentRef.setInput('iconName', 'check');

          fixture.detectChanges();

          const iconElement = fixture.debugElement.query(By.css('fa-icon'));
          expect(iconElement).toBeTruthy();
          expect(iconElement.componentInstance.icon()).toEqual(mockIcon);
        });

        it('should emit action on click if not disabled', () => {
          const actionSpy = vi.spyOn(component, 'onAction');
          vi.spyOn(component, 'isDisabled').mockReturnValue(false);
          fixture.detectChanges();

          const button = fixture.debugElement.query(
            By.css('button[type="button"]')
          );
          button.nativeElement.click();
          expect(actionSpy).toHaveBeenCalled();
        });

        it('should not emit action on click if disabled', () => {
          const actionSpy = vi.spyOn(component, 'onAction');
          vi.spyOn(component, 'isDisabled').mockReturnValue(true);
          fixture.detectChanges();

          const button = fixture.debugElement.query(
            By.css('button[type="button"]')
          );
          button.nativeElement.click();
          expect(actionSpy).not.toHaveBeenCalled();
        });
      });
    });
  });
});
