import { Component, inject, OnInit } from '@angular/core';
import { AccountDistributionAutoService } from '@features/economy/account-distribution-auto/services/account-distribution-auto.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ValidationHandler } from '@shared/handlers';
import {
  Feature,
  SoeAccountDistributionType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IAccountDimSmallDTO,
  IAccountDistributionHeadSmallDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, map, Observable, of, take, tap } from 'rxjs';
import { EconomyService } from '../../../services/economy.service';
import { AccountDistributionGridFilterForm } from '../../models/account-distribution-grid-filter-form.model';
import { AccountDistributionGridFilterDTO } from '../../models/account-distribution.model';
import { AccountDistributionService } from '../../services/account-distribution.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { AccountDistributionUrlParamsService } from '../../services/account-distribution-params.service';

@Component({
  selector: 'soe-account-distribution-grid',
  templateUrl: './account-distribution-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AccountDistributionGridComponent
  extends GridBaseDirective<
    IAccountDistributionHeadSmallDTO,
    AccountDistributionService
  >
  implements OnInit
{
  readonly service = inject(AccountDistributionService);
  private readonly accountDistributionautoService = inject(
    AccountDistributionAutoService
  );
  validationHandler = inject(ValidationHandler);
  private readonly economyService = inject(EconomyService);
  private readonly coreService = inject(CoreService);
  urlService = inject(AccountDistributionUrlParamsService);

  isPeriodAccountDistribution: boolean = false;
  isAutomaticAccountDistribution: boolean = false;
  triggerTypes: Array<SmallGenericType> = [];
  calculationTypes: Array<SmallGenericType> = [];

  //Account Dim
  private accountDim1Name?: string;
  private accountDim2Name?: string;
  private accountDim3Name?: string;
  private accountDim4Name?: string;
  private accountDim5Name?: string;
  private accountDim6Name?: string;

  form: AccountDistributionGridFilterForm =
    new AccountDistributionGridFilterForm({
      validationHandler: this.validationHandler,
      element: new AccountDistributionGridFilterDTO(),
    });

  ngOnInit() {
    super.ngOnInit();

    if (
      this.urlService.isPeriod() ||
      this.urlService.typeId() === SoeAccountDistributionType.Period
    ) {
      this.isPeriodAccountDistribution = true;
    }
    if (
      this.urlService.isAuto() ||
      this.urlService.typeId() === SoeAccountDistributionType.Auto
    ) {
      this.isAutomaticAccountDistribution = true;
    }

    this.startFlow(
      this.urlService.isPeriod()
        ? Feature.Economy_Preferences_VoucherSettings_AccountDistributionPeriod
        : Feature.Economy_Preferences_VoucherSettings_AccountDistributionAuto,
      'Economy.Accounting.AccountDistribution',
      {
        lookups: [
          this.getDimLabels(),
          this.loadTriggerTypes(),
          this.loadCalculationTypes(),
        ],
      }
    );
  }

  showOpenChange(showOpen: boolean) {
    this.form.showOpen.patchValue(showOpen);
    this.refreshGrid();
  }

  showClosedChange(showClosed: boolean) {
    this.form.showClosed.patchValue(showClosed);
    this.refreshGrid();
  }

  private loadTriggerTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(
          TermGroup.AccountDistributionCalculationType,
          false,
          false
        )
        .pipe(tap(types => (this.calculationTypes = types || [])))
    );
  }

  private loadCalculationTypes(): Observable<void> {
    return this.performLoadData.load$(
      this.coreService
        .getTermGroupContent(
          TermGroup.AccountDistributionTriggerType,
          false,
          false
        )
        .pipe(tap(types => (this.triggerTypes = types || [])))
    );
  }

  private getDimLabels() {
    return this.economyService
      .getAccountDimsSmall(
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false
      )
      .pipe(
        tap((accountDims: IAccountDimSmallDTO[]) => {
          const dim1 = accountDims.find(d => d.accountDimNr == 1);
          if (dim1) this.accountDim1Name = dim1.name;

          const dim2 = accountDims.find(d => d.accountDimNr == 2);
          if (dim2) this.accountDim2Name = dim2.name;

          const dim3 = accountDims.find(d => d.accountDimNr == 3);
          if (dim3) this.accountDim3Name = dim3.name;

          const dim4 = accountDims.find(d => d.accountDimNr == 4);
          if (dim4) this.accountDim4Name = dim4.name;

          const dim5 = accountDims.find(d => d.accountDimNr == 5);
          if (dim5) this.accountDim5Name = dim5.name;

          const dim6 = accountDims.find(d => d.accountDimNr == 6);
          if (dim6) this.accountDim6Name = dim6.name;
        })
      );
  }

  override onGridReadyToDefine(
    grid: GridComponent<IAccountDistributionHeadSmallDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.name',
        'common.description',
        'economy.accounting.accountdistribution.sorting',
        'economy.accounting.accountdistribution.type',
        'economy.accounting.accountdistribution.dayinperiod',
        'economy.accounting.accountdistribution.numberoftimes',
        'economy.accounting.accountdistribution.startdate',
        'economy.accounting.accountdistribution.enddate',
        'economy.accounting.account',
        'economy.accounting.accountdistribution.calculationtype',
        'economy.accounting.accountdistribution.totalcount',
        'economy.accounting.accountdistribution.totalamount',
        'economy.accounting.accountdistribution.saldo',
        'economy.accounting.accountdistribution.transferredcount',
        'economy.accounting.accountdistribution.lasttransferdate',
        'economy.accounting.accountdistribution.periodamount',
        'economy.accounting.accountdistribution.remainingamount',
        'economy.accounting.accountdistribution.remainingcount',
        'economy.accounting.accountdistribution.voucherregister',
        'economy.accounting.accountdistribution.import',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
          enableHiding: false,
        });
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 1,
          enableHiding: true,
        });
        this.grid.addColumnDate(
          'startDate',
          terms['economy.accounting.accountdistribution.startdate'],
          {
            flex: 1,
            enableHiding: true,
          }
        );
        this.grid.addColumnDate(
          'endDate',
          terms['economy.accounting.accountdistribution.enddate'],
          {
            flex: 1,
            enableHiding: true,
          }
        );
        if (this.isPeriodAccountDistribution) {
          this.grid.addColumnSelect(
            'triggerType',
            terms['economy.accounting.accountdistribution.type'],
            this.triggerTypes,
            undefined,
            {
              dropDownIdLabel: 'id',
              dropDownValueLabel: 'name',
              flex: 1,
              editable: true,
              enableHiding: true,
              hide: true,
            }
          );
          this.grid.addColumnNumber(
            'dayNumber',
            terms['economy.accounting.accountdistribution.dayinperiod'],
            {
              flex: 1,
              enableHiding: true,
              hide: true,
            }
          );
          this.grid.addColumnNumber(
            'periodValue',
            terms['economy.accounting.accountdistribution.numberoftimes'],
            {
              flex: 1,
              enableHiding: true,
              hide: true,
            }
          );
        }

        this.grid.addColumnSelect(
          'calculationType',
          terms['economy.accounting.accountdistribution.calculationtype'],
          this.calculationTypes,
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 1,
            hide: true,
            enableHiding: true,
          }
        );

        if (this.isPeriodAccountDistribution) {
          this.grid.addColumnNumber(
            'entryTotalAmount',
            terms['economy.accounting.accountdistribution.totalamount'],
            { enableHiding: true, flex: 1, aggFuncOnGrouping: 'sum' }
          );
          this.grid.addColumnNumber(
            'entryPeriodAmount',
            terms['economy.accounting.accountdistribution.periodamount'],
            { enableHiding: true, flex: 1, aggFuncOnGrouping: 'sum' }
          );
          this.grid.addColumnNumber(
            'entryRemainingAmount',
            terms['economy.accounting.accountdistribution.remainingamount'],
            { enableHiding: true, flex: 1, aggFuncOnGrouping: 'sum' }
          );
          this.grid.addColumnNumber(
            'entryTransferredAmount',
            terms['economy.accounting.accountdistribution.saldo'],
            { enableHiding: true, flex: 1, aggFuncOnGrouping: 'sum' }
          );
          this.grid.addColumnNumber(
            'entryTransferredCount',
            terms['economy.accounting.accountdistribution.transferredcount'],
            { enableHiding: true, flex: 1 }
          );
          this.grid.addColumnNumber(
            'entryTotalCount',
            terms['economy.accounting.accountdistribution.totalcount'],
            { enableHiding: true, flex: 1 }
          );
          this.grid.addColumnNumber(
            'entryRemainingCount',
            terms['economy.accounting.accountdistribution.remainingcount'],
            { enableHiding: true, flex: 1 }
          );
          this.grid.addColumnDate(
            'entryLatestTransferDate',
            terms['economy.accounting.accountdistribution.lasttransferdate'],
            { flex: 1 }
          );
        }

        (this.accountDim1Name || this.accountDim1Name?.length == 0) &&
          this.grid.addColumnText('dim1Expression', this.accountDim1Name, {
            flex: 1,
            enableHiding: true,
            filter: true,
            sort: 'asc',
          });

        (this.accountDim2Name || this.accountDim2Name?.length == 0) &&
          this.grid.addColumnText('dim2Expression', this.accountDim2Name, {
            flex: 1,
            enableHiding: true,
            hide: true,
          });

        (this.accountDim3Name || this.accountDim3Name?.length == 0) &&
          this.grid.addColumnText('dim3Expression', this.accountDim3Name, {
            flex: 1,
            enableHiding: true,
            hide: true,
          });

        (this.accountDim4Name || this.accountDim4Name?.length == 0) &&
          this.grid.addColumnText('dim4Expression', this.accountDim4Name, {
            flex: 1,
            enableHiding: true,
            hide: true,
          });

        (this.accountDim5Name || this.accountDim5Name?.length == 0) &&
          this.grid.addColumnText('dim5Expression', this.accountDim5Name, {
            flex: 1,
            enableHiding: true,
            hide: true,
          });

        (this.accountDim6Name || this.accountDim6Name?.length == 0) &&
          this.grid.addColumnText('dim6Expression', this.accountDim6Name, {
            flex: 1,
            enableHiding: true,
            hide: true,
          });

        if (this.isAutomaticAccountDistribution) {
          this.grid.addColumnBool(
            'useInVoucher',
            terms['economy.accounting.accountdistribution.voucherregister'],
            { flex: 1, enableHiding: true }
          );
          this.grid.addColumnBool(
            'useInImport',
            terms['economy.accounting.accountdistribution.import'],
            { flex: 1, enableHiding: true }
          );
        }

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        this.grid.enableGroupTotalFooter();
        super.finalizeInitGrid();
      });
  }

  override loadData(): Observable<IAccountDistributionHeadSmallDTO[]> {
    if (this.isPeriodAccountDistribution) {
      return this.performLoadData.load$(
        this.service
          .getGrid(undefined, {
            loadOpen: this.form.showOpen.value,
            loadClosed: this.form.showClosed.value,
            loadEntries: true,
          })
          .pipe(
            map(data => {
              if (data) {
                data.forEach(item => {
                  if (item)
                    item.entryRemainingCount =
                      item.entryTotalCount - item.entryTransferredCount;
                  item.entryRemainingAmount =
                    item.entryTotalAmount - item.entryTransferredAmount;
                });
              }
              return data ?? [];
            })
          )
      );
    } else if (this.isAutomaticAccountDistribution) {
      return this.performLoadData.load$(
        this.accountDistributionautoService.getGrid().pipe(
          tap(data => {
            this.rowData = new BehaviorSubject<
              IAccountDistributionHeadSmallDTO[]
            >(data || []);
          })
        )
      );
    }

    return of([]);
  }
}
