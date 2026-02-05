import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { CoreStylesComponent } from './styles/core-styles/core-styles.component';
import { AgStylesComponent } from './styles/ag-styles/ag-styles.component';

@NgModule({
  imports: [
    CommonModule,
    TranslateModule.forChild({ extend: true, isolate: false }),
  ],
  declarations: [CoreStylesComponent, AgStylesComponent],
  exports: [TranslateModule, CoreStylesComponent, AgStylesComponent],
  providers: [],
})
export class SharedModule {}
