import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IPaymentMethodSupplierGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { SupplierPaymentMethodsService } from '../../services/supplier-payment-methods.service';

@Component({
  selector: 'soe-supplier-payment-methods-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SupplierPaymentMethodsGridComponent
  extends GridBaseDirective<
    IPaymentMethodSupplierGridDTO,
    SupplierPaymentMethodsService
  >
  implements OnInit
{
  service = inject(SupplierPaymentMethodsService);
  progressService = inject(ProgressService);
  performLoad = new Perform<IPaymentMethodSupplierGridDTO[]>(
    this.progressService
  );

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Preferences_SuppInvoiceSettings_PaymentMethods,
      'Economy.Supplier.PaymentMethods'
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<IPaymentMethodSupplierGridDTO>
  ) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'common.name',
        'economy.common.paymentmethods.payerbankid',
        'economy.common.paymentmethods.exporttype',
        'economy.common.paymentmethods.paymentnr',
        'economy.common.paymentmethods.customernr',
        'economy.common.paymentmethods.accountnr',
        'economy.supplier.invoice.currencycode',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name']);
        this.grid.addColumnText(
          'sysPaymentMethodName',
          terms['economy.common.paymentmethods.exporttype'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'paymentNr',
          terms['economy.common.paymentmethods.paymentnr'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'customerNr',
          terms['economy.common.paymentmethods.customernr'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'accountNr',
          terms['economy.common.paymentmethods.accountnr'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'payerBankId',
          terms['economy.common.paymentmethods.payerbankid'],
          { flex: 1, enableHiding: true }
        );
        this.grid.addColumnText(
          'currencyCode',
          terms['economy.supplier.invoice.currencycode'],
          { flex: 1, enableHiding: true }
        );

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: {
      addEmptyRow: boolean;
      includePaymentInformationRows: boolean;
      includeAccount: boolean;
      onlyCashSales: boolean;
    }
  ): Observable<IPaymentMethodSupplierGridDTO[]> {
    return super.loadData(id, {
      addEmptyRow: false,
      includePaymentInformationRows: true,
      includeAccount: true,
      onlyCashSales: false,
    });
  }
}
