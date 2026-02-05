import { Component, inject, OnInit } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IPayrollProductDistributionRuleHeadDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of, tap } from 'rxjs';
import { DistributionRuleHeadsForm } from '../../models/distribution-rule-heads-form.model';
import { DistributionRuleService } from '../../services/distribution-rule.service';
import { SharedPlanningPeriodService } from '../../services/shared-planning-period-service';

@Component({
  selector: 'soe-distribution-rules-edit',
  templateUrl: './distribution-rules-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DistributionRulesEditComponent
  extends EditBaseDirective<
    IPayrollProductDistributionRuleHeadDTO,
    DistributionRuleService,
    DistributionRuleHeadsForm
  >
  implements OnInit
{
  service = inject(DistributionRuleService);
  coreService = inject(CoreService);

  private sharedService = inject(SharedPlanningPeriodService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Preferences_TimeSettings_PlanningPeriod);
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: IPayrollProductDistributionRuleHeadDTO) => {
          this.form?.customPatchValue(value);
        })
      )
    );
  }

  override newRecord(): Observable<void> {
    if (this.form?.isCopy) {
      this.form?.customPatchValue(this.form.value, true);
    }
    return of(undefined);
  }

  override performSave() {
    if (!this.form || this.form.invalid || !this.service) return;
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(this.form?.getAllValues()).pipe(
        tap(res => {
          this.updateFormValueAndEmitChange(res, false);
          this.sharedService.setData(this.form?.value);
        })
      )
    );
  }
}
