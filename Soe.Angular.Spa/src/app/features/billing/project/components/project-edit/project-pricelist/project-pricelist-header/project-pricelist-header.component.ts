import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnInit,
  Output,
} from '@angular/core';
import { ProjectService } from '@features/billing/project/services/project.service';
import { TranslateService } from '@ngx-translate/core';
import { PricelistTypeDialogComponent } from '@shared/components/pricelist-type-dialog/component/pricelist-type-dialog/pricelist-type-dialog.component';
import { PriceListTypeDialogData } from '@shared/components/pricelist-type-dialog/models/pricelist-type.model';
import { CrudActionTypeEnum } from '@shared/enums';
import {
  SoeCheckboxFormControl,
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IPriceListTypeDTO } from '@shared/models/generated-interfaces/PriceListTypeDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { DateUtil } from '@shared/util/date-util';
import { Perform } from '@shared/util/perform.class';
import { ResponseUtil } from '@shared/util/response-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { filter, tap } from 'rxjs';

@Component({
  selector: 'soe-project-pricelist-header',
  templateUrl: './project-pricelist-header.component.html',
  standalone: false,
})
export class ProjectPricelistHeaderComponent implements OnInit {
  @Input() allPriceLists: IPriceListTypeDTO[] = [];
  @Input() projectPriceLists: ISmallGenericType[] = [];
  @Input() comparisonPriceLists: ISmallGenericType[] = [];
  @Input() priceListTypeId: number = 0;
  @Input() comparisonPricelistId: number = 0;
  @Output() projectStatusChanged = new EventEmitter<number>();
  @Output() pricelistChanged = new EventEmitter<number>();
  @Output() comparisonPricelistChanged = new EventEmitter<number>();
  @Output() priceDateChanged = new EventEmitter<Date>();
  @Output() loadAllProductsChanged = new EventEmitter<boolean>();

  validationHandler = inject(ValidationHandler);
  dialogService = inject(DialogService);
  translate = inject(TranslateService);
  progressService = inject(ProgressService);
  performAction = new Perform<BackendResponse>(this.progressService);
  projectService = inject(ProjectService);
  handler = inject(FlowHandlerService);
  coreService = inject(CoreService);

  form = new SoeFormGroup(this.validationHandler, {
    priceListTypeId: new SoeSelectFormControl(0),
    comparisonPricelistId: new SoeSelectFormControl(0),
    priceDate: new SoeDateFormControl(DateUtil.getToday()),
    isLoadAllProducts: new SoeCheckboxFormControl(false),
  });

  ngOnInit(): void {
    this.form.patchValue({
      priceListTypeId: this.priceListTypeId,
      comparisonPricelistId: this.comparisonPricelistId,
    });
  }

  addPriceList(isEdit: boolean): void {
    const dialogData: PriceListTypeDialogData = {
      title: this.translate.instant('billing.product.pricelist.new'),
      size: 'md',
      priceListTypeId: isEdit ? this.form?.get('priceListTypeId')?.value : 0,
    };

    this.dialogService
      .open(PricelistTypeDialogComponent, dialogData)
      .afterClosed()
      .pipe(filter(value => !!value))
      .subscribe((value: IPriceListTypeDTO) => {
        if (value) {
          this.savePricelistType(value);
        }
      });
  }

  priceListOnChange(value: any) {
    this.form.get('priceListTypeId')?.setValue(value);
    this.pricelistChanged.emit(value);
  }

  savePricelistType(priceListType: IPriceListTypeDTO) {
    this.performAction.crud(
      CrudActionTypeEnum.Save,

      this.projectService.savePriceList(priceListType).pipe(
        tap(result => {
          if (result.success) {
            const entityId = ResponseUtil.getEntityId(result);
            this.projectPriceLists.push({
              id: entityId,
              name: priceListType.name,
            });
            this.priceListOnChange(entityId);
          }
        })
      )
    );
  }
}
