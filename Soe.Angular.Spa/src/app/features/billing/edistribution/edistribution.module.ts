import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EdistributionComponent } from './components/edistribution/edistribution.component';
import { EdistributionGridComponent } from './components/edistribution-grid/edistribution-grid.component';
import { SharedModule } from '@shared/shared.module';
import { EdistributionRoutingModule } from './edistribution-routing.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ReactiveFormsModule } from '@angular/forms';
import { EdestributionGridHeaderComponent } from './components/edestribution-grid-header/edestribution-grid-header.component';

@NgModule({
  declarations: [
    EdistributionComponent,
    EdistributionGridComponent,
    EdestributionGridHeaderComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    EdistributionRoutingModule,
    MultiTabWrapperComponent,
    ToolbarComponent,
    GridWrapperComponent,
    LabelComponent,
    TextboxComponent,
    ReactiveFormsModule,
    SelectComponent,
    ButtonComponent,
  ],
})
export class EdistributionModule {}
