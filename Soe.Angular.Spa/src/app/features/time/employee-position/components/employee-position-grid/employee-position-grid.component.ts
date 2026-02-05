import { Component, OnInit, inject, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  IPositionDTO,
  IPositionGridDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import { take, tap } from 'rxjs/operators';
import { EmployeePositionService } from '../../services/employee-position.service';

export enum FunctionType {
  Update = 1,
}

@Component({
  selector: 'soe-employee-position-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EmployeePositionGridComponent
  extends GridBaseDirective<IPositionGridDTO, EmployeePositionService>
  implements OnInit
{
  service = inject(EmployeePositionService);
  progressService = inject(ProgressService);
  performLoad = new Perform<IPositionGridDTO[]>(this.progressService);

  override ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Employee_Positions,
      'time.employee.position.positions'
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
              id: FunctionType.Update,
              label: this.translate.instant(
                'time.employee.position.updatepositions'
              ),
            },
          ]),
          onItemSelected: event => this.onFunctionSelected(event.value),
        }),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<IPositionGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.code',
        'common.name',
        'common.description',
        'time.employee.position.link',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 10,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 30,
          enableHiding: true,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 60,
          enableHiding: true,
        });
        this.grid.addColumnIcon('', '', {
          width: 22,
          iconName: 'link',
          tooltip: terms['time.employee.position.link'],
          showIcon: row => (row.sysPositionId ? row.sysPositionId > 0 : false),
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

  override loadData(id?: number): Observable<IPositionGridDTO[]> {
    return super.loadData(id, { loadSkills: true });
  }

  onFunctionSelected(selectedItem: MenuButtonItem): void {
    switch (selectedItem.id) {
      case FunctionType.Update:
        this.update();
        break;
    }
  }

  update() {
    return this.performLoad.crud(
      CrudActionTypeEnum.Save,
      this.service
        .updatePositionGrid(
          this.grid.getSelectedRows() as unknown as IPositionDTO
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
}
