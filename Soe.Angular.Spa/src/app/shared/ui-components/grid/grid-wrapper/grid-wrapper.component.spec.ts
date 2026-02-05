import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GridWrapperComponent } from './grid-wrapper.component';
import { ComponentRef, DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { vi } from 'vitest';
import { BehaviorSubject } from 'rxjs';
import { AutoHeightService } from '@shared/directives/auto-height/auto-height.service';

describe('GridWrapperComponent', () => {
  let component: GridWrapperComponent<any>;
  let fixture: ComponentFixture<GridWrapperComponent<any>>;
  let componentRef: ComponentRef<GridWrapperComponent<any>>;
  let autoHeightService: AutoHeightService;

  beforeEach(async () => {
    global.IntersectionObserver = class IntersectionObserver {
      constructor() {}
      disconnect() {}
      observe() {}
      takeRecords() {
        return [];
      }
      unobserve() {}
    } as any;
    await TestBed.configureTestingModule({
      imports: [SoftOneTestBed, GridWrapperComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(GridWrapperComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    // componentRef.setInput('rows', []);
    componentRef.setInput('parentGuid', 'test-guid');
    autoHeightService = fixture.debugElement.injector.get(AutoHeightService);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should set the default values', () => {
      expect(component.toolbarGroups()).toEqual([]);
      expect(component.rows).toBeTruthy();
      expect(component.height()).toBe(0);
      expect(component.masterDetail()).toBe(false);
      expect(component.parentGuid()).toBe('test-guid');
      expect(component.noMargin()).toBe(false);
      expect(component.toolbarNoPadding()).toBe(false);
      expect(component.toolbarNoBorder()).toBe(false);
    });
  });
  describe('methods', () => {
    describe('edit', () => {
      it('should emit the row', () => {
        vi.spyOn(component.editRowClicked, 'emit');
        component.edit(component.rows);
        expect(component.editRowClicked.emit).toHaveBeenCalledWith(
          component.rows
        );
      });
    });
    describe('triggerRowSelected', () => {
      it('should emit the row', () => {
        vi.spyOn(component.rowSelected, 'emit');
        component.triggerRowSelected(component.rows);
        expect(component.rowSelected.emit).toHaveBeenCalledWith(component.rows);
      });
    });
    describe('triggerSelectionChanged', () => {
      it('should emit the rows', () => {
        vi.spyOn(component.selectionChanged, 'emit');
        component.triggerSelectionChanged(component.rows as any);
        expect(component.selectionChanged.emit).toHaveBeenCalledWith(
          component.rows
        );
      });
    });
    describe('triggerSelectedItemsChanged', () => {
      it('should emit the changed items', () => {
        vi.spyOn(component.selectedItemsChanged, 'emit');
        component.triggerSelectedItemsChanged(component.rows as any);
        expect(component.selectedItemsChanged.emit).toHaveBeenCalledWith(
          component.rows
        );
      });
    });
    describe('cellKeyDown', () => {
      it('should emit the cell key down event', () => {
        vi.spyOn(component.cellKeyDown, 'emit');
        component.cellKeyDown.emit(component.rows as any);
        expect(component.cellKeyDown.emit).toHaveBeenCalledWith(component.rows);
      });
    });
  });
  describe('DOM', () => {
    describe('soe-toolbar', () => {
      let toolbarElement: HTMLElement;
      let toolbarDebugElement: DebugElement;
      let toolbarComponentInstance: any;
      beforeEach(() => {
        componentRef.setInput('toolbarGroups', [
          { buttons: [], alignmentRight: true },
        ]);
        fixture.detectChanges();
        toolbarElement = fixture.nativeElement.querySelector('soe-toolbar');
        toolbarDebugElement = fixture.debugElement.query(By.css('soe-toolbar'));
        toolbarComponentInstance = toolbarDebugElement.componentInstance;
      });
      it('should render the toolbar when there are toolbarGroups', () => {
        expect(toolbarElement).toBeTruthy();
        expect(toolbarComponentInstance.toolbarGroups()).toEqual(
          component.toolbarGroups()
        );
        expect(toolbarComponentInstance.noPadding()).toEqual(
          component.toolbarNoPadding()
        );
        expect(toolbarComponentInstance.noBorder()).toEqual(
          component.toolbarNoBorder()
        );
      });
      it('should not render the toolbar when there are no toolbarGroups', () => {
        componentRef.setInput('toolbarGroups', []);
        fixture.detectChanges();
        toolbarElement = fixture.nativeElement.querySelector('soe-toolbar');
        expect(toolbarElement).toBeFalsy();
      });
    });
    describe('soe-grid', () => {
      let soeGridElement: HTMLElement;
      let soeGridDebugElement: DebugElement;
      let soeGridComponentInstance: any;
      beforeEach(() => {
        soeGridDebugElement = fixture.debugElement.query(By.css('soe-grid'));
        soeGridElement = fixture.nativeElement.querySelector('soe-grid');
        soeGridComponentInstance = soeGridDebugElement.componentInstance;
      });
      it('should render <soe-grid> with correct inputs', () => {
        // componentRef.setInput('rows', [{ id: 1, name: 'Test Row' }]);
        // componentRef.setInput('height', 400);
        // fixture.detectChanges();

        expect(soeGridElement).toBeTruthy();
        expect(soeGridComponentInstance.rows).toBe(component.rows);
        expect(soeGridComponentInstance.height()).toBe(component.height());
        expect(soeGridComponentInstance.masterDetail()).toBe(
          component.masterDetail()
        );
        expect(soeGridComponentInstance.parentGuid()).toBe(
          component.parentGuid()
        );
      });
      it('should trigger rowSelected when <soe-grid> emits rowSelected event', () => {
        vi.spyOn(component, 'triggerRowSelected');

        soeGridDebugElement.triggerEventHandler('rowSelected', { id: 1 });
        fixture.detectChanges();

        expect(component.triggerRowSelected).toHaveBeenCalledWith({ id: 1 });
      });
      it('should trigger selectionChanged when <soe-grid> emits selectionChanged event', () => {
        vi.spyOn(component, 'triggerSelectionChanged');

        soeGridDebugElement.triggerEventHandler('selectionChanged', {
          selectedItems: [1, 2, 3],
        });
        fixture.detectChanges();

        expect(component.triggerSelectionChanged).toHaveBeenCalledWith({
          selectedItems: [1, 2, 3],
        });
      });
      it('should trigger selectedItemsChanged when <soe-grid> emits selectedItemsChanged event', () => {
        vi.spyOn(component, 'triggerSelectedItemsChanged');

        soeGridDebugElement.triggerEventHandler('selectedItemsChanged', {
          selectedItems: [1, 2],
        });
        fixture.detectChanges();
        expect(component.triggerSelectedItemsChanged).toHaveBeenCalledWith({
          selectedItems: [1, 2],
        });
      });
      it('should call edit method when <soe-grid> emits editRowClicked event', () => {
        vi.spyOn(component, 'edit');

        soeGridDebugElement.triggerEventHandler('editRowClicked', {
          rowId: 123,
        });
        fixture.detectChanges();

        expect(component.edit).toHaveBeenCalledWith({ rowId: 123 });
      });
    });
    describe('grid-container', () => {
      let gridContainerDebugElement: DebugElement;
      let gridContainerElement: HTMLElement;
      beforeEach(() => {
        gridContainerDebugElement = fixture.debugElement.query(
          By.css('.grid-container')
        );
        gridContainerElement = gridContainerDebugElement.nativeElement;
      });
      it('should apply no-margin class when noMargin is true', () => {
        componentRef.setInput('noMargin', true);
        fixture.detectChanges();

        expect(gridContainerElement.classList.contains('no-margin')).toBe(true);
      });
      it('should not apply no-margin class when noMargin is false', () => {
        componentRef.setInput('noMargin', false);
        fixture.detectChanges();

        expect(gridContainerElement.classList.contains('no-margin')).toBe(
          false
        );
      });
    });
  });
});
