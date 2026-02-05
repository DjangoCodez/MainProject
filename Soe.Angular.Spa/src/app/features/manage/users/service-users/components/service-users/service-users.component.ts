import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { ServiceUsersGridComponent } from '../service-users-grid/service-users-grid.component';
import { ServiceUsersEditComponent } from '../service-users-edit/service-users-edit.component';
import { AddServiceUserForm } from '../../models/add-service-user.form';

@Component({
  templateUrl:
    '../../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
})
export class ServiceUsersComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: ServiceUsersGridComponent,
      editComponent: ServiceUsersEditComponent,
      FormClass: AddServiceUserForm,
      gridTabLabel: 'manage.serviceusers',
      editTabLabel: 'manage.serviceuser',
      createTabLabel: 'manage.serviceuser.new',
    },
  ];
}
