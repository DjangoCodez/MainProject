import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AutocompleteComponent } from '@ui/forms/autocomplete/autocomplete.component'
import { ButtonComponent } from '@ui/button/button/button.component'
import { EditFooterComponent } from '@ui/footer/edit-footer/edit-footer.component'
import { ExpansionPanelComponent } from '@ui/expansion-panel/expansion-panel.component'
import { GridWrapperComponent } from '@ui/grid/grid-wrapper/grid-wrapper.component'
import { MultiTabWrapperComponent } from '@ui/tab/multi-tab-wrapper/multi-tab-wrapper.component'
import { SelectComponent } from '@ui/forms/select/select.component'
import { TextboxComponent } from '@ui/forms/textbox/textbox.component'
import { ToolbarComponent } from '@ui/toolbar/toolbar.component';
import { SharedModule } from '@shared/shared.module';
import { MatchCodeRoutingModule } from './match-codes-routing.module';
import { MatchCodeComponent } from './components/match-codes/match-codes.component';
import { MatchCodeEditComponent } from './components/match-codes-edit/match-codes-edit.component';
import { MatchCodeGridComponent } from './components/match-codes-grid/match-codes-grid.component';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [
    MatchCodeComponent,
    MatchCodeEditComponent,
    MatchCodeGridComponent,
  ],
  imports: [
    SharedModule,
    MultiTabWrapperComponent,
    GridWrapperComponent,
    ToolbarComponent,
    ExpansionPanelComponent,
    TextboxComponent,
    SelectComponent,
    ReactiveFormsModule,
    AutocompleteComponent,
    EditFooterComponent,
    ButtonComponent,
    CommonModule,
    MatchCodeRoutingModule,
  ],
})
export class MatchCodeModule {}
