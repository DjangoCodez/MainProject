import { Component, OnInit, inject } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  InventoryWriteOffTemplatesDTO,
  SaveInventoryWriteOffTemplateModel,
} from '../../models/inventory-write-off-templates.model';
import { InventoryWriteOffTemplatesService } from '../../services/inventory-write-off-templates.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { Observable, tap } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Perform } from '@shared/util/perform.class';
import { InventoryWriteOffMethodsService } from '../../../inventory-write-off-methods/services/inventory-write-off-methods.service';
import { VoucherSeriesTypeService } from '../../../services/voucher-series-type.service';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { CrudActionTypeEnum } from '@shared/enums';
import { InventoryWriteOffTemplateForm } from '../../models/inventory-write-off-templates-form.model';
import { IAccountingSettingsRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TermCollection } from '@shared/localization/term-types';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { InventoriesService } from '@features/economy/inventories/services/inventories.service';

@Component({
  selector: 'soe-inventory-write-off-templates-edit',
  templateUrl: './inventory-write-off-templates-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class InventoryWriteOffTemplatesEditComponent
  extends EditBaseDirective<
    InventoryWriteOffTemplatesDTO,
    InventoryWriteOffTemplatesService,
    InventoryWriteOffTemplateForm
  >
  implements OnInit
{
  service = inject(InventoryWriteOffTemplatesService);
  writeOffMethodsService = inject(InventoryWriteOffMethodsService);
  voucherSeriesTypeService = inject(VoucherSeriesTypeService);
  coreService = inject(CoreService);
  inventoryService = inject(InventoriesService);

  accountSettingTypes: SmallGenericType[] = [];
  baseAccounts: SmallGenericType[] = [];
  writeOffMethods: SmallGenericType[] = [];
  voucherSeriesTypes: SmallGenericType[] = [];
  inventoryWriteOffTemplate?: InventoryWriteOffTemplatesDTO;

  perform = new Perform<any>(this.progressService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Inventory_WriteOffTemplates_Edit, {
      lookups: [
        this.loadWriteOffMethods(),
        this.loadVoucherSeriesTypes(),
        this.loadBaseAccounts(),
      ],
    });
  }

  loadWriteOffMethods(): Observable<SmallGenericType[]> {
    return this.perform.load$(
      this.writeOffMethodsService.getDict(false).pipe(
        tap(value => {
          this.writeOffMethods = value;
        })
      )
    );
  }

  loadVoucherSeriesTypes(): Observable<SmallGenericType[]> {
    return this.perform.load$(
      this.voucherSeriesTypeService.getVoucherSeriesTypesByCompany().pipe(
        tap(value => {
          this.voucherSeriesTypes = value;
        })
      )
    );
  }

  override performSave(): void {
    if (!this.form || this.form.invalid || !this.service) return;

    this.inventoryWriteOffTemplate = {
      inventoryWriteOffMethodId: this.form.inventoryWriteOffMethodId.value,
      name: this.form.value.name,
      description: this.form.value.description,
      voucherSeriesTypeId: this.form.voucherSeriesTypeId.value,
    } as InventoryWriteOffTemplatesDTO;
    const model = new SaveInventoryWriteOffTemplateModel();

    model.inventoryWriteOffTemplate = this.form.isNew
      ? this.inventoryWriteOffTemplate
      : this.form.value;
    model.accountSettings = this.form?.accountingSettings.value;

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model).pipe(tap(this.updateFormValueAndEmitChange))
    );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.inventoryWriteOffTemplate = value;
          this.form?.customPatch(value);
        })
      )
    );
  }

  override loadTerms(): Observable<TermCollection> {
    return super
      .loadTerms([
        'economy.inventory.inventoryaccountsettingtype.inventory',
        'economy.inventory.inventoryaccountsettingtype.accwriteoff',
        'economy.inventory.inventoryaccountsettingtype.writeoff',
        'economy.inventory.inventoryaccountsettingtype.accoverwriteoff',
        'economy.inventory.inventoryaccountsettingtype.overwriteoff',
        'economy.inventory.inventoryaccountsettingtype.accwritedown',
        'economy.inventory.inventoryaccountsettingtype.writedown',
        'economy.inventory.inventoryaccountsettingtype.accwriteup',
        'economy.inventory.inventoryaccountsettingtype.writeup',
      ])
      .pipe(
        tap((terms: TermCollection) => {
          // Must be run after terms are retrieved. onFinished doesn't guarantee that terms are loaded before executing.
          this.loadSettingsTypes(terms);
        })
      );
  }

  loadSettingsTypes(terms: TermCollection) {
    this.accountSettingTypes = this.inventoryService.getSettingsTypes(terms);
  }

  private loadBaseAccounts(): Observable<SmallGenericType[]> {
    return this.inventoryService.getBaseAccounts().pipe(
      tap(result => {
        this.baseAccounts = result;
      })
    );
  }

  accountSettingsChanged(rows: IAccountingSettingsRowDTO[]) {
    this.form?.accountingSettings.rawPatch(rows);
  }
}
