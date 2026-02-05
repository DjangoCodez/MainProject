import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SieService } from '../../../services/sie.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take } from 'rxjs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { CrudActionTypeEnum } from '@shared/enums';
import { FileImportHeadGridDTO } from '../../../models/sie-import-history.model';

@Component({
  selector: 'soe-sie-import-history-grid',
  templateUrl: './sie-import-history-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SieImportHistoryGridComponent
  extends GridBaseDirective<FileImportHeadGridDTO, SieService>
  implements OnInit
{
  private readonly messageBoxService = inject(MessageboxService);
  private readonly dialogService = inject(DialogService);
  private readonly performSave = new Perform(this.progressService);
  service = inject(SieService);
  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Import_Sie, 'Economy.Sie.Imports.History', {
      skipInitialLoad: true,
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<FileImportHeadGridDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.filename',
        'common.created',
        'common.createdby',
        'common.modified',
        'common.modified',
        'common.comment',
        'common.message',
        'common.status',
        'economy.import.sie.reverse',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        const defaultOptions = {
          enableHiding: false,
          suppressFilter: true,
          flex: 1,
          suppressSizeToFit: true,
          minWidth: 100,
        };
        this.grid.addColumnText(
          'fileName',
          terms['common.filename'],
          defaultOptions
        );
        this.grid.addColumnDateTime(
          'created',
          terms['common.created'],
          defaultOptions
        );
        this.grid.addColumnText(
          'createdBy',
          terms['common.createdby'],
          defaultOptions
        );
        this.grid.addColumnDateTime(
          'modified',
          terms['common.modified'],
          defaultOptions
        );
        this.grid.addColumnText(
          'modifiedBy',
          terms['common.modifiedby'],
          defaultOptions
        );
        this.grid.addColumnText(
          'statusStr',
          terms['common.status'],
          defaultOptions
        );
        this.grid.addColumnIcon('comment', '', {
          width: 60,
          tooltip: terms['common.comment'],
          columnSeparator: true,
          iconName: 'comment',
          showIcon: row => !!row.comment?.length,
          onClick: row =>
            this.messageBoxService.information('core.info', row.comment),
        });
        this.grid.addColumnIcon('systemMessage', '', {
          width: 60,
          tooltip: terms['common.message'],
          columnSeparator: true,
          iconName: 'triangle-exclamation',
          iconClass: 'color-warning',
          showIcon: row => !!row.systemMessage?.length,
          onClick: row =>
            this.messageBoxService.warning('core.warning', row.systemMessage),
        });

        this.grid.addColumnIcon('', '', {
          width: 60,
          iconName: 'arrows-rotate-reverse',
          tooltip: terms['economy.import.sie.reverse'],
          pinned: 'right',
          showIcon: row => !!row?.showReverseButton,
          onClick: row => this.triggerReverseImport(row),
        });

        super.finalizeInitGrid();

        this.refreshGrid();
      });
  }

  override loadData(): Observable<FileImportHeadGridDTO[]> {
    return this.performLoadData.load$(this.service.getImportHistory());
  }

  private triggerReverseImport(row: FileImportHeadGridDTO): void {
    const msgBox = this.messageBoxService.warning(
      'economy.import.sie.reverse.confirm.title',
      'economy.import.sie.reverse.confirm.message',
      {
        size: 'lg',
        hideCloseButton: true,
        showInputText: true,
        inputTextLabel: 'common.comment',
        buttons: 'okCancel',
      }
    );

    msgBox.afterClosed().subscribe(result => {
      if (result?.result) {
        const reverseRequest = {
          fileImportHeadId: row.fileImportHeadId,
          comment: result.textValue,
        };

        this.performSave.crud(
          CrudActionTypeEnum.Save,
          this.service.reverseImport(reverseRequest),
          () => {
            this.refreshGrid();
          }
        );
      }
    });
  }
}
