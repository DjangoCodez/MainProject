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
import { TranslateService } from '@ngx-translate/core';
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
  ToolbarGridConfig,
  ToolbarGridOptions,
} from '@ui/toolbar/models/toolbar';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ToolbarUtils } from '@ui/toolbar/utils/toolbar.utils';
import {
  IDefaultFilterSettings,
  ISoeCountInfoOptions,
} from '@ui/grid/interfaces';
import { CellEditingStoppedEvent } from 'ag-grid-community';
import { BehaviorSubject, delay, map, Observable, of, tap } from 'rxjs';

export interface IApiServiceGrid<T> {
  getGrid: (id?: number, additionalProps?: any) => Observable<T[]>;
}

@Directive({
  selector: '[soeGridBase]',
  standalone: true,
})
export class GridBaseDirective<
  T,
  S extends IApiServiceGrid<T> = IApiServiceGrid<T>,
> implements OnInit
{
  guid = input(Guid.newGuid());
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
  saveButtonDisabled = signal(true);
  translate = inject(TranslateService);
  flowHandler = inject(FlowHandlerService);
  service!: S;

  grid!: GridComponent<T>;
  gridName!: string;

  toolbarService = inject(ToolbarService);
  toolbarUtils = new ToolbarUtils();

  progressService = inject(ProgressService);
  performLoadData = new Perform<any>(this.progressService);

  terms: TermCollection = {};

  ngOnInit(): void {}

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
      skipInitialLoad: options?.skipInitialLoad,
      skipDefaultToolbar: options?.skipDefaultToolbar,
      useLegacyToolbar: options?.useLegacyToolbar,
      onPermissionsLoaded: this.onPermissionsLoaded.bind(this),
      onSettingsLoaded: this.onSettingsLoaded.bind(this),
      onGridReadyToDefine: this.onGridReadyToDefine.bind(this),
      onFinished: this.onFinished.bind(this),
    };

    // Default functionality for load data if not overrided in grid component
    if (!options?.skipInitialLoad)
      this.flowHandler.options.data = this.loadAndSetData();

    // Create default toolbar if not skipped in grid component
    if (!this.flowHandler.options.skipDefaultToolbar) {
      if (this.flowHandler.options.useLegacyToolbar) {
        this.flowHandler.options.setupDefaultToolbar =
          this.createLegacyGridToolbar.bind(this);
      } else {
        this.flowHandler.options.setupDefaultToolbar =
          this.createGridToolbar.bind(this);
      }
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
  }

  private loadAndSetData(): Observable<T[]> {
    return this.loadData().pipe(
      map(data => {
        if (this.rowData) this.rowData.next(data); // updates Edit component's child grid(s)
        this.notifyGridDataLoaded(data); // updates Tab component's child grid
        return data;
      }),
      /* delay(0) makes sure grid is properly populated before continuing. Needed for cases where you want to use data in the grid. 
        queueMicroTask and waiting for rowData Observable doesn't work for all scenarios, likely due to happening 
        too early and too late respectively. */
      delay(0),
      tap(data => {
        this.onAfterLoadData(data);
      })
    );
  }

  loadData(id?: number, additionalProps?: any): Observable<T[]> {
    // Override in grid component
    return this.performLoadData.load$(
      this.service.getGrid(id, additionalProps),
      { showDialogDelay: 1000 }
    );
  }

  onAfterLoadData(data?: T[]): void {
    // Override in component
  }

  notifyGridDataLoaded(rows: T[]) {
    this.gridDataLoaded.set({
      gridIndex: this.gridIndex(),
      rows: rows || [],
    });
  }

  onFinished(): void {
    // Override in grid component
  }

  onTabActivated(): void {
    // Override in grid component
  }

  // TODO: Remove parameter createToolbar, when all pages use new flowHandler.startFlow
  // TODO: Then createGridToolbar should not be called inside this method
  setupGrid(grid: GridComponent<T>, gridName: string, createToolbar = true) {
    this.grid = grid;
    this.grid.gridName = gridName;
    if (createToolbar) this.createLegacyGridToolbar();
    this.onGridReady.emit(grid);
  }

  finalizeInitGrid(
    countInfoOptions?: ISoeCountInfoOptions,
    defaultFilter?: IDefaultFilterSettings
  ) {
    this.grid.context.exportFilenameKey = this.exportFilenameKey();
    this.grid.finalizeInitGrid(countInfoOptions, defaultFilter);
    this.gridIsDefined = true;
    this.onGridDefined.emit(this.grid);
    this.onGridIsDefined();
  }

  onGridIsDefined() {
    // Override in grid component
  }

  createGridToolbar(config?: Partial<ToolbarGridConfig>) {
    config ??= {};

    if (!config.reloadOption && !config.hideReload)
      config.reloadOption = this.getDefaultReloadOption();

    if (!config.clearFiltersOption && !config.hideClearFilters)
      config.clearFiltersOption = this.getDefaultClearFiltersOption();

    if (!config.saveOption && config.useDefaltSaveOption)
      config.saveOption = this.getDefaultSaveOption();

    this.toolbarService.createDefaultGridToolbar(config);
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

  protected getDefaultSaveOption() {
    return {
      disabled: this.saveButtonDisabled,
      onAction: () => this.saveStatus(),
    };
  }

  createLegacyGridToolbar(options?: Partial<ToolbarGridOptions>): void {
    this.toolbarUtils.createDefaultLegacyGridToolbar(
      this.grid,
      options || this.getDefaultLegacyToolbarOptions()
    );
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
    // If other conditions are needed to enable/disable save button
    this.saveButtonDisabled.set(Object.keys(items.dict).length === 0);
  }

  saveStatus() {
    // Override in grid component
  }

  clearSelectedItems(): void {
    this.grid?.clearSelectedItems();
  }

  protected refreshGrid(): void {
    this.loadAndSetData().subscribe();
    this.clearSelectedItems();
  }

  protected refreshGrid$(): Observable<T[]> { 
    return this.loadAndSetData().pipe(tap(() => this.clearSelectedItems()));
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
      filteredRows: additionalProps?.filteredRows || rows,
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
