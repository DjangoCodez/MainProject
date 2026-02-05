import { Component, Input, OnInit, inject } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ValidationHandler } from '@shared/handlers';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { ICompanyGroupMappingRowDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, of, take, tap } from 'rxjs';
import { EconomyService } from '../../../services/economy.service';
import { CompanyGroupMappingHeadForm } from '../../models/company-group-mappings-form.model';

@Component({
  selector: 'soe-company-group-mapping-rows',
  templateUrl: './company-group-mapping-rows.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CompanyGroupMappingRowsComponent
  extends GridBaseDirective<ICompanyGroupMappingRowDTO>
  implements OnInit
{
  @Input() rows!: BehaviorSubject<ICompanyGroupMappingRowDTO[]>;
  @Input({ required: true }) form: CompanyGroupMappingHeadForm | undefined;

  validationHandler = inject(ValidationHandler);
  progressService = inject(ProgressService);
  economyService = inject(EconomyService);

  accountDimStd: SmallGenericType[] = [];

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Accounting_CompanyGroup_TransferDefinitions,
      'economy.accounting.companygroup.mappings',
      {
        skipInitialLoad: true,
        lookups: [this.loadAccounts()],
      }
    );
  }

  override onGridReadyToDefine(
    grid: GridComponent<ICompanyGroupMappingRowDTO>
  ) {
    super.onGridReadyToDefine(grid);

    this.grid.api.updateGridOptions({
      onCellValueChanged: this.onCellChanged.bind(this),
    });

    this.translate
      .get([
        'core.warning',
        'economy.accounting.companygroup.validaterowsmessage',
        'economy.accounting.companygroup.childaccountfrom',
        'economy.accounting.companygroup.childaccountto',
        'economy.accounting.companygroup.parentaccount',
        'common.remove',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnAutocomplete(
          'childAccountFrom',
          terms['economy.accounting.companygroup.childaccountfrom'],
          {
            editable: true,
            flex: 20,
            source: () => this.accountDimStd,
            optionIdField: 'id',
            optionNameField: 'name',
          }
        );
        this.grid.addColumnAutocomplete(
          'childAccountTo',
          terms['economy.accounting.companygroup.childaccountto'],
          {
            editable: true,
            flex: 20,
            source: () => this.accountDimStd,
            optionIdField: 'id',
            optionNameField: 'name',
          }
        );
        this.grid.addColumnAutocomplete(
          'groupCompanyAccount',
          terms['economy.accounting.companygroup.parentaccount'],
          {
            editable: true,
            flex: 20,
            source: () => this.accountDimStd,
            optionIdField: 'id',
            optionNameField: 'name',
          }
        );

        this.grid.addColumnIconDelete({
          tooltip: terms['core.delete'],
          onClick: row => {
            this.delete(row);
          },
        });

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid({ hidden: true });
      });
  }

  override onFinished(): void {
    if (this.rows.value.length > 0) this.grid.resetRows();
  }

  onCellChanged() {
    this.form?.markAsDirty();
  }

  delete(row: ICompanyGroupMappingRowDTO): void {
    row.isDeleted = true;
    this.grid.deleteRow(row);
    this.form?.markAsDirty();
  }

  private loadAccounts() {
    return this.economyService
      .getAccountDims(true, false, true, false, false, false, false, false)
      .pipe(
        tap(acctDims => {
          acctDims[0].accounts.forEach(account => {
            this.accountDimStd.push({
              id: Number(account.accountNr),
              name: account.accountNr + ' - ' + account.name,
            });
          });
        })
      );
  }
}
