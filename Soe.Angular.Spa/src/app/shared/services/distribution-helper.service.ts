import { inject } from '@angular/core';
import { AccountDistributionService } from '@features/economy/account-distribution/services/account-distribution.service';
import { TranslateService } from '@ngx-translate/core';
import { AccountingRowDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { SelectAccountDistributionPeriodDialogComponent } from '@shared/components/select-account-distribution-period-dialog/components/select-account-distribution-period-dialog.component';
import { SelectAccountDistributionPeriodDialogData } from '@shared/components/select-account-distribution-period-dialog/models/select-account-distribution-period-dialog.model';
import {
  SoeAccountDistributionType,
  SoeEntityState,
  TermGroup_AccountDistributionCalculationType,
  TermGroup_AccountDistributionPeriodType,
  TermGroup_AccountDistributionRegistrationType,
  TermGroup_AccountDistributionTriggerType,
  WildCard,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDistributionEntryDTO,
  IAccountDistributionHeadDTO,
  IAccountDistributionRowDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { Constants } from '@shared/util/client-constants'
import { StringUtil } from '@shared/util/string-util';
import { AccountingRowsContainers } from '@shared/util/Enumerations';
import { DialogService } from '@ui/dialog/services/dialog.service'
import { DialogSize } from '@ui/dialog/models/dialog'
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { take, tap } from 'rxjs';
import { MessagingService } from './messaging.service';

export class DistributionHelperService {
  private accountingRows: AccountingRowDTO[] = [];
  private selectedAccountDistribution: IAccountDistributionHeadDTO | undefined;
  private matches: IAccountDistributionHeadDTO[] = [];
  private existingEntryRows: IAccountDistributionEntryDTO[] = [];
  private parentGuid!: string;
  private accountDistributionHeadName!: string;
  private accountDistributions: IAccountDistributionHeadDTO[];
  private accountDistributionsForImport: IAccountDistributionHeadDTO[];
  private useAutomaticAccountDistribution: boolean;

  allChangesDone: (val: boolean) => void;
  deleteRow: (row: AccountingRowDTO) => void;
  addRow: () => { row: AccountingRowDTO; rowIndex: number };

  translationService = inject(TranslateService);
  messageboxService = inject(MessageboxService);
  service = inject(AccountDistributionService);
  dialogService = inject(DialogService);
  messagingService = inject(MessagingService);
  constructor(
    accountDistributions: IAccountDistributionHeadDTO[],
    accountDistributionsForImport: IAccountDistributionHeadDTO[],
    useAutomaticAccountDistribution: boolean,
    private container: AccountingRowsContainers,
    allChangesDone: (val: boolean) => void,
    deleteRow: (row: AccountingRowDTO) => void,
    addRow: () => { row: AccountingRowDTO; rowIndex: number }
  ) {
    this.accountDistributions = accountDistributions;
    this.accountDistributionsForImport = accountDistributionsForImport;
    this.useAutomaticAccountDistribution = useAutomaticAccountDistribution;
    this.allChangesDone = allChangesDone;
    this.deleteRow = deleteRow;
    this.addRow = addRow;

    this.service.accountDistributionHeadName$.subscribe(name => {
      this.accountDistributionHeadName = name;
    });
  }

  public setAccountingRows(accountingRows: AccountingRowDTO[]) {
    this.accountingRows = accountingRows;
  }

  public checkAccountDistribution(
    row: AccountingRowDTO,
    parentGuid: string
  ): boolean {
    this.parentGuid = parentGuid;

    if (this.shouldSkipDistribution(row)) return false;

    // Check existing account distribution
    if (row.accountDistributionHeadId) {
      this.selectedAccountDistribution = this.findAccountDistribution(
        row.accountDistributionHeadId
      );

      if (!this.selectedAccountDistribution) return false;

      if (this.isAutomaticDistribution(row)) {
        return this.handleAutomaticDistribution(row);
      }

      if (this.isPeriodDistribution()) {
        return this.handlePeriodDistribution(row);
      }

      // Child rows (generated from a distribution) do not trigger new distributions.
      return false;
    }
    return this.doAccountDistribution(row);
  }

  private handlePeriodDistribution(row: AccountingRowDTO): boolean {
    const { registrationType, sourceId } = this.getRegistrationDetails(row);

    if (!sourceId) {
      this.doAccountDistribution(row);
      return true;
    }

    this.service
      .getAccountDistributionEntriesForSource(
        row.accountDistributionHeadId,
        registrationType,
        sourceId
      )
      .pipe(
        tap(entries => {
          this.existingEntryRows = entries;
          const hasTransferredRows = this.existingEntryRows.some(
            entry => entry.voucherHeadId !== null
          );

          const keys = [
            'economy.accounting.distribution.perioddistribution',
            'economy.accounting.distribution.perioddistribution.warning',
            'economy.accounting.distribution.perioddistribution.voucherexistsmessage',
            'economy.accounting.distribution.perioddistribution.replacemessage',
          ];

          if (hasTransferredRows) {
            this.showPeriodDistributionWarning(keys);
          } else {
            this.showPeriodDistributionConfirmation(keys, row);
          }
        })
      );

    return true;
  }

  private showPeriodDistributionConfirmation(
    keys: string[],
    row: AccountingRowDTO
  ): void {
    this.translationService
      .get(keys)
      .pipe(take(1))
      .subscribe(terms => {
        const dialog = this.messageboxService.warning(terms[0], terms[1]);

        dialog.afterClosed().subscribe((val: any) => {
          this.doAccountDistribution(row);
          this.allChangesDone(true);
        });
      });
  }

  private showPeriodDistributionWarning(keys: string[]): void {
    this.translationService
      .get(keys)
      .pipe(take(1))
      .subscribe(terms => {
        const dialog = this.messageboxService.warning(terms[0], terms[1]);

        dialog.afterClosed().subscribe((val: any) => {
          this.selectedAccountDistribution = undefined;
          this.allChangesDone(true);
        });
      });
  }

  private getRegistrationDetails(row: AccountingRowDTO): {
    registrationType: number;
    sourceId: number;
  } {
    let registrationType = 0;
    let sourceId = 0;

    if (this.container === AccountingRowsContainers.Voucher) {
      registrationType = TermGroup_AccountDistributionRegistrationType.Voucher;
      sourceId = row.voucherHeadId;
    } else if (this.container === AccountingRowsContainers.SupplierInvoice) {
      registrationType =
        TermGroup_AccountDistributionRegistrationType.SupplierInvoice;
      sourceId = row.invoiceId;
    } else if (this.container === AccountingRowsContainers.CustomerInvoice) {
      registrationType =
        TermGroup_AccountDistributionRegistrationType.CustomerInvoice;
      sourceId = row.invoiceId;
    }

    return { registrationType, sourceId };
  }

  private isPeriodDistribution(): boolean {
    return (
      this.selectedAccountDistribution?.type ===
        SoeAccountDistributionType.Period &&
      this.selectedAccountDistribution?.triggerType ===
        TermGroup_AccountDistributionTriggerType.Registration
    );
  }

  private shouldSkipDistribution(row: AccountingRowDTO): boolean {
    return (
      (this.accountDistributions.length === 0 && !row.isAccrualAccount) ||
      (row.debitAmount === 0 && row.creditAmount === 0)
    );
  }

  private findAccountDistribution(
    headId: number
  ): IAccountDistributionHeadDTO | undefined {
    return this.accountDistributions.find(
      a => a.accountDistributionHeadId === headId
    );
  }
  private isAutomaticDistribution(row: AccountingRowDTO): boolean {
    return (
      this.selectedAccountDistribution?.type === SoeAccountDistributionType.Auto
    );
  }

  private handleAutomaticDistribution(row: AccountingRowDTO): boolean {
    const isParentRow =
      this.accountingRows.filter(
        r =>
          r.accountDistributionHeadId === row.accountDistributionHeadId &&
          (r.invoiceRowId <= row.invoiceRowId ||
            r.parentRowId !== row.tempInvoiceRowId)
      ).length === 1;

    if (!isParentRow) return false;

    this.translationService
      .get([
        'economy.accounting.distribution.automaticdistribution.askregenerate.title',
        'economy.accounting.distribution.automaticdistribution.askregenerate.message',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        const dialog = this.messageboxService.question(
          terms[
            'economy.accounting.distribution.automaticdistribution.askregenerate.title'
          ],
          terms[
            'economy.accounting.distribution.automaticdistribution.askregenerate.message'
          ]
        );

        dialog.afterClosed().subscribe((val: any) => {
          if (val) {
            this.deleteChildRows(row);
            this.doAccountDistribution(row);
          } else {
            this.selectedAccountDistribution = undefined;
            this.allChangesDone(true);
          }
        });
      });

    return true;
  }

  private doAccountDistribution(row: AccountingRowDTO): boolean {
    this.matches = [...this.accountDistributions]; // Clone all distributions

    // Filter by date if applicable
    if (row.date) {
      const rowDate = row.date;
      this.matches = this.matches.filter(
        a =>
          (!a.startDate || rowDate.clearHours() >= a.startDate.clearHours()) &&
          (!a.endDate || rowDate.clearHours() <= a.endDate.clearHours())
      );

      if (this.matches.length === 0 && !row.isAccrualAccount) return false;
    }

    // Filter by account dimensions
    this.matches = this.matches.filter(match =>
      this.isMatchForDimensions(match, row)
    );

    if (this.matches.length === 0 && !row.isAccrualAccount) return false;

    // Filter by amount
    const amount = Math.abs(row.debitAmount - row.creditAmount);
    this.matches = this.matches.filter(match =>
      this.isMatchForAmount(match, amount)
    );

    if (this.matches.length === 0 && !row.isAccrualAccount) return false;

    // Determine account distribution type
    const periodAccountDistributions = this.matches.filter(
      match => match.type === SoeAccountDistributionType.Period
    );

    if (periodAccountDistributions.length > 0 || row.isAccrualAccount) {
      this.selectedAccountDistribution = periodAccountDistributions[0];

      this.messagingService.publish(
        Constants.EVENT_SELECT_ACCOUNTDISTRIBUTION_NAME,
        null,
        this.parentGuid
      );
      this.showAccountDistributionPeriodDialog(row, periodAccountDistributions);
      return true;
    }

    this.handleDistribution(row);
    return true;
  }

  private showAccountDistributionPeriodDialog(
    row: AccountingRowDTO,
    periodAccountDistributions?: IAccountDistributionHeadDTO[]
  ) {
    this.translationService
      .get([
        'economy.accounting.distribution.perioddistribution.match',
        'economy.accounting.distribution.perioddistribution.selectdistribution',
        'economy.accounting.distribution.perioddistribution.match.questionplusnbrofperiods',
        'economy.accounting.distribution.perioddistribution.selectdistributionorcreatenew',
        'economy.accounting.distribution.perioddistribution.newtemplate',
        'economy.accounting.distribution.perioddistribution.createnew',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        let message = '';
        let size = 'md';

        const {
          match,
          selectdistribution,
          questionplusnbrofperiods,
          selectdistributionorcreatenew,
          newtemplate,
          createnew,
        } = terms;

        if (row.isAccrualAccount) {
          message =
            periodAccountDistributions && periodAccountDistributions.length >= 1
              ? selectdistributionorcreatenew
              : createnew;
          size = 'lg';

          const newDistributionTemplate: IAccountDistributionHeadDTO =
            this.getNewDistributionTemplate(newtemplate);
          if (periodAccountDistributions)
            periodAccountDistributions.unshift(newDistributionTemplate);
        } else if (
          periodAccountDistributions &&
          periodAccountDistributions.length > 1
        ) {
          message = selectdistribution;
        } else {
          message = questionplusnbrofperiods.format(
            this.selectedAccountDistribution?.name
          );
        }
        if (
          periodAccountDistributions &&
          periodAccountDistributions.length >= 1
        )
          this.selectedAccountDistribution = periodAccountDistributions[0];

        const dialogData = new SelectAccountDistributionPeriodDialogData(
          match,
          size as DialogSize,
          message,
          row,
          this.selectedAccountDistribution,
          periodAccountDistributions,
          this.container,
          this.accountDistributionHeadName
        );
        const accountDistributionPeriodDialog = this.dialogService.open(
          SelectAccountDistributionPeriodDialogComponent,
          dialogData
        );
        accountDistributionPeriodDialog
          .afterClosed()
          .subscribe((result: any) => {
            this.accountDistributionPeriodDialog_Closed(result, row);
          });
      });
  }

  private getNewDistributionTemplate(newTemplateName: string) {
    return {
      accountDistributionHeadId: 0,
      name: newTemplateName,
      actorCompanyId: 0,
      type: 0,
      description: '',
      triggerType: TermGroup_AccountDistributionTriggerType.None,
      calculationType: TermGroup_AccountDistributionCalculationType.Percent,
      calculate: 0,
      periodType: TermGroup_AccountDistributionPeriodType.Unknown,
      periodValue: 0,
      sort: 0,
      dayNumber: 0,
      amount: 0,
      amountOperator: 0,
      keepRow: false,
      useInVoucher: false,
      useInSupplierInvoice: false,
      useInCustomerInvoice: false,
      useInImport: false,
      createdBy: '',
      modifiedBy: '',
      state: SoeEntityState.Active,
      dim1Id: 0,
      dim1Expression: '',
      dim2Id: 0,
      dim2Expression: '',
      dim3Id: 0,
      dim3Expression: '',
      dim4Id: 0,
      dim4Expression: '',
      dim5Id: 0,
      dim5Expression: '',
      dim6Id: 0,
      dim6Expression: '',
      rows: [],
      useInPayrollVacationVoucher: false,
      useInPayrollVoucher: false,
    };
  }

  private accountDistributionPeriodDialog_Closed(
    data: any,
    row: AccountingRowDTO
  ) {
    if (data.result) {
      // Get selected distribution
      const head = data.distributionHead;
      // Set distribution and number of periods on row (will be used when row is saved)
      row.accountDistributionHeadId = head.accountDistributionHeadId;
      row.accountDistributionNbrOfPeriods = data.nbrOfPeriods;
      row.accountDistributionStartDate = data.startDate;
    }

    this.selectedAccountDistribution = this.getMatchedAccountDistribution();

    if (!this.selectedAccountDistribution) {
      this.allChangesDone(false);
      return;
    }

    if (this.useAutomaticAccountDistribution) {
      this.generateAccountDistribution(row);
      return;
    }

    this.translationService
      .get([
        'economy.accounting.distribution.automaticdistribution.match',
        'economy.accounting.distribution.automaticdistribution.match.question',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        const dialog = this.messageboxService.question(
          terms['economy.accounting.distribution.automaticdistribution.match'],
          terms[
            'economy.accounting.distribution.automaticdistribution.match.question'
          ]
        );

        dialog.afterClosed().subscribe((val: any) => {
          this.askAccountDistributionDialogClosed(val, row);
        });
      });
  }

  private askAccountDistributionDialogClosed(val: any, row: AccountingRowDTO) {
    if (val.result) {
      // Use account distribution
      this.generateAccountDistribution(row);
    } else {
      // Don't use account distribution
      this.allChangesDone(true); //NOTE: Checkinventorytrigger does nothing right now, so this is the correct behaviour for now.
    }
  }

  private getMatchedAccountDistribution() {
    return this.matches.find(
      match => match.type === SoeAccountDistributionType.Auto
    );
  }

  private handleDistribution(row: AccountingRowDTO): void {
    this.selectedAccountDistribution = this.getMatchedAccountDistribution();

    if (
      this.selectedAccountDistribution &&
      this.selectedAccountDistribution.type === SoeAccountDistributionType.Auto
    ) {
      if (this.useAutomaticAccountDistribution) {
        this.generateAccountDistribution(row);
      } else {
        this.askUserToGenerateAccountDistribution(row);
      }
    }
  }

  private askUserToGenerateAccountDistribution(row: AccountingRowDTO): void {
    this.translationService
      .get([
        'economy.accounting.distribution.automaticdistribution.askgenerate.title',
        'economy.accounting.distribution.automaticdistribution.askgenerate.message',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        const dialog = this.messageboxService.question(
          terms[
            'economy.accounting.distribution.automaticdistribution.askgenerate.title'
          ],
          terms[
            'economy.accounting.distribution.automaticdistribution.askgenerate.message'
          ]
        );

        dialog.afterClosed().subscribe((val: any) => {
          if (val) {
            this.generateAccountDistribution(row);
          } else {
            this.allChangesDone(true);
          }
        });
      });
  }

  private generateAccountDistribution(row: AccountingRowDTO) {
    if (row == null || this.selectedAccountDistribution == null) return;

    if (!this.selectedAccountDistribution.keepRow) this.deleteRow(row);

    this.loadAccountDistributionRows(
      this.selectedAccountDistribution.accountDistributionHeadId,
      row
    );
  }

  private loadAccountDistributionRows(
    accountDistributionHeadId: number,
    row: AccountingRowDTO
  ) {
    // Fetch the distribution head use getAccountDistributionHead
    this.service.get(accountDistributionHeadId).pipe(
      tap(data => {
        const distributionRows = data.rows.sort(r => r.rowNbr);

        if (this.selectedAccountDistribution?.keepRow) {
          row.accountDistributionHeadId =
            this.selectedAccountDistribution.accountDistributionHeadId;
        }

        const oldLength = this.accountingRows.length;
        let totalDebitSum = 0;
        let totalCreditSum = 0;

        for (let i = 0; i < distributionRows.length; i++) {
          const distributionRow = distributionRows[i];
          const newRow = this.addRow().row;

          newRow.accountDistributionHeadId = row.accountDistributionHeadId;
          if (this.selectedAccountDistribution?.keepRow) {
            newRow.parentRowId = row.tempInvoiceRowId;
          }

          // Helper function to set dimensions
          const setDimension = (
            sourceValue: any,
            distributionValue: any,
            keepSource: boolean
          ) => (keepSource ? sourceValue : distributionValue);

          newRow.dim1Id = distributionRow.dim1Id || row.dim1Id;
          newRow.dim2Id = setDimension(
            row.dim2Id,
            distributionRow.dim2Id,
            distributionRow.dim2KeepSourceRowAccount
          );
          newRow.dim3Id = setDimension(
            row.dim3Id,
            distributionRow.dim3Id,
            distributionRow.dim3KeepSourceRowAccount
          );
          newRow.dim4Id = setDimension(
            row.dim4Id,
            distributionRow.dim4Id,
            distributionRow.dim4KeepSourceRowAccount
          );
          newRow.dim5Id = setDimension(
            row.dim5Id,
            distributionRow.dim5Id,
            distributionRow.dim5KeepSourceRowAccount
          );
          newRow.dim6Id = setDimension(
            row.dim6Id,
            distributionRow.dim6Id,
            distributionRow.dim6KeepSourceRowAccount
          );

          // Set amounts
          this.setAmountFromDistribution(
            distributionRow,
            newRow,
            row,
            this.selectedAccountDistribution?.calculationType,
            oldLength
          );

          totalDebitSum += newRow.debitAmount || 0;
          totalCreditSum += newRow.creditAmount || 0;

          // Adjust amounts for the last row
          if (i === distributionRows.length - 1) {
            const difference = totalDebitSum - totalCreditSum;
            if (difference > 0) {
              newRow.debitAmount = (newRow.debitAmount || 0) - difference;
            } else {
              newRow.creditAmount = (newRow.creditAmount || 0) + difference;
            }
            newRow.debitAmount = parseFloat(newRow.debitAmount.toFixed(2));
            newRow.creditAmount = parseFloat(newRow.creditAmount.toFixed(2));
          }
        }

        // Finalize changes
        this.allChangesDone(false);
      })
    );
  }

  private setAmountFromDistribution(
    distributionRow: IAccountDistributionRowDTO,
    newRow: AccountingRowDTO,
    row: AccountingRowDTO,
    calculationType: TermGroup_AccountDistributionCalculationType | undefined,
    calculateRowNbrOffset: number
  ): void {
    const calculateRowItem =
      distributionRow.calculateRowNbr !== 0
        ? this.accountingRows.find(
            r =>
              r.rowNr ===
              distributionRow.calculateRowNbr + calculateRowNbrOffset
          ) || row
        : row;

    const sourceAmount = Math.abs(
      (calculateRowItem.debitAmount || 0) - (calculateRowItem.creditAmount || 0)
    );
    const isDebitAmount = (calculateRowItem.debitAmount || 0) > 0;

    let targetAmount = 0;
    const balance =
      distributionRow.sameBalance !== 0
        ? distributionRow.sameBalance
        : distributionRow.oppositeBalance;

    switch (calculationType) {
      case TermGroup_AccountDistributionCalculationType.Percent:
        targetAmount = sourceAmount * (balance / 100);
        break;

      case TermGroup_AccountDistributionCalculationType.Amount:
        targetAmount = balance;
        break;

      case TermGroup_AccountDistributionCalculationType.TotalAmount:
        // Implementation needed
        break;

      default:
        break;
    }

    targetAmount = targetAmount.round(2);

    const isSameBalance = distributionRow.sameBalance !== 0;
    newRow.debitAmount = isSameBalance
      ? isDebitAmount
        ? targetAmount
        : 0
      : isDebitAmount
        ? 0
        : targetAmount;
    newRow.debitAmount = newRow.debitAmount.round(2);

    newRow.creditAmount = isSameBalance
      ? isDebitAmount
        ? 0
        : targetAmount
      : isDebitAmount
        ? targetAmount
        : 0;
    newRow.creditAmount = newRow.creditAmount.round(2);

    newRow.amount = (newRow.debitAmount - newRow.creditAmount).round(2);

    // Set row type
    newRow.isDebitRow = newRow.debitAmount > 0;
    newRow.isCreditRow = newRow.creditAmount > 0;
  }

  private isMatchForAmount(
    match: IAccountDistributionHeadDTO,
    amount: number
  ): boolean {
    if (match.amount === 0) return true;

    switch (match.amountOperator) {
      case WildCard.LessThan:
        return amount < match.amount;
      case WildCard.LessThanOrEquals:
        return amount <= match.amount;
      case WildCard.Equals:
        return amount === match.amount;
      case WildCard.GreaterThan:
        return amount > match.amount;
      case WildCard.GreaterThanOrEquals:
        return amount >= match.amount;
      case WildCard.NotEquals:
        return amount !== match.amount;
      default:
        return false;
    }
  }

  private isMatchForDimensions(
    match: IAccountDistributionHeadDTO,
    row: AccountingRowDTO
  ): boolean {
    const dimensionChecks = [
      { expression: match.dim1Expression, number: row.dim1Nr },
      { expression: match.dim2Expression, number: row.dim2Nr },
      { expression: match.dim3Expression, number: row.dim3Nr },
      { expression: match.dim4Expression, number: row.dim4Nr },
      { expression: match.dim5Expression, number: row.dim5Nr },
      { expression: match.dim6Expression, number: row.dim6Nr },
    ];

    return dimensionChecks.every(
      ({ expression, number }) =>
        !expression || this.matchAccount(expression, number)
    );
  }

  private matchAccount(expression: string, accountNr: string): boolean {
    // If expression is empty, entered value must also be empty
    if (!expression && accountNr) return false;

    if (!accountNr) return false;

    const regEx = new RegExp(StringUtil.WildCardToRegEx(expression));
    return regEx.test(accountNr);
  }

  private deleteChildRows(row: AccountingRowDTO) {
    if (row.parentRowId && row.parentRowId > 0) {
      const parentRow = this.accountingRows.find(
        r => r.tempRowId === row.parentRowId
      );
      if (parentRow) {
        row = parentRow;
      }
    }

    let childrens = this.accountingRows.filter(
      r =>
        (r.accountDistributionHeadId === row.accountDistributionHeadId &&
          r.parentRowId === row.tempRowId) ||
        r.parentRowId === row.tempInvoiceRowId
    );
    if (!childrens || childrens.length == 0)
      childrens = this.accountingRows.filter(
        r =>
          r.accountDistributionHeadId === row.accountDistributionHeadId &&
          (r.invoiceRowId > row.invoiceRowId ||
            (r.parentRowId && r.parentRowId === row.invoiceRowId) ||
            (r.parentRowId && r.parentRowId === row.tempInvoiceRowId))
      );

    childrens.forEach(c => this.deleteRow(c));

    const newRow = this.accountingRows.filter(r => r.dim1Id === 0)[0];
    if (newRow) this.deleteRow(newRow);
  }

  public checkDeleteAccountDistribution(row: AccountingRowDTO) {
    // Check if row is parent to any account distrubution rows
    // parentRow is not working, have to be done another way
    //var parentRowId: number = (row.invoiceRowId ? row.invoiceRowId : row.tempInvoiceRowId);

    this.selectedAccountDistribution = this.accountDistributions.find(
      dist => dist.accountDistributionHeadId == row.accountDistributionHeadId
    );

    // Fallback, check distributions for import not connected to current container
    if (!this.selectedAccountDistribution)
      this.selectedAccountDistribution =
        this.accountDistributionsForImport.find(
          dist =>
            dist.accountDistributionHeadId == row.accountDistributionHeadId
        );

    if (
      this.selectedAccountDistribution &&
      this.selectedAccountDistribution.type == SoeAccountDistributionType.Auto
    ) {
      //check for automatic distribution
      //_.filter(this.accountingRows, r => r.accountDistributionHeadId == row.accountDistributionHeadId && r.invoiceRowId <= row.invoiceRowId).length == 1
      if (
        row.accountDistributionHeadId &&
        this.accountingRows.filter(
          r =>
            (r.accountDistributionHeadId == row.accountDistributionHeadId &&
              r.invoiceRowId <= row.invoiceRowId) ||
            r.tempRowId == row.tempInvoiceRowId
        ).length > 0
      ) {
        this.translationService
          .get([
            'economy.accounting.distribution.automaticdistribution.deleterowwarning.title',
            'economy.accounting.distribution.automaticdistribution.deleterowwarning.message',
            'economy.accounting.distribution.automaticdistribution.deleterowwarning.messageexisting',
            'economy.accounting.distribution.automaticdistribution.deleterowwarning.messagerow2',
          ])
          .pipe(take(1))
          .subscribe(terms => {
            const distribution = this.accountDistributions.find(
              a => a.accountDistributionHeadId === row.accountDistributionHeadId
            );
            let text: string;
            if (distribution)
              text = terms[
                'economy.accounting.distribution.automaticdistribution.deleterowwarning.messageexisting'
              ].format(distribution.name);
            else
              text =
                terms[
                  'economy.accounting.distribution.automaticdistribution.deleterowwarning.message'
                ];
            text =
              text +
              '\n' +
              terms[
                'economy.accounting.distribution.automaticdistribution.deleterowwarning.messagerow2'
              ];

            const dialog = this.messageboxService.question(
              terms[
                'economy.accounting.distribution.automaticdistribution.deleterowwarning.title'
              ],
              text
            );

            dialog.afterClosed().subscribe((val: any) => {
              if (val) {
                this.deleteChildRows(row);
              }
            });
          });
      } else this.deleteRow(row);
    } else if (
      this.selectedAccountDistribution &&
      this.selectedAccountDistribution.type ==
        SoeAccountDistributionType.Period &&
      this.selectedAccountDistribution.triggerType ==
        TermGroup_AccountDistributionTriggerType.Registration
    ) {
      // This is a period account distribution row, if removed inform the user
      const registrationType: number = this.getRegistrationType(this.container);
      const sourceId: number = this.getSourceId(this.container, row);

      this.service
        .getAccountDistributionEntriesForSource(
          row.accountDistributionHeadId,
          registrationType,
          sourceId
        )
        .pipe(
          tap(x => {
            this.existingEntryRows = x;
            const transferredRows: boolean =
              this.existingEntryRows.filter(i => i.voucherHeadId != null)
                .length > 0;

            this.translationService
              .get([
                'economy.accounting.distribution.perioddistribution',
                'economy.accounting.distribution.perioddistribution.warning',
                'economy.accounting.distribution.perioddistribution.deleteaccountrow.voucherexistsmessage',
                'economy.accounting.distribution.perioddistribution.deleteaccountrow.deletemessage',
              ])
              .pipe(take(1))
              .subscribe(terms => {
                let message: string =
                  terms[
                    'economy.accounting.distribution.perioddistribution.deleteaccountrow.deletemessage'
                  ];
                if (transferredRows)
                  message =
                    terms[
                      'economy.accounting.distribution.perioddistribution.deleteaccountrow.voucherexistsmessage'
                    ];

                const dialog = this.messageboxService.question(
                  terms['economy.accounting.distribution.perioddistribution'],
                  message
                );

                dialog.afterClosed().subscribe((val: any) => {
                  if (val) {
                  }
                });
              });
          })
        );
    }
  }

  private getRegistrationType(container: AccountingRowsContainers): number {
    switch (container) {
      case AccountingRowsContainers.Voucher:
        return TermGroup_AccountDistributionRegistrationType.Voucher;
      case AccountingRowsContainers.SupplierInvoice:
        return TermGroup_AccountDistributionRegistrationType.SupplierInvoice;
      case AccountingRowsContainers.CustomerInvoice:
        return TermGroup_AccountDistributionRegistrationType.CustomerInvoice;
      default:
        return 0;
    }
  }

  private getSourceId(
    container: AccountingRowsContainers,
    row: AccountingRowDTO
  ): number {
    return container === AccountingRowsContainers.Voucher
      ? row.voucherHeadId
      : row.invoiceId;
  }
}
