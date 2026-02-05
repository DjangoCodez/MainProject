import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ContactPersonForm } from '@shared/components/contact-persons/models/contact-persons-form.model';
import { ContactPersonsEditComponent } from '../contact-persons-edit/contact-persons-edit.component';
import { ContactPersonsGridComponent } from '../contact-persons-grid/contact-persons-grid.component';

@Component({
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class ContactPersonsComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ContactPersonsGridComponent,
      editComponent: ContactPersonsEditComponent,
      FormClass: ContactPersonForm,
      gridTabLabel: 'manage.contactperson.contactpersons.contactpersons',
      editTabLabel: 'manage.contactperson.contactpersons.contactperson',
      createTabLabel: 'manage.contactperson.contactpersons.newcontactperson',
    },
  ];
}
