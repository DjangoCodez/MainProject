import { Component, inject, OnInit, signal } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  Feature,
  TermGroup,
  TermGroup_SysPayrollType,
  TermGroup_TimeWorkReductionWithdrawalMethod,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { Observable, of, take, tap } from 'rxjs';
import {
  ITimeWorkReductionReconciliationDTO,
  ITimeWorkReductionReconciliationYearDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { TimeWorkReductionForm } from '../../models/time-work-reduction-form.model';
import { EmployeeGroupsService } from '@features/time/employee-groups/services/employee-groups.service';
import { TimeWorkAccountService } from '@features/time/time-work-account/services/time-work-account.service';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import { TimeWorkReductionService } from '../../services/time-work-reduction.service';
import {
  ITimeWorkReductionReconciliationDialogData,
  TimeWorkReductionReconciliationDialogComponent,
} from '../time-work-reduction-reconciliation-dialog/time-work-reduction-reconciliation-dialog.component';
import { maxBy } from 'lodash';

@Component({
  selector: 'soe-time-work-reduction-edit',
  templateUrl: './time-work-reduction-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeWorkReductionEditComponent
  extends EditBaseDirective<
    ITimeWorkReductionReconciliationDTO,
    TimeWorkReductionService,
    TimeWorkReductionForm
  >
  implements OnInit
{
  service = inject(TimeWorkReductionService);
  coreService = inject(CoreService);
  employeeGroupsService = inject(EmployeeGroupsService);
  timeWorkAccountService = inject(TimeWorkAccountService);
  dialogService = inject(DialogService);

  timeAccumulators: ISmallGenericType[] = [];
  timeWorkReductionWithdrawalMethods: ISmallGenericType[] = [];
  filteredWithdrawalMethods: ISmallGenericType[] = [];
  pensionPayrollProducts: ISmallGenericType[] = [{ id: 0, name: '' }];
  directPaymentpayrollProducts: ISmallGenericType[] = [{ id: 0, name: '' }];
  payrollProducts: IProductSmallDTO[] = [];
  employeeStatus: ISmallGenericType[] = [];

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Time_Time_TimeWorkReduction, {
      lookups: [
        this.loadEmployeeStatus(),
        this.loadTimeAccumulators(),
        this.loadDefaultWithdrawalMethod(),
        of(this.loadPayrollProducts()),
      ],
    });
  }
  createGridToolbar() {
    //TODO move to subgrid
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('new', {
          iconName: signal('times-circle'),
          caption: signal('time.time.timeworkreduction.newreconcilation'),
          tooltip: signal('time.time.timeworkreduction.newreconcilation'),
          onAction: () => {
            this.editYear(true, undefined as any);
          },
        }),
      ],
    });
  }

  checkChanged(onLoad: boolean = false) {
    setTimeout(() => {
      if (this.form === undefined) return;

      const usePensionDeposit = this.form?.value.usePensionDeposit;
      const useDirectPayment = this.form?.value.useDirectPayment;

      this.filteredWithdrawalMethods = [];

      if (!onLoad)
        this.form.controls.defaultWithdrawalMethod.setValue(undefined);

      if (usePensionDeposit)
        this.filteredWithdrawalMethods.push(
          this.timeWorkReductionWithdrawalMethods.find(
            m =>
              m.id ===
              TermGroup_TimeWorkReductionWithdrawalMethod.PensionDeposit
          )!
        );

      if (useDirectPayment)
        this.filteredWithdrawalMethods.push(
          this.timeWorkReductionWithdrawalMethods.find(
            m =>
              m.id === TermGroup_TimeWorkReductionWithdrawalMethod.DirectPayment
          )!
        );
    }, 100);
  }
  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: ITimeWorkReductionReconciliationDTO) => {
          this.form?.customPatchValue(value);
          this.checkChanged(true);
        })
      )
    );
  }
  loadPayrollProducts() {
    return this.timeWorkAccountService
      .getPayrollProductsSmall()
      .subscribe(x => {
        this.payrollProducts = x;
        this.loadPensionProducts().subscribe(() => {
          this.loadDirectPaymentProducts().subscribe(() => {});
        });
      });
  }

  loadPensionProducts() {
    return this.timeWorkAccountService
      .GetPayrollProductIdsByType(
        TermGroup_SysPayrollType.SE_PensionPremium,
        TermGroup_SysPayrollType.SE_PensionPremium_TimeWorkReduction
      )
      .pipe(
        tap(x => {
          x.forEach(y => {
            const product = this.payrollProducts.find(f => f.productId == y);
            if (product != null) {
              this.pensionPayrollProducts.push({
                id: product.productId,
                name: product.numberName,
              });
            }
          });
        })
      );
  }

  loadDirectPaymentProducts() {
    return this.timeWorkAccountService
      .GetPayrollProductIdsByType(
        TermGroup_SysPayrollType.SE_GrossSalary,
        TermGroup_SysPayrollType.SE_GrossSalary_TimeWorkReduction
      )
      .pipe(
        tap(x => {
          x.forEach(y => {
            const product = this.payrollProducts.find(f => f.productId == y);
            if (product != null) {
              this.directPaymentpayrollProducts.push({
                id: product.productId,
                name: product.numberName,
              });
            }
          });
        })
      );
  }

  private loadTimeAccumulators() {
    return this.service
      .getTimeAccumulatorsForReductionDict()
      .pipe(tap(x => (this.timeAccumulators = x)));
  }

  private loadDefaultWithdrawalMethod() {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeWorkReductionWithdrawalMethod,
        false,
        false
      )
      .pipe(
        tap(x => {
          this.timeWorkReductionWithdrawalMethods = x;
        })
      );
  }

  editYear(
    isNew: boolean,
    row: ITimeWorkReductionReconciliationYearDTO = {} as any
  ) {
    const withdrawalMethods: ISmallGenericType[] = [];

    withdrawalMethods.push(
      this.timeWorkReductionWithdrawalMethods.find(
        m => m.id === TermGroup_TimeWorkReductionWithdrawalMethod.NotChoosed
      )!
    );
    withdrawalMethods.push(...this.filteredWithdrawalMethods);

    const latestYear = maxBy(
      this.form?.timeWorkReductionReconciliationYearDTO?.controls,
      c => c.value.stop?.getTime() ?? 0
    );
    this.dialogService
      .open(TimeWorkReductionReconciliationDialogComponent, {
        title: isNew ? 'common.new' : 'core.edit',
        size: 'fullscreen',
        hideFooter: true,
        new: isNew,
        usePension: this.form?.value.usePensionDeposit ?? false,
        useDirectPayment: this.form?.value.useDirectPayment ?? false,
        withdrawalMethods: withdrawalMethods,
        pensionPayrollProducts: this.pensionPayrollProducts,
        directPaymentpayrollProducts: this.directPaymentpayrollProducts,
        row: row,
        timeWorkReductionReconciliationId: this.form?.getIdControl()?.value,
        latestYear: latestYear?.value,
        employeeStatus: this.employeeStatus,
      } as unknown as ITimeWorkReductionReconciliationDialogData)
      .afterClosed()
      .pipe(take(1))

      .subscribe(() => {
        this.loadData().subscribe(() => {});
      });
  }
  private loadEmployeeStatus() {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeWorkReductionReconciliationEmployeeStatus,
        false,
        true
      )
      .pipe(
        tap(x => {
          this.employeeStatus = x;
        })
      );
  }
  getPensionDepositName(id: number | undefined): string {
    return this.pensionPayrollProducts?.find(p => p.id === id)?.name || '';
  }
  getDirectPaymentName(id: number | undefined): string {
    return (
      this.directPaymentpayrollProducts?.find(p => p.id === id)?.name || ''
    );
  }
}
