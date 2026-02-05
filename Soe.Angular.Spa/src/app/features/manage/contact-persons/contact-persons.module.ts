import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { CategoriesModule } from '@shared/components/categories/categories.module';
import { MultiSelectGridModule } from '@shared/components/multi-select-grid';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ContactPersonsEditComponent } from './components/contact-persons-edit/contact-persons-edit.component';
import { ContactPersonsGridComponent } from './components/contact-persons-grid/contact-persons-grid.component';
import { ContactPersonsComponent } from './components/contact-persons/contact-persons.component';
import { ContactPersonsRoutingModule } from './contact-persons-routing.module';

@NgModule({
  declarations: [
    ContactPersonsComponent,
    ContactPersonsGridComponent,
    ContactPersonsEditComponent,
  ],
  exports: [ContactPersonsEditComponent],
  imports: [
    CommonModule,
    SharedModule,
    ContactPersonsRoutingModule,
    ExpansionPanelComponent,
    ButtonComponent,
    CheckboxComponent,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    DatepickerComponent,
    MultiSelectGridModule,
    CategoriesModule,
  ],
})
export class ContactPersonsModule {}
