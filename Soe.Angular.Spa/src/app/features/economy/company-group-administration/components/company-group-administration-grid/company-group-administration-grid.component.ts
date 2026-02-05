import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ICompanyGroupAdministrationGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs';
import { CompanyGroupAdministrationService } from '../../services/company-group-administration.service';

@Component({
  selector: 'soe-company-group-administration-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CompanyGroupAdministrationGridComponent
  extends GridBaseDirective<
    ICompanyGroupAdministrationGridDTO,
    CompanyGroupAdministrationService
  >
  implements OnInit
{
  service = inject(CompanyGroupAdministrationService);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Accounting_CompanyGroup_Companies,
      'Economy.Accounting.Companygroup.Companies'
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<ICompanyGroupAdministrationGridDTO>
  ): void {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'economy.accounting.companygroup.companynr',
        'common.name',
        'economy.accounting.companygroup.mapping',
        'economy.accounting.companygroup.conversionfactor',
        'core.edit',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnNumber(
          'childCompanyNr',
          terms['economy.accounting.companygroup.companynr'],
          {
            flex: 1,
            enableHiding: false,
          }
        );
        this.grid.addColumnText('childCompanyName', terms['common.name'], {
          flex: 1,
        });
        this.grid.addColumnText(
          'mappingHeadName',
          terms['economy.accounting.companygroup.mapping'],
          { flex: 1 }
        );
        this.grid.addColumnNumber(
          'conversionfactor',
          terms['economy.accounting.companygroup.conversionfactor'],
          { flex: 1, decimals: 4 }
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
}
