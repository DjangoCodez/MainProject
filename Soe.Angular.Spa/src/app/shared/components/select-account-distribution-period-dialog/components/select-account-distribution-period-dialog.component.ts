import { Component, inject } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { SelectAccountDistributionPeriodDialogData } from '../models/select-account-distribution-period-dialog.model';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { ValidationHandler } from '@shared/handlers';
import { AccountingRowDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { IAccountDistributionHeadDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

@Component({
    selector: 'soe-select-account-distribution-period-dialog',
    templateUrl: './select-account-distribution-period-dialog.component.html',
    styleUrls: ['./select-account-distribution-period-dialog.component.scss'],
    providers: [FlowHandlerService],
    standalone: false
})
export class SelectAccountDistributionPeriodDialogComponent extends DialogComponent<SelectAccountDistributionPeriodDialogData> {
  progressService = inject(ProgressService);
  validationHandler = inject(ValidationHandler);

  text!: string;
  rowItem!: AccountingRowDTO;
  selectedAccountDistribution!: IAccountDistributionHeadDTO;
  periodAccountDistributions!: IAccountDistributionHeadDTO[];
  container!: number;
  accountDistributionHeadName!: string;

  constructor(public handler: FlowHandlerService) {
    super();

    this.setDialogParam();
  }

  setDialogParam() {
    if (this.data) {
      if (this.data.text) {
        this.text = this.data.text;
      }
      if (this.data.rowItem) {
        this.rowItem = this.data.rowItem;
      }
      if (this.data.selectedAccountDistribution) {
        this.selectedAccountDistribution =
          this.data.selectedAccountDistribution;
      }
      if (this.data.periodAccountDistributions) {
        this.periodAccountDistributions = this.data.periodAccountDistributions;
      }
      if (this.data.container) {
        this.container = this.data.container;
      }
      if (this.data.accountDistributionHeadName) {
        this.accountDistributionHeadName =
          this.data.accountDistributionHeadName;
      }
    }
  }

  finished() {}
}
