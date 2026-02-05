import {
  ComponentFixture,
  fakeAsync,
  TestBed,
  tick,
} from '@angular/core/testing';

import { TabGroupComponent } from './tab-group.component';
import {
  ComponentRef,
  CUSTOM_ELEMENTS_SCHEMA,
  DebugElement,
  NO_ERRORS_SCHEMA,
  QueryList,
} from '@angular/core';
import { of } from 'rxjs';
import { TabComponent } from '../tab/tab.component';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';

describe('TabGroupComponent', () => {
  let component: TabGroupComponent;
  let fixture: ComponentFixture<TabGroupComponent>;
  let componentRef: ComponentRef<TabGroupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TabGroupComponent, TabComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(TabGroupComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should setup with default values', () => {
      expect(component.activeIndex()).toEqual(0);
      expect(component.hideAdd()).toBe(false);
      expect(component.hideCloseAll()).toBe(false);
      expect(component.preventMultipleNewTabs()).toBe(false);
      expect(component.disableKeyboardNew()).toBe(false);

      expect(component['messageboxService']).toBeTruthy();
      expect(component['subscriptionEnableTabAdd$']).toBeTruthy();
      expect(component.hasAddPermission()).toBe(false);
      expect(component.disabledCloseAll()).toBe(true);

      expect(component['wasDeleteAction']).toBe(false);
      expect(component['destroy$']).toBeTruthy();
      expect(component['prevTabsLength']).toEqual(0);

      expect(component.hasScrollbar()).toBe(false);
    });
  });
  describe('methods', () => {
    describe('ngOnChanges', () => {
      it('should updateTabIndex if activeIndex changes', () => {
        vi.spyOn(component, 'updateTabIndex');
        component.ngOnChanges({
          activeIndex: {
            previousValue: 0,
            currentValue: 1,
            firstChange: false,
            isFirstChange: vi.fn(),
          },
        } as any);
        expect(component.updateTabIndex).toHaveBeenCalled();
      });
      it('should not updateTabIndex if activeIndex does not exist', () => {
        vi.spyOn(component, 'updateTabIndex');
        component.ngOnChanges({} as any);
        expect(component.updateTabIndex).not.toHaveBeenCalled();
      });
    });
    describe('ngAfterContentInit', () => {
      it('should updateTabIndex', () => {
        vi.spyOn(component, 'updateTabIndex');
        component.ngAfterContentInit();
        expect(component.updateTabIndex).toHaveBeenCalled();
      });
    });
    describe('wasLastTabRemoved', () => {
      it('should return true if wasDeleteAction is true', () => {
        component['wasDeleteAction'] = true;
        expect(component.wasLastTabRemoved()).toBe(true);
      });
      it('should return true if prevTabsLength > tabs.length', () => {
        component['wasDeleteAction'] = false;
        component['prevTabsLength'] = 2;
        component.tabs = { length: 1 } as any;
        expect(component.wasLastTabRemoved()).toBe(true);
      });
      it('should return false if wasDeleteAction is false and prevTabsLength <= tabs.length', () => {
        component['wasDeleteAction'] = false;
        component['prevTabsLength'] = 2;
        component.tabs = { length: 2 } as any;
        expect(component.wasLastTabRemoved()).toBe(false);
      });
    });
    describe('selectTab', () => {
      it('should set activeIndex and call updateTabIndex', () => {
        vi.spyOn(component, 'updateTabIndex');
        component.selectTab(1);
        expect(component.activeIndex()).toEqual(1);
        expect(component.updateTabIndex).toHaveBeenCalled();
      });
    });
    describe('addTab', () => {
      it('should emit tabAdded', () => {
        vi.spyOn(component.tabAdded, 'emit');
        component.addTab();
        expect(component.tabAdded.emit).toHaveBeenCalled();
      });
    });
    describe('removeAllTabs', () => {
      it('should close all tabs immediately if none are dirty', () => {
        vi.spyOn(component.allTabsRemoved, 'emit');
        vi.spyOn(component['messageboxService'], 'warning');
        component.tabs = [
          { isDirty: () => false },
          { isDirty: () => false },
        ] as any;
        component.removeAllTabs();
        expect(component['messageboxService'].warning).not.toHaveBeenCalled();
        expect(component.allTabsRemoved.emit).toHaveBeenCalled();
      });
      it('should open a confirmation dialog if any tabs are dirty', () => {
        vi.spyOn(component.allTabsRemoved, 'emit');
        vi.spyOn(component['messageboxService'], 'warning');
        component.tabs = [
          { isDirty: () => true },
          { isDirty: () => false },
        ] as any;
        component.removeAllTabs();
        expect(component['messageboxService'].warning).toHaveBeenCalledWith(
          'core.warning',
          'core.confirmonclosetabs'
        );
        expect(component.allTabsRemoved.emit).not.toHaveBeenCalled();
      });
      it('should close all tabs if the warning returns true', () => {
        component.tabs = [
          { isDirty: () => true },
          { isDirty: () => false },
        ] as any;

        vi.spyOn(component.allTabsRemoved, 'emit');
        vi.spyOn(component['messageboxService'], 'warning').mockReturnValue({
          afterClosed: vi.fn().mockImplementation(callback => {
            return of({ result: true });
          }),
        } as any);

        component.removeAllTabs();

        expect(component['messageboxService'].warning).toHaveBeenCalledWith(
          'core.warning',
          'core.confirmonclosetabs'
        );
        expect(component.allTabsRemoved.emit).toHaveBeenCalled();
      });
    });
    describe('removeTab', () => {
      it('should call onConfirmCloseTab and emit tabRemoved when the tab is not dirty', () => {
        const event = new Event('click');
        const index = 1;

        let tab1 = TestBed.createComponent(TabComponent).componentInstance;
        let tab2 = TestBed.createComponent(TabComponent).componentInstance;

        tab1 = { label: 'Tab1', isDirty: () => false } as any;
        tab2 = { label: 'Tab2', isDirty: () => false } as any;

        component.tabs.reset([tab1, tab2]);

        vi.spyOn(event, 'stopPropagation');
        vi.spyOn(component.tabRemoved, 'emit');

        component.removeTab(event, index);

        expect(event.stopPropagation).toHaveBeenCalled();
        expect(component.tabRemoved.emit).toHaveBeenCalledWith(1);
        expect(component['wasDeleteAction']).toBe(true);
      });
      it('should call onConfirmCloseTab when the message box response is positive', () => {
        const event = new Event('click');
        const index = 1;

        let tab1 = TestBed.createComponent(TabComponent).componentInstance;
        let tab2 = TestBed.createComponent(TabComponent).componentInstance;

        tab1 = { label: 'Tab1', isDirty: () => true } as any;
        tab2 = { label: 'Tab2', isDirty: () => true } as any;

        component.tabs.reset([tab1, tab2]);

        vi.spyOn(component['messageboxService'], 'warning').mockReturnValue({
          afterClosed: vi.fn().mockImplementation(callback => {
            return of({ result: true });
          }),
        } as any);
        vi.spyOn(event, 'stopPropagation');
        vi.spyOn(component.tabRemoved, 'emit');

        component.removeTab(event, index);

        expect(component['messageboxService'].warning).toHaveBeenCalledWith(
          'core.warning',
          'core.confirmonclosetab'
        );
        expect(event.stopPropagation).toHaveBeenCalled();
        expect(component.tabRemoved.emit).toHaveBeenCalledWith(1);
        expect(component['wasDeleteAction']).toBe(true);
      });

      it('should not call onConfirmCloseTab when the message box response is negative', () => {
        const event = new Event('click');
        const index = 1;

        let tab1 = TestBed.createComponent(TabComponent).componentInstance;
        let tab2 = TestBed.createComponent(TabComponent).componentInstance;

        tab1 = { label: 'Tab1', isDirty: () => true } as any;
        tab2 = { label: 'Tab2', isDirty: () => true } as any;

        component.tabs.reset([tab1, tab2]);

        vi.spyOn(component['messageboxService'], 'warning').mockReturnValue({
          afterClosed: vi.fn().mockImplementation(callback => {
            return of({ result: false });
          }),
        } as any);
        vi.spyOn(event, 'stopPropagation');
        vi.spyOn(component.tabRemoved, 'emit');

        component.removeTab(event, index);

        expect(component['messageboxService'].warning).toHaveBeenCalledWith(
          'core.warning',
          'core.confirmonclosetab'
        );
        expect(event.stopPropagation).not.toHaveBeenCalled();
        expect(component.tabRemoved.emit).not.toHaveBeenCalled();
        expect(component['wasDeleteAction']).toBe(false);
      });
    });
    describe('updateTabIndex', () => {
      it('should set isActive to true for the active tab', fakeAsync(() => {
        const tab1 = TestBed.createComponent(TabComponent).componentInstance;
        const tab2 = TestBed.createComponent(TabComponent).componentInstance;

        tab1.isActive.set(false);
        tab2.isActive.set(false);

        component.tabs.reset([tab1, tab2]);
        component.activeIndex.set(1);
        tick(30);

        vi.spyOn(component.tabIndexChanged, 'emit');

        component.updateTabIndex();
        tick(30);

        expect(component.tabIndexChanged.emit).toHaveBeenCalledWith(1);
        expect(tab1.isActive()).toBe(false);
        expect(tab2.isActive()).toBe(true);
      }));
    });
    describe('tabDoubleClick', () => {
      it('should increment doubleClickCount and not emit tabDblClicked if count is 0', () => {
        const tab = TestBed.createComponent(TabComponent).componentInstance;

        tab.doubleClickCount.set(0);

        vi.spyOn(component.tabDblClicked, 'emit');

        component.tabDoubleClick(tab);

        expect(tab.doubleClickCount()).toBe(1);
        expect(component.tabDblClicked.emit).not.toHaveBeenCalled();
      });
      it('should emit tabDblClicked and reset doubleClickCount if count is 1', () => {
        const tab = TestBed.createComponent(TabComponent).componentInstance;

        tab.doubleClickCount.set(1);
        component.activeIndex.set(0);

        vi.spyOn(component.tabDblClicked, 'emit');
        vi.spyOn(tab.doubleClickCount, 'set');

        component.tabDoubleClick(tab);

        expect(tab.doubleClickCount()).toBe(0);
        expect(component.tabDblClicked.emit).toHaveBeenCalledWith(0);
      });
    });
    describe('handleKeyboardNew', () => {
      it('should not addTab if disableKeyboardNew is true', () => {
        componentRef.setInput('disableKeyboardNew', true);
        vi.spyOn(component, 'addTab');
        component.handleKeyboardNew();
        expect(component.addTab).not.toHaveBeenCalled();
      });
      it('should addTab if disableKeyboardNew is false', () => {
        component.hasAddPermission.set(true);
        componentRef.setInput('disableKeyboardNew', false);
        componentRef.setInput('preventNewTabs', false);
        vi.spyOn(component, 'addTab');
        component.handleKeyboardNew();
        expect(component.addTab).toHaveBeenCalled();
      });
      it('should not emit tabAdded if disableKeyboardNew is true', () => {
        componentRef.setInput('disableKeyboardNew', true);
        componentRef.setInput('preventMultipleNewTabs', false);
        fixture.detectChanges();
        vi.spyOn(component, 'addTab');
        component.handleKeyboardNew();
        expect(component.addTab).not.toHaveBeenCalled();
      });
    });
  });
  describe('DOM', () => {
    describe('soe-tab-group', () => {
      let mockTabList: any;
      beforeEach(() => {
        mockTabList = new QueryList<any>();

        // Create mock signal functions for doubleClickCount
        const doubleClickCount1 = vi.fn().mockReturnValue(0);
        doubleClickCount1.set = vi.fn();

        const doubleClickCount2 = vi.fn().mockReturnValue(0);
        doubleClickCount2.set = vi.fn();

        mockTabList.reset([
          {
            label: vi.fn().mockReturnValue('Tab 1'),
            isDirty: vi.fn().mockReturnValue(false),
            closable: vi.fn().mockReturnValue(true),
            disabled: vi.fn().mockReturnValue(false),
            doubleClickCount: doubleClickCount1,
          },
          {
            label: vi.fn().mockReturnValue('Tab 2'),
            isDirty: vi.fn().mockReturnValue(true),
            closable: vi.fn().mockReturnValue(false),
            disabled: vi.fn().mockReturnValue(false),
            doubleClickCount: doubleClickCount2,
          },
        ]);

        component.tabs = mockTabList; // Assign the mock QueryList to the component

        componentRef.setInput('hideAdd', true);
        componentRef.setInput('hideCloseAll', true);

        fixture.detectChanges();
      });
      it('should render tab titles with translation', () => {
        const tabButtons = fixture.debugElement.queryAll(By.css('.soe-tab'));
        expect(tabButtons.length).toBe(2);

        const firstTab = tabButtons[0];
        const label = firstTab.query(
          By.css('.soe-tab__label span')
        ).nativeElement;
        expect(label.innerHTML).toBe('Tab 1');
      });
      it('should set active class on the selected tab', () => {
        const tabButtons = fixture.debugElement.queryAll(By.css('.soe-tab'));
        expect(tabButtons[0].classes['selected']).toBeTruthy();
        expect(tabButtons[1].classes['selected']).toBeFalsy();
      });
      it('should call selectTab when a tab is clicked', () => {
        vi.spyOn(component, 'selectTab');
        const secondTab = fixture.debugElement.queryAll(By.css('.soe-tab'))[1];

        secondTab.triggerEventHandler('click', null);
        expect(component.selectTab).toHaveBeenCalledWith(1);
      });
      it('should show dirty icon if tab is dirty', () => {
        const secondTab = fixture.debugElement.queryAll(By.css('.soe-tab'))[1];
        const dirtyIcon = secondTab.query(By.css('.soe-tab-icon-dirty'));
        expect(dirtyIcon).toBeTruthy();
      });
      it('should call tabDoubleClick when the label is double-clicked', () => {
        vi.spyOn(component, 'tabDoubleClick');
        const firstTabLabel = fixture.debugElement.queryAll(
          By.css('.soe-tab__label')
        )[0];

        firstTabLabel.triggerEventHandler('dblclick', null);
        expect(component.tabDoubleClick).toHaveBeenCalledWith(
          component.tabs.get(0)
        );
      });
      it('should call removeTab when the close button is clicked', () => {
        vi.spyOn(component, 'removeTab');
        const firstTab = fixture.debugElement.queryAll(By.css('.soe-tab'))[0];
        const closeButton = firstTab.query(By.css('button [tabindex="0"]'));

        const clickEvent = new Event('click');
        closeButton.triggerEventHandler('click', clickEvent);
        expect(component.removeTab).toHaveBeenCalledWith(clickEvent, 0); // Index 0
      });
      it('should not display close button if the tab is not closable', () => {
        const secondTab = fixture.debugElement.queryAll(By.css('.soe-tab'))[1];
        const closeButton = secondTab.query(By.css('button [tabindex="0"]'));
        expect(closeButton).toBeFalsy(); // No close button for non-closable tab
      });
    });
    describe('add tab button', () => {
      let addButtonDebug: any;
      beforeEach(() => {
        componentRef.setInput('hideAdd', false);
        addButtonDebug = fixture.debugElement.query(
          By.css('.soe-tab__add-tab-button')
        );
        fixture.detectChanges();
      });
      it('should render addbutton if hideAdd() returns false', () => {
        expect(addButtonDebug).toBeTruthy();
      });
      it('should not render add button if hideAdd() returns true', () => {
        componentRef.setInput('hideAdd', true);

        fixture.detectChanges();

        const addButton = fixture.debugElement.query(
          By.css('.soe-tab__add-tab-button')
        );
        expect(addButton).toBeFalsy();
      });
      it('should call addTab when the button is clicked', () => {
        vi.spyOn(component, 'addTab');

        addButtonDebug.triggerEventHandler('click', null);
        expect(component.addTab).toHaveBeenCalled();
      });
      it('should be disabled if hasAddPermission is false and preventNewTabs is true', () => {
        component.hasAddPermission.set(false);
        componentRef.setInput('preventNewTabs', true);
        fixture.detectChanges();
        expect(addButtonDebug.nativeElement.disabled).toBe(true);
      });
      it('should be disabled if hasAddPermission is false', () => {
        component.hasAddPermission.set(false);
        componentRef.setInput('preventNewTabs', true);
        fixture.detectChanges();
        expect(addButtonDebug.nativeElement.disabled).toBe(true);
      });
      it('should not be disabled if preventNewTabs is false and hasAddPermission is true', () => {
        component.hasAddPermission.set(true);
        componentRef.setInput('preventNewTabs', true);
        fixture.detectChanges();
        expect(addButtonDebug.nativeElement.disabled).toBe(false);
      });
      it('should render FontAwesome faPlus icon inside the add button', () => {
        const faIcon = addButtonDebug.query(By.css('.fa-plus'));
        expect(faIcon).toBeTruthy();
      });
    });
    describe('close all button', () => {
      let closeAllButtonDebug: DebugElement;
      beforeEach(() => {
        componentRef.setInput('hideCloseAll', false);
        componentRef.setInput('hideAdd', true);
        fixture.detectChanges();
        closeAllButtonDebug = fixture.debugElement.query(
          By.css('.soe-tab.slim')
        );
      });
      it('should render closeAllButton if hideCloseAll() returns false', () => {
        expect(closeAllButtonDebug).toBeTruthy();
      });
      it('should not render closeAll button if hideCloseAll() returns true', () => {
        componentRef.setInput('hideCloseAll', true);

        fixture.detectChanges();

        const closeAllButton = fixture.debugElement.query(
          By.css('.soe-tab__close-all-button')
        );
        expect(closeAllButton).toBeFalsy();
      });
      it('should call removeAllTabs when the button is clicked', () => {
        vi.spyOn(component, 'removeAllTabs');

        closeAllButtonDebug.triggerEventHandler('click', null);
        expect(component.removeAllTabs).toHaveBeenCalled();
      });
      it('should be disabled if disabledCloseAll() is true', () => {
        componentRef.setInput('disabledCloseAll', true);
        fixture.detectChanges();
        expect(closeAllButtonDebug.nativeElement.disabled).toBe(true);
      });
      it('should not be disabled if disabledCloseAll() is false', () => {
        component.disabledCloseAll.set(false);
        fixture.detectChanges();
        expect(closeAllButtonDebug.nativeElement.disabled).toBe(false);
      });
      it('should render FontAwesome faAsterisk icon inside the close all button', () => {
        const faIcon = closeAllButtonDebug.query(By.css('.soe-tab__icon'));
        expect(faIcon).toBeTruthy();
      });
    });
  });
});
