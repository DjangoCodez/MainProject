import { ISoeGridContext, ISoeDetailGridContext } from '../interfaces';
import { ColumnUtil } from './column-util';
import { GridOptions, RowSelectionOptions } from 'ag-grid-community';

export declare type SoeGridContext = ISoeGridContext;
export declare type SoeDetailGridContext = ISoeDetailGridContext;

export class OptionsUtil {
  constructor() {}

  defaultSelectionOptions: RowSelectionOptions = {
    mode: 'multiRow',
    enableClickSelection: false,
    hideDisabledCheckboxes: true,
    checkboxes: true,
    selectAll: 'filtered',
  };

  defaultGridOptions: GridOptions = {
    // Accessories
    suppressMenuHide: true,
    suppressContextMenu: true,
    sideBar: null,
    popupParent: document.querySelector('body') || undefined,

    // Row
    rowHeight: 32,

    // Column definitions
    defaultColDef: ColumnUtil.defaultColDef,

    // Column moving
    suppressDragLeaveHidesColumns: true,

    // Column sizing
    //colResizeDefault: 'shift',

    // Cell
    cellSelection: true,

    // Editing
    singleClickEdit: true,

    // Integrated Charts
    chartThemes: ['ag-pastel'],

    // Miscellaneous
    alignedGrids: [],
    debug: false,

    // Overlays
    suppressNoRowsOverlay: true,
    loading: false,

    // Pivot and Aggregation
    suppressAggFuncInHeader: true,

    // Rendering
    animateRows: false,

    // Scrolling
    suppressScrollOnNewData: true,

    // Sorting
    accentedSort: true,

    // Tooltips
    tooltipShowDelay: 1000,

    // Events
    //onColumnResized: event => console.log('A column was resized'),

    // Callbacks
    //getRowHeight: (params) => 25

    // Menu
    columnMenu: 'legacy',

    // Context
    context: {},

    // Master/detail
    detailRowAutoHeight: true,

    stopEditingWhenCellsLoseFocus: true,
  };

  defaultGridContext: SoeGridContext = {
    suppressFiltering: false,
    suppressGridMenu: false,
    suppressDoubleClickToEdit: false,
    exportFilenameKey: '',

    // Footer / records count info
    countInfo: {
      hidden: false,
      pinnedLeftText: '',
      prefixText: '',
      termTotal: 'Total',
      termFiltered: 'Filtered',
      termSelected: 'Selected',
      tooltip: '',
    },
  };

  defaultDetailGridOptions: GridOptions = {
    // Accessories
    sideBar: null,
    suppressContextMenu: true,
    popupParent: document.querySelector('body') || undefined,

    // Column definitions
    columnDefs: [],
    defaultColDef: ColumnUtil.defaultColDef,

    // Column moving
    suppressDragLeaveHidesColumns: true,

    // Column sizing
    //colResizeDefault: 'shift',

    // Editing
    singleClickEdit: true,

    // Integrated Charts
    chartThemes: ['ag-pastel'],

    // Miscellaneous
    debug: false,

    // Overlays
    suppressNoRowsOverlay: true,

    // Pivot and Aggregation
    suppressAggFuncInHeader: true,

    // Rendering
    //animateRows: true,

    // Scrolling
    suppressScrollOnNewData: true,

    // Sorting
    accentedSort: true,

    // Tooltips
    tooltipShowDelay: 1000,

    // Context
    context: {},

    // Master/detail
    detailRowAutoHeight: true,
  };

  defaultDetailGridContext: SoeDetailGridContext = {
    suppressFiltering: false,
    suppressGridMenu: true,
    addDefaultExpanderCol: true,
    autoHeight: true,
  };

  defaultAggregationGridOptions: GridOptions = {
    // Accessories
    sideBar: null,
    suppressContextMenu: true,

    // Selection
    suppressRowHoverHighlight: true,
    suppressCellFocus: true,

    // Miscellaneous
    alignedGrids: [],

    // Scrolling
    suppressHorizontalScroll: true,
  };
}

export class GroupingOptions {
  includeFooter?: boolean;
  includeTotalFooter?: boolean;
  stickyGroupTotalRow?: 'bottom' | 'top';
  stickyGrandTotalRow?: 'bottom' | 'top';
  keepColumnsAfterGroup?: boolean;
  selectChildren?: boolean;
  groupSelectsFiltered?: boolean;
  // keepGroupState?: boolean;  // No longer needed
  totalTerm?: string;
  hideGroupPanel?: boolean;
  suppressCount?: boolean;
}
