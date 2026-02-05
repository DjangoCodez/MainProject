import { CommonModule } from '@angular/common';
import {
  Component,
  computed,
  inject,
  input,
  OnInit,
  signal,
} from '@angular/core';
import { FormArray, ReactiveFormsModule } from '@angular/forms';
import { PlacementsService } from '@features/time/placements/services/placements.service';
import { PlanningEmployeeDTO } from '@features/time/schedule-planning/models/employee.model';
import { TranslateService } from '@ngx-translate/core';
import { IShiftDTO } from '@shared/models/generated-interfaces/TimeSchedulePlanningDTOs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CoreService } from '@shared/services/core.service';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component';
import { SelectComponent } from '@ui/forms/select/select.component';
import { IconModule } from '@ui/icon/icon.module';
import { LabelComponent } from '@ui/label/label.component';
import { tap } from 'rxjs';
import { AbsenceService } from '../../services/absence.service';
import { AbsenceShiftsForm } from './absence-shifts-form.model';

@Component({
  selector: 'soe-absence-shifts',
  imports: [
    ReactiveFormsModule,
    CommonModule,
    LabelComponent,
    SelectComponent,
    AutocompleteComponent,
    IconModule,
  ],
  templateUrl: './absence-shifts.component.html',
  styleUrl: './absence-shifts.component.scss',
})
export class AbsenceShiftsComponent implements OnInit {
  readonly service = inject(AbsenceService);
  private readonly coreService = inject(CoreService);
  private readonly placementService = inject(PlacementsService);
  private readonly translateService = inject(TranslateService);

  // private readonly validationHandler = inject(ValidationHandler);

  // Data
  // public approvalTypes: SmallGenericType[] = [];

  // Inputs
  // public employeeId = input.required<number>()
  // private readonly hiddenEmployeeId = signal(0);
  public shifts = input.required<IShiftDTO[]>();
  public shiftsFormArray = input.required<FormArray<AbsenceShiftsForm>>();
  public absenceStartDate = input.required<Date>();
  public absenceStopDate = input.required<Date>();
  public showApprovalTypeAndReplaceWithEmployee = input.required<boolean>();
  public employeeList = input<PlanningEmployeeDTO[]>([]);
  public readonly hiddenEmployeeId = input<number | undefined>(undefined);
  // public readonly setApprovedYesAsDefault = input<boolean>();
  public readonly onlyNoReplacementIsSelectable = input<boolean>(false);
  public readonly approvalTypes = input<SmallGenericType[]>([]);

  public readonly onDutyLabel = signal('');

  employeesPerShiftMap = computed(() => {
    const map = new Map<number, SmallGenericType[]>();
    // Map not needed if don't show approvalTypeAndReplaceWithEmployee per shift
    if (!this.showApprovalTypeAndReplaceWithEmployee()) return map;
    for (const shiftForm of this.shiftsFormArray().controls) {
      const shiftId = shiftForm.timeScheduleTemplateBlockId.value;
      const shift = this.shifts().find(
        s => s.timeScheduleTemplateBlockId === shiftId
      );

      if (shift) {
        map.set(shiftId, this.getEmployeeListForShift(shift));
      }
    }
    return map;
  });

  ngOnInit(): void {
    this.translateService
      .get(['time.schedule.planning.blocktype.onduty'])
      .pipe(
        tap(terms => {
          this.onDutyLabel.set(
            terms['time.schedule.planning.blocktype.onduty']
          );
        })
      )
      .subscribe();
  }

  //#region Events

  //#region HELPER
  public getEmployeeListForShift(shift?: IShiftDTO): SmallGenericType[] {
    console.log('getEmployeeListForShift', shift, this.employeeList());
    if (
      !(
        this.showApprovalTypeAndReplaceWithEmployee() &&
        this.employeeList().length > 0
      )
    )
      return [];

    if (this.onlyNoReplacementIsSelectable()) {
      // Only show NoReplacement
      return this.employeeList()
        .filter(emp => {
          return emp.employeeId === this.service.NO_REPLACEMENT_EMPLOYEEID; // NO_REPLACEMENT_EMPLOYEEID
        })
        .map(emp => {
          return new SmallGenericType(emp.employeeId, emp.name);
        });
    }

    if (!shift) return [];

    const isZeroLength = shift.startTime.getTime() === shift.stopTime.getTime();
    const hasNoReplacement = this.employeeList().some(
      e => e.employeeId === this.service.NO_REPLACEMENT_EMPLOYEEID
    );
    // const hasEmployment = this.employeeList().some(emp =>
    //   emp.hasEmployment(shift.startTime, shift.stopTime)
    // );
    if (isZeroLength && hasNoReplacement) {
      return this.employeeList()
        .filter(e => e.employeeId === this.service.NO_REPLACEMENT_EMPLOYEEID)
        .map(emp => {
          return new SmallGenericType(emp.employeeId, emp.name);
        });
    }
    return this.service.formatEmployeeListForDisplay(
      this.employeeList(),
      this.hiddenEmployeeId()
    );
    // return this.employeeList().map(emp => {
    //   // Normal case
    //   const isSpecialEmployee =
    //     emp.employeeId === this.service.NO_REPLACEMENT_EMPLOYEEID ||
    //     emp.employeeId === this.hiddenEmployeeId();
    //   return new SmallGenericType(
    //     emp.employeeId,
    //     isSpecialEmployee
    //       ? '{0}'.format(emp.name)
    //       : '({0}) {1}'.format(emp.employeeNr, emp.name)
    //   );
    // });
  }
}

//TODO:
