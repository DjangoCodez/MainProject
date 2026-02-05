import { Component, inject, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take } from 'rxjs';
import { AttestGroupGridDTO } from '../../models/attestation-groups.model';
import { AttestationGroupsService } from '../../services/attestation-groups.service';

@Component({
  selector: 'soe-attestation-groups-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class AttestationGroupsGridComponent
  extends GridBaseDirective<AttestGroupGridDTO, AttestationGroupsService>
  implements OnInit
{
  service = inject(AttestationGroupsService);
  translate = inject(TranslateService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Economy_Preferences_SuppInvoiceSettings_AttestGroups,
      'Economy.Supplier.AttestGroups'
    );
  }

  override onGridReadyToDefine(grid: GridComponent<AttestGroupGridDTO>): void {
    super.onGridReadyToDefine(grid);
    this.translate
      .get(['common.code', 'common.name', 'core.edit'])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 2,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 10,
          sortable: true,
          sort: 'asc',
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        this.grid.context.exportFilenameKey =
          'economy.supplier.attestgroup.attestgroups';
        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: { addEmptyRow: boolean; attestWorkFlowHeadId?: number }
  ): Observable<AttestGroupGridDTO[]> {
    return super.loadData(id, {
      addEmptyRow: false,
      attestWorkFlowHeadId: undefined,
    });
  }
}
