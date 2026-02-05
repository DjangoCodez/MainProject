import { DatePipe, DecimalPipe, LowerCasePipe } from '@angular/common';
import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { PlanningEmployeeDTO } from '@features/time/schedule-planning/models/employee.model';
import { SpEmployeeService } from '@features/time/schedule-planning/services/sp-employee.service';
import { TranslatePipe } from '@ngx-translate/core';
import { MinutesToTimeSpanPipe } from '@shared/pipes';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { Subscription } from 'rxjs';

@Component({
  selector: 'sp-employee-detail',
  imports: [
    DatePipe,
    DecimalPipe,
    LowerCasePipe,
    MinutesToTimeSpanPipe,
    TranslatePipe,
  ],
  templateUrl: './sp-employee-detail.component.html',
  styleUrl: './sp-employee-detail.component.scss',
})
export class SpEmployeeDetailComponent implements OnInit, OnDestroy {
  employeeService = inject(SpEmployeeService);

  employee = signal<PlanningEmployeeDTO | undefined>(undefined);

  isSupportAdmin = SoeConfigUtil.isSupportAdmin;

  private selectedEmployeeChangedSubscription?: Subscription;

  ngOnInit(): void {
    this.selectedEmployeeChangedSubscription =
      this.employeeService.selectedEmployeeChanged.subscribe(
        (employee: PlanningEmployeeDTO | undefined) => {
          this.onSelectedEmployeeChanged(employee);
        }
      );
  }

  ngOnDestroy(): void {
    this.selectedEmployeeChangedSubscription?.unsubscribe();
  }

  onSelectedEmployeeChanged(employee: PlanningEmployeeDTO | undefined) {
    this.employee.set(employee);
  }
}
