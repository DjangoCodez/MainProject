import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SignatoryContractService } from '../../services/signatory-contract.service';
import { Observable } from 'rxjs';
import { TermCollection } from '@shared/localization/term-types';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { GridComponent } from '@ui/grid/grid.component';
import { ISignatoryContractGridDTO } from '@shared/models/generated-interfaces/SignatoryContractGridDTO';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-signatory-contract-grid',
  standalone: false,
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class SignatoryContractGridComponent
  extends GridBaseDirective<ISignatoryContractGridDTO, SignatoryContractService>
  implements OnInit
{
  service = inject(SignatoryContractService);
  gridName = 'Manage.Registry.SignatoryContract';

  override ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Manage_Preferences_Registry_SignatoryContract,
      this.gridName
    );
  }

  override loadTerms(): Observable<TermCollection> {
    const translationsKeys: string[] = [
      'manage.registry.signatorycontract.createddate',
      'manage.registry.signatorycontract.permissions',
      'manage.registry.signatorycontract.authenticationmethod',
      'manage.registry.signatorycontract.revokedat',
      'core.edit',
      'manage.registry.signatorycontract.recipientuser',
    ];

    return super.loadTerms(translationsKeys);
  }

  override onGridReadyToDefine(
    grid: GridComponent<ISignatoryContractGridDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    const defaultOptions = {
      flex: 1,
      enableHiding: false,
      enableGrouping: false,
    };

    this.grid.addColumnDate(
      'created',
      this.terms['manage.registry.signatorycontract.createddate'],
      defaultOptions
    );

    this.grid.addColumnText(
      'recipientUserName',
      this.terms['manage.registry.signatorycontract.recipientuser'],
      defaultOptions
    );

    this.grid.addColumnText(
      'permissions',
      this.terms['manage.registry.signatorycontract.permissions'],
      defaultOptions
    );

    this.grid.addColumnText(
      'authenticationMethod',
      this.terms['manage.registry.signatorycontract.authenticationmethod'],
      defaultOptions
    );

    this.grid.addColumnDate(
      'revokedAt',
      this.terms['manage.registry.signatorycontract.revokedat'],
      defaultOptions
    );

    this.grid.addColumnIconEdit({
      tooltip: this.terms['core.edit'],
      onClick: row => {
        this.edit(row);
      },
    });

    super.finalizeInitGrid();
  }
}
