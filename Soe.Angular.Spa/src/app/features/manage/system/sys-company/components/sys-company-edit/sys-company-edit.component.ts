import { Component, inject, OnInit } from '@angular/core';
import {
  SysCompanyBankAccountDTO,
  SysCompanyDTO,
  SysCompanySettingDTO,
  SysCompanyUniqueValueDTO,
} from 'src/app/features/manage/models/sysCompany.model';
import { SysCompanyService } from '../../services/sys-company.service';
import { SysCompanyForm } from '../../models/sys-company-form.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-sys-company-edit',
  templateUrl: './sys-company-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SysCompanyEditComponent
  extends EditBaseDirective<SysCompanyDTO, SysCompanyService, SysCompanyForm>
  implements OnInit
{
  readonly service = inject(SysCompanyService);

  protected settingRows = new BehaviorSubject<SysCompanySettingDTO[]>([]);
  protected bankAccounts = new BehaviorSubject<SysCompanyBankAccountDTO[]>([]);
  protected uniqueValues = new BehaviorSubject<SysCompanyUniqueValueDTO[]>([]);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Manage_System, { skipDefaultToolbar: true });
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(company => {
          this.form?.customPatch(company);
          this.settingRows.next(company.sysCompanySettingDTOs);
          this.bankAccounts.next(company.sysCompanyBankAccountDTOs || []);
          this.uniqueValues.next(company.sysCompanyUniqueValueDTOs || []);
        })
      )
    );
  }

  override triggerDelete(): void {
    this.messageboxService.warning('core.warning', 'Delete not allowed');
  }
}
