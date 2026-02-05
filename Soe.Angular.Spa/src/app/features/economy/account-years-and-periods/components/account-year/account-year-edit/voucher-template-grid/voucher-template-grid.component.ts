import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject,
} from '@angular/core';
import { AccountYearService } from '@features/economy/account-years-and-periods/services/account-year.service';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { CrudActionTypeEnum } from '@shared/enums';
import { SoeFormGroup } from '@shared/extensions';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { IVoucherGridDTO } from '@shared/models/generated-interfaces/VoucherHeadDTOs';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { Perform } from '@shared/util/perform.class';
import { VoucherService } from '@src/app/features/economy/voucher/services/voucher.service';
import { GridComponent } from '@ui/grid/grid.component';
import { MessageboxService } from '@ui/dialog/services/messagebox.service';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { BehaviorSubject, Observable, of, take, tap } from 'rxjs';
import { ResponseUtil } from '@shared/util/response-util';

@Component({
  selector: 'soe-voucher-template-grid',
  templateUrl: './voucher-template-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class VoucherTemplateGridComponent
  extends GridBaseDirective<IVoucherGridDTO>
  implements OnInit
{
  @Input() form!: SoeFormGroup;
  @Input() rows!: BehaviorSubject<IVoucherGridDTO[]>;
  @Input() accountYearId!: number;
  @Input() status!: number;
  @Input() isDirty!: boolean;

  @Output() reloadVoucherTemplate = new EventEmitter<void>();

  accountYearService = inject(AccountYearService);
  progressService = inject(ProgressService);
  messageBoxService = inject(MessageboxService);
  VoucherService = inject(VoucherService);
  performAction = new Perform<any>(this.progressService);

  ngOnInit(): void {
    super.ngOnInit();

    this.startFlow(
      Feature.Economy_Accounting_Vouchers_Edit,
      'Economy.Accounting.AccountYear.VoucherTemplates',
      { skipInitialLoad: true }
    );
  }

  override onGridReadyToDefine(grid: GridComponent<IVoucherGridDTO>) {
    super.onGridReadyToDefine(grid);

    this.translate
      .get([
        'common.number',
        'common.date',
        'common.text',
        'economy.accounting.voucher.voucherseries',
        'economy.accounting.voucher.vatvoucher',
        'economy.accounting.voucher.sourcetype',
        'economy.accounting.voucher.vouchermodified',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnNumber('voucherNr', terms['common.number'], {
          flex: 1,
        }),
          this.grid.addColumnDate('date', terms['common.date'], {
            flex: 1,
          }),
          this.grid.addColumnText('text', terms['common.text'], {
            flex: 1,
          }),
          this.grid.addColumnText(
            'voucherSeriesTypeName',
            terms['economy.accounting.voucher.voucherseries'],
            {
              flex: 1,
            }
          ),
          this.grid.addColumnBool(
            'vatVoucher',
            terms['economy.accounting.voucher.vatvoucher'],
            {
              flex: 1,
            }
          ),
          this.grid.addColumnIcon('', '', {
            iconName: 'paperclip',
            iconClass: 'paperclip',
            showIcon: row => row.hasDocuments,
            filter: true,
            showSetFilter: true,
          });
        this.grid.addColumnIcon('modified', '', {
          iconName: 'exclamation-circle',
          iconClass: 'warningColor',
          tooltip: terms['economy.accounting.voucher.vouchermodified'],
          showIcon: row => row.hasHistoryRows,
          filter: true,
          showSetFilter: true,
        });

        this.grid.context.suppressGridMenu = true;
        super.finalizeInitGrid({ hidden: true });
      });
  }

  copyVoucherTemplates() {
    this.performAction.crud(
      CrudActionTypeEnum.Save,
      this.accountYearService.copyVoucherTemplate(this.accountYearId).pipe(
        tap(result => {
          if (result.success) {
            this.reloadVoucherTemplate.emit();
          } else {
            const errorMsg = ResponseUtil.getErrorMessage(result);
            this.messageBoxService.error(
              this.translate.instant('common.status'),
              errorMsg ? errorMsg : ''
            );
          }
        })
      ),
      undefined,
      undefined,
      {
        showDialogOnComplete: false,
      }
    );
  }

  loadVoucherTemplates(): Observable<IVoucherGridDTO[]> {
    const id = this.form?.getIdControl()?.value;
    if (id)
      return this.VoucherService.getVoucherTemplates(id).pipe(
        tap(template => {
          this.rows.next(template);
        })
      );
    else return of();
  }
}
