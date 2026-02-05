import {
  Component,
  inject,
  OnInit,
  signal,
  WritableSignal,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { ICustomerGridDTO } from '@shared/models/generated-interfaces/CustomerDTO';
import {
  Feature,
  SoeEntityType,
  SoeModule,
} from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';
import { Observable, take } from 'rxjs';
import { CustomerGridDTO } from '../../models/customer.model';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { Dict } from '@ui/grid/services/selected-item.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BatchUpdateDialogData } from '@shared/components/batch-update/models/batch-update-dialog-data.model';
import { BatchUpdateComponent } from '@shared/components/batch-update/components/batch-update/batch-update.component';
import { CustomerService } from '../../services/customer.service';
import { CrudActionTypeEnum } from '@shared/enums';
import {
  ICustomerUpdateGrid,
  IUpdateIsPrivatePerson,
} from '@shared/models/generated-interfaces/CoreModels';
import { UrlHelperService } from '@shared/services/url-params.service';
import { BackendResponse } from '@shared/interfaces/backend-response.interface';

@Component({
  selector: 'soe-customer-grid',
  templateUrl: './customer-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CustomerGridComponent
  extends GridBaseDirective<ICustomerGridDTO, CustomerService>
  implements OnInit
{
  service = inject(CustomerService);
  urlHelper = inject(UrlHelperService);
  private readonly dialogService = inject(DialogService);
  private readonly progress = inject(ProgressService);
  private readonly perform = new Perform<any>(this.progress);
  customersWithChangedState: WritableSignal<number[]> = signal([]);
  private readonly selectedItems = signal<Dict>(<Dict>{});
  private readonly privatePersonsModified: Array<IUpdateIsPrivatePerson> = [];

  private categoriesPermission = false;
  private editContactPersonPermission = false;
  private batchUpdatePermission = false;
  private modifyPermission = false;

  private toolbarSaveDisabled = signal(true);
  private toolbarMassAdjustDisabled = signal(true);
  private toolbarMassAdjustHidden = signal(true);

  get isEconomyModule() {
    return this.urlHelper.module === SoeModule.Economy;
  }

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      this.isEconomyModule
        ? Feature.Economy_Customer_Customers
        : Feature.Billing_Customer_Customers,
      'Common.Customer.Customers',
      {
        additionalModifyPermissions: [
          Feature.Billing_Customer_Customers_Edit,
          Feature.Economy_Customer_Customers_Edit,
          Feature.Manage_ContactPersons_Edit,
          Feature.Common_Categories_Customer,
          Feature.Economy_Customer_Customers_BatchUpdate,
          Feature.Billing_Customer_Customers_BatchUpdate,
        ],
      }
    );
  }

  override onPermissionsLoaded(): void {
    super.onPermissionsLoaded();

    this.modifyPermission =
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Customer_Customers_Edit
      ) ||
      this.flowHandler.hasModifyAccess(Feature.Economy_Customer_Customers_Edit);

    this.editContactPersonPermission = this.flowHandler.hasModifyAccess(
      Feature.Manage_ContactPersons_Edit
    );

    this.categoriesPermission = this.flowHandler.hasModifyAccess(
      Feature.Common_Categories_Customer
    );

    this.batchUpdatePermission =
      this.flowHandler.hasModifyAccess(
        Feature.Economy_Customer_Customers_BatchUpdate
      ) ||
      this.flowHandler.hasModifyAccess(
        Feature.Billing_Customer_Customers_BatchUpdate
      );
  }

  override createGridToolbar(): void {
    super.createGridToolbar();

    this.toolbarSaveDisabled.set(!this.modifyPermission);
    this.toolbarMassAdjustDisabled.set(
      !this.modifyPermission || this.grid?.getSelectedCount() === 0
    );
    this.toolbarMassAdjustHidden.set(!this.batchUpdatePermission);

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButtonSave({
          disabled: this.toolbarSaveDisabled,
          onAction: () => this.saveStatus(),
        }),
      ],
    });
    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('massAdjust', {
          iconName: signal('pen'),
          caption: signal('common.batchupdate.title'),
          tooltip: signal('common.batchupdate.title'),
          disabled: this.toolbarMassAdjustDisabled,
          hidden: this.toolbarMassAdjustHidden,
          onAction: () => this.openBatchUpdate(),
        }),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<ICustomerGridDTO>): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.active',
        'common.number',
        'common.name',
        'common.orgnr',
        'common.categories.categories',
        'common.customer.customer.invoicereference',
        'common.customer.customer.opencustomercentral',
        'common.customer.customer.showcontactpersons',
        'core.edit',
        'common.service',
        'common.contactaddresses.addressmenu.visiting',
        'common.contactaddresses.addressmenu.billing',
        'common.contactaddresses.addressmenu.delivery',
        'common.contactaddresses.ecommenu.phonehome',
        'common.email',
        'common.privateperson',
        'common.contactaddresses.ecommenu.phonemobile',
        'common.contactaddresses.ecommenu.phonejob',
        'common.customer.customer.invoicedeliverytype',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();

        this.grid.addColumnActive('isActive', terms['common.active'], {
          editable: this.modifyPermission,
          idField: 'actorCustomerId',
          minWidth: 50,
          maxWidth: 50,
        });
        this.grid.addColumnText('customerNr', terms['common.number'], {
          flex: 1,
          maxWidth: 150,
        });
        this.grid.addColumnText('name', terms['common.name'], { flex: 1 });
        this.grid.addColumnText('orgNr', terms['common.orgnr'], { flex: 1 });

        if (this.categoriesPermission) {
          this.grid.addColumnText(
            'categories',
            terms['common.categories.categories'],
            { flex: 1 }
          );
        }

        this.grid.addColumnText(
          'invoiceReference',
          terms['common.customer.customer.invoicereference'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'gridPaymentServiceText',
          terms['common.service'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'invoiceDeliveryTypeText',
          terms['common.customer.customer.invoicedeliverytype'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'gridAddressText',
          terms['common.contactaddresses.addressmenu.visiting'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'gridBillingAddressText',
          terms['common.contactaddresses.addressmenu.billing'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'gridDeliveryAddressText',
          terms['common.contactaddresses.addressmenu.delivery'],
          { flex: 1 }
        );

        this.grid.addColumnText(
          'gridHomePhoneText',
          terms['common.contactaddresses.ecommenu.phonehome'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'gridMobilePhoneText',
          terms['common.contactaddresses.ecommenu.phonemobile'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'gridWorkPhoneText',
          terms['common.contactaddresses.ecommenu.phonejob'],
          { flex: 1 }
        );
        this.grid.addColumnText('gridEmailText', terms['common.email'], {
          flex: 1,
        });

        this.grid.addColumnBool(
          'isPrivatePerson',
          terms['common.privateperson'],
          {
            maxWidth: 100,
            alignCenter: true,
            editable: this.modifyPermission,
            onClick: (data, row) => this.privatePersonToggle(data, row),
          }
        );
        this.grid.addColumnIcon(
          null,
          terms['common.customer.customer.opencustomercentral'],
          {
            iconName: 'calculator-alt',
            enableHiding: false,
            tooltip: terms['common.customer.customer.opencustomercentral'],
            suppressExport: true,
            onClick: row => this.openSource(row),
          }
        );

        if (this.editContactPersonPermission) {
          this.grid.addColumnIcon(
            null,
            terms['common.customer.customer.showcontactpersons'],
            {
              iconName: 'male',
              onClick: row => this.showContactPersons(row),
              suppressExport: true,
            }
          );
        }

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => this.edit(row),
        });

        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: { onlyActive: boolean }
  ): Observable<ICustomerGridDTO[]> {
    return super.loadData(id, { onlyActive: false });
  }

  protected gridRowSelectedChanged(rows: ICustomerGridDTO[]): void {
    this.toolbarMassAdjustDisabled.set(
      rows.length === 0 || !this.modifyPermission
    );
  }

  private privatePersonToggle(data: boolean, row: CustomerGridDTO): void {
    const index = this.privatePersonsModified.findIndex(
      s => s.id === row.actorCustomerId
    );

    if (index !== -1) {
      this.privatePersonsModified.splice(index, 1);
    }

    const update: IUpdateIsPrivatePerson = {
      id: row.actorCustomerId,
      isPrivatePerson: data,
    };

    this.privatePersonsModified.push(update);
    this.setSaveButtonDisabledState();
  }

  saveStatus(): void {
    const customerUpdateGridDto: ICustomerUpdateGrid = {
      items: this.privatePersonsModified,
      model: this.selectedItems(),
    };

    this.perform.crud(
      CrudActionTypeEnum.Save,
      this.service.updateGrid(customerUpdateGridDto),
      (response: BackendResponse) => {
        if (response.success) {
          this.refreshGrid();
        }
      }
    );
  }

  override refreshGrid(): void {
    this.privatePersonsModified.length = 0;
    super.refreshGrid();
  }

  override selectedItemsChanged(items: Dict): void {
    this.selectedItems.set(items);

    this.setSaveButtonDisabledState();
  }

  private setSaveButtonDisabledState(): void {
    this.toolbarSaveDisabled.set(
      (Object.keys(this.selectedItems().dict).length <= 0 &&
        this.privatePersonsModified.length <= 0) ||
        !this.modifyPermission
    );
  }

  openSource(row: CustomerGridDTO): void {
    let url = '';
    url = `/soe/economy/customer/customercentral/?customer=${row.actorCustomerId}`;

    if (url) {
      BrowserUtil.openInNewTab(window, url);
    }
  }

  private openBatchUpdate(): void {
    const dialogOpts = <Partial<BatchUpdateDialogData>>{
      title: 'common.batchupdate.title',
      size: 'lg',
      disableClose: true,
      entityType: SoeEntityType.Customer,
      selectedIds: this.grid?.getSelectedIds('actorCustomerId'),
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

  showContactPersons(row: CustomerGridDTO) {
    let url = '';
    url = `/soe/manage/contactpersons/?actor=${row.actorCustomerId}`;

    if (url) {
      BrowserUtil.openInNewTab(window, url);
    }
  }
}
