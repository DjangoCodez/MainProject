import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  ViewChild,
  OnInit,
} from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { GridComponent } from '@ui/grid/grid.component';
import { take } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';

interface IMultiSelectGridBase<T> {
  selected: boolean;
}

@Component({
  selector: 'soe-multi-select-grid',
  templateUrl: './multi-select-grid.component.html',
  styleUrls: ['./multi-select-grid.component.scss'],
  providers: [FlowHandlerService],
  standalone: false,
})
export class MultiSelectGridComponent<T> implements OnChanges, OnInit {
  @Input() idFieldName = 'id';
  @Input() nameFieldName = 'name';
  @Input() rows = new BehaviorSubject<T[] | undefined>([]);
  @Input() selectedIds: number[] = [];
  @Input() height = 250;
  @Input() showHeader = false;
  @Input() showFilters = false;
  @Input() showFooter = false;
  @Output() selectedItemsChanged = new EventEmitter<number[]>();

  @ViewChild(GridComponent)
  grid!: GridComponent<T>;

  flowHandler = inject(FlowHandlerService);
  translationService = inject(TranslateService);

  remappedRows = new BehaviorSubject<IMultiSelectGridBase<T>[] | undefined>(
    undefined
  );

  ngOnInit(): void {
    this.flowHandler.execute({
      permission: Feature.None,
      setupGrid: this.setupGrid.bind(this),
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.rows?.firstChange) {
      this.rows.asObservable().subscribe(() => {
        this.setRemappedRows();
      });
    }

    if (changes.selectedIds) {
      this.setRemappedRows();
    }
  }

  setRemappedRows() {
    const data: IMultiSelectGridBase<T>[] =
      this.rows.value?.map((item: T) => {
        return {
          ...item,
          selected: this.selectedIds.includes(
            item[this.idFieldName as keyof T] as number
          ),
        };
      }) || [];
    this.remappedRows.next(data);
  }

  setupGrid(grid: GridComponent<T>) {
    this.grid = grid;

    this.grid.context.suppressGridMenu = true;

    this.translationService
      .get(['common.selected', 'common.name'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnBool('selected', terms['common.selected'], {
          editable: true,
          width: 32,
          onClick: this.triggerSelectedItemChanged.bind(this),
        });
        this.grid.addColumnText(
          this.nameFieldName as string,
          terms['common.name'],
          {
            flex: 100,
          }
        );
        this.grid.finalizeInitGrid({ hidden: !this.showFooter });
      });
  }

  triggerSelectedItemChanged(selected: boolean, row: any): void {
    const remappedRow = this.remappedRows.value?.find(
      (r: any) => (r[this.idFieldName] as number) === row[this.idFieldName]
    );
    if (remappedRow) remappedRow.selected = selected;

    this.selectedItemsChanged.emit(
      this.remappedRows.value
        ?.filter(r => r.selected)
        .map((r: any) => r[this.idFieldName] as number)
    );
  }
}
