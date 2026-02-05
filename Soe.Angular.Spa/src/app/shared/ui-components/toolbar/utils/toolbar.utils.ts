import { GridComponent } from '@ui/grid/grid.component';
import { signal } from '@angular/core';
import {
  ToolbarEditOptions,
  ToolbarGridOptions,
  ToolbarGroupButton,
  ToolbarGroups,
} from '../models/toolbar';
import { IconUtil } from '@shared/util/icon-util';

export class ToolbarUtils {
  toolbarGroups: ToolbarGroups[] = [];

  grid: GridComponent<any> | undefined;

  clearToolbarGroups() {
    this.toolbarGroups = [];
  }

  createDefaultLegacyGridToolbar<T>(
    grid: GridComponent<T>,
    options: Partial<ToolbarGridOptions> = new ToolbarGridOptions()
  ) {
    const group = this.createLegacyGroup();

    if (!options.hideClearFilters) {
      group.buttons.push(
        this.createClearFiltersLegacyButton({
          onClick: grid.clearFilters.bind(grid),
        })
      );
    }

    if (options.reloadOption)
      group.buttons.push(this.createReloadLegacyButton(options.reloadOption));

    group.alignmentRight = true;

    if (options.saveOption) {
      const saveGroup = this.createLegacyGroup();
      saveGroup.buttons.push(this.createSaveLegacyButton(options.saveOption));
      saveGroup.alignmentRight = true;
    }
  }

  createDefaultLegacyEditToolbar(
    copyOption?: Partial<ToolbarGroupButton>,
    options?: Partial<ToolbarEditOptions>
  ) {
    const group = this.createLegacyGroup();

    if (copyOption && !options?.hideCopy)
      group.buttons.push(this.createCopyLegacyButton(copyOption));

    group.alignmentRight = true;
  }

  createLegacyGroup(options?: Partial<ToolbarGroups>): ToolbarGroups {
    const alignmentRight = options ? options.alignmentRight : true;
    const group = {
      buttons: options?.buttons || [],
      alignmentRight:
        typeof alignmentRight === 'undefined' ? true : alignmentRight,
    };
    this.toolbarGroups.push(group);
    return group;
  }

  createLegacyButton(option: Partial<ToolbarGroupButton>): ToolbarGroupButton {
    return {
      ...option,
      title: option.title,
      onClick: option.onClick!,
      disabled: option.disabled || signal(false),
      hidden: option.hidden || signal(false),
    };
  }

  createClearFiltersLegacyButton(
    option: Partial<ToolbarGroupButton>
  ): ToolbarGroupButton {
    return this.createLegacyButton({
      ...option,
      icon: IconUtil.createIcon('fal', 'filter-slash'),
      title: 'core.uigrid.gridmenu.clear_all_filters',
    });
  }

  createCopyLegacyButton(
    option: Partial<ToolbarGroupButton>
  ): ToolbarGroupButton {
    return this.createLegacyButton({
      ...option,
      icon: IconUtil.createIcon('fal', 'clone'),
      title: 'core.copy',
    });
  }

  createReloadLegacyButton(
    option: Partial<ToolbarGroupButton>
  ): ToolbarGroupButton {
    return this.createLegacyButton({
      ...option,
      icon: IconUtil.createIcon('fal', 'sync'),
      title: 'core.reload_data',
    });
  }

  createSaveLegacyButton(
    option: Partial<ToolbarGroupButton>
  ): ToolbarGroupButton {
    return this.createLegacyButton({
      ...option,
      icon: IconUtil.createIcon('fal', 'save'),
      title: 'core.save',
    });
  }

  createLegacySortGroup(
    sortFirst = () => {},
    sortUp = () => {},
    sortDown = () => {},
    sortLast = () => {},
    disabled = () => {},
    hidden = () => {}
  ): ToolbarGroups {
    const group = this.createLegacyGroup();

    group.buttons.push(
      this.createLegacyButton({
        ...{
          onClick: () => sortFirst(),
          //disabled: disabled,
        },
        icon: IconUtil.createIcon('fal', 'angle-double-up'),
        title: 'core.sortfirst',
      })
    );
    group.buttons.push(
      this.createLegacyButton({
        ...{
          onClick: () => sortUp(),
          //disabled: disabled,
        },
        icon: IconUtil.createIcon('fal', 'angle-up'),
        title: 'core.sortup',
      })
    );
    group.buttons.push(
      this.createLegacyButton({
        ...{
          onClick: () => sortDown(),
          //disabled: disabled,
        },
        icon: IconUtil.createIcon('fal', 'angle-down'),
        title: 'core.sortdown',
      })
    );
    group.buttons.push(
      this.createLegacyButton({
        ...{
          onClick: () => sortLast(),
          //disabled: disabled,
        },
        icon: IconUtil.createIcon('fal', 'angle-double-down'),
        title: 'core.sortlast',
      })
    );

    group.alignmentRight = false;

    return group;
  }
}
