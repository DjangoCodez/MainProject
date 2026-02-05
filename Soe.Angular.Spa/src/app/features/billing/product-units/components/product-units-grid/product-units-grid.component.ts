import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take } from 'rxjs';
import { ProductUnitSmallDTO } from '../../models/product-units.model';
import { ProductUnitService } from '../../services/product-unit.service';

@Component({
  selector: 'soe-product-units-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProductUnitsGridComponent
  extends GridBaseDirective<ProductUnitSmallDTO, ProductUnitService>
  implements OnInit
{
  service = inject(ProductUnitService);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Preferences_ProductSettings_ProductUnit_Edit,
      'billing.product.productunit.productunits'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<ProductUnitSmallDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get(['common.code', 'common.name', 'core.edit'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
      });
    super.finalizeInitGrid();
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: {
      useCache: boolean;
      cacheExpireTime?: number;
    }
  ): Observable<ProductUnitSmallDTO[]> {
    return super.loadData(id, additionalProps);
  }
}
