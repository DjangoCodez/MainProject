import { Component } from '@angular/core';
import { UnionFeesGridComponent } from '../union-fees-grid/union-fees-grid.component';
import { UnionFeesEditComponent } from '../union-fees-edit/union-fees-edit.component';
import { UnionFeesForm } from '../../models/union-fees-form.model';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class UnionFeesComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: UnionFeesGridComponent,
      editComponent: UnionFeesEditComponent,
      FormClass: UnionFeesForm,
      gridTabLabel: 'time.payroll.unionfee.unionfees',
      editTabLabel: 'time.payroll.unionfee.unionfee',
      createTabLabel: 'time.payroll.unionfee.new',
    },
  ];
}
