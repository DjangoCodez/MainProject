import { Component, OnInit, inject } from '@angular/core';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { Observable, of, tap } from 'rxjs';
import { OpeningHoursService } from '../../services/opening-hours.service';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IOpeningHoursDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { OpeningHoursForm } from '../../models/opening-hours-form.model';

@Component({
  selector: 'soe-opening-hours-edit',
  templateUrl: './opening-hours-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class OpeningHoursEditComponent
  extends EditBaseDirective<
    IOpeningHoursDTO,
    OpeningHoursService,
    OpeningHoursForm
  >
  implements OnInit
{
  service = inject(OpeningHoursService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Manage_Preferences_Registry_OpeningHours, {});
  }

  override loadData(): Observable<void> {
    return super.loadData().pipe(
      tap(() => {
        this.setupDisabledStates();
      })
    );
  }

  override newRecord(): Observable<void> {
    this.setupDisabledStates(); // To make sure disabled states are set up when copying
    return of(undefined);
  }

  setupDisabledStates() {
    this.form?.setupDisabledStates();
  }

  // EVENTS

  onSelectWeekday(value: number) {
    this.form?.disableSpecificDate(value, this.flowHandler.modifyPermission());
  }
}
