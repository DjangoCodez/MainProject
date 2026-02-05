import { CdkMenu, CdkMenuItem } from '@angular/cdk/menu';
import { Component, inject, input, output } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { IconModule } from '@ui/icon/icon.module';
import { PlanningEmployeeDTO } from '../../models/employee.model';
import { SpEventService } from '../../services/sp-event.service';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { SpFilterService } from '../../services/sp-filter.service';
import { SpEmployeeService } from '../../services/sp-employee.service';

export enum EmployeeMenuOption {
  ShowAllEmployees,
  HideEmployeesWithoutShifts,
  ReloadEmployee,
  ReloadAllEmployees,
  Debug,
}

export type EmployeeMenuItemSelected = {
  employee?: PlanningEmployeeDTO;
  option: EmployeeMenuOption;
};

@Component({
  selector: 'sp-employee-menu',
  imports: [CdkMenu, CdkMenuItem, IconModule, TranslateModule],
  templateUrl: './sp-employee-menu.component.html',
  styleUrls: [
    '../../../../../shared/styles/shared-styles/shared-context-menu-styles.scss',
    './sp-employee-menu.component.scss',
  ],
})
export class SpEmployeeMenuComponent {
  employee = input<PlanningEmployeeDTO | undefined>(undefined);

  menuSelected = output<EmployeeMenuItemSelected>();

  private readonly employeeService = inject(SpEmployeeService);
  private readonly eventService = inject(SpEventService);
  readonly filterService = inject(SpFilterService);

  readonly SoeConfigUtil = SoeConfigUtil;
  readonly EmployeeMenuOption = EmployeeMenuOption;

  onMenuSelected(option: EmployeeMenuOption) {
    switch (option) {
      case EmployeeMenuOption.ShowAllEmployees:
        this.showAllEmployeesChanged(true);
        break;
      case EmployeeMenuOption.HideEmployeesWithoutShifts:
        this.showAllEmployeesChanged(false);
        break;
      case EmployeeMenuOption.ReloadEmployee:
        if (this.employee())
          this.eventService.reloadEmployee(this.employee()!.employeeId);
        break;
      case EmployeeMenuOption.ReloadAllEmployees:
        this.eventService.reloadAllEmployees();
        break;
      case EmployeeMenuOption.Debug:
        console.log('Employee:', this.employee());
        break;
      default:
        this.menuSelected.emit({ employee: this.employee(), option: option });
        break;
    }
  }

  private showAllEmployeesChanged(showAll: boolean) {
    this.filterService.showAllEmployees.set(showAll);
    this.employeeService.recalculateEmployeesAndShifts();
  }
}
