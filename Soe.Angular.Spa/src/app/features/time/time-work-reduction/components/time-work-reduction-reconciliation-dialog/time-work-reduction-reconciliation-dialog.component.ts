import {
  Component,
  inject,
  OnInit,
  ViewChild,
  AfterViewInit,
  AfterViewChecked,
} from '@angular/core';
import { ValidationHandler } from '@shared/handlers';

import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

import { TranslateService } from '@ngx-translate/core';
import {
  Feature,
  TermGroup_TimeWorkReductionReconciliationEmployeeStatus,
  TermGroup_TimeWorkReductionWithdrawalMethod,
} from '@shared/models/generated-interfaces/Enumerations';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  ITimeWorkReductionReconciliationEmployeeDTO,
  ITimeWorkReductionReconciliationEmployeeModel,
  ITimeWorkReductionReconciliationGenerateOutcomeModel,
  ITimeWorkReductionReconciliationYearDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { TimeWorkReductionService } from '../../services/time-work-reduction.service';
import { TimeWorkReductionYearForm } from '../../models/time-work-reduction-year-form.model';
import { CrudActionTypeEnum } from '@shared/enums';
import { BehaviorSubject, of, take, tap } from 'rxjs';
import {
  ITimeWorkAccountOutputDialogData,
  TimeWorkAccountOutputComponent,
} from '@features/time/time-work-account/components/time-work-account-output/time-work-account-output.component';
import {
  ITimeWorkReductionReconcilationGenerateOutcomeDialogData,
  TimeWorkReductionReconcilationGenerateOutcome,
} from '../time-work-reduction-reconcilation-generate-outcome/time-work-reduction-reconcilation-generate-outcome.component';
import { Perform } from '@shared/util/perform.class';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

export enum FunctionType {
  CalculateOutcome = 1,
  ExportCalculateOutcome = 2,
  GenerateOutcome = 3,
  ExportPension = 4,
  ReverseTransaction = 5,
}

export interface ITimeWorkReductionReconciliationDialogData extends DialogData {
  new: boolean;
  id: number;
  usePension: boolean;
  useDirectPayment: boolean;
  defaultPaidLeaveNotUsed: TermGroup_TimeWorkReductionWithdrawalMethod;
  pensionPayrollProducts: SmallGenericType[];
  directPaymentpayrollProducts: SmallGenericType[];
  row: ITimeWorkReductionReconciliationYearDTO;
  timeWorkReductionReconciliationId: number;
  latestYear: ITimeWorkReductionReconciliationYearDTO | undefined;
  withdrawalMethods: SmallGenericType[];
  employeeStatus: SmallGenericType[];
}

@Component({
  selector: 'soe-time-work-reduction-reconciliation-dialog',
  templateUrl: './time-work-reduction-reconciliation.component.html',
  providers: [FlowHandlerService, DialogService, ToolbarService],
  standalone: false,
})
export class TimeWorkReductionReconciliationDialogComponent
  extends DialogComponent<ITimeWorkReductionReconciliationDialogData>
  implements OnInit, AfterViewInit, AfterViewChecked
{
  validationHandler = inject(ValidationHandler);
  translateService = inject(TranslateService);
  handler = inject(FlowHandlerService);
  progressService = inject(ProgressService);
  service = inject(TimeWorkReductionService);
  messageboxService = inject(MessageboxService);
  toasterService = inject(ToasterService);
  coreService = inject(CoreService);
  dialogServiceV2 = inject(DialogService);

  performLoad = new Perform<any>(this.progressService);
  performAction = new Perform<TimeWorkReductionService>(this.progressService);
  form: TimeWorkReductionYearForm | undefined;
  idFieldName = '';
  gridInitialized = false;
  pensionPayrollProducts: SmallGenericType[] = [];
  directPaymentpayrollProducts: SmallGenericType[] = [];
  usePension: boolean = false;
  useDirectPayment: boolean = false;
  withdrawalMethods: SmallGenericType[] = [];
  rows: ITimeWorkReductionReconciliationEmployeeDTO[] = [];
  employeeStatus: SmallGenericType[] = [];
  highestStatus = 0;
  lowestStatus = 10;
  hasPension = false;

  menuList: MenuButtonItem[] = [];

  constructor(private translate: TranslateService) {
    super();
    this.pensionPayrollProducts = this.data.pensionPayrollProducts;
    this.directPaymentpayrollProducts = this.data.directPaymentpayrollProducts;
    this.usePension = this.data.usePension;
    this.useDirectPayment = this.data.useDirectPayment;
    this.withdrawalMethods = this.data.withdrawalMethods;
    this.employeeStatus = this.data.employeeStatus;
  }

  // Subgrid
  @ViewChild('subGrid', { static: false })
  subGrid!: GridComponent<ITimeWorkReductionReconciliationEmployeeDTO>;
  subData = new BehaviorSubject<ITimeWorkReductionReconciliationEmployeeDTO[]>(
    []
  );

  ngAfterViewInit() {
    if (this.subGrid) {
      this.setupRowsGrid(this.subGrid);
    }
  }
  ngOnInit() {
    this.handler.execute({
      permission: Feature.Time_Time_TimeWorkReduction,
      lookups: [],
    });

    this.setData(this.data.new);
  }
  ngAfterViewChecked() {
    if (this.subGrid && !this.gridInitialized) {
      this.gridInitialized = true;
    }
  }
  createForm(
    element: ITimeWorkReductionReconciliationYearDTO,
    setIdFieldName = true
  ): TimeWorkReductionYearForm {
    const form = new TimeWorkReductionYearForm({
      validationHandler: this.validationHandler,
      element,
    });
    if (setIdFieldName) this.idFieldName = form.getIdFieldName();

    return form;
  }
  updateFormValueAndEmitChange = (
    crud: BackendResponse,
    close: boolean = false
  ) => {
    if (crud.success) {
      this.form?.patchValue({
        [this.idFieldName]: ResponseUtil.getEntityId(crud),
      });
      this.triggerEvent(this.form, CrudActionTypeEnum.Save);
      if (close) this.closeDialog();
      else this.loadEmployees().subscribe(() => {});
    }
  };

  onCellValueChanged(): void {
    this.form?.patchYearEmployees(this.subData.value);
    this.form?.markAsDirty();
  }
  triggerEvent(
    item: TimeWorkReductionYearForm | undefined,
    action: CrudActionTypeEnum
  ) {
    if (!this.form?.isNew && action != CrudActionTypeEnum.Delete) {
      this.loadEmployees();
      this.form?.markAsPristine();
    } else this.dialogRef.close({ object: item, action });
  }
  cancel() {
    this.dialogRef.close();
  }

  protected ok(): void {
    this.dialogRef.close(this.form?.value);
  }

  setData(newYear = false) {
    let x;
    if (newYear) {
      x = {
        timeWorkReductionReconciliationYearId: 0,
        timeWorkReductionReconciliationId:
          this.data.timeWorkReductionReconciliationId,
        stop: this.data.latestYear?.stop
          ? new Date(
              new Date(this.data.latestYear.stop).setFullYear(
                new Date(this.data.latestYear.stop).getFullYear() + 1
              )
            )
          : new Date(),
        employeeLastDecidedDate: this.data.latestYear?.employeeLastDecidedDate
          ? new Date(
              new Date(
                this.data.latestYear?.employeeLastDecidedDate
              ).setFullYear(
                new Date(
                  this.data.latestYear?.employeeLastDecidedDate
                ).getFullYear() + 1
              )
            )
          : new Date(),
        state: 0,
        timeWorkReductionReconciliationEmployeeDTO: [],
        pensionDepositPayrollProductId: this.usePension
          ? this.data.latestYear?.pensionDepositPayrollProductId
          : 0,
        directPaymentPayrollProductId: this.useDirectPayment
          ? this.data.latestYear?.directPaymentPayrollProductId
          : 0,
      };
    } else {
      x = this.data.row;
    }
    this.form = this.createForm(x, true);
    this.form?.customPatchValue(x);
    this.rows = x.timeWorkReductionReconciliationEmployeeDTO;
    this.form.isNew = newYear;
    if (newYear) this.form.markAsDirty();
    else this.form.markAsPristine();

    this.form.updateValueAndValidity();
    this.setupRows();

    this.loadEmployees().subscribe(() => {});
  }
  private setupRows() {
    if (!this.rows) return;

    this.rows.forEach(x => {});

    this.subData.next(this.rows);

    this.form?.patchYearEmployees(this.subData.value);
  }

  setupRowsGrid(
    grid: GridComponent<ITimeWorkReductionReconciliationEmployeeDTO>
  ) {
    this.subGrid = grid;
    this.translate
      .get([
        'common.employee',
        'time.time.timeworkreduction.minutesoverthreshold',
        'time.payroll.worktimeaccount.employee.sendselectionsuccess',
        'time.payroll.worktimeaccount.employee.selectedwithdrawalmethod',
        'time.payroll.worktimeaccount.employee.selecteddate',
        'time.time.timeworkreduction.accearningminutes',
        'time.time.timeaccumulator.employeegrouprule.thresholdMinutes',
        'core.fileupload.status',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.subGrid.addColumnText(
          'employeeNrAndName',
          terms['common.employee'],
          {
            flex: 20,
            editable: false,
          }
        );

        this.subGrid.addColumnSelect(
          'selectedWithdrawalMethod',
          terms[
            'time.payroll.worktimeaccount.employee.selectedwithdrawalmethod'
          ],
          this.withdrawalMethods || [],
          undefined,
          {
            flex: 10,
            editable: row => {
              return this.setDisabled(
                row.data,
                TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome
              );
            },
          }
        );

        this.subGrid.addColumnTimeSpan(
          'accEarningMinutes',
          terms['time.time.timeworkreduction.accearningminutes'],
          {
            flex: 10,
            editable: false,
          }
        );
        this.subGrid.addColumnTimeSpan(
          'threshold',
          terms['time.time.timeaccumulator.employeegrouprule.thresholdMinutes'],
          {
            flex: 10,
            editable: false,
          }
        );
        this.subGrid.addColumnTimeSpan(
          'minutesOverThreshold',
          terms['time.time.timeworkreduction.minutesoverthreshold'],
          {
            flex: 10,
            editable: false,
          }
        );
        this.subGrid.addColumnSelect(
          'status',
          terms['core.fileupload.status'],
          this.employeeStatus,
          null,
          {
            flex: 10,
            editable: false,
          }
        );

        this.subGrid.setRowSelection('multiRow');
        this.subGrid.alwaysShowVerticalScroll();
        this.subGrid.columns.forEach(col => {
          col.floatingFilter = true;
          col.suppressFiltersToolPanel = true;
          col.sortable = true;
        });
        this.subGrid.finalizeInitGrid();
      });
  }

  save() {
    if (this.form?.value.productId == 0) this.form.value.productId = undefined;
    if (this.form?.value.directPaymentPayrollProductId == 0)
      this.form.value.directPaymentPayrollProductId = undefined;

    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.service
        .saveYear(this.form?.value)
        .pipe(tap(this.updateFormValueAndEmitChange))
    );
  }

  triggerDelete(): void {
    const mb = this.messageboxService.warning(
      'core.delete',
      'core.deletewarning'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) {
        this.performAction.crud(
          CrudActionTypeEnum.Delete,
          this.service
            .deleteYear(this.form!.value.timeWorkReductionReconciliationYearId)
            .pipe(
              tap(crud => {
                this.updateFormValueAndEmitChange(crud, true);
              })
            )
        );
      }
    });
  }

  checkSelection() {
    this.highestStatus = 0;
    this.lowestStatus = 10;
    this.hasPension = false;

    let rows = this.subGrid?.getSelectedRows();

    if (rows === undefined || rows.length == 0) {
      rows = this.rows;
    }

    rows.forEach(row => {
      if (this.highestStatus < row.status) this.highestStatus = row.status;
      if (this.lowestStatus > row.status) this.lowestStatus = row.status;

      if (
        row.selectedWithdrawalMethod ==
          TermGroup_TimeWorkReductionWithdrawalMethod.PensionDeposit ||
        ((this.data?.defaultPaidLeaveNotUsed ==
          TermGroup_TimeWorkReductionWithdrawalMethod.PensionDeposit ||
          this.data?.defaultPaidLeaveNotUsed ==
            TermGroup_TimeWorkReductionWithdrawalMethod.NotChoosed) &&
          row.status ==
            TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome)
      )
        this.hasPension = true;
    });
    this.buildFunctionList();
  }

  loadEmployees() {
    if (this.data.new) return of(undefined);

    return this.performLoad.load$(
      this.service
        .getEmployees(this.form!.value.timeWorkReductionReconciliationYearId)
        .pipe(
          tap((value: ITimeWorkReductionReconciliationEmployeeDTO[]) => {
            this.rows = value;
            this.rows.sort((a, b) =>
              a.employeeNrAndName.localeCompare(b.employeeNrAndName)
            );
            this.setupRows();
            this.form?.patchYearEmployees(this.rows);
            this.checkSelection();
            this.buildFunctionList();
          })
        )
    );
  }

  buildFunctionList() {
    this.menuList = [];
    if (
      this.subGrid === undefined ||
      this.subGrid?.getSelectedCount() == 0 ||
      this.lowestStatus <=
        TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Calculated
    )
      this.menuList.push({
        id: FunctionType.CalculateOutcome,
        label: this.translate.instant(
          'time.payroll.worktimeaccount.employee.calculateoutcome'
        ),
      });
    if (
      this.lowestStatus >=
      TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Calculated
    ) {
      this.menuList.push({
        id: FunctionType.ExportCalculateOutcome,
        label: this.translate.instant(
          'time.time.timeworkreduction.printcalculationdata'
        ),
      });
    }
    if (
      this.highestStatus >=
        TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Choosed ||
      (this.highestStatus ==
        TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Calculated &&
        this.lowestStatus <=
          TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Choosed)
    ) {
      this.menuList.push({
        id: FunctionType.GenerateOutcome,
        label: this.translate.instant(
          'time.payroll.worktimeaccount.employee.generateoutcome'
        ),
      });
    }
    if (
      this.highestStatus >=
        TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome &&
      this.lowestStatus <=
        TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome
    ) {
      this.menuList.push({
        id: FunctionType.ReverseTransaction,
        label: this.translate.instant(
          'time.payroll.worktimeaccount.employee.reversetransaction'
        ),
      });
    }
    if (
      this.data?.usePension &&
      this.hasPension &&
      this.highestStatus >=
        TermGroup_TimeWorkReductionReconciliationEmployeeStatus.Outcome
    ) {
      this.menuList.push({
        id: FunctionType.ExportPension,
        label: this.translate.instant(
          'time.payroll.worktimeaccount.employee.exportpension'
        ),
      });
    }

    return of(undefined);
  }

  peformAction(selected: MenuButtonItem): void {
    switch (selected.id) {
      case FunctionType.CalculateOutcome:
        this.calculateOutcome().subscribe(() => {});
        break;
      case FunctionType.ExportCalculateOutcome:
        this.exportCalculateOutcome();
        break;
      case FunctionType.ReverseTransaction:
        this.reverseTransaction(FunctionType.ReverseTransaction);
        break;

      case FunctionType.GenerateOutcome:
        this.generateOutcome();
        break;
      case FunctionType.ExportPension:
        this.exportPension();
        break;
    }
  }

  showError(result: any) {
    this.messageboxService.error(
      this.translate.instant('core.error'),
      result.errorMessage
    );
  }
  exportCalculateOutcome() {
    const row: object[] = [];
    const header: object[] = [];
    header.push(
      { header: this.translate.instant('time.employee.employeenumber') },
      { header: this.translate.instant('common.employee') },
      {
        header: this.translate.instant(
          'time.payroll.worktimeaccount.employee.selectedwithdrawalmethod'
        ),
      },
      {
        header: this.translate.instant(
          'time.time.timeworkreduction.accearningminutes'
        ),
      },
      {
        header: this.translate.instant(
          'time.time.timeaccumulator.employeegrouprule.thresholdMinutes'
        ),
      },
      {
        header: this.translate.instant(
          'time.time.timeworkreduction.minutesoverthreshold'
        ),
      }
    );
    this.form!.timeWorkReductionReconciliationEmployeeDTO.value.forEach(
      (res: {
        employeeNr: string;
        employeeName: string;
        selectedWithdrawalMethod: number;
        accEarningMinutes: number;
        threshold: number;
        minutesOverThreshold: number;
      }) => {
        const withdrawalMethod = this.withdrawalMethods.find(
          wm => wm.id === res.selectedWithdrawalMethod
        );
        row.push({
          text: [
            res.employeeNr,
            res.employeeName,
            withdrawalMethod?.name ?? this.withdrawalMethods[0]?.name,
            this.formatMinutes(res.accEarningMinutes, true),
            this.formatMinutes(res.threshold),
            this.formatMinutes(res.minutesOverThreshold),
          ],
        });
      }
    );

    this.dialogServiceV2
      .open(TimeWorkAccountOutputComponent, {
        title: this.translate.instant(
          'time.employee.vacationdebt.calculations'
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
        this.loadEmployees().subscribe(() => {});
      });
  }
  calculateOutcome() {
    const empIds = this.subGrid.getSelectedRows().map(x => x.employeeId);
    const timeWorkReductionReconciliationEmployeeIds =
      this.getSelectedEmployees();

    const model = {
      timeWorkReductionReconciliationId:
        this.form?.value.timeWorkReductionReconciliationId,
      timeWorkReductionReconciliationYearId:
        this.form?.value.timeWorkReductionReconciliationYearId,
      timeWorkReductionReconciliationEmployeeIds:
        timeWorkReductionReconciliationEmployeeIds,
      employeeIds: empIds,
    } as ITimeWorkReductionReconciliationEmployeeModel;
    return this.performLoad.load$(this.service.calculate(model)).pipe(
      tap(res => {
        if (!res.result.success) {
          this.showError(res.result);
        } else {
          const row: object[] = [];
          const header: object[] = [];
          header.push(
            { header: this.translate.instant('common.employee') },
            { header: this.translate.instant('core.fileupload.status') },
            { header: this.translate.instant('core.info') }
          );
          res.rows.forEach(
            (r: {
              employeeNrAndName: string;
              employeeStatusName: string;
              codeName: string;
            }) => {
              row.push({
                text: [r.employeeNrAndName, r.employeeStatusName, r.codeName],
              });
            }
          );
          if (res.rows.length == 0) {
            this.toasterService.success(
              this.translate.instant(
                'time.payroll.worktimeaccount.employee.calculateoutcomesuccess'
              )
            );
            this.loadEmployees();
          } else {
            this.dialogServiceV2
              .open(TimeWorkAccountOutputComponent, {
                title: this.translate.instant(
                  'time.payroll.worktimeaccount.employee.calculateoutcome'
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
                this.loadEmployees().subscribe(() => {});
              });
          }
        }
      })
    );
  }
  getSelectedEmployees() {
    return this.subGrid
      .getSelectedRows()
      .map(x => x.timeWorkReductionReconciliationEmployeeId);
  }

  generateOutcome() {
    this.dialogServiceV2
      .open(TimeWorkReductionReconcilationGenerateOutcome, {
        timeWorkReductionReconciliationYearId:
          this.form?.value.timeWorkReductionReconciliationYearId,
        timeWorkReductionReconciliationId:
          this.form?.value.timeWorkReductionReconciliationId,
        employeeLastDecidedDate: this.form?.value.employeeLastDecidedDate,
        timeWorkReductionReconciliationEmployeeIds: this.getSelectedEmployees(),
        stop: this.form?.value.stop,
        size: 'lg',
        title: this.translate.instant(
          'time.payroll.worktimeaccount.employee.generateoutcome'
        ),
      } as unknown as ITimeWorkReductionReconcilationGenerateOutcomeDialogData)
      .afterClosed()
      .pipe(take(1))
      .subscribe(() => {
        this.loadEmployees().subscribe(() => {});
      });
  }

  reverseTransaction(type: FunctionType) {
    const mb = this.messageboxService.warning(
      'core.warning',
      type == FunctionType.ReverseTransaction
        ? 'time.payroll.worktimeaccount.employee.reversetransactionwarning'
        : 'time.payroll.worktimeaccount.employee.reversebalanceendedwarning'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result)
        if (type == FunctionType.ReverseTransaction)
          this.perFormReverseTransaction();
    });
  }
  perFormReverseTransaction() {
    const reverseTransaction = {
      timeWorkReductionReconciliationYearId:
        this.form?.value.timeWorkReductionReconciliationYearId,
      timeWorkReductionReconciliationId:
        this.form?.value.timeWorkReductionReconciliationId,
      overrideChoosen: false,
      timeWorkReductionReconciliationEmployeeIds: this.getSelectedEmployees(),
    } as ITimeWorkReductionReconciliationGenerateOutcomeModel;

    this.performLoad
      .load$(this.service.reverseTransactions(reverseTransaction))
      .pipe(
        tap(data => {
          if (!data.result.success) {
            this.showError(data.result);
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
            data.rows.forEach(
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
            if (data.length == 0) {
              this.toasterService.success(
                this.translate.instant(
                  'time.payroll.worktimeaccount.employee.transactionreversedsuccess'
                )
              );
              this.loadEmployees();
            } else {
              this.dialogServiceV2
                .open(TimeWorkAccountOutputComponent, {
                  title: this.translate.instant(
                    'time.payroll.worktimeaccount.employee.reversetransaction'
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
                  this.loadEmployees().subscribe(() => {});
                });
            }
          }
        })
      )
      .subscribe(() => {});
  }
  exportPension() {
    const pensionExport = {
      timeWorkReductionReconciliationYearId:
        this.form?.value.timeWorkReductionReconciliationYearId,
      timeWorkReductionReconciliationId:
        this.form?.value.timeWorkReductionReconciliationId,
      overrideChoosen: false,
      timeWorkReductionReconciliationEmployeeIds: this.getSelectedEmployees(),
    } as unknown as ITimeWorkReductionReconciliationEmployeeModel;

    this.performLoad
      .load$(
        this.service.getPensionExport(pensionExport).pipe(
          tap(res => {
            const row: object[] = [];
            const header: object[] = [];
            header.push(
              {
                header: this.translate.instant('time.employee.employeenumber'),
              },
              { header: this.translate.instant('common.name') },
              {
                header: this.translate.instant(
                  'time.employee.employee.socialsecnr'
                ),
              },
              {
                header: this.translate.instant(
                  'time.time.timeperiod.paymentdate'
                ),
              },
              { header: this.translate.instant('common.amount') }
            );
            res.forEach(r => {
              row.push({
                text: [
                  r.employeeNr,
                  r.employeeName,
                  r.employeeSocialSec,
                  r.paymentDate != undefined
                    ? r.paymentDate.toFormattedDate()
                    : '',
                  r.amount.toString(),
                ],
              });
            });
            this.dialogServiceV2.open(TimeWorkAccountOutputComponent, {
              title: this.translate.instant(
                'time.payroll.worktimeaccount.employee.exportpension'
              ),
              row: row,
              header: header,
              export: true,
              size: 'lg',
            } as ITimeWorkAccountOutputDialogData);
          })
        )
      )
      .subscribe(() => {});
  }

  setDisabled(
    row: any,
    status: TermGroup_TimeWorkReductionReconciliationEmployeeStatus
  ) {
    return row?.status < status;
  }

  formatMinutes(value: number, wrapInQuotes: boolean = false): string {
    const absValue = Math.abs(value);
    const hours = Math.floor(absValue / 60);
    const minutes = absValue % 60;
    const formatted = `${hours}:${minutes.toString().padStart(2, '0')}`;
    if (value < 0) {
      return wrapInQuotes ? `-'${formatted}'` : `-${formatted}`;
    }
    return formatted;
  }
}
