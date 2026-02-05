import {
  Component,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SubSignatoryContractService } from '../../services/sub-signatory-contract.service';
import { ISignatoryContractDTO } from '@shared/models/generated-interfaces/SignatoryContractDTO';
import {
  Feature,
  TermGroup,
  TermGroup_SignatoryContractPermissionType,
} from '@shared/models/generated-interfaces/Enumerations';
import { Observable, of, tap } from 'rxjs';
import { TermCollection } from '@shared/localization/term-types';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ISubSignatoryContractEditDialogData } from '../../models/sub-signatory-contract-edit-dialog-data';
import { SubSignatoryContractEditDialogComponent } from '../sub-signatory-contract-edit-dialog/sub-signatory-contract-edit-dialog.component';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { SignatoryContractDTO } from '../../models/signatory-contract-edit-dto.model';
import { CoreService } from '@shared/services/core.service';

@Component({
  selector: 'soe-sub-signatory-contract-edit-grid',
  standalone: false,
  templateUrl: './sub-signatory-contract-edit-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
})
export class SubSignatoryContractEditGridComponent extends GridBaseDirective<
  ISignatoryContractDTO,
  SubSignatoryContractService
> {
  readonly signatoryContractId = input.required<number>();
  readonly editable = input.required<boolean>();
  readonly users = input.required<ISmallGenericType[]>();
  readonly parentPermissions = input.required<number[]>();
  readonly changeSubSignatoryContracts = output<ISignatoryContractDTO[]>();

  private readonly dialogService = inject(DialogService);
  private readonly coreService = inject(CoreService);
  private readonly messageboxService = inject(MessageboxService);

  private permissionTerms: ISmallGenericType[] = [];

  service = inject(SubSignatoryContractService);
  gridName = 'Manage.Registry.SignatoryContract.SubSignatoryContractEdit';

  override ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Manage_Preferences_Registry_SignatoryContract_Edit,
      this.gridName,
      {
        lookups: [this.loadPermissionTerms()],
      }
    );
  }

  override loadTerms(): Observable<TermCollection> {
    const translationsKeys = [
      'manage.registry.signatorycontract.recipientuser',
      'manage.registry.signatorycontract.permissions',
      'manage.registry.signatorycontract.error.nousers',
      'core.error',
      'manage.registry.signatorycontract.error.noparentpermissions',
    ];

    return super.loadTerms(translationsKeys);
  }

  override createGridToolbar(): void {
    super.createGridToolbar({
      hideClearFilters: true,
      hideReload: true,
    });

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton(
          'manage.registry.signatorycontract.subsignatorycontract.new',
          {
            iconName: signal('plus'),
            caption: signal('common.newrow'),
            tooltip: signal('common.newrow'),
            disabled: computed(() => !this.editable()),
            hidden: computed(() => !this.editable()),
            onAction: event => {
              this.addSubSignatoryContract();
            },
          }
        ),
      ],
    });
  }

  override onGridReadyToDefine(
    grid: GridComponent<ISignatoryContractDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.setColumns();
    this.grid.context.suppressGridMenu = true;
    this.grid.context.suppressFiltering = true;

    super.finalizeInitGrid({ hidden: true });
  }

  private setColumns(): void {
    this.grid.addColumnText(
      'recipientUserName',
      this.terms['manage.registry.signatorycontract.recipientuser'],
      {
        flex: 2,
      }
    );
    this.grid.addColumnText(
      'permissions',
      this.terms['manage.registry.signatorycontract.permissions'],
      {
        flex: 6,
      }
    );

    if (this.editable()) {
      this.grid.addColumnIconEdit({
        tooltip: this.terms['core.edit'],
        onClick: row => {
          this.editSubSignatoryContract(row);
        },
        flex: 1,
      });

      this.grid.addColumnIconDelete({
        tooltip: this.terms['core.delete'],
        onClick: row => {
          this.deleteSubSignatoryContract(row);
        },
        flex: 1,
      });
    }
  }

  public resetGrid(): void {
    this.resetColumns();
  }

  private resetColumns(): void {
    this.grid.columns = [];
    this.setColumns();
    this.grid.resetColumns();
  }

  override loadData(): Observable<ISignatoryContractDTO[]> {
    if (this.signatoryContractId()) {
      const additionalProps = {
        signatoryContractParentId: this.signatoryContractId(),
      };

      return super.loadData(undefined, additionalProps);
    } else {
      const arr: ISignatoryContractDTO[] = [];
      return of(arr);
    }
  }

  private editSubSignatoryContract(row: ISignatoryContractDTO): void {
    const title = 'manage.registry.signatorycontract.editsubcontract';

    this.changeSubSignatoryContract(row, title, this.users());
  }

  private addSubSignatoryContract(): void {
    const availableUsers: ISmallGenericType[] = this.users().filter(
      user => !this.rowData.value.some(v => v.recipientUserId === user.id)
    );

    if (availableUsers.length) {
      const title = 'manage.registry.signatorycontract.addsubcontract';

      const newRow: ISignatoryContractDTO = new SignatoryContractDTO();

      newRow.recipientUserId = availableUsers[0].id;
      newRow.recipientUserName = availableUsers[0].name;

      this.changeSubSignatoryContract(newRow, title, availableUsers);
    } else {
      this.messageboxService.error(
        this.terms['core.error'],
        this.terms['manage.registry.signatorycontract.error.nousers']
      );
    }
  }

  private changeSubSignatoryContract(
    row: ISignatoryContractDTO,
    title: string,
    users: ISmallGenericType[]
  ): void {
    const permissions = this.parentPermissions().filter(
      p =>
        p !==
        TermGroup_SignatoryContractPermissionType.SignatoryContract_EditContracts
    );
    if (permissions.length) {
      const permissionTerms = this.permissionTerms.filter(
        pt => permissions.includes(pt.id) || row.permissionTypes.includes(pt.id)
      );

      const dialogData: ISubSignatoryContractEditDialogData = {
        title: title,
        size: 'md',
        rowToUpdate: row,
        users: users,
        permissionTerms: permissionTerms,
      };

      this.dialogService
        .open(SubSignatoryContractEditDialogComponent, dialogData)
        .afterClosed()
        .subscribe((value: ISignatoryContractDTO | boolean | undefined) => {
          if (value) {
            const subSignatoryContracts: ISignatoryContractDTO[] = JSON.parse(
              JSON.stringify(this.rowData.value)
            );
            const modifiedSignatoryContract = value as ISignatoryContractDTO;
            const userName: string =
              this.users().find(
                user => user.id === modifiedSignatoryContract.recipientUserId
              )?.name ?? '';

            modifiedSignatoryContract.recipientUserName = userName;
            modifiedSignatoryContract.permissionNames =
              modifiedSignatoryContract.permissionTypes
                .map(pt => this.permissionTerms.find(p => p.id === pt)!.name)
                .sort((a, b) => a.localeCompare(b));

            modifiedSignatoryContract.permissions =
              modifiedSignatoryContract.permissionNames.join(', ');

            if (modifiedSignatoryContract.signatoryContractId !== 0) {
              const subSignatoryContract = subSignatoryContracts.find(
                item =>
                  item.signatoryContractId ===
                  modifiedSignatoryContract.signatoryContractId
              )!;

              const changeSubSignatoryContract = subSignatoryContracts.find(
                item =>
                  item.recipientUserId ===
                    modifiedSignatoryContract.recipientUserId &&
                  item.signatoryContractId !==
                    modifiedSignatoryContract.signatoryContractId
              );

              if (changeSubSignatoryContract) {
                changeSubSignatoryContract.recipientUserId = 0;
                changeSubSignatoryContract.recipientUserName = '';
              }

              subSignatoryContract.recipientUserId =
                modifiedSignatoryContract.recipientUserId;
              subSignatoryContract.recipientUserName =
                modifiedSignatoryContract.recipientUserName;
              subSignatoryContract.permissionTypes =
                modifiedSignatoryContract.permissionTypes;
              subSignatoryContract.permissions =
                modifiedSignatoryContract.permissions;
              subSignatoryContract.permissionNames =
                modifiedSignatoryContract.permissionNames;
            } else {
              const minId = Math.min(
                ...this.rowData.value.map(r => r.signatoryContractId)
              );

              if (!isFinite(minId) || minId > -1) {
                modifiedSignatoryContract.signatoryContractId = -1;
              } else {
                modifiedSignatoryContract.signatoryContractId = minId - 1;
              }

              subSignatoryContracts.push(modifiedSignatoryContract);
            }

            this.rowData.next(subSignatoryContracts);
            this.emitSubSignatoryContracts();
          }
        });
    } else {
      this.messageboxService.error(
        this.terms['core.error'],
        this.terms[
          'manage.registry.signatorycontract.error.noparentpermissions'
        ]
      );
    }
  }

  private emitSubSignatoryContracts(): void {
    this.changeSubSignatoryContracts.emit(this.rowData.value);
  }

  private deleteSubSignatoryContract(row: ISignatoryContractDTO): void {
    const subSignatoryContracts: ISignatoryContractDTO[] = JSON.parse(
      JSON.stringify(this.rowData.value)
    );

    const index = this.rowData.value.findIndex(
      r => r.signatoryContractId === row.signatoryContractId
    );
    if (index > -1) {
      subSignatoryContracts.splice(index, 1);
      this.rowData.next(subSignatoryContracts);
      this.emitSubSignatoryContracts();
    }
  }

  public override refreshGrid(): void {
    super.refreshGrid();
  }

  private loadPermissionTerms(): Observable<ISmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(
        TermGroup.SignatoryContractPermissionType,
        false,
        false,
        false,
        true
      )
      .pipe(
        tap(x => {
          this.permissionTerms = x;
        })
      );
  }
}
