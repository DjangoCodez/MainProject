import { FieldOrPredicate } from '../util/column-util';
import { ICellRendererParams } from 'ag-grid-community';
import { AnimationProp } from '@fortawesome/angular-fontawesome';
import {
  IconName,
  IconPrefix,
  IconProp,
} from '@fortawesome/fontawesome-svg-core';
import {
  StringKeyOfNumberProperty,
  StringKeyOfStringProperty,
} from '@shared/types';

// Checkbox

export interface ICheckboxCellRendererParams<T, R> {
  disabled?: boolean;
  tooltip?: string;
  showCheckbox?: string | ((item: unknown) => boolean);
}

export type CheckboxCellRendererParams = ICheckboxCellRendererParams<
  unknown,
  unknown
> &
  ICellRendererParams;

// Icon

export interface IIconCellRendererParams<T> {
  useIconFromField?: boolean;
  icon?: IconProp;
  iconClass?: string;
  iconClassField?: string;
  iconAnimation?: AnimationProp;
  iconAnimationField?: string;
  tooltip?: string;
  showIcon?: string | ((item: unknown) => boolean);
  // isSubgrid: boolean;
  isFilter?: boolean;
  onClick?: (item: T) => void;
}

export type IconCellRendererParams = IIconCellRendererParams<unknown> &
  ICellRendererParams;

// Shape

export interface IShapeCellRendererParams<T> {
  shape: string;
  color?: string;
  colorField?: string;
  isSelect?: boolean;
  isText?: boolean;
  isFilter?: boolean;
  //showShape?: string | ((item: unknown) => boolean);
  showShapeField?: string;
  tooltip?: string;
  shapeTooltip?: string;
  // displayField?: string;
  useGradient: boolean;
  gradientField?: string;
  // ignoreTextInFilter?: boolean;
  width?: number;
  // showEmptyIcon?: (data: any) => string;
  showIcon?: (item: unknown) => boolean;
  icon?: IconProp;
  iconClass?: string;
}

export type ShapeCellRendererParams = IShapeCellRendererParams<unknown> &
  ICellRendererParams;

// Textbutton

export interface ITextButtonCellRendererParams<T> {
  icon: IconProp;
  iconClass: string;
  tooltip?: string;
  onClick: (item: T) => void;
  show?: FieldOrPredicate<T>;
}

export type TextButtonCellRendererParams =
  ITextButtonCellRendererParams<unknown> & ICellRendererParams;

// Autocomplete

export interface IAutocompleteCellRendererParams<T, U> {
  optionIdField: StringKeyOfNumberProperty<U>;
  optionNameField: StringKeyOfStringProperty<U>;
  optionDisplayNameField: StringKeyOfStringProperty<T>;
  buttonConfiguration: IAutocompleteButtonConfigurationParams<T>;
}

export type AutocompleteCellRendererParams<T, U> =
  IAutocompleteCellRendererParams<T, U> & ICellRendererParams<T, number>;

export interface IAutocompleteButtonConfigurationParams<T> {
  icon: IconProp;
  iconClass: string;
  tooltip?: string;
  onClick: (item: T) => void;
  show?: FieldOrPredicate<T>;
  iconPrefix?: IconPrefix;
  iconName?: IconName;
}

// Two value
export interface ITwoValueCellRendererParams<T> {
  primaryValueKey: StringKeyOfStringProperty<T>;
  secondaryValueKey: StringKeyOfStringProperty<T>;
}

export type TwoValueCellRendererParams<T> = ITwoValueCellRendererParams<T> &
  ICellRendererParams<T>;

// Multi value
export interface IMultiValueCellRendererParams<T, U> {}

export type MultiValueCellRendererParams<T, U> = IMultiValueCellRendererParams<
  T,
  U
> &
  ICellRendererParams<T>;
