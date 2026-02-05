import {
  ChangeDetectorRef,
  Component,
  HostBinding,
  OnChanges,
  SimpleChanges,
  effect,
  inject,
  input,
  signal,
  viewChildren,
} from '@angular/core';
import { BehaviorSubject, delay, Observable, of, take } from 'rxjs';
import { ValidationHandler } from '@shared/handlers';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { NavigatorRecordConfig } from '@ui/record-navigator/record-navigator.component';
import { SoeFormGroup } from '@shared/extensions';
import {
  MultiTabConfig,
  MultiTabWrapperEdit,
  OpenEditInNewTab,
  TabWrapperGridDataLoaded,
  TabWrapperRowEdited,
} from '../models/multi-tab-wrapper.model';
import { Guid } from '@shared/util/string-util';
import { Perform } from '@shared/util/perform.class';
import { deleteItem, upsert } from '@shared/util/array-util';
import { CrudActionTypeEnum } from '@shared/enums';
import {
  ActionTaken,
  CopyActionTaken,
  SetNewRefOnTab,
} from '@shared/directives/edit-base/edit-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ProgressService } from '@shared/services/progress/progress.service';
import { TabComponent } from '../tab/tab.component';
import { TabGroupComponent } from '../tab-group/tab-group.component';
import { CommonModule, NgComponentOutlet } from '@angular/common';
import { ClickOutsideDirective } from '@shared/directives/click-outside/click-outside.directive';

@Component({
  selector: 'soe-multi-tab-wrapper',
  imports: [
    CommonModule,
    NgComponentOutlet,
    ClickOutsideDirective,
    TabComponent,
    TabGroupComponent,
    TranslatePipe,
  ],
  templateUrl: './multi-tab-wrapper.component.html',
  styleUrls: ['./multi-tab-wrapper.component.scss'],
})
export class MultiTabWrapperComponent<TGrid> implements OnChanges {
  config = input<MultiTabConfig[]>([]);
  hideAdd = input(false);
  hideCloseAll = input(false);
  preventMultipleNewTabs = input(false);

  @HostBinding('style.max-width') get width() {
    return this.setWidth();
  }

  cdr = inject(ChangeDetectorRef);
  messageboxService = inject(MessageboxService);
  translate = inject(TranslateService);
  validationHandler = inject(ValidationHandler);
  progressService = inject(ProgressService);
  performGetGrid = new Perform<unknown[]>(this.progressService);
  // Bound in edit-base
  actionTakenSignal = signal<ActionTaken | undefined>(undefined);
  copyActionTakenSignal = signal<CopyActionTaken | undefined>(undefined);
  openEditInNewTabSignal = signal<OpenEditInNewTab | undefined>(undefined);
  setNewRefOnTabSignal = signal<SetNewRefOnTab | undefined>(undefined);
  // Bound in grid-base
  rowEdited = signal<TabWrapperRowEdited<unknown> | undefined>(undefined);
  gridDataLoaded = signal<TabWrapperGridDataLoaded<unknown> | undefined>(
    undefined
  );
  editTabs: MultiTabWrapperEdit[] = [];
  activeTabIndex = 0;
  visibleTabsForCreateTabMenu: MultiTabConfig[] = this.config();
  showAddList = signal(false);
  addButton: HTMLElement | null = null;

  tabOutlets = viewChildren(NgComponentOutlet);

  get hasNewTab() {
    return this.editTabs.some(t => t.isNew);
  }

  get idFieldName() {
    return (
      this.editTabs[this.tabIndexWithoutGrid(this.activeTabIndex)]?.inputs?.form
        ?.idFieldName || ''
    );
  }
  get nameFieldName() {
    return (
      this.editTabs[this.tabIndexWithoutGrid(this.activeTabIndex)]?.inputs?.form
        ?.nameFieldName || ''
    );
  }

  tabIndexWithoutGrid = (tabIndex: number) =>
    tabIndex - this.config().filter(f => f.gridComponent).length;
  addGridsTabsToTabIndex = (tabIndex: number) =>
    tabIndex + this.config().length;

  constructor() {
    effect(() => {
      const gridDataLoaded = this.gridDataLoaded();

      const gridIndex = gridDataLoaded?.gridIndex;
      if (typeof gridIndex === 'undefined' || gridIndex === -1) return;

      if (!gridDataLoaded || !this.config() || !this.config()[gridIndex])
        return;

      this.setGridData(gridIndex, gridDataLoaded.rows || []);
    });

    effect(() => {
      // Called from grid when opening an existing record
      const rowEdited = this.rowEdited();

      const gridIndex = rowEdited?.gridIndex;
      if (typeof gridIndex === 'undefined' || gridIndex === -1) return;

      if (!rowEdited || !this.config() || !this.config()[gridIndex]) return;

      this.createEditTab(rowEdited, gridIndex);
    });

    effect(() => {
      // Called from edit page on save or delete
      const actionTaken = this.actionTakenSignal();
      if (actionTaken) this.performAction(actionTaken);
    });

    effect(() => {
      // Called from edit page on copy
      const copyActionTaken = this.copyActionTakenSignal();
      if (copyActionTaken) this.createCopy(copyActionTaken);
    });

    effect(() => {
      // Called from edit page when opening a record in a new tab
      const openEditInNewTab = this.openEditInNewTabSignal();
      if (openEditInNewTab) this.createEditInNewTab(openEditInNewTab);
    });

    effect(() => {
      // Called from edit page when setting new ref on tab (new record in same tab)
      const setNewRefOnTab = this.setNewRefOnTabSignal();
      if (setNewRefOnTab) this.setNewRefOnTab(setNewRefOnTab);
    });
  }

  private setWidth(): string {
    const wrapper = document.getElementById('wrapper');
    if (wrapper) {
      // Left menu is 200px wide, collapsed left menu is 48px
      // Add to that some margins of 50px
      if (wrapper.classList.contains('collapse-menu'))
        return 'calc(100vw - 98px)';
      else return 'calc(100vw - 250px)';
    }
    return 'auto';
  }

  ngOnChanges({ config }: SimpleChanges) {
    if (config) {
      if (config.currentValue?.length > 0) {
        if (config.currentValue[0].gridComponent) {
          this.activeTabIndex = 0;
        } else if (config.currentValue[0].editComponent) {
          // No grid component, only edit component specified
          // Create a tab with the edit component
          this.createEditTabAsInitial(0);
        }
      }
    }
  }

  setGridData(gridIndex: number, data: unknown[]) {
    this.config()[gridIndex].rowData = new BehaviorSubject<unknown[]>(
      data || []
    );
  }

  tabIndexChanged(index: number) {
    if (this.activeTabIndex === index) return;

    this.activeTabIndex = index;

    this.callOnTabActivatedOnComponent(index);
  }

  private callOnTabActivatedOnComponent(tabIndex: number): void {
    // Wait a tick for outlet render
    queueMicrotask(() => {
      const outlet = this.tabOutlets()[tabIndex];
      if (!outlet) return;

      const instance =
        (outlet as any)?._componentRef?.instance ||
        (outlet as any)?._viewContainerRef?.get(0)?.instance ||
        null;

      if (instance && typeof instance.onTabActivated === 'function') {
        try {
          instance.onTabActivated();
        } catch {
          console.error('Error calling onTabActivated on component instance');
        }
      }
    });
  }

  private createEditTabAsInitial(configIndex: number) {
    const { editComponent, editTabLabel, FormClass } =
      this.config()[configIndex];

    // Create new tab with edit component
    const tab: MultiTabWrapperEdit = {
      ref: Guid.newGuid(),
      isNew: false,
      label: editTabLabel ? this.translate.instant(editTabLabel) : '',
      hideCloseTab: true,
      component: editComponent!,
      recordConfig: new NavigatorRecordConfig(),
      gridIndex: configIndex,
      inputs: {},
    };
    this.setCommonInputs(
      tab,
      FormClass
        ? new FormClass({
            validationHandler: this.validationHandler,
          })
        : null
    );
    this.editTabs.push(tab);
  }

  private createEditTab(
    rowEdited: TabWrapperRowEdited<unknown>,
    gridIndex: number
  ) {
    // Called from grid when opening an existing record
    const {
      editComponent,
      editTabLabel,
      FormClass,
      recordConfig: recordConfig,
    } = this.config()[gridIndex];

    const { row, additionalProps } = rowEdited;

    const _FormClass = additionalProps?.FormClass || FormClass;
    const form: SoeFormGroup = _FormClass
      ? new _FormClass({
          validationHandler: this.validationHandler,
          element: row,
        })
      : null;

    const tabLabel = additionalProps?.editTabLabel || editTabLabel;
    const componentToAdd = additionalProps?.editComponent || editComponent;

    // Always create a new tab if no ID
    const ref =
      _FormClass && form.getIdControl()?.value
        ? `${form.getIdControl()?.value}-${componentToAdd.name}`
        : Guid.newGuid();

    const foundIndex = this.editTabs.findIndex(r => r.ref === ref);
    if (foundIndex !== -1) {
      // If ref exists - a tab is already open.
      this.activeTabIndex = this.addGridsTabsToTabIndex(foundIndex);
      return;
    }

    const filteredRows = rowEdited.filteredRows.map((r: any) => ({
      id: r[form.idFieldName],
      name: r[form.nameFieldName],
    }));

    // Create new tab with edit component for existing record
    const tab: MultiTabWrapperEdit = {
      ref,
      isNew: false,
      label: tabLabel ? this.translate.instant(tabLabel) : '',
      component: componentToAdd,
      recordConfig: recordConfig || new NavigatorRecordConfig(),
      gridIndex: gridIndex,
      inputs: {},
    };

    this.setCommonInputs(tab, form);

    // Filtered rows are added to the form where it's picked up by the toolbar
    tab.inputs.form!.records = filteredRows;

    // Additional data can be sent from the grid to the edit page through additionalProps
    tab.inputs.form!.gridData = additionalProps.gridData;
    this.editTabs.push(tab);
  }

  private createCopy(copyActionTaken: CopyActionTaken) {
    // Called from edit page on copy
    const sourceTab = this.getTabByRef(copyActionTaken.ref);
    if (!sourceTab) return;

    const {
      editComponent,
      createTabLabel,
      FormClass,
      recordConfig: recordConfig,
    } = this.config()[sourceTab.gridIndex];

    if (!editComponent || !createTabLabel || !copyActionTaken.form) return;

    // Create new tab with edit component for copy of an existing record
    const tab: MultiTabWrapperEdit = {
      ref: Guid.newGuid(),
      isNew: true,
      label: this.translate.instant(createTabLabel),
      component: editComponent,
      recordConfig: recordConfig || new NavigatorRecordConfig(),
      gridIndex: sourceTab.gridIndex,
      inputs: {},
    };

    this.setCommonInputs(
      tab,
      FormClass
        ? new FormClass({
            validationHandler: this.validationHandler,
            element: copyActionTaken.form.getAllValues({
              includeDisabled: true,
            }),
          })
        : null
    );

    if (tab.inputs.form) {
      // Filtered rows are added to the form where it's picked up by the toolbar
      tab.inputs.form.records = copyActionTaken.form.records;
      tab.inputs.form.isNew = true;
      tab.inputs.form.isCopy = true;
      if (copyActionTaken.additionalProps)
        tab.inputs.form.additionalPropsOnCopy = copyActionTaken.additionalProps;
      tab.inputs.form.markAsDirty();
    }
    this.editTabs.push(tab);
  }

  private createEditInNewTab(openEditInNewTab: OpenEditInNewTab) {
    // Called from edit page when opening a record in a new tab
    const { id, additionalProps } = openEditInNewTab;

    // Create new tab with edit component
    const tab: MultiTabWrapperEdit = {
      ref: Guid.newGuid(),
      isNew: false,
      label: additionalProps?.editTabLabel
        ? this.translate.instant(additionalProps?.editTabLabel)
        : '',
      component: additionalProps.editComponent,
      recordConfig: new NavigatorRecordConfig(),
      gridIndex: -1,
      inputs: {},
    };
    this.setCommonInputs(
      tab,
      additionalProps.FormClass
        ? new additionalProps.FormClass({
            validationHandler: this.validationHandler,
          })
        : null
    );
    tab.inputs.form!.records = [];
    tab.inputs.form?.patchValue({ [tab.inputs.form?.getIdFieldName()]: id });
    if (additionalProps?.data) tab.inputs.form!.data = additionalProps?.data;
    this.editTabs.push(tab);
  }

  private setNewRefOnTab(setNewRefOnTab: SetNewRefOnTab) {
    const tab = this.getTabByRef(setNewRefOnTab.ref.toString());
    if (!tab) return;

    tab.ref = tab.inputs.ref = setNewRefOnTab.newRef.toString();
    if (setNewRefOnTab.isNew) {
      tab.isNew = true;
      tab.label = this.translate.instant(
        this.config()[tab.gridIndex].createTabLabel || ''
      );
      if (tab.inputs.form) {
        tab.inputs.form.isNew = true;
        tab.inputs.form.isCopy = false;
      }
    }
  }

  private performAction(actionTaken: ActionTaken) {
    const tab = this.getTabByRef(actionTaken.ref);
    if (!tab) return;
    const keepNewFormOnAfterSave =
      actionTaken.additionalProps?.keepNewFormOnAfterSave;
    if (tab.isNew && !keepNewFormOnAfterSave) {
      // Update ref on tab.
      // When tab is opened with a new record, ref will be set to a new Guid.
      // When tab is opened with an existing record, ref will be the ID of the record followed by the name of the edit component.
      // So, when saving a new record we need to change the ref, otherwise if we open the new record again it will open a new tab.
      const { editComponent } = this.config()[tab.gridIndex];
      tab.ref = `${actionTaken.rowItemId}-${editComponent?.name}`;
      tab.inputs.ref = `${actionTaken.rowItemId}-${editComponent?.name}`;
    }

    this.updateGridByAction(
      tab.gridIndex,
      actionTaken.type,
      actionTaken.rowItemId,
      actionTaken.form,
      actionTaken.additionalProps,
      actionTaken.updateGrid
    );
  }

  initalizeAddTab(): void {
    // Check if list of different entities should be shown or not
    this.visibleTabsForCreateTabMenu = this.config();
    if (this.preventMultipleNewTabs() && this.hasNewTab) return;

    if (this.config().length === 1
      && !this.config()[0].addOptions?.length
    ) {
      this.createTabByTabIndex(0);
    } else {
      const firstVisibleTabIndex = this.visibleTabsForCreateTabMenu.findIndex(
        t => !t.hideForCreateTabMenu
      ) 
      if (
        this.visibleTabsForCreateTabMenu.filter(t => !t.hideForCreateTabMenu)
          .length === 1
          && firstVisibleTabIndex > -1
          && !this.config()[firstVisibleTabIndex].addOptions?.length
      ) {
        this.createTabByTabIndex(
          firstVisibleTabIndex
        );
      } else {
        this.addButton = document.querySelector('.soe-tab__add-tab-button');
        // Delay is used so that the clickOutside event isn't
        // triggered when the dropdown is opened
        of(true)
          .pipe(delay(10))
          .subscribe(() => this.showAddList.set(true));
      }
    }
  }

  closeAddSelection() {
    this.showAddList.set(false);
  }

  createTabByTabIndex(index: number, addOptionId?: number) {
    this.addTab(this.config()[index], index, addOptionId);
    this.showAddList.set(false);
  }

  addTab(conf: MultiTabConfig, gridIndex: number, addOptionId?: number) {
    // Create new tab with edit component for new record

    const FormClass = conf.FormClass;
    if (!conf.editComponent || !conf.createTabLabel) return;
    const tab: MultiTabWrapperEdit = {
      ref: Guid.newGuid(),
      isNew: true,
      label: this.translate.instant(conf.createTabLabel),
      component: conf.editComponent,
      recordConfig: conf.recordConfig || new NavigatorRecordConfig(),
      gridIndex,
      inputs: {},
    };
    this.setCommonInputs(
      tab,
      FormClass
        ? new FormClass({
            validationHandler: this.validationHandler,
            element: undefined,
          })
        : null
    );

    if (tab.inputs.form) {
      tab.inputs.form.isNew = true;
    }

    if (addOptionId) {
      tab.inputs.addOptionId = addOptionId;
    }

    // Grid rows will be sent from the grid to the edit page if config property passGridDataOnAdd is set
    if (this.config()[gridIndex].passGridDataOnAdd) {
      const gridTab = this.config()[gridIndex];
      if (gridTab) {
        const gridData: TGrid[] = gridTab.rowData?.value as TGrid[];
        tab.inputs.form!.gridData = gridData;
      }
    }
    this.editTabs.push(tab);
  }

  updateGridByAction(
    gridIndex: number,
    action: number,
    rowItemId: number,
    form?: SoeFormGroup,
    additionalProps?: any,
    updateGrid?: (id?: number, additionalProps?: any) => Observable<any[]>
  ): void {
    switch (action) {
      case CrudActionTypeEnum.Save:
        // Additional save props
        const closeTab = additionalProps?.closeTabOnSave;
        this.updateOrCreateItem(
          gridIndex,
          rowItemId,
          form,
          updateGrid,
          closeTab
        );
        break;
      case CrudActionTypeEnum.Delete:
        // Additional delete props
        const skipUpdateGrid = additionalProps?.skipUpdateGrid;

        if (!skipUpdateGrid) this.removeItemFromGrid(gridIndex, rowItemId);

        // Remove the tab with the deleted record
        this.removeTab(this.activeTabIndex);
        this.activeTabIndex = 0;
        break;
    }
  }

  updateOrCreateItem(
    gridIndex: number,
    rowItemId?: number,
    form?: SoeFormGroup,
    updateGrid?: (id?: number, additionalProps?: any) => Observable<any[]>,
    closeTab = false
  ): void {
    if (!rowItemId) return;

    const gridTab = this.config()[gridIndex];
    if (!gridTab) return;

    // Fetch saved record and update it in the edit component
    // Update form values and set reset-state
    const tabIndex = this.tabIndexWithoutGrid(this.activeTabIndex);
    const tab = this.editTabs[tabIndex];
    // Update refs, and labels
    tab.isNew = false;
    tab.inputs.form = form;
    if (tab.inputs.form) tab.inputs.form.isNew = false;
    tab.label = this.translate.instant(
      this.config()[tab.gridIndex].editTabLabel || ''
    );
    const idFieldName = this.idFieldName;
    if (closeTab) {
      this.removeTab(this.activeTabIndex);
    } else {
      this.upsertItemInRecordNavigator(form?.value);
    }

    // Trigger change-detection
    this.cdr.markForCheck();
    this.editTabs = [...this.editTabs];

    // Fetch saved record and update it in the grid
    if (updateGrid) {
      updateGrid()
        .pipe(take(1))
        .subscribe(data => {
          if (data[0]) {
            gridTab.rowData?.next(
              upsert(
                gridTab.rowData?.value || [],
                <any>data[0],
                idFieldName,
                true
              ) || []
            );
          }
        });
    }
  }

  upsertItemInRecordNavigator(value: any) {
    // If record is modified, rename the record in record navigator in all opened tabs
    this.editTabs.forEach(tab => {
      if (tab.inputs.form?.records) {
        const id = value[this.idFieldName];
        const name = value[this.nameFieldName];
        upsert<SmallGenericType>(
          tab.inputs.form?.records,
          new SmallGenericType(id, name),
          'id',
          true
        );
      }
    });
  }

  removeItemFromGrid(gridIndex: number, rowItemId: number): void {
    const gridTab = this.config()[gridIndex];
    if (!gridTab) return;

    const rowData: TGrid[] = gridTab.rowData?.value as TGrid[];
    if (!rowData) return;

    const editTab =
      this.editTabs[this.tabIndexWithoutGrid(this.activeTabIndex)];
    if (!editTab) return;

    const idFieldName: string = editTab.inputs.form?.idFieldName || '';
    if (!idFieldName) return;

    const row = rowData.find(r => r[<keyof TGrid>idFieldName] === rowItemId);
    if (row) {
      const newRows = deleteItem(rowData || [], row, <keyof TGrid>idFieldName);
      setTimeout(() => {
        gridTab.rowData?.next(newRows);
      }, 0);
      this.removeItemFromRecordNavigator(row, idFieldName);
    }
  }

  removeItemFromRecordNavigator(row: any, idFieldName: string): void {
    // If a record is removed, remove it in record navigator in other opened tabs
    this.editTabs.forEach(tab => {
      const index = tab.inputs.form?.records.findIndex(
        record => record.id === row[idFieldName]
      );
      typeof index !== 'undefined' &&
        index > -1 &&
        tab.inputs.form?.records.splice(index, 1);
    });
  }

  removeTab(tabIndex: number) {
    this.editTabs.splice(this.tabIndexWithoutGrid(tabIndex), 1);
  }

  removeAllTabs() {
    this.editTabs.splice(0, this.editTabs.length);
  }

  tabDoubleClicked(tabIndex: number) {
    const tab = this.editTabs[this.tabIndexWithoutGrid(tabIndex)];
    const form = tab?.inputs?.form;
    if (form) {
      const id = form.getIdControl()?.value;
      this.messageboxService.information(
        'Debug info',
        `${form.getIdFieldName()}: ${id}\ntab.ref: ${tab.ref}\ninputs.ref: ${
          tab?.inputs?.ref
        }`,
        {
          customIconName: 'ban-bug',
          hiddenText: JSON.stringify(form.value),
        }
      );
    }
  }

  // HELP-METHODS

  private setCommonInputs(
    tab: MultiTabWrapperEdit,
    form: SoeFormGroup | undefined
  ) {
    if (!tab.inputs) tab.inputs = {};

    tab.inputs.ref = tab.ref;
    tab.inputs.form = form;
    tab.inputs.actionTakenSignal = this.actionTakenSignal;
    tab.inputs.copyActionTakenSignal = this.copyActionTakenSignal;
    tab.inputs.openEditInNewTabSignal = this.openEditInNewTabSignal;
    tab.inputs.setNewRefOnTabSignal = this.setNewRefOnTabSignal;
  }

  private getTabByRef(
    ref: string | undefined
  ): MultiTabWrapperEdit | undefined {
    if (!ref) return undefined;

    return this.editTabs.find(t => t.ref === ref);
  }
}
