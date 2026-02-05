import { Component, inject, OnInit } from '@angular/core';
import { GridComponent } from '@shared/ui-components/grid/grid.component';
import { take } from 'rxjs/operators';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { MatchCodeGridDTO } from '../../models/match-codes.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { MatchCodeService } from '../../services/match-codes.service';

@Component({
  selector: 'soe-match-settings-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class MatchCodeGridComponent
  extends GridBaseDirective<MatchCodeGridDTO, MatchCodeService>
  implements OnInit
{
  service = inject(MatchCodeService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Preferences_VoucherSettings_MatchCodes,
      'Economy.Accounting.MatchCodes'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<MatchCodeGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.description',
        'common.type',
        'economy.accounting.account',
        'economy.accounting.matchcode.vataccount',
        'core.edit',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          enableHiding: false,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnText('typeName', terms['common.type'], {
          enableHiding: true,
        });
        this.grid.addColumnText(
          'accountNr',
          terms['economy.accounting.account'],
          { enableHiding: true }
        );
        this.grid.addColumnText(
          'vatAccountNr',
          terms['economy.accounting.matchcode.vataccount'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['common.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        this.exportFilenameKey.set('economy.accounting.matchcode.matchcodes');
        super.finalizeInitGrid();
      });
  }
}
