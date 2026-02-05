import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MenuButtonComponent, MenuButtonItem } from './menu-button.component';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import {
  FontAwesomeModule,
  FaIconLibrary,
} from '@fortawesome/angular-fontawesome';
import { fal } from '@fortawesome/pro-light-svg-icons';
import { far } from '@fortawesome/pro-regular-svg-icons';
import { fas } from '@fortawesome/pro-solid-svg-icons';
import { ComponentRef } from '@angular/core';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('MenuButtonComponent', () => {
  let component: MenuButtonComponent;
  let componentRef: ComponentRef<MenuButtonComponent>;
  let fixture: ComponentFixture<MenuButtonComponent>;
  let library: FaIconLibrary;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed, FontAwesomeModule, MenuButtonComponent],
    }).compileComponents();
    library = TestBed.inject(FaIconLibrary);
    library.addIconPacks(fal, far, fas);

    fixture = TestBed.createComponent(MenuButtonComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setups', () => {
    it('should setup component properties with default values', () => {
      expect(component.caption()).toBe('');
      expect(component.variant()).toBe('menu');
      expect(component.behaviour()).toBe('primary');
      expect(component.insideGroup()).toBe(false);
      expect(component.dropUp()).toBe(false);
      expect(component.dropLeft()).toBe(false);
      expect(component.disabled()).toBe(false);
      expect(component.list()).toEqual([]);
      expect(component.selectedItem()).toBeUndefined();
    });
  });
  describe('methods', () => {
    describe('selectItem', () => {
      it('should emit itemSelected and close list on selectItem', () => {
        vi.spyOn(component.itemSelected, 'emit');
        const item: MenuButtonItem = { id: 1, label: 'Item 1' };
        component.selectItem(item);
        expect(component.itemSelected.emit).toHaveBeenCalledWith(item);
        expect(component.isOpen()).toBe(false);
      });
    });
    describe('closeList', () => {
      it('should close the list on closeList', () => {
        component.isOpen.set(true);
        component.closeList();
        expect(component.isOpen()).toBe(false);
      });
    });
    describe('updateOption', () => {
      it('should update selectedItem and close list on updateOption', () => {
        const item: MenuButtonItem = { id: 1, label: 'Item 1' };
        vi.spyOn(component, 'selectItem');
        component.updateOption(item);
        expect(component.selectedItem()).toBe(item);
        expect(component.isOpen()).toBe(false);
        expect(component.selectItem).toHaveBeenCalledWith(item);
      });
    });
    describe('onButtonAction', () => {
      it('should call selectItem or toggleList on onButtonAction based on variant', () => {
        vi.spyOn(component, 'selectItem');
        vi.spyOn(component, 'toggleList');
        componentRef.setInput('variant', 'split');
        component.onButtonAction();
        expect(component.selectItem).toHaveBeenCalled();
        componentRef.setInput('variant', 'menu');
        component.onButtonAction();
        expect(component.toggleList).toHaveBeenCalled();
      });
    });
    describe('toggleList', () => {
      it('should toggle list state on toggleList', () => {
        component.isOpen.set(false);
        component.toggleList();
        expect(component.isOpen()).toBe(true);
        component.toggleList();
        expect(component.isOpen()).toBe(false);
      });
    });
  });
  describe('DOM', () => {
    describe('soe-menu-button-wrapper', () => {
      it('should setup the DOM with default values and change accordingly', () => {
        const menuButtonWrapper = fixture.debugElement.query(
          By.css('.soe-menu-button-wrapper')
        );

        expect(menuButtonWrapper.classes['dropup']).toBeFalsy();
        expect(menuButtonWrapper.classes['dropleft']).toBeFalsy();

        componentRef.setInput('dropUp', true);
        componentRef.setInput('dropLeft', true);
        fixture.detectChanges();
        expect(menuButtonWrapper.classes['dropup']).toBe(true);
        expect(menuButtonWrapper.classes['dropleft']).toBe(true);
      });
      it('should trigger closeList when clickOutside is triggered', () => {
        vi.spyOn(component, 'closeList');
        const menuButtonWrapper = fixture.debugElement.query(
          By.css('.soe-menu-button-wrapper')
        );
        menuButtonWrapper.triggerEventHandler('clickOutside', null);
        expect(component.closeList).toHaveBeenCalled();
      });
    });
    describe('soe-menu-button', () => {
      describe('menu button', () => {
        it('should setup the DOM with default values and change accordingly', () => {
          const menuButton = fixture.debugElement.query(
            By.css('.soe-menu-button')
          );
          expect(menuButton.classes['inside-group']).toBeFalsy();
          expect(menuButton.attributes['disabled']).toBeFalsy();

          componentRef.setInput('insideGroup', true);
          componentRef.setInput('disabled', true);
          fixture.detectChanges();

          expect(menuButton.classes['inside-group']).toBe(true);
          expect(menuButton.nativeElement.disabled).toBe(true);
        });
        it('should trigger onButtonAction() when button is clicked', () => {
          vi.spyOn(component, 'toggleList');
          const menuButton = fixture.debugElement.query(
            By.css('.soe-menu-button')
          );
          menuButton.triggerEventHandler('click', null);
          expect(component.toggleList).toHaveBeenCalled();
        });
      });
      describe('split button', () => {
        it('should render the second button when variant is split with default values and change accordingly', () => {
          componentRef.setInput('variant', 'split');
          fixture.detectChanges();

          const splitButton = fixture.debugElement.query(
            By.css('.soe-menu-button__split')
          );
          expect(splitButton.classes['inside-group']).toBeFalsy();
          expect(splitButton.attributes['disabled']).toBeFalsy();

          componentRef.setInput('insideGroup', true);
          componentRef.setInput('disabled', true);
          fixture.detectChanges();

          expect(splitButton).toBeTruthy();
          expect(splitButton.classes['inside-group']).toBe(true);
          expect(splitButton.nativeElement.disabled).toBe(true);
        });
        it('should call toggleList when the split button is clicked', () => {
          componentRef.setInput('variant', 'split');
          fixture.detectChanges();

          vi.spyOn(component, 'toggleList');
          const splitButton = fixture.debugElement.query(
            By.css('.soe-menu-button__split')
          );

          splitButton.triggerEventHandler('click', null);
          expect(component.toggleList).toHaveBeenCalled();
        });
      });
    });
    describe('soe-menu-button-list', () => {
      let listElement: any;
      beforeEach(() => {
        component.isOpen.set(true);
        fixture.detectChanges();
        listElement = fixture.debugElement.query(
          By.css('.soe-menu-button__list')
        );
      });
      describe('dropdown visibility', () => {
        it('should render the list when isOpen is true', () => {
          expect(listElement).toBeTruthy();
        });

        it('should not render the list when isOpen is false', () => {
          component.isOpen.set(false);
          fixture.detectChanges();

          listElement = fixture.debugElement.query(
            By.css('.soe-menu-button__list')
          );
          expect(listElement).toBeFalsy();
        });
      });
      describe('dropdown list classes', () => {
        it('should apply dropup-content class if dropUp returns true', () => {
          componentRef.setInput('dropUp', true);
          fixture.detectChanges();

          expect(listElement.classes['dropup-content']).toBe(true);
        });

        it('should apply dropleft-content class if dropLeft returns true', () => {
          componentRef.setInput('dropLeft', true);
          fixture.detectChanges();

          expect(listElement.classes['dropleft-content']).toBe(true);
        });
      });
      describe('List items rendering', () => {
        it('should render list items that are not hidden', () => {
          const mockItem = {
            label: 'Item 1',
            type: 'item-type-class',
            hidden: vi.fn(() => false), // Not hidden
            disabled: vi.fn(() => false),
            icon: ['fal', 'home'],
            iconClass: 'custom-icon-class',
          };
          componentRef.setInput('list', [mockItem]);
          fixture.detectChanges();
          const listDebugElement = fixture.debugElement.query(By.css('li'));
          expect(listDebugElement).toBeTruthy();
        });

        it('should not render list items that are hidden', () => {
          const mockItem = {
            label: 'Item 1',
            type: 'item-type-class',
            hidden: vi.fn(() => true), // Hidden
            disabled: vi.fn(() => false),
            icon: ['fal', 'home'],
            iconClass: 'custom-icon-class',
          };
          componentRef.setInput('list', [mockItem]);
          fixture.detectChanges();

          const listDebugElement = fixture.debugElement.query(By.css('li'));
          expect(listDebugElement).toBeFalsy();
        });

        it('should apply disabled class if item is disabled', () => {
          const mockItem = {
            label: 'Item 1',
            type: 'item-type-class',
            hidden: vi.fn(() => false),
            disabled: vi.fn(() => true),
            icon: ['fal', 'home'],
            iconClass: 'custom-icon-class',
          };
          componentRef.setInput('list', [mockItem]);
          fixture.detectChanges();
          const listDebugElement = fixture.debugElement.query(By.css('li'));

          expect(listDebugElement.classes['disabled']).toBe(true);
        });

        it('should not apply disabled class if item is not disabled', () => {
          const mockItem = {
            label: 'Item 1',
            type: 'item-type-class',
            hidden: vi.fn(() => false),
            disabled: vi.fn(() => false),
            icon: ['fal', 'home'],
            iconClass: 'custom-icon-class',
          };
          componentRef.setInput('list', [mockItem]);
          fixture.detectChanges();
          const listDebugElement = fixture.debugElement.query(By.css('li'));

          expect(listDebugElement.classes['disabled']).toBeFalsy();
        });

        it('should call selectItem when an item is clicked and variant is not split', () => {
          const mockItem = {
            label: 'Item 1',
            type: 'item-type-class',
            hidden: vi.fn(() => false), // Not hidden
            disabled: vi.fn(() => false),
            icon: ['fal', 'home'],
            iconClass: 'custom-icon-class',
          };
          componentRef.setInput('list', [mockItem]);
          componentRef.setInput('variant', 'normal'); // Non-split variant
          vi.spyOn(component, 'selectItem');
          fixture.detectChanges();
          const listDebugElement = fixture.debugElement.query(By.css('li'));

          listDebugElement.triggerEventHandler('click', null);

          expect(component.selectItem).toHaveBeenCalledWith(mockItem);
        });

        it('should call updateOption when an item is clicked and variant is split', () => {
          const mockItem = {
            label: 'Item 1',
            type: 'item-type-class',
            hidden: vi.fn(() => false), // Not hidden
            disabled: vi.fn(() => false),
            icon: ['fal', 'home'],
            iconClass: 'custom-icon-class',
          };
          componentRef.setInput('list', [mockItem]);
          componentRef.setInput('variant', 'split'); // Split variant
          vi.spyOn(component, 'updateOption');
          fixture.detectChanges();
          const listDebugElement = fixture.debugElement.query(By.css('li'));

          listDebugElement.triggerEventHandler('click', null);

          expect(component.updateOption).toHaveBeenCalledWith(mockItem);
        });
      });
      describe('Item icon rendering', () => {
        const mockItemWithIcon = {
          label: 'Item with Icon',
          icon: ['fal', 'home'], // FontAwesome icon
          iconClass: 'custom-icon-class',
          hidden: vi.fn(() => false),
          disabled: vi.fn(() => false),
        };
        let iconElement: any;

        beforeEach(() => {
          componentRef.setInput('list', [mockItemWithIcon]);
          fixture.detectChanges();
          iconElement = fixture.debugElement.query(
            By.css('.soe-menu-button__list-icon')
          );
        });

        it('should render the icon if item has an icon', () => {
          expect(iconElement).toBeTruthy();
          expect(iconElement.classes['custom-icon-class']).toBe(true);

          const iconInstance = iconElement.componentInstance;
          expect(iconInstance.icon()).toBe(mockItemWithIcon.icon);
        });

        it('should not render the icon if item has no icon', () => {
          mockItemWithIcon.icon = null as any; // Set icon to null
          fixture.detectChanges();

          iconElement = fixture.debugElement.query(
            By.css('.soe-menu-button__list-icon')
          );
          expect(iconElement).toBeFalsy(); // Icon should not render
        });
      });
    });
  });
});
