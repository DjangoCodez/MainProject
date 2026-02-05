import { Component, OnInit, inject } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  BudgetHeadFlattenedDTO,
  BudgetRowFlattenedDTO,
  GetResultPerPeriodModel,
} from '../../models/budget.model';
import { BudgetService } from '../../services/budget.service';
import { BudgetForm } from '../../models/budget-form.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  BudgetHeadStatus,
  DistributionCodeBudgetType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  BehaviorSubject,
  Observable,
  distinctUntilChanged,
  map,
  of,
  tap,
} from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { EconomyService } from '../../../services/economy.service';
import { DistributionCodesService } from '../../../distribution-codes/services/distribution-codes.service';
import { DistributionCodeHeadDTO } from '../../../distribution-codes/models/distribution-codes.model';
import { AccountDimSmallDTO } from '@shared/components/accounting-rows/models/accounting-rows-model';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { MessageboxComponent } from '@ui/dialog/messagebox/messagebox.component';
import { MessageboxData } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { EditLoadResultComponent } from '../edit-load-result/edit-load-result.component';
import { LoadResultDialogData } from '../edit-load-result/models/edit-load-result.model';
import { MatDialogRef } from '@angular/material/dialog';
import { ProgressOptions } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import { TermCollection } from '@shared/localization/term-types';

@Component({
  selector: 'soe-budget-edit',
  templateUrl: './budget-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class BudgetEditComponent
  extends EditBaseDirective<BudgetHeadFlattenedDTO, BudgetService, BudgetForm>
  implements OnInit
{
  service = inject(BudgetService);
  private economyService = inject(EconomyService);
  private coreService = inject(CoreService);
  private disCodeService = inject(DistributionCodesService);
  private dialogService = inject(DialogService);
  private messageService = inject(MessageboxService);

  accountDims: AccountDimSmallDTO[] = [];
  accountYearsDict: SmallGenericType[] = [];
  distributionCodes: SmallGenericType[] = [];
  distCodesHeads: DistributionCodeHeadDTO[] = [];
  dim2s: SmallGenericType[] = [];
  dim3s: SmallGenericType[] = [];
  useDim2Label!: string;
  useDim3Label!: string;
  dim2Label!: string;
  dim3Label!: string;
  dim2name!: string;
  dim3name!: string;
  noOfPeriod: number = 12;
  budgetHeadId: number = 0;
  budgetHeadStatus: number = 0;
  timerToken: number = 0;
  currentGuid!: string;
  progress!: MatDialogRef<MessageboxComponent<MessageboxData>>;
  progressText!: string;
  isClearGrid: boolean = false;
  setDefinitePersmission: boolean = false;

  //SubGrid
  budgetRowData = new BehaviorSubject<BudgetRowFlattenedDTO[]>([]);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Accounting_Budget_Edit, {
      lookups: [
        this.loadAccountYears(),
        this.loadDistributionCodes(),
        this.loadAccountDims(),
      ],
      additionalModifyPermissions: [
        Feature.Economy_Accounting_Budget_Edit_Definite,
      ],
    });

    this.form?.useDim2.valueChanges
      .pipe(distinctUntilChanged())
      .subscribe(value => {
        if (value) {
          this.dim2Enable();
        } else {
          this.dim2Disabled();
        }
      });

    this.form?.useDim3.valueChanges
      .pipe(distinctUntilChanged())
      .subscribe(value => {
        if (value) {
          this.dim3Enable();
        } else {
          this.dim3Disabled();
        }
      });

    this.form?.noOfPeriods.valueChanges
      .pipe(distinctUntilChanged())
      .subscribe(value => {
        this.onPeriodChanged(value);
      });
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'economy.accounting.budget.use',
      'economy.accounting.budget.default',
      'economy.accounting.budget.getting',
    ]);
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value, true).pipe(
        tap(value => {
          this.form?.reset(value);
          this.form?.customBudgetRowsPatchValues(
            <BudgetRowFlattenedDTO[]>value.rows
          );
          this.budgetHeadId = value.budgetHeadId;
          this.budgetHeadStatus = +value.status;
          this.budgetRowData.next(this.form?.rows.value ?? []);

          if (this.budgetHeadStatus === BudgetHeadStatus.Active) {
            this.form?.lockUnlockFormControls(true);
          }
        })
      )
    );
  }

  override onFinished(): void {
    this.formLockValidate(this.form?.value);
    this.setDefaultValues();
  }

  override newRecord(): Observable<void> {
    let updateValues = () => {};
    updateValues = () => {
      if (this.form?.isCopy) {
        this.budgetRowData.next(this.form.rows.value);
      }
    };

    return of(updateValues());
  }

  override onPermissionsLoaded(): void {
    super.onPermissionsLoaded();
    this.setDefinitePersmission = this.flowHandler.hasModifyAccess(
      Feature.Economy_Accounting_Budget_Edit_Definite
    );
  }

  private setDefaultValues() {
    this.useDim2Label =
      this.terms['economy.accounting.budget.use'] + ' ' + this.dim2name;
    this.useDim3Label =
      this.terms['economy.accounting.budget.use'] + ' ' + this.dim3name;
    this.dim2Label =
      this.terms['economy.accounting.budget.default'] + ' ' + this.dim2name;
    this.dim3Label =
      this.terms['economy.accounting.budget.default'] + ' ' + this.dim3name;
    this.progressText = this.terms['economy.accounting.budget.getting'];
  }

  private loadAccountYears(): Observable<SmallGenericType[]> {
    return this.performLoadData.load$(
      this.economyService.getAccountYears().pipe(
        tap(x => {
          this.accountYearsDict = [{ id: 0, name: '' }, ...[...x].reverse()];
        })
      )
    );
  }

  private loadDistributionCodes(): Observable<SmallGenericType[]> {
    return this.performLoadData.load$(
      this.disCodeService
        .getDistributionCodes(true, DistributionCodeBudgetType.AccountingBudget)
        .pipe(
          map((codeDtos: DistributionCodeHeadDTO[]) => {
            return codeDtos.sort((a, b) => a.name.localeCompare(b.name));
          }),
          tap(codes => {
            this.distCodesHeads = codes;
            const distributionCodeHead = codes.map(code => ({
              id: code.distributionCodeHeadId,
              name: code.name,
            }));
            this.distributionCodes = [
              { id: 0, name: '' },
              ...distributionCodeHead,
            ];
          })
        )
    );
  }

  private loadAccountDims(): Observable<void> {
    return this.performLoadData.load$(
      this.economyService
        .getAccountDimsSmall(
          false,
          false,
          true,
          false,
          false,
          false,
          false,
          true
        )
        .pipe(
          tap((dims: AccountDimSmallDTO[]) => {
            this.accountDims = dims;
            dims.forEach(dim => {
              if (dim.accountDimNr === 2) {
                this.dim2name = dim.name;
                this.dim2s =
                  dim.accounts?.map(
                    a =>
                      new SmallGenericType(
                        a.accountId,
                        a.accountNr + ' ' + a.name
                      )
                  ) ?? [];
              } else if (dim.accountDimNr === 3) {
                this.dim3name = dim.name;
                this.dim3s =
                  dim.accounts?.map(a => ({
                    id: a.accountId,
                    name: a.accountNr + ' ' + a.name,
                  })) ?? [];
                if (!dim.accounts) {
                  dim.accounts = [];
                }
                this.dim2s = [{ id: 0, name: ' ' }, ...this.dim2s];
                this.dim3s = [{ id: 0, name: ' ' }, ...this.dim3s];
              }
            });
          })
        )
    );
  }

  private formLockValidate(value: BudgetHeadFlattenedDTO) {
    if (!value) {
      this.dim2Disabled();
      this.dim3Disabled();
      return;
    }

    if (value?.status === 2 || !this.flowHandler.modifyPermission()) {
      this.form?.disable();
      this.form?.disable();
    } else {
      this.form?.enable();
      this.form?.enable();
    }
    if (!value.useDim2 || !this.flowHandler.modifyPermission()) {
      this.form?.dim2Id.disable();
    } else {
      this.form?.dim2Id.enable();
    }
    if (!value.useDim3) {
      this.form?.dim3Id.disable();
    }

    const disable =
      !this.flowHandler.modifyPermission() || this.form?.lockStatus.value === 2;
    this.form?.lockUnlockFormControls(disable);
  }

  private dim2Enable(): void {
    this.form?.dim2Id.enable();
  }

  private dim3Enable(): void {
    this.form?.dim3Id.enable();
  }

  private dim2Disabled(): void {
    this.form?.dim2Id.disable();
    this.form?.dim2Id.setValue(0);
  }

  private dim3Disabled(): void {
    this.form?.dim3Id.disable();
    this.form?.dim3Id.setValue(0);
  }

  onPeriodChanged(value: string) {
    if (value == '') return;

    this.noOfPeriod = parseInt(value);
  }

  editLoadResultDialog() {
    if (this.form?.accountYearId.value) {
      const dialogData = new LoadResultDialogData();
      dialogData.dim2Name = this.dim2name;
      dialogData.dim3Name = this.dim3name;
      dialogData.size = 'lg';

      this.dialogService
        .open(EditLoadResultComponent, dialogData)
        .afterClosed()
        .subscribe((value: LoadResultDialogData) => {
          this.form?.markAsDirty();
          this.loadPeriodBudgets(value);
        });
    }
  }

  loadPeriodBudgets(dialogResult: LoadResultDialogData) {
    if (!dialogResult) return;

    const dims = [];
    if (dialogResult.useDim2) dims.push(2);
    if (dialogResult.useDim3) dims.push(3);

    const values = new GetResultPerPeriodModel(
      this.form?.accountYearId.value,
      this.noOfPeriod,
      0,
      dims
    );
    this.currentGuid = values.key;
    this.performLoadData
      .load$(this.service.getBalanceChangePerPeriod(values))
      .subscribe(result => {
        this.progress = this.messageService.progress(
          'common.status',
          'core.loading'
        );
        this.timerToken = window.setInterval(
          () => this.getProgress(false),
          500
        );
      });
  }

  private getProgress(keepExistingRows: boolean = false) {
    this.coreService.getProgressInfo(this.currentGuid).subscribe(p => {
      if (p.abort) {
        if (this.progress && this.progress.getState() == 0)
          this.progress.close();
        this.getProcessedResult(keepExistingRows);
      }
    });
  }

  private stopTimer() {
    if (this.timerToken) {
      window.clearInterval(this.timerToken);
      this.timerToken = 0;
    }
  }

  private getProcessedResult(keepExistingRows: boolean = false) {
    this.stopTimer();
    this.performLoadData.load(
      this.service.getBalanceChangeResult(this.currentGuid).pipe(
        map(x => {
          if (x.length > 0) {
            x.forEach((value, index) => {
              value.dim1Id = value.accountId;
              if (value.budgetRowId <= 0) {
                value.budgetRowId = -index - 1;
              }
            });
          }
          this.form?.customBudgetRowsPatchValues(x);
          this.budgetRowData.next(
            <BudgetRowFlattenedDTO[]>this.form?.rows.value
          );
        })
      )
    );
  }

  clearGridRows() {
    this.budgetRowData.next([]);
    this.form?.rows.clear();
    this.form?.markAsDirty();
    this.form?.markAsTouched();
  }

  lock() {
    if (this.budgetHeadStatus !== BudgetHeadStatus.Active) {
      this.budgetHeadStatus = 2;
      this.form?.lockUnlockFormControls(true);
      this.form?.patchValue({ status: BudgetHeadStatus.Active });
      this.form?.markAsDirty();
      this.form?.markAsTouched();
      this.performSave();
    }
  }

  unlock() {
    if (this.budgetHeadStatus !== BudgetHeadStatus.Preliminary) {
      this.form?.lockUnlockFormControls(false);
      this.budgetHeadStatus = 1;
      this.form?.patchValue({ status: BudgetHeadStatus.Preliminary });
      this.form?.markAsDirty();
      this.form?.markAsTouched();
      this.performSave();
    }
  }

  override performSave(options?: ProgressOptions | undefined): void {
    if (!this.form || !this.service) return;

    this.form?.removeEmptyBudgetRows();
    const dto = <BudgetHeadFlattenedDTO>this.form?.getRawValue();
    dto.rows = <BudgetRowFlattenedDTO[]>this.form?.rows.getRawValue();
    dto.rows = dto.rows.map(r => {
      if (r.budgetRowId < 0) r.budgetRowId = 0;
      return r;
    });
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(dto).pipe(
        tap(value => {
          if (value.success) {
            this.updateFormValueAndEmitChange(value);
          }
        })
      ),
      undefined,
      undefined,
      options
    );
  }
}
