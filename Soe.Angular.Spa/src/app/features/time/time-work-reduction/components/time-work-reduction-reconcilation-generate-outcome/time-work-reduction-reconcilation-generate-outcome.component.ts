import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject,
} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import {
  CompanySettingType,
  Feature,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISelectableTimePeriodDTO } from '@shared/models/generated-interfaces/ReportDataDTO';
import { ITimeWorkReductionReconciliationGenerateOutcomeModel } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { finalize, Observable, take, tap } from 'rxjs';

import { TimeWorkReductionService } from '../../services/time-work-reduction.service';
import { TimeWorkAccountService } from '@features/time/time-work-account/services/time-work-account.service';
import {
  ITimeWorkAccountOutputDialogData,
  TimeWorkAccountOutputComponent,
} from '@features/time/time-work-account/components/time-work-account-output/time-work-account-output.component';
import { TimeWorkReducionGenerateOutcomeForm } from '../../models/time-work-reduction-form-year-generate-outcome-form.model';
import { CoreService } from '@shared/services/core.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

export interface ITimeWorkReductionReconcilationGenerateOutcome {
  timeWorkReductionReconciliationYearId: number | undefined;
  action: CrudActionTypeEnum;
}
export interface ITimeWorkReductionReconcilationGenerateOutcomeDialogData
  extends DialogData {
  timeWorkReductionReconciliationYearId: number | undefined;
  timeWorkReductionReconciliationId: number | undefined;
  employeeLastDecidedDate: Date | undefined;
  stop: Date | undefined;
  timeWorkReductionReconciliationEmployeeIds: number[] | undefined;
  form: TimeWorkReducionGenerateOutcomeForm | undefined;
}

@Component({
  selector: 'soe-time-work-reduction-reconcilation--year-generate-outcome',
  templateUrl:
    './time-work-reduction-reconcilation-generate-outcome.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class TimeWorkReductionReconcilationGenerateOutcome
  extends DialogComponent<ITimeWorkReductionReconcilationGenerateOutcomeDialogData>
  implements OnInit
{
  @Input() timeWorkReductionReconciliationYearId: number | undefined;
  @Input() timeWorkReductionReconciliationId: number | undefined;
  @Input() employeeLastDecidedDate: Date | undefined;
  @Input() stop: Date | undefined;
  @Input() timeWorkReductionReconciliationEmployeeIds: number[] | undefined;
  @Input() form: TimeWorkReducionGenerateOutcomeForm | undefined;
  @Output() actionTaken = new EventEmitter<CrudActionTypeEnum>();

  performLoad = new Perform<any>(this.progressService);
  performAction = new Perform<TimeWorkReductionService>(this.progressService);

  // Lookups
  terms: any = [];

  public event: EventEmitter<ITimeWorkReductionReconcilationGenerateOutcome> =
    new EventEmitter();

  get currentLanguage(): string {
    return SoeConfigUtil.language;
  }

  output: any[] = [];
  title = '';
  headerItems: any[] = [];
  headerSize = 0;
  allTimePeriods: ISelectableTimePeriodDTO[] = [];
  paymentDates: any[] = [];
  usePayroll = false;
  dialogServiceV2 = inject(DialogService);
  coreService = inject(CoreService);

  constructor(
    private progressService: ProgressService,
    public flowHandler: FlowHandlerService,
    private validationHandler: ValidationHandler,
    private messageboxService: MessageboxService,
    private translate: TranslateService,
    private timeWorkReductionService: TimeWorkReductionService,
    private timeWorkAccountService: TimeWorkAccountService
  ) {
    super();
    this.setData();
  }

  ngOnInit() {
    this.flowHandler.execute({
      permission: Feature.Time_Time_TimeWorkReduction,
      lookups: [this.loadCompanySettings(), this.loadPeriods()],
    });
  }

  updateStatesAndEmitChange = (backendResponse: BackendResponse) => {
    if (backendResponse.success) {
      this.closeDialog();
    }
  };

  perform(): void {
    if (!this.form || (this.form.invalid && this.usePayroll)) return;

    if (this.form?.value.overrideChoosen) {
      const mb = this.messageboxService.warning(
        'time.payroll.worktimeaccount.employee.generateoutcome',
        'time.time.timeworkreduction.overridechoosenwarning'
      );
      mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
        if (response?.result) this.generateOutcome();
      });
    } else {
      this.generateOutcome();
    }
  }
  loadCompanySettings(): Observable<void> {
    const settingTypes: number[] = [CompanySettingType.UsePayroll];
    return this.coreService.getCompanySettings(settingTypes).pipe(
      tap((settings: any) => {
        this.usePayroll = settings[CompanySettingType.UsePayroll];
      })
    );
  }
  generateOutcome() {
    this.performLoad.load(
      this.timeWorkReductionService.generateOutcome(this.form?.value).pipe(
        finalize(() => {
          if (!this.performLoad.data?.result.success) {
            this.showError(this.performLoad.data?.result);
          } else {
            const row: object[] = [];
            const header: object[] = [];
            header.push(
              { header: this.translate.instant('common.employee') },
              {
                header: this.translate.instant(
                  'time.payroll.worktimeaccount.employee.selectedwithdrawalmethod'
                ),
              },
              { header: this.translate.instant('core.info') }
            );
            this.performLoad.data?.rows.forEach(
              (res: {
                employeeNrAndName: string;
                methodName: string;
                codeName: string;
              }) => {
                row.push({
                  text: [res.employeeNrAndName, res.methodName, res.codeName],
                });
              }
            );
            this.dialogServiceV2
              .open(TimeWorkAccountOutputComponent, {
                title: this.translate.instant(
                  'time.payroll.worktimeaccount.employee.generateoutcome'
                ),
                row: row,
                header: header,
                size: 'lg',
                hideFooter: true,
                export: true,
              } as ITimeWorkAccountOutputDialogData)
              .afterClosed()
              .pipe(take(1))
              .subscribe(() => {
                this.closeDialog();
              });
          }
        })
      )
    );
  }

  showError(result: any) {
    this.messageboxService.error(
      this.translate.instant('core.error'),
      result.errorMessage
    );
  }

  createForm(
    element: ITimeWorkReductionReconciliationGenerateOutcomeModel
  ): TimeWorkReducionGenerateOutcomeForm {
    return new TimeWorkReducionGenerateOutcomeForm({
      validationHandler: this.validationHandler,
      element,
    });
  }

  setData() {
    const element = {
      timeWorkReductionReconciliationYearId:
        this.data.timeWorkReductionReconciliationYearId,
      timeWorkReductionReconciliationId:
        this.data.timeWorkReductionReconciliationId,
      overrideChoosen: true,
      timeWorkReductionReconciliationEmployeeIds:
        this.data.timeWorkReductionReconciliationEmployeeIds,
    } as ITimeWorkReductionReconciliationGenerateOutcomeModel;

    if (
      this.data?.employeeLastDecidedDate &&
      this.data?.employeeLastDecidedDate <= new Date()
    )
      element.overrideChoosen = true;

    this.form = this.createForm(element);
  }
  loadPeriods() {
    return this.timeWorkAccountService.getAllPayrollTimePeriods().pipe(
      tap(x => {
        this.allTimePeriods = x;
        this.allTimePeriods.forEach(p => {
          if (
            !this.paymentDates.find(
              f => f.date.toString() == p.paymentDate.toString()
            )
          )
            this.paymentDates.push({
              id: p.id,
              date: p.paymentDate,
              name: p.paymentDate.toFormattedDate(),
            });
        });
      })
    );
  }
  updateDate() {
    this.form?.PaymentDate.patchValue(
      this.paymentDates.find(f => f.id == this.form?.PaymentDateId.value)?.date
    );
  }
}
