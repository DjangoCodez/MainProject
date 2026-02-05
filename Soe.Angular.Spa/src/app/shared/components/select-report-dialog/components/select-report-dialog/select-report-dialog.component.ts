import { Component, inject } from '@angular/core';
import {
  SoeModule,
  SoeReportTemplateType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CoreService } from '@shared/services/core.service'
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class'
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { Observable, tap } from 'rxjs';
import {
  GetReportsForTypesModel,
  ReportViewDTO,
  SelectReportDialogCloseData,
  SelectReportDialogData,
  SelectReportDialogFormDTO,
} from '../../models/select-report-dialog.model';
import { ReportService } from '@shared/services/report.service';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { SelectReportDialogForm } from '../../models/select-report-dialog-form.model';
import { ValidationHandler } from '@shared/handlers';
import { orderBy } from 'lodash';

@Component({
  selector: 'soe-select-report-dialog',
  templateUrl: './select-report-dialog.component.html',
  styleUrls: ['./select-report-dialog.component.scss'],
  providers: [FlowHandlerService],
  standalone: false,
})
export class SelectReportDialogComponent extends DialogComponent<SelectReportDialogData> {
  langId!: number;
  reports!: ReportViewDTO[];
  module?: SoeModule;
  reportTypes!: SoeReportTemplateType[];
  defaultReportId!: number;
  showCopy!: boolean;
  showEmail!: boolean;

  showReminder!: boolean;
  showLangSelection = true;
  showSavePrintout!: boolean;
  savePrintout!: boolean;

  coreService = inject(CoreService);
  progressService = inject(ProgressService);
  reportService = inject(ReportService);
  validationHandler = inject(ValidationHandler);

  languages: ISmallGenericType[] = [];

  performLanguageLoad = new Perform<ISmallGenericType[]>(this.progressService);
  performReportLoad = new Perform<any>(this.progressService);

  form: SelectReportDialogForm = new SelectReportDialogForm({
    validationHandler: this.validationHandler,
    element: new SelectReportDialogFormDTO(),
  });
  constructor(public handler: FlowHandlerService) {
    super();
    this.handler.execute({
      lookups: [this.loadLanguages()],
      onFinished: this.finished.bind(this),
    });
    this.setDialogParam();
  }

  setDialogParam() {
    if (this.data) {
      if (this.data.langId) {
        this.langId = this.data.langId;
      }
      if (this.data.reports) {
        this.reports = this.data.reports;
      }
      if (this.data.module) {
        this.module = this.data.module;
      }
      if (this.data.reportTypes) {
        this.reportTypes = this.data.reportTypes;
      }
      if (this.data.defaultReportId) {
        this.defaultReportId = this.data.defaultReportId;
      }
      this.showCopy = !!this.data.showCopy;
      this.showEmail = !!this.data.showEmail;
      if (this.data.copyValue) {
        this.form.patchValue({
          isReportCopy: this.data.copyValue,
        });
      }
      this.showReminder = !!this.data.showReminder;
      this.showLangSelection = !!this.data.showLangSelection;
      this.showSavePrintout = !!this.data.showSavePrintout;
      this.savePrintout = !!this.data.savePrintout;
      this.form.patchValue({
        savePrintout: !!this.data.savePrintout,
      });
    }
  }

  loadLanguages(): Observable<ISmallGenericType[]> {
    return this.performLanguageLoad.load$(
      this.coreService
        .getTermGroupContent(TermGroup.Language, true, false)
        .pipe(
          tap(data => {
            this.languages = data;
            if (this.langId || this.langId === 0) {
              this.form.patchValue({
                languageId: this.langId
                  ? this.langId
                  : SoeConfigUtil.sysCountryId,
              });
            }
          })
        )
    );
  }

  finished() {
    this.loadReports();
  }

  loadReports() {
    if (!this.reports || this.reports.length === 0) {
      const model = new GetReportsForTypesModel(
        this.reportTypes,
        true,
        false,
        this.module
      );
      this.performReportLoad.load(
        this.reportService.getReportsForType(model).pipe(
          tap(data => {
            this.reports = orderBy(data, ['reportName'], ['asc']);
            this.setDefaultReport();
          })
        )
      );
    } else {
      this.setDefaultReport();
    }
  }

  setDefaultReport() {
    if (this.defaultReportId) {
      const report = this.reports.find(
        f => f.reportId === this.defaultReportId
      );

      if (report) report.default = true;
    }
  }

  printClick(report: ReportViewDTO) {
    this.close(report, false);
  }

  emailClick(report: ReportViewDTO) {
    this.close(report, true);
  }

  cancel() {
    this.dialogRef.close(null);
  }

  close(report: ReportViewDTO, email: boolean) {
    if (!report) {
      this.cancel();
    } else {
      this.dialogRef.close(
        new SelectReportDialogCloseData(
          report.reportId,
          report.sysReportTemplateTypeId,
          this.form.value.languageId,
          this.form.value.isReportCopy,
          email,
          this.form.value.isReminder,
          this.form.value.savePrintout,
          report.employeeTemplateId
        )
      );
    }
  }
}
