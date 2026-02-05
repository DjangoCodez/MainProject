import { Component, inject, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { ITimeScheduleTypeGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import { take, tap } from 'rxjs/operators';
import { TimeScheduleTypeService } from '../../services/time-schedule-type.service';

@Component({
  selector: 'soe-time-schedule-type-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeScheduleTypeGridComponent
  extends GridBaseDirective<ITimeScheduleTypeGridDTO, TimeScheduleTypeService>
  implements OnInit
{
  service = inject(TimeScheduleTypeService);
  progressService = inject(ProgressService);
  performAction = new Perform<any>(this.progressService);

  private readonly coreService = inject(CoreService);

  hideShowInTerminal = signal(false);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_TimeSettings_TimeScheduleType,
      'Time.Schedule.ScheduleTypes'
    );
  }

  override loadCompanySettings(): Observable<any> {
    return this.coreService
      .getCompanySettings([
        CompanySettingType.PossibilityToRegisterAdditionsInTerminal,
      ])
      .pipe(
        tap(x => {
          this.hideShowInTerminal.set(
            !SettingsUtil.getBoolCompanySetting(
              x,
              CompanySettingType.PossibilityToRegisterAdditionsInTerminal
            )
          );
        })
      );
  }

  override createGridToolbar(): void {
    return super.createGridToolbar({
      useDefaltSaveOption: true,
    });
  }

  override onGridReadyToDefine(grid: GridComponent<ITimeScheduleTypeGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'core.edit',
        'common.active',
        'common.code',
        'common.name',
        'common.description',
        'common.all',
        'time.schedule.scheduletype.bilagaj',
        'time.schedule.scheduletype.showinterminal',
        'time.schedule.scheduletype.replacewithdeviationcause',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnActive('state', terms['common.active'], {
          idField: 'timeScheduleTypeId',
          editable: true,
        });
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 12,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 12,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 22,
        });
        this.grid.addColumnBool('isAll', terms['common.all'], {
          flex: 4,
        });
        this.grid.addColumnBool(
          'isBilagaJ',
          terms['time.schedule.scheduletype.bilagaj'],
          {
            flex: 6,
          }
        );
        if (!this.hideShowInTerminal()) {
          this.grid.addColumnBool(
            'showInTerminal',
            terms['time.schedule.scheduletype.showinterminal'],
            {
              flex: 16,
            }
          );
        }
        this.grid.addColumnText(
          'timeDeviationCauseName',
          terms['time.schedule.scheduletype.replacewithdeviationcause'],
          {
            flex: 28,
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

  override saveStatus(): void {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service
        .updateTimeScheduleTypesState(this.grid.selectedItemsService.toDict())
        .pipe(
          tap((response: any) => {
            if (response.success) this.refreshGrid();
          })
        )
    );
  }
}
