import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ButtonComponent } from '@ui/button/button/button.component';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData, DialogSize } from '@ui/dialog/models/dialog';
import { MultiSelectComponent } from '@ui/forms/select/multi-select/multi-select.component';
import { SpFilterService } from '../../services/sp-filter.service';
import { SchedulePlanningFilter } from '../../models/filter.model';
import { ValidationHandler } from '@shared/handlers';
import { SpFilterDialogForm } from './sp-filter-dialog-form.model';
import { ReactiveFormsModule } from '@angular/forms';
import { UserSelectionComponent } from '@shared/components/user-selection/components/user-selection/user-selection.component';
import { UserSelectionType } from '@shared/models/generated-interfaces/Enumerations';
import { SpSettingService } from '../../services/sp-setting.service';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { IUserSelectionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';

export class SpFilterDialogData implements DialogData {
  title!: string;
  size?: DialogSize;
  disableContentScroll?: boolean;
}

export class SpFilterDialogResult {
  filterChanged: boolean = false;
}

@Component({
  selector: 'sp-filter-dialog',
  imports: [
    ReactiveFormsModule,
    ButtonComponent,
    CheckboxComponent,
    DialogComponent,
    MultiSelectComponent,
    UserSelectionComponent,
  ],
  templateUrl: './sp-filter-dialog.component.html',
  styleUrl: './sp-filter-dialog.component.scss',
})
export class SpFilterDialogComponent
  extends DialogComponent<SpFilterDialogData>
  implements OnInit
{
  readonly filterService = inject(SpFilterService);
  readonly settingService = inject(SpSettingService);

  validationHandler = inject(ValidationHandler);
  form: SpFilterDialogForm = new SpFilterDialogForm({
    validationHandler: this.validationHandler,
    element: new SchedulePlanningFilter(),
  });

  userSelectionType = computed<UserSelectionType>(() => {
    // Each view definition has its own user selection type offset by 100
    const candidate = this.filterService.viewDefinition() + 100;
    return candidate in UserSelectionType
      ? (candidate as UserSelectionType)
      : UserSelectionType.TimeSchedulePlanningView_Schedule;
  });
  selectedUserSelectionId = signal<number>(0);
  selectedUserSelection = signal<IUserSelectionDTO | undefined>(undefined);
  private setInitialDefaultSelection = false;

  ngOnInit(): void {
    this.loadFilter();
  }

  private loadFilter() {
    // Get values from service and set them to the form
    this.form.patchValue({
      employeeIds: this.filterService.employeeIds(),
      showAllEmployees: this.filterService.showAllEmployees(),
      shiftTypeIds: this.filterService.shiftTypeIds(),
      showHiddenShifts: this.filterService.showHiddenShifts(),
      blockTypes: this.filterService.blockTypes(),
    });
  }

  filterByUserSelection() {
    const selection = this.selectedUserSelection();
    console.log('Filtering by user selection', selection);
    if (selection) {
    } else {
      // Clear filter
      this.clear();
    }
  }

  // EVENTS

  onUserSelectionsLoaded(selections: SmallGenericType[]) {
    // If a default user selection has been saved, select it
    if (
      this.settingService.defaultUserSelectionId() > 0 &&
      selections.find(
        s => s.id === this.settingService.defaultUserSelectionId()
      )
    ) {
      this.setInitialDefaultSelection = true;
      this.selectedUserSelectionId.set(
        this.settingService.defaultUserSelectionId()
      );
    }
  }

  onUserSelectionSelected(userSelection: IUserSelectionDTO | undefined) {
    const newId = userSelection?.userSelectionId ?? 0;

    // Prevent reapplying the same selection
    if (
      this.selectedUserSelectionId() !== newId ||
      this.setInitialDefaultSelection
    ) {
      this.setInitialDefaultSelection = false;
      this.selectedUserSelectionId.set(newId);
      this.selectedUserSelection.set(userSelection);
      this.filterByUserSelection();
    }
  }

  onEmployeesChanged(employeeIds: number[]) {
    this.form.patchValue({ employeeIds: employeeIds });
  }

  onShiftTypesChanged(shiftTypeIds: number[]) {
    this.form.patchValue({ shiftTypeIds: shiftTypeIds });
  }

  onBlockTypesChanged(blockTypes: number[]) {
    this.form.patchValue({ blockTypes: blockTypes });
  }

  clear() {
    this.form.patchValue({
      employeeIds: [],
      shiftTypeIds: [],
      blockTypes: [],
      showAllEmployees: false,
      showHiddenShifts: false,
    });

    this.selectedUserSelectionId.set(0);
  }

  cancel() {
    this.dialogRef.close({ filterChanged: false } as SpFilterDialogResult);
  }

  ok() {
    // Save form values to service
    this.filterService.employeeIds.set(this.form.value.employeeIds);
    this.filterService.showAllEmployees.set(this.form.value.showAllEmployees);
    this.filterService.shiftTypeIds.set(this.form.value.shiftTypeIds);
    this.filterService.showHiddenShifts.set(this.form.value.showHiddenShifts);
    this.filterService.blockTypes.set(this.form.value.blockTypes);

    this.dialogRef.close({ filterChanged: true } as SpFilterDialogResult);
  }
}
