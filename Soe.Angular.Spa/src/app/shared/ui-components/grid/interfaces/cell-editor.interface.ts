import {
  StringKeyOfNumberProperty,
  StringKeyOfStringProperty,
} from '@shared/types';
import { DataCallback2, Predicate } from '../util/column-util';
import { IAutocompleteButtonConfigurationParams } from './cell-renderer.interface';

export interface IAutocompleteEditorParams<T, U> {
  updater?: DataCallback2<T, U | undefined>;
  source: (data?: T) => U[];
  disabled: boolean | Predicate<T>;
  delay?: number;
  optionIdField: StringKeyOfNumberProperty<U>;
  optionNameField: StringKeyOfStringProperty<U>;
  optionDisplayNameField: StringKeyOfStringProperty<T>;
  scrollable: boolean;
  limit?: number;
  buttonConfiguration: IAutocompleteButtonConfigurationParams<T>;
  allowNavigationFrom?: (value: any, data: T) => boolean;
}

export interface ICheckboxEditorParams<T> {
  disabled: boolean | Predicate<T>;
  showCheckbox?: string | ((item: unknown) => boolean);
  onClick?: (value: boolean, row: T) => void;
}

export interface IDateEditorParams<T> {
  disabled: boolean | Predicate<T>;
}

export interface INumberEditorParams<T> {
  disabled: boolean | Predicate<T>;
  decimals: number;
}

export interface ITimeEditorParams<T> {
  disabled: boolean | Predicate<T>;
}

export interface ITimeSpanEditorParams<T> {
  disabled: boolean | Predicate<T>;
  disableDurationFormatting: boolean;
}
