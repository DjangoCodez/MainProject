import { Component, inject, OnInit } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ContractGroupDTO } from '../../models/contract-groups.model';
import { ContractGroupsForm } from '../../models/contract-groups-form.model';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { tap } from 'rxjs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ContractGroupsService } from '../../services/contract-groups.service';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-contract-groups-edit',
  templateUrl: './contract-groups-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ContractGroupsEditComponent
  extends EditBaseDirective<
    ContractGroupDTO,
    ContractGroupsService,
    ContractGroupsForm
  >
  implements OnInit
{
  service = inject(ContractGroupsService);
  coreService = inject(CoreService);
  periods: ISmallGenericType[] = [];
  priceManagementTypes: ISmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Billing_Contract_Groups_Edit, {
      lookups: [this.loadPeriods(), this.loadPriceManagementTypes()],
    });
  }

  private loadPeriods() {
    return this.coreService
      .getTermGroupContent(TermGroup.ContractGroupPeriod, false, false)
      .pipe(tap(x => (this.periods = x)));
  }

  private loadPriceManagementTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.ContractGroupPriceManagement, false, false)
      .pipe(
        tap(x => {
          this.priceManagementTypes = x;
        })
      );
  }
}
