import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ITimeDeviationCauseGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take } from 'rxjs';
import { TimeDeviationCausesService } from '../../services/time-deviation-causes.service';

@Component({
  selector: 'soe-time-deviation-causes-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  styleUrl: './time-deviation-causes-grid.component.scss',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeDeviationCausesGridComponent
  extends GridBaseDirective<
    ITimeDeviationCauseGridDTO,
    TimeDeviationCausesService
  >
  implements OnInit
{
  service = inject(TimeDeviationCausesService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_TimeSettings_TimeDeviationCause,
      'Time.Time.TimeDeviationCauses',
      {
        lookups: [this.loadTypes()],
      }
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<ITimeDeviationCauseGridDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.description',
        'core.edit',
        'common.type',
        'time.time.timedeviationcause.timecode',
        'time.time.timedeviationcause.validforstandby',
        'time.time.timedeviationcause.validforhibernating',
        'time.time.timedeviationcause.candidateforovertime',
        'time.time.timedeviationcause.validforstandby.short',
        'time.time.timedeviationcause.validforhibernating.short',
        'time.time.timedeviationcause.candidateforovertime.short',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnSelect(
          'type',
          terms['common.type'],
          this.service.performTypes.data || [],
          undefined,
          {
            flex: 10,
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            editable: false,
          }
        );
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 20,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 30,
        });
        this.grid.addColumnText(
          'timeCodeName',
          terms['time.time.timedeviationcause.timecode'],
          {
            flex: 20,
          }
        );
        this.grid.addColumnBool(
          'validForStandby',
          terms['time.time.timedeviationcause.validforstandby.short'],
          {
            tooltip: terms['time.time.timedeviationcause.validforstandby'],
            flex: 10,
          }
        );
        this.grid.addColumnBool(
          'validForHibernating',
          terms['time.time.timedeviationcause.validforhibernating.short'],
          {
            tooltip: terms['time.time.timedeviationcause.validforhibernating'],
            flex: 10,
          }
        );
        this.grid.addColumnBool(
          'candidateForOvertime',
          terms['time.time.timedeviationcause.candidateforovertime.short'],
          {
            tooltip: terms['time.time.timedeviationcause.candidateforovertime'],
            flex: 10,
          }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  private loadTypes(): Observable<any> {
    return this.service.getTypesDict();
  }
}
