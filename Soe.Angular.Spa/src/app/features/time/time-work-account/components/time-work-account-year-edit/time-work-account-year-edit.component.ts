import {
  Component,
  EventEmitter,
  inject,
  Input,
  OnInit,
  signal,
} from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { CrudActionTypeEnum } from '@shared/enums';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  TermGroup,
  TermGroup_SysPayrollType,
  TermGroup_TimeWorkAccountWithdrawalMethod,
  TermGroup_TimeWorkAccountYearEmployeeStatus,
} from '@shared/models/generated-interfaces/Enumerations';
import { IProductSmallDTO } from '@shared/models/generated-interfaces/ProductDTOs';
import {
  ITimeWorkAccountYearEmployeeDTO,
  ITimeWorkAccountYearEmployeeModel,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { CellValueChangedEvent } from 'ag-grid-community';
import { BehaviorSubject, of } from 'rxjs';
import { finalize, take, tap } from 'rxjs/operators';
import {
  TimeWorkAccountYearDTO,
  TimeWorkAccountYearEmployeeDTO,
} from '../../../models/timeworkaccount.model';
import { TimeWorkAccountYearForm } from '../../models/time-work-account-year-form.model';
import { TimeWorkAccountService } from '../../services/time-work-account.service';
import {
  ITimeWorkAccountYearWorkTimeWeekEventDialogData,
  ITimeWorkAccountYearWorkTimeWeekEventObject,
  TimeWorkAccountYearWorkTimeWeekEditComponent,
} from '../time-work-account-year-worktimeweek-edit/time-work-account-year-worktimeweek-edit.component';

import { DialogComponent } from '@ui/dialog/dialog/dialog.component';
import { DialogData } from '@ui/dialog/models/dialog';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToasterService } from '@ui/toaster/services/toaster.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  ITimeWorkAccountOutputDialogData,
  TimeWorkAccountOutputComponent,
} from '../time-work-account-output/time-work-account-output.component';
import {
  ITimeWorkAcoountYearGenerateOutcomeDialogData,
  TimeWorkAcoountYearGenerateOutcomeComponent,
} from '../time-work-account-year-generate-outcome/time-work-account-year-generate-outcome.component';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';
import { ResponseUtil } from '@shared/util/response-util';

export interface ITimeWorkAccountYearEventObject {
  object: TimeWorkAccountYearForm | undefined;
  action: CrudActionTypeEnum;
}
export interface ITimeWorkAccountYearDialogData extends DialogData {
  new: boolean;
  id: number;
  timeWorkAccountId: number;
  usePension: boolean;
  useDirectPayment: boolean;
  usePaidLeave: boolean;
  defaultPaidLeaveNotUsed: TermGroup_TimeWorkAccountWithdrawalMethod;
}
export enum FunctionType {
  CalculateOutcome = 1,
  SendSelection = 2,
  GenerateOutcome = 3,
  ExportPension = 4,
  ReverseTransaction = 5,
  GenerateUnusedPaidBalance = 6,
  ReversePaidBalance = 7,
}
@Component({
  selector: 'soe-time-work-account-year-edit',
  templateUrl: './time-work-account-year-edit.component.html',
  providers: [FlowHandlerService, DialogService, ToolbarService],
  standalone: false,
})
export class TimeWorkAccountYearEditComponent
  extends DialogComponent<ITimeWorkAccountYearDialogData>
  implements OnInit
{
  @Input() form: TimeWorkAccountYearForm | undefined;
  @Input() timeWorkAccountYear: TimeWorkAccountYearDTO =
    new TimeWorkAccountYearDTO();
  @Input() usePension: boolean | undefined;
  @Input() useDirectPayment: boolean | undefined;
  @Input() usePaidLeave: boolean | undefined;
  @Input() defaultPaidLeaveNotUsed:
    | TermGroup_TimeWorkAccountWithdrawalMethod
    | undefined;

  settingsExpanderIsOpen = true;
  defaultWithdrawalMethods: SmallGenericType[] = [];
  defaultPaidLeaveNotUsedMethods: SmallGenericType[] = [];
  idFieldName = '';
  menuList: MenuButtonItem[] = [];
  dialogServiceV2 = inject(DialogService);
  performLoad = new Perform<any>(this.progressService);
  performAction = new Perform<TimeWorkAccountService>(this.progressService);
  highestStatus = 0;
  lowestStatus = 10;

  rows: ITimeWorkAccountYearEmployeeDTO[] = [];

  workIntervalToolbarService = inject(ToolbarService);

  // Lookups
  terms: any = [];
  withdrawalMethods: SmallGenericType[] = [];
  employeeStatus: SmallGenericType[] = [];
  pensionPayrollProducts: SmallGenericType[] = [{ id: 0, name: '' }];
  directPaymentpayrollProducts: SmallGenericType[] = [{ id: 0, name: '' }];
  timeAccumulators: SmallGenericType[] = [{ id: 0, name: '' }];
  payrollProducts: IProductSmallDTO[] = [];
  showPension: boolean = this.data?.usePension;
  showDirectPayment: boolean = this.data?.useDirectPayment;
  showPaidLeave: boolean = this.data?.usePaidLeave;
  hasPaidLeave = false;
  hasPension = false;
  hasSentDate = true;
  handler = inject(FlowHandlerService);

  public event: EventEmitter<ITimeWorkAccountYearEventObject> =
    new EventEmitter();

  get currentLanguage(): string {
    return SoeConfigUtil.language;
  }

  // Subgrid
  subGrid!: GridComponent<TimeWorkAccountYearEmployeeDTO>;
  subData = new BehaviorSubject<TimeWorkAccountYearEmployeeDTO[]>([]);

  constructor(
    private translate: TranslateService,
    private timeWorkAccountService: TimeWorkAccountService,
    private coreService: CoreService,
    private progressService: ProgressService,
    private messageboxService: MessageboxService,
    private validationHandler: ValidationHandler,
    private toasterService: ToasterService
  ) {
    super();
    this.setData(this.data.new, this.data.id);
  }

  ngOnInit() {
    this.handler.execute({
      permission: Feature.Time_Payroll_TimeWorkAccount,
      lookups: [
        this.loadWithdrawalMethods(),
        this.loadEmployeeStatus(),
        of(this.loadPayrollProducts()),
      ],
      setupGrid: this.setupRowsGrid.bind(this),
    });
  }

  onCellValueChanged(evt: CellValueChangedEvent): void {
    this.form?.patchTimeWorkAccountYearEmployees(this.subData.value);
    this.form?.markAsDirty();
  }

  buildFunctionList() {
    this.menuList = [];
    if (
      this.subGrid === undefined ||
      this.subGrid?.getSelectedCount() == 0 ||
      this.lowestStatus <=
        TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated
    )
      this.menuList.push({
        id: FunctionType.CalculateOutcome,
        label: this.translate.instant(
          'time.payroll.worktimeaccount.employee.calculateoutcome'
        ),
      });

    if (
      this.highestStatus >=
        TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated &&
      this.lowestStatus <=
        TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated
    ) {
      this.menuList.push({
        id: FunctionType.SendSelection,
        label: this.translate.instant(
          'time.payroll.worktimeaccount.employee.sendselection'
        ),
      });
    }
    if (
      (this.highestStatus >=
        TermGroup_TimeWorkAccountYearEmployeeStatus.Choosed ||
        (this.highestStatus ==
          TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated &&
          this.hasSentDate)) &&
      this.lowestStatus <= TermGroup_TimeWorkAccountYearEmployeeStatus.Choosed
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
        TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome &&
      this.lowestStatus <= TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome
    ) {
      this.menuList.push({
        id: FunctionType.ReverseTransaction,
        label: this.translate.instant(
          'time.payroll.worktimeaccount.employee.reversetransaction'
        ),
      });
      if (this.data?.usePaidLeave && this.hasPaidLeave) {
        this.menuList.push({
          id: FunctionType.GenerateUnusedPaidBalance,
          label: this.translate.instant(
            'time.payroll.worktimeaccount.employee.generateunpaidbalance'
          ),
        });
      }
    }
    if (
      this.highestStatus ==
      TermGroup_TimeWorkAccountYearEmployeeStatus.PaidBalance
    ) {
      this.menuList.push({
        id: FunctionType.ReversePaidBalance,
        label: this.translate.instant(
          'time.payroll.worktimeaccount.employee.reversebalanceended'
        ),
      });
    }
    if (
      this.data?.usePension &&
      this.hasPension &&
      this.highestStatus >= TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome
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

  createForm(
    element?: TimeWorkAccountYearDTO,
    setIdFieldName = true
  ): TimeWorkAccountYearForm {
    const form = new TimeWorkAccountYearForm({
      validationHandler: this.validationHandler,
      element,
    });
    if (setIdFieldName) this.idFieldName = form.getIdFieldName();
    return form;
  }

  private setupToolbar(): void {
    this.workIntervalToolbarService.createItemGroup({
      items: [
        this.workIntervalToolbarService.createToolbarButton('new', {
          iconName: signal('plus'),
          caption: signal('common.new'),
          tooltip: signal('common.new'),
          onAction: () => {
            this.editWorkTimeWeek(undefined);
          },
        }),
      ],
    });
  }

  private setupRowsGrid(grid: GridComponent<TimeWorkAccountYearEmployeeDTO>) {
    this.subGrid = grid;
    this.translate
      .get([
        'common.employee',
        'common.from',
        'common.to',
        'core.info',
        'core.delete',
        'core.fileupload.status',
        'time.payroll.worktimeaccount.employee.selectedwithdrawalmethod',
        'time.payroll.worktimeaccount.employee.selecteddate',
        'time.payroll.worktimeaccount.employee.calculatedpaidleavetime',
        'time.payroll.worktimeaccount.employee.calculatedpaidleaveamount',
        'time.payroll.worktimeaccount.employee.calculatedpensiondepositamount',
        'time.payroll.worktimeaccount.employee.calculateddirectpaymentamount',
        'time.payroll.worktimeaccount.employee.calculatingworkingtimepromoted',
        'time.payroll.worktimeaccount.employee.specifiedWorkingTimePromoted',
        'time.payroll.worktimeaccount.employee.sendselectionsuccess',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.subGrid.addColumnText('employeeName', terms['common.employee'], {
          flex: 200,
          editable: false,
        });
        this.subGrid.addColumnDate('earningStart', terms['common.from'], {
          flex: 70,
        });
        this.subGrid.addColumnDate('earningStop', terms['common.to'], {
          flex: 70,
        });
        this.subGrid.addColumnDate(
          'sentDate',
          terms['time.payroll.worktimeaccount.employee.sendselectionsuccess'],
          {
            flex: 70,
          }
        );
        this.subGrid.addColumnSelect(
          'selectedWithdrawalMethod',
          terms[
            'time.payroll.worktimeaccount.employee.selectedwithdrawalmethod'
          ],
          this.withdrawalMethods,
          this.subgridChangeWithdrawalMethod.bind(this),
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            editable: row => {
              return this.setDisabled(
                row.data,
                TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome
              );
            },
            flex: 100,
          }
        );
        this.subGrid.addColumnDate(
          'selectedDate',
          terms['time.payroll.worktimeaccount.employee.selecteddate'],

          { flex: 100 }
        );
        this.subGrid.addColumnNumber(
          'specifiedWorkingTimePromoted',
          terms[
            'time.payroll.worktimeaccount.employee.specifiedWorkingTimePromoted'
          ],

          {
            editable: row => {
              return this.setDisabled(
                row.data,
                TermGroup_TimeWorkAccountYearEmployeeStatus.Choosed
              );
            },
            decimals: 2,
            allowEmpty: true,
            flex: 100,
          }
        );
        this.subGrid.addColumnNumber(
          'calculatedWorkingTimePromoted',
          terms[
            'time.payroll.worktimeaccount.employee.calculatingworkingtimepromoted'
          ],

          {
            editable: false,
            decimals: 2,
            flex: 100,
          }
        );

        this.subGrid.addColumnTimeSpan(
          'calculatedPaidLeaveMinutes',
          terms[
            'time.payroll.worktimeaccount.employee.calculatedpaidleavetime'
          ],
          {
            enableHiding: true,
            hide: !this.showPaidLeave,
            flex: 80,
          }
        );
        this.subGrid.addColumnNumber(
          'calculatedPaidLeaveAmount',
          terms[
            'time.payroll.worktimeaccount.employee.calculatedpaidleaveamount'
          ],
          {
            enableHiding: true,
            hide: !this.showPaidLeave,
            decimals: 2,
            flex: 80,
          }
        );
        this.subGrid.addColumnNumber(
          'calculatedPensionDepositAmount',
          terms[
            'time.payroll.worktimeaccount.employee.calculatedpensiondepositamount'
          ],
          {
            decimals: 2,
            flex: 80,
            enableHiding: true,
            hide: !this.showPension,
          }
        );
        this.subGrid.addColumnNumber(
          'calculatedDirectPaymentAmount',
          terms[
            'time.payroll.worktimeaccount.employee.calculateddirectpaymentamount'
          ],
          {
            decimals: 2,
            flex: 80,
            enableHiding: true,
            hide: !this.showDirectPayment,
          }
        );
        this.subGrid.addColumnSelect(
          'status',
          terms['core.fileupload.status'],
          this.employeeStatus,
          null,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 80,
          }
        );
        this.subGrid.addColumnIcon(null, '', {
          flex: 20,
          iconName: 'info-circle',
          iconClass: 'information-color',
          enableHiding: false,
          tooltip: terms['core.info'],
          showIcon: row => {
            return (
              row.status >=
              TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated
            );
          },
          onClick: row => {
            this.loadInformation(row);
          },
        });
        this.subGrid.addColumnIcon(null, '', {
          flex: 20,
          iconName: 'xmark',
          iconClass: 'icon-delete',
          enableHiding: false,
          tooltip: terms['core.delete'],
          showIcon: row => {
            return (
              row.status ==
              TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated
            );
          },
          onClick: row => {
            this.deleteRow(row);
          },
        });
        this.subGrid.context.suppressGridMenu = true;
        this.subGrid.setRowSelection('singleRow');
        this.subGrid.alwaysShowVerticalScroll();
        this.subGrid.columns.forEach(col => {
          col.floatingFilter = true;
          col.suppressFiltersToolPanel = false;
          col.sortable = true;
        });
        this.subGrid.finalizeInitGrid();
        this.setupToolbar();
      });
  }

  private loadWithdrawalMethods() {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeWorkAccountWithdrawalMethod,
        false,
        false
      )
      .pipe(
        tap(x => {
          x.forEach(m => {
            if (
              m.id == TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed ||
              (m.id ==
                TermGroup_TimeWorkAccountWithdrawalMethod.DirectPayment &&
                this.data?.useDirectPayment) ||
              (m.id ==
                TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit &&
                this.data?.usePension) ||
              (m.id == TermGroup_TimeWorkAccountWithdrawalMethod.PaidLeave &&
                this.data?.usePaidLeave)
            )
              this.withdrawalMethods.push(m);
          });

          this.defaultWithdrawalMethods.push(
            this.withdrawalMethodToSmallGenericType(
              TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit
            )
          );
          this.defaultWithdrawalMethods.push(
            this.withdrawalMethodToSmallGenericType(
              TermGroup_TimeWorkAccountWithdrawalMethod.PaidLeave
            )
          );
          this.defaultWithdrawalMethods.push(
            this.withdrawalMethodToSmallGenericType(
              TermGroup_TimeWorkAccountWithdrawalMethod.DirectPayment
            )
          );

          this.defaultPaidLeaveNotUsedMethods.push(
            this.withdrawalMethodToSmallGenericType(
              TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit
            )
          );
          this.defaultPaidLeaveNotUsedMethods.push(
            this.withdrawalMethodToSmallGenericType(
              TermGroup_TimeWorkAccountWithdrawalMethod.DirectPayment
            )
          );
        })
      );
  }

  private loadEmployeeStatus() {
    return this.coreService
      .getTermGroupContent(
        TermGroup.TimeWorkAccountYearEmployeeStatus,
        false,
        true
      )
      .pipe(
        tap(x => {
          this.employeeStatus = x;
        })
      );
  }

  loadPayrollProducts() {
    return this.timeWorkAccountService
      .getPayrollProductsSmall()
      .subscribe(x => {
        this.payrollProducts = x;
        this.loadPensionProducts().subscribe(() => {
          this.loadDirectPaymentProducts().subscribe(() => {
            this.loadTimeAccumulators().subscribe(() => {});
          });
        });
      });
  }

  loadPensionProducts() {
    return this.timeWorkAccountService
      .GetPayrollProductIdsByType(
        TermGroup_SysPayrollType.SE_PensionPremium,
        TermGroup_SysPayrollType.SE_PensionPremium_WorkingAccount
      )
      .pipe(
        tap(x => {
          x.forEach(y => {
            const product = this.payrollProducts.find(f => f.productId == y);
            if (product != null) {
              this.pensionPayrollProducts.push({
                id: product.productId,
                name: product.numberName,
              });
            }
          });
        })
      );
  }

  loadDirectPaymentProducts() {
    return this.timeWorkAccountService
      .GetPayrollProductIdsByType(
        TermGroup_SysPayrollType.SE_GrossSalary,
        TermGroup_SysPayrollType.SE_GrossSalary_WorkingAccount
      )
      .pipe(
        tap(x => {
          x.forEach(y => {
            const product = this.payrollProducts.find(f => f.productId == y);
            if (product != null) {
              this.directPaymentpayrollProducts.push({
                id: product.productId,
                name: product.numberName,
              });
            }
          });
        })
      );
  }
  loadTimeAccumulators() {
    return this.timeWorkAccountService.getTimeAccumulators().pipe(
      tap(x => {
        x.forEach(y => {
          this.timeAccumulators.push({ id: y.timeAccumulatorId, name: y.name });
        });
      })
    );
  }

  triggerEvent(
    item: TimeWorkAccountYearForm | undefined,
    action: CrudActionTypeEnum
  ) {
    if (!this.form?.isNew && action != CrudActionTypeEnum.Delete) {
      this.loadEmployees();
      this.form?.markAsPristine();
    } else this.dialogRef.close({ object: item, action });
  }

  triggerDelete(): void {
    const mb = this.messageboxService.warning(
      'core.delete',
      'core.deletewarning'
    );
    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) this.performDelete();
    });
  }

  performDelete(): void {
    this.performAction.crud(
      CrudActionTypeEnum.Delete,
      this.timeWorkAccountService.deleteYear(
        this.form?.value.timeWorkAccountYearId,
        this.form?.value.timeWorkAccountId
      ),
      this.triggerEvent.bind(this, this.form, CrudActionTypeEnum.Delete)
    );
  }

  updateBeforeSave(): void {
    this.subGrid.agGrid.api.stopEditing();
    this.form?.patchTimeWorkAccountYearEmployees(this.subData.value);
  }

  performSave(): void {
    if (!this.form || this.form.invalid) return;
    let warning = '';
    if (
      this.data?.useDirectPayment &&
      this.form?.value.directPaymentPayrollProductId == 0
    ) {
      warning += this.translate.instant(
        'time.payroll.worktimeaccount.directpaymentproductmissing'
      );
    }
    if (warning != '') warning += '\r\n';

    if (
      this.data?.usePension &&
      this.form?.value.pensionDepositPayrollProductId == 0
    ) {
      warning += this.translate.instant(
        'time.payroll.worktimeaccount.pensionproductmissing'
      );
    }
    if (warning != '') warning += '\r\n';

    if (this.data?.usePaidLeave && this.form?.value.timeAccumulatorId == 0) {
      warning += this.translate.instant(
        'time.payroll.worktimeaccount.timeaccumulatormissing'
      );
    }
    if (warning != '') {
      this.messageboxService.warning(
        this.translate.instant('error.unabletosave_title'),
        warning
      );
      return;
    }
    this.updateBeforeSave();
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.timeWorkAccountService
        .saveYear(this.form?.value)
        .pipe(tap(this.updateFormValueAndEmitChange))
    );
  }

  updateFormValueAndEmitChange = (crud: BackendResponse) => {
    if (crud.success) {
      this.form?.patchValue({
        [this.idFieldName]: ResponseUtil.getEntityId(crud),
      });
      this.triggerEvent(this.form, CrudActionTypeEnum.Save);
    }
  };

  peformAction(selected: MenuButtonItem): void {
    switch (selected.id) {
      case FunctionType.CalculateOutcome:
        this.calculateOutcome();
        break;
      case FunctionType.SendSelection:
        this.sendSelection();
        break;
      case FunctionType.GenerateOutcome:
        this.generateOutcome(FunctionType.GenerateOutcome);
        break;
      case FunctionType.ExportPension:
        this.exportPension();
        break;
      case FunctionType.ReverseTransaction:
        this.reverseTransaction(FunctionType.ReverseTransaction);
        break;
      case FunctionType.GenerateUnusedPaidBalance:
        this.generateOutcome(FunctionType.GenerateUnusedPaidBalance);
        break;
      case FunctionType.ReversePaidBalance:
        this.reverseTransaction(FunctionType.GenerateUnusedPaidBalance);
        break;
    }
  }
  calculateOutcome() {
    const rows = this.subGrid.getSelectedRows();
    let warning = '';
    if (
      this.form?.value.timeWorkAccountId != 0 &&
      this.form?.value.timeWorkAccountYearId != 0
    ) {
      const calculateTimeWorkAccountAmounts = {
        timeWorkAccountId: this.form?.value.timeWorkAccountId,
        timeWorkAccountYearId: this.form?.value.timeWorkAccountYearId,
        timeWorkAccountYearEmployeeIds: [],
        employeeIds: [],
      } as ITimeWorkAccountYearEmployeeModel;
      if (rows.length != 0) {
        rows.forEach(r => {
          if (
            r.specifiedWorkingTimePromoted !== undefined &&
            r.specifiedWorkingTimePromoted?.toString() !== ''
          ) {
            warning += r.employeeName + '\r\n';
          }
          calculateTimeWorkAccountAmounts.timeWorkAccountYearEmployeeIds.push(
            r.timeWorkAccountYearEmployeeId
          );
          calculateTimeWorkAccountAmounts.employeeIds.push(r.employeeId);
        });
      } else {
        this.rows.forEach(r => {
          if (
            r.specifiedWorkingTimePromoted !== undefined &&
            r.specifiedWorkingTimePromoted?.toString() !== ''
          ) {
            warning += r.employeeName + '\r\n';
          }
        });
      }

      if (warning == '')
        this.caclucalteEmployees(calculateTimeWorkAccountAmounts);
      else {
        const mb = this.messageboxService.warning(
          'time.payroll.worktimeaccount.employee.calculateoutcome',
          this.translate.instant(
            'time.payroll.worktimeaccount.employee.calculateoutcomemanualwarning'
          ) +
            ':\r\n' +
            warning
        );
        mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
          if (response?.result)
            this.caclucalteEmployees(calculateTimeWorkAccountAmounts);
        });
      }
    }
  }
  caclucalteEmployees(
    calculateTimeWorkAccountAmounts: ITimeWorkAccountYearEmployeeModel
  ) {
    this.performLoad.load(
      this.timeWorkAccountService
        .calculateEmployee(calculateTimeWorkAccountAmounts)
        .pipe(
          finalize(() => {
            if (!this.performLoad.data?.result.success) {
              this.showError(this.performLoad.data?.result);
            } else {
              const row: object[] = [];
              const header: object[] = [];
              header.push(
                { header: this.translate.instant('common.employee') },
                { header: this.translate.instant('core.fileupload.status') },
                { header: this.translate.instant('core.info') }
              );
              this.performLoad.data?.rows.forEach(
                (res: {
                  employeeNrAndName: string;
                  employeeStatusName: string;
                  codeName: string;
                }) => {
                  row.push({
                    text: [
                      res.employeeNrAndName,
                      res.employeeStatusName,
                      res.codeName,
                    ],
                  });
                }
              );
              if (this.performLoad.data.length == 0) {
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
                    this.loadEmployees();
                  });
              }
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

  sendSelection() {
    if (
      this.form?.value.timeWorkAccountId != 0 &&
      this.form?.value.timeWorkAccountYearId != 0
    ) {
      const sendSelection = {
        timeWorkAccountId: this.form?.value.timeWorkAccountId,
        timeWorkAccountYearId: this.form?.value.timeWorkAccountYearId,
        timeWorkAccountYearEmployeeIds: this.getSelectedEmployees(),
      } as ITimeWorkAccountYearEmployeeModel;

      this.performLoad.load(
        this.timeWorkAccountService.sendSelection(sendSelection).pipe(
          finalize(() => {
            if (!this.performLoad.data?.result.success) {
              this.showError(this.performLoad.data?.result);
            } else {
              const row: object[] = [];
              const header: object[] = [];
              header.push(
                { header: this.translate.instant('common.employee') },
                { header: this.translate.instant('core.info') }
              );
              this.performLoad.data?.rows.forEach(
                (res: {
                  employeeNrAndName: string;
                  employeeStatusName: string;
                  codeName: string;
                }) => {
                  row.push({
                    text: [res.employeeNrAndName, res.codeName],
                  });
                }
              );

              if (this.performLoad.data.length == 0) {
                this.toasterService.success(
                  this.translate.instant(
                    'time.payroll.worktimeaccount.employee.sendselectionsuccess'
                  )
                );
                this.loadEmployees();
              } else {
                this.dialogServiceV2
                  .open(TimeWorkAccountOutputComponent, {
                    title: this.translate.instant(
                      'time.payroll.worktimeaccount.employee.sendselection'
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
                    this.loadEmployees();
                  });
              }
            }
          })
        )
      );
    }
  }
  generateOutcome(type: FunctionType) {
    this.dialogServiceV2
      .open(TimeWorkAcoountYearGenerateOutcomeComponent, {
        timeWorkAccountYearId: this.form?.value.timeWorkAccountYearId,
        timeWorkAccountId: this.form?.value.timeWorkAccountId,
        employeeLastDecidedDate: this.form?.value.employeeLastDecidedDate,
        timeWorkAccountYearEmployeeIds: this.getSelectedEmployees(),
        functionType: type,
        size: 'lg',
        title:
          type == FunctionType.GenerateUnusedPaidBalance
            ? this.translate.instant(
                'time.payroll.worktimeaccount.employee.generateunpaidbalance'
              )
            : this.translate.instant(
                'time.payroll.worktimeaccount.employee.generateoutcome'
              ),
      } as ITimeWorkAcoountYearGenerateOutcomeDialogData)
      .afterClosed()
      .pipe(take(1))
      .subscribe(() => {
        this.loadEmployees();
      });
  }

  private setupRows() {
    if (!this.rows) return;

    this.rows.forEach(x => {
      x['employeeName'] = x.employeeNumber + ' ' + x.employeeName;
    });

    this.subData.next(this.rows);
  }
  exportPension() {
    const pensionExport = {
      timeWorkAccountId: this.form?.value.timeWorkAccountId,
      timeWorkAccountYearId: this.form?.value.timeWorkAccountYearId,
      timeWorkAccountYearEmployeeIds: this.getSelectedEmployees(),
    } as ITimeWorkAccountYearEmployeeModel;

    this.performLoad.load(
      this.timeWorkAccountService.getPensionExport(pensionExport).pipe(
        tap(res => {
          const row: object[] = [];
          const header: object[] = [];
          header.push(
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
            { header: this.translate.instant('common.amount') },
            { header: this.translate.instant('core.info') }
          );
          res.forEach(r => {
            row.push({
              text: [
                r.employeeNrAndName,
                r.employeeSocialSec,
                r.paymentDate != undefined
                  ? r.paymentDate.toFormattedDate()
                  : '',
                r.amount,
                r.ended
                  ? this.translate.instant(
                      'time.payroll.worktimeaccount.employee.balanceended'
                    )
                  : '',
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
    );
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
        type == FunctionType.ReverseTransaction
          ? this.perFormReverseTransaction()
          : this.perFormReversePaidBalance();
    });
  }

  perFormReversePaidBalance() {
    const reversePaidBalance = {
      timeWorkAccountId: this.form?.value.timeWorkAccountId,
      timeWorkAccountYearId: this.form?.value.timeWorkAccountYearId,
      timeWorkAccountYearEmployeeIds: this.getSelectedEmployees(),
    } as ITimeWorkAccountYearEmployeeModel;

    this.performLoad.load(
      this.timeWorkAccountService.reversePaidBalance(reversePaidBalance).pipe(
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
            if (this.performLoad.data.length == 0) {
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
                    'time.payroll.worktimeaccount.employee.reversebalanceended'
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
                  this.loadEmployees();
                });
            }
          }
        })
      )
    );
  }
  perFormReverseTransaction() {
    const reverseTransaction = {
      timeWorkAccountId: this.form?.value.timeWorkAccountId,
      timeWorkAccountYearId: this.form?.value.timeWorkAccountYearId,
      timeWorkAccountYearEmployeeIds: this.getSelectedEmployees(),
    } as ITimeWorkAccountYearEmployeeModel;

    this.performLoad.load(
      this.timeWorkAccountService.reverseTransaction(reverseTransaction).pipe(
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
            if (this.performLoad.data.length == 0) {
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
                  this.loadEmployees();
                });
            }
          }
        })
      )
    );
  }

  getSelectedEmployees() {
    const rows = this.subGrid.getSelectedRows();
    const timeWorkAccountYearEmployeeIds: number[] = [];
    if (rows.length != 0) {
      rows.forEach(r => {
        timeWorkAccountYearEmployeeIds.push(r.timeWorkAccountYearEmployeeId);
      });
    } else {
      this.rows.forEach(r => {
        timeWorkAccountYearEmployeeIds.push(r.timeWorkAccountYearEmployeeId);
      });
    }
    return timeWorkAccountYearEmployeeIds;
  }

  withdrawalMethodToSmallGenericType(
    method: TermGroup_TimeWorkAccountWithdrawalMethod
  ): SmallGenericType {
    const matchedMethod = this.withdrawalMethods.find(e => e.id == method);
    const type = new SmallGenericType(
      matchedMethod?.id ?? TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed,
      matchedMethod?.name ?? ''
    );

    return type;
  }

  withDrawalMethodName(
    value: TermGroup_TimeWorkAccountWithdrawalMethod
  ): string {
    const matchedMethod = this.withdrawalMethods.find(e => e.id == value);
    return matchedMethod?.name ?? '';
  }

  setData(newYear = false, timeWorkAccountYearId = 0) {
    if (newYear) {
      this.performLoad.load(
        this.timeWorkAccountService
          .getLastYear(this.data.timeWorkAccountId, true)
          .pipe(
            tap(x => {
              if (x != null && x.timeWorkAccountId != 0) {
                x.timeWorkAccountYearId = 0;
                this.form = this.createForm(x, true);
                this.form?.customPatchValue(x);
                this.form?.markAsDirty();
                this.form.isNew = true;
              } else {
                const emptyYear = new TimeWorkAccountYearDTO();
                emptyYear.timeWorkAccountId = this.data.timeWorkAccountId;
                emptyYear.timeWorkAccountYearId = 0;
                this.form = this.createForm(emptyYear, true);
                this.form?.markAsDirty();
                this.form.isNew = true;
              }
            })
          )
      );
    } else {
      this.performLoad.load(
        this.timeWorkAccountService
          .getYear(
            timeWorkAccountYearId,
            this.data.timeWorkAccountId,
            true,
            true
          )
          .pipe(
            tap(x => {
              this.form = this.createForm(x, true);
              this.form?.customPatchValue(x);
              this.loadEmployees();
              this.form?.markAsDirty();
            })
          )
      );
    }
  }

  subgridChangeWithdrawalMethod(row: any) {
    if (row.data) {
      const obj = this.withdrawalMethods.find((d: any) => {
        return d.id == row.data.selectedWithdrawalMethod;
      });
      if (obj) {
        if (obj['id']) row.data['selectedWithdrawalMethod'] = obj['id'];
      }
    }
  }

  loadEmployees(deleted = false) {
    if (deleted)
      this.toasterService.success(this.translate.instant('core.deleted'));
    if (
      !this.form?.value.timeWorkAccountYearId ||
      this.form?.value.timeWorkAccountYearId == 0
    ) {
      return of(undefined);
    } else {
      return of(
        this.performLoad.load(
          this.timeWorkAccountService
            .getYear(
              this.form?.value.timeWorkAccountYearId,
              this.form?.value.timeWorkAccountId,
              true,
              true
            )
            .pipe(
              tap(res => {
                this.form?.customPatchValue(res);
                this.rows = res.timeWorkAccountYearEmployees;
                this.setupRows();
                this.checkSelection();
              })
            )
        )
      );
    }
  }

  editWorkTimeWeek(index?: number) {
    const workTimeWeek =
      index == null
        ? undefined
        : this.form?.timeWorkAccountWorkTimeWeeks.at(index);

    this.dialogServiceV2
      .open(TimeWorkAccountYearWorkTimeWeekEditComponent, {
        title: workTimeWeek ? 'core.edit' : 'common.new',
        form: workTimeWeek,
        index,
        hideFooter: true,
      } as ITimeWorkAccountYearWorkTimeWeekEventDialogData)
      .afterClosed()
      .pipe(take(1))
      .subscribe((res: ITimeWorkAccountYearWorkTimeWeekEventObject) => {
        if (res.action == CrudActionTypeEnum.Save && res.object != undefined) {
          res.object.value.paidLeaveTime = this.convertHoursToHinutes(
            res.object.value.paidLeaveTime
          );
          res.object.value.workTimeWeekFrom = this.convertHoursToHinutes(
            res.object.value.workTimeWeekFrom
          );
          res.object.value.workTimeWeekTo = this.convertHoursToHinutes(
            res.object.value.workTimeWeekTo
          );
          if (res.index == undefined) {
            this.form?.timeWorkAccountWorkTimeWeeks.push(res.object);
          } else {
            this.form?.timeWorkAccountWorkTimeWeeks
              .at(res.index)
              .setValue(res.object.value);
          }

          this.form?.markAsDirty();
        }
      });
  }
  deleteWorkTimeWeek(index: number) {
    this.form?.timeWorkAccountWorkTimeWeeks.removeAt(index);
    this.form?.markAsDirty();
  }

  loadInformation(info: TimeWorkAccountYearEmployeeDTO) {
    if (info.status == TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome) {
      this.performLoad.load(
        this.timeWorkAccountService
          .getPaymentDate(
            this.form?.value.timeWorkAccountYearId,
            info?.timeWorkAccountYearEmployeeId
          )
          .pipe(
            tap(res => {
              let paymentDate = res != null ? res.toString().split('T')[0] : '';
              if (paymentDate != '')
                paymentDate =
                  this.translate.instant('time.time.timeperiod.paymentdate') +
                  ': ' +
                  paymentDate;

              this.showInformation(info, paymentDate);
            })
          )
      );
    } else {
      this.showInformation(info, '');
    }
  }

  showInformation(info: TimeWorkAccountYearEmployeeDTO, paymentDate: string) {
    this.performLoad.load(
      this.timeWorkAccountService
        .loadCalculationBasis(
          info?.timeWorkAccountYearEmployeeId,
          info?.employeeId
        )
        .pipe(
          tap(res => {
            const hasPaymentDate = res.some(r => r.paymentDate);
            const row: object[] = [];
            const header: object[] = [];
            header.push(
              { header: this.translate.instant('common.name') },
              ...(hasPaymentDate
                ? [
                    {
                      header: this.translate.instant(
                        'time.time.timeperiod.paymentdate'
                      ),
                    },
                  ]
                : []),
              { header: this.translate.instant('common.date') },
              {
                header: this.translate.instant(
                  'time.employee.employment.percent'
                ),
              },
              { header: this.translate.instant('common.amount') },
              {
                header: this.translate.instant(
                  'time.employee.employment.baseworktimeweek'
                ),
              },
              {
                header: this.translate.instant(
                  'time.payroll.worktimeaccount.employee.hasabsenceejsemgr'
                ),
              },
              {
                header: this.translate.instant(
                  'time.payroll.worktimeaccount.employee.yearearningdays '
                ),
              }
            );
            res.forEach(r => {
              row.push({
                text: [
                  info.employeeName,
                  ...(hasPaymentDate
                    ? [r.paymentDate?.toFormattedDate() ?? '']
                    : []),
                  r.date.toFormattedDate(),
                  r.employmentPercent,
                  r.amount,
                  r.ruleWorkTimeWeek
                    ? Math.round((r.ruleWorkTimeWeek / 60) * 100) / 100
                    : 0,
                  (r.unpaidAbsenceRatio > 0
                    ? Math.round(r.unpaidAbsenceRatio * 100)
                    : 0) + '%',
                  r.yearEarningDays,
                ],
              });
            });
            this.dialogServiceV2.open(TimeWorkAccountOutputComponent, {
              title: this.translate.instant('core.info'),
              topText: paymentDate,
              row: row,
              header: header,
              export: true,
              size: 'lg',
              hideFooter: true,
            } as ITimeWorkAccountOutputDialogData);
          })
        )
    );
  }
  deleteRow(row: TimeWorkAccountYearEmployeeDTO) {
    const mb = this.messageboxService.warning(
      'core.warning',
      'core.deleterowwarning'
    );

    mb.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response?.result) this.performDeleteRow(row);
    });
  }

  performDeleteRow(row: TimeWorkAccountYearEmployeeDTO) {
    if (row.status == TermGroup_TimeWorkAccountYearEmployeeStatus.Calculated) {
      this.timeWorkAccountService
        .deleteTimeWorkAccountYearEmployeeRow(
          this.form?.value.timeWorkAccountYearId,
          row?.timeWorkAccountYearEmployeeId,
          row?.employeeId
        )
        .pipe(take(1))

        .subscribe(res => {
          if (!res.success) {
            this.showError(res);
          } else {
            this.loadEmployees(true);
          }
        });
    }
  }
  checkSelection() {
    this.highestStatus = 0;
    this.lowestStatus = 10;
    this.hasSentDate = false;
    let rows = this.subGrid?.getSelectedRows();

    if (rows === undefined || rows.length == 0) {
      rows = this.rows;
    }

    rows.forEach(row => {
      if (this.highestStatus < row.status) this.highestStatus = row.status;
      if (this.lowestStatus > row.status) this.lowestStatus = row.status;
      if (
        row.selectedWithdrawalMethod ==
        TermGroup_TimeWorkAccountWithdrawalMethod.PaidLeave
      )
        this.hasPaidLeave = true;
      if (row.sentDate != undefined) this.hasSentDate = true;
      if (
        row.selectedWithdrawalMethod ==
          TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit ||
        ((this.data?.defaultPaidLeaveNotUsed ==
          TermGroup_TimeWorkAccountWithdrawalMethod.PensionDeposit ||
          this.data?.defaultPaidLeaveNotUsed ==
            TermGroup_TimeWorkAccountWithdrawalMethod.NotChoosed) &&
          (row.status ==
            TermGroup_TimeWorkAccountYearEmployeeStatus.PaidBalance ||
            row.status == TermGroup_TimeWorkAccountYearEmployeeStatus.Outcome))
      )
        this.hasPension = true;
    });
    this.buildFunctionList();
  }

  setDisabled(row: any, status: TermGroup_TimeWorkAccountYearEmployeeStatus) {
    return row?.status < status;
  }
  convertHoursToHinutes(value: number) {
    return Math.round(value * 60);
  }

  convertMinutesToHour(value: number) {
    return Math.round((value / 60) * 100) / 100;
  }
}
