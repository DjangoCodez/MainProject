import { Component, inject, OnInit, signal } from '@angular/core';
import { EconomyService } from '@features/economy/services/economy.service';
import { TimeDeviationCausesService } from '@features/time/time-deviation-causes/services/time-deviation-causes.service';
import { TranslateService } from '@ngx-translate/core';
import {
  CompanySettingType,
  TermGroup,
  TermGroup_ControlEmployeeSchedulePlacementType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDimDTO,
  IActivateScheduleControlDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DateUtil } from '@shared/util/date-util';
import { ExportUtil } from '@shared/util/export-util';
import { SettingsUtil } from '@shared/util/settings-util';
import { ButtonComponent } from '@ui/button/button/button.component';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { InstructionComponent } from '@ui/instruction/instruction.component';
import { forkJoin, Observable, take, tap } from 'rxjs';
import { PlacementsControlDialogGridComponent } from './placements-control-dialog-grid/placements-control-dialog-grid.component';
import { ProgressService } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';

export interface IPlacementsControlDialogData extends DialogData {
  control: IActivateScheduleControlDTO;
}
@Component({
  selector: 'soe-placements-control-dialog',
  imports: [
    PlacementsControlDialogGridComponent,
    DialogComponent,
    ButtonComponent,
    SaveButtonComponent,
    InstructionComponent,
  ],
  templateUrl: 'placements-control-dialog.component.html',
  styleUrl: 'placements-control-dialog.component.scss',
  providers: [FlowHandlerService],
})
export class PlacementsControlDialogComponent
  extends DialogComponent<IPlacementsControlDialogData>
  implements OnInit
{
  // Services
  private readonly translateService = inject(TranslateService);
  private readonly timeDeviationCausesService = inject(
    TimeDeviationCausesService
  );
  private readonly coreService = inject(CoreService);
  private readonly economyService = inject(EconomyService);
  private readonly messageboxService = inject(MessageboxService);
  private readonly progressService = inject(ProgressService);

  performAction = new Perform<any>(this.progressService);

  // Data
  private timeDeviationCauses: SmallGenericType[] = [];
  private controlEmployeeSchedulePlacementType: SmallGenericType[] = [];

  // Signals
  private hiddenShort = signal(false);
  private useAccountsHierarchy = signal(false);
  private defaultEmployeeAccountDimId = signal(0);
  private defaultEmployeeAccountDimName = signal('');
  public instruction = signal('');

  // Inputs
  public control = signal<IActivateScheduleControlDTO>(this.data?.control);

  ngOnInit(): void {
    this.performAction.load(
      forkJoin([
        this.loadTimeDeviationCauses(),
        this.loadTypes(),
        this.loadCompanySettings(),
      ]).pipe(
        tap(() => {
          this.setHeads();
          this.setInstructionStrings();
        })
      )
    );
  }

  //LOAD DATA
  private loadTimeDeviationCauses(): Observable<SmallGenericType[]> {
    return this.timeDeviationCausesService
      .getTimeDeviationCausesDict(false, false, false)
      .pipe(
        tap(x => {
          this.timeDeviationCauses = x;
        })
      );
  }

  private loadTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.ControlEmployeeSchedulePlacementType,
        false,
        true
      )
      .pipe(
        tap(x => {
          this.controlEmployeeSchedulePlacementType = x;
        })
      );
  }

  private loadCompanySettings(): Observable<SmallGenericType[]> {
    const settingTypes: number[] = [];
    settingTypes.push(CompanySettingType.UseAccountHierarchy);
    settingTypes.push(CompanySettingType.DefaultEmployeeAccountDimEmployee);
    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap(x => {
        this.useAccountsHierarchy.set(
          SettingsUtil.getBoolCompanySetting(
            x,
            CompanySettingType.UseAccountHierarchy
          )
        );
        this.defaultEmployeeAccountDimId.set(
          SettingsUtil.getIntCompanySetting(
            x,
            CompanySettingType.DefaultEmployeeAccountDimEmployee
          )
        );
        if (this.useAccountsHierarchy()) {
          this.loadDefaultEmployeeAccount().subscribe();
        }
      })
    );
  }

  private loadDefaultEmployeeAccount(): Observable<IAccountDimDTO> {
    return this.economyService
      .getAccountDimByAccountDimId(this.defaultEmployeeAccountDimId(), false)
      .pipe(
        tap(x => {
          this.defaultEmployeeAccountDimName.set(x.name);
        })
      );
  }

  // EVENTS
  public cancel() {
    this.dialogRef.close();
  }

  public activate() {
    this.translateService
      .get([
        'core.warning',
        'time.schedule.activate.confirmtext',
        'time.schedule.activate.delete.message.hidden.info.accountshierarchy',
        'time.schedule.activate.delete.message.hidden.info.category',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        let msg: string = terms['time.schedule.activate.confirmtext'];
        if (this.hiddenShort()) {
          if (this.useAccountsHierarchy()) {
            msg +=
              '\n<b>' +
              terms[
                'time.schedule.activate.delete.message.hidden.info.accountshierarchy'
              ] +
              ' ' +
              this.defaultEmployeeAccountDimName() +
              '</b>';
          } else {
            msg +=
              '\n<b>' +
              terms[
                'time.schedule.activate.delete.message.hidden.info.category'
              ] +
              '</b>';
          }
        }
        this.messageboxService
          .warning(terms['core.warning'], msg)
          .afterClosed()
          .subscribe(result => {
            if (result.result) {
              this.dialogRef.close(true); // Send back to component that should activate
            }
          });
      });
  }

  public exportToExcel() {
    this.translateService
      .get([
        'common.employee',
        'common.startdate',
        'common.stopdate',
        'common.date',
        'common.time.timedeviationcause',
        'common.type',
        'time.schedule.planning.copyschedule.targetdatestart',
        'time.schedule.planning.copyschedule.targetdateend',
        'common.start',
        'common.stop',
        'time.schedule.planning.wholedaylabel',
        'core.info',
        'common.status',
        'time.schedule.absencerequests.result',
        'time.schedule.activate',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        let headers: string[] = [];
        headers.push(this.normalizeText(terms['common.employee']));
        headers.push(terms['common.startdate']);
        headers.push(terms['common.stopdate']);
        headers.push(terms['common.date']);
        headers.push(terms['common.time.timedeviationcause']);
        headers.push(terms['common.type']);
        headers.push(
          terms['time.schedule.planning.copyschedule.targetdatestart']
        );
        headers.push(
          terms['time.schedule.planning.copyschedule.targetdateend']
        );
        headers.push(terms['common.start']);
        headers.push(terms['common.stop']);
        headers.push(terms['time.schedule.planning.wholedaylabel']);
        headers.push(terms['core.info']);
        headers.push(terms['common.status']);
        headers.push(terms['time.schedule.absencerequests.result']);

        let content: string = headers.join(';') + '\r\n';
        let fileName: string =
          this.normalizeText(terms['time.schedule.activate']) + ' ';

        this.control().heads.forEach(head => {
          let timeDeviationCause =
            this.timeDeviationCauses.find(
              p => p.id === head.timeDeviationCauseId
            )?.name ?? ' ';
          let comment = head.comment ? head.comment : ' ';
          let statusName = head.statusName ? head.statusName : ' ';
          let resultStatusName = head.resultStatusName
            ? head.resultStatusName
            : ' ';

          if (head.rows) {
            head.rows.forEach(rowDetails => {
              let rowContent: string[] = [];

              let type =
                this.controlEmployeeSchedulePlacementType.find(
                  p => p.id === rowDetails.type
                )?.name ?? ' ';
              rowContent.push(this.normalizeText(head.employeeNrAndName));
              rowContent.push(DateUtil.localeDateFormat(head.startDate));
              rowContent.push(DateUtil.localeDateFormat(head.stopDate));
              rowContent.push(DateUtil.localeDateFormat(rowDetails.date));
              rowContent.push(this.normalizeText(timeDeviationCause));
              rowContent.push(this.normalizeText(type));
              rowContent.push(
                DateUtil.localeTimeFormat(rowDetails.scheduleStart)
              );
              rowContent.push(
                DateUtil.localeTimeFormat(rowDetails.scheduleStop)
              );
              rowContent.push(DateUtil.localeTimeFormat(rowDetails.start));
              rowContent.push(DateUtil.localeTimeFormat(rowDetails.stop));
              rowContent.push(rowDetails.isWholeDayAbsence ? '1' : '0');
              rowContent.push(this.normalizeText(comment));
              rowContent.push(this.normalizeText(statusName));
              rowContent.push(this.normalizeText(resultStatusName));

              content += rowContent.join(';') + '\r\n';
            });
          } else {
            let rowContent: string[] = [];
            rowContent.push(this.normalizeText(head.employeeNrAndName));
            rowContent.push(DateUtil.localeDateFormat(head.startDate));
            rowContent.push(DateUtil.localeDateFormat(head.stopDate));
            rowContent.push('');
            rowContent.push(this.normalizeText(timeDeviationCause));
            rowContent.push('');
            rowContent.push('');
            rowContent.push('');
            rowContent.push('');
            rowContent.push('');
            rowContent.push('');
            rowContent.push(this.normalizeText(comment));
            rowContent.push(this.normalizeText(statusName));
            rowContent.push(this.normalizeText(resultStatusName));

            content += rowContent.join(';') + '\r\n';
          }
        });
        ExportUtil.ExportToCSV(content, fileName + '.csv');
      });
  }

  // HELPER METHODS
  private setHeads() {
    this.control().heads.forEach(head => {
      if (
        head.type ===
        TermGroup_ControlEmployeeSchedulePlacementType.ShortenIsHidden
      ) {
        this.hiddenShort.set(true);
        if (head.rows && head.rows.length > 0) {
          head.startDate = head.rows[0].start;
        }
      }
    });
  }

  private setInstructionStrings() {
    this.translateService
      .get([
        'time.schedule.activate.absencerequest',
        'time.schedule.activate.absencerequestinfotext',
        'time.schedule.activate.absence',
        'time.schedule.activate.absenceinfotext',
        'time.schedule.activate.changedschedule',
        'time.schedule.activate.changedscheduleinfotext',
        'time.schedule.activate.changedresult',
        'time.schedule.activate.changedresultinfotext',
        'time.schedule.activate.exportinfotext',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.instruction.set(
          terms['time.schedule.activate.absencerequest'] +
            ' ' +
            terms['time.schedule.activate.absencerequestinfotext'] +
            '\n' +
            terms['time.schedule.activate.absence'] +
            ' ' +
            terms['time.schedule.activate.absenceinfotext'] +
            '\n' +
            terms['time.schedule.activate.changedschedule'] +
            ' ' +
            terms['time.schedule.activate.changedscheduleinfotext'] +
            '\n' +
            terms['time.schedule.activate.changedresult'] +
            ' ' +
            terms['time.schedule.activate.changedresultinfotext'] +
            '\n' +
            terms['time.schedule.activate.exportinfotext']
        );
      });
  }

  // HELPER METHODS
  private normalizeText(text: string): string {
    return text
      .replace(/ä/g, 'a')
      .replace(/å/g, 'a')
      .replace(/ö/g, 'o')
      .replace(/Ä/g, 'A')
      .replace(/Å/g, 'A')
      .replace(/Ö/g, 'O');
  }
}
