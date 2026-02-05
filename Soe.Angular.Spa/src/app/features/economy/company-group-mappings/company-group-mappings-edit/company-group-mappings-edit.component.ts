import { Component, OnInit, inject } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CompanyGroupMappingHeadDTO } from '../models/company-group-mappings.model';
import { CompanyGroupMappingsService } from '../services/company-group-mappings.service';
import {
  addNumberValidator,
  CompanyGroupMappingHeadForm,
} from '../models/company-group-mappings-form.model';
import {
  Feature,
  SoeEntityState,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { BehaviorSubject, Observable, of, tap } from 'rxjs';
import { ICompanyGroupMappingRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CrudActionTypeEnum } from '@shared/enums';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-company-group-mappings-edit',
  templateUrl: './company-group-mappings-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CompanyGroupMappingsEditComponent
  extends EditBaseDirective<
    CompanyGroupMappingHeadDTO,
    CompanyGroupMappingsService,
    CompanyGroupMappingHeadForm
  >
  implements OnInit
{
  rowData = new BehaviorSubject<ICompanyGroupMappingRowDTO[]>([]);
  service = inject(CompanyGroupMappingsService);
  originalNumber: number | null = null;

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Economy_Accounting_CompanyGroup_TransferDefinitions);
  }

  override loadData(): Observable<void> {
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap((value: CompanyGroupMappingHeadDTO) => {
          this.rowData.next(value.rows);
          this.form?.customPatchValue(value);
          this.originalNumber = value.number;
        })
      )
    );
  }

  override newRecord(): Observable<void> {
    let clearValues = () => {};

    if (this.form?.isCopy) {
      clearValues = () => {
        this.form?.onDoCopy();
        this.rowData.next(this.form!.rows.value);
      };
    }

    return of(clearValues());
  }

  override performSave(): void {
    if (!this.form || this.form.invalid) return;

    const model = <CompanyGroupMappingHeadDTO>this.form?.getRawValue();
    model.rows = this.rowData.value;

    //Validate rows
    let rowsValid: boolean = true;
    this.rowData.value.forEach(row => {
      if (!row.childAccountFrom || !row.groupCompanyAccount) {
        rowsValid = false;
      }
    });

    if (rowsValid) {
      this.performAction.crud(
        CrudActionTypeEnum.Save,
        this.service.save(model).pipe(
          tap((data: BackendResponse) => {
            if (data.success) {
              this.originalNumber = model.number;
              this.updateFormValueAndEmitChange(data, true);
            }
          })
        )
      );
    } else {
      this.messageboxService.error(
        'core.warning',
        'economy.accounting.companygroup.validaterowsmessage'
      );
    }
  }

  addNewRow() {
    const accounts = this.rowData.value;
    accounts.push({
      childAccountFrom: 0,
      childAccountTo: 0,
      isModified: false,
      companyGroupMappingRowId: 0,
      companyGroupMappingHeadId: 0,
      groupCompanyAccount: 0,
      createdBy: '',
      modifiedBy: '',
      state: SoeEntityState.Active,
      rowNr: 0,
      isDeleted: false,
      isProcessed: false,
      childAccountFromName: '',
      childAccountToName: '',
      groupCompanyAccountName: '',
    });
    this.rowData.next(accounts);
    this.form?.patchAccounts(this.rowData.value);
    this.form?.markAsDirty();
  }

  validateNumber() {
    const numberControl = this.form?.get('number');
    if (!numberControl || !numberControl.value) return;

    if (this.originalNumber === numberControl.value) {
      this.form?.clearAsyncValidators();
      this.form?.updateValueAndValidity();
      return;
    }

    this.service
      .isCompanyGroupMappingHeadNumberExists(
        this.form?.companyGroupMappingHeadId.value,
        numberControl.value
      )
      .subscribe((isExist: boolean) => {
        if (isExist) {
          const errorTerm = this.translate.instant(
            'economy.accounting.companygroup.mapping.numberAlreadyExists'
          );
          this.form?.setAsyncValidators(addNumberValidator(errorTerm));
          this.messageboxService.error('core.warning', errorTerm);
        } else {
          this.form?.clearAsyncValidators();
        }
        this.form?.updateValueAndValidity();
      });
  }
}
