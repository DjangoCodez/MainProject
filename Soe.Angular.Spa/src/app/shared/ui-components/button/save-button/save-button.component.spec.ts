import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { SaveButtonComponent } from './save-button.component';
import { ShortcutService } from '@core/services/shortcut.service';
import { IconUtil } from '@shared/util/icon-util';
import { ComponentRef, ElementRef } from '@angular/core';
import { of } from 'rxjs';
import { IconProp } from '@fortawesome/fontawesome-svg-core';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('SaveButtonComponent', () => {
  let component: SaveButtonComponent;
  let fixture: ComponentFixture<SaveButtonComponent>;
  let componentRef: ComponentRef<SaveButtonComponent>;
  let shortcutService: ShortcutService;

  beforeEach(async () => {
    shortcutService = {
      bindShortcut: vi.fn(),
    } as unknown as ShortcutService;

    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, SaveButtonComponent],
      providers: [
        { provide: ShortcutService, useValue: shortcutService },
        { provide: ElementRef, useValue: {} },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SaveButtonComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  describe('Setup', () => {
    it('should create the component', () => {
      expect(component).toBeTruthy();
    });

    it('should initialize default inputs', () => {
      expect(component.caption()).toBe('core.save');
      expect(component.tooltip()).toBe('');
      expect(component.iconPrefix()).toBe('fal');
      expect(component.inline()).toBe(false);
      expect(component.disabled()).toBe(false);
      expect(component.dirty()).toBe(undefined);
      expect(component.invalid()).toBe(false);
      expect(component.disableKeyboardSave()).toBe(false);
      expect(component.inProgress()).toBe(false);
    });

    // it('should initialize typeClass based on behaviour', () => {
    //   componentRef.setInput('behaviour', 'primary');
    //   fixture.detectChanges();
    //   expect(component.typeClass()).toBe('btn btn-primary');
    // });

    it('should compute saveIsDisabled based on dirty, inProgress, behaviour, and disabled', () => {
      componentRef.setInput('dirty', false);
      componentRef.setInput('inProgress', true);
      fixture.detectChanges();
      expect(component.isDisabled()).toBe(true);

      componentRef.setInput('dirty', true);
      componentRef.setInput('inProgress', false);
      fixture.detectChanges();
      expect(component.isDisabled()).toBe(false);
    });

    it('should set icon if iconName is provided on init', () => {
      const mockIcon: IconProp = { prefix: 'fal', iconName: 'check' };
      vi.spyOn(IconUtil, 'createIcon').mockReturnValue(mockIcon);

      componentRef.setInput('iconName', 'check');
      component.ngOnInit();

      expect(IconUtil.createIcon).toHaveBeenCalledWith('fal', 'check');
      expect(component.icon()).toEqual(mockIcon);
    });

    it('should bind keyboard shortcut on init', () => {
      component.ngOnInit();
      expect(shortcutService.bindShortcut).toHaveBeenCalled();
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

    describe('onValidationErrorsAction', () => {
      it('should emit validationErrorsAction on onValidationErrorsAction', () => {
        const validationSpy = vi.spyOn(
          component.validationErrorsAction,
          'emit'
        );
        const mockEvent = new Event('error');

        component.onValidationErrorsAction(mockEvent);
        expect(validationSpy).toHaveBeenCalledWith(mockEvent);
      });
    });

    // describe('getTypeClass', () => {

    //   it('should return correct class based on behaviour in getTypeClass', () => {
    //     componentRef.setInput('behaviour', 'standard');
    //     expect(component['getTypeClass']()).toBe('btn btn-secondary');

    //     componentRef.setInput('behaviour', 'primary');
    //     expect(component['getTypeClass']()).toBe('btn btn-primary');

    //     componentRef.setInput('behaviour', 'danger');
    //     expect(component['getTypeClass']()).toBe('btn btn-danger');

    //     componentRef.setInput('behaviour', 'close');
    //     expect(component['getTypeClass']()).toBe('btn-close close float-end');
    //   });
    // });

    describe('handleKeyboardSave', () => {
      it('should handle keyboard save correctly', () => {
        const onActionSpy = vi.spyOn(component, 'onAction');
        const mockKeyboardEvent = new KeyboardEvent('keydown', {
          key: 's',
          ctrlKey: true,
        });

        // Set component state for a valid save action
        componentRef.setInput('disableKeyboardSave', false);
        componentRef.setInput('disabled', false);
        componentRef.setInput('invalid', false);
        componentRef.setInput('dirty', true);
        componentRef.setInput('inProgress', false);
        fixture.detectChanges();

        component['handleKeyboardSave'](mockKeyboardEvent);
        expect(onActionSpy).toHaveBeenCalled();
        expect(onActionSpy).toHaveBeenCalledWith(mockKeyboardEvent);
      });

      it('should not handle keyboard save if conditions are not met', () => {
        const onActionSpy = vi.spyOn(component, 'onAction');
        const mockKeyboardEvent = new KeyboardEvent('keydown', {
          key: 's',
          ctrlKey: true,
        });

        // Set component state where save should not trigger
        componentRef.setInput('disabled', true);
        fixture.detectChanges();

        component['handleKeyboardSave'](mockKeyboardEvent);

        expect(onActionSpy).not.toHaveBeenCalled();
      });
    });
  });
  describe('DOM', () => {
    describe('Standard Button', () => {
      it('should display the standard button when not invalid and behaviour is not "close"', () => {
        componentRef.setInput('invalid', false);
        // componentRef.setInput('behaviour', 'primary');
        fixture.detectChanges();

        const button = fixture.debugElement.query(
          By.css('button[type="button"]')
        );
        expect(button).toBeTruthy();
      });

      it('should apply "is-disabled" class based on isDisabled', () => {
        componentRef.setInput('invalid', false);
        vi.spyOn(component, 'isDisabled').mockReturnValue(true);
        fixture.detectChanges();
        // const typeClasses = component.typeClass().split(' ');  // Create array of classes from typeClass()

        const button = fixture.debugElement.query(
          By.css('button[type="button"]')
        );
        // typeClasses.forEach(classname => {
        //   expect(button.classes[classname]).toBeTruthy();
        // });
        expect(button.classes['is-disabled']).toBeTruthy();
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
        componentRef.setInput('invalid', false);
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
        componentRef.setInput('invalid', false);
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

    describe('Button Group (Invalid State)', () => {
      it('should display button group when invalid is true', () => {
        componentRef.setInput('invalid', true);
        fixture.detectChanges();

        const buttonGroup = fixture.debugElement.query(
          By.css('.btn-group[role="group"]')
        );
        expect(buttonGroup).toBeTruthy();
      });

      it('should apply "is-disabled" class on primary button in button group', () => {
        componentRef.setInput('invalid', true);
        fixture.detectChanges();

        const primaryButton = fixture.debugElement.query(
          By.css('.btn-group button:first-child')
        );
        expect(primaryButton.classes['is-disabled']).toBeTruthy();
      });

      it('should render a secondary "!" button with btn-invalid class', () => {
        componentRef.setInput('invalid', true);
        fixture.detectChanges();

        const errorButton = fixture.debugElement.query(
          By.css('.btn-group .btn-invalid')
        );
        expect(errorButton).toBeTruthy();
        expect(errorButton.nativeElement.textContent.trim()).toBe('!');
      });

      it('should emit validationErrorsAction when "!" button is clicked', () => {
        const validationSpy = vi.spyOn(component, 'onValidationErrorsAction');
        componentRef.setInput('invalid', true);
        fixture.detectChanges();

        const errorButton = fixture.debugElement.query(
          By.css('.btn-group .btn-invalid')
        );
        errorButton.nativeElement.click();
        expect(validationSpy).toHaveBeenCalled();
      });
    });
  });
});
