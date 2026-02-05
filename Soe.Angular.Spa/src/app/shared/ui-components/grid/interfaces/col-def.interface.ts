import { ColGroupDef } from 'ag-grid-community';
import {
  CheckboxColumnOptions,
  ColumnOptions,
  SoeColumnType,
} from '../util/column-util';

export interface ISoeColDefContext {
  soeColumnType: SoeColumnType;
  suppressExport?: boolean;
  suppressAutoSizeWithHeader?: boolean;
  options?: ColumnOptions<any>;
}

export interface ISoeColGroupDef extends ColGroupDef {
  soeColumnType: SoeColumnType;
  suppressExport?: boolean;
  options?: ColumnOptions<any>;
}

export interface ISoeActiveColumnParams<T> {
  headerName?: string;
  field?: string;
  options: CheckboxColumnOptions<T>;
}
