import {
  IDistributionCodeHeadDTO,
  IDistributionCodePeriodDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class DistributionCodeHeadDTO implements IDistributionCodeHeadDTO {
  distributionCodeHeadId: number;
  name: string;
  typeId: number;
  type!: string;
  noOfPeriods: number;
  fromDate?: Date;
  actorCompanyId: number;
  parentId?: number;
  subType?: number;
  openingHoursId?: number;
  accountDimId?: number;
  accountDim!: string;
  typeOfPeriod!: string;
  openingHour!: string;
  isInUse: boolean;
  created?: Date;
  createdBy: string;
  modified?: Date;
  modifiedBy: string;
  periods!: IDistributionCodePeriodDTO[];

  constructor() {
    this.distributionCodeHeadId = 0;
    this.typeId = 0;
    this.type = '';
    this.noOfPeriods = 0;
    this.name = '';
    this.actorCompanyId = 0;
    this.isInUse = false;
    this.createdBy = '';
    this.modifiedBy = '';
    this.periods = [];
    this.accountDim = '';
    this.typeOfPeriod = '';
    this.openingHour = '';
    this.parentId = undefined;
  }
}

export class DistributionCodePeriodDTO implements IDistributionCodePeriodDTO {
  distributionCodePeriodId: number;
  parentToDistributionCodePeriodId?: number;
  number: number;
  isAdded: boolean;
  isModified: boolean;
  percent: number;
  comment: string;
  periodSubTypeName: string;

  constructor(
    distributionCodePeriodId: number,
    parentToDistributionCodePeriodId: number,
    number: number,
    isAdded: boolean,
    isModified: boolean,
    percent: number,
    comment: string,
    periodSubTypeName: string
  ) {
    this.distributionCodePeriodId = distributionCodePeriodId;
    this.parentToDistributionCodePeriodId = parentToDistributionCodePeriodId;
    this.number = number;
    this.isAdded = isAdded;
    this.isModified = isModified;
    this.percent = percent;
    this.comment = comment;
    this.periodSubTypeName = periodSubTypeName;
  }
}

export class PeriodSummery {
  diff!: number;
  sumPercent!: number;
}
