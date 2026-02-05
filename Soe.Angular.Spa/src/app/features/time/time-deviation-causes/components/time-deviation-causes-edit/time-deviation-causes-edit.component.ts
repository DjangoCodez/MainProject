import { Component, inject, OnInit } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { TimeboxValue } from '@ui/forms/timebox/timebox.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { TimeDeviationCausesService } from '../../services/time-deviation-causes.service';
import { TimeDeviationCausesForm } from '../../models/time-deviation-causes-form.model';
import { ITimeDeviationCauseDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { Observable, of, tap } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';

@Component({
  selector: 'soe-time-deviation-causes-edit',
  templateUrl: './time-deviation-causes-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeDeviationCausesEditComponent
  extends EditBaseDirective<
    ITimeDeviationCauseDTO,
    TimeDeviationCausesService,
    TimeDeviationCausesForm
  >
  implements OnInit
{
  service = inject(TimeDeviationCausesService);
  coreService = inject(CoreService);
  types: SmallGenericType[] = [];
  timeCodes: SmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_TimeSettings_TimeDeviationCause_Edit,
      {
        lookups: [this.loadTimeCodes()],
      }
    );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: ITimeDeviationCauseDTO) => {
          this.form?.reset(value);
          this.setupFormLogic(); // Setup form logic when loading
          this.form?.setInitialFormattedValues();
        })
      )
    );
  }

  override newRecord(): Observable<void> {
    this.setupFormLogic(); // Setup form logic when copy
    return of(undefined);
  }

  private loadTimeCodes(): Observable<SmallGenericType[]> {
    return this.performLoadData.load$(
      this.service
        .getTimeCodesDict(true, false, true)
        .pipe(tap(x => (this.timeCodes = x)))
    );
  }

  private setupFormLogic() {
    // Form states
    this.form?.setupPlannedAbsenceCheckboxesLogic();
  }

  // EVENTS

  onPlannedAbsenceCheckboxChanged(event: boolean) {
    this.form?.disableFieldsAccordingToPlannedAbsenceCheckbox(
      event,
      this.flowHandler.modifyPermission()
    );
  }

  changeCauseOutsideOfPlannedAbsenceChanged(value: TimeboxValue) {
    this.form?.changeCauseOutsideOfPlannedAbsenceChanged();
  }

  changeCauseInsideOfPlannedAbsenceChanged(value: TimeboxValue) {
    this.form?.changeCauseInsideOfPlannedAbsenceChanged();
  }

  adjustTimeOutsideOfPlannedAbsenceChanged(value: TimeboxValue) {
    this.form?.adjustTimeOutsideOfPlannedAbsenceChanged();
  }

  adjustTimeInsideOfPlannedAbsenceChanged(value: TimeboxValue) {
    this.form?.adjustTimeInsideOfPlannedAbsenceChanged();
  }
}
