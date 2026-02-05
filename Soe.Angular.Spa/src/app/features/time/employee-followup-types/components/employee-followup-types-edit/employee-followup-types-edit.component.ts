import { Component, OnInit, inject } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { tap } from 'rxjs/operators';
import { EmployeeFollowupTypesService } from '../../services/employee-followup-types.service';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { TermGroup } from '@shared/models/generated-interfaces/Enumerations';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IFollowUpTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

@Component({
  selector: 'soe-employee-followup-types-edit',
  templateUrl: './employee-followup-types-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EmployeeFollowupTypesEditComponent
  extends EditBaseDirective<IFollowUpTypeDTO, EmployeeFollowupTypesService>
  implements OnInit
{
  service = inject(EmployeeFollowupTypesService);
  followUpTypes: SmallGenericType[] = [];
  coreService = inject(CoreService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Time_Employee_FollowUpTypes, {
      lookups: [this.loadFollowupTypes()],
    });
  }

  loadFollowupTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.FollowUpTypeType, false, false)
      .pipe(
        tap(x => {
          this.followUpTypes = x;
        })
      );
  }
}
