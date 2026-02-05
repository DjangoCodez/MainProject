import { Component, OnInit, inject, signal } from '@angular/core';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { ExtraFieldsService } from '../../services/extra-fields.service';
import { ExtraFieldForm } from '../../models/extra-fields-form.model';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import {
  CompTermsRecordType,
  SoeEntityState,
  SoeEntityType,
  TermGroup_ExtraFieldType,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable, of, tap } from 'rxjs';
import { ICompTermDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ProgressOptions } from '@shared/services/progress';
import { CompTermDTO } from '@shared/features/language-translations/models/language-translations.model';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  IExtraFieldDTO,
  IExtraFieldValueDTO,
} from '@shared/models/generated-interfaces/ExtraFieldDTO';
import { ExtraFieldsUrlParamsService } from '../../services/extra-fields-url.service';
import { ISysExtraFieldDTO } from '@shared/models/generated-interfaces/SysExtraFieldDTO';

@Component({
  selector: 'soe-extra-fields-edit',
  templateUrl: './extra-fields-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ExtraFieldsEditComponent
  extends EditBaseDirective<IExtraFieldDTO, ExtraFieldsService, ExtraFieldForm>
  implements OnInit
{
  service = inject(ExtraFieldsService);
  coreService = inject(CoreService);
  urlService = inject(ExtraFieldsUrlParamsService);

  isForAccountDim = signal(false);
  sysExtraFields = signal<ISysExtraFieldDTO[]>([]);
  showTranslationsPanel = signal(false);
  showSysEntityType = signal(false);

  // Values
  extraFieldValues: IExtraFieldValueDTO[] = [];
  extraFieldValuesGridDefined = false;
  extraFieldValuesFormPatched = false;

  // Translations
  compTermsRecordType: number = CompTermsRecordType.ExtraField;
  translations: ICompTermDTO[] = [];

  ngOnInit(): void {
    super.ngOnInit();

    if (this.urlService.entityType() == SoeEntityType.Account) {
      this.isForAccountDim.set(true);
    }

    this.startFlow(
      this.service.getPermission(this.urlService.entityType()),
      {}
    );

    if (this.form?.isNew) {
      this.form?.patchValue({
        entity: this.urlService.entityType(),
      });
      // always show translations panel for new records
      this.showTranslationsPanel.set(true);
    }

    if (this.isForAccountDim()) {
      this.form?.patchValue({
        connectedEntity: SoeEntityType.AccountDim,
      });
    } else {
      this.form?.controls.connectedRecordId.disable();
    }

    this.loadSysExtraFields().subscribe();
  }

  onValuesGridDefined() {
    // This method is called when the grid is defined
    // Depending on if the grid is defined or not when we load data,
    // we need to patch the form with the extra field values here or directly in the loadData method
    this.extraFieldValuesGridDefined = true;
    if (!this.extraFieldValuesFormPatched) {
      this.patchExtraFieldValues(this.extraFieldValues);
    }
  }

  loadSysExtraFields(): Observable<ISysExtraFieldDTO[]> {
    return this.performLoadData.load$(
      this.service.getSysExtraFields(this.urlService.entityType()).pipe(
        tap(value => {
          this.populateSysExtraFields(value);
        })
      )
    );
  }

  populateSysExtraFields(sysExtraFields: ISysExtraFieldDTO[]) {
    // add a empty item in sysExtraFields
    sysExtraFields.unshift(<ISysExtraFieldDTO>{
      sysExtraFieldId: 0,
      name: '',
      type: TermGroup_ExtraFieldType.FreeText,
    });
    this.sysExtraFields.set(sysExtraFields);
    this.showSysEntityType.set(
      sysExtraFields.length > 1 // more than the empty item
    );
  }

  populateExtraFields(sysExtraFieldId: number, setFields = true) {
    const sysExtraField = this.sysExtraFields().find(
      sef => sef.sysExtraFieldId === sysExtraFieldId
    );
    setTimeout(() => {
      if (sysExtraFieldId == 0 || !sysExtraField) {
        if (setFields) {
          this.form?.text.patchValue('');
          this.form?.type.patchValue(0);
        }
        this.form?.text.enable();
        this.form?.type.enable();
        this.showTranslationsPanel.set(true);
      } else {
        if (setFields) {
          this.form?.text.patchValue(sysExtraField?.name);
          this.form?.type.patchValue(sysExtraField?.type);
          this.form?.translations.clear();
        }
        this.form?.text.disable();
        this.form?.type.disable();
        this.showTranslationsPanel.set(false);
      }
      this.form?.sysExtraFieldId.patchValue(sysExtraFieldId);
      this.form?.updateValueAndValidity();
    }, 10);
  }

  override loadData(): Observable<void> {
    this.extraFieldValuesGridDefined = false;
    this.extraFieldValuesFormPatched = false;
    return this.performLoadData.load$(
      this.service.get(this.form?.getIdControl()?.value).pipe(
        tap(value => {
          this.form?.reset(value);
          this.form?.patchCompTerms(<CompTermDTO[]>value.translations);
          this.translations = value.translations;

          if (this.extraFieldValuesGridDefined) {
            // Grid already defined, patch the form
            this.patchExtraFieldValues(value.extraFieldValues);
          } else {
            // Grid not defined yet, save the values for later
            this.extraFieldValues = value.extraFieldValues;
          }
          this.populateExtraFields(this.form?.sysExtraFieldId.value, false);
        })
      )
    );
  }

  private patchExtraFieldValues(values: IExtraFieldValueDTO[]) {
    this.extraFieldValuesFormPatched = true;
    this.form?.patchExtraFieldValues(values);
  }

  override newRecord(): Observable<void> {
    let clearValues = () => {};

    if (this.form?.isCopy) {
      clearValues = () => {
        this.form?.onDoCopy();
        this.extraFieldValues = this.form?.extraFieldValues
          .value as IExtraFieldValueDTO[];
        this.translations = this.form?.translations.value as ICompTermDTO[];
      };
    }

    return of(clearValues());
  }

  override performSave(options?: ProgressOptions): void {
    this.form?.patchCompTerms(
      <CompTermDTO[]>(
        this.translations.filter(t => t.state !== SoeEntityState.Deleted)
      )
    );
    super.performSave(options);
  }
}
