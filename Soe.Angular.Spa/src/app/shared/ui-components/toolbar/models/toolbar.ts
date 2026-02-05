import { Signal, Type } from '@angular/core';
import { IconName, IconPrefix } from '@fortawesome/angular-fontawesome';
import { DateRangeValue } from '@ui/forms/datepicker/daterangepicker/daterangepicker.component';
import { DatepickerView } from '@ui/forms/datepicker/datepicker.component';
import {
  MenuButtonBehaviour,
  MenuButtonItem,
  MenuButtonVariant,
} from '@ui/button/menu-button/menu-button.component';
import { SelectProps } from '@ui/forms/select/select.component';
import { IconProp } from '@fortawesome/fontawesome-svg-core';
import {
  ToolbarButtonAction,
  ToolbarButtonBehaviour,
} from '../toolbar-button/toolbar-button.component';
import { ToolbarCheckboxAction } from '../toolbar-checkbox/toolbar-checkbox.component';
import { ToolbarDatepickerAction } from '../toolbar-datepicker/toolbar-datepicker.component';
import { ToolbarDaterangepickerAction } from '../toolbar-daterangepicker/toolbar-daterangepicker.component';
import { ToolbarMenuButtonAction } from '../toolbar-menu-button/toolbar-menu-button.component';
import { ToolbarSelectAction } from '../toolbar-select/toolbar-select.component';
import { CheckboxBehaviour } from '@ui/forms/checkbox/checkbox.component';

export interface ToolbarItemGroupConfig {
  items: IToolbarItemConfig[];
  alignLeft: boolean;
}

export interface IToolbarItemConfig {
  // Common
  component: Type<any>;
  key: string | '';
  disabled: Signal<boolean>;
  hidden: Signal<boolean>;

  // Partly common
  labelKey: Signal<string | undefined>;
  secondaryLabelKey: Signal<string | undefined>;
  secondaryLabelBold: Signal<boolean>;
  secondaryLabelParantheses: Signal<boolean>;
  secondaryLabelPrefixKey: Signal<string | undefined>;
  secondaryLabelPostfixKey: Signal<string | undefined>;
  onValueChanged: (
    $event:
      | ToolbarCheckboxAction
      | ToolbarDatepickerAction
      | ToolbarDaterangepickerAction
      | ToolbarSelectAction
  ) =>
    | { key: string; value: boolean }
    | { key: string; value: Date | undefined }
    | { key: string; value: DateRangeValue | undefined }
    | { key: string; value: number }
    | void;

  // Button/Menu button common
  caption: Signal<string | undefined>;
  tooltip: Signal<string | undefined>;
  iconPrefix: Signal<IconPrefix>;
  iconName: Signal<IconName | undefined>;
  iconClass: Signal<string | undefined>;

  // Button
  buttonBehaviour: Signal<ToolbarButtonBehaviour | undefined>;
  onAction: (
    $event: ToolbarButtonAction
  ) => { key: string; event: Event } | void;

  // Menu button
  menuButtonBehaviour: Signal<MenuButtonBehaviour | undefined>;
  variant: Signal<MenuButtonVariant>;
  insideGroup: Signal<boolean>;
  dropUp: Signal<boolean>;
  dropLeft: Signal<boolean>;
  hideDropdownArrow: Signal<boolean>;
  list: Signal<MenuButtonItem[]>;
  selectedListItem: Signal<MenuButtonItem | undefined>;
  showSelectedItemIcon: Signal<boolean>;
  initialSelectedItemId: Signal<number>;
  unselectItemAfterSelect: Signal<boolean>;
  onItemSelected: ($event: ToolbarMenuButtonAction) => {
    key: string;
    value: MenuButtonItem;
  } | void;

  // Checkbox
  checkboxBehaviour: Signal<CheckboxBehaviour | undefined>;
  checked: Signal<boolean>;

  // Datepicker
  width: Signal<number>;
  view: Signal<DatepickerView>;
  hideToday: Signal<boolean>;
  hideClear: Signal<boolean>;
  showArrows: Signal<boolean>;
  hideCalendarButton: Signal<boolean>;
  minDate: Signal<Date | undefined>;
  maxDate: Signal<Date | undefined>;
  initialDate: Signal<Date | undefined>;

  // Daterangepicker
  labelKeyFrom: Signal<string>;
  secondaryLabelKeyFrom: Signal<string>;
  secondaryLabelBoldFrom: Signal<boolean>;
  secondaryLabelParanthesesFrom: Signal<boolean>;
  secondaryLabelPrefixKeyFrom: Signal<string>;
  secondaryLabelPostfixKeyFrom: Signal<string>;
  lastInPeriodFrom: Signal<boolean>;
  labelKeyTo: Signal<string>;
  secondaryLabelKeyTo: Signal<string>;
  secondaryLabelBoldTo: Signal<boolean>;
  secondaryLabelParanthesesTo: Signal<boolean>;
  secondaryLabelPrefixKeyTo: Signal<string>;
  secondaryLabelPostfixKeyTo: Signal<string>;
  lastInPeriodTo: Signal<boolean>;
  description: Signal<string>;
  separatorDash: Signal<boolean>;
  autoAdjustRange: Signal<boolean>;
  initialDates: Signal<DateRangeValue>;
  deltaDays: Signal<number>;
  offsetDaysOnStep: Signal<number>;

  // Label
  labelLowercase: Signal<boolean>;
  labelCentered: Signal<boolean>;
  secondaryLabelLowercase: Signal<boolean>;
  labelClass: Signal<string>;
  labelValue: Signal<string>;
  tooltipKey: Signal<string>;

  // Select
  items: Signal<SelectProps[]>;
  optionIdField: Signal<string>;
  optionNameField: Signal<string>;
  selectedItem: Signal<SelectProps | undefined>;
  selectedId: Signal<number>;
  initialSelectedId: Signal<number>;
}

export type ToolbarButtonConfig = Pick<
  IToolbarItemConfig,
  | 'component'
  | 'key'
  | 'disabled'
  | 'hidden'
  | 'buttonBehaviour'
  | 'caption'
  | 'tooltip'
  | 'iconPrefix'
  | 'iconName'
  | 'iconClass'
  | 'onAction'
>;

export type ToolbarMenuButtonConfig = Pick<
  IToolbarItemConfig,
  | 'component'
  | 'key'
  | 'disabled'
  | 'hidden'
  | 'menuButtonBehaviour'
  | 'caption'
  | 'tooltip'
  | 'iconPrefix'
  | 'iconName'
  | 'iconClass'
  | 'variant'
  | 'insideGroup'
  | 'dropUp'
  | 'dropLeft'
  | 'hideDropdownArrow'
  | 'list'
  | 'selectedListItem'
  | 'showSelectedItemIcon'
  | 'initialSelectedItemId'
  | 'unselectItemAfterSelect'
  | 'onItemSelected'
>;

export type ToolbarCheckboxConfig = Pick<
  IToolbarItemConfig,
  | 'component'
  | 'key'
  | 'disabled'
  | 'hidden'
  | 'labelKey'
  | 'secondaryLabelKey'
  | 'secondaryLabelBold'
  | 'secondaryLabelParantheses'
  | 'secondaryLabelPrefixKey'
  | 'secondaryLabelPostfixKey'
  | 'checkboxBehaviour'
  | 'checked'
  | 'onValueChanged'
>;

export type ToolbarDatepickerConfig = Pick<
  IToolbarItemConfig,
  | 'component'
  | 'key'
  | 'disabled'
  | 'hidden'
  | 'labelKey'
  | 'secondaryLabelKey'
  | 'secondaryLabelBold'
  | 'secondaryLabelParantheses'
  | 'secondaryLabelPrefixKey'
  | 'secondaryLabelPostfixKey'
  | 'width'
  | 'view'
  | 'hideToday'
  | 'hideClear'
  | 'showArrows'
  | 'hideCalendarButton'
  | 'minDate'
  | 'maxDate'
  | 'initialDate'
  | 'onValueChanged'
>;

export type ToolbarDaterangepickerConfig = Pick<
  IToolbarItemConfig,
  | 'component'
  | 'key'
  | 'disabled'
  | 'hidden'
  | 'labelKeyFrom'
  | 'secondaryLabelKeyFrom'
  | 'secondaryLabelBoldFrom'
  | 'secondaryLabelParanthesesFrom'
  | 'secondaryLabelPrefixKeyFrom'
  | 'secondaryLabelPostfixKeyFrom'
  | 'lastInPeriodFrom'
  | 'labelKeyTo'
  | 'secondaryLabelKeyTo'
  | 'secondaryLabelBoldTo'
  | 'secondaryLabelParanthesesTo'
  | 'secondaryLabelPrefixKeyTo'
  | 'secondaryLabelPostfixKeyTo'
  | 'lastInPeriodTo'
  | 'description'
  | 'width'
  | 'view'
  | 'hideToday'
  | 'hideClear'
  | 'showArrows'
  | 'hideCalendarButton'
  | 'minDate'
  | 'maxDate'
  | 'separatorDash'
  | 'autoAdjustRange'
  | 'initialDates'
  | 'deltaDays'
  | 'offsetDaysOnStep'
  | 'onValueChanged'
>;

export type ToolbarLabelConfig = Pick<
  IToolbarItemConfig,
  | 'component'
  | 'key'
  | 'disabled'
  | 'hidden'
  | 'labelKey'
  | 'secondaryLabelKey'
  | 'secondaryLabelBold'
  | 'secondaryLabelParantheses'
  | 'secondaryLabelPrefixKey'
  | 'secondaryLabelPostfixKey'
  | 'labelLowercase'
  | 'labelCentered'
  | 'secondaryLabelLowercase'
  | 'labelClass'
  | 'labelValue'
  | 'tooltipKey'
>;

export type ToolbarSelectConfig = Pick<
  IToolbarItemConfig,
  | 'component'
  | 'key'
  | 'disabled'
  | 'hidden'
  | 'labelKey'
  | 'secondaryLabelKey'
  | 'secondaryLabelBold'
  | 'secondaryLabelParantheses'
  | 'secondaryLabelPrefixKey'
  | 'secondaryLabelPostfixKey'
  | 'width'
  | 'items'
  | 'optionIdField'
  | 'optionNameField'
  | 'selectedItem'
  | 'selectedId'
  | 'initialSelectedId'
  | 'onValueChanged'
>;

export class ToolbarGridConfig {
  hideReload = false;
  hideClearFilters = false;
  useDefaltSaveOption = false;

  reloadOption?: Partial<ToolbarButtonConfig>;
  clearFiltersOption?: Partial<ToolbarButtonConfig>;
  saveOption?: Partial<ToolbarButtonConfig>;
}

export class ToolbarEditConfig {
  hideCopy = false;
  copyOption?: Partial<ToolbarButtonConfig>;
}

export class ToolbarEmbeddedGridConfig {
  showReload = false;
  showClearFilters = false;
  showSorting = false;
  sortingField = '';
  hideNew = false;

  reloadOption?: Partial<ToolbarButtonConfig>;
  clearFiltersOption?: Partial<ToolbarButtonConfig>;
  newOption?: Partial<ToolbarButtonConfig>;
  sortFirstOption?: () => void;
  sortLastOption?: () => void;
  sortUpOption?: () => void;
  sortDownOption?: () => void;
}

// Legacy below

export interface ToolbarGroups {
  buttons: ToolbarGroupButton[];
  alignmentRight: boolean;
}

export interface ToolbarGroupButton {
  disabled: Signal<boolean>;
  hidden: Signal<boolean>;
  onClick: () => void;
  icon?: IconProp;
  title?: string;
  label?: string;
}

export class ToolbarGridOptions {
  hideReload = false;
  hideClearFilters = false;
  reloadOption?: Partial<ToolbarGroupButton>;
  saveOption?: Partial<ToolbarGroupButton>;
}

export class ToolbarEditOptions {
  hideCopy = false;
}
