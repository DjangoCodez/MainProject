import { Component, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { IEmployeeCollectiveAgreementGridDTO } from '@shared/models/generated-interfaces/EmployeeCollectiveAgreementDTO';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take, tap } from 'rxjs/operators';
import { CollectiveAgreementsService } from '../../services/collective-agreements.service';
import { Observable } from 'rxjs';
import { SettingsUtil } from '@shared/util/settings-util';
import { Perform } from '@shared/util/perform.class';

@Component({
  selector: 'soe-collective-agreements-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CollectiveAgreementsGridComponent
  extends GridBaseDirective<
    IEmployeeCollectiveAgreementGridDTO,
    CollectiveAgreementsService
  >
  implements OnInit
{
  useAnnualLeave = signal(false);

  service = inject(CollectiveAgreementsService);
  coreService = inject(CoreService);

  performLoad = new Perform<any>(this.progressService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_EmployeeCollectiveAgreements,
      'Time.Employee.EmployeeCollectiveAgreement'
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

  override onGridReadyToDefine(
    grid: GridComponent<IEmployeeCollectiveAgreementGridDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.active',
        'common.code',
        'common.name',
        'common.description',
        'common.externalcode',
        'time.employee.employeegroup.employeegroup',
        'time.employee.payrollgroup.payrollgroup',
        'time.employee.vacationgroup.vacationgroup',
        'time.employee.annualleavegroup',
        'time.employee.employeetemplate.employeetemplates',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnActive('state', terms['common.active'], {
          idField: 'employeeCollectiveAgreementId',
        });
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 5,
          enableHiding: false,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 10,
          enableHiding: true,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 20,
          enableHiding: true,
        });
        this.grid.addColumnText('externalCode', terms['common.externalcode'], {
          flex: 5,
          enableHiding: true,
        });
        this.grid.addColumnText(
          'employeeGroupName',
          terms['time.employee.employeegroup.employeegroup'],
          { flex: 15, enableHiding: true }
        );
        this.grid.addColumnText(
          'payrollGroupName',
          terms['time.employee.payrollgroup.payrollgroup'],
          { flex: 15, enableHiding: true }
        );
        this.grid.addColumnText(
          'vacationGroupName',
          terms['time.employee.vacationgroup.vacationgroup'],
          { flex: 15, enableHiding: true }
        );
        if (this.useAnnualLeave()) {
          this.grid.addColumnText(
            'annualLeaveGroupName',
            terms['time.employee.annualleavegroup'],
            { flex: 15, enableHiding: true }
          );
        }
        this.grid.addColumnText(
          'employeeTemplateNames',
          terms['time.employee.employeetemplate.employeetemplates'],
          { flex: 15, enableHiding: true }
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
