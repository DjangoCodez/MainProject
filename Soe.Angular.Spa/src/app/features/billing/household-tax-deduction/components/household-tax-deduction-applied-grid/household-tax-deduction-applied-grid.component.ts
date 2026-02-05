import { Component, inject, OnInit, signal } from '@angular/core';
import { VoucherEditComponent } from '@features/economy/voucher/components/voucher-edit/voucher-edit.component';
import { VoucherForm } from '@features/economy/voucher/models/voucher-form.model';
import { HouseholdTaxDeductionDialogComponent } from '@shared/components/household-tax-deduction-dialog/household-tax-deduction-dialog.component';
import { HouseholdTaxDeductionApplicantDialogData } from '@shared/components/household-tax-deduction-dialog/models/household-tax-deduction-Applicant.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import {
  SoeDateFormControl,
  SoeFormGroup,
  SoeSelectFormControl,
} from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  CompanySettingType,
  Feature,
  SettingMainType,
  SoeHouseholdClassificationGroup,
  SoeOriginStatusClassificationGroup,
  SoeReportTemplateType,
  SoeReportType,
  TermGroup,
  TermGroup_HouseHoldTaxDeductionType,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IHouseholdTaxDeductionApplicantDTO,
  IHouseholdTaxDeductionGridViewDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ReportService } from '@shared/services/report.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { HouseholdDeductionGridButtonFunctions } from '@shared/util/Enumerations';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { uniq } from 'lodash';
import { filter, finalize, Observable, take, tap } from 'rxjs';
import { HouseholdTaxDeductionService } from '../../services/household-tax-deduction.service';
import { HouseholdPartialAmountModal } from './household-partial-amount-modal/household-partial-amount-modal.component';
import { HouseholdPartialAmountDialogData } from './household-partial-amount-modal/household-partial-amount-modal.model';
import { HouseholdSequenceNumberModal } from './household-sequence-number-modal/household-sequence-number-modal.component';
import { HouseholdSequenceNumberDialogData } from './household-sequence-number-modal/household-sequence-number-modal.model';
import { HouseholdTaxDeductionPrintDTO } from '@shared/models/report-print/household-tax-deduction-print.model';
import { RequestReportService } from '@shared/services/request-report.service';

@Component({
  selector: 'soe-household-tax-deduction-grid',
  templateUrl: './household-tax-deduction-applied-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class HouseholdTaxDeductionAppliedGridComponent
  extends GridBaseDirective<
    IHouseholdTaxDeductionGridViewDTO,
    HouseholdTaxDeductionService
  >
  implements OnInit
{
  // classification
  classification: number = SoeHouseholdClassificationGroup.Applied;

  deductionTypeId: number = 0;
  defaultPrintOption: number = 0;
  selectedAmount: number = 0;
  filteredAmount: number = 0;
  applyDate: Date = new Date();
  terms: any;
  deductionTypes: SmallGenericType[] = [];
  householdReportTemplateId = 0;

  // Services
  service = inject(HouseholdTaxDeductionService);
  progressService = inject(ProgressService);
  coreService = inject(CoreService);
  performLoad = new Perform<any>(this.progressService);
  performSave = new Perform<any>(this.progressService);
  messageboxService = inject(MessageboxService);
  dialogService = inject(DialogService);
  reportService = inject(ReportService);
  private readonly requestReportService = inject(RequestReportService);
  validationHandler = inject(ValidationHandler);
  perform = new Perform<any>(this.progressService);

  // Functions
  applyFunctions: MenuButtonItem[] = [];
  printFunctions: MenuButtonItem[] = [];
  selectedPrintOption: any;

  protected isPrinting: boolean = false;

  // Signals
  protected noSelectedRows = signal(true);

  // Form
  form = new SoeFormGroup(this.validationHandler, {
    deductionType: new SoeSelectFormControl(this.deductionTypeId),
    applyDate: new SoeDateFormControl(this.applyDate, { required: true }),
  });

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Invoice_Household_ROT,
      'Billing.Invoices.HouseholdDeduction.Applied',
      {
        lookups: [
          this.loadDeductionTypes(),
          this.loadUserSettings(),
          this.loadDefaultHouseholdReport(),
        ],
      }
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<IHouseholdTaxDeductionGridViewDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'billing.invoices.householddeduction.invoicenr',
        'billing.invoices.householddeduction.property',
        'billing.invoices.householddeduction.socialsecnr',
        'billing.invoices.householddeduction.name',
        'billing.invoices.householddeduction.amount',
        'billing.invoices.householddeduction.seqnbr',
        'billing.invoices.householddeduction.applieddate',
        'billing.invoices.householddeduction.receiveddate',
        'billing.invoices.householddeduction.denieddate',
        'billing.invoices.householddeduction.status',
        'billing.invoices.householddeduction.approved',
        'core.edit',
        'core.delete',
        'common.customer.invoices.showinvoice',
        'common.type',
        'common.percent',
        'common.customer.invoices.paydate',
        'billing.invoices.householddeduction.editinfo',
        'billing.invoices.householddeduction.approvedamount',
        'common.report.selection.vouchernr',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;

        this.grid.enableRowSelection();
        this.grid.addColumnSelect(
          'houseHoldTaxDeductionType',
          terms['common.type'],
          this.deductionTypes,
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 1,
            editable: false,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'invoiceNr',
          terms['billing.invoices.householddeduction.invoicenr'],
          {
            flex: 1,
            enableGrouping: true,
            buttonConfiguration: {
              iconPrefix: 'fal',
              iconName: 'pencil',
              onClick: row => this.editInvoice(row),
            },
          }
        );
        this.grid.addColumnText(
          'property',
          terms['billing.invoices.householddeduction.property'],
          {
            flex: 1,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'socialSecNr',
          terms['billing.invoices.householddeduction.socialsecnr'],
          {
            flex: 1,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'name',
          terms['billing.invoices.householddeduction.name'],
          {
            flex: 1,
            enableGrouping: true,
          }
        );
        this.grid.addColumnNumber(
          'amount',
          terms['billing.invoices.householddeduction.amount'],
          {
            showSetFilter: true,
            aggFuncOnGrouping: 'sum',
            flex: 1,
            enableGrouping: true,
            allowEmpty: true,
            decimals: 2,
          }
        );
        this.grid.addColumnText(
          'seqNr',
          terms['billing.invoices.householddeduction.seqnbr'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnDate(
          'payDate',
          terms['common.customer.invoices.paydate'],
          {
            flex: 1,
            enableGrouping: true,
          }
        );
        this.grid.addColumnDate(
          'appliedDate',
          terms['billing.invoices.householddeduction.applieddate'],
          {
            flex: 1,
            enableGrouping: true,
          }
        );
        this.grid.addColumnText(
          'houseHoldTaxDeductionPercent',
          terms['common.percent'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnIcon(null, '...', {
          enableHiding: false,
          suppressExport: true,
          iconPrefix: 'fal',
          iconName: 'user-edit',
          onClick: row => {
            this.editDeductionInfo(row);
          },
          tooltip: terms['billing.invoices.householddeduction.editinfo'],
        });
        this.grid.addColumnIcon(null, '...', {
          enableHiding: false,
          suppressExport: true,
          iconPrefix: 'fal',
          iconName: 'file-text',
          onClick: row => {
            this.showComment(row);
          },
          showIcon: row => (row.comment && row.comment.length > 0) || false,
          tooltip: terms['common.customer.customer.rot.comment'],
        });
        this.grid.addColumnIcon(null, '...', {
          enableHiding: false,
          suppressExport: true,
          iconPrefix: 'fal',
          iconName: 'leaf',
          iconClass: 'success-color',
          onClick: row => {
            this.showGreenInfo(row);
          },
          showIcon: row =>
            row.houseHoldTaxDeductionType ===
            TermGroup_HouseHoldTaxDeductionType.GREEN,
        });
        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.delete(row);
          },
        });

        this.grid.filterModified.subscribe(() => {
          this.summarizeFiltered();
        });

        super.finalizeInitGrid();

        this.summarizeFiltered();
        this.setSplitButtonFunctions();

        this.selectedPrintOption = this.printFunctions.find(
          (f: any) => f.id === this.defaultPrintOption
        );
      });
  }

  private setSplitButtonFunctions() {
    this.applyFunctions = [];
    this.printFunctions = [];

    this.translate
      .get([
        'billing.invoices.householddeduction.approve',
        'billing.invoices.householddeduction.deny',
        'billing.invoices.householddeduction.savexml',
        'common.printreport',
        'billing.invoices.householddeduction.approvepartial',
        'billing.invoices.householddeduction.withdraw',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.applyFunctions.push({
          id: HouseholdDeductionGridButtonFunctions.SaveReceived,
          label: terms['billing.invoices.householddeduction.approve'],
          icon: 'save',
        });
        this.applyFunctions.push({
          id: HouseholdDeductionGridButtonFunctions.SavePartiallyApproved,
          label: terms['billing.invoices.householddeduction.approvepartial'],
          icon: 'save',
        });
        this.applyFunctions.push({
          id: HouseholdDeductionGridButtonFunctions.SaveDenied,
          label: terms['billing.invoices.householddeduction.deny'],
          icon: 'save',
        });
        this.applyFunctions.push({
          id: HouseholdDeductionGridButtonFunctions.WithdrawApplied,
          label: terms['billing.invoices.householddeduction.withdraw'],
          icon: 'save',
        });

        this.printFunctions.push({
          id: HouseholdDeductionGridButtonFunctions.Print,
          label: terms['common.printreport'],
          icon: 'print',
        });
        this.printFunctions.push({
          id: HouseholdDeductionGridButtonFunctions.SaveXML,
          label: terms['billing.invoices.householddeduction.savexml'],
          icon: 'print',
        });
      });
  }

  selectionChanged(data: any) {
    this.noSelectedRows.set(data.length <= 0);
    this.summarizeSelected();
  }

  private summarize(filtered: IHouseholdTaxDeductionGridViewDTO[]) {
    this.filteredAmount = 0;
    filtered.forEach(row => {
      this.filteredAmount += row.amount;
    });
  }

  private summarizeFiltered() {
    const filtered = this.grid.getFilteredRows();
    this.filteredAmount = 0;
    filtered.forEach(row => {
      this.filteredAmount += row.amount;
    });
  }

  summarizeSelected() {
    this.selectedAmount = 0;
    const rows = this.grid.getSelectedRows();
    rows.forEach(row => {
      this.selectedAmount += row.amount;
    });
  }

  override loadData(
    id?: number | undefined
  ): Observable<IHouseholdTaxDeductionGridViewDTO[]> {
    return this.performLoad.load$(
      this.service.getGrid(undefined, {
        classification: this.classification,
        taxDeductionType: this.deductionTypeId,
      })
    );
  }

  override onAfterLoadData() {
    this.summarizeFiltered();
  }

  taxDeductionTypeChanged(value: number) {
    this.deductionTypeId = value;
    this.refreshGrid();

    this.updateSetting(
      value,
      UserSettingType.BillingInvoiceDefaultHouseholdTaxType
    );
  }

  loadUserSettings() {
    const settingTypes: number[] = [];

    settingTypes.push(UserSettingType.BillingInvoiceDefaultHouseholdTaxType);
    settingTypes.push(
      UserSettingType.BillingInvoiceDefaultHouseholdPrintButtonOption
    );

    return this.coreService.getUserSettings(settingTypes).pipe(
      tap(x => {
        this.deductionTypeId = SettingsUtil.getIntUserSetting(
          x,
          UserSettingType.BillingInvoiceDefaultHouseholdTaxType,
          0
        );
        this.form.patchValue({ deductionType: this.deductionTypeId });

        this.defaultPrintOption = SettingsUtil.getIntUserSetting(
          x,
          UserSettingType.BillingInvoiceDefaultHouseholdPrintButtonOption,
          6
        );
      })
    );
  }

  loadDeductionTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.HouseHoldTaxDeductionType, false, true)
      .pipe(
        tap(res => {
          this.deductionTypes = res;
        })
      );
  }

  loadDefaultHouseholdReport() {
    return this.reportService
      .getSettingOrStandardReportId(
        SettingMainType.Company,
        CompanySettingType.BillingDefaultHouseholdDeductionTemplate,
        SoeReportTemplateType.HousholdTaxDeduction,
        SoeReportType.CrystalReport
      )
      .pipe(
        take(1),
        tap((reportId: number) => {
          this.householdReportTemplateId = reportId;
        })
      );
  }

  public updateSetting(value: number, settingType: number) {
    if (!value) return;
    const model = {
      settingMainType: SettingMainType.User,
      settingTypeId: settingType,
      intValue: value,
    };
    this.coreService
      .saveIntSetting(model)
      .pipe(tap(() => this.refreshGrid()))
      .subscribe();
  }

  // control events
  applyFunctionSelected(item: MenuButtonItem) {
    switch (item.id) {
      case HouseholdDeductionGridButtonFunctions.SaveReceived: {
        this.saveReceived();
        break;
      }
      case HouseholdDeductionGridButtonFunctions.SavePartiallyApproved: {
        this.savePartiallyApproved();
        break;
      }
      case HouseholdDeductionGridButtonFunctions.SaveDenied: {
        this.saveDenied();
        break;
      }
    }
  }

  printFunctionSelected(item: MenuButtonItem) {
    switch (item.id) {
      case HouseholdDeductionGridButtonFunctions.Print: {
        this.initPrint();
        break;
      }
      case HouseholdDeductionGridButtonFunctions.SaveXML: {
        this.initCreateFile();
        break;
      }
    }
  }

  // grid events
  editInvoice(row: IHouseholdTaxDeductionGridViewDTO): void {
    BrowserUtil.openInNewTab(
      window,
      `/soe/billing/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleCustomerInvoices}&invoiceId=${row.invoiceId}&invoiceNr=${row.invoiceNr}`
    );
  }

  editVoucher(row: IHouseholdTaxDeductionGridViewDTO): void {
    this.openEditInNewTab.emit({
      id: row.voucherHeadId as number,
      additionalProps: {
        editComponent: VoucherEditComponent,
        editTabLabel: 'economy.accounting.voucher.voucher',
        FormClass: VoucherForm,
      },
    });
  }

  showGreenInfo(row: IHouseholdTaxDeductionGridViewDTO): void {
    this.service
      .getInfo(row.invoiceId, row.customerInvoiceRowId)
      .subscribe(result => {
        let message = '';

        result.strings.forEach((s: any) => {
          if (s) {
            message += s + '\n';
          }
        });

        this.messageboxService.information(
          this.translate.instant('core.info'),
          message,
          {
            buttons: 'ok',
          }
        );
      });
  }

  showComment(row: IHouseholdTaxDeductionGridViewDTO): void {
    this.messageboxService.information(
      this.translate.instant('common.customer.customer.rot.comment'),
      row.comment,
      {
        buttons: 'ok',
      }
    );
  }

  editDeductionInfo(row: IHouseholdTaxDeductionGridViewDTO): void {
    this.perform.load(
      this.service.getHouseholdRowForEdit(row.customerInvoiceRowId).pipe(
        tap(applicant => {
          const dialogData: HouseholdTaxDeductionApplicantDialogData = {
            title: this.translate.instant(
              'billing.invoices.householddeduction.editinfo'
            ),
            size: 'lg',
            rowToUpdate: applicant,
          };
          this.dialogService
            .open(HouseholdTaxDeductionDialogComponent, dialogData)
            .afterClosed()
            .pipe(filter(value => !!value))
            .subscribe((value: IHouseholdTaxDeductionApplicantDTO) => {
              if (value) {
                applicant.apartmentNr = value.apartmentNr;
                applicant.cooperativeOrgNr = value.cooperativeOrgNr;
                applicant.name = value.name;
                applicant.socialSecNr = value.socialSecNr;
                applicant.property = value.property;
                applicant.comment = value.comment;
                this.perform.crud(
                  CrudActionTypeEnum.Save,
                  this.service.saveHouseholdRowForEdit(applicant).pipe(
                    tap(result => {
                      if (result.success) {
                        this.refreshGrid();
                      } else {
                        this.messageboxService.warning(
                          this.translate.instant('core.warning'),
                          this.translate.instant(
                            'billing.invoices.householddeduction.editinfonotsaved'
                          ),
                          { buttons: 'ok' }
                        );
                      }
                    })
                  )
                );
              }
            });
        })
      )
    );
  }

  delete(row: IHouseholdTaxDeductionGridViewDTO): void {
    const dialog = this.messageboxService.question(
      this.translate.instant('core.verifyquestion'),
      this.translate
        .instant('billing.invoices.householddeduction.deletequestion')
        .format(row.invoiceNr),
      {
        buttons: 'okCancel',
      }
    );

    dialog.afterClosed().subscribe((response: IMessageboxComponentResponse) => {
      if (response.result) {
        this.perform.crud(
          CrudActionTypeEnum.Delete,
          this.service.delete(row.customerInvoiceRowId),
          () => {
            this.refreshGrid();
          }
        );
      }
    });
  }

  // Functions
  private saveReceived(): void {
    const selectedRows = this.grid.getSelectedRows();
    if (!this.validateSelectedRows(selectedRows, true)) return;

    const dict: any = [];
    const invoiceNrs: any[] = [];
    let amount = 0;

    selectedRows.forEach(y => {
      if (y.customerInvoiceRowId > 0) {
        dict.push(y.customerInvoiceRowId);
        invoiceNrs.push(y.invoiceNr);
        amount += y.amount;
      }
    });

    this.perform.crud(
      CrudActionTypeEnum.Save,
      this.service.saveRecieved(dict, this.applyDate),
      () => {
        const dialog = this.messageboxService.question(
          this.translate.instant(
            'billing.invoices.householddeduction.recievedsuccessheader'
          ),
          this.translate.instant(
            'billing.invoices.householddeduction.recievedsuccess'
          ),
          {
            buttons: 'yesNo',
          }
        );

        dialog
          .afterClosed()
          .subscribe((response: IMessageboxComponentResponse) => {
            if (response.result) {
              this.openEditInNewTab.emit({
                id: 0,
                additionalProps: {
                  editComponent: VoucherEditComponent,
                  editTabLabel: 'economy.accounting.voucher.voucher',
                  FormClass: VoucherForm,
                  createHousehold: true,
                  date: this.applyDate,
                  amount: amount,
                  ids: dict,
                  nbrs: invoiceNrs,
                  productId: selectedRows[0].productId,
                },
              });
            }
          });

        this.refreshGrid();
      }
    );
  }

  private savePartiallyApproved(): void {
    const selectedRows = this.grid.getSelectedRows();
    if (selectedRows.length > 1 || selectedRows.length === 0) return;

    const row = selectedRows[0];

    const dialogData: HouseholdPartialAmountDialogData = {
      title: this.translate.instant(
        'billing.invoices.householddeduction.approvepartial'
      ),
      size: 'sm',
      amount: row.amount,
      createInvoice: false,
    };
    this.dialogService
      .open(HouseholdPartialAmountModal, dialogData)
      .afterClosed()
      .pipe(filter(value => !!value))
      .subscribe((value: any) => {
        if (value) {
          this.perform.crud(
            CrudActionTypeEnum.Save,
            this.service
              .savePartiallyApproved(
                row.customerInvoiceRowId,
                value.amount,
                this.applyDate
              )
              .pipe(
                tap(result => {
                  if (result.success) {
                    const dialog = this.messageboxService.question(
                      this.translate.instant(
                        'billing.invoices.householddeduction.recievedsuccessheader'
                      ),
                      this.translate.instant(
                        'billing.invoices.householddeduction.recievedsuccess'
                      ),
                      {
                        buttons: 'yesNo',
                      }
                    );

                    dialog
                      .afterClosed()
                      .subscribe((response: IMessageboxComponentResponse) => {
                        if (response.result) {
                          this.openEditInNewTab.emit({
                            id: 0,
                            additionalProps: {
                              editComponent: VoucherEditComponent,
                              editTabLabel:
                                'economy.accounting.voucher.voucher',
                              FormClass: VoucherForm,
                              createHousehold: true,
                              date: this.applyDate,
                              amount: value.amount,
                              ids: [row.customerInvoiceRowId],
                              nbrs: [row.invoiceNr],
                              productId: selectedRows[0].productId,
                            },
                          });
                        }
                      });

                    if (value.createInvoice) {
                      console.log('not implemented yet');
                    }

                    this.refreshGrid();
                  }
                })
              )
          );
        }
      });
  }

  private saveDenied(): void {
    const selectedRows = this.grid.getSelectedRows();
    if (selectedRows.length > 1 || selectedRows.length === 0) return;

    const row = selectedRows[0];

    this.perform.crud(
      CrudActionTypeEnum.Save,
      this.service.saveDenied(
        row.invoiceId,
        row.customerInvoiceRowId,
        this.applyDate
      ),
      () => {
        const dialog = this.messageboxService.warning(
          this.translate.instant(
            'billing.invoices.householddeduction.deniedsuccessheader'
          ),
          this.translate.instant(
            'billing.invoices.householddeduction.deniedsuccess'
          ),
          {
            buttons: 'okCancel',
          }
        );

        dialog
          .afterClosed()
          .subscribe((response: IMessageboxComponentResponse) => {
            if (response.result) {
              console.log('not implemented yet');
            }
          });

        this.refreshGrid();
      }
    );
  }

  // control events
  withdrawApplied() {
    const ids = this.grid.getSelectedRows().map(r => r.customerInvoiceRowId);

    this.performSave.crud(
      CrudActionTypeEnum.Save,
      this.service.withdrawApplied(ids),
      () => {
        this.messageboxService.information(
          this.translate.instant(
            'billing.invoices.householddeduction.withdrawsuccess'
          ),
          this.translate.instant(
            'billing.invoices.householddeduction.withdrawsuccess'
          ),
          { buttons: 'ok' }
        );

        this.loadData().subscribe();
      }
    );
  }

  private initPrint(): void {
    const selectedRows = this.grid.getSelectedRows();
    if (!this.validateSelectedRows(selectedRows, false)) return;

    this.translate
      .get([
        'core.info',
        'billing.invoices.householddeduction.printreportmessage',
        'billing.invoices.householddeduction.printreportmessage.rotrut',
        'common.printreport',
        'billing.invoices.householddeduction.printreportmessage.green',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        const green =
          selectedRows[0].houseHoldTaxDeductionType ===
          TermGroup_HouseHoldTaxDeductionType.GREEN;
        let message =
          terms['billing.invoices.householddeduction.printreportmessage'] +
          '\n';
        message += green
          ? terms[
              'billing.invoices.householddeduction.printreportmessage.green'
            ]
          : terms[
              'billing.invoices.householddeduction.printreportmessage.rotrut'
            ];

        const dialog = this.messageboxService.question(
          terms['core.info'],
          message,
          {
            buttons: 'okCancel',
          }
        );

        dialog
          .afterClosed()
          .subscribe((response: IMessageboxComponentResponse) => {
            if (response.result) {
              this.checkExistingSequenceNumber(
                SoeReportTemplateType.HousholdTaxDeduction,
                selectedRows
              );
            }
          });
      });
  }

  private initCreateFile(): void {
    const selectedRows = this.grid.getSelectedRows();
    if (!this.validateSelectedRows(selectedRows, false)) return;

    this.checkExistingSequenceNumber(
      SoeReportTemplateType.HouseholdTaxDeductionFile,
      selectedRows
    );
  }

  // Validation
  private validateSelectedRows(
    selectedRows: any[],
    checkUniqueProductId: boolean
  ): boolean {
    const uniqueTypes: number[] = uniq(
      selectedRows.map(a => a.houseHoldTaxDeductionType)
    );
    const uniqueProductIds: number[] = uniq(selectedRows.map(a => a.productId));

    if (
      uniqueTypes.length > 1 ||
      (checkUniqueProductId && uniqueProductIds.length > 1)
    ) {
      this.messageboxService.error(
        this.translate.instant('core.warning'),
        this.translate.instant(
          'billing.invoices.householddeduction.sametypemessage'
        ),
        {
          buttons: 'ok',
        }
      );
      return false;
    }

    return true;
  }

  private checkExistingSequenceNumber(
    templateType: SoeReportTemplateType,
    selectedRows: any[]
  ): void {
    if (selectedRows.length === 0) return;

    const taxDeductionType = selectedRows[0].houseHoldTaxDeductionType;

    if (selectedRows.filter(r => r.seqNr && r.seqNr > 0).length > 0) {
      const dialog = this.messageboxService.question(
        this.translate.instant('core.warning'),
        this.translate.instant(
          'billing.invoices.householddeduction.existingseqnrmessage'
        ),
        {
          buttons: 'okCancel',
        }
      );

      dialog
        .afterClosed()
        .subscribe((response: IMessageboxComponentResponse) => {
          if (response.result) {
            this.showSequenceNumberDialog(templateType, taxDeductionType);
          }
        });
    } else {
      this.showSequenceNumberDialog(templateType, taxDeductionType);
    }
  }

  private showSequenceNumberDialog(
    templateType: SoeReportTemplateType,
    taxDeductionType: TermGroup_HouseHoldTaxDeductionType
  ) {
    let entityName = '';
    switch (taxDeductionType) {
      case TermGroup_HouseHoldTaxDeductionType.ROT:
        entityName = 'HouseholdTaxDeduction';
        break;
      case TermGroup_HouseHoldTaxDeductionType.RUT:
        entityName = 'RutTaxDeduction';
        break;
      case TermGroup_HouseHoldTaxDeductionType.GREEN:
        entityName = 'GreenTaxDeduction';
        break;
    }

    const dialogData: HouseholdSequenceNumberDialogData = {
      title: this.translate.instant(
        'billing.invoices.householddeduction.setseqnrdialogheader'
      ),
      sequenceNumber: 0,
      entityName: entityName,
      size: 'md',
    };
    this.dialogService
      .open(HouseholdSequenceNumberModal, dialogData)
      .afterClosed()
      .subscribe(value => {
        if (value && value > 0) {
          this.printReport(templateType, taxDeductionType, value);
        }
      });
  }

  private printReport(
    templateType: SoeReportTemplateType,
    taxDeductionType: TermGroup_HouseHoldTaxDeductionType,
    sequenceNumber: number
  ) {
    const dict: any = [];
    const selectedRows = this.grid.getSelectedRows();
    selectedRows.forEach(y => {
      if (y.customerInvoiceRowId > 0) {
        dict.push(y.customerInvoiceRowId);
      }
    });

    if (templateType === SoeReportTemplateType.HousholdTaxDeduction) {
      const reportItem = new HouseholdTaxDeductionPrintDTO(dict);
      reportItem.sysReportTemplateTypeId = templateType;
      reportItem.sequenceNumber = sequenceNumber;
      reportItem.useGreen =
        taxDeductionType === TermGroup_HouseHoldTaxDeductionType.GREEN;

      this.isPrinting = true;
      this.perform.load(
        this.requestReportService.printHouseholdTaxDeduction(reportItem).pipe(
          finalize(() => {
            this.isPrinting = false;
          })
        )
      );
    } else {
      this.perform.load(
        this.service
          .getHouseholdTaxDeductionPrintUrl(
            dict,
            this.householdReportTemplateId,
            templateType,
            sequenceNumber,
            taxDeductionType === TermGroup_HouseHoldTaxDeductionType.GREEN
          )
          .pipe(
            tap(url => {
              BrowserUtil.openInSameTab(window, url);
            })
          )
      );
    }
  }
}
