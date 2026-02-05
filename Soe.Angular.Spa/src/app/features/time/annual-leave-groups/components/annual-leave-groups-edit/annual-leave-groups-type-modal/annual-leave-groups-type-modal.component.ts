import { Component, OnInit, inject } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData, DialogSize } from '@ui/dialog/models/dialog'
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { AnnualLeaveGroupsService } from '../../../services/annual-leave-groups.service';
import { DateUtil } from '@shared/util/date-util'
import { Perform } from '@shared/util/perform.class';
import { IAnnualLeaveGroupLimitDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TermGroup_AnnualLeaveGroupType } from '@shared/models/generated-interfaces/Enumerations';
import { tap } from 'rxjs';

export class AnnualLeaveGroupsTypeDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  showHeaderInfo?: boolean;
  type!: TermGroup_AnnualLeaveGroupType;
}

@Component({
  selector: 'soe-annual-leave-groups-type-modal',
  templateUrl: './annual-leave-groups-type-modal.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AnnualLeaveGroupsTypeModalComponent
  extends DialogComponent<AnnualLeaveGroupsTypeDialogData>
  implements OnInit
{
  progressService = inject(ProgressService);
  service = inject(AnnualLeaveGroupsService);
  performTypes = new Perform<IAnnualLeaveGroupLimitDTO[]>(this.progressService);

  ngOnInit() {
    this.loadTypeLimits().subscribe();
  }

  loadTypeLimits() {
    return this.performTypes.load$(this.service.getTypeLimits(this.data.type));
  }

  parseTimeDuration(duration: number): string {
    if (duration === null || duration === undefined) {
      return '';
    }
    return DateUtil.minutesToTimeSpan(duration);
  }

  parseMinutesToDecimal(minutes: number): number {
    if (minutes === null || minutes === undefined) {
      return 0;
    }
    return (minutes / 60).round(1);
  }
}
