import { IAccountNumberNameDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class AccountStdNumberNameDTO implements IAccountNumberNameDTO {
  accountId!: number;
  number!: string;
  name!: string;
  numberName!: string;
}

export class AutocompleteItem {
  id: number;
  name: string;
  number?: string;
  numberName?: string;

  constructor(
    id: number,
    name: string,
    number?: string | undefined,
    numberName?: string | undefined
  ) {
    this.id = id;
    this.name = name;
    this.number = number;
    this.numberName = numberName;
  }
}
