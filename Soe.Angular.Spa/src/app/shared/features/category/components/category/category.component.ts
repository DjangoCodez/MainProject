import { Component } from '@angular/core';
import { MultiTabConfig } from '@ui/tab/models/multi-tab-wrapper.model';
import { CategoryEditComponent } from '../category-edit/category-edit.component';
import { CategoryGridComponent } from '../category-grid/category-grid.component';
import { CategoryForm } from '../../models/category-form.model';
import { CategoryUrlParamsService } from '../../services/category-params.service';

@Component({
  selector: 'soe-category',
  templateUrl:
    '../../../../../shared/ui-components/tab/multi-tab-wrapper/multi-tab-wrapper-template.html',
  standalone: false,
  providers: [CategoryUrlParamsService],
})
export class CategoryComponent {
  config: MultiTabConfig[] = [
    {
      gridComponent: CategoryGridComponent,
      editComponent: CategoryEditComponent,
      FormClass: CategoryForm,
      gridTabLabel: 'common.categories.categories',
      editTabLabel: 'common.categories.category',
      createTabLabel: 'common.categories.category_new',
      exportFilenameKey: 'common.categories.categories',
    },
  ];
}
