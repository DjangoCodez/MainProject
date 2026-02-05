import { Component, inject, input, output } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SignatoryContractPermissionsService } from '../../services/signatory-contract-permissions.service';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { GridComponent } from '@ui/grid/grid.component';
import { TermCollection } from '@shared/localization/term-types';
import { Observable } from 'rxjs';
import { ISignatoryContractPermissionEditItem } from '@shared/models/generated-interfaces/SignatoryContractPermissionEditItem';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-signatory-contract-permission-edit-grid',
  standalone: false,
  templateUrl: './signatory-contract-permission-edit-grid.component.html',
  styleUrls: ['./signatory-contract-permission-edit-grid.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
})
export class SignatoryContractPermissionEditGridComponent extends GridBaseDirective<
  ISignatoryContractPermissionEditItem,
  SignatoryContractPermissionsService
> {
  readonly signatoryContractId = input.required<number>();
  readonly editable = input.required<boolean>();
  readonly permissionChanged = output<number[]>();

  service = inject(SignatoryContractPermissionsService);
  gridName = 'Manage.Registry.SignatoryContract.PermissionEdit';

  override ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Manage_Preferences_Registry_SignatoryContract_Edit,
      this.gridName
    );
  }

  override loadTerms(): Observable<TermCollection> {
    const translationsKeys = ['manage.registry.signatorycontract.permission'];

    return super.loadTerms(translationsKeys);
  }

  override onGridReadyToDefine(
    grid: GridComponent<ISignatoryContractPermissionEditItem>
  ): void {
    super.onGridReadyToDefine(grid);
    this.setColumns();
    this.grid.context.suppressGridMenu = true;
    this.grid.context.suppressFiltering = true;

    super.finalizeInitGrid({ hidden: true });
  }

  public resetGrid(): void {
    this.resetColumns();
  }

  private resetColumns(): void {
    this.grid.columns = [];
    this.setColumns();
    this.grid.resetColumns();
  }

  private setColumns(): void {
    this.grid.addColumnBool('isSelected', '', {
      width: 40,
      editable: this.editable(),
      columnSeparator: true,
      onClick: this.toggleSelected.bind(this),
    });

    this.grid.addColumnText(
      'name',
      this.terms['manage.registry.signatorycontract.permission']
    );
  }

  override loadData(): Observable<ISignatoryContractPermissionEditItem[]> {
    return super.loadData(this.signatoryContractId());
  }

  private toggleSelected(
    isChecked: boolean,
    row: ISignatoryContractPermissionEditItem
  ): void {
    setTimeout(() => {
      const selectedPermissions: number[] = this.rowData.value
        .filter(r => r.isSelected)
        .map(r => r.id);
      this.permissionChanged.emit(selectedPermissions);
    }, 0);
  }

  public override refreshGrid(): void {
    super.refreshGrid();
  }
}
