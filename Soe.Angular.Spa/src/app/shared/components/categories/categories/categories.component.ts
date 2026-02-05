import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import {
  Feature,
  SoeCategoryType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  ICategoryDTO,
  ICompanyCategoryRecordDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { CellValueChangedEvent } from 'ag-grid-community';
import { take, tap } from 'rxjs';
import { CategoryItem } from '../categories.model';

@Component({
  selector: 'soe-categories',
  templateUrl: './categories.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CategoriesComponent
  extends GridBaseDirective<CategoryItem>
  implements OnChanges, OnInit
{
  @Input({ required: true }) categoryType!: SoeCategoryType;
  @Input({ required: true }) readOnly!: boolean;
  @Input({ required: true }) categoryIds!: number[];
  @Input() form: SoeFormGroup | undefined;
  @Input() showDefault = false;
  @Input() showDateFrom = false;
  @Input() showDateTo = false;
  @Input() showIsExecutive = false;
  @Input() useFilters = false;
  @Input() nbrOfRows = 8;
  @Input() useCompCategories = false;
  @Input() compCategories: ICompanyCategoryRecordDTO[] | undefined;

  @Output() categoriesChange = new EventEmitter<CategoryItem[]>();

  readonly coreService = inject(CoreService);
  readonly translate = inject(TranslateService);

  allCategories: ICategoryDTO[] = [];

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.None, 'Common.Directives.Categories', {
      lookups: [this.loadCategories()],
      skipInitialLoad: true,
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.categoryIds) {
      this.updateGridRows();
    }
  }

  loadCategories() {
    return this.coreService
      .getCategoriesGrid(this.categoryType, false, false, false)
      .pipe(
        tap(res => {
          this.allCategories = res;
          this.updateGridRows();
        })
      );
  }

  private updateGridRows(): void {
    if (this.useCompCategories) {
      this.rowData.next(
        this.allCategories
          .map(x => CategoryItem.fromCompCategory(x, this.compCategories))
          .sort((a, b) => {
            if (a.selected && !b.selected) return -1;
            if (!a.selected && b.selected) return 1;
            return 0;
          })
      );
    } else {
      this.rowData.next(
        this.allCategories
          .map(x => CategoryItem.fromCategory(x, this.categoryIds))
          .sort((a, b) => {
            if (a.selected && !b.selected) return -1;
            if (!a.selected && b.selected) return 1;
            return 0;
          })
      );
    }
  }

  override onGridReadyToDefine(grid: GridComponent<CategoryItem>) {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellChanged.bind(this),
    });

    this.translate
      .get([
        'common.categories.selected',
        'common.categories.category',
        'common.categories.standard',
        'common.categories.datefrom',
        'common.categories.dateto',
        'common.categories.isexecutive',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnBool(
          'selected',
          terms['common.categories.selected'],
          {
            width: 40,
            suppressFilter: true,
            editable: !this.readOnly,
            columnSeparator: true,
            onClick: this.toggleSelected.bind(this),
          }
        );
        this.grid.addColumnText('name', terms['common.categories.category'], {
          flex: 1,
        });
        if (this.showDefault) {
          this.grid.addColumnBool(
            'default',
            terms['common.categories.standard'],
            {
              width: 75,
              suppressFilter: true,
              editable: row => this.isEditEnabled(row.data!, 'disabled'),
              onClick: this.toggleDefault.bind(this),
            }
          );
        }
        if (this.showDateFrom) {
          this.grid.addColumnDate(
            'dateFrom',
            terms['common.categories.datefrom'],
            {
              width: 100,
              suppressFilter: true,
              editable: row => this.isEditEnabled(row.data!, 'selected'),
            }
          );
        }
        if (this.showDateTo) {
          this.grid.addColumnDate('dateTo', terms['common.categories.dateto'], {
            width: 100,
            suppressFilter: true,
            editable: row => this.isEditEnabled(row.data!, 'selected'),
          });
        }
        if (this.showIsExecutive) {
          this.grid.addColumnBool(
            'isExecutive',
            terms['common.categories.isexecutive'],
            {
              width: 75,
              suppressFilter: true,
              editable: row => this.isEditEnabled(row.data!, 'disabled'),
              onClick: this.toggleIsExecutive.bind(this),
            }
          );
        }

        this.grid.setNbrOfRowsToShow(this.nbrOfRows, this.nbrOfRows);
        this.grid.context.suppressGridMenu = true;

        if (!this.useFilters) {
          this.grid.context.suppressFiltering = true;
        }

        super.finalizeInitGrid({ hidden: true });
      });
  }

  isEditEnabled(row: CategoryItem, check: 'selected' | 'disabled'): boolean {
    return (
      !this.readOnly &&
      (check !== 'selected' || row.selected) &&
      (check !== 'disabled' || !row.disabled)
    );
  }

  toggleSelected(isChecked: boolean, row: CategoryItem) {
    this.rowData.pipe(take(1)).subscribe(rows => {
      this.emitRows(
        rows.map(x =>
          x.categoryId !== row.categoryId ? x : { ...x, selected: isChecked }
        )
      );
    });
  }

  toggleDefault(isChecked: boolean, row: CategoryItem) {
    this.rowData.pipe(take(1)).subscribe(rows => {
      this.emitRows(
        rows.map(x =>
          x.categoryId !== row.categoryId ? x : { ...x, default: isChecked }
        )
      );
    });
  }

  toggleIsExecutive(isChecked: boolean, row: CategoryItem) {
    this.rowData.pipe(take(1)).subscribe(rows => {
      this.emitRows(
        rows.map(x =>
          x.categoryId !== row.categoryId ? x : { ...x, isExecutive: isChecked }
        )
      );
    });
  }

  onCellChanged(event: CellValueChangedEvent) {
    const { colDef, newValue, oldValue } = event;
    if (newValue === oldValue) {
      return;
    }
    switch (colDef.field) {
      case 'dateFrom':
      case 'dateTo': {
        this.rowData.pipe(take(1)).subscribe(rows => {
          this.emitRows(rows);
        });
      }
    }
  }

  private emitRows(rows: CategoryItem[]) {
    this.categoriesChange.emit(rows.filter(x => x.selected));
    this.form?.markAsDirty();
  }
}
