import { Component, OnInit, inject } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SysLogDTO } from '../../models/support-logs.model';
import { SupportLogsService } from '../../services/support-logs.service';
import { SupportLogsForm } from '../../models/support-logs-form.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { Observable, tap } from 'rxjs';
import { BrowserUtil } from '@shared/util/browser-util';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-support-logs-edit',
  templateUrl: './support-logs-edit.component.html',
  styleUrls: ['./support-logs-edit.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SupportLogsEditComponent
  extends EditBaseDirective<SysLogDTO, SupportLogsService, SupportLogsForm>
  implements OnInit
{
  service = inject(SupportLogsService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Manage_Support_Logs_Edit, {
      useLegacyToolbar: true,
    });
    this.form?.disable();
  }

  loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service
        .get(this.form?.getIdControl()?.value)
        .pipe(tap(value => this.form?.reset(value)))
    );
  }

  override createLegacyEditToolbar(): void {
    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: ['fal', 'download'],
          label: 'common.download',
          title: 'common.download',
          onClick: () => {
            this.downloadFile();
          },
        }),
      ],
    });
  }

  downloadFile() {
    BrowserUtil.openInSameTab(
      window,
      '/soe/manage/support/logs/edit/download/?sysLogId=' +
        this.form?.value[this.idFieldName]
    );
  }
}
