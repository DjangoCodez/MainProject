import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EdiComponent } from './components/edi/edi.component';
import { SharedModule } from '@shared/shared.module';
import { EdiRoutingModule } from './edi-routing.module';
import { EdiGridComponent } from './components/edi-grid/edi-grid.component';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MenuButtonComponent } from '@ui/button/menu-button/menu-button.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { EdiGridHeaderComponent } from './components/edi-grid-header/edi-grid-header.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [EdiComponent, EdiGridComponent, EdiGridHeaderComponent],
  imports: [
    CommonModule,
    SharedModule,
    EdiRoutingModule,
    ToolbarComponent,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    EditFooterComponent,
    SelectComponent,
    ReactiveFormsModule,
    ButtonComponent,
    MenuButtonComponent,
    SaveButtonComponent,
  ],
})
export class EdiModule {}
