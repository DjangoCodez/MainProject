import {
  Component,
  OnInit,
  WritableSignal,
  computed,
  inject,
  signal,
} from '@angular/core';
import { BatchUpdateComponent } from '@shared/components/batch-update/components/batch-update/batch-update.component';
import { BatchUpdateDialogData } from '@shared/components/batch-update/models/batch-update-dialog-data.model';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import {
  Feature,
  SoeEntityType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IDefaultFilterSettings } from '@ui/grid/interfaces';
import { Observable, forkJoin, map, take, tap } from 'rxjs';
import { SupplierExtendedGridDTO } from '../../../models/supplier.model';
import { SupplierService } from '../../../services/supplier.service';
import { SpaNavigationService } from '@shared/services/spa-navigation.service';
import { MultiValueCellRenderer } from '@ui/grid/cell-renderers/multi-value-cell-renderer/multi-value-cell-renderer.component';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-suppliers-grid',
  templateUrl: './suppliers-grid.component.html',
  styleUrls: ['./suppliers-grid.component.scss'],
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SuppliersGridComponent
  extends GridBaseDirective<SupplierExtendedGridDTO, SupplierService>
  implements OnInit
{
  service = inject(SupplierService);
  navigationService = inject(SpaNavigationService);

  hasEditContactPersonPermission = signal(false);
  hasDisabledSaveButtonCustom = computed(
    () =>
      this.suppliersWithChangedState().length === 0 &&
      this.suppliersWithChangedIsPrivatePerson().length === 0
  );
  private readonly dialogService = inject(DialogService);
  private readonly hasBatchUpdatePermission = signal<boolean>(false);
  private readonly rowsNotSelected = signal<boolean>(true);

  suppliersWithChangedState: WritableSignal<number[]> = signal([]);
  suppliersWithChangedIsPrivatePerson: WritableSignal<number[]> = signal([]);

  performAction = new Perform<any>(this.progressService);

  constructor(
    public flowHandler: FlowHandlerService,
    private coreService: CoreService
  ) {
    super();
  }

  ngOnInit(): void {
    this.startFlow(Feature.Economy_Supplier, 'Economy.Supplier.Suppliers', {
      additionalModifyPermissions: [
        Feature.Economy_Supplier_Suppliers_Edit,
        Feature.Economy_Supplier_Suppliers_BatchUpdate,
      ],
      lookups: [this.loadEditContactPersonPermission()],
      useLegacyToolbar: true,
    });
  }

  override onPermissionsLoaded() {
    super.onPermissionsLoaded();
    this.hasBatchUpdatePermission.set(
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Supplier_Suppliers_BatchUpdate
      ) ?? false
    );
  }

  protected gridRowSelectedChanged(rows: SupplierExtendedGridDTO[]): void {
    this.rowsNotSelected.set(rows.length === 0);
  }

  override createLegacyGridToolbar(): void {
    super.createLegacyGridToolbar({
      reloadOption: {
        onClick: () => this.refreshGrid(),
      },
      saveOption: {
        onClick: () => {
          this.save();
        },
        disabled: this.hasDisabledSaveButtonCustom,
      },
    });

    if (this.hasBatchUpdatePermission()) {
      this.toolbarUtils.createLegacyGroup({
        buttons: [
          this.toolbarUtils.createLegacyButton(
            this.toolbarUtils.createLegacyButton({
              icon: 'pencil',
              title: 'common.batchupdate.title',
              label: 'common.batchupdate.title',
              onClick: this.openBatchUpdate.bind(this),
              disabled: this.rowsNotSelected,
              hidden: signal(false),
            })
          ),
        ],
      });
    }
  }

  onStateChanged(data: boolean, row: SupplierExtendedGridDTO): void {
    const existing = this.suppliersWithChangedState();
    this.suppliersWithChangedState.set(existing.toggle(row.actorSupplierId));
  }

  onIsPrivatePersonChanged(data: boolean, row: SupplierExtendedGridDTO) {
    const existing = this.suppliersWithChangedIsPrivatePerson();
    this.suppliersWithChangedIsPrivatePerson.set(
      existing.toggle(row.actorSupplierId)
    );
  }

  onSupplierCentralClick(row: SupplierExtendedGridDTO) {
    this.navigationService.spaNavigate(
      `/soe/economy/supplier/suppliercentral/default.aspx?supplier=${row.actorSupplierId}`
    );
  }

  onContactPersonsClick(row: SupplierExtendedGridDTO) {
    this.navigationService.spaNavigate(
      `/soe/manage/contactpersons/default.aspx?actor=${row.actorSupplierId}`
    );
  }

  override onGridReadyToDefine(grid: GridComponent<SupplierExtendedGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.active',
        'common.number',
        'common.name',
        'common.orgnrshort',
        'common.categories',
        'common.contactaddresses.ecommenu.phonehome',
        'common.contactaddresses.ecommenu.phonejob',
        'common.contactaddresses.ecommenu.phonemobile',
        'common.email',
        'economy.supplier.invoice.paytoaccount',
        'economy.supplier.supplier.opensuppliercentral',
        'economy.supplier.supplier.showcontactpersons',
        'economy.accounting.paymentcondition.paymentcondition',
        'core.edit',
        'common.privateperson',
        'core.aggrid.totals.filtered',
        'core.aggrid.totals.total',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnBool('isActive', terms['common.active'], {
          width: 60,
          enableHiding: false,
          editable: true,
          setChecked: true,
          onClick: this.onStateChanged.bind(this),
        });
        this.grid.addColumnText('supplierNr', terms['common.number'], {
          width: 80,
          enableHiding: false,
          sort: 'asc',
        });
        this.grid.addColumnText('name', terms['common.name'], {
          enableHiding: false,
          flex: 1,
        });
        this.grid.addColumnText('orgNr', terms['common.orgnrshort'], {
          enableHiding: false,
          flex: 1,
        });
        this.grid.addColumnText('categoriesArray', terms['common.categories'], {
          enableHiding: false,
          flex: 1,
          cellRenderer: MultiValueCellRenderer,
          filter: 'agSetColumnFilter',
        });
        this.grid.addColumnText(
          'homePhone',
          terms['common.contactaddresses.ecommenu.phonehome'],
          {
            hide: true,
            flex: 1,
          }
        );
        this.grid.addColumnText(
          'workPhone',
          terms['common.contactaddresses.ecommenu.phonejob'],
          {}
        );
        this.grid.addColumnText(
          'mobilePhone',
          terms['common.contactaddresses.ecommenu.phonemobile'],
          {}
        );
        this.grid.addColumnText('email', terms['common.email'], {});
        this.grid.addColumnText(
          'payToAccount',
          terms['economy.supplier.invoice.paytoaccount'],
          {
            hide: true,
          }
        );
        this.grid.addColumnText(
          'paymentCondition',
          terms['economy.accounting.paymentcondition.paymentcondition'],
          {
            hide: true,
          }
        );
        this.grid.addColumnBool(
          'isPrivatePerson',
          terms['common.privateperson'],
          {
            width: 40,
            enableHiding: false,
            editable: true,
            onClick: this.onIsPrivatePersonChanged.bind(this),
          }
        );
        this.grid.addColumnIcon(
          null,
          terms['economy.supplier.supplier.opensuppliercentral'],
          {
            enableHiding: false,
            suppressExport: true,
            iconPrefix: 'fal',
            iconName: 'calculator-alt',
            onClick: this.onSupplierCentralClick.bind(this),
          }
        );
        if (this.hasEditContactPersonPermission()) {
          this.grid.addColumnIcon(
            null,
            terms['economy.supplier.supplier.showcontactpersons'],
            {
              enableHiding: false,
              suppressExport: true,
              iconPrefix: 'fal',
              iconName: 'male',
              onClick: this.onContactPersonsClick.bind(this),
            }
          );
        }
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });

        const defaultFilter: IDefaultFilterSettings = {
          field: 'isActive',
          filterModel: {
            values: ['true'],
          },
        };

        super.finalizeInitGrid(undefined, defaultFilter);
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: {
      onlyActive: boolean;
    }
  ): Observable<SupplierExtendedGridDTO[]> {
    return super.loadData(id, { onlyActive: false });
  }

  loadEditContactPersonPermission() {
    return this.coreService
      .hasModifyPermissions([Feature.Manage_ContactPersons_Edit])
      .pipe(
        tap(res => {
          this.hasEditContactPersonPermission.set(
            res[Feature.Manage_ContactPersons_Edit]
          );
        })
      );
  }

  save() {
    this.grid.api.stopEditing();

    this.rowData.pipe(take(1)).subscribe(rows => {
      const requests: Observable<BackendResponse>[] = [];

      const changedStates = this.suppliersWithChangedState();
      if (changedStates.length > 0) {
        const dict: Record<number, boolean> = {};
        for (const id of changedStates) {
          dict[id] = rows.find(r => r.actorSupplierId === id)!.isActive!;
        }
        requests.push(this.service.updateSuppliersState(dict));
      }

      const changedIsPrivatePersons =
        this.suppliersWithChangedIsPrivatePerson();
      if (changedIsPrivatePersons.length > 0) {
        const dict: Record<number, boolean> = {};
        for (const id of changedIsPrivatePersons) {
          dict[id] = rows.find(r => r.actorSupplierId === id)!.isPrivatePerson!;
        }
        requests.push(this.service.updateIsPrivatePerson(dict));
      }

      this.performAction.crud(
        CrudActionTypeEnum.Save,
        forkJoin(requests).pipe(
          map(res => res.find(x => !x.success) || res[0]),
          tap(this.updateStatesAndEmitChange)
        )
      );
    });
  }

  updateStatesAndEmitChange = (backendResponse: BackendResponse) => {
    if (backendResponse.success) {
      this.suppliersWithChangedState.set([]);
      this.suppliersWithChangedIsPrivatePerson.set([]);
      this.refreshGrid();
    }
  };

  private openBatchUpdate(): void {
    const dialogOpts = <Partial<BatchUpdateDialogData>>{
      title: 'common.batchupdate.title',
      size: 'lg',
      disableClose: true,
      entityType: SoeEntityType.Supplier,
      selectedIds: this.grid?.getSelectedIds('actorSupplierId'),
    };
    this.dialogService
      .open(BatchUpdateComponent, dialogOpts)
      .afterClosed()
      .subscribe(res => {
        if (res) {
          this.refreshGrid();
        }
      });
  }
}
