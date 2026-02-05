import { Component, inject, OnInit, signal } from '@angular/core';
import { PositionsService } from '@features/manage/positions/services/positions.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IPositionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ISysPositionGridDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { GridComponent } from '@ui/grid/grid.component';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import { EmployeePositionService } from '../../services/employee-position.service';

export enum FunctionType {
  copy = 1,
  copyAndLink = 2,
}

@Component({
  selector: 'soe-employee-system-position-grid',
  templateUrl: 'employee-system-position-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EmployeeSystemPositionGridComponent
  extends GridBaseDirective<ISysPositionGridDTO>
  implements OnInit
{
  progressService = inject(ProgressService);
  positionService = inject(PositionsService);
  employeePositionService = inject(EmployeePositionService);
  performAction = new Perform<unknown>(this.progressService);

  disableButtonFunction = signal(true);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_Positions,
      'time.employee.position.syspositions'
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar();

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarMenuButton('menuButton', {
          tooltip: signal('core.functions'),
          caption: signal('core.functions'),
          list: signal<MenuButtonItem[]>([
            {
              id: FunctionType.copy,
              label: this.translate.instant(
                'time.employee.position.updatesyspositions'
              ),
            },
            {
              id: FunctionType.copyAndLink,
              label: this.translate.instant(
                'time.employee.position.updateandlinksyspositions'
              ),
            },
          ]),
          disabled: this.disableButtonFunction,
          onItemSelected: event => this.onFunctionSelected(event.value),
        }),
      ],
    });
  }

  onFunctionSelected(selected: MenuButtonItem): void {
    switch (selected.id) {
      case FunctionType.copy:
        this.copy();
        break;
      case FunctionType.copyAndLink:
        this.copyAndLink();
        break;
    }
  }

  copy() {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.employeePositionService
        .updateSysPositionGrid(
          this.grid.getSelectedRows() as unknown as IPositionDTO[]
        )
        .pipe(
          tap(result => {
            if (result.success) this.refreshGrid();
          })
        ),
      undefined,
      undefined,
      {
        showToastOnComplete: false,
      }
    );
  }

  copyAndLink() {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.employeePositionService
        .updateAndLinkSysPositionGrid(
          this.grid.getSelectedRows() as unknown as IPositionDTO[]
        )
        .pipe(
          tap(result => {
            if (result.success) this.refreshGrid();
          })
        ),
      undefined,
      undefined,
      {
        showToastOnComplete: false,
      }
    );
  }

  selectionChanged(data: ISysPositionGridDTO[]) {
    this.disableButtonFunction.set(data.length == 0);
  }

  override onGridReadyToDefine(grid: GridComponent<ISysPositionGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'time.employee.position.ssyk',
        'time.employee.position.countrycode',
        'time.employee.position.langcode',
        'time.employee.position.updatepositions',
        'time.employee.position.updatepositions.error',
        'time.employee.position.updatesyspositions',
        'time.employee.position.updateandlinksyspositions',
        'time.employee.position.updateandlinksyspositions.error',
        'time.employee.position.link',
        'common.description',
        'common.name',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();

        this.grid.addColumnText(
          'sysCountryCode',
          terms['time.employee.position.countrycode'],
          {
            flex: 15,
            enableHiding: true,
          }
        );
        this.grid.addColumnText(
          'sysLanguageCode',
          terms['time.employee.position.langcode'],
          {
            flex: 15,
            enableHiding: true,
          }
        );
        this.grid.addColumnText('code', terms['time.employee.position.ssyk'], {
          flex: 15,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 25,
          enableHiding: true,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 30,
          enableHiding: true,
        });
        this.grid.addColumnIcon('isLinked', '', {
          width: 22,
          iconName: 'link',
          tooltip: terms['time.employee.position.link'],
          showIcon: row => row.isLinked,
        });

        this.grid.onSelectionChanged((event: any) =>
          this.selectionChanged(event)
        );

        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: any
  ): Observable<ISysPositionGridDTO[]> {
    return this.positionService.getPositions(
      SoeConfigUtil.sysCountryId,
      SoeConfigUtil.languageId
    );
  }
}
