import { Component, inject, input } from '@angular/core';
import { SpEmployeeRowComponent } from '../sp-employee-row/sp-employee-row.component';
import { SpEmployeeService } from '../../services/sp-employee.service';
import { CdkDropListGroup } from '@angular/cdk/drag-drop';

@Component({
  selector: 'sp-content',
  imports: [CdkDropListGroup, SpEmployeeRowComponent],
  templateUrl: './sp-content.component.html',
  styleUrl: './sp-content.component.scss',
})
export class SpContentComponent {
  hasScrollbar = input(false);

  readonly employeeService = inject(SpEmployeeService);
}
