import { Component, inject, signal, OnInit } from '@angular/core';
import { GridBaseDirective } from '@shared/directives/grid-base/grid-base.directive';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service';
import { ProgressService } from '@shared/services/progress/progress.service';
import { ExportUtil } from '@shared/util/export-util';
import { Perform } from '@shared/util/perform.class';
import { GridComponent } from '@ui/grid/grid.component';
import { ToolbarService } from '@ui/toolbar/services/toolbar.service';
import { Observable, of } from 'rxjs';
import { take, tap } from 'rxjs/operators';
import {
  ISysEdiMessageHeadDTO,
  ISysEdiMessageHeadGridDTO,
} from '../../models/sys-edi-message-head.model';
import { SysEdiMessageHeadService } from '../../services/sys-edi-message-head.service';

@Component({
  selector: 'soe-sys-edi-message-head-grid',
  templateUrl: './sys-edi-message-head-grid.component.html',
  providers: [FlowHandlerService, ToolbarService],
  standalone: false,
})
export class SysEdiMessageHeadGridComponent
  extends GridBaseDirective<ISysEdiMessageHeadGridDTO, SysEdiMessageHeadService>
  implements OnInit
{
  service = inject(SysEdiMessageHeadService);
  progressService = inject(ProgressService);
  flowHandler = inject(FlowHandlerService);
  performDownloadMessageFile = new Perform<any>(this.progressService);
  performLoadMissingSysCompanyId = new Perform<any>(this.progressService);

  open: boolean = true;
  closed: boolean = false;
  raw: boolean = false;

  ngOnInit(): void {
    super.ngOnInit();
    this.startFlow(
      Feature.Manage_System,
      'Soe.Manage.System.Edi.SysEdiMessageHead'
    );
  }

  override createGridToolbar(): void {
    super.createGridToolbar({});

    this.toolbarService.createItemGroup({
      items: [
        this.toolbarService.createToolbarButton('import', {
          iconName: signal('search'),
          caption: signal('manage.system.edi.onlyediwithoutcompany'),
          tooltip: signal('economy.supplier.invoice.getEinvoices'),
          onAction: () => this.searchForMissingSysCompanyId(),
        }),
      ],
    });
  }

  override onGridReadyToDefine(grid: GridComponent<ISysEdiMessageHeadGridDTO>) {
    super.onGridReadyToDefine(grid);
    this.translate
      .get([
        'common.name',
        'manage.system.edi.wholesellerbuyernr',
        'common.syscompany',
        'manage.system.syswholeseller.syswholeseller',
        'common.number',
        'manage.system.edi.invoicedate',
        'common.status',
        'common.sent',
        'common.errormessage',
        'core.edit',
        'common.download',
        'manage.system.edi.invoicenr',
        'manage.system.edi.ordernr',
        'manage.system.edi.messagetype',
      ])
      .pipe(take(1))
      .subscribe(terms => {
        this.grid.addColumnText('buyerName', terms['common.name'], {
          flex: 40,
        });
        this.grid.addColumnText(
          'buyerId',
          terms['manage.system.edi.wholesellerbuyernr'],
          {
            flex: 40,
          }
        );
        this.grid.addColumnText('sysCompanyName', terms['common.syscompany'], {
          flex: 40,
        });
        this.grid.addColumnText(
          'sysWholesellerName',
          terms['manage.system.syswholeseller.syswholeseller'],
          {
            flex: 40,
          }
        );
        this.grid.addColumnText(
          'sellerName',
          terms['manage.system.syswholeseller.syswholeseller'],
          {
            flex: 40,
          }
        );
        this.grid.addColumnText(
          'headSellerOrderNumber',
          terms['common.number'],
          {
            flex: 40,
          }
        );
        this.grid.addColumnDate(
          'headInvoiceDate',
          terms['manage.system.edi.invoicedate'],
          {
            flex: 40,
          }
        );
        this.grid.addColumnText(
          'sysEdiMessageHeadStatus',
          terms['common.status'],
          {
            flex: 40,
          }
        );
        this.grid.addColumnText(
          'messageType',
          terms['manage.system.edi.messagetype'],
          {
            flex: 40,
          }
        );
        this.grid.addColumnText(
          'headInvoiceNumber',
          terms['manage.system.edi.invoicenr'],
          {
            flex: 40,
          }
        );
        this.grid.addColumnText(
          'headBuyerOrderNumber',
          terms['manage.system.edi.ordernr'],
          {
            flex: 40,
          }
        );
        this.grid.addColumnDate('sendDate', terms['common.sent'], {
          flex: 40,
        });
        this.grid.addColumnText('errorMessage', terms['common.errormessage'], {
          flex: 40,
        });
        this.grid.addColumnIcon(null, terms['common.download'], {
          flex: 5,
          iconName: 'download',
          iconClass: 'icon-download',
          pinned: 'right',
          tooltip: terms['common.download'],
          showIcon: row => {
            return !row.onlyInRaw;
          },
          onClick: row => {
            this.downloadMessageFile(row);
          },
        });
        this.grid.addColumnIconEdit({
          tooltip: terms['core.edit'],
          showIcon: row => {
            return !row.onlyInRaw;
          },
          onClick: row => {
            this.edit(row);
          },
        });
        super.finalizeInitGrid();
      });
  }

  override loadData(
    id?: number | undefined
  ): Observable<ISysEdiMessageHeadGridDTO[]> {
    return this.performLoadData.load$(
      this.service.getGridFilter(this.open, this.closed, this.raw, false).pipe(
        tap(data => {
          this.grid.setData(data);
          return data;
        })
      )
    );
  }

  searchForMissingSysCompanyId(): void {
    this.performLoadMissingSysCompanyId.load(
      this.service.searchForMissingSysCompanyId().pipe(
        tap(values => {
          this.grid.setData(values);
        })
      )
    );
  }

  openMessagesChanged(value: boolean) {
    this.open = value;
    this.loadData().subscribe();
  }

  closedMessagesChanged(value: boolean) {
    this.closed = value;
    this.loadData().subscribe();
  }

  rawMessagesChanged(value: boolean) {
    this.raw = value;
    this.loadData().subscribe();
  }

  downloadMessageFile(row: ISysEdiMessageHeadGridDTO) {
    return of(
      this.performDownloadMessageFile.load(
        this.service.getSysEdiMessageHeadMsg(row.sysEdiMessageHeadId).pipe(
          tap(result => {
            if (result.success && result.stringValue) {
              ExportUtil.Export(result.stringValue, 'edimessage.xml');
            }
          })
        )
      )
    );
  }
}
