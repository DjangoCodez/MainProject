import { IGetResultPerPeriodModel } from '@shared/models/generated-interfaces/EconomyModels';
import {
  IBudgetHeadFlattenedDTO,
  IBudgetHeadGridDTO,
  IBudgetRowFlattenedDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { Guid } from '@shared/util/string-util';

export class BudgetHeadGridDTO implements IBudgetHeadGridDTO {
  budgetHeadId!: number;
  name!: string;
  accountYearId!: number;
  accountingYear!: string;
  created!: string;
  noOfPeriods!: string;
  budgetTypeId!: number;
  type!: string;
  status!: string;
  fromDate?: Date;
  toDate?: Date;
  distributionCodeId!: number;
  useDim2!: boolean;
  dim2Id!: number;
  useDim3!: boolean;
  dim3Id!: number;
}

export class BudgetHeadFlattenedDTO implements IBudgetHeadFlattenedDTO {
  budgetHeadId!: number;
  actorCompanyId!: number;
  type!: number;
  distributionCodeSubType!: number;
  accountYearId!: number;
  distributionCodeHeadId?: number;
  noOfPeriods!: number;
  status!: number;
  projectId?: number;
  accountYearText!: string;
  statusName!: string;
  name!: string;
  createdDate!: string;
  useDim2?: boolean | undefined;
  useDim3?: boolean | undefined;
  dim2Id!: number;
  dim3Id!: number;
  created?: Date;
  createdBy!: string;
  modified?: Date;
  modifiedBy!: string;
  rows!: IBudgetRowFlattenedDTO[];
  isModified: boolean = false;

  constructor() {
    this.created = new Date();
    this.modified = new Date();
  }

  fixDates() {
    if (this.created) {
      this.created = new Date(this.created);
    }
    if (this.modified) {
      this.modified = new Date(this.modified);
    }
  }
}

export class BudgetRowFlattenedDTO implements IBudgetRowFlattenedDTO {
  budgetRowId: number;
  budgetHeadId!: number;
  accountId!: number;
  distributionCodeHeadId?: number;
  shiftTypeId?: number;
  budgetRowNr!: number;
  type?: number;
  modifiedUserId?: number;
  isModified!: boolean;
  isDeleted!: boolean;
  modified!: string;
  modifiedBy!: string;
  distributionCodeHeadName!: string;
  totalAmount!: number;
  totalQuantity!: number;
  dim1Id!: number;
  dim1Nr!: string;
  dim1Name!: string;
  dim2Id!: number;
  dim2Nr!: string;
  dim2Name!: string;
  dim3Id!: number;
  dim3Nr!: string;
  dim3Name!: string;
  dim4Id!: number;
  dim4Nr!: string;
  dim4Name!: string;
  dim5Id!: number;
  dim5Nr!: string;
  dim5Name!: string;
  dim6Id!: number;
  dim6Nr!: string;
  dim6Name!: string;
  budgetRowPeriodId1!: number;
  periodNr1!: number;
  startDate1?: Date;
  amount1!: number;
  quantity1!: number;
  budgetRowPeriodId2!: number;
  periodNr2!: number;
  startDate2?: Date;
  amount2!: number;
  quantity2!: number;
  budgetRowPeriodId3!: number;
  periodNr3!: number;
  startDate3?: Date;
  amount3!: number;
  quantity3!: number;
  budgetRowPeriodId4!: number;
  periodNr4!: number;
  startDate4?: Date;
  amount4!: number;
  quantity4!: number;
  budgetRowPeriodId5!: number;
  periodNr5!: number;
  startDate5?: Date;
  amount5!: number;
  quantity5!: number;
  budgetRowPeriodId6!: number;
  periodNr6!: number;
  startDate6?: Date;
  amount6!: number;
  quantity6!: number;
  budgetRowPeriodId7!: number;
  periodNr7!: number;
  startDate7?: Date;
  amount7!: number;
  quantity7!: number;
  budgetRowPeriodId8!: number;
  periodNr8!: number;
  startDate8?: Date;
  amount8!: number;
  quantity8!: number;
  budgetRowPeriodId9!: number;
  periodNr9!: number;
  startDate9?: Date;
  amount9!: number;
  quantity9!: number;
  budgetRowPeriodId10!: number;
  periodNr10!: number;
  startDate10?: Date;
  amount10!: number;
  quantity10!: number;
  budgetRowPeriodId11!: number;
  periodNr11!: number;
  startDate11?: Date;
  amount11!: number;
  quantity11!: number;
  budgetRowPeriodId12!: number;
  periodNr12!: number;
  startDate12?: Date;
  amount12!: number;
  quantity12!: number;
  budgetRowPeriodId13!: number;
  periodNr13!: number;
  startDate13?: Date;
  amount13!: number;
  quantity13!: number;
  budgetRowPeriodId14!: number;
  periodNr14!: number;
  startDate14?: Date;
  amount14!: number;
  quantity14!: number;
  budgetRowPeriodId15!: number;
  periodNr15!: number;
  startDate15?: Date;
  amount15!: number;
  quantity15!: number;
  budgetRowPeriodId16!: number;
  periodNr16!: number;
  startDate16?: Date;
  amount16!: number;
  quantity16!: number;
  budgetRowPeriodId17!: number;
  periodNr17!: number;
  startDate17?: Date;
  amount17!: number;
  quantity17!: number;
  budgetRowPeriodId18!: number;
  periodNr18!: number;
  startDate18?: Date;
  amount18!: number;
  quantity18!: number;
  budgetRowPeriodId19!: number;
  periodNr19!: number;
  startDate19?: Date;
  amount19!: number;
  quantity19!: number;
  budgetRowPeriodId20!: number;
  periodNr20!: number;
  startDate20?: Date;
  amount20!: number;
  quantity20!: number;
  budgetRowPeriodId21!: number;
  periodNr21!: number;
  startDate21?: Date;
  amount21!: number;
  quantity21!: number;
  budgetRowPeriodId22!: number;
  periodNr22!: number;
  startDate22?: Date;
  amount22!: number;
  quantity22!: number;
  budgetRowPeriodId23!: number;
  periodNr23!: number;
  startDate23?: Date;
  amount23!: number;
  quantity23!: number;
  budgetRowPeriodId24!: number;
  periodNr24!: number;
  startDate24?: Date;
  amount24!: number;
  quantity24!: number;
  budgetRowPeriodId25!: number;
  periodNr25!: number;
  startDate25?: Date;
  amount25!: number;
  quantity25!: number;
  budgetRowPeriodId26!: number;
  periodNr26!: number;
  startDate26?: Date;
  amount26!: number;
  quantity26!: number;
  budgetRowPeriodId27!: number;
  periodNr27!: number;
  startDate27?: Date;
  amount27!: number;
  quantity27!: number;
  budgetRowPeriodId28!: number;
  periodNr28!: number;
  startDate28?: Date;
  amount28!: number;
  quantity28!: number;
  budgetRowPeriodId29!: number;
  periodNr29!: number;
  startDate29?: Date;
  amount29!: number;
  quantity29!: number;
  budgetRowPeriodId30!: number;
  periodNr30!: number;
  startDate30?: Date;
  amount30!: number;
  quantity30!: number;
  budgetRowPeriodId31!: number;
  periodNr31!: number;
  startDate31?: Date;
  amount31!: number;
  quantity31!: number;

  // Extensions
  showModalGetPreviousPeriodResult: boolean = true;

  constructor(budgetRowId: number = 0) {
    this.budgetRowId = budgetRowId;

    this.startDate1 = new Date();
    this.startDate2 = new Date();
    this.startDate3 = new Date();
    this.startDate4 = new Date();
    this.startDate5 = new Date();
    this.startDate6 = new Date();
    this.startDate7 = new Date();
    this.startDate8 = new Date();
    this.startDate9 = new Date();
    this.startDate10 = new Date();
    this.startDate11 = new Date();
    this.startDate12 = new Date();
    this.startDate13 = new Date();
    this.startDate14 = new Date();
    this.startDate15 = new Date();
    this.startDate16 = new Date();
    this.startDate17 = new Date();
    this.startDate18 = new Date();
    this.startDate19 = new Date();
    this.startDate20 = new Date();
    this.startDate21 = new Date();
    this.startDate22 = new Date();
    this.startDate23 = new Date();
    this.startDate24 = new Date();
    this.startDate25 = new Date();
    this.startDate26 = new Date();
    this.startDate27 = new Date();
    this.startDate28 = new Date();
    this.startDate29 = new Date();
    this.startDate30 = new Date();
    this.startDate31 = new Date();
  }

  fixDates() {
    this.startDate1 = this.startDate1 ? new Date(this.startDate1) : new Date();
    this.startDate2 = this.startDate2 ? new Date(this.startDate2) : new Date();
    this.startDate3 = this.startDate3 ? new Date(this.startDate3) : new Date();
    this.startDate4 = this.startDate4 ? new Date(this.startDate4) : new Date();
    this.startDate5 = this.startDate5 ? new Date(this.startDate5) : new Date();
    this.startDate6 = this.startDate6 ? new Date(this.startDate6) : new Date();
    this.startDate7 = this.startDate7 ? new Date(this.startDate7) : new Date();
    this.startDate8 = this.startDate8 ? new Date(this.startDate8) : new Date();
    this.startDate9 = this.startDate9 ? new Date(this.startDate9) : new Date();
    this.startDate10 = this.startDate10
      ? new Date(this.startDate10)
      : new Date();
    this.startDate11 = this.startDate11
      ? new Date(this.startDate11)
      : new Date();
    this.startDate12 = this.startDate12
      ? new Date(this.startDate12)
      : new Date();
    this.startDate13 = this.startDate13
      ? new Date(this.startDate13)
      : new Date();
    this.startDate14 = this.startDate14
      ? new Date(this.startDate14)
      : new Date();
    this.startDate15 = this.startDate15
      ? new Date(this.startDate15)
      : new Date();
    this.startDate16 = this.startDate16
      ? new Date(this.startDate16)
      : new Date();
    this.startDate17 = this.startDate17
      ? new Date(this.startDate17)
      : new Date();
    this.startDate18 = this.startDate18
      ? new Date(this.startDate18)
      : new Date();
    this.startDate19 = this.startDate19
      ? new Date(this.startDate19)
      : new Date();
    this.startDate20 = this.startDate20
      ? new Date(this.startDate20)
      : new Date();
    this.startDate21 = this.startDate21
      ? new Date(this.startDate21)
      : new Date();
    this.startDate22 = this.startDate22
      ? new Date(this.startDate22)
      : new Date();
    this.startDate23 = this.startDate23
      ? new Date(this.startDate23)
      : new Date();
    this.startDate24 = this.startDate24
      ? new Date(this.startDate24)
      : new Date();
    this.startDate25 = this.startDate25
      ? new Date(this.startDate25)
      : new Date();
    this.startDate26 = this.startDate26
      ? new Date(this.startDate26)
      : new Date();
    this.startDate27 = this.startDate27
      ? new Date(this.startDate27)
      : new Date();
    this.startDate28 = this.startDate28
      ? new Date(this.startDate28)
      : new Date();
    this.startDate29 = this.startDate29
      ? new Date(this.startDate29)
      : new Date();
    this.startDate30 = this.startDate30
      ? new Date(this.startDate30)
      : new Date();
    this.startDate31 = this.startDate31
      ? new Date(this.startDate31)
      : new Date();
  }
}

export class GetResultPerPeriodModel implements IGetResultPerPeriodModel {
  key: string = Guid.newGuid();
  accountId: number = 0;
  accountYearId!: number;
  noOfPeriods!: number;
  dims!: number[];
  getPrevious!: boolean;

  constructor(
    accountYearId: number,
    noOfPeriods: number,
    accountId: number,
    dims: number[],
    getPrevious: boolean = true
  ) {
    this.accountYearId = accountYearId;
    this.noOfPeriods = noOfPeriods;
    this.accountId = accountId;
    this.dims = dims;
    this.getPrevious = getPrevious;
  }
}
