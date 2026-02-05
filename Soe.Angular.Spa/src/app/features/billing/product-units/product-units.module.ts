import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '@shared/shared.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { ProductUnitsEditComponent } from './components/product-units-edit/product-units-edit.component';
import { ProductUnitsGridComponent } from './components/product-units-grid/product-units-grid.component';
import { ProductUnitsComponent } from './components/product-units/product-units.component';
import { ProductUnitsRoutingModule } from './product-units-routing.module';
import { ReactiveFormsModule } from '@angular/forms';
import { LanguageTranslationsModule } from '@shared/features/language-translations/language-translations.module';

@NgModule({
  declarations: [
    ProductUnitsComponent,
    ProductUnitsEditComponent,
    ProductUnitsGridComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ProductUnitsRoutingModule,
    ExpansionPanelComponent,
    ButtonComponent,
    EditFooterComponent,
    ReactiveFormsModule,
    GridWrapperComponent,
    MultiTabWrapperComponent,
    TextboxComponent,
    ToolbarComponent,
    LanguageTranslationsModule,
  ],
})
export class ProductUnitsModule {}
