import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnInit,
  Output,
} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';

@Component({
    selector: 'soe-edestribution-grid-header',
    templateUrl: './edestribution-grid-header.component.html',
    styleUrl: './edestribution-grid-header.component.scss',
    standalone: false
})
export class EdestributionGridHeaderComponent {
  @Input() distributionTypesDict: SmallGenericType[] = [];
  @Input() originTypesDict: SmallGenericType[] = [];
  @Input() allItemsSelectionDict: SmallGenericType[] = [];
  @Output() distributionTypefilterChanged = new EventEmitter<number>();
  @Output() originTypefilterChanged = new EventEmitter<number>();
  @Output() allItemSelectionfilterChanged = new EventEmitter<number>();

  validationHandler = inject(ValidationHandler);
  translate = inject(TranslateService);

  form = new SoeFormGroup(this.validationHandler, {
    selectedDistributedTypeId: new SoeSelectFormControl(0),
    selectedOriginTypeId: new SoeSelectFormControl(0),
    selectedSelectionTypeId: new SoeSelectFormControl(1),
  });
}
