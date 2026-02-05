import { Component, inject, OnInit, signal } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  Feature,
  CompanySettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { TimeScheduleTypeService } from '../../services/time-schedule-type.service';
import { ITimeScheduleTypeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TimeScheduleTypeForm } from '../../models/time-schedule-type-form.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Observable, tap } from 'rxjs';
import { SettingsUtil } from '@shared/util/settings-util';

@Component({
  selector: 'soe-time-schedule-type-edit',
  templateUrl: './time-schedule-type-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeScheduleTypeEditComponent
  extends EditBaseDirective<
    ITimeScheduleTypeDTO,
    TimeScheduleTypeService,
    TimeScheduleTypeForm
  >
  implements OnInit
{
  service = inject(TimeScheduleTypeService);
  private readonly coreService = inject(CoreService);

  private previousName: string = '';

  hideShowInTerminal = signal(false);

  timeDeviationCauses: SmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Preferences_TimeSettings_TimeScheduleType_Edit,
      {
        lookups: [this.loadTimeDeviationCauses()],
      }
    );
    this.setupFormLogic();
  }

  override loadCompanySettings(): Observable<any> {
    return this.coreService
      .getCompanySettings([
        CompanySettingType.PossibilityToRegisterAdditionsInTerminal,
      ])
      .pipe(
        tap(x => {
          this.hideShowInTerminal.set(
            !SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.PossibilityToRegisterAdditionsInTerminal
            )
          );
        })
      );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: ITimeScheduleTypeDTO) => {
          this.form?.customPatchValue(value);
          this.setupFormLogic();
        })
      )
    );
  }

  private loadTimeDeviationCauses(): Observable<SmallGenericType[]> {
    return this.performLoadData.load$(
      this.service
        .getTimeDeviationCausesDict(true, true, false)
        .pipe(tap(x => (this.timeDeviationCauses = x)))
    );
  }

  //SETUP FORM LOGIC

  private setupFormLogic() {
    this.resetFormLogic();
    this.setupIsAllLogic();
    this.setupIsNotScheduleTimeLogic();
    this.setupUseScheduleTimeFactorLogic();
  }

  private resetFormLogic() {
    this.form?.useScheduleTimeFactor.enable();
    this.form?.isNotScheduleTime.enable();
  }

  private setupIsAllLogic() {
    const isAll = this.form?.isAll.value;
    if (isAll) {
      this.previousName = this.form?.name.value || '';
      this.form?.name.setValue(this.translate.instant('common.all'));
      this.form?.name.disable();
    }
  }

  private setupIsNotScheduleTimeLogic() {
    const isNotScheduleTime = this.form?.isNotScheduleTime.value;
    if (isNotScheduleTime) {
      this.form?.useScheduleTimeFactor.setValue(false);
      this.form?.useScheduleTimeFactor.disable();
    }
  }

  private setupUseScheduleTimeFactorLogic() {
    const useScheduleTimeFactor = this.form?.useScheduleTimeFactor.value;
    if (useScheduleTimeFactor) {
      this.form?.isNotScheduleTime.setValue(false);
      this.form?.isNotScheduleTime.disable();
    }
  }

  //EVENTS

  onIsAllChanged(event: boolean) {
    if (event) {
      this.previousName = this.form?.name.value || '';
      this.form?.name.setValue(this.translate.instant('common.all'));
      this.form?.name.disable();
    } else {
      if (this.flowHandler.modifyPermission()) this.form?.name.enable();
      this.form?.name.setValue(this.previousName);
    }
  }

  onIsNotScheduleTimeChanged(event: boolean) {
    if (event) {
      this.form?.useScheduleTimeFactor.setValue(false);
      this.form?.useScheduleTimeFactor.disable();
    } else if (this.flowHandler.modifyPermission()) {
      this.form?.useScheduleTimeFactor.enable();
    }
  }

  onUseScheduleTimeFactorChanged(event: boolean) {
    if (event) {
      this.form?.isNotScheduleTime.setValue(false);
      this.form?.isNotScheduleTime.disable();
    } else if (this.flowHandler.modifyPermission()) {
      this.form?.isNotScheduleTime.enable();
    }
  }
}
