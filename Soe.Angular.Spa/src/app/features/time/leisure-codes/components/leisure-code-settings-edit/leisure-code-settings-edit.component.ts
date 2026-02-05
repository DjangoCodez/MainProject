import {
  Component,
  computed,
  inject,
  Input,
  OnInit,
  signal,
} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import {
  Feature,
  SettingDataType,
  TermGroup,
  TermGroup_TimeLeisureCodeSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { tap } from 'rxjs/operators';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { TermCollection } from '@shared/localization/term-types';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { LeisureCodeSettingsForm } from '../../models/leisure-code-settings-form.model';
import { IEmployeeGroupTimeLeisureCodeSettingDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { LeisureCodesService } from '../../services/leisure-codes.service';
import { TimeScheduleTypeService } from '@features/time/time-schedule-type/services/time-schedule-type.service';

export interface ILeisureCodeSettingsEventObject {
  object: LeisureCodeSettingsForm | undefined;
  action: CrudActionTypeEnum;
}

export interface ILeisureCodeSettingsDialogData extends DialogData {
  form: LeisureCodeSettingsForm;
}

@Component({
  selector: 'soe-leisure-code-settings-edit',
  templateUrl: './leisure-code-settings-edit.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class LeisureCodeSettingsEditComponent
  extends DialogComponent<ILeisureCodeSettingsDialogData>
  implements OnInit
{
  @Input() form: LeisureCodeSettingsForm | undefined;

  service = inject(LeisureCodesService);
  scheduleTypeService = inject(TimeScheduleTypeService);
  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  performTypes = new Perform<SmallGenericType[]>(this.progressService);
  performSettingSelect = new Perform<SmallGenericType[]>(this.progressService);

  rows = new BehaviorSubject<IEmployeeGroupTimeLeisureCodeSettingDTO[]>([]);

  terms: TermCollection = {};
  idFieldName = '';
  selectedDataType = signal(0);
  selectedDataTypeIsInt = signal(false);
  selectedDataTypeIsDate = signal(false);
  selectedDataTypeIsTime = signal(false);
  selectedDataTypeIsBool = signal(false);
  selectedDataTypeIsString = signal(false);
  selectedDataTypeIsDecimal = signal(false);
  selectedSettingType = signal<
    TermGroup_TimeLeisureCodeSettingType | undefined
  >(undefined);

  selectedSettingRendersNumberbox = computed(() => {
    return (
      this.selectedSettingType() ===
      TermGroup_TimeLeisureCodeSettingType.LeisureHours
    );
  });

  selectedSettingRendersSelect = computed(() => {
    return (
      this.selectedSettingType() ===
        TermGroup_TimeLeisureCodeSettingType.ScheduleType ||
      this.selectedSettingType() ===
        TermGroup_TimeLeisureCodeSettingType.PreferredWeekDay
    );
  });

  // Variables for dirtyhandling and status
  dataLoaded = false;
  inProgress = signal(false);

  constructor(
    private translate: TranslateService,
    private validationHandler: ValidationHandler,
    public flowHandler: FlowHandlerService
  ) {
    super();
    this.form =
      this.data.form ??
      this.createForm(false, {} as IEmployeeGroupTimeLeisureCodeSettingDTO);
    this.dataLoaded = true;
  }

  // INIT

  ngOnInit() {
    this.flowHandler.execute({
      permission: Feature.Time_Export_StandardDefinitions,
      lookups: [this.loadTerms(), this.loadSettingTypes()],
    });
  }

  createForm(
    setIdFieldName = true,
    element?: IEmployeeGroupTimeLeisureCodeSettingDTO
  ): LeisureCodeSettingsForm {
    const form = new LeisureCodeSettingsForm({
      validationHandler: this.validationHandler,
      element,
    });
    if (setIdFieldName) this.idFieldName = form.getIdFieldName();
    return form;
  }

  // SERVICE CALLS

  private loadTerms(): Observable<TermCollection> {
    return this.translate.get(['core.deletewarning', 'core.delete']).pipe(
      tap(terms => {
        this.terms = terms;
      })
    );
  }

  loadSettingTypes() {
    return this.performTypes.load$(
      this.coreService.getTermGroupContent(
        TermGroup.TimeLeisureCodeSettingType,
        false,
        false
      )
    );
  }

  loadScheduleTypes() {
    return this.performSettingSelect.load$(
      this.scheduleTypeService.getTimeScheduleTypesDict(true)
    );
  }

  loadWeekDays() {
    return this.performSettingSelect.load$(
      of(DateUtil.getDayOfWeekNames(true, undefined, true))
    );
  }

  // UTILS

  changeTypeDataType(type: number): void {
    switch (type) {
      case TermGroup_TimeLeisureCodeSettingType.LeisureHours:
        this.setDataType(SettingDataType.Integer, type);
        break;
      case TermGroup_TimeLeisureCodeSettingType.ScheduleType:
        this.loadScheduleTypes().subscribe(() => {
          this.setDataType(SettingDataType.Integer, type);
        });
        break;
      case TermGroup_TimeLeisureCodeSettingType.PreferredWeekDay:
        this.loadWeekDays().subscribe(() => {
          this.setDataType(SettingDataType.Integer, type);
        });
        break;
      default:
        this.setDataType(SettingDataType.Undefined, type);
        break;
    }
  }

  setSelectedSettingType(type: TermGroup_TimeLeisureCodeSettingType): void {
    this.selectedSettingType.set(type);
  }

  setDataType(
    dataType: SettingDataType,
    type: TermGroup_TimeLeisureCodeSettingType
  ): void {
    this.selectedDataType.set(dataType);

    this.form?.controls.name.setValue(
      this.performTypes.data?.find(t => t.id == this.form?.controls.type.value)
        ?.name
    );
    this.form?.controls.dataType.setValue(dataType);

    this.selectedDataTypeIsString.set(false);
    this.selectedDataTypeIsInt.set(false);
    this.selectedDataTypeIsDate.set(false);
    this.selectedDataTypeIsTime.set(false);
    this.selectedDataTypeIsBool.set(false);
    this.selectedDataTypeIsDecimal.set(false);

    switch (dataType) {
      case SettingDataType.String:
        this.selectedDataTypeIsString.set(true);
        this.form?.controls.strData.setValue('');
        break;
      case SettingDataType.Integer:
        this.selectedDataTypeIsInt.set(true);
        this.form?.controls.intData.setValue('');
        break;
      case SettingDataType.Date:
        this.selectedDataTypeIsDate.set(true);
        this.form?.controls.dateData.setValue('');
        break;
      case SettingDataType.Time:
        this.selectedDataTypeIsTime.set(true);
        this.form?.controls.timeData.setValue('');
        break;
      case SettingDataType.Boolean:
        this.selectedDataTypeIsBool.set(true);
        this.form?.controls.boolData.setValue('');
        break;
      case SettingDataType.Decimal:
        this.selectedDataTypeIsDecimal.set(true);
        this.form?.controls.decimalData.setValue('');
        break;
      default:
        break;
    }
    this.setSelectedSettingType(type);
  }

  // ACTIONS

  setFormValues() {
    if (
      this.form?.controls.type.value ==
        TermGroup_TimeLeisureCodeSettingType.ScheduleType ||
      this.form?.controls.type.value ==
        TermGroup_TimeLeisureCodeSettingType.PreferredWeekDay
    )
      this.form?.controls.settingValue.setValue(
        this.performSettingSelect.data?.find(
          x => x.id == this.form?.controls.intData.value
        )?.name
      );
  }

  triggerEvent(action: CrudActionTypeEnum) {
    if (!this.form) return;
    this.setFormValues();
    this.dialogRef.close({ object: this.form, action: action });
  }

  performDelete() {
    this.triggerEvent(CrudActionTypeEnum.Delete);
  }

  performAdd() {
    this.triggerEvent(CrudActionTypeEnum.Save);
  }

  anyInvalidValue() {
    let invalid = true;
    switch (this.selectedSettingType()) {
      case TermGroup_TimeLeisureCodeSettingType.LeisureHours:
      case TermGroup_TimeLeisureCodeSettingType.ScheduleType:
      case TermGroup_TimeLeisureCodeSettingType.PreferredWeekDay:
        invalid =
          this.form?.controls.intData.value === null ||
          this.form?.controls.intData.value === '';
        break;
      default:
        invalid = false;
    }
    return invalid;
  }
}
