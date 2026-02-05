import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IPaymentMethodCustomerGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { CustomerPaymentMethodsService } from '../../../services/customer-payment-methods.service';

@Component({
  selector: 'soe-customer-payment-methods-grid',
  templateUrl:
    '../../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CustomerPaymentMethodsGridComponent
  extends GridBaseDirective<
    IPaymentMethodCustomerGridDTO,
    CustomerPaymentMethodsService
  >
  implements OnInit
{
  service = inject(CustomerPaymentMethodsService);
  progressService = inject(ProgressService);
  performLoad = new Perform<IPaymentMethodCustomerGridDTO[]>(
    this.progressService
  );
  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Preferences_CustInvoiceSettings_PaymentMethods,
      'Economy.Customer.PaymentMethods'
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<IPaymentMethodCustomerGridDTO>
  ) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'common.name',
        'economy.common.paymentmethods.importtype',
        'economy.common.paymentmethods.paymentnr',
        'economy.common.paymentmethods.accountnr',
        'economy.common.paymentmethods.useincashsales',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 2,
          enableHiding: false,
        });
        this.grid.addColumnText(
          'sysPaymentMethodName',
          terms['economy.common.paymentmethods.importtype'],
          { flex: 2, enableHiding: false }
        );
        this.grid.addColumnText(
          'paymentNr',
          terms['economy.common.paymentmethods.paymentnr'],
          { flex: 2, enableHiding: false }
        );
        this.grid.addColumnText(
          'accountNr',
          terms['economy.common.paymentmethods.accountnr'],
          { flex: 2, enableHiding: true }
        );

        this.grid.addColumnBool(
          'useInCashSales',
          terms['economy.common.paymentmethods.useincashsales'],
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
  ): Observable<IPaymentMethodCustomerGridDTO[]> {
    return super.loadData(id, {
      addEmptyRow: false,
      includePaymentInformationRows: true,
      includeAccount: true,
      onlyCashSales: false,
    });
  }
}
