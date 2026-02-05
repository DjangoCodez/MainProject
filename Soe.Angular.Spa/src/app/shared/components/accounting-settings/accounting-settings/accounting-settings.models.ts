import { IAccountingSettingDTO } from '@shared/models/generated-interfaces/AccountingSettingDTO';
import { IAccountingSettingsRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class AccountingSettingsRowDTO implements IAccountingSettingsRowDTO {
  type!: number;
  accountDim1Nr!: number;
  account1Id!: number;
  account1Nr!: string;
  account1Name!: string;
  accountDim2Nr!: number;
  account2Id!: number;
  account2Nr!: string;
  account2Name!: string;
  accountDim3Nr!: number;
  account3Id!: number;
  account3Nr!: string;
  account3Name!: string;
  accountDim4Nr!: number;
  account4Id!: number;
  account4Nr!: string;
  account4Name!: string;
  accountDim5Nr!: number;
  account5Id!: number;
  account5Nr!: string;
  account5Name!: string;
  accountDim6Nr!: number;
  account6Id!: number;
  account6Nr!: string;
  account6Name!: string;
  percent!: number;

  //Extensions
  typeName!: string;
  baseAccount!: string;

  constructor(type?: number) {
    if (type) this.type = type;
  }

  public convertFromAccountSettingDTO(input: IAccountingSettingDTO): void {
    this.type = input.type;
    this.account1Id = input.account1Id;
    this.account1Nr = input.account1Nr;
    this.account1Name = input.account1Name;
    this.account2Id = input.account2Id;
    this.account2Nr = input.account2Nr;
    this.account2Name = input.account2Name;
    this.account3Id = input.account3Id;
    this.account3Nr = input.account3Nr;
    this.account3Name = input.account3Name;
    this.account4Id = input.account4Id;
    this.account4Nr = input.account4Nr;
    this.account4Name = input.account4Name;
    this.account5Id = input.account5Id;
    this.account5Nr = input.account5Nr;
    this.account5Name = input.account5Name;
    this.account6Id = input.account6Id;
    this.account6Nr = input.account6Nr;
    this.account6Name = input.account6Name;
    this.percent = input.percent1;

    this.accountDim1Nr = 1;
    this.accountDim2Nr = 2;
    this.accountDim3Nr = 3;
    this.accountDim4Nr = 4;
    this.accountDim5Nr = 5;
    this.accountDim6Nr = 6;
  }

  public static fromAccountingSettings(
    dto: IAccountingSettingDTO
  ): AccountingSettingsRowDTO {
    const instance = new AccountingSettingsRowDTO();
    instance.convertFromAccountSettingDTO(dto);
    return instance;
  }
}
