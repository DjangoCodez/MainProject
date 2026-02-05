import { Component, OnInit, inject } from '@angular/core';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { SchoolHolidayService } from '../../services/school-holiday.service';
import { ISchoolHolidayDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { SchoolHolidayForm } from '../../models/school-holiday-form.model';

@Component({
  selector: 'soe-school-holiday-edit',
  templateUrl: './school-holiday-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SchoolHolidayEditComponent
  extends EditBaseDirective<
    ISchoolHolidayDTO,
    SchoolHolidayService,
    SchoolHolidayForm
  >
  implements OnInit
{
  service = inject(SchoolHolidayService);
  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Manage_Preferences_Registry_SchoolHoliday);
  }
}
