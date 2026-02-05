import { Component, inject, Input, OnInit } from '@angular/core';
import { SpShiftRequestRecipientsAvailableComponent } from './sp-shift-request-recipients-available.component';
import { SpShiftRequestDialogForm } from '../sp-shift-request-dialog-form.model';
import { SpShiftRequestRecipientsSelectedComponent } from './sp-shift-request-recipients-selected.component';
import { MessageGroupService } from '@features/time/time-schedule-events/services/message-group.service';
import { Observable, map, tap } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { IconButtonComponent } from '@ui/button/icon-button/icon-button.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { SpSettingService } from '@features/time/schedule-planning/services/sp-setting.service';
import { TranslatePipe } from '@ngx-translate/core';
import { SpShiftRequestService } from '@features/time/schedule-planning/services/sp-shift-request.service';
import { Perform } from '@shared/util/perform.class';
import { ProgressService } from '@shared/services/progress';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'sp-shift-request-recipients',
  imports: [
    ReactiveFormsModule,
    ButtonComponent,
    CheckboxComponent,
    IconButtonComponent,
    SelectComponent,
    SpShiftRequestRecipientsAvailableComponent,
    SpShiftRequestRecipientsSelectedComponent,
    TranslatePipe,
  ],
  templateUrl: './sp-shift-request-recipients.component.html',
  styleUrl: './sp-shift-request-recipients.component.scss',
})
export class SpShiftRequestRecipientsComponent implements OnInit {
  @Input({ required: true }) form!: SpShiftRequestDialogForm;

  private readonly messageGroupService = inject(MessageGroupService);
  private readonly shiftRequestService = inject(SpShiftRequestService);
  readonly settingService = inject(SpSettingService);
  private readonly progressService = inject(ProgressService);
  private perform = new Perform<any>(this.progressService);

  messageGroups: ISmallGenericType[] = [];

  ngOnInit(): void {
    this.loadMessageGroups().subscribe();
  }

  private loadMessageGroups(): Observable<ISmallGenericType[]> {
    return this.messageGroupService.getDict().pipe(
      map(groups => {
        // Add empty row
        const empty: ISmallGenericType = {
          id: 0,
          name: '',
        } as ISmallGenericType;

        return [empty, ...groups];
      }),
      tap(groups => {
        this.messageGroups = groups;
      })
    );
  }

  getAvailableEmployees() {
    const obs = this.shiftRequestService.getAvailableEmployees(
      this.form.shifts.map(s => s.timeScheduleTemplateBlockId),
      this.form.possibleEmployees.map(e => e.employeeId),
      this.form.filterOnShiftType,
      this.form.filterOnAvailability,
      this.form.filterOnSkills,
      this.form.filterOnWorkRules,
      this.form.filterOnMessageGroupId
    );

    // TODO: New term
    this.perform
      .load$(obs, {
        title: 'Söker tillgängliga mottagare',
        message:
          'Söker efter tillgängliga mottagare baserat på valda filter...',
      })
      .subscribe(employees => {
        this.form.patchAvailableEmployees(employees);
      });
  }
}
