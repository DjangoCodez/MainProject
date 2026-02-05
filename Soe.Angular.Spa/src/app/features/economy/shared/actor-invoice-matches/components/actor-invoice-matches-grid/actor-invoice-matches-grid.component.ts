import { Component, input, output } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ActorInvoiceMatchesFilterDTO } from '../../models/actor-invoice-matches-filter-dto.model';
import { BehaviorSubject } from 'rxjs';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { IInvoiceMatchingDTO } from '@shared/models/generated-interfaces/InvoiceMatchingDTO';

@Component({
  selector: 'soe-actor-invoice-matches-grid',
  standalone: false,
  templateUrl: './actor-invoice-matches-grid.component.html',
})
export class ActorInvoiceMatchesGridComponent {
  actors = input.required<SmallGenericType[]>();
  types = input.required<SmallGenericType[]>();
  actorLabelKey = input.required<string>();
  gridLabelKey = input.required<string>();
  rowData = input.required<BehaviorSubject<IInvoiceMatchingDTO[]>>();
  guid = input.required<string>();
  toolbarService = input.required<ToolbarService>();

  protected searchClicked = output<ActorInvoiceMatchesFilterDTO>();

  protected triggerSearch(row: ActorInvoiceMatchesFilterDTO): void {
    this.searchClicked.emit(row);
  }
}
