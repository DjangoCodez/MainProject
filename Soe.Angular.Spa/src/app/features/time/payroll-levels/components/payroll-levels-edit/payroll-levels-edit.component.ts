import { Component, inject, OnInit } from '@angular/core';
import { PayrollLevelsForm } from '../../models/payroll-levels-form.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IPayrollLevelDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { PayrollLevelsService } from '../../services/payroll-levels.service';

@Component({
  selector: 'soe-payroll-levels-edit',
  templateUrl: './payroll-levels-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PayrollLevelsEditComponent
  extends EditBaseDirective<
    IPayrollLevelDTO,
    PayrollLevelsService,
    PayrollLevelsForm
  >
  implements OnInit
{
  service = inject(PayrollLevelsService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Employee_PayrollLevels);
  }
}
