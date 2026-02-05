import { Component, inject, OnInit } from '@angular/core';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SkillTypesService } from '../../services/skill-types.service';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { SkillTypesForm } from '../../models/skill-types-form.model';
import { ISkillTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-skill-types-edit',
  templateUrl: './skill-types-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SkillTypesEditComponent
  extends EditBaseDirective<ISkillTypeDTO, SkillTypesService, SkillTypesForm>
  implements OnInit
{
  service = inject(SkillTypesService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Preferences_ScheduleSettings_SkillType_Edit);
  }
}
