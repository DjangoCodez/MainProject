import { Component, OnInit, inject, signal } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  TermGroup_TimeLeisureCodeSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import {
  IEmployeeGroupTimeLeisureCodeDTO,
  ITimeLeisureCodeGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { LeisureCodesService } from '../../services/leisure-codes.service';
import { LeisureCodesForm } from '../../models/leisure-codes-form.model';
import { TimeService } from '@features/time/services/time.service';
import { LeisureCodeTypesService } from '@features/time/leisure-code-types/services/leisure-code-types.service';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  ILeisureCodeSettingsDialogData,
  ILeisureCodeSettingsEventObject,
  LeisureCodeSettingsEditComponent,
} from '../leisure-code-settings-edit/leisure-code-settings-edit.component';
import { CrudActionTypeEnum } from '@shared/enums/action.enum';
import { Observable, of, take, tap } from 'rxjs';
import { LeisureCodeSettingsForm } from '../../models/leisure-code-settings-form.model';
import { TermCollection } from '@shared/localization/term-types';
import { TimeScheduleTypeService } from '@features/time/time-schedule-type/services/time-schedule-type.service';

@Component({
  selector: 'soe-leisure-codes-edit',
  templateUrl: './leisure-codes-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class LeisureCodesEditComponent
  extends EditBaseDirective<
    IEmployeeGroupTimeLeisureCodeDTO,
    LeisureCodesService,
    LeisureCodesForm
  >
  implements OnInit
{
  service = inject(LeisureCodesService);
  private readonly timeService = inject(TimeService);
  private readonly toasterService = inject(ToasterService);
  private readonly leisureCodeTypesService = inject(LeisureCodeTypesService);
  private readonly dialogService = inject(DialogService);
  private readonly scheduleTypeService = inject(TimeScheduleTypeService);

  performEmployeeGroups = new Perform<SmallGenericType[]>(this.progressService);
  performLeisureCodeTypes = new Perform<ITimeLeisureCodeGridDTO[]>(
    this.progressService
  );
  performScheduleTypes = new Perform<SmallGenericType[]>(this.progressService);
  performPreferredWeekDays = new Perform<SmallGenericType[]>(
    this.progressService
  );

  terms: TermCollection = {};
  settingsToolbarService = inject(ToolbarService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_ScheduleSettings_LeisureCodeType_Edit,
      {
        lookups: [
          this.loadEmployeeGroups(),
          this.loadLeisureCodeTypes(),
          this.loadScheduleTypes(),
          this.loadPreferredWeekDays(),
        ],
      }
    );
  }

  override onFinished(): void {
    this.setupToolbar();
  }

  private setupToolbar(): void {
    if (!this.flowHandler.modifyPermission()) return;

    this.settingsToolbarService.clearItemGroups();
    this.settingsToolbarService.createItemGroup({
      items: [
        this.settingsToolbarService.createToolbarButton('new', {
          iconName: signal('plus'),
          caption: signal('time.schedule.leisurecode.setting.new'),
          tooltip: signal('time.schedule.leisurecode.setting.new'),
          onAction: () => {
            this.editSetting();
            this.form?.markAsDirty();
          },
        }),
      ],
    });
  }

  editSetting(form?: any) {
    const isNew =
      !form?.value.exportDefinitionLevelId ||
      form.value.exportDefinitionLevelId === 0;

    this.dialogService
      .open(LeisureCodeSettingsEditComponent, {
        title: form
          ? 'time.schedule.leisurecode.setting.edit'
          : 'time.schedule.leisurecode.setting.new',
        size: 'lg',
        hideFooter: true,
        form: structuredClone(form),
      } as ILeisureCodeSettingsDialogData)
      .afterClosed()
      .pipe(take(1))
      .subscribe(({ object, action }: ILeisureCodeSettingsEventObject) => {
        if (action === CrudActionTypeEnum.Save) {
          const existing = this.form?.settings.value.filter(
            (x: any) => x.type == object?.controls.type.value
          );
          if (existing && existing.length > 0) {
            this.toasterService.error(
              this.terms[
                'time.schedule.leisurecode.setting.duplicatewarningtext'
              ],
              this.terms[
                'time.schedule.leisurecode.setting.duplicatewarningtitle'
              ],
              { closeButton: true }
            );
          } else {
            isNew
              ? this.form?.addSettingForm(object)
              : form?.customPatchValue(object!.value);
            this.form?.markAsDirty();
          }
        } else if (action === CrudActionTypeEnum.Delete && object) {
          this.removeSetting(object);
        }
      });
  }

  removeSetting(settingForm: LeisureCodeSettingsForm) {
    this.form?.settings.value.forEach(
      (el: LeisureCodeSettingsForm, i: number) => {
        el.value === settingForm.value && this.form?.settings.removeAt(i);
      }
    );
  }

  populateData(data: IEmployeeGroupTimeLeisureCodeDTO) {
    // settings
    data.settings.forEach(x => {
      switch (x.type) {
        case TermGroup_TimeLeisureCodeSettingType.ScheduleType:
          x.settingValue =
            this.performScheduleTypes.data?.find(t => t.id == x.intData)
              ?.name || '';
          break;
        case TermGroup_TimeLeisureCodeSettingType.LeisureHours:
          x.settingValue = x.intData?.toString() || '';
          break;
        case TermGroup_TimeLeisureCodeSettingType.PreferredWeekDay:
          x.settingValue =
            this.performPreferredWeekDays.data?.find(t => t.id == x.intData)
              ?.name || '';
          break;
      }
    });
    return data;
  }

  // SERVICE CALLS

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: IEmployeeGroupTimeLeisureCodeDTO) => {
          value = this.populateData(value);
          this.form?.customPatchValue(value);
        })
      )
    );
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'time.schedule.leisurecode.setting.duplicatewarningtitle',
      'time.schedule.leisurecode.setting.duplicatewarningtext',
    ]);
  }

  private loadEmployeeGroups() {
    return this.performEmployeeGroups.load$(
      this.timeService.getEmployeeGroups(true)
    );
  }

  private loadLeisureCodeTypes() {
    return this.performLeisureCodeTypes.load$(
      this.leisureCodeTypesService.getGrid()
    );
  }

  private loadScheduleTypes() {
    return this.performScheduleTypes.load$(
      this.scheduleTypeService.getTimeScheduleTypesDict(true)
    );
  }

  private loadPreferredWeekDays() {
    return this.performPreferredWeekDays.load$(
      of(DateUtil.getDayOfWeekNames(true, undefined, true))
    );
  }
}
