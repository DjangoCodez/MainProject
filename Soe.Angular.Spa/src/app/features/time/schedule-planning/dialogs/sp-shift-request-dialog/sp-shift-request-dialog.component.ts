import { Component, inject, OnInit, signal } from '@angular/core';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { PlanningShiftDTO } from '../../models/shift.model';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { ValidationHandler } from '@shared/handlers';
import { SpShiftRequestDialogForm } from './sp-shift-request-dialog-form.model';
import { PlanningEmployeeDTO } from '../../models/employee.model';
import { SpShiftSimpleComponent } from '../../components/sp-shift-simple/sp-shift-simple.component';
import { SpToolbarEmployeeDate } from '../../toolbar/sp-toolbar-employee-date/sp-toolbar-employee-date';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonComponent } from '@ui/button/button/button.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { TexteditorComponent } from '@ui/forms/texteditor/texteditor.component';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ShiftUtil } from '../../util/shift-util';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { SpSettingService } from '../../services/sp-setting.service';
import { MatStepperModule } from '@angular/material/stepper';
import { IconModule } from '@ui/icon/icon.module';
import { SpShiftRequestRecipientsComponent } from './sp-shift-request-recipients/sp-shift-request-recipients.component';
import { SpShiftService } from '../../services/sp-shift.service';
import { Observable } from 'rxjs';

export class SpShiftRequestDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  hideFooter?: boolean;
  disableContentScroll?: boolean;
  employee!: PlanningEmployeeDTO;
  shift!: PlanningShiftDTO;
  possibleEmployees: PlanningEmployeeDTO[] = [];
}

export class SpShiftRequestDialogResult {
  requestSent = false;
}

@Component({
  selector: 'sp-shift-request-dialog.component',
  imports: [
    ButtonComponent,
    CheckboxComponent,
    DialogComponent,
    IconModule,
    MatStepperModule,
    ReactiveFormsModule,
    SpShiftRequestRecipientsComponent,
    SpShiftSimpleComponent,
    SpToolbarEmployeeDate,
    TextboxComponent,
    TexteditorComponent,
    ToolbarComponent,
    TranslatePipe,
  ],
  templateUrl: './sp-shift-request-dialog.component.html',
  styleUrl: './sp-shift-request-dialog.component.scss',
})
export class SpShiftRequestDialogComponent
  extends DialogComponent<SpShiftRequestDialogData>
  implements OnInit
{
  private readonly shiftService = inject(SpShiftService);
  readonly settingService = inject(SpSettingService);
  private readonly translate = inject(TranslateService);

  validationHandler = inject(ValidationHandler);
  form: SpShiftRequestDialogForm = new SpShiftRequestDialogForm({
    validationHandler: this.validationHandler,
    element: undefined,
  });

  executing = signal(false);

  ngOnInit(): void {
    const shift = this.data.shift;

    if (!this.data.employee || !shift || !this.data.possibleEmployees?.length)
      this.cancel();

    this.getLinkedShifts(shift).subscribe(shifts => {
      ShiftUtil.sortShifts(shifts);
      const date = shifts[0].actualStartDate;
      let subject = `${this.translate.instant('core.xemail.shiftrequest')} ${date.toFormattedDate()}`;
      let text = '';
      shifts.forEach((shift: PlanningShiftDTO) => {
        let msg = `${shift.actualStartTime.toFormattedTime()}-${shift.actualStopTime.toFormattedTime()} ${shift.shiftTypeName}`;

        if (shift.accountName) msg += ` (${shift.accountName})`;

        if (shifts.length === 1) {
          subject += ', ' + msg;
        } else {
          text += msg + '<br/>';
        }
      });
      const shortText = text;

      this.form.reset({
        employeeId: this.data.employee.employeeId,
        employeeName: this.data.employee.name,
        date: date,
        subject: subject,
        text: text,
        shortText: shortText,
        filterOnShiftType: true,
        filterOnSkills: true,
        filterOnWorkRules: true,
        filterOnAvailability: true,
      });
      this.form.possibleEmployees = this.data.possibleEmployees;
      this.form.patchShifts(shifts);
    });
  }

  getLinkedShifts(shift: PlanningShiftDTO): Observable<PlanningShiftDTO[]> {
    return this.shiftService.loadLinkedShifts(
      shift.timeScheduleTemplateBlockId
    );
  }

  cancel() {
    this.dialogRef.close({ requestSent: false } as SpShiftRequestDialogResult);
  }

  ok() {
    this.executing.set(true);
  }
}
