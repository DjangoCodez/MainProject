import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TranslateService } from '@ngx-translate/core';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompTermsRecordType,
  ProductRowsContainers,
  SimpleTextEditorDialogMode,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { ICompTermDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { Observable, take, tap } from 'rxjs';
import { TextBlockDialogForm } from './models/text-block-dialog-form.model';
import {
  TextBlockDialogData,
  TextBlockModel,
  TextblockDTO,
} from './models/text-block-dialog.model';
import { TextBlockDialogService } from './services/text-block-dialog.service';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  templateUrl: './text-block-dialog.component.html',
  styleUrls: ['./text-block-dialog.component.scss'],
  providers: [FlowHandlerService],
  standalone: false,
})
export class TextBlockDialogComponent extends DialogComponent<TextBlockDialogData> {
  messageBoxDialog = inject(MessageboxService);
  validationHandler = inject(ValidationHandler);
  service = inject(TextBlockDialogService);
  progressService = inject(ProgressService);
  handler = inject(FlowHandlerService);
  coreService = inject(CoreService);
  translate = inject(TranslateService);

  terms: Record<string, string>[] = [];

  showOk = signal(false);
  okAlwaysEnabled = signal(false);
  showTitle = signal(false);
  showTemplatesSelect = signal(false);
  showTemplatesButtons = signal(false);
  showTranslations = signal(false);
  originalTextExists = signal(false);
  hasTitleValue = signal(false);
  hasTextValueEntered = signal(false);
  hasTextValue = computed(() => {
    return this.hasTextValueEntered() || this.originalTextExists();
  });
  saveTemplateValid = computed(() => {
    return this.hasTextValue() && this.hasTitleValue();
  });

  compTermsRecordType: number = CompTermsRecordType.Textblock;
  textBlocks: TextblockDTO[] = [];
  languages: SmallGenericType[] = [];
  template: SmallGenericType[] = [];
  translations: ICompTermDTO[] = [];
  templates: TextblockDTO[] = [];

  performLoadTextBlockTemplates = new Perform<TextblockDTO[]>(
    this.progressService
  );
  performAction = new Perform<any>(this.progressService);

  currentTextBlock = new TextBlockDialogData();
  formTextBlock: TextBlockDialogForm = new TextBlockDialogForm({
    validationHandler: this.validationHandler,
    element: new TextblockDTO(),
  });

  constructor() {
    super();
    this.setDialogParam();
    this.handler.execute({
      lookups: [
        this.loadTextBlockTemplates(),
        this.loadLanguages(),
        this.loadTextBlocks(),
      ],
    });
    this.originalTextExists.set(!!this.data.text && this.data.text.length > 0);
    this.setupGui();

    this.formTextBlock?.headline.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((title?: string) => {
        this.hasTitleValue.set(!!title && title?.length > 0);
      });

    this.formTextBlock?.text.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((text?: string) => {
        this.hasTextValueEntered.set(!!text && text?.length > 0);
      });
  }

  loadTextBlockTemplates(): Observable<TextblockDTO[]> {
    return this.performLoadTextBlockTemplates.load$(
      this.service.getAll(this.data.entity).pipe(
        tap(data => {
          this.templates = [];
          const emptyTextBlock = new TextblockDTO();
          emptyTextBlock.textblockId = 0;
          emptyTextBlock.headline = '';
          emptyTextBlock.text = '';

          this.templates = [
            emptyTextBlock,
            ...data.filter(e => e.showInPurchase === true),
          ];
        })
      )
    );
  }

  setupGui() {
    switch (this.data.mode) {
      case SimpleTextEditorDialogMode.Base:
      case SimpleTextEditorDialogMode.EditInvoiceText:
      case SimpleTextEditorDialogMode.AddTextblockToInvoiceRow:
      case SimpleTextEditorDialogMode.EditInvoiceDescription: {
        this.showOk.set(this.data.editPermission);
        this.showTitle.set(false);
        this.showTemplatesSelect.set(true);
        this.showTemplatesButtons.set(false);
        this.showTranslations.set(true);
        break;
      }
      case SimpleTextEditorDialogMode.EditInvoiceRowText: {
        this.showOk.set(this.data.editPermission);
        this.showTitle.set(true);
        this.showTemplatesSelect.set(true);
        this.showTemplatesButtons.set(true);
        this.showTranslations.set(false);
        break;
      }
      case SimpleTextEditorDialogMode.EditTextBlock:
      case SimpleTextEditorDialogMode.NewTextBlock: {
        this.showOk.set(false);
        this.showTitle.set(true);
        this.showTemplatesSelect.set(false);
        this.showTemplatesButtons.set(true);
        this.showTranslations.set(true);
        break;
      }
      case SimpleTextEditorDialogMode.EditWorkingDescription: {
        this.showOk.set(this.data.editPermission);
        this.showTitle.set(true);
        this.showTemplatesSelect.set(this.data.editPermission);
        this.showTemplatesButtons.set(this.data.editPermission);
        this.showTranslations.set(false);

        break;
      }
      case SimpleTextEditorDialogMode.AddSupplierInvoiceBlockReason: {
        this.showOk.set(true);
        this.okAlwaysEnabled.set(true);
        this.showTitle.set(false);
        this.showTemplatesSelect.set(false);
        this.showTemplatesButtons.set(false);
        this.showTranslations.set(false);
        !this.data.editPermission
          ? this.formTextBlock?.text.disable()
          : this.formTextBlock?.text.enable();
        break;
      }
    }
  }

  loadLanguages(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.Language, false, false)
      .pipe(
        tap(term => {
          this.languages = term;
        })
      );
  }

  loadTextBlocks(): Observable<TextblockDTO[]> {
    return this.service.getAll(this.data.entity).pipe(
      tap(temp => {
        if (this.data.container == ProductRowsContainers.Contract)
          this.textBlocks = temp.filter(
            i => i.type === this.data.type && i.showInContract === true
          );
        else if (this.data.container == ProductRowsContainers.Offer)
          this.textBlocks = temp.filter(
            i => i.type === this.data.type && i.showInOffer === true
          );
        else if (this.data.container == ProductRowsContainers.Order)
          this.textBlocks = temp.filter(
            i => i.type === this.data.type && i.showInOrder === true
          );
        else if (this.data.container == ProductRowsContainers.Invoice)
          this.textBlocks = temp.filter(
            i => i.type === this.data.type && i.showInInvoice === true
          );
        else if (this.data.container == ProductRowsContainers.Purchase)
          this.textBlocks = temp.filter(
            i => i.type === this.data.type && i.showInPurchase === true
          );
        else this.textBlocks = temp.filter(i => i.type === this.data.type);
      })
    );
  }

  showExistingTemplateWarning() {
    const keys: string[] = [
      'core.warning',
      'billing.order.textblock.existingtemplatewarning',
    ];
    this.translate
      .get(keys)
      .pipe(take(1))
      .subscribe((terms: any) => {
        this.messageBoxDialog.warning(
          terms['core.warning'],
          terms['billing.order.textblock.existingtemplatewarning']
        );
      });
  }
  setDialogParam() {
    if (this.data) {
      if (this.data.headline) {
        this.formTextBlock.patchValue({
          headline: this.data.headline,
        });
      }
      if (this.data.text) {
        this.formTextBlock.patchValue({
          text: this.data.text,
        });
      }
    }
  }

  setTextValue() {
    this.dialogRef.close(this.formTextBlock.value.text);
  }

  performDelete(): void {
    this.performAction.crud(
      CrudActionTypeEnum.Delete,
      this.service.delete(this.formTextBlock.textBlockId.value),
      returnValue => this.checkTemplateDeleted(returnValue)
    );
  }

  checkTemplateDeleted(returnValue: any) {
    if (returnValue?.success) {
      this.formTextBlock.patchValue({
        textBlockId: undefined,
        headline: '',
        text: '',
      });
      this.loadTextBlockTemplates().subscribe();
    }
  }

  performSave(): void {
    const model = new TextBlockModel();
    model.textBlock = <TextblockDTO>this.formTextBlock.getRawValue();
    model.entity = this.data.entity;
    model.translations = this.translations;
    if (model.textBlock.textblockId == 0) {
      model.textBlock.showInContract = true;
      model.textBlock.showInInvoice = true;
      model.textBlock.showInOffer = true;
      model.textBlock.showInOrder = true;
      model.textBlock.showInPurchase = true;
      const existingTemp = this.templates.filter(
        x => x.headline.toLowerCase() == model.textBlock.headline.toLowerCase()
      );
      if (existingTemp.length > 0) {
        this.showExistingTemplateWarning();
        return;
      }
    }

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service.save(model),
      returnValue => {
        this.reloadTemplate(returnValue);
      }
    );
  }

  reloadTemplate(returnValue: any) {
    if (returnValue?.success) {
      this.loadTextBlockTemplates().subscribe(data => {
        const saveObj = data.find(
          template =>
            template.textblockId === ResponseUtil.getEntityId(returnValue)
        );

        if (saveObj) {
          setTimeout(() => {
            this.formTextBlock.patchValue(
              {
                textBlockId: saveObj.textblockId,
              },
              {
                emitEvent: true,
              }
            );
          }, 500);
        }
      });
    }
  }

  cancel() {
    this.dialogRef.close(false);
  }

  templateOnChange(templateId: number) {
    const template = this.templates.find(f => f.textblockId == templateId);

    this.formTextBlock.patchValue({
      headline: template?.headline,
      text: template?.text,
      showInContract: template?.showInContract,
      showInOffer: template?.showInOffer,
      showInOrder: template?.showInOrder,
      showInInvoice: template?.showInInvoice,
    });
  }
}
