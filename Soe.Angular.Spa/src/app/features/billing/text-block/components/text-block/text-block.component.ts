import { Component } from '@angular/core';
import { TextBlockGridComponent } from '../text-block-grid/text-block-grid.component';
import { TextBlockEditComponent } from '../text-block-edit/text-block-edit.component';
import { TextBlockForm } from '../../models/text-block-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class TextBlockComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: TextBlockGridComponent,
      editComponent: TextBlockEditComponent,
      FormClass: TextBlockForm,
      gridTabLabel: 'billing.invoices.textblocks.textblocks',
      editTabLabel: 'billing.invoices.textblocks.textblock',
      createTabLabel: 'billing.invoices.textblocks.new_textblock',
    },
  ];
}
