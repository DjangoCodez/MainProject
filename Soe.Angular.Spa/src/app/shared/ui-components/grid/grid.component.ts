import { CommonModule } from '@angular/common';
import {
  Component,
  Input,
  OnChanges,
  OnDestroy,
  SimpleChanges,
  ViewChild,
  computed,
  inject,
  input,
  model,
  output,
  signal,
} from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { TranslateService } from '@ngx-translate/core';
import { AutoHeightDirective } from '@shared/directives/auto-height/auto-height.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { MessagingService } from '@shared/services/messaging.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { GridService } from '@shared/services/grid.service';
import { SharedModule } from '@shared/shared.module';
import { StringKeyOfNumberProperty } from '@shared/types';
import { Constants } from '@shared/util/client-constants';
import { DateUtil } from '@shared/util/date-util';
import { NumberUtil } from '@shared/util/number-util';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { StringUtil } from '@shared/util/string-util';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { IconModule } from '@ui/icon/icon.module';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { AgGridAngular, AgGridModule } from 'ag-grid-angular';
import {
  CellClickedEvent,
  CellKeyDownEvent,
  CellValueChangedEvent,
  ColDef,
  ColSpanParams,
  ColumnResizedEvent,
  ColumnRowGroupChangedEvent,
  ColumnState,
  DefaultMenuItem,
  DisplayedColumnsChangedEvent,
  EditableCallbackParams,
  FilterChangedEvent,
  FirstDataRenderedEvent,
  GetContextMenuItemsParams,
  GridApi,
  GridOptions,
  GridReadyEvent,
  IColumnLimit,
  IDetailCellRendererParams,
  IRowNode,
  ManagedGridOptions,
  ModelUpdatedEvent,
  RowClassParams,
  RowDataUpdatedEvent,
  RowDoubleClickedEvent,
  RowDragEndEvent,
  RowGroupingDisplayType,
  RowSelectedEvent,
  RowSelectionOptions,
  SelectionColumnDef,
  SortDirection,
  ValueGetterParams,
  ValueSetterParams,
} from 'ag-grid-community';
import { debounce, groupBy, some } from 'lodash';
import { MultiRowSelectionOptions } from 'node_modules/ag-grid-community/dist/types/src/entities/gridOptions';
import { Observable, Subject } from 'rxjs';
import { take, takeUntil } from 'rxjs/operators';
import { CheckboxCellEditor } from './cell-editors/checkbox-cell-editor/checkbox-cell-editor.component';
import { GroupDisplayType } from './enums/grid-options.enum';
import { GridResizeType } from './enums/resize-type.enum';
import {
  AggregationType,
  IDefaultFilterSettings,
  IGridFilterModified,
  ISoeAggregationConfig,
  ISoeAggregationResult,
  ISoeCountInfoOptions,
  ISoeDragOptions,
  ISoeExportExcelOptions,
} from './interfaces';
import {
  ContextMenuConfig,
  ContextMenuOptions,
  ContextMenuService,
  GetContextMenuCallback,
  GetContextMenuCallbackExtended,
} from './menu-items/context-menu-builder';
import { Dict, SelectedItemService } from './services/selected-item.service';
import {
  AutocompleteColumnOptions,
  CheckboxColumnOptions,
  ColumnHeaderGroupOptions,
  ColumnSingleValueOptions,
  ColumnUtil,
  DateColumnOptions,
  DateTimeColumnOptions,
  ExportUtil,
  FilterUtil,
  GridExportType,
  GridIconUtil,
  GridMenuUtil,
  GroupingOptions,
  ISingelValueConfiguration,
  IconColumnOptions,
  NumberColumnOptions,
  OptionsUtil,
  SelectColumnOptions,
  ShapeColumnOptions,
  SoeColGroupDef,
  SoeColumnType,
  SoeDetailGridContext,
  SoeGridContext,
  SortUtil,
  TextColumnOptions,
  TimeColumnOptions,
  TimeSpanColumnOptions,
  TranslateUtil,
} from './util';

export type AG_NODE_PROPS = { AG_NODE_ID: string };
export type AG_NODE<T> = T & AG_NODE_PROPS;

export declare type CheckboxDataCallback<T> = (
  data: boolean,
  row: AG_NODE<T>
) => void;

export declare type RowClassCallback<T> = (
  params: RowClassParams<T, SoeGridContext>
) => string | string[] | undefined;

export type RowSelectionMode = 'singleRow' | 'multiRow';

export const AG_NODE_ID = 'ag_node_id';

@Component({
  selector: 'soe-grid',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    SharedModule,
    AgGridModule,
    MatAutocompleteModule,
    IconModule,
    AutoHeightDirective,
  ],
  templateUrl: './grid.component.html',
  styleUrls: ['./grid.component.scss'],
  providers: [SelectedItemService],
})
export class GridComponent<T> implements OnDestroy, OnChanges {
  @ViewChild(AgGridAngular) agGrid!: AgGridAngular;
  @ViewChild(AgGridAngular) totalsGrid!: AgGridAngular;
  @Input() options: GridOptions = new OptionsUtil().defaultGridOptions;
  @Input() context: SoeGridContext = new OptionsUtil().defaultGridContext;
  @Input() rows!: Observable<T[] | undefined>;
  parentGuid = input('');
  height = model(0);
  headerHeight = model(32);
  floatingFiltersHeight = model(32);
  rowGroupPanelShow = model(false);
  masterDetail = model(false);

  rowSelected = output<T | undefined>();
  selectionChanged = output<T[]>();
  cellValueChanged = output<CellValueChangedEvent>();
  cellKeyDown = output<CellKeyDownEvent>();
  editRowClicked = output<T>();
  selectedItemsChanged = output<Dict>();
  filterModified = output<IGridFilterModified>();
  cellClicked = output<CellClickedEvent>();

  api!: GridApi;
  totalsApi!: GridApi;

  gridName = '';
  defaultGridState = '';
  private hasRestoredGridState = false;

  gridIsReady = false;

  selection?: RowSelectionOptions;

  selectionColumnDef: SelectionColumnDef = {
    pinned: 'left',
    sortable: true,
    width: 28,
    maxWidth: 28,
    suppressHeaderMenuButton: true,
    suppressHeaderContextMenu: true,
  };

  columns: ColDef[] = [];
  _disableSizeColumnsToFit = false;

  dynamicHeight = true;
  private minRowsToShow = 0;
  private maxRowsToShow?: number = undefined;
  private currentRowCount = 0;

  groupDisplayType: RowGroupingDisplayType | undefined = 'multipleColumns';

  totalRowsCount = signal(0);
  filteredRowsCount = signal(0);
  selectedRowsCount = signal(0);
  totalRowsCountStr = computed(() =>
    this.totalRowsCount().toLocaleString(SoeConfigUtil.languageCode)
  );
  filteredRowsCountStr = computed(() =>
    this.filteredRowsCount().toLocaleString(SoeConfigUtil.languageCode)
  );
  selectedRowsCountStr = computed(() =>
    this.selectedRowsCount().toLocaleString(SoeConfigUtil.languageCode)
  );

  contextMenuService = inject(ContextMenuService);
  contextMenuCallback = signal<GetContextMenuCallback<T> | undefined>(
    undefined
  );
  suppressContextMenu = computed(
    () => this.contextMenuCallback() === undefined
  );

  rowClassCallback = signal<RowClassCallback<T> | undefined>(undefined);
  rowTotalGridClassCallback = signal<
    RowClassCallback<ISoeAggregationResult<T>> | undefined
  >(undefined);

  aggregationConfig?: ISoeAggregationConfig<T> = undefined;
  aggregationResult?: ISoeAggregationResult<T>[] = undefined;
  aggregationErrorResult?: ISoeAggregationResult<T>[] = undefined;
  aggregationGridColumns: ColDef[] = [];
  aggregationGridOptions: GridOptions = new OptionsUtil()
    .defaultAggregationGridOptions;

  selectedItemsService = inject(SelectedItemService);
  progressService = inject(ProgressService);
  messageboxService = inject(MessageboxService);

  icons = GridIconUtil.icons;

  components = {
    // agDateInput: DateCellEditor,
  };

  excelStyles = ExportUtil.excelStyles;

  defaultFilter?: IDefaultFilterSettings;
  masterDetailEnabled = false;
  hasDefaultMasterDetailExpanderColumn = false;
  detailOptions: GridOptions = new OptionsUtil().defaultDetailGridOptions;
  detailColumns: ColDef[] = [];

  columnsAreSizedToFit = true;

  private singleValueConfigurations: ISingelValueConfiguration[] = [];
  private singleValueConfigurationsForDetail: ISingelValueConfiguration[] = [];

  // Utils
  private gridMenuUtil!: GridMenuUtil;
  private filterUtil!: FilterUtil<T>;
  private sortUtil!: SortUtil<T>;

  private _destroy$ = new Subject();

  private perform = new Perform<any>(this.progressService);

  constructor(
    private readonly message: MessagingService,
    private readonly translate: TranslateService,
    private readonly gridService: GridService
  ) {
    // Locale
    // Use provided locale files from AG grid, then override with custom texts
    // English is built in, only our own "aggrid.XXX" and "aggrid.soe.XXX" are translated
    this.options.localeText = {
      ...TranslateUtil.getDefaultLocaleText(this.translate),
    };

    TranslateUtil.getCustomLocaleText(this.translate).subscribe(
      customLocale => {
        this.options.localeText = {
          ...this.options.localeText,
          ...customLocale,
        };
      }
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    const { rows } = changes;
    if (rows && rows.currentValue) {
      this.rows.subscribe(_rows => {
        if (this.gridIsReady) this.setData(_rows || []);
      });
    }
  }

  // INIT

  finalizeInitGrid(
    countInfoOptions?: ISoeCountInfoOptions,
    defaultFilter?: IDefaultFilterSettings
  ) {
    // Export as ExcelTable is not supported in master/detail grids.
    // Do not add the menu items for Export as ExcelTable in that case.
    this.gridMenuUtil = new GridMenuUtil(this);
    this.agGrid.api.updateGridOptions({
      floatingFiltersHeight: this.context.suppressFiltering
        ? 0
        : this.floatingFiltersHeight(),
      getMainMenuItems: params => {
        return this.gridMenuUtil.createDefaultMenu(
          this.gridName,
          false,
          !this.masterDetailEnabled
        );
      },
      onFirstDataRendered: this.onFirstDataRendered.bind(this),
      onFilterChanged: this.onFilterChanged.bind(this),
      onColumnResized: this.onColumnResized.bind(this),
      //onCellKeyDown: this.onCellKeyDown.bind(this),
      onRowDataUpdated: this.onRowDataUpdated.bind(this),
      onColumnRowGroupChanged: this.onColumnRowGroupChanged.bind(this),
    });

    this.gridMenuUtil.closeToolPanel();

    if (!this.context.suppressGridMenu) {
      const colDef = this.gridMenuUtil.createGridMenu();
      this.columns.push(colDef);
    }

    this.checkMasterDetailExpander();
    this.applyCountInfoFeature(countInfoOptions);
    this.applyAggregations();
    this.resetColumns();
    this.restoreGridStateToUser(true);

    this.defaultFilter = defaultFilter;
  }

  // LOCALE

  getLocaleText(key: string): string {
    return this.options.localeText?.[key] ?? '';
  }

  // COLUMNS

  addColumnSingleValue(options?: ColumnSingleValueOptions<T>) {
    const colDef = {
      field: 'soe-ag-single-value-column',
      headerClass: 'soe-ag-single-value-column',
      cellClass: 'soe-ag-single-value-column',
      headerName: '',
      hide: true,
      width: 10,
      suppressHeaderMenuButton: true,
      sortable: false,
      suppressSizeToFit: true,
      suppressMovable: true,
      filter: false,
      resizable: false,
      suppressNavigable: true,
      suppressColumnsToolPanel: true,
      suppressExport: true,
      soeColumnType: SoeColumnType.Text,
      editable: (params: EditableCallbackParams) => {
        const { data } = params.node;
        const singleValueConfig = options?.forDetail
          ? this.singleValueConfigurationsForDetail
          : this.singleValueConfigurations;
        let config = undefined;

        config = singleValueConfig.find(c => c.predicate(data));

        if (config) {
          return config.editable !== undefined ? config.editable : true;
        }
        return false;
      },
      colSpan: (params: ColSpanParams) => {
        const data = params.data;
        const singleValueConfig = options?.forDetail
          ? this.singleValueConfigurationsForDetail
          : this.singleValueConfigurations;

        const config = singleValueConfig.find(c => c.predicate(data));

        if (!config) {
          return 0;
        }

        let columnsToSpan = 1;
        const myPinned = params.column.isPinned();

        while (params.column) {
          const col = params.api.getDisplayedColAfter(params.column);
          if (col == null) {
            break;
          }
          params.column = col;

          if (params.column.isPinned() !== myPinned) {
            continue;
          }

          columnsToSpan++;

          if (
            config.spanTo != null &&
            params.column.getColId() === config.spanTo
          ) {
            break;
          }
        }

        return columnsToSpan;
      },
      valueGetter: (params: ValueGetterParams) => {
        const singleValueConfig = options?.forDetail
          ? this.singleValueConfigurationsForDetail
          : this.singleValueConfigurations;

        const config = singleValueConfig.find(c => c.predicate(params.data));
        return config ? params.data[config.field] : undefined;
      },
      valueSetter: (params: ValueSetterParams) => {
        const singleValueConfig = options?.forDetail
          ? this.singleValueConfigurationsForDetail
          : this.singleValueConfigurations;

        const config = singleValueConfig.find(c => c.predicate(params.data));
        if (!config || params.oldValue === params.newValue) {
          return false;
        }

        params.data[config.field] = params.newValue;
        return true;
      },
      cellRenderer: ({ data, value }: { data: any; value: any }) => {
        const singleValueConfig = options?.forDetail
          ? this.singleValueConfigurationsForDetail
          : this.singleValueConfigurations;

        const config = singleValueConfig.find(c => c.predicate(data));
        if (!config || !config.cellRenderer) {
          return '<span>' + (value || '') + '</span>';
        }

        return config.cellRenderer(data, value);
      },
    } as ColDef;

    if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  enableRowSelection(
    showCheckbox?: (row: IRowNode) => boolean,
    singleRowSelect?: boolean
  ) {
    this.selection = new OptionsUtil().defaultSelectionOptions;
    if (singleRowSelect) this.selection.mode = 'singleRow';
    this.selection.isRowSelectable = showCheckbox;
  }

  addColumnHeader(
    field: string,
    headerName: string,
    options?: ColumnHeaderGroupOptions<T>
  ): SoeColGroupDef {
    const colDef = ColumnUtil.createColumnHeader(field, headerName, options);
    this.columns.push(colDef);

    return colDef;
  }

  addColumnBool(
    field: string,
    headerName: string,
    options?: CheckboxColumnOptions<T>
  ) {
    const colDef = ColumnUtil.createColumnBool(field, headerName, options);

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  addColumnActive(
    field = 'state',
    headerName = this.translate.instant('common.active'),
    options: CheckboxColumnOptions<T> = {}
  ) {
    this.selectedItemsService.selectedItems$
      .pipe(takeUntil(this._destroy$))
      .subscribe((items: Dict) => {
        this.selectedItemsChanged.emit(items);
      });

    const colDef = ColumnUtil.createColumnActive(
      this.selectedItemsService,
      field,
      headerName,
      options
    );

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  addColumnDateTime(
    field: string,
    headerName: string,
    options?: DateTimeColumnOptions<T>
  ) {
    const colDef = ColumnUtil.createColumnDateTime(field, headerName, options);

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  addColumnDate(
    field: string,
    headerName: string,
    options?: DateColumnOptions<T>
  ) {
    const colDef = ColumnUtil.createColumnDate(field, headerName, options);

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  addColumnTime(
    field: string,
    headerName: string,
    options?: TimeColumnOptions<T>
  ) {
    const colDef = ColumnUtil.createColumnTime(field, headerName, options);

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  addColumnTimeSpan(
    field: string,
    headerName: string,
    options?: TimeSpanColumnOptions<T>
  ) {
    const colDef = ColumnUtil.createColumnTimeSpan(field, headerName, options);

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  addColumnIcon(
    field: string | null,
    headerName: string,
    options?: IconColumnOptions<T>,
    p0?: { toolTipField: string }
  ) {
    const colDef = ColumnUtil.createColumnIcon(field, headerName, options);

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  addColumnIconEdit(options?: IconColumnOptions<T>) {
    const colDef = ColumnUtil.createColumnIconEdit(options);

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  addColumnIconDelete(options?: IconColumnOptions<T>) {
    const colDef = ColumnUtil.createColumnIconDelete(options);

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  addColumnModified(field: string, options?: IconColumnOptions<T>) {
    const colDef = ColumnUtil.createColumnModified(field, options);

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  addColumnNumber(
    field: string,
    headerName: string,
    options?: NumberColumnOptions<T>
  ) {
    const colDef = ColumnUtil.createColumnNumber(field, headerName, options);

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  addColumnSelect<U>(
    field: StringKeyOfNumberProperty<T>,
    headerName: string,
    selectOptions: U[],
    onChangeEvent?: null | ((params: any) => void),
    options?: SelectColumnOptions<T, U>
  ) {
    const colDef = ColumnUtil.createColumnSelect(
      <string>field,
      headerName,
      selectOptions,
      onChangeEvent,
      options
    );

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  addColumnShape(
    field: string,
    headerName: string,
    options?: ShapeColumnOptions<T>
  ) {
    const colDef = ColumnUtil.createColumnShape(field, headerName, options);

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  addColumnText(
    field: string,
    headerName: string,
    options?: TextColumnOptions<T>
  ) {
    const colDef = ColumnUtil.createColumnText(field, headerName, options);

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  addColumnAutocomplete<U>(
    field: StringKeyOfNumberProperty<T>,
    headerName: string,
    options: AutocompleteColumnOptions<T, U>
  ) {
    const colDef = ColumnUtil.createColumnAutocomplete(
      <string>field,
      headerName,
      options
    );

    if (options?.headerColumnDef) {
      this.addChild(options.headerColumnDef, colDef);
      return <ColDef>{};
    } else if (options?.returnable) {
      return colDef;
    } else {
      this.columns.push(colDef);
      return <ColDef>{};
    }
  }

  disableSizeColumnsToFit() {
    this._disableSizeColumnsToFit = true;
  }

  resetColumns() {
    this.agGrid.api.updateGridOptions({ columnDefs: this.columns });
  }

  refreshCells() {
    this.agGrid.api.refreshCells();
    // Refresh Total grid if aggregate result is available
    if (this.aggregationResult) this.totalsApi.refreshCells();
  }

  addChild(headerColumnDef: any, columnDef: ColDef) {
    if (!headerColumnDef || !columnDef) return;
    if (!headerColumnDef.children) headerColumnDef.children = [];
    headerColumnDef.children.push(columnDef);
  }

  resizeColumns(resizeType: GridResizeType) {
    switch (resizeType) {
      case GridResizeType.ToFit:
        if (!this._disableSizeColumnsToFit) {
          this.autoSizeColumnsToFit();
          this.columnsAreSizedToFit = true;
        }
        break;
      case GridResizeType.AutoAllAndHeaders:
        this.autoSizeAllColumns(false);
        this.columnsAreSizedToFit = false;
        break;
      case GridResizeType.AutoAllExceptHeaders:
        this.autoSizeAllColumns(true);
        this.columnsAreSizedToFit = false;
        break;
    }
  }

  private autoSizeColumnsToFit() {
    const columnLimits = [] as IColumnLimit[];

    this.options.autoSizeStrategy = {
      type: 'fitGridWidth',
    };
    this.agGrid.gridOptions!.autoSizeStrategy = this.options.autoSizeStrategy;

    this.agGrid
      .api!.getColumns()!
      .filter(c => c.isResizable())
      .forEach(column => {
        if (column.getColDef().minWidth) {
          columnLimits.push({
            key: column.getColId(),
            colId: column.getColId(),
            minWidth: column.getColDef().minWidth,
          } as IColumnLimit);
        }
      });

    this.agGrid.api!.sizeColumnsToFit({ columnLimits: columnLimits });
  }

  private autoSizeAllColumns(skipHeader: boolean) {
    const allColumnIds: string[] = [];
    const exceptColumnIds: string[] = [];

    this.options.autoSizeStrategy = {
      type: 'fitCellContents',
      skipHeader: skipHeader,
    };
    this.agGrid.gridOptions!.autoSizeStrategy = this.options.autoSizeStrategy;

    this.agGrid
      .api!.getColumns()!
      .filter(c => c.isResizable())
      .forEach(column => {
        if (
          !skipHeader &&
          column.getColDef().context.suppressAutoSizeWithHeader
        )
          exceptColumnIds.push(column.getId());
        else allColumnIds.push(column.getId());
      });
    this.agGrid.api!.autoSizeColumns(allColumnIds, skipHeader);
    if (exceptColumnIds.length > 0)
      this.agGrid.api!.autoSizeColumns(exceptColumnIds, true);
  }

  suppressSizeToFitForAllColumns(columnFieldNames: string[] = []) {
    this.columns.forEach(column => {
      if (
        columnFieldNames?.length === 0 ||
        (columnFieldNames?.length > 0 &&
          columnFieldNames.includes(column.field || ''))
      ) {
        column.suppressSizeToFit = true;
      }
    });
  }

  suppressAutoSizeWithHeaderForAllColumns(columnFieldNames: string[] = []) {
    this.columns.forEach(column => {
      if (
        columnFieldNames?.length === 0 ||
        (columnFieldNames?.length > 0 &&
          columnFieldNames.includes(column.field || ''))
      ) {
        column.context.suppressAutoSizeWithHeader = true;
      }
    });
  }

  getColumnDefByField(field: string): any {
    return this.columns.find(colDef => colDef.field === field);
  }

  private createGroupColumnDef(
    headerName: string,
    showRowGroup: string,
    rowGroupIndex: number,
    cellRenderer: string,
    suppressCount: boolean,
    width: number,
    useEmptyCell = false,
    addCheckBox = false,
    resizable = false
  ): any {
    const columnDef = {
      headerName: headerName,
      showRowGroup: showRowGroup,
      rowGroupIndex: rowGroupIndex,
      cellRenderer: cellRenderer,
      cellRendererParams: null,
      width: width,
      filter: false,
      suppressHeaderMenuButton: true,
      resizable: resizable,
    };

    const cellRenderParams = {
      checkbox: addCheckBox,
      suppressCount: suppressCount ? true : false,
    };
    // if (useEmptyCell)
    //     cellRenderParams['innerRenderer'] = () => { return '<span>&nbsp;&nbsp;</span>' };
    (columnDef.cellRendererParams as unknown) = cellRenderParams;

    return columnDef;
  }

  showColumns(columns: string[]) {
    this.agGrid.api.setColumnsVisible(columns, true);
  }

  hideColumns(columns: string[]) {
    this.agGrid.api.setColumnsVisible(columns, false);
  }

  public setSingelValueConfiguration(
    singleValueConfigurations: ISingelValueConfiguration[],
    setVisible: boolean = false
  ): void {
    const columnDef = this.columns.find(
      c => c.field == 'soe-ag-single-value-column'
    );
    if (columnDef) {
      columnDef.hide = false;
      columnDef.cellClassRules = {};
    }

    if (setVisible) this.showColumns(['soe-ag-single-value-column']);

    //group all predicates by cellClass skipping empty ones
    const nonEmptyCellClassPredicates = groupBy(
      singleValueConfigurations.filter(c => c.cellClass),
      x => x.cellClass
    ).value;

    for (const cellClass in nonEmptyCellClassPredicates) {
      //match any of the predicates for a specific css rule.
      if (columnDef?.cellClassRules) {
        columnDef.cellClassRules[cellClass] = ({ data }) =>
          some(nonEmptyCellClassPredicates[cellClass], (c: any) =>
            c.predicate(data)
          );
      }
    }

    this.singleValueConfigurations = singleValueConfigurations;
  }

  public updateSingleValueConfigurationForDetail(
    rowId: string,
    setVisible: boolean = false
  ): void {
    const info = this.api.getDetailGridInfo(rowId);
    const colDefs = info?.api?.getColumnDefs();

    if (colDefs) {
      const colDef: ColDef | undefined = colDefs.find(
        (c: any) => c.field == 'soe-ag-single-value-column'
      );

      if (colDef) {
        colDef.hide = false;
        colDef.cellClassRules = {};
      }

      if (setVisible)
        info?.api?.setColumnsVisible(['soe-ag-single-value-column'], true);

      //group all predicates by cellClass skipping empty ones
      const nonEmptyCellClassPredicates = groupBy(
        this.singleValueConfigurationsForDetail.filter(c => c.cellClass),
        x => x.cellClass
      ).value;

      for (const cellClass in nonEmptyCellClassPredicates) {
        //match any of the predicates for a specific css rule.
        if (colDef?.cellClassRules) {
          colDef.cellClassRules[cellClass] = (data: any) =>
            some(nonEmptyCellClassPredicates[cellClass], (c: any) =>
              c.predicate(data)
            );
        }
      }
    }
    info?.api?.refreshCells({ force: true });
  }

  public setSingelValueConfigurationForDetail(
    singleValueConfigurations: ISingelValueConfiguration[]
  ): void {
    this.singleValueConfigurationsForDetail = singleValueConfigurations;
  }

  // ROWS

  setLoading(value: boolean) {
    if (this.agGrid?.api) this.agGrid.api.setGridOption('loading', value);
  }

  getCurrentRow(): T | undefined {
    return this.sortUtil.getCurrentRow();
  }

  getRowIndex(row: T | number): number {
    if (row) {
      if (typeof row === 'number') {
        return row;
      } else {
        const rowNode = this.getRowNode(row);
        if (rowNode?.rowIndex) return rowNode.rowIndex;
      }
    }

    return this.agGrid.api.getFirstDisplayedRowIndex();
  }

  getRowNode(row: T): IRowNode<T> | undefined {
    const nodeId = this.getRowNodeIdFromData(row);
    return nodeId ? this.agGrid.api.getRowNode(nodeId) : undefined;
  }

  private setNodeIdToData(rowNode: IRowNode<T>) {
    if (rowNode && !rowNode.group) (<any>rowNode.data).AG_NODE_ID = rowNode.id;
  }

  getRowNodeIdFromData(row: T): string {
    return (<any>row).AG_NODE_ID;
  }

  setNbrOfRowsToShow(minNbrOfRows: number, maxNbrOfRows?: number) {
    this.dynamicHeight = false;
    this.minRowsToShow = minNbrOfRows;
    this.maxRowsToShow = maxNbrOfRows;

    // Make sure max is not less than min
    if (this.maxRowsToShow && this.maxRowsToShow < this.minRowsToShow)
      this.maxRowsToShow = this.minRowsToShow;
  }

  updateGridHeightBasedOnNbrOfRows() {
    // Check if filters are displayed
    let showFilters = false;
    this.columns.forEach(colDef => {
      if (colDef.floatingFilter) showFilters = true;
    });

    // Get number of rows in grid
    let nbrOfRows = this.agGrid.rowData ? this.agGrid.rowData.length : 0;

    // Calculate heights of different parts of the grid
    const gridHeaderHeight = this.headerHeight() + 1;
    const gridFilterHeight =
      !showFilters || this.context.suppressFiltering
        ? 0
        : this.floatingFiltersHeight();
    const gridRowHeight = this.options.rowHeight || 32;

    // Check that number of rows are within specified boundaries
    if (nbrOfRows < this.minRowsToShow) {
      nbrOfRows = this.minRowsToShow;
    } else if (this.maxRowsToShow && this.maxRowsToShow < nbrOfRows) {
      nbrOfRows = this.maxRowsToShow;
    }

    // Finally set the height of the grid
    const height =
      gridHeaderHeight + gridFilterHeight + gridRowHeight * nbrOfRows + 2;

    this.height.set(height);
  }

  setRowHeight(height: number) {
    this.headerHeight.set(height);
    this.agGrid.api.setGridOption?.('headerHeight', height);
    this.floatingFiltersHeight.set(height);
    this.agGrid.api.setGridOption?.('floatingFiltersHeight', height);
    this.options.rowHeight = height;
    this.agGrid.api.setGridOption?.('rowHeight', height);

    this.agGrid.api.resetRowHeights();
  }

  setData(rows: T[]) {
    if (this.agGrid) {
      this.agGrid.api.updateGridOptions({ rowData: rows });
    }
  }

  addRow(row: T, isNew: boolean = false) {
    if (isNew) {
      (<any>row)['IS_NEW'] = true;
      this.removeFiltersForColumns();
    }
    this.agGrid.rowData?.push(row);
    this.resetRows(row);
  }

  deleteRow(row: T, sortColumnName?: string) {
    const rows = this.agGrid.rowData;
    if (rows) {
      const index: number = rows.indexOf(row);
      rows.splice(index, 1);
      this.sortUtil.reNumberRows(sortColumnName);
      this.resetRows(undefined, sortColumnName);
    }
  }

  resetRows(row?: T, sortColumnName = '') {
    this.sortUtil.resetRows(row, sortColumnName);
  }

  scrollToFocus(row: T, columnName?: string) {
    this.sortUtil.scrollToFocus(row, columnName);
  }

  alwaysShowVerticalScroll(value = true) {
    this.agGrid.api.updateGridOptions({ alwaysShowVerticalScroll: value });
  }

  removeFiltersForColumns() {
    this.api.getColumnDefs()?.forEach(colDef => {
      // TODO: For now only Text-columns can handle filtering to always show rows with IS_NEW set. Remove all other filters.
      if (colDef.context?.soeColumnType !== SoeColumnType.Text) {
        this.api
          .getColumnFilterInstance(
            colDef.context.field || (<ColDef<any, any>>colDef).field || ''
          )
          .then(dateFilter => {
            if (dateFilter) {
              this.api.setColumnFilterModel(
                colDef.context.field || (<ColDef<any, any>>colDef).field || '',
                null
              );
              this.api.onFilterChanged();
            }
          });
      }
    });
  }

  // TOTAL ROW

  applyCountInfoFeature(countInfoOptions?: ISoeCountInfoOptions) {
    if (countInfoOptions) {
      Object.entries(countInfoOptions).forEach(([key, value]) => {
        if (this.context.countInfo)
          this.context.countInfo[key as keyof ISoeCountInfoOptions] = value;
      });
    }
    if (!this.context.countInfo?.hidden) {
      this.context.countInfo!.termSelected =
        countInfoOptions?.termSelected ||
        this.options.localeText!['selectedRows'] ||
        this.context.countInfo!.termSelected;
      this.context.countInfo!.termFiltered =
        countInfoOptions?.termFiltered ||
        this.options.localeText!['filteredRows'] ||
        this.context.countInfo!.termFiltered;
      this.context.countInfo!.termTotal =
        countInfoOptions?.termTotal ||
        this.options.localeText!['pivotColumnGroupTotals'] ||
        this.context.countInfo!.termTotal;
      this.agGrid.api.addEventListener('rowSelected', () => {
        this.setTotalRowCountInfo();
      });
      this.agGrid.api.addEventListener('modelUpdated', () => {
        this.setTotalRowCountInfo();
      });
    }
  }

  // AGGREGATIONS ROW

  addAggregationsRow(aggregationConfig?: ISoeAggregationConfig<T>) {
    this.aggregationConfig = aggregationConfig;
  }

  setAggregationsErrorRow(
    errorResult?: ISoeAggregationResult<T>[] | undefined
  ) {
    this.aggregationErrorResult = errorResult;
  }

  addAggregationsErrorRow() {
    if (this.aggregationErrorResult) {
      this.aggregationErrorResult.forEach(r => this.aggregationResult?.push(r));
    }
  }

  private applyAggregations() {
    if (!this.aggregationConfig) {
      return;
    }

    this.resetAggregationGridColumns();
    this.applyAggregationEvents();
  }

  public resetAggregationGridColumns(): void {
    const nonTotalColumnType = (soeColumnType: SoeColumnType): boolean => {
      return soeColumnType == SoeColumnType.Icon;
    };

    const getCellRenderer = (
      soeColumnType: SoeColumnType,
      defaultCellRenderer: any
    ) => {
      switch (soeColumnType) {
        case SoeColumnType.Text:
        case SoeColumnType.Select:
        case SoeColumnType.Autocomplete:
        case SoeColumnType.Shape:
        case SoeColumnType.Icon:
          return null;
        default:
          return defaultCellRenderer;
      }
    };

    const allColumns = (cols: ColDef[]): ColDef[] =>
      cols.flatMap(col =>
        (col as any).children ? allColumns((col as any).children) : [col]
      );

    this.aggregationGridColumns = allColumns(this.columns).map(colDef => {
      const {
        context,
        minWidth,
        maxWidth,
        width,
        flex,
        field,
        valueGetter,
        valueFormatter,
        cellRenderer,
        cellRendererParams,
        cellClass,
        hide,
      } = colDef;

      const soeColumnType = context?.soeColumnType || SoeColumnType.Text;

      return {
        soeColumnType: nonTotalColumnType(soeColumnType)
          ? SoeColumnType.Text
          : soeColumnType,
        minWidth,
        maxWidth,
        width,
        flex,
        field,
        pinned: undefined,
        valueGetter,
        valueFormatter,
        cellRenderer: getCellRenderer(soeColumnType, cellRenderer),
        cellRendererParams,
        cellClass,
        hide,
      };
    });

    if (this.selection) {
      const extraCol = ColumnUtil.createColumnText('', '', {
        width: this.selectionColumnDef.width,
        maxWidth: this.selectionColumnDef.maxWidth,
      });
      this.aggregationGridColumns.unshift(extraCol);
    }
  }

  private applyAggregationEvents(): void {
    this.api.addEventListener('cellValueChanged', () => this.runAggregations());
    this.api.addEventListener('rowValueChanged', () => this.runAggregations());
    this.api.addEventListener('modelUpdated', () => this.runAggregations());
    // Removed this since it triggers on scroll and that it not good if you manually resize a column first
    // this.api.addEventListener('viewportChanged', () =>
    //   this.api.sizeColumnsToFit()
    // );
  }

  private runAggregations() {
    const result: ISoeAggregationResult<T> = {};
    const fields = Object.keys(this.aggregationConfig!).map(
      x => x as StringKeyOfNumberProperty<T>
    );
    for (const field of fields) {
      this.api.forEachNodeAfterFilter(n => {
        const val = (n.data[field] as number) || 0;
        if (result[field] === undefined) {
          result[field] = val;
          return;
        }
        switch (this.aggregationConfig![field]) {
          case AggregationType.Max: {
            result[field] = Math.max(result[field]!, val);
            break;
          }
          case AggregationType.Min: {
            result[field] = Math.min(result[field]!, val);
            break;
          }
          case AggregationType.Sum:
          default: {
            result[field]! += val;
            break;
          }
        }
      });
    }
    for (const field of fields) {
      if (result[field] === undefined) {
        result[field] = 0;
      }
    }
    this.aggregationResult = [result];
    this.addAggregationsErrorRow();
    this.api.sizeColumnsToFit();
  }

  private setTotalRowCountInfo = debounce(
    () => {
      let totalRowsCounter = 0;
      let filteredRowsCounter = 0;

      if (!this.agGrid.api.isDestroyed()) {
        this.agGrid.api.forEachNode(params => {
          if (params.data) totalRowsCounter++;
        });

        this.agGrid.api.forEachNodeAfterFilter(params => {
          if (params.data) filteredRowsCounter++;
        });
      }

      this.totalRowsCount.set(totalRowsCounter);
      this.filteredRowsCount.set(filteredRowsCounter);
      const rowsSelected = this.agGrid.api.isDestroyed()
        ? 0
        : this.agGrid.api.getSelectedRows();
      this.selectedRowsCount.set(Object.keys(rowsSelected).length);
    },
    250,
    { leading: false, trailing: true }
  );

  // GROUPING

  useGrouping(options: GroupingOptions = {}) {
    const opt: Partial<ManagedGridOptions> = {
      groupTotalRow: options.stickyGroupTotalRow, // sticky behaviour top/bottom/turned off
      grandTotalRow: options.includeTotalFooter
        ? 'bottom'
        : options.stickyGrandTotalRow, // sticky behaviour top/bottom/turned off
      rowGroupPanelShow: options.hideGroupPanel ? 'never' : 'always',
      suppressDragLeaveHidesColumns: options.keepColumnsAfterGroup,
      suppressGroupChangesColumnVisibility: 'suppressShowOnUngroup',
      onRowGroupOpened: params => {
        this.agGrid.api.sizeColumnsToFit();
      },
    };

    this.groupDisplayType = 'singleColumn';
    const totalTerm = options.totalTerm ? `${options.totalTerm}: ` : 'Total: ';

    if (options.selectChildren) {
      (<MultiRowSelectionOptions>this.selection).groupSelects =
        options.groupSelectsFiltered ? 'filteredDescendants' : 'descendants';

      opt.onRowSelected = (params: RowSelectedEvent<T>) => {
        if (params.node.group) {
          const selected = params.node.isSelected() || false;
          params.node.childrenAfterGroup?.forEach((childNode: IRowNode<T>) => {
            if (
              !options.groupSelectsFiltered ||
              (options.groupSelectsFiltered && childNode.displayed)
            )
              childNode.setSelected(selected);
          });
        }
      };
    }

    opt.autoGroupColumnDef = {
      minWidth: 200,
      resizable: true,
      sortable: true,
      suppressHeaderMenuButton: true,
      // comparator: (valueA, valueB, nodeA, nodeB, isInverted) => {
      //   return AgGridUtility.groupComparator(nodeA, nodeB, valueA, valueB);
      // },
      // cellRendererParams: {
      //   suppressCount: options.suppressCount,
      //   totalValueGetter: function (params: any) {
      //     let value = params.value;
      //     const node = params.node;

      //     //console.log('params: ', params);

      //     if (
      //       value &&
      //       node.field &&
      //       (node.field.toLowerCase() || '').includes('date') &&
      //       DateUtil.isValidDate(value)
      //     ) {
      //       const jsDate = DateUtil.parseDate(value);
      //       if (jsDate)
      //         value = jsDate.toLocaleDateString(SoeConfigUtil.language);
      //     }
      //     return "Totti" + (value ?? '');
      //   },
      // },
      cellRendererParams: {
        suppressCount: options.suppressCount,
        totalValueGetter: (params: any) => {
          let { value, node } = params;
          const colDef = node.rowGroupColumn?.getColDef() ?? undefined;

          if (
            colDef &&
            (colDef.soeColumnType === SoeColumnType.Date ||
              colDef.soeColumnType === SoeColumnType.DateTime)
          ) {
            value = totalTerm + (node.key ? node.key : value);
          } else if (
            colDef &&
            colDef.valueFormatter &&
            typeof colDef.valueFormatter === 'function'
          ) {
            value = totalTerm + colDef.valueFormatter(params);
          }

          if (!value) return this.getLocaleText('footerTotal');
          else return value;
        },
      },
    };

    // Batch update all options
    this.agGrid.api.updateGridOptions(opt);
  }

  setGroupDisplayType(displayType: GroupDisplayType) {
    switch (displayType) {
      case GroupDisplayType.Custom:
        this.groupDisplayType = 'custom';
        break;
      case GroupDisplayType.GroupRows:
        this.groupDisplayType = 'groupRows';
        break;
      case GroupDisplayType.MultipleColumns:
        this.groupDisplayType = 'multipleColumns';
        break;
      case GroupDisplayType.SingleColumn:
        this.groupDisplayType = 'singleColumn';
        break;
      case GroupDisplayType.None:
        this.groupDisplayType = undefined;
        break;
    }
  }

  groupRowsByColumn(
    column: string | any,
    showRowGroup: string | boolean,
    groupDefaultExpanded = 0
  ): void {
    let columnDef = column;
    if (typeof column === 'string') {
      columnDef = this.getColumnDefByField(column);
    } else if (column.colId) {
      //ag-grid columns have colId
      columnDef = column.colDef;
    }
    columnDef.showRowGroup = showRowGroup;
    columnDef.rowGroup = true;
    columnDef.hide = true;

    if (columnDef.cellRendererParams) {
      columnDef.cellRendererParams.suppressCount = true;
      columnDef.cellRendererParams.innerRenderer = columnDef.cellRenderer;
    }
    this.agGrid.api.updateGridOptions({
      groupDefaultExpanded: groupDefaultExpanded,
    });
  }

  public ungroupColumn(column: string | any) {
    let columnDef = column;
    if (typeof column === 'string') {
      columnDef = this.getColumnDefByField(column);
    } else if (column.colId) {
      //ag-grid columns have colId
      columnDef = column.colDef;
    }
    columnDef.showRowGroup = false;
    columnDef.rowGroup = false;
    columnDef.hide = false;
    columnDef.cellRenderer = columnDef['parentCellRenderer'] ?? undefined;
  }

  groupRowsByColumnAndHide(
    column: any,
    cellRenderer: string,
    index: number,
    suppressCount: boolean,
    useEmptyCell = false,
    addCheckBox = false,
    resizable = false
  ): void {
    const groupColumn = this.createGroupColumnDef(
      column.headerName,
      column.field,
      index,
      cellRenderer,
      suppressCount,
      column.width,
      useEmptyCell,
      addCheckBox,
      resizable
    );
    if (groupColumn) {
      this.columns.splice(index, 0, groupColumn);

      column.rowGroup = true;
      column.hide = true;

      if (!this._disableSizeColumnsToFit) this.agGrid.api.sizeColumnsToFit();
    }
  }

  addGroupAggFunction(name: string, aggFunc: (aggFuncParams: any) => void) {
    this.agGrid.api.addAggFuncs({ [name]: aggFunc });
  }

  addGroupTimeSpanSumAggFunction(emptyWhenZero: boolean, noSum = false) {
    const sumTimeSpan = (aggFuncParams: any) => {
      //IAggFuncParams
      let sumMinutes = 0;

      if (!noSum) {
        if (
          aggFuncParams &&
          aggFuncParams.values &&
          aggFuncParams.values.length
        ) {
          for (const value of aggFuncParams.values) {
            if (
              aggFuncParams.colDef['minutesToTimeSpan'] &&
              StringUtil.isNumeric(value)
            )
              sumMinutes += value;
            else sumMinutes += DateUtil.timeSpanToMinutes(value);
          }
        }
      }
      return (emptyWhenZero && sumMinutes <= 0) || noSum
        ? ''
        : DateUtil.minutesToTimeSpan(sumMinutes);
    };
    this.addGroupAggFunction('sumTimeSpan', values => {
      return sumTimeSpan(values);
    });
  }

  addGroupTimeSpanMinAggFunction() {
    this.addGroupAggFunction('minTimeSpan', params => {
      let minMinutes = undefined;
      if (params && params.values) {
        for (const value of params.values) {
          let minutes;
          if (StringUtil.isNumeric(value)) minutes = value;
          else minutes = DateUtil.timeSpanToMinutes(value);
          if (minMinutes === undefined || minMinutes > minutes)
            minMinutes = minutes;
        }
      }
      return DateUtil.minutesToTimeSpan(minMinutes || 0);
    });
  }

  addGroupTimeSpanMaxAggFunction() {
    this.addGroupAggFunction('maxTimeSpan', params => {
      let maxMinutes = undefined;
      if (params && params.values) {
        for (const value of params.values) {
          let minutes;
          if (StringUtil.isNumeric(value)) minutes = value;
          else minutes = DateUtil.timeSpanToMinutes(value);
          if (maxMinutes === undefined || maxMinutes < minutes)
            maxMinutes = minutes;
        }
      }
      return DateUtil.minutesToTimeSpan(maxMinutes || 0);
    });
  }

  addGroupTimeSpanAverageAggFunction() {
    const avgTimeSpan = (aggFuncParams: any) => {
      //IAggFuncParams
      let sumMinutes = 0;

      if (
        aggFuncParams &&
        aggFuncParams.values &&
        aggFuncParams.values.length
      ) {
        for (const value of aggFuncParams.values) {
          if (
            aggFuncParams.colDef['minutesToTimeSpan'] &&
            StringUtil.isNumeric(value)
          )
            sumMinutes += value;
          else sumMinutes += DateUtil.timeSpanToMinutes(value);
        }
      }
      return DateUtil.minutesToTimeSpan(
        sumMinutes / aggFuncParams.values.length
      );
    };
    this.addGroupAggFunction('avgTimeSpan', values => {
      return avgTimeSpan(values);
    });
  }

  addGroupTimeSpanMedianAggFunction() {
    const medianTimeSpan = (aggFuncParams: any) => {
      //IAggFuncParams
      const minutesValues: number[] = [];

      if (
        aggFuncParams &&
        aggFuncParams.values &&
        aggFuncParams.values.length
      ) {
        for (const value of aggFuncParams.values) {
          if (
            aggFuncParams.colDef['minutesToTimeSpan'] &&
            StringUtil.isNumeric(value)
          )
            minutesValues.push(value);
          else minutesValues.push(DateUtil.timeSpanToMinutes(value));
        }
      }
      return DateUtil.minutesToTimeSpan(NumberUtil.median(minutesValues));
    };
    this.addGroupAggFunction('medianTimeSpan', values => {
      return medianTimeSpan(values);
    });
  }

  setAllGroupExpended(expanded: boolean, level?: number) {
    this.agGrid.api.forEachNode(n => {
      if (level !== undefined) {
        if (n.level === level && n.group) n.setExpanded(expanded);
      } else {
        if (n.group) {
          n.setExpanded(expanded);
        }
      }
    });
  }

  enableGroupFooter() {
    this.agGrid.groupTotalRow = 'bottom';
    this.agGrid.api.updateGridOptions({ groupTotalRow: 'bottom' });
  }

  enableGroupTotalFooter() {
    this.agGrid.grandTotalRow = 'bottom';
    this.agGrid.api.updateGridOptions({ grandTotalRow: 'bottom' });
  }

  showGroupPanel() {
    this.rowGroupPanelShow.set(true);
  }

  // MASTER DETAIL

  enableMasterDetail(
    detailOptions: GridOptions,
    detailContext?: SoeDetailGridContext,
    singleValueConfigurationsForDetail?: ISingelValueConfiguration[]
  ) {
    // if configurations for single value are provided, set them
    if (singleValueConfigurationsForDetail) {
      this.setSingelValueConfigurationForDetail(
        singleValueConfigurationsForDetail
      );
    }

    let mergedDetailOptions: GridOptions = new OptionsUtil()
      .defaultDetailGridOptions;
    mergedDetailOptions = Object.assign(mergedDetailOptions, detailOptions);

    let mergedDetailContext: SoeDetailGridContext = new OptionsUtil()
      .defaultDetailGridContext;
    if (detailContext)
      mergedDetailContext = Object.assign(mergedDetailContext, detailContext);

    if (mergedDetailContext.addDefaultExpanderCol) {
      const colDef = ColumnUtil.createColumnMasterDetail();
      if (mergedDetailContext.defaultExpanderColHeader)
        this.addChild(mergedDetailContext.defaultExpanderColHeader, colDef);
      else this.columns.push(colDef);
      this.hasDefaultMasterDetailExpanderColumn = true;
    }

    if (mergedDetailContext.suppressFiltering) {
      mergedDetailOptions.floatingFiltersHeight = 0;
    }

    this.masterDetail.set(true);
    this.masterDetailEnabled = true;

    this.options.detailRowHeight = mergedDetailOptions.detailRowHeight;
    this.options.detailRowAutoHeight = mergedDetailContext.autoHeight;

    this.options.isRowMaster = mergedDetailOptions.isRowMaster;

    // console.log(
    //   'enableMasterDetail',
    //   mergedDetailOptions,
    //   mergedDetailContext,
    //   this.columns
    // );
    this.agGrid.api.updateGridOptions({
      isRowMaster: this.options.isRowMaster,
      detailCellRendererParams: {
        getDetailRowData: mergedDetailContext.getDetailRowData,
        detailGridOptions: this.buildNestedDetailGridOptions(
          mergedDetailOptions,
          mergedDetailContext
        ),
      } as IDetailCellRendererParams,
    });
  }

  buildNestedDetailGridOptions(
    detailOptions: GridOptions,
    detailContext?: SoeDetailGridContext
  ): GridOptions {
    let mergedDetailOptions: GridOptions = new OptionsUtil()
      .defaultDetailGridOptions;
    mergedDetailOptions = Object.assign(mergedDetailOptions, detailOptions);

    let mergedDetailContext: SoeDetailGridContext = new OptionsUtil()
      .defaultDetailGridContext;
    if (detailContext)
      mergedDetailContext = Object.assign(mergedDetailContext, detailContext);

    // console.log(
    //   'buildNestedDetailGridOptions',
    //   mergedDetailOptions,
    //   mergedDetailContext
    // );
    if (mergedDetailContext.detailOptions) {
      mergedDetailOptions.masterDetail = true;
      mergedDetailOptions.columnDefs?.unshift(
        ColumnUtil.createColumnMasterDetail()
      );
      mergedDetailOptions.detailCellRendererParams = {
        getDetailRowData:
          mergedDetailContext.getDetailRowData ||
          ((params: any) => {
            params.successCallback([params]);
          }),
        detailGridOptions: this.buildNestedDetailGridOptions(
          mergedDetailContext.detailOptions,
          mergedDetailContext.detailContext
        ),
      };
      // if (detailContextParams.detailContext?.getDetailRowData) {
      //   detailParams.detailCellRendererParams.getDetailRowData =
      //     detailContextParams.detailContext.getDetailRowData;
      // }
      if (mergedDetailContext.detailOptions.rowData) {
        mergedDetailOptions.rowData = mergedDetailContext.detailOptions.rowData;

        // if (!detailContextParams.detailContext?.getDetailRowData) {
        //   detailParams.detailCellRendererParams.getDetailRowData = (
        //     params: any
        //   ) => {
        //     params.successCallback([params]);
        //   };
        // }
      }
      if (mergedDetailContext.detailOptions.detailRowHeight)
        mergedDetailOptions.detailRowHeight =
          mergedDetailContext.detailOptions.detailRowHeight;
      if (mergedDetailContext.detailContext?.autoHeight)
        mergedDetailOptions.detailRowAutoHeight =
          mergedDetailContext.detailContext?.autoHeight;
    }

    // if (mergedDetailContext.columns && mergedDetailContext.columns.length > 0) {
    //   mergedDetailContext.columns.forEach((column: any) => {
    //     mergedDetailOptions.columnDefs?.push(column);
    //   });
    // }

    // Single row select
    // TODO: Should detail have it's own selection property?
    if (this.selection?.mode === 'singleRow') {
      mergedDetailOptions.onRowSelected = (params: any) => {
        this.agGrid.api.forEachDetailGridInfo(d => {
          const rows = d.api?.getSelectedRows();
          if (rows && rows.length > 1) {
            const selectedIndex = params.rowIndex;
            d.api?.deselectAll();
            d.api?.forEachNode((node: IRowNode) => {
              if (node.rowIndex == selectedIndex) node.setSelected(true);
            });
          }
        });
      };
    }

    return mergedDetailOptions;
  }

  checkMasterDetailExpander() {
    // Since there seem to be a problem when setting the masterDetail option
    // and it gets overridden on the this.options object, we have to use the
    // local variable masterDetailEnabled for now.
    if (
      this.masterDetailEnabled &&
      !this.hasDefaultMasterDetailExpanderColumn
    ) {
      const firstColumn = this.columns.find(x => x.pinned !== 'left');
      if (firstColumn) {
        firstColumn.cellRenderer = 'agGroupCellRenderer';
        if (!firstColumn.cellRendererParams)
          firstColumn.cellRendererParams = {};
        firstColumn.cellRendererParams.suppressDoubleClickExpand = true;
      }
    }
  }

  // SORTING

  sort(field: string, direction: SortDirection) {
    this.sortUtil.sort(field, direction);
  }

  // Sort by renumber column that grid is sorted on
  sortFirst(sortColumnName = '') {
    this.sortUtil.sortFirst(sortColumnName);
  }

  sortUp(sortColumnName = '') {
    this.sortUtil.sortUp(sortColumnName);
  }

  sortDown(sortColumnName = '') {
    this.sortUtil.sortDown(sortColumnName);
  }

  sortLast(sortColumnName = '') {
    this.sortUtil.sortLast(sortColumnName);
  }

  // FILTERING

  onFilterModified(): void {
    this.filterModified.emit(this.filterUtil.getFilterModel());
  }

  setDefaultFilter() {
    this.filterUtil.setDefaultFilter(this.defaultFilter, this.columns);
  }

  clearFilters() {
    ColumnUtil.flagCheckboxColumnsForClear(
      this.agGrid.columnDefs,
      this.agGrid.gridOptions
    );
    this.filterUtil.clearFilters();
    this.setDefaultFilter();
  }

  clearFocusedCell() {
    if (this.agGrid?.api) {
      this.agGrid.api.clearFocusedCell();
    }
  }

  getFilteredRows(): T[] {
    const rows: T[] = [];
    if (this.agGrid.api) {
      this.agGrid.api.forEachNodeAfterFilterAndSort(({ data }) => {
        if (data) rows.push(data);
      });
    }

    return rows;
  }

  getAllRows(): T[] {
    const rows: T[] = [];
    if (this.agGrid.api) {
      this.agGrid.api.forEachNode(node => {
        rows.push(node.data);
      });
    }

    return rows;
  }

  // SELECTING

  setRowSelection(value: RowSelectionMode) {
    if (!this.selection) this.selection = { mode: value };
    else this.selection.mode = value;
  }

  selectAllRows() {
    this.agGrid.api.forEachNode(node => node.setSelected(true));
  }

  clearSelectedRows() {
    this.agGrid.api.forEachNode(node => node.setSelected(false));
  }

  getSelectedRows(): T[] {
    return this.agGrid.api.getSelectedRows();
  }

  getSelectedCount(): number {
    return this.getSelectedRows().length;
  }

  getSelectedIds(idField: string): number[] {
    return this.getSelectedRows().map(row => (<any>row)[idField] as number);
  }

  selectRowOnCellClicked(columnName?: string) {
    const row = this.getCurrentRow();
    if (row) this.scrollToFocus(row, columnName);
  }

  clearSelectedItems() {
    this.selectedItemsService.clear();
  }

  // ROW DRAG

  applyDragOptions(options: ISoeDragOptions) {
    const gridOptions: ManagedGridOptions = {};

    if (options.hideContentOnDrag)
      gridOptions.suppressMoveWhenRowDragging = options.hideContentOnDrag;

    if (options.rowDragableEntireRow)
      gridOptions.rowDragEntireRow = options.rowDragableEntireRow;

    if (options.rowDragFinishedCallback) {
      gridOptions.onRowDragEnd = options.rowDragFinishedCallback;
    }

    if (options.rowDragFinishedSortIndexNrFieldName) {
      gridOptions.onRowDragEnd = (event: RowDragEndEvent) => {
        this.sortUtil.sortIndexNrOnDragEnd(
          options.rowDragFinishedSortIndexNrFieldName || ''
        );
        if (options.rowDragFinishedCallback) {
          options.rowDragFinishedCallback();
        }
      };
    }

    if (options.rowDragMultiRow) {
      gridOptions.rowDragMultiRow = options.rowDragMultiRow;
      gridOptions.rowSelection = 'multiple';
    }

    if (!options.suppressRowDragManaged) gridOptions.rowDragManaged = true;

    this.agGrid.api.updateGridOptions(gridOptions);
  }

  // SIDEBAR

  enableSideBar() {
    this.gridMenuUtil.enableSideBar();
  }

  // EXPORT

  exportRows(exportType: GridExportType, allRows = false) {
    const exportFilename = this.translate.instant(
      this.context.exportFilenameKey || 'core.export'
    );

    ExportUtil.exportRows(
      this.agGrid,
      this.api,
      this.context.exportExcelOptions,
      exportFilename,
      exportType,
      allRows
    );
  }

  setExportExcelOptions(options: ISoeExportExcelOptions) {
    this.context.exportExcelOptions = options;
  }

  // SAVE STATE

  initSaveDefaultGridState() {
    this.validateAdminPassword(
      'core.uigrid.savedefaultstatewarning',
      this.saveDefaultGridState.bind(this)
    );
  }

  private saveDefaultGridState() {
    if (!this.gridName) return;

    this.perform.crud(
      CrudActionTypeEnum.Save,
      this.gridService.saveSysGridState(
        this.gridName,
        this.agGrid.api.getColumnState()
      ),
      undefined,
      undefined,
      {
        showDialog: false,
        showToastOnComplete: true,
      }
    );
  }

  initDeleteDefaultGridState() {
    this.validateAdminPassword(
      'core.uigrid.deletedefaultstatewarning',
      this.deleteDefaultGridState.bind(this)
    );
  }

  private deleteDefaultGridState() {
    if (!this.gridName) return;

    this.perform.crud(
      CrudActionTypeEnum.Delete,
      this.gridService.deleteSysGridState(this.gridName),
      () => this.restoreGridState(this.defaultGridState),
      undefined,
      {
        showDialog: false,
        showToastOnComplete: true,
      }
    );
  }

  private validateAdminPassword(textKey: string, validCallback: () => void) {
    const mb = this.messageboxService.warning('core.warning', textKey, {
      showInputText: true,
      inputTextLabel: 'core.enterpassword',
      isPassword: true,
    });

    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (!response?.result) return;

      if (
        response?.result &&
        response?.textValue === Constants.ADMIN_PASSWORD
      ) {
        validCallback();
      } else {
        this.showWrongPasswordDialog();
      }
    });
  }

  private showWrongPasswordDialog() {
    this.messageboxService.error('core.warning', 'core.wrongpassword', {
      type: 'forbidden',
    });
  }

  saveUserGridState() {
    if (!this.gridName) return;

    this.perform.crud(
      CrudActionTypeEnum.Save,
      this.gridService.saveUserGridState(
        this.gridName,
        this.agGrid.api.getColumnState()
      ),
      undefined,
      undefined,
      {
        showDialog: false,
        showToastOnComplete: true,
      }
    );
  }

  deleteUserGridState() {
    if (!this.gridName) return;

    this.perform.crud(
      CrudActionTypeEnum.Delete,
      this.gridService.deleteUserGridState(this.gridName),
      () => this.restoreGridStateToDefault(),
      undefined,
      {
        showDialog: false,
        showToastOnComplete: true,
      }
    );
  }

  restoreGridStateToDefault() {
    if (!this.gridName || this.context.suppressGridMenu) return;

    this.gridService
      .getSysGridState(this.gridName)
      .pipe(take(1))
      .subscribe(state => {
        this.restoreGridState(state);
      });
  }

  restoreGridStateToUser(saveCurrentAsDefault: boolean) {
    if (!this.gridName || this.context.suppressGridMenu) return;

    if (saveCurrentAsDefault) {
      this.defaultGridState = JSON.stringify(this.agGrid.api.getColumnState());
    }

    // On the server side, if no UserGridState exists SysGridState will be fetched as fallback
    this.gridService
      .getUserGridState(this.gridName)
      .pipe(take(1))
      .subscribe(state => {
        this.restoreGridState(state);
      });
  }

  private restoreGridState(state: string) {
    if (state) {
      this.setColumnStateOnAll(JSON.parse(state));
      this.userGridStateRestored();
    } else if (this.defaultGridState !== '') {
      this.setColumnStateOnAll(JSON.parse(this.defaultGridState));
      this.userGridStateRestored();
    } else {
      if (!this._disableSizeColumnsToFit) this.agGrid.api.sizeColumnsToFit();
      this.userGridStateRestored();
    }
  }

  private setColumnStateOnAll(state: ColumnState[]) {
    this.agGrid.api.applyColumnState({
      state: state,
      applyOrder: true,
    });
  }

  private userGridStateRestored() {
    setTimeout(() => {
      this.hasRestoredGridState = true;
    }, 200);
  }

  private resizeColumnWidth(): void {
    if (!this._disableSizeColumnsToFit && this.api) {
      this.api.sizeColumnsToFit();
    }
    window.addEventListener('resize', () => {
      setTimeout(() => {
        if (!this._disableSizeColumnsToFit && this.api) {
          this.api.sizeColumnsToFit();
        }
      });
    });
    if (!this._disableSizeColumnsToFit && this.api) {
      this.api.sizeColumnsToFit();
    }
  }

  public addContextMenu(
    config?: Partial<ContextMenuConfig>,
    extendedContextMenuItems?: (
      | ContextMenuOptions<T, SoeGridContext>
      | DefaultMenuItem
    )[]
  ) {
    if (!config) config = {};
    return this.setContextMenuCallback((row, params, builder) => {
      if (!row) {
        return builder.build(); // Return empty menu instead of undefined
      }
      return builder.buildDefaultContextMenu(
        params,
        config,
        extendedContextMenuItems
      );
    });
  }

  public setContextMenuCallback(callback: GetContextMenuCallbackExtended<T>) {
    this.contextMenuCallback.set((params: GetContextMenuItemsParams<T>) => {
      const data = params.node?.data;
      return callback(data, params, this.contextMenuService.builder<T>());
    });
  }

  public setRowClassCallback(callback: RowClassCallback<T>) {
    this.rowClassCallback.set(callback);
  }

  public setTotalGridRowClassCallback(
    callback: RowClassCallback<ISoeAggregationResult<T>>
  ) {
    this.rowTotalGridClassCallback.set(callback);
  }

  // EDITING

  applyChanges(): void {
    if (this.agGrid.api.getEditingCells()?.length > 0) {
      this.agGrid.api.stopEditing();
    }
  }

  startEditing(row: number, colummnName: string) {
    if (this.agGrid.api) {
      this.agGrid.api.setFocusedCell(row, colummnName);
      this.agGrid.api.startEditingCell({
        rowIndex: row,
        colKey: colummnName,
      });
    }
  }

  applyStartEditOnCellFocused() {
    this.api.updateGridOptions({
      onCellFocused: this.startEditOnCellFocused.bind(this),
    });
  }

  // EVENTS

  private startEditOnCellFocused(event: any) {
    setTimeout(() => {
      if (
        event.column &&
        event.column.getColDef().field &&
        event.column.getColDef().editable
      ) {
        this.api.startEditingCell({
          rowIndex: event.rowIndex,
          colKey: event.column.getColDef().field,
        });
      }
    }, 0);
  }

  onGridReady(event: GridReadyEvent) {
    this.api = event.api;

    this.filterUtil = new FilterUtil<T>(this.api);
    this.sortUtil = new SortUtil<T>(this);

    this.message.publishGridReady(this, this.parentGuid());
    this.gridIsReady = true;

    if (this.rows) {
      this.rows.pipe(take(1)).subscribe(value => {
        this.setData(value || []);
      });
    }

    // Resizing column width
    if (this.hasRestoredGridState) this.resizeColumnWidth();
  }

  onTotalsGridReady(event: GridReadyEvent) {
    this.totalsApi = event.api;

    this.agGrid.api.updateGridOptions({
      alignedGrids: () => [this.totalsApi],
    });
    setTimeout(() => this.totalsApi.redrawRows(), 0);
  }

  onRowSelected(params: RowSelectedEvent<T>) {
    // AG-Grid calls the "onRowSelected" event twice, once for the row being selected, and once for the row being unselected.
    // Therefore we need to look if the node is actually selected before emitting this event.
    if (params.node.isSelected()) {
      if (this.selection?.mode === 'singleRow' && params.data) {
        const rows = params.api.getSelectedRows();
        if (rows.length > 1) {
          const selectedId = (<any>params.data).AG_NODE_ID;
          params.api.deselectAll();
          params.api.forEachNode((node: IRowNode<T>) => {
            if (node.data && (<any>node.data).AG_NODE_ID === selectedId)
              node.setSelected(true);
          });
        }
      }
      this.rowSelected.emit(params.data);
    }
  }

  onSelectionChanged(params: any) {
    this.selectionChanged.emit(this.agGrid.api.getSelectedRows());
  }

  onRowDoubleClicked(event: RowDoubleClickedEvent) {
    if (!this.context.suppressDoubleClickToEdit)
      this.editRowClicked.emit(event.data);
  }

  onCellClicked(event: CellClickedEvent) {
    if (
      event.colDef.editable &&
      (event.colDef.context.soeColumnType === SoeColumnType.Bool ||
        event.colDef.context.soeColumnType === SoeColumnType.Active)
    ) {
      // When clicking with the mouse in a checkbox cell, set the opposite value directly
      // Will cause a toggle without having to double click in the checkbox
      const cellEditor =
        event.api.getCellEditorInstances()[0] as CheckboxCellEditor<AG_NODE<T>>;
      if (cellEditor) {
        cellEditor.control.setValue(!event.value);
        // onChange was not called in editor due to instant stopEditing and is instead called from here
        cellEditor.onChange();
        event.api.stopEditing();
      }
    }

    this.cellClicked.emit(event);
  }

  onCellKeyDown(
    event: any /* CellKeyDownEvent<any, any> | FullWidthCellKeyDownEvent<any, any> */
  ) {
    const colDef = (event as CellKeyDownEvent).colDef;
    if (
      colDef?.editable
      //  &&
      // (colDef.context.soeColumnType === SoeColumnType.Time ||
      //   colDef.context.soeColumnType === SoeColumnType.TimeSpan)
    ) {
      const oldValue = event.node.data[colDef.field as string];
      const newValue = event.event?.target?.value ?? '';

      if (oldValue !== newValue)
        this.cellKeyDown.emit(event as CellKeyDownEvent);

      if ((event.event as KeyboardEvent).key === 'Enter') {
        event.api.tabToNextCell();
      }
    }
  }

  onCellValueChanged(event: CellValueChangedEvent): void {
    this.cellValueChanged.emit(event);
  }

  onDisplayedColumnsChanged(event: DisplayedColumnsChangedEvent): void {
    if (
      this.hasRestoredGridState &&
      !this._disableSizeColumnsToFit &&
      this.agGrid.api &&
      this.columnsAreSizedToFit
    )
      this.agGrid.api.sizeColumnsToFit();
  }

  onFirstDataRendered(params: FirstDataRenderedEvent) {
    this.setDefaultFilter();
  }

  onColumnResized(event: ColumnResizedEvent) {
    if (event.source == 'uiColumnResized') this.columnsAreSizedToFit = false;
  }

  onColumnRowGroupChanged(event: ColumnRowGroupChangedEvent<T>) {
    const columns = event.columns;
    columns?.forEach((column, index) => {
      const colDef = column?.getColDef();
      if (!column?.isRowGroupActive())
        event.api.setColumnsVisible([colDef?.field || ''], true);
    });
  }

  onFilterChanged(event: FilterChangedEvent) {
    if (this.columnsAreSizedToFit && !this._disableSizeColumnsToFit)
      this.resizeColumns(GridResizeType.ToFit);
  }

  onRowDataUpdated(event: RowDataUpdatedEvent<T>) {
    this.agGrid.api.forEachNode(this.setNodeIdToData);
  }

  onModelUpdated(event: ModelUpdatedEvent<T>): void {
    const rowCount = event.api.getDisplayedRowCount();
    if (!this.dynamicHeight && this.currentRowCount !== rowCount) {
      this.currentRowCount = rowCount;
      this.updateGridHeightBasedOnNbrOfRows();
    }
  }

  ngOnDestroy(): void {
    this._destroy$.next([]);
    this._destroy$.complete();
  }
}
