import {
  Component,
  OnInit,
  SimpleChanges,
  inject,
  input,
  signal,
  OnChanges,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { CurrenciesForm } from '@features/economy/currencies/models/currencies-form.model';
import { forkJoin, take, tap } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Perform } from '@shared/util/perform.class';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CurrencyRatesEditModal } from '../currency-rates-edit-modal/currency-rates-edit-modal.component';
import { CurrencyRatesEditDialogData } from '../currency-rates-edit-modal/currency-rates-edit-modal.model';
import { CurrencyRateDTO } from '@features/economy/currencies/models/currencies.model';
import { ISysCurrencyDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';

@Component({
  selector: 'soe-currency-rates-grid',
  templateUrl: './currency-rates-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CurrencyRatesGridComponent
  extends GridBaseDirective<CurrencyRateDTO>
  implements OnInit, OnChanges
{
  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  dialogService = inject(DialogService);

  performLoad = new Perform<any>(this.progressService);

  form = input.required<CurrenciesForm>();
  sysCurrency = input.required<ISysCurrencyDTO>();

  sources: SmallGenericType[] = [];
  baseCurrencyCode: string = '';
  otherCurrencyCode: string = '';

  constructor() {
    super();
  }

  override ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Economy_Accounting_Budget_Edit, '', {
      skipInitialLoad: true,
      useLegacyToolbar: true,
      lookups: [this.executeLookups()],
    });

    this.form().controls.currencyRates.valueChanges.subscribe(value => {
      const rows = value as CurrencyRateDTO[];
      this.rowData.next(rows.filter(r => !r.doDelete));
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.sysCurrency) {
      this.otherCurrencyCode = this.sysCurrency().code;
      this.updateGridHeaderNames();
    }
  }

  executeLookups() {
    return this.performLoad.load$(
      forkJoin([
        this.coreService.getTermGroupContent(
          TermGroup.CurrencySource,
          false,
          true,
          true,
          true
        ),
        this.coreService.getBaseCurrency(),
      ]).pipe(
        tap(([sources, baseCurrency]) => {
          this.sources = sources;
          this.baseCurrencyCode = baseCurrency.code;
          this.otherCurrencyCode = this.sysCurrency().code;
        })
      )
    );
  }

  override createLegacyGridToolbar(): void {
    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: 'plus',
          label: 'common.newrow',
          onClick: () => this.editRow(undefined),
          disabled: signal(false),
          hidden: signal(false),
        }),
      ],
    });
  }

  override onFinished(): void {
    this.rowData.next(
      this.form().controls.currencyRates.value as CurrencyRateDTO[]
    );
  }

  rateFromBaseText() {
    return `${this.baseCurrencyCode} / ${this.otherCurrencyCode}`;
  }

  rateToBaseText() {
    return `${this.otherCurrencyCode} / ${this.baseCurrencyCode}`;
  }

  updateGridHeaderNames() {
    if (!this.grid) return;
    const rateToBaseCol = this.grid.getColumnDefByField('rateToBase');
    const rateFromBaseCol = this.grid.getColumnDefByField('rateFrom');
    if (rateFromBaseCol && rateToBaseCol) {
      rateToBaseCol.headerName = this.rateToBaseText();
      rateFromBaseCol.headerName = this.rateFromBaseText();
      this.grid.resetColumns();
    }
  }

  onGridReadyToDefine(grid: GridComponent<CurrencyRateDTO>): void {
    super.onGridReadyToDefine(grid);
    this.grid.setNbrOfRowsToShow(4, 20);

    this.translate
      .get([
        'core.edit',
        'core.delete',
        'common.date',
        'economy.accounting.currency.rate',
        'economy.accounting.currency.source',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnModified('isModified');
        this.grid.addColumnDate('date', terms['common.date'], {
          flex: 40,
          sort: 'asc',
        });
        this.grid.addColumnNumber('rateToBase', this.rateToBaseText(), {
          decimals: 4,
          flex: 30,
        });
        this.grid.addColumnNumber('rateFromBase', this.rateFromBaseText(), {
          decimals: 4,
          flex: 30,
        });
        this.grid.addColumnText(
          'sourceName',
          terms['economy.accounting.currency.source'],
          {
            flex: 30,
          }
        );

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.editRow(row);
          },
        });

        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
        });

        this.grid.context.suppressFiltering = true;
        super.finalizeInitGrid();
      });
  }

  setData(data: CurrencyRateDTO[]): void {
    this.rowData.next(data);
  }

  editRow(row: CurrencyRateDTO | undefined): void {
    const rows = this.form().getRates();
    this.dialogService
      .open(
        CurrencyRatesEditModal,
        new CurrencyRatesEditDialogData(
          row,
          rows.filter(r => !r.doDelete),
          this.sources,
          this.baseCurrencyCode,
          this.otherCurrencyCode
        )
      )
      .afterClosed()
      .subscribe(value => {
        if (value && value.isModified) {
          const updatedRow = { ...row, ...value };
          if (row) {
            rows.splice(rows.indexOf(row), 1, updatedRow);
          } else {
            rows.push(updatedRow);
          }
          this.form().patchRates(rows, true);
        }
      });
  }

  deleteRow(row: CurrencyRateDTO): void {
    if (!row.currencyRateId) {
      const rows = this.grid.getAllRows();
      rows.splice(rows.indexOf(row), 1);
      this.form().patchRates(rows, true);
    } else {
      const rows = this.form().getRates();
      const deleteRow = rows.find(r => r.currencyRateId === row.currencyRateId);
      if (deleteRow) {
        deleteRow.doDelete = true;
        deleteRow.isModified = true;
        this.form().patchRates(rows, true);
      }
    }
  }
}
