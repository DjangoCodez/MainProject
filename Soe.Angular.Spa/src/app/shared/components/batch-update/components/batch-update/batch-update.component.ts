import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { TermCollection } from '@shared/localization/term-types';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  BatchUpdateFieldType,
  SoeEntityType,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ProgressOptions } from '@shared/services/progress';
import { Perform } from '@shared/util/perform.class';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { Observable, tap } from 'rxjs';
import { BatchUpdateDialogData } from '../../models/batch-update-dialog-data.model';
import {
  BatchUpdateCollectionForm,
  employeeFromDateValidator,
} from '../../models/batch-update-form.model';
import {
  BatchUpdateDTO,
  PerformBatchUpdateModel,
  RefreshBatchUpdateOptionsModel,
} from '../../models/batch-update.model';
import { BatchUpdateService } from '../../services/batch-update.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-batch-update',
  templateUrl: './batch-update.component.html',
  styleUrls: ['./batch-update.component.scss'],
  providers: [FlowHandlerService],
  standalone: false,
})
export class BatchUpdateComponent
  extends DialogComponent<BatchUpdateDialogData>
  implements OnInit
{
  private readonly flowHandler = inject(FlowHandlerService);
  private readonly translate = inject(TranslateService);
  private readonly service = inject(BatchUpdateService);
  private readonly validationHandler = inject(ValidationHandler);
  private readonly progress = inject(ProgressService);
  private readonly perform = new Perform<any>(this.progress);
  private readonly messageBox = inject(MessageboxService);
  protected form: BatchUpdateCollectionForm = new BatchUpdateCollectionForm({
    validationHandler: this.validationHandler,
    element: new PerformBatchUpdateModel(),
  });

  private terms: TermCollection = {};
  private latestSelectedFromDate?: Date;
  private latestSelectedToDate?: Date;
  protected idCountText = signal<string>('');
  protected batchUpdateInfoText = signal<string>('');
  protected batchUpdates = signal<BatchUpdateDTO[]>([]);
  protected notAddedBatchUpdates = computed(() => {
    return [new BatchUpdateDTO(), ...this.batchUpdates().filter(b => !b.added)];
  });
  protected filterOptions: SmallGenericType[] = [];
  protected filterTranslationKey: string = '';

  protected get addedBatchUpdates(): BatchUpdateDTO[] {
    return this.form?.batchUpdates.value;
  }

  private get entityTypeIsEmployee(): boolean {
    return this.data.entityType === SoeEntityType.Employee;
  }

  private get entityTypeIsPayrollProduct(): boolean {
    return this.data.entityType === SoeEntityType.PayrollProduct;
  }

  private get doUseFilter(): boolean {
    return this.entityTypeIsPayrollProduct;
  }

  private get count(): number {
    return this.data.selectedIds?.length;
  }

  protected doShowFilter(): boolean {
    if (
      this.doUseFilter &&
      this.addedBatchUpdates.filter(b => b.doShowFilter).length > 0
    )
      return true;
    return false;
  }

  protected doShowDates(): boolean {
    return this.entityTypeIsEmployee;
  }

  ngOnInit(): void {
    this.flowHandler.execute({
      permission: undefined,
      terms: this.loadTerms(),
      lookups: [
        this.getBatchUpdateForEntity(),
        this.getBatchUpdateFilterOptions(),
      ],
      onFinished: this.addEmployeeValidator.bind(this),
    });

    this.form?.patchValue({
      entityType: this.data.entityType,
      ids: this.data.selectedIds,
    });
  }

  private addEmployeeValidator(): void {
    if (this.entityTypeIsEmployee)
      this.form?.batchUpdates.addValidators(
        employeeFromDateValidator(
          this.translate.instant('common.batchupdate.datefromnotvalid')
        )
      );
  }

  private loadTerms(): Observable<TermCollection> {
    return this.translate
      .get([
        'common.batchupdate.info',
        'common.batchupdate.selectedrows',
        'common.batchupdate.confirm',
        'common.batchupdate.howto',
        'common.batchupdate.datefromnotvalid',
        'core.info',
        'core.warning',
      ])
      .pipe(
        tap(terms => {
          this.batchUpdateInfoText.set(
            terms['common.batchupdate.info'].replace('{0}', String(this.count))
          );
          this.idCountText.set(
            terms['common.batchupdate.selectedrows'].replace(
              '{0}',
              String(this.count)
            )
          );
          this.terms = terms;
        })
      );
  }

  private getBatchUpdateForEntity(): Observable<void> {
    return this.perform.load$(
      this.service.getBatchUpdateForEntity(this.data.entityType).pipe(
        tap((batchUpdates: BatchUpdateDTO[]): void => {
          this.batchUpdates.set(batchUpdates);
        })
      )
    );
  }

  private getBatchUpdateFilterOptions(): Observable<void> {
    return this.perform.load$(
      this.service.getBatchUpdateFilterOptions(this.data.entityType).pipe(
        tap(options => {
          this.filterOptions = options;
          this.setFilterTranslationKey();
        })
      )
    );
  }

  private getBatchUpdateOptions(batchUpdate: BatchUpdateDTO): Observable<void> {
    return this.perform.load$(
      this.service
        .refreshBatchUpdateOptions(
          new RefreshBatchUpdateOptionsModel(this.data.entityType, batchUpdate)
        )
        .pipe(
          tap((refreshedBatchUpdate: BatchUpdateDTO): void => {
            batchUpdate.options = refreshedBatchUpdate.options;
            if (
              batchUpdate.children &&
              batchUpdate.children.length > 0 &&
              refreshedBatchUpdate.children &&
              refreshedBatchUpdate.children.length ==
                batchUpdate.children.length
            ) {
              for (let i = 0; i < batchUpdate.children.length; i++) {
                batchUpdate.children[i].options =
                  refreshedBatchUpdate.children[i].options;
              }
            }
          })
        )
    );
  }

  private setFilterTranslationKey(): void {
    if (this.data.entityType === SoeEntityType.PayrollProduct)
      this.filterTranslationKey = 'time.employee.payrollgroup.payrollgroups';
  }

  private toggleBatchUpdateData(field: number, added: boolean): void {
    this.batchUpdates.update(bs => {
      bs.forEach(b => {
        if (b.field === field) {
          b.added = added;
        }
      });
      return bs;
    });
    this.batchUpdates.set([...this.batchUpdates()]);
  }

  protected selectedFromDateChanged(idx: number): void {
    this.latestSelectedFromDate =
      this.form?.batchUpdates.at(idx).fromDate.value;
  }

  protected selectedToDateChanged(idx: number): void {
    this.latestSelectedToDate = this.form?.batchUpdates.at(idx).toDate.value;
  }

  protected addRow(): void {
    const selectedBatchUpdate = this.batchUpdates().find(
      b => b.field === this.form?.selectedFieldId.value
    );
    if (selectedBatchUpdate) {
      if (selectedBatchUpdate.doShowFromDate && this.latestSelectedFromDate)
        selectedBatchUpdate.fromDate = this.latestSelectedFromDate;
      if (selectedBatchUpdate.doShowToDate && this.latestSelectedToDate)
        selectedBatchUpdate.toDate = this.latestSelectedToDate;
      if (
        selectedBatchUpdate.dataType == BatchUpdateFieldType.Id &&
        (!selectedBatchUpdate.options ||
          selectedBatchUpdate.options.length == 0)
      )
        this.getBatchUpdateOptions(selectedBatchUpdate);

      this.form?.addBatchUpdate(selectedBatchUpdate);
      this.toggleBatchUpdateData(selectedBatchUpdate.field, true);
      this.form?.selectedFieldId.patchValue(undefined);
    }
  }

  protected removeRow(index: number, batchUpdate: BatchUpdateDTO): void {
    this.form?.removeBatchUpdate(index);
    this.toggleBatchUpdateData(batchUpdate.field, false);
  }

  protected triggerSave(): void {
    this.messageBox
      .question(
        this.terms['core.warning'],
        String(this.terms['common.batchupdate.confirm']).replace(
          '{0}',
          String(this.count)
        ),
        {
          type: 'warning',
          buttons: 'yesNo',
        }
      )
      .afterClosed()
      .subscribe(res => {
        if (res.result === true) {
          this.saveData();
        }
      });
  }

  private saveData(): void {
    const model = <PerformBatchUpdateModel>this.form?.getRawValue();
    this.perform.crud(
      CrudActionTypeEnum.Save,
      this.service.performBatchUpdate(model),
      this.afterSaveSuccess
    );
  }

  private afterSaveSuccess = (result: BackendResponse): void => {
    if (result.success) {
      const message = ResponseUtil.getMessageValue(result);
      if (message && message.length > 0) {
        this.progress.saveComplete(<ProgressOptions>{
          showDialogOnComplete: true,
          showToastOnComplete: false,
          title: 'core.info',
          message: message,
        });
      }
      this.dialogRef.close(true);
    } else {
      this.progress.saveComplete(<ProgressOptions>{
        showDialogOnError: true,
        showToastOnError: false,
        message: ResponseUtil.getErrorMessage(result),
      });
    }
  };
}
