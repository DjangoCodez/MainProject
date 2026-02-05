import { Component, inject, OnInit } from '@angular/core';
import { VoucherEditComponent } from '@features/economy/voucher/components/voucher-edit/voucher-edit.component';
import { VoucherForm } from '@features/economy/voucher/models/voucher-form.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
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
import { IHouseholdTaxDeductionGridViewDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { SettingsUtil } from '@shared/util/settings-util';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import { HouseholdTaxDeductionService } from '../../services/household-tax-deduction.service';

@Component({
  selector: 'soe-household-tax-deduction-grid',
  templateUrl: './household-tax-deduction-received-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class HouseholdTaxDeductionReceivedGridComponent
  extends GridBaseDirective<
    IHouseholdTaxDeductionGridViewDTO,
    HouseholdTaxDeductionService
  >
  implements OnInit
{
  // classification
  classification: number = SoeHouseholdClassificationGroup.Received;

  deductionTypeId: number = 0;
  defaultPrintOption: number = 0;
  selectedAmount: number = 0;
  filteredAmount: number = 0;
  terms: any;
  deductionTypes: SmallGenericType[] = [];

  validationHandler = inject(ValidationHandler);
  service = inject(HouseholdTaxDeductionService);
  progressService = inject(ProgressService);
  coreService = inject(CoreService);
  messageboxService = inject(MessageboxService);
  performLoad = new Perform<any>(this.progressService);

  // Form
  form = new SoeFormGroup(this.validationHandler, {
    deductionType: new SoeSelectFormControl(this.deductionTypeId),
  });

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Invoice_Household_ROT,
      'Billing.Invoices.HouseholdDeduction.Received',
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
          'voucherNr',
          terms['common.report.selection.vouchernr'],
          {
            flex: 1,
            enableGrouping: true,
            buttonConfiguration: {
              iconPrefix: 'fal',
              iconName: 'pencil',
              onClick: row => this.editVoucher(row),
              show: row => Boolean(row.voucherHeadId),
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
        this.grid.addColumnDate(
          'receivedDate',
          terms['billing.invoices.householddeduction.receiveddate'],
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
        this.grid.addColumnNumber(
          'amount',
          terms['billing.invoices.householddeduction.approvedamount'],
          {
            showSetFilter: true,
            aggFuncOnGrouping: 'sum',
            flex: 1,
            decimals: 2,
          }
        );
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
        this.grid.addColumnIcon('statusIcon', '...', {
          enableHiding: false,
          suppressExport: true,
          suppressFilter: true,
          iconPrefix: 'fas',
          tooltipField: 'householdStatus',
          iconClassField: 'statusIconClass',
        });

        this.grid.filterModified.subscribe(() => {
          this.summarizeFiltered();
        });

        super.finalizeInitGrid();

        this.summarizeFiltered();
      });
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

  showComment(row: IHouseholdTaxDeductionGridViewDTO): void {
    this.messageboxService.information(
      this.translate.instant('common.customer.customer.rot.comment'),
      row.comment,
      {
        buttons: 'ok',
      }
    );
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

  // grid events
  editInvoice(row: IHouseholdTaxDeductionGridViewDTO): void {
    BrowserUtil.openInNewTab(
      window,
      `/soe/billing/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleCustomerInvoices}&invoiceId=${row.invoiceId}&invoiceNr=${row.invoiceNr}`
    );
  }

  editVoucher(row: IHouseholdTaxDeductionGridViewDTO): void {
    console.log('open voucher', row);
    this.openEditInNewTab.emit({
      id: row.voucherHeadId as number,
      additionalProps: {
        editComponent: VoucherEditComponent,
        editTabLabel: 'economy.accounting.voucher.voucher',
        FormClass: VoucherForm,
      },
    });
  }
}
