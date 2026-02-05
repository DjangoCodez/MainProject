import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeOriginStatusClassificationGroup,
  SoeOriginType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { IInvoiceDistributionDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { EdistributionService } from '../../services/edistribution.service';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { DialogService } from '@ui/dialog/services/dialog.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { BehaviorSubject, Observable, of, take, tap } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { BrowserUtil } from '@shared/util/browser-util';
import { Perform } from '@shared/util/perform.class';

@Component({
  selector: 'soe-edistribution-grid',
  templateUrl: './edistribution-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class EdistributionGridComponent
  extends GridBaseDirective<IInvoiceDistributionDTO, EdistributionService>
  implements OnInit
{
  service = inject(EdistributionService);
  coreService = inject(CoreService);
  private readonly progress = inject(ProgressService);
  private readonly performLoad = new Perform<IInvoiceDistributionDTO[]>(
    this.progress
  );
  dialogService = inject(DialogService);

  distributionTypes!: ISmallGenericType[];
  distributionStatusTypes: ISmallGenericType[] = [];
  originTypes!: ISmallGenericType[];
  allItemsSelectionDict: ISmallGenericType[] = [];
  distributionTypeSelection!: number;
  originTypeSelection!: number;
  allItemsSelection!: number;
  distributionItems = new BehaviorSubject<IInvoiceDistributionDTO[]>([]);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Billing_Distribution_Reports,
      'Billing.Distribution.EDistribution',
      {
        skipInitialLoad: true,
        lookups: [
          this.loadOriginTypes(),
          this.loadDistributionTypes(),
          this.loadDistributionStatusTypes(),
          this.loadSelectionTypes(),
        ],
      }
    );
  }

  private loadOriginTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.OriginType, true, false)
      .pipe(
        tap(origins => {
          const includes = [
            SoeOriginType.Offer,
            SoeOriginType.Order,
            SoeOriginType.CustomerInvoice,
            SoeOriginType.Purchase,
          ];

          this.originTypes = origins.filter(o => includes.includes(o.id));
          this.originTypes.unshift({
            id: 0,
            name: this.translate.instant('common.all'),
          });
          this.originTypeSelection = 0;
        })
      );
  }

  private loadDistributionTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.EdistributionTypes, true, false)
      .pipe(
        tap(x => {
          this.distributionTypes = x;
          this.distributionTypeSelection = 0;
        })
      );
  }

  private loadDistributionStatusTypes() {
    return this.coreService
      .getTermGroupContent(TermGroup.EDistributionStatusType, true, false)
      .pipe(
        tap(x => {
          this.distributionStatusTypes = x;
        })
      );
  }

  private loadSelectionTypes(): Observable<SmallGenericType[]> {
    return this.coreService
      .getTermGroupContent(TermGroup.GridDateSelectionType, false, true, true)
      .pipe(
        tap(x => {
          this.allItemsSelectionDict = x;
          this.allItemsSelection = 1;
        })
      );
  }

  override loadData(
    id?: number | undefined
  ): Observable<IInvoiceDistributionDTO[]> {
    if (!this.distributionTypes || !this.originTypes) return of([]);

    return this.performLoad.load$(
      this.service
        .getGrid(undefined, {
          originType: this.originTypeSelection,
          type: this.distributionTypeSelection,
          allItemsSelection: this.allItemsSelection,
        })
        .pipe(
          tap(x => {
            this.distributionItems.next(x);
          })
        )
    );
  }

  setDistributionTypeSelection(value: number) {
    this.distributionTypeSelection = value;
    this.refreshGrid();
  }

  setOriginTypeSelection(value: number) {
    this.originTypeSelection = value;
    this.refreshGrid();
  }

  setAllItemSelection(value: number) {
    this.allItemsSelection = value;
    this.refreshGrid();
  }

  override onGridReadyToDefine(
    grid: GridComponent<IInvoiceDistributionDTO>
  ): void {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.message',
        'billing.invoices.householddeduction.seqnbr',
        'common.customer.customer.customerorsuppliernr',
        'common.customer.customer.customerorsuppliername',
        'common.type',
        'common.status',
        'common.messages.sentby',
        'common.messages.sentdate',
        'common.order',
        'common.offer',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.terms = terms;

        this.grid.addColumnText('originTypeName', terms['common.type'], {
          flex: 1,
        });
        this.grid.addColumnText(
          'seqNr',
          terms['billing.invoices.householddeduction.seqnbr'],
          {
            flex: 1,
            enableHiding: true,
            buttonConfiguration: {
              iconPrefix: 'fal',
              iconName: 'pencil',
              onClick: row => this.openSource(row),
            },
          }
        );
        this.grid.addColumnText(
          'customerName',
          terms['common.customer.customer.customerorsuppliername'],
          { flex: 1 }
        );
        this.grid.addColumnText(
          'customerNr',
          terms['common.customer.customer.customerorsuppliernr'],
          { flex: 1 }
        );
        this.grid.addColumnText('typeName', terms['common.type'], { flex: 1 });
        // this.grid.addColumnText('statusName', terms['common.status'], { flex: 1});
        this.grid.addColumnSelect(
          'status',
          terms['common.status'],
          this.distributionStatusTypes,
          undefined,
          {
            dropDownIdLabel: 'id',
            dropDownValueLabel: 'name',
            flex: 1,
            editable: false,
          }
        );
        this.grid.addColumnText('createdBy', terms['common.messages.sentby'], {
          flex: 1,
        });
        this.grid.addColumnDateTime(
          'created',
          terms['common.messages.sentdate'],
          { flex: 1 }
        );
        this.grid.addColumnText('message', terms['common.message'], {
          flex: 1,
          tooltipField: 'message',
        });

        super.finalizeInitGrid();
      });
  }

  openSource(row: IInvoiceDistributionDTO): void {
    let url = '';

    if (row.originTypeId == SoeOriginType.CustomerInvoice) {
      url = `/soe/billing/invoice/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleCustomerInvoices}&invoiceId=${row.originId}&invoiceNr=${row.seqNr}`;
    } else if (row.originTypeId == SoeOriginType.Offer) {
      url = `/soe/billing/offer/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleOffers}&invoiceId=${row.originId}&invoiceNr=${row.seqNr}`;
    } else if (row.originTypeId === SoeOriginType.Purchase) {
      url = `/soe/billing/purchase/list/default.aspx?&purchaseId=${row.originId}&purchaseNr=${row.seqNr}`;
    } else if (row.originTypeId === SoeOriginType.Order) {
      url = `/soe/billing/order/status/default.aspx?classificationgroup=${SoeOriginStatusClassificationGroup.HandleOffers}&invoiceId=${row.originId}&invoiceNr=${row.seqNr}`;
    }

    if (url) {
      BrowserUtil.openInNewTab(window, url);
    }
  }
}
