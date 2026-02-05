import {
  Component,
  computed,
  input,
  OnChanges,
  OnInit,
  output,
  SimpleChanges,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import {
  ISieAccountDimMappingDTO,
  ISieAccountMappingDTO,
} from '@shared/models/generated-interfaces/SieImportDTO';
import {
  IAccountDimDTO,
  IAccountDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { GridComponent } from '@ui/grid/grid.component';
import { CellClassParams, CellValueChangedEvent } from 'ag-grid-community';
import { take } from 'rxjs';

@Component({
  selector: 'sie-preview-account-grid',
  templateUrl: './sie-preview-account-grid.component.html',
  standalone: false,
})
export class SiePreviewAccountGridComponent
  extends GridBaseDirective<ISieAccountMappingDTO>
  implements OnInit, OnChanges
{
  rows = input.required<Array<ISieAccountMappingDTO>>();
  dim = input.required<ISieAccountDimMappingDTO>();
  internalAccounts = input.required<Array<IAccountDTO>>();
  dimInternals = input.required<Array<IAccountDimDTO>>();
  importAccounts = input.required<boolean>();
  importAccountInternal = input.required<boolean>();
  overrideNameConflicts = input.required<boolean>();
  approveEmptyAccountNames = input.required<boolean>();
  sieAccountDimMappingChanged = output<ISieAccountDimMappingDTO>();

  accounts: IAccountDTO[] = [];
  hasConflictRows = computed(() => this.rows().length > 0);
  actionList: ISmallGenericType[] = [];
  constructor() {
    super();
  }

  setActionList() {
    this.actionList = [
      {
        id: 0,
        name: this.translate.instant('core.create'),
      },
      {
        id: 1,
        name: this.translate.instant('common.update'),
      },
      {
        id: 2,
        name: this.translate.instant('common.link'),
      },
    ];
  }

  setGridData(data: ISieAccountMappingDTO[] = this.getGridRows()) {
    this.rowData.next(data);
    this.updateParent();
  }
  getGridRows() {
    return this.rowData.getValue() as ISieAccountMappingDTO[];
  }

  private updateParent() {
    const dim = this.dim();
    dim.accountMappings = this.getGridRows();
    this.sieAccountDimMappingChanged.emit(dim);
  }

  setDimAccounts() {
    this.accounts = [];

    const currentDim = this.dim();
    const internalAccounts = this.internalAccounts();

    if (currentDim.isAccountStd) {
      this.accounts = internalAccounts;
      return;
    }

    const dimInternal = this.dimInternals().find(
      x => x.sysSieDimNr === currentDim.dimNr
    );

    if (dimInternal) {
      this.accounts = internalAccounts.filter(
        x => x.accountDimId === dimInternal.accountDimId
      );
    }
  }

  setActionGridRows() {
    this.rows().forEach(row => {
      this.setRowActionGridRows(row);
    });
  }
  setRowActionGridRows(row: ISieAccountMappingDTO) {
    row.action = 0;
    const acc = this.accounts.find(account => account.accountNr === row.number);
    if (acc) {
      row.accountId = acc.accountId;
      if (row.name !== acc.name) {
        row.action = 1;
      } else {
        row.action = 2;
      }
    }
  }

  override ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Import_Sie,
      'Import.SIE.Account' + this.guid(),
      {
        skipInitialLoad: true,
        skipDefaultToolbar: true,
      }
    );

    this.setActionList();
    this.setDimAccounts();
    this.setActionGridRows();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (
      changes.importAccounts ||
      changes.importAccountInternal ||
      changes.overrideNameConflicts ||
      changes.approveEmptyAccountNames
    ) {
      this.grid?.refreshCells();
    }
  }

  override onGridReadyToDefine(
    grid: GridComponent<ISieAccountMappingDTO>
  ): void {
    super.onGridReadyToDefine(grid);
    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });
    this.translate
      .get([
        'sie.import.preview.account.number',
        'sie.import.preview.account.name',
        'sie.import.preview.action',
        'sie.import.preview.account.match.number',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'number',
          terms['sie.import.preview.account.number'],
          {
            enableHiding: false,
            flex: 1,
            suppressSizeToFit: true,
            minWidth: 100,
            cellClassRules: {
              'error-background-color': (params: CellClassParams) =>
                this.validateImportCellRules(params),
            },
          }
        );
        this.grid.addColumnText(
          'name',
          terms['sie.import.preview.account.name'],
          {
            enableHiding: false,
            flex: 1,
            suppressSizeToFit: true,
            minWidth: 100,
            cellClassRules: {
              'error-background-color': (params: CellClassParams) =>
                this.validateImportCellRules(params),
            },
          }
        );

        this.grid.addColumnSelect(
          'action',
          terms['sie.import.preview.action'],
          this.actionList,
          null,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            enableHiding: false,
            flex: 1,
            suppressSizeToFit: true,
            minWidth: 100,
            cellClassRules: {
              'error-background-color': (params: CellClassParams) =>
                this.validateImportCellRules(params),
            },
            editable: false,
          }
        );
        this.grid.addColumnSelect(
          'accountId',
          terms['sie.import.preview.account.match.number'],
          this.accounts,
          this.matchAccountChanged.bind(this),
          {
            dropDownIdLabel: 'accountId',
            dropDownValueLabel: 'numberName',
            enableHiding: false,
            flex: 1,
            suppressSizeToFit: true,
            minWidth: 100,
            cellClassRules: {
              'error-background-color': (params: CellClassParams) =>
                this.validateImportCellRules(params),
            },
            editable: false,
          }
        );

        this.grid.context.suppressGridMenu = true;
        let length = this.rows().length;
        if (length > 16) length = 16;
        this.grid.setNbrOfRowsToShow(6, length);
        super.finalizeInitGrid();
        this.setGridData(this.rows());
      });
  }
  private validateImportCellRules(params: CellClassParams): boolean {
    const rowData = params.data as ISieAccountMappingDTO;
    if (rowData.name === '' && !this.approveEmptyAccountNames()) return true; // sie name is empty and approveEmptyAccountNames is false
    if (rowData.accountId) {
      const selectedAccount = this.accounts.find(
        account => account.accountId === rowData.accountId
      );
      if (
        selectedAccount &&
        selectedAccount.name !== rowData.name &&
        !this.overrideNameConflicts()
      )
        return true; // sie name is not equal to account name and overrideNameConflicts is false
    }
    return false;
  }

  private onCellValueChanged($event: CellValueChangedEvent) {
    this.updateParent();
  }

  matchAccountChanged(row: any) {
    const rowData = row.data;
    if (!rowData) return;
    this.setRowActionGridRows(rowData);
  }
}
