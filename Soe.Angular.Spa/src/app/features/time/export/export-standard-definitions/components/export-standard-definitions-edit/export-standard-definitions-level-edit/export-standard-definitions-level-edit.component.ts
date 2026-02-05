import { Component, Input, OnInit, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ExportDefinitionLevelDTO } from '../../../../../models/export.model';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { tap } from 'rxjs/operators';
import { ExportStandardDefinitionLevelForm } from '../../../models/export-standard-definition-level-form.model';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData } from '@ui/dialog/models/dialog';
import { TermCollection } from '@shared/localization/term-types';
import { Observable } from 'rxjs';

export interface IExportDefinitionLevelEventObject {
  object: ExportStandardDefinitionLevelForm | undefined;
  action: CrudActionTypeEnum;
}

export interface IExportStandardDefinitionsLevelDialogData extends DialogData {
  form: ExportStandardDefinitionLevelForm;
}

@Component({
    selector: 'soe-export-standard-definitions-level-edit',
    templateUrl: './export-standard-definitions-level-edit.component.html',
    providers: [FlowHandlerService],
    standalone: false
})
export class ExportStandardDefinitionsLevelEditComponent
  extends DialogComponent<IExportStandardDefinitionsLevelDialogData>
  implements OnInit
{
  @Input() form: ExportStandardDefinitionLevelForm | undefined;

  terms: TermCollection = {};
  idFieldName = '';

  // Variables for dirtyhandling and status
  dataLoaded = false;
  inProgress = signal(false);

  constructor(
    private translate: TranslateService,
    private validationHandler: ValidationHandler,
    public flowHandler: FlowHandlerService
  ) {
    super();
    this.form =
      this.data.form ?? this.createForm(new ExportDefinitionLevelDTO(), false);
    this.dataLoaded = true;
  }

  // INIT

  ngOnInit() {
    this.flowHandler.execute({
      permission: Feature.Time_Export_StandardDefinitions,
      lookups: [this.loadTerms()],
    });
  }

  createForm(
    element?: ExportDefinitionLevelDTO,
    setIdFieldName = true
  ): ExportStandardDefinitionLevelForm {
    const form = new ExportStandardDefinitionLevelForm({
      validationHandler: this.validationHandler,
      element,
    });
    if (setIdFieldName) this.idFieldName = form.getIdFieldName();
    return form;
  }

  // SERVICE CALLS

  private loadTerms(): Observable<TermCollection> {
    return this.translate.get(['core.deletewarning', 'core.delete']).pipe(
      tap(terms => {
        this.terms = terms;
      })
    );
  }

  // ACTIONS

  triggerEvent(action: CrudActionTypeEnum) {
    if (!this.form) return;
    this.dialogRef.close({ object: this.form, action: action });
  }

  performDelete() {
    this.triggerEvent(CrudActionTypeEnum.Delete);
  }

  performAdd() {
    this.triggerEvent(CrudActionTypeEnum.Save);
  }
}
