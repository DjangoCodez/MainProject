import { IconProp } from '@fortawesome/fontawesome-svg-core';
import { IconUtil } from './icon-util';

export class ToolbarUtil {
  static createGroup(button?: ToolbarButton): ToolbarButtonGroup {
    const group = new ToolbarButtonGroup();
    if (button) group.buttons.push(button);

    return group;
  }

  static createSortGroup(
    sortFirst = () => {},
    sortUp = () => {},
    sortDown = () => {},
    sortLast = () => {},
    disabled = () => {},
    hidden = () => {}
  ): ToolbarButtonGroup {
    const group = new ToolbarButtonGroup();
    group.addButton({
      icon: IconUtil.createIcon('fal', 'angle-double-up'),
      title: 'core.sortfirst',
      click: () => {
        sortFirst();
      },
      disabled: disabled,
      hidden: hidden,
    });
    group.addButton({
      icon: IconUtil.createIcon('fal', 'angle-up'),
      title: 'core.sortup',
      click: () => {
        sortUp();
      },
      disabled: disabled,
      hidden: hidden,
    });
    group.addButton({
      icon: IconUtil.createIcon('fal', 'angle-down'),
      title: 'core.sortdown',
      click: () => {
        sortDown();
      },
      disabled: disabled,
      hidden: hidden,
    });
    group.addButton({
      icon: IconUtil.createIcon('fal', 'angle-double-down'),
      title: 'core.sortlast',
      click: () => {
        sortLast();
      },
      disabled: disabled,
      hidden: hidden,
    });
    group.alignLeft();
    return group;
  }

  static createNavigationGroup(
    navigateFirst = () => {},
    navigateLeft = () => {},
    navigateRight = () => {},
    navigateLast = () => {},
    disabled = () => {},
    hidden = () => {}
  ): ToolbarButtonGroup {
    const group = new ToolbarButtonGroup();
    group.addButton({
      icon: IconUtil.createIcon('fal', 'angle-double-left'),
      title: 'core.navigatefirsttfirst',
      click: navigateFirst,
      disabled: disabled,
      hidden: hidden,
    });
    group.addButton({
      icon: IconUtil.createIcon('fal', 'angle-left'),
      title: 'core.navigateleft',
      click: navigateLeft,
      disabled: disabled,
      hidden: hidden,
    });
    group.addButton({
      icon: IconUtil.createIcon('fal', 'angle-right'),
      title: 'core.navigateright',
      click: navigateRight,
      disabled: disabled,
      hidden: hidden,
    });
    group.addButton({
      icon: IconUtil.createIcon('fal', 'angle-double-right'),
      title: 'core.navigatelast',
      click: navigateLast,
      disabled: disabled,
      hidden: hidden,
    });
    group.alignLeft();
    return group;
  }

  static createHelpButton(
    click = () => {},
    disabled = () => {},
    hidden = () => {}
  ): ToolbarButton {
    return {
      icon: IconUtil.createIcon('fal', 'question'),
      title: 'core.help',
      click: click,
      disabled: disabled,
      hidden: hidden,
    };
  }

  static createClearFiltersButton(option: ToolbarButtonOption): ToolbarButton {
    return {
      icon: IconUtil.createIcon('fal', 'filter-slash'),
      title: 'core.uigrid.gridmenu.clear_all_filters',
      click: option.onClick,
      disabled: option.disabled
        ? option.disabled
        : () => {
            return false;
          },
      hidden: option.hidden
        ? option.hidden
        : () => {
            return false;
          },
    };
  }

  static createReloadButton(option: ToolbarButtonOption): ToolbarButton {
    return {
      icon: IconUtil.createIcon('fal', 'sync'),
      title: 'core.reload_data',
      click: option.onClick,
      disabled: option.disabled
        ? option.disabled
        : () => {
            return false;
          },
      hidden: option.hidden
        ? option.hidden
        : () => {
            return false;
          },
    };
  }

  static createSaveButton(option: ToolbarButtonOption): ToolbarButton {
    return {
      icon: IconUtil.createIcon('fal', 'save'),
      title: 'core.save',
      click: option.onClick,
      disabled: option.disabled
        ? option.disabled
        : () => {
            return false;
          },
      hidden: option.hidden
        ? option.hidden
        : () => {
            return false;
          },
    };
  }

  static createCopyButton(option: ToolbarButtonOption): ToolbarButton {
    return {
      icon: IconUtil.createIcon('fal', 'clone'),
      title: 'core.copy',
      click: option.onClick,
      disabled: option.disabled
        ? option.disabled
        : () => {
            return false;
          },
      hidden: option.hidden
        ? option.hidden
        : () => {
            return false;
          },
    };
  }
}

export class ToolbarButtonGroup {
  public buttons = new Array<ToolbarButton>();
  public alignmentRight = true;

  public deleteButton(idString: string) {
    this.buttons = this.buttons.filter(button => button.idString !== idString);
  }

  public addButton(button: ToolbarButton) {
    this.buttons.push(button);
  }

  public alignLeft() {
    this.alignmentRight = false;
  }

  public alignRight() {
    this.alignmentRight = true;
  }
}

export type ToolbarButton = {
  icon: IconProp;
  title?: string;
  click: () => void;
  disabled: () => void;
  hidden: () => void;
  idString?: string;
  label?: string;
};

export type ToolbarButtonOption = {
  onClick: () => void;
  disabled?: () => void;
  hidden?: () => void;
};
