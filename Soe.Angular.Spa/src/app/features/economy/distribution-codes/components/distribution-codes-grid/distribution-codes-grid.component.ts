import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IDistributionCodeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { DistributionCodesService } from '../../services/distribution-codes.service';

@Component({
  selector: 'soe-distribution-codes-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DistributionCodesGridComponent
  extends GridBaseDirective<IDistributionCodeGridDTO, DistributionCodesService>
  implements OnInit
{
  service = inject(DistributionCodesService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Preferences_VoucherSettings_DistributionCodes,
      'Economy.Accounting.DistributionCode'
    );
  }

  onGridReadyToDefine(grid: GridComponent<IDistributionCodeGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'economy.accounting.distributioncode.numberofperiods',
        'common.type',
        'common.validfrom',
        'economy.accounting.distributioncode.subtype',
        'economy.accounting.distributioncode.sublevel',
        'common.accountdim',
        'economy.accounting.distributioncode.openinghours',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'noOfPeriods',
          terms['economy.accounting.distributioncode.numberofperiods'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText('type', terms['common.type'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'typeOfPeriod',
          terms['economy.accounting.distributioncode.subtype'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'subLevel',
          terms['economy.accounting.distributioncode.sublevel'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText('accountDim', terms['common.accountdim'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'openingHour',
          terms['economy.accounting.distributioncode.openinghours'],
          { flex: 1, enableHiding: false }
        );
        this.grid.addColumnDate('fromDate', terms['common.validfrom'], {
          flex: 1,
          enableHiding: false,
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
