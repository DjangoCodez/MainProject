import { Component, inject, OnInit } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import {
  IEmploymentTypeDTO,
  IEmploymentTypeSmallDTO,
} from '@shared/models/generated-interfaces/EmploymentTypeDTO';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { EmploymentTypesService } from '../../services/employment-types.service';
import { EmploymentTypesForm } from '../../models/employment-types-form.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, tap } from 'rxjs';

@Component({
  selector: 'soe-employment-types-edit',
  templateUrl: './employment-types-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EmploymentTypesEditComponent
  extends EditBaseDirective<
    IEmploymentTypeDTO,
    EmploymentTypesService,
    EmploymentTypesForm
  >
  implements OnInit
{
  service = inject(EmploymentTypesService);

  standardEmploymentTypes: IEmploymentTypeSmallDTO[] = [];

  ngOnInit() {
    super.ngOnInit();

    this.recordConfig.hideRecordNavigator = true;

    this.startFlow(Feature.Time_Employee_EmploymentTypes, {
      lookups: [this.loadStandardEmploymentTypes()],
    });
  }

  loadStandardEmploymentTypes(): Observable<IEmploymentTypeSmallDTO[]> {
    return this.service.getStandardEmploymentTypes().pipe(
      tap(standardTypes => {
        const mustExistType: number = this.form?.value?.type;
        this.standardEmploymentTypes = standardTypes.filter(
          item => item.active || item.id === mustExistType
        );
      })
    );
  }

  override onFinished(): void {
    // If the employment type is a standard type, disable some fields
    if (
      this.form?.getIdControl()?.value > 0 &&
      this.form?.getIdControl()?.value < 10000
    ) {
      this.form?.controls.type.disable();
      this.form?.controls.name.disable();
      this.form?.controls.description.disable();
      this.form?.controls.isActive.disable();
    }
  }
}
