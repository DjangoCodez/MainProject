import { Component, Input, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { BehaviorSubject, take } from 'rxjs';
import { IVoucherRowHistoryViewDTO } from '@shared/models/generated-interfaces/VoucherRowHistoryDTOs';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-voucher-edit-history-grid',
  templateUrl: './voucher-edit-history-grid.component.html',
  styleUrls: ['./voucher-edit-history-grid.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class VoucherEditHistoryGridComponent
  extends GridBaseDirective<IVoucherRowHistoryViewDTO>
  implements OnInit
{
  @Input() rows = new BehaviorSubject<IVoucherRowHistoryViewDTO[]>([]);
  flowHandler = inject(FlowHandlerService);

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Accounting_Vouchers_Edit,
      'VoucherEditHistoryGrid',
      { skipInitialLoad: true }
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IVoucherRowHistoryViewDTO>) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'economy.accounting.voucher.historytype',
        'economy.accounting.voucher.historyfield',
        'economy.accounting.voucher.historychange',
        'economy.accounting.voucher.historydate',
        'economy.accounting.voucher.historytime',
        'common.user',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'eventType',
          terms['economy.accounting.voucher.historytype'],
          {
            flex: 1,
            editable: false,
          }
        );
        this.grid.addColumnText(
          'fieldModified',
          terms['economy.accounting.voucher.historyfield'],
          {
            flex: 1,
            editable: false,
          }
        );
        this.grid.addColumnText(
          'eventText',
          terms['economy.accounting.voucher.historychange'],
          {
            flex: 1,
            editable: false,
          }
        );
        this.grid.addColumnText(
          'dateTime',
          terms['economy.accounting.voucher.historydate'],
          {
            flex: 1,
            editable: false,
          }
        );
        this.grid.addColumnText(
          'time',
          terms['economy.accounting.voucher.historytime'],
          {
            flex: 1,
            editable: false,
          }
        );
        this.grid.addColumnText('userName', terms['common.user'], {
          flex: 1,
          editable: false,
        });

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid();
      });
  }
}
