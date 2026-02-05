import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  inject,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import { TermCollection } from '@shared/localization/term-types';
import {
  ContactAddressItemType,
  Feature,
  TermGroup,
  TermGroup_SysContactEComType,
  TermGroup_SysContactType,
} from '@shared/models/generated-interfaces/Enumerations';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { BrowserUtil } from '@shared/util/browser-util';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MenuButtonItem } from '@ui/button/menu-button/menu-button.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { RowDoubleClickedEvent } from 'ag-grid-community';
import {
  BehaviorSubject,
  Observable,
  filter,
  forkJoin,
  map,
  mergeMap,
  of,
  take,
  tap,
} from 'rxjs';
import {
  ContactAddressDialogComponent,
  ContactAddressDialogData,
} from '../contact-address-dialog/contact-address-dialog.component';
import { ContactAddressItem, getIcon } from '../contact-addresses.model';

@Component({
  selector: 'soe-contact-addresses',
  templateUrl: './contact-addresses.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class ContactAddressesComponent
  extends GridBaseDirective<ContactAddressItem>
  implements OnChanges
{
  @Input({ required: true }) contactType!: TermGroup_SysContactType;
  @Input({ required: true }) readOnly!: boolean;
  @Input({ required: true }) allowShowSecret!: boolean;
  @Input({ required: true }) ignoreSys!: boolean;
  @Input() form: SoeFormGroup | undefined;
  @Input() addresses: ContactAddressItem[] = [];

  @Output() addressesChange = new EventEmitter<ContactAddressItem[]>();

  readonly coreService = inject(CoreService);
  readonly dialogService = inject(DialogService);

  addressRowTypes: { field1: number; field2: number }[] = [];
  addressTypes: MenuButtonItem[] = [];
  eComTypes: MenuButtonItem[] = [];
  addressRows = new BehaviorSubject<ContactAddressItem[]>([]);

  constructor() {
    super();

    this.startFlow(
      Feature.Manage_ContactPersons,
      'Common.Directives.ContactAddresses',
      { skipInitialLoad: true, lookups: [this.loadLookupData()] }
    );
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.contactType) {
      this.loadLookupData().subscribe();
    }
    if (changes.addresses && changes.addresses.currentValue) {
      this.setData();
    }
  }

  loadLookupData() {
    if (this.contactType) {
      return forkJoin([
        this.loadAddressRowTypes(),
        this.loadAddressTypes(),
        this.loadEComTypes(),
      ]).pipe(
        take(1),
        map(_ => true)
      );
    }
    return of(true);
  }

  override loadTerms(): Observable<TermCollection> {
    return super.loadTerms([
      'common.contactaddresses.name',
      'common.contactaddresses.value',
      'common.contactaddresses.issecret',
      'common.contactaddresses.showmap',
      'common.contactaddresses.addressrow.name',
      'common.contactaddresses.addressrow.address',
      'common.contactaddresses.addressrow.co',
      'common.contactaddresses.addressrow.postaladdress',
      'common.contactaddresses.addressrow.postalcode',
      'common.contactaddresses.addressrow.street',
      'common.contactaddresses.addressrow.entrancecode',
      'common.contactaddresses.addressrow.country',
      'common.contactaddresses.ecom.closestrelative.name',
      'common.contactaddresses.ecom.closestrelative.relation',
      'common.contactaddresses.ecom.latitude',
      'common.contactaddresses.ecom.longitude',
      'common.contactaddresses.addressmenu.label',
      'common.contactaddresses.addressmenu.distribution',
      'common.contactaddresses.addressmenu.visiting',
      'common.contactaddresses.addressmenu.billing',
      'common.contactaddresses.addressmenu.delivery',
      'common.contactaddresses.addressmenu.boardhq',
      'common.contactaddresses.ecommenu.label',
      'common.contactaddresses.ecommenu.companyadminemail',
      'common.contactaddresses.ecommenu.email',
      'common.contactaddresses.ecommenu.phonehome',
      'common.contactaddresses.ecommenu.phonejob',
      'common.contactaddresses.ecommenu.phonemobile',
      'common.contactaddresses.ecommenu.fax',
      'common.contactaddresses.ecommenu.web',
      'common.contactaddresses.ecommenu.closestrelative',
      'common.contactaddresses.ecommenu.coordinates',
      'common.contactaddresses.ecommenu.individualtaxnumber',
      'core.deleterow',
      'common.contactaddresses.invalidemail',
      'common.contactaddresses.missingecom',
      'common.contactaddresses.missingclosestrelative',
      'common.contactaddresses.missingcoordinate',
      'common.contact.information',
      'core.edit',
    ]);
  }

  loadAddressRowTypes() {
    return this.coreService.getAddressRowTypes(this.contactType).pipe(
      tap(res => {
        this.addressRowTypes = res;
      })
    );
  }

  loadAddressTypes() {
    return this.coreService.getAddressTypes(this.contactType).pipe(
      mergeMap(addressTypeIds =>
        this.coreService
          .getTermGroupContent(
            TermGroup.SysContactAddressType,
            false,
            true,
            false,
            true
          )
          .pipe(
            tap(aTypes => {
              // Filter address types based on contact type
              this.addressTypes = aTypes.reduce((arr, item) => {
                if (addressTypeIds.indexOf(item.id) >= 0) {
                  return [
                    ...arr,
                    {
                      id: item.id,
                      label: item.name,
                      icon: ['fal', getIcon(item.id)],
                    },
                  ];
                }
                return arr;
              }, [] as MenuButtonItem[]);
            })
          )
      )
    );
  }

  loadEComTypes() {
    return this.coreService.getEComTypes(this.contactType).pipe(
      mergeMap(eComTypeIds =>
        this.coreService
          .getTermGroupContent(
            TermGroup.SysContactEComType,
            false,
            true,
            false,
            true
          )
          .pipe(
            tap(eTypes => {
              // Filter ecom types based on contact type
              this.eComTypes = eTypes.reduce((arr, item) => {
                if (
                  eComTypeIds.indexOf(item.id) >= 0 &&
                  !(this.ignoreSys && item.id === 8) &&
                  !(this.ignoreSys && item.id === 5)
                ) {
                  const itemId = item.id + 10;
                  return [
                    ...arr,
                    {
                      id: itemId,
                      label: item.name,
                      icon: ['fal', getIcon(itemId)],
                    },
                  ];
                }
                return arr;
              }, [] as MenuButtonItem[]);
            })
          )
      )
    );
  }

  override onGridReadyToDefine(grid: GridComponent<ContactAddressItem>): void {
    super.onGridReadyToDefine(grid);

    this.grid.addColumnIcon('typeIcon', '', {
      flex: 1,
      iconPrefix: 'fal',
      suppressFilter: true,
    });
    this.grid.addColumnText(
      'name',
      this.terms['common.contactaddresses.name'],
      {
        width: 125,
        suppressFilter: true,
        editable: false,
      }
    );
    this.grid.addColumnText(
      'displayAddress',
      this.terms['common.contactaddresses.value'],
      {
        flex: 1,
        suppressFilter: true,
      }
    );
    this.grid.addColumnBool(
      'isSecret',
      this.terms['common.contactaddresses.issecret'],
      {
        width: 75,
        suppressFilter: true,
        editable: !this.readOnly,
        onClick: (isChecked, row) => this.toggleIsSecret(isChecked, row),
      }
    );
    this.grid.addColumnIcon(
      null,
      this.terms['common.contactaddresses.showmap'],
      {
        width: 100,
        maxWidth: 200,
        iconName: 'map',
        suppressFilter: true,
        onClick: row => this.openMap(row),
        showIcon: row => this.shouldShowMapIcon(row),
      }
    );
    this.grid.addColumnIconEdit({
      suppressFilter: true,
      onClick: row => this.showEditAddress(row),
    });
    if (!this.readOnly) {
      this.grid.addColumnIconDelete({
        suppressFilter: true,
        onClick: row => this.deleteAddress(row),
      });
    }

    this.grid.onRowDoubleClicked = (
      event: RowDoubleClickedEvent<ContactAddressItem, any>
    ) => {
      this.showEditAddress(event.data!);
    };

    this.grid.setNbrOfRowsToShow(8);
    this.grid.context.suppressGridMenu = true;

    this.setData();

    super.finalizeInitGrid({
      hidden: true,
    });
  }

  toggleIsSecret(isChecked: boolean, row: ContactAddressItem) {
    this.addressRows.pipe(take(1)).subscribe(rows => {
      const indexToUpdate = rows.indexOf(row);
      this.addressesChange.emit(
        this.addresses.map((item, i) =>
          i !== indexToUpdate ? item : { ...item, isSecret: isChecked }
        )
      );
      this.form?.markAsDirty();
    });
  }

  showEditAddress(row: ContactAddressItem) {
    this.showContactAddressDialog(row, undefined);
  }

  showAddAddress(selectedItem: MenuButtonItem) {
    this.showContactAddressDialog(undefined, selectedItem.id);
  }

  showContactAddressDialog(
    rowToUpdate?: ContactAddressItem,
    newRowAddressItemType?: number
  ) {
    const dialogData: ContactAddressDialogData = {
      title: this.terms['common.contact.information'],
      size: 'lg',
      rowToUpdate,
      newRowAddressItemType,
      addressRowTypes: this.addressRowTypes,
      addressTypes: this.addressTypes.map(x => ({ id: x.id!, name: x.label! })),
      eComTypes: this.eComTypes.map(x => ({ id: x.id!, name: x.label! })),
      allowShowSecret: this.allowShowSecret,
      readOnly: this.readOnly,
    };
    this.dialogService
      .open(ContactAddressDialogComponent, dialogData)
      .afterClosed()
      .pipe(filter(value => !!value))
      .subscribe((value: ContactAddressItem) => {
        if (rowToUpdate) {
          this.addressRows.pipe(take(1)).subscribe(rows => {
            const indexToUpdate = rows.indexOf(rowToUpdate);
            this.addressesChange.emit(
              this.addresses.map((item, i) =>
                i === indexToUpdate ? value : item
              )
            );
            this.form?.markAsDirty();
          });
        } else if (newRowAddressItemType) {
          this.addressesChange.emit([...this.addresses, value]);
          this.form?.markAsDirty();
        }
      });
  }

  shouldShowMapIcon(row: ContactAddressItem): boolean {
    return (
      (row.isAddress &&
        row.sysContactAddressTypeId != ContactAddressItemType.AddressBoardHQ) ||
      row.sysContactEComTypeId == TermGroup_SysContactEComType.Coordinates
    );
  }

  openMap(row: ContactAddressItem) {
    let parameters = row.displayAddress;
    if (
      row.contactAddressItemType === ContactAddressItemType.Coordinates &&
      row.eComDescription
    ) {
      const split: string[] = row.eComDescription.split(';');
      if (split.length > 1) {
        const longitude = Number(split[0]);
        const latitude = Number(split[1]);
        parameters = `${latitude},${longitude}`;
      }
    }
    BrowserUtil.openInNewTab(
      window,
      `https://www.google.com/maps/search/?api=1&query=${parameters}`
    );
  }

  deleteAddress(row: ContactAddressItem) {
    this.addressRows.pipe(take(1)).subscribe(rows => {
      const indexToRemove = rows.indexOf(row);
      this.addressesChange.emit(
        this.addresses.filter((_, i) => i !== indexToRemove)
      );
      this.form?.markAsDirty();
    });
  }

  private setData(): void {
    this.addressRows.next(
      this.addresses.map(x => ContactAddressItem.fromServerModel(x))
    );
    this.grid?.refreshCells();
  }
}
