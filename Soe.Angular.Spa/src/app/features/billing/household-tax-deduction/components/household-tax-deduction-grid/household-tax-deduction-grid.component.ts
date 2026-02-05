import { Component, inject, Input, OnInit, signal } from '@angular/core';
import { HouseholdTaxDeductionDialogComponent } from '@shared/components/household-tax-deduction-dialog/household-tax-deduction-dialog.component';
import { HouseholdTaxDeductionApplicantDialogData } from '@shared/components/household-tax-deduction-dialog/models/household-tax-deduction-Applicant.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { SoeFormGroup, SoeSelectFormControl } from '@shared/extensions';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import {
  Feature,
  SettingMainType,
  SoeHouseholdClassificationGroup,
  SoeOriginStatusClassificationGroup,
  TermGroup,
  UserSettingType,
} from '@shared/models/generated-interfaces/Enumerations';
import {
  IHouseholdTaxDeductionApplicantDTO,
  IHouseholdTaxDeductionGridViewDTO,
} from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { IMessageboxComponentResponse } from '@ui/dialog/models/messagebox';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { filter, Observable, take, tap } from 'rxjs';
import { HouseholdTaxDeductionService } from '../../services/household-tax-deduction.service';

@Component({
  selector: 'soe-household-tax-deduction-grid',
  templateUrl: './household-tax-deduction-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class HouseholdTaxDeductionGridComponent
  extends GridBaseDirective<
    IHouseholdTaxDeductionGridViewDTO,
    HouseholdTaxDeductionService
  >
  implements OnInit
{
  // classification
  classification: number = SoeHouseholdClassificationGroup.Apply;

  deductionTypeId: number = 0;
  defaultPrintOption: number = 0;
  selectedAmount: number = 0;
  filteredAmount: number = 0;
  terms: any;
  deductionRows: IHouseholdTaxDeductionGridViewDTO[] = [];
  @Input() deductionTypes: SmallGenericType[] = [];

  // Services
  service = inject(HouseholdTaxDeductionService);
  progressService = inject(ProgressService);
  coreService = inject(CoreService);
  performLoad = new Perform<any>(this.progressService);
  performSave = new Perform<any>(this.progressService);
  messageboxService = inject(MessageboxService);
  dialogService = inject(DialogService);

  validationHandler = inject(ValidationHandler);
  perform = new Perform<any>(this.progressService);

  // Signals
  protected applyButtonDisabled = signal(true);

  // Form
  form = new SoeFormGroup(this.validationHandler, {
    deductionType: new SoeSelectFormControl(this.deductionTypeId),
  });

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Invoice_Household_ROT,
      'Billing.Invoices.HouseholdDeduction.Apply',
      {
        lookups: [this.loadDeductionTypes(), this.loadUserSettings()],
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
        'common.customer.customer.rot.comment',
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
        this.grid.addColumnDate(
          'payDate',
          terms['common.customer.invoices.paydate'],
          {
            flex: 1,
            enableGrouping: true,
          }
        );
        this.grid.addColumnIcon(null, '...', {
          enableHiding: false,
          suppressExport: true,
          iconPrefix: 'fal',
          iconName: 'user-edit',
          onClick: this.editDeductionInfo.bind(this),
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

        /*this.form = new SoeFormGroup(this.validationHandler, {
          deductionType: new SoeSelectFormControl(this.deductionTypeId),
        });*/

        if (this.deductionRows.some(y => y.amount < 0)) {
          this.messageboxService.warning(
            'core.warning',
            'billing.invoices.householddeduction.creditwarning',
            {
              buttons: 'ok',
            }
          );
        }
      });
  }

  private summarize(filtered: IHouseholdTaxDeductionGridViewDTO[]) {
    this.filteredAmount = 0;
    filtered.forEach(row => {
      this.filteredAmount += row.amount;
    });
  }

  selectionChanged(data: any) {
    this.applyButtonDisabled.set(data.length <= 0);
    this.summarizeSelected();
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
      this.service
        .getGrid(undefined, {
          classification: this.classification,
          taxDeductionType: this.deductionTypeId,
        })
        .pipe(
          tap(data => {
            this.deductionRows = data;

            if (this.deductionRows.some(y => y.amount < 0)) {
              this.messageboxService.warning(
                'core.warning',
                'billing.invoices.householddeduction.creditwarning',
                {
                  buttons: 'ok',
                }
              );
            }
            return data;
          })
        )
    );
  }

  override onAfterLoadData() {
    this.summarizeFiltered();
  }

  taxDeductionTypeChanged(value: number) {
    this.deductionTypeId = value;
    this.loadData().subscribe();

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
      .getTermGroupContent(TermGroup.HouseHoldTaxDeductionType, false, false)
      .pipe(
        tap(res => {
          this.deductionTypes = res;
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
      .pipe(tap(() => this.loadData().subscribe()))
      .subscribe();
  }

  // control events
  saveApplied() {
    const selectedRows = this.grid.getSelectedRows();
    if (selectedRows.some(r => r.amount < 0)) {
      this.messageboxService.warning(
        this.translate.instant('core.warning'),
        this.translate.instant('billing.invoices.householddeduction.cantapply'),
        { buttons: 'ok' }
      );
    } else {
      const ids = selectedRows.map(r => r.customerInvoiceRowId);

      this.performSave.crud(
        CrudActionTypeEnum.Save,
        this.service.saveApplied(ids),
        () => {
          this.messageboxService.information(
            this.translate.instant(
              'billing.invoices.householddeduction.appliedsuccessheader'
            ),
            this.translate.instant(
              'billing.invoices.householddeduction.appliedsuccess'
            ),
            { buttons: 'ok' }
          );

          this.loadData().subscribe();
        }
      );
    }
  }

  // grid events
  editInvoice(row: IHouseholdTaxDeductionGridViewDTO): void {
    BrowserUtil.openInNewTab(
      window,
      `/soe/billing/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleCustomerInvoices}&invoiceId=${row.invoiceId}&invoiceNr=${row.invoiceNr}`
    );
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
                this.performSave.crud(
                  CrudActionTypeEnum.Save,
                  this.service.saveHouseholdRowForEdit(applicant).pipe(
                    tap(result => {
                      if (result.success) {
                        this.loadData().subscribe();
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
            this.loadData().subscribe();
          }
        );
      }
    });
  }
}
