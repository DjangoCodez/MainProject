import { AccountingRowDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { IAccountDistributionHeadDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';

export class SelectAccountDistributionPeriodDialogData implements DialogData {
  title!: string;
  content?: string;
  primaryText?: string;
  secondaryText?: string;
  size?: DialogSize;

  text!: string;
  rowItem!: AccountingRowDTO;
  selectedAccountDistribution!: IAccountDistributionHeadDTO | undefined;
  periodAccountDistributions!: IAccountDistributionHeadDTO[];
  container!: number;
  accountDistributionHeadName!: string;
  constructor(
    title: string,
    size: DialogSize,
    text: string,
    rowItem: AccountingRowDTO,
    selectedAccountDistribution: IAccountDistributionHeadDTO | undefined,
    periodAccountDistributions: IAccountDistributionHeadDTO[] = [],
    container: number,
    accountDistributionHeadName: string
  ) {
    this.title = title;
    this.size = size;
    this.text = text;
    this.rowItem = rowItem;
    this.selectedAccountDistribution = selectedAccountDistribution;
    this.periodAccountDistributions = periodAccountDistributions;
    this.container = container;
    this.accountDistributionHeadName = accountDistributionHeadName;
  }
}
