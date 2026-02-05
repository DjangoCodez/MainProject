import { Component, inject, OnInit } from '@angular/core';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IProductGroupGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { take } from 'rxjs/operators';
import { ProductGroupsService } from '../../services/product-groups.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
@Component({
  selector: 'soe-product-groups-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProductGroupsGridComponent
  extends GridBaseDirective<IProductGroupGridDTO, ProductGroupsService>
  implements OnInit
{
  service = inject(ProductGroupsService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Preferences_ProductSettings_ProductGroup,
      'Billing.Invoices.Productgroups'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IProductGroupGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.name', 'common.code', 'core.edit'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('code', terms['common.code'], { flex: 1 });
        this.grid.addColumnText('name', terms['common.name'], { flex: 1 });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        this.exportFilenameKey.set(
          'billing.invoices.productgroups.productgroup'
        );
        super.finalizeInitGrid();
      });
  }
}
