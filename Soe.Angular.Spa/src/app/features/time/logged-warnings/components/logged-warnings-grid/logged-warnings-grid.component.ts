import { Component, OnInit, inject, signal, effect } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { IWorkRuleBypassLogGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { LoggedWarningsService } from '../../services/logged-warnings.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { GridComponent } from '@ui/grid/grid.component';
import { Feature, TermGroup_GridDateSelectionType } from '@shared/models/generated-interfaces/Enumerations';
import { Observable, take } from 'rxjs';

@Component({
  selector: 'soe-logged-warnings-grid',
  templateUrl: './logged-warnings-grid.component.html',
  standalone: false,
  providers: [FlowHandlerService, ToolbarService],
})
export class LoggedWarningsGridComponent
  extends GridBaseDirective<IWorkRuleBypassLogGridDTO, LoggedWarningsService>
  implements OnInit
{
  service = inject(LoggedWarningsService);
  dateSelection = signal(TermGroup_GridDateSelectionType.One_Day);

  private dateSelectionEffect = effect(() => {
    this.dateSelection();
    if (this.grid) {
      this.refreshGrid();
    }
  });

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Schedule_SchedulePlanning_LoggedWarnings,
      'Time.Schedule.LoggedWarnings'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IWorkRuleBypassLogGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.date',
        'common.employee',
        'time.schedule.workrulebypass.messagetext',
        'time.schedule.workrulebypass.actiontext',
        'time.schedule.workrulebypass.createdby',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnDate('date', terms['common.date'], { flex: 15 });
        this.grid.addColumnText('employeeNrAndName', terms['common.employee'], {
          flex: 20,
        });
        this.grid.addColumnText(
          'message',
          terms['time.schedule.workrulebypass.messagetext'],
          {
            flex: 45,
          }
        );
        this.grid.addColumnText(
          'actionText',
          terms['time.schedule.workrulebypass.actiontext'],
          {
            flex: 10,
          }
        );
        this.grid.addColumnText(
          'createdBy',
          terms['time.schedule.workrulebypass.createdby'],
          {
            flex: 10,
          }
        );

        this.grid.options = {
          ...this.grid.options,
          rowSelection: undefined,
        };

        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number,
    additionalProps?: any
  ): Observable<IWorkRuleBypassLogGridDTO[]> {
    return this.performLoadData.load$(
      this.service.getGrid(id, { dateSelection: this.dateSelection() }),
      { showDialogDelay: 1000 }
    );
  }
}
