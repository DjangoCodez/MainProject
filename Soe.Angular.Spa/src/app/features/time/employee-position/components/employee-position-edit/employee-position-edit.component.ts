import { Component, OnInit, inject } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  IPositionDTO,
  IPositionSkillDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressOptions } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { PositionsService } from '@src/app/features/manage/positions/services/positions.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, Observable, of, tap } from 'rxjs';
import { EmployeePositionForm } from '../../models/employee-position-form.model';
import { EmployeePositionService } from '../../services/employee-position.service';

@Component({
  selector: 'soe-employee-position-edit',
  templateUrl: './employee-position-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EmployeePositionEditComponent
  extends EditBaseDirective<
    IPositionDTO,
    EmployeePositionService,
    EmployeePositionForm
  >
  implements OnInit
{
  service = inject(EmployeePositionService);
  positionsService = inject(PositionsService);
  performSysposition = new Perform<SmallGenericType[]>(this.progressService);
  selectedSkills = new BehaviorSubject<IPositionSkillDTO[]>([]);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Employee_Positions_Edit, {
      lookups: [this.loadSysposition()],
    });
  }

  loadSysposition(): Observable<SmallGenericType[]> {
    return this.performSysposition.load$(
      this.positionsService.getSysPositionsDict(
        SoeConfigUtil.sysCountryId,
        SoeConfigUtil.languageId,
        true
      )
    );
  }

  override performSave(options?: ProgressOptions): void {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service
        .save(this.form?.getRawValue())
        .pipe(tap(this.updateFormValueAndEmitChange)),
      undefined,
      undefined,
      options
    );
  }

  override newRecord(): Observable<void> {
    this.setupFormLogic();
    return of(undefined);
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value, true).pipe(
        tap(value => {
          this.form?.reset(value);
          this.selectedSkills.next(value.positionSkills);
          if (value.sysPositionId) {
            this.form?.patchValue({
              isLinked: true,
            });
          }
          this.setupFormLogic();
        })
      )
    );
  }

  setupFormLogic(): void {
    this.form?.setInitialDisabledState(this.flowHandler.modifyPermission());
  }

  // EVENTS

  isLinkedChange(event: boolean) {
    this.form?.eventSetDisabledState(
      event,
      this.flowHandler.modifyPermission()
    );
  }

  protected rowSelectionChanged(rows: IPositionSkillDTO[]): void {
    this.form?.customPositionSkillsPatchValue(rows);
  }
}
