import { Component, OnInit, inject } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { VoucherSeriesTypeService } from '../../../../services/voucher-series-type.service';
import { VoucherSeriesTypeDTO } from '../../../../models/voucher-series-type.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-voucher-series-edit',
  templateUrl: './voucher-series-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class VoucherSeriesEditComponent
  extends EditBaseDirective<VoucherSeriesTypeDTO, VoucherSeriesTypeService>
  implements OnInit
{
  service = inject(VoucherSeriesTypeService);

  override ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Economy_Accounting_VoucherSeries_Edit);
  }
}
