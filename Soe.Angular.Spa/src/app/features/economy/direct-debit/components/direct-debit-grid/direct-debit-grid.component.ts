import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IInvoiceExportDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';
import { DirectDebitService } from '../../services/direct-debit.service';

@Component({
  selector: 'soe-direct-debit-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DirectDebitGridComponent
  extends GridBaseDirective<IInvoiceExportDTO>
  implements OnInit
{
  service = inject(DirectDebitService);

  ngOnInit(): void {
    this.startFlow(
      Feature.Economy_Export_Invoices_PaymentService,
      'economy.export.paymentservice.paymentservices'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IInvoiceExportDTO>): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'economy.export.paymentservice.batchcid',
        'economy.export.paymentservice.exportdate',
        'economy.export.paymentservice.paymentservice',
        'economy.export.paymentservice.totalamount',
        'economy.export.paymentservice.numberofinvoices',
        'core.createdby',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'batchId',
          terms['economy.export.paymentservice.batchcid'],
          { width: 100 }
        );
        this.grid.addColumnDate(
          'exportDate',
          terms['economy.export.paymentservice.exportdate'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'sysPaymentServiceId',
          terms['economy.export.paymentservice.paymentservice'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'totalAmount',
          terms['economy.export.paymentservice.totalamount'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'numberOfInvoices',
          terms['economy.export.paymentservice.numberofinvoices'],
          { flex: 1 }
        );
        this.grid.addColumnText('createdBy', terms['core.createdby'], {
          flex: 1,
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['çore.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }
}
