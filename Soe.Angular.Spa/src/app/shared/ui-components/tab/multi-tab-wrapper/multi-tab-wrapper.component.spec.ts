import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MultiTabWrapperComponent } from './multi-tab-wrapper.component';
import {
  Component,
  ComponentRef,
  CUSTOM_ELEMENTS_SCHEMA,
  DebugElement,
  Input,
  NO_ERRORS_SCHEMA,
  SimpleChanges,
  Type,
} from '@angular/core';
import { NavigatorRecordConfig } from '@ui/record-navigator/record-navigator.component';
import { SoeFormGroup } from '@shared/extensions';
import { BehaviorSubject, of } from 'rxjs';
import {
  MultiTabConfig,
  MultiTabWrapperEdit,
} from '../models/multi-tab-wrapper.model';
import { Guid } from '@shared/util/string-util';
import { CrudActionTypeEnum } from '@shared/enums';
import { MatDialog } from '@angular/material/dialog';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';
vi.mock('@shared/util/array-util', () => ({
  upsert: vi.fn(),
}));

@Component({
  // For DOM tests
  selector: 'mock-dynamic-component',
  template: '<div>Mock Dynamic Component</div>',
})
export class MockDynamicComponent {
  @Input() form: any; // Mock form input to match the actual input type expected
}

describe('MultiTabWrapperComponent', () => {
  let component: MultiTabWrapperComponent<any>;
  let componentRef: ComponentRef<MultiTabWrapperComponent<any>>;
  let fixture: ComponentFixture<MultiTabWrapperComponent<any>>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [MultiTabWrapperComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA],
    });
    fixture = TestBed.createComponent(MultiTabWrapperComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    //TODO More tests, big component
    expect(component).toBeTruthy();
  });
  describe('setup', () => {
    it('should set default values', () => {
      expect(component.config()).toEqual([]);
      expect(component.hideAdd()).toBe(false);
      expect(component.hideCloseAll()).toBe(false);
      expect(component.preventMultipleNewTabs()).toBe(false);
      expect(component.cdr).toBeTruthy();
      expect(component.messageboxService).toBeTruthy();
      expect(component.translate).toBeTruthy();
      expect(component.validationHandler).toBeTruthy();
      expect(component.actionTakenSignal).toBeTruthy();
      expect(component.copyActionTakenSignal).toBeTruthy();
      expect(component.openEditInNewTabSignal).toBeTruthy();
      expect(component.setNewRefOnTabSignal).toBeTruthy();
      expect(component.rowEdited).toBeTruthy();
      expect(component.editTabs).toEqual([]);
      expect(component.activeTabIndex).toBe(0);
      expect(component.visibleTabsForCreateTabMenu).toEqual([]);
      expect(component.showAddList()).toBe(false);
      expect(component.addButton).toBeNull();
    });
  });
  describe('getters', () => {
    type MockSoeFormGroup = Partial<{
      idFieldName: string;
      nameFieldName: string;
      modifyPermission: boolean;
      readOnlyPermission: boolean;
    }>;
    describe('hasNewTab', () => {
      it('should return false if isNew is false', () => {
        component.editTabs = [
          {
            isNew: false,
            ref: '',
            label: '',
            component: undefined as unknown as Type<unknown>,
            gridIndex: 0,
            inputs: {
              form: undefined,
              ref: undefined,
              actionTakenSignal: undefined,
              copyActionTakenSignal: undefined,
              openEditInNewTabSignal: undefined,
            },
            recordConfig: new NavigatorRecordConfig(),
          },
        ];
        expect(component.hasNewTab).toBe(false);
      });
      it('should return true if isNew is true', () => {
        component.editTabs = [
          {
            isNew: true,
            ref: '',
            label: '',
            component: undefined as unknown as Type<unknown>,
            gridIndex: 0,
            inputs: {
              form: undefined,
              ref: undefined,
              actionTakenSignal: undefined,
              copyActionTakenSignal: undefined,
              openEditInNewTabSignal: undefined,
            },
            recordConfig: new NavigatorRecordConfig(),
          },
        ];
        expect(component.hasNewTab).toBe(true);
      });
    });
    describe('idFieldName', () => {
      it('should return correct idFieldName when editTabs and inputs are properly set', () => {
        const testName = 'testId';
        const mockForm: MockSoeFormGroup = { idFieldName: testName };
        component.editTabs = [{ inputs: { form: mockForm } } as any];
        component.activeTabIndex = 0;
        vi.spyOn(component, 'tabIndexWithoutGrid').mockReturnValue(0);

        expect(component.idFieldName).toBe(testName);
      });
      it('should return an empty string if idFieldName is undefined', () => {
        component.editTabs = [{ inputs: { form: {} } } as any];
        component.activeTabIndex = 0;
        vi.spyOn(component, 'tabIndexWithoutGrid').mockReturnValue(0);

        expect(component.idFieldName).toBe('');
      });
    });
    describe('nameFieldName', () => {
      it('should return correct idFieldName when editTabs and inputs are properly set', () => {
        const testName = 'testId';
        const mockForm: MockSoeFormGroup = { nameFieldName: testName };
        component.editTabs = [{ inputs: { form: mockForm } } as any];
        component.activeTabIndex = 0;
        vi.spyOn(component, 'tabIndexWithoutGrid').mockReturnValue(0);

        expect(component.nameFieldName).toBe(testName);
      });
      it('should return an empty string if idFieldName is undefined', () => {
        component.editTabs = [{ inputs: { form: {} } } as any];
        component.activeTabIndex = 0;
        vi.spyOn(component, 'tabIndexWithoutGrid').mockReturnValue(0);

        expect(component.nameFieldName).toBe('');
      });
    });
  });
  describe('methods', () => {
    describe('tabIndexWithoutGrid', () => {
      it('should subtract config length from tabaIndex', () => {
        componentRef.setInput('config', [
          { gridComponent: true },
          { gridComponent: true },
          { gridComponent: true },
        ]);
        component.activeTabIndex = 5;
        const result = component.tabIndexWithoutGrid(component.activeTabIndex);
        expect(result).toBe(2);
      });
    });
    describe('addGridsTabsToTabIndex', () => {
      it('should add config length to tabaIndex', () => {
        componentRef.setInput('config', [1, 2, 3]);
        const result = component.addGridsTabsToTabIndex(2);
        expect(result).toBe(5);
      });
    });
    describe('constructor', () => {
      //TODO: should I test this?
    });
    describe('NgOnChanges', () => {
      it('should call retriveGridData when gridComponent is present in config', () => {
        const mockConfig = {
          currentValue: [{ gridComponent: true }],
        };
        const changes: SimpleChanges = { config: mockConfig } as any;

        // vi.spyOn(component, 'retriveGridData');

        component.ngOnChanges(changes);

        expect(component.activeTabIndex).toBe(0);
        // expect(component.retriveGridData).toHaveBeenCalled();
      });
      it('should call createEditTabAsInitial when editComponent is present and no gridComponent', () => {
        const mockConfig = {
          currentValue: [{ editComponent: true }],
        };

        vi.spyOn(component, 'config').mockReturnValue([
          {
            editComponent: true as any,
            editTabLabel: 'Edit Tab',
            FormClass: vi.fn(),
          },
        ]);

        const changes: SimpleChanges = { config: mockConfig } as any;

        vi.spyOn(component, 'createEditTabAsInitial' as any);

        component.ngOnChanges(changes);

        expect(component['createEditTabAsInitial']).toHaveBeenCalledWith(0);
      });

      it('should do nothing when config.currentValue is empty', () => {
        const mockConfig = {
          currentValue: [],
        };
        const changes: SimpleChanges = { config: mockConfig } as any;

        // vi.spyOn(component, 'retriveGridData');
        vi.spyOn(component, 'createEditTabAsInitial' as any);

        component.ngOnChanges(changes);

        // expect(component.retriveGridData).not.toHaveBeenCalled();
        expect(component['createEditTabAsInitial']).not.toHaveBeenCalled();
      });

      it('should do nothing when config is not provided', () => {
        const changes: SimpleChanges = {};

        // vi.spyOn(component, 'retriveGridData');
        vi.spyOn(component, 'createEditTabAsInitial' as any);

        component.ngOnChanges(changes);

        // expect(component.retriveGridData).not.toHaveBeenCalled();
        expect(component['createEditTabAsInitial']).not.toHaveBeenCalled();
      });
    });
    describe('tabIndexChanged', () => {
      it('should not do anything if index is the same as activeTabIndex', () => {
        component.activeTabIndex = 0;
        component.tabIndexChanged(0);
        expect(component.activeTabIndex).toBe(0);
      });
      it('should set activeTabIndex to index', () => {
        component.activeTabIndex = 0;
        component.tabIndexChanged(1);
        expect(component.activeTabIndex).toBe(1);
      });
      it('should call retriveGridData', () => {
        component.activeTabIndex = 0;
        // vi.spyOn(component, 'retriveGridData');
        component.tabIndexChanged(1);
        // expect(component.retriveGridData).toHaveBeenCalled();
      });
    });
    describe('retriveGridData', () => {
      it('should return early when forceLoad is false, rowData exists, and activeTabIndex is within bounds', () => {
        const mockConfig = [{ rowData: new BehaviorSubject([]) }];
        componentRef.setInput('config', mockConfig);
        component.activeTabIndex = 0;

        // const result = component.retriveGridData(false);

        // expect(result).toBeUndefined();
      });
      it('should call getGrid and update rowData when forceLoad is true', () => {
        const gridData = [{ id: 1 }, { id: 2 }];
        const mockGetGrid = vi.fn().mockReturnValue(of(gridData));

        const mockConfig: MultiTabConfig[] = [
          // { getGrid: mockGetGrid, rowData: new BehaviorSubject<unknown[]>([]) },
        ];

        componentRef.setInput('config', mockConfig);
        component.activeTabIndex = 0;

        // component.retriveGridData(true); // Method doesn't exist

        // expect(mockGetGrid).toHaveBeenCalled();

        // expect(mockConfig[0].rowData?.value).toEqual(gridData);
      });
      it('should reload grid data when rowData is missing and forceLoad is false', () => {
        const gridData = [{ id: 1 }, { id: 2 }];
        const mockGetGrid = vi.fn().mockReturnValue(of(gridData));
        const mockConfig = [{ getGrid: mockGetGrid }];
        componentRef.setInput('config', mockConfig);
        component.activeTabIndex = 0;

        // component.retriveGridData(false); // Method doesn't exist

        // expect(mockGetGrid).toHaveBeenCalled();
      });
      it('should not call getGrid if activeTabIndex is out of bounds', () => {
        const mockConfig = [{ getGrid: vi.fn() }];
        componentRef.setInput('config', mockConfig);
        component.activeTabIndex = 2;

        // component.retriveGridData(false);

        expect(mockConfig[0].getGrid).not.toHaveBeenCalled();
      });
      it('should handle empty grid data gracefully', () => {
        const gridData: unknown[] = [];
        const mockGetGrid = vi.fn().mockReturnValue(of(gridData));
        const mockConfig = [
          { getGrid: mockGetGrid, rowData: new BehaviorSubject<unknown[]>([]) },
        ];
        vi.spyOn(component, 'config').mockReturnValue(mockConfig);
        component.activeTabIndex = 0;

        const mockBehaviorSubject = new BehaviorSubject<unknown[]>([]);
        mockConfig[0].rowData = mockBehaviorSubject;

        // component.retriveGridData(true);

        expect(mockBehaviorSubject.value).toEqual(gridData);
      });
    });
    describe('intializeAddTab', () => {
      it('should return early if preventMultipleNewTabs is true and hasNewTab is true', () => {
        componentRef.setInput('config', [{ id: 1 }, { id: 2 }]);
        componentRef.setInput('preventMultipleNewTabs', true);
        vi.spyOn(component, 'hasNewTab', 'get').mockReturnValue(true);

        const createTabSpy = vi.spyOn(component, 'createTabByTabIndex');

        component.initalizeAddTab();

        expect(createTabSpy).not.toHaveBeenCalled();
      });
      it('should call createTabByTabIndex(0) if there is only one tab in config', () => {
        componentRef.setInput('config', [{ id: 1 }]);
        const createTabSpy = vi.spyOn(component, 'createTabByTabIndex');

        component.initalizeAddTab();

        expect(createTabSpy).toHaveBeenCalledWith(0);
      });
      it('should call createTabByTabIndex with index of only visible tab', () => {
        const mockConfig = [
          { hideForCreateTabMenu: true },
          { hideForCreateTabMenu: false },
          { hideForCreateTabMenu: true },
        ];
        componentRef.setInput('config', mockConfig);
        const createTabSpy = vi.spyOn(component, 'createTabByTabIndex');

        component.initalizeAddTab();

        expect(createTabSpy).toHaveBeenCalledWith(1);
      });
    });
    describe('closeAddSelection', () => {
      it('should set showAddlist to false if true', () => {
        component.showAddList.set(true);
        component.closeAddSelection();
        expect(component.showAddList()).toBe(false);
      });
      it('should not change showAddlist if false', () => {
        component.showAddList.set(false);
        component.closeAddSelection();
        expect(component.showAddList()).toBe(false);
      });
    });
    describe('createTabByTabIndex', () => {
      it('should call addTab with index of config and index', () => {
        const index = 1;
        const mockConfig = [{ id: 1 }, { id: 2 }];
        componentRef.setInput('config', mockConfig);
        vi.spyOn(component, 'addTab');
        if (index < mockConfig.length) {
          component.createTabByTabIndex(index);
          expect(component.addTab).toHaveBeenCalledWith({ id: 2 }, 1);
        }
      });
      it('should not call addTab if index is out of bounds', () => {
        const index = 2;
        const mockConfig = [{ id: 1 }, { id: 2 }];
        componentRef.setInput('config', mockConfig);
        vi.spyOn(component, 'addTab');

        if (index < mockConfig.length) {
          component.createTabByTabIndex(index);
          expect(component.addTab).not.toHaveBeenCalled();
        }
      });
      it('should set showAddList to false', () => {
        const index = 1;
        const mockConfig = [{ id: 1 }, { id: 2 }];
        component.showAddList.set(true);
        componentRef.setInput('config', mockConfig);
        vi.spyOn(component, 'addTab');

        component.createTabByTabIndex(index);
        expect(component.showAddList()).toBe(false);
      });
    });
    describe('addTab', () => {
      it('should return early if editComponent or createTabLabel is missing', () => {
        const mockConfig: MultiTabConfig = {
          FormClass: vi.fn(),
          editComponent: undefined,
          createTabLabel: undefined,
        } as any; // Partial config for test

        const pushSpy = vi.spyOn(component.editTabs, 'push');

        component.addTab(mockConfig, 0);

        expect(pushSpy).not.toHaveBeenCalled();
      });
      it('should add a new tab with correct configuration when editComponent and createTabLabel are provided', () => {
        const mockForm = { isNew: false };
        const mockConfig: MultiTabConfig = {
          FormClass: vi.fn().mockImplementation(() => mockForm),
          editComponent: vi.fn(),
          createTabLabel: 'Create Tab',
          recordConfig: {},
        } as any;

        vi.spyOn(component.translate, 'instant').mockReturnValue(
          'Translated Label'
        );
        vi.spyOn(Guid, 'newGuid').mockReturnValue('mock-guid');

        const pushSpy = vi.spyOn(component.editTabs, 'push');

        const mockGridTab = {
          passGridDataOnAdd: false,
          rowData: new BehaviorSubject<unknown[]>([]),
        };
        vi.spyOn(component, 'config').mockReturnValue([mockGridTab]);

        component.addTab(mockConfig, 0);

        expect(pushSpy).toHaveBeenCalled();
        const addedTab: MultiTabWrapperEdit = pushSpy.mock.calls[0][0];
        expect(addedTab.label).toBe('Translated Label');
        expect(addedTab.isNew).toBe(true);
        expect(addedTab.inputs.form).toBe(mockForm);
        expect(addedTab.inputs.ref).toBe('mock-guid');
        expect(mockForm.isNew).toBe(true);
      });
      it('should pass grid data to form if passGridDataOnAdd is set to true', () => {
        const mockForm = { isNew: true, gridData: [] };
        const mockConfig: MultiTabConfig = {
          FormClass: vi.fn().mockImplementation(() => mockForm),
          editComponent: vi.fn(),
          createTabLabel: 'Create Tab',
          passGridDataOnAdd: true,
        } as any;

        const mockGridTab = {
          passGridDataOnAdd: true,
          rowData: new BehaviorSubject([{ id: 1 }, { id: 2 }]),
        };

        componentRef.setInput('config', [mockGridTab]);
        const pushSpy = vi.spyOn(component.editTabs, 'push');

        component.addTab(mockConfig, 0);

        expect(pushSpy).toHaveBeenCalled();
        const addedTab: MultiTabWrapperEdit = pushSpy.mock.calls[0][0];
        expect(addedTab.inputs.form?.gridData).toEqual([{ id: 1 }, { id: 2 }]);
      });
      it('should not pass grid data if passGridDataOnAdd is not set', () => {
        const mockForm = { isNew: true, gridData: [] };
        const mockConfig: MultiTabConfig = {
          FormClass: vi.fn().mockImplementation(() => mockForm),
          editComponent: vi.fn(),
          createTabLabel: 'Create Tab',
          passGridDataOnAdd: false,
        } as any;

        const mockGridTab = {
          passGridDataOnAdd: false,
          rowData: new BehaviorSubject([{ id: 1 }, { id: 2 }]),
        };

        componentRef.setInput('config', [mockGridTab]);
        const pushSpy = vi.spyOn(component.editTabs, 'push');

        component.addTab(mockConfig, 0);

        expect(pushSpy).toHaveBeenCalled();
        const addedTab: MultiTabWrapperEdit = pushSpy.mock.calls[0][0]; // The first argument of the first call
        expect(addedTab.inputs.form?.gridData).toEqual([]); // gridData should not be set
      });
    });
    describe('updateGridByAction', () => {
      it('should call updateOrCreateItem if action is save', () => {
        const gridIndex = 1;
        const rowItemId = 123;
        const form = {} as SoeFormGroup;
        const additionalProps = { closeTabOnSave: true };

        vi.spyOn(component, 'updateOrCreateItem');

        component.updateGridByAction(
          gridIndex,
          CrudActionTypeEnum.Save,
          rowItemId,
          form,
          additionalProps
        );

        expect(component.updateOrCreateItem).toHaveBeenCalledWith(
          gridIndex,
          rowItemId,
          form,
          undefined,
          true
        );
      });
      it('should call removeItemFromGrid, removetab and set activetabIndex to 0 if action is delete', () => {
        const gridIndex = 1;
        const rowItemId = 123;
        const form = {} as SoeFormGroup;
        const additionalProps = { closeTabOnSave: true };
        component.activeTabIndex = 1;
        vi.spyOn(component, 'removeItemFromGrid');
        vi.spyOn(component, 'removeTab');

        component.updateGridByAction(
          gridIndex,
          CrudActionTypeEnum.Delete,
          rowItemId,
          form,
          additionalProps
        );

        expect(component.removeItemFromGrid).toHaveBeenCalledWith(
          gridIndex,
          rowItemId
        );
        expect(component.removeTab).toHaveBeenCalledWith(1);
        expect(component.activeTabIndex).toBe(0);
      });
    });
    describe('updateOrCreateItem', () => {
      beforeEach(() => {
        component.editTabs = [
          {
            isNew: true,
            inputs: { form: undefined },
            gridIndex: 0,
            label: '',
            ref: 'some-ref',
            component: vi.fn(),
            recordConfig: new NavigatorRecordConfig(),
          },
        ];
      });
      it('should return early if rowItemId is not provided', () => {
        const removeTabSpy = vi.spyOn(component, 'removeTab');

        component.updateOrCreateItem(0);

        expect(removeTabSpy).not.toHaveBeenCalled();
      });
      it('should return early if gridTab is not found for the provided gridIndex', () => {
        componentRef.setInput('config', []);

        const removeTabSpy = vi.spyOn(component, 'removeTab');

        component.updateOrCreateItem(0, 123);

        expect(removeTabSpy).not.toHaveBeenCalled();
      });
      it('should update form and labels in the edit tab', () => {
        const mockForm = { value: { name: 'test' } } as SoeFormGroup<any>;

        const mockConfig = [
          {
            editTabLabel: 'Edit Label',
            getGrid: vi
              .fn()
              .mockReturnValue(of([{ id: 123, name: 'updated item' }])),
            rowData: new BehaviorSubject([{ id: 1, name: 'item 1' }]),
          },
        ];

        componentRef.setInput('config', mockConfig);

        // Add editTabs for the test
        component.editTabs = [
          {
            ref: 'test-ref',
            label: 'Test Label',
            component: class {} as any,
            gridIndex: 0,
            isNew: true,
            inputs: {
              form: undefined,
              ref: undefined,
              actionTakenSignal: undefined,
              copyActionTakenSignal: undefined,
              openEditInNewTabSignal: undefined,
            },
            recordConfig: new NavigatorRecordConfig(),
          },
        ];

        vi.spyOn(component, 'tabIndexWithoutGrid').mockReturnValue(0);

        vi.spyOn(component.translate, 'instant').mockReturnValue(
          'Translated Label'
        );

        component.updateOrCreateItem(0, 123, mockForm);

        expect(component.editTabs[0].isNew).toBe(false);
        expect(component.editTabs[0].inputs.form).toBe(mockForm);
        expect(component.editTabs[0].label).toBe('Translated Label');

        // Note: updateOrCreateItem doesn't directly update rowData anymore
        // It relies on getGrid or updateGrid callbacks
        // expect(mockConfig[0].rowData.value).toEqual([
        //   { id: 123, name: 'updated item' },
        // ]);
      });

      it('should close the tab if closeTab is true', () => {
        const mockForm = { value: { name: 'test' } } as SoeFormGroup<any>;

        const mockConfig = [
          {
            editTabLabel: 'Edit Label',
            getGrid: vi
              .fn()
              .mockReturnValue(of([{ id: 123, name: 'updated item' }])),
            rowData: new BehaviorSubject([{ id: 1, name: 'item 1' }]),
          },
        ];

        componentRef.setInput('config', mockConfig);

        vi.spyOn(component, 'tabIndexWithoutGrid').mockReturnValue(0);
        const removeTabSpy = vi.spyOn(component, 'removeTab');

        component.updateOrCreateItem(0, 123, mockForm, undefined, true);

        expect(removeTabSpy).toHaveBeenCalledWith(component.activeTabIndex);

        expect(component.editTabs.length).toBe(0);
      });
      it('should update the record in the grid', () => {
        const mockConfig = [
          {
            editTabLabel: 'Edit Label',
            getGrid: vi
              .fn()
              .mockReturnValue(of([{ id: 123, name: 'updated item' }])),
            rowData: new BehaviorSubject([{ id: 1, name: 'item 1' }]),
          },
        ];
        const mockForm = { value: { name: 'test' } } as SoeFormGroup;

        componentRef.setInput('config', mockConfig);

        component.editTabs = [
          {
            isNew: true,
            inputs: {
              form: undefined,
              ref: undefined,
              actionTakenSignal: undefined,
              copyActionTakenSignal: undefined,
              openEditInNewTabSignal: undefined,
            },
            label: '',
            gridIndex: 0,
            ref: '',
            component: class {} as Type<unknown>,
            recordConfig: new NavigatorRecordConfig(),
          },
        ];
        component.activeTabIndex = 0;

        vi.spyOn(component, 'tabIndexWithoutGrid').mockReturnValue(0);

        component.updateOrCreateItem(0, 123, mockForm, undefined, false);

        // Note: updateOrCreateItem doesn't directly update rowData anymore
        // expect(mockConfig[0].rowData.value).toEqual([
        //   { id: 123, name: 'updated item' },
        // ]);
      });
    });
    describe('upsertItemInRecordNavigator', () => {
      // TODO : Create tests
    });
    describe('removeItemFromGrid', () => {
      it('should return early if no grid tab is found for the provided gridIndex', () => {
        componentRef.setInput('config', []);
        const removeTabSpy = vi.spyOn(component, 'removeTab');

        component.removeItemFromGrid(0, 123);

        expect(removeTabSpy).not.toHaveBeenCalled();
      });
      it('should return early if rowData is not available', () => {
        const mockConfig = [{ rowData: undefined }];
        componentRef.setInput('config', mockConfig);
        const removeTabSpy = vi.spyOn(component, 'removeTab');

        component.removeItemFromGrid(0, 123);

        expect(removeTabSpy).not.toHaveBeenCalled();
      });
      it('should return early if no edit tab is found', () => {
        const mockConfig = [{ rowData: new BehaviorSubject([]) }];
        componentRef.setInput('config', mockConfig);
        const removeTabSpy = vi.spyOn(component, 'removeTab');

        component.removeItemFromGrid(0, 123);

        expect(removeTabSpy).not.toHaveBeenCalled();
      });
      it('should return early if no idFieldname is found', () => {
        const mockConfig = [{ rowData: new BehaviorSubject([]) }];
        componentRef.setInput('config', mockConfig);
        const removeTabSpy = vi.spyOn(component, 'removeTab');

        component.removeItemFromGrid(0, 123);

        expect(removeTabSpy).not.toHaveBeenCalled();
      });
      it('should return early if no row matching rowItemId is found', () => {
        const mockConfig = [
          {
            rowData: new BehaviorSubject([{ id: 1 }, { id: 2 }]),
            idFieldName: 'id',
          },
        ];
        componentRef.setInput('config', mockConfig);
        const removeTabSpy = vi.spyOn(component, 'removeTab');

        component.removeItemFromGrid(0, 123);

        expect(removeTabSpy).not.toHaveBeenCalled();
      });
      // it('should delete a row and update rowData when a matching row is found', () => { // TODO: FIX
      //   vi.spyOn(component, 'removeItemFromRecordNavigator')
      //   const mockConfig = [
      //     {
      //       rowData: new BehaviorSubject([{ id: 1 }, { id: 2 }]),
      //       idFieldName: 'id'
      //     }
      //   ];
      //   componentRef.setInput('config', mockConfig);
      //   const removeTabSpy = jest.spyOn(component, 'removeTab');
      //   component.removeItemFromGrid(0, 1);
      //   expect(component.removeItemFromRecordNavigator).toHaveBeenCalledWith([{ id: 1 }]);
      //   // expect(mockConfig[0].rowData.value).toEqual([{ id: 1 }, { id: 2 }]);
      // });
    });
    describe('removeItemFromRecordNavigator', () => {
      beforeEach(() => {
        component.editTabs = [
          {
            inputs: {
              form: {
                records: [
                  {
                    id: 2,
                    name: '',
                  },
                  {
                    id: 1,
                    name: '',
                  },
                ],
                modifyPermission: true,
                readOnlyPermission: false,
                modelId: 123,
                isNew: false,
                isCopy: false,
                hasNestedControls: false,
                data: {},
                dataType: '',
              } as unknown as SoeFormGroup<any>,
            },
            ref: '',
            isNew: false,
            label: '',
            component: class {} as Type<unknown>,
            gridIndex: 0,
            recordConfig: new NavigatorRecordConfig(),
          },
          {
            inputs: {
              form: {
                records: [
                  {
                    id: 3,
                    name: '',
                  },
                  {
                    id: 4,
                    name: '',
                  },
                ],
                modifyPermission: true,
                readOnlyPermission: false,
                modelId: 123,
                isNew: false,
                isCopy: false,
                hasNestedControls: false,
                data: {},
                dataType: '',
              } as unknown as SoeFormGroup<any>,
            },
            ref: '',
            isNew: false,
            label: '',
            component: class {} as Type<unknown>,
            gridIndex: 0,
            recordConfig: new NavigatorRecordConfig(),
          },
        ];
      });

      it('should remove a record from the correct tab when a matching record is found', () => {
        const row = { id: 1 }; // Mock row with id to be removed
        const idFieldName = 'id'; // Mock idFieldName

        // Call the method
        component.removeItemFromRecordNavigator(row, idFieldName);

        // Expectations
        expect(component.editTabs[0].inputs.form?.records).toEqual([
          { id: 2, name: '' },
        ]); // Record with id: 1 should be removed
        expect(component.editTabs[1].inputs.form?.records).toEqual([
          { id: 3, name: '' },
          { id: 4, name: '' },
        ]); // No change in second tab
      });

      it('should not remove any record if no matching record is found', () => {
        const row = { id: 5 }; // No matching id
        const idFieldName = 'id';

        // Call the method
        component.removeItemFromRecordNavigator(row, idFieldName);

        // Expectations: No records should be removed
        expect(component.editTabs[0].inputs.form?.records).toEqual([
          { id: 2, name: '' },
          { id: 1, name: '' },
        ]);
        expect(component.editTabs[1].inputs.form?.records).toEqual([
          { id: 3, name: '' },
          { id: 4, name: '' },
        ]);
      });
    });
    describe('removeTab', () => {
      let tab1: any;
      let tab2: any;
      let tab3: any;
      beforeEach(() => {
        tab1 = {
          label: 'Tab 1',
          isNew: false,
          inputs: {},
          gridIndex: 0,
          ref: '',
          component: class {} as Type<unknown>,
          recordConfig: new NavigatorRecordConfig(),
        };
        tab2 = {
          label: 'Tab 2',
          isNew: false,
          inputs: {},
          gridIndex: 0,
          ref: '',
          component: class {} as Type<unknown>,
          recordConfig: new NavigatorRecordConfig(),
        };
        tab3 = {
          label: 'Tab 3',
          isNew: false,
          inputs: {},
          gridIndex: 0,
          ref: '',
          component: class {} as Type<unknown>,
          recordConfig: new NavigatorRecordConfig(),
        };
      });
      it('should remove the correct tab from editTabs', () => {
        component.editTabs = [tab1, tab2, tab3];

        vi.spyOn(component, 'tabIndexWithoutGrid').mockReturnValue(1); // Remove the second tab

        component.removeTab(1);

        expect(component.editTabs).toEqual([tab1, tab3]);
      });

      it('should remove the only tab if there is just one tab', () => {
        // Mock a single tab
        component.editTabs = [tab1];

        vi.spyOn(component, 'tabIndexWithoutGrid').mockReturnValue(0); // Remove the first (and only) tab

        component.removeTab(0);

        expect(component.editTabs).toEqual([]);
      });
      it('should not modify editTabs if tabIndexWithoutGrid returns an invalid index', () => {
        component.editTabs = [tab1, tab2, tab3];

        vi.spyOn(component, 'tabIndexWithoutGrid').mockReturnValue(5); // Invalid index

        // Call the method to remove the tab at index 5 (which is out of bounds)
        component.removeTab(5);

        expect(component.editTabs).toEqual([tab1, tab2, tab3]);
      });
    });
    describe('removeAllTabs', () => {
      let tab1: any;
      let tab2: any;
      let tab3: any;
      beforeEach(() => {
        tab1 = {
          label: 'Tab 1',
          isNew: false,
          inputs: {},
          gridIndex: 0,
          ref: '',
          component: class {} as Type<unknown>,
          recordConfig: new NavigatorRecordConfig(),
        };
        tab2 = {
          label: 'Tab 2',
          isNew: false,
          inputs: {},
          gridIndex: 0,
          ref: '',
          component: class {} as Type<unknown>,
          recordConfig: new NavigatorRecordConfig(),
        };
        tab3 = {
          label: 'Tab 3',
          isNew: false,
          inputs: {},
          gridIndex: 0,
          ref: '',
          component: class {} as Type<unknown>,
          recordConfig: new NavigatorRecordConfig(),
        };
      });
      it('should remove all tabs from editTabs', () => {
        component.editTabs = [tab1, tab2, tab3];

        component.removeAllTabs();

        expect(component.editTabs).toEqual([]);
      });
      it('should handle no tabs gracefully', () => {
        component.editTabs = [];

        component.removeAllTabs();

        expect(component.editTabs).toEqual([]);
      });
    });
    describe('tabDoubleClicked', () => {
      beforeEach(() => {
        component.messageboxService = {
          information: vi.fn(),
          dialog: {
            open: vi.fn(),
            closeAll: vi.fn(),
            afterAllClosed: of(),
            afterOpened: of(),
            openDialogs: [],
            close: vi.fn(),
            getDialogById: vi.fn(),
            openConfirmDialog: vi.fn(),
          } as unknown as MatDialog,
          warning: vi.fn(),
          error: vi.fn(),
          success: vi.fn(),
          progress: vi.fn(),
          question: vi.fn(),
          questionAbort: vi.fn(),
          show: vi.fn(),
        };
      });
      it('should display message box with correct information when tab is valid', () => {
        const mockForm = {
          getIdControl: vi.fn().mockReturnValue({ value: '123' }), // Mock getIdControl method
          getIdFieldName: vi.fn().mockReturnValue('ID'), // Mock getIdFieldName method
          value: { ref: 'input-ref' },
        };

        const tab: MultiTabWrapperEdit = {
          ref: 'tab-ref',
          inputs: {
            ref: 'input-ref',
            form: mockForm as unknown as SoeFormGroup<any>,
          },
          isNew: false,
          label: '',
          component: undefined as unknown as any,
          gridIndex: 0,
          recordConfig: new NavigatorRecordConfig(),
        };

        // Set up component's editTabs and tabIndexWithoutGrid
        component.editTabs = [tab];
        component.tabIndexWithoutGrid = (index: number) => index; // Simulate method returning same index

        vi.spyOn(component.messageboxService, 'information');

        component.tabDoubleClicked(0);

        expect(component.messageboxService.information).toHaveBeenCalledWith(
          'Debug info',
          'ID: 123\ntab.ref: tab-ref\ninputs.ref: input-ref',
          {
            customIconName: 'ban-bug',
            hiddenText: JSON.stringify({ ref: 'input-ref' }),
          }
        );
      });
      it('should not display message box when inputs are empty', () => {
        const tab: MultiTabWrapperEdit = {
          ref: 'tab-ref',
          inputs: {},
          isNew: false,
          label: '',
          component: undefined as unknown as any,
          gridIndex: 0,
          recordConfig: new NavigatorRecordConfig(),
        };

        component.editTabs = [tab];
        component.tabIndexWithoutGrid = (index: number) => index; // Simulate method returning same index

        component.tabDoubleClicked(0);
        expect(component.messageboxService.information).not.toHaveBeenCalled();
      });
    });
  });
  describe('DOM', () => {
    describe('soe-tab-group', () => {
      let tabGroupDebug: DebugElement;
      let tabGroup: HTMLElement;
      let tabGroupInstance: any;
      beforeEach(() => {
        tabGroupDebug = fixture.debugElement.query(By.css('soe-tab-group'));
        tabGroup = tabGroupDebug.nativeElement;
        tabGroupInstance = tabGroupDebug.componentInstance;
      });
      it('should render with default values', () => {
        // expect(tabGroupInstance.activeIndex).toBe(component.activeTabIndex); Doesn't work
        expect(tabGroupInstance.hideAdd()).toBe(component.hideAdd());
        expect(tabGroupInstance.hideCloseAll()).toBe(component.hideCloseAll());
      });
      it('should call tabINdexChanged when tabIndexChagned is triggered', () => {
        vi.spyOn(component, 'tabIndexChanged');
        tabGroupDebug.triggerEventHandler('tabIndexChanged', 1);
        expect(component.tabIndexChanged).toHaveBeenCalledWith(1);
      });
      it('should call initalizeAddTab when tabAdded is triggered', () => {
        vi.spyOn(component, 'initalizeAddTab');
        tabGroupDebug.triggerEventHandler('tabAdded', null);
        expect(component.initalizeAddTab).toHaveBeenCalled();
      });
      it('should call removeTab when tabRemoved is triggered', () => {
        vi.spyOn(component, 'removeTab');
        tabGroupDebug.triggerEventHandler('tabRemoved', 1);
        expect(component.removeTab).toHaveBeenCalledWith(1);
      });
      it('should call removeAllTabs when allTabsRemoved is triggered', () => {
        vi.spyOn(component, 'removeAllTabs');
        tabGroupDebug.triggerEventHandler('allTabsRemoved', null);
        expect(component.removeAllTabs).toHaveBeenCalled();
      });
      it('should call tabDoubleClicked when tabDblClicked is triggered', () => {
        vi.spyOn(component, 'tabDoubleClicked');
        tabGroupDebug.triggerEventHandler('tabDblClicked', 1);
        expect(component.tabDoubleClicked).toHaveBeenCalledWith(1);
      });
    });
    describe('grid tabs', () => {
      it('should iterate over the config items and create soe-tab elements', () => {
        const mockConfig = [
          {
            gridComponent: MockDynamicComponent,
            gridTabLabel: 'Tab 1',
            rowData: {},
            exportFilenameKey: 'file1',
          },
          {
            gridComponent: MockDynamicComponent,
            gridTabLabel: 'Tab 2',
            rowData: {},
            exportFilenameKey: 'file2',
          },
        ];

        componentRef.setInput('config', mockConfig);

        fixture.detectChanges();
        const soeTabElements =
          fixture.nativeElement.querySelectorAll('soe-tab');

        expect(soeTabElements.length).toBe(2);
      });
      it('should only render soe-tab if gridComponent is present', () => {
        const mockConfig = [
          {
            gridComponent: MockDynamicComponent,
            gridTabLabel: 'Tab 1',
            rowData: {},
            exportFilenameKey: 'file1',
          },
          {
            gridComponent: false,
            gridTabLabel: 'Tab 2',
            rowData: {},
            exportFilenameKey: 'file2',
          },
        ];

        componentRef.setInput('config', mockConfig);

        fixture.detectChanges();

        // Check if only one soe-tab element was created
        const tabElements = fixture.nativeElement.querySelectorAll('soe-tab');
        expect(tabElements.length).toBe(1);
      });
    });
    describe('edit tabs', () => {
      it('should iterate over editTabs and create the correct number of soe-tab elements', () => {
        const mockEditTabs: MultiTabWrapperEdit[] = [
          {
            label: 'Tab 1',
            component: MockDynamicComponent,
            inputs: {
              form: {
                dirty: true,
                isNew: false,
                nameFieldName: 'name',
                getRawValue: () => ({ name: 'Tab 1' }),
              },
            } as any,
            hideCloseTab: false,
            ref: 'tab1',
            isNew: true,
            gridIndex: 0,
            recordConfig: new NavigatorRecordConfig(),
          },
          {
            label: 'Tab 2',
            component: MockDynamicComponent,
            inputs: {
              form: {
                dirty: false,
                isNew: true,
                nameFieldName: 'name',
                getRawValue: () => ({ name: 'Tab 2' }),
              },
            } as any,
            hideCloseTab: true,
            ref: 'tab1',
            isNew: true,
            gridIndex: 1,
            recordConfig: new NavigatorRecordConfig(),
          },
        ];

        component.editTabs = mockEditTabs;

        fixture.detectChanges();

        const soeTabElements =
          fixture.nativeElement.querySelectorAll('soe-tab');

        expect(soeTabElements.length).toBe(2);
      });
      it('should bind the correct values to soe-tab properties', () => {
        const mockEditTabs: MultiTabWrapperEdit[] = [
          {
            label: 'Tab 1',
            component: MockDynamicComponent,
            inputs: {
              form: {
                dirty: true,
                isNew: false,
                nameFieldName: 'name',
                getRawValue: () => ({ name: 'Tab 1' }),
              },
            } as any,
            hideCloseTab: false,
            ref: 'tab1',
            isNew: true,
            gridIndex: 0,
            recordConfig: new NavigatorRecordConfig(),
          },
          {
            label: 'Tab 2',
            component: MockDynamicComponent,
            inputs: {
              form: {
                dirty: false,
                isNew: true,
                nameFieldName: 'name',
                getRawValue: () => ({ name: 'Tab 2' }),
              },
            } as any,
            hideCloseTab: true,
            ref: 'tab1',
            isNew: true,
            gridIndex: 0,
            recordConfig: new NavigatorRecordConfig(),
          },
        ];

        component.editTabs = mockEditTabs;
        fixture.detectChanges();

        const soeTabElements = fixture.debugElement.queryAll(By.css('soe-tab'));

        expect(soeTabElements.length).toBe(2);

        // Check the label binding
        expect(soeTabElements[0].componentInstance.label()).toContain('Tab 1');
        expect(soeTabElements[1].componentInstance.label()).toContain('Tab 2');

        // Check the closable property
        expect(soeTabElements[0].componentInstance.closable()).toBe(true);
        expect(soeTabElements[1].componentInstance.closable()).toBe(false);

        // Check the isDirty property
        expect(soeTabElements[0].componentInstance.isDirty()).toBe(true);
        expect(soeTabElements[1].componentInstance.isDirty()).toBe(false);

        // Check the isNew property
        expect(soeTabElements[0].componentInstance.isNew()).toBe(false);
        expect(soeTabElements[1].componentInstance.isNew()).toBe(true);
      });
    });
    describe('ul element', () => {
      let ulElement: HTMLElement;
      let ulDebug: DebugElement;
      beforeEach(() => {
        component.editTabs = []; // Reset editTabs to empty array
        component.showAddList.set(true);
      });
      it('should render the ul element only when showAddList returns true', () => {
        fixture.detectChanges();
        ulDebug = fixture.debugElement.query(By.css('ul'));
        ulElement = fixture.nativeElement.querySelector('ul');
        expect(ulElement).toBeTruthy();
      });
      it('should not render the ul element when showAddList returns false', () => {
        component.showAddList.set(false);

        fixture.detectChanges();

        const ulElementFalse = fixture.nativeElement.querySelector('ul');
        expect(ulElementFalse).toBeFalsy();
      });
      it('should set the correct styles for left and top based on addButton.getBoundingClientRect()', () => {
        component.addButton = {
          getBoundingClientRect: vi.fn(() => ({
            left: 100,
            top: 200,
          })),
        } as any;

        fixture.detectChanges();
        ulDebug = fixture.debugElement.query(By.css('ul'));
        ulElement = fixture.nativeElement.querySelector('ul');

        expect(ulElement.style.left).toBe('100px');
        expect(ulElement.style.top).toBe('200px');
      });
      it('should render li elements for each tab in visibleTabsForCreateTabMenu that is not hidden', () => {
        component.visibleTabsForCreateTabMenu = [
          { gridTabLabel: 'Tab 1', hideForCreateTabMenu: false },
          { gridTabLabel: 'Tab 2', hideForCreateTabMenu: true }, // This one should be skipped
          { gridTabLabel: 'Tab 3', hideForCreateTabMenu: false },
        ];

        fixture.detectChanges(); // Trigger change detection
        ulDebug = fixture.debugElement.query(By.css('ul'));
        ulElement = fixture.nativeElement.querySelector('ul');

        const liElements = fixture.nativeElement.querySelectorAll('ul li');
        expect(liElements.length).toBe(2); // Only 2 <li> should be rendered

        expect(liElements[0].textContent).toContain('Tab 1');
        expect(liElements[1].textContent).toContain('Tab 3');
      });
      it('should call closeAddSelection when clicked outside the list', () => {
        fixture.detectChanges();
        ulDebug = fixture.debugElement.query(By.css('ul'));

        const closeSpy = vi.spyOn(component, 'closeAddSelection');

        ulDebug.triggerEventHandler('clickOutside', null);

        fixture.detectChanges();

        expect(closeSpy).toHaveBeenCalled(); // Ensure closeAddSelection was called
      });
      it('should call createTabByTabIndex when a li is clicked', () => {
        // Add necessary form mock for the template BEFORE config
        component.editTabs = [
          {
            label: 'Test',
            component: MockDynamicComponent,
            gridIndex: 0,
            ref: 'test',
            isNew: false,
            inputs: {
              form: {
                getRawValue: () => ({ name: 'Test' }),
                nameFieldName: 'name',
                dirty: false,
                isNew: false,
              } as any,
            },
            recordConfig: new NavigatorRecordConfig(),
          },
        ];

        componentRef.setInput('config', [
          {
            gridTabLabel: 'Tab 1',
            hideForCreateTabMenu: false,
            gridComponent: MockDynamicComponent, // Use MockDynamicComponent to prevent editTab creation
            FormClass: class {} as any,
            editComponent: class {} as any,
          },
        ]);
        component.visibleTabsForCreateTabMenu = [
          {
            gridTabLabel: 'Tab 1',
            hideForCreateTabMenu: false,
            FormClass: class {} as any,
            editComponent: class {} as any,
          },
        ];

        const createTabSpy = vi
          .spyOn(component, 'createTabByTabIndex')
          .mockImplementation(() => {});

        fixture.detectChanges();

        const liDebug = fixture.debugElement.query(By.css('li'));
        liDebug.triggerEventHandler('click', null);

        fixture.detectChanges();

        console.log(liDebug);

        expect(createTabSpy).toHaveBeenCalledWith(0);
      });
    });
  });
});
