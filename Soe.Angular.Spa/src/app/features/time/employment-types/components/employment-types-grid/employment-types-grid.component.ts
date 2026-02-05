import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { IEmploymentTypeGridDTO } from '@shared/models/generated-interfaces/EmploymentTypeDTO';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { of } from 'rxjs';
import { take, tap } from 'rxjs/operators';
import { EmploymentTypesService } from '../../services/employment-types.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-employment-types-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EmploymentTypesGridComponent
  extends GridBaseDirective<IEmploymentTypeGridDTO, EmploymentTypesService>
  implements OnInit
{
  service = inject(EmploymentTypesService);
  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  performAction = new Perform<any>(this.progressService);
  hasCompanySettingsModifyPermission = false;

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(
      Feature.Time_Employee_EmploymentTypes,
      'Time.Employee.EmploymentTypes',
      { lookups: [this.loadAdditionalPermissions()] }
    );
  }

  override createGridToolbar(): void {
    return super.createGridToolbar({
      useDefaltSaveOption: this.hasCompanySettingsModifyPermission,
    });
  }

  loadAdditionalPermissions() {
    return of(
      this.coreService
        .hasModifyPermissions([Feature.Time_Preferences_CompSettings])
        .subscribe(response => {
          this.hasCompanySettingsModifyPermission =
            response[Feature.Time_Preferences_CompSettings];
        })
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IEmploymentTypeGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.active',
        'common.code',
        'common.description',
        'common.externalcode',
        'common.name',
        'common.type',
        'core.edit',
        'time.employee.employmenttype.excludefromworktimeweekcalculationonsecondaryemployment',
        'time.employee.employmenttype.standardorown',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnActive('state', terms['common.active'], {
          idField: 'gridId',
          resizable: false,
          editable:
            this.flowHandler.modifyPermission() &&
            this.hasCompanySettingsModifyPermission,
          showCheckbox: row => {
            return (row.gridId || 0) != 0;
          },
        });
        this.grid.addColumnText('typeName', terms['common.type'], {
          flex: 10,
        });
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 5,
          enableHiding: true,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 10,
          enableHiding: true,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 20,
          enableHiding: true,
        });
        this.grid.addColumnText('externalCode', terms['common.externalcode'], {
          flex: 5,
          enableHiding: true,
        });
        this.grid.addColumnBool(
          'excludeFromWorkTimeWeekCalculationOnSecondaryEmployment',
          terms[
            'time.employee.employmenttype.excludefromworktimeweekcalculationonsecondaryemployment'
          ],
          { flex: 25, enableHiding: true }
        );
        this.grid.addColumnText(
          'standardText',
          terms['time.employee.employmenttype.standardorown'],
          {
            flex: 10,
            enableHiding: true,
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
        .updateEmploymentTypesState(this.grid.selectedItemsService.toDict())
        .pipe(
          tap((response: BackendResponse) => {
            if (response.success) {
              this.refreshGrid();
            }
          })
        )
    );
  }
}
