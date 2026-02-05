import { Component, inject, OnInit } from '@angular/core';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { CurrencyDTO } from '../../models/currencies.model';
import { CurrenciesService } from '../../services/currencies.service';
import { CurrenciesForm } from '../../models/currencies-form.model';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import {
  Feature,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { forkJoin, of, tap } from 'rxjs';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { ISysCurrencyDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';

@Component({
  selector: 'soe-match-settings-edit',
  templateUrl: './currencies-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class CurrenciesEditComponent
  extends EditBaseDirective<CurrencyDTO, CurrenciesService, CurrenciesForm>
  implements OnInit
{
  coreService = inject(CoreService);
  service = inject(CurrenciesService);

  intervalTypes: SmallGenericType[] = [];
  sysCurrencies: ISysCurrencyDTO[] = [];

  get currencyId() {
    return this.form?.getIdControl()?.value;
  }

  get selectedCurrency() {
    const sysCurrencyId = this.form?.sysCurrencyId.value;
    return this.sysCurrencies.find(c => c.sysCurrencyId === sysCurrencyId);
  }

  constructor() {
    super();
  }

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(Feature.Economy_Preferences_Currency_Edit, {
      skipDefaultToolbar: true,
      lookups: [this.executeLookups()],
    });
  }

  executeLookups() {
    return this.performLoadData.load$(
      forkJoin([
        this.coreService.getTermGroupContent(
          TermGroup.CurrencyIntervalType,
          false,
          true,
          true,
          true
        ),
        this.service.getSysCurrencies(!this.currencyId),
      ]).pipe(
        tap(([intervalTypes, sysCurrencies]) => {
          this.intervalTypes = intervalTypes;
          this.sysCurrencies = sysCurrencies;
        })
      )
    );
  }

  setSysCurrencyCode(id?: number) {
    if (!id) id = this.form?.sysCurrencyId.value;

    const option = this.sysCurrencies.find(c => c.sysCurrencyId === id);
    this.form?.description.setValue(option?.description || '');
  }

  override loadData() {
    return this.performLoadData.load$(
      this.service.get(this.currencyId).pipe(
        tap(currency => {
          this.form?.customReset(currency as CurrencyDTO);
          this.setSysCurrencyCode();
        })
      )
    );
  }
}
