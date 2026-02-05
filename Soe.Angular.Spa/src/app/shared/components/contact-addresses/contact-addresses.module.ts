import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { IconModule } from '@ui/icon/icon.module'
import { LabelComponent } from '@ui/label/label.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ContactAddressDialogComponent } from './contact-address-dialog/contact-address-dialog.component';
import { ContactAddressesComponent } from './contact-addresses/contact-addresses.component';

@NgModule({
  declarations: [ContactAddressesComponent, ContactAddressDialogComponent],
  exports: [ContactAddressesComponent],
  imports: [
    CommonModule,
    SharedModule,
    TranslateModule,
    DialogComponent,
    GridWrapperComponent,
    ButtonComponent,
    MenuButtonComponent,
    SaveButtonComponent,
    LabelComponent,
    IconModule,
    ToolbarComponent,
    TextboxComponent,
    ReactiveFormsModule,
  ],
})
export class ContactAddressesModule {}
