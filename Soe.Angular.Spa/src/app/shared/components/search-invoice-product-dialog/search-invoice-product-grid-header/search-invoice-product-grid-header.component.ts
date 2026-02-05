import {
  Component,
  DestroyRef,
  ElementRef,
  EventEmitter,
  inject,
  Input,
  Output,
  signal,
} from '@angular/core';
import { ShortcutService } from '@core/services/shortcut.service';
import {
  SoeFormGroup,
  SoeSelectFormControl,
  SoeTextFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ISysProductGroupSmallDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';

@Component({
  selector: 'soe-search-invoice-product-grid-header',
  templateUrl: './search-invoice-product-grid-header.component.html',
  standalone: false,
})
export class SearchInvoiceProductGridHeaderComponent {
  @Input() firstTierCategories: ISysProductGroupSmallDTO[] = [];
  @Input() secondTierCategories = signal<ISysProductGroupSmallDTO[]>([]);
  @Input() useExtendSearchInfo: boolean = false;
  @Output() onSearchClick = new EventEmitter<string>();
  @Output() firstTierCategoryChanged = new EventEmitter<number>();
  @Output() secondTierCategoryChanged = new EventEmitter<number>();
  validationHandler = inject(ValidationHandler);
  shortcutService = inject(ShortcutService);
  form = new SoeFormGroup(this.validationHandler, {
    freetextsearch: new SoeTextFormControl(''),
    selectedFirstTierCategoryId: new SoeSelectFormControl(0),
    selectedSecondTierCategoryId: new SoeSelectFormControl(0),
  });

  constructor(
    private element: ElementRef,
    private destroyRef: DestroyRef
  ) {
    this.shortcutService.bindShortcut(
      this.element,
      this.destroyRef,
      ['Enter'],
      e => this.search()
    );
  }

  search(): void {
    this.onSearchClick.emit(this.form.value.freetextsearch);
  }
}
