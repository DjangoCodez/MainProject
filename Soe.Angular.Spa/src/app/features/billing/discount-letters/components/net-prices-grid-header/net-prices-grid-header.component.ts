import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject,
  input,
} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { take } from 'rxjs';

enum SupplierAgreementNetPricesButtonFunctions {
  RemoveNetPrices = 2,
  AddNetPrices = 3,
}

@Component({
  selector: 'soe-net-prices-grid-header',
  templateUrl: './net-prices-grid-header.component.html',
  styleUrls: ['./net-prices-grid-header.component.scss'],
  standalone: false,
})
export class NetPricesGridHeaderComponent implements OnInit {
  @Input() wholesellersDict: SmallGenericType[] = [];
  @Output() filterChanged = new EventEmitter<number>();
  @Output() clickRemoveNetprices = new EventEmitter<void>();
  @Output() clickAddNetprices = new EventEmitter<void>();
  disableDeleteFunction = input(false);

  validationHandler = inject(ValidationHandler);
  translate = inject(TranslateService);
  buttonFunctions: MenuButtonItem[] = [];
  firstAction?: MenuButtonItem;

  form = new SoeFormGroup(this.validationHandler, {
    selectedWholesellerId: new SoeSelectFormControl(0),
  });

  ngOnInit(): void {
    this.translate
      .get([
        'billing.invoices.supplieragreement.removenetprices',
        'billing.invoices.supplieragreement.addnetprices',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.buttonFunctions = [
          {
            id: SupplierAgreementNetPricesButtonFunctions.AddNetPrices,
            label: terms['billing.invoices.supplieragreement.addnetprices'],
            icon: ['fal', 'file-import'],
          },
          {
            id: SupplierAgreementNetPricesButtonFunctions.RemoveNetPrices,
            label: terms['billing.invoices.supplieragreement.removenetprices'],
            icon: ['fal', 'times'],
            iconClass: 'errorColor',
            disabled: this.disableDeleteFunction,
          },
        ];
        this.firstAction = this.buttonFunctions[0];
      });
  }

  actionSelected(item: MenuButtonItem) {
    switch (item.id) {
      case SupplierAgreementNetPricesButtonFunctions.RemoveNetPrices: {
        this.clickRemoveNetprices.emit();
        break;
      }
      case SupplierAgreementNetPricesButtonFunctions.AddNetPrices: {
        this.clickAddNetprices.emit();
        break;
      }
    }
  }
}
