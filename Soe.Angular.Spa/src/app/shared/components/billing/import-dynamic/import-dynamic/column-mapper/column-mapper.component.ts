import { CdkDragEnd } from '@angular/cdk/drag-drop';
import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  ViewEncapsulation,
  effect,
  inject,
  input,
  model,
} from '@angular/core';
import { IImportFieldDTO } from '@shared/models/generated-interfaces/ImportDynamicDTO';
import { faAsterisk } from '@fortawesome/pro-light-svg-icons';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { FieldOptionsComponent } from '../field-options/field-options.component';
import { FieldOptionsData } from '../field-options/field-options.model';
import { ImportDynamicForm, ImportFieldDTO } from '../import-dynamic.model';
import { TranslateService } from '@ngx-translate/core';

export interface ColumnMapperColumns {
  columnHeader: string;
  columnIndex: number;
}

export interface ColumnMapperFormData {
  label: string;
  objectKey: string;
}

@Component({
  selector: 'soe-column-mapper',
  templateUrl: './column-mapper.component.html',
  styleUrls: ['./column-mapper.component.scss'],
  encapsulation: ViewEncapsulation.None,
  standalone: false,
})
export class ColumnMapperComponent<T> implements OnChanges {
  @Input({ required: true }) columns: ColumnMapperColumns[] = [];
  fields = model<ImportFieldDTO[]>([]);
  @Input() columnHeaderLabel = '';
  @Input() fieldHeaderLabel = '';
  @Input() disableMultipleSelection = false;
  @Input() importDynamicForm!: ImportDynamicForm;
  fileRows = input(new Array<string[]>([]));
  skipFirstRow = input<boolean>();
  @Output() allRequiredFieldsSet = new EventEmitter<boolean>();

  translate = inject(TranslateService);
  dialogService = inject(DialogService);
  assignedColumns: number[] = [];
  mappedObject: Record<string, ColumnMapperColumns | null> = {};
  isDragging = false;

  readonly faAsterisk = faAsterisk;
  selectedRecordIndex = 0;
  recordNavRecords: SmallGenericType[] = [];

  constructor() {
    effect(() => {
      this.recordNavRecords = this.fileRows().map(
        (x, i) => new SmallGenericType(i, String(i + 1))
      );
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    const { formData } = changes;
    if (formData) {
      this.constructFormKeys(formData.currentValue);
    }
  }

  unlinkFormItem(item: IImportFieldDTO) {
    this.mappedObject[item.field] = null;
    this.fields.update(fields => {
      fields.forEach(field => {
        if (field.field === item.field) {
          field.index = -1;
        }
      });
      return fields;
    });
    this.updateAssignedColumns();
  }

  updateAssignedColumns(): void {
    this.assignedColumns = Object.values(this.mappedObject)
      .filter(v => v)
      .map(x => (x as ColumnMapperColumns).columnIndex);

    this.emitRequiredFieldStatus();
  }

  private emitRequiredFieldStatus(): void {
    const _fields = this.fields();
    const requiredFieldCount = _fields.filter(f => f.isRequired).length;

    this.allRequiredFieldsSet.emit(
      _fields.filter(f => f.isRequired && f.index > -1).length ===
        requiredFieldCount
    );
  }

  dragStarted() {
    this.isDragging = true;
  }

  dragEnded(ended: CdkDragEnd, column: ColumnMapperColumns) {
    this.isDragging = false;
    if (ended.event.target instanceof HTMLInputElement) {
      const id = ended.event.target.id;
      if (id) {
        this.mappedObject[id] = column;
        this.fields.update(fields => {
          fields.forEach(field => {
            if (field.field === id) {
              field.index = column.columnIndex;
            }
          });
          return fields;
        });
        this.updateAssignedColumns();
      }
    }
  }

  constructFormKeys(formValues: IImportFieldDTO[]) {
    formValues.forEach(item => {
      this.mappedObject[item.field] = null;
    });
  }

  recordChanged(event: SmallGenericType): void {
    this.selectedRecordIndex = event.id;
  }

  showFieldOption(field: IImportFieldDTO): void {
    if (field && field.index !== null && field.index !== undefined) {
      let uniqueValues: string[] = [];

      if (field.index !== -1 && field.enableValueMapping) {
        let allValues = this.fileRows().map(x => x[field.index]);

        if (this.skipFirstRow()) allValues = allValues.slice(1);

        uniqueValues = Array.from(new Set(allValues));
      }

      this.dialogService
        .open(FieldOptionsComponent, <FieldOptionsData>{
          field: field,
          uniqueValues: uniqueValues,
          size: 'md',
          title: 'common.importdynamic.fieldsettings',
          disableClose: true,
        })
        .afterClosed()
        .subscribe(value => {
          if (value) {
            this.fields.update(fs => {
              const idx = fs.findIndex(x => x.field === field.field);
              fs[idx] = value;
              return fs;
            });
          }
        });
    }
  }

  getReferenceValue(colIdx: number): string {
    if (colIdx === -1) return '';
    if (this.fileRows().length === 0) return '';

    if (this.fileRows()[this.selectedRecordIndex][colIdx] === '')
      return this.translate.instant('common.importdynamic.cellmissingvalue');

    return this.fileRows()[this.selectedRecordIndex][colIdx];
  }
}
