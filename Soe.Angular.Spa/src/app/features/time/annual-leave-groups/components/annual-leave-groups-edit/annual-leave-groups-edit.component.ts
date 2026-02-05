import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  Feature,
  TermGroup_AnnualLeaveGroupType,
} from '@shared/models/generated-interfaces/Enumerations';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { IAnnualLeaveGroupDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DialogData } from '@ui/dialog/models/dialog';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { AnnualLeaveGroupsForm } from '../../models/annual-leave-groups-form.model';
import { AnnualLeaveGroupsService } from '../../services/annual-leave-groups.service';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { TimeDeviationCausesService } from '@features/time/time-deviation-causes/services/time-deviation-causes.service';
import { ProgressOptions } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import { tap } from 'rxjs';
import { AnnualLeaveGroupsTypeModalComponent } from './annual-leave-groups-type-modal/annual-leave-groups-type-modal.component';

@Component({
  selector: 'soe-annual-leave-groups-edit',
  templateUrl: './annual-leave-groups-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AnnualLeaveGroupsEditComponent
  extends EditBaseDirective<
    IAnnualLeaveGroupDTO,
    AnnualLeaveGroupsService,
    AnnualLeaveGroupsForm
  >
  implements OnInit
{
  dialogServiceV2 = inject(DialogService);
  service = inject(AnnualLeaveGroupsService);
  timeDeviationCausesService = inject(TimeDeviationCausesService);
  performTypes = new Perform<SmallGenericType[]>(this.progressService);
  performCauses = new Perform<SmallGenericType[]>(this.progressService);

  typeChosen = signal(false);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Employee_AnnualLeaveGroups, {
      lookups: [this.loadTypes(), this.loadTimeDeviationCauses()],
    });

    this.form?.type.valueChanges.subscribe(value => {
      this.typeChosen.set(value !== null && value !== undefined && value > 0);
    });
  }

  loadTypes() {
    return this.performTypes.load$(this.service.getTypes());
  }

  loadTimeDeviationCauses() {
    return this.performCauses.load$(
      this.timeDeviationCausesService.getTimeDeviationCausesDict(
        true,
        false,
        true
      )
    );
  }

  override performSave(options?: ProgressOptions | undefined): void {
    if (!this.form || !this.service) return;

    const dto = this.form?.getRawValue();

    // Convert rest time minimum from "h:mm" to minutes
    if (dto.ruleRestTimeMinimum.toString().includes(':')) {
      dto.ruleRestTimeMinimum = DateUtil.timeSpanToMinutes(
        dto.ruleRestTimeMinimum
      );
    }

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(dto).pipe(
        tap(value => {
          if (value.success) {
            this.updateFormValueAndEmitChange(value);
          }
        })
      ),
      undefined,
      undefined,
      options
    );
  }

  openTypeDetailsDialog() {
    const dialogOpts = <Partial<DialogData>>{
      size: 'md',
      title: this.performTypes.data?.find(x => x.id === this.form?.value.type)
        ?.name,
      type: <TermGroup_AnnualLeaveGroupType>this.form?.value.type,
    };
    this.dialogServiceV2.open(AnnualLeaveGroupsTypeModalComponent, dialogOpts);
  }
}
