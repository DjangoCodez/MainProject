import { Component } from '@angular/core';
import { CollectiveAgreementsForm } from '../../models/collective-agreements-form.model';
import { CollectiveAgreementsGridComponent } from '../collective-agreements-grid/collective-agreements-grid.component';
import { CollectiveAgreementsEditComponent } from '../collective-agreements-edit/collective-agreements-edit.component';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';

@Component({
  selector: 'soe-collective-agreements',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class CollectiveAgreementsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: CollectiveAgreementsGridComponent,
      editComponent: CollectiveAgreementsEditComponent,
      FormClass: CollectiveAgreementsForm,
      gridTabLabel:
        'time.employee.employeecollectiveagreement.employeecollectiveagreements',
      editTabLabel:
        'time.employee.employeecollectiveagreement.employeecollectiveagreement',
      createTabLabel: 'time.employee.employeecollectiveagreement.new',
    },
  ];
}
