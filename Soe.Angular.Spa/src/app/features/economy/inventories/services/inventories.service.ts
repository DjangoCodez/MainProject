import { inject, Injectable } from '@angular/core';
import { CoreService } from '@shared/services/core.service';
import { SoeHttpClient } from '@shared/services/http.service';
import {
  deleteInventory,
  getInventories,
  getInventoriesDict,
  getInventory,
  getNextInventoryNr,
  saveInventory,
  saveAdjustment,
  getInventoryTraceViews,
} from '@shared/services/generated-service-endpoints/economy/InventoryV2.endpoints';
import { map, Observable, tap } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  InventoryDTO,
  InventoryGridDTO,
  SaveAdjustmentModel,
} from '../models/inventories.model';
import { IInventoryTraceViewDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TermCollection } from '@shared/localization/term-types';
import {
  CompanySettingType,
  InventoryAccountType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  SettingsUtil,
  UserCompanySettingCollection,
} from '@shared/util/settings-util';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Injectable({
  providedIn: 'root',
})
export class InventoriesService {
  constructor(private http: SoeHttpClient) {}

  coreService = inject(CoreService);

  getGridAdditionalProps = {
    setting: '',
  };
  getGrid(
    id?: number,
    additionalProps?: { setting: string }
  ): Observable<InventoryGridDTO[]> {
    if (additionalProps) this.getGridAdditionalProps = additionalProps;
    return this.http.get<InventoryGridDTO[]>(
      getInventories(this.getGridAdditionalProps.setting, id)
    );
  }

  get(id: number): Observable<InventoryDTO> {
    return this.http.get<InventoryDTO>(getInventory(id)).pipe(
      map(data => {
        const obj = new InventoryDTO();
        Object.assign(obj, data);
        return obj;
      })
    );
  }

  getInventoriesDict(): Observable<SmallGenericType[]> {
    return this.http.get<SmallGenericType[]>(getInventoriesDict());
  }

  getNextInventoryNr(): Observable<number> {
    return this.http.get<number>(getNextInventoryNr());
  }

  getInventoryTraceViews(id: number): Observable<IInventoryTraceViewDTO[]> {
    return this.http.get<IInventoryTraceViewDTO[]>(getInventoryTraceViews(id));
  }

  save(model: any): Observable<any> {
    return this.http.post<any>(saveInventory(), model);
  }

  saveAdjustment(model: SaveAdjustmentModel): Observable<any> {
    return this.http.post<SaveAdjustmentModel>(saveAdjustment(), model);
  }

  delete(id: number): Observable<BackendResponse> {
    return this.http.delete(deleteInventory(id));
  }

  getSettingsTypes(terms: TermCollection): SmallGenericType[] {
    const accountSettings: { type: InventoryAccountType; termKey: string }[] = [
      {
        type: InventoryAccountType.Inventory,
        termKey: 'economy.inventory.inventoryaccountsettingtype.inventory',
      },
      {
        type: InventoryAccountType.AccWriteOff,
        termKey: 'economy.inventory.inventoryaccountsettingtype.accwriteoff',
      },
      {
        type: InventoryAccountType.WriteOff,
        termKey: 'economy.inventory.inventoryaccountsettingtype.writeoff',
      },
      {
        type: InventoryAccountType.AccOverWriteOff,
        termKey:
          'economy.inventory.inventoryaccountsettingtype.accoverwriteoff',
      },
      {
        type: InventoryAccountType.OverWriteOff,
        termKey: 'economy.inventory.inventoryaccountsettingtype.overwriteoff',
      },
      {
        type: InventoryAccountType.AccWriteDown,
        termKey: 'economy.inventory.inventoryaccountsettingtype.accwritedown',
      },
      {
        type: InventoryAccountType.WriteDown,
        termKey: 'economy.inventory.inventoryaccountsettingtype.writedown',
      },
      {
        type: InventoryAccountType.AccWriteUp,
        termKey: 'economy.inventory.inventoryaccountsettingtype.accwriteup',
      },
      {
        type: InventoryAccountType.WriteUp,
        termKey: 'economy.inventory.inventoryaccountsettingtype.writeup',
      },
    ];

    return accountSettings.map(
      setting => new SmallGenericType(setting.type, terms[setting.termKey])
    );
  }

  getBaseAccounts(): Observable<SmallGenericType[]> {
    return this.coreService
      .getCompanySettings(InventoriesService.baseAccountSettingTypes, false)
      .pipe(
        map((companySettings: UserCompanySettingCollection) => {
          return InventoriesService.baseAccountSettingTypes
            .map(setting =>
              InventoriesService.getInventoryAccount(companySettings, setting)
            )
            .filter(account => !!account);
        })
      );
  }

  private static getInventoryAccount(
    companySettingsObject: UserCompanySettingCollection,
    companySetting: CompanySettingType
  ): SmallGenericType | undefined {
    const accountId = SettingsUtil.getIntCompanySetting(
      companySettingsObject,
      companySetting
    );
    // accountId is 0 if setting is not found, 0 is falsy and will therefor return undefined.
    if (!accountId) return;

    const inventoryAccount =
      this.companySettingToAccountTypeMap[companySetting];
    if (!inventoryAccount) return;

    return new SmallGenericType(inventoryAccount, accountId.toString());
  }

  private static readonly baseAccountSettingTypes: CompanySettingType[] = [
    CompanySettingType.AccountInventoryInventories,
    CompanySettingType.AccountInventoryAccWriteOff,
    CompanySettingType.AccountInventoryWriteOff,
    CompanySettingType.AccountInventoryAccOverWriteOff,
    CompanySettingType.AccountInventoryOverWriteOff,
    CompanySettingType.AccountInventoryAccWriteDown,
    CompanySettingType.AccountInventoryWriteDown,
    CompanySettingType.AccountInventoryAccWriteUp,
    CompanySettingType.AccountInventoryWriteUp,
    CompanySettingType.AccountInventorySalesLoss,
    CompanySettingType.AccountInventorySales,
    CompanySettingType.AccountInventorySalesProfit,
  ];

  private static readonly companySettingToAccountTypeMap: {
    [key in CompanySettingType]?: InventoryAccountType;
  } = {
    [CompanySettingType.AccountInventoryInventories]:
      InventoryAccountType.Inventory,
    [CompanySettingType.AccountInventoryAccWriteOff]:
      InventoryAccountType.AccWriteOff,
    [CompanySettingType.AccountInventoryWriteOff]:
      InventoryAccountType.WriteOff,
    [CompanySettingType.AccountInventoryAccOverWriteOff]:
      InventoryAccountType.AccOverWriteOff,
    [CompanySettingType.AccountInventoryOverWriteOff]:
      InventoryAccountType.OverWriteOff,
    [CompanySettingType.AccountInventoryAccWriteDown]:
      InventoryAccountType.AccWriteDown,
    [CompanySettingType.AccountInventoryWriteDown]:
      InventoryAccountType.WriteDown,
    [CompanySettingType.AccountInventoryAccWriteUp]:
      InventoryAccountType.AccWriteUp,
    [CompanySettingType.AccountInventoryWriteUp]: InventoryAccountType.WriteUp,
    [CompanySettingType.AccountInventorySalesLoss]:
      InventoryAccountType.SalesLoss,
    [CompanySettingType.AccountInventorySales]: InventoryAccountType.Sales,
    [CompanySettingType.AccountInventorySalesProfit]:
      InventoryAccountType.SalesProfit,
  };
}
