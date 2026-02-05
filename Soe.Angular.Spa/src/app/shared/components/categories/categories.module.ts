import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '@shared/shared.module';
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { LabelComponent } from '@ui/label/label.component';
import { CategoriesComponent } from './categories/categories.component';

@NgModule({
  declarations: [CategoriesComponent],
  exports: [CategoriesComponent],
  imports: [
    CommonModule,
    SharedModule,
    TranslateModule,
    GridWrapperComponent,
    LabelComponent,
    ExpansionPanelComponent,
  ],
})
export class CategoriesModule {}
