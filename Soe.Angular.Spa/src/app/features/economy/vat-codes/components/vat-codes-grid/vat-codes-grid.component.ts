import { Component, inject, OnInit } from '@angular/core';
import { GridComponent } from '@shared/ui-components/grid/grid.component';
import { take } from 'rxjs/operators';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { IVatCodeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { VatCodeService } from '../../services/vat-codes.service';

@Component({
  selector: 'soe-vat-codes-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class VatCodesGridComponent
  extends GridBaseDirective<IVatCodeGridDTO, VatCodeService>
  implements OnInit
{
  service = inject(VatCodeService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Preferences_VoucherSettings_VatCodes_Edit,
      'Economy.Accounting.VatCodes'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IVatCodeGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.code',
        'common.name',
        'economy.accounting.vatcode.account',
        'economy.accounting.vatcode.purchasevataccount',
        'common.percentage',
        'common.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('code', terms['common.code'], { flex: 15 });
        this.grid.addColumnText('name', terms['common.name'], { flex: 40 });
        this.grid.addColumnText(
          'account',
          terms['economy.accounting.vatcode.account'],
          { flex: 15 }
        );
        this.grid.addColumnText(
          'purchaseVATAccount',
          terms['economy.accounting.vatcode.purchasevataccount'],
          { flex: 15 }
        );
        this.grid.addColumnNumber('percent', terms['common.percentage'], {
          flex: 15,
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['common.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }
}
