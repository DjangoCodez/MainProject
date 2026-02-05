import {
  ProjectCentralBudgetRowType,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IBudgetHeadDTO,
  IBudgetPeriodDTO,
  IBudgetRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class BudgetRowDTO implements IBudgetRowDTO {
  budgetRowId!: number;
  budgetHeadId!: number;
  accountId!: number;
  timeCodeId!: number;
  name!: string;
  distributionCodeHeadId?: number | undefined;
  shiftTypeId?: number | undefined;
  budgetRowNr!: number;
  type?: number | undefined;
  modifiedUserId?: number | undefined;
  isAdded!: boolean;
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
  budgetHead!: IBudgetHeadDTO;
  periods!: IBudgetPeriodDTO[];
}

export class BudgetRowEx extends BudgetRowDTO {
  //Extended properties
  ib?: number;
  budget?: number;
  hours?: number;
  totalHours?: number;
  ibHours?: number;
  ibType!: ProjectCentralBudgetRowType;
  ibQuantityType?: ProjectCentralBudgetRowType;
  hidden?: boolean;
  icon?: string;
  expander?: ' ' | undefined;
  showIbAmount?: boolean;
  showIbHours?: boolean;
  state?: SoeEntityState;

  static fromBudgetRowDto(rows: IBudgetRowDTO[]): BudgetRowEx[] {
    return rows.map(r => <BudgetRowEx>r);
  }

  static toBudgetRowEx(row: IBudgetRowDTO): BudgetRowEx {
    return <BudgetRowEx>row;
  }
}

export enum SumType {
  Amount = 'totalAmount',
  Hours = 'totalQuantity',
  ResultAmount = 'totalAmountResult',
  ResultHours = 'totalQuantityResult',
  CompBudgetAmount = 'totalAmountCompBudget',
  CompBudgetHours = 'totalQuantityCompBudget',
}

export class BudgetHeadDTO implements IBudgetHeadDTO {
  budgetHeadId!: number;
  actorCompanyId!: number;
  type!: number;
  accountYearId?: number | undefined;
  distributionCodeHeadId?: number | undefined;
  noOfPeriods!: number;
  status!: number;
  projectId?: number | undefined;
  accountYearText!: string;
  statusName!: string;
  name!: string;
  createdDate!: string;
  fromDate?: Date | undefined;
  toDate?: Date | undefined;
  useDim2?: boolean | undefined;
  useDim3?: boolean | undefined;
  dim2Id!: number;
  dim3Id!: number;
  created?: Date | undefined;
  createdBy!: string;
  modified?: Date | undefined;
  modifiedBy!: string;
  rows!: IBudgetRowDTO[];
}
