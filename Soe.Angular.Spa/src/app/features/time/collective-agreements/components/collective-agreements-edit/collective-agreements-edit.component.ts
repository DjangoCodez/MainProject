import { Component, OnInit, inject, signal } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { CollectiveAgreementsService } from '../../services/collective-agreements.service';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { TimeService } from '../../../services/time.service';
import { CollectiveAgreementsForm } from '../../models/collective-agreements-form.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IEmployeeCollectiveAgreementDTO } from '@shared/models/generated-interfaces/EmployeeCollectiveAgreementDTO';
import { AnnualLeaveGroupsService } from '@features/time/annual-leave-groups/services/annual-leave-groups.service';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';

@Component({
  selector: 'soe-collective-agreements-edit',
  templateUrl: './collective-agreements-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CollectiveAgreementsEditComponent
  extends EditBaseDirective<
    IEmployeeCollectiveAgreementDTO,
    CollectiveAgreementsService,
    CollectiveAgreementsForm
  >
  implements OnInit
{
  useAnnualLeave = signal(false);

  service = inject(CollectiveAgreementsService);
  timeService = inject(TimeService);
  annualLeaveGroupsService = inject(AnnualLeaveGroupsService);
  coreService = inject(CoreService);

  performLoad = new Perform<any>(this.progressService);

  employeeGroups: SmallGenericType[] = [];
  payrollGroups: SmallGenericType[] = [];
  vacationGroups: SmallGenericType[] = [];
  annualLeaveGroups: SmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Employee_EmployeeCollectiveAgreements, {
      lookups: [
        this.loadEmployeeGroups(),
        this.loadPayrollGroups(),
        this.loadVacationGroups(),
        this.loadAnnualLeaveGroups(),
      ],
    });
  }

  loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.form?.reset(value);
        })
      )
    );
  }

  override loadCompanySettings(): Observable<void> {
    const settingTypes: number[] = [CompanySettingType.UseAnnualLeave];
    return this.performLoad.load$(
      this.coreService.getCompanySettings(settingTypes).pipe(
        tap(x => {
          this.useAnnualLeave.set(
            SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.UseAnnualLeave
            )
          );
        })
      )
    );
  }

  loadEmployeeGroups(): Observable<SmallGenericType[]> {
    return this.timeService.getEmployeeGroups(false).pipe(
      tap(e => {
        this.employeeGroups = e;
      })
    );
  }

  loadPayrollGroups(): Observable<SmallGenericType[]> {
    return this.timeService.getPayrollGroups(false).pipe(
      tap(p => {
        this.payrollGroups = p;
      })
    );
  }

  loadVacationGroups(): Observable<SmallGenericType[]> {
    return this.timeService.getVacationGroups(false).pipe(
      tap(v => {
        this.vacationGroups = v;
      })
    );
  }

  loadAnnualLeaveGroups(): Observable<SmallGenericType[]> {
    return this.annualLeaveGroupsService.getAnnualLeaveGroups(true).pipe(
      tap(a => {
        this.annualLeaveGroups = a;
      })
    );
  }
}
