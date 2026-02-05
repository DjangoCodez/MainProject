import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { TimeWorkAccountWorkTimeWeekDTO } from '../../../models/timeworkaccount.model';
import { TimeWorkAccountService } from '../../services/time-work-account.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { CrudActionTypeEnum } from '@shared/enums';
import { Perform } from '@shared/util/perform.class'
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { ValidationHandler } from '@shared/handlers';
import { CellValueChangedEvent } from 'ag-grid-community';
import { TimeWorkAccountWorkTimeWeekForm } from '../../models/time-work-account-year-worktimeweek-form.model';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData } from '@ui/dialog/models/dialog';

export interface ITimeWorkAccountYearWorkTimeWeekEventObject {
  object: TimeWorkAccountWorkTimeWeekForm | undefined;
  index: number | undefined;
  action: CrudActionTypeEnum;
}
export interface ITimeWorkAccountYearWorkTimeWeekEventDialogData
  extends DialogData {
  form: TimeWorkAccountWorkTimeWeekForm | undefined;
  index: number | undefined;
}
@Component({
    selector: 'soe-time-work-account-year-worktimeweek-edit',
    templateUrl: './time-work-account-year-worktimeweek-edit.component.html',
    providers: [FlowHandlerService],
    standalone: false
})
export class TimeWorkAccountYearWorkTimeWeekEditComponent
  extends DialogComponent<ITimeWorkAccountYearWorkTimeWeekEventDialogData>
  implements OnInit
{
  @Input() form: TimeWorkAccountWorkTimeWeekForm | undefined;
  @Input() index: number | undefined;
  @Output() actionTaken = new EventEmitter<CrudActionTypeEnum>();

  idFieldName = '';
  performAction = new Perform<TimeWorkAccountService>(this.progressService);

  // Lookups
  terms: any = [];

  public event: EventEmitter<ITimeWorkAccountYearWorkTimeWeekEventObject> =
    new EventEmitter();

  get currentLanguage(): string {
    return SoeConfigUtil.language;
  }

  constructor(
    private progressService: ProgressService,
    private validationHandler: ValidationHandler,
    public handler: FlowHandlerService
  ) {
    super();
    this.setData(this.data.index, this.data.form);
  }

  ngOnInit() {
    this.handler.execute({
      permission: Feature.Time_Payroll_TimeWorkAccount,
    });
  }

  onCellValueChanged(evt: CellValueChangedEvent): void {
    this.form?.markAsDirty();
  }

  createForm(
    element?: TimeWorkAccountWorkTimeWeekDTO,
    setIdFieldName = true
  ): TimeWorkAccountWorkTimeWeekForm {
    const form = new TimeWorkAccountWorkTimeWeekForm({
      validationHandler: this.validationHandler,
      element,
    });
    if (setIdFieldName) this.idFieldName = form.getIdFieldName();
    return form;
  }

  triggerEvent(
    index: number | undefined,
    item: TimeWorkAccountWorkTimeWeekForm | undefined,
    action: CrudActionTypeEnum
  ) {
    this.dialogRef.close({ index, object: item, action });
  }

  performSave(): void {
    if (!this.form || this.form.invalid) return;

    this.triggerEvent(this.index, this.form, CrudActionTypeEnum.Save);
  }

  performCancel(): void {
    this.triggerEvent(undefined, undefined, CrudActionTypeEnum.Save);
  }

  setData(index?: number, form?: TimeWorkAccountWorkTimeWeekForm) {
    this.index = index;
    this.form = this.createForm(form?.value);
    this.form!.patchValue(this.form?.value);
  }
}
