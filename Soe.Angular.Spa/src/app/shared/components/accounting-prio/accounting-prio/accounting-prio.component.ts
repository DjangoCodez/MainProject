
import {
  Component,
  effect,
  inject,
  input,
  OnInit,
  output,
} from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SoeFormGroup } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  TermGroup,
  TermGroup_EmployeeGroupInvoiceProductAccountingPrio,
  TermGroup_EmployeeGroupPayrollProductAccountingPrio,
  TermGroup_TimeProjectInvoiceProductAccountingPrio,
  TermGroup_TimeProjectPayrollProductAccountingPrio,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { LabelComponent } from '@ui/label/label.component'
import { SelectComponent } from '@ui/forms/select/select.component';
import { Observable, tap } from 'rxjs';
import {
  AccountingPrioForm,
  IAccountingPrio,
} from '../models/accounting-prio-form.model';

@Component({
  selector: 'soe-accounting-prio',
  imports: [SelectComponent, ReactiveFormsModule, LabelComponent],
  templateUrl: './accounting-prio.component.html',
  styleUrl: './accounting-prio.component.scss',
})
export class AccountingPrioComponent implements OnInit {
  accountingPrioForm!: AccountingPrioForm;

  form = input.required<SoeFormGroup>();
  accountingPrios = input.required<string>(); // Input and outputs accountingPrios in stringformat '0,0,0,0,0'
  accountingPrioChange = output<string>();
  prioItemsTermGroup = input.required<number>(); // TermGroup cointaining items
  showNumbering = input(true);
  accountingLabelKey = input('');

  accountingPrioItems: SmallGenericType[] = [];

  coreService = inject(CoreService);
  validationHandler = inject(ValidationHandler);

  constructor() {
    effect(() => {
      if (this.accountingPrioForm) {
        const prioValues = this.parseAccountingPrio(this.accountingPrios());
        this.accountingPrioForm.patchValue(prioValues, { emitEvent: false });
      }
    });
  }

  ngOnInit() {
    const prioValues = this.parseAccountingPrio(this.accountingPrios());

    this.accountingPrioForm = new AccountingPrioForm({
      validationHandler: this.validationHandler,
      element: prioValues,
    });

    this.accountingPrioForm.valueChanges.subscribe(() => {
      const prioString = this.formatAccountingPrio(
        this.accountingPrioForm.value
      );
      this.accountingPrioChange.emit(prioString);
      this.form()?.markAsDirty();
      this.form()?.markAsTouched();
    });

    this.getOrderedAccountingPrios(this.prioItemsTermGroup()).subscribe();
  }

  private parseAccountingPrio(accountingPrioString: string): IAccountingPrio {
    const accountingPrios: number[] = accountingPrioString
      ? accountingPrioString.split(',').map(v => Number(v))
      : [0, 0, 0, 0, 0];
    return {
      accountingPrio1: accountingPrios[0] || 0,
      accountingPrio2: accountingPrios[1] || 0,
      accountingPrio3: accountingPrios[2] || 0,
      accountingPrio4: accountingPrios[3] || 0,
      accountingPrio5: accountingPrios[4] || 0,
    };
  }

  private formatAccountingPrio(accountingPrios: IAccountingPrio): string {
    return [
      accountingPrios.accountingPrio1,
      accountingPrios.accountingPrio2,
      accountingPrios.accountingPrio3,
      accountingPrios.accountingPrio4,
      accountingPrios.accountingPrio5,
    ].join(',');
  }

  private loadOrderedTermGroupItems<T>(
    termGroup: TermGroup,
    orderedPrioValues: number[]
  ): Observable<any[]> {
    return this.coreService.getTermGroupContent(termGroup, false, false).pipe(
      tap(items => {
        // Add items in the order specified by orderedPrioValues
        orderedPrioValues.forEach(enumValue => {
          const item = items.find(y => y.id === enumValue);
          if (item) this.accountingPrioItems.push(item);
        });
      })
    );
  }
  private getOrderedAccountingPrios(termGroup: TermGroup) {
    // Add ordered values for more types if necessary
    let orderedPrioValues: number[] = [];

    // These are the types in the desired order.
    if (termGroup === TermGroup.EmployeeGroupPayrollProductAccountingPrio) {
      orderedPrioValues = [
        TermGroup_EmployeeGroupPayrollProductAccountingPrio.NotUsed,
        TermGroup_EmployeeGroupPayrollProductAccountingPrio.PayrollProduct,
        TermGroup_EmployeeGroupPayrollProductAccountingPrio.EmploymentAccount,
        TermGroup_EmployeeGroupPayrollProductAccountingPrio.Project,
        TermGroup_EmployeeGroupPayrollProductAccountingPrio.Customer,
        TermGroup_EmployeeGroupPayrollProductAccountingPrio.EmployeeGroup,
      ];
    } else if (
      termGroup === TermGroup.EmployeeGroupInvoiceProductAccountingPrio
    ) {
      orderedPrioValues = [
        TermGroup_EmployeeGroupInvoiceProductAccountingPrio.NotUsed,
        TermGroup_EmployeeGroupInvoiceProductAccountingPrio.InvoiceProduct,
        TermGroup_EmployeeGroupInvoiceProductAccountingPrio.EmploymentAccount,
        TermGroup_EmployeeGroupInvoiceProductAccountingPrio.Project,
        TermGroup_EmployeeGroupInvoiceProductAccountingPrio.Customer,
      ];
    } else if (
      termGroup === TermGroup.TimeProjectPayrollProductAccountingPrio
    ) {
      orderedPrioValues = [
        TermGroup_TimeProjectPayrollProductAccountingPrio.NotUsed,
        TermGroup_TimeProjectPayrollProductAccountingPrio.EmployeeAccount,
        TermGroup_TimeProjectPayrollProductAccountingPrio.Project,
        TermGroup_TimeProjectPayrollProductAccountingPrio.Customer,
        TermGroup_TimeProjectPayrollProductAccountingPrio.PayrollProduct,
        TermGroup_TimeProjectPayrollProductAccountingPrio.EmployeeGroup,
      ];
    } else if (
      termGroup === TermGroup.TimeProjectInvoiceProductAccountingPrio
    ) {
      orderedPrioValues = [
        TermGroup_TimeProjectInvoiceProductAccountingPrio.NotUsed,
        TermGroup_TimeProjectInvoiceProductAccountingPrio.EmployeeAccount,
        TermGroup_TimeProjectInvoiceProductAccountingPrio.Project,
        TermGroup_TimeProjectInvoiceProductAccountingPrio.Customer,
        TermGroup_TimeProjectInvoiceProductAccountingPrio.InvoiceProduct,
      ];
    } else {
      orderedPrioValues = [];
    }

    return this.loadOrderedTermGroupItems(termGroup, orderedPrioValues);
  }
}
