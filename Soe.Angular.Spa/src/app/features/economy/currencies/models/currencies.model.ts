import {
  TermGroup_CurrencyIntervalType,
  TermGroup_CurrencySource,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  ICurrencyDTO,
  ICurrencyGridDTO,
  ICurrencyRateDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class CurrencyDTO implements ICurrencyDTO {
  currencyId: number;
  sysCurrencyId: number;
  intervalType: TermGroup_CurrencyIntervalType;
  intervalName: string;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  currencyRates: CurrencyRateDTO[];

  //extensions:
  code!: string;
  name!: string;
  description!: string;

  constructor() {
    this.currencyId = 0;
    this.sysCurrencyId = 0;
    this.intervalType = TermGroup_CurrencyIntervalType.Manually;
    this.intervalName = '';
    this.currencyRates = [];
  }
}

export class CurrencyRateDTO implements ICurrencyRateDTO {
  currencyRateId: number;
  currencyId: number;
  rateToBase: number;
  rateFromBase: number;
  source: TermGroup_CurrencySource;
  date: Date;
  sourceName: string;
  doDelete: boolean;
  isModified: boolean;

  constructor() {
    this.currencyRateId = 0;
    this.currencyId = 0;
    this.rateToBase = 0;
    this.rateFromBase = 0;
    this.source = TermGroup_CurrencySource.Manually;
    this.date = new Date();
    this.doDelete = false;
    this.sourceName = '';
    this.isModified = false;
  }
}

export class CurrencyGridDTO implements ICurrencyGridDTO {
  currencyId!: number;
  code!: string;
  name!: string;
  intervalName!: string;

  //Extensions
  description!: string;
}
