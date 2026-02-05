import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject,
} from '@angular/core';
import { ValidationHandler } from '@shared/handlers';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CoreService } from '@shared/services/core.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import {
  TermGroup,
  TermGroup_PurchaseCartStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { tap } from 'rxjs';
import { PriceOptimizationGridHeaderForm } from '../../models/price-optimization-grid-header-form.model';
import { PurchaseCartFilterDTO } from '../../models/price-optimization.model';
import { Perform } from '@shared/util/perform.class';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'soe-price-optimization-grid-header',
  templateUrl: './price-optimization-grid-header.component.html',
  standalone: false,
})
export class PriceOptimizationGridHeaderComponent implements OnInit {
  validationHandler = inject(ValidationHandler);
  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  translate = inject(TranslateService);
  performLoad = new Perform<any>(this.progressService);

  @Input() cartStatus: ISmallGenericType[] = [];
  @Output() filterChange = new EventEmitter<PurchaseCartFilterDTO>();
  @Output() filterReady = new EventEmitter<PurchaseCartFilterDTO>();

  allItemsSelectionDict: ISmallGenericType[] = [];

  formFilter: PriceOptimizationGridHeaderForm =
    new PriceOptimizationGridHeaderForm({
      validationHandler: this.validationHandler,
      element: new PurchaseCartFilterDTO(),
    });

  ngOnInit() {
    this.loadSelectionTypes();
  }

  allItemsSelectionChanged(event: number) {
    this.formFilter.allItemsSelectionId.patchValue(event);
    this.emitFilterOnChange();
  }

  statusSelectionComplete(event: number[]) {
    this.formFilter.selectedCartStatusIds.patchValue(
      event || [TermGroup_PurchaseCartStatus.Open]
    );

    this.emitFilterOnChange();
  }

  emitFilterOnChange() {
    this.filterChange.emit(this.formFilter.value);
  }

  loadSelectionTypes() {
    this.performLoad.load(
      this.coreService
        .getTermGroupContent(
          TermGroup.ChangeStatusGridAllItemsSelection,
          false,
          true,
          true
        )
        .pipe(
          tap(data => {
            this.allItemsSelectionDict = data;
          })
        )
    );
  }
}
