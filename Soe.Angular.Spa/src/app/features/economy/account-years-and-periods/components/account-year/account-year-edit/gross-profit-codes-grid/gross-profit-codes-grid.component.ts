import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject,
} from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { SoeFormGroup } from '@shared/extensions';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, take, tap } from 'rxjs';
import { GrossProfitCodeDTO } from 'src/app/features/economy/gross-profit-codes/models/gross-profit-codes.model';
import { AccountYearService } from '../../../../services/account-year.service';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-gross-profit-codes-grid',
  templateUrl: './gross-profit-codes-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class GrossProfitCodesGridComponent
  extends GridBaseDirective<GrossProfitCodeDTO>
  implements OnInit
{
  @Input() form!: SoeFormGroup;
  @Input() rows!: BehaviorSubject<GrossProfitCodeDTO[]>;
  @Input() accountYearId!: number;
  @Input() status!: number;
  @Input() isDirty!: boolean;

  @Output() reloadGrossProfit = new EventEmitter<void>();

  accountYearService = inject(AccountYearService);
  messageBoxService = inject(MessageboxService);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Preferences_VoucherSettings_GrossProfitCodes_Edit,
      'Economy.Accounting.AccountYear.GrossProfitCodes',
      { skipInitialLoad: true }
    );
  }

  override onGridReadyToDefine(grid: GridComponent<GrossProfitCodeDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.code',
        'common.name',
        'economy.accounting.grossprofitcode.accountyear',
        'common.description',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('code', terms['common.code'], {
          flex: 1,
        });
        this.grid.addColumnText('name', terms['common.name'], {
          flex: 1,
        });
        this.grid.addColumnText(
          'accountYearId',
          terms['economy.accounting.grossprofitcode.accountyear'],
          {
            flex: 1,
          }
        );
        this.grid.addColumnText('description', terms['common.description'], {
          flex: 1,
        });

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid({ hidden: true });
      });
  }

  copyGrossProfitCodes() {
    return this.accountYearService
      .copyGrossProfitCodes(this.accountYearId)
      .pipe(
        tap(result => {
          if (result.success) {
            this.reloadGrossProfit.emit();
          } else {
            const errorMsg = ResponseUtil.getErrorMessage(result);
            this.messageBoxService.error(
              this.translate.instant('common.status'),
              errorMsg ? errorMsg : ''
            );
          }
        })
      );
  }
}
