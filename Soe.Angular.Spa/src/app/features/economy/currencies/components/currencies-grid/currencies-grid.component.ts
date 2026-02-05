import { Component, inject, OnInit } from '@angular/core';
import { GridComponent } from '@shared/ui-components/grid/grid.component';
import { take } from 'rxjs/operators';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CurrencyGridDTO } from '../../models/currencies.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CurrenciesService } from '../../services/currencies.service';

@Component({
  selector: 'soe-match-settings-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CurrenciesGridComponent
  extends GridBaseDirective<CurrencyGridDTO>
  implements OnInit
{
  service = inject(CurrenciesService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Preferences_Currency,
      'Economy.Accounting.Currency'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<CurrencyGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.code',
        'core.edit',
        'economy.accounting.currency.rateupdate',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('code', terms['common.code'], {
          enableHiding: false,
          flex: 20,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          enableHiding: false,
          sort: 'asc',
          flex: 40,
        });
        this.grid.addColumnText(
          'intervalName',
          terms['economy.accounting.currency.rateupdate'],
          {
            enableHiding: false,
            flex: 40,
          }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        this.exportFilenameKey.set('economy.accounting.currency.currencies');
        super.finalizeInitGrid();
      });
  }
}
