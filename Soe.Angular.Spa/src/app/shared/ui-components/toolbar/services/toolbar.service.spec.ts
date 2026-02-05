import { TestBed } from '@angular/core/testing';
import { SoftOneTestBed } from '@src/SoftOneTestBed';
import { signal } from '@angular/core';
import { vi } from 'vitest';

import { ToolbarService } from './toolbar.service';
import { ToolbarButtonComponent } from '../toolbar-button/toolbar-button.component';
import { ToolbarCheckboxComponent } from '../toolbar-checkbox/toolbar-checkbox.component';
import { ToolbarDatepickerComponent } from '../toolbar-datepicker/toolbar-datepicker.component';
import { ToolbarDaterangepickerComponent } from '../toolbar-daterangepicker/toolbar-daterangepicker.component';
import { ToolbarLabelComponent } from '../toolbar-label/toolbar-label.component';
import { ToolbarMenuButtonComponent } from '../toolbar-menu-button/toolbar-menu-button.component';
import { ToolbarSelectComponent } from '../toolbar-select/toolbar-select.component';

describe('ToolbarService', () => {
  let service: ToolbarService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SoftOneTestBed],
      providers: [ToolbarService],
    });
    service = TestBed.inject(ToolbarService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('setup', () => {
    it('should initialize with empty toolbarItemGroups', () => {
      expect(service.toolbarItemGroups).toEqual([]);
    });
  });

  describe('clearItemGroups', () => {
    it('should clear all toolbar item groups', () => {
      service.createItemGroup();
      service.createItemGroup();
      expect(service.toolbarItemGroups.length).toBe(2);

      service.clearItemGroups();

      expect(service.toolbarItemGroups).toEqual([]);
    });
  });

  describe('createItemGroup', () => {
    it('should create a default item group with alignLeft false and empty items', () => {
      const group = service.createItemGroup();

      expect(group.alignLeft).toBe(false);
      expect(group.items).toEqual([]);
      expect(service.toolbarItemGroups.length).toBe(1);
      expect(service.toolbarItemGroups[0]).toBe(group);
    });

    it('should create an item group with custom config', () => {
      const items = [{ key: 'test' } as any];
      const group = service.createItemGroup({
        alignLeft: true,
        items: items,
      });

      expect(group.alignLeft).toBe(true);
      expect(group.items).toBe(items);
    });

    it('should add the group to toolbarItemGroups', () => {
      const group1 = service.createItemGroup();
      const group2 = service.createItemGroup();

      expect(service.toolbarItemGroups).toEqual([group1, group2]);
    });
  });

  describe('createSortItemGroup', () => {
    it('should create a sort item group with all four sort buttons', () => {
      const sortFirst = vi.fn();
      const sortUp = vi.fn();
      const sortDown = vi.fn();
      const sortLast = vi.fn();

      service.createSortItemGroup(sortFirst, sortUp, sortDown, sortLast);

      expect(service.toolbarItemGroups.length).toBe(1);
      expect(service.toolbarItemGroups[0].alignLeft).toBe(true);
      expect(service.toolbarItemGroups[0].items.length).toBe(4);
    });

    it('should create sort buttons with correct icons and tooltips', () => {
      service.createSortItemGroup();

      const items = service.toolbarItemGroups[0].items;
      expect(items[0].key).toBe('sortFirst');
      expect(items[0].iconName?.()).toBe('angle-double-up');
      expect(items[0].tooltip?.()).toBe('core.sortfirst');

      expect(items[1].key).toBe('sortUp');
      expect(items[1].iconName?.()).toBe('angle-up');
      expect(items[1].tooltip?.()).toBe('core.sortup');

      expect(items[2].key).toBe('sortDown');
      expect(items[2].iconName?.()).toBe('angle-down');
      expect(items[2].tooltip?.()).toBe('core.sortdown');

      expect(items[3].key).toBe('sortLast');
      expect(items[3].iconName?.()).toBe('angle-double-down');
      expect(items[3].tooltip?.()).toBe('core.sortlast');
    });

    it('should call the appropriate callback when onAction is invoked', () => {
      const sortFirst = vi.fn();
      const sortUp = vi.fn();
      const sortDown = vi.fn();
      const sortLast = vi.fn();

      service.createSortItemGroup(sortFirst, sortUp, sortDown, sortLast);
      const items = service.toolbarItemGroups[0].items;

      items[0].onAction?.();
      expect(sortFirst).toHaveBeenCalled();

      items[1].onAction?.();
      expect(sortUp).toHaveBeenCalled();

      items[2].onAction?.();
      expect(sortDown).toHaveBeenCalled();

      items[3].onAction?.();
      expect(sortLast).toHaveBeenCalled();
    });
  });

  describe('createToolbarButton', () => {
    it('should create a toolbar button with key and component', () => {
      const button = service.createToolbarButton('testButton', {});

      expect(button.key).toBe('testButton');
      expect(button.component).toBe(ToolbarButtonComponent);
    });

    it('should set button properties from config', () => {
      const onAction = vi.fn();
      const button = service.createToolbarButton('testButton', {
        caption: signal('Test Caption'),
        tooltip: signal('Test Tooltip'),
        iconName: signal('test-icon'),
        disabled: signal(true),
        hidden: signal(false),
        onAction: onAction,
      });

      expect(button.caption?.()).toBe('Test Caption');
      expect(button.tooltip?.()).toBe('Test Tooltip');
      expect(button.iconName?.()).toBe('test-icon');
      expect(button.disabled?.()).toBe(true);
      expect(button.hidden?.()).toBe(false);
      expect(button.onAction).toBe(onAction);
    });
  });

  describe('createToolbarButtonClearFilters', () => {
    it('should create a clear filters button with default icon and tooltip', () => {
      const button = service.createToolbarButtonClearFilters({});

      expect(button.key).toBe('clearFilters');
      expect(button.iconName?.()).toBe('filter-slash');
      expect(button.tooltip?.()).toBe('core.uigrid.gridmenu.clear_all_filters');
    });

    it('should call onAction when provided', () => {
      const onAction = vi.fn();
      const button = service.createToolbarButtonClearFilters({ onAction });

      button.onAction?.();
      expect(onAction).toHaveBeenCalled();
    });
  });

  describe('createToolbarButtonReload', () => {
    it('should create a reload button with default icon and tooltip', () => {
      const button = service.createToolbarButtonReload({});

      expect(button.key).toBe('reload');
      expect(button.iconName?.()).toBe('sync');
      expect(button.tooltip?.()).toBe('core.reload_data');
    });
  });

  describe('createToolbarButtonSave', () => {
    it('should create a save button with default icon and tooltip', () => {
      const button = service.createToolbarButtonSave({});

      expect(button.key).toBe('save');
      expect(button.iconName?.()).toBe('save');
      expect(button.tooltip?.()).toBe('core.save');
    });
  });

  describe('createToolbarButtonCopy', () => {
    it('should create a copy button with default icon and tooltip', () => {
      const button = service.createToolbarButtonCopy({});

      expect(button.key).toBe('copy');
      expect(button.iconName?.()).toBe('clone');
      expect(button.tooltip?.()).toBe('core.copy');
    });
  });

  describe('createToolbarButtonAdd', () => {
    it('should create an add button with default icon and tooltip', () => {
      const button = service.createToolbarButtonAdd({});

      expect(button.key).toBe('addrow');
      expect(button.iconName?.()).toBe('plus');
      expect(button.tooltip?.()).toBe('common.newrow');
    });
  });

  describe('createToolbarMenuButton', () => {
    it('should create a menu button with key and component', () => {
      const menuButton = service.createToolbarMenuButton('testMenu', {});

      expect(menuButton.key).toBe('testMenu');
      expect(menuButton.component).toBe(ToolbarMenuButtonComponent);
    });

    it('should set menu button properties from config', () => {
      const list = signal([{ id: '1', label: 'Item 1' }]);
      const menuButton = service.createToolbarMenuButton('testMenu', {
        list: list,
        dropUp: signal(true),
        dropLeft: signal(false),
        hideDropdownArrow: signal(true),
      });

      expect(menuButton.list).toBe(list);
      expect(menuButton.dropUp?.()).toBe(true);
      expect(menuButton.dropLeft?.()).toBe(false);
      expect(menuButton.hideDropdownArrow?.()).toBe(true);
    });
  });

  describe('createToolbarCheckbox', () => {
    it('should create a checkbox with key and component', () => {
      const checkbox = service.createToolbarCheckbox('testCheckbox', {});

      expect(checkbox.key).toBe('testCheckbox');
      expect(checkbox.component).toBe(ToolbarCheckboxComponent);
    });

    it('should set checkbox properties from config', () => {
      const onValueChanged = vi.fn();
      const checkbox = service.createToolbarCheckbox('testCheckbox', {
        checked: signal(true),
        labelKey: signal('test.label'),
        onValueChanged: onValueChanged,
      });

      expect(checkbox.checked?.()).toBe(true);
      expect(checkbox.labelKey?.()).toBe('test.label');
      expect(checkbox.onValueChanged).toBe(onValueChanged);
    });
  });

  describe('createToolbarDatepicker', () => {
    it('should create a datepicker with key and component', () => {
      const datepicker = service.createToolbarDatepicker('testDatepicker', {});

      expect(datepicker.key).toBe('testDatepicker');
      expect(datepicker.component).toBe(ToolbarDatepickerComponent);
    });

    it('should set datepicker properties from config', () => {
      const onValueChanged = vi.fn();
      const initialDate = new Date();
      const datepicker = service.createToolbarDatepicker('testDatepicker', {
        initialDate: signal(initialDate),
        width: signal('200px'),
        hideToday: signal(true),
        onValueChanged: onValueChanged,
      });

      expect(datepicker.initialDate?.()).toBe(initialDate);
      expect(datepicker.width?.()).toBe('200px');
      expect(datepicker.hideToday?.()).toBe(true);
      expect(datepicker.onValueChanged).toBe(onValueChanged);
    });
  });

  describe('createToolbarDaterangepicker', () => {
    it('should create a daterangepicker with key and component', () => {
      const daterangepicker = service.createToolbarDaterangepicker(
        'testDaterange',
        {}
      );

      expect(daterangepicker.key).toBe('testDaterange');
      expect(daterangepicker.component).toBe(ToolbarDaterangepickerComponent);
    });

    it('should set daterangepicker properties from config', () => {
      const onValueChanged = vi.fn();
      const daterangepicker = service.createToolbarDaterangepicker(
        'testDaterange',
        {
          labelKeyFrom: signal('from.label'),
          labelKeyTo: signal('to.label'),
          width: signal('300px'),
          onValueChanged: onValueChanged,
        }
      );

      expect(daterangepicker.labelKeyFrom?.()).toBe('from.label');
      expect(daterangepicker.labelKeyTo?.()).toBe('to.label');
      expect(daterangepicker.width?.()).toBe('300px');
      expect(daterangepicker.onValueChanged).toBe(onValueChanged);
    });
  });

  describe('createToolbarLabel', () => {
    it('should create a label with key and component', () => {
      const label = service.createToolbarLabel('testLabel', {});

      expect(label.key).toBe('testLabel');
      expect(label.component).toBe(ToolbarLabelComponent);
    });

    it('should set label properties from config', () => {
      const label = service.createToolbarLabel('testLabel', {
        labelKey: signal('test.label'),
        labelValue: signal('Test Value'),
        labelLowercase: signal(true),
        secondaryLabelKey: signal('secondary.label'),
      });

      expect(label.labelKey?.()).toBe('test.label');
      expect(label.labelValue?.()).toBe('Test Value');
      expect(label.labelLowercase?.()).toBe(true);
      expect(label.secondaryLabelKey?.()).toBe('secondary.label');
    });
  });

  describe('createToolbarSelect', () => {
    it('should create a select with key and component', () => {
      const select = service.createToolbarSelect('testSelect', {});

      expect(select.key).toBe('testSelect');
      expect(select.component).toBe(ToolbarSelectComponent);
    });

    it('should set select properties from config', () => {
      const items = signal([{ id: 1, name: 'Option 1' }]);
      const onValueChanged = vi.fn();
      const select = service.createToolbarSelect('testSelect', {
        items: items,
        optionIdField: signal('id'),
        optionNameField: signal('name'),
        selectedId: signal(1),
        onValueChanged: onValueChanged,
      });

      expect(select.items).toBe(items);
      expect(select.optionIdField?.()).toBe('id');
      expect(select.optionNameField?.()).toBe('name');
      expect(select.selectedId?.()).toBe(1);
      expect(select.onValueChanged).toBe(onValueChanged);
    });
  });

  describe('createDefaultGridToolbar', () => {
    it('should create clearFilters and reload buttons when options are provided', () => {
      const clearFiltersAction = vi.fn();
      const reloadAction = vi.fn();

      service.createDefaultGridToolbar({
        clearFiltersOption: { onAction: clearFiltersAction },
        reloadOption: { onAction: reloadAction },
      });

      expect(service.toolbarItemGroups.length).toBe(1);
      expect(service.toolbarItemGroups[0].items.length).toBe(2);
      expect(service.toolbarItemGroups[0].items[0].key).toBe('clearFilters');
      expect(service.toolbarItemGroups[0].items[1].key).toBe('reload');
    });

    it('should create save button in separate group when saveOption is provided', () => {
      const saveAction = vi.fn();

      service.createDefaultGridToolbar({
        saveOption: { onAction: saveAction },
      });

      expect(service.toolbarItemGroups.length).toBe(1);
      expect(service.toolbarItemGroups[0].items.length).toBe(1);
      expect(service.toolbarItemGroups[0].items[0].key).toBe('save');
    });

    it('should not create any groups when no options are provided', () => {
      service.createDefaultGridToolbar({});

      expect(service.toolbarItemGroups.length).toBe(0);
    });
  });

  describe('createDefaultEmbeddedGridToolbar', () => {
    it('should create clearFilters, reload, and new buttons when options are provided', () => {
      const clearFiltersAction = vi.fn();
      const reloadAction = vi.fn();
      const newAction = vi.fn();

      service.createDefaultEmbeddedGridToolbar({
        clearFiltersOption: { onAction: clearFiltersAction },
        reloadOption: { onAction: reloadAction },
        newOption: { onAction: newAction },
      });

      expect(service.toolbarItemGroups.length).toBe(1);
      expect(service.toolbarItemGroups[0].items.length).toBe(3);
      expect(service.toolbarItemGroups[0].items[0].key).toBe('clearFilters');
      expect(service.toolbarItemGroups[0].items[1].key).toBe('reload');
      expect(service.toolbarItemGroups[0].items[2].key).toBe('addrow');
    });

    it('should create sort item group when showSorting is true', () => {
      const sortFirst = vi.fn();
      const sortUp = vi.fn();
      const sortDown = vi.fn();
      const sortLast = vi.fn();

      service.createDefaultEmbeddedGridToolbar({
        showSorting: true,
        sortFirstOption: sortFirst,
        sortUpOption: sortUp,
        sortDownOption: sortDown,
        sortLastOption: sortLast,
      });

      expect(service.toolbarItemGroups.length).toBe(1);
      expect(service.toolbarItemGroups[0].items.length).toBe(4);
      expect(service.toolbarItemGroups[0].alignLeft).toBe(true);
    });
  });

  describe('createDefaultEditToolbar', () => {
    it('should create copy button when copyOption is provided and not hidden', () => {
      const copyAction = vi.fn();

      service.createDefaultEditToolbar({
        copyOption: { onAction: copyAction },
        hideCopy: false,
      });

      expect(service.toolbarItemGroups.length).toBe(1);
      expect(service.toolbarItemGroups[0].items.length).toBe(1);
      expect(service.toolbarItemGroups[0].items[0].key).toBe('copy');
    });

    it('should not create copy button when hideCopy is true', () => {
      const copyAction = vi.fn();

      service.createDefaultEditToolbar({
        copyOption: { onAction: copyAction },
        hideCopy: true,
      });

      expect(service.toolbarItemGroups.length).toBe(0);
    });

    it('should not create copy button when copyOption is not provided', () => {
      service.createDefaultEditToolbar({});

      expect(service.toolbarItemGroups.length).toBe(0);
    });
  });
});
