import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { Feature } from '@shared/models/generated-interfaces/Enumerations';
import { FlowHandlerService } from '@shared/services/flow-handler.service'
import { ProgressService } from '@shared/services/progress/progress.service';
import { CrudActionTypeEnum } from '@shared/enums';
import { ExportUtil } from '@shared/util/export-util'
import { Perform } from '@shared/util/perform.class'
import { SoeConfigUtil } from '@shared/util/soeconfig-util';
import { DialogComponent } from '@ui/dialog/dialog/dialog.component'
import { DialogData } from '@ui/dialog/models/dialog';

export interface ITimeWorkAccountOutput {
  row: any | undefined;
  header: any | undefined;
  action: CrudActionTypeEnum;
}
export interface ITimeWorkAccountOutputDialogData extends DialogData {
  row: any | undefined;
  header: any | undefined;
  topText: string | undefined;
  export: boolean | undefined;
  directExport: boolean | undefined;
}
@Component({
    selector: 'soe-time-work-account-output',
    templateUrl: './time-work-account-output.component.html',
    providers: [FlowHandlerService],
    standalone: false
})
export class TimeWorkAccountOutputComponent
  extends DialogComponent<ITimeWorkAccountOutputDialogData>
  implements OnInit
{
  @Input() row: any | undefined;
  @Input() header: any | undefined;
  @Input() topText: string | undefined;
  @Input() export: boolean | undefined;
  @Input() directExport: boolean | undefined;
  @Output() actionTaken = new EventEmitter<CrudActionTypeEnum>();

  performLoad = new Perform<any>(this.progressService);

  // Lookups
  terms: any = [];

  public event: EventEmitter<ITimeWorkAccountOutput> = new EventEmitter();

  get currentLanguage(): string {
    return SoeConfigUtil.language;
  }

  output: any[] = [];
  title = '';
  headerItems: any[] = [];
  headerSize = 0;

  constructor(
    private progressService: ProgressService,
    public handler: FlowHandlerService
  ) {
    super();
    this.setData(this.data.row, this.data.header);
  }

  ngOnInit() {
    this.handler.execute({
      permission: Feature.Time_Payroll_TimeWorkAccount,
    });
  }

  triggerEvent(
    index: number | undefined,
    item: any | undefined,
    action: CrudActionTypeEnum
  ) {
    this.dialogRef.close({ index, object: item, action });
  }

  setData(row?: any, header?: any) {
    this.output = row;
    this.headerItems = header;
    this.headerSize = header?.length ?? 0;
    this.export = this.data.export;
    this.topText = this.data?.topText ?? '';

    if (this.data?.directExport) {
      this.exportToExcel();
      this.closeDialog();
    }
  }

  exportToExcel() {
    const headers: string[] = [];
    this.headerItems.forEach(head => {
      headers.push(head.header);
    });

    let content: string = headers.join(';') + '\r\n';
    const fileName: string = this.data?.title ?? 'output';

    this.output?.forEach(row => {
      const rowContent: string[] = [];
      for (let i = 0; i < this.headerSize; i++) {
        if (row.text[i]) rowContent.push(row.text[i]);
        else rowContent.push('');
      }
      content += rowContent.join(';') + '\r\n';
    });

    ExportUtil.ExportToCSV('\uFEFF' + content, fileName + '.csv');
  }
}
