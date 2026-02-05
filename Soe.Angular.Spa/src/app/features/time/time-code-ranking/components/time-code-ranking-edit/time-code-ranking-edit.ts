import { Component, inject, OnInit } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { TimeCodeRankingForm } from '../../models/time-code-ranking-form.model';
import { TimeCodeRankingService } from '../../services/time-code-ranking';
import {
  Feature,
  SoeTimeCodeType,
  TermGroup,
  TermGroup_TimeCodeClassification,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  ITimeCodeDTO,
  ITimeCodeRankingDTO,
  ITimeCodeRankingGroupDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service';
import { Observable, of, tap } from 'rxjs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ValidationHandler } from '@shared/handlers';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { CrudActionTypeEnum } from '@shared/enums/action.enum';
import { Perform } from '@shared/util/perform.class';

@Component({
  selector: 'soe-time-code-ranking-edit',
  standalone: false,
  templateUrl: './time-code-ranking-edit.html',
  styles: ``,
  providers: [FlowHandlerService, ToolbarService],
})
export class TimeCodeRankingEdit
  extends EditBaseDirective<
    ITimeCodeRankingGroupDTO,
    TimeCodeRankingService,
    TimeCodeRankingForm
  >
  implements OnInit
{
  form: any;
  leftTimeCodes: SmallGenericType[] = [];
  rightTimeCodes: SmallGenericType[] = [];
  rightTimeCodesAll: SmallGenericType[] = [];
  rightTimeCodesNotInUse: SmallGenericType[] = [];
  addedMisssingTimeCodes: number[] = [];
  operatorTypes: SmallGenericType[] = [];
  operatorTypesFiltered: SmallGenericType[] = [];
  filteredLeftTimeCodes: SmallGenericType[][] = [];
  performLoadData = new Perform<ITimeCodeRankingGroupDTO>(this.progressService);
  performSaveData = new Perform<TimeCodeRankingService>(this.progressService);
  coreService = inject(CoreService);
  service = inject(TimeCodeRankingService);
  validationHandler = inject(ValidationHandler);
  timeCodeRankingGroup: ITimeCodeRankingGroupDTO =
    {} as ITimeCodeRankingGroupDTO;
  timeCodeRankingGroupInv: ITimeCodeRankingGroupDTO =
    {} as ITimeCodeRankingGroupDTO;

  ngOnInit() {
    super.ngOnInit();
    if (!this.form) {
      this.form = new TimeCodeRankingForm({
        validationHandler: this.validationHandler,
        element: {} as ITimeCodeRankingGroupDTO,
      });
    }

    this.startFlow(Feature.Time_Preferences_TimeSettings_TimeCodeRanking, {
      useLegacyToolbar: true,
      lookups: [this.getTimeCodes(), this.loadSelectionTypes()],
      onGridReadyToDefine: () => {
        this.loadData().subscribe();
      },
    });
  }
  loadData(id?: number) {
    const timeCodeRankingGroupId = id ?? this.form?.getIdControl()?.value ?? 0;
    if (timeCodeRankingGroupId == 0 || timeCodeRankingGroupId === undefined)
      return of(null);

    return this.service.get(timeCodeRankingGroupId).pipe(
      tap((result: any) => {
        if (!this.form) {
          this.form = new TimeCodeRankingForm({
            validationHandler: this.validationHandler,
            element: {} as ITimeCodeRankingGroupDTO,
          });
        }
        this.timeCodeRankingGroup = {
          ...result,
          timeCodeRankings:
            result.timeCodeRankings?.filter(
              (r: ITimeCodeRankingDTO) => r.operatorType === 0
            ) || [],
        };
        this.timeCodeRankingGroupInv = {
          ...result,
          timeCodeRankings:
            result.timeCodeRankings?.filter(
              (r: ITimeCodeRankingDTO) => r.operatorType === 1
            ) || [],
        };
        this.addedMisssingTimeCodes = [];
        if (this.timeCodeRankingGroupInv?.timeCodeRankings.length > 0) {
          this.addedMisssingTimeCodes.push(
            ...this.timeCodeRankingGroupInv.timeCodeRankings.flatMap(
              r => r.rightTimeCodeIds || []
            )
          );
        }

        if (this.timeCodeRankingGroup.timeCodeRankings) {
          this.updateRightTimeCodesInv(this.timeCodeRankingGroup);
        }
        this.form?.customPatchValue(this.timeCodeRankingGroup);
        this.form?.markAsPristine();
        this.form?.markAsUntouched();
        this.updateLeftTimeCodes();
      })
    );
  }
  private loadSelectionTypes() {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeCodeRankingOperatorType,
        true,
        false,
        true
      )
      .pipe(
        tap(x => {
          this.operatorTypes = x.map(
            item => new SmallGenericType(item.id, item.name)
          );

          this.operatorTypesFiltered = x
            .filter(item => item.id === 0)
            .map(item => new SmallGenericType(item.id, item.name));
        })
      );
  }
  getTimeCodes() {
    return this.service
      .getTimeCodes(SoeTimeCodeType.Work, true, true, false)
      .pipe(
        tap((result: any) => {
          this.leftTimeCodes = (result as ITimeCodeDTO[])
            .filter(
              x =>
                x.classification ===
                TermGroup_TimeCodeClassification.InconvinientWorkingHours
            )
            .map(x => new SmallGenericType(x.timeCodeId, x.name));
          this.rightTimeCodesAll = (result as ITimeCodeDTO[])
            .filter(
              x =>
                x.classification ===
                  TermGroup_TimeCodeClassification.AdditionalHours ||
                x.classification ===
                  TermGroup_TimeCodeClassification.OvertimeHours
            )
            .map(x => new SmallGenericType(x.timeCodeId, x.name));
          if (this.form.isNew) {
            this.addedMisssingTimeCodes.push(
              ...this.rightTimeCodesAll.map(tc => tc.id)
            );
          }
        })
      );
  }
  getLeftTimeCodes(idx: number): SmallGenericType[] {
    const rows = this.form?.value?.timeCodeRankings ?? [];
    if (!rows || rows.length === 0) {
      return this.leftTimeCodes;
    }
    const selected = rows[idx]?.leftTimeCodeId;
    const allSelectedIds = rows
      .map((row: any, index: number) =>
        index !== idx ? row.leftTimeCodeId : null
      )
      .filter((id: any) => id !== null && id !== undefined);
    return this.leftTimeCodes.filter(
      x => x.id === selected || !allSelectedIds.includes(x.id)
    );
  }
  disableAddButton(): boolean {
    const nextIndex = this.form?.value?.timeCodeRankings?.length ?? 0;
    const timeCodes = this.getLeftTimeCodes(nextIndex);
    return timeCodes.length === 0;
  }
  updateLeftTimeCodes(): void {
    const controls = this.form?.timeCodeRankings?.controls ?? [];
    this.filteredLeftTimeCodes = [];
    for (let i = 0; i < controls.length; i++) {
      this.filteredLeftTimeCodes[i] = this.getLeftTimeCodes(i);
    }
    this.cdr.detectChanges();
  }
  checkDates(): boolean {
    return (
      this.form.value.stopDate == '' ||
      this.form.value.stopDate >= this.form.value.startDate
    );
  }

  validate(isDelete: boolean): void {
    let saveValid = false;
    if (!this.checkDates()) {
      this.messageboxService.error(
        'time.time.timecode.timecoderanking',
        this.translate.instant('time.time.attest.timestamps.invaliddate')
      );
      return;
    }
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.validateTimeCodeRanking(this.form.value || [], isDelete),
      (result: any) => {
        if (result && result.success) {
          if (isDelete) {
            this.triggerDelete();
            return;
          } else {
            this.beforeSave();
            return;
          }
        }
      },
      (error: any) => {
        let dialog;
        let errorMessage = error?.errorMessage + '\r\n';
        if (error?.strings) {
          saveValid = true;
          errorMessage += error.strings.join(' \r\n');
          errorMessage += '\r\n\r\n' + this.translate.instant('core.continue');
          dialog = this.messageboxService.warning(
            this.translate.instant('time.time.timecode.timecoderanking'),
            errorMessage,
            {
              buttons: 'okCancel',
            }
          );
        } else {
          dialog = this.messageboxService.warning(
            this.translate.instant('time.time.timecode.timecoderanking'),
            errorMessage,
            {
              buttons: 'ok',
            }
          );
        }
        dialog
          .afterClosed()
          .subscribe((response: IMessageboxComponentResponse) => {
            if (response.result && saveValid) {
              if (isDelete) {
                this.triggerDelete();
                return;
              } else {
                this.beforeSave();
                return;
              }
            } else {
              this.loadData().subscribe();
            }
          });
      },
      { showDialogOnError: false, showToast: false }
    );
  }

  beforeSave(): void {
    const dto = this.form?.getRawValue();
    dto.timeCodeRankings.forEach((element: ITimeCodeRankingDTO) => {
      if (element.operatorType === 1) return;

      const existingInv = this.timeCodeRankingGroupInv.timeCodeRankings?.find(
        r => r.leftTimeCodeId === element.leftTimeCodeId
      );
      if (existingInv) {
        existingInv.rightTimeCodeIds = this.rightTimeCodes
          .filter(tc => !element.rightTimeCodeIds?.includes(tc.id))
          .map(tc => tc.id);

        dto.timeCodeRankings.push(existingInv);
      } else {
        const newRow: ITimeCodeRankingDTO = {
          timeCodeRankingId: 0,
          leftTimeCodeId: element.leftTimeCodeId,
          operatorType: 1,
          rightTimeCodeIds: this.rightTimeCodes
            .filter(tc => !element.rightTimeCodeIds?.includes(tc.id))
            .map(tc => tc.id),
          actorCompanyId: element.actorCompanyId,
          leftTimeCodeName: '',
          rightTimeCodeNames: [],
        };
        dto.timeCodeRankings.push(newRow);
      }
    });

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(dto).pipe(
        tap(res => {
          this.updateFormValueAndEmitChange(res);
          if (res.success) this.triggerCloseDialog(res);
        })
      )
    );
  }

  protected onAddTimeCode(): void {
    this.form?.onAddTimeCode();
    this.updateRightTimeCodesInv(this.form.value);
    this.form.customPatchValue(this.form.value);
  }

  protected onDeleteTimeCode(idx: number): void {
    const mb = this.messageboxService.warning(
      'core.delete',
      'core.deletewarning'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) {
        this.form.value.timeCodeRankings =
          this.form.value.timeCodeRankings.filter(
            (_: any, i: any) => i !== idx
          );

        this.form.markAsDirty();
        this.updateLeftTimeCodes();
        this.updateRightTimeCodesInv(this.form.value);
        this.form.customPatchValue(this.form.value);
      }
    });
  }

  updateRightTimeCodesInv(result: ITimeCodeRankingGroupDTO): void {
    const rankingsWithType0 =
      this.timeCodeRankingGroup.timeCodeRankings?.filter(
        r => r.operatorType === 0
      ) || [];
    const rankingsWithType1 =
      this.timeCodeRankingGroupInv.timeCodeRankings?.filter(
        r => r.operatorType === 1
      ) || [];

    this.rightTimeCodes = this.rightTimeCodesAll.filter(tc => {
      const inType0 = rankingsWithType0?.some(r =>
        r.rightTimeCodeIds?.includes(tc.id)
      );
      const inType1 = rankingsWithType1.some(r =>
        r.rightTimeCodeIds?.includes(tc.id)
      );
      const inTypeMissing = this.addedMisssingTimeCodes.includes(tc.id);
      return inType0 || inType1 || inTypeMissing;
    });

    this.rightTimeCodesNotInUse = this.rightTimeCodesAll.filter(tc => {
      const inUse = this.rightTimeCodes.some(rtc => rtc.id === tc.id);
      return !inUse;
    });

    result.timeCodeRankings.forEach((x: any) => {
      x.rightTimeCodeIdsInv = this.rightTimeCodes
        .filter(tc => !x.rightTimeCodeIds?.includes(tc.id))
        .map(tc => tc.name)
        .join(', ');
      x.operatorTypeInv =
        this.operatorTypes.find(ot => ot.id === 1)?.name || '';
    });
  }
  addMissing(index: number): void {
    const timeCode = this.rightTimeCodesNotInUse[index];
    this.form.value.timeCodeRankings.forEach((ranking: ITimeCodeRankingDTO) => {
      if (ranking.rightTimeCodeIds?.includes(timeCode.id)) {
        return;
      }
      if (ranking.operatorType !== 0) {
        ranking.rightTimeCodeIds.push(timeCode.id);
        this.rightTimeCodes.push({ id: timeCode.id, name: timeCode.name });
      }
    });

    const onlyOperatorType0 = this.form.value.timeCodeRankings.filter(
      (ranking: ITimeCodeRankingDTO) => ranking.operatorType === 0
    );
    if (onlyOperatorType0.length === this.form.value.timeCodeRankings.length) {
      onlyOperatorType0.forEach((ranking: ITimeCodeRankingDTO) => {
        const missingTimeCode = ranking;
        const newTimeCode: ITimeCodeRankingDTO = {
          timeCodeRankingId: 0,
          leftTimeCodeId: missingTimeCode.leftTimeCodeId,
          operatorType: 1,
          rightTimeCodeIds: [timeCode.id],
          actorCompanyId: missingTimeCode.actorCompanyId,
          leftTimeCodeName: '',
          rightTimeCodeNames: [],
        };

        this.timeCodeRankingGroup.timeCodeRankings?.push(newTimeCode);
        this.rightTimeCodes.push({ id: timeCode.id, name: timeCode.name });
      });
    }
    this.addedMisssingTimeCodes.push(timeCode.id);
    this.updateRightTimeCodesInv(this.form.value);
    this.form.customPatchValue(this.form.value);
    this.form.markAsDirty();
  }

  onRightTimeCodesChanged(): void {
    setTimeout(() => {
      this.updateRightTimeCodesInv(this.form.value);
      this.form.customPatchValue(this.form.value);
    }, 200);
  }
  override newRecord(): Observable<void> {
    if (this.form?.isCopy) {
      this.onRightTimeCodesChanged();
    }

    return of();
  }
}
