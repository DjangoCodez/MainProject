import { Component, inject, Input, OnInit, signal } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { BehaviorSubject, take, tap } from 'rxjs';
import {
  SysBankDTO,
  SysCompanyBankAccountDTO,
} from 'src/app/features/manage/models/sysCompany.model';
import { SysCompanyForm } from '../../../models/sys-company-form.model';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { CellValueChangedEvent } from 'ag-grid-community';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { SysCompanyService } from '../../../services/sys-company.service';
import { AG_NODE, GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-sys-company-bank-accounts-grid',
  templateUrl: './sys-company-bank-accounts-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SysCompanyBankAccountsGridComponent
  extends GridBaseDirective<SysCompanyBankAccountDTO>
  implements OnInit
{
  @Input({ required: true }) bankAccounts = new BehaviorSubject<
    SysCompanyBankAccountDTO[]
  >([]);
  @Input({ required: true }) form: SysCompanyForm | undefined;
  private readonly sysCompanyService = inject(SysCompanyService);
  private readonly coreService = inject(CoreService);
  private readonly performLoad = new Perform(this.progressService);

  sysBanks: SysBankDTO[] = [];
  accountTypes: SmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Manage_System,
      'Soe.Manage.System.SysCompany.SysCompany.BankAccounts',
      {
        skipInitialLoad: true,
        lookups: [this.loadSysBanks(), this.loadAccountTypes()],
      }
    );
    setTimeout(() => {
      console.log(this.flowHandler.allowFetchGrid());
    }, 10_000);
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideReload: true,
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('newrow', {
          iconName: signal('plus'),
          caption: signal('common.newrow'),
          tooltip: signal('common.newrow'),
          onAction: () => this.onToolbarButtonClick(),
        }),
      ],
    });
  }

  private loadSysBanks() {
    return this.sysCompanyService
      .getSysBanks()
      .pipe(tap(x => (this.sysBanks = x)));
  }

  private loadAccountTypes() {
    return this.coreService
      .getTermGroupContent(
        TermGroup.ISOPaymentAccountType,
        false,
        true,
        true,
        true
      )
      .pipe(tap(x => (this.accountTypes = x)));
  }

  onToolbarButtonClick(): void {
    const row = new SysCompanyBankAccountDTO();
    this.form?.addBankAccount(row);
    this.grid?.addRow(row);
    this.grid?.clearSelectedRows();
  }

  override onGridReadyToDefine(
    grid: GridComponent<SysCompanyBankAccountDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellValueChanged.bind(this),
    });

    this.translate
      .get([
        'manage.system.syscompany.sysbank',
        'manage.system.syscompany.accounttype',
        'manage.system.syscompany.accountnr',
        'core.delete',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnModified('isModifed');

        this.grid.addColumnSelect(
          'sysBankId',
          terms['manage.system.syscompany.sysbank'],
          this.sysBanks,
          undefined,
          {
            dropDownIdLabel: 'sysBankId',
            dropDownValueLabel: 'nameWithBic',
            flex: 1,
            editable: true,
          }
        );

        this.grid.addColumnSelect(
          'accountType',
          terms['manage.system.syscompany.accounttype'],
          this.accountTypes,
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 1,
            editable: true,
          }
        );

        this.grid.addColumnText(
          'paymentNr',
          terms['manage.system.syscompany.accountnr'],
          {
            flex: 1,
            editable: true,
          }
        );

        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.deleteRow(row);
          },
        });
        this.grid.setNbrOfRowsToShow(10);
        super.finalizeInitGrid();
      });
  }

  onCellValueChanged(event: CellValueChangedEvent): void {
    if (
      event.rowIndex !== null &&
      event.rowIndex !== undefined &&
      event.rowIndex >= 0
    ) {
      event.data.isModifed = true;
      this.form?.updateBankAccount(event.rowIndex, event.data);
    }
  }

  deleteRow(row: AG_NODE<SysCompanyBankAccountDTO>): void {
    this.form?.deleteBankAccount(+row.AG_NODE_ID);
    this.grid?.deleteRow(row);
    this.grid?.clearSelectedRows();
  }
}
