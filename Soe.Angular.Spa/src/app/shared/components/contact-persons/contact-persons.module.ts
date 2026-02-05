import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '@shared/shared.module';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ContactPersonsComponent } from './contact-persons/contact-persons.component';
import { ReactiveFormsModule } from '@angular/forms';
import { ContactPersonsModule as ContactPersonsFeatureModule } from '../../../features/manage/contact-persons/contact-persons.module';

@NgModule({
  declarations: [ContactPersonsComponent],
  exports: [ContactPersonsComponent],
  imports: [
    CommonModule,
    SharedModule,
    TranslateModule,
    DialogComponent,
    GridWrapperComponent,
    ButtonComponent,
    LabelComponent,
    ToolbarComponent,
    ReactiveFormsModule,
    ContactPersonsFeatureModule,
    AutocompleteComponent,
  ],
})
export class ContactPersonsModule {}
