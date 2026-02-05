import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IGrossProfitCodeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { GrossProfitCodesService } from '../../services/gross-profit-codes.service';

@Component({
  selector: 'soe-gross-profit-codes-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class GrossProfitCodesGridComponent
  extends GridBaseDirective<IGrossProfitCodeGridDTO, GrossProfitCodesService>
  implements OnInit
{
  service = inject(GrossProfitCodesService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Preferences_VoucherSettings_GrossProfitCodes,
      'Economy.Accounting.GrossProfitCodes'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IGrossProfitCodeGridDTO>) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'common.code',
        'common.name',
        'economy.accounting.grossprofitcode.accountyear',
        'common.description',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 4,
          enableHiding: true,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 4,
          enableHiding: false,
        });
        this.grid.addColumnText(
          'accountYear',
          terms['economy.accounting.grossprofitcode.accountyear'],
          {
            flex: 4,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 6,
          enableHiding: true,
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }
}
