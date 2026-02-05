import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { MatchCodesForm } from '../../models/match-codes-form.model';
import { MatchCodeGridComponent } from '../match-codes-grid/match-codes-grid.component';
import { MatchCodeEditComponent } from '../match-codes-edit/match-codes-edit.component';

@Component({
  selector: 'soe-match-settings',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class MatchCodeComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: MatchCodeGridComponent,
      editComponent: MatchCodeEditComponent,
      FormClass: MatchCodesForm,
      gridTabLabel: 'economy.accounting.matchcode.matchcodes',
      editTabLabel: 'economy.accounting.matchcode.matchcode',
      createTabLabel: 'economy.accounting.matchcode.newmatchcode',
    },
  ];
}
