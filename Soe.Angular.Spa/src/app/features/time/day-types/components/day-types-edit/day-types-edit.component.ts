import { Component, OnInit, inject } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { Observable } from 'rxjs';
import { DayTypesService } from '../../services/day-types.service';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { DayTypesForm } from '../../models/day-types-form.model';
import { IDayTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-day-types-edit',
  templateUrl: './day-types-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class DayTypesEditComponent
  extends EditBaseDirective<IDayTypeDTO, DayTypesService, DayTypesForm>
  implements OnInit
{
  service = inject(DayTypesService);
  performWeekDays = new Perform<SmallGenericType[]>(this.progressService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Preferences_ScheduleSettings_DayTypes_Edit, {
      lookups: this.loadDayOfWeeks(),
      // finished: () => {
      //   console.log(this.form?.gridData);
      // },
    });
  }

  loadDayOfWeeks(): Observable<SmallGenericType[]> {
    return this.performWeekDays.load$(this.service.getDaysOfWeek(true), {
      showDialog: false,
    });
  }

  // override newRecord(): Observable<void> {
  //   if (this.form?.isCopy && this.form?.additionalPropsOnCopy) {
  //     console.log(this.form?.additionalPropsOnCopy);
  //   }
  //   return of();
  // }

  // override copy() {
  //   const additionalProps = {
  //     label: 'Additional data sent from edit component',
  //     date: new Date(),
  //   };
  //   super.copy(additionalProps);
  // }
}
