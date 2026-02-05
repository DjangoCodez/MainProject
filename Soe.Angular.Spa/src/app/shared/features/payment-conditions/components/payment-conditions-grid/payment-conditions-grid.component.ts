import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeModule,
} from '@shared/models/generated-interfaces/Enumerations';
import { IPaymentConditionGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { PaymentConditionsService } from '../../services/payment-conditions.service';
import { UrlHelperService } from '@shared/services/url-params.service';

@Component({
  selector: 'soe-payment-conditions-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PaymentConditionsGridComponent
  extends GridBaseDirective<IPaymentConditionGridDTO, PaymentConditionsService>
  implements OnInit
{
  service = inject(PaymentConditionsService);
  urlHelper = inject(UrlHelperService);

  get isEconomyModule() {
    return this.urlHelper.module === SoeModule.Economy;
  }

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      this.isEconomyModule
        ? Feature.Economy_Preferences_PayCondition
        : Feature.Billing_Preferences_PayCondition,
      this.isEconomyModule
        ? 'Economy.Accounting.PaymentConditions'
        : 'Billing.Invoices.PaymentConditions'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IPaymentConditionGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.code',
        'common.name',
        'common.paymentcondition.days',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 1,
          enableHiding: this.isEconomyModule,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnNumber(
          'days',
          terms['common.paymentcondition.days'],
          {
            flex: 1,
            clearZero: true,
            alignLeft: true,
            enableHiding: this.isEconomyModule,
          }
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
}
