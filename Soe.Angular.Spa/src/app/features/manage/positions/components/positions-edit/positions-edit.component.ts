import { Component, OnInit, inject } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { PositionsService } from '../../services/positions.service';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { ISysPositionDTO } from '@shared/models/generated-interfaces/SOESysModelDTOs';
import { PositionsForm } from '../../models/positions-form.model';

@Component({
  selector: 'soe-positions-edit',
  templateUrl: './positions-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class PositionsEditComponent
  extends EditBaseDirective<ISysPositionDTO, PositionsService, PositionsForm>
  implements OnInit
{
  sysCountries: SmallGenericType[] = [];
  sysLanguageId: SmallGenericType[] = [];
  service = inject(PositionsService);
  coreService = inject(CoreService);

  ngOnInit() {
    super.ngOnInit();
    this.startFlow(Feature.Manage_Preferences_Registry_Positions_Edit, {
      lookups: [this.loadCountries(), this.loadLanguages()],
    });
  }

  loadCountries(): Observable<SmallGenericType[]> {
    return this.coreService
      .getCountries(false, true)
      .pipe(tap(x => (this.sysCountries = x)));
  }

  loadLanguages(): Observable<SmallGenericType[]> {
    return this.coreService
      .getLanguages(false)
      .pipe(tap(x => (this.sysLanguageId = x)));
  }
}
