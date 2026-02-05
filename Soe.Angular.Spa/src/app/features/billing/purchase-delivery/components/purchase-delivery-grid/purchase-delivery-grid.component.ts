import { Component, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  SettingMainType,
  TermGroup,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import { IPurchaseDeliveryGridDTO } from '@shared/models/generated-interfaces/PurchaseDeliveryDTOs ';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable } from 'rxjs';
import { take, tap } from 'rxjs/operators';
import { PurchaseDeliveryForm } from '../../models/purchase-delivery-form.model';
import { PurchaseDeliveryDTO } from '../../models/purchase-delivery.model';
import { PurchaseDeliveryService } from '../../services/purchase-delivery.service';

@Component({
  selector: 'soe-purchase-delivery-grid',
  templateUrl: './purchase-delivery-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PurchaseDeliveryGridComponent
  extends GridBaseDirective<IPurchaseDeliveryGridDTO, PurchaseDeliveryService>
  implements OnInit
{
  progressService = inject(ProgressService);
  service = inject(PurchaseDeliveryService);
  coreService = inject(CoreService);
  validationHandler = inject(ValidationHandler);
  performLoadSelectionTypes = new Perform<SmallGenericType[]>(
    this.progressService
  );
  performGridLoad = new Perform<IPurchaseDeliveryGridDTO[]>(
    this.progressService
  );
  performLoad = new Perform<unknown>(this.progressService);

  form: PurchaseDeliveryForm = new PurchaseDeliveryForm({
    validationHandler: this.validationHandler,
    element: new PurchaseDeliveryDTO(),
  });

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Purchase_Delivery_List,
      'billing.purchase.delivery.deliveries',
      {
        lookups: [this.loadSelectionTypes()],
      }
    );
    this.form.valueChanges.subscribe(() => this.changeSelectedType());
  }

  override onGridReadyToDefine(grid: GridComponent<IPurchaseDeliveryGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'billing.purchase.delivery.deliveryno',
        'billing.purchase.delivery.deliverydate',
        'billing.purchase.supplierno',
        'billing.purchase.suppliername',
        'billing.purchase.purchasenr',
        'billing.purchase.delivery.createddate',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText(
          'deliveryNr',
          terms['billing.purchase.delivery.deliveryno'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnDate(
          'deliveryDate',
          terms['billing.purchase.delivery.deliverydate'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'supplierNr',
          terms['billing.purchase.supplierno'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'supplierName',
          terms['billing.purchase.suppliername'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'purchaseNr',
          terms['billing.purchase.purchasenr'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnDate(
          'created',
          terms['billing.purchase.delivery.createddate'],
          {
            flex: 1,
          }
        );

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            // We need to react to this in the edit component
            const additionalProps = {
              id: row.purchaseDeliveryId,
            };
            this.edit(row, additionalProps);
          },
        });
        super.finalizeInitGrid();
      });
  }

  loadSelectionTypes() {
    return this.performLoadSelectionTypes.load$(
      this.coreService.getTermGroupContent(
        TermGroup.ChangeStatusGridAllItemsSelection,
        false,
        true,
        true
      )
    );
  }

  changeSelectedType() {
    const model = {
      settingMainType: SettingMainType.User,
      settingTypeId: UserSettingType.BillingPurchaseAllItemsSelection,
      intValue: this.form.value.deliveryType,
    };
    this.saveIntSetting(model);
  }

  override loadData(
    id?: number | undefined
  ): Observable<IPurchaseDeliveryGridDTO[]> {
    return this.performGridLoad.load$(
      this.service.getGrid(undefined, {
        selectedId: this.form.value.deliveryType,
      })
    );
  }

  saveIntSetting(model: unknown) {
    this.performLoad.load(
      this.coreService.saveIntSetting(model).pipe(
        tap(() => {
          this.refreshGrid();
        })
      )
    );
  }
}
