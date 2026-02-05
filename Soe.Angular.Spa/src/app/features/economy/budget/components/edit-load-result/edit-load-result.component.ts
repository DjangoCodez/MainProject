import { Component, inject, signal } from '@angular/core';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import {
  EditLoadResultDTO,
  LoadResultDialogData,
} from './models/edit-load-result.model';
import { ValidationHandler } from '@shared/handlers';
import { EditLoadResultForm } from './models/edit-load-result-form.model';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'soe-edit-load-result',
  templateUrl: './edit-load-result.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class EditLoadResultComponent extends DialogComponent<LoadResultDialogData> {
  loadPrevious: boolean = false;
  validationHandler = inject(ValidationHandler);
  translate = inject(TranslateService);
  useDim2 = signal(false);
  useDim3 = signal(false);
  messageLoadResults = signal<string>(
    this.translate.instant('economy.accounting.budget.getresultheadertext')
  );
  messageResultInfoText = signal<string>(
    this.translate.instant('economy.accounting.budget.getresultinfotext')
  );
  messageIncludeAcountDim = signal<string>(
    this.translate.instant('economy.accounting.budget.includecountdim')
  );

  form: EditLoadResultForm = new EditLoadResultForm({
    validationHandler: this.validationHandler,
    element: new EditLoadResultDTO(),
  });

  constructor() {
    super();
    this.data.title = this.messageLoadResults();
  }

  closeDialog() {
    this.dialogRef.close(undefined);
  }

  buttonYesClick() {
    this.data.useDim2=this.form?.useDim2.value;
    this.data.useDim3=this.form?.useDim3.value;
    this.dialogRef.close(this.data);
  }

}
