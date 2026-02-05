import {
  AfterViewChecked,
  ChangeDetectorRef,
  Component,
  inject,
  OnInit,
} from '@angular/core';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { ValidationHandler } from '@shared/handlers';
import { ProjectPersonsDialogData } from '@features/billing/project/models/project-persons.model';
import { ProjectPersonForm } from '@features/billing/project/models/project-person-form.model';

@Component({
  selector: 'soe-project-person-dialog',
  templateUrl: './project-person-dialog.component.html',
  standalone: false,
})
export class ProjectPersonDialogComponent
  extends DialogComponent<ProjectPersonsDialogData>
  implements OnInit, AfterViewChecked
{
  validationHandler = inject(ValidationHandler);

  form: ProjectPersonForm;
  isEmployeeCostDisabled: boolean = true;

  constructor(private cdRef: ChangeDetectorRef) {
    super();
    this.form = new ProjectPersonForm({
      validationHandler: this.validationHandler,
      element: this.data.rowToUpdate,
    });
  }
  ngOnInit(): void {
    this.form.dateFrom?.valueChanges.subscribe(date => {
      if (date) {
        this.form.employeeCalculatedCost?.enable();
      } else {
        this.form.employeeCalculatedCost?.disable();
      }
    });
  }

  ngAfterViewChecked(): void {
    this.cdRef.detectChanges();
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  protected ok(): void {
    this.dialogRef.close(this.form.getAllValues());
  }
}
