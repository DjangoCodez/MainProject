import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
} from '@angular/core';
import {
  SoeCheckboxFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { TermGroup_ProjectStatus } from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';

@Component({
  selector: 'soe-project-grid-header',
  templateUrl: './project-grid-header.component.html',
  standalone: false,
})
export class ProjectGridHeaderComponent implements OnChanges {
  @Input() projectStatuses: ISmallGenericType[] = [];
  @Input() initialSelectedStatusIds: number[] = [];
  @Output() projectStatusesChanged = new EventEmitter<number[]>();
  @Output() showMineChanged = new EventEmitter<boolean>();

  validationHandler = inject(ValidationHandler);

  form = new SoeFormGroup(this.validationHandler, {
    selectedProjectStatusIds: new SoeSelectFormControl([]),
    loadMine: new SoeCheckboxFormControl(false),
  });

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialSelectedStatusIds']) {
      this.form.controls.selectedProjectStatusIds.setValue(
        this.initialSelectedStatusIds
      );
    }
  }

  projectStatusSelectionComplete() {
    if (this.form.controls.selectedProjectStatusIds.value.length === 0) {
      this.form.controls.selectedProjectStatusIds.setValue(
        TermGroup_ProjectStatus.Unknown
      );
    }
    this.projectStatusesChanged.emit(
      this.form.controls.selectedProjectStatusIds.value
    );
  }
}
