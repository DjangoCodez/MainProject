import { Component, OnInit, inject } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { StockDTO } from '../../models/stock-warehouse.model';
import { StockWarehouseService } from '../../services/stock-warehouse.service';
import {
  CompanySettingType,
  Feature,
  ProductAccountType,
  TermGroup_SysContactAddressType,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable, of, tap } from 'rxjs';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { EconomyService } from '@features/economy/services/economy.service';
import { StockWarehouseForm } from '../../models/stock-warehouse-form.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { IContactAddressDTO } from '@shared/models/generated-interfaces/ContactDTO';
import { ProgressOptions } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import { StockWarehouseEditGridComponent } from './stock-warehouse-edit-grid/stock-warehouse-edit-grid.component';
import { IAccountingSettingsRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { StockWarehouseValidatorService } from '../../services/stock-warehouse-validator.service';
import { TermCollection } from '@shared/localization/term-types';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-stock-warehouse-edit',
  templateUrl: './stock-warehouse-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class StockWarehouseEditComponent
  extends EditBaseDirective<StockDTO, StockWarehouseService, StockWarehouseForm>
  implements OnInit
{
  // @ViewChild(StockWarehouseEditGridComponent)
  gridShelves: StockWarehouseEditGridComponent | undefined;
  coreService = inject(CoreService);
  service = inject(StockWarehouseService);
  economyService = inject(EconomyService);
  stockWarehouseValidatorService = inject(StockWarehouseValidatorService);
  performLoadAccounts = new Perform<SmallGenericType[]>(this.progressService);
  performLoadAddresses = new Perform<IContactAddressDTO[]>(
    this.progressService
  );
  performLoadCompSettings = new Perform<SmallGenericType[]>(
    this.progressService
  );
  accountStdsDict: ISmallGenericType[] = [];
  stockAccountSettingTypes: SmallGenericType[] = [];
  stockBaseAccounts: SmallGenericType[] = [];
  isWarehouseProductsExpanded: boolean = false;

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Billing_Stock_Place, {
      lookups: [this.loadAccountDict(), this.loadDeliveryAddresses()],
    });

    this.form?.addStockShelfvesValidators(this.stockWarehouseValidatorService);

    //Ensure to apply edited data in the grid when clicking outside
    document.body.addEventListener('click', (event: any) => {
      if (event.target.closest('ag-grid-angular') == null) {
        this.applyChanges();
      }
    });
  }

  applyChanges() {
    if (this.gridShelves && this.gridShelves.gridUpdated && this.form?.dirty) {
      this.gridShelves?.grid?.applyChanges();
      this.form?.updateValueAndValidity();
    }
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'billing.products.products.stockaccountsettingtype.stockin',
      'billing.products.products.stockaccountsettingtype.stockinchange',
      'billing.products.products.stockaccountsettingtype.stockout',
      'billing.products.products.stockaccountsettingtype.stockoutchange',
      'billing.products.products.stockaccountsettingtype.stockinv',
      'billing.products.products.stockaccountsettingtype.stockinvchange',
      'billing.products.products.stockaccountsettingtype.stockloss',
      'billing.products.products.stockaccountsettingtype.stocklosschange',
      'billing.products.products.stockaccountsettingtype.stocktransferchange',
    ]);
  }

  override loadCompanySettings(): Observable<any> {
    const settingTypes: CompanySettingType[] = [
      CompanySettingType.AccountStockIn,
      CompanySettingType.AccountStockInChange,
      CompanySettingType.AccountStockOut,
      CompanySettingType.AccountStockOutChange,
      CompanySettingType.AccountStockInventory,
      CompanySettingType.AccountStockInventoryChange,
      CompanySettingType.AccountStockLoss,
      CompanySettingType.AccountStockLossChange,
      CompanySettingType.AccountStockTransferChange,
    ];
    return this.performLoadCompSettings.load$(
      this.coreService.getCompanySettings(settingTypes, false).pipe(
        tap(x => {
          this.stockBaseAccounts = [];
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockIn,
              SettingsUtil.getIntCompanySetting(
                x,
                CompanySettingType.AccountStockIn
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockInChange,
              SettingsUtil.getIntCompanySetting(
                x,
                CompanySettingType.AccountStockInChange
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockOut,
              SettingsUtil.getIntCompanySetting(
                x,
                CompanySettingType.AccountStockOut
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockOutChange,
              SettingsUtil.getIntCompanySetting(
                x,
                CompanySettingType.AccountStockOutChange
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockInv,
              SettingsUtil.getIntCompanySetting(
                x,
                CompanySettingType.AccountStockInventory
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockInvChange,
              SettingsUtil.getIntCompanySetting(
                x,
                CompanySettingType.AccountStockInventoryChange
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockLoss,
              SettingsUtil.getIntCompanySetting(
                x,
                CompanySettingType.AccountStockLoss
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockLossChange,
              SettingsUtil.getIntCompanySetting(
                x,
                CompanySettingType.AccountStockLossChange
              ).toString()
            )
          );
          this.stockBaseAccounts.push(
            new SmallGenericType(
              ProductAccountType.StockTransferChange,
              SettingsUtil.getIntCompanySetting(
                x,
                CompanySettingType.AccountStockTransferChange
              ).toString()
            )
          );
        })
      )
    );
  }

  loadAccountDict(): Observable<SmallGenericType[]> {
    return this.performLoadAccounts.load$(
      this.economyService.getAccountStdsDict(true).pipe(
        tap(x => {
          this.accountStdsDict = x;
        })
      )
    );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.form?.customPatch(value);
        })
      )
    );
  }

  override onFinished(): void {
    this.createSettingTypes();
  }

  loadDeliveryAddresses(): Observable<IContactAddressDTO[]> {
    return this.performLoadAddresses.load$(
      this.coreService.getContactAddresses(
        SoeConfigUtil.actorCompanyId,
        TermGroup_SysContactAddressType.Delivery,
        true,
        false,
        false
      )
    );
  }

  accountSettingsChanged(rows: IAccountingSettingsRowDTO[]) {
    this.form?.customAccountingSettingsPathValue(rows);
  }

  private createSettingTypes() {
    this.stockAccountSettingTypes = [];
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockIn,
        this.terms['billing.products.products.stockaccountsettingtype.stockin']
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockInChange,
        this.terms[
          'billing.products.products.stockaccountsettingtype.stockinchange'
        ]
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockOut,
        this.terms['billing.products.products.stockaccountsettingtype.stockout']
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockOutChange,
        this.terms[
          'billing.products.products.stockaccountsettingtype.stockoutchange'
        ]
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockInv,
        this.terms['billing.products.products.stockaccountsettingtype.stockinv']
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockInvChange,
        this.terms[
          'billing.products.products.stockaccountsettingtype.stockinvchange'
        ]
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockLoss,
        this.terms[
          'billing.products.products.stockaccountsettingtype.stockloss'
        ]
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockLossChange,
        this.terms[
          'billing.products.products.stockaccountsettingtype.stocklosschange'
        ]
      )
    );
    this.stockAccountSettingTypes.push(
      new SmallGenericType(
        ProductAccountType.StockTransferChange,
        this.terms[
          'billing.products.products.stockaccountsettingtype.stocktransferchange'
        ]
      )
    );
  }

  override newRecord(): Observable<void> {
    let resetValues = () => {};

    if (this.form?.isCopy) {
      resetValues = () => {
        this.form?.onDoCopy();
      };
    }

    return of(resetValues());
  }

  override performSave(options?: ProgressOptions | undefined): void {
    if (!this.form || this.form.invalid || !this.service) return;

    this.form?.removeEmptyStockShelfs();
    this.form?.filterModifiedWarehouseProducts();
    const dto = <StockDTO>this.form?.getRawValue();

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(dto).pipe(
        tap(value => {
          if (value.success) {
            this.updateFormValueAndEmitChange(value);
            this.loadData();
          }
        })
      ),
      undefined,
      undefined,
      options
    );
  }
}
