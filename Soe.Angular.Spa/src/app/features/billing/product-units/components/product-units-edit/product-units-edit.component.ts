import { Component, OnInit, inject } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ProductUnitService } from '../../services/product-unit.service';
import {
  CompTermsRecordType,
  Feature,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ICompTermDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { LanguageTranslationsService } from '@shared/features/language-translations/services/language-translations.service';
import { ProductUnitsForm } from '../../models/product-units-form.model';
import { Observable, of, tap } from 'rxjs';
import { CrudActionTypeEnum } from '@shared/enums';
import { ProgressOptions } from '@shared/services/progress';
import {
  ProductUnitDTO,
  ProductUnitSmallDTO,
} from '../../models/product-units.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-product-units-edit',
  templateUrl: './product-units-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ProductUnitsEditComponent
  extends EditBaseDirective<
    ProductUnitDTO,
    ProductUnitService,
    ProductUnitsForm
  >
  implements OnInit
{
  service = inject(ProductUnitService);
  translationsService = inject(LanguageTranslationsService);

  compTermsRecordType: number = CompTermsRecordType.ProductUnitName;
  translations: ICompTermDTO[] = [];

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Billing_Product_Products_Edit);
  }

  override newRecord(): Observable<void> {
    let clearValues = () => {};

    if (this.form?.isCopy) {
      clearValues = () => {
        this.form?.onDoCopy();
        this.translations = this.form?.translations.value as ICompTermDTO[];
      };
    } else if (!this.form?.isNew) this.getTranslations();

    return of(clearValues());
  }

  getTranslations() {
    this.performLoadData.load(
      this.translationsService
        .getTranslations(
          this.compTermsRecordType,
          this.form?.productUnitId.value,
          true
        )
        .pipe(
          tap(value => {
            this.translations = value;
            this.form?.patchCompTerms(value);
          })
        )
    );
  }

  override loadData(): Observable<void> {
    return super.loadData().pipe(tap(() => this.getTranslations()));
  }

  override performSave(options?: ProgressOptions): void {
    if (!this.form || this.form.invalid || !this.service) return;

    const productUnit = {
      productUnitId: this.form.getRawValue().productUnitId,
      code: this.form.getRawValue().code,
      name: this.form.getRawValue().name,
    } as ProductUnitSmallDTO;
    this.form?.patchCompTerms(
      this.translations.filter(t => t.state !== SoeEntityState.Deleted)
    );

    const model = {
      productUnit: productUnit,
      translations: this.form?.translations.value,
    };

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model).pipe(tap(this.updateFormValueAndEmitChange)),
      undefined,
      undefined,
      options
    );
  }
}
