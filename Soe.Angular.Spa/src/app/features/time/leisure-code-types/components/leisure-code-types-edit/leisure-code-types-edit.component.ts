import { Component, OnInit, inject } from '@angular/core';
import { SmallGenericType } from '@shared/models/generic-type.model';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { Observable, of, tap } from 'rxjs';
import { EditBaseDirective } from '@shared/directives/edit-base/edit-base.directive';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { Perform } from '@shared/util/perform.class';
import { addEmptyOption } from '@shared/util/array-util';
import { ITimeLeisureCodeDTO } from '@shared/models/generated-interfaces/SOECompModelDTOs';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { LeisureCodeTypesForm } from '../../models/leisure-code-types-form.model';
import { LeisureCodeTypesService } from '../../services/leisure-code-types.service';

@Component({
  selector: 'soe-leisure-code-types-edit',
  templateUrl: './leisure-code-types-edit.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class LeisureCodeTypesEditComponent
  extends EditBaseDirective<
    ITimeLeisureCodeDTO,
    LeisureCodeTypesService,
    LeisureCodeTypesForm
  >
  implements OnInit
{
  service = inject(LeisureCodeTypesService);
  performTypes = new Perform<SmallGenericType[]>(this.progressService);

  ngOnInit() {
    super.ngOnInit();

    this.startFlow(
      Feature.Time_Preferences_ScheduleSettings_LeisureCodeType_Edit,
      {
        lookups: this.loadTypes(),
      }
    );
  }

  loadTypes() {
    return this.performTypes.load$(this.service.getTypes());
  }
}
