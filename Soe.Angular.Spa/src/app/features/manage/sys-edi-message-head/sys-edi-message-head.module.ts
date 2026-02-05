import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component';
import { DeleteButtonComponent } from '@ui/button/delete-button/delete-button.component';
import { GridComponent } from '@ui/grid/grid.component';
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component';
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component';
import { TextboxComponent } from '@ui/forms/textbox/textbox.component';
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SysEdiMessageHeadEditComponent } from './components/sys-edi-message-head-edit/sys-edi-message-head-edit.component';
import { SysEdiMessageHeadGridComponent } from './components/sys-edi-message-head-grid/sys-edi-message-head-grid.component';
import { SysEdiMessageHeadComponent } from './components/sys-edi-message-head/sys-edi-message-head.component';
import { SysEdiMessageHeadRoutingModule } from './sys-edi-message-head-routing.module';
import { CheckboxComponent } from '@ui/forms/checkbox/checkbox.component';
import { SysEdiMessageHeadGridHeaderComponent } from './components/sys-edi-message-head-grid-header/sys-edi-message-head-grid-header.component';

@NgModule({
  declarations: [
    SysEdiMessageHeadComponent,
    SysEdiMessageHeadEditComponent,
    SysEdiMessageHeadGridComponent,
    SysEdiMessageHeadGridHeaderComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    SysEdiMessageHeadRoutingModule,
    GridComponent,
    GridWrapperComponent,
    SaveButtonComponent,
    DeleteButtonComponent,
    ReactiveFormsModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    AutocompleteComponent,
    CheckboxComponent,
  ],
})
export class SysEdiMessageHeadModule {}
