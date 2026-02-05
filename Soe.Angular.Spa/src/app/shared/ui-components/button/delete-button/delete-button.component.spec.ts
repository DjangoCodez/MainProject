import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { DeleteButtonComponent } from './delete-button.component';
import { ShortcutService } from '@core/services/shortcut.service';
import { IconUtil } from '@shared/util/icon-util';
import { ComponentRef, ElementRef } from '@angular/core';
import { IconProp } from '@fortawesome/fontawesome-svg-core';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('DeleteButtonComponent', () => {
  let component: DeleteButtonComponent;
  let fixture: ComponentFixture<DeleteButtonComponent>;
  let componentRef: ComponentRef<DeleteButtonComponent>;
  let shortcutService: ShortcutService;

  beforeEach(async () => {
    shortcutService = {
      bindShortcut: vi.fn(),
    } as unknown as ShortcutService;

    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, DeleteButtonComponent],
      providers: [
        { provide: ShortcutService, useValue: shortcutService },
        { provide: ElementRef, useValue: {} },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DeleteButtonComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  describe('Setup', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize default inputs', () => {
      expect(component.caption()).toBe('core.delete');
      expect(component.tooltip()).toBe('core.delete');
      expect(component.iconPrefix()).toBe('fal');
      expect(component.inline()).toBe(false);
      expect(component.disabled()).toBe(false);
      expect(component.dirty()).toBe(false);
      expect(component.inProgress()).toBe(false);
    });

    it('should set icon if iconName is provided on init', () => {
      const mockIcon: IconProp = { prefix: 'fal', iconName: 'check' };
      vi.spyOn(IconUtil, 'createIcon').mockReturnValue(mockIcon);

      componentRef.setInput('iconName', 'check');
      component.ngOnInit();

      expect(IconUtil.createIcon).toHaveBeenCalledWith('fal', 'check');
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

    describe('DOM', () => {
      describe('Delete Button', () => {
        it('should display the delete button', () => {
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
          component.icon.set(mockIcon);
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
          expect(button).toBeTruthy();
          button.nativeElement.click();
          expect(actionSpy).not.toHaveBeenCalled();
        });
      });
    });
  });
});
