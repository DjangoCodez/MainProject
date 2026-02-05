import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';

@Component({
    selector: 'soe-edi-grid-header',
    templateUrl: './edi-grid-header.component.html',
    standalone: false
})
export class EdiGridHeaderComponent {

  @Input() allItemsSelectionDict: ISmallGenericType[] = [];
  @Output() allItemSelectionfilterChanged = new EventEmitter<number>();

  validationHandler = inject(ValidationHandler);

  form = new SoeFormGroup(this.validationHandler, {
    selectedSelectionTypeId: new SoeSelectFormControl(99),
  });
}
