import { NgModule } from '@angular/core';
import { MarkupComponent } from './components/markup/markup.component';
import { MarkupGridComponent } from './components/markup-grid/markup-grid.component';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { MarkupRoutingModule } from './markup-routing.module';

@NgModule({
  declarations: [MarkupComponent, MarkupGridComponent],
  imports: [
    CommonModule,
    SharedModule,
    MarkupRoutingModule,
    ButtonComponent,
    EditFooterComponent,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    ReactiveFormsModule,
  ],
})
export class MarkupModule {}
