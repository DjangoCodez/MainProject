import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject,
} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { take } from 'rxjs';

enum SupplierAgreementButtonFunctions {
  ImportAgreement = 1,
  RemoveAgreement = 2,
  AddRow = 3,
}

@Component({
  selector: 'soe-discount-letters-grid-header',
  templateUrl: './discount-letters-grid-header.component.html',
  standalone: false,
})
export class DiscountLettersGridHeaderComponent implements OnInit {
  @Input() wholesellersDict: SmallGenericType[] = [];
  @Output() filterChanged = new EventEmitter<number>();
  @Output() clickImportAgreement = new EventEmitter<void>();
  @Output() clickRemoveAgreement = new EventEmitter<void>();
  @Output() clickAddRow = new EventEmitter<void>();

  validationHandler = inject(ValidationHandler);
  translate = inject(TranslateService);

  form = new SoeFormGroup(this.validationHandler, {
    selectedWholesellerId: new SoeSelectFormControl(0),
  });

  buttonFunctions: MenuButtonItem[] = [];
  firstAction?: MenuButtonItem;

  ngOnInit(): void {
    this.translate
      .get([
        'billing.invoices.supplieragreement.importagreement',
        'billing.invoices.supplieragreement.deleteagreement',
        'billing.invoices.supplieragreement.adddiscount',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.buttonFunctions = [
          {
            id: SupplierAgreementButtonFunctions.ImportAgreement,
            label: terms['billing.invoices.supplieragreement.importagreement'],
            icon: ['fal', 'file-import'],
          },
          {
            id: SupplierAgreementButtonFunctions.RemoveAgreement,
            label: terms['billing.invoices.supplieragreement.deleteagreement'],
            icon: ['fal', 'times'],
            iconClass: 'errorColor',
          },
          {
            id: SupplierAgreementButtonFunctions.AddRow,
            label: terms['billing.invoices.supplieragreement.adddiscount'],
            icon: ['fal', 'plus'],
          },
        ];
        this.firstAction = this.buttonFunctions[0];
      });
  }

  actionSelected(item: MenuButtonItem) {
    switch (item.id) {
      case SupplierAgreementButtonFunctions.ImportAgreement: {
        this.clickImportAgreement.emit();
        break;
      }
      case SupplierAgreementButtonFunctions.RemoveAgreement: {
        this.clickRemoveAgreement.emit();
        break;
      }
      case SupplierAgreementButtonFunctions.AddRow: {
        this.clickAddRow.emit();
        break;
      }
    }
  }
}
