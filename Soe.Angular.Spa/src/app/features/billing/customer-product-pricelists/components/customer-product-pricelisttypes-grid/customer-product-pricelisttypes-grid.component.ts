import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IPriceListTypeGridDTO } from '@shared/models/generated-interfaces/PriceListTypeDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { IconUtil } from '@shared/util/icon-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { of, take } from 'rxjs';
import { CustomerProductPricelistsTypeService } from '../../services/customer-product-priceliststype.service';
import {
  PriceListUpdateComponent,
  PriceListUpdateDialogData,
} from './pricelist-update-modal/pricelist-update-modal.component';

@Component({
  selector: 'soe-customer-product-pricelisttypes-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CustomerProductPriceListTypesGridComponent
  extends GridBaseDirective<
    IPriceListTypeGridDTO,
    CustomerProductPricelistsTypeService
  >
  implements OnInit
{
  hasPriceUpdatePermission = signal(false);
  disablePriceUpdate = signal(true);
  hidePriceUpdate = computed(() => !this.hasPriceUpdatePermission());
  coreService = inject(CoreService);
  dialogService = inject(DialogService);
  service = inject(CustomerProductPricelistsTypeService);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Preferences_InvoiceSettings_Pricelists,
      'Billing.Customer.Product.PriceListTypes',
      {
        skipInitialLoad: false,
        useLegacyToolbar: true,
        lookups: [this.loadPriceUpdatePermission()],
      }
    );
  }

  override createLegacyGridToolbar(): void {
    super.createLegacyGridToolbar();
    this.toolbarUtils.createLegacyGroup({
      buttons: [
        this.toolbarUtils.createLegacyButton({
          icon: IconUtil.createIcon('fal', 'chart-mixed-up-circle-dollar'),
          title: 'billing.product.pricelist.priceadjustment',
          onClick: () => this.openPriceEditModal(),
          disabled: this.disablePriceUpdate,
          hidden: this.hidePriceUpdate,
        }),
      ],
    });
  }

  onGridReadyToDefine(grid: GridComponent<IPriceListTypeGridDTO>) {
    super.onGridReadyToDefine(grid);
    this.grid.setRowSelection('multiRow');

    this.translate
      .get([
        'common.name',
        'common.description',
        'common.currency',
        'common.incvat',
        'billing.projects.list.projectpricelist',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 20,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 30,
        });
        this.grid.addColumnText('currency', terms['common.currency'], {
          flex: 5,
        });
        this.grid.addColumnBool('inclusiveVat', terms['common.incvat'], {
          flex: 2,
        });
        this.grid.addColumnBool(
          'isProjectPriceList',
          terms['billing.projects.list.projectpricelist'],
          { enableHiding: true, flex: 2 }
        );
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => this.edit(row),
        });

        this.grid.selectionChanged.subscribe(data =>
          this.selectionChanged(data)
        );
        super.finalizeInitGrid();
      });
  }

  loadPriceUpdatePermission() {
    return of(
      this.coreService
        .hasModifyPermissions([
          Feature.Billing_Preferences_InvoiceSettings_Pricelists_PriceUpdate,
        ])
        .subscribe(res => {
          this.hasPriceUpdatePermission.set(
            res[
              Feature.Billing_Preferences_InvoiceSettings_Pricelists_PriceUpdate
            ]
          );
        })
    );
  }

  openPriceEditModal() {
    this.dialogService.open(PriceListUpdateComponent, {
      title: 'billing.product.pricelist.priceadjustment',
      size: 'lg',
      selectedRows: this.grid.getSelectedRows().map(x => x.priceListTypeId),
    } as PriceListUpdateDialogData);
  }

  selectionChanged(data: IPriceListTypeGridDTO[]) {
    this.disablePriceUpdate.set(data.length === 0);
  }
}
