import {
  Component,
  OnInit,
  computed,
  inject,
  resource,
  signal,
} from '@angular/core';
import {
  Feature,
  TermGroup_AnnualLeaveTransactionType,
} from '@shared/models/generated-interfaces/Enumerations';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { IAnnualLeaveTransactionEditDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { AnnualLeaveBalanceService } from '../../services/annual-leave-balance.service';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ProgressOptions } from '@shared/services/progress';
import { CrudActionTypeEnum } from '@shared/enums';
import { Observable, tap } from 'rxjs';
import { AnnualLeaveTransactionForm } from '../../models/annual-leave-balance-form.model';
import { EmployeeService } from '@features/time/services/employee.service';
import { Validators } from '@angular/forms';
import { rxResource } from '@angular/core/rxjs-interop';

@Component({
  selector: 'soe-annual-leave-balance-edit',
  templateUrl: './annual-leave-balance-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AnnualLeaveBalanceEditComponent
  extends EditBaseDirective<
    IAnnualLeaveTransactionEditDTO,
    AnnualLeaveBalanceService,
    AnnualLeaveTransactionForm
  >
  implements OnInit
{
  selectedDate = signal(new Date());

  employees: SmallGenericType[] = [];
  allTypes: any[] = [];
  types: any[] = [];

  service = inject(AnnualLeaveBalanceService);
  employeeService = inject(EmployeeService);

  performTypes = new Perform<SmallGenericType[]>(this.progressService);
  performEmployees = new Perform<SmallGenericType[]>(this.progressService);

  manuallyEarnedChosen = signal(true);
  manuallySpentChosen = signal(false);

  hasManuallySpentFlag = signal(false);
  hasManuallyEarnedFlag = signal(false);
  chosenType = signal<TermGroup_AnnualLeaveTransactionType | null>(null);
  isCalculatedTypeWithManuallySpentFlag = computed(() => {
    return (
      this.chosenType() === TermGroup_AnnualLeaveTransactionType.Calculated &&
      this.hasManuallySpentFlag()
    );
  });
  permittedToDelete = computed(() => {
    return (
      this.chosenType() ===
        TermGroup_AnnualLeaveTransactionType.ManuallyEarned ||
      this.chosenType() ===
        TermGroup_AnnualLeaveTransactionType.ManuallySpent ||
      (this.chosenType() === TermGroup_AnnualLeaveTransactionType.Calculated &&
        this.isCalculatedTypeWithManuallySpentFlag())
    );
  });

  employeesResource = rxResource({
    params: () => ({
      dateFrom: this.selectedDate(),
      dateTo: this.selectedDate(),
      employeeIds: [],
      showInactive: false,
      showEnded: true,
      showNotStarted: true,
      filterOnAnnualLeaveAgreement: true,
    }),
    stream: ({ params }) =>
      this.employeeService.getEmployeesForGridDict(params).pipe(
        tap(value => {
          this.employees = value;
          this.verifySelectedEmployee();
        })
      ),
  });

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Time_Employee_AnnualLeaveBalance, {
      lookups: [this.loadTypes()],
    });

    this.form?.type.valueChanges.subscribe(value => {
      this.calculateChangesAccordingToType();
    });
    this.form?.dateEarned.valueChanges.subscribe(value => {
      if (value) {
        this.selectedDate.set(value);
      }
    });
    this.form?.dateSpent.valueChanges.subscribe(value => {
      if (value) {
        this.selectedDate.set(value);
      }
    });
    this.form?.annualLeaveTransactionId.valueChanges.subscribe(value => {
      if (value && value > 0) {
        this.form?.type?.disable();
        this.form?.employeeId?.disable();
      } else {
        this.form?.type?.enable();
        this.form?.employeeId?.enable();
      }
    });
    this.form?.type.valueChanges.subscribe(value => {
      this.chosenType.set(value);
      if (value === TermGroup_AnnualLeaveTransactionType.Calculated) {
        this.types = this.allTypes.filter(
          t => t.id == TermGroup_AnnualLeaveTransactionType.Calculated
        );
      } else if (value === TermGroup_AnnualLeaveTransactionType.YearlyBalance) {
        this.types = this.allTypes.filter(
          t => t.id == TermGroup_AnnualLeaveTransactionType.YearlyBalance
        );
      } else {
        this.types = this.allTypes.filter(
          t =>
            t.id === TermGroup_AnnualLeaveTransactionType.ManuallyEarned ||
            t.id === TermGroup_AnnualLeaveTransactionType.ManuallySpent
        );
      }
    });
    this.form?.manuallySpent.valueChanges.subscribe(value => {
      this.hasManuallySpentFlag.set(value);
    });
    this.setInitValues();
  }

  setInitValues() {
    if (this.form?.isNew) {
      this.form?.type.setValue(
        TermGroup_AnnualLeaveTransactionType.ManuallyEarned
      );
    }
  }

  verifySelectedEmployee() {
    if (
      this.employees.filter(e => e.id === this.form?.employeeId.value)
        .length === 0
    ) {
      this.form?.employeeId.setValue(null);
    }
  }

  calculateChangesAccordingToType() {
    if (!this.form) return;

    const type = this.form.type.value;

    this.manuallyEarnedChosen.set(
      type === TermGroup_AnnualLeaveTransactionType.ManuallyEarned
    );
    this.manuallySpentChosen.set(
      type === TermGroup_AnnualLeaveTransactionType.ManuallySpent
    );

    this.form.dateEarned.clearValidators();
    this.form.dateSpent.clearValidators();
    this.form.minutesSpent.clearValidators();
    this.form.minutesEarned.clearValidators();
    this.form.accumulatedMinutes.clearValidators();

    if (this.manuallyEarnedChosen()) {
      this.form.dateEarned.setValue(DateUtil.getToday());
      this.form.dateSpent.setValue(null, { emitEvent: false });
      this.form.minutesSpent.setValue(null);

      //this.form.minutesEarned.setValidators([Validators.required]);
      this.form.dateEarned.setValidators([Validators.required]);
      this.form.accumulatedMinutes.setValidators([Validators.required]);
    } else if (this.manuallySpentChosen()) {
      this.form.dateSpent.setValue(DateUtil.getToday());
      this.form.dateEarned.setValue(null, { emitEvent: false });
      this.form.minutesEarned.setValue(null);

      this.form.dateSpent.setValidators([Validators.required]);
      this.form.minutesSpent.setValidators([Validators.required]);
    }
    this.form.updateValueAndValidity();
  }

  loadTypes() {
    return this.performTypes.load$(
      this.service.getTransactionTypes(false).pipe(
        tap(value => {
          this.allTypes = value;
          this.types = value.filter(
            t =>
              t.id === TermGroup_AnnualLeaveTransactionType.ManuallyEarned ||
              t.id === TermGroup_AnnualLeaveTransactionType.ManuallySpent
          );
        })
      )
    );
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      (<any>this.service).get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.form?.customPatchValue(<IAnnualLeaveTransactionEditDTO>value);
        })
      ),
      { showDialogDelay: 1000 }
    );
  }

  override performSave(options?: ProgressOptions | undefined): void {
    if (!this.form || !this.service) return;

    const dto = this.form?.getRawValue();

    // Convert hours from "h:mm" to minutes
    if (
      dto.minutesEarned != null &&
      dto.minutesEarned.toString().includes(':')
    ) {
      dto.minutesEarned = DateUtil.timeSpanToMinutes(dto.minutesEarned);
    }
    if (dto.minutesSpent != null && dto.minutesSpent.toString().includes(':')) {
      dto.minutesSpent = DateUtil.timeSpanToMinutes(dto.minutesSpent);
    }
    if (
      dto.accumulatedMinutes != null &&
      dto.accumulatedMinutes.toString().includes(':')
    ) {
      dto.accumulatedMinutes = DateUtil.timeSpanToMinutes(
        dto.accumulatedMinutes
      );
    }

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
