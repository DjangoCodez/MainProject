import { NgModule } from '@angular/core';
import { ActorInvoiceMatchesFilterComponent } from './components/actor-invoice-matches-filter/actor-invoice-matches-filter.component';
import { ReactiveFormsModule } from '@angular/forms';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { DatepickerComponent } from '@ui/forms/datepicker/datepicker.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { NumberboxComponent } from '@ui/forms/numberbox/numberbox.component'
import { SaveButtonComponent } from '@ui/button/save-button/save-button.component'
import { SelectComponent } from '@ui/forms/select/select.component';
import { ActorInvoiceMatchesGridComponent } from './components/actor-invoice-matches-grid/actor-invoice-matches-grid.component';
import { ActorInvoiceMatchesService } from './services/actor-invoice-matches.service';

@NgModule({
  declarations: [
    ActorInvoiceMatchesFilterComponent,
    ActorInvoiceMatchesGridComponent,
  ],
  imports: [
    ReactiveFormsModule,
    ExpansionPanelComponent,
    AutocompleteComponent,
    SelectComponent,
    NumberboxComponent,
    DatepickerComponent,
    ButtonComponent,
    SaveButtonComponent,
    GridWrapperComponent,
  ],
  exports: [ActorInvoiceMatchesGridComponent],
  providers: [ActorInvoiceMatchesService],
})
export class ActorInvoiceMatchesModule {}
