import { Component, OnInit, inject } from '@angular/core';
import { tap } from 'rxjs/operators';
import { SkillsService } from '../../services/skills.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { Perform } from '@shared/util/perform.class';
import { SkillTypesService } from '../../../skill-types/services/skill-types.service';
import { ISkillDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SkillsForm } from '../../models/skill-form.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-skills-edit',
  templateUrl: './skills-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SkillsEditComponent
  extends EditBaseDirective<ISkillDTO, SkillsService, SkillsForm>
  implements OnInit
{
  service = inject(SkillsService);
  private readonly skillTypesService = inject(SkillTypesService);
  private readonly performSkillTypes = new Perform<SmallGenericType[]>(
    this.progressService
  );

  skillTypes: SmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Preferences_ScheduleSettings_Skill_Edit, {
      lookups: this.loadSkillTypes(),
    });
  }

  private loadSkillTypes() {
    return this.performSkillTypes.load$(
      this.skillTypesService.getSkillTypesDict(true).pipe(
        tap(x => {
          this.skillTypes = x;
        })
      )
    );
  }
}
