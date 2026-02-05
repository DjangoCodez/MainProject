import { Component, inject, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import {
  Feature,
  SoeEntityType,
  TermGroup,
} from '@shared/models/generated-interfaces/Enumerations';
import { ISmallGenericType } from '@shared/models/generated-interfaces/GenericType';
import { CoreService } from '@shared/services/core.service';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, take, tap } from 'rxjs';
import { ITextBlockGridDTO } from '../../models/text-block.model';
import { TextBlockService } from '../../services/text-block.service';
@Component({
  selector: 'soe-text-block-grid',
  templateUrl:
    '../../../../../shared/ui-components/grid/grid-wrapper/grid-wrapper-template.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class TextBlockGridComponent
  extends GridBaseDirective<ITextBlockGridDTO, TextBlockService>
  implements OnInit
{
  textBlockTypes: ISmallGenericType[] = [];

  coreService = inject(CoreService);
  service = inject(TextBlockService);
  progressService = inject(ProgressService);

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Billing_Preferences_Textblock,
      'Billing.Invoices.TextBlocks',
      {
        lookups: [this.loadTextBlockTypes()],
      }
    );
  }

  private loadTextBlockTypes(): Observable<ISmallGenericType[]> {
    this.textBlockTypes = [];
    return this.coreService
      .getTermGroupContent(TermGroup.TextBlockType, false, true)
      .pipe(
        tap(x => {
          this.textBlockTypes = x;
        })
      );
  }

  override onGridReadyToDefine(grid: GridComponent<ITextBlockGridDTO>) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'common.active',
        'common.name',
        'common.type',
        'common.offer',
        'common.contract',
        'common.order',
        'common.customerinvoice',
        'billing.purchase.list.purchase',
        'billing.invoices.textblocks.textblocks',
        'core.edit',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.enableRowSelection();
        this.grid.addColumnText('headline', terms['common.name'], {
          flex: 1,
        });

        this.grid.addColumnSelect(
          'type',
          terms['common.type'],
          this.textBlockTypes,
          undefined,
          {
            flex: 1,
            editable: false,
          }
        );

        this.grid.addColumnBool('showInOrder', terms['common.order'], {
          flex: 1,
          enableHiding: true,
        });

        this.grid.addColumnBool(
          'showInInvoice',
          terms['common.customerinvoice'],
          {
            flex: 1,
            enableHiding: true,
          }
        );

        this.grid.addColumnBool('showInOffer', terms['common.offer'], {
          flex: 1,
          enableHiding: true,
        });

        this.grid.addColumnBool('showInContract', terms['common.contract'], {
          flex: 1,
          enableHiding: true,
        });

        this.grid.addColumnBool(
          'showInPurchase',
          terms['billing.purchase.list.purchase'],
          {
            flex: 1,
            enableHiding: true,
          }
        );

        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined,
    additionalProps?: {
      entity: number;
      useCache: boolean;
      cacheExpireTime?: number;
    }
  ): Observable<ITextBlockGridDTO[]> {
    return super.loadData(id, {
      entity: SoeEntityType.CustomerInvoice,
      useCache: false,
    });
  }
}
