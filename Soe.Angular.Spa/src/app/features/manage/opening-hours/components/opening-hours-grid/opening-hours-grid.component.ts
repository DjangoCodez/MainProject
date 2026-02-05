import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IOpeningHoursGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { OpeningHoursService } from '../../services/opening-hours.service';

@Component({
  selector: 'soe-opening-hours-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class OpeningHoursGridComponent
  extends GridBaseDirective<IOpeningHoursGridDTO, OpeningHoursService>
  implements OnInit
{
  service = inject(OpeningHoursService);

  ngOnInit() {
    this.startFlow(
      Feature.Manage_Preferences_Registry_OpeningHours,
      'Manage.Registry.OpeningHours',
      {
        lookups: [this.loadWeekDays()],
      }
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IOpeningHoursGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'manage.registry.openinghours.description',
        'manage.registry.openinghours.standardweekday',
        'manage.registry.openinghours.specificdate',
        'manage.registry.openinghours.openingtime',
        'manage.registry.openinghours.closingtime',
        'common.validfrom',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 25,
        });
        this.grid.addColumnText(
          'description',
          terms['manage.registry.openinghours.description'],
          {
            flex: 25,
          }
        );
        this.grid.addColumnSelect(
          'standardWeekDay',
          terms['manage.registry.openinghours.standardweekday'],
          this.service.performWeekDays.data || [],
          undefined,
          {
            flex: 20,
          }
        );
        this.grid.addColumnDate(
          'specificDate',
          terms['manage.registry.openinghours.specificdate'],
          {
            flex: 15,
          }
        );
        this.grid.addColumnTime(
          'openingTime',
          terms['manage.registry.openinghours.openingtime'],
          {
            dateFormat: 'HH:mm',
            width: 100,
          }
        );
        this.grid.addColumnTime(
          'closingTime',
          terms['manage.registry.openinghours.closingtime'],
          {
            dateFormat: 'HH:mm',
            width: 100,
          }
        );
        this.grid.addColumnDate('fromDate', terms['common.validfrom'], {
          flex: 10,
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  private loadWeekDays(): Observable<SmallGenericType[]> {
    return this.service.getWeekdaysDict();
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: { fromDate: string; toDate: string }
  ): Observable<IOpeningHoursGridDTO[]> {
    return super.loadData(id, {
      fromDate: '',
      toDate: '',
    });
  }
}
