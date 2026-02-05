import { Directive, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { IInvoiceMatchingDTO } from '@shared/models/generated-interfaces/InvoiceMatchingDTO';
import {
  Feature,
  SoeOriginType,
} from '@shared/models/generated-interfaces/Enumerations';
import { map, Observable, of, tap } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { TermCollection } from '@shared/localization/term-types';
import { GridComponent } from '@ui/grid/grid.component';
import { ActorInvoiceMatchesFilterDTO } from '@features/economy/shared/actor-invoice-matches/models/actor-invoice-matches-filter-dto.model';
import { ActorInvoiceMatchesService } from '../services/actor-invoice-matches.service';

@Directive()
export abstract class ActorInvoiceMatchesGridBaseDirective
  extends GridBaseDirective<IInvoiceMatchingDTO, ActorInvoiceMatchesService>
  implements OnInit
{
  private readonly permission: Feature;
  private readonly originType: SoeOriginType;
  protected readonly actorLabelKey: string;
  protected readonly gridLabelKey: string;
  private readonly gridColumnLabels: Record<
    'actorName' | 'invoiceNr' | 'paymentNr' | 'amount' | 'date',
    string
  >;

  service = inject(ActorInvoiceMatchesService);

  protected actors: SmallGenericType[] = [];
  protected types: SmallGenericType[] = [];

  private typesGridFilterItems: SmallGenericType[] = [];

  private filter?: ActorInvoiceMatchesFilterDTO;

  public constructor(
    permission: Feature,
    gridName: string,
    originType: SoeOriginType,
    actorLabelKey: string,
    gridLabelKey: string,
    gridColumnLabels: Record<
      'actorName' | 'invoiceNr' | 'paymentNr' | 'amount' | 'date',
      string
    >
  ) {
    super();
    this.permission = permission;
    this.gridName = gridName;
    this.originType = originType;
    this.actorLabelKey = actorLabelKey;
    this.gridLabelKey = gridLabelKey;
    this.gridColumnLabels = gridColumnLabels;
  }

  override ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(this.permission, this.gridName, {
      skipInitialLoad: true,
      lookups: [this.loadActors()],
    });
  }

  override loadTerms(
    translationsKeys: string[] = []
  ): Observable<TermCollection> {
    translationsKeys = ['common.type'].concat(translationsKeys);

    return super.loadTerms(translationsKeys).pipe(
      tap(x => {
        this.setTypes();
      })
    );
  }

  protected abstract loadActors(): Observable<SmallGenericType[]>;

  protected abstract setTypes(): void;

  protected setTypesGridFilterItems(items: SmallGenericType[]): void {
    this.typesGridFilterItems = items;
  }

  override onGridReadyToDefine(grid: GridComponent<IInvoiceMatchingDTO>): void {
    super.onGridReadyToDefine(grid);

    const defaultOptions = {
      flex: 1,
      enableHiding: false,
      enableGrouping: false,
    };

    this.grid.addColumnText(
      'actorName',
      this.terms[this.gridColumnLabels.actorName],
      defaultOptions
    );

    this.grid.addColumnText(
      'invoiceNr',
      this.terms[this.gridColumnLabels.invoiceNr],
      defaultOptions
    );

    this.grid.addColumnText(
      'paymentNr',
      this.terms[this.gridColumnLabels.paymentNr],
      defaultOptions
    );

    this.grid.addColumnNumber(
      'amount',
      this.terms[this.gridColumnLabels.amount],
      {
        ...defaultOptions,
        decimals: 2,
      }
    );

    this.grid.addColumnSelect(
      'typeNameId',
      this.terms['common.type'],
      this.typesGridFilterItems,
      undefined,
      {
        ...defaultOptions,
      }
    );

    this.grid.addColumnDate(
      'date',
      this.terms[this.gridColumnLabels.date],
      defaultOptions
    );

    this.grid.addColumnIconEdit({
      ...defaultOptions,
      tooltip: this.terms['economy.supplier.invoice.matches.showinvoice'],
      onClick: row => {
        this.openInvoice(row);
      },
    });

    super.finalizeInitGrid();
  }

  protected abstract openInvoice(row: IInvoiceMatchingDTO): void;

  protected abstract setTypeName(row: IInvoiceMatchingDTO): void;

  private setTypeNameId(row: IInvoiceMatchingDTO): void {
    row.typeNameId =
      this.typesGridFilterItems.find(x => x.name === row.typeName)?.id ??
      undefined;
  }

  protected abstract setAmount(row: IInvoiceMatchingDTO): void;

  protected searchInvoiceMatches(filter: ActorInvoiceMatchesFilterDTO): void {
    this.filter = filter;

    this.filter.originType = this.originType;

    this.refreshGrid();
  }

  override loadData(): Observable<IInvoiceMatchingDTO[]> {
    if (!this.filter) {
      return of([]);
    }

    const additionalProps = {
      filter: this.filter,
    };

    return super.loadData(undefined, additionalProps).pipe(
      map((data: IInvoiceMatchingDTO[]) => {
        data.forEach(row => {
          this.setTypeName(row);
          this.setTypeNameId(row);
          this.setAmount(row);
        });

        return data;
      })
    );
  }
}
