
import { Component, input, output } from '@angular/core';
import { SoeFormGroup } from '@shared/extensions';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { CreatedModifiedComponent } from '@ui/created-modified/created-modified.component'
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component'
import { MenuButtonComponent, MenuButtonItem } from '@ui/button/menu-button/menu-button.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'soe-edit-footer',
  imports: [
    ReactiveFormsModule,
    ButtonComponent,
    DeleteButtonComponent,
    MenuButtonComponent,
    SaveButtonComponent,
    CheckboxComponent,
    CreatedModifiedComponent
],
  templateUrl: './edit-footer.component.html',
  styleUrls: ['./edit-footer.component.scss'],
})
export class EditFooterComponent {
  form = input.required<SoeFormGroup>();
  modifyPermission = input(false);
  idFieldName = input('');
  showActive = input(false);
  showCancel = input(false);
  hideDelete = input(false);
  hideSave = input(false);
  saveMenuList = input<MenuButtonItem[]>([]);
  saveMenuDefaultItem = input<MenuButtonItem>();
  saveMenuDropUp = input(false);
  saveMenuDropLeft = input(false);
  inProgress = input(false);

  activeChanged = output<boolean>();
  cancelled = output<void>();
  deleted = output<void>();
  saved = output<void>();
  saveMenuListItemSelected = output<MenuButtonItem>();

  // No signal because of the form properties
  get saveIsDisabled() {
    return (
      !this.modifyPermission() ||
      !this.form().dirty ||
      this.form().invalid ||
      this.inProgress()
    );
  }

  onActiveChanged(value: boolean) {
    this.activeChanged.emit(value);
  }

  onCancelled() {
    this.cancelled.emit();
  }

  onDeleted() {
    this.deleted.emit();
  }

  onSaved() {
    this.saved.emit();
  }

  onSaveMenuListItemSelected(item: MenuButtonItem): void {
    if (!item) {
      if (this.saveMenuDefaultItem()) {
        item = this.saveMenuDefaultItem()!;
      } else {
        if (this.saveMenuList() && this.saveMenuList().length > 0)
          item = this.saveMenuList()[0];
      }
    }
    this.saveMenuListItemSelected.emit(item);
  }
}
