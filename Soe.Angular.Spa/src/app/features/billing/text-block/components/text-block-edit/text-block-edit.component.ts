import { Component, inject, OnInit } from '@angular/core';
import { TextBlockModel } from '../../models/text-block.model';
import { TextBlockService } from '../../services/text-block.service';
import { TextBlockForm } from '../../models/text-block-form.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { LanguageTranslationsService } from '@shared/features/language-translations/services/language-translations.service';
import {
  CompTermsRecordType,
  Feature,
  SoeEntityState,
  SoeEntityType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable, of, tap } from 'rxjs';
import { ICompTermDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CrudActionTypeEnum } from '@shared/enums';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-text-block-edit',
  templateUrl: './text-block-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TextBlockEditComponent
  extends EditBaseDirective<TextBlockModel, TextBlockService, TextBlockForm>
  implements OnInit
{
  compTermsRecordType: number = CompTermsRecordType.Textblock;
  translations: ICompTermDTO[] = [];
  textBlockTypes: ISmallGenericType[] = [];

  coreService = inject(CoreService);
  service = inject(TextBlockService);
  translationsService = inject(LanguageTranslationsService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(Feature.Billing_Preferences_Textblock_Edit, {
      lookups: [this.loadTextBlockTypes()],
    });
  }

  override newRecord(): Observable<void> {
    let clearValues = () => {};

    if (this.form?.isCopy) {
      clearValues = () => {
        this.form?.onDoCopy();
        this.translations = this.form?.translations.value as ICompTermDTO[];
      };
    } else this.getTranslations();

    return of(clearValues());
  }

  private loadTextBlockTypes(): Observable<ISmallGenericType[]> {
    this.textBlockTypes = [];
    return this.coreService
      .getTermGroupContent(TermGroup.TextBlockType, false, true)
      .pipe(
        tap(x => {
          this.textBlockTypes = x;
        })
      );
  }

  getTranslations() {
    this.performLoadData.load(
      this.translationsService
        .getTranslations(
          this.compTermsRecordType,
          this.form?.textblockId.value,
          true
        )
        .pipe(
          tap(value => {
            this.translations = value;
            this.form?.patchCompTerms(value);
          })
        )
    );
  }

  override loadData(): Observable<void> {
    return super.loadData().pipe(tap(() => this.getTranslations()));
  }

  override performSave(): void {
    if (!this.form || this.form.invalid || !this.service) return;

    const model = new TextBlockModel();
    model.textBlock = this.form.value;
    model.entity = SoeEntityType.CustomerInvoice;
    model.translations = this.translations.filter(
      t => t.state !== SoeEntityState.Deleted
    );

    this.form?.patchCompTerms(model.translations);

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model).pipe(tap(this.updateFormValueAndEmitChange))
    );
  }
}
