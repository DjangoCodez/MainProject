import { Component, inject, OnInit, signal } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CategoryService } from '../../services/category.service';
import { CategoryForm } from '../../models/category-form.model';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { Observable, of, tap } from 'rxjs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ICategoryDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CategoryUrlParamsService } from '../../services/category-params.service';

@Component({
  selector: 'soe-category-edit',
  templateUrl: './category-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CategoryEditComponent
  extends EditBaseDirective<ICategoryDTO, CategoryService, CategoryForm>
  implements OnInit
{
  readonly service = inject(CategoryService);
  readonly urlService = inject(CategoryUrlParamsService);
  protected parentCategories: ISmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(this.urlService.feature(), {
      lookups: [this.loadParentCategories()],
    });
  }

  override newRecord(): Observable<void> {
    if (this.form?.isNew)
      this.form?.type.patchValue(this.urlService.categoryType());
    return of(void 0);
  }

  private loadParentCategories(): Observable<void> {
    if (!this.urlService.showSubCategory()) {
      return of(void 0);
    }

    let excludeCategory = this.form?.getIdControl()?.value;
    if (this.form?.isCopy) excludeCategory = 0;
    return this.performLoadData.load$(
      this.service
        .getCategoriesDict(
          this.urlService.categoryType(),
          true,
          excludeCategory
        )
        .pipe(
          tap(c => {
            this.parentCategories = c;
          })
        )
    );
  }
}
