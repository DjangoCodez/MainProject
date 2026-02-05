import {
  Component,
  OnInit,
  Input,
  inject,
  signal,
  effect,
} from '@angular/core';
import { TimeWorkAccountDTO } from '../../../models/timeworkaccount.model';
import { TimeWorkAccountService } from '../../services/time-work-account.service';
import {
  Feature,
  TermGroup,
  TermGroup_TimeWorkAccountWithdrawalMethod,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { TimeWorkAccountForm } from '../../models/time-work-account-form.model';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { tap } from 'rxjs/operators';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { BehaviorSubject, Observable } from 'rxjs';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ITimeWorkAccountYearDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

@Component({
  selector: 'soe-time-work-account-edit',
  templateUrl: './time-work-account-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TimeWorkAccountEditComponent
  extends EditBaseDirective<TimeWorkAccountDTO, TimeWorkAccountService>
  implements OnInit
{
  @Input() form: TimeWorkAccountForm | undefined;

  settingsExpanderIsOpen = true;
  defaultWithdrawalMethods: SmallGenericType[] = [];
  defaultPaidLeaveNotUsedMethods: SmallGenericType[] = [];
  timeWorkAccount: TimeWorkAccountDTO = new TimeWorkAccountDTO();
  timeWorkAccountYears = new BehaviorSubject<ITimeWorkAccountYearDTO[]>([]);
  service = inject(TimeWorkAccountService);
  coreService = inject(CoreService);
  timeWorkAccountId = signal(0);
  // Lookups
  withdrawalMethods: SmallGenericType[] = [];

  get currentLanguage(): string {
    return SoeConfigUtil.language;
  }

  // INIT

  ngOnInit() {
    super.ngOnInit();
    this.loadDataFn = this.loadData;
    this.startFlow(Feature.Time_Payroll_TimeWorkAccount, {
      lookups: this.loadWithdrawalMethods(),
      useLegacyToolbar: true,
    });
  }
  constructor() {
    super();
    effect(() => {
      this.timeWorkAccountId.set(this.form?.value.timeWorkAccountId);
    });
  }
  // SERVICE CALLS

  private loadWithdrawalMethods() {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeWorkAccountWithdrawalMethod,
        false,
        true
      )
      .pipe(
        tap(x => {
          this.withdrawalMethods = x;
          this.updateSelectableWithdrawalMethods(true);
        })
      );
  }

  loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value, true).pipe(
        tap(value => {
          this.form?.customPatchValue(value);
          this.timeWorkAccountYears.next(value.timeWorkAccountYears);
          this.timeWorkAccountId.set(value.timeWorkAccountId);
        })
      )
    );
  }

  // HELPERS

  withdrawalMethodToSmallGenericType(
    method: TermGroup_TimeWorkAccountWithdrawalMethod
  ): SmallGenericType {
    const matchedMethod = this.withdrawalMethods.find(e => e.id == method);
    const type = new SmallGenericType(
      matchedMethod?.id ?? TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed,
      matchedMethod?.name ?? ''
    );

    return type;
  }

  updateSelectableWithdrawalMethods(initial: boolean) {
    // DefaultWithdrawalMethods
    this.defaultWithdrawalMethods.length = 0;
    this.defaultWithdrawalMethods.push(
      this.withdrawalMethodToSmallGenericType(
        TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed
      )
    );

    if (this.form?.value.usePensionDeposit) {
      this.defaultWithdrawalMethods.push(
        this.withdrawalMethodToSmallGenericType(
          TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit
        )
      );
    }

    if (this.form?.value.usePaidLeave) {
      this.defaultWithdrawalMethods.push(
        this.withdrawalMethodToSmallGenericType(
          TermGroup_TimeWorkAccountWithdrawalMethod.PaidLeave
        )
      );
    }

    if (this.form?.value.useDirectPayment) {
      this.defaultWithdrawalMethods.push(
        this.withdrawalMethodToSmallGenericType(
          TermGroup_TimeWorkAccountWithdrawalMethod.DirectPayment
        )
      );
    }

    // DefaultPaidLeaveNotUsedMethods
    this.defaultPaidLeaveNotUsedMethods.length = 0;
    this.defaultPaidLeaveNotUsedMethods.push(
      this.withdrawalMethodToSmallGenericType(
        TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed
      )
    );

    if (this.form?.value.usePensionDeposit) {
      this.defaultPaidLeaveNotUsedMethods.push(
        this.withdrawalMethodToSmallGenericType(
          TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit
        )
      );
    }

    if (this.form?.value.useDirectPayment) {
      this.defaultPaidLeaveNotUsedMethods.push(
        this.withdrawalMethodToSmallGenericType(
          TermGroup_TimeWorkAccountWithdrawalMethod.DirectPayment
        )
      );
    }

    if (!initial) {
      this.form?.defaultWithdrawalMethod.reset();
      this.timeWorkAccount.defaultWithdrawalMethod =
        TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed;
      this.form?.defaultPaidLeaveNotUsed.reset();
      this.timeWorkAccount.defaultPaidLeaveNotUsed =
        TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed;
    }
  }
}
