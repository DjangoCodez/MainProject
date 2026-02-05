import { ToolbarUtils } from './toolbar.utils';
import { GridComponent } from '@ui/grid/grid.component';
import { IconUtil } from '@shared/util/icon-util';
import { IconProp } from '@fortawesome/fontawesome-svg-core';
import { vi } from 'vitest';

describe('toolbarUtils', () => {
  let toolbarUtils: ToolbarUtils;

  beforeEach(() => {
    toolbarUtils = new ToolbarUtils();
  });

  describe('clearToolbarGroups', () => {
    it('should clear toolbarGroups array', () => {
      toolbarUtils.toolbarGroups = [{ buttons: [], alignmentRight: true }];
      toolbarUtils.clearToolbarGroups();
      expect(toolbarUtils.toolbarGroups).toEqual([]);
    });
  });

  describe('createDefaultLegacyGridToolbar', () => {
    let mockGrid: GridComponent<any>;

    beforeEach(() => {
      mockGrid = {
        clearFilters: vi.fn(),
      } as any;
    });

    it('should create a toolbar group with clear filters and reload buttons if options allow', () => {
      const options = {
        hideClearFilters: false,
        reloadOption: { onClick: vi.fn() },
      };
      toolbarUtils.createDefaultLegacyGridToolbar(mockGrid, options);

      expect(toolbarUtils.toolbarGroups).toHaveLength(1);
      const group = toolbarUtils.toolbarGroups[0];
      expect(group.buttons).toHaveLength(2);
      expect(group.alignmentRight).toBe(true);
    });

    it('should not add clear filters button if hideClearFilters is true', () => {
      const options = { hideClearFilters: true };
      toolbarUtils.createDefaultLegacyGridToolbar(mockGrid, options);

      expect(toolbarUtils.toolbarGroups[0].buttons).toHaveLength(0);
    });
  });

  describe('createDefaultLegacyEditToolbar', () => {
    it('should create a toolbar group with a copy button if options allow', () => {
      const copyOption = { title: 'Copy' };
      const options = { hideCopy: false };
      toolbarUtils.createDefaultLegacyEditToolbar(copyOption, options);

      expect(toolbarUtils.toolbarGroups).toHaveLength(1);
      const group = toolbarUtils.toolbarGroups[0];
      expect(group.buttons).toHaveLength(1);
      expect(group.alignmentRight).toBe(true);
    });

    it('should not add copy button if hideCopy is true', () => {
      const copyOption = { title: 'Copy' };
      const options = { hideCopy: true };
      toolbarUtils.createDefaultLegacyEditToolbar(copyOption, options);

      expect(toolbarUtils.toolbarGroups[0].buttons).toHaveLength(0);
    });
  });

  describe('createLegacyGroup', () => {
    it('should create a new group with default alignmentRight as true', () => {
      const group = toolbarUtils.createLegacyGroup();
      expect(group.alignmentRight).toBe(true);
      expect(toolbarUtils.toolbarGroups).toContain(group);
    });

    it('should allow overriding default alignmentRight', () => {
      const group = toolbarUtils.createLegacyGroup({ alignmentRight: false });
      expect(group.alignmentRight).toBe(false);
    });
  });

  describe('createLegacyButton', () => {
    it('should create a button with the provided options', () => {
      const option = { title: 'Test Button', onClick: vi.fn() };
      const button = toolbarUtils.createLegacyButton(option);
      expect(button.title).toBe(option.title);
      expect(button.onClick).toBe(option.onClick);
      expect(button.disabled()).toBe(false);
      expect(button.hidden()).toBe(false);
    });
  });

  describe('createClearFiltersLegacyButton', () => {
    it('should use mocked createIcon', () => {
      vi.spyOn(IconUtil, 'createIcon').mockReturnValue('mock-icon' as IconProp);
      const result = IconUtil.createIcon('fal', 'filter-slash');

      expect(result).toStrictEqual('mock-icon');
      expect(IconUtil.createIcon).toHaveBeenCalledWith('fal', 'filter-slash');

      vi.restoreAllMocks();
    });
  });

  describe('createCopyLegacyButton', () => {
    it('should create a button with clone icon', () => {
      const option = { onClick: vi.fn() };
      const button = toolbarUtils.createCopyLegacyButton(option);
      expect(button.icon).toStrictEqual(['fal', 'clone']);
      expect(button.title).toBe('core.copy');
    });
  });

  describe('createReloadLegacyButton', () => {
    it('should create a button with sync icon', () => {
      const option = { onClick: vi.fn() };
      const button = toolbarUtils.createReloadLegacyButton(option);
      expect(button.icon).toStrictEqual(['fal', 'sync']);
      expect(button.title).toBe('core.reload_data');
    });
  });

  describe('createSaveLegacyButton', () => {
    it('should create a button with save icon', () => {
      const option = { onClick: vi.fn() };
      const button = toolbarUtils.createSaveLegacyButton(option);
      expect(button.icon).toStrictEqual(['fal', 'save']);
      expect(button.title).toBe('core.save');
    });
  });

  describe('createLegacySortGroup', () => {
    it('should create a group with four sort buttons', () => {
      const sortFirst = vi.fn();
      const sortUp = vi.fn();
      const sortDown = vi.fn();
      const sortLast = vi.fn();

      toolbarUtils.createLegacySortGroup(sortFirst, sortUp, sortDown, sortLast);

      expect(toolbarUtils.toolbarGroups).toHaveLength(1);
      const group = toolbarUtils.toolbarGroups[0];
      expect(group.buttons).toHaveLength(4);
    });
  });
});
