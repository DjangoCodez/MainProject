import {
  Directive,
  inject,
  input,
  Input,
  model,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { FormArray } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { SoeFormGroup } from '@shared/extensions';
import { TermCollection } from '@shared/localization/term-types';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  FlowHandlerOptions,
  FlowHandlerService,
} from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Guid } from '@shared/util/string-util';
import { Perform } from '@shared/util/perform.class';
import { Dict } from '@ui/grid/services/selected-item.service';
import { GridComponent } from '@ui/grid/grid.component';
import {
  OpenEditInNewTab,
  TabWrapperGridDataLoaded,
  TabWrapperRowEdited,
  TabWrapperRowEditedAdditionalProps,
} from '@ui/tab/models/multi-tab-wrapper.model';
import {
  ToolbarEmbeddedGridConfig,
  ToolbarGridOptions,
} from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ToolbarUtils } from '@ui/toolbar/utils/toolbar.utils';
import {
  IDefaultFilterSettings,
  ISoeCountInfoOptions,
} from '@ui/grid/interfaces';
import {
  CellEditingStartedEvent,
  CellEditingStoppedEvent,
  ColDef,
} from 'ag-grid-community';
import { BehaviorSubject, Observable, of, tap } from 'rxjs';

export class EmbeddedGridOptions<T, F extends SoeFormGroup> {
  rowType?: T;
  rowFormType?: SoeFormGroup<F>;
  formRows?: FormArray;
  newRowStartEditField?: string;
  showValidationErrors?: boolean = false;

  constructor(init?: Partial<EmbeddedGridOptions<T, F>>) {
    Object.assign(this, init);
  }
}

export interface IApiServiceGrid<T> {
  getGrid: (id?: number, additionalProps?: any) => Observable<T[]>;
}

@Directive({
  selector: '[soeEmbeddedGridBase]',
  standalone: true,
})
export class EmbeddedGridBaseDirective<
  T,
  FormType extends SoeFormGroup = SoeFormGroup,
  RowFormType extends SoeFormGroup = SoeFormGroup,
> implements OnInit
{
  guid = input(Guid.newGuid());
  @Input() form: FormType | undefined;
  @Input() rowData = new BehaviorSubject<T[]>([]);
  exportFilenameKey = model('');
  additionalGridProps = input<any>({});
  // rowEdited and gridIndex is used in multi-tab-wrapper
  // to retrieve the edited row without the output emitter.
  @Input() rowEdited = signal<TabWrapperRowEdited<T> | null>(null);
  @Input() gridDataLoaded = signal<TabWrapperGridDataLoaded<T> | null>(null);
  gridIndex = input(-1);

  onGridDefined = output<GridComponent<T>>();
  onGridReady = output<GridComponent<T>>();
  gridIsDefined = false;

  // For sub grids.
  // Will be passed on to the edit component that triggers a signal to the tab wrapper
  openEditInNewTab = output<OpenEditInNewTab>();

  selectedRows = signal<T[]>([]);
  translate = inject(TranslateService);
  flowHandler = inject(FlowHandlerService);

  grid!: GridComponent<T>;
  gridName!: string;
  height = input(0); // override in component
  toolbarNoMargin = input(false); // override in component
  toolbarNoBorder = input(false); // override in component
  toolbarNoPadding = input(false); // override in component
  toolbarNoTopBottomPadding = input(false); // override in component
  noMargin = input(false); // override in component

  toolbarService = inject(ToolbarService);
  toolbarUtils = new ToolbarUtils();

  progressService = inject(ProgressService);
  performLoadData = new Perform<any>(this.progressService);

  embeddedGridOptions!: EmbeddedGridOptions<T, RowFormType>;

  terms: TermCollection = {};

  ngOnInit(): void {
    // Must be called first in ngOnInit of sub class
    this.embeddedGridOptions = new EmbeddedGridOptions<T, RowFormType>();
  }

  startFlow(
    permission: Feature,
    gridName: string,
    options?: Omit<
      FlowHandlerOptions,
      | 'parentGuid'
      | 'terms'
      | 'companySettings'
      | 'userSettings'
      | 'onPermissionsLoaded'
      | 'onSettingsLoaded'
      | 'setupDefaultToolbar'
      | 'onGridReadyToDefine'
      | 'onFinished'
    > // Omit since they are implemented in sub class and not passed as options
  ) {
    this.gridName = gridName;

    this.flowHandler.options = {
      parentGuid: this.guid(),
      permission: permission,
      additionalReadPermissions: options?.additionalReadPermissions,
      additionalModifyPermissions: options?.additionalModifyPermissions,
      terms: this.loadTerms(),
      companySettings: this.loadCompanySettings(),
      userSettings: this.loadUserSettings(),
      lookups: options?.lookups,
      skipDefaultToolbar: options?.skipDefaultToolbar,
      onPermissionsLoaded: this.onPermissionsLoaded.bind(this),
      onSettingsLoaded: this.onSettingsLoaded.bind(this),
      onGridReadyToDefine: this.onGridReadyToDefine.bind(this),
      onFinished: this.onFinished.bind(this),
    };

    // Create default toolbar if not skipped in grid component
    if (!this.flowHandler.options.skipDefaultToolbar) {
      this.flowHandler.options.setupDefaultToolbar =
        this.createGridToolbar.bind(this);
    }

    this.flowHandler.executeForGrid();
  }

  onPermissionsLoaded(): void {
    // Override in grid component
  }

  loadTerms(translationsKeys: string[] = []): Observable<TermCollection> {
    // Override compleately in grid component or call super.loadTerms and pass translationsKeys

    if (translationsKeys.length > 0) {
      return this.translate.get(translationsKeys).pipe(
        tap(terms => {
          this.terms = terms;
        })
      );
    }

    return of({});
  }

  loadCompanySettings(): Observable<void> {
    // Override in grid component
    return of(undefined);
  }

  loadUserSettings(): Observable<void> {
    // Override in grid component
    return of(undefined);
  }

  onSettingsLoaded(): void {
    // Override in grid component
  }

  onGridReadyToDefine(grid: GridComponent<T>) {
    // Override in grid component
    // Must call super!
    this.setupGrid(grid, this.gridName, false);
    this.setupEditListeners();
  }

  onFinished(): void {
    // Override in grid component
  }

  onCellEditingStarted(event: CellEditingStartedEvent) {
    // Override in grid component
  }

  onCellEditingStopped(event: CellEditingStoppedEvent) {
    // Override in grid component
    const valuesChanged = this.onCellEditingStoppedCheckIfHasChanged(event);
    if (valuesChanged) this.form?.markAsDirty();
    else return;

    if (!event.colDef?.field) return;
    if (
      typeof this.embeddedGridOptions.rowFormType === undefined ||
      typeof this.embeddedGridOptions.rowType === undefined ||
      this.embeddedGridOptions.formRows === undefined
    )
      return;

    const type = this.embeddedGridOptions.rowType;
    const field = event.colDef.field as keyof typeof type;

    // Add index signature to rowValue for string key access
    const rowValue = event.data as typeof type & { [key: string]: any };

    // Find corresponding FormGroup. Prefer id; fallback to index by reference.
    const idFieldControl = this.embeddedGridOptions.rowFormType?.getIdControl();
    const parent = idFieldControl?.parent as SoeFormGroup;
    const idFieldName =
      Object.keys(parent.controls).find(
        name => idFieldControl === parent.controls[name]
      ) || null;

    const rowIdValue =
      idFieldName && rowValue && rowValue?.hasOwnProperty(idFieldName)
        ? rowValue[idFieldName]
        : -1;
    let idx;
    if (rowIdValue <= 0) {
      idx = event.rowIndex || 0;
    } else {
      idx = this.embeddedGridOptions.formRows.controls.findIndex(
        (c: any) =>
          (idFieldName && c.value[idFieldName] === rowIdValue) ||
          c.value === rowValue
      );
    }

    const ctrl = this.embeddedGridOptions.formRows.at(idx) as
      | RowFormType
      | undefined;
    if (!ctrl) return;

    // Patch the single field so Angular emits value & validators re-run
    ctrl.patchValue({ [field]: rowValue[field] }, { emitEvent: true });
    ctrl.markAsDirty();
    ctrl.updateValueAndValidity({ emitEvent: true });
  }

  getFormControl(params: any) {
    const rowIndex = params.node.rowIndex;
    const field = params.colDef.field!;
    return (
      this.embeddedGridOptions.formRows?.at(rowIndex) as SoeFormGroup
    ).get(field)!;
  }

  setupValidation() {
    if (
      this.embeddedGridOptions.formRows !== undefined &&
      this.embeddedGridOptions.showValidationErrors
    ) {
      // setup signal
      this.embeddedGridOptions.formRows.valueChanges.subscribe(v => {
        this.grid.api.refreshCells({ force: true });
      });

      // setup column class rules
      this.grid.columns.forEach((colDef: ColDef) => {
        if (!colDef.cellClassRules) {
          colDef.cellClassRules = {};
        }
        this.addValidationClassRule(colDef);
      });
    }
  }

  addValidationClassRule(colDef: ColDef) {
    // if cellClassRules exists, add to it
    if (colDef.cellClassRules) {
      colDef.cellClassRules!['error-background-color'] = (params: any) => {
        if (!params.colDef.field) return false;

        const ctrl = this.getFormControl(params);
        if (ctrl && !ctrl.valid) {
          // && ctrl.touched
          return true;
        }
        return false;
      };
    }
  }

  setupEditListeners() {
    this.grid.api.updateGridOptions({
      onCellEditingStopped: this.onCellEditingStopped.bind(this),
      onCellEditingStarted: this.onCellEditingStarted.bind(this),
    });
  }

  // TODO: Remove parameter createToolbar, when all pages use new flowHandler.startFlow
  // TODO: Then createGridToolbar should not be called inside this method
  setupGrid(grid: GridComponent<T>, gridName: string, createToolbar = true) {
    this.grid = grid;
    this.grid.gridName = gridName;
    this.onGridReady.emit(grid);
  }

  finalizeInitGrid(
    countInfoOptions?: ISoeCountInfoOptions,
    defaultFilter?: IDefaultFilterSettings
  ) {
    this.setupValidation();
    this.grid.context.exportFilenameKey = this.exportFilenameKey();
    this.grid.finalizeInitGrid(countInfoOptions, defaultFilter);
    this.gridIsDefined = true;
    this.onGridDefined.emit(this.grid);
    this.onGridIsDefined();
  }

  onGridIsDefined() {
    // Override in grid component
  }

  addRow(
    rowItem: T | undefined = undefined,
    formRows: FormArray | undefined = undefined,
    rowFormType: any = null
  ) {
    // Override in grid component

    if (
      rowItem !== undefined &&
      formRows !== undefined &&
      rowFormType !== null
    ) {
      const row = new rowFormType({
        validationHandler: this.form?.formValidationHandler,
        element: rowItem,
      });
      formRows?.push(row);

      this.grid.addRow(rowItem, true);
      this.grid.api.refreshCells();
      this.form?.markAsDirty();

      // start editing?
      if (this.embeddedGridOptions.newRowStartEditField !== undefined) {
        const colKey = this.embeddedGridOptions.newRowStartEditField || '';
        setTimeout(() => {
          this.grid.scrollToFocus(rowItem, colKey);
          const lastRowId = this.grid.api.getLastDisplayedRowIndex();
          this.grid.api.setFocusedCell(lastRowId, colKey);
          this.grid.api.startEditingCell({
            rowIndex: lastRowId,
            colKey: colKey,
          });
        }, 50);
      }
    }
  }

  deleteRow(rowItem: any, formRows: FormArray | undefined = undefined) {
    // Override in grid component

    if (formRows !== undefined) formRows?.removeAt(rowItem.AG_NODE_ID);

    this.grid.deleteRow(rowItem);
    this.form?.markAsDirty();
  }

  createGridToolbar(config?: Partial<ToolbarEmbeddedGridConfig>) {
    if (!config) config = {};

    if (!config.reloadOption && config.showReload)
      config.reloadOption = this.getDefaultReloadOption();

    if (!config.clearFiltersOption && config.showClearFilters)
      config.clearFiltersOption = this.getDefaultClearFiltersOption();

    if (!config.newOption && !config.hideNew)
      config.newOption = this.getDefaultNewOption();

    if (config.showSorting && !config.sortFirstOption)
      config.sortFirstOption = this.getDefaultSortFirstOption(
        config.sortingField
      );
    if (config.showSorting && !config.sortLastOption)
      config.sortLastOption = this.getDefaultSortLastOption(
        config.sortingField
      );
    if (config.showSorting && !config.sortUpOption)
      config.sortUpOption = this.getDefaultSortUpOption(config.sortingField);
    if (config.showSorting && !config.sortDownOption)
      config.sortDownOption = this.getDefaultSortDownOption(
        config.sortingField
      );

    this.toolbarService.createDefaultEmbeddedGridToolbar(config);
  }

  protected getDefaultReloadOption() {
    return {
      onAction: () => this.refreshGrid(),
    };
  }

  protected getDefaultClearFiltersOption() {
    return {
      onAction: () => this.grid?.clearFilters(),
    };
  }

  protected getDefaultNewOption() {
    return {
      onAction: () => this.addRow(),
    };
  }

  protected getDefaultSortFirstOption(sortingField: string = 'sort') {
    return () => {
      this.grid?.sortFirst(sortingField);
      this.form?.markAsDirty();
    };
  }

  protected getDefaultSortLastOption(sortingField: string = 'sort') {
    return () => {
      this.grid?.sortLast(sortingField);
      this.form?.markAsDirty();
    };
  }

  protected getDefaultSortUpOption(sortingField: string = 'sort') {
    return () => {
      this.grid?.sortUp(sortingField);
      this.form?.markAsDirty();
    };
  }

  protected getDefaultSortDownOption(sortingField: string = 'sort') {
    return () => {
      this.grid?.sortDown(sortingField);
      this.form?.markAsDirty();
    };
  }

  protected getDefaultLegacyToolbarOptions(): Partial<ToolbarGridOptions> {
    return {
      reloadOption: {
        onClick: this.refreshGrid.bind(this),
      },
    };
  }

  selectionChanged(rows: T[]): void {
    this.selectedRows.set(rows);
  }

  selectedItemsChanged(items: Dict): void {
    // Override in grid component
  }

  clearSelectedItems(): void {
    this.grid?.clearSelectedItems();
  }

  protected refreshGrid(): void {
    this.clearSelectedItems();
  }

  edit(row: T, additionalProps?: TabWrapperRowEditedAdditionalProps) {
    if (!additionalProps) additionalProps = {};

    additionalProps.gridData = this.getAdditionalPropsGridData(
      this.grid.getAllRows()
    );

    const rows = this.grid.getFilteredRows();
    this.rowEdited.set({
      gridIndex: additionalProps?.gridIndex || this.gridIndex(),
      rows: additionalProps?.rows || this.grid.rows || [],
      row,
      filteredRows: rows,
      additionalProps,
    });
  }

  getAdditionalPropsGridData(rows: T[]): any {
    // Override in edit component
    return {};
  }

  // Grid events

  protected onCellEditingStoppedCheckIfHasChanged(
    event: CellEditingStoppedEvent
  ): boolean {
    // Ignore if no field or no change
    if (!event.colDef.field || event.newValue === event.oldValue) return false;

    // Compare dates to avoid unnecessary updates
    // Comparing dates above with === will always return false
    if (
      event.newValue instanceof Date &&
      event.oldValue instanceof Date &&
      event.newValue.getTime() === event.oldValue.getTime()
    )
      return false;

    return true;
  }
}
