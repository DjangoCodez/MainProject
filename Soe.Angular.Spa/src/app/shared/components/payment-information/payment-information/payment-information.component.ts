import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  inject,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import {
  Feature,
  TermGroup,
  TermGroup_Country,
  TermGroup_ForeignPaymentForm,
  TermGroup_ForeignPaymentIntermediaryCode,
  TermGroup_ForeignPaymentMethod,
  TermGroup_SysPaymentType,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { IPaymentInformationDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { AG_NODE, GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import {
  CellValueChangedEvent,
  IRowNode,
  RowDataUpdatedEvent,
} from 'ag-grid-community';
import { forkJoin, of, take, tap } from 'rxjs';
import { PaymentInformationForm } from '../payment-information-form.model';
import { PaymentInformationValidatorService } from '../payment-information-validator.service';
import { PaymentInformationRowDTO } from '../payment-information.model';
import { PaymentInformationService } from '../services/payment-information.service';

type FormWithPaymentInformation = SoeFormGroup & {
  paymentInformationDomestic?: PaymentInformationForm;
  paymentInformationForeign?: PaymentInformationForm;
};

@Component({
  selector: 'soe-payment-information',
  templateUrl: './payment-information.component.html',
  styleUrls: ['./payment-information.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PaymentInformationComponent
  extends GridBaseDirective<PaymentInformationRowDTO>
  implements OnChanges, OnInit
{
  @Input({ required: true }) isForeign!: boolean;
  @Input({ required: true }) paymentInformation!: IPaymentInformationDTO;
  @Input() isCompany: boolean | undefined;
  @Input() form: FormWithPaymentInformation | undefined;

  @Output() paymentInformationChange =
    new EventEmitter<IPaymentInformationDTO>();

  readonly coreService = inject(CoreService);
  readonly paymentInformationValidatorService = inject(
    PaymentInformationValidatorService
  );

  paymentCodes: ISmallGenericType[] = [];
  sysPaymentTypes: ISmallGenericType[] = [];
  foreignPaymentForms: ISmallGenericType[] = [];
  foreignPaymentMethod: ISmallGenericType[] = [];
  foreignPaymentChargeCode: ISmallGenericType[] = [];
  foreignPaymentIntermediaryCode: ISmallGenericType[] = [];

  paymentInformationService = inject(PaymentInformationService);
  messageBoxService = inject(MessageboxService);

  ngOnInit(): void {
    super.ngOnInit();
    this.form?.paymentInformationForeign?.setValidatorService(
      this.paymentInformationValidatorService
    );
    this.form?.paymentInformationDomestic?.setValidatorService(
      this.paymentInformationValidatorService
    );

    const gridName = `Common.Directives.PaymentInformation.${this.isForeign ? 'Foreign' : 'Domestic'}`;
    this.startFlow(Feature.None, gridName, {
      skipInitialLoad: true,
      lookups: [this.executeLookups()],
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.paymentInformation && changes.paymentInformation.currentValue) {
      this.rowData.next(this.paymentInformation?.rows || []);
    }
  }

  executeLookups() {
    return forkJoin([
      this.coreService.getTermGroupContent(
        TermGroup.SysPaymentType,
        false,
        false,
        false,
        true
      ),
      this.isForeign
        ? this.coreService.getTermGroupContent(
            TermGroup.CentralBankCode,
            true,
            false,
            false,
            true
          )
        : of([]),
      this.coreService.getTermGroupContent(
        TermGroup.ForeignPaymentForm,
        false,
        false,
        false,
        true
      ),
      this.coreService.getTermGroupContent(
        TermGroup.ForeignPaymentMethod,
        false,
        false,
        false,
        true
      ),
      this.coreService.getTermGroupContent(
        TermGroup.ForeignPaymentChargeCode,
        false,
        false,
        false,
        true
      ),
      this.coreService.getTermGroupContent(
        TermGroup.ForeignPaymentIntermediaryCode,
        false,
        false,
        false,
        true
      ),
    ]).pipe(
      tap(
        ([
          sysPaymentTypes,
          paymentCodes,
          foreignPaymentForms,
          foreignPaymentMethod,
          foreignPaymentChargeCode,
          foreignPaymentIntermediaryCode,
        ]) => {
          this.sysPaymentTypes = sysPaymentTypes;
          this.paymentCodes = paymentCodes.map(x =>
            x.id === 0 ? x : { id: x.id, name: `${x.id} - ${x.name}` }
          );
          this.foreignPaymentForms = foreignPaymentForms;
          this.foreignPaymentMethod = foreignPaymentMethod;
          this.foreignPaymentChargeCode = foreignPaymentChargeCode;
          this.foreignPaymentIntermediaryCode = foreignPaymentIntermediaryCode;
        }
      )
    );
  }

  override onGridReadyToDefine(grid: GridComponent<PaymentInformationRowDTO>) {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions<AG_NODE<PaymentInformationRowDTO>>({
      onRowDataUpdated: this.onDataUpdated.bind(this),
      onCellValueChanged: this.onCellChanged.bind(this),
    });

    this.grid.agGrid.api.sizeColumnsToFit(); //Waybe should an attribute directive linked to the grid component?

    this.translate
      .get([
        'core.remove',
        'economy.supplier.supplier.actorpayment',
        'economy.supplier.supplier.paymenttype',
        'economy.supplier.supplier.account',
        'economy.supplier.supplier.checkbox',
        'economy.supplier.supplier.defaultwithinpayment',
        'economy.supplier.supplier.bic',
        'economy.supplier.supplier.accountcode',
        'economy.supplier.supplier.accountcurrency',
        'economy.supplier.supplier.paymentcode',
        'economy.supplier.supplier.paymentintermediary',
        'economy.supplier.supplier.paymentform',
        'economy.supplier.supplier.paymentmethod',
        'economy.supplier.supplier.handlingfee',
        'economy.supplier.supplier.changingofbankaccountnumber',
        'common.standard',
        'economy.supplier.supplier.iban',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        if (!this.isForeign) {
          this.grid.addColumnSelect(
            'sysPaymentTypeId',
            terms['economy.supplier.supplier.paymenttype'],
            this.sysPaymentTypes,
            null,
            {
              flex: 50,
              editable: true,
            }
          );
        }
        this.grid.addColumnText('bic', terms['economy.supplier.supplier.bic'], {
          flex: 50,
          editable: true,
        });
        this.grid.addColumnText(
          'paymentNr',
          this.isForeign
            ? terms['economy.supplier.supplier.iban']
            : terms['economy.supplier.supplier.account'],
          {
            flex: 50,
            editable: true,
          }
        );
        if (this.isForeign) {
          this.grid.addColumnText(
            'clearingCode',
            terms['economy.supplier.supplier.accountcode'],
            {
              flex: 50,
              editable: true,
            }
          );
          this.grid.addColumnText(
            'currencyAccount',
            terms['economy.supplier.supplier.accountcurrency'],
            {
              flex: 50,
              editable: true,
            }
          );
          if (SoeConfigUtil.sysCountryId === TermGroup_Country.SE) {
            this.grid.addColumnSelect(
              <any>'paymentCode',
              terms['economy.supplier.supplier.paymentcode'],
              this.paymentCodes,
              null,
              {
                flex: 50,
                editable: true,
              }
            );
          } else {
            this.grid.addColumnText(
              'paymentCode',
              terms['economy.supplier.supplier.paymentcode'],
              {
                flex: 50,
                editable: true,
              }
            );
          }
          this.grid.addColumnSelect(
            'intermediaryCode',
            terms['economy.supplier.supplier.paymentintermediary'],
            this.foreignPaymentIntermediaryCode,
            null,
            {
              flex: 50,
              editable: true,
            }
          );
          this.grid.addColumnSelect(
            'paymentForm',
            terms['economy.supplier.supplier.paymentform'],
            this.foreignPaymentForms,
            null,
            {
              flex: 50,
              editable: true,
            }
          );
          this.grid.addColumnSelect(
            'paymentMethodCode',
            terms['economy.supplier.supplier.paymentmethod'],
            this.foreignPaymentMethod,
            null,
            {
              flex: 50,
              editable: true,
            }
          );
          this.grid.addColumnSelect(
            'chargeCode',
            terms['economy.supplier.supplier.handlingfee'],
            this.foreignPaymentChargeCode,
            null,
            {
              flex: 50,
              editable: true,
            }
          );
        }
        this.grid.addColumnBool('default', terms['common.standard'], {
          editable: true,
          maxWidth: 50,
          onClick: (isChecked, row) => this.toggleDefault(isChecked, row),
        });
        this.grid.addColumnIconDelete({
          onClick: row => this.deleteRow(row),
        });

        if (this.isCompany) this.grid.context.suppressFiltering = true;
        this.grid.context.suppressGridMenu = true;

        super.finalizeInitGrid();
      });
  }

  toggleDefault(isChecked: boolean, row: PaymentInformationRowDTO) {
    this.rowData.pipe(take(1)).subscribe(rows => {
      this.emitRows(
        rows.map(x => ({
          ...x,
          default: (isChecked && x === row) || (!isChecked && x === rows[0]),
        }))
      );
    });
  }

  onDataUpdated(event: RowDataUpdatedEvent) {
    if (event.context.newRow) {
      const rowIndex = this.grid.api.getLastDisplayedRowIndex();
      const colKey = this.isForeign ? 'bic' : 'sysPaymentTypeId';
      this.grid.api.setFocusedCell(rowIndex, colKey);
      this.grid.api.startEditingCell({ rowIndex, colKey });
      event.context.newRow = false;
    }
  }

  onCellChanged(
    event: CellValueChangedEvent<AG_NODE<PaymentInformationRowDTO>>
  ) {
    const { colDef, newValue, oldValue } = event;
    if (newValue === oldValue) {
      return;
    }
    switch (colDef.field) {
      case 'bic':
        this.onBicChanged(newValue, event.data);
        this.updateRow(event.data);
        break;
      case 'paymentNr':
        this.onPaymentNumberChanged(newValue, event.node);
        this.updateRow(event.data);
        break;
      case 'default':
        // this.defaultChanged(newValue, event.data);
        break;
      case 'sysPaymentTypeId':
      case 'clearingCode':
      case 'currencyAccount':
      case 'paymentCode':
      case 'intermediaryCode':
      case 'paymentForm':
      case 'paymentMethodCode':
      case 'chargeCode': {
        this.updateRow(event.data);
        break;
      }
    }
    this.form?.updateValueAndValidity();
  }

  defaultChanged(newValue: boolean, row: AG_NODE<PaymentInformationRowDTO>) {
    this.rowData.pipe(take(1)).subscribe(rows => {
      rows.forEach(x => {
        x.default = false;
      });
      rows[+row.AG_NODE_ID] = { ...rows[+row.AG_NODE_ID], default: newValue };
      if (!newValue && rows.length > 0) {
        rows[0].default = true;
      }
      this.emitRows([...rows]);
    });
  }

  onPaymentNumberChanged(
    paymentNumber: string,
    row: IRowNode<AG_NODE<PaymentInformationRowDTO>>
  ) {
    const rowData = row.data!;
    if (!paymentNumber) return;
    if (paymentNumber.substring(0, 2).toLowerCase() === 'fi') {
      this.validateIban(paymentNumber, rowData).subscribe(isValid => {
        if (isValid) {
          this.getBicFromIban(paymentNumber, row);
        } else {
          this.focusPaymentNr(row.data!);
        }
      });
    } else if (
      rowData.sysPaymentTypeId === TermGroup_SysPaymentType.BIC ||
      this.isForeign
    ) {
      this.validateIbanAndBic(paymentNumber, rowData.bic, row);
    } else if (rowData.sysPaymentTypeId == TermGroup_SysPaymentType.SEPA) {
      this.validateIban(paymentNumber, rowData).subscribe();
    } else if (rowData.sysPaymentTypeId === TermGroup_SysPaymentType.BG) {
      this;
    }
  }

  validateIbanAndBic(
    iban: string,
    bic: string,
    row: IRowNode<PaymentInformationRowDTO>
  ) {
    const isValidBic = this.paymentInformationService.isValidBic(bic, true);
    if (isValidBic) {
      this.validateIban(iban, row.data!).subscribe();
    } else if (
      this.isForeign &&
      SoeConfigUtil.sysCountryId !== TermGroup_Country.FI
    ) {
      this.messageBoxService
        .error(
          'BIC',
          this.translate.instant('economy.supplier.supplier.mandatorybic')
        )
        .afterClosed()
        .subscribe(() => {
          this.focusBic(row.data!);
        });
    } else {
      this.validateIbanOrFocusCell(iban, row);
    }
  }

  onBicChanged(bic: string, row: PaymentInformationRowDTO) {
    const isValid = this.paymentInformationService.isValidBic(bic, true);
    if (!isValid) {
      this.messageBoxService
        .error(
          'BIC',
          this.translate.instant('economy.supplier.supplier.invalidbic')
        )
        .afterClosed()
        .subscribe(() => {
          this.focusBic(row);
        });
    }
  }

  validateIbanOrFocusCell(
    iban: string,
    row: IRowNode<PaymentInformationRowDTO>
  ) {
    this.paymentInformationService.isIbanValid(iban).subscribe(isValid => {
      if (!isValid) {
        this.focusPaymentNr(row.data!);
      }
    });
  }

  getBicFromIban(
    iban: string,
    row: IRowNode<AG_NODE<PaymentInformationRowDTO>>
  ) {
    return this.paymentInformationService
      .getBicFromIban(iban)
      .subscribe(bic => {
        row.data!.bic = bic;
        row.updateData(row.data!);
        this.updateRow(row.data!);
      });
  }

  validateIban(iban: string, row: PaymentInformationRowDTO) {
    return this.paymentInformationService.isIbanValid(iban).pipe(
      tap(isValid => {
        if (!isValid) {
          this.messageBoxService
            .error(
              'IBAN',
              this.translate.instant('economy.supplier.supplier.ibannotvalid')
            )
            .afterClosed()
            .subscribe(() => {
              this.focusPaymentNr(row);
            });
        }
      })
    );
  }

  focusPaymentNr(row: PaymentInformationRowDTO) {
    this.grid.api.startEditingCell({
      rowIndex: this.grid.getRowIndex(row),
      colKey: 'paymentNr',
    });
  }

  focusBic(row: PaymentInformationRowDTO) {
    this.grid.api.startEditingCell({
      rowIndex: this.grid.getRowIndex(row),
      colKey: 'bic',
    });
  }

  deleteRow(row: PaymentInformationRowDTO) {
    this.rowData.pipe(take(1)).subscribe(rows => {
      this.emitRows(rows.filter(x => x !== row));
    });
  }

  addRow() {
    const sysPaymentType = this.sysPaymentTypes.find(x =>
      this.isForeign
        ? x.id === TermGroup_SysPaymentType.BIC
        : x.id === TermGroup_SysPaymentType.BG
    );
    const foreignPaymentMethod = this.foreignPaymentMethod.find(
      x => this.isForeign && x.id === TermGroup_ForeignPaymentMethod.Normal
    );
    const foreignPaymentForm = this.foreignPaymentForms.find(
      x => this.isForeign && x.id === TermGroup_ForeignPaymentForm.Account
    );
    const foreignPaymentChargeCode = this.foreignPaymentChargeCode.find(
      x => this.isForeign && x.id === TermGroup_ForeignPaymentMethod.Normal
    );
    const foreignPaymentIntermediaryCode =
      this.foreignPaymentIntermediaryCode.find(
        x =>
          this.isForeign &&
          x.id === TermGroup_ForeignPaymentIntermediaryCode.BGC
      );

    const newRow = new PaymentInformationRowDTO({
      default: !this.paymentInformation.rows.some(x => x.default),
      sysPaymentType,
      foreignPaymentMethod,
      foreignPaymentForm,
      foreignPaymentChargeCode,
      foreignPaymentIntermediaryCode,
    });
    this.rowData.pipe(take(1)).subscribe(rows => {
      this.emitRows([...rows, newRow]);
      this.grid.options.context.newRow = true;
    });
  }

  private updateRow(changedRow: AG_NODE<PaymentInformationRowDTO>) {
    const rows = this.rowData.value;

    const row = rows[+changedRow.AG_NODE_ID];
    if (!row) return;
    rows[+changedRow.AG_NODE_ID] = { ...row, ...changedRow };
    this.emitRows(rows);
  }

  private emitRows(rows: PaymentInformationRowDTO[]) {
    this.paymentInformationChange.emit({ ...this.paymentInformation, rows });
    this.form?.markAsDirty();
  }
}
