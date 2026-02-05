import {
  Component,
  EventEmitter,
  OnInit,
  Output,
  inject,
  signal,
} from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { SoeOriginType } from '@shared/models/generated-interfaces/Enumerations';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { tap } from 'rxjs';
import { AnnualLeaveBalanceFilterForm } from '@features/time/annual-leave-balance/models/annual-leave-balance-filter-form.model';
import { SearchAnnualLeaveTransactionModel } from '@features/time/annual-leave-balance/models/annual-leave-balance.model';
import { AnnualLeaveBalanceService } from '@features/time/annual-leave-balance/services/annual-leave-balance.service';
import { EmployeeService } from '@features/time/services/employee.service';
import { rxResource } from '@angular/core/rxjs-interop';

@Component({
  selector: 'soe-annual-leave-balance-grid-filter',
  templateUrl: './annual-leave-balance-grid-filter.component.html',
  standalone: false,
})
export class AnnualLeaveBalanceGridFilterComponent implements OnInit {
  @Output() searchClick = new EventEmitter<SearchAnnualLeaveTransactionModel>();
  @Output() filterChange = new EventEmitter<SoeOriginType>();

  validationHandler = inject(ValidationHandler);
  employeeService = inject(EmployeeService);
  service = inject(AnnualLeaveBalanceService);
  performEmployees = new Perform<SmallGenericType[]>(this.progressService);
  employees: SmallGenericType[] = [];
  loadingEmployees = signal(false);
  selectedDateFrom = signal(new Date());
  selectedDateTo = signal(new Date());

  employeesResource = rxResource({
    params: () => ({
      dateFrom: this.selectedDateFrom(),
      dateTo: this.selectedDateTo(),
      employeeIds: [],
      showInactive: false,
      showEnded: true,
      showNotStarted: true,
      filterOnAnnualLeaveAgreement: true,
    }),
    stream: ({ params }) => {
      this.loadingEmployees.set(true);
      this.formFilter.employeeIds.disable();
      return this.employeeService.getEmployeesForGridDict(params).pipe(
        tap(value => {
          this.employees = value;
          this.loadingEmployees.set(false);
          this.formFilter.employeeIds.enable();
          this.verifySelectedEmployees();
        })
      );
    },
  });

  formFilter: AnnualLeaveBalanceFilterForm = new AnnualLeaveBalanceFilterForm({
    validationHandler: this.validationHandler,
    element: new SearchAnnualLeaveTransactionModel(),
  });

  constructor(private progressService: ProgressService) {}

  ngOnInit() {
    this.formFilter.dateFrom.valueChanges.subscribe(value => {
      if (value) {
        this.selectedDateFrom.set(value);
      }
    });
    this.formFilter.dateTo.valueChanges.subscribe(value => {
      if (value) {
        this.selectedDateTo.set(value);
      }
    });
  }

  loadEmployees() {
    this.performEmployees.load(
      this.employeeService
        .getEmployeesForGridDict({
          dateFrom: new Date(),
          dateTo: new Date(),
          employeeIds: [],
          showInactive: false,
          showEnded: true,
          showNotStarted: true,
          filterOnAnnualLeaveAgreement: false,
        })
        .pipe(
          tap(value => {
            this.employees = value;
          })
        )
    );
  }

  search(): void {
    const searchDto = this.formFilter
      .value as SearchAnnualLeaveTransactionModel;
    this.searchClick.emit({
      employeeIds: searchDto.employeeIds,
      dateFrom: searchDto.dateFrom,
      dateTo: new Date(
        searchDto.dateTo.getFullYear(),
        searchDto.dateTo.getMonth(),
        searchDto.dateTo.getDate() + 1
      ),
    });
  }

  verifySelectedEmployees() {
    // cross check this.formFilter.employeeIds with this.employees and remove selected that does not exist
    const validEmployeeIds = this.employees.map(emp => emp.id);
    this.formFilter.employeeIds.setValue(
      this.formFilter.employeeIds.value.filter((id: any) =>
        validEmployeeIds.includes(id)
      )
    );
  }
}
