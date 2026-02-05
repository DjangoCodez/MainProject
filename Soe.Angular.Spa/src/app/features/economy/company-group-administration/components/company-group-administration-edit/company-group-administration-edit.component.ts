import { Component, OnInit, inject } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { CompanyGroupAdministrationDTO } from '../../models/company-group-administration.model';
import { CompanyGroupAdministrationService } from '../../services/company-group-administration.service';
import { CompanyGroupAdministrationForm } from '../../models/company-group-administration-form.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-company-group-administration-edit',
  templateUrl: './company-group-administration-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CompanyGroupAdministrationEditComponent
  extends EditBaseDirective<
    CompanyGroupAdministrationDTO,
    CompanyGroupAdministrationService,
    CompanyGroupAdministrationForm
  >
  implements OnInit
{
  service = inject(CompanyGroupAdministrationService);
  childCompanies: SmallGenericType[] = [];
  companyGroupMappings: SmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Economy_Accounting_CompanyGroup_Companies_Edit, {
      additionalReadPermissions: [
        Feature.Economy_Accounting_CompanyGroup_Companies_Edit,
      ],
      additionalModifyPermissions: [
        Feature.Economy_Accounting_CompanyGroup_Companies_Edit,
      ],
      lookups: [this.loadChildCompanies(), this.loadCompanyGroupMappings()],
    });
  }

  private loadChildCompanies(): Observable<SmallGenericType[]> {
    return this.service.getChildCompanies().pipe(
      tap(x => {
        this.childCompanies = x;
      })
    );
  }

  private loadCompanyGroupMappings(): Observable<SmallGenericType[]> {
    return this.service.getCompanyGroupMappings(true).pipe(
      tap(x => {
        this.companyGroupMappings = x;
      })
    );
  }
}
