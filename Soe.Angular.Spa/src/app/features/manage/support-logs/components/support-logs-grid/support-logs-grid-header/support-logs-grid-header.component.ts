import {
  Component,
  HostListener,
  OnInit,
  inject,
  input,
  model,
  output,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl } from '@angular/forms';
import { SearchSysLogsForm } from '@features/manage/support-logs/models/support-logs-search-form.model';
import {
  LevelOption,
  SearchSysLogsDTO,
} from '@features/manage/support-logs/models/support-logs.model';
import { ValidationHandler } from '@shared/handlers';

@Component({
    selector: 'soe-support-logs-grid-header',
    templateUrl: './support-logs-grid-header.component.html',
    styleUrls: ['./support-logs-grid-header.component.scss'],
    standalone: false
})
export class SupportLogsGridHeaderComponent implements OnInit {
  isSearch = input.required<boolean>();
  searchDto = model.required<SearchSysLogsDTO>();
  searchTextChanged = output<string | null | undefined>();

  validationHandler = inject(ValidationHandler);
  emptyLabel = '\u00a0';
  levelFilterOptions: LevelOption[] = [];

  formSearch: SearchSysLogsForm = new SearchSysLogsForm({
    validationHandler: this.validationHandler,
    element: new SearchSysLogsDTO(),
  });
  searchTerm = new FormControl('');

  private formValueChanged$ =
    this.formSearch.valueChanges.pipe(takeUntilDestroyed());
  private searchTextChanged$ =
    this.searchTerm.valueChanges.pipe(takeUntilDestroyed());
  private levelChanged$ =
    this.formSearch.levelSelect.valueChanges.pipe(takeUntilDestroyed());

  ngOnInit(): void {
    this.setupLevels();
    this.formValueChanged$.subscribe(() => {
      this.searchDto.set(this.formSearch.value as SearchSysLogsDTO);
    });
    this.searchTextChanged$.subscribe(() => {
      this.searchTextChanged.emit(this.searchTerm.value);
    });
    this.levelChanged$.subscribe((value: number) => {
      const item = this.levelFilterOptions.find(x => x.id === value);
      this.formSearch.level.setValue(item?.idStr ?? 'NONE');
    });
  }

  private setupLevels(): void {
    this.levelFilterOptions.push(<LevelOption>{
      id: 1,
      idStr: 'NONE',
      name: 'All',
    });
    this.levelFilterOptions.push(<LevelOption>{
      id: 2,
      idStr: 'INFO',
      name: 'Information',
    });
    this.levelFilterOptions.push(<LevelOption>{
      id: 3,
      idStr: 'WARN',
      name: 'Warning',
    });
    this.levelFilterOptions.push(<LevelOption>{
      id: 4,
      idStr: 'ERROR',
      name: 'Error',
    });
  }

  @HostListener('window:keydown.enter', ['$event'])
  handleEnterPress(event: KeyboardEvent) {
    event.preventDefault();
  }
}
