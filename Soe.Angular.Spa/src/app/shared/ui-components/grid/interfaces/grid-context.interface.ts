import { ColDef, GridOptions } from 'ag-grid-enterprise';
import { ISoeCountInfoOptions } from '.';
import { ISoeExportExcelOptions } from './export-excel-options.interface';
import { SoeColGroupDef } from '../util';

export interface ISoeGridContext {
  suppressFiltering?: boolean;
  suppressGridMenu?: boolean;
  suppressDoubleClickToEdit?: boolean;
  exportFilenameKey?: string;
  countInfo?: ISoeCountInfoOptions;
  detailRow?: GridOptions;
  exportExcelOptions?: ISoeExportExcelOptions;
}

export interface ISoeDetailGridContext {
  suppressFiltering?: boolean;
  suppressGridMenu?: boolean;
  options?: GridOptions;
  masterDetail?: boolean;
  columnDefs?: ColDef[];
  groupDefaultExpanded?: number;
  autoHeight?: boolean;
  detailRowHeight?: number;
  detailHeightByChildCollectionName?: string;
  pagination?: boolean;
  paginationAutoPageSize?: boolean;
  addDefaultExpanderCol?: boolean;
  defaultExpanderColHeader?: SoeColGroupDef;
  rowData?: any;
  getDetailRowData?: (params: any) => void;
  detailOptions?: GridOptions;
  detailContext?: ISoeDetailGridContext;
}
