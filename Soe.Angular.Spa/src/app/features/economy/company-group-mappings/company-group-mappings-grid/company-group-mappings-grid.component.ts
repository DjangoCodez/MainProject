import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';
import { CompanyGroupMappingHeadDTO } from '../models/company-group-mappings.model';
import { CompanyGroupMappingsService } from '../services/company-group-mappings.service';

@Component({
  selector: 'soe-company-group-mappings-grid',
  templateUrl:
    '../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CompanyGroupMappingsGridComponent
  extends GridBaseDirective<
    CompanyGroupMappingHeadDTO,
    CompanyGroupMappingsService
  >
  implements OnInit
{
  service = inject(CompanyGroupMappingsService);
  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Accounting_CompanyGroup_TransferDefinitions,
      'economy.accounting.companygroup.mappings',
      { skipInitialLoad: true }
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<CompanyGroupMappingHeadDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.number',
        'common.name',
        'common.description',
        'core.edit',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('number', terms['common.number'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 1,
          enableHiding: false,
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

  override onFinished(): void {
    this.refreshGrid();
  }
}
