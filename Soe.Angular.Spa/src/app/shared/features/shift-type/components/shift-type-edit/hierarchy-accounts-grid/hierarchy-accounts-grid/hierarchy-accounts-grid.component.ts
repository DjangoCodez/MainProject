import { Component, inject, Input, input, OnInit, signal } from '@angular/core';
import { EmbeddedGridBaseDirective } from '@shared/directives/grid-base/embedded-grid-base.directive';
import {
  createAccountIdValidator,
  HierarchyAccountsForm,
} from '@shared/features/shift-type/models/hierarchy-accounts.form.model';
import { ShiftTypeForm } from '@shared/features/shift-type/models/shift-type-form.model';
import { TermCollection } from '@shared/localization/term-types';
import {
  Feature,
  TermGroup,
  TermGroup_AttestRoleUserAccountPermissionType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDimSmallDTO,
  IShiftTypeHierarchyAccountDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellEditingStoppedEvent } from 'ag-grid-community';
import { BehaviorSubject, Observable, startWith, take, tap } from 'rxjs';

@Component({
  selector: 'soe-hierarchy-accounts-grid',
  templateUrl:
    '../../../../../../../shared/ui-components/grid/grid-wrapper/embedded-grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class HierarchyAccountsGridComponent
  extends EmbeddedGridBaseDirective<IShiftTypeHierarchyAccountDTO>
  implements OnInit
{
  @Input({ required: true }) form!: ShiftTypeForm;

  defaultAccountDim = input(0);

  toolbarNoBorder = input(true);
  toolbarNoMargin = input(true);
  toolbarNoTopBottomPadding = input(true);
  height = input(66);

  hierarchyAccounts = new BehaviorSubject<IShiftTypeHierarchyAccountDTO[]>([]);
  accountDims: IAccountDimSmallDTO[] = [];
  accountIds: SmallGenericType[] = [];
  accountDimLevels: SmallGenericType[] = [];
  accountPermissionTypes: SmallGenericType[] = [];

  private accountIdToDimMap = new Map<number, number>();

  readonly coreService = inject(CoreService);
  readonly progress = inject(ProgressService);
  readonly messageboxService = inject(MessageboxService);
  readonly performLoadAccount = new Perform<IAccountDimSmallDTO[]>(
    this.progress
  );

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_ScheduleSettings_ShiftType_Edit,
      'time.schedule.shifttype.shifttype.accounthierarchy',
      {
        skipInitialLoad: true,
        lookups: [this.loadAccounts(true), this.loadAccountPermissionTypes()],
      }
    );

    this.form.hierarchyAccounts.valueChanges
      .pipe(startWith(this.form.hierarchyAccounts.getRawValue()))
      .subscribe(v => {
        this.initRows(v as IShiftTypeHierarchyAccountDTO[]);
      });
  }

  override createGridToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('addrow', {
          iconName: signal('plus'),
          caption: signal('common.newrow'),
          tooltip: signal('common.newrow'),
          onAction: () => this.addRow(),
        }),
      ],
    });
  }

  private initRows(rows: IShiftTypeHierarchyAccountDTO[]) {
    this.updateHierarchyAccountDims();
    // Set validation per row
    this.form.hierarchyAccounts.controls.forEach((ctrl, idx) => {
      // Map by index because new rows may all have id=0
      const thisRow = rows[idx];
      if (thisRow) (<any>thisRow).warnings = ctrl.validateRow();
    });
    this.rowData.next(rows);
  }

  override onGridReadyToDefine(
    grid: GridComponent<IShiftTypeHierarchyAccountDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.accountdim', 'common.name', 'core.permission'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'accountDimId' as keyof IShiftTypeHierarchyAccountDTO,
          terms['common.accountdim'],
          this.accountDimLevels || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 33,
            editable: true,
          }
        );
        this.grid.addColumnSelect(
          'accountId',
          terms['common.name'],
          this.accountIds || [],
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 33,
            editable: true,
            dynamicSelectOptions: (row: any) => {
              const dimId = row?.data?.accountDimId || 0;
              return this.getAccountsForDimension(dimId);
            },
          }
        );
        this.grid.addColumnSelect(
          'accountPermissionType',
          terms['core.permission'],
          this.accountPermissionTypes,
          undefined,
          {
            editable: true,
            flex: 33,
          }
        );
        this.grid.addColumnIcon('warnings', '', {
          width: 40,
          iconName: 'exclamation-circle',
          iconClass: 'warning-color',
          enableHiding: false,
          tooltip: terms['core.warning'],
          showIcon: row => {
            return (<any>row).warnings?.length > 0;
          },
          onClick: row => {
            this.showRowWarnings(row);
          },
        });
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
          suppressFloatingFilter: true,
        });

        this.grid.context.suppressGridMenu = true;
        this.grid.setNbrOfRowsToShow(1);
        super.finalizeInitGrid({ hidden: true });
      });
  }

  override addRow(): void {
    const row: any = {
      shiftTypeHierarchyAccountId: 0,
      accountDimId: 0,
      accountId: 0,
      accountPermissionType: 0,
    };
    super.addRow(row, this.form.hierarchyAccounts, HierarchyAccountsForm);
  }

  override deleteRow(row: any) {
    super.deleteRow(row, this.form.hierarchyAccounts);
  }

  override onCellEditingStopped(event: CellEditingStoppedEvent) {
    super.onCellEditingStopped(event);

    if (!this.onCellEditingStoppedCheckIfHasChanged(event)) return;
    if (!event.colDef?.field) return;

    const field = event.colDef.field as keyof IShiftTypeHierarchyAccountDTO;
    const rowValue = event.data as IShiftTypeHierarchyAccountDTO;

    // Find corresponding FormGroup. Prefer id; fallback to index by reference.
    let idx = this.form.hierarchyAccounts.controls.findIndex(
      (c: any) =>
        (rowValue.shiftTypeHierarchyAccountId &&
          c.value.shiftTypeHierarchyAccountId ===
            rowValue.shiftTypeHierarchyAccountId) ||
        c.value === rowValue
    );
    if (idx === -1 && typeof event.rowIndex === 'number') {
      // If grid is sorted/filtered this may not align, but fallback
      idx = event.rowIndex;
    }
    const ctrl = this.form.hierarchyAccounts.at(idx) as
      | HierarchyAccountsForm
      | undefined;
    if (!ctrl) return;

    // When changing accountDim, always reset accountId
    switch (event.colDef.field) {
      case 'accountDimId':
        ctrl.controls.accountId.patchValue(0);
        break;
    }
    if (event.data) event.data.warnings = ctrl.validateRow();

    // Patch the single field so Angular emits value & validators re-run
    ctrl.patchValue({ [field]: rowValue[field] }, { emitEvent: true });

    ctrl.markAsDirty();
    ctrl.updateValueAndValidity({ emitEvent: true });

    // Optionally trigger array validity if you have cross-row validators
    this.form.hierarchyAccounts.updateValueAndValidity({ emitEvent: false });
  }

  // region LOAD DATA
  override loadTerms(): Observable<TermCollection> {
    return super
      .loadTerms(['time.schedule.shifttype.missinghierarchyaccount'])
      .pipe(
        tap(() => {
          this.addFormValidators();
        })
      );
  }

  private loadAccounts(useCache: boolean): Observable<IAccountDimSmallDTO[]> {
    return this.performLoadAccount.load$(
      this.coreService
        .getAccountDimsSmall(
          false,
          true,
          true,
          false,
          true,
          false,
          false,
          true,
          useCache
        )
        .pipe(
          tap(x => {
            this.accountDims = x;
            // Populate accountDims
            this.accountDims.forEach(dim => {
              this.accountDimLevels.push({
                id: dim.accountDimId,
                name: dim.name,
              });
              // Populate accountIds
              dim.accounts.forEach(account => {
                this.accountIds.push({
                  id: account.accountId,
                  name: account.name,
                });
                // Set accountIdToDimMap
                this.accountIdToDimMap.set(account.accountId, dim.accountDimId);
              });
            });
            this.updateHierarchyAccountDims();
          })
        )
    );
  }

  private loadAccountPermissionTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.AttestRoleUserAccountPermissionType,
        false,
        false,
        true
      )
      .pipe(
        tap(x => {
          this.accountPermissionTypes = x.filter(
            y => y.id !== TermGroup_AttestRoleUserAccountPermissionType.ReadOnly
          );
        })
      );
  }

  // region HELPER METHODS
  private getAccountsForDimension(dimId: number): SmallGenericType[] {
    if (!dimId) return [];

    return this.accountIds.filter(account => {
      const accountDimId = this.accountIdToDimMap.get(account.id);
      return accountDimId === dimId;
    });
  }

  private updateHierarchyAccountDims(): void {
    const hierarchyAccounts = this.form?.hierarchyAccounts.value;
    if (!hierarchyAccounts || hierarchyAccounts.length === 0) return;

    hierarchyAccounts.forEach(account => {
      const accountId = account.accountId;
      if (accountId && this.accountIdToDimMap.has(accountId)) {
        const dimId = this.accountIdToDimMap.get(accountId);
        account.accountDimId = dimId;
      }
    });
  }

  private showRowWarnings(row: any) {
    const warnings: string[] = row.warnings;
    if (warnings.length > 0) {
      this.translate
        .get(warnings)
        .pipe(take(1))
        .subscribe(terms => {
          this.messageboxService.warning(
            this.terms['core.warning'],
            warnings.map(w => terms[w]).join('<br>'),
            { buttons: 'ok' }
          );
        });
    }
  }

  private addFormValidators() {
    this.form.addValidators([
      createAccountIdValidator(
        this.terms['time.schedule.shifttype.missinghierarchyaccount']
      ),
    ]);
  }
}
