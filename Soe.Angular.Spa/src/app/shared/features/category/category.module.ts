import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { CategoryRoutingModule } from './category-routing.module';
import { CategoryEditComponent } from './components/category-edit/category-edit.component';
import { CategoryGridComponent } from './components/category-grid/category-grid.component';
import { CategoryComponent } from './components/category/category.component';

@NgModule({
  declarations: [
    CategoryComponent,
    CategoryEditComponent,
    CategoryGridComponent,
  ],
  imports: [
    CommonModule,
    CategoryRoutingModule,
    ExpansionPanelComponent,
    ButtonComponent,
    ReactiveFormsModule,
    EditFooterComponent,
    GridWrapperComponent,
    SelectComponent,
    SharedModule,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
  ],
})
export class CategoryModule {}
