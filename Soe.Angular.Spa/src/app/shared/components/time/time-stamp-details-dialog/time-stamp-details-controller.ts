import { Component, inject, OnInit } from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { TranslateService } from '@ngx-translate/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { AdjustTimeStampsService } from '../../../../features/time/adjust-time-stamps/services/adjust-time-stamps.service';
import {
  SoeEntityType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { ITrackChangesLogDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { IUserAgentClientInfoDTO } from '@shared/models/generated-interfaces/UserAgentClientInfoDTO';
import { CoreService } from '@shared/services/core.service';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';

import { TrackChangesService } from '@shared/services/track-changes.service';
import {
  TimeStampDetailsForm,
  TimeStampEntryDTO,
} from './models/time-stamp-details.form.model';
export interface ITimeStampDetailsDialogData extends DialogData {
  timeStampEntryId: number;
}

@Component({
  selector: 'soe-time-work-reduction-reconciliation-dialog',
  templateUrl: './time-stamp-details-controller.html',
  providers: [FlowHandlerService, DialogService, ToolbarService],
  standalone: false,
})
export class TimeStampDetailsDialogData
  extends DialogComponent<ITimeStampDetailsDialogData>
  implements OnInit
{
  validationHandler = inject(ValidationHandler);
  translateService = inject(TranslateService);
  handler = inject(FlowHandlerService);
  progressService = inject(ProgressService);
  performLoad = new Perform<ISmallGenericType[]>(this.progressService);
  performTrackChanges = new Perform<ITrackChangesLogDTO[]>(
    this.progressService
  );
  service = inject(AdjustTimeStampsService);
  coreService = inject(CoreService);
  trackChangesService = inject(TrackChangesService);
  timeStampEntryId: number = this.data.timeStampEntryId;
  notTerminalInfoText = this.translateService.instant(
    'time.time.attest.timestamps.noterminal'
  );
  manualAdjustedInfoText = this.translateService.instant(
    'time.time.attest.timestamps.manuallyadjusted'
  );
  form!: TimeStampDetailsForm;
  isSupportAdmin: boolean = SoeConfigUtil.isSupportAdmin;
  statuses: ISmallGenericType[] = [];
  originTypes: ISmallGenericType[] = [];
  timeStampEntry: TimeStampEntryDTO = new TimeStampEntryDTO();
  clientInfo: IUserAgentClientInfoDTO = {} as IUserAgentClientInfoDTO;
  isChangeLogOpen: boolean = false;
  changes: ITrackChangesLogDTO[] = [];
  searching: boolean = false;
  selectedChange: any | null = null;
  clientInfoLoaded = false;

  get clientData(): string {
    const c = this.clientInfo as any;
    return c?.data ?? c?.info ?? c?.raw ?? '';
  }
  get deviceString(): string {
    return '{0} {1} {2}'
      .format(this.deviceBrand, this.deviceFamily, this.deviceModel)
      .trim();
  }

  get osString(): string {
    const c = this.clientInfo as any;
    return '{0} {1}'.format(c.osFamily, c.osVersion);
  }

  get uaString(): string {
    const c = this.clientInfo as any;
    return '{0} {1}'.format(c.userAgentFamily, c.userAgentVersion);
  }

  get deviceBrand(): string {
    const c = this.clientInfo as any;
    return c?.deviceBrand ?? c?.brand ?? '';
  }

  get deviceFamily(): string {
    const c = this.clientInfo as any;
    return c?.deviceFamily ?? c?.family ?? '';
  }

  get deviceModel(): string {
    const c = this.clientInfo as any;
    return c?.deviceModel ?? c?.model ?? '';
  }

  selectChange(change: any): void {
    this.selectedChange = this.selectedChange === change ? null : change;
  }
  ngOnInit() {
    this.form = this.createForm();
    this.handler.execute({
      lookups: [
        this.loadStatuses(),
        this.loadOriginTypes(),
        this.loadAgentClientInfo(),
      ],
    });
  }

  close() {
    this.dialogRef.close();
  }

  private loadStatuses(): Observable<ISmallGenericType[]> {
    return this.performLoad.load$(
      this.coreService
        .getTermGroupContent(TermGroup.TimeStampEntryStatus, false, false)
        .pipe(
          tap(data => {
            this.statuses = data;
          })
        )
    );
  }

  private loadOriginTypes(): Observable<ISmallGenericType[]> {
    return this.performLoad.load$(
      this.coreService
        .getTermGroupContent(TermGroup.TimeStampEntryOriginType, false, false)
        .pipe(
          tap(data => {
            this.originTypes = data;
            this.loadTimeStampEntry().subscribe();
          })
        )
    );
  }
  createForm(element?: TimeStampDetailsForm): TimeStampDetailsForm {
    return new TimeStampDetailsForm({
      validationHandler: this.validationHandler,
      element,
    });
  }
  private loadTimeStampEntry(): Observable<TimeStampEntryDTO> {
    return this.service.getTimeStamp(this.timeStampEntryId).pipe(
      tap(x => {
        this.timeStampEntry = x;
        if (this.timeStampEntry) {
          if (this.timeStampEntry.timeTerminalId)
            this.timeStampEntry.terminalInfo = '{0} (ID: {1})'.format(
              this.timeStampEntry.timeTerminalName,
              this.timeStampEntry.timeTerminalId.toString()
            );
          this.timeStampEntry.statusText = '{0}: {1}'.format(
            this.timeStampEntry.status.toString(),
            this.statuses.find(s => s.id === this.timeStampEntry.status)
              ?.name || ''
          );
          this.timeStampEntry.originTypeText = '{0}: {1}'.format(
            this.timeStampEntry.originType.toString(),
            this.originTypes.find(o => o.id === this.timeStampEntry.originType)
              ?.name || ''
          );
        }
      })
    );
  }

  search(): void {
    this.searching = true;

    this.performTrackChanges
      .load$(
        this.trackChangesService
          .getTrackChangesLog(
            SoeEntityType.TimeStampEntry as number,
            this.timeStampEntryId,
            this.formatForApi(this.form.value.selectedDateFrom),
            this.formatForApi(this.form.value.selectedDateTo)
          )
          .pipe(
            tap(x => {
              this.changes = x;
              this.isChangeLogOpen = true;
              this.searching = false;
            })
          )
      )
      .subscribe();
  }

  loadAgentClientInfo(): Observable<IUserAgentClientInfoDTO> {
    return this.service
      .getTimeStampEntryUserAgentClientInfo(this.timeStampEntryId)
      .pipe(
        tap(data => {
          if (data) {
            this.clientInfoLoaded = true;
          }
          this.clientInfo = data;
        })
      );
  }
  private formatForApi(value?: Date | string | null): string {
    if (!value) return '';
    const d = value instanceof Date ? value : new Date(value);
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${d.getFullYear()}${pad(d.getMonth() + 1)}${pad(d.getDate())}T${pad(
      d.getHours()
    )}${pad(d.getMinutes())}${pad(d.getSeconds())}`;
  }
  checkSelectedDates(): boolean {
    return (
      this.form.value.selectedDateFrom != null &&
      this.form.value.selectedDateTo != null &&
      this.form.value.selectedDateTo >= this.form.value.selectedDateFrom
    );
  }
}
