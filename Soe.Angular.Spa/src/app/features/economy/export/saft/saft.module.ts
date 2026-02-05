import { NgModule } from '@angular/core';
import { SaftGridComponent } from './components/saft-grid/saft-grid.component';
import { SaftComponent } from './components/saft/saft.component';
import { SaftRoutingModule } from './saft-routing.module';
import { ButtonComponent } from '@ui/button/button/button.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component';
import { SaftGridSearchComponent } from './components/saft-grid-search/saft-grid-search.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [SaftComponent, SaftGridComponent, SaftGridSearchComponent],
  imports: [
    GridWrapperComponent,
    MultiTabWrapperComponent,
    ButtonComponent,
    DatepickerComponent,
    ReactiveFormsModule,
    SaftRoutingModule,
  ],
})
export class SaftModule {}
