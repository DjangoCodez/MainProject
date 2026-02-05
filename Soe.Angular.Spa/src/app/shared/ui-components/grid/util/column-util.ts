import { IconName, IconPrefix } from '@fortawesome/fontawesome-svg-core';
import { DateUtil } from '@shared/util/date-util';
import { IconUtil } from '@shared/util/icon-util';
import { NumberUtil } from '@shared/util/number-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { CheckboxCellEditor } from '../cell-editors/checkbox-cell-editor/checkbox-cell-editor.component';
import {
  DateCellEditor,
  DateCellEditorParams,
} from '../cell-editors/date-cell-editor/date-cell-editor.component';
import {
  AutocompleteCellEditor,
  AutocompleteCellEditorParams,
  getAutocompleteCacheKey,
} from '../cell-editors/autocomplete-cell-editor/autocomplete-cell-editor.component';
import { CheckboxCellRenderer } from '../cell-renderers/checkbox-cell-renderer/checkbox-cell-renderer.component';
import { IconCellRenderer } from '../cell-renderers/icon-cell-renderer/icon-cell-renderer.component';
import {
  ShapeCellRenderer,
  ShapeType,
} from '../cell-renderers/shape-cell-renderer/shape-cell-renderer.component';
import {
  CheckboxFloatingFilter,
  CheckboxFloatingFilterParams,
} from '../column-filters/checkbox-column-filter/checkbox-column-filter.component';
import { CheckboxIndeterminate } from '../enums/checkbox-state.enum';
import {
  AutocompleteCellRendererParams,
  IAutocompleteButtonConfigurationParams,
  ICheckboxCellRendererParams,
  ICheckboxEditorParams,
  IIconButtonConfiguration,
  IIconCellRendererParams,
  IShapeCellRendererParams,
  IShapeConfiguration,
  ISoeColDefContext,
  ISoeColGroupDef,
  ITextButtonCellRendererParams,
  ShapeCellRendererParams,
} from '../interfaces';
import { SelectedItemService } from '../services/selected-item.service';
import { AG_NODE, CheckboxDataCallback } from '../grid.component';
import { TextButtonCellRenderer } from '../cell-renderers/text-button-cell-renderer/text-button-cell-renderer.component';
import { signal } from '@angular/core';
import { Observable } from 'rxjs';
import {
  StringKeyOfNumberProperty,
  StringKeyOfStringProperty,
} from '@shared/types';
import {
  CellClassParams,
  CellClassRules,
  CheckboxSelectionCallback,
  ColDef,
  ColGroupDef,
  ColSpanParams,
  EditableCallback,
  EditableCallbackParams,
  GridOptions,
  IDateFilterParams,
  IGroupCellRendererParams,
  IMultiFilterParams,
  IRowNode,
  ISetFilterParams,
  NewValueParams,
  RowDragCallback,
  SortDirection,
  SuppressKeyboardEventParams,
  ValueFormatterParams,
  ValueGetterParams,
  ValueSetterParams,
} from 'ag-grid-community';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import { AutocompleteCellRenderer } from '../cell-renderers/autocomplete-cell-renderer/autocomplete-cell-renderer.component';
import {
  IconInnerHeaderComponent,
  IIconInnerHeaderParams,
} from '../header-components/icon-inner-header/icon-inner-header.component';
import {
  TimeCellEditor,
  TimeCellEditorParams,
} from '../cell-editors/time-cell-editor/time-cell-editor.component';
import {
  NumberCellEditor,
  NumberCellEditorParams,
} from '../cell-editors/number-cell-editor/number-cell-editor.component';
import {
  TimeSpanCellEditor,
  TimeSpanCellEditorParams,
} from '../cell-editors/time-span-cell-editor/time-span-cell-editor.component';
import { AnimationProp } from '@fortawesome/angular-fontawesome';

export declare type ColumnDateTimePivotType =
  | 'year'
  | 'quarter'
  | 'month'
  | 'formattedMonth'
  | 'day'
  | 'hour'
  | 'minute'
  | 'second';
export declare type Field = string;
export declare type Predicate<T> = (data: AG_NODE<T>) => boolean;
export declare type FieldOrPredicate<T> = string | Predicate<AG_NODE<T>>;
export declare type BoolOrCallback<T> =
  | boolean
  | ((data: AG_NODE<T>) => boolean);
export declare type DataCallback<T> = (data: AG_NODE<T>) => void;
export declare type DataCallback2<T, U> = (data: AG_NODE<T>, data2: U) => void;
export declare type SoeColDefContext = ISoeColDefContext;
export declare type SoeColGroupDef = ColGroupDef & ISoeColGroupDef;
export enum SoeColumnType {
  Active,
  Bool,
  Date,
  DateTime,
  Icon,
  Number,
  RowSelection,
  Select,
  Shape,
  Text,
  Time,
  TimeSpan,
  Autocomplete,
}

// COLUMN OPTIONS

export class ColumnOptions<T> {
  // Value
  valueGetter?: (params: ValueGetterParams<T>) => any;
  valueSetter?: (params: ValueSetterParams<T>) => boolean;

  // Align
  alignLeft?: boolean;
  alignRight?: boolean;
  alignCenter?: boolean;
  pinned?: 'left' | 'right';

  // Size
  width?: number;
  minWidth?: number;
  maxWidth?: number;
  flex?: number;
  resizable?: boolean;
  suppressSizeToFit?: boolean;
  suppressAutoSizeWithHeader?: boolean;

  // Filter
  filter?: boolean | string;
  suppressFilter?: boolean;
  suppressFloatingFilter?: boolean;
  showSetFilter?: boolean;
  filterOptions?: any[];

  // Tooltip
  tooltip?: string;
  tooltipField?: string;

  // Header
  iconHeaderParams?: IIconInnerHeaderParams;

  // Export
  suppressExport?: boolean;

  // Visibility
  enableHiding?: boolean;
  hide?: boolean;

  /**
   * editable props determines whether the column should be editable or
   */
  editable?: boolean | EditableCallback<T>;

  // Drag row
  rowDragable?: boolean | RowDragCallback<any, any>;

  // Sorting
  sortable?: boolean;
  sort?: SortDirection;
  sortIndex?: number | null;

  colSpan?: (params: ColSpanParams<T, any>) => number;

  // Grouping
  enableGrouping?: boolean;
  grouped?: boolean;

  // Grouped header column
  headerColumnDef?: SoeColGroupDef;

  // CSS
  cellClassRules?: CellClassRules<T>;
  strikeThrough?: boolean | Predicate<T>;
  columnSeparator?: boolean;
  headerSeparator?: boolean;

  // Checkbox selection
  checkboxSelection?: boolean | CheckboxSelectionCallback | undefined;

  // Select items
  dynamicSelectOptions?: (row: T) => any[];
  changeSelectData?: (row: T) => any;

  // Custom cell renderer
  cellRenderer?: any;
  cellRendererParams?: any;

  public static default<T>(): ColumnOptions<T> {
    return {
      alignLeft: true,
      filter: true,
      floatingFilter: true,
      enableHiding: false,
      sortable: true,
      resizable: true,
    } as ColumnOptions<T>;
  }
}

export class ColumnSingleValueOptions<T> extends ColumnOptions<T> {
  returnable?: boolean;
  forDetail?: boolean;

  public static default<T>(): ColumnOptions<T> {
    return {} as ColumnOptions<T>;
  }
}

export class ColumnHeaderGroupOptions<T> {
  suppressStickyLabel?: boolean;
  suppressSpanHeaderHeight?: boolean;
  openByDefault?: boolean;
  tooltip?: string;
  marryChildren?: boolean;
  enableHiding?: boolean;

  public static default<T>(): ColumnOptions<T> {
    return {
      marryChildren: true,
      enableHiding: true,
    } as ColumnOptions<T>;
  }
}

export class CheckboxColumnOptions<T> extends ColumnOptions<T> {
  checked?: boolean;
  setChecked?: boolean | CheckboxIndeterminate;
  filterIndeterminate?: boolean;
  showCheckbox?: FieldOrPredicate<T>;
  onClick?: CheckboxDataCallback<T>;
  idField?: string;
  returnable?: boolean;

  public static default<T>(): ColumnOptions<T> {
    return {
      editable: false,
      setChecked: false,
      suppressAutoSizeWithHeader: true,
    } as ColumnOptions<T>;
  }
}

export class DateTimeColumnOptions<T> extends ColumnOptions<T> {
  dateFormat?: string;
  showSeconds?: boolean;
  returnable?: boolean;
  pivoting?: boolean;
  pivotHierarchy?: ColumnDateTimePivotType[];

  public static default<T>(): ColumnOptions<T> {
    return {} as ColumnOptions<T>;
  }
}

export class DateColumnOptions<T> extends DateTimeColumnOptions<T> {
  public static default<T>(): ColumnOptions<T> {
    return {} as ColumnOptions<T>;
  }
}

export class TimeColumnOptions<T> extends DateTimeColumnOptions<T> {
  aggFuncOnGrouping?: any;
  useLocaleTimeFormatting?: boolean;

  public static default<T>(): ColumnOptions<T> {
    return { alignRight: true } as ColumnOptions<T>;
  }
}

export class TimeSpanColumnOptions<T> extends ColumnOptions<T> {
  showDays?: boolean;
  showSeconds?: boolean;
  padHours?: boolean;
  maxOneDay?: boolean;
  clearZero?: BoolOrCallback<T>;
  aggFuncOnGrouping?: any;
  disableTimeFormatting?: boolean;
  returnable?: boolean;

  public static default<T>(): ColumnOptions<T> {
    return {
      alignRight: true,
    } as ColumnOptions<T>;
  }
}

export class IconColumnOptions<T> extends ColumnOptions<T> {
  useIconFromField?: boolean;
  iconPrefix?: IconPrefix;
  iconName?: IconName;
  //iconNames?: string;
  iconClass?: string;
  iconClassField?: string;
  iconAnimation?: AnimationProp;
  iconAnimationField?: string;
  showIcon?: FieldOrPredicate<T>;
  onClick?: DataCallback<T>;
  enableResize?: boolean;
  showTooltipInFilter?: boolean;
  returnable?: boolean;

  public static default<T>(): ColumnOptions<T> {
    return {
      useIconFromField: false,
      iconPrefix: 'fal',
      width: 30,
      minWidth: 30,
      maxWidth: 30,
      filter: false,
      suppressExport: true,
      suppressSizeToFit: true,
      suppressFilter: true,
      showTooltipInFilter: false,
    } as ColumnOptions<T>;
  }
}

export class NumberColumnOptions<T> extends ColumnOptions<T> {
  decimals?: number;
  maxDecimals?: number;
  formatAsText?: boolean;
  clearZero?: BoolOrCallback<T>;
  allowEmpty?: boolean;
  aggFuncOnGrouping?: string;
  buttonConfiguration?: IIconButtonConfiguration<T>;
  returnable?: boolean;

  public static default<T>(): ColumnOptions<T> {
    return {
      alignRight: true,
    } as ColumnOptions<T>;
  }
}

export class SelectColumnOptions<T, U> extends ColumnOptions<T> {
  dropDownIdLabel?: StringKeyOfNumberProperty<U>;
  dropDownValueLabel?: StringKeyOfStringProperty<U>;
  shapeConfiguration?: IShapeConfiguration<T>;
  dynamicSelectOptions?: (row: T) => any[];
  returnable?: boolean;
  sortByDisplayValue?: boolean;

  public static default<T>(): ColumnOptions<T> {
    return {
      dropDownIdLabel: 'id',
      dropDownValueLabel: 'name',
      sortByDisplayValue: true,
    } as ColumnOptions<T>;
  }
}

export class ShapeColumnOptions<T> extends ColumnOptions<T> {
  shape?: ShapeType;
  color?: string;
  colorField?: string;
  isSelect?: boolean;
  showShapeField?: string;
  useGradient?: boolean;
  gradientField?: string;
  width?: number;
  iconPrefix?: IconPrefix;
  iconName?: IconName;
  iconClass?: string;
  iconClassField?: string;
  iconAnimation?: AnimationProp;
  iconAnimationField?: string;
  showIcon?: FieldOrPredicate<T>;
  returnable?: boolean;

  public static default<T>(): ColumnOptions<T> {
    return { width: 20, suppressSizeToFit: true } as ColumnOptions<T>;
  }
}

export class TextColumnOptions<T> extends ColumnOptions<T> {
  buttonConfiguration?: IIconButtonConfiguration<T>;
  shapeConfiguration?: IShapeConfiguration<T>;
  usePlainText?: boolean;
  returnable?: boolean;

  public static default<T>(): ColumnOptions<T> {
    return {} as ColumnOptions<T>;
  }
}

export class AutocompleteColumnOptions<T, U> extends ColumnOptions<T> {
  source!: (data?: T) => U[];
  updater?: DataCallback2<T, U | undefined>;
  delay?: number = 0;
  useSroll?: boolean;
  /** The name of the property containing the value being stored */
  optionIdField?: StringKeyOfNumberProperty<U>;
  /** The name of the property containing the string to show when autocomplete is open */
  optionNameField?: StringKeyOfStringProperty<U>;
  /** The name of the property containing the string to show in the grid (will use @see optionNameField if not set) */
  optionDisplayNameField?: StringKeyOfStringProperty<T>;
  limit?: number = 20;
  scrollable?: boolean = true;
  buttonConfiguration?: IIconButtonConfiguration<T>;
  allowNavigationFrom?: (value: any, data: T) => boolean = () => true;
  returnable?: boolean;

  public static default<T>(): ColumnOptions<T> {
    return { optionIdField: 'id', optionNameField: 'name' } as ColumnOptions<T>;
  }
}

export interface ISingelValueConfiguration {
  predicate: (data: any) => boolean;
  field: string;
  editable?: boolean;
  cellClass?: string;
  cellRenderer?: (data: any, value: any) => string;
  spanTo?: Field;
}

// UTIL CLASS

export class ColumnUtil {
  // FORMATTERS

  private static dateFormatter<T>(
    params: ValueFormatterParams,
    options: DateColumnOptions<T>
  ) {
    let formatted = '';
    if (params.value) {
      // If grouped, ensure value is a Date object
      if (params.node?.group) params.value = new Date(params.value);

      const date = DateUtil.parseDateOrJson(params.value);
      if (date) {
        formatted = options.dateFormat
          ? date.toFormattedDate(options.dateFormat)
          : date.toLocaleDateString(SoeConfigUtil.languageCode);
      }
    }
    return formatted;
  }

  private static dateTimeFormatter<T>(
    params: ValueFormatterParams,
    options: DateTimeColumnOptions<T>
  ) {
    let formatted = '';
    if (params.value) {
      const date = DateUtil.parseDateOrJson(params.value);
      if (date) {
        formatted = options.dateFormat
          ? date.toFormattedDate(options.dateFormat)
          : `${date.toLocaleDateString(
              SoeConfigUtil.languageCode
            )} ${date.toLocaleTimeString(
              SoeConfigUtil.languageCode,
              options.showSeconds ? {} : { hour: '2-digit', minute: '2-digit' }
            )}`;
      }
    }
    return formatted;
  }

  private static timeFormatter<T>(
    params: ValueFormatterParams,
    options: TimeColumnOptions<T>
  ) {
    let formatted = '';
    if (params.value) {
      const date = DateUtil.parseDateOrJson(params.value);
      if (date) {
        // Only allow locale formatting for non-editable time columns
        const languageCode =
          options.useLocaleTimeFormatting && !options.editable
            ? SoeConfigUtil.languageCode
            : 'sv-SE';
        formatted = options.dateFormat
          ? date.toFormattedDate(options.dateFormat)
          : date.toLocaleTimeString(
              languageCode,
              options.showSeconds ? {} : { hour: '2-digit', minute: '2-digit' }
            );
      }
    }
    return formatted;
  }

  private static timeSpanFormatter<T>(
    params: ValueFormatterParams,
    options: TimeSpanColumnOptions<T>
  ) {
    let formatted = '';
    if (
      params.value ||
      (typeof options.clearZero === 'function'
        ? !options.clearZero(params.node?.data)
        : !options.clearZero)
    ) {
      formatted = DateUtil.minutesToTimeSpan(
        params.value || 0,
        options.showDays,
        options.showSeconds,
        options.padHours,
        options.maxOneDay
      );
    }

    return formatted;
  }

  private static numberFormatter<T>(
    params: ValueFormatterParams,
    options: NumberColumnOptions<T>
  ) {
    let { value, node } = params;
    const { clearZero, allowEmpty, formatAsText, decimals, maxDecimals } =
      options;

    if (
      (node?.group && node?.expanded && !node?.footer) ||
      (node?.group && !options?.aggFuncOnGrouping)
    )
      return '';

    if (typeof value === 'string' && value.length > 0) {
      value = NumberUtil.parseDecimal(value);
    }

    if (
      typeof value === 'undefined' ||
      value === null ||
      isNaN(value) ||
      value === 0
    ) {
      const clearZeroResult =
        typeof clearZero === 'function'
          ? clearZero(params.node?.data)
          : clearZero;

      return allowEmpty ||
        clearZeroResult ||
        (params.context.aggregation === true && value !== 0)
        ? ''
        : formatAsText
          ? '0'
          : NumberUtil.formatDecimal(0, decimals, maxDecimals);
    }

    return formatAsText
      ? value.toString()
      : NumberUtil.formatDecimal(value, decimals, maxDecimals);
  }

  // COMPARATORS

  private static dateComparator = (d1: string | Date, d2: string | Date) => {
    const date1 = DateUtil.parseDateOrJson(d1);
    const date2 = DateUtil.parseDateOrJson(d2);

    if (!date1 && !date2) return 0;
    if (!date1) return -1;
    if (!date2) return 1;

    date1.clearHours();
    date2.clearHours();

    if (date1.isEqual(date2)) return 0;
    else if (date1.isAfter(date2)) return -1;
    else if (date1.isBefore(date2)) return 1;

    return 0;
  };

  private static dateTimeComparator = (
    d1: string | Date,
    d2: string | Date
  ) => {
    const date1 = DateUtil.parseDateOrJson(d1);
    const date2 = DateUtil.parseDateOrJson(d2);

    if (!date1 && !date2) return 0;
    if (!date1) return -1;
    if (!date2) return 1;

    if (date1.isEqual(date2)) return 0;
    else if (date1.isAfter(date2)) return 1;
    else if (date1.isBefore(date2)) return -1;

    return 0;
  };

  private static timeComparator = (d1: string | Date, d2: string | Date) => {
    const date1 = DateUtil.parseDateOrJson(d1);
    const date2 = DateUtil.parseDateOrJson(d2);

    if (!date1 && !date2) return 0;
    if (!date1) return -1;
    if (!date2) return 1;

    date1.clearDate();
    date2.clearDate();

    if (date1.isEqual(date2)) return 0;
    else if (date1.isAfter(date2)) return 1;
    else if (date1.isBefore(date2)) return -1;

    return 0;
  };

  // RENDERERS

  private static checkboxCellRendererParams<T, R>(
    options: CheckboxColumnOptions<T>
  ) {
    const { editable, showCheckbox } = options;

    return {
      disabled: !editable,
      showCheckbox,
    } as ICheckboxCellRendererParams<T, R>;
  }

  private static iconCellRendererParams<T>(options: IconColumnOptions<T>) {
    const {
      useIconFromField,
      iconPrefix,
      iconName,
      iconClass,
      iconClassField,
      iconAnimation,
      iconAnimationField,
      tooltip,
      showIcon,
      onClick,
    } = options;

    const icon = IconUtil.createIcon(iconPrefix, iconName);

    return {
      useIconFromField,
      icon,
      iconClass,
      iconClassField,
      iconAnimation,
      iconAnimationField,
      tooltip,
      showIcon,
      onClick,
    } as IIconCellRendererParams<T>;
  }

  private static shapeCellRendererParams<T>(options: ShapeColumnOptions<T>) {
    const {
      shape,
      color,
      colorField,
      isSelect,
      showShapeField,
      useGradient,
      gradientField,
      width,
      iconPrefix,
      iconName,
      iconClass,
      showIcon,
    } = options;

    const icon = IconUtil.createIcon(iconPrefix, iconName);

    return {
      shape: shape,
      color: color,
      colorField: colorField,
      isSelect: isSelect,
      showShapeField: showShapeField,
      useGradient: useGradient,
      gradientField: gradientField,
      width: width,
      icon: icon,
      showIcon: showIcon,
      iconClass: iconClass,
    } as IShapeCellRendererParams<T>;
  }

  private static textButtonCellRendererParams<T>(
    options: TextColumnOptions<T>
  ) {
    const { buttonConfiguration } = options;

    const icon = IconUtil.createIcon(
      buttonConfiguration?.iconPrefix,
      buttonConfiguration?.iconName
    );

    return {
      icon: icon,
      iconClass: buttonConfiguration?.iconClass,
      tooltip: buttonConfiguration?.tooltip,
      onClick: buttonConfiguration?.onClick,
      show: buttonConfiguration?.show,
    } as ITextButtonCellRendererParams<T>;
  }

  private static autocompleteCellRendererParams<T, U>(
    options: AutocompleteColumnOptions<T, U>
  ) {
    let buttonConfiguration = <IAutocompleteButtonConfigurationParams<T>>{};

    if (options.buttonConfiguration) {
      const icon = IconUtil.createIcon(
        options.buttonConfiguration?.iconPrefix,
        options.buttonConfiguration?.iconName
      );
      buttonConfiguration = {
        icon: icon,
        iconClass: options.buttonConfiguration.iconClass ?? '',
        tooltip: options.buttonConfiguration.tooltip,
        onClick: options.buttonConfiguration.onClick,
        // Show button is default true, if no function is specified for it
        show: options.buttonConfiguration.show
          ? options.buttonConfiguration.show
          : () => true,
      } as IAutocompleteButtonConfigurationParams<T>;
    }

    return {
      optionIdField: options.optionIdField,
      optionNameField: options.optionNameField,
      optionDisplayNameField: options.optionDisplayNameField,
      buttonConfiguration: buttonConfiguration,
    } as AutocompleteCellRendererParams<T, U>;
  }

  private static autocompleteCellRendererDefaultParams<T, U>(
    options: AutocompleteColumnOptions<T, U>
  ): AutocompleteCellRendererParams<T, U> {
    return {
      optionIdField: options.optionIdField,
      optionNameField: options.optionNameField,
      optionDisplayNameField: options.optionDisplayNameField,
      buttonConfiguration: <IAutocompleteButtonConfigurationParams<T>>{},
    } as AutocompleteCellRendererParams<T, U>;
  }

  private static lookupValue<T>(
    items: T[],
    dropDownIdLabel: StringKeyOfNumberProperty<T>,
    dropDownValueLabel: StringKeyOfStringProperty<T>,
    id: number
  ) {
    return (
      items.find(x => x[dropDownIdLabel] === id)?.[dropDownValueLabel] || ''
    );
  }

  // EDITORS

  private static getCheckboxCellEditorParams<T>(
    options: CheckboxColumnOptions<T>
  ) {
    const { editable, showCheckbox, onClick } = options;

    return {
      disabled: !editable,
      showCheckbox: showCheckbox,
      onClick: onClick,
    } as ICheckboxEditorParams<T>;
  }

  private static getAutocompleteCellEditorParams<T, U>(
    options: AutocompleteColumnOptions<T, U>
  ) {
    let buttonConfiguration = <IAutocompleteButtonConfigurationParams<T>>{};

    if (options.buttonConfiguration) {
      const icon = IconUtil.createIcon(
        options.buttonConfiguration?.iconPrefix,
        options.buttonConfiguration?.iconName
      );
      buttonConfiguration = {
        icon: icon,
        iconClass: options.buttonConfiguration?.iconClass ?? '',
        tooltip: options.buttonConfiguration?.tooltip,
        onClick: options.buttonConfiguration?.onClick,
        // Show button is default true, if no function is specified for it
        show: options.buttonConfiguration.show
          ? options.buttonConfiguration.show
          : () => true,
      } as IAutocompleteButtonConfigurationParams<T>;
    }

    return {
      disabled: !options.editable,
      updater: options.updater,
      source: options.source,
      delay: options.delay,
      limit: options.limit,
      scrollable: options.scrollable,
      optionIdField: options.optionIdField,
      optionNameField: options.optionNameField,
      optionDisplayNameField: options.optionDisplayNameField,
      buttonConfiguration: buttonConfiguration,
      allowNavigationFrom: options.allowNavigationFrom,
    } as AutocompleteCellEditorParams<T, U>;
  }

  // FILTERS

  private static getCheckboxFloatingFilterParams<T>(
    options: CheckboxColumnOptions<T>
  ) {
    return {
      canBeIndeterminate:
        typeof options.filterIndeterminate === 'boolean'
          ? options.filterIndeterminate
          : true,
      setChecked:
        typeof options.setChecked === 'undefined' ? true : options.setChecked,
    };
  }

  private static get dateFilterParams(): IDateFilterParams {
    return {
      // https://www.ag-grid.com/angular-data-grid/filter-date/
      comparator: this.dateComparator,
      browserDatePicker: true,
      maxValidYear: 9999,
      includeBlanksInLessThan: true,
      inRangeInclusive: true,
    };
  }

  private static get dateMultiFilterParams(): IMultiFilterParams {
    return {
      filters: [
        {
          filter: 'agDateColumnFilter',
          filterParams: this.dateFilterParams,
        },
        {
          filter: 'agSetColumnFilter',
          filterParams: {} as ISetFilterParams,
        },
      ],
    };
  }

  private static get numberFilterOptions() {
    return [
      {
        displayKey: 'contains',
        displayName: 'Contains',
        predicate: (filterValue: number, cellValue: number) => {
          return cellValue || cellValue === 0
            ? cellValue.toString().includes(filterValue.toString())
            : false;
        },
      },
      {
        displayKey: 'startsWith',
        displayName: 'Starts with',
        predicate: (filterValue: number, cellValue: number) => {
          return cellValue || cellValue === 0
            ? cellValue.toString().startsWith(filterValue.toString())
            : false;
        },
      },
      'lessThan',
      'equals',
      'greaterThan',
    ];
  }

  private static get numberFilterParams() {
    return {
      filterOptions: this.numberFilterOptions,
      defaultOption: 'contains',
    };
  }

  private static get numberMultiFilterParams(): IMultiFilterParams {
    return {
      filters: [
        {
          filter: 'agNumberColumnFilter',
          filterParams: this.numberFilterParams,
        },
        {
          filter: 'agSetColumnFilter',
          filterParams: {} as ISetFilterParams,
        },
      ],
    };
  }

  private static getSelectFilterParams<T>(
    _this: any,
    data: T[],
    dropDownIdLabel: StringKeyOfNumberProperty<T>,
    dropDownValueLabel: StringKeyOfStringProperty<T>
  ) {
    return {
      newRowsAction: 'keep',
      comparator: (a: string | null, b: string | null) => {
        if (!a) {
          return -1;
        } else if (!b) {
          return 1;
        }

        const aItem = data.find(d => d[dropDownIdLabel] === +a);
        const bItem = data.find(d => d[dropDownIdLabel] === +b);

        if (!aItem) {
          return -1;
        } else if (!bItem) {
          return 1;
        }

        const aText = aItem[dropDownValueLabel]! as string;
        const bText = bItem[dropDownValueLabel]! as string;

        return aText.toLowerCase().localeCompare(bText.toLowerCase());
      },
      valueFormatter: function (params: ValueFormatterParams) {
        return _this.lookupValue(
          data,
          dropDownIdLabel,
          dropDownValueLabel,
          params.value
        );
      },
      refreshValuesOnOpen: true,
    };
  }

  private static get textFilterParams() {
    return {
      debounceMs: 200,
      defaultOption: 'contains',
      newRowsAction: 'keep',
      textMatcher: ({
        value,
        filterText,
        node,
      }: {
        value: any;
        filterText: string;
        node: any;
      }) => {
        if (node.data?.IS_NEW) return true;
        if (value == null) return false;
        return value
          .toString()
          .toLowerCase()
          .includes(filterText.toLowerCase());
      },
    };
  }

  private static get textMultiFilterParams(): IMultiFilterParams {
    return {
      filters: [
        {
          filter: 'agTextColumnFilter',
          filterParams: this.textFilterParams,
        },
        {
          filter: 'agSetColumnFilter',
        },
      ],
    };
  }

  private static get autocompleteFilterParams() {
    return {
      debounceMs: 200,
      defaultOption: 'contains',
      newRowsAction: 'keep',
    };
  }

  private static get autocompleteMultiFilterParams(): IMultiFilterParams {
    return {
      filters: [
        {
          filter: 'agTextColumnFilter',
          filterParams: this.autocompleteFilterParams,
        },
        {
          filter: 'agSetColumnFilter',
        },
      ],
    };
  }

  // COLUMN DEFINITIONS

  static get defaultColDef(): ColDef {
    return {
      context: {
        soeColumnType: SoeColumnType.Text,
      } as SoeColDefContext,
      headerClass: ['soe-ag-header'],
      floatingFilter: true,
      suppressFloatingFilterButton: true,
      suppressHeaderMenuButton: true,
      sortable: true,
      resizable: true,
    };
  }

  static createColumnMasterDetail<T>(): ColDef {
    const options: TextColumnOptions<T> = {};
    const colDef = this.createColumnText('expander', '', options);
    colDef.cellRenderer = 'agGroupCellRenderer';
    colDef.cellRendererParams = {
      suppressDoubleClickExpand: true,
    } as IGroupCellRendererParams;
    colDef.width = 40;
    colDef.maxWidth = 40;
    colDef.filter = false;
    colDef.suppressSizeToFit = true;
    colDef.context.suppressExport = true;
    colDef.showRowGroup = true;

    return colDef;
  }

  // TODO: Should be replaced by enableRowSelection()
  // Still used in Master/Detailed grid
  static createColumnRowSelection<T>(
    showCheckbox?: (row: IRowNode) => boolean,
    columnSeparator?: boolean
  ): ColDef {
    // https://www.ag-grid.com/angular-data-grid/row-selection/

    // Set default values
    const options = {
      editable: false,
      checkboxSelection: showCheckbox ? showCheckbox : true,
      pinned: 'left',
      lockPosition: true,
      width: 27,
      maxWidth: 27,
      minWidth: 27,
      suppressHeaderMenuButton: true,
      sortable: false,
      suppressSizeToFit: true,
      suppressMovable: true,
      filter: false,
      resizable: false,
      suppressNavigable: true,
      suppressColumnsToolPanel: true,
      suppressExport: true,
      suppressFilter: true,
      enableHiding: false,
      columnSeparator: columnSeparator,
    } as ColumnOptions<T>;

    const colDef = this.createColDef(
      SoeColumnType.RowSelection,
      'soe-row-selection',
      '',
      options
    );

    if (typeof colDef.cellClass === 'undefined') colDef.cellClass = '';
    colDef.cellClass += ' row-selection-column';
    if (typeof colDef.headerClass === 'undefined') colDef.headerClass = '';
    colDef.headerClass += ' row-selection-column';

    return colDef;
  }

  static createColumnHeader<T>(
    field: string,
    headerName: string,
    options?: ColumnHeaderGroupOptions<T>
  ): SoeColGroupDef {
    options = this.setDefaultOptions<T, ColumnHeaderGroupOptions<T>>(
      options || {},
      ColumnHeaderGroupOptions.default()
    );
    const colDef = <SoeColGroupDef>(
      this.createColDef(SoeColumnType.Text, field, headerName, options)
    );

    const { marryChildren, suppressStickyLabel, openByDefault, tooltip } =
      options;

    if (suppressStickyLabel) colDef.suppressStickyLabel = suppressStickyLabel;
    if (marryChildren) colDef.marryChildren = marryChildren;
    if (openByDefault) colDef.openByDefault = openByDefault;
    if (tooltip) colDef.headerTooltip = tooltip;

    return colDef;
  }

  static createColumnBool<T>(
    field: string | null,
    headerName: string,
    options?: CheckboxColumnOptions<T>
  ): ColDef {
    options = this.setDefaultOptions<T, CheckboxColumnOptions<T>>(
      options || {},
      CheckboxColumnOptions.default()
    );

    const colDef = this.createColDef(
      SoeColumnType.Bool,
      field || '',
      headerName,
      options
    );

    const { suppressFilter } = options;

    // Format

    // We are not using the build in CheckboxEditor/Renderer
    // Problems with double click to toggle, styling etc...
    // colDef.cellEditor = 'agCheckboxCellEditor';
    // colDef.cellRenderer = 'agCheckboxCellRenderer';
    // colDef.cellRendererParams = {
    //   disabled: !options.editable,
    // };

    colDef.cellEditor = CheckboxCellEditor<AG_NODE<T>>;
    colDef.cellEditorParams = this.getCheckboxCellEditorParams(options);
    colDef.cellRenderer = CheckboxCellRenderer;
    colDef.cellRendererParams = this.checkboxCellRendererParams(options);

    colDef.valueGetter = (params: ValueGetterParams) => {
      if (params.data) {
        if (field) {
          return field === 'state'
            ? params.data[field] === SoeEntityState.Active
            : params.data[field];
        }
      }

      return false;
    };

    // Editing
    if (!options.editable) colDef.editable = false;

    colDef.valueSetter = (params: ValueSetterParams) => {
      if (params.data && field) {
        if (field == 'state') {
          params.data[field] = params.newValue
            ? SoeEntityState.Active
            : SoeEntityState.Inactive;
          params.data.isActive = params.newValue;
        } else {
          params.data[field] = params.newValue;
        }
      }

      return true;
    };

    // Filter
    if (!suppressFilter) {
      colDef.filter = true;
      colDef.icons = { filter: ' ' }; // Hide funnel icon in header
      colDef.suppressFloatingFilterButton = true;
      colDef.floatingFilterComponent = CheckboxFloatingFilter;
      colDef.floatingFilterComponentParams =
        this.getCheckboxFloatingFilterParams(
          options
        ) as CheckboxFloatingFilterParams;
    }

    this.applyOptions(colDef, options);

    return colDef;
  }

  static createColumnActive<T>(
    selectedItemsService: SelectedItemService<boolean, T>,
    field = 'state',
    headerName: string,
    options: CheckboxColumnOptions<T> = {}
  ): ColDef {
    options = this.setDefaultOptions<T, CheckboxColumnOptions<T>>(
      options || {},
      {
        ...CheckboxColumnOptions.default(),
        maxWidth: 80,
        editable: false,
        setChecked: true,
        pinned: 'left',
      }
    );

    options.onClick = (val: boolean, item: T) => {
      selectedItemsService.toggle(item, options.idField, val);
    };

    const colDef = this.createColumnBool(field, headerName, options);
    colDef.context.soeColumnType = SoeColumnType.Active;

    colDef.suppressSizeToFit = options.suppressSizeToFit
      ? options.suppressSizeToFit
      : true;

    return colDef;
  }

  static createColumnDateTime<T>(
    field: string,
    headerName: string,
    options?: DateTimeColumnOptions<T>
  ): ColDef {
    options = this.setDefaultOptions<T, DateTimeColumnOptions<T>>(
      options || {},
      DateTimeColumnOptions.default()
    );
    const colDef = this.createColDef(
      SoeColumnType.DateTime,
      field,
      headerName,
      options
    );

    const {
      editable,
      suppressFilter,
      showSetFilter,
      cellClassRules,
      pivoting,
      pivotHierarchy,
    } = options;

    // Cell class rules
    colDef.cellClassRules = cellClassRules;

    // Format
    colDef.valueFormatter = (params: ValueFormatterParams) =>
      this.dateTimeFormatter(params, options!);

    // Sort
    colDef.comparator = this.dateTimeComparator;

    // Editing
    colDef.cellEditor = DateCellEditor<AG_NODE<T>>;
    colDef.cellEditorParams = {
      disabled: editable ?? false,
    } as DateCellEditorParams<T>;

    // Filter
    if (!suppressFilter) {
      if (showSetFilter) {
        colDef.filter = 'agMultiColumnFilter';
        colDef.filterParams = this.dateMultiFilterParams;
      } else {
        colDef.filter = 'agDateColumnFilter';
        colDef.filterParams = this.dateFilterParams;
      }
    }

    // Pivoting
    if (pivoting) {
      colDef.pivot = true;
      if (pivotHierarchy && pivotHierarchy.length > 0) {
        colDef.groupHierarchy = pivotHierarchy;
      }
    }

    this.applyOptions(colDef, options);

    return colDef;
  }

  static createColumnDate<T>(
    field: string,
    headerName: string,
    options?: DateColumnOptions<T>
  ): ColDef {
    options = this.setDefaultOptions<T, DateColumnOptions<T>>(
      options || {},
      DateColumnOptions.default()
    );
    const colDef = this.createColDef(
      SoeColumnType.Date,
      field,
      headerName,
      options
    );

    const {
      editable,
      suppressFilter,
      showSetFilter,
      cellClassRules,
      pivoting,
      pivotHierarchy,
    } = options;

    // Cell class rules
    colDef.cellClassRules = cellClassRules;

    // Format
    colDef.valueFormatter = (params: ValueFormatterParams) =>
      this.dateFormatter(params, options!);

    // Sort
    colDef.comparator = this.dateComparator;

    // Editing
    colDef.cellEditor = DateCellEditor<AG_NODE<T>>;
    colDef.cellEditorParams = {
      disabled: typeof editable === 'undefined' ? false : !editable,
    } as DateCellEditorParams<T>;

    // Filter
    if (!suppressFilter) {
      if (showSetFilter) {
        colDef.filter = 'agMultiColumnFilter';
        colDef.filterParams = this.dateMultiFilterParams;
      } else {
        colDef.filter = 'agDateColumnFilter';
        colDef.filterParams = this.dateFilterParams;
      }
    }

    // Pivoting
    if (pivoting) {
      colDef.pivot = true;
      if (pivotHierarchy && pivotHierarchy.length > 0) {
        colDef.groupHierarchy = pivotHierarchy;
      }
    }

    this.applyOptions(colDef, options);

    return colDef;
  }

  static createColumnTime<T>(
    field: string,
    headerName: string,
    options?: TimeColumnOptions<T>
  ): ColDef {
    options = this.setDefaultOptions<T, TimeColumnOptions<T>>(
      options || {},
      TimeColumnOptions.default()
    );
    const colDef = this.createColDef(
      SoeColumnType.Time,
      field,
      headerName,
      options
    );

    const {
      editable,
      suppressFilter,
      showSetFilter,
      aggFuncOnGrouping,
      pivoting,
      pivotHierarchy,
    } = options;

    // Format
    colDef.valueFormatter = (params: ValueFormatterParams) =>
      this.timeFormatter(params, options!);

    // Sort
    colDef.comparator = this.timeComparator;

    // Editing
    colDef.cellEditor = TimeCellEditor<AG_NODE<T>>;
    colDef.cellEditorParams = {
      disabled: typeof editable === 'undefined' ? false : !editable,
    } as TimeCellEditorParams<T>;

    if (editable) {
      colDef.suppressKeyboardEvent = (
        params: SuppressKeyboardEventParams<any, any>
      ) => {
        // For some reason, moving to next cell with Enter key will not patch the new value
        // Supress Enter key to prevent this, user needs to press Tab key instead
        return params.event.key === 'Enter';
      };
    }

    // Filter
    if (!suppressFilter) {
      if (showSetFilter) {
        colDef.filter = 'agMultiColumnFilter';
        colDef.filterParams = this.dateMultiFilterParams;
      } else {
        colDef.filter = 'agDateColumnFilter';
        colDef.filterParams = this.dateFilterParams;
      }
    }

    // Grouping
    if (aggFuncOnGrouping) colDef.aggFunc = aggFuncOnGrouping;

    // Pivoting
    if (pivoting) {
      colDef.pivot = true;
      if (pivotHierarchy && pivotHierarchy.length > 0) {
        colDef.groupHierarchy = pivotHierarchy;
      }
    }

    this.applyOptions(colDef, options);

    return colDef;
  }

  static createColumnTimeSpan<T>(
    field: string,
    headerName: string,
    options?: TimeSpanColumnOptions<T>
  ): ColDef {
    options = this.setDefaultOptions<T, TimeSpanColumnOptions<T>>(
      options || {},
      TimeSpanColumnOptions.default()
    );
    const colDef = this.createColDef(
      SoeColumnType.TimeSpan,
      field,
      headerName,
      options
    );

    const { editable, aggFuncOnGrouping } = options;

    // Format
    colDef.valueFormatter = (params: ValueFormatterParams) =>
      this.timeSpanFormatter(params, options!);

    // Grouping
    if (aggFuncOnGrouping) colDef.aggFunc = aggFuncOnGrouping;

    // Editing
    colDef.cellEditor = TimeSpanCellEditor<AG_NODE<T>>;
    colDef.cellEditorParams = {
      disabled: typeof editable === 'undefined' ? false : !editable,
      disableDurationFormatting: options?.disableTimeFormatting,
    } as TimeSpanCellEditorParams<T>;

    if (editable) {
      colDef.suppressKeyboardEvent = (
        params: SuppressKeyboardEventParams<any, any>
      ) => {
        // For some reason, moving to next cell with Enter key will not patch the new value
        // Supress Enter key to prevent this, user needs to press Tab key instead
        return params.event.key === 'Enter';
      };
    }

    // TODO: Add filter

    this.applyOptions(colDef, options);

    return colDef;
  }

  static createColumnIcon<T>(
    field: string | null,
    headerName: string,
    options?: IconColumnOptions<T>
  ): ColDef {
    options = this.setDefaultOptions<T, IconColumnOptions<T>>(
      options || {},
      IconColumnOptions.default()
    );

    const colDef = this.createColDef(
      SoeColumnType.Icon,
      field || 'icon',
      headerName,
      options
    );

    if (field && !options.iconName) options.useIconFromField = true;

    // Format
    colDef.cellRenderer = options.cellRenderer ?? IconCellRenderer;
    colDef.cellRendererParams = this.iconCellRendererParams(options);
    colDef.valueFormatter = (params: ValueFormatterParams) => {
      // Prevent the following error in console:
      // grid.component.ts:879 AG Grid: error #48 Cell data type is "object" but no Value Formatter has been provided.
      // Please either provide an object data type definition with a Value Formatter, or set "colDef.valueFormatter"
      // See https://www.ag-grid.com/angular-data-grid/errors/48?_version_=33.0.4&property=Formatter
      return '';
    };

    // Filter
    if (!options.suppressFilter) {
      colDef.filter = 'agSetColumnFilter';
      colDef.filterParams = {
        cellRenderer: IconCellRenderer,
        cellRendererParams: {
          useIconFromField: options.useIconFromField,
          icon: IconUtil.createIcon(options.iconPrefix, options.iconName),
          iconClass: options.iconClass,
          iconClassField: options.iconClassField,
          iconAnimation: options.iconAnimation,
          iconAnimationField: options.iconAnimationField,
          tooltip: options.tooltip,
          showIcon: options.showIcon,
          width: options.width,
          isFilter: true,
          data: {},
        } as IIconCellRendererParams<T>,

        comparator: (
          a: any,
          b: any,
          nodeA: any,
          nodeB: any,
          isInverted: boolean
        ) => {
          if (!a && b) return 1;
          if (a && !b) return -1;
          if (a === b) return 0;

          const textA = a.includes('|') ? a.split('|')[1] : a;
          const textB = b.includes('|') ? b.split('|')[1] : a;

          return (textA > textB ? 1 : -1) * (isInverted ? -1 : 1);
        },
      };
      colDef.filterValueGetter = params => {
        let icon = [];

        if (
          params.colDef.cellRendererParams.useIconFromField &&
          params.colDef.field
        ) {
          const fieldData = params.data[params.colDef.field];
          if (fieldData) {
            if (
              params.colDef.cellRendererParams.icon &&
              Array.isArray(params.colDef.cellRendererParams.icon)
            ) {
              icon = [params.colDef.cellRendererParams.icon[0], fieldData];
            } else {
              icon = fieldData;
            }
          } else {
            icon = [];
          }
        }
        const cssClass = options.iconClassField
          ? params.data[options.iconClassField!]
          : options.iconClass
            ? options.iconClass
            : '';

        const tooltipText =
          options.tooltipField &&
          params.data[options.tooltipField] &&
          options.showTooltipInFilter
            ? params.data[options.tooltipField]
            : '';
        return icon.length == 2
          ? icon[0] + '|' + icon[1] + '|' + tooltipText + '|' + cssClass
          : '';
      };
    }

    if (options.enableResize) {
      if (options.width) colDef.width = options.width;
      options.maxWidth = undefined; // also set to prevent being overwritten in applyOptions
      colDef.maxWidth = undefined;
      colDef.context.suppressAutoSizeWithHeader = false;
      colDef.resizable = true;
    } else {
      colDef.resizable = false;
    }
    colDef.suppressSizeToFit = true;
    colDef.suppressAutoSize = true;
    colDef.suppressNavigable = true; // Will prevent from tabbing into the cell
    colDef.cellStyle = { padding: '0', 'text-overflow': 'clip' };

    this.applyOptions(colDef, options);

    return colDef;
  }

  private static createColumnIconStandard<T>(
    field: string | null,
    iconName: IconName,
    iconClass: string,
    options?: IconColumnOptions<T>
  ) {
    options = this.setDefaultOptions<T, IconColumnOptions<T>>(
      options || {},
      IconColumnOptions.default()
    );
    options.iconName = iconName;
    options.iconClass = iconClass;
    if (typeof options.pinned === 'undefined') options.pinned = 'right';
    options.enableHiding = false;
    // options.suppressExport = true;

    const colDef = this.createColumnIcon(field, '', options);
    colDef.sortable = false;

    return colDef;
  }

  static createColumnIconEdit<T>(options?: IconColumnOptions<T>): ColDef {
    options = this.setDefaultOptions<T, IconColumnOptions<T>>(
      options || {},
      IconColumnOptions.default()
    );

    const colDef = this.createColumnIconStandard(
      null,
      'pen',
      'icon-edit',
      options
    );

    return colDef;
  }

  static createColumnIconDelete<T>(options?: IconColumnOptions<T>): ColDef {
    options = this.setDefaultOptions<T, IconColumnOptions<T>>(
      options || {},
      IconColumnOptions.default()
    );

    options.width = options.minWidth = options.maxWidth = 25;

    const colDef = this.createColumnIconStandard(
      null,
      'times',
      'icon-delete',
      options
    );

    return colDef;
  }

  static createColumnModified<T>(
    field: string,
    options?: IconColumnOptions<T>
  ): ColDef {
    options = this.setDefaultOptions<T, IconColumnOptions<T>>(
      options || {},
      IconColumnOptions.default()
    );

    options.pinned = 'left';
    options.width = options.minWidth = options.maxWidth = 25;
    options.onClick = () => {};

    const colDef = this.createColumnIconStandard(
      field,
      'asterisk',
      '',
      options
    );

    colDef.cellClassRules = {
      'color-transparent': (params: CellClassParams) => !params.value,
      'icon-primary': (params: CellClassParams) => params.value,
    };

    return colDef;
  }

  static createColumnNumber<T>(
    field: string,
    headerName: string,
    options?: NumberColumnOptions<T>
  ) {
    options = this.setDefaultOptions<T, NumberColumnOptions<T>>(
      options || {},
      NumberColumnOptions.default()
    );

    //Overwrite right align if left align or center align is set
    options.alignRight = !(options.alignLeft || options.alignCenter);

    const colDef = this.createColDef(
      SoeColumnType.Number,
      field,
      headerName,
      options
    );

    const {
      editable,
      suppressFilter,
      showSetFilter,
      cellClassRules,
      aggFuncOnGrouping,
      buttonConfiguration,
    } = options;

    // Cell class rules
    colDef.cellClassRules = cellClassRules;

    // Button configuration
    if (buttonConfiguration) {
      // Button with icon
      colDef.cellRenderer = TextButtonCellRenderer;
      colDef.cellRendererParams = this.textButtonCellRendererParams(options);
    }

    // Format
    // numberFormatter is using the current user language, thus we need to provide numberbox with custom methods to parse and clean input values.
    colDef.valueFormatter = (params: ValueFormatterParams) =>
      this.numberFormatter(params, options!);
    colDef.context.customNumberInputParser = (str: string) =>
      NumberUtil.parseNumberByCurrentUserLanguage(str);
    colDef.context.customPrepareCalculationExpression = (str: string) =>
      NumberUtil.prepareCalculationExpression(str);

    // Editing
    colDef.cellEditor = NumberCellEditor<AG_NODE<T>>;
    colDef.cellEditorParams = {
      decimals: options.decimals,
    } as NumberCellEditorParams<T>;

    if (editable) {
      colDef.suppressKeyboardEvent = (
        params: SuppressKeyboardEventParams<any, any>
      ) => {
        // For some reason, moving to next cell with Enter key will not patch the new value
        // Supress Enter key to prevent this, user needs to press Tab key instead
        return params.event.key === 'Enter';
      };
    }

    // Filter
    if (!suppressFilter) {
      if (showSetFilter) {
        colDef.filter = 'agMultiColumnFilter';
        colDef.filterParams = this.numberMultiFilterParams;
      } else {
        colDef.filter = 'agNumberColumnFilter';
        colDef.filterParams = this.numberFilterParams;
      }
    }

    // Grouping
    if (aggFuncOnGrouping) colDef.aggFunc = aggFuncOnGrouping;

    this.applyOptions(colDef, options);

    return colDef;
  }

  static createColumnSelect<T, U>(
    field: string,
    headerName: string,
    selectOptions: U[],
    onChangeEvent?: any,
    options?: SelectColumnOptions<T, U>
  ) {
    const data = signal<U[]>([]);
    options = this.setDefaultOptions<T, SelectColumnOptions<T, U>>(
      options || {},
      SelectColumnOptions.default()
    );
    const colDef = this.createColDef(
      SoeColumnType.Select,
      field,
      headerName,
      options
    );

    const {
      dropDownIdLabel,
      dropDownValueLabel,
      suppressFilter,
      cellClassRules,
      shapeConfiguration,
      sortByDisplayValue,
    } = options;

    // Cell class rules
    colDef.cellClassRules = cellClassRules;

    // Data
    const updateSelectOptionsList = (selectOptions: U[]) => {
      const _data = selectOptions.map(x => ({ ...x }));
      data.set(_data);
      return _data;
    };

    const selectedOptions: U[] = [];

    updateSelectOptionsList(selectOptions);

    colDef.valueFormatter = (params: ValueFormatterParams) => {
      return this.lookupValue(
        changeSelectData(params),
        dropDownIdLabel!,
        dropDownValueLabel!,
        parseInt(params.value)
      ) as string;
    };
    colDef.valueGetter = (params: ValueGetterParams) => {
      return params.data?.[field];
    };
    colDef.valueSetter = (params: ValueSetterParams<any, U>) => {
      params.data[field] = params.newValue?.[dropDownIdLabel!];
      return true;
    };

    if (sortByDisplayValue) {
      colDef.comparator = (valueA: number, valueB: number) => {
        const nameA =
          (this.lookupValue(
            data(),
            dropDownIdLabel!,
            dropDownValueLabel!,
            valueA
          ) as string) || '';
        const nameB =
          (this.lookupValue(
            data(),
            dropDownIdLabel!,
            dropDownValueLabel!,
            valueB
          ) as string) || '';
        return nameA.toLowerCase().localeCompare(nameB.toLowerCase());
      };
    }

    // Format
    if (shapeConfiguration) {
      // Shape
      colDef.cellRenderer = ShapeCellRenderer;
      colDef.cellRendererParams = {
        shape: shapeConfiguration.shape,
        color: shapeConfiguration.color,
        colorField: shapeConfiguration.colorField,
        showShapeField: shapeConfiguration.showShapeField,
        useGradient: shapeConfiguration.useGradient,
        gradientField: shapeConfiguration.gradientField,
        width: shapeConfiguration.width,
        isSelect: true,
      } as IShapeCellRendererParams<T>;
    }

    if (onChangeEvent && typeof onChangeEvent === 'function') {
      colDef.onCellValueChanged = ($event: NewValueParams) => {
        onChangeEvent($event);
      };
    }
    // We store the options so that we can react to changes
    // from the grid component
    colDef.context.options = options;

    const changeSelectData = ($ev: any) => {
      // Options per row in the grid
      const options = colDef.context.options?.dynamicSelectOptions?.($ev) as [];

      if (options) {
        selectOptions = [];
        const rows = $ev?.node?.parent?.allLeafChildren as [];
        rows.forEach((row: any) => {
          const selectedOption = options.find(
            x => x[dropDownIdLabel!] === row.data[field]
          );
          if (selectedOption) selectedOptions.push(selectedOption);
        });

        options?.forEach((option: any) => {
          const otherOption = selectOptions.some(
            y => y[dropDownIdLabel!] === option[dropDownIdLabel!]
          );
          if (!otherOption) selectOptions.push(option);
        });
      } else {
        selectedOptions.splice(0);
        selectedOptions.push(...selectOptions);
      }

      const _data = updateSelectOptionsList(selectOptions);
      return _data;
    };

    // Editing
    colDef.cellEditor = 'agRichSelectCellEditor';
    colDef.cellEditorParams = {
      values: colDef.context.options.dynamicSelectOptions
        ? changeSelectData
        : data(),
      useFormatter: true,
      formatValue: (itemOrId: U | number) =>
        (itemOrId || itemOrId === 0) && typeof itemOrId === 'number'
          ? this.lookupValue(
              data(),
              dropDownIdLabel!,
              dropDownValueLabel!,
              itemOrId
            )
          : itemOrId
            ? itemOrId[dropDownValueLabel!]
            : '',
    };

    // Filter
    if (!suppressFilter) {
      colDef.filter = 'agSetColumnFilter';
      colDef.filterParams = this.getSelectFilterParams(
        this,
        selectedOptions,
        dropDownIdLabel!,
        dropDownValueLabel!
      );
    }

    this.applyOptions(colDef, options);
    return colDef;
  }

  static createColumnShape<T>(
    field: string,
    headerName: string,
    options?: ShapeColumnOptions<T>
  ) {
    options = this.setDefaultOptions<T, ShapeColumnOptions<T>>(
      options || {},
      ShapeColumnOptions.default()
    );
    const colDef = this.createColDef(
      SoeColumnType.Shape,
      field,
      headerName,
      options
    );

    colDef.minWidth = colDef.maxWidth = (colDef.width || 20) + 6;

    if (typeof colDef.cellClass === 'undefined') colDef.cellClass = '';
    colDef.cellClass += ' shape-column';

    // TODO: Get rid of floating filter input, only show funnel icon

    const { suppressFilter } = options;

    // Format
    colDef.cellRenderer = ShapeCellRenderer;
    colDef.cellRendererParams = this.shapeCellRendererParams(options);

    // Filter
    if (!suppressFilter) {
      colDef.filter = 'agSetColumnFilter';
      colDef.filterParams = {
        cellRenderer: ShapeCellRenderer,
        cellRendererParams: {
          shape: options.shape,
          colorField: options.colorField,
          width: options.width,
          useGradient: options.useGradient,
          gradientField: options.gradientField,
          isFilter: true,
          data: {},
        } as ShapeCellRendererParams,

        comparator: (
          a: any,
          b: any,
          nodeA: any,
          nodeB: any,
          isInverted: boolean
        ) => {
          if (!a && b) return 1;
          if (a && !b) return -1;
          if (a === b) return 0;

          const textA = a.includes('|') ? a.split('|')[1] : a;
          const textB = b.includes('|') ? b.split('|')[1] : a;

          return (textA > textB ? 1 : -1) * (isInverted ? -1 : 1);
        },
      };
      colDef.filterValueGetter = params => {
        const part1 = options.colorField
          ? params.data[options.colorField]
          : options.color;
        const part2 =
          options.tooltipField && params.data[options.tooltipField]
            ? params.data[options.tooltipField]
            : '';
        return part1 !== '' ? part1 + '|' + part2 : options.color;
      };
    }

    this.applyOptions(colDef, options);

    return colDef;
  }

  static createColumnText<T>(
    field: string,
    headerName: string,
    options?: TextColumnOptions<T>
  ) {
    options = this.setDefaultOptions<T, TextColumnOptions<T>>(
      options || {},
      TextColumnOptions.default()
    );
    const colDef = this.createColDef(
      SoeColumnType.Text,
      field,
      headerName,
      options
    );

    const {
      suppressFilter,
      showSetFilter,
      filterOptions,
      cellClassRules,
      usePlainText,
      buttonConfiguration,
      tooltip,
      shapeConfiguration,
      cellRenderer,
      cellRendererParams,
      filter,
    } = options;

    // Cell class rules
    colDef.cellClassRules = cellClassRules;

    // Explicit text editor to prevent AG Grid from guessing
    colDef.cellEditor = 'agTextCellEditor';

    // Filter
    if (!suppressFilter) {
      if (filter) {
        colDef.filter = filter;
      } else if (showSetFilter) {
        colDef.filter = 'agMultiColumnFilter';
        colDef.filterParams = this.textMultiFilterParams;
      } else {
        colDef.filter = 'agTextColumnFilter';
        colDef.filterParams = this.textFilterParams;
      }

      if (filterOptions && Array.isArray(filterOptions)) {
        colDef.filterParams.filterOptions = filterOptions;
      }
    }

    // Cell renderer
    if (!usePlainText && buttonConfiguration) {
      // Button with icon
      colDef.cellRenderer = TextButtonCellRenderer;
      colDef.cellRendererParams = this.textButtonCellRendererParams(options);
    } else if (!usePlainText && shapeConfiguration) {
      // Shape
      colDef.cellRenderer = ShapeCellRenderer;
      colDef.cellRendererParams = {
        shape: shapeConfiguration.shape,
        color: shapeConfiguration.color,
        colorField: shapeConfiguration.colorField,
        showShapeField: shapeConfiguration.showShapeField,
        useGradient: shapeConfiguration.useGradient,
        gradientField: shapeConfiguration.gradientField,
        width: shapeConfiguration.width,
        shapeTooltip: shapeConfiguration.tooltip,
        tooltip: tooltip,
        isText: true,
      } as IShapeCellRendererParams<T>;
    }
    if (cellRenderer) {
      colDef.cellRenderer = cellRenderer;
      colDef.cellRendererParams = cellRendererParams;
    }

    // Sorting
    colDef.comparator = (
      valueA: any,
      valueB: any,
      nodeA: any,
      nodeB: any,
      isInverted: any
    ): any => {
      if (!valueA) return 1;
      if (!valueB) return -1;
      if (typeof valueA === 'number' || !isNaN(+valueA)) {
        if (typeof valueB === 'string' && isNaN(+valueB)) {
          return 1;
        }
        if (+valueA === +valueB) {
          return 0;
        }
        if (+valueA > +valueB) {
          return -1;
        }
        if (+valueA < +valueB) {
          return 1;
        }
      } else if (typeof valueA === 'string') {
        if (typeof valueB === 'number' || !isNaN(+valueB)) return -1;
        return valueA.toLowerCase().localeCompare(valueB.toLowerCase());
      } else {
        return 0;
      }
    };

    this.applyOptions(colDef, options);

    return colDef;
  }

  static createColumnAutocomplete<T, U>(
    field: string,
    headerName: string,
    options: AutocompleteColumnOptions<T, U>
  ) {
    options = this.setDefaultOptions<T, AutocompleteColumnOptions<T, U>>(
      options || {},
      <any>AutocompleteColumnOptions.default()
    );

    const colDef = this.createColDef(
      SoeColumnType.Autocomplete,
      field,
      headerName,
      options
    );

    const {
      suppressFilter,
      showSetFilter,
      filterOptions,
      cellClassRules,
      buttonConfiguration,
      cellRenderer,
      cellRendererParams,
    } = options;

    // Cell class rules
    colDef.cellClassRules = cellClassRules;

    // Format
    colDef.valueFormatter = (params: ValueFormatterParams<T, number>) => {
      // Earlier we cached the specific array per row.
      // Now we cache the id to name globally instead.
      const cacheKey = getAutocompleteCacheKey(field);

      if (!params.context[cacheKey]) {
        const value: any = {};
        for (const option of options.source(params?.data) || []) {
          value[option[options.optionIdField!]] =
            option[options.optionNameField!];
        }
        params.context[cacheKey] = value;
      }

      const cache = params.context[cacheKey];
      const row = params.data ?? ({} as any);
      const lookupId = row[field];

      if (cache[lookupId]) {
        return cache[lookupId];
      }

      const item = options
        .source(params.data)
        ?.find(x => x[options.optionIdField!] === params.value);

      const textValue =
        item?.[options.optionNameField!] ??
        params.data?.[options.optionDisplayNameField!] ??
        '';

      cache[lookupId] = textValue;

      return textValue;
    };

    // We store the options so that we can react to changes
    // from the grid component
    colDef.context.options = options;

    // Editing
    colDef.cellEditor = AutocompleteCellEditor<AG_NODE<T>, U>;
    colDef.cellEditorParams = this.getAutocompleteCellEditorParams(options);

    // Cell renderer

    // Default cell renderer to show text value
    colDef.cellRenderer = AutocompleteCellRenderer;
    colDef.cellRendererParams =
      this.autocompleteCellRendererDefaultParams(options);

    if (buttonConfiguration) {
      colDef.cellRenderer = AutocompleteCellRenderer;
      colDef.cellRendererParams = this.autocompleteCellRendererParams(options);
    }

    if (cellRenderer) {
      colDef.cellRenderer = cellRenderer;
      colDef.cellRendererParams = cellRendererParams;
    }

    // Filter

    if (!suppressFilter) {
      if (showSetFilter) {
        colDef.filter = 'agMultiColumnFilter';
        colDef.filterParams = this.autocompleteMultiFilterParams;
      } else {
        colDef.filter = 'agTextColumnFilter';
        colDef.filterParams = this.autocompleteFilterParams;
        colDef.filterValueGetter = (params: ValueGetterParams) => {
          const { data } = params;
          return data[options.optionDisplayNameField];
        };
      }

      if (filterOptions && Array.isArray(filterOptions)) {
        colDef.filterParams.filterOptions = filterOptions;
      }
    }

    this.applyOptions(colDef, options);

    return colDef;
  }

  // HELP METHODS

  private static setDefaultOptions<U, T extends ColumnOptions<U>>(
    options: T,
    defaultOptions: T
  ): T {
    if (options && Object.keys(options).length !== 0) {
      Object.assign(defaultOptions, defaultOptions, options);
    }

    return defaultOptions;
  }

  private static createColDef<T>(
    soeColumnType: SoeColumnType,
    field: string,
    headerName: string,
    options?: ColumnOptions<T>
  ): ColDef {
    options = this.setDefaultOptions<T, ColumnOptions<T>>(
      options || {},
      ColumnOptions.default()
    );
    const colDef: ColDef = {
      context: { soeColumnType: soeColumnType } as SoeColDefContext,
      field,
      headerName,
    };

    const {
      alignRight,
      alignCenter,
      pinned,
      colSpan,
      width,
      minWidth,
      maxWidth,
      flex,
      editable,
      suppressFilter,
      suppressFloatingFilter,
      suppressSizeToFit,
      tooltip,
      tooltipField,
      iconHeaderParams,
      suppressExport,
      enableHiding,
      hide,
      sortable,
      sort,
      sortIndex,
      enableGrouping,
      grouped,
      checkboxSelection,
      cellClassRules,
      resizable,
      rowDragable,
      columnSeparator,
      headerSeparator,
      valueGetter,
      valueSetter,
    } = options;

    // Align
    if (alignRight) colDef.cellClass = 'grid-text-right';
    else if (alignCenter) colDef.cellClass = 'grid-text-center';
    else colDef.cellClass = 'text-left';

    if (pinned) {
      colDef.pinned = pinned;
      colDef.lockPinned = true;
      if (suppressSizeToFit === undefined) colDef.suppressSizeToFit = true;
    }

    if (colSpan) colDef.colSpan = colSpan;

    // Size
    if (width) colDef.width = width;
    if (minWidth) colDef.minWidth = minWidth;
    if (maxWidth) colDef.maxWidth = maxWidth;
    if (suppressSizeToFit) colDef.suppressSizeToFit = suppressSizeToFit;
    if (flex) colDef.flex = flex;
    colDef.resizable = resizable;

    // Editing
    colDef.editable = typeof editable === 'undefined' ? false : editable;

    if (typeof editable === 'function') {
      colDef.onCellValueChanged = (event: NewValueParams<any>) => {
        colDef.editable = editable({ ...event } as EditableCallbackParams<T>);
      };
    }

    // Filter
    if (suppressFilter) {
      colDef.filter = false;
    } else if (!suppressFloatingFilter) {
      colDef.suppressFloatingFilterButton = false;
    }
    colDef.floatingFilter = !suppressFilter && !suppressFloatingFilter;

    // Tooltip
    if (tooltipField) colDef.tooltipField = tooltipField;
    colDef.headerTooltip = tooltip || headerName;

    // Header component
    if (iconHeaderParams) {
      colDef.headerComponent = IconInnerHeaderComponent;
      colDef.headerComponentParams = iconHeaderParams;
    }

    // Export
    if (suppressExport) colDef.context.suppressExport = suppressExport;

    // Visibility
    colDef.menuTabs = enableHiding
      ? ['generalMenuTab', 'filterMenuTab', 'columnsMenuTab']
      : ['generalMenuTab', 'filterMenuTab'];
    colDef.suppressColumnsToolPanel = !enableHiding;
    if (hide) colDef.hide = hide;

    // Row dragable
    if (rowDragable) colDef.rowDrag = rowDragable;

    // Sorting
    colDef.sortable = sortable;
    if (sort) colDef.sort = sort;
    if (sortIndex) colDef.sortIndex = sortIndex;

    // Grouping
    if (enableGrouping) {
      colDef.enableRowGroup = enableGrouping;
    }
    if (grouped) colDef.rowGroup = grouped;

    if (cellClassRules) colDef.cellClassRules = cellClassRules;

    // Checkbox selection
    // TODO: Remove or replace with rowSelection API (still used in Master/Detailed grid, called from createColumnRowSelection())
    if (checkboxSelection) colDef.checkboxSelection = checkboxSelection;

    // Column separator
    if (headerSeparator) {
      colDef.headerClass += ' column-separator';
    }
    if (columnSeparator) {
      colDef.headerClass += ' column-separator';
      colDef.cellClass += ' column-separator';
    }

    if (valueGetter) colDef.valueGetter = valueGetter;
    if (valueSetter) colDef.valueSetter = valueSetter;

    return colDef;
  }

  static isEvaluatedTrue<T>(obj: any, field: FieldOrPredicate<T>): boolean {
    if (field) {
      return typeof field === 'function' ? field(obj) : (obj[field] as boolean);
    }

    return false;
  }

  private static applyOptions(
    colDef: ColDef,
    options: ColumnOptions<any>
  ): void {
    if (!options) {
      return;
    }

    if (options.width) colDef.width = options.width;
    if (options.maxWidth) colDef.maxWidth = options.maxWidth;
    if (options.minWidth) colDef.minWidth = options.minWidth;
    if (options.flex) colDef.flex = options.flex;
    if (options.suppressAutoSizeWithHeader)
      colDef.context.suppressAutoSizeWithHeader =
        options.suppressAutoSizeWithHeader;

    if (options.strikeThrough) {
      if (!colDef.cellClassRules) colDef.cellClassRules = {};

      if (typeof options.strikeThrough === 'function') {
        colDef.cellClassRules['strike-through'] = options.strikeThrough;
      } else if (typeof options.strikeThrough === 'boolean') {
        colDef.cellClassRules['strike-through'] = () =>
          options.strikeThrough as boolean;
      }
    }
  }

  private static isSoeColDef(colDef: ColDef): boolean {
    return colDef.context?.soeColumnType !== undefined;
  }

  static flagCheckboxColumnsForClear(
    columnDefs: (ColDef<any, any> | ColGroupDef<any>)[] | null | undefined,
    gridOptions: GridOptions<any> | undefined
  ) {
    if (!columnDefs || !gridOptions) {
      return;
    }
    let numCheckboxColumns = 0;
    for (const colDef of columnDefs) {
      if (
        this.isSoeColDef(colDef) &&
        (colDef.context.soeColumnType === SoeColumnType.Active ||
          colDef.context.soeColumnType === SoeColumnType.Bool)
      ) {
        numCheckboxColumns++;
      }
    }
    if (numCheckboxColumns > 0) {
      gridOptions.context.checkboxClearFlags = numCheckboxColumns;
    }
  }

  static checkAndRemoveClearFlag(context: any) {
    if (!context.checkboxClearFlags) {
      return false;
    }
    context.checkboxClearFlags--;
    return true;
  }
}
