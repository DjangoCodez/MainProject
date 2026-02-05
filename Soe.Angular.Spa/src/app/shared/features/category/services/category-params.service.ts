import {
  computed,
  DestroyRef,
  inject,
  Injectable,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import {
  Feature,
  SoeCategoryType,
} from '@shared/models/generated-interfaces/Enumerations';
import { tap } from 'rxjs';

@Injectable()
export class CategoryUrlParamsService {
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  categoryType = signal<number>(0);
  readonly params$ = this.route.queryParamMap
    .pipe(
      takeUntilDestroyed(this.destroyRef),
      tap(params => {
        this.categoryType.set(Number(params.get('type')));
      })
    )
    .subscribe();

  showSubCategory = computed(() => {
    const hidableCategories = [
      SoeCategoryType.Product,
      SoeCategoryType.Customer,
      SoeCategoryType.Supplier,
      SoeCategoryType.Project,
      SoeCategoryType.Contract,
      SoeCategoryType.Order,
      SoeCategoryType.ContactPerson,
      SoeCategoryType.Dokument,
    ];
    return !hidableCategories.some(x => x === this.categoryType());
  });

  showParentCategoryLabel = computed(() => {
    return this.categoryType() === SoeCategoryType.Inventory;
  });

  feature = computed(() => {
    switch (this.categoryType()) {
      case SoeCategoryType.Product:
        return Feature.Common_Categories_Product_Edit;
      case SoeCategoryType.Customer:
        return Feature.Common_Categories_Customer_Edit;
      case SoeCategoryType.Supplier:
        return Feature.Common_Categories_Supplier_Edit;
      case SoeCategoryType.ContactPerson:
        return Feature.Common_Categories_ContactPersons_Edit;
      case SoeCategoryType.AttestRole:
        return Feature.Common_Categories_AttestRole_Edit;
      case SoeCategoryType.Employee:
        return Feature.Common_Categories_Employee_Edit;
      case SoeCategoryType.Project:
        return Feature.Common_Categories_Project_Edit;
      case SoeCategoryType.Contract:
        return Feature.Common_Categories_Contract_Edit;
      case SoeCategoryType.Inventory:
        return Feature.Common_Categories_Inventory_Edit;
      case SoeCategoryType.Order:
        return Feature.Common_Categories_Order_Edit;
      case SoeCategoryType.PayrollProduct:
        return Feature.Common_Categories_PayrollProduct_Edit;
      case SoeCategoryType.Dokument:
        return Feature.Common_Categories_Document_Edit;
      default:
        return 0;
    }
  });

  public destroy() {
    this.params$.unsubscribe();
  }
}
