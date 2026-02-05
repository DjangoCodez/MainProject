import { Component, OnInit, inject } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { EndReasonsService } from '../../services/end-reasons.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IEndReasonDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericForm } from '@shared/components/billing/import-dynamic/import-dynamic/import-dynamic.model';

@Component({
  selector: 'soe-end-reasons-edit',
  templateUrl: './end-reasons-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EndReasonsEditComponent
  extends EditBaseDirective<IEndReasonDTO, EndReasonsService>
  implements OnInit
{
  service = inject(EndReasonsService);

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Time_Employee_EndReasons);

    // Filter out sysendReasons from record navigator, their id is less than 0
    if (this.form?.records) {
      this.form.records = this.form.records.filter(record => record.id > 0);
    }
  }
}
