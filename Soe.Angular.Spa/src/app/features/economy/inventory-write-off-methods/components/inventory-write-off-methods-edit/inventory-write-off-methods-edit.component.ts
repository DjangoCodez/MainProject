import {
  AfterViewInit,
  Component,
  ElementRef,
  OnInit,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { Perform } from '@shared/util/perform.class';
import { focusOnElement } from '@shared/util/focus-util';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { tap } from 'rxjs/operators';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Observable } from 'rxjs';
import { InventoryWriteOffMethodsService } from '../../services/inventory-write-off-methods.service';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { InventoryWriteOffMethodDTO } from '../../models/inventory-write-off-method.model';
import { InventoryWriteOffMethodForm } from '../../models/inventory-write-off-method-form.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { SoeFormControl } from '@shared/extensions';

@Component({
  selector: 'soe-inventory-write-off-methods-edit',
  templateUrl: './inventory-write-off-methods-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class InventoryWriteOffMethodsEditComponent
  extends EditBaseDirective<
    InventoryWriteOffMethodDTO,
    InventoryWriteOffMethodsService,
    InventoryWriteOffMethodForm
  >
  implements OnInit, AfterViewInit
{
  @ViewChild('name')
  public name!: ElementRef;

  service = inject(InventoryWriteOffMethodsService);
  coreService = inject(CoreService);
  performWriteoffMethodTypeLoad = new Perform<SmallGenericType[]>(
    this.progressService
  );
  performPeriodTypeLoad = new Perform<SmallGenericType[]>(this.progressService);
  isIteration = signal<boolean>(true);

  writeoffMethodTypes: SmallGenericType[] = [];
  periodTypes: SmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Inventory_WriteOffMethods_Edit, {
      lookups: [this.loadTypes(), this.loadPeriodTypes()],
      skipDefaultToolbar: true,
    });
  }

  ngAfterViewInit(): void {
    if (this.form?.isNew) {
      this.focusOnName();
    }
  }

  focusOnName() {
    focusOnElement((<any>this.name).inputER.nativeElement, 150);
  }

  override loadData(): Observable<void> {
    return super.loadData().pipe(
      tap(() => {
        this.writeOffTypeChanged(this.form?.type.value as number);
        this.validateWriteOffMethodUsage(!!this.form?.hasAcitveWirteOffs.value);
      })
    );
  }

  loadPeriodTypes(): Observable<SmallGenericType[]> {
    return this.performPeriodTypeLoad.load$(
      this.coreService
        .getTermGroupContent(
          TermGroup.InventoryWriteOffMethodPeriodType,
          false,
          false
        )
        .pipe(tap(priodTypes => (this.periodTypes = priodTypes)))
    );
  }

  loadTypes(): Observable<SmallGenericType[]> {
    return this.performWriteoffMethodTypeLoad.load$(
      this.coreService
        .getTermGroupContent(
          TermGroup.InventoryWriteOffMethodType,
          false,
          false
        )
        .pipe(tap(types => (this.writeoffMethodTypes = types)))
    );
  }

  protected writeOffTypeChanged(value: number): void {
    this.isIteration.set(value == 1 || value == 3);
  }

  private validateWriteOffMethodUsage(hasAcitveWirteOffs: boolean): void {
    const controls = [
      this.form?.type,
      this.form?.periodType,
      this.form?.periodValue,
      this.form?.yearPercent,
    ];

    if (hasAcitveWirteOffs) {
      this.form?.disableFormControls(<SoeFormControl[]>controls);
    } else {
      this.form?.enableFormControls(<SoeFormControl[]>controls);
    }
  }
}
