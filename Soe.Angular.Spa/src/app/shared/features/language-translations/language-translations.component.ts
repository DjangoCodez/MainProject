import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  computed,
  inject,
  signal,
} from '@angular/core';
import { ValidationErrors } from '@angular/forms';
import { TranslateService } from '@ngx-translate/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import {
  Feature,
  SoeEntityState,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { ICompTermDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  CellEditingStoppedEvent,
  RowDataUpdatedEvent,
} from 'ag-grid-community';
import { BehaviorSubject, take, tap } from 'rxjs';
import { CompTermDTO } from './models/language-translations.model';
import { AIUtilityService } from '@shared/services/ai-utility.service';

@Component({
  selector: 'soe-language-translations',
  templateUrl: './language-translations.component.html',
  styleUrls: ['./language-translations.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class LanguageTranslationsComponent
  extends GridBaseDirective<CompTermDTO>
  implements OnInit, OnChanges
{
  @Input() form: SoeFormGroup | undefined;
  @Input() compTermRecordType!: number;
  @Input() recordId!: number;
  @Input() readOnly = false;
  @Input() compTermRows: ICompTermDTO[] = [];
  @Input() textToTranslate?: string;
  @Output() compTermRowsChange: EventEmitter<ICompTermDTO[]> = new EventEmitter<
    ICompTermDTO[]
  >();
  @Output() formChange: EventEmitter<SoeFormGroup> =
    new EventEmitter<SoeFormGroup>();

  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  translate = inject(TranslateService);
  messageboxService = inject(MessageboxService);
  aiUtilityService = inject(AIUtilityService);
  performLanguagesLoad = new Perform<ISmallGenericType[]>(this.progressService);

  languages: ISmallGenericType[] = [];
  rowData = new BehaviorSubject<CompTermDTO[]>([]);

  toolbarAddRowDisabled = signal(false);
  toolbarAiTranslateHidden = computed(
    () => !this.aiUtilityService.hasPermission() || !this.textToTranslate
  );

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.None, 'translations.rows', {
      lookups: [this.loadLanguages()],
      skipInitialLoad: true,
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes?.compTermRows) {
      this.setGridData(this.compTermRows);
      this.validateAddButton();
    }
  }

  loadLanguages() {
    return this.performLanguagesLoad
      .load$(
        this.coreService.getTermGroupContent(
          TermGroup.Language,
          false,
          true,
          false
        )
      )
      .pipe(
        tap(types => {
          this.languages = types;
          this.validateAddButton();
        })
      );
  }

  getTranslationSuggestions() {
    if (!this.textToTranslate) return;

    this.aiUtilityService
      .getTranslationSuggestions(
        this.textToTranslate,
        this.languages.map(l => l.id)
      )
      .subscribe(suggestions => {
        const termRows = [...this.rowData.value];
        for (const langId in suggestions) {
          const row = termRows.find(r => r.lang == +langId);
          if (row) {
            row.name = suggestions[+langId];
            row.isModified = true;
            continue;
          }

          const newRow = this.createEmptyRow();
          newRow.lang = +langId;
          newRow.name = suggestions[+langId];
          termRows.push(newRow);
        }
        this.onChanged(termRows);
      });
  }

  onPermissionsLoaded(): void {
    super.onPermissionsLoaded();
  }

  override createGridToolbar(): void {
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('addrow', {
          iconName: signal('plus'),
          caption: signal('common.newrow'),
          tooltip: signal('common.newrow'),
          disabled: this.toolbarAddRowDisabled,
          onAction: () => this.addRow(),
        }),
      ],
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('ai-translate', {
          iconName: signal('wand-sparkles'),
          caption: signal('common.ai.translate'),
          tooltip: signal('common.ai.translatetooltip'),
          hidden: this.toolbarAiTranslateHidden,
          onAction: () => this.getTranslationSuggestions(),
        }),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<CompTermDTO>) {
    super.onGridReadyToDefine(grid);

    grid.setNbrOfRowsToShow(5, 10);

    this.grid.api.updateGridOptions({
      onRowDataUpdated: this.onRowDataUpdated.bind(this),
      onCellEditingStopped: this.onCellEditingStopped.bind(this),
    });

    this.translate
      .get(['common.language', 'common.text', 'core.delete'])
      .pipe(take(1))
      .subscribe(terms => {
        if (!this.readOnly) this.grid.addColumnModified('isModified');
        this.grid.addColumnSelect(
          'lang',
          terms['common.language'],
          this.languages,
          () => {},
          {
            flex: 1,
            editable: !this.readOnly,
          }
        );
        this.grid.addColumnText('name', terms['common.text'], {
          flex: 1,
          editable: !this.readOnly,
        });
        if (!this.readOnly) {
          this.grid.addColumnIconDelete({
            tooltip: terms['core.delete'],
            onClick: row => this.deleteRow(<CompTermDTO>row),
          });
        }

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid();
      });
    this.setGridData(this.compTermRows);
  }

  validateAddButton() {
    if (this.languages.length === 0) return;

    let shouldDisable = false;

    if (this.readOnly && !this.toolbarAddRowDisabled()) {
      shouldDisable = true;
    } else {
      // Check if all languages are already added
      if (!this.toolbarAddRowDisabled()) {
        const langIds = this.languages.map(l => l.id);
        const usedLangIds = this.rowData.value
          .filter(r => r.state !== SoeEntityState.Deleted)
          .map(r => r.lang);
        const missingLangIds = langIds.filter(l => !usedLangIds.includes(l));
        if (missingLangIds.length === 0) {
          shouldDisable = true;
        }
      }
    }

    if (this.toolbarAddRowDisabled() !== shouldDisable) {
      this.toolbarAddRowDisabled.set(shouldDisable);
    }
  }

  createEmptyRow(): CompTermDTO {
    return {
      compTermId: this.getIdForAddLanguage(),
      lang: 0,
      name: '',
      langName: '',
      recordId: this.recordId,
      recordType: this.compTermRecordType,
      state: 0,
      isModified: true,
    };
  }
  addRow() {
    const termRows = this.rowData.value;
    termRows.push(this.createEmptyRow());
    this.grid.options.context.newRow = true;
    this.setGridData(termRows);
    this.validateAddButton();
    this.onChanged(this.rowData.value);
  }

  deleteRow(row: CompTermDTO): void {
    this.rowData.value.forEach(s => {
      if (s.compTermId === row.compTermId) {
        s.state = SoeEntityState.Deleted;
      }
    });
    this.setGridData(this.rowData.value);
    this.validateAddButton();
    this.onChanged(this.rowData.value);
  }

  getIdForAddLanguage() {
    let id = 0;
    this.rowData.value.forEach((s, index) => {
      if (s.compTermId < id) {
        id = s.compTermId;
      }
    });
    return id - 1;
  }

  onRowDataUpdated(event: RowDataUpdatedEvent): void {
    if (event.context.newRow && !event.api.isAnyFilterPresent()) {
      const index = event.api.getLastDisplayedRowIndex();
      event.api.setFocusedCell(index, 'lang');
      event.api.startEditingCell({
        rowIndex: index,
        colKey: 'lang',
      });
      event.context.newRow = false;
    }
  }

  onCellEditingStopped(data: CellEditingStoppedEvent) {
    if (data.colDef.field === 'lang') {
      let duplicate = false;

      this.rowData.value
        .filter(r => r.state != SoeEntityState.Deleted)
        .forEach((s, index) => {
          if (
            index != data.rowIndex &&
            data.newValue.name !== ' ' &&
            data.newValue.id == s.lang
          ) {
            duplicate = true;
          }
        });
      if (duplicate) {
        this.grid.applyChanges();

        this.rowData.value.forEach(obj => {
          if (obj.compTermId === data.data.compTermId) {
            obj.lang = data.oldValue;
          }
        });

        this.messageboxService.warning(
          'core.warning',
          'common.extrafields.warning.duplicateRecord',
          { buttons: 'ok' }
        );
      } else {
        data.data.isModified = true;
      }
    } else {
      data.data.isModified = true;
    }
    this.onChanged(this.rowData.value);
  }

  onChanged(rows: CompTermDTO[]) {
    this.validateCompTermRows();
    this.compTermRowsChange.emit(rows);
    this.form?.markAsDirty();
    this.form?.markAsTouched();
  }

  validateCompTermRows() {
    let missingLangulage = false;
    let missingTranslation = false;

    this.rowData.value.forEach(obj => {
      if (obj.state != SoeEntityState.Deleted) {
        if (!(obj.lang && obj.lang > 0)) {
          missingLangulage = true;
        }
        if (!(obj.name && obj.name.length > 0)) {
          missingTranslation = true;
        }
      }
    });

    if (missingLangulage || missingTranslation) {
      const errors: ValidationErrors = {};

      const missingTerm = this.translate.instant('core.missingmandatoryfield');
      const languageTerm = this.translate.instant('common.language');
      const textTerm = this.translate.instant('common.text');
      if (missingLangulage) {
        errors[missingTerm + ' ' + languageTerm] = true;
      }
      if (missingTranslation) {
        errors[missingTerm + ' ' + textTerm] = true;
      }
      this.form?.setErrors(errors);
    } else {
      this.form?.setErrors(null);
      this.form?.updateValueAndValidity();
    }
  }

  setGridData(compTermRows: ICompTermDTO[]) {
    if (this.grid) {
      this.rowData.next(
        <CompTermDTO[]>(
          compTermRows.filter(r => r.state != SoeEntityState.Deleted)
        )
      );
      this.validateCompTermRows();
    }
  }
}
