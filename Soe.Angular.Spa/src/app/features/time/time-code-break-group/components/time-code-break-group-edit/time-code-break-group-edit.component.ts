import { Component, inject, OnInit } from '@angular/core';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { TimeCodeBreakGroupService } from '../../services/time-code-break-group.service';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ITimeCodeBreakGroupDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

@Component({
  selector: 'soe-time-code-break-group-edit',
  templateUrl: './time-code-break-group-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeCodeBreakGroupEditComponent
  extends EditBaseDirective<ITimeCodeBreakGroupDTO, TimeCodeBreakGroupService>
  implements OnInit
{
  service = inject(TimeCodeBreakGroupService);

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Preferences_TimeSettings_TimeCodeBreakGroup_Edit
    );
  }
}
