import { Injectable, signal } from '@angular/core';
import {
  IToolbarItemConfig,
  ToolbarButtonConfig,
  ToolbarCheckboxConfig,
  ToolbarDatepickerConfig,
  ToolbarDaterangepickerConfig,
  ToolbarEditConfig,
  ToolbarEmbeddedGridConfig,
  ToolbarGridConfig,
  ToolbarItemGroupConfig,
  ToolbarLabelConfig,
  ToolbarMenuButtonConfig,
  ToolbarSelectConfig,
} from '../models/toolbar';
import { ToolbarButtonComponent } from '../toolbar-button/toolbar-button.component';
import { ToolbarCheckboxComponent } from '../toolbar-checkbox/toolbar-checkbox.component';
import { ToolbarDatepickerComponent } from '../toolbar-datepicker/toolbar-datepicker.component';
import { ToolbarDaterangepickerComponent } from '../toolbar-daterangepicker/toolbar-daterangepicker.component';
import { ToolbarLabelComponent } from '../toolbar-label/toolbar-label.component';
import { ToolbarMenuButtonComponent } from '../toolbar-menu-button/toolbar-menu-button.component';
import { ToolbarSelectComponent } from '../toolbar-select/toolbar-select.component';

@Injectable()
export class ToolbarService {
  toolbarItemGroups: ToolbarItemGroupConfig[] = [];

  // DEFAULT TOOLBARS

  createDefaultGridToolbar<T>(
    config: Partial<ToolbarGridConfig> = new ToolbarGridConfig()
  ) {
    if (config.clearFiltersOption || config.reloadOption) {
      const group = this.createItemGroup();

      if (config.clearFiltersOption) {
        group.items.push(
          this.createToolbarButtonClearFilters(config.clearFiltersOption)
        );
      }

      if (config.reloadOption) {
        group.items.push(this.createToolbarButtonReload(config.reloadOption));
      }
    }

    if (config.saveOption) {
      this.createItemGroup({
        items: [this.createToolbarButtonSave(config.saveOption)],
      });
    }
  }

  createDefaultEmbeddedGridToolbar<T>(
    config: Partial<ToolbarEmbeddedGridConfig> = new ToolbarEmbeddedGridConfig()
  ) {
    if (config.clearFiltersOption || config.reloadOption || config.newOption) {
      const group = this.createItemGroup();

      if (config.clearFiltersOption) {
        group.items.push(
          this.createToolbarButtonClearFilters(config.clearFiltersOption)
        );
      }

      if (config.reloadOption) {
        group.items.push(this.createToolbarButtonReload(config.reloadOption));
      }

      if (config.newOption) {
        group.items.push(this.createToolbarButtonAdd(config.newOption));
      }
    }

    if (config.showSorting) {
      this.createSortItemGroup(
        config.sortFirstOption,
        config.sortUpOption,
        config.sortDownOption,
        config.sortLastOption
      );
    }
  }

  createDefaultEditToolbar(config: Partial<ToolbarEditConfig>) {
    if (config.copyOption && !config.hideCopy)
      this.createItemGroup({
        items: [this.createToolbarButtonCopy(config.copyOption)],
      });
  }

  // GROUP

  clearItemGroups() {
    this.toolbarItemGroups = [];
  }

  createItemGroup(
    config?: Partial<ToolbarItemGroupConfig>
  ): ToolbarItemGroupConfig {
    const group = {
      alignLeft: config?.alignLeft || false,
      items: config?.items || [],
    };
    this.toolbarItemGroups.push(group);
    return group;
  }

  createSortItemGroup(
    sortFirst = () => {},
    sortUp = () => {},
    sortDown = () => {},
    sortLast = () => {}
  ) {
    this.createItemGroup({
      alignLeft: true,
      items: [
        this.createToolbarButton('sortFirst', {
          iconName: signal('angle-double-up'),
          tooltip: signal('core.sortfirst'),
          onAction: () => sortFirst(),
        }),
        this.createToolbarButton('sortUp', {
          iconName: signal('angle-up'),
          tooltip: signal('core.sortup'),
          onAction: () => sortUp(),
        }),
        this.createToolbarButton('sortDown', {
          iconName: signal('angle-down'),
          tooltip: signal('core.sortdown'),
          onAction: () => sortDown(),
        }),
        this.createToolbarButton('sortLast', {
          iconName: signal('angle-double-down'),
          tooltip: signal('core.sortlast'),
          onAction: () => sortLast(),
        }),
      ],
    });
  }

  // COMMON

  private setCommonConfig(
    config: Partial<IToolbarItemConfig>,
    cfg: IToolbarItemConfig
  ) {
    if (config.disabled) cfg.disabled = config.disabled;
    if (config.hidden) cfg.hidden = config.hidden;
  }

  private setCommonLabelConfig(
    config: Partial<IToolbarItemConfig>,
    cfg: IToolbarItemConfig
  ) {
    if (config.labelKey) cfg.labelKey = config.labelKey;
    if (config.labelLowercase) cfg.labelLowercase = config.labelLowercase;
    if (config.labelCentered) cfg.labelCentered = config.labelCentered;
    if (config.secondaryLabelKey)
      cfg.secondaryLabelKey = config.secondaryLabelKey;
    if (config.secondaryLabelBold)
      cfg.secondaryLabelBold = config.secondaryLabelBold;
    if (config.secondaryLabelParantheses)
      cfg.secondaryLabelParantheses = config.secondaryLabelParantheses;
    if (config.secondaryLabelPrefixKey)
      cfg.secondaryLabelPrefixKey = config.secondaryLabelPrefixKey;
    if (config.secondaryLabelPostfixKey)
      cfg.secondaryLabelPostfixKey = config.secondaryLabelPostfixKey;
    if (config.secondaryLabelLowercase)
      cfg.secondaryLabelLowercase = config.secondaryLabelLowercase;
    if (config.labelClass) cfg.labelClass = config.labelClass;
    if (config.labelValue) cfg.labelValue = config.labelValue;
    if (config.tooltipKey) cfg.tooltipKey = config.tooltipKey;
  }

  private setCommonButtonConfig(
    config: Partial<IToolbarItemConfig>,
    cfg: IToolbarItemConfig
  ) {
    this.setCommonConfig(config, cfg);
    if (config.caption) cfg.caption = config.caption;
    if (config.tooltip) cfg.tooltip = config.tooltip;
    if (config.iconPrefix) cfg.iconPrefix = config.iconPrefix;
    if (config.iconName) cfg.iconName = config.iconName;
    if (config.iconClass) cfg.iconClass = config.iconClass;
  }

  private setCommonDatepickerConfig(
    config: Partial<IToolbarItemConfig>,
    cfg: IToolbarItemConfig
  ) {
    if (config.width) cfg.width = config.width;
    if (config.view) cfg.view = config.view;
    if (config.hideToday) cfg.hideToday = config.hideToday;
    if (config.hideClear) cfg.hideClear = config.hideClear;
    if (config.showArrows) cfg.showArrows = config.showArrows;
    if (config.hideCalendarButton)
      cfg.hideCalendarButton = config.hideCalendarButton;
    if (config.minDate) cfg.minDate = config.minDate;
    if (config.maxDate) cfg.maxDate = config.maxDate;
    if (config.onValueChanged) cfg.onValueChanged = config.onValueChanged;
  }

  // BUTTON

  createToolbarButton(
    key: string,
    config: Partial<ToolbarButtonConfig>
  ): IToolbarItemConfig {
    const cfg = { ...config, key: key, component: ToolbarButtonComponent };

    this.setCommonButtonConfig(
      config as IToolbarItemConfig,
      cfg as IToolbarItemConfig
    );

    if (config.buttonBehaviour) cfg.buttonBehaviour = config.buttonBehaviour;
    if (config.onAction) cfg.onAction = config.onAction;

    return cfg as IToolbarItemConfig;
  }

  createToolbarButtonClearFilters(
    config: Partial<ToolbarButtonConfig>
  ): IToolbarItemConfig {
    return this.createToolbarButton('clearFilters', {
      ...config,
      iconName: signal('filter-slash'),
      tooltip: signal('core.uigrid.gridmenu.clear_all_filters'),
    });
  }

  createToolbarButtonReload(
    config: Partial<ToolbarButtonConfig>
  ): IToolbarItemConfig {
    return this.createToolbarButton('reload', {
      ...config,
      iconName: signal('sync'),
      tooltip: signal('core.reload_data'),
    });
  }

  createToolbarButtonSave(
    config: Partial<ToolbarButtonConfig>
  ): IToolbarItemConfig {
    return this.createToolbarButton('save', {
      ...config,
      iconName: signal('save'),
      tooltip: signal('core.save'),
    });
  }

  createToolbarButtonCopy(
    config: Partial<ToolbarButtonConfig>
  ): IToolbarItemConfig {
    return this.createToolbarButton('copy', {
      ...config,
      iconName: signal('clone'),
      tooltip: signal('core.copy'),
    });
  }

  createToolbarButtonAdd(
    config: Partial<ToolbarButtonConfig>
  ): IToolbarItemConfig {
    return this.createToolbarButton('addrow', {
      ...config,
      iconName: signal('plus'),
      tooltip: signal('common.newrow'),
    });
  }

  // MENU BUTTON

  createToolbarMenuButton(
    key: string,
    config: Partial<ToolbarMenuButtonConfig>
  ): IToolbarItemConfig {
    const cfg = {
      ...config,
      key: key,
      component: ToolbarMenuButtonComponent,
    };

    this.setCommonButtonConfig(
      config as IToolbarItemConfig,
      cfg as IToolbarItemConfig
    );

    if (config.menuButtonBehaviour)
      cfg.menuButtonBehaviour = config.menuButtonBehaviour;
    if (config.variant) cfg.variant = config.variant;
    if (config.insideGroup) cfg.insideGroup = config.insideGroup;
    if (config.dropUp) cfg.dropUp = config.dropUp;
    if (config.dropLeft) cfg.dropLeft = config.dropLeft;
    if (config.hideDropdownArrow)
      cfg.hideDropdownArrow = config.hideDropdownArrow;
    if (config.list) cfg.list = config.list;
    if (config.selectedListItem) cfg.selectedListItem = config.selectedListItem;
    if (config.showSelectedItemIcon)
      cfg.showSelectedItemIcon = config.showSelectedItemIcon;
    if (config.initialSelectedItemId)
      cfg.initialSelectedItemId = config.initialSelectedItemId;
    if (config.unselectItemAfterSelect)
      cfg.unselectItemAfterSelect = config.unselectItemAfterSelect;

    return cfg as IToolbarItemConfig;
  }

  // CHECKBOX

  createToolbarCheckbox(
    key: string,
    config: Partial<ToolbarCheckboxConfig>
  ): IToolbarItemConfig {
    const cfg = { ...config, key: key, component: ToolbarCheckboxComponent };

    this.setCommonConfig(
      config as IToolbarItemConfig,
      cfg as IToolbarItemConfig
    );
    this.setCommonLabelConfig(
      config as IToolbarItemConfig,
      cfg as IToolbarItemConfig
    );

    if (config.checkboxBehaviour)
      cfg.checkboxBehaviour = config.checkboxBehaviour;
    if (config.checked) cfg.checked = config.checked;
    if (config.onValueChanged) cfg.onValueChanged = config.onValueChanged;

    return cfg as IToolbarItemConfig;
  }

  // DATEPICKER

  createToolbarDatepicker(
    key: string,
    config: Partial<ToolbarDatepickerConfig>
  ): IToolbarItemConfig {
    const cfg = { ...config, key: key, component: ToolbarDatepickerComponent };

    this.setCommonConfig(
      config as IToolbarItemConfig,
      cfg as IToolbarItemConfig
    );
    this.setCommonLabelConfig(
      config as IToolbarItemConfig,
      cfg as IToolbarItemConfig
    );
    this.setCommonDatepickerConfig(
      config as IToolbarItemConfig,
      cfg as IToolbarItemConfig
    );

    if (config.initialDate) cfg.initialDate = config.initialDate;

    return cfg as IToolbarItemConfig;
  }

  // DATERANGEPICKER

  createToolbarDaterangepicker(
    key: string,
    config: Partial<ToolbarDaterangepickerConfig>
  ): IToolbarItemConfig {
    const cfg = {
      ...config,
      key: key,
      component: ToolbarDaterangepickerComponent,
    };

    this.setCommonConfig(
      config as IToolbarItemConfig,
      cfg as IToolbarItemConfig
    );
    this.setCommonDatepickerConfig(
      config as IToolbarItemConfig,
      cfg as IToolbarItemConfig
    );

    if (config.labelKeyFrom) cfg.labelKeyFrom = config.labelKeyFrom;
    if (config.secondaryLabelKeyFrom)
      cfg.secondaryLabelKeyFrom = config.secondaryLabelKeyFrom;
    if (config.secondaryLabelBoldFrom)
      cfg.secondaryLabelBoldFrom = config.secondaryLabelBoldFrom;
    if (config.secondaryLabelParanthesesFrom)
      cfg.secondaryLabelParanthesesFrom = config.secondaryLabelParanthesesFrom;
    if (config.secondaryLabelPrefixKeyFrom)
      cfg.secondaryLabelPrefixKeyFrom = config.secondaryLabelPrefixKeyFrom;
    if (config.secondaryLabelPostfixKeyFrom)
      cfg.secondaryLabelPostfixKeyFrom = config.secondaryLabelPostfixKeyFrom;
    if (config.lastInPeriodFrom) cfg.lastInPeriodFrom = config.lastInPeriodFrom;

    if (config.labelKeyTo) cfg.labelKeyTo = config.labelKeyTo;
    if (config.secondaryLabelKeyTo)
      cfg.secondaryLabelKeyTo = config.secondaryLabelKeyTo;
    if (config.secondaryLabelBoldTo)
      cfg.secondaryLabelBoldTo = config.secondaryLabelBoldTo;
    if (config.secondaryLabelParanthesesTo)
      cfg.secondaryLabelParanthesesTo = config.secondaryLabelParanthesesTo;
    if (config.secondaryLabelPrefixKeyTo)
      cfg.secondaryLabelPrefixKeyTo = config.secondaryLabelPrefixKeyTo;
    if (config.secondaryLabelPostfixKeyTo)
      cfg.secondaryLabelPostfixKeyTo = config.secondaryLabelPostfixKeyTo;
    if (config.lastInPeriodTo) cfg.lastInPeriodTo = config.lastInPeriodTo;

    if (config.description) cfg.description = config.description;
    if (config.initialDates) cfg.initialDates = config.initialDates;

    return cfg as IToolbarItemConfig;
  }

  // LABEL

  createToolbarLabel(
    key: string,
    config: Partial<ToolbarLabelConfig>
  ): IToolbarItemConfig {
    const cfg = { ...config, key: key, component: ToolbarLabelComponent };

    this.setCommonConfig(
      config as IToolbarItemConfig,
      cfg as IToolbarItemConfig
    );
    this.setCommonLabelConfig(
      config as IToolbarItemConfig,
      cfg as IToolbarItemConfig
    );

    return cfg as IToolbarItemConfig;
  }

  // SELECT

  createToolbarSelect(
    key: string,
    config: Partial<ToolbarSelectConfig>
  ): IToolbarItemConfig {
    const cfg = { ...config, key: key, component: ToolbarSelectComponent };

    this.setCommonConfig(
      config as IToolbarItemConfig,
      cfg as IToolbarItemConfig
    );
    this.setCommonLabelConfig(
      config as IToolbarItemConfig,
      cfg as IToolbarItemConfig
    );

    if (config.width) cfg.width = config.width;
    if (config.items) cfg.items = config.items;
    if (config.optionIdField) cfg.optionIdField = config.optionIdField;
    if (config.optionNameField) cfg.optionNameField = config.optionNameField;
    if (config.selectedItem) cfg.selectedItem = config.selectedItem;
    if (config.selectedId) cfg.selectedId = config.selectedId;
    if (config.initialSelectedId)
      cfg.initialSelectedId = config.initialSelectedId;
    if (config.onValueChanged) cfg.onValueChanged = config.onValueChanged;

    return cfg as IToolbarItemConfig;
  }
}
