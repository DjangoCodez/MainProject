import { IAccountingSettingDTO } from '@shared/models/generated-interfaces/AccountingSettingDTO';
import { ISaveInventoryWriteOffTemplateModel } from '@shared/models/generated-interfaces/EconomyModels';
import { SoeEntityState } from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountSmallDTO,
  IInventoryWriteOffTemplateDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class InventoryWriteOffTemplatesDTO
  implements IInventoryWriteOffTemplateDTO
{
  inventoryWriteOffTemplateId: number;
  actorCompanyId: number;
  inventoryWriteOffMethodId: number;
  inventoryWriteOffName: string;
  voucherSeriesTypeId: number;
  voucherSeriesName: string;
  name: string;
  description: string;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  state: SoeEntityState;
  accountingSettings: IAccountingSettingDTO[] = [];
  inventoryAccounts: { [key: number]: IAccountSmallDTO };
  accWriteOffAccounts: { [key: number]: IAccountSmallDTO };
  writeOffAccounts: { [key: number]: IAccountSmallDTO };
  accOverWriteOffAccounts: { [key: number]: IAccountSmallDTO };
  overWriteOffAccounts: { [key: number]: IAccountSmallDTO };
  accWriteDownAccounts: { [key: number]: IAccountSmallDTO };
  writeDownAccounts: { [key: number]: IAccountSmallDTO };
  accWriteUpAccounts: { [key: number]: IAccountSmallDTO };
  writeUpAccounts: { [key: number]: IAccountSmallDTO };

  constructor() {
    this.inventoryWriteOffTemplateId = 0;
    this.actorCompanyId = 0;
    this.inventoryWriteOffMethodId = 0;
    this.inventoryWriteOffName = '';
    this.voucherSeriesTypeId = 0;
    this.voucherSeriesName = '';
    this.name = '';
    this.description = '';
    this.createdBy = '';
    this.modifiedBy = '';
    this.state = SoeEntityState.Active;
    this.inventoryAccounts = {};
    this.accWriteOffAccounts = {};
    this.writeOffAccounts = {};
    this.accOverWriteOffAccounts = {};
    this.overWriteOffAccounts = {};
    this.accWriteDownAccounts = {};
    this.writeDownAccounts = {};
    this.accWriteUpAccounts = {};
    this.writeUpAccounts = {};
    this.accountingSettings = [];
  }
}

export class SaveInventoryWriteOffTemplateModel
  implements ISaveInventoryWriteOffTemplateModel
{
  inventoryWriteOffTemplate!: IInventoryWriteOffTemplateDTO;
  accountSettings!: IAccountingSettingDTO[];
}
