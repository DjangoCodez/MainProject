import { Component, OnInit, inject, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeCategoryType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ICategoryGridDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import { CategoryService } from '../../services/category.service';
import { CategoryUrlParamsService } from '../../services/category-params.service';
import { ToolbarSelectAction } from '@ui/toolbar/toolbar-select/toolbar-select.component';
import { SmallGenericType } from '@shared/models/generic-type.model';

@Component({
  selector: 'soe-category-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CategoryGridComponent
  extends GridBaseDirective<ICategoryGridDTO, CategoryService>
  implements OnInit
{
  service = inject(CategoryService);
  urlService = inject(CategoryUrlParamsService);
  categoryTypes: SmallGenericType[] = [];
  selectedCategoryType = SoeCategoryType.Unknown;

  ngOnInit(): void {
    super.ngOnInit();
    this.selectedCategoryType = this.urlService.categoryType();
    console.log('this.selectedCategoryType', this.selectedCategoryType);
    this.startFlow(Feature.Common_Categories, 'common.categories.categories', {
      lookups: [this.loadCateogries()],
    });
  }

  override createGridToolbar(): void {
    super.createGridToolbar();

    if (this.selectedCategoryType === SoeCategoryType.Unknown) {
      this.toolbarService.createItemGroup({
        items: [
          this.toolbarService.createToolbarSelect('categoryType', {
            labelKey: signal('common.type'),
            items: signal(this.categoryTypes),
            initialSelectedId: signal(this.selectedCategoryType),
            onValueChanged: event => {
              this.selectedCategoryType = (event as ToolbarSelectAction).value;
              this.urlService.categoryType.set(this.selectedCategoryType);
              this.refreshGrid();
            },
          }),
        ],
        alignLeft: true,
      });
    }
  }

  override onGridReadyToDefine(grid: GridComponent<ICategoryGridDTO>): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.code',
        'common.name',
        'common.categories.childrengroupname',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 33,
          enableHiding: false,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 33,
          enableHiding: false,
        });
        if (this.urlService.showSubCategory()) {
          this.grid.addColumnText(
            'childrenNamesString',
            terms['common.categories.childrengroupname'],
            {
              flex: 33,
              enableHiding: true,
            }
          );
        }
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: { categoryType: SoeCategoryType }
  ): Observable<ICategoryGridDTO[]> {
    return super.loadData(id, { categoryType: this.selectedCategoryType });
  }

  loadCateogries() {
    return this.service.getCategoryTypesByPermission().pipe(
      tap(x => {
        this.categoryTypes = x;
      })
    );
  }
}
