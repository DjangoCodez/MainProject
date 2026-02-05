import { ComponentFixture, TestBed } from '@angular/core/testing';
import { IconButtonComponent } from './icon-button.component';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import {
  FontAwesomeModule,
  FaIconLibrary,
} from '@fortawesome/angular-fontawesome';
import { fal } from '@fortawesome/pro-light-svg-icons';
import { far } from '@fortawesome/pro-regular-svg-icons';
import { fas } from '@fortawesome/pro-solid-svg-icons';
import { ComponentRef, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('IconButtonComponent', () => {
  let component: IconButtonComponent;
  let componentRef: ComponentRef<IconButtonComponent>;
  let fixture: ComponentFixture<IconButtonComponent>;
  let library: FaIconLibrary;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, FontAwesomeModule, IconButtonComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA], // Add CUSTOM_ELEMENTS_SCHEMA
    }).compileComponents();

    library = TestBed.inject(FaIconLibrary);
    library.addIconPacks(fal, far, fas);

    fixture = TestBed.createComponent(IconButtonComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;

    componentRef.setInput('iconName', 'circle-info');

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setups', () => {
    it('should have default input values', () => {
      expect(component.iconPrefix()).toBe('fal');
      expect(component.iconClass()).toBe('');
      expect(component.tooltip()).toBe('');
      expect(component.noBorder()).toBe(false);
      expect(component.noMargins()).toBe(false);
      expect(component.transparent()).toBe(false);
      expect(component.disabled()).toBe(false);
      expect(component.outsideGroup()).toBe(false);
      expect(component.insideGroup()).toBe(false);
      expect(component.firstInGroup()).toBe(false);
      expect(component.lastInGroup()).toBe(false);
      expect(component.narrow()).toBe(false);
      expect(component.attachedToTextarea()).toBe(false);
      expect(component.attachedToTextbox()).toBe(false);
    });
  });
  describe('methods', () => {
    describe('performAction', () => {
      it('should emit action event', () => {
        vi.spyOn(component.action, 'emit');
        const event = new Event('click');
        component.performAction(event);
        expect(component.action.emit).toHaveBeenCalledWith(event);
      });
    });
  });
  describe('DOM', () => {
    describe('button', () => {
      let buttonElement: any;
      beforeEach(() => {
        // buttonElement = fixture.nativeElement.querySelector('button');
      });
      it('should render with default values', () => {
        expect(fixture.nativeElement.querySelector('button')).toBeTruthy();
        expect(
          fixture.nativeElement.querySelector('button.no-border')
        ).toBeFalsy();
        expect(
          fixture.nativeElement.querySelector('button.no-margins')
        ).toBeFalsy();
        expect(
          fixture.nativeElement.querySelector('button.transparent')
        ).toBeFalsy();
        expect(
          fixture.nativeElement.querySelector('button.outside-group')
        ).toBeFalsy();
        expect(
          fixture.nativeElement.querySelector('button.inside-group')
        ).toBeFalsy();
        expect(
          fixture.nativeElement.querySelector('button.first-in-group')
        ).toBeFalsy();
        expect(
          fixture.nativeElement.querySelector('button.last-in-group')
        ).toBeFalsy();
        expect(
          fixture.nativeElement.querySelector('button.narrow')
        ).toBeFalsy();
        expect(
          fixture.nativeElement.querySelector('button.textarea-btn')
        ).toBeFalsy();
        expect(
          fixture.nativeElement.querySelector('button.textbox-btn')
        ).toBeFalsy();
        expect(
          fixture.nativeElement.querySelector('button').disabled
        ).toBeFalsy();
        expect(fixture.nativeElement.querySelector('button').title).toBe('');
      });
      it('should render differently if component has other values', () => {
        componentRef.setInput('noBorder', true);
        componentRef.setInput('noMargins', true);
        componentRef.setInput('transparent', true);
        componentRef.setInput('outsideGroup', true);
        componentRef.setInput('insideGroup', true);
        componentRef.setInput('firstInGroup', true);
        componentRef.setInput('lastInGroup', true);
        componentRef.setInput('narrow', true);
        componentRef.setInput('attachedToTextarea', true);
        componentRef.setInput('attachedToTextbox', true);
        componentRef.setInput('disabled', true);
        componentRef.setInput('tooltip', 'Tooltip');
        fixture.detectChanges();

        expect(
          fixture.nativeElement.querySelector('button.no-border')
        ).toBeTruthy();
        expect(
          fixture.nativeElement.querySelector('button.no-margins')
        ).toBeTruthy();
        expect(
          fixture.nativeElement.querySelector('button.transparent')
        ).toBeTruthy();
        expect(
          fixture.nativeElement.querySelector('button.outside-group')
        ).toBeTruthy();
        expect(
          fixture.nativeElement.querySelector('button.inside-group')
        ).toBeTruthy();
        expect(
          fixture.nativeElement.querySelector('button.first-in-group')
        ).toBeTruthy();
        expect(
          fixture.nativeElement.querySelector('button.last-in-group')
        ).toBeTruthy();
        expect(
          fixture.nativeElement.querySelector('button.narrow')
        ).toBeTruthy();
        expect(
          fixture.nativeElement.querySelector('button.textarea-btn')
        ).toBeTruthy();
        expect(
          fixture.nativeElement.querySelector('button.textbox-btn')
        ).toBeTruthy();
        expect(
          fixture.nativeElement.querySelector('button.is-disabled')
        ).toBeTruthy();
        expect(fixture.nativeElement.querySelector('button').title).toBe(
          'Tooltip'
        );
      });
      it('should call performAction when not disabled', () => {
        vi.spyOn(component, 'performAction');
        vi.spyOn(component, 'disabled').mockReturnValue(false);

        const button = fixture.debugElement.query(By.css('button'));
        button.triggerEventHandler('click', new Event('click'));

        expect(component.performAction).toHaveBeenCalled();
      });
      it('should not call performAction when disabled', () => {
        vi.spyOn(component, 'performAction');
        vi.spyOn(component, 'disabled').mockReturnValue(true);

        const button = fixture.debugElement.query(By.css('button'));
        button.triggerEventHandler('click', new Event('click'));

        expect(component.performAction).not.toHaveBeenCalled();
      });
    });
    describe('fa-icon', () => {
      it('should have default DOM values', () => {
        expect(fixture.nativeElement.querySelector('fa-icon')).toBeTruthy();
        const faIconComponent = fixture.debugElement.query(
          By.css('fa-icon')
        ).componentInstance;
        expect(faIconComponent.icon()).toEqual(['fal', 'circle-info']);
        expect(
          fixture.debugElement.query(By.css('fa-icon')).attributes['class']
        ).toBe('ng-fa-icon');
      });
    });
  });
});
