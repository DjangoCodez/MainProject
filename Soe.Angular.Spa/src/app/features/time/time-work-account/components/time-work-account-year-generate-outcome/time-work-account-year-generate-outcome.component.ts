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
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ISelectableTimePeriodDTO } from '@shared/models/generated-interfaces/ReportDataDTO';
import { ITimeWorkAccountGenerateOutcomeModel } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { finalize, take, tap } from 'rxjs';
import { TimeWorkAccountGenerateOutcomeForm } from '../../models/time-work-account-year-generate-outcome-form.model';
import { TimeWorkAccountService } from '../../services/time-work-account.service';
import {
  ITimeWorkAccountOutputDialogData,
  TimeWorkAccountOutputComponent,
} from '../time-work-account-output/time-work-account-output.component';
import { FunctionType } from '../time-work-account-year-edit/time-work-account-year-edit.component';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

export interface ITimeWorkAcoountYearGenerateOutcome {
  timeWorkAccountYearId: number | undefined;
  action: CrudActionTypeEnum;
}
export interface ITimeWorkAcoountYearGenerateOutcomeDialogData
  extends DialogData {
  timeWorkAccountYearId: number | undefined;
  timeWorkAccountId: number | undefined;
  employeeLastDecidedDate: Date | undefined;
  timeWorkAccountYearEmployeeIds: number[] | undefined;
  form: TimeWorkAccountGenerateOutcomeForm | undefined;
  functionType: FunctionType | undefined;
}

@Component({
  selector: 'soe-time-work-account-year-generate-outcome',
  templateUrl: './time-work-account-year-generate-outcome.component.html',
  providers: [FlowHandlerService],
  standalone: false,
})
export class TimeWorkAcoountYearGenerateOutcomeComponent
  extends DialogComponent<ITimeWorkAcoountYearGenerateOutcomeDialogData>
  implements OnInit
{
  @Input() timeWorkAccountYearId: number | undefined;
  @Input() timeWorkAccounId: number | undefined;
  @Input() employeeLastDecidedDate: Date | undefined;
  @Input() timeWorkAccountYearEmployeeIds: number[] | undefined;
  @Input() form: TimeWorkAccountGenerateOutcomeForm | undefined;
  @Input() functionType: FunctionType | undefined;
  @Output() actionTaken = new EventEmitter<CrudActionTypeEnum>();

  performLoad = new Perform<any>(this.progressService);
  performAction = new Perform<TimeWorkAccountService>(this.progressService);

  // Lookups
  terms: any = [];

  public event: EventEmitter<ITimeWorkAcoountYearGenerateOutcome> =
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
  dialogServiceV2 = inject(DialogService);
  showCheckbox = false;

  constructor(
    private progressService: ProgressService,
    public flowHandler: FlowHandlerService,
    private validationHandler: ValidationHandler,
    private messageboxService: MessageboxService,
    private translate: TranslateService,
    private timeWorkAccountService: TimeWorkAccountService
  ) {
    super();
    this.setData();
  }

  ngOnInit() {
    this.flowHandler.execute({
      permission: Feature.Time_Payroll_TimeWorkAccount,
      lookups: [this.loadPeriods()],
    });
  }

  updateStatesAndEmitChange = (backendResponse: BackendResponse) => {
    if (backendResponse.success) {
      this.closeDialog();
    }
  };

  perform(): void {
    switch (this.data.functionType) {
      case FunctionType.GenerateOutcome:
        this.performGenerateOutcome();
        break;
      case FunctionType.GenerateUnusedPaidBalance:
        this.generateUnusedPaidBalance();
        break;
    }
  }

  performGenerateOutcome(): void {
    if (!this.form || this.form.invalid) return;

    if (this.form?.value.overrideChoosen) {
      const mb = this.messageboxService.warning(
        'time.payroll.worktimeaccount.employee.generateoutcome',
        'time.payroll.worktimeaccount.overridechoosenwarning'
      );
      mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
        if (response?.result) this.generateOutcome();
      });
    } else {
      this.generateOutcome();
    }
  }

  generateOutcome() {
    this.performLoad.load(
      this.timeWorkAccountService.generateOutcome(this.form?.value).pipe(
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
  generateUnusedPaidBalance() {
    this.performLoad.load(
      this.timeWorkAccountService
        .generateUnusedPaidBalance(this.form?.value)
        .pipe(
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
                    'time.payroll.worktimeaccount.employee.generateunpaidbalance'
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
    element: ITimeWorkAccountGenerateOutcomeModel
  ): TimeWorkAccountGenerateOutcomeForm {
    return new TimeWorkAccountGenerateOutcomeForm({
      validationHandler: this.validationHandler,
      element,
    });
  }

  setData() {
    const element = {
      timeWorkAccountYearId: this.data.timeWorkAccountYearId,
      timeWorkAccountId: this.data.timeWorkAccountId,
      overrideChoosen: false,
      timeWorkAccountYearEmployeeIds: this.data.timeWorkAccountYearEmployeeIds,
    } as ITimeWorkAccountGenerateOutcomeModel;

    if (
      this.data?.employeeLastDecidedDate &&
      this.data?.employeeLastDecidedDate <= new Date()
    )
      element.overrideChoosen = true;

    this.showCheckbox = this.data?.functionType == FunctionType.GenerateOutcome;

    this.form = this.createForm(element);
  }
  loadPeriods() {
    return this.timeWorkAccountService.getAllPayrollTimePeriods().pipe(
      tap(x => {
        this.allTimePeriods = x;
        this.allTimePeriods.forEach(p => {
          if (!this.paymentDates.find(f => f.date == p.paymentDate))
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
